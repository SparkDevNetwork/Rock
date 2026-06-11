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

namespace Rock.Utility
{
    /// <summary>
    /// Controls whether campaign connection requests are created all at once
    /// or only as needed to meet the daily connection limit.
    /// </summary>
    [Enums.EnumDomain( "Connection" )]
    public enum CreateConnectionRequestOptions
    {
        /// <summary>
        /// Create requests only as needed, based on how many assigned requests
        /// each connector currently has.
        /// </summary>
        AsNeeded = 0,

        /// <summary>
        /// Create all connection requests at once when the job runs.
        /// </summary>
        AllAtOnce = 1,
    }
}
