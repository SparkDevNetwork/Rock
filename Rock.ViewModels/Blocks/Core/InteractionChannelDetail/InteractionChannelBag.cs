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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.InteractionChannelDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class InteractionChannelBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the channel detail template.
        /// </summary>
        public string ChannelDetailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the channel list template.
        /// </summary>
        public string ChannelListTemplate { get; set; }

        /// <summary>
        /// Gets or sets the length of time (in minutes) that components of this channel should be cached
        /// </summary>
        public int? ComponentCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the component detail template.
        /// </summary>
        public string ComponentDetailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the component list template.
        /// </summary>
        public string ComponentListTemplate { get; set; }

        /// <summary>
        /// Gets or sets the engagement strength.
        /// </summary>
        public int? EngagementStrength { get; set; }

        /// <summary>
        /// Gets or sets the interaction custom 1 label.
        /// </summary>
        public string InteractionCustom1Label { get; set; }

        /// <summary>
        /// Gets or sets the interaction custom 2 label.
        /// </summary>
        public string InteractionCustom2Label { get; set; }

        /// <summary>
        /// Gets or sets the interaction custom indexed 1 label.
        /// </summary>
        public string InteractionCustomIndexed1Label { get; set; }

        /// <summary>
        /// Gets or sets the interaction detail template.
        /// </summary>
        public string InteractionDetailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the interaction list template.
        /// </summary>
        public string InteractionListTemplate { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is an active group. This value is required.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the interaction channel name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the retention days.
        /// </summary>
        public int? RetentionDuration { get; set; }

        /// <summary>
        /// Gets or sets the session list template.
        /// </summary>
        public string SessionListTemplate { get; set; }

        /// <summary>
        /// Gets or sets the content displayed in View-Mode. It is generated via Lava using either the DefaultTemplate
        /// block setting or the <see cref="InteractionChannelBag.ChannelDetailTemplate"/> if one is provided.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public string Content { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether the current user has Administrate Authorization.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can administrate; otherwise, <c>false</c>.
        /// </value>
        public bool CanAdministrate { get; set; }
    }
}
