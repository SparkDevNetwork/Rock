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

using System;
using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.BulkImport
{
    /// <summary>
    /// The initialization data for the Bulk Import Tool block.
    /// </summary>
    public class CsvImportBox
    {
        /// <summary>
        /// Gets or sets the Encrypted Root Folder.
        /// </summary>
        public string RootFolder { get; set; }

        /// <summary>
        /// Gets or sets the list of previous sources for the import.
        /// </summary>
        public List<ListItemBag> Sources { get; set; } = new List<ListItemBag>();

        /// <summary>
        /// Gets or sets the available Suffix options for this Rock instance.
        /// </summary>
        public string SuffixOptions { get; set; }

        /// <summary>
        /// Gets or sets the available Connection Status options for this Rock instance.
        /// </summary>
        public string ConnectionStatusOptions { get; set; }

        /// <summary>
        /// Gets or sets the available Grade options for this Rock instance.
        /// </summary>
        public string GradeOptions { get; set; }

        /// <summary>
        /// Gets or sets the available Email Preference options for this Rock instance.
        /// </summary>
        public string EmailPreferenceOptions { get; set; }

        /// <summary>
        /// Gets or sets the available Gender options for this Rock instance.
        /// </summary>
        public string GenderOptions { get; set; }

        /// <summary>
        /// Gets or sets the available Marital Status options for this Rock instance.
        /// </summary>
        public string MaritalStatusOptions { get; set; }

        /// <summary>
        /// Gets or sets the available Record Status options for this Rock instance.
        /// </summary>
        public string RecordStatusOptions { get; set; }
    }
}