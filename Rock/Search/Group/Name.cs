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
            return new GroupService( new RockContext() ).Queryable()
                .Where( g => g.GroupType.ShowInNavigation
                    && g.Name.Contains( searchTerm ) );
        }

        /// <inheritdoc/>
        public override IOrderedQueryable<object> SearchQuery( string searchTerm )
        {
            return GetSearchResults( searchTerm )
                .OrderBy( g => g.Name );
        }

        /// <summary>
        /// Returns a list of matching groups
        /// </summary>
        /// <param name="searchterm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search( string searchterm )
        {
            var groupQry = ( IQueryable<Model.Group> ) SearchQuery( searchterm );

            // Note: extra spaces intentional with the label span to keep the markup from showing in the search input on selection
            return GetSearchResults( searchterm )
                .OrderBy( g => g.Name )
                .Select( g => g.Campus == null ? g.Name : g.Name + "                                               <span class='search-accessory label label-default pull-right'>" + (g.Campus.ShortCode != "" ? g.Campus.ShortCode : g.Campus.Name) + "</span>" );
        }
    }
}
