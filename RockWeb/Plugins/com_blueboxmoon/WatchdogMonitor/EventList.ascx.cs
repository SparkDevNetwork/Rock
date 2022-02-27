using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using com.blueboxmoon.WatchdogMonitor.Checks;
using com.blueboxmoon.WatchdogMonitor.Model;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor
{
    [DisplayName( "Event List" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "View events that have occurred in the system." )]

    public partial class EventList : RockBlock
    {
        #region Base Method Overrides

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

            gEvents.DataKeyNames = new [] { "Id" };
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !IsPostBack )
            {
                using ( var rockContext = new RockContext() )
                {
                    var devices = new WatchdogDeviceService( rockContext ).Queryable().AsNoTracking()
                        .OrderBy( d => d.Name )
                        .Select( d => new
                        {
                            d.Id,
                            d.Name
                        } )
                        .ToList();
                    ddlDevice.Items.Add( new ListItem() );
                    foreach ( var device in devices )
                    {
                        ddlDevice.Items.Add( new ListItem( device.Name, device.Id.ToString() ) );
                    }

                    ddlState.Items.Add( new ListItem() );
                    ddlState.Items.Add( new ListItem( ServiceCheckState.Warning.ConvertToString(), ServiceCheckState.Warning.ConvertToInt().ToString() ) );
                    ddlState.Items.Add( new ListItem( ServiceCheckState.Error.ConvertToString(), ServiceCheckState.Error.ConvertToInt().ToString() ) );
                }

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
            lTitle.Text = "<i class='fa fa-calendar-times'></i> Event List";

            if ( !string.IsNullOrWhiteSpace( PageParameter( "DeviceId" ) ) )
            {
                var device = new WatchdogDeviceService( new RockContext() ).Get( PageParameter( "DeviceId" ).AsInteger() );

                if ( device != null )
                {
                    lTitle.Text = "<i class='fa fa-calendar-times'></i> " + device.Name + " Event List";
                }

                gfEvents_ClearFilterClick( this, null );
                gEvents.ColumnsOfType<RockBoundField>().Where( f => f.HeaderText == "Device" ).First().Visible = false;
                ddlDevice.Visible = false;
            }
            else
            {
                gEvents.ColumnsOfType<RockBoundField>().Where( f => f.HeaderText == "Device" ).First().Visible = true;
                ddlDevice.Visible = true;
            }

            BindEventsFilter();
            BindEventsGrid();
        }

        /// <summary>
        /// Binds the events filter.
        /// </summary>
        private void BindEventsFilter()
        {
            ddlDevice.SelectedValue = gfEvents.GetUserPreference( "Device" );
            ddlState.SelectedValue = gfEvents.GetUserPreference( "State" );
            drpDateRange.DelimitedValues = gfEvents.GetUserPreference( "DateRange" );
            tbLastMessage.Text = gfEvents.GetUserPreference( "LastMessage" );
        }

        /// <summary>
        /// Binds the recent events grid.
        /// </summary>
        private void BindEventsGrid()
        {
            var rockContext = new RockContext();
            var eventService = new WatchdogServiceCheckEventService( rockContext );
            var qry = new WatchdogServiceCheckEventService( new RockContext() ).Queryable().AsNoTracking();

            int? deviceId = PageParameter( "DeviceId" ).AsIntegerOrNull();
            if ( !deviceId.HasValue )
            {
                deviceId = gfEvents.GetUserPreference( "Device" ).AsIntegerOrNull();
            }
            if ( deviceId.HasValue )
            {
                qry = qry.Where( e => e.ServiceCheck.DeviceId == deviceId.Value );
            }

            var state = gfEvents.GetUserPreference( "State" ).ConvertToEnumOrNull<ServiceCheckState>();
            if ( state.HasValue )
            {
                qry = qry.Where( e => e.State == state.Value );
            }

            var filterDateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( gfEvents.GetUserPreference( "DateRange" ) );
            if ( filterDateRange.Start.HasValue )
            {
                qry = qry.Where( e => e.StartDateTime >= filterDateRange.Start );
            }
            if ( filterDateRange.End.HasValue )
            {
                var endDate = filterDateRange.End.Value.AddDays( 1 );
                qry = qry.Where( e => e.EndDateTime.HasValue && e.EndDateTime < endDate );
            }

            var lastMessage = gfEvents.GetUserPreference( "LastMessage" );
            if ( !string.IsNullOrWhiteSpace( lastMessage ) )
            {
                qry = qry.Where( e => e.LastSummary.Contains( lastMessage ) );
            }

            var events = qry
                .OrderBy( e => e.EndDateTime.HasValue )
                .ThenByDescending( e => e.EndDateTime )
                .ToList()
                .Where( e => e.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                .Select( e => new
                {
                    e.Id,
                    DeviceName = e.ServiceCheck.Device.Name,
                    ServiceCheckName = e.ServiceCheck.ServiceCheckType.Name,
                    State = e.State.ToString(),
                    HtmlState = !e.EndDateTime.HasValue ? GetLabelForState( e.State ) : e.State.ToString(),
                    e.StartDateTime,
                    e.EndDateTime,
                    e.LastSummary,
                    e.IsSilenced
                } )
                .ToList();

            gEvents.DataSource = events;
            gEvents.DataBind();
        }

        /// <summary>
        /// Gets the label HTML for the service check state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        protected string GetLabelForState( ServiceCheckState state )
        {
            switch ( state )
            {
                case ServiceCheckState.Error:
                    {
                        return "<span class='label label-danger'>Error</span>";
                    }

                case ServiceCheckState.Warning:
                    {
                        return "<span class='label label-warning'>Warning</span>";
                    }

                case ServiceCheckState.OK:
                    {
                        return "<span class='label label-success'>OK</span>";
                    }

                case ServiceCheckState.Unknown:
                    {
                        return "<span class='label label-default'>Unknown</span>";
                    }

                default:
                    throw new NotImplementedException();
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
        /// Handles the GridRebind event of the gEvents control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gEvents_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindEventsGrid();
        }

        /// <summary>
        /// Handles the ToggleSilenceClick event of the gEvents control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gEvents_ToggleSilenceClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var serviceCheckEvent = new WatchdogServiceCheckEventService( rockContext ).Get( e.RowKeyId );

                serviceCheckEvent.IsSilenced = !serviceCheckEvent.IsSilenced;

                rockContext.SaveChanges();
            }

            BindEventsGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gEvents control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gEvents_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var lbSilence = e.Row.ControlsOfTypeRecursive<LinkButton>()
                    .Where( c => c.CssClass.Contains( "js-toggle-silence" ) )
                    .Cast<LinkButton>()
                    .FirstOrDefault();

                DateTime? endDateTime = ( DateTime? ) e.Row.DataItem.GetPropertyValue( "EndDateTime" );
                bool isSilenced = ( bool ) e.Row.DataItem.GetPropertyValue( "IsSilenced" );

                if ( endDateTime.HasValue )
                {
                    lbSilence.Visible = false;
                }
                else
                {
                    lbSilence.Visible = true;
                    lbSilence.ToolTip = isSilenced ? "Enable notifications" : "Silence notifications";

                    if ( isSilenced )
                    {
                        lbSilence.RemoveCssClass( "btn-default" ).AddCssClass( "btn-warning" );
                    }
                    else
                    {
                        lbSilence.RemoveCssClass( "btn-warning" ).AddCssClass( "btn-default" );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfEvents control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfEvents_ApplyFilterClick( object sender, EventArgs e )
        {
            int? deviceId = ddlDevice.SelectedValueAsInt();
            gfEvents.SaveUserPreference( "Device", deviceId.HasValue ? deviceId.Value.ToString() : string.Empty );
            gfEvents.SaveUserPreference( "State", ddlState.SelectedValue );
            gfEvents.SaveUserPreference( "DateRange", drpDateRange.DelimitedValues );
            gfEvents.SaveUserPreference( "LastMessage", tbLastMessage.Text );

            BindEventsGrid();
        }

        /// <summary>
        /// Handles the DisplayFilterValue event of the gfEvents control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfEvents_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key == "Device" )
            {
                var device = new WatchdogDeviceService( new RockContext() ).Get( e.Value.AsInteger() );
                if ( device != null )
                {
                    e.Value = device.Name;
                }
            }
            else if ( e.Key == "State" )
            {
                var state = e.Value.ConvertToEnumOrNull<ServiceCheckState>();

                e.Value = state.HasValue ? state.Value.ConvertToString() : string.Empty;
            }
            else if ( e.Key == "StartDate" )
            {
                e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
            }
            else if ( e.Key == "EndDate" )
            {
                e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
            }
            else if ( e.Key == "LastMessage" )
            {
                /* Intentionally left blank. */
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfEvents control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfEvents_ClearFilterClick( object sender, EventArgs e )
        {
            gfEvents.DeleteUserPreferences();
            BindEventsFilter();
        }

        #endregion
    }
}