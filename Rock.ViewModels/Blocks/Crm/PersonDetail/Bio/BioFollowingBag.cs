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
namespace Rock.ViewModels.Blocks.Crm.PersonDetail.Bio
{
    /// <summary>
    /// Describes the follow state of the person being viewed by the Person
    /// Bio block.
    /// </summary>
    public class BioFollowingBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current person is
        /// following the person being viewed.
        /// </summary>
        public bool IsFollowed { get; set; }

        /// <summary>
        /// Gets or sets the total number of people following the person being
        /// viewed.
        /// </summary>
        public int FollowerCount { get; set; }
    }
}
