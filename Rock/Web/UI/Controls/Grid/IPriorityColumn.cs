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
namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Column specifies a column priority for controlling visibility based on screen size
    /// </summary>
    public interface IPriorityColumn
    {
        /// <summary>
        /// Gets or sets the column priority.
        /// </summary>
        /// <value>
        /// The column priority.
        /// </value>
        ColumnPriority ColumnPriority { get; set; }
    }
}
