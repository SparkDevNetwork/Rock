//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.PhoneNumber"/> entities.
    /// </summary>
    public partial class PhoneNumberService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.PhoneNumber">Phone Numbers</see> that belong to a <see cref="Rock.Model.Person"/>.
        /// </summary>
        /// <param name="personId">A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> to retrieve phone numbers for.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.PhoneNumber">PhoneNumbers</see> that belong to the specified <see cref="Rock.Model.Person"/>.</returns>
        public IEnumerable<PhoneNumber> GetByPersonId( int personId )
        {
            return Repository.Find( t => t.PersonId == personId );
        }
    }
}
