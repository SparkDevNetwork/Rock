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
    /// The choices a person made to start a sample data install.
    /// </summary>
    public class SampleDataInstallerInstallBag
    {
        /// <summary>
        /// Gets or sets the dataset key of the hosted package to install.
        /// </summary>
        public string Dataset { get; set; }

        /// <summary>
        /// Gets or sets the keys of the parts to include.
        /// </summary>
        public List<string> Parts { get; set; }

        /// <summary>
        /// Gets or sets the number of years of history to generate.
        /// </summary>
        public int HistoryYears { get; set; }

        /// <summary>
        /// Gets or sets the date that every relative date counts back from.
        /// </summary>
        public DateTime? AnchorDate { get; set; }

        /// <summary>
        /// Gets or sets the text that every random choice starts from.
        /// </summary>
        public string Seed { get; set; }

        /// <summary>
        /// Gets or sets the RealTime connection that receives progress reports.
        /// </summary>
        public string RealTimeConnectionId { get; set; }
    }
}
