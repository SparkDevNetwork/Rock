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

namespace Rock.ViewModels.Blocks.Reporting.DynamicData
{
    /// <summary>
    /// A bag that contains information about a column's configuration for the dynamic data block.
    /// </summary>
    public class ColumnConfigurationBag
    {
        /// <summary>
        /// Gets or sets the actual column name, as represented in the SQL query.
        /// </summary>
        public string ActualColumnName { get; set; }

        /// <summary>
        /// Gets or sets the runtime name, which might differ from the actual column name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Split Case name.
        /// </summary>
        public string SplitCaseName { get; set; }

        /// <summary>
        /// Gets or sets the camelCase name.
        /// </summary>
        public string CamelCaseName { get; set; }

        /// <summary>
        /// Gets or sets the column type.
        /// </summary>
        public string ColumnType { get; set; }

        /// <summary>
        /// Gets or sets the minimum window size for the column to be displayed.
        /// </summary>
        public string VisiblePriority { get; set; }

        /// <summary>
        /// Gets or sets the width of the column.
        /// </summary>
        public string Width { get; set; }

        /// <summary>
        /// Gets or sets whether to enable filtering of this column.
        /// </summary>
        public bool EnableFiltering { get; set; }

        /// <summary>
        /// Gets or sets whether to hide this column from rendering on screen.
        /// It may still be include in exports and other operations.
        /// </summary>
        public bool HideFromGrid { get; set; }

        /// <summary>
        /// Gets or sets whether to exclude this field when exporting data
        /// to be downloaded by the individual.
        /// </summary>
        public bool ExcludeFromExport { get; set; }

        /// <summary>
        /// Gets or sets whether to show the last name first, if a person column.
        /// </summary>
        public bool PersonShowLastNameFirst { get; set; }
    }
}
