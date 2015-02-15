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
    [DisplayName( "Resident Project Point of Assessment List" )]
    [Category( "CCV > Residency" )]
    [Description( "Displays a list of a resident's project's points of assessment." )]

    public partial class ResidentProjectPointOfAssessmentList : RockBlock, ISecondaryBlock
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
            gList.Actions.ShowAdd = false;
            gList.GridRebind += gList_GridRebind;

            gList.Actions.ShowAdd = false;
            gList.IsDeleteEnabled = false;
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
                int? competencyPersonProjectId = this.PageParameter( "CompetencyPersonProjectId" ).AsInteger();
                hfCompetencyPersonProjectId.Value = competencyPersonProjectId.ToString();
                BindGrid();
            }
        }

        #endregion

        #region Grid Events

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
            var competencyPersonProjectService = new ResidencyService<CompetencyPersonProject>( new ResidencyContext() );
            int competencyPersonProjectId = hfCompetencyPersonProjectId.ValueAsInt();
            CompetencyPersonProject competencyPersonProject = competencyPersonProjectService.Get( competencyPersonProjectId );

            if ( competencyPersonProject.CompetencyPerson.PersonId != CurrentPersonId )
            {
                // somebody besides the Resident is logged in
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["CompetencyPersonId"] = competencyPersonProject.CompetencyPersonId.ToString();
                NavigateToParentPage( qryString );
                return;
            }

            var rawList = competencyPersonProject.Project.ProjectPointOfAssessments
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
                    a.AssessmentText
                } ).ToList();

            if ( competencyPersonProject != null )
            {
                gList.DataSource = selectList;
                gList.DataBind();
            }
            else
            {
                NavigateToParentPage();
                return;
            }
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