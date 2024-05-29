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

using Microsoft.Extensions.DependencyInjection;

namespace Rock.Configuration
{
    // Use System.Web.VirtualPathUtility.ToAbsolute("~/") to get virtual root path.

    /// <summary>
    /// The RockApp class provides access to all the configuration information
    /// about a running Rock instance.
    /// </summary>
    public class RockApp
    {
        #region Fields

        /// <summary>
        /// The service provider that controls the entire application.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// The Rock initialization settings currently being used for this
        /// instance of Rock.
        /// </summary>
        private readonly Lazy<IInitializationSettings> _initializationSettings;

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
        public virtual IInitializationSettings InitializationSettings => _initializationSettings.Value;

        /// <summary>
        /// The hosting settings for the current Rock instance. These settings
        /// are provided by the environment and cannot be changed.
        /// </summary>
        /// <value>The hosting settings.</value>
        public virtual IHostingSettings HostingSettings => _serviceProvider.GetRequiredService<IHostingSettings>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RockApp"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used for dependency injection.</param>
        internal RockApp( IServiceProvider serviceProvider )
        {
            _serviceProvider = serviceProvider;
            _initializationSettings = new Lazy<IInitializationSettings>( () => _serviceProvider.GetRequiredService<IInitializationSettings>() );
        }

        #endregion

        #region Methods

        /// <inheritdoc cref="IServiceProvider.GetService(Type)"/>
        internal object GetService( Type type )
        {
            return _serviceProvider.GetService( type );
        }

        #endregion
    }
}
