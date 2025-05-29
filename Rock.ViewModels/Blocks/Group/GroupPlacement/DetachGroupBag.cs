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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// Represents the data needed to detach a group from a placement context.
    /// </summary>
    public class DetachGroupBag
    {
        /// <summary>
        /// The encrypted identifier key of the group to be detached.
        /// </summary>
        public string GroupIdKey { get; set; }

        /// <summary>
        /// The placement mode indicating the context in which the group is being detached.
        /// </summary>
        public PlacementMode PlacementMode { get; set; }

        /// <summary>
        /// Contextual placement keys that define the relationship or linkage of the group within the placement system.
        /// </summary>
        public GroupPlacementKeysBag GroupPlacementKeys { get; set; }
    }
}
