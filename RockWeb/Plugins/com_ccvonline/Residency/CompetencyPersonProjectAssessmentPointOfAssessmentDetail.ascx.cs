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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_ccvonline.Residency
{
    /// <summary>
    /// Note: This isn't a standard DetailPage.  It takes a two parameters instead of just one
    /// </summary>
    [DisplayName( "Resident Project Point of Assessment Detail" )]
    [Category( "CCV > Residency" )]
    [Description( "Displays the details of a project's point of assessment for a resident." )]

    public partial class CompetencyPersonProjectAssessmentPointOfAssessmentDetail : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                int projectPointOfAssessmentId = PageParameter( "ProjectPointOfAssessmentId" ).AsInteger();
                int competencyPersonProjectAssessmentId = PageParameter( "CompetencyPersonProjectAssessmentId" ).AsInteger();
                ShowDetail( projectPointOfAssessmentId, competencyPersonProjectAssessmentId );
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? competencyPersonProjectAssessmentId = this.PageParameter( pageReference, "CompetencyPersonProjectAssessmentId" ).AsInteger();
            if ( competencyPersonProjectAssessmentId != null )
            {
                breadCrumbs.Add( new BreadCrumb( "Point of Assessment", pageReference ) );
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            // Cancelling on Add/Edit.  Return to Grid
            // if this page was called from the CompetencyPersonProjectAssessment Detail page, return to that
            string competencyPersonProjectAssessmentId = PageParameter( "CompetencyPersonProjectAssessmentId" );
            if ( !string.IsNullOrWhiteSpace( competencyPersonProjectAssessmentId ) )
            {
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["CompetencyPersonProjectAssessmentId"] = competencyPersonProjectAssessmentId;
                NavigateToParentPage( qryString );
            }
            else
            {
                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var residencyContext = new ResidencyContext();
            int projectPointOfAssessmentId = hfProjectPointOfAssessmentId.ValueAsInt();
            int competencyPersonProjectAssessmentId = hfCompetencyPersonProjectAssessmentId.ValueAsInt();

            var competencyPersonProjectAssessmentPointOfAssessmentService = new ResidencyService<CompetencyPersonProjectAssessmentPointOfAssessment>( residencyContext );
            CompetencyPersonProjectAssessmentPointOfAssessment competencyPersonProjectAssessmentPointOfAssessment = null;
            competencyPersonProjectAssessmentPointOfAssessment = competencyPersonProjectAssessmentPointOfAssessmentService.Queryable()
                .Where( a => a.ProjectPointOfAssessmentId.Equals( projectPointOfAssessmentId ) && a.CompetencyPersonProjectAssessmentId.Equals( competencyPersonProjectAssessmentId ) )
                .FirstOrDefault();

            if ( competencyPersonProjectAssessmentPointOfAssessment == null )
            {
                // Create a record to store the rating for this PointOfAssessment if one doesn't already exist
                competencyPersonProjectAssessmentPointOfAssessment = new CompetencyPersonProjectAssessmentPointOfAssessment { Id = 0 };
                competencyPersonProjectAssessmentPointOfAssessment.ProjectPointOfAssessmentId = projectPointOfAssessmentId;
                competencyPersonProjectAssessmentPointOfAssessment.CompetencyPersonProjectAssessmentId = competencyPersonProjectAssessmentId;
                competencyPersonProjectAssessmentPointOfAssessmentService.Add( competencyPersonProjectAssessmentPointOfAssessment );
            }

            competencyPersonProjectAssessmentPointOfAssessment.Rating = tbRating.Text.AsInteger();
            competencyPersonProjectAssessmentPointOfAssessment.RatingNotes = tbRatingNotes.Text;

            if ( !competencyPersonProjectAssessmentPointOfAssessment.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            // get the CompetencyPersonProjectAssessment using the same dbContext 
            var competencyPersonProjectAssessmentService = new ResidencyService<CompetencyPersonProjectAssessment>( competencyPersonProjectAssessmentPointOfAssessmentService.ResidencyContext );
            CompetencyPersonProjectAssessment competencyPersonProjectAssessment = competencyPersonProjectAssessmentService.Get( competencyPersonProjectAssessmentId );

            // set Overall Rating based on average of POA ratings
            competencyPersonProjectAssessment.OverallRating = (decimal?)competencyPersonProjectAssessment.CompetencyPersonProjectAssessmentPointOfAssessments.Average( a => a.Rating );
            residencyContext.SaveChanges();

            if ( competencyPersonProjectAssessmentId != 0 )
            {
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["CompetencyPersonProjectAssessmentId"] = competencyPersonProjectAssessmentId.ToString();
                NavigateToParentPage( qryString );
            }
            else
            {
                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="projectPointOfAssessmentId">The project point of assessment id.</param>
        /// <param name="competencyPersonProjectAssessmentId">The competency person project assessment id.</param>
        public void ShowDetail( int projectPointOfAssessmentId, int competencyPersonProjectAssessmentId )
        {
            pnlDetails.Visible = true;
            var residencyContext = new ResidencyContext();

            var qry = new ResidencyService<CompetencyPersonProjectAssessmentPointOfAssessment>( residencyContext ).Queryable();
            CompetencyPersonProjectAssessmentPointOfAssessment competencyPersonProjectAssessmentPointOfAssessment = null;
            competencyPersonProjectAssessmentPointOfAssessment = qry
                .Where( a => a.ProjectPointOfAssessmentId.Equals( projectPointOfAssessmentId ) && a.CompetencyPersonProjectAssessmentId.Equals( competencyPersonProjectAssessmentId ) ).FirstOrDefault();

            if ( competencyPersonProjectAssessmentPointOfAssessment == null )
            {
                competencyPersonProjectAssessmentPointOfAssessment = new CompetencyPersonProjectAssessmentPointOfAssessment { Id = 0 };
                competencyPersonProjectAssessmentPointOfAssessment.ProjectPointOfAssessmentId = projectPointOfAssessmentId;
                competencyPersonProjectAssessmentPointOfAssessment.ProjectPointOfAssessment
                    = new ResidencyService<ProjectPointOfAssessment>( residencyContext ).Get( projectPointOfAssessmentId );
                competencyPersonProjectAssessmentPointOfAssessment.CompetencyPersonProjectAssessmentId = competencyPersonProjectAssessmentId;
                competencyPersonProjectAssessmentPointOfAssessment.CompetencyPersonProjectAssessment
                    = new ResidencyService<CompetencyPersonProjectAssessment>( residencyContext ).Get( competencyPersonProjectAssessmentId );
            }

            hfProjectPointOfAssessmentId.Value = competencyPersonProjectAssessmentPointOfAssessment.ProjectPointOfAssessmentId.ToString();
            hfCompetencyPersonProjectAssessmentId.Value = competencyPersonProjectAssessmentPointOfAssessment.CompetencyPersonProjectAssessmentId.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( "Project Assessment- Point of Assessment" );
            }

            lReadOnlyTitle.Text = competencyPersonProjectAssessmentPointOfAssessment.ProjectPointOfAssessment.Project.Name.FormatAsHtmlTitle();

            var personProject = competencyPersonProjectAssessmentPointOfAssessment.CompetencyPersonProjectAssessment.CompetencyPersonProject;
            var projectAssessment = competencyPersonProjectAssessmentPointOfAssessment.CompetencyPersonProjectAssessment;

            lblMainDetailsCol1.Text = new DescriptionList()
                .Add( "Resident", personProject.CompetencyPerson.Person )
                .Add( "Project", string.Format( "{0} - {1}", personProject.Project.Name, personProject.Project.Description ) )
                .Add( "Competency", personProject.CompetencyPerson.Competency.Name ).Html;

            lblMainDetailsCol2.Text = new DescriptionList()
                .Add( "Assessment #", competencyPersonProjectAssessmentPointOfAssessment.ProjectPointOfAssessment.AssessmentOrder )
                .Add( "Assessment Text", competencyPersonProjectAssessmentPointOfAssessment.ProjectPointOfAssessment.AssessmentText )
                .Add( "Assessor", projectAssessment.AssessorPerson )
                .Html;

            tbRating.Text = competencyPersonProjectAssessmentPointOfAssessment.Rating.ToString();
            tbRatingNotes.Text = competencyPersonProjectAssessmentPointOfAssessment.RatingNotes;

            tbRating.ReadOnly = readOnly;
            tbRatingNotes.ReadOnly = readOnly;

            btnCancel.Visible = !readOnly;
            btnSave.Text = readOnly ? "Close" : "Save";
        }

        #endregion
    }
}