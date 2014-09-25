// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Rock.Constants;
using Rock.Data;
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
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var service = new PageService( new RockContext() );
                var page = service.Get( new Guid( value ) );
                if ( page != null )
                {
                    formattedValue = page.InternalName;
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new PagePicker { ID = id }; 
        }

        /// <summary>
        /// Reads new values entered by the user for the field.  Returns with Page.Guid,PageRoute.Guid or just Page.Guid 
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
                    var pageRoute = new PageRouteService( new RockContext() ).Get( pageRouteId ?? 0 );
                    if ( pageRoute != null )
                    {
                        result = string.Format( "{0},{1}", pageRoute.Page.Guid, pageRoute.Guid );
                    }
                }
                else
                {
                    var page = new PageService( new RockContext() ).Get( ppPage.PageId ?? 0 );
                    if ( page != null )
                    {
                        result = page.Guid.ToString();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Sets the value ( as either Page.Guid,PageRoute.Guid or just Page.Guid if not specific to a route )
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

                    var rockContext = new RockContext();

                    if ( valuePair.Length == 2 )
                    {
                        Guid pageRouteGuid;
                        Guid.TryParse( valuePair[1], out pageRouteGuid );
                        pageRoute = new PageRouteService( rockContext ).Get( pageRouteGuid );
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
                            page = new PageService( rockContext ).Get( pageGuid );
                        }

                        ppPage.SetValue( page );
                    }
        	    }
        	}
        }

    }
}