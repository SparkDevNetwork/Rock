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
    /// The box that contains all the initialization information for the dynamic data block.
    /// </summary>
    public class DynamicDataInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the quick return page title.
        /// </summary>
        public string QuickReturnPageTitle { get; set; }

        /// <summary>
        /// Gets or sets whether the results should be displayed using a lava template.
        /// </summary>
        public bool IsLavaTemplateDisplayMode { get; set; }

        /// <summary>
        /// Gets or sets the lava template results.
        /// </summary>
        public LavaTemplateResultsBag LavaTemplateResults { get; set; }

        /// <summary>
        /// Gets or sets the grid results.
        /// </summary>
        public GridResultsBag GridResults { get; set; }
    }
}
