//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Rock.Attribute;
using Rock.Model;

namespace Rock.Search.Person
{
    /// <summary>
    /// Searches for people with matching names
    /// </summary>
    [Description("Person Name Search")]
    [Export(typeof(SearchComponent))]
    [ExportMetadata("ComponentName", "Person Name")]
    [TextField( "Search Label", "The text to display in the search type dropdown", false, "Name" )]
    [TextField( "Result URL", "The url to redirect user to after they have entered search text.  (use '{0}' for the search text)" )]
    public class Name : SearchComponent
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
            return new PersonService().Queryable().SelectFullNames( searchterm, true );
        }
    }
}