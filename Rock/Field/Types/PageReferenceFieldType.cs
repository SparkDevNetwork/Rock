//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Rock.Constants;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class PageReferenceFieldType : FieldType
    {
        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var service = new PageService();
                var page = service.Get( new Guid( value ) );
                if ( page != null )
                {
                    return page.Name;
                }
            }

            return string.Empty;
        }
        
        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new PagePicker { ID = id }; 
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            PagePicker ppPage = control as PagePicker;
            string result = null;

            if ( ppPage != null )
            {
                //// Value is in format "Page.Guid,PageRoute.Guid"
                //// If only a Page is specified, this is just a reference to a page without a special route

                if ( ppPage.IsPageRoute )
                {
                    int? pageRouteId = ppPage.PageRouteId;
                    var pageRoute = new PageRouteService().Get( ppPage.PageRouteId ?? 0);
                    if ( pageRoute != null )
                    {
                        result = string.Format( "{0},{1}", pageRoute.Page.Guid, pageRoute.Guid );
                    }
                }
                else
                {
                    var page = new PageService().Get( ppPage.PageId ?? 0 );
                    if ( page != null )
                    {
                        result = page.Guid.ToString();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
	            PagePicker ppPage = control as PagePicker;
    	        if ( ppPage != null )
        	    {
                    string[] valuePair = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

                    Page page = null;
                    PageRoute pageRoute = null;

                    //// Value is in format "Page.Guid,PageRoute.Guid"
                    //// If only the Page.Guid is specified this is just a reference to a page without a special route
                    //// In case the PageRoute record can't be found from PageRoute.Guid (maybe the pageroute was deleted), fall back to the Page without a PageRoute

                    if ( valuePair.Length == 2 )
                    {
                        Guid pageRouteGuid;
                        Guid.TryParse( valuePair[1], out pageRouteGuid );
                        pageRoute = new PageRouteService().Get( pageRouteGuid );
                    }

                    if ( pageRoute != null )
                    {
                        ppPage.SetValue( pageRoute );
                    }
                    else
                    {
                        if ( valuePair.Length > 0 )
                        {
                            Guid pageGuid;
                            Guid.TryParse( valuePair[0], out pageGuid );
                            page = new PageService().Get( pageGuid );
                        }

                        ppPage.SetValue( page );
                    }
        	    }
        	}
        }

    }
}