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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
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

    [LinkedPage("Detail Page")] 
    public partial class ScheduleList : RockBlock
    {
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

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gSchedules.Actions.ShowAdd = canAddEditDelete;
            gSchedules.IsDeleteEnabled = canAddEditDelete;
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
            NavigateToLinkedPage( "DetailPage", "scheduleId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSchedules_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "scheduleId", e.RowKeyId );
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
            ScheduleService scheduleService = new ScheduleService( new RockContext() );
            SortProperty sortProperty = gSchedules.SortProperty;
            var qry = scheduleService.Queryable().Select( a =>
                new
                {
                    a.Id,
                    a.Name,
                    CategoryName = a.Category.Name
                } );

            if ( sortProperty != null )
            {
                gSchedules.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gSchedules.DataSource = qry.OrderBy( s => s.Name ).ToList();
            }

            gSchedules.EntityTypeId = EntityTypeCache.Read<Schedule>().Id;
            gSchedules.DataBind();
        }

        #endregion
    }
}