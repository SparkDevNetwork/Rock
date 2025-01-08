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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

using Rock.Cms;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class RestAction
    {
        #region Properties

        private RockCacheability _cacheControlHeader;
        /// <summary>
        /// Gets the cache control header.
        /// </summary>
        /// <value>
        /// The cache control header.
        /// </value>
        [NotMapped]
        public RockCacheability CacheControlHeader
        {
            get
            {
                if ( _cacheControlHeader == null && CacheControlHeaderSettings.IsNotNullOrWhiteSpace() )
                {
                    _cacheControlHeader = Newtonsoft.Json.JsonConvert.DeserializeObject<RockCacheability>( CacheControlHeaderSettings );
                }
                return _cacheControlHeader;
            }
        }

        #endregion

        #region ISecured

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.Controller != null ? this.Controller : base.ParentAuthority;
            }
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var metadata = GetMetadata();

                if ( metadata?.SupportedActions != null )
                {
                    return new Dictionary<string, string>( metadata.SupportedActions );
                }

                return CalculateSupportedActions();
            }
        }

        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return RestActionCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
#pragma warning disable CS0618 // Type or member is obsolete
            RestActionCache.UpdateCachedEntity( this.ApiId, entityState );
#pragma warning restore CS0618 // Type or member is obsolete
            RestActionCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Calculates the support actions of this endpoint. The <see cref="Controller"/>
        /// property must be set and valid for this to work.
        /// </summary>
        /// <returns>A dictionary of the supported actions.</returns>
        internal Dictionary<string, string> CalculateSupportedActions()
        {
            if ( Controller == null )
            {
                return base.SupportedActions;
            }

            var actions = new Dictionary<string, string>( Controller.CalculateSupportedActions( Controller.GetMetadata() ) );

            var type = Reflection.FindType( typeof( object ), Controller.ClassName );
            var method = type?.GetMethods()
                .Where( m => m.GetCustomAttribute<RestActionGuidAttribute>()?.Guid == Guid )
                .FirstOrDefault();

            if ( method != null )
            {
                foreach ( var sa in method.GetCustomAttributes<SecurityActionAttribute>() )
                {
                    actions.TryAdd( sa.Action, sa.Description );
                }

                foreach ( var action in method.GetCustomAttributes<ExcludeSecurityActionsAttribute>().SelectMany( esa => esa.Actions ) )
                {
                    actions.Remove( action );
                }
            }

            return actions;
        }

        /// <summary>
        /// Gets the metadata for this instance.
        /// </summary>
        /// <returns>An instance of <see cref="RestActionMetadata"/> or <c>null</c>.</returns>
        internal RestActionMetadata GetMetadata()
        {
            return this.GetAdditionalSettings<RestActionMetadata>();
        }

        /// <summary>
        /// Updates this instance to use the specified metadata.
        /// </summary>
        /// <param name="data">The metadata or <c>null</c>.</param>
        internal void SetMetadata( RestActionMetadata data )
        {
            this.SetAdditionalSettings( data );
        }

        #endregion
    }
}
