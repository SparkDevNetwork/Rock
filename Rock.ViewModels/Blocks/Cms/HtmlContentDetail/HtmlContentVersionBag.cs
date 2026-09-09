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

namespace Rock.ViewModels.Blocks.Cms.HtmlContentDetail
{
    /// <summary>
    /// One row of the Version History grid in the Edit HTML modal.
    /// </summary>
    public class HtmlContentVersionBag
    {
        /// <summary>
        /// Gets or sets the identifier key of the HtmlContent row.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the version number.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the display text for the version, such as "Version 3".
        /// </summary>
        public string VersionText { get; set; }

        /// <summary>
        /// Gets or sets the ISO 8601 date this version was last modified. The
        /// client renders it as elapsed time.
        /// </summary>
        public string ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person who last modified this version.
        /// </summary>
        public string ModifiedByName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this version is approved.
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person who approved this version,
        /// or null when it has not been approved.
        /// </summary>
        public string ApprovedByName { get; set; }

        /// <summary>
        /// Gets or sets the ISO 8601 date the content becomes visible, or null
        /// for no lower bound.
        /// </summary>
        public string StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the ISO 8601 date the content stops being visible, or
        /// null for no upper bound.
        /// </summary>
        public string ExpireDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the highest version
        /// number, which the grid marks as Current instead of offering Select.
        /// </summary>
        public bool IsCurrent { get; set; }
    }
}
