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

using Rock.Enums.Cms;

namespace Rock.ViewModels.Cms
{
    /// <summary>
    /// Contains the configuration required to initialize an Obsidian block in
    /// the web browser.
    /// </summary>
    public class ObsidianBlockConfigBag
    {
        /// <summary>
        /// Gets or sets the URL used to load the Obsidian block JavaScript file.
        /// </summary>
        /// <value>
        /// The URL used to load the Obsidian block JavaScript file.
        /// </value>
        public string BlockFileUrl { get; set; }

        /// <summary>
        /// Gets or sets the HTML element identifier that the block will be
        /// mounted on as its root.
        /// </summary>
        /// <value>
        /// The HTML element identifier that the block will be mounted on.
        /// </value>
        public string RootElementId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the block that is being
        /// represented by this configuration.
        /// </summary>
        /// <value>
        /// The unique identifier of the block.
        /// </value>
        public Guid BlockGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the block type that is being
        /// represented by this configuration.
        /// </summary>
        /// <value>
        /// The unique identifier of the block type.
        /// </value>
        public Guid BlockTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the custom configuration values for the block. This
        /// object is made available to the block component.
        /// </summary>
        /// <value>
        /// The custom configuration values for the block.
        /// </value>
        public object ConfigurationValues { get; set; }

        /// <summary>
        /// Gets or sets the initial content to be rendered for the block
        /// until the component loads. This is only valid when refreshing
        /// the block initialization for a block reload operation.
        /// </summary>
        /// <value>The initial content.</value>
        public string InitialContent { get; set; }

        /// <summary>
        /// Gets or sets the custom configuration actions that should be added
        /// to the block's configuration bar.
        /// </summary>
        /// <value>
        /// The custom configuration actions for the block.
        /// </value>
        public List<BlockCustomActionBag> CustomConfigurationActions { get; set; }

        /// <summary>
        /// Gets or sets the person preferences associated with this block.
        /// </summary>
        /// <value>The person preferences associated with this block.</value>
        public ObsidianBlockPreferencesBag Preferences { get; set; }

        /// <summary>
        /// Gets or sets the reload mode when the block configuration changes.
        /// </summary>
        /// <value>The reload mode.</value>
        public BlockReloadMode ReloadMode { get; set; }
    }
}
