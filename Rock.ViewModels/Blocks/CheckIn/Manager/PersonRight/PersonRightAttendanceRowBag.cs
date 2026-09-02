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
namespace Rock.ViewModels.Blocks.CheckIn.Manager.PersonRight
{
    /// <summary>
    /// A single row rendered in the recent-attendances grid.
    /// </summary>
    public class PersonRightAttendanceRowBag
    {
        /// <summary>
        /// Gets or sets the attendance IdKey, used to build the row's
        /// navigation URL to the Attendance Detail page.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the pre-rendered HTML for the "When" cell: check-in
        /// date over schedule name, plus an optional "by: {name}" link when
        /// the attendance was checked in by another person.
        /// </summary>
        public string WhenHtml { get; set; }

        /// <summary>
        /// Gets or sets the pre-rendered HTML for the "Location" cell:
        /// location name (as a link to the manager page when the attendance
        /// is currently active) over group name.
        /// </summary>
        public string LocationHtml { get; set; }

        /// <summary>
        /// Gets or sets the pre-rendered HTML for the "Code" cell: the
        /// attendance code plus a "Current" pill when the attendance is
        /// currently active.
        /// </summary>
        public string CodeHtml { get; set; }

        /// <summary>
        /// Gets or sets the search family / search-result group name shown
        /// in the "Search Family" cell.
        /// </summary>
        public string SearchResultGroupName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attendance is
        /// currently active. Drives the row's success styling.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the URL the row navigates to when clicked
        /// (Attendance Detail page for this attendance).
        /// </summary>
        public string RowUrl { get; set; }
    }
}
