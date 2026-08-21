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

using System;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.DependencyInjection;

using Rock.Communication.Chat;
using Rock.Data;

namespace Rock.Configuration
{
    // Use System.Web.VirtualPathUtility.ToAbsolute("~/") to get virtual root path.

    /// <summary>
    /// The RockApp class provides access to all the configuration information
    /// about a running Rock instance.
    /// </summary>
    public class RockApp : IServiceProvider
    {
        #region Fields

        /// <summary>
        /// The service provider that controls the entire application.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current instance that is handling the running Rock application.
        /// </summary>
        /// <value>The current instance.</value>
        public static RockApp Current { get; internal set; }

        /// <summary>
        /// <para>
        /// Gets the Rock initialization settings currently being used for
        /// this instance of Rock.
        /// </para>
        /// <para>
        /// These values do not change until Rock restarts. Therefore they
        /// will not reflect any changes made via a call to
        /// <see cref="InitializationSettings.Save()"/>.
        /// </para>
        /// </summary>
        /// <value>The initialization settings of the running application.</value>
        public virtual IInitializationSettings InitializationSettings => _serviceProvider.GetRequiredService<IInitializationSettings>();

        /// <summary>
        /// The hosting settings for the current Rock instance. These settings
        /// are provided by the environment and cannot be changed.
        /// </summary>
        /// <value>The hosting settings.</value>
        public virtual IHostingSettings HostingSettings => _serviceProvider.GetRequiredService<IHostingSettings>();

        #endregion

        #region Constructors

        /// <summary>
        /// Performs all static initialization of the RockApp class.
        /// </summary>
        static RockApp()
        {
#if WEBFORMS
            if ( System.Diagnostics.Process.GetCurrentProcess().ProcessName == "ef6" )
            {
                // Special case for a process named "ef6". This means we are running
                // an EF design time operation. I can't find any hooks for this in
                // EF 6 (EF Core has them), which means we can't initialize a proper
                // RockApp. So this is a special case where we manually initialize
                // the current instance.
                //
                // The body lives in a separate, non-inlined method on purpose.
                // The JIT resolves every type referenced by a method when that
                // method is compiled, so if the StreamChatProvider registration
                // lived directly in this static constructor it would force the
                // loader to resolve the stream-chat-net assembly the moment any
                // code touched RockApp - even outside the ef6 design-time case.
                // Keeping it in a method that is only jitted when this branch
                // actually runs limits that dependency to the ef6 process.
                InitializeForEntityFrameworkDesignTime();
            }
#endif
        }

#if WEBFORMS
        /// <summary>
        /// Builds the minimal <see cref="RockApp"/> used during EF6 design-time
        /// operations (migrations run from the CLI). Intentionally not inlined
        /// so the JIT does not pull its type references - notably
        /// <c>StreamChatProvider</c> and therefore the stream-chat-net assembly -
        /// into the static constructor. See <see cref="RockApp()"/>.
        /// </summary>
        [MethodImpl( MethodImplOptions.NoInlining )]
        private static void InitializeForEntityFrameworkDesignTime()
        {
            var sc = new ServiceCollection();

            sc.AddSingleton<IConnectionStringProvider, WebFormsConnectionStringProvider>();
            sc.AddSingleton<IInitializationSettings, WebFormsInitializationSettings>();
            sc.AddSingleton<IDatabaseConfiguration, DatabaseConfiguration>();
            sc.AddSingleton<IHostingSettings, HostingSettings>();
            sc.AddSingleton<IChatProvider, StreamChatProvider>();
            sc.AddSingleton<IRockContextFactory, RockContextFactory>();

            Current = new RockApp( sc.BuildServiceProvider() );
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="RockApp"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used for dependency injection.</param>
        internal RockApp( IServiceProvider serviceProvider )
        {
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Methods

        /// <inheritdoc cref="IServiceProvider.GetService(Type)"/>
        public object GetService( Type type )
        {
            return _serviceProvider.GetService( type );
        }

        #endregion
    }
}
