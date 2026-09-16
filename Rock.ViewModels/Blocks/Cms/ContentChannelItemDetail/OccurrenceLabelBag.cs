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

namespace Rock.ViewModels.Blocks.Cms.ContentChannelItemDetail
{
    /// <summary>
    /// A single header-area label for an event occurrence associated with a content channel item.
    /// </summary>
    public class OccurrenceLabelBag
    {
        /// <summary>
        /// Gets or sets the raw occurrence display text. Rendered through escaped text interpolation client-side.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the link to the Event Occurrence Page, or null when unconfigured (renders as plain text).
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the leading icon CSS class.
        /// </summary>
        public string IconCssClass { get; set; }
    }
}
