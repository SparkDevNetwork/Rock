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

namespace Rock.ViewModels.Blocks.BulkImport
{
    /// <summary>
    /// The request parameters for importing the CSV data
    /// </summary>
    public class CsvImportStartImportOptionsBag
    {
        /// <summary>
        /// Gets or sets a map of Person fields (the key) to the name of the CSV column namee (the value) that the column should be mapped to.
        /// </summary>
        public Dictionary<string, string> ColumnMappings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether existing records should be updated if a match is found.
        /// </summary>
        public bool AllowUpdatingExisting { get; set; } = true;

        /// <summary>
        /// Gets or sets the name of the CSV file with the import data.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the description of the source of the data being imported.
        /// </summary>
        public string SourceDescription { get; set; }

        /// <summary>
        /// Gets or sets the number of records that will be imported.
        /// This is used to provide feedback to the user about how many records are being processed.
        /// </summary>
        public int RecordCount { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the realtime session so messages can be sent to a specific block.
        /// </summary>
        public string SessionId { get; set; }
    }
}