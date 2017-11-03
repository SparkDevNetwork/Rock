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

namespace Rock.Slingshot.Model
{
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay( "{Name}" )]
    public class GroupImport
    {
        /// <summary>
        /// Gets or sets the group foreign identifier.
        /// </summary>
        /// <value>
        /// The group foreign identifier.
        /// </value>
        public int GroupForeignId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the parent group foreign identifier.
        /// </summary>
        /// <value>
        /// The parent group foreign identifier.
        /// </value>
        public int? ParentGroupForeignId { get; set; }

        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the group members.
        /// </summary>
        /// <value>
        /// The group members.
        /// </value>
        public List<GroupMemberImport> GroupMemberImports { get; set; }
    }
}
