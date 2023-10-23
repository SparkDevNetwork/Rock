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

using System.Collections.Generic;

namespace Rock.ViewModels.Core.Grid
{
    /// <summary>
    /// The data that describes the contents of a grid.
    /// </summary>
    public class GridDataBag
    {
        /// <summary>
        /// Gets or sets the row data to display in the grid.
        /// </summary>
        /// <value>The row data to display in the grid.</value>
        public List<Dictionary<string, object>> Rows { get; set; }
    }
}
