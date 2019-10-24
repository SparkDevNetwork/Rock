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
    /// Interface for all Rock grid fields
    /// </summary>
    public interface IRockGridField
    {
        /// <summary>
        /// Gets or sets the header text.
        /// </summary>
        /// <value>
        /// The header text.
        /// </value>
        string HeaderText { get; set; }

        /// <summary>
        /// When exporting a grid with an Export source of ColumnOutput, this property controls whether a column is included
        /// in the export or not
        /// </summary>
        ExcelExportBehavior ExcelExportBehavior { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IRockGridField"/> is visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if visible; otherwise, <c>false</c>.
        /// </value>
        bool Visible { get; set; }
    }

    /// <summary>
    /// Enum that defines when a column should be included in an Excel export ( when in ColumnOutput ExportSource )
    /// </summary>
    public enum ExcelExportBehavior
    {
        /// <summary>
        /// The always include
        /// </summary>
        AlwaysInclude,

        /// <summary>
        /// The include if visible
        /// </summary>
        IncludeIfVisible,

        /// <summary>
        /// The never include
        /// </summary>
        NeverInclude,
    }

}
