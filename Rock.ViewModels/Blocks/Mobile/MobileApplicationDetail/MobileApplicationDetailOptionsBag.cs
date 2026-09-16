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

namespace Rock.ViewModels.Blocks.Mobile.MobileApplicationDetail
{
    /// <summary>
    /// The options bag for the Mobile Application Detail block. Provides
    /// read-only context that supplements the editable Application bag —
    /// styles, dropdown enum lists, picker qualifiers, and pre-rendered
    /// summary content used by the view panel.
    /// </summary>
    public class MobileApplicationDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the integer ID of the Site. Exposed for legacy display
        /// purposes (the UI shows "Site Id: N" in the header label).
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Gets or sets the friendly text representation of the most recent
        /// deploy ("5 days ago"), or null when the application has never been
        /// deployed.
        /// </summary>
        public string LastDeployText { get; set; }

        /// <summary>
        /// Gets or sets the long-form, locale-friendly text displayed as the
        /// hover tooltip on the deploy badge (e.g. "Sunday, March 2, 2025 5:42 PM").
        /// Null when the application has never been deployed.
        /// </summary>
        public string LastDeployTooltip { get; set; }

        /// <summary>
        /// Gets or sets the pre-rendered HTML displayed in the application
        /// summary on the view panel. Pre-rendering on the server avoids
        /// duplicating field-formatting logic in Vue.
        /// </summary>
        public string ApplicationDetailsHtml { get; set; }

        /// <summary>
        /// Gets or sets the URL of the preview thumbnail image, or null if no
        /// thumbnail is configured.
        /// </summary>
        public string PreviewThumbnailUrl { get; set; }

        /// <summary>
        /// Gets or sets the comma-separated list of deep link domains shown
        /// in the info banner above the Deep Links grid.
        /// </summary>
        public string DeepLinkDomainsText { get; set; }

        /// <summary>
        /// Gets or sets the current styles bag. Provided as part of the
        /// options because the styles tab is edited in place inside the view
        /// panel and needs an initial value separate from the application
        /// entity bag.
        /// </summary>
        public MobileApplicationStylesBag Styles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user is
        /// authorized to edit the application. Drives whether edit affordances
        /// (Edit button, Styles save, grid add/delete/reorder) are shown.
        /// </summary>
        public bool IsEditable { get; set; }

        /// <summary>
        /// Gets or sets the available shell types that can be selected for the
        /// application (Flyout, Tabbed, Blank).
        /// </summary>
        public List<ListItemBag> ApplicationTypes { get; set; }

        /// <summary>
        /// Gets or sets the available Android tab locations.
        /// </summary>
        public List<ListItemBag> AndroidTabLocations { get; set; }

        /// <summary>
        /// Gets or sets the device orientations available for the lock-phone
        /// and lock-tablet pickers.
        /// </summary>
        public List<ListItemBag> DeviceOrientations { get; set; }

        /// <summary>
        /// Gets or sets the available mobile style frameworks (Standard /
        /// Blended / Legacy).
        /// </summary>
        public List<ListItemBag> MobileStyleFrameworks { get; set; }

        /// <summary>
        /// Gets or sets the available iOS blur styles for the navigation bar.
        /// </summary>
        public List<ListItemBag> IOSBlurStyles { get; set; }

        /// <summary>
        /// Gets or sets the defined-type Guid that the Auth0 connection status
        /// picker should constrain its values to.
        /// </summary>
        public Guid? ConnectionStatusDefinedTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the defined-type Guid that the Auth0 record status
        /// picker should constrain its values to.
        /// </summary>
        public Guid? RecordStatusDefinedTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the entity type Guid used by the Person Attribute
        /// Categories category picker.
        /// </summary>
        public Guid? PersonAttributeCategoryEntityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the qualifier column applied to the Person Attribute
        /// Categories category picker.
        /// </summary>
        public string PersonAttributeCategoryQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the qualifier value applied to the Person Attribute
        /// Categories category picker.
        /// </summary>
        public string PersonAttributeCategoryQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the entity type Guid used by the Campus Filter data
        /// view picker.
        /// </summary>
        public Guid? CampusFilterEntityTypeGuid { get; set; }
    }
}
