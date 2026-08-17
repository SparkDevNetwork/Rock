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

namespace Rock.ViewModels.Blocks.Group.GroupMap
{
    /// <summary>
    /// Identifies the map item a person clicked so the block can render its info window Lava.
    /// </summary>
    public class GroupMapInfoWindowRequestBag
    {
        /// <summary>
        /// Gets or sets the Id of the group whose marker was clicked.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the clicked location, used to resolve the address portion
        /// of the info window.
        /// </summary>
        public int LocationId { get; set; }
    }
}
