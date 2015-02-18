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
using church.ccv.Residency.Data;
using church.ccv.Residency.Model;
using Rock;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Residency
{
    [DisplayName( "Project Assessment - Point of Assessment List" )]
    [Category( "CCV > Residency" )]
    [Description( "Lists all the resident's points of assessment for a project assessment." )]

    [LinkedPage( "Detail Page" )]
    public partial class CompetencyPersonProjectAssessmentPointOfAssessmentList : RockBlock, ISecondaryBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // NOTE:  this is special case of where we need two key fields, and no add or delete
            gList.DataKeyNames = new string[] { "ProjectPointOfAssessmentId", "CompetencyPersonProjectAssessmentId" };
            gList.Actions.ShowAdd = false;
            gList.IsDeleteEnabled = false;
            gList.GridRebind += gList_GridRebind;
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
                int? competencyPersonProjectAssessmentId = this.PageParameter( "CompetencyPersonProjectAssessmentId" ).AsInteger();
                hfCompetencyPersonProjectAssessmentId.Value = competencyPersonProjectAssessmentId.ToString();
                BindGrid();
            }
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Edit event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Edit( object sender, RowEventArgs e )
        {
            gList_ShowEdit( (int)e.RowKeyValues["ProjectPointOfAssessmentId"], (int)e.RowKeyValues["CompetencyPersonProjectAssessmentId"] );
        }

        /// <summary>
        /// Gs the list_ show edit.
        /// </summary>
        /// <param name="competencyPersonProjectAssessmentPointOfAssessmentId">The residency competency person project assessment point of assessment id.</param>
        protected void gList_ShowEdit( int projectPointOfAssessmentId, int competencyPersonProjectAssessmentId )
        {
            NavigateToLinkedPage( "DetailPage", "ProjectPointOfAssessmentId", projectPointOfAssessmentId, "CompetencyPersonProjectAssessmentId", competencyPersonProjectAssessmentId );
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
            SortProperty sortProperty = gList.SortProperty;
            var residencyContext = new ResidencyContext();
            int competencyPersonProjectAssessmentId = hfCompetencyPersonProjectAssessmentId.ValueAsInt();

            List<CompetencyPersonProjectAssessmentPointOfAssessment> personPointOfAssessmentList = new ResidencyService<CompetencyPersonProjectAssessmentPointOfAssessment>( residencyContext ).Queryable()
                .Where( a => a.CompetencyPersonProjectAssessmentId.Equals( competencyPersonProjectAssessmentId ) ).ToList();

            CompetencyPersonProjectAssessment competencyPersonProjectAssessment
                = new ResidencyService<CompetencyPersonProjectAssessment>( residencyContext ).Get( competencyPersonProjectAssessmentId );

            List<ProjectPointOfAssessment> projectPointOfAssessmentList;
            if ( competencyPersonProjectAssessment != null )
            {
                projectPointOfAssessmentList = new ResidencyService<ProjectPointOfAssessment>( residencyContext ).Queryable()
                    .Where( a => a.ProjectId.Equals( competencyPersonProjectAssessment.CompetencyPersonProject.ProjectId ) ).ToList();
            }
            else
            {
                projectPointOfAssessmentList = new List<ProjectPointOfAssessment>();
            }

            foreach ( var item in projectPointOfAssessmentList )
            {
                if ( item.PointOfAssessmentTypeValue != null )
                {
                    item.PointOfAssessmentTypeValue.LoadAttributes();
                }
            }

            var joinedItems = from projectPointOfAssessment in projectPointOfAssessmentList
                              join personPointOfAssessment in personPointOfAssessmentList
                              on projectPointOfAssessment.Id equals personPointOfAssessment.ProjectPointOfAssessmentId into groupJoin
                              from qryResult in groupJoin.DefaultIfEmpty()
                              select new
                              {
                                  // note: two key fields, since we want to show all the Points of Assessment for this Project, if the person hasn't had a rating on it yet
                                  ProjectPointOfAssessmentId = projectPointOfAssessment.Id,
                                  CompetencyPersonProjectAssessmentId = competencyPersonProjectAssessmentId,
                                  ProjectPointOfAssessmentColor = projectPointOfAssessment.PointOfAssessmentTypeValue != null ? projectPointOfAssessment.PointOfAssessmentTypeValue.GetAttributeValue( "Color" ) : string.Empty,
                                  ProjectPointOfAssessment = projectPointOfAssessment,
                                  CompetencyPersonProjectAssessmentPointOfAssessment = personPointOfAssessmentList.FirstOrDefault( a => a.ProjectPointOfAssessmentId.Equals( projectPointOfAssessment.Id ) )
                              };

            if ( sortProperty != null )
            {
                gList.DataSource = joinedItems.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gList.DataSource = joinedItems.OrderBy( s => s.ProjectPointOfAssessment.AssessmentOrder ).ToList();
            }

            gList.DataBind();
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on it's page.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlList.Visible = visible;
        }

        #endregion
    }
}