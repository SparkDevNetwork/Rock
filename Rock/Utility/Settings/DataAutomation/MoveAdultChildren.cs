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

namespace Rock.Utility.Settings.DataAutomation
{
 
    /// <summary>
    /// Settings for controlling how adult children are moved to their own family
    /// </summary>
    public class MoveAdultChildren
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InactivatePeople"/> class.
        /// </summary>
        public MoveAdultChildren()
        {
            AdultAge = 18;
            UseSameHomeAddress = true;
            UseSameHomePhone = true;
            MaximumRecords = 200;
            WorkflowTypeIds = new List<int>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the adult age.
        /// </summary>
        /// <value>
        /// The adult age.
        /// </value>
        public int AdultAge { get; set; }

        /// <summary>
        /// Gets or sets the parent relationship identifier.
        /// </summary>
        /// <value>
        /// The parent relationship identifier.
        /// </value>
        public int? ParentRelationshipId { get; set; }

        /// <summary>
        /// Gets or sets the sibling relationship identifier.
        /// </summary>
        /// <value>
        /// The sibling relationship identifier.
        /// </value>
        public int? SiblingRelationshipId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use same home address].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use same home address]; otherwise, <c>false</c>.
        /// </value>
        public bool UseSameHomeAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use same home phone].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use same home phone]; otherwise, <c>false</c>.
        /// </value>
        public bool UseSameHomePhone { get; set; }

        /// <summary>
        /// Gets or sets the workflow type ids.
        /// </summary>
        /// <value>
        /// The workflow type ids.
        /// </value>
        public List<int> WorkflowTypeIds { get; set; }

        /// <summary>
        /// Gets or sets the maximum records.
        /// </summary>
        /// <value>
        /// The maximum records.
        /// </value>
        public int MaximumRecords { get; set; }


    }

}
