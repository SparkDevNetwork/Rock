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

namespace Rock.Enums.Blocks.Engagement.SignUp
{
    /// <summary>
    /// The register mode options available in the Sign-Up Register block.
    /// </summary>
    public enum RegisterMode
    {
        /// <summary>
        /// The logged-in user and any family members will be presented for selection.
        /// </summary>
        Family = 0,

        /// <summary>
        /// An anonymous form (with the logged-in user pre-filled, if applicable) will be presented.
        /// </summary>
        Anonymous = 1,

        /// <summary>
        /// The specified group's members - assuming the logged-in user belongs to the group - will be presented for selection.
        /// </summary>
        Group = 2
    }
}
