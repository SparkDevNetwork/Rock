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

namespace Rock.ViewModels.Blocks.Example.SampleDataInstaller
{
    /// <summary>
    /// Contains the choices offered by the Sample Data Installer block.
    /// </summary>
    public class SampleDataInstallerOptionsBag
    {
        /// <summary>
        /// Gets or sets the parts of the sample data a person can choose,
        /// in load order.
        /// </summary>
        public List<SampleDataInstallerPartBag> Parts { get; set; }
    }
}
