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

using System.ComponentModel;

namespace Rock.Enums.Connection
{
    /// <summary>
    /// Due Date Calculation Mode
    /// </summary>
    public enum DueDateCalculationMode
    {
        /// <summary>
        /// Fixed Days from Start configured on Connection Type
        /// </summary>
        [Description( "Fixed Days From Start (Type Level)" )]
        FixedDaysFromStartTypeLevel = 0,

        /// <summary>
        /// Fixed Days from Start configured on Connection Opportunity
        /// </summary>
        [Description( "Fixed Days From Start (Opportunity Level)" )]
        FixedDaysFromStartOpportunityLevel = 1,

        /// <summary>
        /// Sets the due date according to the allowed time for the request’s current status, updating each time the status changes.
        /// </summary>
        DurationPerStatus = 2,
    }
}
