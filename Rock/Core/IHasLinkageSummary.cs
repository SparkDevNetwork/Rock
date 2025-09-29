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

using Rock.Data;

namespace Rock.Core
{
    /// <summary>
    /// Indicates that an entity can provide a custom summary of how it should
    /// be displayed when showing that it is linked to another entity.
    /// </summary>
    public interface IHasLinkageSummary
    {
        /// <summary>
        /// The value to display as the summary text of the entity. This is
        /// typically the name or title of the entity.
        /// </summary>
        /// <param name="rockContext">The database context to use for any queries that might be needed to determine the summary value.</param>
        /// <returns>The summary text value to represent the entity.</returns>
        string SummaryValue( RockContext rockContext );

        /// <summary>
        /// Returns the parent entity that should be included when displaying
        /// the summary linkage. This should only return parents if the parent
        /// provides useful context for the entity. An example of when to do
        /// this would be a <see cref="Model.ContentChannelItem"/> returning
        /// its parent <see cref="Model.ContentChannel"/>.
        /// </summary>
        /// <param name="rockContext">The database context to use for any queries that might be needed to determine the parent entity.</param>
        /// <returns>The parent object as either an <see cref="Data.IEntity"/> or <see cref="Web.Cache.IEntityCache"/> or <c>null</c>.</returns>
        object SummaryParent( RockContext rockContext );
    }
}
