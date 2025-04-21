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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupMemberScheduleTemplateDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupMemberScheduleTemplateBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Calendar Content for the attached Rock.Model.Schedule
        /// </summary>
        public string CalendarContent { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Schedule, which indicates the Schedule that a GroupMember is associated with (Every Week, Every Other Week, etc)
        /// </summary>
        public string Schedule { get; set; }
    }
}
