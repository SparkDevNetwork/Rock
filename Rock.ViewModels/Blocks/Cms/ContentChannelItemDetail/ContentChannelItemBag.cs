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
using Rock.Model;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.ContentChannelItemDetail
{
    /// <summary>
    /// The item details for the Content Channel Item Detail block.
    /// </summary>
    public class ContentChannelItemBag : EntityBagBase
    {
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the publish start date/time with the Rock org-timezone offset.
        /// Always populated; round-trips unchanged for NoDates channels.
        /// </summary>
        public DateTimeOffset? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the publish end date/time. Null when not a DateRange type or no expire was set.
        /// </summary>
        public DateTimeOffset? ExpireDateTime { get; set; }

        /// <summary>
        /// Gets or sets the sort priority. Round-trips on save even when DisablePriority hides the control.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the approval status. Applied on save only by status logic, not the scalar UpdateEntityFromBox path.
        /// </summary>
        public ContentChannelItemStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the display name of the approver/denier, or null if no approval has occurred.
        /// </summary>
        public string ApprovedByName { get; set; }

        /// <summary>
        /// Gets or sets the approved/denied date as ISO 8601, or null if no approval has occurred.
        /// </summary>
        public string ApprovedDateTime { get; set; }

        /// <summary>
        /// Gets or sets whether the read-only status display is shown in place of the Approval Status toggle.
        /// True when the channel requires approval, the type does not disable status, and the user lacks APPROVE.
        /// Defaults to false.
        /// </summary>
        public bool IsReadOnlyStatusShown { get; set; }

        /// <summary>
        /// Gets or sets which content editor renders: None, Html, or Structured.
        /// </summary>
        public ContentChannelItemContentEditor ContentEditorType { get; set; }

        /// <summary>
        /// Gets or sets the HTML content. Initial value for the HTML editor.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the structured content JSON. Initial value for the structured editor.
        /// </summary>
        public string StructuredContent { get; set; }

        /// <summary>
        /// Gets or sets the structured editor tool-set Guid; null uses the system default.
        /// </summary>
        public Guid? StructuredContentToolValueGuid { get; set; }

        /// <summary>
        /// Gets or sets whether the HTML editor opens in code mode. Defaults to false.
        /// </summary>
        public bool IsContentEditorStartingInCodeMode { get; set; }

        /// <summary>
        /// Gets or sets the encrypted image root for the HTML editor, or blank.
        /// </summary>
        public string EncryptedContentImageRootFolder { get; set; }

        /// <summary>
        /// Gets or sets the encrypted document root for the HTML editor.
        /// </summary>
        public string EncryptedContentDocumentRootFolder { get; set; }

        /// <summary>
        /// Gets or sets the URL slug rows. Persisted for existing items; staged (Id == 0) for new items, written inside Save.
        /// </summary>
        public List<UrlSlugBag> UrlSlugs { get; set; }

        /// <summary>
        /// Gets or sets the channel-URL prefix shown before each slug (ItemUrl with {{Slug}} stripped), or empty.
        /// </summary>
        public string SlugUrlPrefix { get; set; }

        /// <summary>
        /// Gets or sets the item's global key for stable external links.
        /// Persisted on save for existing items; new items get it from the AssignItemGlobalKey PreSave hook.
        /// </summary>
        public string ItemGlobalKey { get; set; }

        /// <summary>
        /// Gets or sets the content-library status: None, Uploaded, or Downloaded.
        /// </summary>
        public ContentChannelItemLibraryStatus LibraryStatus { get; set; }

        /// <summary>
        /// Gets or sets the license display name, or null when no license resolves.
        /// </summary>
        public string LibraryLicenseName { get; set; }

        /// <summary>
        /// Gets or sets the by-person name for the status panel (uploader or creator depending on status).
        /// </summary>
        public string LibraryByPersonName { get; set; }

        /// <summary>
        /// Gets or sets the on-date for the status panel (upload date or creation date depending on status).
        /// </summary>
        public DateTime? LibraryOnDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Content Experience Level selection.
        /// </summary>
        public ContentLibraryItemExperienceLevel? ExperienceLevel { get; set; }

        /// <summary>
        /// Gets or sets the selected Content Topic Guid, or null.
        /// </summary>
        public string ContentLibraryContentTopicGuid { get; set; }

        /// <summary>
        /// Gets or sets the personalization segment Guids. A present empty list removes all
        /// associations on save; the property being absent from ValidProperties leaves them unchanged.
        /// </summary>
        public List<string> SelectedSegmentGuids { get; set; }

        /// <summary>
        /// Gets or sets the request filter Guids. A present empty list removes all
        /// associations on save; the property being absent from ValidProperties leaves them unchanged.
        /// </summary>
        public List<string> SelectedRequestFilterGuids { get; set; }

        /// <summary>
        /// Gets or sets the Content Intent Guids. A present empty list removes all
        /// associations on save; the property being absent from ValidProperties leaves them unchanged.
        /// </summary>
        public List<string> SelectedIntentGuids { get; set; }

        /// <summary>
        /// Gets or sets whether the Child Items stack is shown. True when the channel has child channels and the item is saved.
        /// Defaults to false.
        /// </summary>
        public bool IsChildItemsStackShown { get; set; }

        /// <summary>
        /// Gets or sets whether the Parent Items stack is shown. True when the channel has parent channels and the item is saved.
        /// Defaults to false.
        /// </summary>
        public bool IsParentItemsStackShown { get; set; }

        /// <summary>
        /// Gets or sets whether the child grid renders a reorder handle. True when the channel orders children manually
        /// and the list was not security filtered. Defaults to false.
        /// </summary>
        public bool IsChildReorderEnabled { get; set; }

        /// <summary>
        /// Gets or sets the server-built child-items grid data.
        /// </summary>
        public GridDataBag ChildItemsGridData { get; set; }

        /// <summary>
        /// Gets or sets the read-only parent-items grid data.
        /// </summary>
        public GridDataBag ParentItemsGridData { get; set; }
    }
}
