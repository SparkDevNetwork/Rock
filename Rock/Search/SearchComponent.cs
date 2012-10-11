//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Extension;

namespace Rock.Search
    
    /// <summary>
    /// 
    /// </summary>
    public abstract class SearchComponent : Component
        
        /// <summary>
        /// The label to display for the type of search
        /// </summary>
        public abstract string SearchLabel      get; }

        /// <summary>
        /// The url to redirect user to after they've entered search criteria
        /// </summary>
        public abstract string ResultUrl      get; }

        /// <summary>
        /// Returns a list of value/label results matching the searchterm
        /// </summary>
        /// <param name="searchterm">The searchterm.</param>
        /// <returns></returns>
        public abstract IQueryable<string> Search( string searchterm );
    }
}