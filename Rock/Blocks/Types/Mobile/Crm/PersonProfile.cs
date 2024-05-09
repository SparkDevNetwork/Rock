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
using System.ComponentModel;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Crm.PersonProfile;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using System.Linq;
using Rock.Mobile;
using Rock.Data;
using System;
using System.Collections.Generic;
using Rock.Security;
using Rock.Common.Mobile.ViewModel;

namespace Rock.Blocks.Types.Mobile.Crm
{
    /// <summary>
    /// The Rock Mobile Person Profile block, used to display and edit
    /// a <see cref="Rock.Model.Person"/> in Rock.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    [DisplayName( "Person Profile" )]
    [Category( "Mobile > Crm" )]
    [Description( "The person profile block." )]
    [IconCssClass( "fa fa-user" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [DefinedValueField(
        "Phone Types",
        Key = AttributeKey.PhoneTypes,
        Description = "The phone numbers to display for editing.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE,
        IsRequired = false,
        AllowMultiple = true,
        Order = 0 )]

    [CodeEditorField( "Header Template",
        Description = "Lava template used to render the header of the block.",
        IsRequired = false,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKey.HeaderTemplate,
        DefaultValue = _defaultHeaderXaml,
        Order = 1 )]

    [CodeEditorField( "Custom Actions Template",
        Description = "Lava template used to render custom actions (such as navigation) below the contact buttons.",
        IsRequired = false,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKey.CustomActionsTemplate,
        Order = 2 )]

    [CodeEditorField( "Badge Bar Template",
        Description = "Lava template used to render custom XAML below the Custom Actions Template.",
        IsRequired = false,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKey.BadgeBarTemplate,
        Order = 3 )]

    [BooleanField( "Show Demographics Panel",
        Description = "When enabled, the demographics panel will be shown.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowDemographicsPanel,
        Order = 4
        )]

    [BooleanField( "Show Contact Information Panel",
        Description = "When enabled, the contact information panel will be shown.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowContactInformationPanel,
        Order = 5
        )]


    [LinkedPage(
        "Reminder Page",
        Description = "Page to link to when the reminder button is tapped.",
        IsRequired = false,
        Key = AttributeKey.ReminderPage,
        Order = 6 )]


    #endregion

    [ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CRM_PERSON_PROFILE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_CRM_PERSON_PROFILE )]
    public class PersonProfile : RockBlockType
    {
        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 5 );

        /// <inheritdoc />
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Crm.PersonProfile.Configuration
            {
                ConnectionStatusValues = GetConnectionStatusValues(),
                InactiveReasonValues = GetInactiveReasonValues(),
                RecordStatusValues = GetRecordStatusValues(),
                SuffixValues = GetSuffixValues(),
                GradeValues = GetGradeValues(),
                MaritalStatusValues = GetMaritalStatusValues(),
                ShowDemographicsPanel = GetAttributeValue( AttributeKey.ShowDemographicsPanel ).AsBoolean(),
                ShowContactInformationPanel = GetAttributeValue( AttributeKey.ShowContactInformationPanel ).AsBoolean(),
                ReminderPageGuid = GetAttributeValue( AttributeKey.ReminderPage ).AsGuidOrNull(),
                AreRemindersConfigured = CheckReminderConfiguration()
            };
        }

        #endregion

        #region Keys

        /// <summary>
        /// The attribute keys for this block.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The header template attribute key.
            /// </summary>
            public const string HeaderTemplate = "HeaderTemplate";

            /// <summary>
            /// The header template attribute key.
            /// </summary>
            public const string CustomActionsTemplate = "CustomActionsTemplate";

            /// <summary>
            /// The header template attribute key.
            /// </summary>
            public const string BadgeBarTemplate = "BadgeBarTemplate";

            /// <summary>
            /// The header template attribute key.
            /// </summary>
            public const string PhoneTypes = "PhoneTypes";

            /// <summary>
            /// The show demographics panel attribute key.
            /// </summary>
            public const string ShowDemographicsPanel = "ShowDemographicsPanel";

            /// <summary>
            /// The show contact information panel attribute key.
            /// </summary>
            public const string ShowContactInformationPanel = "ShowContactInformationPanel";

