﻿// <copyright>
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
using Rock.Web.Cache;

namespace Rock.Search.Person
{
    /// <summary>
    /// Searches for businesses with matching names
    /// </summary>
    [Description("Business Name Search")]
    [Export(typeof(SearchComponent))]
    [ExportMetadata("ComponentName", "Business Name")]
    [Rock.SystemGuid.EntityTypeGuid( "944ACDD0-A4AC-4E5A-8689-E2D8EF773BC2")]
    public class BusinessName : SearchComponent
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
                defaults.Add( "SearchLabel", "Business" );
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
            var recordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

            return new PersonService( new RockContext() ).Queryable()
                .Where( q => q.RecordTypeValueId == recordTypeValueId && q.LastName.Contains( searchTerm ) );
        }

        /// <inheritdoc/>
        public override IOrderedQueryable<object> SearchQuery( string searchTerm )
        {
            return GetSearchResults( searchTerm )
                .OrderBy( p => p.LastName );
        }

        /// <summary>
        /// Returns a list of matching businesses
        /// </summary>
        /// <param name="searchterm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search( string searchterm )
        {
            var searchQry = GetSearchResults( searchterm );

            // Note: extra spaces intentional with the label span to keep the markup from showing in the search input on selection
            return searchQry
                .OrderBy( q => q.LastName )
            .Select( b => b.PrimaryCampus == null ? b.LastName : b.LastName + "                                                          <span class='search-accessory label label-default pull-right'>" + (b.PrimaryCampus.ShortCode != "" ? b.PrimaryCampus.ShortCode : b.PrimaryCampus.Name) + "</span>" )
                .Distinct();
        }
    }
}
