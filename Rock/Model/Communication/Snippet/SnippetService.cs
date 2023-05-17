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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Rock.Model
{
    public partial class SnippetService
    {
        /// <summary>
        /// Gets the authorized snippets that match the query expression.
        /// </summary>
        /// <param name="currentPerson">The current person to use when checking authorization.</param>
        /// <param name="queryExpression">The query expression to use when filtering snippets.</param>
        /// <returns>An enumeration of <see cref="Snippet"/> objects that match the query and can be viewed by <paramref name="currentPerson"/>.</returns>
        internal IEnumerable<Snippet> GetAuthorizedSnippets( Person currentPerson, Expression<Func<Snippet, bool>> queryExpression )
        {
            var currentPersonId = currentPerson?.Id;

            // Use a custom select that includes the owner person id for performance
            // reasons. This way we don't need to include the entire PersonAlias
            // model and we don't need to lazy load it later to check if they are
            // the owner.
            return Queryable()
                .Where( queryExpression )
                .Where( s => !s.OwnerPersonAliasId.HasValue || s.OwnerPersonAlias.PersonId == currentPersonId )
                .Select( s => new
                {
                    Snippet = s,
                    OwnerPersonId = ( int? ) s.OwnerPersonAlias.PersonId
                } )
                .ToList()
                .Where( s => s.OwnerPersonId == currentPersonId || s.Snippet.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .Select( s => s.Snippet )
                .ToList();
        }
    }
}
