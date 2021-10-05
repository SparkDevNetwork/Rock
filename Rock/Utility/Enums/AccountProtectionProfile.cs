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

namespace Rock.Utility.Enums
{
    /// <summary>
    /// This enum is used be the merge and duplicate prevention processes to determine if a person can be merged.
    /// </summary>
    public enum AccountProtectionProfile
    {
        /// <summary>
        /// The low
        /// </summary>
        Low = 0,

        /// <summary>
        /// The medium
        /// </summary>
        Medium = 1,

        /// <summary>
        /// The high
        /// </summary>
        High = 2,

        /// <summary>
        /// The extreme
        /// </summary>
        Extreme = 3,
    }
}
