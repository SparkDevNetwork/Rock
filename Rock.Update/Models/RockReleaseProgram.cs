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

namespace Rock.Update.Models
{
    /// <summary>
    /// One of three options that represent which release 'program' one can be on.
    /// </summary>
    public enum RockReleaseProgram
    {
        /// <summary>
        /// Check for Rock updates for Alpha testers
        /// </summary>
        Alpha = 1,

        /// <summary>
        /// Check for Rock updates for Beta testers
        /// </summary>
        Beta = 2,

        /// <summary>
        /// Check for Rock updates released to Production
        /// </summary>
        Production = 3,

        /// <summary>
        /// Check for Rock updates that are in pre-release test
        /// </summary>
        Test = 4
    }
}
