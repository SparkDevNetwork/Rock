//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.SystemGuid
    
    /// <summary>
    /// System Pages.  NOTE: Some of these are referenced in Migrations to avoid string-typos.
    /// </summary>
    public static class Page
        
        /// <summary>
        /// Gets the Plugin Settings guid
        /// </summary>
        public static Guid PLUGIN_SETTINGS      get      return new Guid( "1AFDA740-8119-45B8-AF4D-58856D469BE5" ); } }

        /// <summary>
        /// Gets the Plugin Manager guid
        /// </summary>
        public static Guid PLUGIN_MANAGER      get      return new Guid( "B13FCF9A-FAE5-4E53-AF7C-32DF9CA5AAE3" ); } }
    }
}