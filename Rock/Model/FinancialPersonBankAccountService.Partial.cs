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
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Service and data access class for <see cref="Rock.Model.FinancialPersonBankAccount"/> objects.
    /// </summary>
    public partial class FinancialPersonBankAccountService
    {
        /// <summary>
        /// Gets the specified bank account record.
        /// </summary>
        /// <param name="routingNumber">The routing number.</param>
        /// <param name="accountNumber">The account number.</param>
        /// <returns></returns>
        public FinancialPersonBankAccount Get(string routingNumber, string accountNumber)
        {
            var encodedValue = FinancialPersonBankAccount.EncodeAccountNumber(routingNumber, accountNumber);
            return this.Queryable().Where( a => a.AccountNumberSecured == encodedValue ).FirstOrDefault();
        }
    }
}