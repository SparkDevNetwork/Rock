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

using Rock.Security;

namespace Rock.Model
{
    public partial class EmailSection
    {
        /// <summary>
        /// Determines whether the specified action is authorized for the given person.
        /// </summary>
        /// <param name="action">The action being requested.</param>
        /// <param name="person">The person requesting authorization.</param>
        /// <returns><c>true</c> if the specified action is authorized; otherwise, <c>false</c>.</returns>
        public override bool IsAuthorized( string action, Person person )
        {
            if ( !IsSystem
                && person != null
                && CreatedByPersonAlias?.PersonId == person.Id
                && ( action == Authorization.EDIT || action == Authorization.DELETE ) )
            {
                // Allow people to manage the personal sections they created from the email builder.
                return true;
            }

            return base.IsAuthorized( action, person );
        }
    }
}
