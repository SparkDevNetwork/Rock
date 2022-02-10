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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// FinancialTransactionDetail Logic
    /// </summary>
    public partial class FinancialTransactionDetail
    {
        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this detail item.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this detail item.
        /// </returns>
        public override string ToString()
        {
            return this.Amount.ToStringSafe();
        }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check the default authorization on the current type, and
        /// then the authorization on the Rock.Security.GlobalDefault entity
        /// </summary>
        public override ISecured ParentAuthority
        {
            get
            {
                if ( this.TransactionId != 0 )
                {
                    FinancialTransaction parentTransaction = this.Transaction;
                    if ( parentTransaction == null )
                    {
                        // All we need to auth a FinancialTransaction is FinancialTransaction object with the TransactionId
                        parentTransaction = new FinancialTransaction { Id = this.TransactionId };
                    }

                    return parentTransaction;
                }
                else
                {

                    return base.ParentAuthority;
                }
            }
        }

        #endregion Public Methods
    }
}