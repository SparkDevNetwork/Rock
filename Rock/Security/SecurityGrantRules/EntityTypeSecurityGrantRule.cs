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
    /// Grants permission to all entities of a entity type.
    /// </summary>
    /// <seealso cref="Rock.Security.SecurityGrantRule" />
    [Rock.SystemGuid.SecurityGrantRuleGuid( "9d81709f-a1d3-4f0b-8b69-e81f4fc82e20" )]
    public class EntityTypeSecurityGrantRule : SecurityGrantRule
    {
        #region Properties

        /// <summary>
        /// Gets the entity type identifier that must match to grant access.
        /// </summary>
        /// <value>The entity type identifier that must match to grant access.</value>
        [JsonProperty( "et", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public int EntityTypeId { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="EntityTypeSecurityGrantRule"/> class from being created.
        /// </summary>
        private EntityTypeSecurityGrantRule()
            : base( Authorization.VIEW )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTypeSecurityGrantRule"/> class
        /// for granting <see cref="Authorization.VIEW"/> access.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier that must be matched to grant access.</param>
        public EntityTypeSecurityGrantRule( int entityTypeId )
            : this( entityTypeId, Authorization.VIEW )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySecurityGrantRule"/> class.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier that must be matches to grant access.</param>
        /// <param name="action">The action that will be authorized.</param>
        public EntityTypeSecurityGrantRule( int entityTypeId, string action )
            : base( action )
        {
            EntityTypeId = entityTypeId;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool IsAccessGranted( object obj, string action )
        {
            if ( obj is IEntity entity )
            {
                return entity.TypeId == EntityTypeId;
            }
            else if ( obj is IEntityCache cachedEntity )
            {
                return cachedEntity.CachedEntityTypeId == EntityTypeId;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
