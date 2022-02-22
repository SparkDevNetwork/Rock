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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Data;
using Rock.Model;

namespace Rock.Search.Person
{
    /// <summary>
    /// Searches for people with matching birthdates
    /// </summary>
    [Description( "Person Birthdate Search" )]
    [Export( typeof( SearchComponent ) )]
    [ExportMetadata( "ComponentName", "Person Birthdate" )]
    public class BirthDate : SearchComponent
    {
        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>();
                defaults.Add( "SearchLabel", "Birthdate" );
                return defaults;
            }
        }

        /// <summary>
        /// Gets the search result entity queryable that matches the search term.
        /// </summary>
        /// <param name="searchTerm">The search term used to find results.</param>
        /// <returns>A queryable of entity objects that match the search term.</returns>
        private IQueryable<Model.Person> GetSearchResults( string searchTerm )
        {
            // notes, due to how AsDateTime() works:
            // * a searchterm of 'mm/d' will get converted to 'mm/dd/currentyear'
            // * 1 or 2 digit years will assume the current century
            // * 3 digit years will result in a year < 1000
            DateTime? birthDate = searchTerm.AsDateTime();

            if ( !birthDate.HasValue || birthDate.Value.Year < 1000 )
            {
                // return no results if a full birthdate hasn't been specified
                return new List<Model.Person>().AsQueryable();
            }

            var personService = new PersonService( new RockContext() );

            return personService.Queryable()
                .Where( a => a.BirthDate.HasValue && a.BirthDate.Value == birthDate );
        }

        /// <inheritdoc/>
        public override IOrderedQueryable<object> SearchQuery( string searchTerm )
        {
            return GetSearchResults( searchTerm )
                .OrderBy( p => p.NickName )
                .ThenBy( p => p.LastName );
        }

        /// <summary>
        /// Returns a list of matching people
        /// </summary>
        /// <param name="searchterm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search( string searchterm )
        {
            var qryList = GetSearchResults( searchterm )
                .Select( a => new
                {
                    a.Id,
                    BirthDate = a.BirthDate.Value,
                    a.NickName,
                    a.LastName
                } )
                .ToList()
                .Select( a => new
                {
                    a.Id,
                    BirthDateParam = a.BirthDate.ToString("o").UrlEncode(),
                    a.NickName,
                    a.LastName
                } ).ToList();

            // NOTE: Put a bunch of whitespace before and after it so that the Search box shows blank instead of stringified html
            var results = qryList.Select( r => $"                                                                       <data birthdate='{r.BirthDateParam}' person-id={r.Id}></data>{r.NickName + " " + r.LastName}                                                                       " ).ToList().AsQueryable();

            return results;
        }
    }
}
