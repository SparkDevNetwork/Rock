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

namespace Rock.Model
{
    /// <summary>
    /// A POCO to store the ID and the group name of a <seealso cref="GroupSync"/>.
    /// </summary>
    public class GroupSyncInfo
    {
        /// <summary>
        /// The ID of the <seealso cref="GroupSync"/>.
        /// </summary>
        public int SyncId { get; set; }

        /// <summary>
        /// The name of the group which the <seealso cref="GroupSync"/> is Associated with.
        /// </summary>
        public string GroupName { get; set; }
    }
}
