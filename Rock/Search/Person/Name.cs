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

using Rock.Attribute;
using Rock.Data;
using Rock.Enums;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Search.Person
{
    /// <summary>
    /// Searches for people with matching names
    /// </summary>
    [Description("Person Name Search")]
    [Export(typeof(SearchComponent))]
    [ExportMetadata("ComponentName", "Person Name")]
    [BooleanField("Allow Search by Only First Name", "By default, when searching with only one name (without a space or comma), only people with a matching Last Names will be included.  Select this option to also include people with a matching First Name", false, "", 4, "FirstNameSearch")]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.SEARCH_COMPONENT_PERSON_NAME )]
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
        /// The URL to redirect user to after they've entered search criteria
        /// </summary>
        public override string ResultUrl
        {
            get
            {
                bool allowFirstNameSearch = GetAttributeValue( "FirstNameSearch" ).AsBooleanOrNull() ?? false;
                if ( allowFirstNameSearch )
                {
                    string url = base.ResultUrl;
                    return url + ( url.Contains( "?" ) ? "&" : "?" ) + "AllowFirstNameOnly=true";
                }

                return base.ResultUrl;
            }
        }

        /// <inheritdoc/>
        public override IOrderedQueryable<object> SearchQuery( string searchTerm )
        {
            bool allowFirstNameSearch = GetAttributeValue( "FirstNameSearch" ).AsBooleanOrNull() ?? false;

            return new PersonService( new RockContext() )
                .GetByFullNameOrdered( searchTerm, true, false, allowFirstNameSearch, out _);
        }

        /// <summary>
        /// Returns a list of matching people
        /// </summary>
        /// <param name="searchterm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search( string searchterm )
        {
            bool allowFirstNameSearch = GetAttributeValue( "FirstNameSearch" ).AsBooleanOrNull() ?? false;

            bool reversed = false;
            var qry = new PersonService( new RockContext() ).GetByFullNameOrdered( searchterm, true, false, allowFirstNameSearch, out reversed );

            IQueryable<string> resultQry;

            var disableCampusLabel = CampusCache.All( false ).Count() == 1;

            // Note: extra spaces intentional with the label span to keep the markup from showing in the search input on selection
            if ( reversed )
            {
                if ( disableCampusLabel )
                {
                    resultQry = qry.Select( p => p.LastName + ", " + p.NickName ).Distinct();
                }
                else
                {
                    resultQry = qry.Select( p => p.PrimaryCampus == null ? p.LastName + ", " + p.NickName : p.LastName + ", " + p.NickName + "                                             <span class='search-accessory label label-default pull-right'>" + (p.PrimaryCampus.ShortCode != "" ? p.PrimaryCampus.ShortCode : p.PrimaryCampus.Name) + "</span>" ).Distinct();
                }
                
            }
            else
            {
                if ( disableCampusLabel )
                {
                    resultQry = qry.Select( p => p.NickName + " " + p.LastName ).Distinct();
                }
                else
                {
                    resultQry = qry.Select( p => p.PrimaryCampus == null ? p.NickName + " " + p.LastName : p.NickName + " " + p.LastName + "                                               <span class='search-accessory label label-default pull-right'>" + (p.PrimaryCampus.ShortCode != "" ? p.PrimaryCampus.ShortCode : p.PrimaryCampus.Name) + "</span>" ).Distinct();
                }  
            }

            return resultQry;
        }
    }
}
