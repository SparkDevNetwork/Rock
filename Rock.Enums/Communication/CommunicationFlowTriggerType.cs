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

namespace Rock.Enums.Communication
{
    /// <summary>
    /// Represents the types of triggers that determine when a Communication Flow is executed.
    /// </summary>
    public enum CommunicationFlowTriggerType
    {
        /// <summary>
        /// The Communication Flow runs on a recurring schedule (e.g., weekly, monthly).
        /// </summary>
        Recurring = 1,

        /// <summary>
        /// The Communication Flow runs on demand, such as by a Workflow or manual event.
        /// </summary>
        OnDemand = 2,

        /// <summary>
        /// The Communication Flow is scheduled to run only once at a specific date and time.
        /// </summary>
        OneTime = 3,
    }
}
