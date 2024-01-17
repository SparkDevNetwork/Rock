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
using Rock.Attribute;
using Rock.Model;
using Rock.Web.HttpModules;

namespace Rock.RealTime.AspNet
{
    /// <summary>
    /// Handles OWIN configuration of the RealTime engine as well as legacy SignalR support.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.14.1", true )]
    public static class AspNetEngineStartup
    {
        /// <summary>
        /// Configurations the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        public static void Configure( IAppBuilder app )
        {
            // Suppress SignalR "The remote host closed the connection" errors
            // that make it to the global hub pipeline.
            // This is unlikely to happen but adding just in case.
            GlobalHost.HubPipeline.AddModule( new SignalRErrorModule() );
            
            // Suppress SignalR "The remote host closed the connection" errors in the legacy SignalR pipeline.
            var legacySignalRHubConfig = new HubConfiguration();
            legacySignalRHubConfig.Resolver?.Resolve<IHubPipeline>()?.AddModule( new SignalRErrorModule() );

            app.MapSignalR( legacySignalRHubConfig );

            /* 02/18/2022 MDP
             By default, Signal R will use reflection to find classes that inherit from Microsoft.AspNet.SignalR.
             It looks in *all* DLLs in RockWeb/bin. It does this on the first page that includes <script src="/SignalR/hubs"></script>.
             This initial hit can take 30-60 seconds, so we'll register our own assembly locator to only look in Rock and Rock Plugins.
             RockWeb.RockMessageHub will be the only Hub. So it doesn't make sense to look in all DLL for any more.
            */
            GlobalHost.DependencyResolver.Register( typeof( IAssemblyLocator ), () => new RockHubAssemblyLocator() );
            GlobalHost.DependencyResolver.Register( typeof( IHubDescriptorProvider ), () => new LegacyHubDescriptorProvider( GlobalHost.DependencyResolver ) );

            // Initialize the Rock RealTime system.
            var rtHubConfiguration = new HubConfiguration
            {
                Resolver = new DefaultDependencyResolver()
            };
            rtHubConfiguration.Resolver.Register( typeof( IHubDescriptorProvider ), () => new RealTimeHubDescriptorProvider() );
            rtHubConfiguration.Resolver.Register( typeof( JsonSerializer ), () => CreateRealTimeSerializer() );
            
            // Suppress SignalR "The remote host closed the connection" errors in the Rock RealTime pipeline.
            rtHubConfiguration.Resolver?.Resolve<IHubPipeline>()?.AddModule( new SignalRErrorModule() );

            var azureEndpoint = System.Configuration.ConfigurationManager.AppSettings["AzureSignalREndpoint"];
            var azureAccessKey = System.Configuration.ConfigurationManager.AppSettings["AzureSignalRAccessKey"];

            app.Map( "/rock-rt", subApp =>
            {
                // Register some logic to handle adding a claim for the anonymous
                // person identifier if we have one.
                subApp.Use( RegisterSignalRClaims );

                if ( azureEndpoint.IsNullOrWhiteSpace() || azureAccessKey.IsNullOrWhiteSpace() )
                {
                    subApp.RunSignalR( rtHubConfiguration );
                }
                else
                {
                    var connectionString = $"Endpoint=https://{azureEndpoint};AccessKey={azureAccessKey};Version=1.0;";
                    subApp.RunAzureSignalR( "Rock", connectionString, rtHubConfiguration );
                }
            } );

            RealTimeHelper.Initialize( new AspNetEngine( rtHubConfiguration ) );
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
                ContractResolver = new RealTimeContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            return JsonSerializer.Create( settings );
        }

        /// <summary>
        /// Special contract resolver to deal with making user-data camelCase
        /// properties while leaving SignalR internal messages PascalCase.
        /// </summary>
        private class RealTimeContractResolver : DefaultContractResolver
        {
            #region Fields

            /// <summary>
            /// All types matching this assembly will use <see cref="_defaultContractResolver"/>.
            /// </summary>
            private readonly Assembly _coreAssembly = typeof( Hub<> ).Assembly;

            /// <summary>
            /// The contract resolver that uses PascalCase.
            /// </summary>
            private readonly IContractResolver _defaultContractResolver = new DefaultContractResolver();

            /// <summary>
            /// The contract resolver that uses camelCase.
            /// </summary>
            private readonly IContractResolver _camelCaseContractResolver = new CamelCasePropertyNamesContractResolver();

            #endregion

            /// <inheritdoc/>
            public override JsonContract ResolveContract( Type type )
            {
                if ( type.Assembly == _coreAssembly )
                {
                    return _defaultContractResolver.ResolveContract( type );
                }
                else
                {
                    return _camelCaseContractResolver.ResolveContract( type );
                }
            }
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
