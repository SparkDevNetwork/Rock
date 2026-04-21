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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The response that will be returned by the GetMediaElementMetadata API
    /// action of the MediaElementPicker control.
    /// </summary>
    public class MediaElementPickerGetMediaElementMetadataResponseBag
    {
        /// <summary>
        /// The name of the media element.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The URL of the thumbnail to display for the media element.
        /// </summary>
        public string ThumbnailUrl { get; set; }
    }
}
