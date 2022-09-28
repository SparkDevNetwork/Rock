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
    /// Searches for people with matching phones
    /// </summary>
    [Description( "Person Phone Search" )]
    [Export(typeof(SearchComponent))]
    [ExportMetadata("ComponentName", "Person Phone")]
    [Rock.SystemGuid.EntityTypeGuid( "5F92ECC3-4EBD-4C41-A691-C03F1DA4F7BF")]
    public class Phone : SearchComponent
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
                defaults.Add( "SearchLabel", "Phone" );
                return defaults;
            }
        }

        /// <inheritdoc/>
        public override IOrderedQueryable<object> SearchQuery( string searchTerm )
        {
            var rockContext = new RockContext();
            var phoneNumberService = new PhoneNumberService( rockContext );
            var personService = new PersonService( rockContext );

            var personIdQry = phoneNumberService.GetPersonIdsByNumber( searchTerm );

            return personService.Queryable()
                .Where( p => personIdQry.Contains( p.Id ) )
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
            return new PhoneNumberService( new RockContext() ).GetNumbersBySearchterm( searchterm );
        }
    }
}
