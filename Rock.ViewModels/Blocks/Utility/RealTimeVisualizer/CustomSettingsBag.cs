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

namespace Rock.ViewModels.Blocks.Utility.RealTimeVisualizer
{
    /// <summary>
    /// The settings that will be edited in the custom settings panel for the
    /// Real Time Visualizer block.
    /// </summary>
    public class CustomSettingsBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the selected theme.
        /// </summary>
        /// <value>The unique identifier of the selected theme.</value>
        public Guid? ThemeGuid { get; set; }

        /// <summary>
        /// Gets or sets the custom page template.
        /// </summary>
        /// <value>The custom page template.</value>
        public string PageTemplate { get; set; }

        /// <summary>
        /// Gets or sets the custom CSS stylesheet content.
        /// </summary>
        /// <value>The custom CSS stylesheet content.</value>
        public string Style { get; set; }

        /// <summary>
        /// Gets or sets the custom script that will show a message.
        /// </summary>
        /// <value>The custom script that will show a message.</value>
        public string Script { get; set; }

        /// <summary>
        /// Gets or sets the custom theme settings.
        /// </summary>
        /// <value>The custom theme settings.</value>
        public Dictionary<string, string> ThemeSettings { get; set; }

        /// <summary>
        /// Gets or sets the topic configuration for which topics and channels to monitor.
        /// </summary>
        /// <value>The topic configuration for which topics and channels to monitor.</value>
        public List<TopicAndChannelBag> TopicConfiguration { get; set; }
    }
}
