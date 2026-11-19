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

namespace Rock.ViewModels.Blocks.Event.RegistrationDetail
{
    /// <summary>
    /// The values used to preview or update the payment plan of a registration.
    /// </summary>
    public class PaymentPlanConfigurationRequestBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the configuration should be built
        /// from the existing payment plan instead of the other values.
        /// </summary>
        public bool IsInitial { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the desired payment frequency defined value.
        /// </summary>
        public Guid? FrequencyGuid { get; set; }

        /// <summary>
        /// Gets or sets the desired start date of the payment plan.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the desired number of payments.
        /// </summary>
        public int? NumberOfPayments { get; set; }
    }
}
