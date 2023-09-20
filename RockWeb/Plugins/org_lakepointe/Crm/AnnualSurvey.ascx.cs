using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_lakepointe.Crm
{
    [DisplayName( "Annual Survey" )]
    [Category( "LPC > CRM" )]
    [Description( "Allows a person to update the information for their family and to complete a short survey." )]

    /*General Configuration */
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, "", "Configuration" )]
    [BooleanField( "Enable Lava Debugging", "Shows the fields that are available to merge in Lava.", false, "Configuration" )]
    [BooleanField( "Data Entry", "True/False flag indicating if the block is being used for data entry.", false, "Configuration" )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Survey Complete Date Attribute", "The attribute to save the survey completion date in.", true, false, "C618929D-F66F-4DCF-917B-49E945D5C3EC", "Configuration" )]
    [GroupField( "Respondant Group", "The group to put people in when they have completed the Annual Survey.", false, "", "Configuration" )]

    /*Welcome Block Settings*/
    [CodeEditorField( "Welcome Message Template", "The lava template for the message to show on the Welcome Screen.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, "", "Welcome", 0 )]
    [TextField( "Begin Survey Button Text", "The text to display on the Begin Survey button.", false, "Begin Survey", "Welcome" )]
    [TextField( "Begin Survey Button Loading Text", "The text to display on the Begin Survey button while loading the survey.", false, "Loading Survey", "Welcome" )]

    /*Family Information Settings*/
    [CodeEditorField( "Family Info Template", "The lava template for the message to show on the Welcome Screen.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, "", "Family Info", 0 )]
    [BooleanField( "Show Family Campus", "True/False flag indicating if the family campus should be displayed.", true, "Family Info" )]
    [BooleanField( "Show No Longer Attends Checkbox", "True/False flag indicating if the show no longer attends checkbox should be visible.", true, "Family Info" )]
    [TextField( "Family Info Next Button Text", "The text to display on the Next Button of the Family Information screen.", false, "Next", "Family Info" )]
    [TextField( "Family Info Back Button Text", "The text to display on the Back button of the Family Information screen.", false, "Back", "Family Info" )]

    /*Family Member Settings*/
    [CodeEditorField( "Family Member Template", "The lava template for the message to show as an intro to the Family Member screen.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, "", "Family Member", 0 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS, "Default Record Status", "The record status that is applied to  new family members.", true, false, Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING, "Family Member" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE, "Available Phone Number Types", "The phone number types that dispaly on the family member detail.", false, true, "", "Family Member" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Default Connection Status", "The connection status that is applied to new family members.", false, false, Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT, "Family Member" )]
    [TextField( "Family Member Next Button Text", "The text to display on the Next Button of the Family Member screen", false, "Next", "Family Member" )]
    [TextField( "Family Member Back Button Text", "The text to display on the Back Button of the Family Member screen.", false, "Back", "Family Member" )]

    /*No Longer Attends Settings*/
    [CodeEditorField( "No Longer Attends Template", "The lava template for the message to display on the No Longer Attends Confirmation Template", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, "", "No Longer Attends" )]
    [TextField( "No Longer Attends Next Button Text", "Text to display on the Next Button on confirmation screen.", false, "Finish", "No Longer Attends" )]
    [TextField( "No Longer Attends Back Button Text", "Text to display on the Back Button.", false, "Go Back", "No Longer Attends" )]

    /*Ask More Questions Settings*/
    [TextField( "Ask More Questions Next Button Text", "Text to display on the Next button on the Ask More Questions Screen", false, "Next", "Ask More Questions" )]
    [TextField( "Ask More Questions Back Button Text", "Text to display on the Back button on the Ask More Questions Screen", false, "Back", "Ask More Questions" )]

    /*Survey Question Panel Settings */
    [CodeEditorField( "Survey Question Intro", "Intro message to display on Survey question panel.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, "", "Survey Questions" )]
    [AttributeCategoryField( "Survey Question Attribute Categories", "The Attribute Categories to retrieve the Survey Questions from ", true, "Rock.Model.Person", true, "", "Survey Questions" )]
    [TextField( "Survey Questions Next Button Text", "Text to display on the Next Button on the Survey Questions Screen.", false, "Next", "Survey Questions" )]
    [TextField( "Survey Questions Back Button Text", "Text to display on the Back Button of the Survey Questions Screen.", false, "Back", "Survey Questions" )]

    /*Survey Complete Settings*/
    [CodeEditorField( "Survey Complete Template", "The lava template for the message to display on the Survey Complete Template.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, "", "Survey Complete" )]
    [TextField( "Survey Complete Next Button Text", "Text to display on the next button of the Survey Complete screen.", false, "Return", "Survey Complete" )]
    [UrlLinkField( "Redirect Page", "The page to redirect the user to on the completion of the survey.", false, "https://www.lakepointe.org/", "Survey Complete" )]
    public partial class AnnualSurvey : RockBlock
    {
        #region Fields
        private RockContext context = null;
        private string AnnualSurvey_ProfileChangeRequestGuid = "37C0D4EA-13BD-4C97-89BB-7D30325D324B";
        private string AnnualSurvey_FamilyNoLongerAttendsDVGuid = "B1120C60-AE6E-408B-99BB-3612B58874EB";
        private string AnnualSurvey_RemoveFromFamilyDVGuid = "A3F16682-ABEA-478A-978C-762F913C715D";
        private string AnnualSurvey_InactivatePersonDVGuid = "16BDE7C2-1CBF-4F8C-8F15-53B333E69448";
        #endregion

        #region Properties
        protected AnnualSurveyFamily CurrentFamily { get; set; }
        protected AnnualSurveyStep CurrentStep { get; set; }

        #endregion

        #region Base Control Methods

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            CurrentFamily = ViewState["CurrentFamily"].ToString().FromJsonOrNull<AnnualSurveyFamily>();
            CurrentStep = (AnnualSurveyStep)ViewState["CurrentStep"];
            LoadFamilyMemberControls( false );
            LoadSurveyQuestionAttributes();

            switch ( CurrentStep )
            {
                case AnnualSurveyStep.FAMILY_INFO:
                    btnNext.CausesValidation = true;
                    btnNext.ValidationGroup = "FamilyInfo";
                    valSummary.ValidationGroup = "FamilyInfo";
                    break;
                case AnnualSurveyStep.NO_LONGER_ATTENDS_CONFIRM:
                    break;
                case AnnualSurveyStep.FAMILY_MEMBER_INFO:
                    btnNext.CausesValidation = true;
                    btnNext.ValidationGroup = "FamilyMember";
                    valSummary.ValidationGroup = "FamilyMember";
                    break;
                case AnnualSurveyStep.ASK_MORE_QUESTIONS:
                    break;
                case AnnualSurveyStep.ANSWER_SURVEY_QUESTIONS:
                    break;
                case AnnualSurveyStep.SURVEY_COMPLETE:
                    break;
                default:
                    btnNext.CausesValidation = false;
                    btnNext.ValidationGroup = "";
                    break;
            }
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            context = new RockContext();

            this.BlockUpdated += AnnualSurvey_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );



        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbError.Visible = false;
            nbError.Text = String.Empty;
            nbError.NotificationBoxType = NotificationBoxType.Danger;

            if ( !Page.IsPostBack )
            {
                InitializeSurvey();
            }

            if ( CurrentFamily != null )
            {
                if ( CurrentFamily.FamilyMembers
                    .Where( fm => fm.FamilyMemberRole != null )
                    .Where( fm => fm.FamilyMemberRole.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                    .Count() >= 2 )
                {
                    liAddAdult.Visible = false;
                }
                else
                {
                    liAddAdult.Visible = true;
                }
            }
        }

        protected override object SaveViewState()
        {
            ViewState["CurrentFamily"] = CurrentFamily.ToJson();
            ViewState["CurrentStep"] = CurrentStep;
            return base.SaveViewState();
        }

        #endregion

        #region Events
        private void AnnualSurvey_BlockUpdated( object sender, EventArgs e )
        {
            InitializeSurvey();
        }

        protected void btnNext_Click( object sender, EventArgs e )
        {
            switch ( CurrentStep )
            {
                case AnnualSurveyStep.WELCOME:
                    CurrentStep = AnnualSurveyStep.FAMILY_INFO;
                    break;
                case AnnualSurveyStep.FAMILY_INFO:
                    UpdateFamily();
                    break;
                case AnnualSurveyStep.FAMILY_MEMBER_INFO:
                    UpdateFamilyMembers( true );
                    break;
                case AnnualSurveyStep.ASK_MORE_QUESTIONS:
                    HandleAskMoreQuestionsResponse();
                    break;
                case AnnualSurveyStep.ANSWER_SURVEY_QUESTIONS:
                    if(!UpdateSurveyQuestions())
                    {
                        return;
                    }
                    break;
                case AnnualSurveyStep.NO_LONGER_ATTENDS_CONFIRM:
                    UpdateNoLongerAttends();
                    break;
                case AnnualSurveyStep.SURVEY_COMPLETE:
                    CompleteSurvey();
                    break;
                default:
                    break;
            }

            LoadSurvey();
        }

        protected void btnPrevious_Click( object sender, EventArgs e )
        {
            switch ( CurrentStep )
            {
                case AnnualSurveyStep.WELCOME:
                    break;
                case AnnualSurveyStep.FAMILY_INFO:
                    CurrentStep = AnnualSurveyStep.WELCOME;
                    CurrentFamily = null;
                    break;
                case AnnualSurveyStep.FAMILY_MEMBER_INFO:
                    CurrentStep = AnnualSurveyStep.FAMILY_INFO;
                    break;
                case AnnualSurveyStep.NO_LONGER_ATTENDS_CONFIRM:
                    CurrentStep = AnnualSurveyStep.FAMILY_INFO;
                    break;
                case AnnualSurveyStep.ASK_MORE_QUESTIONS:
                    CurrentStep = AnnualSurveyStep.FAMILY_MEMBER_INFO;
                    break;
                case AnnualSurveyStep.ANSWER_SURVEY_QUESTIONS:
                    CurrentStep = AnnualSurveyStep.ASK_MORE_QUESTIONS;
                    CurrentFamily.SurveyQuestionsLoaded = false;
                    break;
                default:
                    break;
            }
            LoadSurvey();
        }

        protected void btnAddFamilyMember_Click( object sender, EventArgs e )
        {

            UpdateFamilyMembers( false );

            var index = CurrentFamily.FamilyMembers.Select( fm => fm.Order ).Max();
            index++;

            var lbItem = (LinkButton)sender;

            if ( lbItem == null )
            {
                return;
            }

            var role = default( GroupTypeRoleCache );
            if ( lbItem.CommandArgument.Equals( "adult", StringComparison.InvariantCultureIgnoreCase ) )
            {
                role = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(), context ).Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() );
            }
            else
            {
                role = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(), context ).Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() );
            }

            var familyMember = new AnnualSurveyPerson()
            {
                PersonId = 0,
                FamilyMemberRole = role,
                FamilyMemberRoleId = role.Id,
                IsActive = true,
                IsRespondant = false,
                Order = index,
                Person = new Person()
                {
                    RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid(), context ).Id,
                    RecordStatusValueId = DefinedValueCache.Get( GetAttributeValue( "DefaultRecordStatus" ).AsGuid(), context ).Id,
                    ConnectionStatusValueId = DefinedValueCache.Get( GetAttributeValue( "DefaultConnectionStatus" ).AsGuid(), context ).Id
                }
            };

            CurrentFamily.FamilyMembers.Add( familyMember );

            LoadFamilyMemberControls( true );

            var hasTwoAdults = CurrentFamily.FamilyMembers
                .Where( fm => fm.FamilyMemberRole != null )
                .Where( fm => fm.FamilyMemberRole.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                .Count() >= 2;
            liAddAdult.Visible = !hasTwoAdults;

            AnnualSurveyPersonInformation personInfo = (AnnualSurveyPersonInformation)phFamilyMembers.FindControl( string.Format( "fmc_{0}", index ) );
            if ( personInfo != null )
            {
                personInfo.Focus();
            }

        }

        private void FamilyMemberControl_RemoveFamilyMember( object sender, EventArgs e )
        {
            UpdateFamilyMembers( false );
            AnnualSurveyPersonInformation control = (AnnualSurveyPersonInformation)sender;

            if ( control != null )
            {
                var order = control.ID.Replace( "fmc_", "" ).AsInteger();
                CurrentFamily.FamilyMembers.RemoveAll( m => m.Order == order );
            }

            var hasTwoAdults = CurrentFamily.FamilyMembers
                .Where( fm => fm.FamilyMemberRole != null )
                .Where( fm => fm.FamilyMemberRole.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                .Count() >= 2;
            liAddAdult.Visible = !hasTwoAdults;

            LoadFamilyMemberControls( true );

        }

        #endregion

        #region Methods

        private void InitializeSurvey()
        {
            CurrentStep = AnnualSurveyStep.WELCOME;

            LoadSurvey();
        }

        private void CompleteSurvey()
        {
            CurrentFamily = null;
            hfRespondant.Value = String.Empty;

            Response.Redirect( GetAttributeValue( "RedirectPage" ), true );
        }

        private void SetFocusToTopOfForm()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append( "$(document).ready(function() {" );
            sb.Append( "$('#hlTopOfForm').focus();" );
            sb.Append( "});" );
            ScriptManager.RegisterStartupScript( upnlContent, upnlContent.GetType(), "FocusToTop" + RockDateTime.Now.Ticks, sb.ToString(), true );

        }

        private List<AttributeCache> GetCategoryAttributeList( string attributeKey )
        {
            var attributeList = new List<AttributeCache>();
            foreach ( Guid categoryGuid in GetAttributeValue( attributeKey ).SplitDelimitedValues( false ).AsGuidList() )
            {
                var category = CategoryCache.Get( categoryGuid );
                if ( category != null )
                {
                    foreach ( var attribute in new AttributeService( context ).GetByCategoryId( category.Id, false ).OrderBy( a => a.Order ) )
                    {
                        if ( attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                        {
                            attributeList.Add( AttributeCache.Get( attribute ) );
                        }
                    }
                }
            }

            return attributeList;
        }

        private void HandleAskMoreQuestionsResponse()
        {
            CurrentFamily.AnswerSurveyQuestions = rblAskMoreQuestions.SelectedValue.AsBoolean();

            if ( CurrentFamily.AnswerSurveyQuestions )
            {
                CurrentStep = AnnualSurveyStep.ANSWER_SURVEY_QUESTIONS;
            }
            else
            {
                CurrentStep = AnnualSurveyStep.SURVEY_COMPLETE;
            }

        }

        private void LoadSurvey()
        {
            pnlWelcome.Visible = false;
            pnlFamily.Visible = false;
            pnlFamilyMembers.Visible = false;
            pnlAskMoreQuestions.Visible = false;
            pnlNoLongerAttends.Visible = false;
            pnlSurveyQuestions.Visible = false;
            pnlSurveyComplete.Visible = false;
            bool focusToTopOfForm = true;
            switch ( CurrentStep )
            {
                case AnnualSurveyStep.WELCOME:
                    LoadWelcome();
                    focusToTopOfForm = false;
                    break;
                case AnnualSurveyStep.FAMILY_INFO:
                    LoadFamilyInfo();
                    break;
                case AnnualSurveyStep.FAMILY_MEMBER_INFO:
                    LoadFamilyMembers();
                    break;
                case AnnualSurveyStep.ASK_MORE_QUESTIONS:
                    LoadAskMoreQuestions();
                    break;
                case AnnualSurveyStep.ANSWER_SURVEY_QUESTIONS:
                    LoadSurveyQuestions();
                    break;
                case AnnualSurveyStep.NO_LONGER_ATTENDS_CONFIRM:
                    LoadNoLongerAttendsConfirmation();
                    break;
                case AnnualSurveyStep.SURVEY_COMPLETE:
                    LoadSurveyComplete();
                    break;

                default:
                    break;
            }

            if ( focusToTopOfForm )
            {
                SetFocusToTopOfForm();
            }
        }

        private void LoadAskMoreQuestions()
        {
            pnlAskMoreQuestions.Visible = true;

            rblAskMoreQuestions.SelectedValue = CurrentFamily.AnswerSurveyQuestions.ToYesNo();

            btnNext.Text = GetAttributeValue( "AskMoreQuestionsNextButtonText" );
            btnNext.Visible = true;
            btnPrevious.Text = GetAttributeValue( "AskMoreQuestionsBackButtonText" );
            btnPrevious.Visible = true;

        }

        private void LoadFamilyInfo()
        {

            if ( CurrentFamily == null )
            {
                var dataEntryMode = GetAttributeValue( "DataEntry" ).AsBoolean();
                if ( !dataEntryMode )
                {
                    dataEntryMode = PageParameter( "DataEntry" ).AsBoolean();
                }

                var respondantGuid = PageParameter( "Person" ).AsGuidOrNull();

                Person respondant = null;
                if ( respondantGuid.HasValue )
                {
                    respondant = new PersonService( context ).Get( respondantGuid.Value );
                }

                if ( !dataEntryMode && respondant == null && CurrentPerson != null )
                {
                    respondant = CurrentPerson;
                }

                if ( respondant != null )
                {
                    CurrentFamily = new AnnualSurveyFamily( context, respondant.GetFamily( context ), respondant.Id );
                    lFamilyTitle.Text = string.Concat( CurrentFamily.FamilyName, " ", "Family" );

                    if ( CurrentFamily.HomeAddress != null )
                    {
                        apHomeAddress.SetValues( CurrentFamily.HomeAddress.Location );
                        cbIsMailingAddress.Checked = CurrentFamily.HomeAddress.IsMailingLocation;
                        cbIsPhysicalAddress.Checked = CurrentFamily.HomeAddress.IsMappedLocation;
                    }
                }
                else
                {
                    CurrentFamily = new AnnualSurveyFamily( context, null, null );
                    cbIsMailingAddress.Checked = true;
                    cbIsPhysicalAddress.Checked = true;
                    
                    cbNoLongerAttend.Visible = false;
                    cbNoLongerAttend.Checked = false;
                }
            }
            if ( !String.IsNullOrWhiteSpace( CurrentFamily.FamilyName ) )
            {
                lFamilyTitle.Text = String.Concat( CurrentFamily.FamilyName, " Family" );
            }
            cpFamilyCampus.SelectedCampusId = CurrentFamily.PrimaryCampus.Id;

            if ( CurrentFamily.HomeAddress != null )
            {
                apHomeAddress.SetValues( CurrentFamily.HomeAddress.Location );
            }

            cbNoLongerAttend.Checked = !CurrentFamily.CurrentlyAttends;

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "SurveyFamily", CurrentFamily );
            lFamilyIntro.Text = GetAttributeValue( "FamilyInfoTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );

            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lFamilyDebug.Visible = true;
                lFamilyDebug.Text = mergeFields.lavaDebugInfo();
            }
            cbNoLongerAttend.Visible = GetAttributeValue( "ShowNoLongerAttendsCheckbox" ).AsBoolean();

            pnlFamily.Visible = true;
            valSummary.ValidationGroup = "Family Info";
            btnNext.Text = GetAttributeValue( "FamilyInfoNextButtonText" );
            btnNext.CausesValidation = true;
            btnNext.ValidationGroup = "FamilyInfo";
            btnNext.Visible = true;

            btnPrevious.Text = GetAttributeValue( "FamilyInfoBackButtonText" );

            btnPrevious.Visible = true;

        }

        private void LoadFamilyMembers()
        {
            pnlFamilyMembers.Visible = true;
            btnNext.Text = GetAttributeValue( "FamilyMemberNextButtonText" );
            btnNext.CausesValidation = true;
            btnNext.ValidationGroup = "FamilyMember";
            btnNext.Visible = true;

            btnPrevious.Text = GetAttributeValue( "FamilyMemberBackButtonText" );
            btnPrevious.Visible = true;

            valSummary.ValidationGroup = "FamilyMember";

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "SurveyFamily", CurrentFamily );
            lFamilyMember.Text = GetAttributeValue( "FamilyMemberTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );

            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lFamilyMemberDebug.Visible = true;
                lFamilyMember.Text = mergeFields.lavaDebugInfo();
            }

            foreach ( var fm in CurrentFamily.FamilyMembers )
            {
                LoadFamilyMemberControls( true );
            }


        }

        private void LoadFamilyMemberControls( bool setValues )
        {
            phFamilyMembers.Controls.Clear();
            if ( CurrentFamily == null )
            {
                return;
            }

            var phoneNumberTypeGuids = GetAttributeValue( "AvailablePhoneNumberTypes" ).SplitDelimitedValues()
                .Select( g => g.AsGuidOrNull() )
                .Where( g => g != null )
                .ToList();

            var phoneNumberTypeIds = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid(), context ).DefinedValues
                .Where( dv => phoneNumberTypeGuids.Contains( dv.Guid ) )
                .Select( dv => dv.Id )
                .ToList();


            foreach ( var fm in CurrentFamily.FamilyMembers.OrderBy( fm => fm.Order ) )
            {
                var familyMemberControl = (AnnualSurveyPersonInformation)Page.LoadControl( "~/Plugins/org_lakepointe/Crm/AnnualSurveyPersonInformation.ascx" );
                familyMemberControl.RemoveFamilyMember += FamilyMemberControl_RemoveFamilyMember;
                familyMemberControl.EnableViewState = true;
                familyMemberControl.ID = string.Format( "fmc_{0}", fm.Order );
                familyMemberControl.ValidationGroup = "FamilyMember";
                familyMemberControl.FamilyMemberRoleIsEditable = false;

                familyMemberControl.CurrentPerson = fm.Person;
                familyMemberControl.IsMemberOfFamily = !fm.RemoveFromFamily;
                familyMemberControl.IsActivelyAttending = fm.IsActive;
                familyMemberControl.FamilyMemberRole = fm.FamilyMemberRole;

                familyMemberControl.PhoneNumberTypeLUIDs = phoneNumberTypeIds;
                if ( setValues )
                {

                    familyMemberControl.LoadPerson();
                }
                familyMemberControl.LoadPhoneNumbers( setValues );
                phFamilyMembers.Controls.Add( familyMemberControl );

            }
        }

        private void LoadNoLongerAttendsConfirmation()
        {

            pnlNoLongerAttends.Visible = true;
            btnNext.Text = GetAttributeValue( "NoLongerAttendsNextButtonText" );
            btnNext.Visible = true;
            btnPrevious.Text = GetAttributeValue( "NoLongerAttendsBackButtonText" );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "SurveyFamily", CurrentFamily );
            lNoLongerAttends.Text = GetAttributeValue( "NoLongerAttendsTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );

            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lNoLongerAttends.Visible = true;
                lNoLongerAttends.Text = mergeFields.lavaDebugInfo();
            }
        }

        private void LoadSurveyComplete()
        {

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "SurveyFamily", CurrentFamily );
            lSurveyComplete.Text = GetAttributeValue( "SurveyCompleteTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );

            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lSurveyCompleteDebug.Visible = true;
                lSurveyCompleteDebug.Text = mergeFields.lavaDebugInfo();
            }

            pnlSurveyComplete.Visible = true;
            btnNext.Text = GetAttributeValue( "SurveyCompleteNextButtonText" );
            btnNext.Visible = true;
            btnPrevious.Visible = false;
            btnPrevious.Text = String.Empty;

        }

        private void LoadSurveyQuestionAttributes( bool setValues = false )
        {
            phSurveyQuestions.Controls.Clear();

            var attributeList = GetCategoryAttributeList( "SurveyQuestionAttributeCategories" );
            Person respondant = null;

            if ( CurrentFamily != null )
            {
                respondant = CurrentFamily.FamilyMembers.Where( m => m.IsRespondant ).Select( fm => fm.Person ).FirstOrDefault();
            }

            if ( respondant == null )
            {
                respondant = new Person();
            }

            respondant.LoadAttributes( context );

            foreach ( var attribute in attributeList )
            {
                string value1 = respondant != null && setValues ? respondant.GetAttributeValue( attribute.Key ) : string.Empty;
                var div1 = new HtmlGenericControl( "Div" );
                phSurveyQuestions.Controls.Add( div1 );
                div1.AddCssClass( "col-sm-12" );
                var ctrl1 = attribute.AddControl( div1.Controls, value1, "SurveyQuestions", setValues, true, attribute.IsRequired, null, null, null, string.Format( "attribute_field_{0}", attribute.Id ) );

            }


        }

        private void LoadSurveyQuestions()
        {
            lSurveyQuestionsTitle.Text = "Survey Questions";

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "SurveyFamily", CurrentFamily );
            lSurveyQuestionsIntro.Text = GetAttributeValue( "Survey Question Intro" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );

            LoadSurveyQuestionAttributes( !CurrentFamily.SurveyQuestionsLoaded );

            CurrentFamily.SurveyQuestionsLoaded = true;

            valSummary.ValidationGroup = "SurveyQuestions";

            btnNext.Text = GetAttributeValue( "SurveyQuestionsNextButtonText" );
            //btnNext.ValidationGroup = "SurveyQuestions";
            //btnNext.CausesValidation = true;
            btnNext.Visible = true;
            btnPrevious.Text = GetAttributeValue( "SurveyQuestionsBackButtonText" );
            btnPrevious.CausesValidation = false;
            btnPrevious.Visible = true;
            pnlSurveyQuestions.Visible = true;

        }

        private void LoadWelcome()
        {
            btnNext.Text = GetAttributeValue( "BeginSurveyButtonText" );
            btnNext.DataLoadingText = GetAttributeValue( "BeginSurveyButtonLoadingText" );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            lWelcome.Text = GetAttributeValue( "WelcomeMessageTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );

            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lWelcomeDebug.Visible = true;
                lWelcomeDebug.Text = mergeFields.lavaDebugInfo();
            }

            btnPrevious.Visible = false;
            btnNext.Visible = true;
            btnNext.CausesValidation = false;
            pnlWelcome.Visible = true;
        }

        private void SaveFamily()
        {
            var mobilePhoneNumberTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), context ).Id;
            var homeLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid(), context ).Id;
            var previousAddressId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid(), context ).Id;
            var defaultRecordStatus = DefinedValueCache.Get( GetAttributeValue( "DefaultRecordStatus" ).AsGuid(), context );
            var defaultRecordType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid(), context );
            var defaultConnectionStatus = DefinedValueCache.Get( GetAttributeValue( "DefaultConnectionStatus" ).AsGuid(), context );
            var phoneNumberTypeGuids = GetAttributeValue( "AvailablePhoneNumberTypes" ).SplitDelimitedValues( false );
            var completedsurveyAttribute = AttributeCache.Get( GetAttributeValue( "SurveyCompleteDateAttribute" ).AsGuid(), context );
            var verifyPhotoGroupGuid = "2108EF9C-10DC-4466-973D-D25AAB7818BE".AsGuid();

            var personContext = new RockContext();
            var personService = new PersonService( personContext );
            var groupService = new GroupService( personContext );

            var surveyCompleteGroupGuid = GetAttributeValue( "RespondantGroup" ).AsGuidOrNull();

           
            Group respondantGroup = null;

            if ( surveyCompleteGroupGuid.HasValue )
            {
                respondantGroup = new GroupService( context ).Get( surveyCompleteGroupGuid.Value );
            }

            var photoVerifyGroup = new GroupService( context ).Get( verifyPhotoGroupGuid );

            var primaryFamily = default( Group );
            var respondantid = default( int? );

            if ( CurrentFamily.FamilyId == 0 )
            {
                var respondant = CurrentFamily.FamilyMembers.FirstOrDefault( fm => fm.IsRespondant );
                var respondantMobilePhone = respondant.Person.PhoneNumbers.Where( pn => pn.NumberTypeValueId == mobilePhoneNumberTypeId ).Select( pn => pn.Number ).SingleOrDefault();

                var query = new PersonService.PersonMatchQuery( respondant.Person.FirstName, respondant.Person.LastName,
                    respondant.Person.Email, respondantMobilePhone, respondant.Person.Gender, respondant.Person.BirthDate );
                var person = personService.FindPerson( query, false );

                if ( person != null )
                {
                    primaryFamily = person.GetFamily( personContext );
                }
            }
            else if ( CurrentFamily.FamilyId > 0 )
            {
                primaryFamily = groupService.Get( CurrentFamily.FamilyId );
            }
            if ( primaryFamily == null )
            {
                primaryFamily = new Group();

                groupService.Add( primaryFamily );
                var adults = CurrentFamily.FamilyMembers
                        .Where( fm => fm.FamilyMemberRole.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                        .OrderBy( fm => fm.Person.Gender );
                var familyName = String.Empty;
                if ( adults.Count() > 1 )
                {
                    var adult1 = adults.First();
                    var adult2 = adults.Skip( 1 ).Take( 1 ).First();
                    familyName = string.Format( "{0}, {1} & {2}", adult1.Person.LastName, adult1.Person.NickName, adult2.Person.NickName );
                }
                if ( adults.Count() == 1 )
                {
                    familyName = string.Format( "{0}, {1}", adults.First().Person.LastName, adults.First().Person.NickName );
                }
                else
                {
                    var person1 = CurrentFamily.FamilyMembers.OrderBy( fm => fm.Order ).FirstOrDefault();
                    familyName = string.Format( "{0}, {1}", person1.Person.LastName, person1.Person.NickName );
                }
                primaryFamily.Name = familyName;
                primaryFamily.GroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(), context ).Id;
            }

            if ( primaryFamily.Campus.Id != CurrentFamily.PrimaryCampus.Id )
            {
                primaryFamily.CampusId = CurrentFamily.PrimaryCampus.Id;
            }

            if ( !primaryFamily.IsActive )
            {
                primaryFamily.IsActive = true;
            }

            var homeLocation = primaryFamily.GroupLocations.Where( l => l.GroupLocationTypeValueId == homeLocationTypeId ).FirstOrDefault();

            if ( homeLocation != null && homeLocation.LocationId != CurrentFamily.HomeAddress.LocationId )
            {
                homeLocation.GroupLocationTypeValueId = previousAddressId;
                homeLocation.IsMailingLocation = false;
                homeLocation.IsMappedLocation = false;
                primaryFamily.GroupLocations.Add( new GroupLocation()
                {
                    GroupLocationTypeValueId = homeLocationTypeId,
                    LocationId = CurrentFamily.HomeAddress.LocationId,
                    IsMailingLocation = CurrentFamily.HomeAddress.IsMailingLocation,
                    IsMappedLocation = CurrentFamily.HomeAddress.IsMappedLocation
                } );
            }
            else if ( homeLocation == null )
            {
                primaryFamily.GroupLocations.Add( new GroupLocation()
                {
                    GroupLocationTypeValueId = homeLocationTypeId,
                    LocationId = CurrentFamily.HomeAddress.LocationId,
                    IsMailingLocation = CurrentFamily.HomeAddress.IsMailingLocation,
                    IsMappedLocation = CurrentFamily.HomeAddress.IsMappedLocation
                } );
            }
            else 
            {
                if ( homeLocation.IsMailingLocation != CurrentFamily.HomeAddress.IsMailingLocation )
                {
                    homeLocation.IsMailingLocation = CurrentFamily.HomeAddress.IsMailingLocation;
                }
                if ( homeLocation.IsMappedLocation != CurrentFamily.HomeAddress.IsMappedLocation )
                {
                    homeLocation.IsMappedLocation = CurrentFamily.HomeAddress.IsMappedLocation;
                }
            }

            personContext.SaveChanges();

            foreach ( var fm in CurrentFamily.FamilyMembers.OrderBy( o => o.Order ) )
            {
                var primaryFamilyMember = default( GroupMember );
                bool photoChanged = false;

                if ( fm.PersonId > 0 )
                {
                    primaryFamilyMember = primaryFamily.Members.Where( m => m.PersonId == fm.PersonId ).FirstOrDefault();
                }

                if ( primaryFamilyMember == null )
                {
                    primaryFamilyMember = new GroupMember()
                    {
                        GroupId = primaryFamily.Id,
                        GroupMemberStatus = GroupMemberStatus.Active,
                        GroupRoleId = fm.FamilyMemberRole.Id,
                        Person = new Person()
                        {
                            RecordTypeValueId = defaultRecordType.Id,
                            RecordStatusValueId = defaultRecordStatus.Id,
                            ConnectionStatusValueId = defaultConnectionStatus.Id,
                            GivingGroupId = primaryFamily.Id
                        }
                    };
                    primaryFamily.Members.Add( primaryFamilyMember );

                }

                if ( fm.Person.PhotoId != primaryFamilyMember.Person.PhotoId )
                {
                    if ( fm.Person.PhotoId != null )
                    {
                        photoChanged = true;
                    }
                    primaryFamilyMember.Person.PhotoId = fm.Person.PhotoId;
                }

                if ( !fm.Person.FirstName.Equals( primaryFamilyMember.Person.FirstName, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    primaryFamilyMember.Person.FirstName = fm.Person.FirstName.Trim();
                }
                if ( !fm.Person.NickName.Equals( primaryFamilyMember.Person.NickName, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    primaryFamilyMember.Person.NickName = fm.Person.NickName.Trim();
                }
                if ( !fm.Person.LastName.Equals( primaryFamilyMember.Person.LastName, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    primaryFamilyMember.Person.LastName = fm.Person.LastName.Trim();
                }

                if ( fm.Person.Gender != primaryFamilyMember.Person.Gender )
                {
                    primaryFamilyMember.Person.Gender = fm.Person.Gender;
                }

                if ( fm.Person.MaritalStatusValueId != primaryFamilyMember.Person.MaritalStatusValueId )
                {
                    primaryFamilyMember.Person.MaritalStatusValueId = fm.Person.MaritalStatusValueId ;
                }

                if ( fm.Person.BirthDate != primaryFamilyMember.Person.BirthDate )
                {
                    primaryFamilyMember.Person.SetBirthDate( fm.Person.BirthDate );
                }

                if ( fm.Person.GradeOffset != primaryFamilyMember.Person.GradeOffset )
                {
                    primaryFamilyMember.Person.GradeOffset = fm.Person.GradeOffset;
                }

                if ( !fm.Person.Email.Equals( primaryFamilyMember.Person.Email, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    primaryFamilyMember.Person.Email = fm.Person.Email.Trim();
                }

                if ( fm.Person.EmailPreference != primaryFamilyMember.Person.EmailPreference )
                {
                    primaryFamilyMember.Person.EmailPreference = fm.Person.EmailPreference;
                }

                if ( fm.Person.IsEmailActive != primaryFamilyMember.Person.IsEmailActive )
                {
                    primaryFamilyMember.Person.IsEmailActive = fm.Person.IsEmailActive;
                }

                var phoneService = new PhoneNumberService( personContext );
                foreach ( var phoneTypeGuid in phoneNumberTypeGuids )
                {
                    var phoneTypeId = DefinedValueCache.Get( phoneTypeGuid.AsGuid(), context ).Id;

                    var enteredNumber = fm.Person.PhoneNumbers.Where( pn => pn.NumberTypeValueId == phoneTypeId ).FirstOrDefault();
                    var currentNumber = primaryFamilyMember.Person.PhoneNumbers.Where( pn => pn.NumberTypeValueId == phoneTypeId ).FirstOrDefault();

                    if ( currentNumber != null && enteredNumber == null )
                    {
                        primaryFamilyMember.Person.PhoneNumbers.Remove( currentNumber );
                        phoneService.Delete( currentNumber );
                    }
                    else if ( currentNumber == null && enteredNumber != null )
                    {
                        currentNumber = new PhoneNumber();
                        currentNumber.NumberTypeValueId = phoneTypeId;
                        primaryFamilyMember.Person.PhoneNumbers.Add( currentNumber );
                    }

                    if ( enteredNumber != null )
                    {
                        if ( currentNumber.Number != enteredNumber.Number )
                        {
                            currentNumber.Number = enteredNumber.Number;
                        }

                        if ( currentNumber.NumberFormatted != enteredNumber.NumberFormatted )
                        {
                            currentNumber.NumberFormatted = enteredNumber.NumberFormatted;
                        }

                        if ( currentNumber.CountryCode != enteredNumber.CountryCode )
                        {
                            currentNumber.CountryCode = enteredNumber.CountryCode;
                        }

                        if ( currentNumber.IsMessagingEnabled != enteredNumber.IsMessagingEnabled )
                        {
                            currentNumber.IsMessagingEnabled = enteredNumber.IsMessagingEnabled;
                        }
                    }

                }

                personContext.SaveChanges();
                if ( fm.IsRespondant )
                {
                    hfRespondant.Value = primaryFamilyMember.Person.Guid.ToString();
                    respondantid = primaryFamilyMember.Person.Id;
                }

                primaryFamilyMember.Person.LoadAttributes( personContext );
                primaryFamilyMember.Person.SetAttributeValue( completedsurveyAttribute.Key, RockDateTime.Now );              
                primaryFamilyMember.Person.SaveAttributeValues( personContext );

                if ( respondantGroup != null )
                {
                    var roleId = GroupTypeCache.Get( respondantGroup.GroupTypeId, context ).DefaultGroupRoleId;
                    var gmService = new GroupMemberService( personContext );

                    var gm = gmService.Queryable()
                        .Where( m => m.GroupId == respondantGroup.Id )
                        .Where( m => m.PersonId == primaryFamilyMember.Person.Id )
                        .FirstOrDefault();

                    if ( gm == null )
                    {
                        gm = new GroupMember();
                        gm.GroupId = respondantGroup.Id;
                        gm.PersonId = primaryFamilyMember.Person.Id;
                        gm.GroupMemberStatus = GroupMemberStatus.Active;
                        gm.GroupRoleId = roleId.Value;
                        gmService.Add( gm );

                    }
                    else if ( gm != null && gm.GroupMemberStatus != GroupMemberStatus.Active )
                    {
                        gm.GroupMemberStatus = GroupMemberStatus.Active;
                    }
                    personContext.SaveChanges();
                }

                if ( photoChanged )
                {
                    var roleId = GroupTypeCache.Get( photoVerifyGroup.GroupType ).DefaultGroupRoleId;
                    var gmService = new GroupMemberService( personContext );

                    var gm = gmService.Queryable()
                        .Where( m => m.GroupId == photoVerifyGroup.Id )
                        .Where( m => m.PersonId == primaryFamilyMember.Person.Id )
                        .Where( m => m.GroupRoleId == roleId.Value )
                        .FirstOrDefault();

                    if ( gm == null )
                    {
                        gm = new GroupMember();
                        gm.GroupId = photoVerifyGroup.Id;
                        gm.PersonId = primaryFamilyMember.Person.Id;
                        gm.GroupRoleId = roleId.Value;
                        gm.GroupMemberStatus = GroupMemberStatus.Active;
                        gmService.Add( gm );
                    }
                    else if ( gm != null && gm.GroupMemberStatus != GroupMemberStatus.Active )
                    {
                        gm.GroupMemberStatus = GroupMemberStatus.Active;
                    }

                }

                if ( !fm.IsActive )
                {
                    StartProfileUpdateWorkflow( AnnualSurvey_InactivatePersonDVGuid, primaryFamilyMember.Person );
                }
                if ( fm.RemoveFromFamily )
                {
                    StartProfileUpdateWorkflow( AnnualSurvey_RemoveFromFamilyDVGuid, primaryFamilyMember.Person );
                }


            }

            CurrentFamily = new AnnualSurveyFamily( context, primaryFamily, respondantid );


        }

        private void StartProfileUpdateWorkflow( string changeTypeGuid, Person person )
        {
            var workflowType = new WorkflowTypeService( context ).Get( AnnualSurvey_ProfileChangeRequestGuid.AsGuid() );
            //var noLongerAttendsDefinedValue = DefinedValueCache.Get( AnnualSurvey_FamilyNoLongerAttendsDVGuid.AsGuid(), context );

            var respondant = new PersonService( context ).Get( CurrentFamily.FamilyMembers.Where( fm => fm.IsRespondant ).FirstOrDefault().PersonId );

            if ( workflowType != null && respondant != null )
            {
                Dictionary<string, string> workflowAttributes = new Dictionary<string, string>();
                workflowAttributes.Add( "RequestType", changeTypeGuid );
                workflowAttributes.Add( "PersonAffected", person.PrimaryAlias.Guid.ToString() );
                workflowAttributes.Add( "Requester", respondant.PrimaryAlias.Guid.ToString() );
                string name = string.Empty;
                if ( changeTypeGuid.Equals( AnnualSurvey_FamilyNoLongerAttendsDVGuid, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    name = string.Format( "{0} Family No Longer Attends", CurrentFamily.FamilyName );
                }
                else
                {
                    name = string.Format( "{0} - Annual Survey Profile Change Request", person.FullName );
                }
                
                var transaction = new Rock.Transactions.LaunchWorkflowTransaction<Group>( workflowType.Id, name, CurrentFamily.FamilyId );
                transaction.WorkflowAttributeValues = workflowAttributes;
                Rock.Transactions.RockQueue.GetStandardQueuedTransactions().Add( transaction );
            }
        }

        private void UpdateFamily()
        {
            if ( !Page.IsValid )
            {
                return;
            }
            Group family = null;
            var showNoLongerAttends = GetAttributeValue( "ShowNoLongerAttendsCheckbox" ).AsBoolean();

            if ( showNoLongerAttends && cbNoLongerAttend.Checked )
            {
                CurrentFamily.CurrentlyAttends = false;
                CurrentFamily.NeedsUpdate = true;
                CurrentFamily.AnswerSurveyQuestions = false;
                CurrentStep = AnnualSurveyStep.NO_LONGER_ATTENDS_CONFIRM;
                //LoadSurvey();
                return;
            }

            if ( CurrentFamily.FamilyId > 0 )
            {
                family = new GroupService( context ).Get( CurrentFamily.FamilyId );
            }
            if ( cpFamilyCampus.SelectedCampusId.HasValue && CurrentFamily.PrimaryCampus.Id != cpFamilyCampus.SelectedCampusId )
            {
                CurrentFamily.PrimaryCampus = CampusCache.Get( cpFamilyCampus.SelectedCampusId.Value, context );
                CurrentFamily.NeedsUpdate = true;
            }

            if ( !String.IsNullOrEmpty( apHomeAddress.Street1 ) )
            {
                var location = new Location();
                apHomeAddress.GetValues( location );
                var location2 = new LocationService( context ).Get( location.Street1, location.Street2, location.City, location.State, location.PostalCode, location.Country, family, true, true );

                if ( location2 != null )
                {
                    if ( CurrentFamily.HomeAddress == null )
                    {
                        CurrentFamily.HomeAddress = new GroupLocation();
                        CurrentFamily.HomeAddress.LocationId = location2.Id;
                        CurrentFamily.HomeAddress.Location = location2;
                        CurrentFamily.HomeAddress.IsMailingLocation = cbIsMailingAddress.Checked;
                        CurrentFamily.HomeAddress.IsMappedLocation = cbIsPhysicalAddress.Checked;
                    }
                    else
                    {
                        CurrentFamily.HomeAddress.LocationId = location2.Id;
                        CurrentFamily.HomeAddress.Location = location2;
                        CurrentFamily.HomeAddress.IsMailingLocation = cbIsMailingAddress.Checked;
                        CurrentFamily.HomeAddress.IsMappedLocation = cbIsPhysicalAddress.Checked;
                    }
                    CurrentFamily.NeedsUpdate = true;
                }
            }

            CurrentStep = AnnualSurveyStep.FAMILY_MEMBER_INFO;
            //LoadSurvey();
        }



        private void UpdateFamilyMembers( bool advanceToNextStep )
        {
            foreach ( var fm in CurrentFamily.FamilyMembers.OrderBy( m => m.Order ) )
            {
                string controlId = string.Format( "fmc_{0}", fm.Order );
                var familyMemberControl = (AnnualSurveyPersonInformation)phFamilyMembers.FindControl( controlId );

                familyMemberControl.UpdatePerson();
                fm.Person = familyMemberControl.CurrentPerson;
                fm.IsActive = familyMemberControl.IsActivelyAttending;
                fm.RemoveFromFamily = !familyMemberControl.IsMemberOfFamily;
            }

            if ( advanceToNextStep )
            {
                SaveFamily();
                CurrentStep = AnnualSurveyStep.ASK_MORE_QUESTIONS;
            }
        }

        private void UpdateNoLongerAttends()
        {
            if ( CurrentFamily.CurrentlyAttends )
            {
                CurrentStep = AnnualSurveyStep.FAMILY_INFO;
                //LoadSurvey();
                return;
            }
            var respondant = new PersonService( context ).Get( CurrentFamily.FamilyMembers.Where( fm => fm.IsRespondant ).FirstOrDefault().PersonId );
            if ( respondant != null )
            {
                StartProfileUpdateWorkflow( AnnualSurvey_FamilyNoLongerAttendsDVGuid, respondant );
            }


            CurrentStep = AnnualSurveyStep.SURVEY_COMPLETE;
            //LoadSurvey();
        }

        private bool UpdateSurveyQuestions()
        {
            if ( !Page.IsValid )
            {
                return false;
            }
            var respondantGuid = CurrentFamily.FamilyMembers.Where( fm => fm.IsRespondant ).FirstOrDefault().Person.Guid;

            var personRockContext = new RockContext();
            var respondant = new PersonService( personRockContext ).Get( respondantGuid );

            respondant.LoadAttributes( personRockContext );

           var attributesAreValid = GetSurveyAttributeValues( phSurveyQuestions, respondant, true );


            respondant.SaveAttributeValues( personRockContext );

            if ( !attributesAreValid )
            {
                LoadSurveyQuestionAttributes( true );

                return false;
            }

            CurrentStep = AnnualSurveyStep.SURVEY_COMPLETE;
            return true;
        }

        private bool GetSurveyAttributeValues( Control parentControl, Person person, bool setEmptyValue )
        {
            bool attributesAreValid = true;
            var invalidAttributes = new List<string>();
            if ( person.Attributes != null )
            {
                foreach ( var attribute in person.Attributes )
                {
                    Control control = parentControl.FindControl( string.Format( "attribute_field_{0}", attribute.Value.Id ) );
                    if ( control != null )
                    {
                        var value = new AttributeValueCache();
                        value.Value = attribute.Value.FieldType.Field.GetEditValue( control, attribute.Value.QualifierValues );
                        if(attribute.Value.IsRequired && value.Value.IsNullOrWhiteSpace())
                        {
                            attributesAreValid = false;
                            invalidAttributes.Add( attribute.Value.Name );
                        }
                        else if ( setEmptyValue || value.Value.IsNotNullOrWhiteSpace() )
                        {
                            person.AttributeValues[attribute.Key] = value;
                        }
                    }
                }

                if ( !attributesAreValid )
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append( "Please correct the following:" );
                    sb.Append( "<ul>" );
                    foreach ( var attributeName in invalidAttributes )
                    {
                        sb.AppendFormat( "<li>{0} is required.</li>", attributeName );
                    }
                    sb.Append( "</ul>" );

                    nbError.NotificationBoxType = NotificationBoxType.Validation;
                    nbError.Text = sb.ToString();
                    nbError.Visible = true;

                    SetFocusToTopOfForm();
                }
            }
            return attributesAreValid;
        }

        #endregion



    }

    public enum AnnualSurveyStep
    {
        WELCOME = 0,
        FAMILY_INFO = 1,
        NO_LONGER_ATTENDS_CONFIRM = 2,
        FAMILY_MEMBER_INFO = 3,
        ASK_MORE_QUESTIONS = 5,
        ANSWER_SURVEY_QUESTIONS = 6,
        SURVEY_COMPLETE = 9
    }

    [Serializable]
    public class AnnualSurveyFamily : Rock.Lava.ILiquidizable
    {
        #region Fields

        #endregion

        #region Properties
        public int FamilyId { get; set; }
        public string FamilyName { get; set; }
        public Guid FamilyGuid { get; set; }
        public CampusCache PrimaryCampus { get; set; }
        public GroupLocation HomeAddress { get; set; }
        public List<AnnualSurveyPerson> FamilyMembers { get; set; }
        public bool CurrentlyAttends { get; set; }
        public bool NeedsUpdate { get; set; }
        public bool AnswerSurveyQuestions { get; set; }
        public bool SurveyQuestionsLoaded { get; set; }

        [Rock.Lava.LavaHiddenAttribute]
        public List<string> AvailableKeys
        {
            get
            {
                var availableKeys = new List<string> { "FamilyId", "FamilyName", "FamilyGuid", "FamilyMembers", "PrimaryCampus", "CurrentlyAttends", "NeedsUpdate" };
                if ( this.HomeAddress != null )
                {
                    availableKeys.AddRange( this.HomeAddress.AvailableKeys );
                }

                return availableKeys;
            }
        }

        [Rock.Lava.LavaHiddenAttribute]
        public object this[object key]
        {
            get
            {
                switch ( key.ToStringSafe() )
                {
                    case "FamilyId": return FamilyId;
                    case "FamilyName": return FamilyName;
                    case "FamilyGuid": return FamilyGuid;
                    case "FamilyMembers": return FamilyMembers;
                    case "PrimaryCampus": return PrimaryCampus;
                    case "CurrentlyAttends": return CurrentlyAttends;
                    case "NeedsUpdate": return NeedsUpdate;
                    default: return HomeAddress;
                }
            }
        }


        #endregion

        #region Constructor
        public AnnualSurveyFamily() { }

        public AnnualSurveyFamily( RockContext context )
        {
            InitializeFamily( context, null, null );
        }

        public AnnualSurveyFamily( RockContext context, Group family, int? respondantPersonId )
        {
            InitializeFamily( context, family, respondantPersonId );
        }
        #endregion

        #region Public Methods

        public void OrderFamilyMembers()
        {
            if ( FamilyMembers == null || FamilyMembers.Count() == 0 )
            {
                return;
            }
            int order = 0;
            var roles = GroupTypeCache.GetFamilyGroupType().Roles;
            var adultRole = roles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).SingleOrDefault();
            var childRole = roles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).SingleOrDefault();


            var orderedFamilyMemberIds = new List<int>();
            //get respondant
            var respondant = FamilyMembers.Where( m => m.IsRespondant ).FirstOrDefault();

            if ( respondant != null )
            {
                respondant.Order = order;
                orderedFamilyMemberIds.Add( respondant.PersonId );
                order++;
            }
            //get adults

            var adults = FamilyMembers
                .Where( fm => fm.FamilyMemberRoleId == adultRole.Id )
                .Where( fm => !orderedFamilyMemberIds.Contains( fm.PersonId ) )
                .OrderBy( fm => fm.Person.Gender )
                .ThenBy( fm => fm.Person.BirthDate )
                .ToList();

            foreach ( var a in adults )
            {
                a.Order = order;
                orderedFamilyMemberIds.Add( a.PersonId );
                order++;
            }


            //get children
            var children = FamilyMembers
                .Where( fm => fm.FamilyMemberRoleId == childRole.Id )
                .Where( fm => !orderedFamilyMemberIds.Contains( fm.PersonId ) )
                .OrderBy( fm => fm.Person.BirthDate )
                .ToList();

            foreach ( var c in children )
            {
                c.Order = order;
                orderedFamilyMemberIds.Add( c.PersonId );
                order++;
            }

            //get other
            var other = FamilyMembers.Where( fm => !orderedFamilyMemberIds.Contains( fm.PersonId ) )
                .OrderBy( fm => fm.Person.Id )
                .ToList();

            foreach ( var fm in other )
            {
                fm.Order = order;
                orderedFamilyMemberIds.Add( fm.PersonId );
                order++;
            }

        }

        #endregion

        #region Private Methds
        private CampusCache GetDefaultCampus( RockContext context )
        {
            var defaultCampusId = new CampusService( context ).Queryable()
                .AsNoTracking()
                .Where( c => c.IsActive == true )
                .OrderBy( c => c.Order )
                .ThenBy( c => c.Name )
                .Select( c => c.Id )
                .FirstOrDefault();

            return CampusCache.Get( defaultCampusId, context );
        }

        private void InitializeFamily( RockContext context, Group family, int? respondantPersonId )
        {
            NeedsUpdate = false;
            CurrentlyAttends = true;
            AnswerSurveyQuestions = true;
            SurveyQuestionsLoaded = false;

            if ( family == null )
            {
                var adult = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Roles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).FirstOrDefault();
                FamilyId = 0;
                FamilyGuid = Guid.NewGuid();
                FamilyName = null;
                PrimaryCampus = GetDefaultCampus( context );
                HomeAddress = null;
                FamilyMembers = new List<AnnualSurveyPerson>();
                FamilyMembers.Add( new AnnualSurveyPerson
                {
                    PersonId = 0,
                    IsRespondant = true,
                    FamilyMemberRole = adult,
                    FamilyMemberRoleId = adult.Id,
                    Person = new Person(),
                    IsActive = true,
                    Order = 0

                } );
            }
            else
            {
                FamilyId = family.Id;
                FamilyName = family.Name;
                FamilyGuid = family.Guid;

                if ( family.CampusId.HasValue )
                {
                    PrimaryCampus = CampusCache.Get( family.CampusId.Value, context );
                }
                else
                {
                    PrimaryCampus = GetDefaultCampus( context );
                }
                var homeAddressType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid(), context );
                HomeAddress = family.GroupLocations
                    .Where( l =>
                        l.GroupLocationTypeValueId.HasValue &&
                        l.GroupLocationTypeValueId == homeAddressType.Id )
                    .FirstOrDefault();

                var inactivePersonStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid(), context );

                var activeFamilyMembers = family.Members.Where( fm => fm.GroupMemberStatus == GroupMemberStatus.Active )
                    .Where( fm => fm.Person.RecordStatusValueId != inactivePersonStatus.Id )
                    .ToList();
                FamilyMembers = AnnualSurveyPerson.LoadFamilyMembers( context, activeFamilyMembers, respondantPersonId );
                OrderFamilyMembers();
            }
        }

        public object ToLiquid()
        {
            return this;
        }

        public bool ContainsKey( object key )
        {
            var additionalKeys = new List<string> { "FamilyId", "FamilyName", "FamilyGuid", "FamilyMembers", "PrimaryCampus", "CurrentlyAttends", "NeedsUpdate" };

            if ( additionalKeys.Contains( key.ToStringSafe() ) )
            {
                return true;
            }
            else
            {
                return HomeAddress.ContainsKey( key.ToStringSafe() );
            }

        }
        #endregion
    }

    [Serializable]
    [DotLiquid.LiquidType( "PersonId", "FamilyMemberRoleId", "FamilyMemberRole", "Person", "IsActive", "Order", "IsRespondant", "NeedsUpdate", "RemoveFromFamily" )]
    public class AnnualSurveyPerson
    {
        #region Fields
        private bool _NeedsUpdate = false;
        private bool _RemoveFromFamily = false;
        private bool _isRespondant = false;
        private GroupTypeRoleCache _familyMemberRole;
        #endregion

        #region Properties
        public int PersonId { get; set; }
        public int FamilyMemberRoleId { get; set; }

        public GroupTypeRoleCache FamilyMemberRole
        {
            get
            {
                if ( _familyMemberRole == null && FamilyMemberRoleId > 0 )
                {
                    _familyMemberRole = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ).Roles.Where( r => r.Id == FamilyMemberRoleId ).SingleOrDefault();
                }
                return _familyMemberRole;
            }
            set
            {
                _familyMemberRole = value;
            }


        }

        public Person Person { get; set; }
        public bool IsActive { get; set; }
        public int Order { get; set; }

        public bool IsRespondant
        {
            get
            {
                return _isRespondant;
            }
            set
            {
                _isRespondant = value;
            }
        }

        private bool NeedsUpdate
        {
            get
            {
                return _NeedsUpdate;
            }
            set
            {
                _NeedsUpdate = value;
            }
        }

        public bool RemoveFromFamily
        {
            get
            {
                return _RemoveFromFamily;
            }
            set
            {
                _RemoveFromFamily = value;
            }
        }


        public string OtherUpdate { get; set; }
        #endregion

        #region PublicMethods
        public static List<AnnualSurveyPerson> LoadFamilyMembers( RockContext context, List<GroupMember> familyMembers, int? respondantPersonId )
        {
            var roles = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(), context ).Roles;
            var surveyPeople = new List<AnnualSurveyPerson>();
            foreach ( var member in familyMembers )
            {
                var fm = new AnnualSurveyPerson();
                fm.PersonId = member.PersonId;
                fm.FamilyMemberRoleId = member.GroupRoleId;
                fm.FamilyMemberRole = roles.Where( r => r.Id == member.GroupRoleId ).SingleOrDefault();

                fm.Person = member.Person;
                fm.IsActive = true;

                if ( respondantPersonId.HasValue )
                {
                    fm.IsRespondant = fm.PersonId == respondantPersonId.Value;
                }

                fm.NeedsUpdate = false;
                fm.RemoveFromFamily = false;
                surveyPeople.Add( fm );
            }

            return surveyPeople;

        }
        #endregion
    }

}