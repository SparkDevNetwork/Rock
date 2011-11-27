using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Routing;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;


namespace Rock.Helpers
{
    /// <summary>
    /// Helper class to work with the PageReference field type
    /// </summary>
    public class PageReference : System.Web.UI.UserControl
    {
        // private members
        private int _pageId = -1;
        private int _routeId = -1;
        
        // public properties

        /// <summary>
        /// Gets the page id.
        /// </summary>
        public int PageId 
        {
            get
            {
                return _pageId;
            }            
        }

        /// <summary>
        /// Gets the route id.
        /// </summary>
        public int RouteId
        {
            get
            {
                return _routeId;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                if ( _pageId != -1 )
                    return true;
                else
                    return false;
            }
        }

        // constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference"/> class.
        /// </summary>
        public PageReference(){}

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference"/> class.
        /// </summary>
        /// <param name="reference">The reference.</param>
        public PageReference( string reference )
        {
            string[] items = reference.Split( ',' );

            if ( items.Length == 2 )
            {
                Int32.TryParse( items[0], out _pageId );
                Int32.TryParse( items[1], out _routeId );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference"/> class.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        /// <param name="routeId">The route id.</param>
        public PageReference( int pageId, int routeId )
        {
            _pageId = pageId;
            _routeId = routeId;
        }
    }
}