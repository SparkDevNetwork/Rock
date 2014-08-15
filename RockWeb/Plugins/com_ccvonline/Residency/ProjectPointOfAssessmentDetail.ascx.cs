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
using com.ccvonline.Residency.Data;
using com.ccvonline.Residency.Model;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_ccvonline.Residency
{
    [DisplayName( "Point of Assessment Detail" )]
    [Category( "CCV > Residency" )]
    [Description( "Displays the details of a project's point of assessment." )]

    public partial class ProjectPointOfAssessmentDetail : RockBlock, IDetailBlock
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
                int? projectPointOfAssessmentId = PageParameter( "ProjectPointOfAssessmentId" ).AsIntegerOrNull();
                if ( projectPointOfAssessmentId.HasValue )
                {
                    ShowDetail( projectPointOfAssessmentId.Value, PageParameter( "ProjectId" ).AsIntegerOrNull() );
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

            int? projectPointOfAssessmentId = this.PageParameter( pageReference, "ProjectPointOfAssessmentId" ).AsInteger();
            if ( projectPointOfAssessmentId != null )
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
            Dictionary<string, string> qryString = new Dictionary<string, string>();
            qryString["ProjectId"] = hfProjectId.Value;
            NavigateToParentPage( qryString );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var residencyContext = new ResidencyContext();
            ProjectPointOfAssessment projectPointOfAssessment;
            ResidencyService<ProjectPointOfAssessment> projectPointOfAssessmentService = new ResidencyService<ProjectPointOfAssessment>( residencyContext );

            int projectPointOfAssessmentId = int.Parse( hfProjectPointOfAssessmentId.Value );

            if ( projectPointOfAssessmentId == 0 )
            {
                projectPointOfAssessment = new ProjectPointOfAssessment();
                projectPointOfAssessment.AssessmentOrder = lblAssessmentOrder.Text.AsInteger();
                projectPointOfAssessment.ProjectId = hfProjectId.ValueAsInt();
                projectPointOfAssessmentService.Add( projectPointOfAssessment );
            }
            else
            {
                projectPointOfAssessment = projectPointOfAssessmentService.Get( projectPointOfAssessmentId );
            }

            projectPointOfAssessment.PointOfAssessmentTypeValueId = ddlPointOfAssessmentTypeValue.SelectedValueAsInt();
            projectPointOfAssessment.AssessmentText = tbAssessmentText.Text;
            projectPointOfAssessment.IsPassFail = cbIsPassFail.Checked;

            if ( !projectPointOfAssessment.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            residencyContext.SaveChanges();

            Dictionary<string, string> qryString = new Dictionary<string, string>();
            qryString["ProjectId"] = hfProjectId.Value;
            NavigateToParentPage( qryString );
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            var definedTypeCache = DefinedTypeCache.Read( com.ccvonline.Residency.SystemGuid.DefinedType.RESIDENCY_POINT_OF_ASSESSMENT_TYPE.AsGuid() );

            var list = definedTypeCache.DefinedValues.OrderBy( a => a.Name ).ToList();
            foreach ( var item in list )
            {
                ddlPointOfAssessmentTypeValue.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
            }

            ddlPointOfAssessmentTypeValue.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="projectPointOfAssessmentId">The project point of assessment identifier.</param>
        public void ShowDetail( int projectPointOfAssessmentId )
        {
            ShowDetail( projectPointOfAssessmentId, null );
        }

        /// <summary>
        /// Shows the detail
        /// </summary>
        /// <param name="projectPointOfAssessmentId">The project point of assessment identifier.</param>
        /// <param name="projectId">The residency project id.</param>
        public void ShowDetail( int projectPointOfAssessmentId, int? projectId )
        {
            pnlDetails.Visible = true;

            LoadDropDowns();

            var residencyContext = new ResidencyContext();

            // Load depending on Add(0) or Edit
            ProjectPointOfAssessment projectPointOfAssessment = null;
            var projectPointOfAssessmentService = new ResidencyService<ProjectPointOfAssessment>( residencyContext );

            string projectName = new ResidencyService<Project>( residencyContext ).Queryable()
                .Where( a => a.Id.Equals( projectId.Value ) )
                .Select( a => a.Name ).FirstOrDefault();

            if ( !projectPointOfAssessmentId.Equals( 0 ) )
            {
                projectPointOfAssessment = projectPointOfAssessmentService.Get( projectPointOfAssessmentId );
            }
            else
            {
                // don't try add if there wasn't a projectId specified
                if ( projectId != null )
                {
                    projectPointOfAssessment = new ProjectPointOfAssessment { Id = 0, ProjectId = projectId.Value };

                    int maxAssessmentOrder = projectPointOfAssessmentService.Queryable()
                        .Where( a => a.ProjectId.Equals( projectPointOfAssessment.ProjectId ) )
                        .Select( a => a.AssessmentOrder ).DefaultIfEmpty( 0 ).Max();

                    projectPointOfAssessment.AssessmentOrder = maxAssessmentOrder + 1;
                }
            }

            if ( projectPointOfAssessment == null )
            {
                return;
            }

            lReadOnlyTitle.Text = ( "Point of Assessment " + projectPointOfAssessment.AssessmentOrder.ToString() ).FormatAsHtmlTitle();

            hfProjectPointOfAssessmentId.Value = projectPointOfAssessment.Id.ToString();
            hfProjectId.Value = projectId.ToString();
            lblAssessmentOrder.Text = projectPointOfAssessment.AssessmentOrder.ToString();
            tbAssessmentText.Text = projectPointOfAssessment.AssessmentText;
            ddlPointOfAssessmentTypeValue.SetValue( projectPointOfAssessment.PointOfAssessmentTypeValueId );
            cbIsPassFail.Checked = projectPointOfAssessment.IsPassFail;

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( "Point of Assessment" );
            }

            if ( readOnly )
            {
                btnCancel.Text = "Close";
            }

            tbAssessmentText.ReadOnly = readOnly;
            btnSave.Visible = !readOnly;
        }

        #endregion
    }
}