            /// <summary>
            /// The reminder page attribute key.
            /// </summary>
            public const string ReminderPage = "ReminderPage";
        }

        #endregion

        #region Constants

        private const string _defaultHeaderXaml = @"<StackLayout Spacing=""4"">
    
    {% if CanEdit %}
      <Button Text=""Edit""
        Command=""{Binding ShowEdit}""
        HorizontalOptions=""End""
        StyleClass=""btn, btn-link"" />
    {% endif %}
    
    <!-- The main layout of the block. -->
    <StackLayout HorizontalOptions=""Center""
        Spacing=""4"">
        <Rock:Image Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' | Append:Person.PhotoUrl | Escape }}"" 
            WidthRequest=""128"" 
            HeightRequest=""128"">
            <Rock:CircleTransformation />
        </Rock:Image>
        
        {% if Person.IsDeceased %}
            <Label Text=""Deceased""
                StyleClass=""text-danger""
                FontAttributes=""Bold""
                HorizontalTextAlignment=""Center"" />
        {% endif %}
        
        <Label Text=""{{ Person.FullName }}""
            StyleClass=""h2""
            HorizontalTextAlignment=""Center"" />
            
        <!-- Our campus, connection status and record status -->
        <StackLayout Orientation=""Horizontal""
            HorizontalOptions=""Center"">
            
            <!-- Campus -->
            <Frame Padding=""4""
                CornerRadius=""4""
                HasShadow=""False""
                BackgroundColor=""#d9f2fe"">
                <Label Text=""{{ Person | Campus | Property:'Name' }}""
                    TextColor=""#0079b0""
                    VerticalTextAlignment=""Center""
                    FontSize=""14"" />
            </Frame>
            
            <!-- Connection Status -->
            <Frame Padding=""4""
                CornerRadius=""4""
                HasShadow=""False""
                BackgroundColor=""#dcf6ed"">
                <Label Text=""{{ Person.ConnectionStatusValue.Value }}""
                    FontSize=""14""
                    TextColor=""#065f46"" />
            </Frame>
            
            <!-- Record Status -->
            {% assign recordStatus = Person.RecordStatusValue.Value %}
            {% if recordStatus == 'Inactive' %}
                <Frame Padding=""8""
                    CornerRadius=""4""
                    HasShadow=""False""
                    BackgroundColor=""#f9e5e2"">
                    <Label Text=""{{ Person.RecordStatusValue.Value }}""
                        TextColor=""#ac3523"" />
                </Frame>
            {% endif %}
            
        </StackLayout>
    </StackLayout>
