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

namespace Rock.ViewModels.Blocks.Utility.InternalCommunicationView
{
    /// <summary>
    /// The rendered content for a single page of the Internal Communication View block.
    /// </summary>
    public class InternalCommunicationViewContentBag
    {
        /// <summary>
        /// Gets or sets the fully resolved HTML for the block title, including the icon and the Lava title template.
        /// </summary>
        public string TitleHtml { get; set; }

        /// <summary>
        /// Gets or sets the resolved HTML body content produced by the body Lava template.
        /// </summary>
        public string BodyHtml { get; set; }

        /// <summary>
        /// Gets or sets the notification message shown when configuration is missing or no items are available.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the alert type used for the notification message (e.g. "validation" or "info").
        /// </summary>
        public string MessageType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an older item exists, which enables the Previous action.
        /// </summary>
        public bool ShowPrevious { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a newer item exists, which enables the Next action.
        /// </summary>
        public bool ShowNext { get; set; }

        /// <summary>
        /// Gets or sets the zero-based page offset that this content represents.
        /// </summary>
        public int PageIndex { get; set; }
    }
}
