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

using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Search.Group
{
    /// <summary>
    /// Searches for groups with matching names
    /// </summary>
    [Description("Group Name Search")]
    [Export(typeof(SearchComponent))]
    [ExportMetadata("ComponentName", "Group Name")]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.SEARCH_COMPONENT_GROUP_NAME )]
    public class Name : SearchComponent
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
                defaults.Add( "SearchLabel", "Name" );
                return defaults;
            }
        }

        /// <summary>
        /// Gets the search result entity queryable that matches the search term.
        /// </summary>
        /// <param name="searchTerm">The search term used to find results.</param>
        /// <returns>A queryable of entity objects that match the search term.</returns>
        private IQueryable<Model.Group> GetSearchResults( string searchTerm )
        {
            if ( searchTerm.IsSingleSpecialCharacter() )
            {
                return Enumerable.Empty<Model.Group>().AsQueryable();
            }

            return new GroupService( RockApp.Current.CreateRockContext() ).Queryable()
                .Where( g => g.GroupType.ShowInNavigation
                    && g.Name.Contains( searchTerm ) );
        }

        /// <summary>
        /// Gets the groups that match the search term and that the current person is allowed to view.
        /// </summary>
        /// <param name="searchTerm">The search term used to find results.</param>
        /// <returns>The viewable matching groups, ordered by name.</returns>
        private List<Model.Group> GetViewableSearchResults( string searchTerm )
        {
            var currentPerson = RockRequestContextAccessor.Current?.CurrentPerson;

            // VIEW can't be checked in SQL, so every match is loaded before the filter.
            return GetSearchResults( searchTerm )
                .OrderBy( g => g.Name )
                .ToList()
                .Where( g => g.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .ToList();
        }

        /// <inheritdoc/>
        public override IOrderedQueryable<object> SearchQuery( string searchTerm )
        {
            // This is a bit of a cheat. Since everything will have the same
            // order .OrderBy() will return an IOrderedQueryable in the original
            // order of the results.
            return GetViewableSearchResults( searchTerm )
                .Cast<object>()
                .AsQueryable()
                .OrderBy( g => true );
        }

        /// <summary>
        /// Returns a list of matching groups
        /// </summary>
        /// <param name="searchterm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search( string searchterm )
        {
            /*
                10/18/2026 - MSE

                Smart Search renders each result string as HTML, so the group name and campus
                values are HTML encoded. Encoding can't be done in SQL, so the results are
                materialized first and capped at 50 after the VIEW filter.

                Reason: Prevent stored XSS through group names in Smart Search results.
            */
            var groupItems = GetViewableSearchResults( searchterm )
                .Take( 50 )
                .Select( g =>
                {
                    var campus = g.CampusId.HasValue ? CampusCache.Get( g.CampusId.Value ) : null;

                    return new
                    {
                        g.Name,
                        HasCampus = campus != null,
                        CampusText = campus?.ShortCode.IsNotNullOrWhiteSpace() == true ? campus.ShortCode : campus?.Name
                    };
                } )
                .ToList();

            // Note: extra spaces intentional with the label span to keep the markup from showing in the search input on selection
            return groupItems
                .Select( g => !g.HasCampus ? g.Name.EncodeHtml() : g.Name.EncodeHtml() + "                                               <span class='search-accessory label label-default pull-right'>" + g.CampusText.EncodeHtml() + "</span>" )
                .AsQueryable();
        }
    }
}
