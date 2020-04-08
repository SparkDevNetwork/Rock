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

using Rock.Web.Cache;

namespace Rock.Financial
{
    /// <summary>
    /// Information related to a scheduled payment frequency
    /// </summary>
    public class PaymentSchedule
    {
        /// <summary>
        /// Gets or sets the person id that is authorizing the payment schedule
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the transaction frequency value.
        /// </summary>
        /// <value>
        /// The transaction frequency value.
        /// </value>
        public DefinedValueCache TransactionFrequencyValue { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the number of payments.
        /// </summary>
        /// <value>
        /// The number of payments.
        /// </value>
        public int? NumberOfPayments { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} starting on {1}",
                TransactionFrequencyValue.Description, StartDate.ToShortDateString() );
        }
    }
}
