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
using System.Threading.Tasks;

using Rock.ViewModels.Event.RegistrationEntry;
using Rock.ViewModels.Group.GroupMember;

namespace Rock.RealTime.Topics
{
    /// <summary>
    /// A topic interface for sending messages to clients that are monitoring
    /// for changes with Group Placements.
    /// </summary>
    internal interface IGroupPlacement
    {
        /// <summary>
        /// Called when an Registrant is created or updated in a way that
        /// would change the values of the message bag.
        /// </summary>
        /// <param name="bag">The message bag that represents the registrant.</param>
        Task RegistrantUpdated( RegistrationRegistrantUpdatedMessageBag bag );

        /// <summary>
        /// Called when a Registrant has been deleted.
        /// </summary>
        /// <param name="groupMemberGuid">The registrant's unique identifier.</param>
        /// <param name="bag">The message bag that represents the registrant, may be <c>null</c> in some cases.</param>
        Task RegistrantDeleted( Guid groupMemberGuid, RegistrationRegistrantUpdatedMessageBag bag );

        /// <summary>
        /// Called when an Group Member is created or updated in a way that
        /// would change the values of the message bag.
        /// </summary>
        /// <param name="bag">The message bag that represents the group member.</param>
        Task GroupMemberUpdated( GroupMemberUpdatedMessageBag bag );

        /// <summary>
        /// Called when a Group Member has been deleted.
        /// </summary>
        /// <param name="groupMemberGuid">The group member's unique identifier.</param>
        /// <param name="bag">The message bag that represents the group member, may be <c>null</c> in some cases.</param>
        Task GroupMemberDeleted( Guid groupMemberGuid, GroupMemberUpdatedMessageBag bag );
    }
}
