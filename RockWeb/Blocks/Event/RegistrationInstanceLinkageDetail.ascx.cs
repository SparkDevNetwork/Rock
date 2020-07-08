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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Registration Instance Linkage Detail" )]
    [Category( "Event" )]
    [Description( "Block for editing a linkage associated to an event registration instance." )]

    public partial class RegistrationInstanceLinkageDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Keys

        /// <summary>
        /// Page Parameter Keys
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The registration instance identifier
            /// </summary>
            public const string RegistrationInstanceId = "RegistrationInstanceId";
        }

        #endregion Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "LinkageId" ).AsInteger() );
            }
        }

        #endregion

        #region Events

        #region Main Form Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new EventItemOccurrenceGroupMapService( rockContext );

                EventItemOccurrenceGroupMap linkage = null;

                int? linkageId = hfLinkageId.Value.AsIntegerOrNull();
                if ( linkageId.HasValue )
                {
                    linkage = service.Get( linkageId.Value );
                }

                if ( linkage == null )
                {
                    linkage = new EventItemOccurrenceGroupMap();
                    linkage.RegistrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsInteger();
                    service.Add( linkage );
                }

                linkage.EventItemOccurrenceId = hfLinkageEventItemOccurrenceId.Value.AsIntegerOrNull();
                linkage.GroupId = gpLinkageGroup.SelectedValueAsInt();
                linkage.PublicName = tbLinkagePublicName.Text;
                linkage.UrlSlug = tbLinkageUrlSlug.Text;

                if ( !Page.IsValid || !linkage.IsValid )
                {
                    return;
                }

                rockContext.SaveChanges();
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion

        #region Linkage Dialog Events

        /// <summary>
        /// Handles the Click event of the lbLinkageEventItemOccurrenceAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLinkageEventItemOccurrenceAdd_Click( object sender, EventArgs e )
        {
            // Event Calendar list for new Linkage dialog
            ddlCalendar.Items.Clear();

            using ( var rockContext = new RockContext() )
            {
                foreach ( var calendar in new EventCalendarService( rockContext )
                    .Queryable().AsNoTracking() )
                {
                    if ( calendar.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ddlCalendar.Items.Add( new ListItem( calendar.Name, calendar.Id.ToString() ) );
                    }
                }
            }

            if ( ddlCalendar.Items.Count > 0 )
            {
                nbNoCalendar.Visible = false;
                ddlCalendar.Visible = true;
                dlgAddCalendarItemPage1.SaveButtonText = "Next";
            }
            else
            {
                nbNoCalendar.Visible = true;
                ddlCalendar.Visible = false;
                dlgAddCalendarItemPage1.SaveButtonText = string.Empty;
            }

            ShowDialog( "AddCalendarItemPage1" );
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgAddCalendarItemPage1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgAddCalendarItemPage1_SaveClick( object sender, EventArgs e )
        {
            drpLinkageDateRange.LowerValue = RockDateTime.Today.AddYears( -1 ).AddDays( 1 );
            drpLinkageDateRange.UpperValue = RockDateTime.Today.AddYears( 1 ).AddDays( -1 );

            HideDialog();
            ShowDialog( "AddCalendarItemPage2" );
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgAddCalendarItemPage2 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgAddCalendarItemPage2_SaveClick( object sender, EventArgs e )
        {
            int? calendarId = ddlCalendar.SelectedValueAsInt();
            DateTime fromDate = drpLinkageDateRange.LowerValue ?? RockDateTime.Today.AddYears( -1 ).AddDays( 1 );
            DateTime toDate = drpLinkageDateRange.UpperValue ?? RockDateTime.Today.AddYears( 1 ).AddDays( -1 );

            ddlCalendarItem.Items.Clear();

            if ( calendarId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    ddlCalendarItem.DataSource = new EventCalendarItemService( rockContext )
                        .Queryable( "EventItem.EventItemOccurrences.Schedule" )
                        .AsNoTracking()
                        .Where( i =>
                            i.EventCalendarId == calendarId.Value &&
                            i.EventItem != null &&
                            i.EventItem.IsActive &&
                            i.EventItem.IsApproved &&
                            i.EventItem.EventItemOccurrences.Any() )
                        .OrderBy( i => i.EventItem.Name )
                        .ToList()
                        .Where( i => i.EventItem.GetStartTimes( fromDate, toDate.AddDays( 1 ) ).Any() )
                        .Select( c => new
                        {
                            Id = c.EventItem.Id,
                            Name = c.EventItem.Name
                        } )
                        .ToList();
                    ddlCalendarItem.DataBind();
                }

                if ( ddlCalendarItem.Items.Count > 0 )
                {
                    ddlCalendarItem.SelectedIndex = 0;
                    BindLinkageCalendarItemOccurrence();

                    nbNoLinkage.Visible = false;
                    dlgAddCalendarItemPage3.SaveButtonText = "OK";
                }
                else
                {
                    nbNoLinkage.Visible = true;
                    dlgAddCalendarItemPage3.SaveButtonText = string.Empty;
                }
            }

            HideDialog();
            ShowDialog( "AddCalendarItemPage3" );
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgAddCalendarItemPage3 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgAddCalendarItemPage3_SaveClick( object sender, EventArgs e )
        {
            // Save selection to hidden field
            using ( var rockContext = new RockContext() )
            {
                int? eventItemOccurrenceId = ddlCalendarItemOccurrence.SelectedValueAsInt();
                if ( eventItemOccurrenceId.HasValue )
                {
                    var eventItemOccurrence = new EventItemOccurrenceService( rockContext )
                        .Queryable( "EventItem,Campus" ).AsNoTracking()
                        .Where( c => c.Id == eventItemOccurrenceId.Value )
                        .FirstOrDefault();
                    if ( eventItemOccurrence != null )
                    {
                        hfLinkageEventItemOccurrenceId.Value = eventItemOccurrence.Id.ToString();
                        lLinkageEventItemOccurrence.Text = eventItemOccurrence.ToString();
                        lbLinkageEventItemOccurrenceAdd.Visible = false;
                        lbLinkageEventItemOccurrenceRemove.Visible = true;
                    }
                }
            }

            HideDialog();
        }

        /// <summary>
        /// Handles the Click event of the lbLinkageEventItemOccurrenceRemove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLinkageEventItemOccurrenceRemove_Click( object sender, EventArgs e )
        {
            hfLinkageEventItemOccurrenceId.Value = string.Empty;
            lLinkageEventItemOccurrence.Text = string.Empty;
            lbLinkageEventItemOccurrenceAdd.Visible = true;
            lbLinkageEventItemOccurrenceRemove.Visible = false;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCalendarItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCalendarItem_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindLinkageCalendarItemOccurrence();
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Navigates to the parent page with necessary query params.
        /// </summary>
        private void NavigateToParentPage()
        {
            NavigateToParentPage( new Dictionary<string, string>
            {
                { PageParameterKey.RegistrationInstanceId, PageParameter( PageParameterKey.RegistrationInstanceId ) }
            } );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        public void ShowDetail( int linkageId )
        {
            pnlDetails.Visible = true;

            EventItemOccurrenceGroupMap linkage = null;

            var rockContext = new RockContext();

            if ( !linkageId.Equals( 0 ) )
            {
                linkage = new EventItemOccurrenceGroupMapService( rockContext ).Get( linkageId );
                lActionTitle.Text = ActionTitle.Edit( "Linkage" ).FormatAsHtmlTitle();
            }

            if ( linkage == null )
            {
                linkage = new EventItemOccurrenceGroupMap { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( "Linkage" ).FormatAsHtmlTitle();
            }

            hfLinkageId.Value = linkage.Id.ToString();

            if ( linkage.EventItemOccurrence != null )
            {
                hfLinkageEventItemOccurrenceId.Value = linkage.EventItemOccurrence.Id.ToString();
                lLinkageEventItemOccurrence.Text = linkage.EventItemOccurrence.ToString();
                lbLinkageEventItemOccurrenceAdd.Visible = false;
                lbLinkageEventItemOccurrenceRemove.Visible = true;
            }
            else
            {
                hfLinkageEventItemOccurrenceId.Value = string.Empty;
                lLinkageEventItemOccurrence.Text = string.Empty;
                lbLinkageEventItemOccurrenceAdd.Visible = true;
                lbLinkageEventItemOccurrenceRemove.Visible = false;
            }

            gpLinkageGroup.SetValue( linkage.Group );
            gpLinkageGroup_SelectItem( null, null );

            tbLinkagePublicName.Text = linkage.PublicName;
            tbLinkageUrlSlug.Text = linkage.UrlSlug;
        }

        /// <summary>
        /// Binds the linkage calendar item campus.
        /// </summary>
        private void BindLinkageCalendarItemOccurrence()
        {
            ddlCalendarItemOccurrence.Items.Clear();

            int? eventItemId = ddlCalendarItem.SelectedValueAsInt();
            if ( eventItemId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    ddlCalendarItemOccurrence.DataSource = new EventItemOccurrenceService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( c => c.EventItemId == eventItemId.Value )
                        .ToList()
                        .Select( c => new
                        {
                            c.Id,
                            c.NextStartDateTime,
                            Campus = c.Campus != null ? c.Campus.Name : "All Campuses",
                            Order = c.CampusId.HasValue ? 1 : 0
                        } )
                        .Select( c => new
                        {
                            c.Id,
                            c.NextStartDateTime,
                            c.Campus,
                            Name = c.NextStartDateTime.HasValue ? string.Format( "{0} - {1}", c.Campus, c.NextStartDateTime.Value.ToShortDateTimeString() ) : c.Campus,
                            c.Order
                        } )
                        .OrderBy( c => c.Order )
                        .ThenBy( c => c.Campus )
                        .ThenBy( c => c.NextStartDateTime ?? DateTime.MinValue )
                        .ToList();
                    ddlCalendarItemOccurrence.DataBind();

                    if ( ddlCalendarItemOccurrence.Items.Count > 0 )
                    {
                        ddlCalendarItemOccurrence.SelectedIndex = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "ADDCALENDARITEMPAGE1":
                    dlgAddCalendarItemPage1.Show();
                    break;
                case "ADDCALENDARITEMPAGE2":
                    dlgAddCalendarItemPage2.Show();
                    break;
                case "ADDCALENDARITEMPAGE3":
                    dlgAddCalendarItemPage3.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "ADDCALENDARITEMPAGE1":
                    dlgAddCalendarItemPage1.Hide();
                    break;
                case "ADDCALENDARITEMPAGE2":
                    dlgAddCalendarItemPage2.Hide();
                    break;
                case "ADDCALENDARITEMPAGE3":
                    dlgAddCalendarItemPage3.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion

        /// <summary>
        /// Handles the SelectItem event of the gpLinkageGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpLinkageGroup_SelectItem( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var registrationInstance = new RegistrationInstanceService( rockContext ).Get( PageParameter( PageParameterKey.RegistrationInstanceId ).AsInteger() );
            var group = new GroupService( rockContext ).Get( gpLinkageGroup.SelectedValue.AsInteger() );
            bool showGroupTypeWarning = false;
            if ( registrationInstance != null && group != null )
            {
                if ( registrationInstance.RegistrationTemplate.GroupTypeId != group.GroupTypeId )
                {
                    showGroupTypeWarning = true;
                }
            }

            nbGroupTypeWarning.Visible = showGroupTypeWarning;
        }
    }
}