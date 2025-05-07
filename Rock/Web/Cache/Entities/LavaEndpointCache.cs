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
using System.Runtime.Serialization;

using Rock.Cms;
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
    public class LavaEndpointCache : ModelCache<LavaEndpointCache, LavaEndpoint>, IHasReadOnlyAdditionalSettings
    {
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
        /// Gets or sets the field type slug.
        /// </summary>
        /// <value>
        /// The field type slug.
        /// </value>
        [DataMember]
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the field type SecurityMode.
        /// </summary>
        /// <value>
        /// The field type SecurityMode.
        /// </value>
        [DataMember]
        public LavaEndpointSecurityMode SecurityMode { get; set; }

        /// <summary>
        /// Gets or sets the field type IsActive.
        /// </summary>
        /// <value>
        /// The field type IsActive.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the field type IsSystem.
        /// </summary>
        /// <value>
        /// The field type IsSystem.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the field type AdditionalSettingsJson.
        /// </summary>
        /// <value>
        /// The field type AdditionalSettingsJson.
        /// </value>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        /// <summary>
        /// Gets or sets the field type HttpMethod.
        /// </summary>
        /// <value>
        /// The field type HttpMethod.
        /// </value>
        [DataMember]
        public LavaEndpointHttpMethod HttpMethod { get; set; }

        /// <summary>
        /// Gets or sets the field type EnabledLavaCommands.
        /// </summary>
        /// <value>
        /// The field type EnabledLavaCommands.
        /// </value>
        [DataMember]
        public string EnabledLavaCommands { get; set; }

        /// <summary>
        /// Gets or sets the field type CodeTemplate.
        /// </summary>
        /// <value>
        /// The field type CodeTemplate.
        /// </value>
        [DataMember]
        public string CodeTemplate { get; set; }

        /// <summary>
        /// Gets or sets the field type LavaApplicationId.
        /// </summary>
        /// <value>
        /// The field type LavaApplicationId.
        /// </value>
        [DataMember]
        public int LavaApplicationId { get; set; }

        /// <summary>
        /// Gets the cache object that represents the LavaApplication.
        /// </summary>
        public LavaApplicationCache LavaApplication => LavaApplicationCache.Get( this.LavaApplicationId );

        /// <summary>
        /// Gets or sets the cache control headers.
        /// </summary>
        /// <value>
        /// The cache control headers.
        /// </value>
        public string CacheControlHeaderSettings { get; set; }

        /// <summary>
        /// Gets the cache control header.
        /// </summary>
        /// <value>
        /// The cache control header.
        /// </value>
        public string CacheControlHeader { get; private set; }

        /// <summary>
        /// Gets or sets the field type LavaApplication.
        /// </summary>
        /// <value>
        /// The field type LavaApplication.
        /// </value>
        public LavaApplicationCache LavaApplicationCache => LavaApplicationCache.Get( this.LavaApplicationId );

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override ISecured ParentAuthority
        {
            get
            {
                // Send the endpoint's parent application
                if ( this.LavaApplication != null )
                {
                    return this.LavaApplication;
                }

                // At this point it's a bit of game over. We do not want to return base. ParentAuthority as we want these to have explicit security on them.
                return null;
            }
        }

        /// <summary>
        /// We have custom logic for authorization based on the configuration.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="person"></param>
        /// <returns></returns>
        public override bool IsAuthorized( string action, Person person )
        {
            // We have custom logic if we're trying to determine execute permission and we're configured
            // to read it off of the application.

            if ( action == Authorization.EXECUTE )
            {
                // If endpoint is handling it's own security use that.
                if ( this.SecurityMode == LavaEndpointSecurityMode.EndpointExecute )
                {
                    return base.IsAuthorized( action, person );
                }

                // If the endpoint is deferring to the application use the application logic.
                // If it's not default look to the application for direction
                if ( this.SecurityMode == LavaEndpointSecurityMode.ApplicationView )
                {
                    return this.LavaApplication.IsAuthorized( Model.LavaApplication.EXECUTE_VIEW, person );
                }

                if ( this.SecurityMode == LavaEndpointSecurityMode.ApplicationEdit )
                {
                    return this.LavaApplication.IsAuthorized( Model.LavaApplication.EXECUTE_EDIT, person );
                }

                if ( this.SecurityMode == LavaEndpointSecurityMode.ApplicationAdministrate )
                {
                    return this.LavaApplication.IsAuthorized( Model.LavaApplication.EXECUTE_ADMINISTRATE, person );
                }
            }

            // If it's not EXECUTE then allow if they are in the override roles.
            var isInOverrideRole = RoleCache.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() ).IsPersonInRole( person.Guid )
                                || RoleCache.Get( SystemGuid.Group.GROUP_LAVA_APPLICATION_DEVELOPERS.AsGuid() ).IsPersonInRole( person.Guid );

            if ( isInOverrideRole )
            {
                return true;
            }

            // Otherwise use default security.
            return base.IsAuthorized( action, person );
        }

        /// <summary>
        /// Get's the additional endpoint settings.
        /// </summary>
        internal LavaEndpointAdditionalSettings EndpointAdditionalSettings
        {
            get
            {
                if ( _endpointSettings == null )
                {
                    _endpointSettings = this.GetAdditionalSettings<LavaEndpointAdditionalSettings>();
                }

                return _endpointSettings;
            }
        }
        private LavaEndpointAdditionalSettings _endpointSettings = null;

        /// <summary>
        /// Gets whether the cross-site forgery protection is on or not.
        /// </summary>
        public bool EnableCrossSiteForgeryProtection
        { 
            get
            {
                return this.EndpointAdditionalSettings.EnableCrossSiteForgeryProtection;
            }
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var lavaEndpoint = entity as LavaEndpoint;
            if ( lavaEndpoint == null )
            {
                return;
            }

            Name = lavaEndpoint.Name;
            Description = lavaEndpoint.Description;
            IsActive = lavaEndpoint.IsActive;
            IsSystem = lavaEndpoint.IsSystem;
            AdditionalSettingsJson = lavaEndpoint.AdditionalSettingsJson;
            Slug = lavaEndpoint.Slug;
            CodeTemplate = lavaEndpoint.CodeTemplate;
            EnabledLavaCommands = lavaEndpoint.EnabledLavaCommands;
            LavaApplicationId = lavaEndpoint.LavaApplicationId;
            HttpMethod = lavaEndpoint.HttpMethod;
            SecurityMode = lavaEndpoint.SecurityMode;
            CacheControlHeaderSettings = lavaEndpoint.CacheControlHeaderSettings;
            CacheControlHeader = lavaEndpoint.CacheControlHeader.ToStringSafe();
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
