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
using Rock.CRM;

namespace Rock.Search.Person
{
    [Description("Person Email Search")]
    [Export(typeof(SearchComponent))]
    [ExportMetadata("ComponentName", "Person Email")]
    [Rock.Attribute.Property( 1, "Search Label", "Behavior", "The text to display in the search type dropdown", false, "Email" )]
    [Rock.Attribute.Property( 2, "Result URL", "Behavior", "The url to redirect user to after they have entered search text.  (use '{0}' for the search text)", true, "" )]
    public class Email : SearchComponent
    {
        public override string SearchLabel
        {
            get
            {
                if ( !String.IsNullOrWhiteSpace( AttributeValue( "SearchLabel" ) ) )
                    return AttributeValue( "SearchLabel" );
                else
                    return "Email";
            }
        }

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

        public override IQueryable<string> Search( string searchterm )
        {
            return new List<string>().AsQueryable();
        }
    }
}