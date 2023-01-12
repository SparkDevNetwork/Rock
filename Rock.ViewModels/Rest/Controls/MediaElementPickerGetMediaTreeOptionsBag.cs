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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetMediaTree API action of
    /// the MediaElementPicker control.
    /// </summary>
    public class MediaElementPickerGetMediaTreeOptionsBag
    {
        /// <summary>
        /// GUID of the selected media element, if one is selected
        /// </summary>
        public Guid? MediaElementGuid { get; set; } = null;

        /// <summary>
        /// GUID of the selected media folder, if one is selected
        /// </summary>
        public Guid? MediaFolderGuid { get; set; } = null;

        /// <summary>
        /// GUID of the selected media account, if one is selected
        /// </summary>
        public Guid? MediaAccountGuid { get; set; } = null;
    }
}
