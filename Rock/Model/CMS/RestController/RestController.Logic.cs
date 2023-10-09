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
using System.Data.Entity;
using System.Linq;
using System.Reflection;

using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class RestController
    {
        /// <summary>
        /// The cached actions that are supported by this instance.
        /// </summary>
        private Dictionary<string, string> _supportedActions;

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return RestControllerCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            RestControllerCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion

        /// <inheritdoc/>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                // If we don't already have the supported actions cached, then
                // use reflection to find the original type and check for
                // any SecurityActionAttributes defined on it.
                if ( _supportedActions == null )
                {
                    var actions = new Dictionary<string, string>( base.SupportedActions );

                    // Might be nice to cache this data in the future.
                    var type = Reflection.FindType( typeof( object ), ClassName );

                    if ( type != null )
                    {
                        foreach ( var sa in type.GetCustomAttributes<SecurityActionAttribute>() )
                        {
                            actions.AddOrIgnore( sa.Action, sa.Description );
                        }
                    }

                    _supportedActions = actions;
                }

                return _supportedActions;
            }
        }

    }
}
