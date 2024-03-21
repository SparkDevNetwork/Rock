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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// </summary>
    [DisplayName( "Public Profile Edit" )]
    [Category( "CMS" )]
    [Description( "Public block for users to manage their accounts" )]

    #region "Block Attributes"
    [DefinedValueField(
        "Default Connection Status",
        Key = AttributeKey.DefaultConnectionStatus,
        Description = "The connection status that should be set by default",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        IsRequired = false,
        AllowMultiple = false,
        Order = 0 )]

    [BooleanField(
        "Disable Name Edit",
        Key = AttributeKey.DisableNameEdit,
        Description = "Whether the First and Last Names can be edited.",
        DefaultBooleanValue = false,
        Order = 1 )]

    [BooleanField(
        "Show Title",
        Key = AttributeKey.ShowTitle,
        Description = "Whether to show the person's title (e.g. Mr., Mrs. etc...)",
        DefaultBooleanValue = true,
        Order = 2 )]

    [BooleanField(
        "Show Suffix",
        Key = AttributeKey.ShowSuffix,
        Description = "Whether to show the person's suffix (e.g. Roman Numerals, Jr., Ph.D., etc...)",
        DefaultBooleanValue = true,
        Order = 3 )]

    [BooleanField(
        "Show Nick Name",
        Key = AttributeKey.ShowNickName,
        Description = "Whether to show the person's Nickname in addition to the First Name in the edit screen.",
        DefaultBooleanValue = true,
        Order = 4 )]

    [CustomDropdownListField(
        "Display Mode",
        Key = AttributeKey.DisplayMode,
        Description = "Specifies the Display Mode. To prevent people from editing their profile or family records choose 'View Only'.",
        ListSource = "VIEW^View Only,EDIT^Edit Only,VIEWEDIT^View & Edit",
        IsRequired = true,
        DefaultValue = "VIEWEDIT",
        Order = 5
        )]

    [BooleanField(
        "Show Family Members",
        Key = AttributeKey.ShowFamilyMembers,
        Description = "Whether family members are shown or not.",
        DefaultBooleanValue = true,
        Order = 6 )]

    [BooleanField(
        "Show Addresses",
        Key = AttributeKey.ShowAddresses,
        Description = "Whether the address section is shown or not during editing.",
        DefaultBooleanValue = true,
        Order = 8 )]

    [GroupLocationTypeField(
        "Address Type",
        Key = AttributeKey.AddressTypeValueGuid,
        Description = "The type of address to be displayed / edited.",
        GroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
        Order = 9 )]

    [BooleanField(
        "Show Phone Numbers",
        Key = AttributeKey.ShowPhoneNumbers,
        Description = "Allows hiding the phone numbers.",
        DefaultBooleanValue = false,
        Order = 10 )]

    [DefinedValueField(
        "Phone Types",
        Key = AttributeKey.PhoneTypeValueGuids,
        Description = "The types of phone numbers to display / edit.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE,
        IsRequired = false,
        AllowMultiple = true,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME,
        Order = 11 )]

    [DefinedValueField(
        "Required Adult Phone Types",
        Key = AttributeKey.RequiredAdultPhoneTypes,
        Description = "The phone numbers that are required when editing an adult record.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE,
        IsRequired = false,
        AllowMultiple = true,
        Order = 12 )]

    [BooleanField(
        "Highlight Mobile Phone",
        Key = AttributeKey.HighlightMobilePhone,
        Description = "Determines if the emphasis box should be placed around the mobile number.",
        DefaultBooleanValue = true,
        Order = 13 )]

    [TextField(
        "Mobile Highlight Title",
        Key = AttributeKey.MobileHighlightTitle,
        Description = "The text to use for the mobile highlight title (only displayed if Highlight Mobile Phone is selected).",
        IsRequired = false,
        DefaultValue = "Help Us Keep You Informed",
        Order = 14 )]

    [TextField(
        "Mobile Highlight Text",
        Description = "The text to use for the mobile highlight text (only displayed if Highlight Mobile Phone is selected).",
        IsRequired = false,
        DefaultValue = "Help us keep you in the loop by providing your mobile phone number and opting in for text messages. We'll only send you the most important information at this number.",
        Order = 15,
        Key = AttributeKey.MobileHighlightText )]

    [BooleanField(
        "Require Adult Email Address",
        Key = AttributeKey.RequireAdultEmailAddress,
        Description = "Require an email address on adult records",
        DefaultBooleanValue = true,
        Order = 16 )]

    [BooleanField(
        "Show Email Preference",
        Key = AttributeKey.ShowEmailPreference,
        Description = "Show the email preference and allow it to be edited",
        DefaultBooleanValue = true,
        Order = 17 )]

    [BooleanField(
        "Show Communication Preference",
        Key = AttributeKey.ShowCommunicationPreference,
        Description = "Show the communication preference and allow it to be edited",
        DefaultBooleanValue = true,
        Order = 18 )]

    [LinkedPage(
        "Workflow Launch Page",
        Key = AttributeKey.RequestChangesPage,
        Description = "Page used to launch the workflow to make a profile change request",
        IsRequired = false,
        Order = 19 )]

    [TextField(
        "Request Changes Text",
        Key = AttributeKey.RequestChangesText,
        Description = "The text to use for the request changes button (only displayed if there is a 'Workflow Launch Page' configured).",
        IsRequired = false,
        DefaultValue = "Request Additional Changes",
        Order = 20 )]

    [AttributeField(
        "Family Attributes",
        Key = AttributeKey.FamilyAttributes,
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        EntityTypeQualifierColumn = "GroupTypeId",
        EntityTypeQualifierValue = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        Description = "The family attributes that should be displayed / edited.",
        IsRequired = false,
        AllowMultiple = true,
        Order = 21 )]

    [AttributeField(
        "Person Attributes (adults)",
        Key = AttributeKey.PersonAttributesAdults,
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        Description = "The person attributes that should be displayed / edited for adults.",
        IsRequired = false,
        AllowMultiple = true,
        Order = 22 )]

    [AttributeField(
        "Person Attributes (children)",
        Key = AttributeKey.PersonAttributesChildren,
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        Description = "The person attributes that should be displayed / edited for children.",
        IsRequired = false,
        AllowMultiple = true,
        Order = 23 )]

    [BooleanField(
        "Show Campus Selector",
        Key = AttributeKey.ShowCampusSelector,
        Description = "Allows selection of primary campus.",
        DefaultBooleanValue = false,
        Order = 24 )]

    [TextField(
        "Campus Selector Label",
        Key = AttributeKey.CampusSelectorLabel,
        Description = "The label for the campus selector (only effective when \"Show Campus Selector\" is enabled).",
        IsRequired = false,
        DefaultValue = "Campus",
        Order = 25 )]

    [BooleanField(
        "Require Gender",
        Key = AttributeKey.RequireGender,
        Description = "Controls whether or not the gender field is required.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Order = 26 )]

    [BooleanField(
        "Show Gender",
        Key = AttributeKey.ShowGender,
        Description = "Whether gender is shown or not.",
        DefaultBooleanValue = true,
        Order = 27 )]

    [CustomDropdownListField(
        "Race",
        Key = AttributeKey.RaceOption,
        Description = "Allow Race to be optionally selected.",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Order = 28 )]

    [CustomDropdownListField(
        "Ethnicity",
        Key = AttributeKey.EthnicityOption,
        Description = "Allow Ethnicity to be optionally selected.",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Order = 29 )]

    [CodeEditorField( "View Template",
        Key = AttributeKey.ViewTemplate,
        Description = "The lava template to use to format the view details.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = "{% include '~/Assets/Lava/PublicProfile.lava' %}",
        Order = 30 )]

    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "841D1670-8BFD-4913-8409-FB47EB7A2AB9" )]
    public partial class PublicProfileEdit : RockBlock
    {
        private static class AttributeKey
        {
            public const string DefaultConnectionStatus = "DefaultConnectionStatus";
            public const string DisableNameEdit = "DisableNameEdit";
            public const string ShowTitle = "ShowTitle";
            public const string ShowSuffix = "ShowSuffix";
            public const string ShowNickName = "ShowNickName";
            public const string DisplayMode = "DisplayMode";
            public const string ShowGender = "ShowGender";
            public const string ShowFamilyMembers = "ShowFamilyMembers";
            public const string ShowAddresses = "ShowAddresses";
            public const string AddressTypeValueGuid = "AddressType";
            public const string ShowPhoneNumbers = "ShowPhoneNumbers";
            public const string PhoneTypeValueGuids = "PhoneNumbers";
            public const string HighlightMobilePhone = "HighlightMobilePhone";
            public const string MobileHighlightTitle = "MobileHighlightTitle";
            public const string MobileHighlightText = "MobileHighlightText";
            public const string RequiredAdultPhoneTypes = "RequiredAdultPhoneTypes";
            public const string RequireAdultEmailAddress = "RequireAdultEmailAddress";
            public const string ShowEmailPreference = "ShowEmailPreference";
            public const string ShowCommunicationPreference = "ShowCommunicationPreference";
            public const string RequestChangesPage = "WorkflowLaunchPage";
            public const string RequestChangesText = "RequestChangesText";
            public const string FamilyAttributes = "FamilyAttributes";
            public const string PersonAttributesAdults = "PersonAttributes(adults)";
            public const string PersonAttributesChildren = "PersonAttributes(children)";
            public const string ShowCampusSelector = "ShowCampusSelector";
            public const string CampusSelectorLabel = "CampusSelectorLabel";
            public const string RequireGender = "RequireGender";
            public const string ViewTemplate = "ViewTemplate";
            public const string RaceOption = "RaceOption";
            public const string EthnicityOption = "EthnicityOption";
        }

        private static class PageParametersName
        {
            public const string ReturnUrl = "ReturnUrl";
        }

        private static class MergeFieldKey
        {
            /// <summary>
            /// If only View mode should be enabled.
            /// If this is true, the Edit button should not be visible
            /// </summary>
            public const string ViewOnly = "ViewOnly";

            /// <summary>
            /// Whether the public profile displays in View, Edit or View & Edit mode.
            /// </summary>
            public const string DisplayMode = "DisplayMode";

            /// <summary>
            /// The family that is currently selected (the person could be in multiple families).
            /// <see cref="Rock.Model.Group" />
            /// </summary>
            public const string Family = "Family";

            /// <summary>
            /// True if Family Members should be listed
            /// </summary>
            public const string ShowFamilyMembers = "ShowFamilyMembers";

            /// <summary>
            /// True if Email Preference options should be shown.
            /// </summary>
            public const string ShowEmailPreference = "ShowEmailPreference";

            /// <summary>
            /// The members of the selected family.
            /// List of <see cref="Rock.Model.GroupMember"/>
            /// </summary>
            public const string FamilyMembers = "FamilyMembers";

            /// <summary>
            /// True if gender should be shown.
            /// </summary>
            public const string ShowGender = "ShowGender";

            /// <summary>
            /// True if the person's title should be shown.
            /// </summary>
            public const string ShowTitle = "ShowTitle";

            /// <summary>
            /// True if the person's suffix should be shown.
            /// </summary>
            public const string ShowSuffix = "ShowSuffix";

            /// <summary>
            /// The families that this person is in.
            /// List <see cref="Rock.Model.Group" />
            /// </summary>
            public const string Families = "Families";

            /// <summary>
            /// The address type defined value id that is displayed.
            /// <see cref="DefinedValueCache">DefinedValue Id</see>
            /// </summary>
            public const string AddressTypeValueId = "AddressTypeValueId";

            /// <summary>
            /// The address to be displayed.
            /// <see cref="Rock.Model.GroupLocation"/>
            /// </summary>
            public const string Address = "Address";

            /// <summary>
            /// True if addresses should be shown.
            /// </summary>
            public const string ShowAddresses = "ShowAddresses";

            /// <summary>
            /// True if phone numbers should be shown
            /// </summary>
            public const string ShowPhoneNumbers = "ShowPhoneNumbers";

            /// <summary>
            /// The Phone Types defined value ids  that should be shown.
            /// List of <see cref="DefinedValueCache">DefinedValue Id</see>
            /// </summary>
            public const string DisplayedPhoneTypeValueIds = "DisplayedPhoneTypeValueIds";

            /// <summary>
            /// The URL to use when they press the Request Changes button
            /// </summary>
            public const string RequestChangesPageUrl = "RequestChangesPageUrl";

            /// <summary>
            /// The text to shown on the Request Changes button
            /// Only show the button if <see cref="RequestChangesPageUrl"/> has a value
            /// </summary>
            public const string RequestChangesText = "RequestChangesText";

            /// <summary>
            /// The family attributes that should be displayed.
            /// List of <see cref="AttributeCache" />
            /// </summary>
            public const string FamilyAttributes = "FamilyAttributes";

            /// <summary>
            /// The person attributes that should be displayed for Adults
            /// List of <see cref="AttributeCache" />
            /// </summary>
            public const string PersonAttributesAdults = "PersonAttributesAdults";

            /// The person attributes that should be displayed for Children
            /// List of <see cref="AttributeCache" />
            /// </summary>
            public const string PersonAttributesChildren = "PersonAttributesChildren";
        }

        private static class EventArgumentKey
        {
            public const string SelectFamily = "SelectFamily";
            public const string EditPerson = "EditPerson";
            public const string AddGroupMember = "AddGroupMember";
        }

        private static class ListSource
        {
            public const string HIDE_OPTIONAL_REQUIRED = "Hide,Optional,Required";
        }

        private static class DisplayMode
        {
            public const string ViewOnly = "VIEW";
            public const string EditOnly = "EDIT";
            public const string ViewAndEdit = "EDITVIEW";
        }

        #region Fields

        private List<Guid> _requiredPhoneNumberGuids = new List<Guid>();
        private bool _isEditRecordAdult = false;
        private string _displayMode;

        #endregion

        #region Properties

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            ScriptManager.RegisterStartupScript( ddlGradePicker, ddlGradePicker.GetType(), "grade-selection-" + BlockId.ToString(), ddlGradePicker.GetJavascriptForYearPicker( ypGraduation ), true );

            dvpTitle.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_TITLE ) ).Id;

            dvpSuffix.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_SUFFIX ) ).Id;

            _displayMode = GetAttributeValue( AttributeKey.DisplayMode );

            SetElementVisibility();

            RockPage.AddCSSLink( "~/Styles/fluidbox.css" );
            RockPage.AddScriptLink( "~/Scripts/imagesloaded.min.js" );
            RockPage.AddScriptLink( "~/Scripts/jquery.fluidbox.min.js" );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += PublicProfileEdit_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upContent );

            cpCampus.Label = GetAttributeValue( AttributeKey.CampusSelectorLabel );

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.RequiredAdultPhoneTypes ) ) )
            {
                _requiredPhoneNumberGuids = GetAttributeValue( AttributeKey.RequiredAdultPhoneTypes ).Split( ',' ).Select( Guid.Parse ).ToList();
            }

            rContactInfo.ItemDataBound += rContactInfo_ItemDataBound;

            string smsScript = @"
    $('.js-sms-number').click(function () {
        if ($(this).is(':checked')) {
            $('.js-sms-number').not($(this)).prop('checked', false);
        }
    });
