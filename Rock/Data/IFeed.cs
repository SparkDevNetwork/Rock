//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.Data
    
    /// <summary>
    /// Represents a model that supports generating a feed
    /// </summary>
    interface IFeed
        
        /// <summary>
        /// Returns the feed.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="count">The count.</param>
        /// <param name="format">The format.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns></returns>
        string ReturnFeed( int key, int count, string format, out string errorMessage, out string contentType );
    }
}
