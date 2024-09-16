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
    /// Contains the information to create a payment plan in the Registration Entry block.
    /// </summary>
    public class RegistrationEntryCreatePaymentPlanRequestBag
    {
        /// <summary>
        /// Gets or sets the transaction frequency unique identifier.
        /// </summary>
        /// <value>
        /// The transaction frequency unique identifier.
        /// </value>
        public Guid TransactionFrequencyGuid { get; set; }
        
        /// <summary>
        /// Gets or sets the transaction frequency text.
        /// </summary>
        /// <value>
        /// The transaction frequency text.
        /// </value>
        public string TransactionFrequencyText { get; set; }

        /// <summary>
        /// Gets or sets the amount to pay per payment.
        /// </summary>
        /// <value>
        /// The amount to pay per payment.
        /// </value>
        public decimal AmountPerPayment { get; set; }

        /// <summary>
        /// Gets or sets the number of payments.
        /// </summary>
        /// <value>
        /// The number of payments.
        /// </value>
        public int NumberOfPayments { get; set; }

        /// <summary>
        /// Gets or sets the start date of the recurring payments.
        /// </summary>
        /// <value>
        /// The start date of the recurring payments.
        /// </value>
        public DateTimeOffset StartDate { get; set; }
    }
}
