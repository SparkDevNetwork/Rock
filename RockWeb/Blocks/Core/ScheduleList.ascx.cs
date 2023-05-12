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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    [DisplayName( "Schedule List" )]
    [Category( "Core" )]
    [Description( "Lists all the schedules." )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        IsRequired = false,
        Order = 0 )]

    [BooleanField(
        "Filter Category From Query String",
        Key = AttributeKey.FilterCategoryFromQueryString,
        DefaultBooleanValue = false,
        Order = 1
        )]

    [Rock.SystemGuid.BlockTypeGuid( "C1B934D1-2139-471E-B2B8-B22FF4499B2F" )]
    public partial class ScheduleList : RockBlock, ICustomGridColumns, ISecondaryBlock
    {
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string FilterCategoryFromQueryString = "FilterCategoryFromQueryString";
        }

        private static class PageParameterKey
        {
            public const string CategoryId = "CategoryId";
            public const string CategoryGuid = "CategoryGuid";
        }

        public static class GridUserPreferenceKey
        {
            public const string Category = "Category";
            public const string ActiveStatus = "Active Status";
        }

        #region properties

        private HashSet<int> _schedulesWithAttendance = null;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += ScheduleList_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upScheduleList );

            if ( !this.IsPostBack )
            {
                BindFilter();
            }

            fSchedules.ApplyFilterClick += fSchedules_ApplyFilterClick;
            fSchedules.ClearFilterClick += fSchedules_ClearFilterClick;
            fSchedules.DisplayFilterValue += fSchedules_DisplayFilterValue;

            gSchedules.DataKeyNames = new string[] { "Id" };

            gSchedules.Actions.AddClick += gSchedules_Add;
            gSchedules.GridRebind += gSchedules_GridRebind;
            gSchedules.GridReorder += gSchedules_GridReorder;
            gSchedules.RowDataBound += gSchedules_RowDataBound;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            var hasDetailPage = this.GetAttributeValue( AttributeKey.DetailPage ).IsNotNullOrWhiteSpace();

            gSchedules.Actions.ShowAdd = canAddEditDelete && hasDetailPage;
            gSchedules.IsDeleteEnabled = canAddEditDelete;

            // make a custom delete confirmation dialog
            gSchedules.ShowConfirmDeleteDialog = false;

            string deleteScript = @"
    $('table.js-grid-schedule-list a.grid-delete-button').on('click', function( e ){
        var $btn = $(this);
        e.preventDefault();

        var confirmMsg = 'Are you sure you want to delete this schedule?';
        if ($btn.closest('tr').hasClass('js-has-attendance')) {
            confirmMsg = 'This schedule has attendance history. If you delete this, the attendance history will no longer be associated with the schedule. ' + confirmMsg;
        }

        Rock.dialogs.confirm(confirmMsg, function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( gSchedules, gSchedules.GetType(), "deleteScheduleScript", deleteScript, true );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the ScheduleList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ScheduleList_BlockUpdated( object sender, EventArgs e )
        {
            this.NavigateToCurrentPageReference();
        }

        /// <summary>
        /// fs the schedules display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void fSchedules_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case GridUserPreferenceKey.Category:

                    {
                        var categoryId = e.Value.AsIntegerOrNull();
                        e.Value = string.Empty;
                        if ( categoryId.HasValue && categoryId > 0 )
                        {
                            var category = CategoryCache.Get( categoryId.Value );
                            if ( category != null )
                            {
                                e.Value = category.Name;
                            }
                        }

                        break;
                    }

                case GridUserPreferenceKey.ActiveStatus:

                    {
                        if ( !string.IsNullOrEmpty( e.Value ) && e.Value == "all" )
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }

                default:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the fSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void fSchedules_ClearFilterClick( object sender, EventArgs e )
        {
            fSchedules.DeleteFilterPreferences();
            BindFilter();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the fSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void fSchedules_ApplyFilterClick( object sender, EventArgs e )
        {
            fSchedules.SetFilterPreference( GridUserPreferenceKey.Category, cpCategoryFilter.SelectedValue );
            fSchedules.SetFilterPreference( GridUserPreferenceKey.ActiveStatus, ddlActiveFilter.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            cpCategoryFilter.EntityTypeId = EntityTypeCache.GetId<Rock.Model.Schedule>() ?? 0;
            cpCategoryFilter.SetValue( fSchedules.GetFilterPreference( GridUserPreferenceKey.Category ).AsIntegerOrNull() );
            cpCategoryFilter.Visible = !this.GetAttributeValue( AttributeKey.FilterCategoryFromQueryString ).AsBoolean();
            var itemActiveStatus = fSchedules.GetFilterPreference( GridUserPreferenceKey.ActiveStatus );
            ddlActiveFilter.SetValue( itemActiveStatus );
        }

        /// <summary>
        /// Handles the GridReorder event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        private void gSchedules_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var scheduleList = GetSortedScheduleList( rockContext );
            if ( scheduleList != null )
            {
                new ScheduleService( rockContext ).Reorder( scheduleList, e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        private void gSchedules_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var schedule = e.Row.DataItem as Schedule;

                if ( schedule != null )
                {
                    var scheduleId = schedule.Id;

                    if ( _schedulesWithAttendance.Contains( scheduleId ) )
                    {
                        e.Row.AddCssClass( "js-has-attendance" );
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                int? categoryId = null;

                if ( this.GetAttributeValue( AttributeKey.FilterCategoryFromQueryString ).AsBoolean() )
                {
                    categoryId = this.PageParameter( PageParameterKey.CategoryId ).AsIntegerOrNull();

                    if ( !categoryId.HasValue )
                    {
                        var categoryGuid = this.PageParameter( PageParameterKey.CategoryGuid ).AsGuidOrNull();
                        if ( categoryGuid.HasValue )
                        {
                            categoryId = CategoryCache.GetId( this.PageParameter( PageParameterKey.CategoryGuid ).AsGuid() );
                        }
                    }

                    if ( !categoryId.HasValue )
                    {
                        // if we are in FilterCategoryFromQueryString mode, only show if there is a CategoryId in the URL
                        pnlScheduleList.Visible = false;
                        return;
                    }
                }

                hfCategoryId.Value = categoryId.ToString();

                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSchedules_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "ScheduleId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSchedules_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "ScheduleId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSchedules_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            ScheduleService scheduleService = new ScheduleService( rockContext );
            Schedule schedule = scheduleService.Get( e.RowKeyId );
            if ( schedule != null )
            {
                string errorMessage;
                if ( !scheduleService.CanDelete( schedule, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                scheduleService.Delete( schedule );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSchedules_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            List<Schedule> sortedScheduleList = GetSortedScheduleList( new RockContext() );
            gSchedules.DataSource = sortedScheduleList;

            var categoryId = hfCategoryId.Value.AsIntegerOrNull();
            var categoryGridField = gSchedules.ColumnsWithDataField( "Category.Name" ).FirstOrDefault();

            if ( categoryGridField != null )
            {
                // only show the Category Grid field is a category wasn't specified
                categoryGridField.Visible = !categoryId.HasValue;
            }

            gSchedules.EntityTypeId = EntityTypeCache.Get<Schedule>().Id;
            gSchedules.DataBind();
        }

        /// <summary>
        /// Gets the sorted schedule list.
        /// </summary>
        /// <returns></returns>
        private List<Schedule> GetSortedScheduleList( RockContext rockContext )
        {
            ScheduleService scheduleService = new ScheduleService( rockContext );
            var scheduleQuery = scheduleService.Queryable().Where( a => !string.IsNullOrEmpty( a.Name ) );
            var categoryGuid = this.PageParameter( PageParameterKey.CategoryGuid ).AsGuidOrNull();
            int? categoryId;

            if ( this.GetAttributeValue( AttributeKey.FilterCategoryFromQueryString ).AsBoolean() )
            {
                categoryId = hfCategoryId.Value.AsIntegerOrNull();
            }
            else
            {
                categoryId = fSchedules.GetFilterPreference( GridUserPreferenceKey.Category ).AsIntegerOrNull();
            }

            if ( categoryId.HasValue )
            {
                scheduleQuery = scheduleQuery.Where( a => a.CategoryId == categoryId.Value );
            }

            string activeFilterValue = fSchedules.GetFilterPreference( GridUserPreferenceKey.ActiveStatus );
            if ( !string.IsNullOrWhiteSpace( activeFilterValue ) )
            {
                if ( activeFilterValue != "all" )
                {
                    var activeFilter = activeFilterValue.AsBoolean();
                    scheduleQuery = scheduleQuery.Where( b => b.IsActive == activeFilter );
                }
            }

            // populate _schedulesWithAttendance so that a warning can be displayed if a schedule with attendances is deleted
            var displayedScheduleIds = scheduleQuery.Select( a => a.Id ).ToList();
            var attendancesQry = new AttendanceService( rockContext )
                .Queryable()
                .Where( a => a.Occurrence.ScheduleId.HasValue && displayedScheduleIds.Contains( a.Occurrence.ScheduleId.Value ) )
                .Select( a => a.Occurrence.ScheduleId.Value );

            _schedulesWithAttendance = new HashSet<int>( attendancesQry.Distinct().ToList() );

            var sortedScheduleList = scheduleQuery.ToList().OrderByOrderAndNextScheduledDateTime();
            return sortedScheduleList;
        }

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlScheduleList.Visible = visible;
        }

        #endregion
    }
}