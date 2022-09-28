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
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// FinancialPersonBankAccount Logic
    /// </summary>
    public partial class FinancialPersonBankAccount : Model<FinancialPersonBankAccount>
    {
        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.AccountNumberMasked;
        }

        /// <summary>
        /// Encodes the account number.
        /// </summary>
        /// <param name="routingNumber">The routing number.</param>
        /// <param name="accountNumber">The account number.</param>
        /// <returns></returns>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">Account encoding requires a 'PasswordKey' app setting</exception>
        public static string EncodeAccountNumber( string routingNumber, string accountNumber )
        {
            string toHash = string.Format( "{0}|{1}", routingNumber.Trim(), accountNumber.Trim() );
            return Rock.Security.Encryption.GetSHA1Hash( toHash );
        }

        #endregion
    }
}