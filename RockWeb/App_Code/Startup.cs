// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Owin;

using Rock;
using Rock.Model;
using Rock.Utility;

[assembly: OwinStartup( typeof( RockWeb.Startup ) )]
namespace RockWeb
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configurations the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        public void Configuration( IAppBuilder app )
        {
            ConfigureSignalR( app );

            try
            {
                // This is for OIDC Connect
                Rock.Oidc.Startup.OnStartup( app );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }

            try
            {
                // Find any plugins that implement IRockOwinStartup
                var startups = new Dictionary<int, List<IRockOwinStartup>>();
                foreach ( var startupType in Rock.Reflection.FindTypes( typeof( IRockOwinStartup ) ).Select( a => a.Value ).ToList() )
                {
                    var startup = Activator.CreateInstance( startupType ) as IRockOwinStartup;
                    startups.AddOrIgnore( startup.StartupOrder, new List<IRockOwinStartup>() );
                    startups[startup.StartupOrder].Add( startup );
                }

                foreach ( var startupList in startups.OrderBy( s => s.Key ).Select( s => s.Value ) )
                {
                    foreach ( var startup in startupList )
                    {
                        startup.OnStartup( app );
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
        }

        /// <summary>
        /// Configure SignalR for use by the application.
        /// </summary>
        /// <param name="app">The application builder to be configured.</param>
        private void ConfigureSignalR( IAppBuilder app )
        {
            app.MapSignalR();

            /* 02/18/2022 MDP
             By default, Signal R will use reflection to find classes that inherit from Microsoft.AspNet.SignalR.
             It looks in *all* DLLs in RockWeb/bin. It does this on the first page that includes <script src="/SignalR/hubs"></script>.
             This initial hit can take 30-60 seconds, so we'll register our own assembly locator to only look in Rock and Rock Plugins.
             RockWeb.RockMessageHub will be the only Hub. So it doesn't make sense to look in all DLL for any more.
            */
            GlobalHost.DependencyResolver.Register( typeof( IAssemblyLocator ), () => new RockHubAssemblyLocator() );
            GlobalHost.DependencyResolver.Register( typeof( IHubDescriptorProvider ), () => new LegacyHubDescriptorProvider( GlobalHost.DependencyResolver ) );

            // TODO: This should be updated to determine from RockContext data if
            // we should use Azure for the RealTime engine in the future once that
            // UI is implemented.
            bool useAzure = false;

            // Initialize the Rock RealTime system.
            var rtHubConfiguration = new HubConfiguration
            {
                Resolver = new DefaultDependencyResolver()
            };
            rtHubConfiguration.Resolver.Register( typeof( IHubDescriptorProvider ), () => new RealTimeHubDescriptorProvider() );
            rtHubConfiguration.Resolver.Register( typeof( JsonSerializer ), () => CreateRealTimeSerializer() );
            if ( !useAzure )
            {
                app.Map( "/rock-rt", subApp =>
                {
                    // Register some logic to handle adding a claim for the anonymous
                    // person identifier if we have one.
                    subApp.Use( RegisterSignalRClaims );
                    subApp.RunSignalR( rtHubConfiguration );
                } );
            }
            else
            {
                app.Map( "/rock-rt", subApp =>
                {
                    // Register some logic to handle adding a claim for the anonymous
                    // person identifier if we have one.
                    subApp.Use( RegisterSignalRClaims );
                    subApp.RunAzureSignalR( "Rock"/*, "connectionString"*/, rtHubConfiguration );
                } );
            }

            Rock.WebStartup.RockApplicationStartupHelper.InitializeRockRealTime( rtHubConfiguration );
        }

        #region SignalR Support

        /// <summary>
        /// Register any additional claims required by SignalR for a SignalR request.
        /// </summary>
        /// <param name="context">The context that identifies the request.</param>
        /// <param name="nextHandler">The next handler to be called after this one.</param>
        /// <returns>A Task that indicates when this process has completed.</returns>
        private static Task RegisterSignalRClaims( IOwinContext context, Func<Task> nextHandler )
        {
            if ( !( context.Request.User is ClaimsPrincipal claimsPrincipal ) )
            {
                return nextHandler();
            }

            // Check if we have a logged in person, if so don't check for visitor.
            if ( claimsPrincipal.Identity?.Name.IsNotNullOrWhiteSpace() == true )
            {
                var user = UserLoginService.GetCurrentUser();

                if ( user != null )
                {
                    var identity = new ClaimsIdentity( new Claim[] { new Claim( "rock:person", user.PersonId.Value.ToString() ) } );

                    claimsPrincipal.AddIdentity( identity );

                    return nextHandler();
                }
            }

            var visitorKeyCookie = context.Request.Cookies[Rock.Personalization.RequestCookieKey.ROCK_VISITOR_KEY];

            if ( visitorKeyCookie.IsNotNullOrWhiteSpace() )
            {
                var identity = new ClaimsIdentity( new Claim[] { new Claim( "rock:visitor", visitorKeyCookie ) } );

                claimsPrincipal.AddIdentity( identity );
            }

            return nextHandler();
        }

        /// <summary>
        /// Creates the real time serializer used to format data.
        /// </summary>
        /// <returns>A <see cref="JsonSerializer"/> configured for use with RealTime engine.</returns>
        private static JsonSerializer CreateRealTimeSerializer()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            return JsonSerializer.Create( settings );
        }

        /// <summary>
        /// Class RockHubAssemblyLocator.
        /// Implements the <see cref="Microsoft.AspNet.SignalR.Hubs.IAssemblyLocator" />
        /// </summary>
        /// <seealso cref="Microsoft.AspNet.SignalR.Hubs.IAssemblyLocator" />
        private class RockHubAssemblyLocator : IAssemblyLocator
        {
            /// <summary>
            /// Gets the assemblies.
            /// </summary>
            /// <returns>IList&lt;Assembly&gt;.</returns>
            public IList<Assembly> GetAssemblies()
            {
                return Rock.Reflection.GetRockAndPluginAssemblies();
            }
        }

        /// <summary>
        /// Locates hubs for the legacy SignalR system. This will filter out any
        /// hubs that have a period in the name since that is not allowed by SignalR.
        /// </summary>
        private class LegacyHubDescriptorProvider : IHubDescriptorProvider
        {
            private readonly Lazy<IHubDescriptorProvider> _baseProvider;

            /// <summary>
            /// Creates a new instance of <see cref="LegacyHubDescriptorProvider"/>
            /// that will be used to find all registered hubs for legacy SignalR.
            /// </summary>
            /// <param name="resolver">The resolver to find dependencies at runtime.</param>
            public LegacyHubDescriptorProvider( IDependencyResolver resolver )
            {
                _baseProvider = new Lazy<IHubDescriptorProvider>( () => new ReflectedHubDescriptorProvider( resolver ) );
            }

            /// <inheritdoc/>
            public IList<HubDescriptor> GetHubs()
            {
                var hubs = _baseProvider.Value.GetHubs();

                // Filter out hubs from plugins which might have a "." in the
                // name since that will cause a startup failure.
                return hubs.Where( h => !h.Name.Contains( "." ) ).ToList();
            }

            /// <inheritdoc/>
            public bool TryGetHub( string hubName, out HubDescriptor descriptor )
            {
                return _baseProvider.Value.TryGetHub( hubName, out descriptor );
            }
        }

        /// <summary>
        /// This is a custom implementation for the "rock-rt" endpoint to
        /// ensure that the only hub that shows up is the RealTime hub.
        /// </summary>
        private class RealTimeHubDescriptorProvider : IHubDescriptorProvider
        {
            /// <summary>
            /// Lazy loaded dictionary of hubs to provide to the resolver.
            /// </summary>
            private readonly Lazy<IDictionary<string, HubDescriptor>> _hubs = new Lazy<IDictionary<string, HubDescriptor>>( BuildHubsCache );

            /// <inheritdoc/>
            public IList<HubDescriptor> GetHubs()
            {
                return _hubs.Value.Select( kv => kv.Value ).Distinct().ToList();
            }

            /// <inheritdoc/>
            public bool TryGetHub( string hubName, out HubDescriptor descriptor )
            {
                return _hubs.Value.TryGetValue( hubName, out descriptor );
            }

            /// <summary>
            /// Build the collection of known hubs, in this case just our RealTime hub.
            /// </summary>
            /// <returns>A dictionary of hub names for keys and hub descriptors for values.</returns>
            protected static IDictionary<string, HubDescriptor> BuildHubsCache()
            {
                var type = typeof( Rock.RealTime.AspNet.RealTimeHub );

                var descriptor = new HubDescriptor
                {
                    NameSpecified = true,
                    Name = "realTime",
                    HubType = type
                };

                return new Dictionary<string, HubDescriptor>( StringComparer.OrdinalIgnoreCase )
                {
                    [descriptor.Name] = descriptor
                };
            }
        }

        #endregion
    }
}
