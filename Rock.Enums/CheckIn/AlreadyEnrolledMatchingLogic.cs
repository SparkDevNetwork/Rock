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

namespace Rock.Enums.CheckIn
{
    /// <summary>
    /// Determines the matching logic used when the area attendance rule
    /// is set to already enrolled.
    /// </summary>
    public enum AlreadyEnrolledMatchingLogic
    {
        /// <summary>
        /// The person must be enrolled as an active member of the group.
        /// </summary>
        MustBeEnrolled = 0,

        /// <summary>
        /// The person must be enrolled as an active member of the group. In
        /// addition, the group will be marked as preferred. If any preferred
        /// group is available then all non-preferred groups are removed.
        /// </summary>
        PreferEnrolledGroups = 1
    }
}
