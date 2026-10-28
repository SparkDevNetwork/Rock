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
    /// One hosted sample data package, shown as a dataset card a person can
    /// choose. An older package index leaves the text fields empty and the
    /// counts at 0.
    /// </summary>
    public class SampleDataInstallerPackageBag
    {
        /// <summary>
        /// Gets or sets the key of the dataset in the package, such as "core-1".
        /// </summary>
        public string Dataset { get; set; }

        /// <summary>
        /// Gets or sets the name of the dataset.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets one or two sentences about who is in the dataset.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets one sentence about what the dataset is best for.
        /// </summary>
        public string BestFor { get; set; }

        /// <summary>
        /// Gets or sets a few short phrases that set the dataset apart.
        /// </summary>
        public List<string> Highlights { get; set; }

        /// <summary>
        /// Gets or sets the number of people a typical install creates.
        /// </summary>
        public int People { get; set; }

        /// <summary>
        /// Gets or sets the number of households a typical install creates.
        /// </summary>
        public int Households { get; set; }

        /// <summary>
        /// Gets or sets the number of campuses the dataset creates.
        /// </summary>
        public int Campuses { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds a typical install of every
        /// part takes.
        /// </summary>
        public int TypicalInstallSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the package can be
        /// installed on this Rock. A package serves one Rock major release
        /// and needs a loader at least as new as its minimum loader version.
        /// </summary>
        public bool IsInstallable { get; set; }
    }
}
