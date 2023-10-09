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

using Rock.Security;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class RestAction
    {
        /// <summary>
        /// The cached actions that are supported by this instance.
        /// </summary>
        private Dictionary<string, string> _supportedActions;

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
                // If we don't already have the supported actions cached, then
                // use reflection to find the original method and check for
                // any SecurityActionAttributes defined on it.
                if ( _supportedActions == null )
                {
                    var controller = RestControllerCache.Get( ControllerId );

                    if ( controller != null )
                    {
                        var actions = new Dictionary<string, string>( controller.SupportedActions );

                        // Might be nice to cache this data in the future.
                        var type = Reflection.FindType( typeof( object ), controller.ClassName );
                        var method = type?.GetMethods()
                            .Where( m => m.GetCustomAttribute<RestActionGuidAttribute>()?.Guid == Guid )
                            .FirstOrDefault();

                        if ( method != null )
                        {
                            foreach ( var sa in method.GetCustomAttributes<SecurityActionAttribute>() )
                            {
                                actions.AddOrIgnore( sa.Action, sa.Description );
                            }
                        }

                        // If this is the new API endpoints, automatically remove actions
                        // that do not apply based on the method.
                        if ( controller.ClassName.StartsWith( "Rock.Rest.v2." ) )
                        {
                            if ( Method.Equals( "GET", System.StringComparison.OrdinalIgnoreCase ) )
                            {
                                actions.Remove( Authorization.EDIT );
                                actions.Remove( "UnrestrictedEdit" );
                            }
                            else
                            {
                                actions.Remove( Authorization.VIEW );
                                actions.Remove( "UnrestrictedView" );
                            }
                        }

                        _supportedActions = actions;
                    }
                    else
                    {
                        _supportedActions = base.SupportedActions;
                    }
                }

                return _supportedActions;
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
            return RestActionCache.Get( Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            RestActionCache.UpdateCachedEntity( Id, entityState );
        }

        #endregion
    }
}
