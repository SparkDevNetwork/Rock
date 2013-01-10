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
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.Search.Person
{
    /// <summary>
    /// Searches for people with matching phones
    /// </summary>
    [Description( "Person Phone Search" )]
    [Export(typeof(SearchComponent))]
    [ExportMetadata("ComponentName", "Person Phone")]
    [TextField( 1, "Search Label", "Behavior", "The text to display in the search type dropdown", false, "Phone" )]
    [TextField( 2, "Result URL", "Behavior", "The url to redirect user to after they have entered search text.  (use '{0}' for the search text)", true, "" )]
    public class Phone : SearchComponent
    {
        /// <summary>
        /// The text to display as the search type
        /// </summary>
        public override string SearchLabel
        {
            get
            {
                if ( !String.IsNullOrWhiteSpace( GetAttributeValue( "SearchLabel" ) ) )
                    return GetAttributeValue( "SearchLabel" );
                else
                    return "Phone";
            }
        }

        /// <summary>
        /// The URL to redirect user to for search
        /// </summary>
        public override string ResultUrl
        {
            get
            {
                if ( !String.IsNullOrWhiteSpace( GetAttributeValue( "ResultURL" ) ) )
                    return GetAttributeValue( "ResultURL" );
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
            var phoneNumberService = new PhoneNumberService();

            return phoneNumberService.Queryable().
                Where( n => n.Number.Contains( searchterm ) ).
                OrderBy( n => n.Number ).
                Select( n => n.Number ).Distinct();
        }
    }
}