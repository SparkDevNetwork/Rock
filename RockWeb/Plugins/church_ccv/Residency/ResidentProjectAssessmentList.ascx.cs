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
using System.Web.UI.WebControls;
using church.ccv.Residency.Data;
using church.ccv.Residency.Model;
using Rock;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Residency
{
    [DisplayName( "Resident Project Assessment List" )]
    [Category( "CCV > Residency" )]
    [Description( "Displays a list of a resident's project assessments." )]

    [LinkedPage( "Detail Page" )]
    [BooleanField( "Show Competency Column" )]
    [BooleanField( "Show Project Column" )]
    [BooleanField( "Show Grid Title" )]
    public partial class ResidentProjectAssessmentList : RockBlock, ISecondaryBlock
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

            Dictionary<string, BoundField> boundFields = gList.Columns.OfType<BoundField>().ToDictionary( a => a.DataField );
            boundFields["AssessorPerson.FullName"].NullDisplayText = Rock.Constants.None.TextHtml;
            boundFields["AssessmentDateTime"].NullDisplayText = "not completed";

            boundFields["CompetencyPersonProject.CompetencyPerson.Competency.Track.Name"].Visible = this.GetAttributeValue( "ShowCompetencyColumn" ).AsBoolean();
            boundFields["CompetencyPersonProject.CompetencyPerson.Competency.Name"].Visible = this.GetAttributeValue( "ShowCompetencyColumn" ).AsBoolean();
            boundFields["CompetencyPersonProject.Project.Name"].Visible = this.GetAttributeValue( "ShowProjectColumn" ).AsBoolean();
            lblTitle.Visible = this.GetAttributeValue( "ShowGridTitle" ).AsBoolean();
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
        /// Handles the Edit event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "CompetencyPersonProjectAssessmentId", e.RowKeyId );
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
            var competencyPersonProjectAssessmentService = new ResidencyService<CompetencyPersonProjectAssessment>( new ResidencyContext() );
            int competencyPersonProjectId = hfCompetencyPersonProjectId.ValueAsInt();
            SortProperty sortProperty = gList.SortProperty;

            var qry = competencyPersonProjectAssessmentService.Queryable( "AssessorPerson" );

            if ( competencyPersonProjectId != 0 )
            {
                // limit to specific project (and current person)
                qry = qry.Where( a => a.CompetencyPersonProjectId.Equals( competencyPersonProjectId ) );
            }
            else
            {
                // limit only to current person
                qry = qry.Where( a => a.CompetencyPersonProject.CompetencyPerson.PersonId == this.CurrentPersonId );
            }

            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderByDescending( s => s.AssessmentDateTime ).ThenBy( s => s.AssessorPerson );
            }

            gList.DataSource = qry.ToList();
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
            gList.Visible = visible;
        }

        #endregion
    }
}