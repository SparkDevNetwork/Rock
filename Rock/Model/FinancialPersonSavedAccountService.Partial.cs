//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Service class for Financial Person Saved Account objects.
    /// </summary>
    public partial class FinancialPersonSavedAccountService
    {
        /// <summary>
        /// Gets saved accounts by person id
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public IQueryable<FinancialPersonSavedAccount> GetByPersonId(int personId)
        {
            return this.Queryable().Where( a => a.PersonId == personId );
        }
    }
}