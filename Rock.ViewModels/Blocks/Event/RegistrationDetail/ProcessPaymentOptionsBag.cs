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

using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Event.RegistrationDetail
{
    /// <summary>
    /// The values needed to show the form used to charge a new payment.
    /// </summary>
    public class ProcessPaymentOptionsBag
    {
        /// <summary>
        /// Gets or sets the configuration of the gateway control.
        /// </summary>
        public GatewayControlBag GatewayControl { get; set; }

        /// <summary>
        /// Gets or sets the default amount of the payment.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the default payer.
        /// </summary>
        public ListItemBag Payer { get; set; }

        /// <summary>
        /// Gets or sets the default billing address.
        /// </summary>
        public AddressControlBag BillingAddress { get; set; }
    }
}
