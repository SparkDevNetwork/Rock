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

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceFeeList
{
    /// <summary>
    /// The additional configuration options for the Registration Instance Fee List block.
    /// </summary>
    public class RegistrationInstanceFeeListOptionsBag
    {
        /// <summary>
        /// Gets or sets the title of the export file.
        /// </summary>
        /// <value>
        /// The name of the export title.
        /// </value>
        public string ExportTitleName { get; set; }

        /// <summary>
        /// Gets or sets the file name of the export file.
        /// </summary>
        /// <value>
        /// The name of the export file.
        /// </value>
        public string ExportFileName { get; set; }

        /// <summary>
        /// Gets or sets the fee name items for the fee name filter dropdown.
        /// </summary>
        /// <value>
        /// The fee name items.
        /// </value>
        public List<ListItemBag> FeeNameItems { get; set; }

        /// <summary>
        /// Gets or sets the fee options items for the fee options filter dropdown.
        /// </summary>
        /// <value>
        /// The fee option items.
        /// </value>
        public List<ListItemBag> FeeOptionsItems { get; set; }
    }
}
