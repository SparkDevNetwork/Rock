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

using Rock.ViewModels.Utility;
using Rock.Model;
using System;

namespace Rock.ViewModels.Blocks.Cms.ContentChannelDetail
{
    /// <summary>
    /// The item details for the Content Channel Detail block.
    /// </summary>
    public class ContentChannelBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the collection of Categories that this Content Channel is associated with.
        /// NOTE: Since changes to Categories aren't tracked by ChangeTracker, set the ModifiedDateTime if Categories are modified.
        /// </summary>
        public List<ListItemBag> Categories { get; set; }

        /// <summary>
        /// Gets or sets the channel URL.
        /// </summary>
        public string ChannelUrl { get; set; }

        /// <summary>
        /// Gets or sets the collection of ContentChannels that this ContentChannel allows as children.
        /// </summary>
        public List<ListItemBag> ChildContentChannels { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether child items are manually ordered or not
        /// </summary>
        public bool ChildItemsManuallyOrdered { get; set; }

        /// <summary>
        /// Gets or sets the type of the content channel.
        /// </summary>
        public ListItemBag ContentChannelType { get; set; }

        /// <summary>
        /// Gets or sets the type of the control to render when editing content for items of this type.
        /// </summary>
        public int ContentControlType { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable personalization].
        /// </summary>
        public bool EnablePersonalization { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable RSS].
        /// </summary>
        public bool EnableRss { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is index enabled.
        /// </summary>
        public bool IsIndexEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this content is structured.
        /// </summary>
        public bool IsStructuredContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is tagging enabled.
        /// </summary>
        public bool IsTaggingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether items are manually ordered or not
        /// </summary>
        public bool ItemsManuallyOrdered { get; set; }

        /// <summary>
        /// Gets or sets the item tag Rock.Model.Category.
        /// </summary>
        public ListItemBag ItemTagCategory { get; set; }

        /// <summary>
        /// Gets or sets the item URL.
        /// </summary>
        public string ItemUrl { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires approval].
        /// </summary>
        public bool RequiresApproval { get; set; }

        /// <summary>
        /// Gets or sets the root image directory to use when the HTML control type is used
        /// </summary>
        public string RootImageDirectory { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.DefinedValue representing the content channel's structure content tool.
        /// </summary>
        public ListItemBag StructuredContentToolValue { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes a feed can stay cached before refreshing it from the source.
        /// </summary>
        public int? TimeToLive { get; set; }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public List<string> Settings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is content library enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is content library enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsContentLibraryEnabled { get; set; }

        /// <summary>
        /// Gets or sets the license type unique identifier.
        /// </summary>
        /// <value>
        /// The license type unique identifier.
        /// </value>
        public Guid? LicenseTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the summary attribute unique identifier.
        /// </summary>
        /// <value>
        /// The summary attribute unique identifier.
        /// </value>
        public Guid? SummaryAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the author attribute unique identifier.
        /// </summary>
        /// <value>
        /// The author attribute unique identifier.
        /// </value>
        public Guid? AuthorAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the image attribute unique identifier.
        /// </summary>
        /// <value>
        /// The image attribute unique identifier.
        /// </value>
        public Guid? ImageAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the item attributes.
        /// </summary>
        /// <value>
        /// The item attributes.
        /// </value>
        public List<PublicEditableAttributeBag> ItemAttributes { get; set; }
    }
}
