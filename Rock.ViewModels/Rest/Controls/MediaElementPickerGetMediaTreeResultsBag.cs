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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The results of the GetMediaTree API action of the MediaElementPicker control.
    /// </summary>
    public class MediaElementPickerGetMediaTreeResultsBag
    {
        /// <summary>
        /// The selected Media Element as a ListItemBag
        /// </summary>
        public ListItemBag MediaElement { get; set; }

        /// <summary>
        /// The selected Media Folder as a ListItemBag
        /// </summary>
        public ListItemBag MediaFolder { get; set; }

        /// <summary>
        /// The selected Media Account as a ListItemBag
        /// </summary>
        public ListItemBag MediaAccount { get; set; }

        /// <summary>
        /// The list of Media Elements in the above folder as ListItemBags
        /// </summary>
        public List<ListItemBag> MediaElements { get; set; }

        /// <summary>
        /// The list of Media Folders in the above account as ListItemBags
        /// </summary>
        public List<ListItemBag> MediaFolders { get; set; }

        /// <summary>
        /// The list of Media Accounts as ListItemBags
        /// </summary>
        public List<ListItemBag> MediaAccounts { get; set; }
    }
}
