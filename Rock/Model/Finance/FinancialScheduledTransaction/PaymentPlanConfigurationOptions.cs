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
using Rock.Attribute;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Options for building a payment plan configuration.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.16.6" )]
    public class PaymentPlanConfigurationOptions
    {
        /// <summary>
        /// Gets or sets the currency precision for the amounts in the plan.
        /// </summary>
        /// <remarks>
        /// The number of decimals to the right of the decimal point. For USD and many other currencies, this would be 2; e.g., $2.77 has a precision of 2.
        /// </remarks>
        public int CurrencyPrecision { get; set; }

        /// <summary>
        /// Gets or sets the desired, allowed payment frequencies.
        /// </summary>
        public List<DefinedValueCache> DesiredAllowedPaymentFrequencies { get; set; }

        /// <summary>
        /// Gets or sets the desired number of payments for the payment plan.
        /// </summary>
        /// <remarks>
        /// See <see cref="IsNumberOfPaymentsLimited"/> for details on how this value may be different in the resulting payment plan.
        /// </remarks>
        public int DesiredNumberOfPayments { get; set; }

        /// <summary>
        /// Gets or sets the desired payment frequency; e.g., monthly, weekly, etc.
        /// </summary>
        /// <value>
        /// Should be one of the <see cref="DesiredAllowedPaymentFrequencies"/>; otherwise, the resulting <see cref="PaymentPlanConfiguration.PaymentFrequencyConfiguration"/> will be <see langword="null"/>.
        /// </value>
        public DefinedValueCache DesiredPaymentFrequency { get; set; }

        /// <summary>
        /// Gest or sets the desired date when the payment plan payments should start.
        /// </summary>
        public DateTime DesiredStartDate { get; set; }

        /// <summary>
        /// Gets or sets the date when the payment plan payments should end.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to limit the number of payments from the <see cref="MinNumberOfPayments"/> to the max allowed for the selected <see cref="DesiredPaymentFrequency"/>.
        /// </summary>
        /// <value>
        /// If set to <see langword="true"/> and the resulting number of payments would exceed the payment plan end date, the value would instead be capped at the max number of payments.
        /// <para>If set to <see langword="false"/>, the number of payments will not be capped and could exceed the payment plan end date.</para>
        /// </value>
        public bool IsNumberOfPaymentsLimited { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of payments allowed for selection.
        /// </summary>
        public int MinNumberOfPayments { get; set; }

        /// <summary>
        /// Gets or sets the amount that can be configured for a payment plan.
        /// </summary>
        /// <remarks>
        ///     Since the amount may not be evenly divided, the resulting <see cref="PaymentPlanConfiguration.PlannedAmount"/> may be less than this number.
        ///     <para>
        ///         The remainder can be found by <c>PaymentPlanConfigurationOptions.AmountForPaymentPlan - PaymentPlanConfiguration.PlannedAmount</c>.
        ///     </para>
        /// </remarks>
        public decimal AmountForPaymentPlan { get; set; }
    }
}
