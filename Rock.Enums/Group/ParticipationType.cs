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
    /// <summary>
    /// Represents and indicates the participation type or mode used in a Fundraising Opportunity <see cref="Rock.Model.GroupType"/> to determine the way contributions and participants are displayed.
    /// </summary>
    [Enums.EnumDomain( "Group" )]
    public enum ParticipationType
    {
        /// <summary>
        /// Participation for this fundraising opportunity is for individuals.
        /// </summary>
        Individual = 1,

        /// <summary>
        /// Participation for this fundraising opportunity is for families.
        /// </summary>
        Family = 2
    }
}