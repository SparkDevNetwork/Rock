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

using System.ComponentModel.DataAnnotations.Schema;

namespace Rock.Model
{
    public partial class Snippet
    {
        #region Properties

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
                return this.SnippetType != null ? this.SnippetType : base.ParentAuthority;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified action is authorized. If the Snippet is personal and
        /// owned by the person, it returns true, but if it's personal and NOT owned by the person
        /// it returns false -- otherwise (not a personal Snippet) it returns what the chain of authority
        ///  determines.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>True if the person is authorized; false otherwise.</returns>
        public override bool IsAuthorized( string action, Person person )
        {
            if ( OwnerPersonAlias != null && person != null && OwnerPersonAlias.PersonId == person.Id )
            {
                // always allow people to do anything with their own snippets
                return true;
            }
            else if ( OwnerPersonAlias != null && person != null && OwnerPersonAlias.PersonId != person.Id )
            {
                // always prevent people from doing anything with someone else's snippets
                return false;
            }
            else
            {
                return base.IsAuthorized( action, person );
            }
        }

        #endregion
    }
}
