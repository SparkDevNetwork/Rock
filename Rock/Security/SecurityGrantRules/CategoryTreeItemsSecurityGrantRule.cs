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

namespace Rock.Security.SecurityGrantRules
{
    /// <summary>
    /// Grants permission for a category tree to load the categorized items
    /// of a single entity type. Each item is still checked for VIEW access.
    /// </summary>
    [Rock.SystemGuid.SecurityGrantRuleGuid( "233EF5E6-2910-4C81-B732-9B35BAE5E9BF" )]
    internal sealed class CategoryTreeItemsSecurityGrantRule : SecurityGrantRule
    {
        #region Properties

        /// <summary>
        /// Gets the entity type identifier whose categorized items may be loaded.
        /// </summary>
        /// <value>The entity type identifier whose categorized items may be loaded.</value>
        [JsonProperty( "et", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public int EntityTypeId { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="CategoryTreeItemsSecurityGrantRule"/> class from being created.
        /// </summary>
        private CategoryTreeItemsSecurityGrantRule()
            : base( Authorization.VIEW )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryTreeItemsSecurityGrantRule"/> class
        /// for granting <see cref="Authorization.VIEW"/> access.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier whose categorized items may be loaded.</param>
        public CategoryTreeItemsSecurityGrantRule( int entityTypeId )
            : base( Authorization.VIEW )
        {
            EntityTypeId = entityTypeId;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool IsAccessGranted( object obj, string action )
        {
            // Only our own access object for the same entity type is allowed.
            return obj is CategoryTreeItemsAccess access
                && access.EntityTypeId == EntityTypeId;
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The object that is checked against this rule when a category tree
        /// requests the categorized items of an entity type.
        /// </summary>
        internal sealed class CategoryTreeItemsAccess
        {
            /// <summary>
            /// Gets the entity type identifier whose categorized items are requested.
            /// </summary>
            /// <value>The entity type identifier whose categorized items are requested.</value>
            public int EntityTypeId { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CategoryTreeItemsAccess"/> class.
            /// </summary>
            /// <param name="entityTypeId">The entity type identifier whose categorized items are requested.</param>
            public CategoryTreeItemsAccess( int entityTypeId )
            {
                EntityTypeId = entityTypeId;
            }
        }

        #endregion
    }
}
