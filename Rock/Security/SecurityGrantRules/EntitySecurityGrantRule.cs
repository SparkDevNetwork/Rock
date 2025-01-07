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

using Newtonsoft.Json;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Security.SecurityGrantRules
{
    /// <summary>
    /// Grants permission to a specific entity given its entity type identifier
    /// and its entity identifier.
    /// </summary>
    /// <seealso cref="Rock.Security.SecurityGrantRule" />
    [Rock.SystemGuid.SecurityGrantRuleGuid( "7da622dc-3d38-4941-8ca0-179752c8e43d" )]
    public class EntitySecurityGrantRule : SecurityGrantRule
    {
        #region Properties

        /// <summary>
        /// Gets the entity type identifier that must match to grant access.
        /// </summary>
        /// <value>The entity type identifier that must match to grant access.</value>
        [JsonProperty( "et", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public int EntityTypeId { get; private set; }

        /// <summary>
        /// Gets the entity identifier that must match to grant access.
        /// </summary>
        /// <value>The entity identifier that must match to grant access.</value>
        [JsonProperty( "e", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public int EntityId { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="EntitySecurityGrantRule"/> class from being created.
        /// </summary>
        private EntitySecurityGrantRule()
            : base( Authorization.VIEW )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySecurityGrantRule"/> class
        /// for granting <see cref="Authorization.VIEW"/> access.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier that must be matched to grant access.</param>
        /// <param name="entityId">The entity identifier that must be matched to grant access.</param>
        public EntitySecurityGrantRule( int entityTypeId, int entityId )
            : this( entityTypeId, entityId, Authorization.VIEW )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySecurityGrantRule"/> class.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier that must be matches to grant access.</param>
        /// <param name="entityId">The entity identifier that must be matches to grant access.</param>
        /// <param name="action">The action that will be authorized.</param>
        public EntitySecurityGrantRule( int entityTypeId, int entityId, string action )
            : base( action )
        {
            EntityTypeId = entityTypeId;
            EntityId = entityId;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool IsAccessGranted( object obj, string action )
        {
            if ( obj is IEntity entity )
            {
                return entity.TypeId == EntityTypeId && entity.Id == EntityId;
            }
            else if ( obj is IEntityCache cachedEntity )
            {
                return cachedEntity.CachedEntityTypeId == EntityTypeId && cachedEntity.Id == EntityId;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
