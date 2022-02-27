using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

using com.blueboxmoon.WatchdogMonitor;
using com.blueboxmoon.WatchdogMonitor.Model;

using Microsoft.AspNet.SignalR;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor
{
    [DisplayName( "Service Check Test" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "Performs a manual run of a service check type with the supplied configuration options." )]

    public partial class ServiceCheckTest : RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the service check type entity type identifier.
        /// </summary>
        /// <value>
        /// The service check type entity type identifier.
        /// </value>
        protected int? ServiceCheckTypeEntityTypeId
        {
            get
            {
                return ViewState["ServiceCheckTypeEntityTypeId"] as int?;
            }
            set
            {
                ViewState["ServiceCheckTypeEntityTypeId"] = value;
            }
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            if ( ( ServiceCheckTypeEntityTypeId ?? 0 ) != 0 )
            {
                var serviceCheckType = new WatchdogServiceCheckType
                {
                    EntityTypeId = ServiceCheckTypeEntityTypeId.Value
                };

                BuildDynamicControls( serviceCheckType, false );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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
                ShowDetails();
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            BindDevices();
            BindProviders();
            dvpCollector.BindToDefinedType( DefinedTypeCache.Get( com.blueboxmoon.WatchdogMonitor.SystemGuid.DefinedType.WATCHDOG_MONITOR_COLLECTORS.AsGuid() ) );
        }

        /// <summary>
        /// Binds the devices.
        /// </summary>
        private void BindDevices()
        {
            var devices = new WatchdogDeviceService( new RockContext() ).Queryable()
                .OrderBy( t => t.Name )
                .ToList();

            ddlDevice.Items.Clear();
            ddlDevice.Items.Add( new ListItem() );
            foreach ( var type in devices )
            {
                ddlDevice.Items.Add( new ListItem( type.Name, type.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Binds the providers.
        /// </summary>
        private void BindProviders()
        {
            var providers = ServiceCheckContainer.Instance.Components.Values
                .Select( t => t.Value )
                .OrderBy( t => t.Title )
                .ToList();

            ddlProvider.Items.Clear();
            ddlProvider.Items.Add( new ListItem() );
            foreach ( var type in providers )
            {
                ddlProvider.Items.Add( new ListItem( type.Title, type.TypeId.ToString() ) );
            }
        }

        /// <summary>
        /// Build the dynamic edit controls for this service check type.
        /// </summary>
        /// <param name="serviceCheckType">The service check type whose controls need to be built.</param>
        /// <param name="setValues">Whether or not to set the initial values.</param>
        protected void BuildDynamicControls( WatchdogServiceCheckType serviceCheckType, bool setValues )
        {
            serviceCheckType.LoadAttributes();
            phAttributes.Controls.Clear();

            Helper.AddEditControls( serviceCheckType, phAttributes, setValues, lbRun.ValidationGroup, new List<string> { "Active", "Order" }, false );

            foreach ( var tb in phAttributes.ControlsOfTypeRecursive<TextBox>() )
            {
                tb.AutoCompleteType = AutoCompleteType.Disabled;
                tb.Attributes["autocomplete"] = "off";
            }
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
            ShowDetails();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlProvider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlProvider_SelectedIndexChanged( object sender, EventArgs e )
        {
            ServiceCheckTypeEntityTypeId = ddlProvider.SelectedValueAsId();

            var serviceCheckType = new WatchdogServiceCheckType
            {
                EntityTypeId = ddlProvider.SelectedValueAsId() ?? 0
            };

            BuildDynamicControls( serviceCheckType, true );
        }

        /// <summary>
        /// Handles the Click event of the lbRun control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRun_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var entityType = new EntityTypeService( rockContext ).Get( ServiceCheckTypeEntityTypeId.Value );
            var device = new WatchdogDeviceService( rockContext ).Get( ddlDevice.SelectedValueAsId().Value );

            //
            // Setup the service check type.
            //
            var serviceCheckType = new WatchdogServiceCheckType
            {
                EntityType = entityType,
                EntityTypeId = entityType.Id
            };
            serviceCheckType.LoadAttributes( rockContext );
            Helper.GetEditValues( phAttributes, serviceCheckType );

            //
            // Setup the service check.
            //
            var serviceCheck = new WatchdogServiceCheck
            {
                Device = device,
                DeviceId = device.Id,
                ServiceCheckType = serviceCheckType,
                CollectorId = dvpCollector.SelectedDefinedValueId
            };

            //
            // Send the request to the hub.
            //
            var watchdogHub = GlobalHost.ConnectionManager.GetHubContext<WatchdogMessageHub>();
            var response = AsyncHelper.RunSync( () => WatchdogMessageHub.RunCheckNow( watchdogHub, serviceCheck ) );

            nbResultError.Text = response.Item2 ?? string.Empty;

            if ( response.Item1 != null )
            {
                ltResultStatus.Text = response.Item1.State.ToString();
                ltResultValue.Text = response.Item1.Value.ToString();
                ltResultSummary.Text = response.Item1.Summary;

                pnlResult.Visible = true;
            }
            else
            {
                pnlResult.Visible = false;
            }
        }

        #endregion

        #region Support Classes

        internal static class AsyncHelper
        {
            private static readonly TaskFactory _myTaskFactory = new TaskFactory(
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default );

            public static TResult RunSync<TResult>( Func<Task<TResult>> func )
            {
                return _myTaskFactory.StartNew( func ).Unwrap().GetAwaiter().GetResult();
            }

            public static void RunSync( Func<Task> func )
            {
                _myTaskFactory.StartNew( func ).Unwrap().GetAwaiter().GetResult();
            }
        }

        #endregion
    }
}