";
            ScriptManager.RegisterStartupScript( rContactInfo, rContactInfo.GetType(), "sms-number-" + BlockId.ToString(), smsScript, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( CurrentPerson == null )
            {
                pnlView.Visible = false;
                pnlEdit.Visible = false;
                pnlEdit.Visible = false;
                nbNotAuthorized.Visible = true;
                return;
            }

            if ( !Page.IsPostBack )
            {
                if ( _displayMode == DisplayMode.EditOnly )
                {
                    ShowEditPersonDetails( CurrentPerson.Guid );
                }
                else
                {
                    ShowViewDetail();
                }
            }
            else
            {
                var handled = HandleLavaPostback( this.Request.Params["__EVENTTARGET"], this.Request.Params["__EVENTARGUMENT"] );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the PublicProfileEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PublicProfileEdit_BlockUpdated( object sender, EventArgs e )
        {
            SetElementVisibility();

            if ( _displayMode != DisplayMode.EditOnly )
            {
                ShowViewDetail();
            }
        }

        /// <summary>
        /// Handles any custom post-backs from the Lava.
        /// Returns true if one of the custom Lava post-backs was handled
        /// </summary>
        /// <param name="eventTarget">The event target.</param>
        /// <param name="eventArgument">The event argument.</param>
        /// <returns></returns>
        private bool HandleLavaPostback( string eventTarget, string eventArgument )
        {
            if ( !eventTarget.Equals( upContent.UniqueID, StringComparison.OrdinalIgnoreCase ) )
            {
                // post back from some other block
                return false;
            }

            var eventArgumentParts = eventArgument.Split( '^' );
            if ( !eventArgumentParts.Any() )
            {
                return false;
            }

            var eventArgumentKey = eventArgumentParts[0];
            var eventArgumentValue = eventArgumentParts.Length > 1 ? eventArgumentParts[1] : string.Empty;

            if ( eventArgumentKey == EventArgumentKey.SelectFamily )
            {
                hfGroupId.Value = this.Request.Form["selectFamily"];
                ShowViewDetail();
                return true;
            }
            else if ( eventArgumentKey == EventArgumentKey.EditPerson )
            {
                ShowEditPersonDetails( eventArgumentValue.AsGuid() );
                return true;
            }
            else if ( eventArgumentKey == EventArgumentKey.AddGroupMember )
            {
                AddGroupMember( eventArgumentValue.AsInteger() );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowViewDetail()
        {
            var rockContext = new RockContext();
            var groupMemberService = new GroupMemberService( rockContext );
            var attributeValueService = new AttributeValueService( rockContext );

            // invalid situation; return and report nothing.
            if ( CurrentPerson == null )
            {
                return;
            }

            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage );
            var personFamilies = CurrentPerson.GetFamilies();
            Group selectedFamily;
            var selectedGroupId = hfGroupId.Value.AsIntegerOrNull();
            if ( selectedGroupId.HasValue )
            {
                selectedFamily = personFamilies.FirstOrDefault( a => a.Id == selectedGroupId );
            }
            else
            {
                selectedFamily = CurrentPerson.GetFamily();
            }

            if ( selectedFamily == null )
            {
                selectedFamily = CurrentPerson.GetFamily();
            }

            hfGroupId.Value = selectedFamily.Id.ToString();

            mergeFields.Add( MergeFieldKey.Family, selectedFamily );

            var familyMembers = groupMemberService.Queryable()
                            .Where( gm => gm.GroupId == selectedFamily.Id
                                 && gm.PersonId != CurrentPerson.Id
                                 && gm.Person.IsDeceased == false )
                            .OrderBy( m => m.GroupRole.Order )
                            .ToList();

            var orderedMembers = new List<GroupMember>();

            // Add adult males
            orderedMembers.AddRange( familyMembers
                .Where( m => m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                    m.Person.Gender == Gender.Male )
                .OrderByDescending( m => m.Person.Age ) );

            // Add adult females
            orderedMembers.AddRange( familyMembers
                .Where( m => m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                    m.Person.Gender != Gender.Male )
                .OrderByDescending( m => m.Person.Age ) );

            // Add non-adults
            orderedMembers.AddRange( familyMembers
                .Where( m => !m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) )
                .OrderByDescending( m => m.Person.Age ) );

            // Prevent breaking changes for the legacy, "ViewOnly" MergeField.
            var isViewOnly = _displayMode == DisplayMode.ViewOnly;

            mergeFields.Add( MergeFieldKey.FamilyMembers, orderedMembers );

            mergeFields.Add( MergeFieldKey.ShowFamilyMembers, GetAttributeValue( AttributeKey.ShowFamilyMembers ).AsBoolean() );
            mergeFields.Add( MergeFieldKey.Families, personFamilies );
            mergeFields.Add( MergeFieldKey.ViewOnly, isViewOnly );
            mergeFields.Add( MergeFieldKey.DisplayMode, _displayMode );
            mergeFields.Add( MergeFieldKey.AddressTypeValueId, DefinedValueCache.GetId( GetAttributeValue( AttributeKey.AddressTypeValueGuid ).AsGuid() ) );

            var groupLocationTypeValueGuid = this.GetAttributeValue( AttributeKey.AddressTypeValueGuid ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();
            var groupLocationTypeValueId = DefinedValueCache.GetId( groupLocationTypeValueGuid );

            if ( groupLocationTypeValueId.HasValue )
            {
                var familyGroupLocation = selectedFamily.GroupLocations.Where( a => a.GroupLocationTypeValueId == groupLocationTypeValueId.Value ).FirstOrDefault();
                mergeFields.Add( MergeFieldKey.Address, familyGroupLocation );
            }

            var showAddresses = GetAttributeValue( AttributeKey.ShowAddresses ).AsBoolean();
            pnlAddress.Visible = showAddresses;
            mergeFields.Add( MergeFieldKey.ShowAddresses, showAddresses );

            var showGender = GetAttributeValue( AttributeKey.ShowGender ).AsBoolean();
            rblGender.Visible = showGender;
            mergeFields.Add( MergeFieldKey.ShowGender, showGender );

            var showTitle = GetAttributeValue( AttributeKey.ShowTitle ).AsBoolean();
            mergeFields.Add( MergeFieldKey.ShowTitle, showTitle );

            var showSuffix = GetAttributeValue( AttributeKey.ShowSuffix ).AsBoolean();
            mergeFields.Add( MergeFieldKey.ShowSuffix, showSuffix );

            var showEmailPreference = GetAttributeValue( AttributeKey.ShowEmailPreference ).AsBoolean();
            rblEmailPreference.Visible = showEmailPreference;
            mergeFields.Add( MergeFieldKey.ShowEmailPreference, showEmailPreference );

            mergeFields.Add( MergeFieldKey.ShowPhoneNumbers, GetAttributeValue( AttributeKey.ShowPhoneNumbers ).AsBoolean() );

            var phoneTypeValueIds = GetAttributeValues( AttributeKey.PhoneTypeValueGuids ).AsGuidList().Select( a => DefinedValueCache.GetId( a ) ).ToList();
            mergeFields.Add( MergeFieldKey.DisplayedPhoneTypeValueIds, phoneTypeValueIds );

            var requestChangesPageUrl = LinkedPageUrl( AttributeKey.RequestChangesPage, new Dictionary<string, string>() );
            mergeFields.Add( MergeFieldKey.RequestChangesPageUrl, requestChangesPageUrl );
            mergeFields.Add( MergeFieldKey.RequestChangesText, GetAttributeValue( AttributeKey.RequestChangesText ) );

            var familyAttributes = GetAttributeValues( AttributeKey.FamilyAttributes ).AsGuidList().Select( a => AttributeCache.Get( a ) );
            mergeFields.Add( MergeFieldKey.FamilyAttributes, familyAttributes );

            mergeFields.Add( MergeFieldKey.PersonAttributesChildren, GetAttributeValues( AttributeKey.PersonAttributesChildren ).AsGuidList().Select( a => AttributeCache.Get( a ) ) );
            mergeFields.Add( MergeFieldKey.PersonAttributesAdults, GetAttributeValues( AttributeKey.PersonAttributesAdults ).AsGuidList().Select( a => AttributeCache.Get( a ) ) );

            var viewPersonLavaTemplate = GetAttributeValue( AttributeKey.ViewTemplate );

            var viewPersonHtml = viewPersonLavaTemplate.ResolveMergeFields( mergeFields ).ResolveClientIds( upContent.UniqueID );
            lViewPersonContent.Visible = true;
            lViewPersonContent.Text = viewPersonHtml;

            hfEditPersonGuid.Value = Guid.Empty.ToString();
            pnlEdit.Visible = false;
            pnlView.Visible = true;

            rblRole.Items.Clear();
            var familyRoles = selectedFamily.GroupType.Roles.OrderBy( r => r.Order ).ToList();
            foreach ( var role in familyRoles )
            {
                rblRole.Items.Add( new ListItem( role.Name, role.Id.ToString() ) );
            }

            rblRole.SetValue( CurrentPerson.GetFamilyRole() );
        }

        /// <summary>
        /// Verifies whether the current person is in the given group (Family).
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns>
        ///   <c>true</c> if the current person is in the group; otherwise, <c>false</c>.
        /// </returns>
        private bool IsCurrentPersonInGroup( Group group )
        {
            if ( group == null )
            {
                return false;
            }

            return group.Members.Where( gm => gm.PersonId == CurrentPersonId ).Any();
        }

        /// <summary>
        /// Verifies that the personGuid (if not empty) or the given person is a member of the given group.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="group">The group.</param>
        private bool IsValidPersonForGroup( Guid personGuid, Person person, Group group )
        {
            if ( personGuid == Guid.Empty )
            {
                // When the personGuid is empty, then we check based on the given person's Id is in the group.
                return IsValidPersonForGroup( person, group );
            }
            else
            {
                // Is the given person (their guid) in the group?
                return group.Members.Where( gm => gm.Person.Guid == personGuid ).Any();
            }
        }

        /// <summary>
        /// Verifies the given person's Id is a member of the given group.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="group">The group.</param>
        private bool IsValidPersonForGroup( Person person, Group group )
        {
            // Is the given person' (their Id) in the group?
            return group.Members.Where( gm => gm.PersonId == person.Id ).Any();
        }

        /// <summary>
        /// Displays or hides the selected elements based on the business rules
        /// </summary>
        private void SetElementVisibility()
        {
            dvpTitle.Visible = GetAttributeValue( AttributeKey.ShowTitle ).AsBoolean();
            dvpSuffix.Visible = GetAttributeValue( AttributeKey.ShowSuffix ).AsBoolean();
            tbNickName.Visible = GetAttributeValue( AttributeKey.ShowNickName ).AsBoolean();

            rpRace.Visible = GetAttributeValue( AttributeKey.RaceOption ) != "Hide";
            rpRace.Required = GetAttributeValue( AttributeKey.RaceOption ) == "Required";

            epEthnicity.Visible = GetAttributeValue( AttributeKey.EthnicityOption ) != "Hide";
            epEthnicity.Required = GetAttributeValue( AttributeKey.EthnicityOption ) == "Required";

            // There's no need to show the Cancel button when the DisplayMode is EditOnly.
            if ( _displayMode == DisplayMode.EditOnly )
            {
                btnCancel.Visible = false;
            }
        }

        #endregion

        #region Events

        #region View Events

        /// <summary>
        /// Handles the Click event of the lbEditPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditPerson_Click( object sender, EventArgs e )
        {
            ShowEditPersonDetails( CurrentPerson.Guid );
        }

        /// <summary>
        /// Handles the Click event of the lbMoved control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbMoved_Click( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( acAddress.Street1 ) )
            {
                hfStreet1.Value = acAddress.Street1;
                hfStreet2.Value = acAddress.Street2;
                hfCity.Value = acAddress.City;
                hfState.Value = acAddress.State;
                hfPostalCode.Value = acAddress.PostalCode;
                hfCountry.Value = acAddress.Country;

                Location currentAddress = new Location();
                acAddress.Required = true;
                acAddress.GetValues( currentAddress );
                lPreviousAddress.Text = string.Format( "<strong>Previous Address</strong><br />{0}", currentAddress.FormattedHtmlAddress );

                acAddress.Street1 = string.Empty;
                acAddress.Street2 = string.Empty;
                acAddress.PostalCode = string.Empty;
                acAddress.City = string.Empty;

                cbIsMailingAddress.Checked = true;
                cbIsPhysicalAddress.Checked = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void AddGroupMember( int groupId )
        {
            ShowEditPersonDetails( Guid.Empty );
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var personGuid = hfEditPersonGuid.Value.AsGuid();

            if ( !hfGroupId.Value.AsIntegerOrNull().HasValue )
            {
                return;
            }

            var groupId = hfGroupId.Value.AsIntegerOrNull();
            if ( !groupId.HasValue )
            {
                // GroupId wasn't specified due to invalid situation
                // Return and report nothing.
                return;
            }

            var group = new GroupService( rockContext ).Get( groupId.Value );
            if ( group == null )
            {
                // A valid group wasn't specified.
                // Return and report nothing.
                return;
            }

            // invalid situation; return and report nothing.
            if ( !IsCurrentPersonInGroup( group ) )
            {
                return;
            }

            // Validate before continuing; either the personGuid or the CurrentPerson must be in the group.
            if ( !IsValidPersonForGroup( personGuid, CurrentPerson, group ) )
            {
                return;
            }

            bool showPhoneNumbers = GetAttributeValue( AttributeKey.ShowPhoneNumbers ).AsBoolean();
            bool showCommunicationPreference = GetAttributeValue( AttributeKey.ShowCommunicationPreference ).AsBoolean();
            var communicationPreference = rblCommunicationPreference.SelectedValueAsEnum<CommunicationType>();

            var wrapTransactionResult = rockContext.WrapTransactionIf( () =>
            {
                var personService = new PersonService( rockContext );

                if ( personGuid == Guid.Empty )
                {
                    var groupMemberService = new GroupMemberService( rockContext );
                    var groupMember = new GroupMember() { Person = new Person(), Group = group, GroupId = group.Id };
                    groupMember.Person.TitleValueId = dvpTitle.SelectedValueAsId();
                    groupMember.Person.FirstName = tbFirstName.Text;
                    groupMember.Person.NickName = tbNickName.Text;
                    groupMember.Person.LastName = tbLastName.Text;
                    groupMember.Person.SuffixValueId = dvpSuffix.SelectedValueAsId();
                    groupMember.Person.Gender = rblGender.SelectedValueAsEnum<Gender>();
                    DateTime? birthdate = bpBirthDay.SelectedDate;
                    if ( birthdate.HasValue )
                    {
                        // If setting a future birthdate, subtract a century until birthdate is not greater than today.
                        var today = RockDateTime.Today;
                        while ( birthdate.Value.CompareTo( today ) > 0 )
                        {
                            birthdate = birthdate.Value.AddYears( -100 );
                        }
                    }

                    groupMember.Person.SetBirthDate( birthdate );
                    if ( ddlGradePicker.Visible )
                    {
                        groupMember.Person.GradeOffset = ddlGradePicker.SelectedValueAsInt();
                    }

                    var role = group.GroupType.Roles.Where( r => r.Id == ( rblRole.SelectedValueAsInt() ?? 0 ) ).FirstOrDefault();
                    if ( role != null )
                    {
                        groupMember.GroupRole = role;
                        groupMember.GroupRoleId = role.Id;
                    }

                    var connectionStatusGuid = GetAttributeValue( AttributeKey.DefaultConnectionStatus ).AsGuidOrNull();
                    if ( connectionStatusGuid.HasValue )
                    {
                        groupMember.Person.ConnectionStatusValueId = DefinedValueCache.Get( connectionStatusGuid.Value ).Id;
                    }
                    else
                    {
                        groupMember.Person.ConnectionStatusValueId = CurrentPerson.ConnectionStatusValueId;
                    }

                    var headOfHousehold = GroupServiceExtensions.HeadOfHousehold( group.Members.AsQueryable() );
                    if ( headOfHousehold != null )
                    {
                        DefinedValueCache dvcRecordStatus = DefinedValueCache.Get( headOfHousehold.RecordStatusValueId ?? 0 );
                        if ( dvcRecordStatus != null )
                        {
                            groupMember.Person.RecordStatusValueId = dvcRecordStatus.Id;
                        }
                    }

                    if ( groupMember.GroupRole.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                    {
                        groupMember.Person.GivingGroupId = group.Id;
                    }

                    groupMember.Person.IsEmailActive = true;
                    groupMember.Person.EmailPreference = EmailPreference.EmailAllowed;
                    groupMember.Person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

                    groupMemberService.Add( groupMember );
                    rockContext.SaveChanges();
                    personGuid = groupMember.Person.Guid;
                }

                var person = personService.Get( personGuid );
                if ( person != null )
                {
                    int? orphanedPhotoId = null;
                    if ( person.PhotoId != imgPhoto.BinaryFileId )
                    {
                        orphanedPhotoId = person.PhotoId;
                        person.PhotoId = imgPhoto.BinaryFileId;
                    }

                    person.TitleValueId = dvpTitle.SelectedValueAsInt();
                    person.FirstName = tbFirstName.Text;
                    person.NickName = tbNickName.Text;
                    person.LastName = tbLastName.Text;
                    person.SuffixValueId = dvpSuffix.SelectedValueAsInt();
                    person.RaceValueId = rpRace.SelectedValueAsId();
                    person.EthnicityValueId = epEthnicity.SelectedValueAsId();

                    var birthMonth = person.BirthMonth;
                    var birthDay = person.BirthDay;
                    var birthYear = person.BirthYear;

                    var birthday = bpBirthDay.SelectedDate;
                    if ( birthday.HasValue )
                    {
                        // If setting a future birthdate, subtract a century until birthdate is not greater than today.
                        var today = RockDateTime.Today;
                        while ( birthday.Value.CompareTo( today ) > 0 )
                        {
                            birthday = birthday.Value.AddYears( -100 );
                        }

                        person.BirthMonth = birthday.Value.Month;
                        person.BirthDay = birthday.Value.Day;
                        if ( birthday.Value.Year != DateTime.MinValue.Year )
                        {
                            person.BirthYear = birthday.Value.Year;
                        }
                        else
                        {
                            person.BirthYear = null;
                        }
                    }
                    else
                    {
                        person.SetBirthDate( null );
                    }

                    if ( ddlGradePicker.Visible )
                    {
                        int? graduationYear = null;
                        if ( ypGraduation.SelectedYear.HasValue )
                        {
                            graduationYear = ypGraduation.SelectedYear.Value;
                        }

                        person.GraduationYear = graduationYear;
                    }

                    person.Gender = rblGender.SelectedValue.ConvertToEnum<Gender>();

                    // update campus
                    // bool showCampus = GetAttributeValue( AttributeKey.ShowCampusSelector ).AsBoolean();
                    // Even if the block is set to show the picker it will not be visible if there is only one campus so use the Visible prop instead of the attribute value here.
                    if ( cpCampus.Visible )
                    {
                        var primaryFamily = person.GetFamily( rockContext );
                        if ( primaryFamily.CampusId != cpCampus.SelectedCampusId )
                        {
                            primaryFamily.CampusId = cpCampus.SelectedCampusId;
                        }
                    }

                    if ( showPhoneNumbers )
                    {
                        var phoneNumberTypeIds = new List<int>();

                        bool smsSelected = false;

                        foreach ( RepeaterItem item in rContactInfo.Items )
                        {
                            HiddenField hfPhoneType = item.FindControl( "hfPhoneType" ) as HiddenField;
                            PhoneNumberBox pnbPhone = item.FindControl( "pnbPhone" ) as PhoneNumberBox;
                            CheckBox cbSms = item.FindControl( "cbSms" ) as CheckBox;

                            if ( hfPhoneType != null
                                && pnbPhone != null
                                && cbSms != null )
                            {
                                if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( pnbPhone.Number ) ) )
                                {
                                    int phoneNumberTypeId;
                                    if ( int.TryParse( hfPhoneType.Value, out phoneNumberTypeId ) )
                                    {
                                        var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneNumberTypeId );
                                        string oldPhoneNumber = string.Empty;
                                        if ( phoneNumber == null )
                                        {
                                            phoneNumber = new PhoneNumber { NumberTypeValueId = phoneNumberTypeId };
                                            person.PhoneNumbers.Add( phoneNumber );
                                        }
                                        else
                                        {
                                            oldPhoneNumber = phoneNumber.NumberFormattedWithCountryCode;
                                        }

                                        phoneNumber.CountryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );
                                        phoneNumber.Number = PhoneNumber.CleanNumber( pnbPhone.Number );

                                        // Only allow one number to have SMS selected
                                        if ( smsSelected )
                                        {
                                            phoneNumber.IsMessagingEnabled = false;
                                        }
                                        else
                                        {
                                            phoneNumber.IsMessagingEnabled = cbSms.Checked;
                                            smsSelected = cbSms.Checked;
                                        }

                                        phoneNumberTypeIds.Add( phoneNumberTypeId );
                                    }
                                }
                            }
                        }

                        var selectedPhoneTypeGuids = GetAttributeValue( AttributeKey.PhoneTypeValueGuids ).Split( ',' ).AsGuidList();

                        var phoneNumberService = new PhoneNumberService( rockContext );

                        // Remove any duplicate numbers
                        var hasDuplicate = person.PhoneNumbers.GroupBy( pn => pn.Number ).Where( g => g.Count() > 1 ).Any();

                        if ( hasDuplicate )
                        {
                            var listOfValidNumbers = person.PhoneNumbers
                                .OrderBy( o => o.NumberTypeValueId )
                                .GroupBy( pn => pn.Number )
                                .Select( y => y.First() )
                                .ToList();
                            var removedNumbers = person.PhoneNumbers.Except( listOfValidNumbers ).ToList();
                            phoneNumberService.DeleteRange( removedNumbers );
                            person.PhoneNumbers = listOfValidNumbers;
                        }
                    }

                    person.Email = tbEmail.Text.Trim();
                    person.EmailPreference = rblEmailPreference.SelectedValue.ConvertToEnum<EmailPreference>();

                    /* 2020-10-06 MDP
                     To help prevent a person from setting their communication preference to SMS, even if they don't have an SMS number,
                      we'll require an SMS number in these situations. The goal is to only enforce if they are able to do something about it.
                      1) The block is configured to show both 'Communication Preference' and 'Phone Numbers'.
                      2) Communication Preference is set to SMS

                     Edge cases
                       - Both #1 and #2 are true, but no Phone Types are selected in block settings. In this case, still enforce.
                         Think of this as a block configuration issue (they shouldn't have configured it that way)

                       - Person has an SMS phone number, but the block settings don't show it. We'll see if any of the Person's phone numbers
                         have SMS, including ones that are not shown. So, they can set communication preference to SMS without getting a warning.

                    NOTE: We might have already done a save changes at this point, but we are in a DB Transaction, so it'll get rolled back if
                        we return false, with a warning message.
                     */

                    if ( showCommunicationPreference && showPhoneNumbers && communicationPreference == CommunicationType.SMS )
                    {
                        if ( !person.PhoneNumbers.Any( a => a.IsMessagingEnabled ) )
                        {
                            nbCommunicationPreferenceWarning.Text = "A phone number with SMS enabled is required when Communication Preference is set to SMS.";
                            nbCommunicationPreferenceWarning.NotificationBoxType = NotificationBoxType.Warning;
                            nbCommunicationPreferenceWarning.Visible = true;
                            return false;
                        }
                    }

                    person.CommunicationPreference = communicationPreference;

                    person.LoadAttributes();

                    if ( avcPersonAttributesAdult.Visible )
                    {
                        avcPersonAttributesAdult.GetEditValues( person );
                    }
                    else
                    {
                        avcPersonAttributesChild.GetEditValues( person );
                    }

                    if ( person.IsValid )
                    {
                        if ( rockContext.SaveChanges() > 0 )
                        {
                            if ( orphanedPhotoId.HasValue )
                            {
                                BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                                var binaryFile = binaryFileService.Get( orphanedPhotoId.Value );
                                if ( binaryFile != null )
                                {
                                    // marked the old images as IsTemporary so they will get cleaned up later
                                    binaryFile.IsTemporary = true;
                                    rockContext.SaveChanges();
                                }
                            }

                            // if they used the ImageEditor, and cropped it, the original file is still in BinaryFile. So clean it up.
                            if ( imgPhoto.CropBinaryFileId.HasValue )
                            {
                                if ( imgPhoto.CropBinaryFileId != person.PhotoId )
                                {
                                    BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                                    var binaryFile = binaryFileService.Get( imgPhoto.CropBinaryFileId.Value );
                                    if ( binaryFile != null && binaryFile.IsTemporary )
                                    {
                                        string errorMessage;
                                        if ( binaryFileService.CanDelete( binaryFile, out errorMessage ) )
                                        {
                                            binaryFileService.Delete( binaryFile );
                                            rockContext.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }

                        person.SaveAttributeValues( rockContext );

                        // save family information
                        if ( pnlAddress.Visible )
                        {
                            Guid? familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuidOrNull();
                            if ( familyGroupTypeGuid.HasValue )
                            {
                                var familyGroup = new GroupService( rockContext )
                                    .Queryable()
                                    .Where( f =>
                                        f.Id == groupId.Value &&
                                        f.Members.Any( m => m.PersonId == person.Id ) )
                                    .FirstOrDefault();
                                if ( familyGroup != null )
                                {
                                    Guid? addressTypeGuid = GetAttributeValue( AttributeKey.AddressTypeValueGuid ).AsGuidOrNull();
                                    if ( addressTypeGuid.HasValue )
                                    {
                                        var groupLocationService = new GroupLocationService( rockContext );

                                        var dvHomeAddressType = DefinedValueCache.Get( addressTypeGuid.Value );
                                        var familyAddress = groupLocationService.Queryable().Where( l => l.GroupId == familyGroup.Id && l.GroupLocationTypeValueId == dvHomeAddressType.Id ).FirstOrDefault();
                                        if ( familyAddress != null && string.IsNullOrWhiteSpace( acAddress.Street1 ) )
                                        {
                                            // delete the current address
                                            groupLocationService.Delete( familyAddress );
                                            rockContext.SaveChanges();
                                        }
                                        else
                                        {
                                            if ( !string.IsNullOrWhiteSpace( acAddress.Street1 ) )
                                            {
                                                if ( familyAddress == null )
                                                {
                                                    familyAddress = new GroupLocation();
                                                    groupLocationService.Add( familyAddress );
                                                    familyAddress.GroupLocationTypeValueId = dvHomeAddressType.Id;
                                                    familyAddress.GroupId = familyGroup.Id;
                                                    familyAddress.IsMailingLocation = true;
                                                    familyAddress.IsMappedLocation = true;
                                                }
                                                else if ( hfStreet1.Value != string.Empty )
                                                {
                                                    // user clicked move so create a previous address
                                                    var previousAddress = new GroupLocation();
                                                    groupLocationService.Add( previousAddress );

                                                    var previousAddressValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() );
                                                    if ( previousAddressValue != null )
                                                    {
                                                        previousAddress.GroupLocationTypeValueId = previousAddressValue.Id;
                                                        previousAddress.GroupId = familyGroup.Id;

                                                        Location previousAddressLocation = new Location();
                                                        previousAddressLocation.Street1 = hfStreet1.Value;
                                                        previousAddressLocation.Street2 = hfStreet2.Value;
                                                        previousAddressLocation.City = hfCity.Value;
                                                        previousAddressLocation.State = hfState.Value;
                                                        previousAddressLocation.PostalCode = hfPostalCode.Value;
                                                        previousAddressLocation.Country = hfCountry.Value;

                                                        previousAddress.Location = previousAddressLocation;
                                                    }
                                                }

                                                familyAddress.IsMailingLocation = cbIsMailingAddress.Checked;
                                                familyAddress.IsMappedLocation = cbIsPhysicalAddress.Checked;

                                                var loc = new Location();
                                                acAddress.GetValues( loc );

                                                familyAddress.Location = new LocationService( rockContext ).Get(
                                                    loc.Street1, loc.Street2, loc.City, loc.State, loc.PostalCode, loc.Country, familyGroup, true );

                                                // since there can only be one mapped location, set the other locations to not mapped
                                                if ( familyAddress.IsMappedLocation )
                                                {
                                                    var groupLocations = groupLocationService.Queryable()
                                                        .Where( l => l.GroupId == familyGroup.Id && l.Id != familyAddress.Id ).ToList();

                                                    foreach ( var groupLocation in groupLocations )
                                                    {
                                                        groupLocation.IsMappedLocation = false;
                                                    }
                                                }

                                                rockContext.SaveChanges();
                                            }
                                        }
                                    }

                                    familyGroup.LoadAttributes();
                                    avcFamilyAttributes.GetEditValues( familyGroup );
                                    familyGroup.SaveAttributeValues();
                                }
                            }
                        }
                    }
                }

                return true;
            } );

            if ( wrapTransactionResult )
            {
                if ( _displayMode == DisplayMode.EditOnly )
                {
                    // When in EditOnly mode if there's a ReturnUrl specified navigate to that page.
                    // Otherwise stay on the page, but show a saved success message.
                    var returnUrl = PageParameter( PageParametersName.ReturnUrl );

                    if ( returnUrl.IsNotNullOrWhiteSpace() )
                    {
                        string redirectUrl = Server.UrlDecode( returnUrl );

                        string queryString = string.Empty;
                        if ( redirectUrl.Contains( "?" ) )
                        {
                            queryString = redirectUrl.Split( '?' ).Last();
                        }
                        Context.Response.Redirect( redirectUrl );
                    }

                    hlblSuccess.Visible = true;
                }
                else
                {
                    NavigateToCurrentPage();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            ShowViewDetail();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblRole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblRole_SelectedIndexChanged( object sender, EventArgs e )
        {
            var selectedId = rblRole.SelectedValueAsId();

            if ( selectedId.HasValue )
            {
                SetControlsForRoleType( selectedId.Value );
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rContactInfo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rContactInfo_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var pnbPhone = e.Item.FindControl( "pnbPhone" ) as PhoneNumberBox;
            if ( pnbPhone != null )
            {
                pnbPhone.ValidationGroup = BlockValidationGroup;
                var phoneNumber = e.Item.DataItem as PhoneNumber;
                HtmlGenericControl phoneNumberContainer = ( HtmlGenericControl ) e.Item.FindControl( "divPhoneNumberContainer" );

                if ( _isEditRecordAdult && ( phoneNumber != null ) )
                {
                    pnbPhone.Required = _requiredPhoneNumberGuids.Contains( phoneNumber.NumberTypeValue.Guid );
                    if ( pnbPhone.Required )
                    {
                        pnbPhone.RequiredErrorMessage = string.Format( "{0} phone is required", phoneNumber.NumberTypeValue.Value );
                        phoneNumberContainer.AddCssClass( "required" );
                    }
                }

                if ( phoneNumber.NumberTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() )
                {
                    if ( GetAttributeValue( AttributeKey.HighlightMobilePhone ).AsBoolean() )
                    {
                        var hightlightTitle = ( Literal ) e.Item.FindControl( "litHighlightTitle" );
                        hightlightTitle.Text = $"<h4>{GetAttributeValue( AttributeKey.MobileHighlightTitle )}</h4>";
                        hightlightTitle.Visible = true;

                        var hightlightText = ( Literal ) e.Item.FindControl( "litHighlightText" );
                        hightlightText.Text = $"<p>{GetAttributeValue( AttributeKey.MobileHighlightText )}</p>";
                        hightlightText.Visible = true;

                        phoneNumberContainer.AddCssClass( "well" );
                    }
                }
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Shows the edit person details.
        /// </summary>
        /// <param name="personGuid">The person's global unique identifier.</param>
        private void ShowEditPersonDetails( Guid personGuid )
        {
            lViewPersonContent.Visible = false;
            var childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();

            RockContext rockContext = new RockContext();

            var groupId = hfGroupId.Value.AsIntegerOrNull();

            if ( !groupId.HasValue )
            {
                // It's possible the DisplayMode doesn't default to View
                // so we may need to initialize the selected family ourselves.
                groupId = CurrentPerson.GetFamily().Id;

                if ( groupId == 0 )
                {
                    // invalid situation; return and report nothing.
                    return;
                }

                // We were able to initialize the selected family -
                // ensure the hidden field reflects the correct value.
                hfGroupId.Value = groupId.ToString();
            }


            var group = new GroupService( rockContext ).Get( groupId.Value );
            if ( group == null )
            {
                // invalid situation; return and report nothing.
                return;
            }

            hfEditPersonGuid.Value = personGuid.ToString();
            var person = new Person();

            if ( personGuid == Guid.Empty )
            {
                rblRole.Visible = true;
                rblRole.Required = true;

                tbFirstName.Enabled = true;
                tbLastName.Enabled = true;
            }
            else
            {
                person = new PersonService( rockContext ).Get( personGuid );

                if ( GetAttributeValue( AttributeKey.DisableNameEdit ).AsBoolean() )
                {
                    tbFirstName.Enabled = false;
                    tbLastName.Enabled = false;
                }
            }

            if ( person == null )
            {
                return;
            }

            if ( personGuid == Guid.Empty )
            {
                // make them pick the family role on a new person
                rblRole.SelectedValue = null;
            }
            else
            {
                rblRole.SetValue( person.GetFamilyRole() );
            }

            imgPhoto.BinaryFileId = person.PhotoId;
            imgPhoto.NoPictureUrl = Person.GetPersonNoPictureUrl( person, 200, 200 );
            dvpTitle.SetValue( person.TitleValueId );
            tbFirstName.Text = person.FirstName;
            tbNickName.Text = person.NickName;
            tbLastName.Text = person.LastName;
            dvpSuffix.SetValue( person.SuffixValueId );
            bpBirthDay.SelectedDate = person.BirthDate;
            rpRace.SetValue( person.RaceValueId );
            epEthnicity.SetValue( person.EthnicityValueId );

            // Setup the gender radio button list according to the required field
            var genderRequired = GetAttributeValue( AttributeKey.RequireGender ).AsBooleanOrNull() ?? true;
            if ( !genderRequired )
            {
                rblGender.Items.Add( new ListItem( "Unknown", "Unknown" ) );
            }

            // Add this check to handle if the gender requirement became required after an "Unknown" value was already set
            if ( rblGender.Items.FindByValue( person.Gender.ConvertToString() ) != null )
            {
                rblGender.SelectedValue = person.Gender.ConvertToString();
            }

            if ( group.Members.Where( gm => gm.PersonId == person.Id && gm.GroupRole.Guid == childGuid ).Any() )
            {
                _isEditRecordAdult = false;
                tbEmail.Required = false;

                // don't display campus selector to children. Rated PG.
                cpCampus.Visible = false;

                if ( person.GraduationYear.HasValue )
                {
                    ypGraduation.SelectedYear = person.GraduationYear.Value;
                }
                else
                {
                    ypGraduation.SelectedYear = null;
                }

                ddlGradePicker.Visible = true;
                if ( !person.HasGraduated ?? false )
                {
                    int gradeOffset = person.GradeOffset.Value;
                    var maxGradeOffset = ddlGradePicker.MaxGradeOffset;

                    // keep trying until we find a Grade that has a gradeOffset that includes the Person's gradeOffset (for example, there might be combined grades)
                    while ( !ddlGradePicker.Items.OfType<ListItem>().Any( a => a.Value.AsInteger() == gradeOffset ) && gradeOffset <= maxGradeOffset )
                    {
                        gradeOffset++;
                    }

                    ddlGradePicker.SetValue( gradeOffset );
                }
                else
                {
                    ddlGradePicker.SelectedIndex = 0;
                }
            }
            else
            {
                _isEditRecordAdult = true;
                bool requireEmail = GetAttributeValue( AttributeKey.RequireAdultEmailAddress ).AsBoolean();
                tbEmail.Required = requireEmail;
                ddlGradePicker.Visible = false;

                // show/hide campus selector
                bool showCampus = GetAttributeValue( AttributeKey.ShowCampusSelector ).AsBoolean();
                cpCampus.Visible = showCampus;
                if ( showCampus )
                {
                    cpCampus.Campuses = CampusCache.All( false );

                    // Use the current person's campus if this a new person
                    if ( personGuid == Guid.Empty )
                    {
                        cpCampus.SetValue( CurrentPerson.PrimaryCampus );
                    }
                    else
                    {
                        cpCampus.SetValue( person.GetCampus() );
                    }
                }
            }

            tbEmail.Text = person.Email;
            rblEmailPreference.SelectedValue = person.EmailPreference.ConvertToString( false );

            rblCommunicationPreference.Visible = this.GetAttributeValue( AttributeKey.ShowCommunicationPreference ).AsBoolean();
            rblCommunicationPreference.SetValue( person.CommunicationPreference == CommunicationType.SMS ? "2" : "1" );

            // Person Attributes
            var personAttributeAdultGuidList = GetAttributeValue( AttributeKey.PersonAttributesAdults ).SplitDelimitedValues().AsGuidList();
            var personAttributeChildGuidList = GetAttributeValue( AttributeKey.PersonAttributesChildren ).SplitDelimitedValues().AsGuidList();

            avcPersonAttributesAdult.IncludedAttributes = personAttributeAdultGuidList.Select( a => AttributeCache.Get( a ) ).ToArray();
            avcPersonAttributesChild.IncludedAttributes = personAttributeChildGuidList.Select( a => AttributeCache.Get( a ) ).ToArray();

            avcPersonAttributesAdult.AddEditControls( person, true );
            avcPersonAttributesChild.AddEditControls( person, true );

            avcPersonAttributesAdult.Visible = false;
            avcPersonAttributesChild.Visible = false;
            pnlPersonAttributes.Visible = false;

            if ( personGuid != Guid.Empty )
            {
                if ( person.AgeClassification != AgeClassification.Child && personAttributeAdultGuidList.Any() )
                {
                    avcPersonAttributesAdult.Visible = true;
                    pnlPersonAttributes.Visible = true;
                }
                else if ( person.AgeClassification == AgeClassification.Child && personAttributeChildGuidList.Any() )
                {
                    avcPersonAttributesChild.Visible = true;
                    pnlPersonAttributes.Visible = true;
                }
            }

            // Family Attributes
            if ( person.Id == CurrentPerson.Id )
            {
                List<Guid> familyAttributeGuidList = GetAttributeValue( AttributeKey.FamilyAttributes ).SplitDelimitedValues().AsGuidList();
                if ( familyAttributeGuidList.Any() )
                {
                    pnlFamilyAttributes.Visible = true;
                    avcFamilyAttributes.IncludedAttributes = familyAttributeGuidList.Select( a => AttributeCache.Get( a ) ).ToArray();
                    avcFamilyAttributes.AddEditControls( group, true );
                }
                else
                {
                    pnlFamilyAttributes.Visible = false;
                }

                lPreviousAddress.Text = string.Empty;
                acAddress.Required = false;

                Guid? locationTypeGuid = GetAttributeValue( AttributeKey.AddressTypeValueGuid ).AsGuidOrNull();
                if ( locationTypeGuid.HasValue )
                {
                    pnlAddress.Visible = GetAttributeValue( AttributeKey.ShowAddresses ).AsBoolean();

                    var addressTypeDv = DefinedValueCache.Get( locationTypeGuid.Value );

                    // if address type is home enable the move and is mailing/physical
                    if ( addressTypeDv.Guid == Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )
                    {
                        lbMoved.Visible = true;
                        cbIsMailingAddress.Visible = true;
                        cbIsPhysicalAddress.Visible = true;
                    }
                    else
                    {
                        lbMoved.Visible = false;
                        cbIsMailingAddress.Visible = false;
                        cbIsPhysicalAddress.Visible = false;
                    }

                    lAddressTitle.Text = addressTypeDv.Value + " Address";

                    var familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuidOrNull();

                    if ( familyGroupTypeGuid.HasValue )
                    {
                        var familyGroupType = GroupTypeCache.Get( familyGroupTypeGuid.Value );

                        var familyAddress = new GroupLocationService( rockContext ).Queryable()
                                            .Where( l => l.GroupId == groupId.Value
                                                    && l.GroupLocationTypeValueId == addressTypeDv.Id
                                                    && l.Group.Members.Any( m => m.PersonId == person.Id ) )
                                            .FirstOrDefault();
                        if ( familyAddress != null )
                        {
                            acAddress.SetValues( familyAddress.Location );

                            cbIsMailingAddress.Checked = familyAddress.IsMailingLocation;
                            cbIsPhysicalAddress.Checked = familyAddress.IsMappedLocation;
                        }
                    }
                }
            }
            else
            {
                pnlFamilyAttributes.Visible = false;
                pnlAddress.Visible = false;
            }

            BindPhoneNumbers( person );

            pnlView.Visible = false;
            pnlEdit.Visible = true;
        }

        /// <summary>
        /// Binds the phone numbers.
        /// </summary>
        /// <param name="person">The person.</param>
        private void BindPhoneNumbers( Person person )
        {
            if ( person == null )
            {
                person = new Person();
            }

            bool showPhoneNumbers = GetAttributeValue( AttributeKey.ShowPhoneNumbers ).AsBoolean();
            pnlPhoneNumbers.Visible = showPhoneNumbers;
            if ( !showPhoneNumbers )
            {
                return;
            }

            var phoneNumbers = new List<PhoneNumber>();
            var phoneNumberTypes = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE ) );
            var mobilePhoneType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );
            var selectedPhoneTypeGuids = GetAttributeValue( AttributeKey.PhoneTypeValueGuids ).Split( ',' ).AsGuidList();

            if ( phoneNumberTypes.DefinedValues.Where( pnt => selectedPhoneTypeGuids.Contains( pnt.Guid ) ).Any() )
            {
                foreach ( var phoneNumberType in phoneNumberTypes.DefinedValues.Where( pnt => selectedPhoneTypeGuids.Contains( pnt.Guid ) ) )
                {
                    var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneNumberType.Id );
                    if ( phoneNumber == null )
                    {
                        var numberType = new DefinedValue();
                        numberType.Id = phoneNumberType.Id;
                        numberType.Value = phoneNumberType.Value;
                        numberType.Guid = phoneNumberType.Guid;

                        phoneNumber = new PhoneNumber { NumberTypeValueId = numberType.Id, NumberTypeValue = numberType };
                        phoneNumber.IsMessagingEnabled = mobilePhoneType != null && phoneNumberType.Id == mobilePhoneType.Id;
                    }
                    else
                    {
                        // Update number format, just in case it wasn't saved correctly
                        phoneNumber.NumberFormatted = PhoneNumber.FormattedNumber( phoneNumber.CountryCode, phoneNumber.Number );
                    }

                    phoneNumbers.Add( phoneNumber );
                }

                rContactInfo.DataSource = phoneNumbers;
                rContactInfo.DataBind();
            }
        }

        /// <summary>
        /// Gets the person attribute Guids.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        private List<Guid> GetPersonAttributeGuids( int personId )
        {
            GroupMemberService groupMemberService = new GroupMemberService( new RockContext() );
            List<Guid> attributeGuidList = new List<Guid>();
            var adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            var childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
            var groupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

            if ( groupMemberService.Queryable().Where( gm =>
               gm.PersonId == personId &&
               gm.Group.GroupType.Guid == groupTypeGuid &&
               gm.GroupRole.Guid == adultGuid ).Any() )
            {
                attributeGuidList = GetAttributeValue( AttributeKey.PersonAttributesAdults ).SplitDelimitedValues().AsGuidList();
            }
            else
            {
                attributeGuidList = GetAttributeValue( AttributeKey.PersonAttributesChildren ).SplitDelimitedValues().AsGuidList();
            }

            return attributeGuidList;
        }

        /// <summary>
        /// Sets the visibility of the controls for role.
        /// </summary>
        /// <param name="roleTypeId">The role type identifier.</param>
        public void SetControlsForRoleType( int roleTypeId )
        {
            var adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            var groupTypeFamily = GroupTypeCache.GetFamilyGroupType();

            if ( groupTypeFamily.Roles.Where( gr =>
                           gr.Guid == adultGuid &&
                           gr.Id == roleTypeId ).Any() )
            {
                ddlGradePicker.Visible = false;
                tbEmail.Required = GetAttributeValue( AttributeKey.RequireAdultEmailAddress ).AsBoolean();
                _isEditRecordAdult = true;
            }
            else
            {
                ddlGradePicker.Visible = true;
                tbEmail.Required = false;
                _isEditRecordAdult = false;
            }

            avcPersonAttributesAdult.Visible = _isEditRecordAdult;
            avcPersonAttributesChild.Visible = !_isEditRecordAdult;

            if ( _isEditRecordAdult )
            {
                var attributeGuidListAdult = GetAttributeValue( AttributeKey.PersonAttributesAdults ).SplitDelimitedValues().AsGuidList();
                pnlPersonAttributes.Visible = attributeGuidListAdult.Any();
            }
            else
            {
                var attributeGuidListChild = GetAttributeValue( AttributeKey.PersonAttributesChildren ).SplitDelimitedValues().AsGuidList();
                pnlPersonAttributes.Visible = attributeGuidListChild.Any();
            }

            // Go through the ContactInfo items, and set or reset the control features per the selected Role.
            foreach ( RepeaterItem ri in rContactInfo.Items )
            {
                SetContactInfo( ri );
            }
        }

        private void SetContactInfo( RepeaterItem ri )
        {
            var pnbPhone = ri.FindControl( "pnbPhone" ) as PhoneNumberBox;
            HiddenField hfPhoneType = ri.FindControl( "hfPhoneType" ) as HiddenField;
            int phoneTypeId = hfPhoneType.Value.AsInteger();

            var phoneNumberTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() );
            var phoneNumberDefinedType = phoneNumberTypes.DefinedValues.Where( re => re.Id == phoneTypeId ).FirstOrDefault();

            if ( pnbPhone != null )
            {
                pnbPhone.ValidationGroup = BlockValidationGroup;

                var phoneNumber = new PhoneNumber { NumberTypeValueId = phoneTypeId };
                HtmlGenericControl phoneNumberContainer = ( HtmlGenericControl ) ri.FindControl( "divPhoneNumberContainer" );

                if ( _isEditRecordAdult && ( phoneNumber != null ) )
                {
                    pnbPhone.Required = _requiredPhoneNumberGuids.Contains( phoneNumberDefinedType.Guid );
                    if ( pnbPhone.Required )
                    {
                        pnbPhone.RequiredErrorMessage = string.Format( "{0} phone is required", phoneNumberDefinedType.Value );
                        phoneNumberContainer.AddCssClass( "required" );
                    }
                }

                // If not an adult record (child) and the phoneNumber has a value, remove the required status.
                if ( !_isEditRecordAdult && ( phoneNumber != null ) )
                {
                    pnbPhone.Required = false;
                    pnbPhone.RequiredErrorMessage = string.Empty;
                    phoneNumberContainer.RemoveCssClass( "required" );
                }
            }
        }

        #endregion
    }
}