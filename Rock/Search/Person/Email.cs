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
        /// Returns a list of matching people
        /// </summary>
        /// <param name="searchterm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search( string searchterm )
        {
            var personService = new PersonService( new RockContext() );

            return personService.Queryable().
                Where( p => p.Email.Contains( searchterm ) ).
                OrderBy( p => p.Email ).
                Select( p => p.Email ).Distinct();
        }
    }
}