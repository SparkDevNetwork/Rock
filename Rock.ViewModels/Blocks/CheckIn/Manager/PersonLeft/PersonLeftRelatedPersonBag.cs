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
namespace Rock.ViewModels.Blocks.CheckIn.Manager.PersonLeft
{
    /// <summary>
    /// Describes a single family-member or related-person tile rendered by
    /// the Check-in Manager Person Profile (limited) block.
    /// </summary>
    public class PersonLeftRelatedPersonBag
    {
        /// <summary>
        /// Gets or sets the nick name shown under the tile photo.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the pre-rendered photo image tag HTML for the
        /// person, sized appropriately for the panel.
        /// </summary>
        public string PhotoImageTag { get; set; }

        /// <summary>
        /// Gets or sets the URL that the tile navigates to when clicked
        /// (typically the same Person Profile page for the family member).
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the known-relationship role name (such as "Grandmother")
        /// shown as a subtitle on related-people tiles. Null on family
        /// tiles.
        /// </summary>
        public string RelationshipName { get; set; }
    }
}
