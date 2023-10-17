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
using System.Linq;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cached representation of <see cref="Rock.Model.EntitySearch"/>.
    /// </summary>
    public class EntitySearchCache : ModelCache<EntitySearchCache, EntitySearch>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the search query.
        /// </summary>
        /// <value>A <see cref="string"/> that represents the name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the Id of the <see cref="Rock.Model.EntityType"/> that
        /// will be targeted by this search. This property is required.
        /// </summary>
        /// <value>An <see cref="int"/> representing the Id</value>
        public int EntityTypeId { get; private set; }

        /// <summary>
        /// Gets the  key of this search. This is used to identify
        /// this search item through the API and Lava. This value must be
        /// unique for a given <see cref="EntityTypeId"/>. This property
        /// is required.
        /// </summary>
        /// <value>A <see cref="string"/> that represents the key.</value>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the text that describes the purpose of this search.
        /// </summary>
        /// <value>A <see cref="string"/> that describes the search.</value>
        public string Description { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this search is active.
        /// </summary>
        /// <value><c>true</c> if this search is active; otherwise, <c>false</c>.</value>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// Gets the expression that will be used to filter the query.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>Where()</c> expression.</value>
        public string WhereExpression { get; private set; }

        /// <summary>
        /// Gets the expression that will be used to group the results.
        /// This is processed after <see cref="WhereExpression"/>.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>GroupBy()</c> expression.</value>
        public string GroupByExpression { get; private set; }

        /// <summary>
        /// Gets the expression that will be used to define the structure
        /// of the resulting items. This is processed after <see cref="GroupByExpression"/>.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>Select()</c> expression.</value>
        public string SelectExpression { get; private set; }

        /// <summary>
        /// Gets the expression that will be used to define the structure
        /// of the resulting items. This is processed after <see cref="GroupByExpression"/>
        /// and instead of <see cref="SelectExpression"/>.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>SelectMany()</c> expression.</value>
        public string SelectManyExpression { get; private set; }

        /// <summary>
        /// Gets the expression that will be used to order the results.
        /// This is processed after <see cref="SelectExpression"/>.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>OrderBy()</c> expression.</value>
        public string OrderByExpression { get; private set; }

        /// <summary>
        /// Gets the maximum number of results per query. More data can
        /// be retrieved by subsequent queries that skip the first n items.
        /// </summary>
        /// <value>An optional <see cref="int"/> containing the maximum number of results per query.</value>
        public int? MaximumResultsPerQuery { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this search will entity
        /// enforce entity security. Entity security has a pretty heafty
        /// performance hit and should only be used when it is actually needed.
        /// </summary>
        /// <value><c>true</c> if this search will enforce entity security; otherwise, <c>false</c>.</value>
        public bool IsEntitySecurityEnforced { get; private set; }

        /// <summary>
        /// <para>
        /// Gets the property paths to be included by Entity Framework.
        /// This is only valid when <see cref="IsEntitySecurityEnforced"/> is <c>true</c>.
        /// </para>
        /// <para>
        /// Example: <c>GroupType,Members.Person</c>
        /// </para>
        /// </summary>
        /// <value>The property paths to include as a comma seperated list.</value>
        public string IncludePaths { get; private set; }

        /// <summary>
        /// Gets a value indicating whether search query will allow
        /// custom refinement options in the form of an additional user query.
        /// </summary>
        /// <value><c>true</c> if this query allows refinement; otherwise, <c>false</c>.</value>
        public bool IsRefinementAllowed { get; private set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the Entity Type that will be queried by this search.
        /// </summary>
        /// <value>
        /// The <see cref="EntityTypeCache"/> that will be queried by this search.
        /// </value>
        public EntityTypeCache EntityType => EntityTypeCache.Get( EntityTypeId );

        #endregion

        #region Public Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is EntitySearch entitySearch ) )
            {
                return;
            }

            Name = entitySearch.Name;
            EntityTypeId = entitySearch.EntityTypeId;
            Key = entitySearch.Key;
            Description = entitySearch.Description;
            IsActive = entitySearch.IsActive;
            WhereExpression = entitySearch.WhereExpression;
            GroupByExpression = entitySearch.GroupByExpression;
            SelectExpression = entitySearch.SelectExpression;
            SelectManyExpression = entitySearch.SelectManyExpression;
            OrderByExpression = entitySearch.OrderByExpression;
            MaximumResultsPerQuery = entitySearch.MaximumResultsPerQuery;
            IsEntitySecurityEnforced = entitySearch.IsEntitySecurityEnforced;
            IncludePaths = entitySearch.IncludePaths;
            IsRefinementAllowed = entitySearch.IsRefinementAllowed;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the entity search cache object that matches the entity type
        /// and search key.
        /// </summary>
        /// <param name="entityTypeCache">The <see cref="EntityTypeCache"/> object that identifies the type of entity search.</param>
        /// <param name="searchKey">The key that identifies the entity search within the entity type.</param>
        /// <returns>A reference to the <see cref="EntitySearchCache"/> or <c>null</c> if not found.</returns>
        public static EntitySearchCache GetByEntityTypeAndKey( EntityTypeCache entityTypeCache, string searchKey )
        {
            return All()
                .Where( es => es.EntityTypeId == entityTypeCache.Id
                    && es.Key.Equals( searchKey, StringComparison.OrdinalIgnoreCase ) )
                .FirstOrDefault();
        }

        #endregion
    }
}