</StackLayout>";

        #endregion

        #region Methods

        /// <summary>
        /// Checks if there's any reminder with the PersonAlias entity type.
        /// </summary>
        private static bool CheckReminderConfiguration()
        {
            using ( var rockContext = new RockContext() )
            {
                var personAliasEntityTypeId = EntityTypeCache.Get( typeof( PersonAlias ) ).Id;

                var reminderTypesExist = new ReminderTypeService( rockContext )
                    .Queryable()
                    .Where( rt => rt.EntityTypeId == personAliasEntityTypeId )
                    .Any();

                return reminderTypesExist;
            }
        }

        /// <summary>
        /// Gets the connection status defined values.
        /// </summary>
        /// <returns>List&lt;ListItemViewModel&gt;.</returns>
        private List<ListItemViewModel> GetConnectionStatusValues()
        {
            var connectionStatusType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS );

            return connectionStatusType.DefinedValues.Where( v => v.IsActive )
                .Select( v => new ListItemViewModel
                {
                    Text = v.Value,
                    Value = v.Guid.ToString()
                } ).ToList();
        }

        /// <summary>
        /// Gets the record status defined values.
        /// </summary>
        /// <returns>List&lt;ListItemViewModel&gt;.</returns>
        private List<ListItemViewModel> GetRecordStatusValues()
        {
            var recordStatusType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS );

            return recordStatusType.DefinedValues.Where( v => v.IsActive )
                .Select( v => new ListItemViewModel
                {
                    Text = v.Value,
                    Value = v.Guid.ToString()
                } ).ToList();
        }

        /// <summary>
        /// Gets the inactive reason defined values.
        /// </summary>
        /// <returns>List&lt;ListItemViewModel&gt;.</returns>
        private List<ListItemViewModel> GetInactiveReasonValues()
        {
            var inactiveReasonType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON );

            return inactiveReasonType.DefinedValues.Where( v => v.IsActive )
                .Select( v => new ListItemViewModel
                {
                    Text = v.Value,
                    Value = v.Guid.ToString()
                } ).ToList();
        }

        /// <summary>
        /// Gets the suffix defined values.
        /// </summary>
        /// <returns>List&lt;ListItemViewModel&gt;.</returns>
        private List<ListItemViewModel> GetSuffixValues()
        {
            var inactiveReasonType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_SUFFIX );

            return inactiveReasonType.DefinedValues.Where( v => v.IsActive )
                .Select( v => new ListItemViewModel
                {
                    Text = v.Value,
                    Value = v.Guid.ToString()
                } ).ToList();
        }

        /// <summary>
        /// Gets the marital status defined values.
        /// </summary>
        /// <returns>List&lt;ListItemViewModel&gt;.</returns>
        private List<ListItemViewModel> GetMaritalStatusValues()
        {
            var maritalStatusType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS );

            return maritalStatusType.DefinedValues.Where( v => v.IsActive )
                .Select( v => new ListItemViewModel
                {
                    Text = v.Value,
                    Value = v.Guid.ToString()
                } ).ToList();
        }

        /// <summary>
        /// Gets the grade defined values.
        /// </summary>
        /// <returns>List&lt;ListItemViewModel&gt;.</returns>
        private List<ListItemViewModel> GetGradeValues()
        {
            var gradeType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SCHOOL_GRADES );

            return gradeType.DefinedValues.Where( v => v.IsActive )
                .Select( v => new ListItemViewModel
                {
                    Text = v.Value,
                    Value = v.Guid.ToString()
                } ).ToList();
        }

        /// <summary>
        /// Gets the header template.
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetHeaderTemplate( Person person )
        {
            var template = GetAttributeValue( AttributeKey.HeaderTemplate );
            var mergeFields = RequestContext.GetCommonMergeFields();

            mergeFields.Add( "Person", person );
            mergeFields.Add( "CanEdit", IsAuthorizedToEditPerson() );

            template = template.ResolveMergeFields( mergeFields );

            return template;
        }

        /// <summary>
        /// Gets the header template.
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetBadgeBarTemplate( Person person )
        {
            var template = GetAttributeValue( AttributeKey.BadgeBarTemplate );
            var mergeFields = RequestContext.GetCommonMergeFields();

            mergeFields.Add( "Person", person );

            template = template.ResolveMergeFields( mergeFields );

            return template;
        }

        /// <summary>
        /// Gets the header template.
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetCustomActionsTemplate( Person person )
        {
            var template = GetAttributeValue( AttributeKey.CustomActionsTemplate );
            var mergeFields = RequestContext.GetCommonMergeFields();

            mergeFields.Add( "Person", person );

            template = template.ResolveMergeFields( mergeFields );

            return template;
        }

        /// <summary>
        /// Gets the contact information for the person.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        private ContactInformationBag GetPersonContactInformation( Person person )
        {
            // The list of allowed phone types we want to display.
            var phoneNumberTypeGuids = GetAttributeValue( AttributeKey.PhoneTypes )
                .Split( ',' )
                .Where( guidString => guidString.IsNotNullOrWhiteSpace() )
                .Select( Guid.Parse )
                .ToList();

            // Get the person data for those types.
            var phoneNumbers = phoneNumberTypeGuids.Select( guid => GetPersonPhoneNumberBag( person, guid ) )
                .ToList();

            var email = new EmailInfoBag
            {
                Email = person.Email,
                EmailPreference = person.EmailPreference.ToMobile(),
                IsActive = person.IsEmailActive
            };

            return new ContactInformationBag
            {
                PhoneNumbers = phoneNumbers,
                Email = email,
                CommunicationPreference = person.CommunicationPreference.ToMobile(),
            };
        }

        /// <summary>
        /// Gets the demographic information for the person.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        private DemographicInfoBag GetPersonDemographicInformation( Person person )
        {
            return new DemographicInfoBag
            {
                AnniversaryDate = person.AnniversaryDate,
                BirthDate = person.BirthDate,
                FormattedAge = person.FormatAge( true ),
                Gender = person.Gender.ToMobile(),
                GraduationYear = person.GraduationYear,
                HasGraduated = person.HasGraduated,
                IsDeceased = person.IsDeceased,
                MaritalStatus = person.MaritalStatusValue?.Value
            };
        }

        /// <summary>
        /// Converts a <see cref="PhoneNumber" /> into a mobile friendly phone number bag.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="phoneTypeGuid"></param>
        /// <returns></returns>
        private PhoneNumberBag GetPersonPhoneNumberBag( Person person, Guid phoneTypeGuid )
        {
            // Get the phone number for the person.
            var phoneNumber = person.GetPhoneNumber( phoneTypeGuid );
            var phoneType = DefinedValueCache.Get( phoneTypeGuid );

            // Invalid type and non-existing phone-number.
            if ( phoneNumber == null && phoneType == null )
            {
                return null;
            }

            // If we found a type but not a number, early out with just information about the type.
            if ( phoneNumber == null && phoneType != null )
            {
                return new PhoneNumberBag
                {
                    TypeGuid = phoneTypeGuid,
                    TypeValue = phoneType.Value
                };
            }

            return new PhoneNumberBag
            {
                PhoneNumber = phoneNumber.NumberFormatted,
                Description = phoneNumber.Description,
                Guid = phoneNumber.Guid,
                IsSmsEnabled = phoneNumber.IsMessagingEnabled,
                TypeValue = phoneType.Value,
                IsUnlisted = phoneNumber.IsUnlisted,
                CountryCode = phoneNumber.CountryCode,
                TypeGuid = phoneTypeGuid
            };
        }

        /// <summary>
        /// Creates or updates a phone number for a person.
        /// </summary>
        private static int? CreateOrUpdatePersonPhoneNumber( Person person, PhoneNumberBag phoneNumberBag, RockContext rockContext )
        {
            var phoneNumberService = new PhoneNumberService( rockContext );
            var phoneNumberType = DefinedValueCache.Get( phoneNumberBag.TypeGuid );

            PhoneNumber phoneNumber;

            if ( phoneNumberType == null )
            {
                return null;
            }

            // If this is a new phone number.
            if ( phoneNumberBag.Guid == null )
            {
                phoneNumber = new PhoneNumber
                {
                    NumberTypeValueId = phoneNumberType.Id,
                };

                person.PhoneNumbers.Add( phoneNumber );
            }
            // This phone number already exists.
            else
            {
                phoneNumber = phoneNumberService.Get( phoneNumberBag.Guid.Value );
            }

            phoneNumber.CountryCode = PhoneNumber.CleanNumber( phoneNumberBag.CountryCode );
            phoneNumber.Number = PhoneNumber.CleanNumber( phoneNumberBag.PhoneNumber );

            if ( string.IsNullOrWhiteSpace( phoneNumber.Number ) )
            {
                person.PhoneNumbers.Remove( phoneNumber );
                phoneNumberService.Delete( phoneNumber );
            }

            phoneNumber.IsUnlisted = phoneNumberBag.IsUnlisted;
            phoneNumber.IsMessagingEnabled = phoneNumberBag.IsSmsEnabled;

            rockContext.SaveChanges();
            return phoneNumber.Id;
        }

        /// <summary>
        /// Updates the person email.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="emailInfoBag">The email information bag.</param>
        /// <remarks>You still need to call RockContext.SaveChanges().</remarks>
        private void UpdatePersonEmail( Person person, EmailInfoBag emailInfoBag )
        {
            person.Email = emailInfoBag.Email;
            person.EmailPreference = emailInfoBag.EmailPreference.ToNative();
            person.IsEmailActive = emailInfoBag.IsActive;
        }

        /// <summary>
        /// Determines whether the current person is authorized to edit another person.
        /// </summary>
        /// <returns><c>true</c> if is authorized to edit; otherwise, <c>false</c>.</returns>
        private bool IsAuthorizedToEditPerson()
        {
            if ( RequestContext.CurrentPerson != null )
            {
                var currentPerson = RequestContext.CurrentPerson;

                // The security on this block is to check whether or not the person
                // attempting to make the edit has permission to edit the block itself.
                if ( BlockCache.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether a Person is followed by the Current Person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <returns><c>true</c> if [is person followed] [the specified rock context]; otherwise, <c>false</c>.</returns>
        private bool IsPersonFollowed( RockContext rockContext, Person person )
        {
            var follower = RequestContext.CurrentPerson;

            if ( follower == null )
            {
                return false;
            }

            var followingService = new FollowingService( rockContext );
            var personAliasEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.PERSON_ALIAS );

            var followingList = followingService
                .Queryable()
                .Where( f => f.EntityTypeId == personAliasEntityTypeId && f.EntityId == person.PrimaryAliasId )
                .ToList();

            return followingList.Any( f => f.PersonAlias.PersonId == follower.Id );
        }

        /// <summary>
        /// Gets a mobile friendly person bag from a specified <see cref="Person" />.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>PersonBag.</returns>
        private PersonBag GetPersonBag( Person person, RockContext rockContext )
        {
            return new PersonBag
            {
                Guid = person.Guid,
                PrimaryAliasGuid = person.PrimaryAlias.Guid,
                CanEdit = IsAuthorizedToEditPerson(),
                IsFollowed = IsPersonFollowed( rockContext, person ),
                FirstName = person.FirstName,
                NickName = person.NickName,
                LastName = person.LastName,
                DeceasedDate = person.DeceasedDate,
                InactiveNote = person.InactiveReasonNote,
                ConnectionStatus = person.ConnectionStatusValue?.Guid,
                RecordStatus = person.RecordStatusValue?.Guid,
                InactiveReason = person.RecordStatusReasonValue?.Guid,
                Suffix = person.SuffixValue?.Guid,
                AnniversaryDate = person.AnniversaryDate,
                BirthDate = person.BirthDate,
                Gender = person.Gender.ToMobile(),
                MaritalStatus = person.MaritalStatusValue?.Guid,
                CommunicationPreference = person.CommunicationPreference.ToMobile(),
                PhotoUrl = MobileHelper.BuildPublicApplicationRootUrl( person.PhotoUrl )
                //  Grade = person.GradeFormatted
            };
        }

        /// <summary>
        /// Updates the person from a person bag.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="personBag">The person bag.</param>
        /// <remarks>You still need to call RockContext.SaveChanges()</remarks>
        private void UpdatePersonFromBag( Person person, PersonBag personBag )
        {
            person.FirstName = personBag.FirstName;
            person.NickName = personBag.NickName;
            person.LastName = personBag.LastName;
            person.Gender = personBag.Gender.ToNative();
            person.InactiveReasonNote = personBag.InactiveNote;

            person.SuffixValueId = personBag.Suffix.HasValue ? DefinedValueCache.GetId( personBag.Suffix.Value ) : null;
            person.ConnectionStatusValueId = personBag.ConnectionStatus.HasValue ? DefinedValueCache.GetId( personBag.ConnectionStatus.Value ) : null;
            person.RecordStatusValueId = personBag.RecordStatus.HasValue ? DefinedValueCache.GetId( personBag.RecordStatus.Value ) : null;
            person.RecordStatusReasonValueId = personBag.InactiveReason.HasValue ? DefinedValueCache.GetId( personBag.InactiveReason.Value ) : null;
            person.DeceasedDate = personBag.DeceasedDate.HasValue ? personBag.DeceasedDate.Value.DateTime : ( DateTime? ) null;
            person.MaritalStatusValueId = personBag.MaritalStatus.HasValue ? DefinedValueCache.GetId( personBag.MaritalStatus.Value ) : null;
            person.AnniversaryDate = personBag.AnniversaryDate.HasValue ? personBag.AnniversaryDate.Value.DateTime : ( DateTime? ) null;

            var birthday = personBag.BirthDate;

            if ( birthday.HasValue )
            {
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

                person.SetBirthDate( personBag.BirthDate.Value.DateTime );
            }
            else
            {
                person.SetBirthDate( null );
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the person profile data.
        /// </summary>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult GetPersonProfileData()
        {
            using ( var rockContext = new RockContext() )
            {
                var person = RequestContext.GetContextEntity<Person>();

                if ( person == null )
                {
                    return ActionNotFound();
                }

                return ActionOk( new ResponseBag
                {
                    HeaderTemplate = GetHeaderTemplate( person ),
                    BadgeBarTemplate = GetBadgeBarTemplate( person ),
                    CustomActionsTemplate = GetCustomActionsTemplate( person ),
                    ContactInformation = GetPersonContactInformation( person ),
                    DemographicInformation = GetPersonDemographicInformation( person ),
                    PersonInformation = GetPersonBag( person, rockContext )
                } );
            }
        }

        /// <summary>
        /// Updates the person communication preference.
        /// </summary>
        /// <param name="communicationType">Type of the communication.</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult UpdatePersonCommunicationPreference( Rock.Common.Mobile.Enums.CommunicationType communicationType )
        {
            // Get the person from context.
            var requestPersonId = RequestContext.GetContextEntity<Person>()?.Id;

            if ( requestPersonId == null )
            {
                return ActionNotFound();
            }

            using ( var rockContext = new RockContext() )
            {
                var person = new PersonService( rockContext )
                    .Get( requestPersonId.Value );

                if ( !IsAuthorizedToEditPerson() )
                {
                    return ActionForbidden();
                }

                person.CommunicationPreference = communicationType.ToNative();
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Updates the phone number of a Person.
        /// </summary>
        /// <param name="phoneNumberBag"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult UpdatePersonPhoneNumber( PhoneNumberBag phoneNumberBag )
        {
            // Get the person from context.
            var requestPersonId = RequestContext.GetContextEntity<Person>()?.Id;

            if ( requestPersonId == null )
            {
                return ActionNotFound();
            }

            using ( var rockContext = new RockContext() )
            {
                var person = new PersonService( rockContext )
                    .Get( requestPersonId.Value );

                if ( !IsAuthorizedToEditPerson() )
                {
                    return ActionForbidden();
                }

                var id = CreateOrUpdatePersonPhoneNumber( person, phoneNumberBag, rockContext );

                if ( id == null || id == 0 )
                {
                    return ActionInternalServerError( "There was an error creating/updating the phone number." );
                }
            }

            return ActionOk();
        }

        /// <summary>
        /// Updates the person email.
        /// </summary>
        /// <param name="emailInfoBag">The email information bag.</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult UpdatePersonEmail( EmailInfoBag emailInfoBag )
        {
            // Get the person from context.
            var requestPersonId = RequestContext.GetContextEntity<Person>()?.Id;

            if ( requestPersonId == null )
            {
                return ActionNotFound( "Unable to find a Person from context." );
            }

            using ( var rockContext = new RockContext() )
            {
                var person = new PersonService( rockContext )
                    .Get( requestPersonId.Value );

                if ( person == null )
                {
                    return ActionNotFound();
                }

                if ( !IsAuthorizedToEditPerson() )
                {
                    return ActionForbidden();
                }

                UpdatePersonEmail( person, emailInfoBag );
                rockContext.SaveChanges();
            }

            return ActionOk();
        }

        /// <summary>
        /// Updates the person.
        /// </summary>
        /// <param name="personBag">The person bag.</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult UpdatePerson( PersonBag personBag )
        {

            using ( var rockContext = new RockContext() )
            {
                var person = new PersonService( rockContext ).Get( personBag.Guid );
                if ( person == null )
                {
                    return ActionNotFound();
                }

                if ( !IsAuthorizedToEditPerson() )
                {
                    return ActionForbidden( "You do not have access to update this Person profile." );
                }

                UpdatePersonFromBag( person, personBag );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Toggles whether or not this Person is followed
        /// by the current person or not.
        /// </summary>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult ToggleFollow()
        {
            var requestPersonId = RequestContext.GetContextEntity<Person>()?.Id;

            if ( requestPersonId == null )
            {
                return ActionNotFound( "Unable to find a Person from context." );
            }

            if ( RequestContext.CurrentPerson?.PrimaryAliasId == null )
            {
                return ActionForbidden();
            }

            using ( var rockContext = new RockContext() )
            {
                var person = new PersonService( rockContext )
                    .Get( requestPersonId.Value );

                if ( person == null )
                {
                    return ActionNotFound();
                }

                var followingService = new FollowingService( rockContext );
                var isFollowed = followingService.TogglePersonFollowing( person.PrimaryAliasId.Value, RequestContext.CurrentPerson.PrimaryAliasId.Value );

                rockContext.SaveChanges();

                return ActionOk( new
                {
                    IsFollowed = isFollowed
                } );
            }
        }

        #endregion
    }
}