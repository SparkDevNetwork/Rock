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
using Rock.Model;
using Rock.Web.UI;

namespace Rock.Search.Person
{
    /// <summary>
    /// Searches for people with matching names
    /// </summary>
    [Description("Person Name Search")]
    [Export(typeof(SearchComponent))]
    [ExportMetadata("ComponentName", "Person Name")]
    [BlockProperty( 1, "Search Label", "Behavior", "The text to display in the search type dropdown", false, "Name" )]
    [BlockProperty( 2, "Result URL", "Behavior", "The url to redirect user to after they have entered search text.  (use '{0}' for the search text)", true, "" )]
    public class Name : SearchComponent
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
                    return "Name";
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
            string fName = string.Empty;
            string lName = string.Empty;

            var names = searchterm.SplitDelimitedValues();

            bool lastFirst = searchterm.Contains(",");

            if (lastFirst)
            {
                // last, first
                lName = names.Length >= 1 ? names[0] : string.Empty;
                fName = names.Length >= 2 ? names[1] : string.Empty;
            }
            else
            {
                // first last
                fName = names.Length >= 1 ? names[0] : string.Empty;
                lName = names.Length >= 2 ? names[1] : string.Empty;
            }

            var personService = new PersonService();

            var people = personService.Queryable().
                Where( p => ( ( p.NickName ?? p.GivenName ).StartsWith( fName ) && p.LastName.StartsWith( lName ) ) );

            if ( lastFirst )
                people = people.OrderBy( p => p.LastName ).ThenBy( p => p.NickName ?? p.GivenName );
            else
                people = people.OrderBy( p => p.NickName ?? p.GivenName ).ThenBy( p => p.LastName );

            return people.
                Select( p => ( lastFirst ? p.LastName + ", " + ( p.NickName ?? p.GivenName ) : ( p.NickName ?? p.GivenName ) + " " + p.LastName ) ).
                Distinct();
        }
    }
}