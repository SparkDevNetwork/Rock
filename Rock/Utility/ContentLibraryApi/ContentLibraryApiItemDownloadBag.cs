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

namespace Rock.Utility.ContentLibraryApi
{
    /// <summary>
    /// Bag containing the information required to download a Content Library item.
    /// </summary>
    public class ContentLibraryApiItemDownloadBag
    {
        /// <summary>
        /// The name of the person downloading the Content Library item.
        /// </summary>
        public string DownloadedBy { get; set; }

        /// <summary>
        /// Indicates whether structured content data is included in the download.
        /// </summary>
        public bool IsStructuredContentIncluded { get; set; }

        /// <summary>
        /// The key of the organization downloading the Content Library item.
        /// </summary>
        public string OrganizationKey { get; set; }
    }
}
