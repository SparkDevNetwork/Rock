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

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceLinkageList
{
    /// <summary>
    /// The additional configuration options for the Registration Instance Linkage List block.
    /// </summary>
    public class RegistrationInstanceLinkageListOptionsBag
    {
        /// <summary>
        /// Gets or sets the campus items.
        /// </summary>
        /// <value>
        /// The campus items.
        /// </value>
        public List<ListItemBag> CampusItems { get; set; }

        /// <summary>
        /// Gets or sets the registration template identifier key.
        /// </summary>
        /// <value>
        /// The registration template identifier key.
        /// </value>
        public string RegistrationTemplateIdKey { get; set; }

        /// <summary>
        /// Gets or sets the title for the exported excel or csv file.
        /// </summary>
        /// <value>
        /// The export title.
        /// </value>
        public string ExportTitle { get; set; }

        /// <summary>
        /// Gets or sets the name for the exported excel or csv file.
        /// </summary>
        /// <value>
        /// The name of the export file.
        /// </value>
        public string ExportFileName { get; set; }
    }
}
