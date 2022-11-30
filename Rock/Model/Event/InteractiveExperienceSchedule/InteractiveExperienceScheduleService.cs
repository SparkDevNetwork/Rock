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

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class InteractiveExperienceScheduleService
    {
        /// <summary>
        /// Checks if the schedule requirements have been met by the
        /// specified person.
        /// </summary>
        /// <param name="interactiveExperienceScheduleId">The identifier of the <see cref="InteractiveExperienceSchedule"/> the person should be checked against.</param>
        /// <param name="person">The <see cref="Person"/> to be checked, this may be <c>null</c>.</param>
        /// <returns><c>true</c> if the <paramref name="person"/> passes requirements of the schedule; otherwise <c>false</c>.</returns>
        internal static bool AreRequirementsMetForPerson( int interactiveExperienceScheduleId, Person person )
        {
            var experienceSchedule = InteractiveExperienceScheduleCache.Get( interactiveExperienceScheduleId );

            if ( experienceSchedule == null )
            {
                return false;
            }

            // Early check, if there is no filtering then they can join and
            // we don't need to create a RockContext.
            if ( !experienceSchedule.GroupId.HasValue && !experienceSchedule.DataViewId.HasValue )
            {
                return true;
            }

            using ( var rockContext = new RockContext() )
            {
                // If the schedule has a group then the person must be an active
                // member of the group. Check this first since it is a cheaper
                // operation than running a data view.
                if ( experienceSchedule.GroupId.HasValue )
                {
                    if ( person == null )
                    {
                        return false;
                    }

                    var isActiveMember = new GroupMemberService( rockContext ).Queryable()
                        .Where( gm => gm.GroupId == experienceSchedule.GroupId.Value
                            && gm.PersonId == person.Id
                            && gm.GroupMemberStatus == GroupMemberStatus.Active
                            && !gm.IsArchived )
                        .Any();

                    if ( !isActiveMember )
                    {
                        return false;
                    }
                }

                // If the schedule has a dataview then the person must be a
                // member of that dataview.
                if ( experienceSchedule.DataViewId.HasValue )
                {
                    if ( person == null )
                    {
                        return false;
                    }

                    var dataView = new DataViewService( rockContext ).Get( experienceSchedule.DataViewId.Value );
                    var personService = new PersonService( rockContext );
                    var inDataView = personService.GetQueryUsingDataView( dataView )
                        .Where( p => p.Id == person.Id )
                        .Any();

                    if ( !inDataView )
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
