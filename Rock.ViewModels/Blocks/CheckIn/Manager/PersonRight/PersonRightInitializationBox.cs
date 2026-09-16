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
using System.Collections.Generic;

using Rock.ViewModels.Crm;

namespace Rock.ViewModels.Blocks.CheckIn.Manager.PersonRight
{
    /// <summary>
    /// Initialization payload for the Check-in Manager Person Recent
    /// Attendances (right side) block.
    /// </summary>
    public class PersonRightInitializationBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block content should be
        /// rendered. False when no person could be resolved or the current user
        /// is not authorized to view the block.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the pre-rendered HTML for the gender letter widget
        /// (e.g. an "M" or "F" inside the WebForms-styled markup). Empty when
        /// the person's gender is Unknown.
        /// </summary>
        public string GenderHtml { get; set; }

        /// <summary>
        /// Gets or sets the pre-rendered HTML for the age widget (e.g.
        /// "43yrs" over the short birth-date). Empty when the person has no
        /// birth date.
        /// </summary>
        public string AgeHtml { get; set; }

        /// <summary>
        /// Gets or sets the pre-rendered HTML for the grade widget (e.g.
        /// "4th" over "Grade" or "1" over "Year"). Empty when the person has
        /// no formatted grade.
        /// </summary>
        public string GradeHtml { get; set; }

        /// <summary>
        /// Gets or sets the rendered badge content for the left badge zone.
        /// </summary>
        public List<RenderedBadgeBag> LeftBadges { get; set; }

        /// <summary>
        /// Gets or sets the rendered badge content for the right badge zone.
        /// </summary>
        public List<RenderedBadgeBag> RightBadges { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Reprint Labels button
        /// should be visible.
        /// </summary>
        public bool IsReprintLabelsVisible { get; set; }

        /// <summary>
        /// Gets or sets the URL of the Attendance History linked page. Null
        /// hides the Attendance History button.
        /// </summary>
        public string AttendanceHistoryUrl { get; set; }

        /// <summary>
        /// Gets or sets the recent attendance rows shown in the check-in
        /// history grid. Empty hides the whole grid panel.
        /// </summary>
        public List<PersonRightAttendanceRowBag> Attendances { get; set; }
    }
}
