//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

using Rock;
using Rock.Crm;
using Rock.Web.UI;

namespace Rock.Search.Person
{
    /// <summary>
    /// Searches for people with matching address
    /// </summary>
    [Description( "Person Address Search" )]
    [Export(typeof(SearchComponent))]
    [ExportMetadata("ComponentName", "Person Address")]
    [BlockProperty( 1, "Search Label", "Behavior", "The text to display in the search type dropdown", false, "Address" )]
    [BlockProperty( 2, "Result URL", "Behavior", "The url to redirect user to after they have entered search text.  (use '{0}' for the search text)", true, "" )]
    public class Address : SearchComponent
    {
        /// <summary>
        /// The text to display as the search type
        /// </summary>
        public override string SearchLabel
        {
            get
            {
                if ( !String.IsNullOrWhiteSpace( AttributeValue( "SearchLabel" ) ) )
                    return AttributeValue( "SearchLabel" );
                else
                    return "Address";
            }
        }

        /// <summary>
        /// The URL to redirect user to for search
        /// </summary>
        public override string ResultUrl
        {
            get
            {
                if ( !String.IsNullOrWhiteSpace( AttributeValue( "ResultURL" ) ) )
                    return AttributeValue( "ResultURL" );
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Returns a list of matching people
        /// </summary>
        /// <param name="searchterm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search( string searchterm )
        {
            var service = new LocationService();

            return service.Queryable().
                Where( a => a.Street1.Contains( searchterm ) ).
                OrderBy( a => a.Street1 ).
                Select( a => a.Street1 + " " + a.City ).Distinct();
        }
    }
}