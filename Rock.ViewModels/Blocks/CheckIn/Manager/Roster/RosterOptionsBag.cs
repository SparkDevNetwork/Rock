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
namespace Rock.ViewModels.Blocks.CheckIn.Manager.Roster
{
    /// <summary>
    /// The initialization options for the Roster block.
    /// </summary>
    public class RosterOptionsBag
    {
        /// <summary>
        /// The error message to display. If this is set, then the block should
        /// not attempt to load any other data.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Determines if the group column should be displayed when a single
        /// schedule is selected.
        /// </summary>
        public bool IsGroupColumnEnabled { get; set; }

        /// <summary>
        /// Determines if the checkout all button should be displayed.
        /// </summary>
        public bool IsCheckoutAllEnabled { get; set; }

        /// <summary>
        /// Determines if the "mark all present" button should be displayed.
        /// </summary>
        public bool IsMarkAllPresentEnabled { get; set; }

        /// <summary>
        /// Determines if the "staying for service" button should be displayed.
        /// </summary>
        public bool IsStayingButtonEnabled { get; set; }

        /// <summary>
        /// Determines if the "not present" button should be displayed to mark
        /// attendees that are present as no longer present.
        /// </summary>
        public bool IsNotPresentButtonEnabled { get; set; }

        /// <summary>
        /// Determines if the "present" button should be displayed on the
        /// checked-out roster view. The button will always be displayed on the
        /// checked-in roster view.
        /// </summary>
        public bool IsPresentButtonEnabled { get; set; }

        /// <summary>
        /// Determines if the "delete" button should be displayed to allow
        /// deleting an attendance record entirely.
        /// </summary>
        public bool IsDeleteButtonEnabled { get; set; }

        /// <summary>
        /// The URL template to use for person profile links.
        /// </summary>
        public string PersonPageUrl { get; set; }
    }
}
