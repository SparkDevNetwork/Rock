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

        #region Formatting

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
                //// Value is in format "Page.Guid,PageRoute.Guid"
                string[] valuePair = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                if ( valuePair.Length > 0 )
                {
                    Guid? pageGuid = valuePair[0].AsGuidOrNull();
                    if ( pageGuid.HasValue )
                    {
                        var page = Rock.Web.Cache.PageCache.Read( pageGuid.Value );
                        if ( page != null )
                        {
                            if ( valuePair.Length > 1 )
                            {
                                Guid? routeGuid = valuePair[1].AsGuidOrNull();
                                if ( routeGuid.HasValue )
                                {
                                    var route = page.PageRoutes.FirstOrDefault( r => r.Guid.Equals( routeGuid.Value ) );
                                    if ( route != null )
                                    {
                                        return string.Format( "{0} ({1})", page.PageTitle, route.Route );
                                    }
                                }
                            }

                            return page.PageTitle;
                        }
                    }
                }
            }

            return value;
        }

        #endregion

        #region Edit Control

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

        #endregion

        #region Filter Control

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            // This field type does not support filtering
            return null;
        }

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion

    }
}