﻿// <copyright>
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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Prayer
{
    [DisplayName( "Prayer Request List" )]
    [Category( "Prayer" )]
    [Description( "Displays a list of prayer requests for the configured top-level group category." )]

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve prayer requests and comments." )]

    [LinkedPage( "Detail Page", Order = 0 )]
    public partial class PrayerRequestList : RockBlock
    {
        #region Fields

        /// <summary>
        /// The prayer request key parameter used in the QueryString for detail page.
        /// </summary>
        private static readonly string _PrayerRequestKeyParameter = "prayerRequestId";

        /// <summary>
        /// The PrayerRequest entity type id.  This causes only categories that are appropriate to the PrayerRequest entity to be listed.
        /// </summary>
        private int? _prayerRequestEntityTypeId = null;

        /// <summary>
        /// Holds whether or not the person can add, edit, and delete.
        /// </summary>
        private bool _canAddEditDelete = false;

        /// <summary>
        /// Holds whether or not the person can approve requests.
        /// </summary>
        private bool _canApprove = false;

        #endregion

        #region Filter's User Preference Setting Keys
        /// <summary>
        /// Constant like string-key-settings that are tied to user saved filter preferences.
        /// </summary>
        public static class FilterSetting
        {
            public static readonly string PrayerCategory = "Prayer Category";
            public static readonly string DateRange = "Date Range";
            public static readonly string ApprovalStatus = "Approval Status";
            public static readonly string UrgentStatus = "Urgent Status";
            public static readonly string ActiveStatus = "Active Status";
            public static readonly string PublicStatus = "Public/Private";
            public static readonly string CommentingStatus = "Commenting Status";
        }
        #endregion

        #region Base Control Methods

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            PrayerRequest prayerRequest = new PrayerRequest();
            Type type = prayerRequest.GetType();
            _prayerRequestEntityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( type.FullName );

            BindFilter();

            gPrayerRequests.DataKeyNames = new string[] { "id" };
            gPrayerRequests.Actions.AddClick += gPrayerRequests_Add;
            gPrayerRequests.GridRebind += gPrayerRequests_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            _canApprove = IsUserAuthorized( "Approve" );
            _canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gPrayerRequests.Actions.ShowAdd = _canAddEditDelete;
            gPrayerRequests.IsDeleteEnabled = _canAddEditDelete;
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Filter
        /// <summary>
        /// Binds any needed data to the Grid Filter also using the user's stored
        /// preferences.
        /// </summary>
        private void BindFilter()
        {
            // Set the Approval Status
            var item = ddlApprovedFilter.Items.FindByValue( gfFilter.GetUserPreference( FilterSetting.ApprovalStatus ) );
            if ( item != null )
            {
                item.Selected = true;
            }

            // Set the Public Status
            var itemPublic = ddlPublicFilter.Items.FindByValue( gfFilter.GetUserPreference( FilterSetting.PublicStatus ) );
            if ( itemPublic != null )
            {
                itemPublic.Selected = true;
            }

            // Set the Commenting Status
            var itemAllowComments = ddlAllowCommentsFilter.Items.FindByValue( gfFilter.GetUserPreference( FilterSetting.CommentingStatus ) );
            if ( itemAllowComments != null )
            {
                itemAllowComments.Selected = true;
            }

            // Set the Active Status
            var itemActiveStatus = ddlActiveFilter.Items.FindByValue( gfFilter.GetUserPreference( FilterSetting.ActiveStatus ) );
            if ( itemActiveStatus != null )
            {
                itemActiveStatus.Selected = true;
            }

            // Set the Active Status
            var itemUrgentStatus = ddlUrgentFilter.Items.FindByValue( gfFilter.GetUserPreference( FilterSetting.UrgentStatus ) );
            if ( itemUrgentStatus != null )
            {
                itemUrgentStatus.Selected = true;
            }
            
            drpDateRange.DelimitedValues = gfFilter.GetUserPreference( FilterSetting.DateRange );

            // Set the category picker's selected value
            int selectedPrayerCategoryId = gfFilter.GetUserPreference( FilterSetting.PrayerCategory ).AsInteger();
            Category prayerCategory = new CategoryService( new RockContext() ).Get( selectedPrayerCategoryId );
            catpPrayerCategoryFilter.SetValue( prayerCategory );
        }

        /// <summary>
        /// Handles the Apply Filter event for the GridFilter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gfFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFilter.SaveUserPreference( FilterSetting.PrayerCategory, catpPrayerCategoryFilter.SelectedValue == Rock.Constants.None.IdValue ? string.Empty : catpPrayerCategoryFilter.SelectedValue );
            gfFilter.SaveUserPreference( FilterSetting.DateRange, drpDateRange.DelimitedValues );

            // only save settings that are not the default "all" preference...
            if ( ddlApprovedFilter.SelectedValue == "all" )
            {
                gfFilter.SaveUserPreference( FilterSetting.ApprovalStatus, string.Empty );
            }
            else
            {
                gfFilter.SaveUserPreference( FilterSetting.ApprovalStatus, ddlApprovedFilter.SelectedValue );
            }

            if ( ddlUrgentFilter.SelectedValue == "all" )
            {
                gfFilter.SaveUserPreference( FilterSetting.UrgentStatus, string.Empty );
            }
            else
            {
                gfFilter.SaveUserPreference( FilterSetting.UrgentStatus, ddlUrgentFilter.SelectedValue );
            }

            if ( ddlPublicFilter.SelectedValue == "all" )
            {
                gfFilter.SaveUserPreference( FilterSetting.PublicStatus, string.Empty );
            }
            else
            {
                gfFilter.SaveUserPreference( FilterSetting.PublicStatus, ddlPublicFilter.SelectedValue );
            }

            if ( ddlActiveFilter.SelectedValue == "all" )
            {
                gfFilter.SaveUserPreference( FilterSetting.ActiveStatus, string.Empty );
            }
            else
            {
                gfFilter.SaveUserPreference( FilterSetting.ActiveStatus, ddlActiveFilter.SelectedValue );
            }

            if ( ddlAllowCommentsFilter.SelectedValue == "all" )
            {
                gfFilter.SaveUserPreference( FilterSetting.CommentingStatus, string.Empty );
            }
            else
            {
                gfFilter.SaveUserPreference( FilterSetting.CommentingStatus, ddlAllowCommentsFilter.SelectedValue );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles displaying the stored filter values.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e as DisplayFilterValueArgs (hint: e.Key and e.Value).</param>
        protected void gfFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Date Range":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                // don't display dead setting
                case "From Date":
                    e.Value = string.Empty;
                    break;

                // don't display dead setting
                case "To Date":
                    e.Value = string.Empty;
                    break;

                case "Prayer Category":

                    int categoryId = e.Value.AsIntegerOrNull() ?? All.Id;
                    if ( categoryId == All.Id )
                    {
                        e.Value = "All";
                    }
                    else
                    {
                        var category = Rock.Web.Cache.CategoryCache.Read( categoryId );
                        if ( category != null )
                        {
                            e.Value = category.Name;
                        }
                    }

                    break;
            }
        }

        #endregion

        #region Prayer Request Grid

        /// <summary>
        /// Binds the grid to a list of Prayer Requests.
        /// </summary>
        private void BindGrid()
        {
            PrayerRequestService prayerRequestService = new PrayerRequestService( new RockContext() );
            SortProperty sortProperty = gPrayerRequests.SortProperty;

            var prayerRequests = prayerRequestService.Queryable().Select( a =>
                new
                {
                    a.Id,
                    FullName = a.FirstName + " " + a.LastName,
                    CategoryName = a.CategoryId.HasValue ? a.Category.Name : null,
                    EnteredDate = a.EnteredDateTime,
                    a.ExpirationDate,
                    a.Text,
                    a.FlagCount,
                    a.IsApproved,
                    a.CategoryId,
                    CategoryParentCategoryId = a.CategoryId.HasValue ? a.Category.ParentCategoryId : null,
                    a.IsUrgent,
                    a.IsPublic,
                    a.IsActive,
                    a.AllowComments
                } );

            // Filter by prayer category if one is selected...
            int selectedPrayerCategoryID = catpPrayerCategoryFilter.SelectedValue.AsIntegerOrNull() ?? All.Id;
            if ( selectedPrayerCategoryID != All.Id && selectedPrayerCategoryID != None.Id )
            {
                prayerRequests = prayerRequests.Where( c => c.CategoryId == selectedPrayerCategoryID
                    || c.CategoryParentCategoryId == selectedPrayerCategoryID );
            }

            // Filter by approved/unapproved
            if ( ddlApprovedFilter.SelectedIndex > -1 )
            {
                if ( ddlApprovedFilter.SelectedValue == "unapproved" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsApproved == false || !a.IsApproved.HasValue );
                }
                else if ( ddlApprovedFilter.SelectedValue == "approved" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsApproved == true );
                }
            }

            // Filter by urgent/non-urgent
            if ( ddlUrgentFilter.SelectedIndex > -1 )
            {
                if ( ddlUrgentFilter.SelectedValue == "non-urgent" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsUrgent == false || !a.IsUrgent.HasValue );
                }
                else if ( ddlUrgentFilter.SelectedValue == "urgent" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsUrgent == true );
                }
            }

            // Filter by public/non-public
            if ( ddlPublicFilter.SelectedIndex > -1 )
            {
                if ( ddlPublicFilter.SelectedValue == "non-public" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsPublic == false || !a.IsPublic.HasValue );
                }
                else if ( ddlPublicFilter.SelectedValue == "public" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsPublic == true );
                }
            }

            // Filter by active/inactive
            if ( ddlActiveFilter.SelectedIndex > -1 )
            {
                if ( ddlActiveFilter.SelectedValue == "inactive" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsActive == false || !a.IsActive.HasValue );
                }
                else if ( ddlActiveFilter.SelectedValue == "active" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsActive == true );
                }
            }

            // Filter by active/inactive
            if ( ddlAllowCommentsFilter.SelectedIndex > -1 )
            {
                if ( ddlAllowCommentsFilter.SelectedValue == "unallow" )
                {
                    prayerRequests = prayerRequests.Where( a => a.AllowComments == false || !a.AllowComments.HasValue );
                }
                else if ( ddlAllowCommentsFilter.SelectedValue == "allow" )
                {
                    prayerRequests = prayerRequests.Where( a => a.AllowComments == true );
                }
            }

            // Filter by Date Range
            if ( drpDateRange.LowerValue.HasValue )
            {
                DateTime startDate = drpDateRange.LowerValue.Value.Date;
                prayerRequests = prayerRequests.Where( a => a.EnteredDate >= startDate );
            }

            if ( drpDateRange.UpperValue.HasValue )
            {
                // Add one day in order to include everything up to the end of the selected datetime.
                var endDate = drpDateRange.UpperValue.Value.AddDays( 1 );
                prayerRequests = prayerRequests.Where( a => a.EnteredDate < endDate );
            }

            // Don't show expired prayer requests.
            // TODO save users filter setting?
            if ( !cbShowExpired.Checked )
            {
                prayerRequests = prayerRequests.Where( a => a.ExpirationDate == null || RockDateTime.Today <= a.ExpirationDate );
            }

            // Sort by the given property otherwise sort by the EnteredDate (and Id)
            // (this is a hack because the Date field alone doesn't sort in the descending direction well)
            if ( sortProperty != null )
            {
                gPrayerRequests.DataSource = prayerRequests.Sort( sortProperty ).ToList();
            }
            else
            {
                // TODO Figure out how to tell Grid what Direction and Property it's sorting on
                //sortProperty.Direction = SortDirection.Ascending;
                //sortProperty.Property = "EnteredDate";
                gPrayerRequests.DataSource = prayerRequests.OrderByDescending( p => p.EnteredDate ).ThenByDescending( p => p.Id ).ToList();
            }

            gPrayerRequests.DataBind();
        }

        /// <summary>
        /// Handles the Add event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", _PrayerRequestKeyParameter, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", _PrayerRequestKeyParameter, e.RowKeyId );
        }

        /// <summary>
        /// Handles the CheckChanged event of the gPrayerRequests IsApproved field.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_CheckChanged( object sender, RowEventArgs e )
        {
            bool failure = true;

            if ( e.RowKeyValue != null )
            {
                var rockContext = new RockContext();
                PrayerRequestService prayerRequestService = new PrayerRequestService( rockContext );
                PrayerRequest prayerRequest = prayerRequestService.Get( e.RowKeyId );

                if ( prayerRequest != null )
                {
                    failure = false;

                    // if it was approved, set it to unapproved... otherwise
                    if ( prayerRequest.IsApproved ?? false )
                    {
                        prayerRequest.IsApproved = false;
                    }
                    else
                    {
                        prayerRequest.IsApproved = true;
                        prayerRequest.ApprovedByPersonAliasId = CurrentPersonAliasId;
                        prayerRequest.ApprovedOnDateTime = RockDateTime.Now;

                        // reset the flag count only to zero ONLY if it had a value previously.
                        if ( prayerRequest.FlagCount.HasValue && prayerRequest.FlagCount > 0 )
                        {
                            prayerRequest.FlagCount = 0;
                        }
                    }

                    rockContext.SaveChanges();
                }

                BindGrid();
            }

            if ( failure )
            {
                maGridWarning.Show( "Unable to approve that prayer request", ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            PrayerRequestService prayerRequestService = new PrayerRequestService( rockContext );
            PrayerRequest prayerRequest = prayerRequestService.Get( e.RowKeyId );

            if ( prayerRequest != null )
            {
                DeleteAllRelatedNotes( prayerRequest, rockContext );

                string errorMessage;
                if ( !prayerRequestService.CanDelete( prayerRequest, out errorMessage ) )
                {
                    maGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                prayerRequestService.Delete( prayerRequest );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Deletes all related notes.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        private void DeleteAllRelatedNotes( PrayerRequest prayerRequest, RockContext rockContext )
        {
            var noteTypeService = new NoteTypeService( rockContext );
            var noteType = noteTypeService.Get( _prayerRequestEntityTypeId.Value, "Prayer Comment" );
            var noteService = new NoteService( rockContext );
            var prayerComments = noteService.Get( noteType.Id, prayerRequest.Id );
            foreach ( Note prayerComment in prayerComments )
            {
                noteService.Delete( prayerComment );
            }
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gPrayerRequests_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles disabling the Toggle fields if the user does not have Approval rights.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( _canApprove )
            {
                return;
            }

            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                foreach ( TableCell cell in e.Row.Cells )
                {
                    foreach ( Control c in cell.Controls )
                    {
                        Toggle toggle = c as Toggle;
                        if ( toggle != null )
                        {
                            toggle.Enabled = false;
                        }
                    }
                }
            }
        }

        #endregion
    }
}