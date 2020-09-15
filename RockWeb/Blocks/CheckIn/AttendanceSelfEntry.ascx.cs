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
    [LocationField(
        "Location",
        Description = "Optional location....",
        IsRequired = false,
        Order = 0,
        CurrentPickerMode = LocationPickerMode.Named,
        AllowedPickerModes = new LocationPickerMode[] { LocationPickerMode.Named },
        Key = AttributeKey.Location )]
    [GroupTypeField(
        "Check-in Configuration",
        groupTypePurposeValueGuid: Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE,
        Description = "This will be the group type that we will use to determine where to check them in.",
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.GroupType.GROUPTYPE_SERVICE_ATTENDANCE,
        Order = 1,
        Key = AttributeKey.CheckinConfiguration )]
    [BooleanField(
        "Primary Person Birthday Shown",
        Description = "Should birthday be displayed for primary person?",
        DefaultBooleanValue = true,
        Order = 2,
        Key = AttributeKey.PrimaryPersonBirthdayShown )]
    [BooleanField(
        "Primary Person Birthday Required",
        Description = "Determine if birthday for primary person is required.",
        DefaultBooleanValue = false,
        Order = 3,
        Key = AttributeKey.PrimaryPersonBirthdayRequired )]
    [BooleanField(
        "Primary Person Address Shown",
        Description = "Should address be displayed for primary person?",
        DefaultBooleanValue = true,
        Order = 4,
        Key = AttributeKey.PrimaryPersonAddressShown )]
    [BooleanField(
        "Primary Person Address Required",
        Description = "Determine if address for primary person is required.",
        DefaultBooleanValue = false,
        Order = 5,
        Key = AttributeKey.PrimaryPersonAddressRequired )]
    [BooleanField(
        "Primary Person Mobile Phone Shown",
        Description = "Should mobile phone be displayed for primary person?",
        DefaultBooleanValue = true,
        Order = 6,
        Key = AttributeKey.PrimaryPersonMobilePhoneShown )]
    [BooleanField(
        "Primary Person Mobile Phone Required",
        Description = "Determine if mobile phone for primary person is required.",
        DefaultBooleanValue = false,
        Order = 7,
        Key = AttributeKey.PrimaryPersonMobilePhoneRequired )]
    [BooleanField(
        "Other Person Birthday Shown",
        Description = "Should birthday be displayed for other person?",
        DefaultBooleanValue = true,
        Order = 8,
        Key = AttributeKey.OtherPersonBirthdayShown )]
    [BooleanField(
        "Other Person Birthday Required",
        Description = "Determine if birthday for other person is required.",
        DefaultBooleanValue = false,
        Order = 9,
        Key = AttributeKey.OtherPersonBirthdayRequired )]
    [BooleanField(
        "Other Person Mobile Phone Shown",
        Description = "Should mobile phone be displayed for other person?",
        DefaultBooleanValue = true,
        Order = 10,
        Key = AttributeKey.OtherPersonMobilePhoneShown )]
    [BooleanField(
        "Other Person Mobile Phone Required",
        Description = "Determine if mobile phone for other person is required.",
        DefaultBooleanValue = false,
        Order = 11,
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
        Order = 12,
        Key = AttributeKey.KnownRelationshipTypes )]
    [UrlLinkField(
        "Redirect URL",
        Description = "The URL to redirect the individual to when they check-in.",
        IsRequired = false,
        Order = 13,
        Key = AttributeKey.RedirectURL )]
    [TextField(
        "Check-in Button Text",
        Description = "The text that should be shown on the check-in button.",
        IsRequired = false,
        DefaultValue = "Check-in",
        Order = 14,
        Key = AttributeKey.CheckinButtonText )]
    [WorkflowTypeField(
        "Workflow",
        AllowMultiple = false,
        Key = AttributeKey.Workflow,
        Description = "The optional workflow type to launch when a person is checked in. The primary person will be passed to the workflow as the entity. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: GroupId, LocationId (if found), ScheduleId (if found), CheckedInPersonIds. (NOTE: If you want a workflow 'form' type of workflow use the Redirect URL setting instead.)",
        IsRequired = false,
        Order = 15 )]
    [IntegerField(
        "Hide Individuals Younger Than",
        Description = "The age that should be used as the cut-off for displaying on the attendance list. The value of 14 will hide individuals younger than 14. Individuals without an age will always be shown. Defaults to blank.",
        Key = AttributeKey.HideIndividualsYoungerThan,
        IsRequired =false,
        Order = 26
        )]
    [DefinedValueField(
        "Hide Individuals in Grade Less Than",
        Description = "Individuals in grades lower than this value will not be show on the attendance list. Defaults to empty (not set).",
        Key = AttributeKey.HideIndividualsInGradeLessThan,
        IsRequired = false,
        Order = 27,
        DisplayDescription = true,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.SCHOOL_GRADES
        )]
    #region Messages Block Attribute Settings

    [TextField(
        "Unknown Individual Panel 1 Title",
        Description = "The title to display on the primary watcher panel.",
        IsRequired = false,
        Category = AttributeCategory.Messages,
        Order = 16,
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
        Order = 17,
        DefaultValue = " We love to learn a little about you so that we can best serve you and your family.",
        Key = AttributeKey.UnknownIndividualPanel1IntroText )]
    [TextField(
        "Unknown Individual Panel 2 Title",
        Description = "The title to display on the other watcher panel.",
        IsRequired = false,
        Category = AttributeCategory.Messages,
        Order = 18,
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
        Order = 19,
        Key = AttributeKey.UnknownIndividualPanel2IntroText )]
    [TextField(
        "Unknown Individual Panel 3 Title",
        Description = "The title to display on the account panel.",
        IsRequired = false,
        Category = AttributeCategory.Messages,
        DefaultValue = "Would You Like to Create An Account?",
        Order = 20,
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
        Order = 21,
        Key = AttributeKey.UnknownIndividualPanel3IntroText )]
    [TextField(
        "Known Individual Panel 1 Title",
        Description = "The title to display on the known individual panel.",
        IsRequired = false,
        Category = AttributeCategory.Messages,
        Order = 22,
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
        Order = 23,
        Key = AttributeKey.KnownIndividualPanel1IntroText )]
    [TextField(
        "Known Individual Panel 2 Title",
        Description = "The title to display on the success panel.",
        IsRequired = false,
        Category = AttributeCategory.Messages,
        Order = 24,
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
        Order = 25,
        Key = AttributeKey.KnownIndividualPanel2IntroText )]
    #endregion Messages Block Attribute Settings

    #endregion Block Attributes
    public partial class AttendanceSelfEntry : RockBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string Location = "Location";
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
            public const string HideIndividualsYoungerThan = "HideIndividualsYoungerThan";
            public const string HideIndividualsInGradeLessThan = "HideIndividualsInGradeLessThan";
        }

        /// <summary>
        /// Keys to use for Block Attribute Categories
        /// </summary>
        private static class AttributeCategory
        {
            public const string Messages = "Messages";
        }

        /// <summary>
        /// Kyes to use for parameters passed into the block
        /// </summary>
        private static class PageParameterKey
        {
            public const string LocationId = "LocationId";
        }

        /// <summary>
        /// Keys used to access values stored in the ViewState obj.
        /// </summary>
        private static class ViewStateKey
        {
            public const string OtherWatchers = "OtherWatchers";
            public const string PrimaryWatcher = "PrimaryWatcher";
            public const string Location = "Location";
        }

        #endregion Keys

        #region Fields

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
                OtherWatchers.ForEach( a => a.Selected = true );
            }
            else
            {
                PrimaryWatcher = new Watcher();
                PrimaryWatcher.Guid = Guid.NewGuid();
                OtherWatchers = new List<Watcher>();
            }

            PrimaryWatcher.NickName = tbFirstName.Text;
            PrimaryWatcher.LastName = tbLastName.Text;
            PrimaryWatcher.EmailAddress = tbEmail.Text;
            PrimaryWatcher.Selected = true;
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

            ShowOtherWatcher();
        }

        #endregion Primary Watcher Panel Events

        #region Other Watchers Panel Events

        /// <summary>
        /// Handles the ItemDataBound event of the rptListed control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptListed_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var watcher = e.Item.DataItem as Watcher;
                if ( watcher != null )
                {
                    LinkButton lbDelete = e.Item.FindControl( "lbDelete" ) as LinkButton;
                    if ( lbDelete != null )
                    {
                        lbDelete.Visible = watcher.Id == default( int ) || ( watcher.RelationshipTypeGuid != Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid()
                                            && watcher.RelationshipTypeGuid != Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() );
                    }
                }
            }
        }

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
                BindRelationDropDown();
                ddlRelation_SelectedIndexChanged( null, null );
                BindCurrentlyListed();
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
            AddNewIndividual();
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
            if ( tbOtherFirstName.Text.IsNotNullOrWhiteSpace() || tbOtherLastName.Text.IsNotNullOrWhiteSpace() )
            {
                List<string> errors = new List<string>();
                if ( tbOtherFirstName.Text.IsNullOrWhiteSpace() )
                {
                    errors.Add( "First Name is required." );
                }

                if ( tbOtherLastName.Text.IsNullOrWhiteSpace() )
                {
                    errors.Add( "Last Name is required." );
                }

                if ( bpOtherBirthDay.Visible && GetAttributeValue( AttributeKey.OtherPersonBirthdayRequired ).AsBoolean() && !bpOtherBirthDay.SelectedDate.HasValue )
                {
                    errors.Add( "Birthday is required." );
                }

                if ( pnlOtherPhone.Visible && GetAttributeValue( AttributeKey.OtherPersonMobilePhoneRequired ).AsBoolean() && pnOtherMobile.Number.IsNullOrWhiteSpace() )
                {
                    errors.Add( "Mobile Phone is required." );
                }

                if ( errors.Any() )
                {
                    nbOtherWarning.Visible = true;
                    nbOtherWarning.Title = string.Empty;
                    nbOtherWarning.Text = errors.AsDelimited( "<br/>" );
                    return;
                }
                AddNewIndividual();
            }

            Person primaryPerson = null;
            if ( isLoggedIn )
            {
                var rockContext = new RockContext();
                primaryPerson = GetLoggedInPerson( rockContext );
                var family = primaryPerson.GetFamily( rockContext );

                primaryPerson.NickName = PrimaryWatcher.NickName.FixCase();
                primaryPerson.LastName = PrimaryWatcher.LastName.FixCase();

                if ( PrimaryWatcher.BirthDate.HasValue )
                {
                    primaryPerson.SetBirthDate( PrimaryWatcher.BirthDate );
                }

                // Save the email address
                if ( PrimaryWatcher.EmailAddress.IsNotNullOrWhiteSpace() )
                {
                    primaryPerson.Email = PrimaryWatcher.EmailAddress;
                }

                rockContext.SaveChanges();

                // Save the mobile phone number
                if ( PrimaryWatcher.MobilePhoneNumber.IsNotNullOrWhiteSpace() )
                {
                    SavePhoneNumber( primaryPerson.Id, PrimaryWatcher, rockContext );
                }

                SaveHomeAddress( rockContext, family );

                AddOrUpdateOtherWatchers( rockContext, primaryPerson, family );
            }

            pnlOtherWatcher.Visible = false;
            if ( isLoggedIn )
            {
                ShowKnownIndividual();
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
                    nbAccountWarning.Text = "Please type a password. A password is required if you would like to create an account.";
                    nbAccountWarning.NotificationBoxType = NotificationBoxType.Danger;
                    nbAccountWarning.Visible = true;
                    return;
                }

                if ( txtUserName.Text.IsNullOrWhiteSpace() )
                {
                    nbAccountWarning.Title = "Missing Information";
                    nbAccountWarning.Text = "Please type a username. A username is required if you would like to create an account.";
                    nbAccountWarning.NotificationBoxType = NotificationBoxType.Danger;
                    nbAccountWarning.Visible = true;
                    return;
                }

                if ( new UserLoginService( rockContext ).GetByUserName( txtUserName.Text ) != null )
                {
                    nbAccountWarning.Title = "Invalid Username";
                    nbAccountWarning.Text = "The selected username is already being used. Please select a different username.";
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
            Group family = null;
            var personService = new PersonService( rockContext );
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
                person.NickName = PrimaryWatcher.NickName.FixCase();
                person.LastName = PrimaryWatcher.LastName.FixCase();
            }

            if ( PrimaryWatcher.BirthDate.HasValue )
            {
                person.SetBirthDate( PrimaryWatcher.BirthDate );
            }

            // Save the email address
            if ( PrimaryWatcher.EmailAddress.IsNotNullOrWhiteSpace() )
            {
                person.Email = PrimaryWatcher.EmailAddress;
            }

            if ( family == null )
            {
                family = PersonService.SaveNewPerson( person, rockContext );
            }

            rockContext.SaveChanges();
            PrimaryWatcher.Id = person.Id;

            // Save the mobile phone number
            if ( PrimaryWatcher.MobilePhoneNumber.IsNotNullOrWhiteSpace() )
            {
                SavePhoneNumber( person.Id, PrimaryWatcher, rockContext );
            }

            SaveHomeAddress( rockContext, family );

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

            Rock.Security.Authorization.SetUnsecurePersonIdentifier( person.PrimaryAlias.Guid );

            if ( isAccountRequired )
            {
                Authorization.SetAuthCookie( txtUserName.Text, false, false );
            }

            pnlAccount.Visible = false;
            ShowKnownIndividual();
        }

        #region Known Individual Panel Events

        /// <summary>
        /// Handles the lbAdjustWatchers Click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAdjustWatchers_Click( object sender, EventArgs e )
        {
            var memberIds = GetSelectedCheckedInPersonIds();
            foreach ( var watcher in OtherWatchers )
            {
                watcher.Selected = memberIds.Contains( watcher.Id );
            }

            PrimaryWatcher.Selected = memberIds.Contains( PrimaryWatcher.Id );

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
            var descendantGroupTypeIds = new GroupTypeService( rockContext ).GetAllAssociatedDescendents( groupType.Id ).Select( a => a.Id ).ToList();

            var groups = new GroupService( rockContext )
                    .Queryable()
                    .Where( a =>
                        a.GroupTypeId == groupType.Id ||
                        descendantGroupTypeIds.Contains( a.GroupTypeId ) )
                    .IsActive();

            var primaryPerson = new PersonService( rockContext ).Get( PrimaryWatcher.Id );

            Group attendanceGroup = null;
            if ( groups.Any() )
            {
                Guid? locationGuid = GetAttributeValue( AttributeKey.Location ).AsGuidOrNull();
                int? LocationIdParam = PageParameter( PageParameterKey.LocationId ).AsIntegerOrNull();

                // Try to set the locationId to the value specified by the block setting (preferred) or PageParameter. A null is okay here.
                int? locationId = locationGuid.HasValue ? new LocationService( rockContext ).GetId( locationGuid.Value ) : LocationIdParam;
                int? scheduleId = null;
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

                    // If we have the location and it is valid then use it
                    if ( locationId.HasValue && group.GroupLocations.Select( l => l.LocationId == locationId ).Any() )
                    {
                        GroupLocation groupLocation = group.GroupLocations.Where( l => l.LocationId == locationId ).FirstOrDefault();
                        var schedule = groupLocation.Schedules.Where( a => a.WasScheduleActive( campusCurrentDateTime ) ).FirstOrDefault();
                        if ( schedule != null )
                        {
                            scheduleId = schedule.Id;
                            attendanceGroup = group;
                        }
                    }

                    // If we don't have the location or it was invalid or if the location specified didn't have a schedule then we need to loop through the group locations.
                    if ( !scheduleId.HasValue )
                    {
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
                    }

                    if ( scheduleId.HasValue )
                    {
                        break;
                    }
                }

                if ( !scheduleId.HasValue )
                {
                    attendanceGroup = groups.Where( a => a.GroupLocations.Any() ).FirstOrDefault();
                    if ( attendanceGroup != null )
                    {
                        locationId = attendanceGroup.GroupLocations.Select( a => a.LocationId ).FirstOrDefault();
                    }
                }

                if ( !scheduleId.HasValue && !locationId.HasValue )
                {
                    attendanceGroup = groups.First();
                }

                var attendanceService = new AttendanceService( rockContext );

                var personIds = GetSelectedCheckedInPersonIds();

                var persons = new PersonService( rockContext ).GetListByIds( personIds );
                foreach ( var person in persons )
                {
                    if ( person.PrimaryAliasId.HasValue )
                    {
                        var attendance = attendanceService.AddOrUpdate( person.PrimaryAliasId.Value, campusCurrentDateTime, attendanceGroup.Id, locationId, scheduleId, attendanceGroup.CampusId );

                    }
                }

                rockContext.SaveChanges();

                Guid? workflowTypeGuid = GetAttributeValue( AttributeKey.Workflow ).AsGuidOrNull();
                if ( workflowTypeGuid.HasValue )
                {
                    var workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );
                    if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                    {
                        try
                        {
                            // Create parameters
                            var parameters = new Dictionary<string, string>();
                            parameters.Add( "GroupId", attendanceGroup.Id.ToString() );
                            if ( locationId.HasValue )
                            {
                                parameters.Add( "LocationId", locationId.Value.ToString() );
                            }
                            if ( scheduleId.HasValue )
                            {
                                parameters.Add( "ScheduleId", scheduleId.Value.ToString() );
                            }
                            parameters.Add( "CheckedInPersonIds", personIds.AsDelimited( "," ) );

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
                var family = primaryPerson.GetFamily( rockContext );
                string url = redirectUrl + ( redirectUrl.Contains( "?" ) ? "&" : "?" ) + "PersonAliasGuid=" + GetUnsecuredPersonIdentifier().ToStringSafe();
                url = url + "&PersonId=" + primaryPerson.Id;
                if ( attendanceGroup != null )
                {
                    url = url + "&GroupId=" + attendanceGroup.Id;
                }
                if ( family != null )
                {
                    url = url + "&FamilyId=" + family.Id;
                }
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
                GetData( person );
                ShowKnownIndividual();
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
            pnlKnownIndividual.Visible = true;
            btnCheckIn.Text = GetAttributeValue( AttributeKey.CheckinButtonText );

            var validWatchers = OtherWatchers.AsEnumerable();

            var hideIndividualsYoungerThan = GetAttributeValue( AttributeKey.HideIndividualsYoungerThan).AsIntegerOrNull();
            if ( hideIndividualsYoungerThan.HasValue )
            {
                validWatchers = OtherWatchers
                                .Where( a =>
                                            !a.BirthDate.HasValue ||
                                            a.BirthDate.Value.Age() >= hideIndividualsYoungerThan.Value );
            }

            var gradeLessThanDefinedValue = GetAttributeValue( AttributeKey.HideIndividualsInGradeLessThan ).AsGuidOrNull();
            if ( gradeLessThanDefinedValue.HasValue )
            {
               var gradeOffsetValue =  DefinedValueCache.Get( gradeLessThanDefinedValue.Value );
                if ( gradeOffsetValue != null )
                {
                    // Need to Show Individuals who are in Grade greater Than
                    validWatchers = OtherWatchers
                                    .Where( a =>
                                                !a.GradeOffset.HasValue ||
                                                a.GradeOffset.Value <= gradeOffsetValue.Value.AsInteger() );
                }
            }

            var familyMembers = validWatchers
                            .Where( a => a.RelationshipTypeGuid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid()
                                    || a.RelationshipTypeGuid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() )
                            .ToList();
            familyMembers.Insert( 0, PrimaryWatcher );

            var otherMembers = validWatchers
                            .Where( a => a.RelationshipTypeGuid != Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid()
                                    && a.RelationshipTypeGuid != Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() )
                            .ToList();
            rptFamilyMembers.DataSource = familyMembers;
            rptFamilyMembers.DataBind();
            rptOtherMembers.DataSource = otherMembers;
            rptOtherMembers.DataBind();
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
            nbOtherWarning.Visible = false;
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
            rptListed.DataSource = OtherWatchers.ToList();
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

                if ( watcher.BirthDate.HasValue )
                {
                    person.SetBirthDate( watcher.BirthDate );
                }

                // Save the email address
                if ( watcher.EmailAddress.IsNotNullOrWhiteSpace() )
                {
                    person.Email = watcher.EmailAddress;
                }


                if ( shouldBeInPrimaryFamily )
                {
                    var roleId = familyGroupType.Roles.FirstOrDefault( r => r.Guid == watcher.RelationshipTypeGuid ).Id;
                    if ( watcher.RelationshipTypeGuid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                    {

                        person.MaritalStatusValueId = marriedMartialStatusValueId;
                        if ( primaryPerson.MaritalStatusValueId != marriedMartialStatusValueId )
                        {
                            primaryPerson.MaritalStatusValueId = marriedMartialStatusValueId;
                        }
                    }

                    if ( isNew )
                    {
                        PersonService.AddPersonToFamily( person, true, family.Id, roleId, rockContext );
                    }
                    else
                    {
                        rockContext.SaveChanges();
                    }
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
                    }

                    var relationshipTypeId = configuredKnownRelationships.FirstOrDefault( a => a.Guid == watcher.RelationshipTypeGuid ).Id;
                    groupMemberService.CreateKnownRelationship( primaryPerson.Id, person.Id, relationshipTypeId );
                }

                watcher.Id = person.Id;

                // Save the mobile phone number
                if ( watcher.MobilePhoneNumber.IsNotNullOrWhiteSpace() )
                {
                    SavePhoneNumber( person.Id, watcher, rockContext );
                }
            }
        }

        /// <summary>
        /// Add the new individual
        /// </summary>
        private void AddNewIndividual()
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
            otherWatcher.Selected = true;
            OtherWatchers.Add( otherWatcher );
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
            if ( Request.Cookies[Rock.Security.Authorization.COOKIE_UNSECURED_PERSON_IDENTIFIER] != null )
            {
                return Request.Cookies[Rock.Security.Authorization.COOKIE_UNSECURED_PERSON_IDENTIFIER].Value.AsGuidOrNull();
            }

            return null;
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
        /// Save the home address.
        /// </summary>
        private void SaveHomeAddress( RockContext rockContext, Group family )
        {
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
            PrimaryWatcher.Selected = true;

            Rock.Security.Authorization.SetUnsecurePersonIdentifier( person.PrimaryAlias.Guid );

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

        /// <summary>
        /// Gets the selected Checked-In Members
        /// </summary>
        private List<int> GetSelectedCheckedInPersonIds()
        {
            var members = new List<int>();
            var familyMembers = hfSelectedFamilyMembers.Value.SplitDelimitedValues().AsIntegerList();
            if ( familyMembers != null )
            {
                members.AddRange( familyMembers );
            }

            var otherMembers = hfSelectedOtherMembers.Value.SplitDelimitedValues().AsIntegerList();
            if ( otherMembers != null )
            {
                members.AddRange( otherMembers );
            }

            return members;
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
            /// Gets or sets the grade offset.
            /// </summary>
            /// <value>
            /// The grade offset.
            /// </value>
            public int? GradeOffset { get; set; }

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
            /// Gets or sets if selected.
            /// </summary>
            /// <value>
            /// True/False.
            /// </value>
            public bool Selected { get; set; }

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
                GradeOffset = person.GradeOffset;
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