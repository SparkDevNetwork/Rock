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
    /// Data access/service class for <see cref="Rock.Model.FinancialAccount"/> objects.
    /// </summary>
    public partial class FinancialAccountService
    {

        /// <summary>
        /// Gets immediate children of a account (id) or a rootGroupId. Specify 0 for both Id and rootGroupId to get top level accounts limited
        /// </summary>
        /// <param name="id">The ID of the account to get the children of (or 0 to use rootAccountId)</param>
        /// <param name="includeInactiveAccounts">if set to <c>true</c> [include inactive Accounts].</param>
        /// <returns></returns>
        public IQueryable<FinancialAccount> GetChildren( int id, bool includeInactiveAccounts )
        {
            var qry = Queryable();

            if ( id == 0 )
            {
                qry = qry.Where( a => a.ParentAccountId == null );
            }
            else
            {
                qry = qry.Where( a => a.ParentAccountId == id );
            }

            if ( !includeInactiveAccounts )
            {
                qry = qry.Where( a => a.IsActive );
            }

            return qry;
        }
    }
}
