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
namespace Rock.Model
{
    public partial class TaggedItem
    {
        /// <summary>
        /// Gets the parent security authority for this TagItem
        /// </summary>
        /// <value>
        /// An entity that implements the <see cref="Rock.Security.ISecured"/> interface that this TagItem inherits security authority from.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.Tag != null ? this.Tag : base.ParentAuthority;
            }
        }

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        /// <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsAuthorized( string action, Person person )
        {
            if ( this.Tag?.OwnerPersonAlias != null && person != null && this.Tag?.OwnerPersonAlias.PersonId == person.Id )
            {
                // always allow people to do anything with their own tags
                return true;
            }
            else if ( this.Tag?.OwnerPersonAlias != null && person != null && this.Tag?.OwnerPersonAlias.PersonId != person.Id )
            {
                // always prevent people from doing anything with someone else's tags
                return false;
            }
            else
            {
                return base.IsAuthorized( action, person );
            }
        }
    }
}
