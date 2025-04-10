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

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// The SMS Conversations Initialization Box
    /// </summary>
    public class PlacementGroupTypeRoleBag
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? MaxCount { get; set; }

        public int? MinCount { get; set; }

        public bool IsLeader { get; set; } // TODO - may remove
    }
}
