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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Data;
using Rock.Model;

namespace Rock.Search.Person
{
    /// <summary>
    /// Searches for people who's email matches selected term
    /// </summary>
    [Description( "Person Email Search" )]
    [Export(typeof(SearchComponent))]
    [ExportMetadata("ComponentName", "Person Email")]
    [Rock.SystemGuid.EntityTypeGuid( "00095C10-72C9-4C82-844E-AE8B146DE4F1")]
    public class Email : SearchComponent
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
                defaults.Add( "SearchLabel", "Email" );
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
            var personService = new PersonService( new RockContext() );

            return personService.Queryable()
                .Where( p => p.Email.Contains( searchTerm ) );
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
            return GetSearchResults( searchterm )
                .OrderBy( p => p.Email )
                .Select( p => p.Email )
                .Distinct();
        }
    }
}
