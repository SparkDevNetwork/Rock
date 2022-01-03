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

using Rock.UniversalSearch;

namespace Rock.Search.Other
{
    /// <summary>
    /// Searches for people with matching names
    /// </summary>
    [Description("Universal Search")]
    [Export(typeof(SearchComponent))]
    [ExportMetadata("ComponentName", "Universal Search")]
    public class Universal : SearchComponent
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
                defaults.Add( "SearchLabel", "Search" );
                return defaults;
            }
        }

        /// <summary>
        /// Gets the search result queryable that matches the search term.
        /// </summary>
        /// <param name="searchTerm">The search term used to find results.</param>
        /// <returns>A queryable of index models that match the search term.</returns>
        private List<UniversalSearch.IndexModels.IndexModelBase> GetSearchResults( string searchTerm )
        {
            // get configured entities and turn it into a list of entity ids
            List<int> entityIds = new List<int>();

            var searchEntitiesSetting = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchEntities" );

            if ( !string.IsNullOrWhiteSpace( searchEntitiesSetting ) )
            {
                entityIds = searchEntitiesSetting.Split( ',' ).Select( int.Parse ).ToList();
            }

            // get the field criteria
            var fieldCriteriaSetting = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchFieldCriteria" );
            SearchFieldCriteria fieldCriteria = new SearchFieldCriteria();

            // get the search type
            var searchType = SearchType.Wildcard;

            if ( !string.IsNullOrWhiteSpace( Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchSearchType" ) ) )
            {
                searchType = ( SearchType ) Enum.Parse( typeof( SearchType ), Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchSearchType" ) );
            }

            if ( !string.IsNullOrWhiteSpace( fieldCriteriaSetting ) )
            {
                foreach ( var queryString in fieldCriteriaSetting.ToKeyValuePairList() )
                {
                    // check that multiple values were not passed
                    var values = queryString.Value.ToString().Split( ',' );

                    foreach ( var value in values )
                    {
                        fieldCriteria.FieldValues.Add( new FieldValue { Field = queryString.Key, Value = value } );
                    }
                }
            }

            var client = IndexContainer.GetActiveComponent();

            return client.Search( searchTerm, searchType, entityIds, fieldCriteria );
        }

        /// <inheritdoc/>
        public override IOrderedQueryable<object> SearchQuery( string searchTerm )
        {
            // This is a bit of a cheat. Since everything will have the same
            // order .OrderBy() will return an IOrderedQueryable in the original
            // order of the results.
            return GetSearchResults( searchTerm )
                .AsQueryable()
                .OrderBy( a => true );
        }

        /// <summary>
        /// Returns a list of matching people
        /// </summary>
        /// <param name="searchterm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search( string searchterm )
        {
            var results = GetSearchResults( searchterm );

            // NOTE: Put a bunch of whitespace before and after it so that the Search box shows blank instead of stringified html
            return results.Select( r => $"                                                                       <data return-type='{r.IndexModelType}' return-id={r.Id}></data><i class='{ r.IconCssClass}'></i> {r.DocumentName}                                                                               " ).ToList().AsQueryable();
        }
    }
}
