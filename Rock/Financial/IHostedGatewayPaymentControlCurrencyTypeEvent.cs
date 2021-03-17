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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Web.Cache;

namespace Rock.Financial
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHostedGatewayPaymentControlCurrencyTypeEvent
    {
        /// <summary>
        /// Gets the currency type value.
        /// </summary>
        /// <value>
        /// The currency type value.
        /// </value>
        DefinedValueCache CurrencyTypeValue { get; }

        /// <summary>
        /// Occurs when a payment token is received from the hosted gateway
        /// </summary>
        event EventHandler<HostedGatewayPaymentControlCurrencyTypeEventArgs> CurrencyTypeChange;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class HostedGatewayPaymentControlCurrencyTypeEventArgs : EventArgs
    {
        /// <summary>
        /// The hosted gateway payment control
        /// </summary>
        public IHostedGatewayPaymentControlCurrencyTypeEvent hostedGatewayPaymentControl;
    }


}
