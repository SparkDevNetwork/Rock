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

namespace Rock.ViewModels.Blocks.Example.SampleDataInstaller
{
    /// <summary>
    /// Contains the initial state data for the Sample Data Installer block.
    /// </summary>
    public class SampleDataInstallerBag
    {
        /// <summary>
        /// Gets or sets the reasons this database cannot take sample data.
        /// An empty list means the database is ready for an install.
        /// </summary>
        public List<string> DatabaseProblems { get; set; }

        /// <summary>
        /// Gets or sets the default number of years of history to generate.
        /// </summary>
        public int HistoryYears { get; set; }

        /// <summary>
        /// Gets or sets the default date that every relative date counts back from.
        /// </summary>
        public DateTime? AnchorDate { get; set; }

        /// <summary>
        /// Gets or sets the default text that every random choice starts from.
        /// </summary>
        public string Seed { get; set; }

        /// <summary>
        /// Gets or sets the URL that searches for the Decker family, shown
        /// after a successful install.
        /// </summary>
        public string PersonSearchUrl { get; set; }
    }
}
