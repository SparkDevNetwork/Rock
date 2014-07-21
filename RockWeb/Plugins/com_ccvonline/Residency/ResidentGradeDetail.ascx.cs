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
using System.Web.UI;
using System.Linq;
using com.ccvonline.Residency.Data;
using com.ccvonline.Residency.Model;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;
using System.Web.UI.WebControls;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;
using System.ComponentModel;

namespace RockWeb.Plugins.com_ccvonline.Residency
{
    [DisplayName( "Project Grading Form" )]
    [Category( "CCV > Residency" )]
    [Description( "Form for grading a project." )]

    [LinkedPage( "Person Project Detail Page" )]
    public partial class ResidentGradeDetail : RockBlock, IDetailBlock
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
                ShowDetail( hfCompetencyPersonProjectId.ValueAsInt() );
            }

            // minimize the chance of using the Browser Back button to accidently "re-grade" the project after the residentGraderSessionKey has expired
            Page.Response.Cache.SetCacheability( System.Web.HttpCacheability.NoCache );
            Page.Response.Cache.SetExpires( DateTime.UtcNow.AddHours( -1 ) );
            Page.Response.Cache.SetNoStore();
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
                    breadCrumbs.Add( new BreadCrumb( competencyPersonProject.Project.Name + " - Grade", pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "Project - Grade", pageReference ) );
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
            NavigateToGraderLogin();
        }

        /// <summary>
        /// Navigates to grader login.
        /// </summary>
        private void NavigateToGraderLogin()
        {
            Dictionary<string, string> qryString = new Dictionary<string, string>();
            qryString["CompetencyPersonProjectId"] = hfCompetencyPersonProjectId.Value;
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
            CompetencyPersonProject competencyPersonProject;
            ResidencyService<CompetencyPersonProject> competencyPersonProjectService = new ResidencyService<CompetencyPersonProject>( residencyContext );

            CompetencyPersonProjectAssessment competencyPersonProjectAssessment;
            ResidencyService<CompetencyPersonProjectAssessment> competencyPersonProjectAssessmentService = new ResidencyService<CompetencyPersonProjectAssessment>( residencyContext );

            ResidencyService<CompetencyPersonProjectAssessmentPointOfAssessment> competencyPersonProjectAssessmentPointOfAssessmentService = new ResidencyService<CompetencyPersonProjectAssessmentPointOfAssessment>( residencyContext );

            int competencyPersonProjectId = hfCompetencyPersonProjectId.ValueAsInt();
            if ( competencyPersonProjectId == 0 )
            {
                // shouldn't happen
                competencyPersonProject = new CompetencyPersonProject();
                competencyPersonProjectService.Add( competencyPersonProject );
                residencyContext.SaveChanges();
            }
            else
            {
                competencyPersonProject = competencyPersonProjectService.Get( competencyPersonProjectId );
            }

            int competencyPersonProjectAssessmentId = hfCompetencyPersonProjectAssessmentId.ValueAsInt();
            if ( competencyPersonProjectAssessmentId == 0 )
            {
                competencyPersonProjectAssessment = new CompetencyPersonProjectAssessment();
                competencyPersonProjectAssessmentService.Add( competencyPersonProjectAssessment );
            }
            else
            {
                competencyPersonProjectAssessment = competencyPersonProjectAssessmentService.Get( competencyPersonProjectAssessmentId );
                competencyPersonProjectAssessment.CompetencyPersonProjectAssessmentPointOfAssessments = new List<CompetencyPersonProjectAssessmentPointOfAssessment>();
            }

            // set competencyPersonProjectAssessment.CompetencyPersonProjectId after saving competencyPersonProject in case it is new
            competencyPersonProjectAssessment.AssessmentDateTime = DateTime.Now;
            competencyPersonProjectAssessment.RatingNotes = tbRatingNotesOverall.Text;
            competencyPersonProjectAssessment.AssessorPersonId = hfAssessorPersonId.ValueAsInt();

            if ( !competencyPersonProjectAssessment.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            List<CompetencyPersonProjectAssessmentPointOfAssessment> competencyPersonProjectAssessmentPointOfAssessmentList = new List<CompetencyPersonProjectAssessmentPointOfAssessment>();

            foreach ( RepeaterItem item in rptPointOfAssessment.Items.OfType<RepeaterItem>() )
            {
                HiddenField hfProjectPointOfAssessmentId = item.FindControl( "hfProjectPointOfAssessmentId" ) as HiddenField;
                int projectPointOfAssessmentId = hfProjectPointOfAssessmentId.ValueAsInt();

                CompetencyPersonProjectAssessmentPointOfAssessment competencyPersonProjectAssessmentPointOfAssessment = competencyPersonProjectAssessmentPointOfAssessmentService.Queryable()
                    .Where( a => a.ProjectPointOfAssessmentId == projectPointOfAssessmentId )
                    .Where( a => a.CompetencyPersonProjectAssessmentId == competencyPersonProjectAssessmentId ).FirstOrDefault();

                if ( competencyPersonProjectAssessmentPointOfAssessment == null )
                {
                    competencyPersonProjectAssessmentPointOfAssessment = new CompetencyPersonProjectAssessmentPointOfAssessment();
                    //// set competencyPersonProjectAssessmentPointOfAssessment.CompetencyPersonProjectAssessmentId = competencyPersonProjectAssessment.Id in save in case it's new
                    competencyPersonProjectAssessmentPointOfAssessment.ProjectPointOfAssessmentId = projectPointOfAssessmentId;
                    competencyPersonProjectAssessmentPointOfAssessment.ProjectPointOfAssessment = new ResidencyService<ProjectPointOfAssessment>( residencyContext ).Get( projectPointOfAssessmentId );
                }

                RockDropDownList ddlPointOfAssessmentRating = item.FindControl( "ddlPointOfAssessmentRating" ) as RockDropDownList;
                RockCheckBox ckPointOfAssessmentPassFail = item.FindControl( "ckPointOfAssessmentPassFail" ) as RockCheckBox;
                TextBox tbRatingNotesPOA = item.FindControl( "tbRatingNotesPOA" ) as TextBox;

                if ( competencyPersonProjectAssessmentPointOfAssessment.ProjectPointOfAssessment.IsPassFail )
                {
                    // Pass = 4, Fail = 1 (even though 5 is the highest)
                    competencyPersonProjectAssessmentPointOfAssessment.Rating = ckPointOfAssessmentPassFail.Checked ? 4 : 1;
                }
                else
                {
                    competencyPersonProjectAssessmentPointOfAssessment.Rating = ddlPointOfAssessmentRating.SelectedValueAsInt();
                }
                
                competencyPersonProjectAssessmentPointOfAssessment.RatingNotes = tbRatingNotesPOA.Text;

                competencyPersonProjectAssessmentPointOfAssessmentList.Add( competencyPersonProjectAssessmentPointOfAssessment );
            }

            residencyContext.WrapTransaction( () =>
            {
                competencyPersonProjectAssessment.CompetencyPersonProjectId = competencyPersonProject.Id;

                // set Overall Rating based on average of POA ratings
                competencyPersonProjectAssessment.OverallRating = (decimal?)competencyPersonProjectAssessmentPointOfAssessmentList.Average( a => a.Rating );

                // Save changes first to make sure we have a valid competencyPersonProject.Id
                residencyContext.SaveChanges();
                foreach ( var competencyPersonProjectAssessmentPointOfAssessment in competencyPersonProjectAssessmentPointOfAssessmentList )
                {
                    competencyPersonProjectAssessmentPointOfAssessment.CompetencyPersonProjectAssessmentId = competencyPersonProjectAssessment.Id;

                    if ( competencyPersonProjectAssessmentPointOfAssessment.Id == 0 )
                    {
                        competencyPersonProjectAssessmentPointOfAssessmentService.Add( competencyPersonProjectAssessmentPointOfAssessment );
                    }
                }

                residencyContext.SaveChanges();
            } );

            Guid personProjectDetailPageGuid = ( this.GetAttributeValue( "PersonProjectDetailPage" ) ?? string.Empty ).AsGuid();

            var page = PageCache.Read( personProjectDetailPageGuid );

            if ( page != null )
            {
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["CompetencyPersonProjectId"] = hfCompetencyPersonProjectId.Value;
                NavigateToPage( page.Guid, qryString );
            }
            else
            {
                throw new Exception( "PersonProjectDetailPage not configured correctly" );
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="competencyPersonProjectId">The competency person project identifier.</param>
        public void ShowDetail( int itemId )
        {
            pnlDetails.Visible = true;

            hfCompetencyPersonProjectId.Value = this.PageParameter( "CompetencyPersonProjectId" );
            int competencyPersonProjectId = hfCompetencyPersonProjectId.ValueAsInt();

            // first try to get the key from the url (in case it from an emailed request)
            string emailedKey = Server.UrlDecode( this.PageParameter( "GradeKey" ) );

            if ( !string.IsNullOrWhiteSpace( emailedKey ) )
            {
                // if they got here from email, put the key in session, and reload page without the key in the url to minimize chance of problems
                Session["ResidentGraderSessionKey"] = emailedKey;
                Dictionary<string, string> qryParams = new Dictionary<string, string>();
                qryParams.Add( "CompetencyPersonProjectId", competencyPersonProjectId.ToString() );
                NavigateToPage( this.RockPage.Guid, qryParams );
                return;
            }

            string encryptedKey = Session["ResidentGraderSessionKey"] as string;

            // clear the residentGraderSessionKey so they don't accidently grade this again with a stale grader login
            Session["ResidentGraderSessionKey"] = null;

            string residentGraderSessionKey = string.Empty;

            if ( !string.IsNullOrWhiteSpace( encryptedKey ) )
            {
                residentGraderSessionKey = Rock.Security.Encryption.DecryptString( encryptedKey );
            }

            string[] residentGraderSessionKeyParts = residentGraderSessionKey.Split( new char[] { '|' } );

            Person assessorPerson = null;
            bool fromEmailedRequest = false;

            // verify that the residentGraderSessionKey is for this Project, has a valid Person, and isn't stale (helps minimize the chance of incorrect teacher from a previous teacher login)
            if ( residentGraderSessionKeyParts.Length == 3 )
            {
                string userLoginGuid = residentGraderSessionKeyParts[1];
                if ( residentGraderSessionKeyParts[0].Equals( competencyPersonProjectId.ToString() ) )
                {
                    //// a live (via Facilitator Login) request goes stale if it has been 10 minutes between the Login and this page loading
                    //// an emailed request just sets Ticks to 0 since we can't really do anything to prevent it

                    assessorPerson = new UserLoginService( new Rock.Data.RockContext() ).Get( new Guid( userLoginGuid ) ).Person;

                    string ticks = residentGraderSessionKeyParts[2];
                    if ( ticks.Equals( "0" ) )
                    {
                        // if ticks is 0, that means this is from an emailed request.
                        fromEmailedRequest = true;
                    }
                    else
                    {
                        TimeSpan elapsed = DateTime.Now - new DateTime( long.Parse( ticks ) );
                        if ( elapsed.TotalMinutes > 10 )
                        {
                            // somehow, the session key is stale, so don't trust it
                            assessorPerson = null;
                        }
                    }
                }
            }

            if ( assessorPerson == null )
            {
                NavigateToGraderLogin();
                return;
            }

            int assessorPersonId = assessorPerson.Id;
            hfAssessorPersonId.Value = assessorPerson.Id.ToString();

            var residencyContext = new ResidencyContext();

            CompetencyPersonProject competencyPersonProject = new ResidencyService<CompetencyPersonProject>( residencyContext ).Get( competencyPersonProjectId );

            if ( competencyPersonProject.CompetencyPerson.PersonId != CurrentPersonId )
            {
                if ( !fromEmailedRequest )
                {
                    // somebody besides the Resident is logged in and this isn't an emailed grade request
                    NavigateToParentPage();
                    return;
                }
            }

            hfCompetencyPersonProjectId.Value = competencyPersonProject.Id.ToString();

            competencyPersonProject.CompetencyPersonProjectAssessments = competencyPersonProject.CompetencyPersonProjectAssessments ?? new List<CompetencyPersonProjectAssessment>();

            // look for an incomplete assessment, or start a new one
            CompetencyPersonProjectAssessment competencyPersonProjectAssessment = competencyPersonProject.CompetencyPersonProjectAssessments
                .Where( a => a.AssessmentDateTime == null ).FirstOrDefault();

            if ( competencyPersonProjectAssessment == null )
            {
                competencyPersonProjectAssessment = new CompetencyPersonProjectAssessment
                {
                    CompetencyPersonProject = competencyPersonProject
                };
            }

            hfCompetencyPersonProjectAssessmentId.Value = competencyPersonProjectAssessment.Id.ToString();

            // populate page
            lblMainDetails.Text = new DescriptionList()
                .Add( "Student", competencyPersonProject.CompetencyPerson.Person )
                .Add( "Competency", competencyPersonProject.CompetencyPerson.Competency.Name )
                .Add( "Project", string.Format( "{0} - {1}", competencyPersonProject.Project.Name, competencyPersonProject.Project.Description ) )
                .Html;

            List<ProjectPointOfAssessment> projectPointOfAssessmentList = new ResidencyService<ProjectPointOfAssessment>( residencyContext ).Queryable()
                .Where( a => a.ProjectId == competencyPersonProject.ProjectId ).ToList();

            // get any POA Ratings that might exist
            List<CompetencyPersonProjectAssessmentPointOfAssessment> competencyPersonProjectAssessmentPointOfAssessmentList = new ResidencyService<CompetencyPersonProjectAssessmentPointOfAssessment>( residencyContext ).Queryable()
                .Where( a => a.CompetencyPersonProjectAssessmentId == competencyPersonProjectAssessment.Id ).ToList();

            var competencyPersonProjectAssessmentPointOfAssessmentListJoined = from projectPointOfAssessment in projectPointOfAssessmentList
                                                                               join poa in competencyPersonProjectAssessmentPointOfAssessmentList
                                                                               on projectPointOfAssessment.Id equals poa.ProjectPointOfAssessmentId into groupJoin
                                                                               from qryResult in groupJoin.DefaultIfEmpty()
                                                                               select ( qryResult ?? new CompetencyPersonProjectAssessmentPointOfAssessment
                                                                                 {
                                                                                     ProjectPointOfAssessmentId = projectPointOfAssessment.Id,
                                                                                     ProjectPointOfAssessment = projectPointOfAssessment,
                                                                                     CompetencyPersonProjectAssessmentId = competencyPersonProjectAssessment.Id,
                                                                                     CompetencyPersonProjectAssessment = competencyPersonProjectAssessment
                                                                                 } );

            rptPointOfAssessment.DataSource = competencyPersonProjectAssessmentPointOfAssessmentListJoined.OrderBy( a => a.ProjectPointOfAssessment.AssessmentOrder ).ToList();
            rptPointOfAssessment.DataBind();

            tbRatingNotesOverall.Text = competencyPersonProjectAssessment.RatingNotes;
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptPointOfAssessment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPointOfAssessment_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            CompetencyPersonProjectAssessmentPointOfAssessment competencyPersonProjectAssessmentPointOfAssessment = e.Item.DataItem as CompetencyPersonProjectAssessmentPointOfAssessment;
            if ( competencyPersonProjectAssessmentPointOfAssessment != null )
            {
                RockDropDownList ddlPointOfAssessmentRating = e.Item.FindControl( "ddlPointOfAssessmentRating" ) as RockDropDownList;
                ddlPointOfAssessmentRating.Items.Clear();
                ddlPointOfAssessmentRating.Items.Add( new ListItem( "-", Rock.Constants.None.IdValue ) );
                for ( int ratingOption = 1; ratingOption <= 5; ratingOption++ )
                {
                    ddlPointOfAssessmentRating.Items.Add( new ListItem( ratingOption.ToString(), ratingOption.ToString() ) );
                }

                ddlPointOfAssessmentRating.SetValue( competencyPersonProjectAssessmentPointOfAssessment.Rating.ToString() );
                ddlPointOfAssessmentRating.Visible = !competencyPersonProjectAssessmentPointOfAssessment.ProjectPointOfAssessment.IsPassFail;
                HiddenField hfProjectPointOfAssessmentId = e.Item.FindControl( "hfProjectPointOfAssessmentId" ) as HiddenField;

                RockCheckBox ckPointOfAssessmentPassFail = e.Item.FindControl( "ckPointOfAssessmentPassFail" ) as RockCheckBox;
                ckPointOfAssessmentPassFail.Visible = competencyPersonProjectAssessmentPointOfAssessment.ProjectPointOfAssessment.IsPassFail;
                ckPointOfAssessmentPassFail.Checked = competencyPersonProjectAssessmentPointOfAssessment.Rating == 5;

                hfProjectPointOfAssessmentId.Value = competencyPersonProjectAssessmentPointOfAssessment.ProjectPointOfAssessmentId.ToString();

                Literal lblAssessmentText = e.Item.FindControl( "lblAssessmentText" ) as Literal;
                lblAssessmentText.Text = string.Format(
                    "{0}. {1}",
                    competencyPersonProjectAssessmentPointOfAssessment.ProjectPointOfAssessment.AssessmentOrder,
                    competencyPersonProjectAssessmentPointOfAssessment.ProjectPointOfAssessment.AssessmentText );

                TextBox tbRatingNotesPOA = e.Item.FindControl( "tbRatingNotesPOA" ) as TextBox;
                tbRatingNotesPOA.Text = competencyPersonProjectAssessmentPointOfAssessment.RatingNotes;
            }
        }

        #endregion
    }
}