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
    [DisplayName( "Resident Project Detail" )]
    [Category( "CCV > Residency" )]
    [Description( "Displays the details of a resident project." )]

    public partial class CompetencyPersonProjectDetail : RockBlock, IDetailBlock
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
                string CompetencyPersonProjectId = PageParameter( "CompetencyPersonProjectId" );
                if ( !string.IsNullOrWhiteSpace( CompetencyPersonProjectId ) )
                {
                    ShowDetail( CompetencyPersonProjectId.AsInteger(), PageParameter( "CompetencyPersonId" ).AsIntegerOrNull() );
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

            int? competencyPersonProjectId = this.PageParameter( pageReference, "CompetencyPersonProjectId" ).AsInteger();
            if ( competencyPersonProjectId != null )
            {
                CompetencyPersonProject competencyPersonProject = new ResidencyService<CompetencyPersonProject>( new ResidencyContext() ).Get( competencyPersonProjectId.Value );
                if ( competencyPersonProject != null )
                {
                    breadCrumbs.Add( new BreadCrumb( competencyPersonProject.Project.Name, pageReference ) );
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

            if ( hfCompetencyPersonProjectId.ValueAsInt().Equals( 0 ) )
            {
                // Cancelling on Add.  Return to Grid
                // if this page was called from the CompetencyPerson Detail page, return to that
                string competencyPersonId = PageParameter( "CompetencyPersonId" );
                if ( !string.IsNullOrWhiteSpace( competencyPersonId ) )
                {
                    Dictionary<string, string> qryString = new Dictionary<string, string>();
                    qryString["CompetencyPersonId"] = competencyPersonId;
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
                ResidencyService<CompetencyPersonProject> service = new ResidencyService<CompetencyPersonProject>( new ResidencyContext() );
                CompetencyPersonProject item = service.Get( hfCompetencyPersonProjectId.ValueAsInt() );
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
            ResidencyService<CompetencyPersonProject> service = new ResidencyService<CompetencyPersonProject>( new ResidencyContext() );
            CompetencyPersonProject item = service.Get( hfCompetencyPersonProjectId.ValueAsInt() );
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
            CompetencyPersonProject competencyPersonProject;
            ResidencyService<CompetencyPersonProject> competencyPersonProjectService = new ResidencyService<CompetencyPersonProject>( residencyContext );

            int competencyPersonProjectId = int.Parse( hfCompetencyPersonProjectId.Value );

            if ( competencyPersonProjectId == 0 )
            {
                competencyPersonProject = new CompetencyPersonProject();
                competencyPersonProjectService.Add( competencyPersonProject );

                // these inputs are only editable on Add
                competencyPersonProject.ProjectId = ddlProject.SelectedValueAsInt() ?? 0;
                competencyPersonProject.CompetencyPersonId = hfCompetencyPersonId.ValueAsInt();
            }
            else
            {
                competencyPersonProject = competencyPersonProjectService.Get( competencyPersonProjectId );
            }

            if ( !string.IsNullOrWhiteSpace( tbMinAssessmentCountOverride.Text ) )
            {
                competencyPersonProject.MinAssessmentCount = tbMinAssessmentCountOverride.Text.AsInteger();
            }
            else
            {
                competencyPersonProject.MinAssessmentCount = null;
            }

            if ( !competencyPersonProject.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            residencyContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["CompetencyPersonProjectId"] = competencyPersonProject.Id.ToString();
            NavigateToPage( this.RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="competencyPersonProjectId">The competency person project identifier.</param>
        public void ShowDetail( int competencyPersonProjectId )
        {
            ShowDetail( competencyPersonProjectId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="competencyPersonProjectId">The competency person project identifier.</param>
        /// <param name="competencyPersonId">The competency person id.</param>
        public void ShowDetail( int competencyPersonProjectId, int? competencyPersonId )
        {
            var residencyContext = new ResidencyContext();

            pnlDetails.Visible = true;

            // Load depending on Add(0) or Edit
            CompetencyPersonProject competencyPersonProject = null;
            if ( !competencyPersonProjectId.Equals( 0 ) )
            {
                competencyPersonProject = new ResidencyService<CompetencyPersonProject>( residencyContext ).Get( competencyPersonProjectId );
            }
            
            if ( competencyPersonProject == null )
            {
                competencyPersonProject = new CompetencyPersonProject { Id = 0 };
                competencyPersonProject.CompetencyPersonId = competencyPersonId ?? 0;
                competencyPersonProject.CompetencyPerson = new ResidencyService<CompetencyPerson>( residencyContext ).Get( competencyPersonProject.CompetencyPersonId );
            }

            hfCompetencyPersonProjectId.Value = competencyPersonProject.Id.ToString();
            hfCompetencyPersonId.Value = competencyPersonProject.CompetencyPersonId.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( CompetencyPersonProject.FriendlyTypeName );
            }

            if ( readOnly )
            {
                ShowReadonlyDetails( competencyPersonProject );
            }
            else
            {
                if ( competencyPersonProject.Id > 0 )
                {
                    ShowReadonlyDetails( competencyPersonProject );
                }
                else
                {
                    ShowEditDetails( competencyPersonProject );
                }
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            var residencyContext = new ResidencyContext();
            var projectQry = new ResidencyService<Project>( residencyContext ).Queryable();

            int competencyPersonId = hfCompetencyPersonId.ValueAsInt();
            CompetencyPerson competencyPerson = new ResidencyService<CompetencyPerson>( residencyContext ).Get( competencyPersonId );
            projectQry = projectQry.Where( a => a.CompetencyId == competencyPerson.CompetencyId );

            // list 
            List<int> assignedProjectIds = new ResidencyService<CompetencyPersonProject>( residencyContext ).Queryable()
                .Where( a => a.CompetencyPersonId.Equals( competencyPersonId ) )
                .Select( a => a.ProjectId ).ToList();

            var list = projectQry.Where( a => !assignedProjectIds.Contains( a.Id ) ).OrderBy( a => a.Name ).ToList();

            ddlProject.DataSource = list;
            ddlProject.DataBind();

            bool addMode = hfCompetencyPersonProjectId.ValueAsInt() == 0;

            if ( addMode )
            {
                // if Adding a project, warn if there are no Projects left to add
                pnlEditProject.Visible = list.Any();
                nbAllProjectsAlreadyAdded.Visible = !list.Any();
                btnSave.Visible = list.Any();
            }
            else
            {
                pnlEditProject.Visible = true;
                nbAllProjectsAlreadyAdded.Visible = false;
                btnSave.Visible = true;
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="competencyPersonProject">The competency person project.</param>
        private void ShowEditDetails( CompetencyPersonProject competencyPersonProject )
        {
            if ( competencyPersonProject.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( CompetencyPersonProject.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = CompetencyPersonProject.FriendlyTypeName;
            }

            LoadDropDowns();

            SetEditMode( true );

            lblPersonName.Text = competencyPersonProject.CompetencyPerson.Person.FullName;
            lblCompetency.Text = competencyPersonProject.CompetencyPerson.Competency.Name;

            if ( competencyPersonProject.MinAssessmentCount != null )
            {
                tbMinAssessmentCountOverride.Text = competencyPersonProject.MinAssessmentCount.ToString();
            }
            else
            {
                tbMinAssessmentCountOverride.Text = string.Empty;
            }

            /* Only allow changing the Project when in Add mode (If this record has already be saved, the child tables (especially assessments) are assuming this project doesn't change) */

            if ( competencyPersonProject.Project != null )
            {
                lblProject.Text = string.Format( "{0} - {1}", competencyPersonProject.Project.Name, competencyPersonProject.Project.Description );
                lblMinAssessmentCountDefault.Text = competencyPersonProject.Project.MinAssessmentCountDefault.ToString();
            }
            else
            {
                // they haven't picked a Project yet, so set the lblMinAssessmentCountDefault.text based on whatever project the ddl defaults to
                ddlProject_SelectedIndexChanged( null, null );

                // shouldn't happen in Edit, but just in case
                lblProject.Text = Rock.Constants.None.Text;
            }

            bool addMode = competencyPersonProject.Id == 0;

            ddlProject.Visible = addMode;
            lblProject.Visible = !addMode;

            lblPersonName.Visible = true;
            lblCompetency.Visible = true;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="competencyPersonProject">The competency person project.</param>
        private void ShowReadonlyDetails( CompetencyPersonProject competencyPersonProject )
        {
            lReadOnlyTitle.Text = competencyPersonProject.Project.Name.FormatAsHtmlTitle();

            SetEditMode( false );

            lblMainDetails.Text = new DescriptionList()
                .Add( "Resident", competencyPersonProject.CompetencyPerson.Person )
                .Add( "Competency", competencyPersonProject.CompetencyPerson.Competency.Name )
                .Add( "Project Description",  competencyPersonProject.Project.Description)
                .Add( "Assessments Required", competencyPersonProject.MinAssessmentCount ?? competencyPersonProject.Project.MinAssessmentCountDefault )
                .Html;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlProject_SelectedIndexChanged( object sender, EventArgs e )
        {
            var project = new ResidencyService<Project>( new ResidencyContext() ).Get( ddlProject.SelectedValueAsInt() ?? 0 );
            if ( project != null )
            {
                lblMinAssessmentCountDefault.Text = project.MinAssessmentCountDefault.ToString();
            }
        }

        #endregion
    }
}