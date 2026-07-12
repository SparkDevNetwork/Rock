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

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInAreasAndGroups
{
    /// <summary>
    /// The request payload for saving a new or existing check-in area. The server treats
    /// <see cref="AreaDetailBag.IdKey"/> as the create-vs-update discriminator: empty/null means create a new
    /// area under <see cref="ParentAreaIdKey"/> (or under the configuration when that is also empty/null).
    /// </summary>
    public class SaveAreaRequestBag
    {
        /// <summary>
        /// Gets or sets the editable area detail.
        /// </summary>
        public AreaDetailBag Area { get; set; }

        /// <summary>
        /// Gets or sets the hashed identifier of the parent area under which to create this area when
        /// <see cref="AreaDetailBag.IdKey"/> is empty/null. When this is also empty/null, the new area is
        /// added under the check-in configuration itself. Ignored on update.
        /// </summary>
        public string ParentAreaIdKey { get; set; }
    }
}
