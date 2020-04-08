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
using System.Text.RegularExpressions;

using Rock.Web.Cache;

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
        public IQueryable<PhoneNumber> GetByPersonId( int personId )
        {
            return Queryable().Where( t => t.PersonId == personId );
        }

        /// <summary>
        /// Returns a list of phone numbers that match the given search term.
        /// </summary>
        /// <param name="searchterm">A partial phone number search string (everything but digits will be removed before attempting the search).</param>
        /// <returns>A queryable list of <see cref="System.String"/> phone numbers (as strings)</returns>
        public IQueryable<string> GetNumbersBySearchterm( string searchterm )
        {
            return GetBySearchterm( searchterm ).OrderBy( n => n.Number ).
                Select( n => n.Number ).Distinct();
        }

        /// <summary>
        /// Returns a list of PersonIds <see cref="System.Int32"/> of people who have a phone number that match the given search term.
        /// </summary>
        /// <param name="searchterm">A partial phone number search string (everything but digits will be removed before attempting the search).</param>
        /// <returns>A queryable list of <see cref="System.Int32"/> PersonIds</returns>
        public IQueryable<int> GetPersonIdsByNumber( string searchterm )
        {
            return GetBySearchterm( searchterm ).Select( n => n.PersonId ).Distinct();
        }

        /// <summary>
        /// Returns a queryable set of <see cref="Rock.Model.PhoneNumber">Phone Numbers</see> that match the given search term.
        /// </summary>
        /// <param name="searchterm">A partial phone number search string (everything but digits will be removed before attempting the search).</param>
        /// <returns>A queryable list of <see cref="Rock.Model.PhoneNumber">PhoneNumbers</see></returns>
        public IQueryable<PhoneNumber> GetBySearchterm( string searchterm )
        {
            // remove everything but numbers
            Regex rgx = new Regex( @"[^\d]" );
            searchterm = rgx.Replace( searchterm, "" );

            // if no digits exist return empty (otherwise we'll get all records)
            if ( searchterm.Length > 0 )
            {
                return Queryable().
                    Where( n => n.Number.Contains( searchterm ) );
            }
            else
            {
                return Queryable().Where( n => n.Id == -1);
            }
        }

        /// <summary>
        /// Gets a PhoneNumber object for the supplied person ID and DefinedValue GUID
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="definedValueGuid">The type value unique identifier.</param>
        /// <returns></returns>
        public PhoneNumber GetNumberByPersonIdAndType( int personId, string definedValueGuid)
        {
            int mobilePhoneTypeId = DefinedValueCache.Get( definedValueGuid ).Id;
            return Queryable().Where( p => p.PersonId == personId && p.NumberTypeValueId == mobilePhoneTypeId ).FirstOrDefault();
        }
    }
}
