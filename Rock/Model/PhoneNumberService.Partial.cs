//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        /// <summary>
        /// Returns a list of phone numbers that match the given search term.
        /// </summary>
        /// <param name="searchterm">A partial phone number search string (everything but digits will be removed before attempting the search).</param>
        /// <returns>A querable list of <see cref="System.String"/> phone numbers (as strings)</returns>
        public IQueryable<string> GetNumbersBySearchterm( string searchterm )
        {
            return GetBySearchterm( searchterm ).OrderBy( n => n.Number ).
                Select( n => n.Number ).Distinct();
        }

        /// <summary>
        /// Returns a list of PersonIds <see cref="System.Int32"/> of people who have a phone number that match the given search term.
        /// </summary>
        /// <param name="searchterm">A partial phone number search string (everything but digits will be removed before attempting the search).</param>
        /// <returns>A querable list of <see cref="System.Int32"/> PersonIds</returns>
        public IQueryable<int> GetPersonIdsByNumber( string searchterm )
        {
            return GetBySearchterm( searchterm ).Select( n => n.PersonId ).Distinct();
        }

        /// <summary>
        /// Returns a querable set of <see cref="Rock.Model.PhoneNumber">Phone Numbers</see> that match the given search term.
        /// </summary>
        /// <param name="searchterm">A partial phone number search string (everything but digits will be removed before attempting the search).</param>
        /// <returns>A querable list of <see cref="Rock.Model.PhoneNumber">PhoneNumbers</see></returns>
        public IQueryable<PhoneNumber> GetBySearchterm( string searchterm )
        {
            // remove everything but numbers
            Regex rgx = new Regex( @"[^\d]" );
            searchterm = rgx.Replace( searchterm, "" );

            return Repository.AsQueryable().
                Where( n => n.Number.Contains( searchterm ) );
        }
    }
}
