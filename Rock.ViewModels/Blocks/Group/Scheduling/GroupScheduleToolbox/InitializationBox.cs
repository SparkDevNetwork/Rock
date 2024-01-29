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

using Rock.Enums.Blocks.Group.Scheduling;
using Rock.ViewModels.Utility;

using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox
{
    /// <summary>
    /// The box that contains all the initialization information for the group schedule toolbox block.
    /// </summary>
    public class InitializationBox : BlockBox
    {
        #region Additional Time Sign-Ups

        /// <summary>
        /// Gets or sets whether the "additional time sign-ups" feature is enabled.
        /// </summary>
        public bool IsAdditionalTimeSignUpsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the additional time sign-ups button text.
        /// </summary>
        public string AdditionalTimeSignUpsButtonText { get; set; }

        /// <summary>
        /// Gets or sets the additional time sign-ups header HTML.
        /// </summary>
        public string AdditionalTimeSignUpsHeaderHtml { get; set; }

        /// <summary>
        /// Gets or sets the immediate needs title.
        /// </summary>
        public string ImmediateNeedsTitle { get; set; }

        /// <summary>
        /// Gets or sets the immediate needs introduction.
        /// </summary>
        public string ImmediateNeedsIntroduction { get; set; }

        #endregion Additional Time Sign-Ups

        #region Current Schedule

        /// <summary>
        /// Gets or sets the current schedule button text.
        /// </summary>
        public string CurrentScheduleButtonText { get; set; }

        /// <summary>
        /// Gets or sets the current schedule header HTML.
        /// </summary>
        public string CurrentScheduleHeaderHtml { get; set; }

        #endregion Current Schedule

        #region Schedule Preferences

        /// <summary>
        /// Gets or sets whether the "schedule preferences" feature is enabled.
        /// </summary>
        public bool IsSchedulePreferencesEnabled { get; set; }

        /// <summary>
        /// Gets or sets the schedule preferences button text.
        /// </summary>
        public string SchedulePreferencesButtonText { get; set; }

        /// <summary>
        /// Gets or sets the schedule preferences header HTML.
        /// </summary>
        public string SchedulePreferencesHeaderHtml { get; set; }

        #endregion Schedule Preferences

        #region Schedule Unavailability

        /// <summary>
        /// Gets or sets whether the "schedule unavailability" feature is enabled.
        /// </summary>
        public bool IsScheduleUnavailabilityEnabled { get; set; }

        /// <summary>
        /// Gets or sets the schedule unavailability button text.
        /// </summary>
        public string ScheduleUnavailabilityButtonText { get; set; }

        /// <summary>
        /// Gets or sets the schedule unavailability header HTML.
        /// </summary>
        public string ScheduleUnavailabilityHeaderHtml { get; set; }

        #endregion Schedule Unavailability

        #region Shared Settings (Applies to Multiple Panels)

        /// <summary>
        /// Gets or sets the action header HTML.
        /// </summary>
        public string ActionHeaderHtml { get; set; }

        /// <summary>
        /// Gets or sets the toolbox action type.
        /// </summary>
        public ToolboxActionType ToolboxActionType { get; set; }

        /// <summary>
        /// Gets or sets the toolbox person's schedulable family member people. Note that the
        /// toolbox person themself will always be the first person in this list, regardless
        /// of whether or not they belong to any schedulable groups.
        /// </summary>
        public List<ListItemBag> SchedulableFamilyMembers { get; set; }

        #endregion Shared Settings (Applies to Multiple Panels)
    }
}
