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

namespace Rock.ViewModels.Blocks.Cms.ObsidianContentDetail
{
    /// <summary>
    /// The request sent to the SaveContent block action when an authorized
    /// editor saves authored Obsidian content.
    /// </summary>
    public class SaveObsidianContentRequestBag
    {
        /// <summary>
        /// Gets or sets the clean Vue source the author wrote.
        /// </summary>
        /// <value>The authored Vue source.</value>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the compiled SystemJS module string produced in the
        /// author's browser.
        /// </summary>
        /// <value>The compiled component module.</value>
        public string CompiledContent { get; set; }

        /// <summary>
        /// Gets or sets the Vue version the compile targeted.
        /// </summary>
        /// <value>The targeted Vue version.</value>
        public string CompiledVueVersion { get; set; }
    }
}
