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

namespace Rock.Model
{
    /// <summary>
    /// Represents and indicates the  attendance rule to use when a Rock.Model.Person checks in to a Rock.Model.Group of this Rock.Model.GroupType
    /// </summary>
    [Enums.EnumDomain( "Group" )]
    public enum AttendanceRule
    {
        /// <summary>
        /// None, person does not need to belong to the group, and they will not automatically 
        /// be added to the group
        /// </summary>
        None = 0,

        /// <summary>
        /// Person will be added to the group whenever they check-in
        /// </summary>
        AddOnCheckIn = 1,

        /// <summary>
        /// User must already belong to the group before they will be allowed to check-in
        /// </summary>
        [Obsolete( "Use AlreadyEnrolledInGroup instead." )]
        AlreadyBelongs = 2,

        /// <summary>
        /// The person must already be a member of the group before they will
        /// be allowed to check into the group.
        /// </summary>
        AlreadyEnrolledInGroup = 2
    }
}
