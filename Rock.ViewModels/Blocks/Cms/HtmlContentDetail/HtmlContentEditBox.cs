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

namespace Rock.ViewModels.Blocks.Cms.HtmlContentDetail
{
    /// <summary>
    /// Everything the Edit HTML modal needs when it opens: the version loaded
    /// into the editor, the block-derived options, and the version history.
    /// </summary>
    public class HtmlContentEditBox
    {
        /// <summary>
        /// Gets or sets the version of the content currently loaded into the editor.
        /// </summary>
        public HtmlContentEditBag Content { get; set; }

        /// <summary>
        /// Gets or sets the block-derived options that control which editor
        /// features are shown.
        /// </summary>
        public HtmlContentEditOptionsBag Options { get; set; }

        /// <summary>
        /// Gets or sets the version history rows, newest version first. This is
        /// null when versioning is not enabled for the block.
        /// </summary>
        public List<HtmlContentVersionBag> Versions { get; set; }

        /// <summary>
        /// Gets or sets the security grant token the editor's controls, such as
        /// the image browser, use to reach the asset manager endpoints.
        /// </summary>
        public string SecurityGrantToken { get; set; }
    }
}
