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

using System.ComponentModel;

namespace Rock.Model
{
    /// <summary>
    /// Represents how a Group meets: in person, online, or a combination of the two.
    /// </summary>
    [Enums.EnumDomain( "Group" )]
    public enum MeetingStyle
    {
        /// <summary>
        /// The group meets in person at a physical location.
        /// </summary>
        [Description( "In-Person" )]
        InPerson = 1,

        /// <summary>
        /// The group meets online.
        /// </summary>
        [Description( "Online" )]
        Online = 2,

        /// <summary>
        /// The group meets both in person and online.
        /// </summary>
        [Description( "Hybrid" )]
        Hybrid = 3
    }
}
