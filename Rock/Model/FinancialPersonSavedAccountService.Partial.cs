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
    /// Service and data access class for <see cref="Rock.Model.FinancialPersonSavedAccount"/> objects.
    /// </summary>
    public partial class FinancialPersonSavedAccountService
    {
        /// <summary>
        /// Returns an queryable collection of saved accounts (<see cref="Rock.Model.FinancialPersonSavedAccount"/> by PersonId
        /// </summary>
        /// <param name="personId">A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> to retrieve saved accounts for.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.FinancialPersonSavedAccount">Saved Accounts</see> belonging to the specified <see cref="Rock.Model.Person"/>.</returns>
        public IQueryable<FinancialPersonSavedAccount> GetByPersonId(int personId)
        {
            return this.Queryable().Where( a => a.PersonAlias.PersonId == personId );
        }
    }
}