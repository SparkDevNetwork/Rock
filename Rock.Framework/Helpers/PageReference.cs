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
        public int PageId 
        {
            get
            {
                return _pageId;
            }            
        }

        public int RouteId
        {
            get
            {
                return _routeId;
            }
        }

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
        public PageReference(){}

        public PageReference( string reference )
        {
            string[] items = reference.Split( ',' );

            if ( items.Length == 2 )
            {
                Int32.TryParse( items[0], out _pageId );
                Int32.TryParse( items[1], out _routeId );
            }
        }

        public PageReference( int pageId, int routeId )
        {
            _pageId = pageId;
            _routeId = routeId;
        }
    }
}