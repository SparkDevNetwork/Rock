// <copyright>
// Copyright by BEMA Software Services
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
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Model;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class ReservationTypeService : Service<ReservationType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationTypeService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ReservationTypeService( RockContext context ) : base( context ) { }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( ReservationType item, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( new Service<Reservation>( Context ).Queryable().Any( a => a.ReservationTypeId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", ReservationType.FriendlyTypeName, Reservation.FriendlyTypeName );
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the given person is active in the given group.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <returns>
        ///   <c>true</c> if [is person in group with identifier] [the specified person]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPersonInGroupWithId( Person person, int? groupId )
        {
            bool isInGroup = false;
            if ( groupId != null && person != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var groupMemberService = new GroupMemberService( rockContext );
                    if ( groupMemberService.GetByGroupIdAndPersonId( groupId.Value, person.Id, true )
                        .AsNoTracking().Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active && gm.Group.IsActive == true ).Any() )
                    {
                        isInGroup = true;
                    }
                }
            }

            return isInGroup;
        }

        /// <summary>
        /// Determines whether the given person is active in the given group.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <returns>
        ///   <c>true</c> if [is person in group with unique identifier] [the specified person]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPersonInGroupWithGuid( Person person, Guid? groupGuid )
        {
            bool isInGroup = false;
            if ( groupGuid != null && person != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var groupMemberService = new GroupMemberService( rockContext );
                    if ( groupMemberService.Queryable().AsNoTracking()
                        .Where( gm => gm.Group.Guid == groupGuid && gm.PersonId == person.Id && gm.GroupMemberStatus == GroupMemberStatus.Active && gm.Group.IsActive == true ).Any() )
                    {
                        isInGroup = true;
                    }
                }
            }

            return isInGroup;
        }

    }
}
