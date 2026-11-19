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
    /// One part of the sample data that a person can choose to include.
    /// </summary>
    public class SampleDataInstallerPartBag
    {
        /// <summary>
        /// Gets or sets the key that identifies the part when installing.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name of the part.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a one sentence description of the part.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the keys of the parts this part needs. When one of
        /// them is left out, this part is left out too.
        /// </summary>
        public List<string> DependsOn { get; set; }
    }
}
