// <copyright>
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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// A DataView Filter that selects people based on their personal devices.
    /// </summary>
    [Description( "Filter people based on their personal devices." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Has Personal Device" )]
    [Rock.SystemGuid.EntityTypeGuid( "C07DCF0F-2BBE-4B3E-AF33-0D06FF5D260C" )]
    public class PersonalDevicesFilter : DataFilterComponent
    {
        #region Settings

        /// <summary>
        /// Settings for the Data Filter component.
        /// </summary>
        public class FilterSettings
        {
            /// <summary>
            /// Gets or sets the values for the Site filter.
            /// </summary>
            public List<Guid> SiteGuids = new List<Guid>();

            /// <summary>
            /// Gets or sets the values for the Notifications Enabled filter.
            /// </summary>
            public bool? NotificationsEnabled = null;

            /// <summary>
            /// Gets or sets the values for the Device Platform filter.
            /// </summary>
            public List<Guid> DevicePlatformGuids = new List<Guid>();

            /// <summary>
            /// Gets or sets the values for the Device Type filter.
            /// </summary>
            public List<Guid> DeviceTypeGuids = new List<Guid>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Rock.Model.Person ).FullName; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override string GetTitle( Type entityType )
        {
            return "Personal Devices";
        }

        /// <inheritdoc/>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
    var result = """";

    var siteNames = $('.js-selector-sites', $content).find(':selected');
    if ( siteNames.length > 0 ) {
        var siteNamesDelimitedList = siteNames.map(function() {{ return $(this).text() }}).get().join(', ');
        result += "" with Site: "" + siteNamesDelimitedList + ""; "";
    }

    var notificationsEnabled = $('.js-selector-notifications-enabled', $content).find(':selected').text();
    if ( notificationsEnabled.length > 0 ) {
        result += "" with Notifications: "" + notificationsEnabled + ""; "";
    }

    var devicePlatforms = $('.js-selector-device-platforms', $content).find(':selected');
    if ( devicePlatforms.length > 0 ) {
        var devicePlatformsDelimitedList = devicePlatforms.map(function() {{ return $(this).text() }}).get().join(', ');
        result += "" with Platform: "" + devicePlatformsDelimitedList + ""; "";
    }

    var deviceTypes = $('.js-selector-device-types', $content).find(':selected');
    if ( deviceTypes.length > 0 ) {
        var deviceTypesDelimitedList = deviceTypes.map(function() {{ return $(this).text() }}).get().join(', ');
        result += "" with Type: "" + deviceTypesDelimitedList + ""; "";
    }

    if ( result.length == 0 ) {
        result = "" (any)"";
    }
    result = ""Personal Devices"" + result.replace(/([;\s])*$/, '');
    return result;
}
";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Personal Devices (any)";

            var settings = DataComponentSettingsHelper.DeserializeFilterSettings( selection, new FilterSettings() );

            // Sites
            var sites = new List<string>();
            if ( settings.SiteGuids != null )
            {
                sites = SiteCache.All()
                    .Where( s => settings.SiteGuids.Contains( s.Guid ) )
                    .Select( s => s.Name )
                    .ToList();
            }

            // Mobile Device Platform
            var devicePlatforms = new List<string>();
            if ( settings.DevicePlatformGuids != null )
            {
                devicePlatforms = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSONAL_DEVICE_PLATFORM.AsGuid() )
                    .DefinedValues
                    .Where( v => settings.DevicePlatformGuids.Contains( v.Guid ) )
                    .OrderBy( v => v.Order )
                    .ThenBy( v => v.Value )
                    .Select( v => v.Value )
                    .ToList();
            }

            // Mobile Device Type
            var deviceTypes = new List<string>();
            if ( settings.DeviceTypeGuids != null )
            {
                deviceTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSONAL_DEVICE_TYPE.AsGuid() )
                    .DefinedValues
                    .Where( v => settings.DeviceTypeGuids.Contains( v.Guid ) )
                    .OrderBy( v => v.Order )
                    .ThenBy( v => v.Value )
                    .Select( v => v.Value )
                    .ToList();
            }

            // Create a description of the settings.
            var settingsPhrases = new List<string>();

            if ( sites.Any() )
            {
                settingsPhrases.Add( "with Site: " + sites.AsDelimited( ", " ) );
            }

            if ( settings.NotificationsEnabled != null )
            {
                settingsPhrases.Add( "with Notifications: " + ( settings.NotificationsEnabled.Value ? "Yes" : "No" ) );
            }

            if ( devicePlatforms.Any() )
            {
                settingsPhrases.Add( "with Platform: " + devicePlatforms.AsDelimited( ", " ) );
            }

            if ( deviceTypes.Any() )
            {
                settingsPhrases.Add( "with Type: " + deviceTypes.AsDelimited( ", " ) );
            }

            if ( settingsPhrases.Any() )
            {
                result = "Personal Devices " + settingsPhrases.JoinStrings( "; " );
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            // Sites
            var rlbWebsites = new RockListBox();
            rlbWebsites.ID = filterControl.GetChildControlInstanceName( "rlbWebsites" );
            rlbWebsites.CssClass = "js-selector-sites";
            rlbWebsites.Label = "Sites";
            rlbWebsites.Help = "The sites for which the device is registered.";
            filterControl.Controls.Add( rlbWebsites );

            var sites = SiteCache.GetAllActiveSites().ToList();
            rlbWebsites.InitializeListItems( sites,
                k => k.Guid.ToString(),
                v => v.Name,
                allowEmptySelection: true );

            // Notifications Enabled
            var ddlNotifications = new RockDropDownList();
            ddlNotifications.ID = filterControl.GetChildControlInstanceName( "ddlNotifications" );
            ddlNotifications.CssClass = "js-selector-notifications-enabled";
            ddlNotifications.Label = "Notifications Enabled";
            ddlNotifications.Help = "Specifies if notifications are enabled for the device.";
            filterControl.Controls.Add( ddlNotifications );

            ddlNotifications.Items.Add( string.Empty );
            ddlNotifications.Items.Add( "Yes" );
            ddlNotifications.Items.Add( "No" );

            // Device Platform
            var ddlDevicePlatform = new RockListBox();
            ddlDevicePlatform.ID = filterControl.GetChildControlInstanceName( "ddlPlatform" );
            ddlDevicePlatform.CssClass = "js-selector-device-platforms";
            ddlDevicePlatform.Label = "Mobile Device Platforms";
            ddlDevicePlatform.Help = "The platform or operating system used by the device.";
            filterControl.Controls.Add( ddlDevicePlatform );

            var platforms = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSONAL_DEVICE_PLATFORM.AsGuid() )
                .DefinedValues
                .Where( v => v.IsActive )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Value );
            ddlDevicePlatform.InitializeListItems( platforms,
                k => k.Guid.ToString(),
                v => v.Value,
                allowEmptySelection: true );

            // Device Type
            var ddlDeviceType = new RockListBox();
            ddlDeviceType.ID = filterControl.GetChildControlInstanceName( "ddlDeviceType" );
            ddlDeviceType.CssClass = "js-selector-device-types";
            ddlDeviceType.Label = "Mobile Device Types";
            ddlDeviceType.Help = "The physical type of the device.";
            filterControl.Controls.Add( ddlDeviceType );

            var deviceTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSONAL_DEVICE_TYPE.AsGuid() )
                .DefinedValues
                .Where( v => v.IsActive )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Value );
            ddlDeviceType.InitializeListItems( deviceTypes,
                k => k.Guid.ToString(),
                v => v.Value,
                allowEmptySelection: true );

            return new Control[] { rlbWebsites, ddlNotifications, ddlDevicePlatform, ddlDeviceType };
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var rlbWebsites = ( controls[0] as RockListBox );
            var ddlNotificationsEnabled = ( controls[1] as RockDropDownList );
            var rlbDevicePlatform = ( controls[2] as RockListBox );
            var rlbDeviceType = ( controls[3] as RockListBox );

            var settings = new FilterSettings();

            settings.SiteGuids = rlbWebsites.SelectedValuesAsGuid;
            settings.NotificationsEnabled = ddlNotificationsEnabled.SelectedValue.AsBooleanOrNull();
            settings.DevicePlatformGuids = rlbDevicePlatform.SelectedValuesAsGuid;
            settings.DeviceTypeGuids = rlbDeviceType.SelectedValuesAsGuid;

            var json = DataComponentSettingsHelper.SerializeFilterSettings( settings );
            return json;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var rlbWebsites = ( controls[0] as RockListBox );
            var ddlNotificationsEnabled = ( controls[1] as RockDropDownList );
            var rlbDevicePlatform = ( controls[2] as RockListBox );
            var rlbDeviceType = ( controls[3] as RockListBox );

            var settings = DataComponentSettingsHelper.DeserializeFilterSettings( selection, new FilterSettings() );

            rlbWebsites.SetValues( settings.SiteGuids );

            var notificationsEnabled = string.Empty;
            if ( settings.NotificationsEnabled != null )
            {
                notificationsEnabled = settings.NotificationsEnabled.Value ? "Yes" : "No";
            }
            ddlNotificationsEnabled.SelectedValue = notificationsEnabled;

            rlbDevicePlatform.SetValues( settings.DevicePlatformGuids );
            rlbDeviceType.SetValues( settings.DeviceTypeGuids );
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var settings = DataComponentSettingsHelper.DeserializeFilterSettings( selection, new FilterSettings() );

            var dataContext = ( RockContext ) serviceInstance.Context;
            var deviceService = new PersonalDeviceService( dataContext );
            var deviceQuery = deviceService.Queryable();

            // Filter by Site.
            if ( settings.SiteGuids != null && settings.SiteGuids.Any() )
            {
                var siteIds = SiteCache.GetAllActiveSites()
                    .Where( s => settings.SiteGuids.Contains( s.Guid ) )
                    .Select( s => s.Id )
                    .ToList();
                deviceQuery = deviceQuery.Where( d => d.SiteId != null
                    && siteIds.Contains( d.SiteId.Value ) );
            }

            // Filter by Notifications Enabled.
            if ( settings.NotificationsEnabled != null )
            {
                deviceQuery = deviceQuery.Where( d => d.NotificationsEnabled == settings.NotificationsEnabled );
            }

            // Filter by Mobile Device Platform.
            if ( settings.DevicePlatformGuids != null && settings.DevicePlatformGuids.Any() )
            {
                var platformIds = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSONAL_DEVICE_PLATFORM )
                    .DefinedValues
                    .Where( v => settings.DevicePlatformGuids.Contains( v.Guid ) )
                    .Select( v => v.Id )
                    .ToList();
                deviceQuery = deviceQuery.Where( d => d.PlatformValueId != null
                    && platformIds.Contains( d.PlatformValueId.Value ) );
            }

            // Filter by Mobile Device Type.
            if ( settings.DeviceTypeGuids != null && settings.DeviceTypeGuids.Any() )
            {
                var typeIds = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSONAL_DEVICE_TYPE )
                    .DefinedValues
                    .Where( v => settings.DeviceTypeGuids.Contains( v.Guid ) )
                    .Select( v => v.Id )
                    .ToList();
                deviceQuery = deviceQuery.Where( d => d.PersonalDeviceTypeValueId != null
                    && typeIds.Contains( d.PersonalDeviceTypeValueId.Value ) );
            }

            // Create Person Query.
            var personService = new PersonService( ( RockContext ) serviceInstance.Context );
            var qry = personService.Queryable()
                        .Where( p => deviceQuery.Any( x => x.PersonAlias.PersonId == p.Id ) );

            var extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}