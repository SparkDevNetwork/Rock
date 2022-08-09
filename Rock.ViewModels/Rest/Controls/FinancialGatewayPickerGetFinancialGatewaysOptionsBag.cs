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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetFinancialGateways API action of
    /// the FinancialGatewayPicker control.
    /// </summary>
    public class FinancialGatewayPickerGetFinancialGatewaysOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether inactive gateways should be included.
        /// This checks both the FinancialGateway model and the GatewayComponent.
        /// </summary>
        /// <value><c>true</c> if [show inactive]; otherwise, <c>false</c>.</value>
        public bool IncludeInactive { get; set; } = false;

        /// <summary>
        /// If set to true then gateways that do not support Rock initiated transactions will be included.
        /// These GatewayComponents are used to download externally created transactions and do not allow Rock
        /// to create the transaction.
        /// <strong>This property does not affect if inactive gateways are shown or not.</strong>
        /// The inclusion or exclusion of inactive gateways is controlled exclusively by the "IncludeInactive" property.
        /// </summary>
        /// <value><c>true</c> if [show all gateway components]; otherwise, <c>false</c>.</value>
        public bool ShowAllGatewayComponents { get; set; } = false;
    }
}
