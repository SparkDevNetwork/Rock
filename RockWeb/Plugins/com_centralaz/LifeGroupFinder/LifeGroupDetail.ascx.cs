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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.LifeGroupFinder
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Life Group Detail" )]
    [Category( "com_centralaz > Groups" )]
    [Description( "Allows a person to register for a group." )]

    [CustomRadioListField( "Group Member Status", "The group member status to use when adding person to group (default: 'Pending'.)", "2^Pending,1^Active,0^Inactive", true, "2", "", 1 )]
    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 2 )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49", "", 3 )]
    [WorkflowTypeField( "Workflow", "An optional workflow to start when registration is created. The GroupMember will set as the workflow 'Entity' when processing is started.", false, false, "", "", 4 )]
    [WorkflowTypeField( "Email Workflow", "An optional workflow to start when an email request is created. The GroupMember will set as the workflow 'Entity' when processing is started.", false, false, "", "", 4 )]
    [BooleanField( "Enable Debug", "Shows the fields available to merge in lava.", false, "", 5 )]
    [CodeEditorField( "Lava Template", "The lava template to use to format the group details.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, @"
", "", 6 )]
    [LinkedPage( "Result Page", "An optional page to redirect user to after they have been registered for the group.", false, "", "", 7 )]
    [CodeEditorField( "Result Lava Template", "The lava template to use to format result message after user has been registered. Will only display if user is not redirected to a Result Page ( previous setting ).", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, @"
", "", 8 )]
    public partial class LifeGroupDetail : RockBlock
    {
        public String focusOnState
        {
            get
            {
                var focusOnState = Session["FocusOn"] as String;
                if ( focusOnState == null )
                {
                    focusOnState = String.Empty;

                    Session["FocusOn"] = focusOnState;
                }
                Session["FocusOn"] = String.Empty;
                return focusOnState;
            }

            set
            {
                Session["FocusOn"] = value;
            }
        }

        #region Fields

        RockContext _rockContext = null;
        string _mode = "Simple";
        Group _group = null;
        GroupTypeRole _defaultGroupRole = null;
        DefinedValueCache _dvcConnectionStatus = null;
        DefinedValueCache _dvcRecordStatus = null;
        DefinedValueCache _married = null;
        DefinedValueCache _homeAddressType = null;
        GroupTypeCache _familyType = null;
        GroupTypeRoleCache _adultRole = null;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            ClearErrorMessage();
            if ( CurrentPerson != null )
            {
                pnlLogin.Visible = false;
            }
            if ( !CheckSettings() )
            {
                nbNotice.Visible = true;
                pnlView.Visible = false;
            }
            else
            {
                nbNotice.Visible = false;
                if ( !Page.IsPostBack )
                {
                    ShowDetails();
                }
                BuildMap();
            }
            lbRegister.Attributes.Add( "onclick", "return jumpToControl()" );
            lbEmail.Attributes.Add( "onclick", "return jumpToControl()" );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetails();
        }

        protected void lbLogin_Click( object sender, EventArgs e )
        {
            var site = RockPage.Layout.Site;
            if ( site.LoginPageId.HasValue )
            {
                site.RedirectToLoginPage( true );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRegister control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRegister_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                if ( !String.IsNullOrWhiteSpace( tbEmail.Text ) && !String.IsNullOrWhiteSpace( tbSecondEmail.Text ) && tbEmail.Text == tbSecondEmail.Text )
                {
                    nbErrorMessage.Title = "Error";
                    nbErrorMessage.Text = "Please enter a different email for each person.";
                }
                else
                {
                    var rockContext = new RockContext();
                    var personService = new PersonService( rockContext );

                    Person person = null;
                    Person secondPerson = null;
                    Group family = null;
                    Group secondFamily = null;

                    var changes = new List<string>();
                    var secondPersonChanges = new List<string>();
                    var familyChanges = new List<string>();
                    var secondFamilyChanges = new List<string>();

                    // Only use current person if the name entered matches the current person's name
                    if ( CurrentPerson != null &&
                        tbFirstName.Text.Trim().Equals( CurrentPerson.FirstName.Trim(), StringComparison.OrdinalIgnoreCase ) &&
                        tbLastName.Text.Trim().Equals( CurrentPerson.LastName.Trim(), StringComparison.OrdinalIgnoreCase ) )
                    {
                        person = personService.Get( CurrentPerson.Id );
                    }

                    // Try to find person by name/email 
                    if ( person == null )
                    {
                        var matches = personService.GetByMatch( tbFirstName.Text.Trim(), tbLastName.Text.Trim(), tbEmail.Text.Trim() );
                        if ( matches.Count() == 1 )
                        {
                            person = matches.First();
                        }
                    }

                    // Check to see if this is a new person
                    if ( person == null )
                    {
                        // If so, create the person and family record for the new person
                        person = new Person();
                        person.FirstName = tbFirstName.Text.Trim();
                        person.LastName = tbLastName.Text.Trim();
                        person.Email = tbEmail.Text.Trim();
                        person.IsEmailActive = true;
                        person.EmailPreference = EmailPreference.EmailAllowed;
                        person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                        person.ConnectionStatusValueId = _dvcConnectionStatus.Id;
                        person.RecordStatusValueId = _dvcRecordStatus.Id;
                        person.Gender = Gender.Unknown;

                        family = PersonService.SaveNewPerson( person, rockContext, _group.CampusId, false );
                    }
                    else
                    {
                        // updating current existing person
                        History.EvaluateChange( changes, "Email", person.Email, tbEmail.Text );
                        person.Email = tbEmail.Text;

                        // Get the current person's families
                        var families = person.GetFamilies( rockContext );
                        family = families.FirstOrDefault();

                    }

                    SetPhoneNumber( rockContext, person, pnHome, null, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid(), changes );

                    if ( pnlSecondSignup.Visible )
                    {
                        // Try to find second person by name/email 
                        if ( secondPerson == null )
                        {
                            var matches = personService.GetByMatch( tbSecondFirstName.Text.Trim(), tbSecondLastName.Text.Trim(), tbSecondEmail.Text.Trim() );
                            if ( matches.Count() == 1 )
                            {
                                secondPerson = matches.First();
                            }
                        }

                        // Check to see if this is a new person
                        if ( secondPerson == null )
                        {
                            // If so, create the second person and family record for the new person
                            secondPerson = new Person();
                            secondPerson.FirstName = tbSecondFirstName.Text.Trim();
                            secondPerson.LastName = tbSecondLastName.Text.Trim();
                            secondPerson.Email = tbSecondEmail.Text.Trim();
                            secondPerson.IsEmailActive = true;
                            secondPerson.EmailPreference = EmailPreference.EmailAllowed;
                            secondPerson.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                            secondPerson.ConnectionStatusValueId = _dvcConnectionStatus.Id;
                            secondPerson.RecordStatusValueId = _dvcRecordStatus.Id;
                            secondPerson.Gender = Gender.Unknown;

                            family = PersonService.SaveNewPerson( secondPerson, rockContext, _group.CampusId, false );
                        }
                        else
                        {
                            // updating current existing person
                            History.EvaluateChange( changes, "Email", secondPerson.Email, tbSecondEmail.Text );
                            secondPerson.Email = tbSecondEmail.Text;

                            // Get the current person's families
                            var families = secondPerson.GetFamilies( rockContext );
                            family = families.FirstOrDefault();

                        }

                        SetPhoneNumber( rockContext, secondPerson, pnSecondHome, null, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid(), changes );
                    }

                    // Save the person and change history 
                    rockContext.SaveChanges();
                    HistoryService.SaveChanges( rockContext, typeof( Person ),
                        Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), person.Id, changes );
                    HistoryService.SaveChanges( rockContext, typeof( Person ),
                        Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(), person.Id, familyChanges );
                    if ( secondPerson != null )
                    {
                        rockContext.SaveChanges();
                        HistoryService.SaveChanges( rockContext, typeof( Person ),
                            Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), secondPerson.Id, changes );
                        HistoryService.SaveChanges( rockContext, typeof( Person ),
                            Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(), secondPerson.Id, secondFamilyChanges );
                    }

                    // Check to see if a workflow should be launched for each person
                    WorkflowType workflowType = null;
                    Guid? workflowTypeGuid = GetAttributeValue( "Workflow" ).AsGuidOrNull();
                    if ( workflowTypeGuid.HasValue )
                    {
                        var workflowTypeService = new WorkflowTypeService( rockContext );
                        workflowType = workflowTypeService.Get( workflowTypeGuid.Value );
                    }

                    // Save the registrations ( and launch workflows )
                    var newGroupMembers = new List<GroupMember>();
                    AddPersonToGroup( rockContext, person, workflowType, newGroupMembers, false );
                    if ( secondPerson != null )
                    {
                        AddPersonToGroup( rockContext, secondPerson, workflowType, newGroupMembers, false );
                    }

                    // Show the results
                    pnlView.Visible = false;
                    pnlSecondSignup.Visible = false;
                    pnlSignup.Visible = false;

                    pnlResult.Visible = true;
                    tbResultEmail.Text = tbEmail.Text;
                    tbResultFirstName.Text = tbFirstName.Text;
                    pnResultHome.Text = pnHome.Text;
                    tbResultLastName.Text = tbLastName.Text;
                    if ( secondPerson != null )
                    {
                        pnlSecondResult.Visible = true;
                        tbSecondResultEmail.Text = tbSecondEmail.Text;
                        tbSecondResultFirstName.Text = tbSecondFirstName.Text;
                        pnSecondResultHome.Text = pnSecondHome.Text;
                        tbSecondResultLastName.Text = tbSecondLastName.Text;
                    }

                    // Show lava content
                    var mergeFields = new Dictionary<string, object>();
                    mergeFields.Add( "Group", _group );
                    mergeFields.Add( "GroupMembers", newGroupMembers );

                    bool showDebug = UserCanEdit && GetAttributeValue( "EnableDebug" ).AsBoolean();

                    string template = GetAttributeValue( "ResultLavaTemplate" );
                }
            }
        }
        protected void lbGoBack_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }
        protected void lbRegister_Click( object sender, EventArgs e )
        {
            pnHome.Visible = true;
            btnEmail.Visible = false;
            btnRegister.Visible = true;
            cbSecondSignup.Visible = true;
            if ( cbSecondSignup.Checked )
            {
                pnlSecondSignup.Visible = true;
            }
            lLastName.Visible = true;
            lHome.Visible = true;
            lEmail.Visible = true;
            lSecondSignup.Visible = true;
            lFirstName.Text = "We treat Life Groups like family, and to us family uses real names.";
            focusOnState = "tbFirstName";
            tbFirstName.Focus();

        }
        protected void lbPhone_Click( object sender, EventArgs e )
        {

        }
        protected void lbEmail_Click( object sender, EventArgs e )
        {
            pnHome.Visible = false;
            btnEmail.Visible = true;
            btnRegister.Visible = false;
            cbSecondSignup.Visible = false;
            pnlSecondSignup.Visible = false;
            lLastName.Visible = false;
            lHome.Visible = false;
            lEmail.Visible = false;
            lSecondSignup.Visible = false;
            lFirstName.Text = "For more information about this group, please fill out your first name, last name, and email.";
            focusOnState = "tbFirstName";
            tbFirstName.Focus();
        }
        protected void cbSecondSignup_CheckedChanged( object sender, EventArgs e )
        {
            if ( cbSecondSignup.Checked )
            {
                pnlSecondSignup.Visible = true;
            }
            else
            {
                pnlSecondSignup.Visible = false;
            }
        }

        protected void lbExit_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }
        protected void lbChange_Click( object sender, EventArgs e )
        {
            //TODO: cancel incorrect member signup
            pnlSignup.Visible = true;
            pnlResult.Visible = false;
            if ( pnlSecondResult.Visible )
            {
                pnlSecondSignup.Visible = true;
                pnlSecondResult.Visible = false;
            }
            btnRegister.Text = "Correct it!";
        }

        protected void btnEmail_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );

                Person person = null;
                Group family = null;

                var changes = new List<string>();
                var familyChanges = new List<string>();

                // Only use current person if the name entered matches the current person's name
                if ( CurrentPerson != null &&
                    tbFirstName.Text.Trim().Equals( CurrentPerson.FirstName.Trim(), StringComparison.OrdinalIgnoreCase ) &&
                    tbLastName.Text.Trim().Equals( CurrentPerson.LastName.Trim(), StringComparison.OrdinalIgnoreCase ) )
                {
                    person = personService.Get( CurrentPerson.Id );
                }

                // Try to find person by name/email 
                if ( person == null )
                {
                    var matches = personService.GetByMatch( tbFirstName.Text.Trim(), tbLastName.Text.Trim(), tbEmail.Text.Trim() );
                    if ( matches.Count() == 1 )
                    {
                        person = matches.First();
                    }
                }

                // Check to see if this is a new person
                if ( person == null )
                {
                    // If so, create the person and family record for the new person
                    person = new Person();
                    person.FirstName = tbFirstName.Text.Trim();
                    person.LastName = tbLastName.Text.Trim();
                    person.Email = tbEmail.Text.Trim();
                    person.IsEmailActive = true;
                    person.EmailPreference = EmailPreference.EmailAllowed;
                    person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    person.ConnectionStatusValueId = _dvcConnectionStatus.Id;
                    person.RecordStatusValueId = _dvcRecordStatus.Id;
                    person.Gender = Gender.Unknown;

                    family = PersonService.SaveNewPerson( person, rockContext, _group.CampusId, false );
                }
                else
                {
                    // updating current existing person
                    History.EvaluateChange( changes, "Email", person.Email, tbEmail.Text );
                    person.Email = tbEmail.Text;

                    // Get the current person's families
                    var families = person.GetFamilies( rockContext );
                    family = families.FirstOrDefault();

                }

                SetPhoneNumber( rockContext, person, pnHome, null, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid(), changes );

                // Save the person and change history 
                rockContext.SaveChanges();
                HistoryService.SaveChanges( rockContext, typeof( Person ),
                    Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), person.Id, changes );
                HistoryService.SaveChanges( rockContext, typeof( Person ),
                    Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(), person.Id, familyChanges );

                // Check to see if a workflow should be launched for each person
                WorkflowType workflowType = null;
                Guid? workflowTypeGuid = GetAttributeValue( "EmailWorkflow" ).AsGuidOrNull();
                if ( workflowTypeGuid.HasValue )
                {
                    var workflowTypeService = new WorkflowTypeService( rockContext );
                    workflowType = workflowTypeService.Get( workflowTypeGuid.Value );
                }

                // Save the registrations ( and launch workflows )
                var newGroupMembers = new List<GroupMember>();
                AddPersonToGroup( rockContext, person, workflowType, newGroupMembers, true );

                // Show the results            
                // NavigateToLinkedPage( "ResultPage" );
                pnlView.Visible = false;
                pnlSecondSignup.Visible = false;
                pnlSignup.Visible = false;
                pnlEmailSent.Visible = true;
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Clears the error message title and text.
        /// </summary>
        private void ClearErrorMessage()
        {
            nbErrorMessage.Title = string.Empty;
            nbErrorMessage.Text = string.Empty;
        }
        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            _rockContext = _rockContext ?? new RockContext();

            if ( _group != null )
            {

                _group.LoadAttributes();
                string vidTag = GetVideoTag( _group.GetAttributeValue( "MainVideo" ), 350, 200 );
                if ( !string.IsNullOrWhiteSpace( _group.GetAttributeValue( "MainVideo" ) ) )
                {
                    string videoUrl = ResolveRockUrl( String.Format( "~/GetFile.ashx?guid={0}", _group.GetAttributeValue( "MainVideo" ) ) );
                    lMainMedia.Text = vidTag;
                }
                else
                {
                    string imgTag = GetImageTag( _group.GetAttributeValue( "MainPhoto" ), 350, 200 );
                    if ( !string.IsNullOrWhiteSpace( _group.GetAttributeValue( "MainPhoto" ) ) )
                    {
                        string imageUrl = ResolveRockUrl( String.Format( "~/GetImage.ashx?guid={0}", _group.GetAttributeValue( "MainPhoto" ) ) );
                        lMainMedia.Text = string.Format( "<a href='{0}'>{1}</a>", imageUrl, imgTag );
                    }
                    else
                    {
                        GroupMember leader = _group.Members.FirstOrDefault( m => m.GroupRole.IsLeader == true );
                        if ( leader != null )
                        {

                            Person person = leader.Person;
                            string imgPersonTag = Rock.Model.Person.GetPhotoImageTag( person.PhotoId, person.Age, person.Gender, 200, 200 );
                            if ( person.PhotoId.HasValue )
                            {
                                lMainMedia.Text = string.Format( "<a href='{0}'>{1}</a>", person.PhotoUrl, imgPersonTag );
                            }
                            else
                            {
                                lMainMedia.Text = imgTag;
                            }
                        }
                    }
                }
                string groupPhotoTag1 = GetImageTag( _group.GetAttributeValue( "GroupPhoto1" ), 350, 200 );
                if ( !string.IsNullOrWhiteSpace( _group.GetAttributeValue( "GroupPhoto1" ) ) )
                {
                    string imageUrl = ResolveRockUrl( String.Format( "~/GetImage.ashx?guid={0}", _group.GetAttributeValue( "GroupPhoto1" ) ) );
                    lGroupPhoto1.Text = string.Format( "<a href='{0}'>{1}</a>", imageUrl, groupPhotoTag1 );
                }
                string groupPhotoTag2 = GetImageTag( _group.GetAttributeValue( "GroupPhoto2" ), 350, 200 );
                if ( !string.IsNullOrWhiteSpace( _group.GetAttributeValue( "GroupPhoto2" ) ) )
                {
                    string imageUrl = ResolveRockUrl( String.Format( "~/GetImage.ashx?guid={0}", _group.GetAttributeValue( "GroupPhoto2" ) ) );
                    lGroupPhoto2.Text = string.Format( "<a href='{0}'>{1}</a>", imageUrl, groupPhotoTag2 );
                }
                string groupPhotoTag3 = GetImageTag( _group.GetAttributeValue( "GroupPhoto3" ), 350, 200 );
                if ( !string.IsNullOrWhiteSpace( _group.GetAttributeValue( "GroupPhoto3" ) ) )
                {
                    string imageUrl = ResolveRockUrl( String.Format( "~/GetImage.ashx?guid={0}", _group.GetAttributeValue( "GroupPhoto3" ) ) );
                    lGroupPhoto3.Text = string.Format( "<a href='{0}'>{1}</a>", imageUrl, groupPhotoTag3 );
                }


                lDescription.Text = _group.Description;
                BuildMap();

                // Show lava content
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Group", _group );

                bool showDebug = UserCanEdit && GetAttributeValue( "EnableDebug" ).AsBoolean();
                lLavaOutputDebug.Visible = showDebug;
                if ( showDebug )
                {
                    lLavaOutputDebug.Text = mergeFields.lavaDebugInfo( _rockContext );
                }
                lGroupName.Text = _group.Name;

                string template = GetAttributeValue( "LavaTemplate" );
                lLavaOverview.Text = template.ResolveMergeFields( mergeFields );

                pnlSignup.AddCssClass( "col-md-12" );

                if ( CurrentPersonId.HasValue )
                {
                    var personService = new PersonService( _rockContext );
                    Person person = personService
                        .Queryable( "PhoneNumbers.NumberTypeValue" ).AsNoTracking()
                        .FirstOrDefault( p => p.Id == CurrentPersonId.Value );

                    tbFirstName.Text = CurrentPerson.FirstName;
                    tbLastName.Text = CurrentPerson.LastName;
                    tbEmail.Text = CurrentPerson.Email;

                    Guid homePhoneType = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid();
                    var homePhone = person.PhoneNumbers
                        .FirstOrDefault( n => n.NumberTypeValue.Guid.Equals( homePhoneType ) );
                    if ( homePhone != null )
                    {
                        pnHome.Text = homePhone.Number;
                    }
                }
            }
        }

        public string GetImageTag( String imageGuid, int? maxWidth = null, int? maxHeight = null )
        {
            var photoUrl = new StringBuilder();

            photoUrl.Append( System.Web.VirtualPathUtility.ToAbsolute( "~/" ) );

            string styleString = string.Empty;

            if ( !String.IsNullOrWhiteSpace( imageGuid ) )
            {
                photoUrl.AppendFormat( "GetImage.ashx?guid={0}", imageGuid );

                if ( maxWidth.HasValue )
                {
                    photoUrl.AppendFormat( "&width={0}", maxWidth.Value );
                }
                if ( maxHeight.HasValue )
                {
                    photoUrl.AppendFormat( "&height={0}", maxHeight.Value );
                }
            }
            else
            {
                photoUrl.Append( "Assets/Images/no-picture.svg?" );

                if ( maxWidth.HasValue || maxHeight.HasValue )
                {
                    styleString = string.Format( " style='{0}{1}'",
                        maxWidth.HasValue ? "max-width:" + maxWidth.Value.ToString() + "px; " : "",
                        maxHeight.HasValue ? "max-height:" + maxHeight.Value.ToString() + "px;" : "" );
                }
            }

            return string.Format( "<img src='{0}'{1}/>", photoUrl.ToString(), styleString );
        }
        public string GetVideoTag( String videoGuid, int? maxWidth = null, int? maxHeight = null )
        {
            var videoUrl = new StringBuilder();
            var videoSize = new StringBuilder();

            videoUrl.Append( RockPage.ResolveRockUrlIncludeRoot( "~/" ) );

            string styleString = string.Empty;

            if ( !String.IsNullOrWhiteSpace( videoGuid ) )
            {
                videoUrl.AppendFormat( "GetFile.ashx?guid={0}", videoGuid );

                if ( maxWidth.HasValue )
                {
                    videoSize.AppendFormat( "width='{0}'", maxWidth.Value );
                }
                if ( maxHeight.HasValue )
                {
                    videoSize.AppendFormat( "height='{0}'", maxHeight.Value );
                }
            }
            else
            {
                videoUrl.Append( "Assets/Images/no-picture.svg?" );

                if ( maxWidth.HasValue || maxHeight.HasValue )
                {
                    styleString = string.Format( " style='{0}{1}'",
                        maxWidth.HasValue ? "max-width:" + maxWidth.Value.ToString() + "px; " : "",
                        maxHeight.HasValue ? "max-height:" + maxHeight.Value.ToString() + "px;" : "" );
                }
            }

            return string.Format( "<video controls {0} name='media'><source src='{1}'{2} type='video/mp4'></video>", videoSize.ToString(), videoUrl.ToString(), styleString );
        }

        private void BuildMap()
        {
            // Get Map Style
            phMaps.Controls.Clear();
            var mapStyleValue = DefinedValueCache.Read( GetAttributeValue( "MapStyle" ) );
            if ( mapStyleValue == null )
            {
                mapStyleValue = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK );
            }

            if ( mapStyleValue != null )
            {
                string mapStyle = mapStyleValue.GetAttributeValue( "StaticMapStyle" );
                if ( !string.IsNullOrWhiteSpace( mapStyle ) )
                {
                    foreach ( GroupLocation groupLocation in _group.GroupLocations.OrderBy( gl => ( gl.GroupLocationTypeValue != null ) ? gl.GroupLocationTypeValue.Order : int.MaxValue ) )
                    {
                        if ( groupLocation.Location != null )
                        {
                            if ( groupLocation.Location.GeoPoint != null )
                            {
                                string markerPoints = string.Format( "{0},{1}", groupLocation.Location.GeoPoint.Latitude, groupLocation.Location.GeoPoint.Longitude );
                                string mapLink = System.Text.RegularExpressions.Regex.Replace( mapStyle, @"\{\s*MarkerPoints\s*\}", markerPoints );
                                mapLink = System.Text.RegularExpressions.Regex.Replace( mapLink, @"\{\s*PolygonPoints\s*\}", string.Empty );
                                mapLink += "&sensor=false&size=350x200&zoom=13&format=png";
                                var literalcontrol = new Literal()
                                {
                                    Text = string.Format(
                                    "<div class='group-location-map'><img src='{0}'/></div>",
                                    mapLink ),
                                    Mode = LiteralMode.PassThrough
                                };
                                phMaps.Controls.Add( literalcontrol );
                            }
                            else if ( groupLocation.Location.GeoFence != null )
                            {
                                string polygonPoints = "enc:" + groupLocation.Location.EncodeGooglePolygon();
                                string mapLink = System.Text.RegularExpressions.Regex.Replace( mapStyle, @"\{\s*MarkerPoints\s*\}", string.Empty );
                                mapLink = System.Text.RegularExpressions.Regex.Replace( mapLink, @"\{\s*PolygonPoints\s*\}", polygonPoints );
                                mapLink += "&sensor=false&size=350x200&format=png";
                                phMaps.Controls.Add(
                                    new LiteralControl( string.Format(
                                        "<div class='group-location-map'><img src='{0}'/></div>",
                                        mapLink ) ) );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds the person to group.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="workflowType">Type of the workflow.</param>
        /// <param name="groupMembers">The group members.</param>
        private void AddPersonToGroup( RockContext rockContext, Person person, WorkflowType workflowType, List<GroupMember> groupMembers, bool emailOnly )
        {
            if ( person != null )
            {
                if ( !_group.Members
                    .Any( m =>
                        m.PersonId == person.Id &&
                        m.GroupRoleId == _defaultGroupRole.Id ) )
                {
                    var groupMemberService = new GroupMemberService( rockContext );
                    var groupMember = new GroupMember();
                    groupMember.PersonId = person.Id;
                    groupMember.GroupRoleId = _defaultGroupRole.Id;
                    groupMember.GroupMemberStatus = (GroupMemberStatus)GetAttributeValue( "GroupMemberStatus" ).AsInteger();
                    groupMember.GroupId = _group.Id;
                    groupMemberService.Add( groupMember );
                    rockContext.SaveChanges();
                    if ( emailOnly )
                    {
                        groupMember.LoadAttributes();
                        groupMember.SetAttributeValue( "InfoSeeker", "True" );
                        groupMember.SaveAttributeValues();
                        rockContext.SaveChanges();
                    }

                    if ( workflowType != null )
                    {
                        try
                        {
                            var workflowService = new WorkflowService( rockContext );
                            var workflow = Workflow.Activate( workflowType, person.FullName );
                            workflow.LoadAttributes();
                            workflow.SetAttributeValue( "GroupLeader", _group.Members.FirstOrDefault( m => m.GroupRole.IsLeader == true ).Person.PrimaryAlias.Guid.ToString() );
                            workflow.SetAttributeValue( "GroupMember", person.PrimaryAlias.Guid.ToString() );
                            List<string> workflowErrors;
                            if ( workflow.Process( rockContext, groupMember, out workflowErrors ) )
                            {
                                if ( workflow.IsPersisted || workflowType.IsPersisted )
                                {
                                    if ( workflow.Id == 0 )
                                    {
                                        workflowService.Add( workflow );
                                    }

                                    rockContext.WrapTransaction( () =>
                                    {
                                        rockContext.SaveChanges();
                                        workflow.SaveAttributeValues( _rockContext );
                                        foreach ( var activity in workflow.Activities )
                                        {
                                            activity.SaveAttributeValues( rockContext );
                                        }
                                    } );
                                }
                            }

                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex, this.Context );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks the settings.
        /// </summary>
        /// <returns></returns>
        private bool CheckSettings()
        {
            _rockContext = _rockContext ?? new RockContext();

            _mode = GetAttributeValue( "Mode" );

            int groupId = PageParameter( "GroupId" ).AsInteger();
            _group = new GroupService( _rockContext )
                .Queryable( "GroupType.DefaultGroupRole" ).AsNoTracking()
                .FirstOrDefault( g => g.Id == groupId );
            if ( _group == null || _group.Members.FirstOrDefault( m => m.GroupRole.IsLeader == true ) == null )
            {
                nbNotice.Heading = "Unknown Group";
                nbNotice.Text = "<p>This page requires a valid group id parameter, and there was not one provided.</p>";
                return false;
            }
            else
            {
                _defaultGroupRole = _group.GroupType.DefaultGroupRole;
            }

            _dvcConnectionStatus = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
            if ( _dvcConnectionStatus == null )
            {
                nbNotice.Heading = "Invalid Connection Status";
                nbNotice.Text = "<p>The selected Connection Status setting does not exist.</p>";
                return false;
            }

            _dvcRecordStatus = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() );
            if ( _dvcRecordStatus == null )
            {
                nbNotice.Heading = "Invalid Record Status";
                nbNotice.Text = "<p>The selected Record Status setting does not exist.</p>";
                return false;
            }

            _married = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() );
            _homeAddressType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
            _familyType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            _adultRole = _familyType.Roles.FirstOrDefault( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) );

            if ( _married == null || _homeAddressType == null || _familyType == null || _adultRole == null )
            {
                nbNotice.Heading = "Missing System Value";
                nbNotice.Text = "<p>There is a missing or invalid system value. Check the settings for Marital Status of 'Married', Location Type of 'Home', Group Type of 'Family', and Family Group Role of 'Adult'.</p>";
                return false;
            }

            return true;

        }

        /// <summary>
        /// Sets the phone number.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="pnbNumber">The PNB number.</param>
        /// <param name="cbSms">The cb SMS.</param>
        /// <param name="phoneTypeGuid">The phone type unique identifier.</param>
        /// <param name="changes">The changes.</param>
        private void SetPhoneNumber( RockContext rockContext, Person person, PhoneNumberBox pnbNumber, RockCheckBox cbSms, Guid phoneTypeGuid, List<string> changes )
        {
            var phoneType = DefinedValueCache.Read( phoneTypeGuid );
            if ( phoneType != null )
            {
                var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneType.Id );
                string oldPhoneNumber = string.Empty;
                if ( phoneNumber == null )
                {
                    phoneNumber = new PhoneNumber { NumberTypeValueId = phoneType.Id };
                }
                else
                {
                    oldPhoneNumber = phoneNumber.NumberFormattedWithCountryCode;
                }

                phoneNumber.CountryCode = PhoneNumber.CleanNumber( pnbNumber.CountryCode );
                phoneNumber.Number = PhoneNumber.CleanNumber( pnbNumber.Number );

                if ( string.IsNullOrWhiteSpace( phoneNumber.Number ) )
                {
                    if ( phoneNumber.Id > 0 )
                    {
                        new PhoneNumberService( rockContext ).Delete( phoneNumber );
                        person.PhoneNumbers.Remove( phoneNumber );
                    }
                }
                else
                {
                    if ( phoneNumber.Id <= 0 )
                    {
                        person.PhoneNumbers.Add( phoneNumber );
                    }
                    if ( cbSms != null && cbSms.Checked )
                    {
                        phoneNumber.IsMessagingEnabled = true;
                        person.PhoneNumbers
                            .Where( n => n.NumberTypeValueId != phoneType.Id )
                            .ToList()
                            .ForEach( n => n.IsMessagingEnabled = false );
                    }
                }

                History.EvaluateChange( changes,
                    string.Format( "{0} Phone", phoneType.Value ),
                    oldPhoneNumber, phoneNumber.NumberFormattedWithCountryCode );
            }
        }

        #endregion

    }
}