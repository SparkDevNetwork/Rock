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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Group.GroupRegistration;
using Rock.ViewModels.Controls;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays the details of a particular group.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Group Registration" )]
    [Category( "Group" )]
    [Description( "Allows a person to register for a group." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [GroupTypesField( "Allowed Group Types",
        Key = AttributeKey.AllowedGroupTypes,
        Description = "This setting restricts which types of groups a person can be added to, however selecting a specific group via the Group setting will override this restriction.",
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP,
        Category = "",
        Order = 0 )]

    [GroupField( "Group",
        Key = AttributeKey.Group,
        Description = "Optional group to add person to. If omitted, the group's Guid should be passed via the Query string (GroupGuid=).",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 0 )]

    [BooleanField( "Enable Passing Group Id",
        Key = AttributeKey.EnablePassingGroupId,
        Description = "If enabled, allows the ability to pass in a group's Id (GroupId=) instead of the Guid.",
        DefaultBooleanValue = true,
        Category = "",
        Order = 0 )]

    [CustomRadioListField( "Mode",
        Key = AttributeKey.Mode,
        Description = "The mode to use when displaying registration details.",
        ListSource = "Simple^Simple,Full^Full,FullSpouse^Full With Spouse",
        IsRequired = true,
        DefaultValue = "Simple",
        Category = "",
        Order = 1 )]

    [CustomRadioListField( "Group Member Status",
        Key = AttributeKey.GroupMemberStatus,
        Description = "The group member status to use when adding person to group (default: 'Pending'.)",
        ListSource = "2^Pending,1^Active,0^Inactive",
        IsRequired = true,
        DefaultValue = "2",
        Category = "",
        Order = 2 )]

    [DefinedValueField( "Connection Status",
        Key = AttributeKey.ConnectionStatus,
        Description = "The connection status to use for new individuals (default: 'Prospect'.)",
        IsRequired = true,
        AllowMultiple = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        DefaultValue =  "368DD475-242C-49C4-A42C-7278BE690CC2",
        Category = "",
        Order = 3 )]

    [DefinedValueField( "Record Status",
        Key = AttributeKey.RecordStatus,
        Description = "The record status to use for new individuals (default: 'Pending'.)",
        IsRequired = true,
        AllowMultiple = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        DefaultValue = "283999EC-7346-42E3-B807-BCE9B2BABB49",
        Category = "",
        Order = 4 )]

    [WorkflowTypeField( "Workflow",
        Key = AttributeKey.Workflow,
        Description = "An optional workflow to start when registration is created. The GroupMember will set as the workflow 'Entity' when processing is started.",
        AllowMultiple = false,
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 5 )]

    [CodeEditorField( "Lava Template",
        Key = AttributeKey.LavaTemplate,
        Description = "The lava template to use to format the group details.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"
<div class='alert alert-info'>
    Please complete the form below to register for {{ Group.Name }}. 
</div>",
        Category = "",
        Order = 7 )]

    [LinkedPage( "Result Page",
        Key = AttributeKey.ResultPage,
        Description = "An optional page to redirect user to after they have been registered for the group.",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 8 )]

    [CodeEditorField( "Result Lava Template",
        Key = AttributeKey.ResultLavaTemplate,
        Description = "The lava template to use to format result message after user has been registered. Will only display if user is not redirected to a Result Page ( previous setting ).",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"
<div class='alert alert-success'>
    You have been registered for {{ Group.Name }}. You should be hearing from the leader soon.
</div>",
        Category = "",
        Order = 9 )]

    [CustomRadioListField( "Auto Fill Form",
        Key = AttributeKey.AutoFillForm,
        Description = "If set to FALSE then the form will not load the context of the logged in user (default: 'True'.)",
        ListSource = "true^True,false^False",
        IsRequired = true,
        DefaultValue = "true",
        Category = "",
        Order = 10 )]

    [TextField( "Register Button Alt Text",
        Key = AttributeKey.RegisterButtonAltText,
        Description = "Alternate text to use for the Register button (default is 'Register').",
        IsRequired = false,
        DefaultValue = "Register",
        Category = "",
        Order = 11 )]

    [BooleanField( "Prevent Overcapacity Registrations",
        Key = AttributeKey.PreventOvercapacityRegistrations,
        Description = "When set to true, user cannot register for groups that are at capacity or whose default GroupTypeRole are at capacity. If only one spot is available, no spouses can be registered.",
        DefaultBooleanValue = true,
        Category = "",
        Order = 12 )]

    [BooleanField( "Require Email",
        Key = AttributeKey.RequireEmail,
        Description = "Should email be required for registration?",
        DefaultBooleanValue = true,
        Order = 13 )]

    [BooleanField( "Require Mobile Phone",
        Key = AttributeKey.RequireMobilePhone,
        Description = "Should mobile phone numbers be required (when visible) for registration?  NOTE: Certain fields such as phone numbers and address are not shown when the block is configured for 'Simple' mode.",
        DefaultBooleanValue = false,
        Order = 14 )]

    [CustomDropdownListField(
        "Show SMS Opt-in",
        Key = AttributeKey.DisplaySmsOptIn,
        Description = "If selected this option will show the SMS Opt-In text and checkbox for the selected persons.",
        ListSource = "Hide,First Adult,All Adults",
        IsRequired = true,
        DefaultValue = "Hide",
        Order = 15 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "bbce9c47-b14d-4122-86a0-08441dee2759" )]
    [Rock.SystemGuid.BlockTypeGuid( "5e000376-ff90-4962-a053-ec1473da5c45" )]
    public class GroupRegistration : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupGuid = "GroupGuid";
        }

        private static class AttributeKey
        {
            public const string AllowedGroupTypes = "AllowedGroupTypes";
            public const string Group = "Group";
            public const string EnablePassingGroupId = "EnablePassingGroupId";
            public const string Mode = "Mode";
            public const string GroupMemberStatus = "GroupMemberStatus";
            public const string ConnectionStatus = "ConnectionStatus";
            public const string RecordStatus = "RecordStatus";
            public const string Workflow = "Workflow";
            public const string LavaTemplate = "LavaTemplate";
            public const string ResultPage = "ResultPage";
            public const string ResultLavaTemplate = "ResultLavaTemplate";
            public const string AutoFillForm = "AutoFillForm";
            public const string RegisterButtonAltText = "RegisterButtonAltText";
            public const string PreventOvercapacityRegistrations = "PreventOvercapacityRegistrations";
            public const string RequireEmail = "RequireEmail";
            public const string RequireMobilePhone = "RequireMobilePhone";
            public const string DisplaySmsOptIn = "DisplaySmsOptIn";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new GroupRegistrationBlockBox();

                SetBoxInitialEntityState( box, rockContext );
                GetSettings( rockContext, box );

                return box;
            }
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( GroupRegistrationBlockBox box, RockContext rockContext )
        {
            box.Entity = new GroupRegistrationBag();

            Guid? groupGuid = GetAttributeValue( AttributeKey.Group ).AsGuidOrNull();

            var group = GetGroup( rockContext );

            bool isFromBlockAttribute = groupGuid.HasValue && group.Guid == groupGuid.Value;

            if ( group == null )
            {
                box.ErrorMessage = "This page requires a valid group identifying parameter and there was not one provided.";
            }
            else
            {
                var groupTypeGuids = this.GetAttributeValue( AttributeKey.AllowedGroupTypes ).SplitDelimitedValues().AsGuidList();

                if ( !isFromBlockAttribute && groupTypeGuids.Any() && !groupTypeGuids.Contains( group.GroupType.Guid ) )
                {
                    box.ErrorMessage = "The selected group is a restricted group type therefore this block cannot be used to add people to these groups (unless configured to allow).";
                }
            }

            var connectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
            if ( connectionStatus == null )
            {
                box.ErrorMessage = "The selected Connection Status setting does not exist.";
            }

            var recordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );
            if ( recordStatus == null )
            {
                box.ErrorMessage = "The selected Record Status setting does not exist.";
            }

            var married = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() );
            var homeAddressType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
            var familyType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            var adultRole = familyType?.Roles?.FirstOrDefault( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) );

            if ( married == null || homeAddressType == null || familyType == null || adultRole == null )
            {
                box.ErrorMessage = "There is a missing or invalid system value. Check the settings for Marital Status of 'Married', Location Type of 'Home', Group Type of 'Family', and Family Group Role of 'Adult'.";
            }
        }

        private Rock.Model.Group GetGroup( RockContext rockContext )
        {
            Rock.Model.Group group = null;

            var groupService = new GroupService( rockContext );
            Guid? groupGuid = GetAttributeValue( AttributeKey.Group ).AsGuidOrNull();

            if ( groupGuid.HasValue )
            {
                group = groupService.Get( groupGuid.Value );
            }

            if ( group == null )
            {
                groupGuid = PageParameter( PageParameterKey.GroupGuid ).AsGuidOrNull();
                if ( groupGuid.HasValue )
                {
                    group = groupService.Get( groupGuid.Value );
                }
            }

            if ( group == null && GetAttributeValue( AttributeKey.EnablePassingGroupId ).AsBoolean( false ) )
            {
                int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
                if ( groupId.HasValue )
                {
                    group = groupService.Get( groupId.Value );
                }
            }

            return group;
        }

        private void GetSettings( RockContext rockContext, GroupRegistrationBlockBox box )
        {
            rockContext = rockContext ?? new RockContext();
            var group = GetGroup( rockContext );
            var currentPerson = RequestContext.CurrentPerson;

            if ( group != null )
            {
                // Show lava content
                var mergeFields = new Dictionary<string, object>
                {
                    { "Group", group }
                };

                var template = GetAttributeValue( AttributeKey.LavaTemplate );
                box.LavaOverview = template.ResolveMergeFields( mergeFields );

                box.IsEmailRequired = GetAttributeValue( AttributeKey.RequireEmail ).AsBoolean();
                box.IsMobilePhoneRequired = GetAttributeValue( AttributeKey.RequireMobilePhone ).AsBoolean();
                box.Mode = GetAttributeValue(AttributeKey.Mode);
                box.AutoFill = GetAttributeValue(AttributeKey.AutoFillForm).AsBoolean();
                box.RegisterButtonAltText = GetAttributeValue(AttributeKey.RegisterButtonAltText);

                var phoneLabel = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Value;
                phoneLabel = phoneLabel.Trim().EndsWith( "Phone" ) ? phoneLabel : phoneLabel + " Phone";
                box.PhoneLabel = phoneLabel;

                SetSmsOptInSettings( box );

                if ( currentPerson != null && box.AutoFill )
                {
                    var personService = new PersonService( rockContext );
                    Person person = personService
                        .Queryable( "PhoneNumbers.NumberTypeValue" ).AsNoTracking()
                        .FirstOrDefault( p => p.Id == currentPerson.Id );

                    box.Entity.FirstName = currentPerson.FirstName;
                    box.Entity.LastName = currentPerson.LastName;
                    box.Entity.Email = currentPerson.Email;

                    if ( box.Mode != "Simple" )
                    {
                        Guid homePhoneType = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid();
                        var homePhone = person.PhoneNumbers
                            .FirstOrDefault( n => n.NumberTypeValue.Guid.Equals( homePhoneType ) );
                        if ( homePhone != null )
                        {
                            box.Entity.HomePhone = homePhone.Number;
                        }

                        Guid cellPhoneType = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid();
                        var cellPhone = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValue.Guid.Equals( cellPhoneType ) );
                        if ( cellPhone != null )
                        {
                            box.Entity.MobilePhone = cellPhone.Number;
                            box.Entity.IsMessagingEnabled = cellPhone.IsMessagingEnabled;
                        }

                        var homeAddress = person.GetHomeLocation();
                        if ( homeAddress != null )
                        {
                            box.Entity.Address = new AddressControlBag
                            {
                                Street1 = homeAddress.Street1,
                                Street2 = homeAddress.Street2,
                                City = homeAddress.City,
                                State = homeAddress.State,
                                PostalCode = homeAddress.PostalCode,
                                Country = homeAddress.Country
                            };
                        }

                        if ( box.Mode == "FullSpouse" )
                        {
                            var spouse = person.GetSpouse( rockContext );
                            if ( spouse != null )
                            {
                                box.Entity.SpouseFirstName = spouse.FirstName;
                                box.Entity.SpouseLastName = spouse.LastName;
                                box.Entity.SpouseEmail = spouse.Email;

                                var spouseCellPhone = spouse.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValue.Guid.Equals( cellPhoneType ) );
                                if ( spouseCellPhone != null )
                                {
                                    box.Entity.SpouseMobilePhone = spouseCellPhone.Number;
                                    box.Entity.SpouseIsMessagingEnabled = spouseCellPhone.IsMessagingEnabled;
                                }
                            }
                        }
                    }
                }

                if ( GetAttributeValue( AttributeKey.PreventOvercapacityRegistrations ).AsBoolean() )
                {
                    int openGroupSpots = 2;
                    int openRoleSpots = 2;
                    var defaultGroupRole = group.GroupType.DefaultGroupRole;

                    // If the group has a GroupCapacity, check how far we are from hitting that.
                    if ( group.GroupCapacity.HasValue )
                    {
                        openGroupSpots = group.GroupCapacity.Value - group.ActiveMembers().Count();
                    }

                    // When someone registers for a group on the front-end website, they automatically get added with the group's default
                    // GroupTypeRole. If that role exists and has a MaxCount, check how far we are from hitting that.
                    if ( defaultGroupRole != null && defaultGroupRole.MaxCount.HasValue )
                    {
                        openRoleSpots = defaultGroupRole.MaxCount.Value - group.Members
                            .Where( m => m.GroupRoleId == defaultGroupRole.Id && m.GroupMemberStatus == GroupMemberStatus.Active )
                            .Count();
                    }

                    // Between the group's GroupCapacity and DefaultGroupRole.MaxCount, grab the one we're closest to hitting, and how close we are to
                    // hitting it.
                    box.OpenSpots = Math.Min( openGroupSpots, openRoleSpots );

                    // If no spots are open, display a message that says so.
                    if ( box.OpenSpots <= 0 )
                    {
                        box.ErrorMessage = "This group is at or exceeds capacity.";
                    }
                }
            }
        }

        private void SetSmsOptInSettings( GroupRegistrationBlockBox box )
        {
            var displaySmsAttributeValue = this.GetAttributeValue( AttributeKey.DisplaySmsOptIn );
            var smsOptInDisplayText = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.SMS_OPT_IN_MESSAGE_LABEL );

            //Options for displaying the SMS Opt-In checkbox: Hide,First Adult,All Adults
            switch ( displaySmsAttributeValue )
            {
                case "Hide":
                    box.SmsIsHidden = true;
                    box.SmsIsShowFirstAdult = false;
                    box.SmsIsShowAllAdults = false;
                    break;

                case "First Adult":
                    box.SmsOptInDisplayText = smsOptInDisplayText;
                    box.SmsIsHidden = false;
                    box.SmsIsShowFirstAdult = true;
                    box.SmsIsShowAllAdults = false;
                    break;

                case "All Adults":
                    box.SmsOptInDisplayText = smsOptInDisplayText;
                    box.SmsIsHidden = false;
                    box.SmsIsShowFirstAdult = true;
                    box.SmsIsShowAllAdults = true;
                    break;

                default:
                    box.SmsIsHidden = true;
                    box.SmsIsShowFirstAdult = false;
                    box.SmsIsShowAllAdults = false;
                    break;
            }
        }

        /// <summary>
        /// Sets the phone number.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="pnbNumber">The PNB number.</param>
        /// <param name="enableSms">The cb SMS.</param>
        /// <param name="phoneTypeGuid">The phone type unique identifier.</param>
        private void SetPhoneNumber( RockContext rockContext, Person person, string pnbNumber, bool enableSms, Guid phoneTypeGuid )
        {
            var phoneType = DefinedValueCache.Get( phoneTypeGuid );
            if ( phoneType == null )
            {
                return;
            }

            var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneType.Id ) ?? new PhoneNumber { NumberTypeValueId = phoneType.Id };
            phoneNumber.CountryCode = PhoneNumber.CleanNumber( Rock.Model.PhoneNumber.DefaultCountryCode() );
            phoneNumber.Number = PhoneNumber.CleanNumber( pnbNumber );

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
                if ( enableSms )
                {
                    phoneNumber.IsMessagingEnabled = true;
                    person.PhoneNumbers
                        .Where( n => n.NumberTypeValueId != phoneType.Id )
                        .ToList()
                        .ForEach( n => n.IsMessagingEnabled = false );
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
        private bool AddPersonToGroup( RockContext rockContext, Person person, WorkflowTypeCache workflowType, List<GroupMember> groupMembers, out string errorMessage )
        {
            var group = GetGroup( rockContext );
            var defaultGroupRole = group.GroupType.DefaultGroupRole;
            errorMessage = string.Empty;

            if ( person == null )
            {
                errorMessage = "Not able to find the correct person.";
                return false;
            }

            GroupMember groupMember = null;
            if ( !group.Members
                .Any( m =>
                    m.PersonId == person.Id &&
                    m.GroupRoleId == defaultGroupRole.Id ) )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                groupMember = new GroupMember();
                groupMember.PersonId = person.Id;
                groupMember.GroupRoleId = defaultGroupRole.Id;
                groupMember.GroupMemberStatus = (GroupMemberStatus)GetAttributeValue( "GroupMemberStatus" ).AsInteger();
                groupMember.GroupId = group.Id;
                if ( groupMember.IsValidGroupMember( rockContext ) )
                {
                    groupMemberService.Add( groupMember );
                    rockContext.SaveChanges();
                    groupMembers.Add( groupMember );
                }
                else
                {
                    errorMessage = groupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                    return false;
                }
            }
            else
            {
                GroupMemberStatus status = (GroupMemberStatus)GetAttributeValue( "GroupMemberStatus" ).AsInteger();
                groupMember = group.Members.Where( m =>
                    m.PersonId == person.Id &&
                    m.GroupRoleId == defaultGroupRole.Id ).FirstOrDefault();
                if ( groupMember.GroupMemberStatus != status )
                {
                    var groupMemberService = new GroupMemberService( rockContext );

                    // reload this group member in the current context
                    groupMember = groupMemberService.Get( groupMember.Id );
                    groupMember.GroupMemberStatus = status;
                    if ( groupMember.IsValidGroupMember( rockContext ) )
                    {
                        rockContext.SaveChanges();
                    }
                    else
                    {
                        errorMessage = groupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                        return false;
                    }
                }
            }

            if ( groupMember != null && workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                try
                {
                    var workflow = Rock.Model.Workflow.Activate( workflowType, person.FullName );
                    new WorkflowService( rockContext ).Process( workflow, groupMember, out List<string> workflowErrors );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            }

            return true;
        }

        public string LinkedPageUrl( string attributeKey, Dictionary<string, string> queryParams = null )
        {
            var pageReference = new PageReference( GetAttributeValue( attributeKey ), queryParams );
            if ( pageReference.PageId > 0 )
            {
                return pageReference.BuildUrl();
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="groupRegistrationBag">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( GroupRegistrationBag groupRegistrationBag )
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );

                Person person = null;
                Person spouse = null;
                Rock.Model.Group family = null;
                GroupLocation homeLocation = null;

                var connectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
                var recordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );
                var homeAddressType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                var isSimple = GetAttributeValue( AttributeKey.Mode ) == "Simple";
                var isFullWithSpouse = GetAttributeValue( AttributeKey.Mode ) == "FullSpouse";

                var isCurrentPerson = RequestContext.CurrentPerson != null
                    && RequestContext.CurrentPerson.NickName.IsNotNullOrWhiteSpace()
                    && RequestContext.CurrentPerson.LastName.IsNotNullOrWhiteSpace()
                    && groupRegistrationBag.FirstName.Trim().Equals( RequestContext.CurrentPerson.NickName.Trim(), StringComparison.OrdinalIgnoreCase )
                    && groupRegistrationBag.LastName.Trim().Equals( RequestContext.CurrentPerson.LastName.Trim(), StringComparison.OrdinalIgnoreCase );

                // Only use current person if the name entered matches the current person's name and autofill mode is true
                if ( GetAttributeValue( AttributeKey.AutoFillForm ).AsBoolean() && isCurrentPerson )
                {
                    person = personService.Get( RequestContext.CurrentPerson.Id );
                }

                // Try to find person by name/email 
                if ( person == null )
                {
                    var personQuery = new PersonService.PersonMatchQuery( groupRegistrationBag.FirstName.Trim(), groupRegistrationBag.LastName.Trim(), groupRegistrationBag.Email.Trim(), groupRegistrationBag.MobilePhone.Trim() );
                    person = personService.FindPerson( personQuery, true );
                }

                // Check to see if this is a new person
                if ( person == null )
                {
                    var group = GetGroup( rockContext );
                    // If so, create the person and family record for the new person
                    person = new Person();
                    person.FirstName = groupRegistrationBag.FirstName.Trim();
                    person.LastName = groupRegistrationBag.LastName.Trim();
                    person.Email = groupRegistrationBag.Email.Trim();
                    person.IsEmailActive = true;
                    person.EmailPreference = EmailPreference.EmailAllowed;
                    person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    person.ConnectionStatusValueId = connectionStatus.Id;
                    person.RecordStatusValueId = recordStatus.Id;
                    person.Gender = Gender.Unknown;

                    family = PersonService.SaveNewPerson( person, rockContext, group.CampusId, false );
                }
                else
                {
                    // updating current existing person
                    person.Email = groupRegistrationBag.Email;

                    // Get the current person's families
                    var families = person.GetFamilies( rockContext );

                    // If address can being entered, look for first family with a home location
                    if ( !isSimple )
                    {
                        foreach ( var aFamily in families )
                        {
                            homeLocation = aFamily.GroupLocations
                                .Where( l =>
                                    l.GroupLocationTypeValueId == homeAddressType.Id &&
                                    l.IsMappedLocation )
                                .FirstOrDefault();
                            if ( homeLocation != null )
                            {
                                family = aFamily;
                                break;
                            }
                        }
                    }

                    // If a family wasn't found with a home location, use the person's first family
                    if ( family == null )
                    {
                        family = families.FirstOrDefault();
                    }
                }

                // If using a 'Full' view, save the phone numbers and address
                if ( !isSimple )
                {
                    if ( !string.IsNullOrWhiteSpace( groupRegistrationBag.HomePhone ) )
                    {
                        SetPhoneNumber( rockContext, person, groupRegistrationBag.HomePhone, false, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
                    }
                    if ( !string.IsNullOrWhiteSpace( groupRegistrationBag.MobilePhone ) )
                    {
                        SetPhoneNumber( rockContext, person, groupRegistrationBag.MobilePhone, groupRegistrationBag.IsMessagingEnabled, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                    }

                    if ( !string.IsNullOrWhiteSpace( groupRegistrationBag.Address?.Street1 ) )
                    {
                        var editedLocation = new Location()
                        {
                            Street1 = groupRegistrationBag.Address.Street1,
                            Street2 = groupRegistrationBag.Address.Street2,
                            City = groupRegistrationBag.Address.City,
                            State = groupRegistrationBag.Address.State,
                            PostalCode = groupRegistrationBag.Address.PostalCode,
                            Country = groupRegistrationBag.Address.Country,
                        };

                        if ( !LocationService.ValidateLocationAddressRequirements( editedLocation, out string validationMessage ) )
                        {
                            return ActionBadRequest( validationMessage );
                        }

                        var location = new LocationService( rockContext ).Get( groupRegistrationBag.Address.Street1, groupRegistrationBag.Address.Street2, groupRegistrationBag.Address.City, groupRegistrationBag.Address.State, groupRegistrationBag.Address.PostalCode, groupRegistrationBag.Address.Country );
                        if ( location != null )
                        {
                            if ( homeLocation == null )
                            {
                                homeLocation = new GroupLocation();
                                homeLocation.GroupLocationTypeValueId = homeAddressType.Id;
                                // If there are not any addresses with a Map Location, set the first home location to be a mapped location
                                homeLocation.IsMappedLocation = true;
                                family.GroupLocations.Add( homeLocation );
                            }

                            homeLocation.Location = location;
                        }
                    }

                    // Check for the spouse
                    if ( isFullWithSpouse && groupRegistrationBag.SpouseFirstName.IsNotNullOrWhiteSpace() && groupRegistrationBag.SpouseLastName.IsNotNullOrWhiteSpace() )
                    {
                        spouse = person.GetSpouse( rockContext );
                        bool isSpouseMatch = true;
                        var familyType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                        var married = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() );
                        var adultRole = familyType?.Roles?.FirstOrDefault( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) );

                        if ( spouse == null ||
                            !groupRegistrationBag.SpouseFirstName.Trim().Equals( spouse.FirstName.Trim(), StringComparison.OrdinalIgnoreCase ) ||
                            !groupRegistrationBag.SpouseLastName.Trim().Equals( spouse.LastName.Trim(), StringComparison.OrdinalIgnoreCase ) )
                        {
                            spouse = new Person();
                            isSpouseMatch = false;

                            spouse.FirstName = groupRegistrationBag.SpouseFirstName.FixCase();
                            spouse.LastName = groupRegistrationBag.SpouseLastName.FixCase();

                            spouse.ConnectionStatusValueId = connectionStatus.Id;
                            spouse.RecordStatusValueId = recordStatus.Id;
                            spouse.Gender = Gender.Unknown;

                            spouse.IsEmailActive = true;
                            spouse.EmailPreference = EmailPreference.EmailAllowed;

                            var groupMember = new GroupMember();
                            groupMember.GroupRoleId = adultRole.Id;
                            groupMember.Person = spouse;

                            family.Members.Add( groupMember );

                            spouse.MaritalStatusValueId = married.Id;
                            person.MaritalStatusValueId = married.Id;
                        }

                        spouse.Email = groupRegistrationBag.Email;

                        if ( !isSpouseMatch || !string.IsNullOrWhiteSpace( groupRegistrationBag.HomePhone ) )
                        {
                            SetPhoneNumber( rockContext, spouse, groupRegistrationBag.HomePhone, false, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
                        }

                        if ( !isSpouseMatch || !string.IsNullOrWhiteSpace( groupRegistrationBag.SpouseMobilePhone ) )
                        {
                        
                            SetPhoneNumber( rockContext, spouse, groupRegistrationBag.SpouseMobilePhone, groupRegistrationBag.SpouseIsMessagingEnabled, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                        }
                    }
                }

                string errorMessage = string.Empty;
                // Save the registrations ( and launch workflows )
                var newGroupMembers = new List<GroupMember>();
                // Save the person/spouse and change history 
                var isAddingPeopleToGroupSuccessful = rockContext.WrapTransactionIf( () =>
                {
                    rockContext.SaveChanges();

                    // Check to see if a workflow should be launched for each person
                    WorkflowTypeCache workflowType = null;
                    Guid? workflowTypeGuid = GetAttributeValue( "Workflow" ).AsGuidOrNull();
                    if ( workflowTypeGuid.HasValue )
                    {
                        workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );
                    }

                    bool isAddPersonValid = AddPersonToGroup( rockContext, person, workflowType, newGroupMembers, out errorMessage );
                    if ( !isAddPersonValid )
                    {
                        return false;
                    }

                    if ( spouse != null )
                    {
                        isAddPersonValid = AddPersonToGroup( rockContext, spouse, workflowType, newGroupMembers, out errorMessage );
                        if ( !isAddPersonValid )
                        {
                            return false;
                        }
                    }

                    return true;
                } );

                if ( isAddingPeopleToGroupSuccessful )
                {
                    var group = GetGroup( rockContext );

                    // Show lava content
                    var mergeFields = new Dictionary<string, object>();
                    mergeFields.Add( "Group", group );
                    mergeFields.Add( "GroupMembers", newGroupMembers );

                    string template = GetAttributeValue( "ResultLavaTemplate" );
                    var resultLavaTemplate = template.ResolveMergeFields( mergeFields );
                    var linkedPageUrl = this.GetLinkedPageUrl( AttributeKey.ResultPage, new Dictionary<string, string>
                    {
                        [PageParameterKey.GroupId] = family.IdKey
                    } );

                    // Will only redirect if a value is specified
                    if ( linkedPageUrl.IsNotNullOrWhiteSpace() )
                    {
                        return ActionContent( System.Net.HttpStatusCode.Created, linkedPageUrl );
                    }

                    return ActionOk( resultLavaTemplate );
                }
                else
                {
                    return ActionBadRequest( errorMessage );
                }
            }
        }

        #endregion
    }
}
