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

namespace Rock.Model.CMS.ContentChannelItem.Options
{
    /// <summary>
    /// The options for uploading a Content Channel Item as a Content Library Item.
    /// </summary>
    public class ContentLibraryItemUploadOptions
    {
        /// <summary>
        /// Gets or sets the content channel item identifier.
        /// </summary>
        /// <value>
        /// The content channel item identifier.
        /// </value>
        public int ContentChannelItemId { get; set; }

        /// <summary>
        /// Gets the content library uploaded by person alias identifier.
        /// </summary>
        /// <value>
        /// The content library uploaded by person alias identifier.
        /// </value>
        public int? UploadedByPersonAliasId { get; set; }
    }
}
