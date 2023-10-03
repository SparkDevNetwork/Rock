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
using System.Linq;
using System.Web;
using Rock.Data;
using Z.EntityFramework.Plus;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for <see cref="Rock.Model.GroupMember"/> entity objects. 
    /// </summary>
    public partial class GroupMemberService
    {
        /// <summary>
        /// Archives the specified group member with an option to null the GroupMemberId from Registrant tables
        /// </summary>
        /// <param name="groupMember">The group member.</param>
        /// <param name="currentPersonAliasId">The current person alias identifier (leave null to have Rock figure it out)</param>
        /// <param name="removeFromRegistrants">if set to <c>true</c> [remove from registrants].</param>
        public void Archive( GroupMember groupMember, int? currentPersonAliasId, bool removeFromRegistrants )
        {
            if ( removeFromRegistrants )
            {
                RegistrationRegistrantService registrantService = new RegistrationRegistrantService( this.Context as RockContext );
                foreach ( var registrant in registrantService.Queryable().Where( r => r.GroupMemberId == groupMember.Id ) )
                {
                    registrant.GroupMemberId = null;
                }
            }

            if ( !currentPersonAliasId.HasValue )
            {
                if ( HttpContext.Current != null && HttpContext.Current.Items.Contains( "CurrentPerson" ) )
                {
                    currentPersonAliasId = ( HttpContext.Current.Items["CurrentPerson"] as Person )?.PrimaryAliasId;
                }
            }

            groupMember.IsArchived = true;
            groupMember.ArchivedByPersonAliasId = currentPersonAliasId;
            groupMember.ArchivedDateTime = RockDateTime.Now;
        }
    }
}
