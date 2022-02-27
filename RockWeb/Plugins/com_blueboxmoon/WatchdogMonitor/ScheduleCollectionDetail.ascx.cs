using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Web.UI;

using com.blueboxmoon.WatchdogMonitor.Cache;
using com.blueboxmoon.WatchdogMonitor.Model;
using Rock.Security;

namespace RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor
{
    [DisplayName( "Schedule Collection Detail" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "View and edit the details of a monitoring schedule." )]

    public partial class ScheduleCollectionDetail : RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the state of the schedule detail.
        /// </summary>
        /// <value>
        /// The state of the schedule detail.
        /// </value>
        public List<string> ScheduleDetailState
        {
            get
            {
                return ( List<string> ) ViewState["ScheduleDetailState"];
            }
            set
            {
                ViewState["ScheduleDetailState"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the schedule builder should be visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the schedule builder should be visible; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSB
        {
            get
            {
                return ( ( bool? ) ViewState["ShowSB"] ) ?? false;
            }
            set
            {
                ViewState["ShowSB"] = value;
                if ( value && _sbContents == null )
                {
                    _sbContents = new Rock.Web.UI.Controls.ScheduleBuilderPopupContents
                    {
                        ID = "sbContents_422AFB41_3261_44D6_AD27_D0ECA0B99DDB",
                        ClientIDMode = ClientIDMode.Static,
                        ValidationGroup = mdlComponentEdit.ValidationGroup
                    };
                    phContents.Controls.Add( _sbContents );
                }
                else if ( !value && _sbContents != null )
                {
                    phContents.Controls.Remove( _sbContents );
                    _sbContents = null;
                }
            }
        }

        private Rock.Web.UI.Controls.ScheduleBuilderPopupContents _sbContents;

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            if ( ShowSB )
            {
                _sbContents = new Rock.Web.UI.Controls.ScheduleBuilderPopupContents
                {
                    ID = "sbContents_422AFB41_3261_44D6_AD27_D0ECA0B99DDB",
                    ClientIDMode = ClientIDMode.Static,
                    ValidationGroup = mdlComponentEdit.ValidationGroup
                };
                phContents.Controls.Add( _sbContents );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gComponents.Actions.ShowAdd = true;
            gComponents.Actions.AddClick += gComponents_AddClick;
            mdlComponentEdit.OnCancelScript = string.Format( "document.getElementById('{0}').click(); return false;", lbEditCancel.ClientID );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                int scheduleId = PageParameter( "ScheduleId" ).AsInteger();

                if ( scheduleId != 0 )
                {
                    ShowDetails( scheduleId );
                }
                else
                {
                    ShowEdit( 0 );
                }

                RegisterJavaScript();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        /// <param name="scheduleId">The schedule identifier.</param>
        protected void ShowDetails( int scheduleId )
        {
            var schedule = new WatchdogScheduleCollectionService( new RockContext() ).Get( scheduleId );

            if ( schedule == null )
            {
                schedule = new WatchdogScheduleCollection();
            }

            if ( !schedule.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                nbWarningMessage.Text = Rock.Constants.EditModeMessage.NotAuthorizedToView( schedule.GetType().GetFriendlyTypeName() );
                pnlDetails.Visible = false;
                pnlEdit.Visible = false;

                return;
            }

            nbWarningMessage.Text = string.Empty;

            lName.Text = schedule.Name;
            lDescription.Text = schedule.Description;

            List<string> schedules = new List<string>();
            try
            {
                schedules = schedule.ScheduleJson.FromJsonOrNull<List<string>>();
            }
            catch
            {
                schedules = null;
            }
            if ( schedules == null )
            {
                schedules = new List<string>();
            }
            lSchedules.Text = string.Join( "<br>", schedules.Select( s => FriendlyScheduleText( s, false ) ) );

            lbEdit.Visible = schedule.IsAuthorized( Authorization.EDIT, CurrentPerson );

            pnlDetails.Visible = true;
            pnlEdit.Visible = false;
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="scheduleId">The schedule identifier.</param>
        protected void ShowEdit( int scheduleId )
        {
            var schedule = new WatchdogScheduleCollectionService( new RockContext() ).Get( scheduleId );

            if ( schedule == null )
            {
                schedule = new WatchdogScheduleCollection();
            }

            if ( !schedule.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                nbWarningMessage.Text = Rock.Constants.EditModeMessage.NotAuthorizedToEdit( schedule.GetType().GetFriendlyTypeName() );
                pnlDetails.Visible = false;
                pnlEdit.Visible = false;

                return;
            }

            nbWarningMessage.Text = string.Empty;

            lEditTitle.Text = schedule.Id == 0 ? "Add Schedule" : "Edit Schedule";

            tbEditName.Text = schedule.Name;
            tbEditDescription.Text = schedule.Description;

            try
            {
                ScheduleDetailState = schedule.ScheduleJson.FromJsonOrNull<List<string>>();
            }
            catch
            {
                ScheduleDetailState = null;
            }
            if ( ScheduleDetailState == null )
            {
                ScheduleDetailState = new List<string>();
            }

            BindGrid();

            pnlDetails.Visible = false;
            pnlEdit.Visible = true;
        }

        /// <summary>
        /// Friendlies the schedule text.
        /// </summary>
        /// <param name="calendar">The calendar.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        protected string FriendlyScheduleText( string calendar, bool condensed = true )
        {
            return new Rock.Model.Schedule { iCalendarContent = calendar }.ToFriendlyScheduleText( condensed );
        }

        /// <summary>
        /// Determines whether [is valid schedule] [the specified schedule].
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <returns>
        ///   <c>true</c> if [is valid schedule] [the specified schedule]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsValidSchedule( string schedule )
        {
            DDay.iCal.Event calEvent = Rock.Model.ScheduleICalHelper.GetCalenderEvent( schedule );
            if ( calEvent == null || calEvent.DTStart == null )
            {
                return false;
            }

            var endDT = calEvent.DTStart.Add( calEvent.Duration );
            if (endDT.Date.Add(endDT.TimeOfDay) > calEvent.DTStart.Date.Add(calEvent.DTStart.TimeOfDay).AddDays(1))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            EnsureChildControls();
            var script = string.Format( @"Rock.controls.scheduleBuilder.initialize({{ id: '{0}' }});", "sbContents_422AFB41_3261_44D6_AD27_D0ECA0B99DDB" );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "schedule_builder-init_" + "sbContents_422AFB41_3261_44D6_AD27_D0ECA0B99DDB", script, true );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            var components = ScheduleDetailState
                .Select( s => new
                {
                    ScheduleText = FriendlyScheduleText( s )
                } ).ToList();

            gComponents.DataSource = components;
            gComponents.DataBind();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the AddClick event of the gComponents control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gComponents_AddClick( object sender, EventArgs e )
        {
            hfRowId.Value = string.Empty;
            ShowSB = true;

            nbComponentEditError.Text = string.Empty;
            _sbContents.iCalendarContent = string.Empty;

            mdlComponentEdit.Show();
        }

        /// <summary>
        /// Handles the RowSelected event of the gComponents control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gComponents_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            hfRowId.Value = e.RowIndex.ToString();
            ShowSB = true;

            nbComponentEditError.Text = string.Empty;
            _sbContents.iCalendarContent = ScheduleDetailState[e.RowIndex];

            mdlComponentEdit.Show();
        }

        /// <summary>
        /// Handles the GridRebind event of the gComponents control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gComponents_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the gComponentsDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gComponentsDelete_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            ScheduleDetailState.RemoveAt( e.RowIndex );

            BindGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdlEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void mdlEdit_SaveClick( object sender, EventArgs e )
        {
            var schedules = ScheduleDetailState;

            var method = _sbContents.GetType().GetMethod( "GetCalendarContentFromControls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
            var scheduleText = ( string ) method.Invoke( _sbContents, new object[] { } );

            nbComponentEditError.Text = string.Empty;
            if ( !IsValidSchedule( scheduleText ) )
            {
                nbComponentEditError.Text = "Invalid schedule. Schedule must have start date and not go past a midnight boundary.";
                return;
            }

            if ( hfRowId.Value.AsIntegerOrNull().HasValue )
            {
                schedules[hfRowId.Value.AsInteger()] = scheduleText;
            }
            else
            {
                schedules.Add( scheduleText );
            }

            ScheduleDetailState = schedules;

            // Enforce a schedule not going past midnight. It can end ON midnight, but not past.
            // Then the above checks will work.
            // Also need to check for invalid (blank) schedule.

            BindGrid();
            ShowSB = false;
            mdlComponentEdit.Hide();
        }

        /// <summary>
        /// Handles the Click event of the lbEditCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditCancel_Click( object sender, EventArgs e )
        {
            ShowSB = false;
            mdlComponentEdit.Hide();
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEdit( PageParameter( "ScheduleId" ).AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var scheduleService = new WatchdogScheduleCollectionService( rockContext );
            var schedule = scheduleService.Get( PageParameter( "ScheduleId" ).AsInteger() );

            if ( schedule == null )
            {
                schedule = new WatchdogScheduleCollection();
                scheduleService.Add( schedule );
            }

            if ( schedule.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                schedule.Name = tbEditName.Text;
                schedule.Description = tbEditDescription.Text;
                schedule.ScheduleJson = ScheduleDetailState.ToJson();

                rockContext.SaveChanges();
                WatchdogScheduleCollectionCache.Remove( schedule.Id );
            }

            NavigateToCurrentPage( new Dictionary<string, string> { { "ScheduleId", schedule.Id.ToString() } } );
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            if ( PageParameter( "ScheduleId" ).AsInteger() == 0 )
            {
                NavigateToParentPage();
            }
            else
            {
                ShowDetails( PageParameter( "ScheduleId" ).AsInteger() );
            }
        }

        #endregion
    }
}
