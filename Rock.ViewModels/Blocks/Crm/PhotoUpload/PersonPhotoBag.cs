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

namespace Rock.ViewModels.Blocks.Crm.PhotoUpload
{
    /// <summary>
    /// 
    /// </summary>
    public class PersonPhotoBag
    {
        /// <summary>
        /// Gets or sets the IdKey of the person with which
        /// the profile photo belongs to.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the FullName of the person with which
        /// the profile photo belongs to.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the ProfilePhoto for the person
        /// </summary>
        public ListItemBag ProfilePhoto { get; set; }

        /// <summary>
        /// Gets or sets the Url of the photo to be displayed
        /// when the person does not have a profile photo.
        /// </summary>
        public string NoPhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets IsStaffMemberDisabled which is used
        /// to determine if the is a staff member that should
        /// be disabled from the photo upload process.
        /// </summary>
        public bool IsStaffMemberDisabled { get; set; }
    }
}
