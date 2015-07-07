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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Registration Instance Linkage Detail" )]
    [Category( "Event" )]
    [Description( "Template block for editing a linkage associated to an event registration instance." )]

    public partial class RegistrationInstanceLinkageDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {

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
                var service = new EventItemCampusGroupMapService( rockContext );

                EventItemCampusGroupMap linkage = null;

                int? linkageId = hfLinkageId.Value.AsIntegerOrNull();
                if ( linkageId.HasValue )
                {
                    linkage = service.Get( linkageId.Value );
                }

                if ( linkage == null )
                {
                    linkage = new EventItemCampusGroupMap();
                    linkage.RegistrationInstanceId = PageParameter( "RegistrationInstanceId" ).AsInteger();
                    service.Add( linkage );
                }

                linkage.EventItemCampusId = hfLinkageEventItemCampusId.Value.AsIntegerOrNull();
                linkage.GroupId = gpLinkageGroup.SelectedValueAsInt();
                linkage.PublicName = tbLinkagePublicName.Text;
                linkage.UrlSlug = tbLinkageUrlSlug.Text;

                if ( !Page.IsValid || !linkage.IsValid )
                {
                    return;
                }

                rockContext.SaveChanges();
            }

            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "RegistrationInstanceId", PageParameter( "RegistrationInstanceId" ) );
            qryParams.Add( "Tab", "3" );
            NavigateToParentPage( qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "RegistrationInstanceId", PageParameter( "RegistrationInstanceId" ) );
            qryParams.Add( "Tab", "3" );
            NavigateToParentPage( qryParams );
        }

        #endregion

        #region Linkage Dialog Events

        /// <summary>
        /// Handles the Click event of the lbLinkageEventItemCampusAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLinkageEventItemCampusAdd_Click( object sender, EventArgs e )
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
                        .Queryable( "EventItem.EventItemCampuses.EventItemSchedules.Schedule" )
                        .AsNoTracking()
                        .Where( i =>
                            i.EventCalendarId == calendarId.Value &&
                            i.EventItem != null &&
                            i.EventItem.IsActive &&
                            i.EventItem.IsApproved &&
                            i.EventItem.EventItemCampuses.Any() )
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
                    BindLinkageCalendarItemCampus();

                    nbNoLinkage.Visible = false;
                    dlgAddCalendarItemPage3.SaveButtonText = "Ok";
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
                int? eventItemCampusId = ddlCalendarItemCampus.SelectedValueAsInt();
                if ( eventItemCampusId.HasValue )
                {
                    var eventItemCampus = new EventItemCampusService( rockContext )
                        .Queryable( "EventItem,Campus" ).AsNoTracking()
                        .Where( c => c.Id == eventItemCampusId.Value )
                        .FirstOrDefault();
                    if ( eventItemCampus != null )
                    {
                        hfLinkageEventItemCampusId.Value = eventItemCampus.Id.ToString();
                        lLinkageEventItemCampus.Text = eventItemCampus.ToString();
                        lbLinkageEventItemCampusAdd.Visible = false;
                        lbLinkageEventItemCampusRemove.Visible = true;
                    }
                }
            }

            HideDialog();
        }

        /// <summary>
        /// Handles the Click event of the lbLinkageEventItemCampusRemove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLinkageEventItemCampusRemove_Click( object sender, EventArgs e )
        {
            hfLinkageEventItemCampusId.Value = string.Empty;
            lLinkageEventItemCampus.Text = string.Empty;
            lbLinkageEventItemCampusAdd.Visible = true;
            lbLinkageEventItemCampusRemove.Visible = false;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCalendarItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCalendarItem_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindLinkageCalendarItemCampus();
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        public void ShowDetail( int linkageId )
        {
            pnlDetails.Visible = true;

            EventItemCampusGroupMap linkage = null;

            var rockContext = new RockContext();

            if ( !linkageId.Equals( 0 ) )
            {
                linkage = new EventItemCampusGroupMapService( rockContext ).Get( linkageId );
                lActionTitle.Text = ActionTitle.Edit( "Linkage" ).FormatAsHtmlTitle();
            }

            if ( linkage == null )
            {
                linkage = new EventItemCampusGroupMap { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( "Linkage" ).FormatAsHtmlTitle();
            }

            hfLinkageId.Value = linkage.Id.ToString();

            if ( linkage.EventItemCampus != null )
            {
                hfLinkageEventItemCampusId.Value = linkage.EventItemCampus.Id.ToString();
                lLinkageEventItemCampus.Text = linkage.EventItemCampus.ToString();
                lbLinkageEventItemCampusAdd.Visible = false;
                lbLinkageEventItemCampusRemove.Visible = true;
            }
            else
            {
                hfLinkageEventItemCampusId.Value = string.Empty;
                lLinkageEventItemCampus.Text = string.Empty;
                lbLinkageEventItemCampusAdd.Visible = true;
                lbLinkageEventItemCampusRemove.Visible = false;
            }

            gpLinkageGroup.SetValue( linkage.Group );

            tbLinkagePublicName.Text = linkage.PublicName;
            tbLinkageUrlSlug.Text = linkage.UrlSlug;
        }

        /// <summary>
        /// Binds the linkage calendar item campus.
        /// </summary>
        private void BindLinkageCalendarItemCampus()
        {
            ddlCalendarItemCampus.Items.Clear();

            int? eventItemId = ddlCalendarItem.SelectedValueAsInt();
            if ( eventItemId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    ddlCalendarItemCampus.DataSource = new EventItemCampusService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( c => c.EventItemId == eventItemId.Value )
                        .Select( c => new
                        {
                            Id = c.Id,
                            Name = c.Campus != null ? c.Campus.Name : "All Campuses",
                            Order = c.CampusId.HasValue ? 1 : 0
                        } )
                        .OrderBy( c => c.Order )
                        .ThenBy( c => c.Name )
                        .ToList();
                    ddlCalendarItemCampus.DataBind();

                    if ( ddlCalendarItem.Items.Count > 0 )
                    {
                        ddlCalendarItem.SelectedIndex = 0;
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

    }
}