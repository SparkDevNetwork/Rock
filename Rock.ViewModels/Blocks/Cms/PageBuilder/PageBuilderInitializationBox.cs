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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.PageBuilder
{
    /// <summary>
    /// The top-level initialization object returned by the Page Builder block.
    /// </summary>
    public class PageBuilderInitializationBox
    {
        /// <summary>
        /// Gets or sets the URL of the page the builder frames for composing.
        /// </summary>
        public string TargetPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the page the builder frames, which the page's
        /// own blocks are addressed by.
        /// </summary>
        public Guid? TargetPageGuid { get; set; }

        /// <summary>
        /// Gets or sets the internal name of the page the builder frames.
        /// </summary>
        public string TargetPageName { get; set; }

        /// <summary>
        /// Gets or sets the interaction intents the page the builder frames is tagged with.
        /// </summary>
        public List<ListItemBag> TargetPageIntents { get; set; }

        /// <summary>
        /// Gets or sets the message to display instead of the builder when it cannot run.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the module types listed in the sidebar, in display order.
        /// </summary>
        public List<PageBuilderModuleTypeBag> ModuleTypes { get; set; }
    }
}
