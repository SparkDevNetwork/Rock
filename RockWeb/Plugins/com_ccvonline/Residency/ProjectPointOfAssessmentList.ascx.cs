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
using System.Linq;
using System.Web.UI;
using com.ccvonline.Residency.Data;
using com.ccvonline.Residency.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_ccvonline.Residency
{
    [DisplayName( "Point of Assessment List" )]
    [Category( "CCV > Residency" )]
    [Description( "Displays a list of a project's points of assessment." )]

    [LinkedPage( "Detail Page" )]
    public partial class ProjectPointOfAssessmentList : RockBlock, ISecondaryBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gList.DataKeyNames = new string[] { "id" };
            gList.Actions.ShowAdd = true;
            gList.Actions.AddClick += gList_Add;
            gList.GridRebind += gList_GridRebind;
            gList.GridReorder += gList_GridReorder;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( Rock.Security.Authorization.EDIT );
            gList.Actions.ShowAdd = canAddEditDelete;
            gList.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                int? projectId = this.PageParameter( "ProjectId" ).AsInteger();

                hfProjectId.Value = projectId.ToString();
                BindGrid();
            }
        }

        /// <summary>
        /// Handles the GridReorder event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gList_GridReorder( object sender, GridReorderEventArgs e )
        {
            int oldIndex = e.OldIndex;
            int newIndex = e.NewIndex;
            int projectId = hfProjectId.ValueAsInt();

            var residencyContext = new ResidencyContext();

            var projectPointOfAssessmentService = new ResidencyService<ProjectPointOfAssessment>( residencyContext );
            var items = projectPointOfAssessmentService.Queryable()
                .Where( a => a.ProjectId.Equals( projectId ) )
                .OrderBy( a => a.AssessmentOrder ).ToList();

            ProjectPointOfAssessment movedItem = items[oldIndex];
            items.RemoveAt( oldIndex );
            if ( newIndex >= items.Count )
            {
                items.Add( movedItem );
            }
            else
            {
                items.Insert( newIndex, movedItem );
            }

            UpdateItemOrders( residencyContext, items );

            BindGrid();
        }

        /// <summary>
        /// Updates the AssessmentOrder of all the ProjectPointOfAssessment items and SavesChanges
        /// </summary>
        /// <param name="residencyContext">The residency context.</param>
        /// <param name="items">The items.</param>
        private void UpdateItemOrders( ResidencyContext residencyContext, List<ProjectPointOfAssessment> items )
        {
            residencyContext.WrapTransaction( () =>
            {
                int order = 1;
                foreach ( ProjectPointOfAssessment item in items )
                {
                    if ( item != null )
                    {
                        if ( item.AssessmentOrder != order )
                        {
                            // temporarily, set the order to negative in case another row has this value.
                            item.AssessmentOrder = -order;
                        }
                    }

                    order++;
                }

                residencyContext.SaveChanges();

                foreach ( ProjectPointOfAssessment item in items )
                {
                    if ( item != null )
                    {
                        if ( item.AssessmentOrder < 0 )
                        {
                            // update the value back to positive now that all the rows have their new order
                            item.AssessmentOrder = -item.AssessmentOrder;
                        }
                    }
                }

                residencyContext.SaveChanges();
            } );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gList_Add( object sender, EventArgs e )
        {
            gList_ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Edit( object sender, RowEventArgs e )
        {
            gList_ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Gs the list_ show edit.
        /// </summary>
        /// <param name="projectPointOfAssessmentId">The residency project point of assessment id.</param>
        protected void gList_ShowEdit( int projectPointOfAssessmentId )
        {
            NavigateToLinkedPage( "DetailPage", "ProjectPointOfAssessmentId", projectPointOfAssessmentId, "ProjectId", hfProjectId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the Delete event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Delete( object sender, RowEventArgs e )
        {
            var residencyContext = new ResidencyContext();
            var projectPointOfAssessmentService = new ResidencyService<ProjectPointOfAssessment>( residencyContext );

            ProjectPointOfAssessment projectPointOfAssessment = projectPointOfAssessmentService.Get( e.RowKeyId );
            if ( projectPointOfAssessment != null )
            {
                string errorMessage;
                if ( !projectPointOfAssessmentService.CanDelete( projectPointOfAssessment, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                projectPointOfAssessmentService.Delete( projectPointOfAssessment );
                residencyContext.SaveChanges();

                int iProjectId = hfProjectId.ValueAsInt();

                // after an item is deleted, we need to renumber all the items
                List<ProjectPointOfAssessment> items = projectPointOfAssessmentService.Queryable().Where( a => a.ProjectId == iProjectId ).OrderBy( a => a.AssessmentOrder ).ToList();
                UpdateItemOrders( residencyContext, items );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gList_GridRebind( object sender, EventArgs e )
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
            var projectPointOfAssessmentService = new ResidencyService<ProjectPointOfAssessment>( new ResidencyContext() );
            int projectId = hfProjectId.ValueAsInt();
            var rawList = projectPointOfAssessmentService.Queryable()
                .Where( a => a.ProjectId == projectId )
                .OrderBy( s => s.AssessmentOrder ).ToList();

            foreach ( var item in rawList )
            {
                if ( item.PointOfAssessmentTypeValue != null )
                {
                    item.PointOfAssessmentTypeValue.LoadAttributes();
                }
            }

            var selectList = rawList.Select( a =>
                new
                {
                    a.Id,
                    ProjectPointOfAssessmentColor = a.PointOfAssessmentTypeValue != null ? a.PointOfAssessmentTypeValue.GetAttributeValue( "Color" ) : string.Empty,
                    a.PointOfAssessmentTypeValue,
                    a.AssessmentOrder,
                    a.AssessmentText,
                } ).ToList();

            gList.DataSource = selectList;
            gList.DataBind();
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlList.Visible = visible;
        }

        #endregion
    }
}