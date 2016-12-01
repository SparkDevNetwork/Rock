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
using System.Web.UI;
using church.ccv.Residency.Data;
using church.ccv.Residency.Model;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;

namespace RockWeb.Plugins.church_ccv.Residency
{
    [DisplayName( "Resident Competency Detail" )]
    [Category( "CCV > Residency" )]
    [Description( "Simple detail form that for a resident's assignment to a specific competency." )]

    public partial class CompetencyPersonProjectAssessmentDetail : RockBlock, IDetailBlock
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
                string competencyPersonProjectAssessmentId = PageParameter( "CompetencyPersonProjectAssessmentId" );
                if ( !string.IsNullOrWhiteSpace( competencyPersonProjectAssessmentId ) )
                {
                    ShowDetail( competencyPersonProjectAssessmentId.AsInteger(), PageParameter( "CompetencyPersonProjectId" ).AsIntegerOrNull() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
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
                breadCrumbs.Add( new BreadCrumb( "Assessment", pageReference ) );
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
            SetEditMode( false );

            if ( hfCompetencyPersonProjectAssessmentId.ValueAsInt().Equals( 0 ) )
            {
                // Cancelling on Add.  Return to Grid
                // if this page was called from the Project Detail page, return to that
                string competencyPersonProjectId = PageParameter( "CompetencyPersonProjectId" );
                if ( !string.IsNullOrWhiteSpace( competencyPersonProjectId ) )
                {
                    Dictionary<string, string> qryString = new Dictionary<string, string>();
                    qryString["CompetencyPersonProjectId"] = competencyPersonProjectId;
                    NavigateToParentPage( qryString );
                }
                else
                {
                    NavigateToParentPage();
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                ResidencyService<CompetencyPersonProjectAssessment> service = new ResidencyService<CompetencyPersonProjectAssessment>( new ResidencyContext() );
                CompetencyPersonProjectAssessment item = service.Get( hfCompetencyPersonProjectAssessmentId.ValueAsInt() );
                ShowReadonlyDetails( item );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            ResidencyService<CompetencyPersonProjectAssessment> service = new ResidencyService<CompetencyPersonProjectAssessment>( new ResidencyContext() );
            CompetencyPersonProjectAssessment item = service.Get( hfCompetencyPersonProjectAssessmentId.ValueAsInt() );
            ShowEditDetails( item );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var residencyContext = new ResidencyContext();
            CompetencyPersonProjectAssessment competencyPersonProjectAssessment;
            ResidencyService<CompetencyPersonProjectAssessment> competencyPersonProjectAssessmentService = new ResidencyService<CompetencyPersonProjectAssessment>( residencyContext );

            int competencyPersonProjectAssessmentId = int.Parse( hfCompetencyPersonProjectAssessmentId.Value );

            if ( competencyPersonProjectAssessmentId == 0 )
            {
                competencyPersonProjectAssessment = new CompetencyPersonProjectAssessment();
                competencyPersonProjectAssessmentService.Add( competencyPersonProjectAssessment );
            }
            else
            {
                competencyPersonProjectAssessment = competencyPersonProjectAssessmentService.Get( competencyPersonProjectAssessmentId );
            }

            competencyPersonProjectAssessment.CompetencyPersonProjectId = hfCompetencyPersonProjectId.ValueAsInt();
            competencyPersonProjectAssessment.AssessorPersonId = ppAssessor.PersonId;
            competencyPersonProjectAssessment.AssessmentDateTime = dtpAssessmentDateTime.SelectedDateTime;
            competencyPersonProjectAssessment.RatingNotes = tbRatingNotes.Text;
            competencyPersonProjectAssessment.ResidentComments = tbResidentComments.Text;

            if ( !competencyPersonProjectAssessment.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            residencyContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["CompetencyPersonProjectAssessmentId"] = competencyPersonProjectAssessment.Id.ToString();
            NavigateToPage( this.RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="competencyPersonProjectAssessmentId">The competency person project assessment identifier.</param>
        public void ShowDetail( int competencyPersonProjectAssessmentId )
        {
            ShowDetail( competencyPersonProjectAssessmentId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="competencyPersonProjectAssessmentId">The competency person project assessment identifier.</param>
        /// <param name="competencyPersonProjectId">The competency person project id.</param>
        public void ShowDetail( int competencyPersonProjectAssessmentId, int? competencyPersonProjectId )
        {
            pnlDetails.Visible = true;
            var residencyContext = new ResidencyContext();

            // Load depending on Add(0) or Edit
            CompetencyPersonProjectAssessment competencyPersonProjectAssessment = null;
            if ( !competencyPersonProjectAssessmentId.Equals( 0 ) )
            {
                competencyPersonProjectAssessment = new ResidencyService<CompetencyPersonProjectAssessment>( residencyContext ).Get( competencyPersonProjectAssessmentId );
            }
            
            if ( competencyPersonProjectAssessment == null )
            {
                competencyPersonProjectAssessment = new CompetencyPersonProjectAssessment { Id = 0 };
                competencyPersonProjectAssessment.CompetencyPersonProjectId = competencyPersonProjectId ?? 0;
                competencyPersonProjectAssessment.CompetencyPersonProject
                    = new ResidencyService<CompetencyPersonProject>( residencyContext ).Get( competencyPersonProjectAssessment.CompetencyPersonProjectId );
            }

            hfCompetencyPersonProjectAssessmentId.Value = competencyPersonProjectAssessment.Id.ToString();
            hfCompetencyPersonProjectId.Value = competencyPersonProjectAssessment.CompetencyPersonProjectId.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( CompetencyPersonProjectAssessment.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                ShowReadonlyDetails( competencyPersonProjectAssessment );
            }
            else
            {
                btnEdit.Visible = true;
                if ( competencyPersonProjectAssessment.Id > 0 )
                {
                    ShowReadonlyDetails( competencyPersonProjectAssessment );
                }
                else
                {
                    ShowEditDetails( competencyPersonProjectAssessment );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="competencyPersonProjectAssessment">The competency person project assessment.</param>
        private void ShowEditDetails( CompetencyPersonProjectAssessment competencyPersonProjectAssessment )
        {
            lReadOnlyTitle.Text = string.Format("Assessment for {0}", competencyPersonProjectAssessment.CompetencyPersonProject.Project.Name).FormatAsHtmlTitle();

            SetEditMode( true );

            lblEditDetails.Text = new DescriptionList()
                .Add( "Resident", competencyPersonProjectAssessment.CompetencyPersonProject.CompetencyPerson.Person )
                .Add( "Competency", competencyPersonProjectAssessment.CompetencyPersonProject.CompetencyPerson.Competency.Name )
                .Add( "Project", string.Format( "{0} - {1}", competencyPersonProjectAssessment.CompetencyPersonProject.Project.Name, competencyPersonProjectAssessment.CompetencyPersonProject.Project.Description ) )
                .Html;

            ppAssessor.SetValue( competencyPersonProjectAssessment.AssessorPerson );
            dtpAssessmentDateTime.SelectedDateTime = competencyPersonProjectAssessment.AssessmentDateTime;
            lblOverallRating.Text = competencyPersonProjectAssessment.OverallRating.ToString();
            tbRatingNotes.Text = competencyPersonProjectAssessment.RatingNotes;
            tbResidentComments.Text = competencyPersonProjectAssessment.ResidentComments;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="competencyPersonProjectAssessment">The competency person project assessment.</param>
        private void ShowReadonlyDetails( CompetencyPersonProjectAssessment competencyPersonProjectAssessment )
        {
            lReadOnlyTitle.Text = string.Format( "Assessment for {0}", competencyPersonProjectAssessment.CompetencyPersonProject.Project.Name ).FormatAsHtmlTitle();
            
            SetEditMode( false );

            string projectText = string.Format( "{0} - {1}", competencyPersonProjectAssessment.CompetencyPersonProject.Project.Name, competencyPersonProjectAssessment.CompetencyPersonProject.Project.Description );

            lblMainDetailsCol1.Text = new DescriptionList()
                .Add( "Resident", competencyPersonProjectAssessment.CompetencyPersonProject.CompetencyPerson.Person )
                .Add( "Competency", competencyPersonProjectAssessment.CompetencyPersonProject.Project.Competency.Name )
                .Add( "Project", projectText ).Html;

            lblMainDetailsCol2.Text = new DescriptionList()
                .Add( "Assessor", competencyPersonProjectAssessment.AssessorPerson )
                .Add( "Assessment Date/Time", competencyPersonProjectAssessment.AssessmentDateTime )
                .Add( "Rating", competencyPersonProjectAssessment.OverallRating.ToString() )
                .Html;
        }

        #endregion
    }
}