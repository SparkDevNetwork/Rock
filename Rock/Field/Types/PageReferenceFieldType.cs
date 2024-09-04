﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
#if WEBFORMS
using System.Web.UI;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Stored as "Page.Guid" or "Page.Guid,PageRoute.Guid"
    /// </summary>
    [Serializable]
    [FieldTypeUsage( FieldTypeUsage.System )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.PAGE_REFERENCE )]
    public class PageReferenceFieldType : FieldType, IEntityReferenceFieldType
    {

        #region Formatting

        /// <summary>
        /// Gets the text value.
        /// </summary>
        /// <param name="privateValue">The private value.</param>
        /// <param name="privateConfigurationValues">The private configuration values.</param>
        /// <returns>System.String.</returns>
        /// <inheritdoc />
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( privateValue ) )
            {
                //// Value is in format "Page.Guid,PageRoute.Guid"
                string[] valuePair = privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                if ( valuePair.Length > 0 )
                {
                    Guid? pageGuid = valuePair[0].AsGuidOrNull();
                    if ( pageGuid.HasValue )
                    {
                        var page = PageCache.Get( pageGuid.Value );
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

            return privateValue;
        }

        #endregion

        #region Edit Control

        /// <inheritdoc />
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc />
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var pageRouteBag = new PageRouteValueBag();
            string[] valuePair = ( privateValue ?? string.Empty ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

            //// Value is in format "Page.Guid,PageRoute.Guid"
            //// If only the Page.Guid is specified this is just a reference to a page without a special route
            //// In case the PageRoute record can't be found from PageRoute.Guid (maybe the pageroute was deleted), fall back to the Page without a PageRoute

            if ( valuePair.Length > 0 )
            {
                var pageGuid = valuePair[0].AsGuidOrNull();
                if ( pageGuid.HasValue )
                {
                    var page = PageCache.Get( pageGuid.Value );
                    if ( page != null )
                    {
                        pageRouteBag.Page = page.ToListItemBag();

                        if ( valuePair.Length > 1 )
                        {
                            var routeGuid = valuePair[1].AsGuidOrNull();
                            if ( routeGuid.HasValue )
                            {
                                var route = page.PageRoutes.Find( r => r.Guid.Equals( routeGuid.Value ) );
                                if ( route != null )
                                {
                                    pageRouteBag.Route = new ListItemBag()
                                    {
                                        Text = route.Route,
                                        Value = route.Guid.ToString()
                                    };
                                }
                            }
                        }
                    }
                }
            }

            return pageRouteBag.ToCamelCaseJson( false, true );
        }

        /// <inheritdoc />
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var jsonValue = publicValue.FromJsonOrNull<PageRouteValueBag>();

            if ( jsonValue != null )
            {
                if ( jsonValue.Route != null )
                {
                    return $"{jsonValue.Page.Value},{jsonValue.Route.Value}";
                }
                else
                {
                    return jsonValue.Page.Value;
                }
            }

            return base.GetPrivateEditValue( publicValue, privateConfigurationValues );
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            List<ReferencedEntity> referencedEntities = new List<ReferencedEntity>();

            if ( !string.IsNullOrWhiteSpace( privateValue ) )
            {
                //// Value is in format "Page.Guid,PageRoute.Guid"
                string[] valuePair = privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                if ( valuePair.Length > 0 )
                {
                    Guid? pageGuid = valuePair[0].AsGuidOrNull();
                    if ( pageGuid.HasValue )
                    {
                        var page = PageCache.Get( pageGuid.Value );
                        if ( page != null )
                        {
                            referencedEntities.Add( new ReferencedEntity( EntityTypeCache.GetId<Rock.Model.Page>().Value, page.Id ) );
                            if ( valuePair.Length > 1 )
                            {
                                Guid? routeGuid = valuePair[1].AsGuidOrNull();
                                if ( routeGuid.HasValue )
                                {
                                    var route = page.PageRoutes.FirstOrDefault( r => r.Guid.Equals( routeGuid.Value ) );

                                    if ( route != null )
                                    {
                                        referencedEntities.Add( new ReferencedEntity( EntityTypeCache.GetId<Rock.Model.PageRoute>().Value, route.Id ) );
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return referencedEntities;
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a Page.PageTitle and sometimes also PageRoute.Route and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Rock.Model.Page>().Value, nameof( Rock.Model.Page.PageTitle ) ),
                new ReferencedProperty( EntityTypeCache.GetId<PageRoute>().Value, nameof( PageRoute.Route ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns>System.String.</returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            // Note that the original FormatValue didn't call Base.FormatValue, so it wouldn't have done any condensing.
            return GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new PagePicker { ID = id };
        }

        /// <summary>
        /// Reads new values entered by the user for the field.  Returns with Page.Guid,PageRoute.Guid or just Page.Guid 
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            PagePicker ppPage = control as PagePicker;
            string result = string.Empty;

            if ( ppPage != null )
            {
                //// Value is in format "Page.Guid,PageRoute.Guid"
                //// If only a Page is specified, this is just a reference to a page without a special route

                using ( var rockContext = new RockContext() )
                {

                    if ( ppPage.IsPageRoute )
                    {
                        int? pageRouteId = ppPage.PageRouteId;
                        var pageRoute = new PageRouteService( rockContext ).GetNoTracking( pageRouteId ?? 0 );
                        if ( pageRoute != null )
                        {
                            result = string.Format( "{0},{1}", pageRoute.Page.Guid, pageRoute.Guid );
                        }
                    }
                    else
                    {
                        var page = new PageService( rockContext ).GetNoTracking( ppPage.PageId ?? 0 );
                        if ( page != null )
                        {
                            result = page.Guid.ToString();
                        }
                    }
                }

                return result;
            }

            return null;
        }

        /// <summary>
        /// Sets the value ( as either Page.Guid,PageRoute.Guid or just Page.Guid if not specific to a route )
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            PagePicker ppPage = control as PagePicker;
            if ( ppPage != null )
            {
                string[] valuePair = ( value ?? string.Empty ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

                Rock.Model.Page page = null;
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

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            // This field type does not support filtering
            return null;
        }

#endif
        #endregion

    }
}