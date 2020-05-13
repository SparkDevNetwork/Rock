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

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]

    public partial class ScheduleList : RockBlock, ICustomGridColumns
    {
        public static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
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

            gSchedules.DataKeyNames = new string[] { "Id" };
            gSchedules.Actions.ShowAdd = true;
            gSchedules.Actions.AddClick += gSchedules_Add;
            gSchedules.GridRebind += gSchedules_GridRebind;
            gSchedules.RowDataBound += gSchedules_RowDataBound;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gSchedules.Actions.ShowAdd = canAddEditDelete;
            gSchedules.IsDeleteEnabled = canAddEditDelete;

            // make a custom delete confirmation dialog
            gSchedules.ShowConfirmDeleteDialog = false;

            string deleteScript = @"
    $('table.js-grid-schedule-list a.grid-delete-button').on('click', function( e ){
        var $btn = $(this);
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this schedule?', function (result) {
            if (result) {
                if ( $btn.closest('tr').hasClass('js-has-attendance') ) {
                    Rock.dialogs.confirm('This schedule has attendance history. Are you sure that you want to delete this schedule and all of its attendance history?', function (result) {
                        if (result) {
                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                        }
                    });
                } else {
                    window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                }
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( gSchedules, gSchedules.GetType(), "deleteScheduleScript", deleteScript, true );
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
                var scheduleRow = e.Row.DataItem as object;

                if ( scheduleRow != null )
                {
                    var scheduleId = ( int ) scheduleRow.GetPropertyValue( "Id" );

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
            var rockContext = new RockContext();
            ScheduleService scheduleService = new ScheduleService( rockContext );
            SortProperty sortProperty = gSchedules.SortProperty;
            var qry = scheduleService.Queryable().Select( a =>
                new
                {
                    a.Id,
                    a.Name,
                    CategoryName = a.Category.Name
                } );

            _schedulesWithAttendance = new HashSet<int>( new AttendanceService( rockContext ).Queryable().Where( a => a.Occurrence.ScheduleId.HasValue ).Select( a => a.Occurrence.ScheduleId.Value ).Distinct().ToList() );

            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( s => s.Name );
            }

            gSchedules.SetLinqDataSource( qry.AsNoTracking() );

            gSchedules.EntityTypeId = EntityTypeCache.Get<Schedule>().Id;
            gSchedules.DataBind();
        }

        #endregion
    }
}