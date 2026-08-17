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

namespace Rock.ViewModels.Blocks.Cms.CustomComponentDetail
{
    /// <summary>
    /// The response returned by the SaveContent block action after a
    /// successful server-side compile and save.
    /// </summary>
    public class SaveCustomComponentResponseBag
    {
        /// <summary>
        /// Gets or sets the compiled SystemJS module string the server produced,
        /// so the editor can render the saved component without a page reload.
        /// </summary>
        /// <value>The compiled component module.</value>
        public string CompiledContent { get; set; }
    }
}
