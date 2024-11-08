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
using System;

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// Contains the information to get scheduled payment dates in the Registration Entry block.
    /// </summary>
    public class RegistrationEntryGetScheduledPaymentDatesRequestBag
    {
        /// <summary>
        /// Gets or sets the scheduled transaction frequency value unique identifier.
        /// </summary>
        public Guid ScheduledTransactionFrequencyValueGuid { get; set; }
        
        /// <summary>
        /// Gets or sets the payment start date.
        /// </summary>
        public DateTime PaymentStartDate { get; set; }

        /// <summary>
        /// Gets or sets the number of payments.
        /// </summary>
        public int NumberOfPayments { get; set; }
    }
}
