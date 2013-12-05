//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Attribute;
using Rock.Extension;

namespace Rock.Search
{
    /// <summary>
    /// 
    /// </summary>
    [TextField( "Search Label", "The text to display in the search type dropdown", false, "Search" )]
    [TextField( "Result URL", "The url to redirect user to after they have entered search text.  (use '{0}' for the search text)" )]
    public abstract class SearchComponent : Component
    {
        /// <summary>
        /// The label to display for the type of search
        /// </summary>
        public virtual string SearchLabel
        {
            get
            {

                if ( !String.IsNullOrWhiteSpace( GetAttributeValue( "SearchLabel" ) ) )
                {
                    return GetAttributeValue( "SearchLabel" );
                }
                else
                {
                    return "Search";
                }
            }
        }

        /// <summary>
        /// The url to redirect user to after they've entered search criteria
        /// </summary>
        public virtual string ResultUrl
        {
            get
            {
                if ( !String.IsNullOrWhiteSpace( GetAttributeValue( "ResultURL" ) ) )
                {
                    return GetAttributeValue( "ResultURL" );
                }
                else
                {
                    return string.Empty;
                }
            }
        }


        /// <summary>
        /// Returns a list of value/label results matching the searchterm
        /// </summary>
        /// <param name="searchterm">The searchterm.</param>
        /// <returns></returns>
        public abstract IQueryable<string> Search( string searchterm );
    }
}