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
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Search.Person
{
    /// <summary>
    /// Searches for people with matching names
    /// </summary>
    [Description( "Person Name Search" )]
    [Export( typeof( SearchComponent ) )]
    [ExportMetadata( "ComponentName", "Person Name" )]
    [BooleanField( "Allow Search by Only First Name", "By default, when searching with only one name (without a space or comma), only people with a matching Last Names will be included.  Select this option to also include people with a matching First Name", false, "", 4, "FirstNameSearch" )]
    [Rock.SystemGuid.EntityTypeGuid( "3B1D679A-290F-4A53-8E11-159BF0517A19" )]
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
                .GetByFullNameOrdered( searchTerm, true, false, allowFirstNameSearch, out _ );
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

            // Get the full list of ordered people from this search term.
            var qry = new PersonService( new RockContext() ).GetByFullNameOrdered( searchterm, true, false, allowFirstNameSearch, out reversed );

            IQueryable<string> resultQry;

            // If there isn't more than one campus configured, we don't
            // need to show the label.
            var disableCampusLabel = CampusCache.All( false ).Count() == 1;

            // Since we need to .Distinct() this, we convert it into
            // an anonymous type and re-order (since Distinct() in SQL throws
            // order out of the window).
            var personNameSearchBags = qry.Select( p => new PersonNameSearchBag
            {
                NickName = p.NickName,
                LastName = p.LastName,
                CampusName = disableCampusLabel || p.PrimaryCampus == null ? "" : p.PrimaryCampus.Name,
                CampusShortCode = disableCampusLabel || p.PrimaryCampus == null ? "" : p.PrimaryCampus.ShortCode
            } ).Distinct();

            personNameSearchBags = OrderByFullName( personNameSearchBags, reversed );

            if ( reversed )
            {
                if ( disableCampusLabel )
                {
                    resultQry = personNameSearchBags.Select( p => p.LastName + ", " + p.NickName );
                }
                else
                {
                    // Note: extra spaces intentional with the label span to keep the markup from showing in the search input on selection
                    resultQry = personNameSearchBags.Select( p =>
                        ( p.CampusShortCode == null || p.CampusShortCode == "" ) || ( p.CampusName == null || p.CampusName == "" )
                        ? p.LastName + ", " + p.NickName
                        : p.LastName + ", " + p.NickName + "                                             <span class='search-accessory label label-default pull-right'>" + ( p.CampusShortCode != "" ? p.CampusShortCode : p.CampusName ) + "</span>" );
                }
            }
            else
            {
                if ( disableCampusLabel )
                {
                    resultQry = personNameSearchBags.Select( p => p.NickName + " " + p.LastName );
                }
                else
                {
                    // Note: extra spaces intentional with the label span to keep the markup from showing in the search input on selection
                    resultQry = personNameSearchBags.Select( p =>
                        ( p.CampusShortCode == null || p.CampusShortCode == "" ) || ( p.CampusName == null || p.CampusName == "" )
                        ? p.NickName + " " + p.LastName
                        : p.NickName + " " + p.LastName + "                                               <span class='search-accessory label label-default pull-right'>" + ( p.CampusShortCode != "" ? p.CampusShortCode : p.CampusName ) + "</span>" );
                }
            }

            return resultQry;
        }

        /// <summary>
        /// A class used to store information about a person when
        /// being searched for.
        /// </summary>
        private class PersonNameSearchBag
        {
            /// <summary>
            /// Gets or sets the nickname of the person.
            /// </summary>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the last name of the person.
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the campus name of the person.
            /// </summary>
            public string CampusName { get; set; }

            /// <summary>
            /// Gets or sets the campus short code of the person.
            /// </summary>
            public string CampusShortCode { get; set; }
        }

        /// <summary>
        /// Orders a queryable of <see cref="Rock.Model.Person" />
        /// by first name and nick name, based on the <paramref name="reversed"/> param.
        /// </summary>
        /// <param name="qry">The person query to order.</param>
        /// <param name="reversed">Whether the list is sorted by nick name or last name first.</param>
        /// <returns></returns>
        private IOrderedQueryable<PersonNameSearchBag> OrderByFullName( IQueryable<PersonNameSearchBag> qry, bool reversed )
        {
            if ( reversed )
            {
                return qry.OrderBy( p => p.LastName ).ThenBy( p => p.NickName );
            }
            else
            {
                return qry.OrderBy( p => p.NickName ).ThenBy( p => p.LastName );
            }
        }
    }
}
