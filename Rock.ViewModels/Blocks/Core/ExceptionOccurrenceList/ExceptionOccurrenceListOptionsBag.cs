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

namespace Rock.ViewModels.Blocks.Core.ExceptionOccurrenceList
{
    /// <summary>
    /// The additional configuration options for the Exception Occurrence List block.
    /// </summary>
    public class ExceptionOccurrenceListOptionsBag
    {
        /// <summary>
        /// Gets or sets the exception type name from the template exception
        /// (e.g., "System.Web.HttpException").
        /// </summary>
        public string ExceptionType { get; set; }

        /// <summary>
        /// Gets or sets the full description from the template exception.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the list of available sites for the filter dropdown.
        /// </summary>
        public List<ListItemBag> SiteItems { get; set; }
    }
}
