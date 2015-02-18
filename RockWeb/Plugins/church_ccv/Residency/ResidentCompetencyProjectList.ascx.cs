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
using church.ccv.Residency.Data;
using church.ccv.Residency.Model;
using Rock;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Residency
{
    [DisplayName( "Resident Competency Project List" )]
    [Category( "CCV > Residency" )]
    [Description( "Lists all of a resident's projects for a competency." )]

    [LinkedPage( "Detail Page" )]
    public partial class ResidentCompetencyProjectList : RockBlock, ISecondaryBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gProjectList.DataKeyNames = new string[] { "Id" };
            gProjectList.Actions.ShowAdd = false;
            gProjectList.GridRebind += gList_GridRebind;
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
                int? competencyPersonId = this.PageParameter( "CompetencyPersonId" ).AsInteger();
                hfCompetencyPersonId.Value = competencyPersonId.ToString();
                BindGrid();
            }
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Binds the project list grid.
        /// </summary>
        protected void BindGrid()
        {
            var competencyPersonProjectService = new ResidencyService<CompetencyPersonProject>( new ResidencyContext() );
            int competencyPersonId = hfCompetencyPersonId.ValueAsInt();
            SortProperty sortProperty = gProjectList.SortProperty;
            var qry = competencyPersonProjectService.Queryable();

            qry = qry.Where( a => a.CompetencyPersonId.Equals( competencyPersonId ) );

            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( s => s.Project.Name ).ThenBy( s => s.Project.Description );
            }

            var resultList = qry.ToList().Select( a => new
            {
                Id = a.Id,
                Name = a.Project.Name,
                Description = a.Project.Description,
                MinAssessmentCount = a.MinAssessmentCount ?? a.Project.MinAssessmentCountDefault,
                AssessmentCompleted = a.CompetencyPersonProjectAssessments.Where( b => b.AssessmentDateTime != null ).Count(),
                AssessmentRemaining = Math.Max( a.MinAssessmentCount ?? a.Project.MinAssessmentCountDefault - a.CompetencyPersonProjectAssessments.Where( b => b.AssessmentDateTime != null ).Count() ?? 0, 0 )
            } ).ToList();

            gProjectList.DataSource = resultList;
            gProjectList.DataBind();
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

        /// <summary>
        /// Handles the RowSelected event of the gProjectList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gProjectList_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "CompetencyPersonProjectId", e.RowKeyId );
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on it's page.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            gProjectList.Visible = visible;
        }

        #endregion
    }
}