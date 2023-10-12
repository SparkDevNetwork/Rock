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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.EntitySearchDetail
{
    /// <summary>
    /// The item details for the Entity Search Detail block.
    /// </summary>
    public class EntitySearchBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the text that describes the purpose of this search.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Entity Type that will be queried by this search.
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// Gets or sets the expression that will be used to group the results.
        /// This is processed after Rock.Model.EntitySearch.WhereExpression.
        /// </summary>
        public string GroupByExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this search is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this search will entity
        /// enforce entity security. Entity security has a pretty heafty
        /// performance hit and should only be used when it is actually needed.
        /// </summary>
        public bool IsEntitySecurityEnforced { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether search query will allow
        /// custom refinement options in the form of an additional user query.
        /// </summary>
        public bool IsRefinementAllowed { get; set; }

        /// <summary>
        /// Gets or sets the  key of this search. This is used to identify
        /// this search item through the API and Lava. This value must be
        /// unique for a given Rock.Model.EntitySearch.EntityTypeId. This property
        /// is required.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of results per query. More data can
        /// be retrieved by subsequent queries that skip the first n items.
        /// </summary>
        public int? MaximumResultsPerQuery { get; set; }

        /// <summary>
        /// Gets or sets the name of the search query.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the expression that will be used to order the results.
        /// This is processed after Rock.Model.EntitySearch.SelectExpression.
        /// </summary>
        public string OrderByExpression { get; set; }

        /// <summary>
        /// Gets or sets the expression that will be used to define the structure
        /// of the resulting items. This is processed after Rock.Model.EntitySearch.GroupByExpression.
        /// </summary>
        public string SelectExpression { get; set; }

        /// <summary>
        /// Gets or sets the expression that will be used to filter the query.
        /// </summary>
        public string WhereExpression { get; set; }
    }
}
