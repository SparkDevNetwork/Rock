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

namespace Rock.Model.Finance.FinancialPersonSavedAccountService.Options
{
    /// <summary>
    /// Describes the query options when retrieving <see cref="FinancialPersonSavedAccount"/>
    /// objects from the database.
    /// </summary>
    public class FinancialPersonSavedAccountQueryOptions
    {
        /// <summary>
        /// Gets or sets the person identifiers to limit the results to.
        /// </summary>
        /// <value>The person identifiers to limit the results to.</value>
        public List<int> PersonIds { get; set; }

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
