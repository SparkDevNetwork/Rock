//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
            return this.Queryable().Where( a => a.PersonId == personId );
        }
    }
}