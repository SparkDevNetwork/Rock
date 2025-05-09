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

using Rock.Model;

namespace Rock.Core.EntitySearch
{
    /// <summary>
    /// Defines the structure of a <see cref="Rock.Model.EntitySearch"/> query
    /// or a <see cref="Rock.Web.Cache.EntitySearchCache"/> query.
    /// </summary>
    internal class EntitySearchSystemQuery
    {
        /// <summary>
        /// Gets or sets the expression that will be used to filter the query.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>Where()</c> expression.</value>
        public string WhereExpression { get; set; }

        /// <summary>
        /// Gets or sets the expression that will be used to group the results.
        /// This is processed after <see cref="WhereExpression"/>.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>GroupBy()</c> expression.</value>
        public string GroupByExpression { get; set; }

        /// <summary>
        /// Gets or sets the expression that will be used to define the structure
        /// of the resulting items. This is processed after <see cref="GroupByExpression"/>.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>Select()</c> expression.</value>
        public string SelectExpression { get; set; }

        /// <summary>
        /// Gets or sets the expression that will be used to define the structure
        /// of the resulting items. This is processed after <see cref="GroupByExpression"/>
        /// and instead of <see cref="SelectExpression"/>.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>SelectMany()</c> expression.</value>
        public string SelectManyExpression { get; set; }

        /// <summary>
        /// Gets or sets the expression that will be used to sort the results.
        /// This is processed after <see cref="SelectExpression"/>.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>OrderBy()</c> expression.</value>
        public string SortExpression { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of results per query. More data can
        /// be retrieved by subsequent queries that skip the first n items.
        /// </summary>
        /// <value>An optional <see cref="int"/> containing the maximum number of results per query.</value>
        public int? MaximumResultsPerQuery { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this search will
        /// enforce entity security. Entity security has a pretty heafty
        /// performance hit and should only be used when it is actually needed.
        /// </summary>
        /// <value><c>true</c> if this search will enforce entity security; otherwise, <c>false</c>.</value>
        public bool IsEntitySecurityEnabled { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the property paths to be included by Entity Framework.
        /// This is only valid when <see cref="IsEntitySecurityEnabled"/> is <c>true</c>.
        /// </para>
        /// <para>
        /// Example: <c>GroupType,Members.Person</c>
        /// </para>
        /// </summary>
        /// <value>The property paths to include as a comma seperated list.</value>
        public string IncludePaths { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether search query will allow
        /// custom refinement options in the form of an additional user query.
        /// </summary>
        /// <value><c>true</c> if this query allows refinement; otherwise, <c>false</c>.</value>
        public bool IsRefinementAllowed { get; set; }

        /// <summary>
        /// Gets or sets the current person that is requesting this query be executed.
        /// </summary>
        /// <value>An optional <see cref="Person"/> object representing the current person.</value>
        public Person CurrentPerson { get; set; }
    }
}
