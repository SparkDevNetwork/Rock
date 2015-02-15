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
    [DisplayName( "Project Detail" )]
    [Category( "CCV > Residency" )]
    [Description( "Displays the details of a project." )]

    public partial class ProjectDetail : RockBlock, IDetailBlock
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
                string projectId = PageParameter( "ProjectId" );
                if ( !string.IsNullOrWhiteSpace( projectId ) )
                {
                    ShowDetail( projectId.AsInteger(), PageParameter( "CompetencyId" ).AsIntegerOrNull() );
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

            int? projectId = this.PageParameter( pageReference, "ProjectId" ).AsInteger();
            if ( projectId != null )
            {
                Project project = new ResidencyService<Project>( new ResidencyContext() ).Get( projectId.Value );
                if ( project != null )
                {
                    breadCrumbs.Add( new BreadCrumb( project.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "Project", pageReference ) );
                }
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

            if ( hfProjectId.ValueAsInt().Equals( 0 ) )
            {
                // Cancelling on Add.  Return to Grid
                // if this page was called from the Competency Detail page, return to that
                string competencyId = PageParameter( "CompetencyId" );
                if ( !string.IsNullOrWhiteSpace( competencyId ) )
                {
                    Dictionary<string, string> qryString = new Dictionary<string, string>();
                    qryString["CompetencyId"] = competencyId;
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
                ResidencyService<Project> service = new ResidencyService<Project>( new ResidencyContext() );
                Project item = service.Get( hfProjectId.ValueAsInt() );
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
            ResidencyService<Project> service = new ResidencyService<Project>( new ResidencyContext() );
            Project item = service.Get( hfProjectId.ValueAsInt() );
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

            Project project;
            ResidencyService<Project> projectService = new ResidencyService<Project>( residencyContext );
            ResidencyService<ProjectPointOfAssessment> projectPointOfAssessmentService = new ResidencyService<ProjectPointOfAssessment>( residencyContext );

            int projectId = int.Parse( hfProjectId.Value );

            if ( projectId == 0 )
            {
                project = new Project();
                projectService.Add( project );
            }
            else
            {
                project = projectService.Get( projectId );
            }

            project.Name = tbName.Text;
            project.Description = tbDescription.Text;
            project.CompetencyId = hfCompetencyId.ValueAsInt();
            project.MinAssessmentCountDefault = tbMinAssessmentCountDefault.Text.AsIntegerOrNull();

            if ( !project.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            if ( hfCloneFromProjectId.ValueAsInt() > 0 )
            {
                var cloneProject = projectService.Get( hfCloneFromProjectId.ValueAsInt() );
                foreach ( var projectPointOfAssessment in cloneProject.ProjectPointOfAssessments )
                {
                    var newProjectPointOfAssessment = new ProjectPointOfAssessment
                    {
                        PointOfAssessmentTypeValueId = projectPointOfAssessment.PointOfAssessmentTypeValueId,
                        AssessmentOrder = projectPointOfAssessment.AssessmentOrder,
                        AssessmentText = projectPointOfAssessment.AssessmentText,
                        IsPassFail = projectPointOfAssessment.IsPassFail
                    };

                    projectPointOfAssessmentService.Add( newProjectPointOfAssessment );
                }
            }

            residencyContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["ProjectId"] = project.Id.ToString();
            NavigateToPage( this.RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        public void ShowDetail( int projectId )
        {
            ShowDetail( projectId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="competencyId">The residency competency id.</param>
        public void ShowDetail( int projectId, int? competencyId )
        {
            pnlDetails.Visible = true;

            Project project = null;

            var residencyContext = new ResidencyContext();

            if ( !projectId.Equals( 0 ) )
            {
                project = new ResidencyService<Project>( residencyContext ).Get( projectId );
            }
            
            if ( project == null )
            {
                project = new Project { Id = 0 };
                project.CompetencyId = competencyId ?? 0;
                project.Competency = new ResidencyService<Competency>( residencyContext ).Get( project.CompetencyId );
            }

            hfProjectId.Value = project.Id.ToString();
            hfCloneFromProjectId.Value = ( PageParameter( "CloneFromProjectId" ).AsInteger() ).ToString();
            hfCompetencyId.Value = project.CompetencyId.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Project.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                ShowReadonlyDetails( project );
            }
            else
            {
                btnEdit.Visible = true;
                if ( project.Id > 0 )
                {
                    ShowReadonlyDetails( project );
                }
                else
                {
                    ShowEditDetails( project );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="project">The residency project.</param>
        private void ShowEditDetails( Project project )
        {
            if ( project.Id == 0 )
            {
                if ( hfCloneFromProjectId.ValueAsInt() > 0 )
                {
                    var cloneProject = new ResidencyService<Project>( new ResidencyContext() ).Get( hfCloneFromProjectId.ValueAsInt() );
                    string title = string.Format( "Clone project from {0}", cloneProject.Name );
                    nbCloneMessage.Text = string.Format( "This will add a new project, and copy all the points of assessments from {0}", cloneProject.Name );
                    lReadOnlyTitle.Text = title.FormatAsHtmlTitle();
                }
                else
                {
                    lReadOnlyTitle.Text = ActionTitle.Add( Project.FriendlyTypeName ).FormatAsHtmlTitle();
                }
            }
            else
            {
                lReadOnlyTitle.Text = project.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbName.Text = project.Name;
            tbDescription.Text = project.Description;
            lblPeriod.Text = project.Competency.Track.Period.Name;
            lblTrack.Text = project.Competency.Track.Name;
            lblCompetency.Text = project.Competency.Name;
            tbMinAssessmentCountDefault.Text = project.MinAssessmentCountDefault.ToString();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="project">The residency project.</param>
        private void ShowReadonlyDetails( Project project )
        {
            lReadOnlyTitle.Text = project.Name.FormatAsHtmlTitle();

            SetEditMode( false );

            lblMainDetails.Text = new DescriptionList()
                .Add( "Description", project.Description )
                .Add( "Competency", project.Competency.Name )
                .Add( "Default # of Assessments", project.MinAssessmentCountDefault )
                .Html;
        }

        #endregion
    }
}