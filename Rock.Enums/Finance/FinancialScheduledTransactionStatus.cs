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
    /// The status of a Scheduled Transaction
    /// </summary>
    [Enums.EnumDomain( "Finance" )]
    public enum FinancialScheduledTransactionStatus
    {
        /// <summary>
        /// Scheduled Transaction is operating normally
        /// </summary>
        Active = 0,

        /// <summary>
        /// Scheduled Transaction completed
        /// </summary>
        Completed = 1,

        /// <summary>
        /// Scheduled Transaction is paused
        /// </summary>
        Paused = 2,

        /// <summary>
        /// Scheduled Transaction is cancelled
        /// </summary>
        Canceled = 3,

        /// <summary>
        /// Scheduled Transaction is failed
        /// </summary>
        Failed = 4,

        /// <summary>
        /// Scheduled Transaction is Past Due
        /// </summary>
        PastDue = 5
    }
}