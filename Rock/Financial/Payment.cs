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
using System.Collections.Generic;

using DotLiquid;

using Rock.Web.Cache;

namespace Rock.Financial
{
    /// <summary>
    /// Information about a scheduled payment transaction that has been processed
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the transaction code.
        /// </summary>
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the transaction date time.
        /// </summary>
        public DateTime TransactionDateTime { get; set; }

        /// <summary>
        /// Gets the currency type value.
        /// </summary>
        public virtual DefinedValueCache CurrencyTypeValue { get; set; }

        /// <summary>
        /// Gets the credit card type value id.
        /// </summary>
        public virtual DefinedValueCache CreditCardTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the gateway schedule id.
        /// </summary>
        public string GatewayScheduleId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether schedule is still active.
        /// </summary>
        public bool? ScheduleActive { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the transaction has been settled by the processor/gateway.
        /// </summary>
        public bool? IsSettled { get; set; }

        /// <summary>
        /// The group/batch identifier used by the processor/gateway when the transaction has been settled.
        /// </summary>
        public string SettledGroupId { get; set; }

        /// <summary>
        /// Gets or sets the date that the transaction was settled by the processor/gateway.
        /// </summary>
        public DateTime? SettledDate { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is failure.
        /// </summary>
        public bool IsFailure { get; set; }

        /// <summary>
        /// Additional payment attributes
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; }
    }
}