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

namespace Rock.ViewModels.Blocks.Lms.LearningParticipantDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class LearningParticipantBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the number of absences for this participant in this class.
        /// If the Class doesn't take attendance null will be returned.
        /// </summary>
        public int? Absences { get; set; }

        /// <summary>
        /// Gets or sets the label style for the students absences in the class.
        /// </summary>
        public string AbsencesLabelStyle { get; set; }

        /// <summary>
        /// Gets or sets the grade percent achieved for this participant.
        /// </summary>
        public decimal CurrentGradePercent { get; set; }

        /// <summary>
        /// Gets or sets the text for the currently achieved Rock.Model.LearningGradingSystemScale.
        /// </summary>
        public string CurrentGradeText { get; set; }

        /// <summary>
        /// Gets or sets whether the particpiant is a facilitator.
        /// </summary>
        public bool IsFacilitator { get; set; }

        /// <summary>
        /// Gets or sets the (group type) role of the participant.
        /// </summary>
        public ListItemBag ParticipantRole { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Person representing the GroupMember.
        /// </summary>
        public ListItemBag PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the shcedule time text for the participant detail.
        /// </summary>
        public string ScheduledTime { get; set; }

        /// <summary>
        /// Gets or sets the semester name for the participant detail.
        /// </summary>
        public string SemesterName { get; set; }
    }
}
