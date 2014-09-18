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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using com.ccvonline.Residency.Data;
using com.ccvonline.Residency.Model;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_ccvonline.Residency
{
    [DisplayName( "Project Grading Request" )]
    [Category( "CCV > Residency" )]
    [Description( "Form where a resident can request that a project be graded." )]

    [LinkedPage( "Resident Grade Detail Page" )]

    [SecurityRoleField( "Residency Grader Security Role", key: "ResidencyGraderSecurityRole" )]
    public partial class ResidentGradeRequest : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            // clear the session residentGraderSessionKey just in case
            Session["ResidentGraderSessionKey"] = string.Empty;

            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                hfCompetencyPersonProjectId.Value = this.PageParameter( "CompetencyPersonProjectId" );

                lFacilitatorLoginTitle.Text = "Facilitator Login".FormatAsHtmlTitle();
                lblEmailRequestTitle.Text = "Email Request to Facilitator".FormatAsHtmlTitle();

                LoadDropDowns();
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
                breadCrumbs.Add( new BreadCrumb( "Grade Request", pageReference ) );
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
        /// Loads the drop downs.
        /// </summary>
        protected void LoadDropDowns()
        {
            var graderRoleGuid = this.GetAttributeValue( "ResidencyGraderSecurityRole" ).AsGuidOrNull();

            List<Person> facilitatorList = new List<Person>();

            if ( graderRoleGuid.HasValue )
            {
                Group residencyGraderSecurityRole = new GroupService( new Rock.Data.RockContext() ).Get( graderRoleGuid.Value );
                if ( residencyGraderSecurityRole != null )
                {
                    foreach ( var groupMember in residencyGraderSecurityRole.Members.ToList().OrderBy( a => a.Person.FullName ) )
                    {
                        facilitatorList.Add( groupMember.Person );
                    }
                }
            }

            CompetencyPersonProject competencyPersonProject = new ResidencyService<CompetencyPersonProject>( new ResidencyContext() ).Get( hfCompetencyPersonProjectId.ValueAsInt() );
            if ( competencyPersonProject != null )
            {
                if ( competencyPersonProject.Project.Competency.TeacherOfRecordPerson != null )
                {
                    // insert the TeacherOfRecordPerson as the first person in the list
                    facilitatorList.Insert( 0, competencyPersonProject.Project.Competency.TeacherOfRecordPerson );
                }
            }

            if ( facilitatorList.Any() )
            {
                nbSendMessage.Text = string.Empty;
                foreach ( var item in facilitatorList )
                {
                    ddlFacilitators.Items.Add( new ListItem( item.ToString(), item.Id.ToString() ) );
                }
            }
            else
            {
                nbSendMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                nbSendMessage.Text = "No facilitators configured";
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnLogin_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                // controls will render messages
                return;
            }

            nbWarningMessage.Text = string.Empty;

            var rockContext = new Rock.Data.RockContext();

            var userLoginService = new UserLoginService( rockContext );
            var userLogin = userLoginService.GetByUserName( tbUserName.Text );
            if ( userLogin != null && userLogin.EntityType != null )
            {
                var component = AuthenticationContainer.GetComponent( userLogin.EntityType.Name );
                if ( component.IsActive && !component.RequiresRemoteAuthentication )
                {
                    if ( component.Authenticate( userLogin, tbPassword.Text ) )
                    {
                        var graderRoleGuid = this.GetAttributeValue( "ResidencyGraderSecurityRole" ).AsGuid();

                        Group residencyGraderSecurityRole = new GroupService( rockContext ).Get( graderRoleGuid );

                        // Grader must either by member of ResidencyGraderSecurityRole or the Teacher of Record for this project's competency
                        bool userAuthorizedToGrade = ( residencyGraderSecurityRole != null ) && residencyGraderSecurityRole.Members.Any( a => a.PersonId == userLogin.PersonId );
                        if ( !userAuthorizedToGrade )
                        {
                            CompetencyPersonProject competencyPersonProject = new ResidencyService<CompetencyPersonProject>( new ResidencyContext() ).Get( hfCompetencyPersonProjectId.ValueAsInt() );
                            if ( competencyPersonProject != null )
                            {
                                userAuthorizedToGrade = competencyPersonProject.Project.Competency.TeacherOfRecordPersonId.Equals( userLogin.PersonId );
                            }

                            if ( competencyPersonProject.CompetencyPerson.PersonId != CurrentPersonId )
                            {
                                // somebody besides the Resident is logged in
                                NavigateToParentPage();
                                return;
                            }
                        }

                        if ( userAuthorizedToGrade )
                        {
                            string gradeDetailPageGuid = this.GetAttributeValue( "ResidentGradeDetailPage" );
                            if ( !string.IsNullOrWhiteSpace( gradeDetailPageGuid ) )
                            {
                                var page = new PageService( rockContext ).Get( new Guid( gradeDetailPageGuid ) );
                                if ( page != null )
                                {
                                    string identifier = hfCompetencyPersonProjectId.Value + "|" + userLogin.Guid + "|" + DateTime.Now.Ticks;
                                    string residentGraderSessionKey = Rock.Security.Encryption.EncryptString( identifier );
                                    Session["ResidentGraderSessionKey"] = residentGraderSessionKey;
                                    var queryString = new Dictionary<string, string>();
                                    queryString.Add( "CompetencyPersonProjectId", hfCompetencyPersonProjectId.Value );

                                    NavigateToPage( page.Guid, queryString );

                                    return;
                                }
                            }

                            nbWarningMessage.Text = "Ooops! Grading page not configured.";
                            return;
                        }
                        else
                        {
                            nbWarningMessage.Text = "User not authorized to grade this project";
                            return;
                        }
                    }
                }
            }

            nbWarningMessage.Text = "Invalid Login Information";
        }

        /// <summary>
        /// Handles the Click event of the btnSendRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendRequest_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                // controls will render error messages
                return;
            }

            int personId = ddlFacilitators.SelectedValueAsInt() ?? 0;
            var rockContext = new Rock.Data.RockContext();

            Person facilitator = new PersonService( rockContext ).Get( personId );

            if ( facilitator == null )
            {
                nbSendMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
                nbSendMessage.Text = "Facilitator not found";
                return;
            }

            string gradeDetailPageGuid = this.GetAttributeValue( "ResidentGradeDetailPage" );

            CompetencyPersonProject competencyPersonProject = new ResidencyService<CompetencyPersonProject>( new ResidencyContext() ).Get( hfCompetencyPersonProjectId.ValueAsInt() );

            var userLoginService = new UserLoginService( rockContext );
            var facilitatorUserLogin = userLoginService.GetByPersonId( facilitator.Id ).FirstOrDefault();

            Uri gradeDetailPageUrl = null;
            if ( !string.IsNullOrWhiteSpace( gradeDetailPageGuid ) )
            {
                PageCache pageCache = PageCache.Read( new Guid( gradeDetailPageGuid ) );
                if ( pageCache != null )
                {
                    Dictionary<string, string> queryString = new Dictionary<string, string>();

                    int routeId = 0;
                    {
                        var pageRoute = pageCache.PageRoutes.FirstOrDefault();
                        if ( pageRoute != null )
                        {
                            routeId = pageRoute.Id;
                        }
                    }

                    // set Ticks (3rd part) to 0 since this is an emailed request
                    string identifier = hfCompetencyPersonProjectId.Value + "|" + facilitatorUserLogin.Guid + "|0";
                    string gradeKey = Rock.Security.Encryption.EncryptString( identifier );

                    queryString.Add( "CompetencyPersonProjectId", hfCompetencyPersonProjectId.Value );
                    queryString.Add( "GradeKey", Server.UrlEncode( gradeKey ) );

                    PageReference pageReference = new PageReference( pageCache.Id, routeId, queryString );

                    Uri rootUri = new Uri( this.RootPath );
                    gradeDetailPageUrl = new Uri( rootUri, pageReference.BuildUrl() );
                }
            }
            else
            {
                nbWarningMessage.Text = "Ooops! Grading page not configured.";
                return;
            }

            var mergeObjects = new Dictionary<string, object>();
            mergeObjects.Add( "Facilitator", facilitator.ToDictionary() );
            
            // load person using rockContext to avoid error when doing ToLiquid
            var person = new PersonService( rockContext ).Get( competencyPersonProject.CompetencyPerson.PersonId );
            mergeObjects.Add( "Resident", person.ToLiquid() );

            mergeObjects.Add( "Project", competencyPersonProject.Project.ToDictionary() );

            mergeObjects.Add( "GradeDetailPageUrl", gradeDetailPageUrl.ToString() );

            var recipients = new List<RecipientData>();

            recipients.Add( new RecipientData( facilitator.Email, mergeObjects ) );

            Email.Send( com.ccvonline.Residency.SystemGuid.EmailTemplate.RESIDENCY_PROJECT_GRADE_REQUEST.AsGuid(), recipients );

            var parentParams = new Dictionary<string, string>();
            parentParams.Add( "CompetencyPersonProjectId", hfCompetencyPersonProjectId.Value );
            NavigateToParentPage( parentParams );
        }

        #endregion
    }
}