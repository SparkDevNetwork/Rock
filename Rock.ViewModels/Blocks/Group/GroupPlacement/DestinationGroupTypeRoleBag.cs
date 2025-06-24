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
    /// Represents a role within a destination group type, including its display name and member count constraints.
    /// </summary>
    public class DestinationGroupTypeRoleBag
    {
        /// <summary>
        /// The encrypted identifier key for the group role.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// The display name of the group role.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The optional maximum number of people allowed in this role within the group.
        /// </summary>
        public int? MaxCount { get; set; }

        /// <summary>
        /// The optional minimum number of people required in this role within the group.
        /// </summary>
        public int? MinCount { get; set; }
    }
}
