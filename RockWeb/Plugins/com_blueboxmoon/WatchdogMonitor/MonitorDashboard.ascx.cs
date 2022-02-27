using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using com.blueboxmoon.WatchdogMonitor.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor
{
    [DisplayName( "Monitor Dashboard" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "Presents device status in a dashboard." )]

    [CustomCheckboxListField( "Device Groups", "The device groups to be made available to the lava template.", @"SELECT [Guid] AS [Value], [Name] AS [Text] FROM [_com_blueboxmoon_WatchdogMonitor_WatchdogDeviceGroup] ORDER BY [Name]", false, "", order: 0 )]
    [LinkedPage( "Device Page", "The page that allows the user to view the details of a device.", false, "", "", 1 )]
    [CodeEditorField( "Lava Template", "The lava template to use when rendering the dashboard.", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, true, @"{% include '~/Plugins/com_blueboxmoon/WatchdogMonitor/Assets/DevicesAsGrid.lava' %}

<p class=""text-right small margin-t-lg"">
    Last Updated {{ 'Now' | DateAdd:0 }}
</p>
", order: 2 )]
    [IntegerField( "Refresh Period", "The number of seconds to auto-refresh the dashboard contents.", true, 10, order: 3 )]
    [BooleanField( "Enforce Security", "If true, then items the current user does not have access to will not be shown.", false, order: 4 )]
    public partial class MonitorDashboard : RockBlock
    {
        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( "~/Plugins/com_blueboxmoon/WatchdogMonitor/Styles/style.css" );
            RockPage.AddScriptLink( "~/Scripts/moment.min.js" );
            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.min.js" );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/WatchdogMonitor/Scripts/hammer.min.js" );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/WatchdogMonitor/Scripts/chartjs-plugin-zoom.min.js" );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/WatchdogMonitor/Scripts/Graphs.js" );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !IsPostBack )
            {
                ShowContent();
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Shows the content.
        /// </summary>
        private void ShowContent()
        {
            var groupGuids = GetAttributeValue( "DeviceGroups" ).SplitDelimitedValues().AsGuidList();
            var rockContext = new RockContext();
            var deviceGroupService = new WatchdogDeviceGroupService( rockContext );
            List<WatchdogDevice> devices;

            var deviceGroups = deviceGroupService.Queryable().AsNoTracking().ToList();

            if ( groupGuids.Any() )
            {
                deviceGroups = deviceGroups.Where( g => !groupGuids.Any() || groupGuids.Contains( g.Guid ) ).ToList();
                devices = deviceGroups.SelectMany( g => g.Devices ).DistinctBy( d => d.Id ).ToList();
            }
            else
            {
                devices = new WatchdogDeviceService( rockContext ).Queryable().AsNoTracking().ToList();
            }

            //
            // If they want to enforce security, filter out anything they don't have access to.
            //
            if ( GetAttributeValue( "EnforceSecurity" ).AsBoolean() )
            {
                foreach ( var deviceGroup in deviceGroups )
                {
                    deviceGroup.Devices = deviceGroup.Devices
                        .Where( d => d.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        .ToList();
                }

                devices = devices.Where( d => d.IsAuthorized( Authorization.VIEW, CurrentPerson ) ).ToList();
            }

            var lavaTemplate = GetAttributeValue( "LavaTemplate" );

            var linkedPages = new Dictionary<string, object>();
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "DevicePage" ) ) )
            {
                linkedPages.Add( "DevicePage", LinkedPageRoute( "DevicePage" ) );
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
            mergeFields.AddOrReplace( "Groups", deviceGroups );
            mergeFields.AddOrReplace( "Devices", devices );
            mergeFields.AddOrReplace( "LinkedPages", linkedPages );

            lContent.Text = lavaTemplate.ResolveMergeFields( mergeFields );
            hfRefreshPeriod.Value = GetAttributeValue( "RefreshPeriod" );
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowContent();
        }

        /// <summary>
        /// Handles the Click event of the lbRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            ShowContent();
        }

        #endregion
    }
}