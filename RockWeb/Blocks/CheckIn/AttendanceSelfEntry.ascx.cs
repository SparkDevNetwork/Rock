// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName( "Attendance Self Entry" )]
    [Category( "Check-in" )]
    [Description( "Allows quick self service attendance recording." )]

    #region Block Attributes

    [GroupTypeField(
        "Check-in Configuration",
        groupTypePurposeValueGuid: Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE,
        Description = "This will be the group type that we will use to determine where to check them in.",
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.GroupType.GROUPTYPE_SERVICE_ATTENDANCE,
        Order = 0,
        Key = AttributeKey.CheckinConfiguration )]
    [BooleanField(
        "Primary Person Birthday Shown",
        Description = "Should birthday be displayed for primary person?",
        DefaultBooleanValue = true,
        Order = 1,
        Key = AttributeKey.PrimaryPersonBirthdayShown )]
    [BooleanField(
        "Primary Person Birthday Required",
        Description = "Determine if birthday for primary person is required.",
        DefaultBooleanValue = false,
        Order = 2,
        Key = AttributeKey.PrimaryPersonBirthdayRequired )]
    [BooleanField(
        "Primary Person Address Shown",
        Description = "Should address be displayed for primary person?",
        DefaultBooleanValue = true,
        Order = 3,
        Key = AttributeKey.PrimaryPersonAddressShown )]
    [BooleanField(
        "Primary Person Address Required",
        Description = "Determine if address for primary person is required.",
        DefaultBooleanValue = false,
        Order = 4,
        Key = AttributeKey.PrimaryPersonAddressRequired )]
    [BooleanField(
        "Primary Person Mobile Phone Shown",
        Description = "Should mobile phone be displayed for primary person?",
        DefaultBooleanValue = true,
        Order = 5,
        Key = AttributeKey.PrimaryPersonMobilePhoneShown )]
    [BooleanField(
        "Primary Person Mobile Phone Required",
        Description = "Determine if mobile phone for primary person is required.",
        DefaultBooleanValue = false,
        Order = 6,
        Key = AttributeKey.PrimaryPersonMobilePhoneRequired )]
    [BooleanField(
        "Other Person Birthday Shown",
        Description = "Should birthday be displayed for other person?",
        DefaultBooleanValue = true,
        Order = 7,
        Key = AttributeKey.OtherPersonBirthdayShown )]
    [BooleanField(
        "Other Person Birthday Required",
        Description = "Determine if birthday for other person is required.",
        DefaultBooleanValue = false,
        Order = 8,
        Key = AttributeKey.OtherPersonBirthdayRequired )]
    [BooleanField(
        "Other Person Mobile Phone Shown",
        Description = "Should mobile phone be displayed for other person?",
        DefaultBooleanValue = true,
        Order = 9,
        Key = AttributeKey.OtherPersonMobilePhoneShown )]
    [BooleanField(
        "Other Person Mobile Phone Required",
        Description = "Determine if mobile phone for other person is required.",
        DefaultBooleanValue = false,
        Order = 10,
        Key = AttributeKey.OtherPersonMobilePhoneRequired )]
    [CustomEnhancedListField(
        "Known Relationship Types",
        description: "A checkbox list of Known Relationship types that should be included in the Relation dropdown.",
        listSource: @"
SELECT 
	R.[Guid] AS [Value],
	R.[Name] AS [Text]
FROM [GroupType] T
INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id]
WHERE T.[Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
AND R.[Name] <> 'Child'
ORDER BY [Text]",
        DefaultValue = "6b05f48e-5235-45de-970e-fe145bd28e1d",
        IsRequired = false,
        Order = 11,
        Key = AttributeKey.KnownRelationshipTypes )]
    [CodeEditorField(
        "Redirect URL",
        Description = "The URL to redirect the individual to when they check-in. The merge fields that are available includes 'PersonAliasGuid'.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        IsRequired = false,
        Order = 12,
        Key = AttributeKey.RedirectURL )]
    [TextField(
        "Check-in Button Text",
        Description = "The text that should be shown on the check-in button.",
        IsRequired = false,
        DefaultValue = "Check-in",
        Order = 13,
        Key = AttributeKey.CheckinButtonText )]
    [WorkflowTypeField(
        "Workflow",
        AllowMultiple = false,
        Key = AttributeKey.Workflow,
        Description = "The optional workflow type to launch when a person is checked in. The primary person will be passed to the workflow as the entity. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: FamilyPersonIds, OtherPersonIds.",
        IsRequired = false,
        Order = 14 )]
    #region Messages Block Attribute Settings

    [TextField(
        "Unknown Individual Panel 1 Title",
        Description = "The title to display on the primary watcher panel.",
        IsRequired = false,
        Category = AttributeCategory.Messages,
        Order = 15,
        DefaultValue = "Tell Us a Little About You...",
        Key = AttributeKey.UnknownIndividualPanel1Title )]
    [CodeEditorField(
        "Unknown Individual Panel 1 Intro Text",
        Description = "The intro text to display on the primary watcher panel.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        Category = AttributeCategory.Messages,
        Order = 16,
        DefaultValue = " We love to learn a little about you so that we can best serve you and your family.",
        Key = AttributeKey.UnknownIndividualPanel1IntroText )]
    [TextField(
        "Unknown Individual Panel 2 Title",
        Description = "The title to display on the other watcher panel.",
        IsRequired = false,
        Category = AttributeCategory.Messages,
        Order = 17,
        DefaultValue = "Who Else Is Joining You?",
        Key = AttributeKey.UnknownIndividualPanel2Title )]
    [CodeEditorField(
        "Unknown Individual Panel 2 Intro Text",
        Description = "The intro text to display on the other watcher panel.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "We'd love to know more about others watching with you.",
        Category = AttributeCategory.Messages,
        Order = 18,
        Key = AttributeKey.UnknownIndividualPanel2IntroText )]
    [TextField(
        "Unknown Individual Panel 3 Title",
        Description = "The title to display on the account panel.",
        IsRequired = false,
        Category = AttributeCategory.Messages,
        DefaultValue = "Would You Like to Create An Account?",
        Order = 19,
        Key = AttributeKey.UnknownIndividualPanel3Title )]
    [CodeEditorField(
        "Unknown Individual Panel 3 Intro Text",
        Description = "The intro text to display on the account panel.",
        DefaultValue = "Creating an account will help you to connect on our website.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        Category = AttributeCategory.Messages,
        Order = 20,
        Key = AttributeKey.UnknownIndividualPanel3IntroText )]
    [TextField(
        "Known Individual Panel 1 Title",
        Description = "The title to display on the known individual panel.",
        IsRequired = false,
        Category = AttributeCategory.Messages,
        Order = 21,
        DefaultValue = "Great to see you {{ CurrentPerson.NickName }}!",
        Key = AttributeKey.KnownIndividualPanel1Title )]
    [CodeEditorField(
        "Known Individual Panel 1 Intro Text",
        Description = "The intro text to display on the known individual panel.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        Category = AttributeCategory.Messages,
        DefaultValue = "We'd love to know who is watching with you today.",
        Order = 22,
        Key = AttributeKey.KnownIndividualPanel1IntroText )]
    [TextField(
        "Known Individual Panel 2 Title",
        Description = "The title to display on the success panel.",
        IsRequired = false,
        Category = AttributeCategory.Messages,
        Order = 23,
        DefaultValue = "Thanks for Connecting!",
        Key = AttributeKey.KnownIndividualPanel2Title )]
    [CodeEditorField(
        "Known Individual Panel 2 Intro Text",
        Description = "The intro text to display on the success panel.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "Thank you for connecting with us today. We hope that your enjoy the service!",
        Category = AttributeCategory.Messages,
        Order = 24,
        Key = AttributeKey.KnownIndividualPanel2IntroText )]
    #endregion Messages Block Attribute Settings
    #endregion Block Attributes
    public partial class AttendanceSelfEntry : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string CheckinConfiguration = "CheckinConfiguration";
            public const string PrimaryPersonBirthdayShown = "PrimaryPersonBirthdayShown";
            public const string PrimaryPersonBirthdayRequired = "PrimaryPersonBirthdayRequired";
            public const string PrimaryPersonAddressShown = "PrimaryPersonAddressShown";
            public const string PrimaryPersonAddressRequired = "PrimaryPersonAddressRequired";
            public const string PrimaryPersonMobilePhoneShown = "PrimaryPersonMobilePhoneShown";
            public const string PrimaryPersonMobilePhoneRequired = "PrimaryPersonMobilePhoneRequired";
            public const string OtherPersonBirthdayShown = "OtherPersonBirthdayShown";
            public const string OtherPersonBirthdayRequired = "OtherPersonBirthdayRequired";
            public const string OtherPersonMobilePhoneShown = "OtherPersonMobilePhoneShown";
            public const string OtherPersonMobilePhoneRequired = "OtherPersonMobilePhoneRequired";
            public const string KnownRelationshipTypes = "KnownRelationshipTypes";
            public const string RedirectURL = "RedirectURL";
            public const string CheckinButtonText = "CheckinButtonText";
            public const string Workflow = "Workflow";
            public const string UnknownIndividualPanel1Title = "UnknownIndividualPanel1Title";
            public const string UnknownIndividualPanel1IntroText = "UnknownIndividualPanel1IntroText";
            public const string UnknownIndividualPanel2Title = "UnknownIndividualPanel2Title";
            public const string UnknownIndividualPanel2IntroText = "UnknownIndividualPanel2IntroText";
            public const string UnknownIndividualPanel3Title = "UnknownIndividualPanel3Title";
            public const string UnknownIndividualPanel3IntroText = "UnknownIndividualPanel3IntroText";
            public const string KnownIndividualPanel1Title = "KnownIndividualPanel1Title";
            public const string KnownIndividualPanel1IntroText = "KnownIndividualPanel1IntroText";
            public const string KnownIndividualPanel2Title = "KnownIndividualPanel2Title";
            public const string KnownIndividualPanel2IntroText = "KnownIndividualPanel2IntroText";
        }

        #endregion

        #region Attribute Categories

        /// <summary>
        /// Keys to use for Block Attribute Categories
        /// </summary>
        private static class AttributeCategory
        {
            public const string Messages = "Messages";
        }

        #endregion

        #region ViewStateKeys

        private static class ViewStateKey
        {
            public const string OtherWatchers = "OtherWatchers";
            public const string PrimaryWatcher = "PrimaryWatcher";
            public const string Location = "Location";
        }

        #endregion ViewStateKeys

        #region Fields

        private const string ROCK_UNSECUREDPERSONIDENTIFIER = "rock_UnsecuredPersonIdentifier";
        private const string KNOWN_RELATIONSHIP_SPOUSE = "Spouse";

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the primary person that have been added by user
        /// </summary>
        /// <value>
        /// The primary person.
        /// </value>
        protected Watcher PrimaryWatcher { get; set; }

        /// <summary>
        /// Gets or sets the Location that have been added by user
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        protected Location Location { get; set; }

        /// <summary>
        /// Gets or sets the other watchers that have been added by user
        /// </summary>
        /// <value>
        /// The other watchers.
        /// </value>
        protected List<Watcher> OtherWatchers { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );


            string json = ViewState[ViewStateKey.PrimaryWatcher] as string;
            if ( json.IsNotNullOrWhiteSpace() )
            {
                PrimaryWatcher = JsonConvert.DeserializeObject<Watcher>( json );
            }

            json = ViewState[ViewStateKey.Location] as string;
            if ( json.IsNotNullOrWhiteSpace() )
            {
                Location = JsonConvert.DeserializeObject<Location>( json );
            }

            OtherWatchers = ViewState[ViewStateKey.OtherWatchers] as List<Watcher> ?? new List<Watcher>();
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            bpBirthDay.Visible = GetAttributeValue( AttributeKey.PrimaryPersonBirthdayShown ).AsBoolean();
            bpBirthDay.Required = GetAttributeValue( AttributeKey.PrimaryPersonBirthdayRequired ).AsBoolean();

            bpOtherBirthDay.Visible = GetAttributeValue( AttributeKey.OtherPersonBirthdayShown ).AsBoolean();
            bpOtherBirthDay.Required = GetAttributeValue( AttributeKey.OtherPersonBirthdayRequired ).AsBoolean();

            pnlPhone.Visible = GetAttributeValue( AttributeKey.PrimaryPersonMobilePhoneShown ).AsBoolean();
            pnbPhone.Required = GetAttributeValue( AttributeKey.PrimaryPersonMobilePhoneRequired ).AsBoolean();

            pnlOtherPhone.Visible = GetAttributeValue( AttributeKey.OtherPersonMobilePhoneShown ).AsBoolean();
            pnOtherMobile.Required = GetAttributeValue( AttributeKey.OtherPersonMobilePhoneRequired ).AsBoolean();

            acAddress.Visible = GetAttributeValue( AttributeKey.PrimaryPersonAddressShown ).AsBoolean();
            acAddress.Required = GetAttributeValue( AttributeKey.PrimaryPersonAddressRequired ).AsBoolean();

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upDetail );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Attendance Self Entry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            this.NavigateToCurrentPageReference( new Dictionary<string, string>() );
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                SetStart();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState[ViewStateKey.PrimaryWatcher] = JsonConvert.SerializeObject( PrimaryWatcher, Formatting.None, jsonSetting );
            ViewState[ViewStateKey.Location] = JsonConvert.SerializeObject( Location, Formatting.None, jsonSetting );
            ViewState[ViewStateKey.OtherWatchers] = OtherWatchers;

            return base.SaveViewState();
        }

        #endregion

        #region Event Handlers
        #region Primary Watcher Panel Events

        /// <summary>
        /// Handles the Click event of the btnLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnLogin_Click( object sender, EventArgs e )
        {
            var site = RockPage.Layout.Site;
            if ( site.LoginPageId.HasValue )
            {
                site.RedirectToLoginPage( true );
            }
            else
            {
                System.Web.Security.FormsAuthentication.RedirectToLoginPage();
            }
        }

        /// <summary>
        /// Handles the btnPrimaryNext Click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnPrimaryNext_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var personQuery = new PersonService.PersonMatchQuery( tbFirstName.Text.Trim(), tbLastName.Text.Trim(), tbEmail.Text.Trim(), pnbPhone.Text.Trim(), birthDate: bpBirthDay.SelectedDate );
            var person = personService.FindPerson( personQuery, true );

            if ( person != null )
            {
                GetData( person, rockContext );
                var userLoginService = new Rock.Model.UserLoginService( new RockContext() );
                var userLogins = userLoginService.GetByPersonId( person.Id )
                    .Where( l => l.IsLockedOut != true )
                    .ToList();
                if ( userLogins.Any() )
                {
                    SetUnsecuredPersonIdentifier( person.PrimaryAlias.Guid );
                }
            }
            else
            {
                PrimaryWatcher = new Watcher();
                PrimaryWatcher.NickName = tbFirstName.Text;
                PrimaryWatcher.LastName = tbLastName.Text;
                PrimaryWatcher.EmailAddress = tbEmail.Text;
                if ( bpBirthDay.Visible )
                {
                    PrimaryWatcher.BirthDate = bpBirthDay.SelectedDate;
                }

                if ( pnlPhone.Visible )
                {
                    PrimaryWatcher.MobilePhoneNumber = pnbPhone.Number;
                    PrimaryWatcher.MobileCountryCode = pnbPhone.CountryCode;
                    PrimaryWatcher.IsMessagingEnabled = cbIsMessagingEnabled.Checked;
                }

                if ( acAddress.Visible )
                {
                    Location = Location ?? new Location();
                    acAddress.GetValues( Location );
                }

                OtherWatchers = new List<Watcher>();
            }

            ShowOtherWatcher();
        }

        #endregion Primary Watcher Panel Events

        #region Other Watchers Panel Events

        /// <summary>
        /// Handles the ItemCommand event of the rptListed control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptListed_ItemCommand( object sender, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "delete" )
            {
                var personGuid = e.CommandArgument.ToString().AsGuid();
                OtherWatchers.RemoveAll( a => a.Guid == personGuid );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRelation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlRelation_SelectedIndexChanged( object sender, EventArgs e )
        {
            var isChildSelected = ddlRelation.SelectedValue.AsGuid() == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
            tbOtherEmail.Visible = !isChildSelected;
            pnlOtherPhone.Visible = !isChildSelected;
            if ( isChildSelected )
            {
                tbOtherEmail.Text = string.Empty;
                pnOtherMobile.Text = string.Empty;
            }
        }

        /// <summary>
        /// Handles the lbAddIndividual Click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddIndividual_Click( object sender, EventArgs e )
        {
            var otherWatcher = new Watcher();
            otherWatcher.Guid = Guid.NewGuid();
            otherWatcher.NickName = tbOtherFirstName.Text;
            otherWatcher.LastName = tbOtherLastName.Text;
            otherWatcher.RelationshipType = ddlRelation.SelectedItem.Text;
            otherWatcher.RelationshipTypeGuid = ddlRelation.SelectedValue.AsGuid();

            if ( tbOtherEmail.Visible )
            {
                otherWatcher.EmailAddress = tbOtherEmail.Text;
            }

            if ( bpOtherBirthDay.Visible )
            {
                otherWatcher.BirthDate = bpOtherBirthDay.SelectedDate;
            }

            if ( pnOtherMobile.Visible )
            {
                otherWatcher.MobilePhoneNumber = pnOtherMobile.Number;
                otherWatcher.MobileCountryCode = pnOtherMobile.CountryCode;
                otherWatcher.IsMessagingEnabled = cbOtherMessagingEnabled.Checked;
            }
            OtherWatchers.Add( otherWatcher );

            ClearNewIndividualControl();
            BindRelationDropDown();
            ddlRelation_SelectedIndexChanged( null, null );
            BindCurrentlyListed();
        }

        /// <summary>
        /// Handles the btnOtherNext Click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnOtherNext_Click( object sender, EventArgs e )
        {
            var isLoggedIn = IsLoggedIn();

            Person primaryPerson = null;
            if ( isLoggedIn )
            {
                var rockContext = new RockContext();
                primaryPerson = GetLoggedInPerson( rockContext );
                var family = primaryPerson.GetFamily( rockContext );
                AddOrUpdateOtherWatchers( rockContext, primaryPerson, family );
            }

            pnlOtherWatcher.Visible = false;
            if ( isLoggedIn )
            {
                var selectedIndividuals = cblIndividuals.SelectedValuesAsInt;
                ShowKnownIndividual();
                if ( selectedIndividuals.Any() )
                {
                    cblIndividuals.SetValues( selectedIndividuals );
                }
                else
                {
                    cblIndividuals.SetValue( primaryPerson.Id );
                }
            }
            else
            {
                ShowAccount();
            }
        }

        #endregion Other Watchers Panel Events

        /// <summary>
        /// Handles the btnAccountNext Click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnAccountNext_Click( object sender, EventArgs e )
        {
            var isAccountRequired = txtUserName.Text.IsNotNullOrWhiteSpace() || txtPassword.Text.IsNotNullOrWhiteSpace();
            var rockContext = new RockContext();

            if ( isAccountRequired )
            {
                if ( txtPassword.Text.IsNullOrWhiteSpace() )
                {
                    nbAccountWarning.Title = "Missing Information";
                    nbAccountWarning.Text = "You forget to enter the password. Password is required in order to create account.";
                    nbAccountWarning.NotificationBoxType = NotificationBoxType.Danger;
                    nbAccountWarning.Visible = true;
                    return;
                }

                if ( txtUserName.Text.IsNullOrWhiteSpace() )
                {
                    nbAccountWarning.Title = "Missing Information";
                    nbAccountWarning.Text = "You forget to enter the password. Password is required in order to create account.";
                    nbAccountWarning.NotificationBoxType = NotificationBoxType.Danger;
                    nbAccountWarning.Visible = true;
                    return;
                }

                if ( new UserLoginService( rockContext ).GetByUserName( txtUserName.Text ) != null )
                {
                    nbAccountWarning.Title = "Invalid Username";
                    nbAccountWarning.Text = "The selected Username is already being used.  Please select a different Username";
                    nbAccountWarning.NotificationBoxType = NotificationBoxType.Danger;
                    nbAccountWarning.Visible = true;
                    return;
                }

                if ( !UserLoginService.IsPasswordValid( txtPassword.Text ) )
                {
                    nbAccountWarning.Title = string.Empty;
                    nbAccountWarning.Text = UserLoginService.FriendlyPasswordRules();
                    nbAccountWarning.NotificationBoxType = NotificationBoxType.Danger;
                    nbAccountWarning.Visible = true;
                    return;
                }
            }
            Person person = null;
            var personService = new PersonService( rockContext );
            Group family = null;
            if ( PrimaryWatcher.Id != default( int ) )
            {
                person = personService.Get( PrimaryWatcher.Id );
            }

            // Otherwise create a new person
            if ( person == null )
            {
                var recordTypePersonId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                var recordStatusValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
                var connectionStatusValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() );

                person = new Person();
                person.Guid = PrimaryWatcher.Guid;
                person.FirstName = PrimaryWatcher.NickName.FixCase();
                person.LastName = PrimaryWatcher.LastName.FixCase();
                person.RecordTypeValueId = recordTypePersonId;
                person.RecordStatusValueId = recordStatusValue != null ? recordStatusValue.Id : ( int? ) null;
                person.ConnectionStatusValueId = connectionStatusValue != null ? connectionStatusValue.Id : ( int? ) null;
            }
            else
            {
                family = person.GetFamily( rockContext );
                person.NickName = PrimaryWatcher.NickName;
                person.LastName = PrimaryWatcher.LastName;
            }

            person.SetBirthDate( PrimaryWatcher.BirthDate );

            // Save the email address
            if ( PrimaryWatcher.EmailAddress.IsNotNullOrWhiteSpace() )
            {
                person.Email = PrimaryWatcher.EmailAddress;
            }


            if ( family != null )
            {
                var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );

                var adultRole = familyGroupType
                                .Roles
                                .Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                                .FirstOrDefault();
                PersonService.AddPersonToFamily( person, false, family.Id, adultRole.Id, rockContext );
            }
            else
            {
                family = PersonService.SaveNewPerson( person, rockContext );
            }

            PrimaryWatcher.Id = person.Id;

            // Save the mobile phone number
            if ( PrimaryWatcher.MobilePhoneNumber.IsNotNullOrWhiteSpace() )
            {
                SavePhoneNumber( person.Id, PrimaryWatcher, rockContext );
            }

            // Save the family address
            var homeLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
            if ( homeLocationType != null && Location != null && GetAttributeValue( AttributeKey.PrimaryPersonAddressShown ).AsBoolean() )
            {
                if ( Location.Street1.IsNotNullOrWhiteSpace() && Location.City.IsNotNullOrWhiteSpace() )
                {
                    Location = new LocationService( rockContext ).Get(
                        Location.Street1, Location.Street2, Location.City, Location.State, Location.PostalCode, Location.Country, family, true );
                }
                else
                {
                    Location = null;
                }

                // Check to see if family has an existing home address
                var groupLocation = family.GroupLocations
                    .FirstOrDefault( l =>
                        l.GroupLocationTypeValueId.HasValue &&
                        l.GroupLocationTypeValueId.Value == homeLocationType.Id );

                if ( Location != null )
                {
                    if ( groupLocation == null || groupLocation.LocationId != Location.Id )
                    {
                        // If family does not currently have a home address or it is different than the one entered, add a new address (move old address to prev)
                        GroupService.AddNewGroupAddress( rockContext, family, homeLocationType.Guid.ToString(), Location, true, string.Empty, true, true );
                    }
                }

                rockContext.SaveChanges();
            }

            if ( isAccountRequired )
            {
                var userLogin = UserLoginService.Create(
                    rockContext,
                    person,
                    Rock.Model.AuthenticationServiceType.Internal,
                    EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                    txtUserName.Text,
                    txtPassword.Text,
                    true );
            }

            AddOrUpdateOtherWatchers( rockContext, person, family );

            SetUnsecuredPersonIdentifier( person.PrimaryAlias.Guid );

            if ( isAccountRequired )
            {
                Authorization.SetAuthCookie( txtUserName.Text, false, false );
            }

            pnlAccount.Visible = false;
            ShowKnownIndividual();
            foreach ( ListItem item in cblIndividuals.Items )
            {
                item.Selected = true;
            }
        }

        #region Known Individual Panel Events

        /// <summary>
        /// Handles the lbAdjustWatchers Click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAdjustWatchers_Click( object sender, EventArgs e )
        {
            pnlKnownIndividual.Visible = false;
            ShowOtherWatcher();
        }

        /// <summary>
        /// Handles the btnCheckIn Click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCheckIn_Click( object sender, EventArgs e )
        {
            pnlKnownIndividual.Visible = false;

            var configuredGroupTypeGuid = GetAttributeValue( AttributeKey.CheckinConfiguration ).AsGuid();
            var rockContext = new RockContext();

            var groupType = GroupTypeCache.Get( configuredGroupTypeGuid );
            var descendantGroupTypeIds = new GroupTypeService( rockContext ).GetAllAssociatedDescendents( groupType.Id, 10 ).Select( a => a.Id ).ToList();

            var groups = new GroupService( rockContext )
                    .Queryable()
                    .Where( a =>
                        a.GroupTypeId == groupType.Id ||
                        descendantGroupTypeIds.Contains( a.GroupTypeId ) )
                    .IsActive();

            if ( groups.Any() )
            {
                Group attendanceGroup = null;
                int? scheduleId = null;
                int? locationId = null;
                var campusCurrentDateTime = RockDateTime.Now;

                foreach ( var group in groups )
                {
                    if ( group.CampusId.HasValue )
                    {
                        var campus = CampusCache.Get( group.CampusId.Value );
                        if ( campus != null )
                        {
                            campusCurrentDateTime = campus.CurrentDateTime;
                        }
                    }

                    foreach ( var groupLocation in group.GroupLocations )
                    {
                        var schedule = groupLocation.Schedules.Where( a => a.WasScheduleActive( campusCurrentDateTime ) ).FirstOrDefault();
                        if ( schedule != null )
                        {
                            locationId = groupLocation.LocationId;
                            scheduleId = schedule.Id;
                            attendanceGroup = group;
                            break;
                        }
                    }

                    if ( scheduleId.HasValue )
                    {
                        break;
                    }
                }

                if ( attendanceGroup == null )
                {
                    attendanceGroup = groups.First();
                }

                var attendanceService = new AttendanceService( rockContext );
                var persons = new PersonService( rockContext ).GetListByIds( cblIndividuals.SelectedValuesAsInt );
                foreach ( var person in persons )
                {
                    if ( person.PrimaryAliasId.HasValue )
                    {
                        var attendance = attendanceService.AddOrUpdate( person.PrimaryAliasId.Value, campusCurrentDateTime, attendanceGroup.Id, locationId, scheduleId, attendanceGroup.CampusId );
                    }
                }

                Guid? workflowTypeGuid = GetAttributeValue( AttributeKey.Workflow ).AsGuidOrNull();
                if ( workflowTypeGuid.HasValue )
                {
                    var workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );
                    if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                    {
                        try
                        {
                            var otherPersonIds = OtherWatchers
                                 .Where( a => a.RelationshipTypeGuid != Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() &&
                                              a.RelationshipTypeGuid != Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() &&
                                              cblIndividuals.SelectedValuesAsInt.Contains( a.Id ) )
                                 .Select( a => a.Id )
                                 .ToList();

                            var familyPersonIds = cblIndividuals.SelectedValuesAsInt.Where( a => !otherPersonIds.Contains( a ) ).ToList();
                            var primaryPerson = new PersonService( rockContext ).Get( PrimaryWatcher.Id );
                            // Create parameters
                            var parameters = new Dictionary<string, string>();
                            parameters.Add( "FamilyPersonIds", familyPersonIds.AsDelimited( "," ) );
                            parameters.Add( "OtherPersonIds", otherPersonIds.AsDelimited( "," ) );

                            primaryPerson.LaunchWorkflow( workflowTypeGuid, primaryPerson.FullName, parameters );

                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex, this.Context );
                        }
                    }
                }
            }
            else
            {
                var exception = new Exception( string.Format( "No Group Match found for the check-in configuration (GroupTyeGuid : {0}).", configuredGroupTypeGuid ) );
                ExceptionLogService.LogException( exception, Context, this.RockPage.PageId, this.RockPage.Site.Id, CurrentPersonAlias );
            }

            pnlKnownIndividual.Visible = false;
            string redirectUrl = GetAttributeValue( AttributeKey.RedirectURL );
            if ( redirectUrl.IsNotNullOrWhiteSpace() )
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.Add( "PersonAliasGuid", GetUnsecuredPersonIdentifier() );
                var url = ResolveUrl( redirectUrl.ResolveMergeFields( mergeFields ) );
                Response.Redirect( url, false );
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                ShowSuccess();
            }
        }

        #endregion Known Individual Panel Events

        #endregion

        #region Methods

        /// <summary>
        /// Sets the start panel
        /// </summary>
        private void SetStart()
        {
            if ( IsLoggedIn() )
            {
                Person person = GetLoggedInPerson();
                SetUnsecuredPersonIdentifier( person.PrimaryAlias.Guid );
                GetData( person );
                ShowKnownIndividual();
                cblIndividuals.SetValue( person.Id );
            }
            else
            {
                lPanel1Title.Text = GetAttributeValue( AttributeKey.UnknownIndividualPanel1Title );
                lPanel1Text.Text = GetAttributeValue( AttributeKey.UnknownIndividualPanel1IntroText );
                pnlPrimaryWatcher.Visible = true;
            }
        }

        /// <summary>
        /// Gets the logged in person
        /// </summary>
        private Person GetLoggedInPerson( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            Person person = null;

            if ( CurrentPersonId.HasValue )
            {
                person = new PersonService( rockContext ).Get( CurrentPersonId.Value );
            }

            if ( person == null )
            {
                var primaryPersonAliasGuid = GetUnsecuredPersonIdentifier().Value;
                person = new PersonAliasService( rockContext ).GetPerson( primaryPersonAliasGuid );
            }

            return person;
        }

        /// <summary>
        /// Determines if logged in
        /// </summary>
        private bool IsLoggedIn()
        {
            return CurrentPerson != null || GetUnsecuredPersonIdentifier().HasValue;
        }

        /// <summary>
        /// Shows the known individuals panel
        /// </summary>
        private void ShowKnownIndividual()
        {
            var rockContext = new RockContext();
            var person = GetLoggedInPerson();
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, person );
            lKnownIndividualTitle.Text = GetAttributeValue( AttributeKey.KnownIndividualPanel1Title ).ResolveMergeFields( mergeFields );
            lKnownIndividualText.Text = GetAttributeValue( AttributeKey.KnownIndividualPanel1IntroText ).ResolveMergeFields( mergeFields );
            lbAdjustWatchers.Visible = OtherWatchers
                                                .Any( a => a.RelationshipTypeGuid != Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid()
                                                            && a.RelationshipTypeGuid != Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() );
            pnlKnownIndividual.Visible = true;
            btnCheckIn.Text = GetAttributeValue( AttributeKey.CheckinButtonText );
            cblIndividuals.Items.Clear();
            cblIndividuals.Items.Add( new ListItem( PrimaryWatcher.FullName, PrimaryWatcher.Id.ToString() ) );
            foreach ( var otherWatcher in OtherWatchers )
            {
                cblIndividuals.Items.Add( new ListItem( string.Format( "{0} <span class='subtitle'>{1}</span>", otherWatcher.FullName, otherWatcher.RelationshipType ), otherWatcher.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Shows the success panel
        /// </summary>
        private void ShowSuccess()
        {
            var rockContext = new RockContext();
            var person = GetLoggedInPerson();
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, person );
            lSuccessTitle.Text = GetAttributeValue( AttributeKey.KnownIndividualPanel2Title ).ResolveMergeFields( mergeFields );
            lSuccessText.Text = GetAttributeValue( AttributeKey.KnownIndividualPanel2IntroText ).ResolveMergeFields( mergeFields );
            pnlThanks.Visible = true;
        }


        #region Other Watchers Panel

        /// <summary>
        /// Shows the other watcher panel
        /// </summary>
        private void ShowOtherWatcher()
        {
            lPanel2Title.Text = GetAttributeValue( AttributeKey.UnknownIndividualPanel2Title );
            lPanel2Text.Text = GetAttributeValue( AttributeKey.UnknownIndividualPanel2IntroText );
            pnlPrimaryWatcher.Visible = false;
            pnlOtherWatcher.Visible = true;
            ClearNewIndividualControl();
            BindRelationDropDown();
            ddlRelation_SelectedIndexChanged( null, null );
            BindCurrentlyListed();
        }

        /// <summary>
        /// Binds the relation dropdown list.
        /// </summary>
        private void BindRelationDropDown()
        {
            List<GroupTypeRoleCache> knownRelationhips = GetConfiguredKnownRelationship();

            ddlRelation.Items.Clear();

            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );

            var adultRole = familyGroupType
                            .Roles
                            .Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                            .FirstOrDefault();
            if ( adultRole != null && !OtherWatchers.Any( a => a.RelationshipTypeGuid == adultRole.Guid ) )
            {
                ddlRelation.Items.Add( new ListItem( KNOWN_RELATIONSHIP_SPOUSE, adultRole.Guid.ToString() ) );
            }

            var childRole = familyGroupType
                                        .Roles
                                        .Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() )
                                        .FirstOrDefault();

            if ( childRole != null )
            {
                ddlRelation.Items.Add( new ListItem( childRole.Name, childRole.Guid.ToString() ) );
            }

            foreach ( var knownRelationship in knownRelationhips )
            {
                ddlRelation.Items.Add( new ListItem( knownRelationship.Name, knownRelationship.Guid.ToString() ) );
            }
        }

        private List<GroupTypeRoleCache> GetConfiguredKnownRelationship()
        {
            var selectedRelationshipTypeGuids = GetAttributeValue( AttributeKey.KnownRelationshipTypes )
                            .SplitDelimitedValues()
                            .AsGuidList();
            var groupeType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
            var knownRelationhips = groupeType
                                    .Roles
                                    .Where( a => selectedRelationshipTypeGuids.Contains( a.Guid ) )
                                    .ToList();
            return knownRelationhips;
        }

        /// <summary>
        /// Gets the related watchers on configured known relationships.
        /// </summary>
        private List<Watcher> GetKnownRelationshipWatchers( Person person, List<GroupTypeRoleCache> configuredKnownRelationships, RockContext rockContext = null )
        {
            var otherIndividuals = new List<Watcher>();
            rockContext = rockContext ?? new RockContext();

            if ( configuredKnownRelationships.Any() )
            {
                var selectedRelationshipTypeGuids = configuredKnownRelationships.Select( a => a.Guid ).ToList();
                GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                Guid knownRelationshipGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS );
                Guid knownRelationshipOwner = new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER );
                var familyMembersKnownRelationshipGroups = groupMemberService.Queryable()
                                                        .Where( g => g.Group.GroupType.Guid == knownRelationshipGuid
                                                                        && g.GroupRole.Guid == knownRelationshipOwner
                                                                        && g.PersonId == person.Id )
                                                        .Select( m => m.GroupId );

                otherIndividuals = groupMemberService.Queryable()
                                        .Where( g => selectedRelationshipTypeGuids.Contains( g.GroupRole.Guid )
                                                        && familyMembersKnownRelationshipGroups.Contains( g.GroupId )
                                                        && !g.Person.IsDeceased )
                                        .Distinct()
                                        .AsEnumerable()
                                        .Select( a => new Watcher( a.Person, a.GroupRole.Guid, a.GroupRole.Name ) )
                                        .Distinct()
                                        .ToList();

            }

            return otherIndividuals;
        }

        /// <summary>
        /// Binds the currently listed relationship to the repeater control.
        /// </summary>
        private void BindCurrentlyListed()
        {
            rptListed.DataSource = OtherWatchers
                                                .Select( a => new
                                                {
                                                    Guid = a.Guid,
                                                    Name = a.FullName,
                                                    Relationship = a.RelationshipType
                                                } )
                                                .ToList();
            rptListed.DataBind();
        }

        /// <summary>
        /// Gets the relation
        /// </summary>
        private string GetRelation( int relationshipid )
        {
            if ( relationshipid == default( int ) )
            {
                return KNOWN_RELATIONSHIP_SPOUSE;
            }
            else
            {
                var groupeType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
                return groupeType.Roles.Where( a => a.Id == relationshipid )
                    .Select( a => a.Name )
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// Clears the values from the other watchers panel.
        /// </summary>
        private void ClearNewIndividualControl()
        {
            tbOtherFirstName.Text = string.Empty;
            tbOtherLastName.Text = string.Empty;
            tbOtherEmail.Text = string.Empty;
            bpOtherBirthDay.SelectedDate = null;
            pnOtherMobile.Text = string.Empty;
            cbOtherMessagingEnabled.Checked = false;
        }

        /// <summary>
        /// Update the other watchers
        /// </summary>
        private void AddOrUpdateOtherWatchers( RockContext rockContext, Person primaryPerson, Group family )
        {
            var personService = new PersonService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );

            var configuredKnownRelationships = GetConfiguredKnownRelationship();
            if ( configuredKnownRelationships.Any() )
            {
                var existingKnownRelationshipWatchers = GetKnownRelationshipWatchers( primaryPerson, configuredKnownRelationships, rockContext );
                if ( existingKnownRelationshipWatchers.Any() )
                {
                    //remove the relationship for the person removed from the currently listed (excluding spouse and child)
                    var removedIndividuals = existingKnownRelationshipWatchers.Where( a => !OtherWatchers.Any( b => b.Id == a.Id ) );
                    foreach ( var removeIndividual in removedIndividuals )
                    {
                        var relationshipTypeId = configuredKnownRelationships.FirstOrDefault( a => a.Guid == removeIndividual.RelationshipTypeGuid ).Id;
                        groupMemberService.DeleteKnownRelationship( primaryPerson.Id, removeIndividual.Id, relationshipTypeId );
                    }
                }
            }

            var recordTypePersonId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            var recordStatusValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
            var connectionStatusValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() );
            var marriedMartialStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() ).Id;

            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );

            var newFamilyIds = new Dictionary<string, int>();
            foreach ( var watcher in OtherWatchers.Where( a => a.Id == default( int ) ) )
            {
                // Get what the family/relationship state should be for the child
                bool shouldBeInPrimaryFamily =
                    ( watcher.RelationshipTypeGuid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ||
                    watcher.RelationshipTypeGuid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() );

                Person person = null;
                // If person was not found, Look for existing person in same family with same name and birthdate
                if ( shouldBeInPrimaryFamily && watcher.BirthDate.HasValue )
                {
                    var possibleMatch = new Person { NickName = watcher.NickName, LastName = watcher.LastName };
                    possibleMatch.SetBirthDate( watcher.BirthDate );
                    person = family.MatchingFamilyMember( possibleMatch );
                }
                else if ( !shouldBeInPrimaryFamily )
                {
                    var personQuery = new PersonService.PersonMatchQuery( watcher.NickName, watcher.LastName, watcher.EmailAddress, watcher.MobilePhoneNumber, birthDate: watcher.BirthDate );
                    person = personService.FindPerson( personQuery, false );
                }

                bool isNew = false;
                // Otherwise create a new person
                if ( person == null )
                {
                    isNew = true;
                    person = new Person();
                    person.Guid = watcher.Guid;
                    person.FirstName = watcher.NickName.FixCase();
                    person.LastName = watcher.LastName.FixCase();
                    person.RecordTypeValueId = recordTypePersonId;
                    person.RecordStatusValueId = recordStatusValue != null ? recordStatusValue.Id : ( int? ) null;
                    person.ConnectionStatusValueId = connectionStatusValue != null ? connectionStatusValue.Id : ( int? ) null;
                }
                else
                {
                    person.NickName = watcher.NickName;
                    person.LastName = watcher.LastName;
                }

                person.SetBirthDate( watcher.BirthDate );

                // Save the email address
                if ( watcher.EmailAddress.IsNotNullOrWhiteSpace() )
                {
                    person.Email = watcher.EmailAddress;
                }


                if ( shouldBeInPrimaryFamily )
                {
                    bool notInPrimaryFamily = family.Members.Any( m => m.PersonId == person.Id );
                    var roleId = familyGroupType.Roles.FirstOrDefault( r => r.Guid == watcher.RelationshipTypeGuid ).Id;
                    if ( watcher.RelationshipTypeGuid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                    {

                        person.MaritalStatusValueId = marriedMartialStatusValueId;
                        if ( primaryPerson.MaritalStatusValueId != marriedMartialStatusValueId )
                        {
                            primaryPerson.MaritalStatusValueId = marriedMartialStatusValueId;
                        }
                    }
                    PersonService.AddPersonToFamily( person, notInPrimaryFamily, family.Id, roleId, rockContext );
                }
                else
                {
                    // Get any other family memberships.
                    if ( !groupMemberService.Queryable()
                        .Any( m =>
                            m.Group.GroupTypeId == familyGroupType.Id &&
                            m.PersonId == person.Id &&
                            m.GroupId != family.Id ) )
                    {

                        // Check to see if we've already created a family with someone who has same last name
                        string key = watcher.LastName.ToLower();
                        int? newFamilyId = newFamilyIds.ContainsKey( key ) ? newFamilyIds[key] : ( int? ) null;
                        // If not, create a new family
                        if ( !newFamilyId.HasValue )
                        {
                            var watcherFamily = CreateNewFamily( familyGroupType.Id, watcher.LastName );
                            new GroupService( rockContext ).Add( watcherFamily );
                            rockContext.SaveChanges();
                            newFamilyId = watcherFamily.Id;
                            newFamilyIds.Add( key, watcherFamily.Id );
                        }

                        var roleId = familyGroupType.Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;

                        PersonService.AddPersonToFamily( person, isNew, newFamilyId.Value, roleId, rockContext );

                        var relationshipTypeId = configuredKnownRelationships.FirstOrDefault( a => a.Guid == watcher.RelationshipTypeGuid ).Id;
                        groupMemberService.CreateKnownRelationship( primaryPerson.Id, person.Id, relationshipTypeId );
                    }
                }

                watcher.Id = person.Id;

                // Save the mobile phone number
                if ( watcher.MobilePhoneNumber.IsNotNullOrWhiteSpace() )
                {
                    SavePhoneNumber( person.Id, watcher, rockContext );
                }
            }
        }


        #endregion Other Watchers Panel

        #region Account Panel

        /// <summary>
        /// Shows the account panel
        /// </summary>
        private void ShowAccount()
        {
            lPanel3Title.Text = GetAttributeValue( AttributeKey.UnknownIndividualPanel3Title );
            lPanel3Text.Text = GetAttributeValue( AttributeKey.UnknownIndividualPanel3IntroText );
            pnlAccount.Visible = true;
        }

        #endregion Account Panel

        /// <summary>
        /// Get the unsecured person identifier from the cookie.
        /// </summary>
        private Guid? GetUnsecuredPersonIdentifier()
        {
            if ( Request.Cookies[ROCK_UNSECUREDPERSONIDENTIFIER] != null )
            {
                return Request.Cookies[ROCK_UNSECUREDPERSONIDENTIFIER].Value.AsGuidOrNull();
            }

            return null;
        }

        /// <summary>
        /// Set the unsecured person identifier to the cookie.
        /// </summary>
        private void SetUnsecuredPersonIdentifier( Guid personAliasGuid )
        {
            HttpCookie httpcookie = new HttpCookie( ROCK_UNSECUREDPERSONIDENTIFIER );
            httpcookie.Expires = RockDateTime.Now.AddMinutes( 480 );
            httpcookie.Value = personAliasGuid.ToString();
            Response.Cookies.Add( httpcookie );
        }

        /// <summary>
        /// Saves the phone number.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="number">The number.</param>
        /// <param name="countryCode">The country code.</param>
        private void SavePhoneNumber( int personId, Watcher watcher, RockContext rockContext )
        {
            string phone = PhoneNumber.CleanNumber( watcher.MobilePhoneNumber );

            var phoneNumberService = new PhoneNumberService( rockContext );

            var phType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            if ( phType != null )
            {
                var phoneNumber = phoneNumberService.Queryable()
                    .Where( n =>
                        n.PersonId == personId &&
                        n.NumberTypeValueId.HasValue &&
                        n.NumberTypeValueId.Value == phType.Id )
                    .FirstOrDefault();

                if ( phone.IsNotNullOrWhiteSpace() )
                {
                    if ( phoneNumber == null )
                    {
                        phoneNumber = new PhoneNumber();
                        phoneNumberService.Add( phoneNumber );

                        phoneNumber.PersonId = personId;
                        phoneNumber.NumberTypeValueId = phType.Id;
                    }

                    phoneNumber.CountryCode = PhoneNumber.CleanNumber( watcher.MobileCountryCode );
                    phoneNumber.Number = phone;
                    phoneNumber.IsMessagingEnabled = watcher.IsMessagingEnabled;
                }
                else
                {
                    if ( phoneNumber != null )
                    {
                        phoneNumberService.Delete( phoneNumber );
                    }
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Creates a new family group.
        /// </summary>
        /// <param name="familyGroupTypeId">The family group type identifier.</param>
        /// <returns></returns>
        private Group CreateNewFamily( int familyGroupTypeId, string lastName )
        {
            // If we don't have an existing family, create a new family
            var family = new Group();
            family.Name = lastName.FixCase() + " Family";
            family.GroupTypeId = familyGroupTypeId;

            return family;
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        private void GetData( Person person, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var personService = new PersonService( rockContext );
            var family = person.GetFamily( rockContext );

            PrimaryWatcher = new Watcher( person, Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid(), string.Empty );

            var familyMembers = personService
                .GetFamilyMembers( family, person.Id )
                .Where( a => !a.Person.IsDeceased )
                .AsEnumerable();

            OtherWatchers = familyMembers
                .Select( a => new Watcher( a.Person, a.GroupRole.Guid, a.GroupRole.Name ) )
                .ToList();

            var spouse = personService.GetSpouse( person );
            if ( spouse != null && OtherWatchers.Any( a => a.Id == spouse.Id ) )
            {
                var spouseWatcher = OtherWatchers.First( a => a.Id == spouse.Id );
                spouseWatcher.RelationshipType = KNOWN_RELATIONSHIP_SPOUSE;
            }

            var configuredKnownRelationships = GetConfiguredKnownRelationship();
            var otherIndividuals = GetKnownRelationshipWatchers( person, configuredKnownRelationships, rockContext );

            if ( otherIndividuals.Any() )
            {
                OtherWatchers.AddRange( otherIndividuals );
            }
        }


        #endregion

        /// <summary>
        /// Helper Class for serializing other watcher data in viewstate
        /// </summary>
        [Serializable]
        public class Watcher
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the first name.
            /// </summary>
            /// <value>
            /// The first name.
            /// </value>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            /// <value>
            /// The last name.
            /// </value>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the birth date.
            /// </summary>
            /// <value>
            /// The birth date.
            /// </value>
            public DateTime? BirthDate { get; set; }

            /// <summary>
            /// Gets or sets the mobile phone number.
            /// </summary>
            /// <value>
            /// The mobile phone number.
            /// </value>
            public string MobilePhoneNumber { get; set; }

            /// <summary>
            /// Gets or sets the mobile country code.
            /// </summary>
            /// <value>
            /// The mobile country code.
            /// </value>
            public string MobileCountryCode { get; set; }

            /// <summary>
            /// Gets or sets the email address.
            /// </summary>
            /// <value>
            /// The mobile phone number.
            /// </value>
            public string EmailAddress { get; set; }

            /// <summary>
            /// Gets or sets the type of the relationship identifier.
            /// </summary>
            /// <value>
            /// The type of the relationship identifier.
            /// </value>
            public Guid RelationshipTypeGuid { get; set; }

            /// <summary>
            /// Gets or sets the type of the relationship.
            /// </summary>
            /// <value>
            /// The type of the relationship.
            /// </value>
            public string RelationshipType { get; set; }

            /// <summary>
            /// Gets or sets the Is Messaging Enabled bool.
            /// </summary>
            /// <value>
            /// True/False.
            /// </value>
            public bool IsMessagingEnabled { get; set; }

            /// <summary>
            /// Gets  full name of the person.
            /// </summary>
            /// <value>
            /// A <see cref="System.String"/> containing the full name of the person.
            /// </value>
            public virtual string FullName
            {
                get
                {
                    return string.Format( "{0} {1}", NickName, LastName );
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Watcher"/> class.
            /// </summary>
            public Watcher()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Watcher"/> class.
            /// </summary>
            /// <param name="person">The person.</param>
            /// <param name="relationshipTypeGuid">The relationship type indentifier.</param>
            /// <param name="relationshipTypeName">The relationship type.</param>
            public Watcher( Person person, Guid relationshipTypeGuid, string relationshipTypeName )
            {
                Id = person.Id;
                Guid = person.Guid;
                NickName = person.NickName;
                LastName = person.LastName;
                BirthDate = person.BirthDate;
                EmailAddress = person.Email;
                var mobilePhone = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                if ( mobilePhone != null )
                {
                    MobilePhoneNumber = mobilePhone.Number;
                    MobileCountryCode = mobilePhone.CountryCode;
                    IsMessagingEnabled = mobilePhone.IsMessagingEnabled;
                }
                RelationshipTypeGuid = relationshipTypeGuid;
                RelationshipType = relationshipTypeName;
            }
        }
    }
}