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
        /// Gets or sets the term that names a single row.
        /// </summary>
        public string ItemTerm { get; set; }

        /// <summary>
        /// Gets or sets the warning message shown in place of the grid.
        /// </summary>
        public string WarningMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grid should be rendered.
        /// </summary>
        public bool IsGridVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Date Added column is shown.
        /// </summary>
        public bool IsDateAddedColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Note column is shown.
        /// </summary>
        public bool IsNoteColumnVisible { get; set; }
    }
}
