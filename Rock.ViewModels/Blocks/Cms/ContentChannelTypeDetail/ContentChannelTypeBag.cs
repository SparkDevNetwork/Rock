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
using Rock.Model;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Cms.ContentChannelTypeDetail
{
    /// <summary>
    /// The item details for the Content Channel Type Detail block.
    /// </summary>
    public class ContentChannelTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets an Rock.Model.ContentChannelDateType enumeration that represents the type of date range that this DateRangeTypeEnum supports.
        /// </summary>
        public int DateRangeType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [disable content field].
        /// </summary>
        public bool DisableContentField { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [disable priority].
        /// </summary>
        public bool DisablePriority { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [disable status].
        /// If this is set to True, all of the ContentChannelItems are "Approved"
        /// </summary>
        public bool DisableStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether time should be included with the single or date range values
        /// </summary>
        public bool IncludeTime { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this ContentType is part of the Rock core system/framework. 
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name of the ContentType. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A flag indicating if a Rock.Model.ContentChannel of this ContentChannelType will be shown in the content channel list.
        /// When false, it means any 'Channel Types Include' settings MUST specifically include in order to show it.
        /// </summary>
        public bool ShowInChannelList { get; set; }

        /// <summary>
        /// Gets or sets the item attributes.
        /// </summary>
        /// <value>
        /// The item attributes.
        /// </value>
        public List<PublicEditableAttributeBag> ItemAttributes { get; set; }

        /// <summary>
        /// Gets or sets the channel attributes.
        /// </summary>
        /// <value>
        /// The channel attributes.
        /// </value>
        public List<PublicEditableAttributeBag> ChannelAttributes { get; set; }
    }
}
