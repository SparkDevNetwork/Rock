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
using System.Runtime.Serialization;

using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Cms;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a Lava application that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class LavaApplicationCache : ModelCache<LavaApplicationCache, LavaApplication>
    {
        #region Private Members
        private List<int> _lavaEndpointIds = null;

        private Dictionary<string, int> _lavaEndpointSlugs = null;
        private readonly object _lavaEndpointLock = new object();
        private readonly object _lavaEndpointSlugLock = new object();
        
        #endregion

        #region Static Members

        private static Dictionary<string, int> _lavaApplicationSlugs = null;
        private static readonly object _lavaApplicationSlugLock = new object();

        /// <summary>
        /// Gets a Lava Application based on it's slug. This uses a dictionary to improve the speed of the lookup.
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public static LavaApplicationCache GetBySlug( string slug )
        {
            // Build the lookup in a local and publish it only once complete, the same as
            // GetEndpoint(). See the engineering note there.
            var lavaApplicationSlugs = _lavaApplicationSlugs;

            // Load slug dictionary if needed.
            if ( lavaApplicationSlugs == null )
            {
                lock ( _lavaApplicationSlugLock )
                {
                    lavaApplicationSlugs = _lavaApplicationSlugs;

                    if ( lavaApplicationSlugs == null )
                    {
                        lavaApplicationSlugs = new Dictionary<string, int>();

                        foreach ( var lavaApplication in LavaApplicationCache.All() )
                        {
                            lavaApplicationSlugs.AddOrReplace( lavaApplication.Slug, lavaApplication.Id );
                        }

                        _lavaApplicationSlugs = lavaApplicationSlugs;
                    }
                }
            }

            // Lookup Lava application by slug
            return LavaApplicationCache.Get( lavaApplicationSlugs.GetValueOrDefault( slug, 0 ) );
        }

        /// <summary>
        /// Flushes the cache of Lava Applications by slug.
        /// </summary>
        public static void FlushSlugCache()
        {
            _lavaApplicationSlugs = null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the field type name.
        /// </summary>
        /// <value>
        /// The field type name.
        /// </value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the field type description.
        /// </summary>
        /// <value>
        /// The field type description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the field type IsSystem.
        /// </summary>
        /// <value>
        /// The field type IsSystem.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the field type IsActive.
        /// </summary>
        /// <value>
        /// The field type IsActive.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the field type slug.
        /// </summary>
        /// <value>
        /// The field type slug.
        /// </value>
        [DataMember]
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the field type AdditionalSettingsJson.
        /// </summary>
        /// <value>
        /// The field type AdditionalSettingsJson.
        /// </value>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        /// <summary>
        /// Gets or sets the field type ConfigurationRiggingJson.
        /// </summary>
        /// <value>
        /// The field type ConfigurationRiggingJson.
        /// </value>
        [DataMember]
        public string ConfigurationRiggingJson { get; set; }

        /// <summary>
        /// Gets the configuration rigging as a dynamic.
        /// </summary>
        [DataMember]
        public dynamic ConfigurationRigging
        {
            get
            {
                if ( _configurationRigging == null )
                {
                    _configurationRigging = this.ConfigurationRiggingJson.FromJsonDynamic();
                }

                return _configurationRigging;
            }
        }
        private dynamic _configurationRigging = null;

        /// <summary>
        /// Gets the Lava endpoints.
        /// </summary>
        /// <value>
        /// The Lava endpoints.
        /// </value>
        public List<LavaEndpointCache> LavaEndpoints
        {
            get
            {
                var lavaEndpoints = new List<LavaEndpointCache>();

                // Read the field once so a concurrent FlushEndpoints() cannot null it
                // between the check and the loop below.
                var lavaEndpointIds = _lavaEndpointIds;

                if ( lavaEndpointIds == null )
                {
                    lock ( _lavaEndpointLock )
                    {
                        lavaEndpointIds = _lavaEndpointIds;

                        if ( lavaEndpointIds == null )
                        {
                            using ( var rockContext = RockApp.Current.CreateRockContext() )
                            {
                                lavaEndpointIds = new LavaEndpointService( rockContext )
                                    .GetByLavApplicationId( Id )
                                    .Select( v => v.Id )
                                    .ToList();
                            }

                            _lavaEndpointIds = lavaEndpointIds;
                        }
                    }
                }

                foreach ( var id in lavaEndpointIds )
                {
                    var laveEndpoint = LavaEndpointCache.Get( id );
                    if ( laveEndpoint != null )
                    {
                        lavaEndpoints.Add( laveEndpoint );
                    }
                }

                return lavaEndpoints;
            }
        }

        /// <summary>
        /// Gets the parent authority where security authorizations are being inherited from.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override ISecured ParentAuthority
        {
            get
            {
                // We intentionally want to break security inheritance to force the application to
                // explicitly declare cases where everyone should have access. Otherwise, they would
                // inherit the global default of "Can View".
                return null;
            }
        }

        /// <summary>
        /// Provides custom security authorization logic. 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="person"></param>
        /// <returns></returns>
        public override bool IsAuthorized( string action, Person person )
        {
            // If the person is a Rock Admin or Lava Application Developer always allow. The
            // RoleCache.Get() calls can return null when a role is inactive or missing, so the
            // results must be null-checked before testing role membership.
            var isInOverrideRole = ( person != null ) && ( RoleCache.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() )?.IsPersonInRole( person.Guid ) == true
                                || RoleCache.Get( SystemGuid.Group.GROUP_LAVA_APPLICATION_DEVELOPERS.AsGuid() )?.IsPersonInRole( person.Guid ) == true );

            if ( isInOverrideRole )
            {
                return true;
            }

            // Check to see if user is authorized using normal authorization rules
            return base.IsAuthorized( action, person );
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the Lava Endpoint that matches the slug and HTTP method
        /// </summary>
        /// <param name="slug"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public LavaEndpointCache GetEndpoint( string slug, LavaEndpointHttpMethod method )
        {
            var lavaEndpointSlugs = _lavaEndpointSlugs;

            // Load the cache if needed
            if ( lavaEndpointSlugs == null )
            {
                lock ( _lavaEndpointSlugLock )
                {
                    lavaEndpointSlugs = _lavaEndpointSlugs;

                    if ( lavaEndpointSlugs == null )
                    {
                        lavaEndpointSlugs = new Dictionary<string, int>();

                        foreach ( var lavaEndpoint in this.LavaEndpoints )
                        {
                            var key = $"{lavaEndpoint.Slug}-{lavaEndpoint.HttpMethod}";
                            lavaEndpointSlugs.AddOrReplace( key, lavaEndpoint.Id );
                        }

                        _lavaEndpointSlugs = lavaEndpointSlugs;
                    }
                }
            }

            // Return the matching Lava endpoint
            return LavaEndpointCache.Get( lavaEndpointSlugs.GetValueOrDefault( $"{slug}-{method}", 0 ) );
        }

        /// <summary>
        /// Flushes the application's endpoints.
        /// </summary>
        public void FlushEndpoints()
        {
            _lavaEndpointIds = null;
            _lavaEndpointSlugs = null;
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var lavaApplication = entity as LavaApplication;
            if ( lavaApplication == null )
            {
                return;
            }

            Name = lavaApplication.Name;
            Description = lavaApplication.Description;
            IsActive = lavaApplication.IsActive;
            IsSystem = lavaApplication.IsSystem;
            AdditionalSettingsJson = lavaApplication.AdditionalSettingsJson;
            ConfigurationRiggingJson = lavaApplication.ConfigurationRiggingJson;
            Slug = lavaApplication.Slug;

            // Reset list of endpoints to they will be loaded on demand.
            _lavaEndpointIds = null;
            _lavaEndpointSlugs = null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
