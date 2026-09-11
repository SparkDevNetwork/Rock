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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupMemberList
{
    /// <summary>
    /// The additional configuration options for the Group Member List block.
    /// </summary>
    public class GroupMemberListOptionsBag
    {
        /// <summary>
        /// Gets or sets the title displayed in the panel header.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the hashed identifier of the group being listed.
        /// </summary>
        public string GroupIdKey { get; set; }

        /// <summary>
        /// Gets or sets the term that names a single row.
        /// </summary>
        public string ItemTerm { get; set; }

        /// <summary>
        /// Gets or sets the name given to the exported file.
        /// </summary>
        public string ExportTitle { get; set; }

        /// <summary>
        /// Gets or sets the warning message shown in place of the grid.
        /// </summary>
        public string WarningMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grid should be rendered.
        /// </summary>
        public bool IsGridVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Date Added column is shown on screen. The
        /// column is exported either way.
        /// </summary>
        public bool IsDateAddedColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Note column is shown on screen. The column
        /// is exported either way.
        /// </summary>
        public bool IsNoteColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Marital Status column is shown on screen. The
        /// column is exported either way.
        /// </summary>
        public bool IsMaritalStatusColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Gender column is shown on screen. The
        /// column is exported either way.
        /// </summary>
        public bool IsGenderColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the First Attended and Last Attended columns
        /// are shown. Neither column is exported.
        /// </summary>
        public bool IsAttendanceColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Registration column is shown on screen, which takes a listed
        /// member who was added to the group through a registration. The column is never exported.
        /// </summary>
        public bool IsRegistrationColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the filter modal offers the Family Campus filter.
        /// </summary>
        public bool IsCampusFilterVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the filter modal offers the Signed Document filter, which
        /// takes a group that requires a signature document.
        /// </summary>
        public bool IsSignedDocumentFilterVisible { get; set; }

        /// <summary>
        /// Gets or sets the registration instances the group is linked to, which the filter modal's Registration
        /// filter chooses from. The filter is offered only when there is at least one.
        /// </summary>
        public List<ListItemBag> RegistrationInstances { get; set; }
    }
}
