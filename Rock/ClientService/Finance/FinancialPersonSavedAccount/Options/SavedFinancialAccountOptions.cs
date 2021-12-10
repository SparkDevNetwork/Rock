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

namespace Rock.ClientService.Finance.FinancialPersonSavedAccount.Options
{
    /// <summary>
    /// The options that describe the saved financial account queries in
    /// <see cref="FinancialPersonSavedAccountClientService"/>.
    /// </summary>
    public class SavedFinancialAccountOptions
    {
        /// <summary>
        /// Gets or sets the financial gateway unique identifiers to limit the
        /// results to.
        /// </summary>
        /// <value>The financial gateway unique identifiers.</value>
        public List<Guid> FinancialGatewayGuids { get; set; }

        /// <summary>
        /// Gets or sets the currency type unique identifiers to limit the
        /// results to.
        /// </summary>
        /// <value>The currency type unique identifiers.</value>
        public List<Guid> CurrencyTypeGuids { get; set; }
    }
}
