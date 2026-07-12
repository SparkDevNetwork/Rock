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
using Rock.Crm.RecordSource;
using Rock.Data;
using Rock.Enums.Blocks.Connection.ConnectionRequestEntry;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Connection.ConnectionRequestEntry;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Connection
{
    [DisplayName( "Connection Request Entry" )]
    [Category( "Connection" )]
    [Description( "Public-facing block that lets a person request one or more connection opportunities." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [ConnectionTypesField( "Connection Types",
        Description = "The connection types used to determine which connection opportunities are available on the form.",
        IsRequired = true,
        Category = AttributeCategory.BasicSettings,
        Order = 0,
        Key = AttributeKey.ConnectionTypes )]

    [BooleanField( "Display Banner",
        Description = "Controls whether to show a banner at the top of the form.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.BasicSettings,
        Order = 1,
        Key = AttributeKey.DisplayBanner )]

    [CustomDropdownListField( "First Time Guest",
        Description = "Controls whether the form shows the first-time guest option and whether it is required.",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Hide",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 2,
        Key = AttributeKey.FirstTimeGuest )]

    [ConnectionOpportunityField( "First Time Guest Opportunity",
        Description = "The opportunity used when a person selects \"I am a first time guest,\" adding their request to this additional opportunity.",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 3,
        Key = AttributeKey.FirstTimeGuestOpportunity )]

    [CustomDropdownListField( "Title",
        Description = "Controls whether the form shows the person's title (such as Mr. or Mrs.) and whether it is required.",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Hide",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 4,
        Key = AttributeKey.Title )]

    [CustomDropdownListField( "Suffix",
        Description = "Controls whether the form shows the person's name suffix (such as Jr., Sr., or III), and whether it is required.",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Hide",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 5,
        Key = AttributeKey.Suffix )]

    [CustomDropdownListField( "Birthdate",
        Description = "Controls whether the form shows the person's birthdate and whether it is required.",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Show",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 6,
        Key = AttributeKey.Birthdate )]

    [CustomDropdownListField( "Gender",
        Description = "Controls whether the form shows the person's gender and whether it is required.",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Show",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 7,
        Key = AttributeKey.Gender )]

    [CustomDropdownListField( "Profile Photo",
        Description = "Controls whether the form shows the person's profile photo and whether it is required.",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Hide",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 8,
        Key = AttributeKey.ProfilePhoto )]

    [CustomDropdownListField( "Marital Status",
        Description = "Controls whether the form shows the person's marital status and whether it is required.",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Show",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 9,
        Key = AttributeKey.MaritalStatus )]

    [CustomDropdownListField( "Spouse First Name",
        Description = "Controls whether the form shows the spouse's first name and whether it is required. Note: this will only show if Marital Status is set as \"Married.\"",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Show",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 10,
        Key = AttributeKey.SpouseFirstName )]

    [CustomDropdownListField( "Spouse Last Name",
        Description = "Controls whether the form shows the spouse's last name and whether it is required. Note: this will only show if Marital Status is set as \"Married.\"",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Show",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 11,
        Key = AttributeKey.SpouseLastName )]

    [CustomDropdownListField( "Spouse Gender",
        Description = "Controls whether the form shows the spouse's gender and whether it is required. Note: this will only show if Marital Status is set as \"Married.\"",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Show",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 12,
        Key = AttributeKey.SpouseGender )]

    [CustomDropdownListField( "Email",
        Description = "Controls whether the form shows the person's email address and whether it is required.",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Required",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 13,
        Key = AttributeKey.Email )]

    [CustomDropdownListField( "Spouse Email",
        Description = "Controls whether the form shows the spouse's email address and whether it is required. Note: this will only show if Marital Status is set as \"Married.\"",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Hide",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 14,
        Key = AttributeKey.SpouseEmail )]

    [CustomDropdownListField( "Mobile Phone",
        Description = "Controls whether the form shows the person's mobile phone number and whether it is required.",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Show",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 15,
        Key = AttributeKey.MobilePhone )]

    [CustomDropdownListField( "Spouse Mobile Phone",
        Description = "Controls whether the form shows the spouse's mobile phone number and whether it is required. Note: this will only show if Marital Status is set as \"Married.\"",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Hide",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 16,
        Key = AttributeKey.SpouseMobilePhone )]

    [CustomDropdownListField( "SMS Enabled",
        Description = "Controls whether the form shows consent to receive text messages and whether it is required.",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Show",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 17,
        Key = AttributeKey.SmsEnabled )]

    [CustomDropdownListField( "Address",
        Description = "Controls whether the form shows the person's address and whether it is required.",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Show",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 18,
        Key = AttributeKey.Address )]

    [CustomDropdownListField( "Additional Comments",
        Description = "Controls whether the form shows additional comments and whether it is required.",
        ListSource = ListSource.HideShowRequired,
        DefaultValue = "Show",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 19,
        Key = AttributeKey.AdditionalComments )]

    [BooleanField( "Enable Captcha",
        Description = "Determines whether CAPTCHA verification is enabled for this form.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.BasicSettings,
        Order = 20,
        Key = AttributeKey.EnableCaptcha )]

    [CategoryField( "Person Attribute Category",
        Description = "The category used to determine which person attributes are available on the form.",
        EntityTypeName = "Rock.Model.Person",
        AllowMultiple = false,
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 21,
        Key = AttributeKey.PersonAttributeCategory )]

    [CategoryField( "Preferred Service Time",
        Description = "The category used to determine which service times are available on the form to set as Preferred. If no category is set, the field is hidden.",
        EntityTypeName = "Rock.Model.Schedule",
        AllowMultiple = false,
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 22,
        Key = AttributeKey.PreferredServiceTime )]

    [UrlLinkField( "Optional Redirect URL",
        Description = "The URL to redirect the person to after a request is submitted. Leaving blank will generate a default completion message.",
        IsRequired = false,
        Category = AttributeCategory.BasicSettings,
        Order = 23,
        Key = AttributeKey.OptionalRedirectUrl )]

    [DefinedValueField( "Connection Status",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        Description = "The connection status to use for new individuals (default: 'Prospect').",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT,
        Category = AttributeCategory.BasicSettings,
        Order = 24,
        Key = AttributeKey.ConnectionStatus )]

    [DefinedValueField( "Record Status",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        Description = "The record status to use for new individuals (default: 'Pending').",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
        Category = AttributeCategory.BasicSettings,
        Order = 25,
        Key = AttributeKey.RecordStatus )]

    [DefinedValueField( "Record Source",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
        Description = "The record source to use for new individuals (default: 'Serving Connection'). If a 'RecordSource' page parameter is found, it will be used instead.",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_SERVING_CONNECTION,
        Category = AttributeCategory.BasicSettings,
        Order = 26,
        Key = AttributeKey.RecordSource )]

    [TextField( "Banner Icon",
        Description = "The icon used to display in the banner.",
        DefaultValue = "ti ti-route-alt-left",
        IsRequired = false,
        Category = AttributeCategory.CustomizeTextBanner,
        Order = 27,
        Key = AttributeKey.BannerIcon )]

    [TextField( "Banner Title",
        Description = "The title used to display in the banner.",
        DefaultValue = "Next Steps",
        IsRequired = false,
        Category = AttributeCategory.CustomizeTextBanner,
        Order = 28,
        Key = AttributeKey.BannerTitle )]

    [TextField( "Banner Description",
        Description = "The description used to display in the banner.",
        DefaultValue = "We want to connect with you and help you take a next step!",
        IsRequired = false,
        Category = AttributeCategory.CustomizeTextBanner,
        Order = 29,
        Key = AttributeKey.BannerDescription )]

    [TextField( "Personal Information Section Title",
        Description = "The title displayed for the personal information section.",
        DefaultValue = "Personal Information",
        IsRequired = false,
        Category = AttributeCategory.CustomizeTextPersonalInformation,
        Order = 30,
        Key = AttributeKey.PersonalInformationTitle )]

    [TextField( "Personal Information Section Description",
        Description = "The supporting text displayed below the section title to provide context.",
        DefaultValue = "Help us get to know you and support you more personally.",
        IsRequired = false,
        Category = AttributeCategory.CustomizeTextPersonalInformation,
        Order = 31,
        Key = AttributeKey.PersonalInformationDescription )]

    [TextField( "Contact Information Section Title",
        Description = "The title displayed for the contact information section.",
        DefaultValue = "Contact Information",
        IsRequired = false,
        Category = AttributeCategory.CustomizeTextContactInformation,
        Order = 32,
        Key = AttributeKey.ContactInformationTitle )]

    [TextField( "Contact Information Section Description",
        Description = "The supporting text displayed below the section title to provide context.",
        DefaultValue = "Provide the best ways for us to stay in touch with you.",
        IsRequired = false,
        Category = AttributeCategory.CustomizeTextContactInformation,
        Order = 33,
        Key = AttributeKey.ContactInformationDescription )]

    [TextField( "Additional Comments Label",
        Description = "The text field label for capturing additional comments.",
        DefaultValue = "Additional Comments",
        IsRequired = false,
        Category = AttributeCategory.CustomizeTextContactInformation,
        Order = 34,
        Key = AttributeKey.AdditionalCommentsLabel )]

    [TextField( "Connection Opportunities Section Title",
        Description = "The title displayed for the connection opportunities section.",
        DefaultValue = "Connection Opportunities",
        IsRequired = false,
        Category = AttributeCategory.CustomizeTextConnectionOpportunities,
        Order = 35,
        Key = AttributeKey.ConnectionOpportunitiesTitle )]

    [TextField( "Connection Opportunities Section Description",
        Description = "The supporting text displayed below the section title to provide context.",
        DefaultValue = "Select the areas where you'd like to get involved.",
        IsRequired = false,
        Category = AttributeCategory.CustomizeTextConnectionOpportunities,
        Order = 36,
        Key = AttributeKey.ConnectionOpportunitiesDescription )]

    [TextField( "Additional Information Section Title",
        Description = "The title displayed for the additional information section.",
        DefaultValue = "Additional Information",
        IsRequired = false,
        Category = AttributeCategory.CustomizeTextAdditionalInformation,
        Order = 37,
        Key = AttributeKey.AdditionalInformationTitle )]

    [TextField( "Additional Information Section Description",
        Description = "The supporting text displayed below the section title to provide context.",
        DefaultValue = "Provide any additional details to help us better understand your request to get connected.",
        IsRequired = false,
        Category = AttributeCategory.CustomizeTextAdditionalInformation,
        Order = 38,
        Key = AttributeKey.AdditionalInformationDescription )]

    [TextField( "Submission Success Section Title",
        Description = "The headline displayed after a connection request is successfully submitted.",
        DefaultValue = "Submitted Connection Request Successfully",
        IsRequired = false,
        Category = AttributeCategory.CustomizeTextSubmissionSuccess,
        Order = 39,
        Key = AttributeKey.SubmissionSuccessTitle )]

    [TextField( "Submission Success Section Description",
        Description = "The message displayed after submission to confirm the request was received.",
        DefaultValue = "Thanks for taking a step to get more connected! We'll be in contact soon.",
        IsRequired = false,
        Category = AttributeCategory.CustomizeTextSubmissionSuccess,
        Order = 40,
        Key = AttributeKey.SubmissionSuccessDescription )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "B50D3225-D224-45B4-A24A-31E91C1C2DAB" )]
    [Rock.SystemGuid.BlockTypeGuid( "AD404374-5DA6-4F13-B997-E29494D708A4" )]
    [ContextAware( typeof( Campus ) )]
    public class ConnectionRequestEntry : RockBlockType
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ConnectionTypes = "ConnectionTypes";
            public const string DisplayBanner = "DisplayBanner";
            public const string FirstTimeGuest = "FirstTimeGuest";
            public const string FirstTimeGuestOpportunity = "FirstTimeGuestOpportunity";
            public const string Title = "Title";
            public const string Suffix = "Suffix";
            public const string Birthdate = "Birthdate";
            public const string Gender = "Gender";
            public const string ProfilePhoto = "ProfilePhoto";
            public const string MaritalStatus = "MaritalStatus";
            public const string SpouseFirstName = "SpouseFirstName";
            public const string SpouseLastName = "SpouseLastName";
            public const string SpouseGender = "SpouseGender";
            public const string Email = "Email";
            public const string SpouseEmail = "SpouseEmail";
            public const string MobilePhone = "MobilePhone";
            public const string SpouseMobilePhone = "SpouseMobilePhone";
            public const string SmsEnabled = "SmsEnabled";
            public const string Address = "Address";
            public const string AdditionalComments = "AdditionalComments";
            public const string EnableCaptcha = "EnableCaptcha";
            public const string PersonAttributeCategory = "PersonAttributeCategory";
            public const string PreferredServiceTime = "PreferredServiceTime";
            public const string OptionalRedirectUrl = "OptionalRedirectUrl";
            public const string ConnectionStatus = "ConnectionStatus";
            public const string RecordStatus = "RecordStatus";
            public const string RecordSource = "RecordSource";
            public const string BannerIcon = "BannerIcon";
            public const string BannerTitle = "BannerTitle";
            public const string BannerDescription = "BannerDescription";
            public const string PersonalInformationTitle = "PersonalInformationTitle";
            public const string PersonalInformationDescription = "PersonalInformationDescription";
            public const string ContactInformationTitle = "ContactInformationTitle";
            public const string ContactInformationDescription = "ContactInformationDescription";
            public const string AdditionalCommentsLabel = "AdditionalCommentsLabel";
            public const string ConnectionOpportunitiesTitle = "ConnectionOpportunitiesTitle";
            public const string ConnectionOpportunitiesDescription = "ConnectionOpportunitiesDescription";
            public const string AdditionalInformationTitle = "AdditionalInformationTitle";
            public const string AdditionalInformationDescription = "AdditionalInformationDescription";
            public const string SubmissionSuccessTitle = "SubmissionSuccessTitle";
            public const string SubmissionSuccessDescription = "SubmissionSuccessDescription";
        }

        #endregion Attribute Keys

        #region Attribute Categories

        private static class AttributeCategory
        {
            public const string BasicSettings = "Basic Settings";
            public const string CustomizeTextBanner = "Customize Text^Banner";
            public const string CustomizeTextPersonalInformation = "Customize Text^Personal Information Section";
            public const string CustomizeTextContactInformation = "Customize Text^Contact Information Section";
            public const string CustomizeTextConnectionOpportunities = "Customize Text^Connection Opportunities Section";
            public const string CustomizeTextAdditionalInformation = "Customize Text^Additional Information Section";
            public const string CustomizeTextSubmissionSuccess = "Customize Text^Submission Success Alert";
        }

        #endregion Attribute Categories

        #region List Sources

        private static class ListSource
        {
            public const string HideShowRequired = "Hide,Show,Required";
        }

        #endregion List Sources

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ConnectionRequestEntryInitializationBox
            {
                IsBannerVisible = GetAttributeValue( AttributeKey.DisplayBanner ).AsBoolean(),
                BannerIconCssClass = GetAttributeValue( AttributeKey.BannerIcon ),
                BannerTitle = GetAttributeValue( AttributeKey.BannerTitle ),
                BannerDescription = GetAttributeValue( AttributeKey.BannerDescription ),

                PersonalInformationTitle = GetAttributeValue( AttributeKey.PersonalInformationTitle ),
                PersonalInformationDescription = GetAttributeValue( AttributeKey.PersonalInformationDescription ),
                ContactInformationTitle = GetAttributeValue( AttributeKey.ContactInformationTitle ),
                ContactInformationDescription = GetAttributeValue( AttributeKey.ContactInformationDescription ),
                ConnectionOpportunitiesTitle = GetAttributeValue( AttributeKey.ConnectionOpportunitiesTitle ),
                ConnectionOpportunitiesDescription = GetAttributeValue( AttributeKey.ConnectionOpportunitiesDescription ),
                AdditionalInformationTitle = GetAttributeValue( AttributeKey.AdditionalInformationTitle ),
                AdditionalInformationDescription = GetAttributeValue( AttributeKey.AdditionalInformationDescription ),
                AdditionalCommentsLabel = GetAttributeValue( AttributeKey.AdditionalCommentsLabel ),
                SubmissionSuccessTitle = GetAttributeValue( AttributeKey.SubmissionSuccessTitle ),
                SubmissionSuccessDescription = GetAttributeValue( AttributeKey.SubmissionSuccessDescription )
            };

            var currentPerson = GetCurrentPerson();

            // Connection Types is the one setting the block cannot function without. When it is
            // missing, surface an admin-only warning (visitors simply see nothing) rather than
            // rendering an empty form.
            box.IsConfigured = GetAttributeValue( AttributeKey.ConnectionTypes ).SplitDelimitedValues().AsGuidList().Any();
            if ( !box.IsConfigured && BlockCache.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                box.ConfigurationMessage = "The Connection Request Entry block is not configured. Select one or more Connection Types in the block settings to offer connection opportunities.";
            }

            SetCampusInitialization( box, currentPerson );
            SetFieldVisibility( box );
            SetOptions( box );
            SetOpportunities( box );
            SetPersonAttributes( box );

            box.IsFirstTimeGuestOpportunityConfigured = GetAttributeValue( AttributeKey.FirstTimeGuestOpportunity ).AsGuidOrNull().HasValue;

            // Configured but offering nothing: warn editors that the chosen connection type(s)
            // have no active opportunities, so they recognize a data/config gap rather than a defect.
            if ( box.IsConfigured
                && box.Opportunities.Count == 0
                && BlockCache.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                box.OpportunitiesAdminMessage = "The configured connection type(s) have no active opportunities to display. Add or activate an opportunity, or update the block's Connection Types setting.";
            }

            box.IsCaptchaEnabled = GetAttributeValue( AttributeKey.EnableCaptcha ).AsBoolean()
                && !Captcha.CaptchaService.ShouldDisableCaptcha( false );

            box.PrefilledValues = GetPrefilledValues( currentPerson, box );

            return box;
        }

        /// <summary>
        /// Populates the campus list and the pre-selected campus on the initialization box.
        /// </summary>
        private void SetCampusInitialization( ConnectionRequestEntryInitializationBox box, Person currentPerson )
        {
            var campuses = CampusCache.All()
                .Where( c => c.IsActive ?? false )
                .OrderBy( c => c.Name )
                .ToList();

            box.IsCampusVisible = campuses.Count > 1;

            int? selectedCampusId = RequestContext.GetContextEntity<Campus>()?.Id;

            if ( !selectedCampusId.HasValue && currentPerson != null )
            {
                selectedCampusId = currentPerson.PrimaryCampusId ?? currentPerson.GetCampus()?.Id;
            }

            if ( !selectedCampusId.HasValue )
            {
                selectedCampusId = campuses.FirstOrDefault()?.Id;
            }

            var selectedCampus = selectedCampusId.HasValue ? CampusCache.Get( selectedCampusId.Value ) : null;
            box.SelectedCampus = selectedCampus != null
                ? new ListItemBag { Value = selectedCampus.Guid.ToString(), Text = selectedCampus.Name }
                : null;
        }

        /// <summary>
        /// Maps each Show/Hide/Required setting to its field-visibility property on the box.
        /// </summary>
        private void SetFieldVisibility( ConnectionRequestEntryInitializationBox box )
        {
            box.FirstTimeGuestVisibility = GetFieldVisibility( AttributeKey.FirstTimeGuest );
            box.TitleVisibility = GetFieldVisibility( AttributeKey.Title );
            box.SuffixVisibility = GetFieldVisibility( AttributeKey.Suffix );
            box.BirthDateVisibility = GetFieldVisibility( AttributeKey.Birthdate );
            box.GenderVisibility = GetFieldVisibility( AttributeKey.Gender );
            box.ProfilePhotoVisibility = GetFieldVisibility( AttributeKey.ProfilePhoto );
            box.MaritalStatusVisibility = GetFieldVisibility( AttributeKey.MaritalStatus );
            box.SpouseFirstNameVisibility = GetFieldVisibility( AttributeKey.SpouseFirstName );
            box.SpouseLastNameVisibility = GetFieldVisibility( AttributeKey.SpouseLastName );
            box.SpouseGenderVisibility = GetFieldVisibility( AttributeKey.SpouseGender );
            box.EmailVisibility = GetFieldVisibility( AttributeKey.Email );
            box.SpouseEmailVisibility = GetFieldVisibility( AttributeKey.SpouseEmail );
            box.MobilePhoneVisibility = GetFieldVisibility( AttributeKey.MobilePhone );
            box.SpouseMobilePhoneVisibility = GetFieldVisibility( AttributeKey.SpouseMobilePhone );
            box.SmsConsentVisibility = GetFieldVisibility( AttributeKey.SmsEnabled );
            box.AddressVisibility = GetFieldVisibility( AttributeKey.Address );
            box.AdditionalCommentsVisibility = GetFieldVisibility( AttributeKey.AdditionalComments );

            // Preferred Service Time is only offered when a schedule category is configured.
            var hasServiceTimeCategory = GetAttributeValue( AttributeKey.PreferredServiceTime ).AsGuidOrNull().HasValue;
            box.PreferredServiceTimeVisibility = hasServiceTimeCategory
                ? ConnectionRequestEntryFieldVisibility.Optional
                : ConnectionRequestEntryFieldVisibility.Hidden;
        }

        /// <summary>
        /// Populates the Title, Suffix, Marital Status, and Preferred Service Time option lists on the box.
        /// </summary>
        private void SetOptions( ConnectionRequestEntryInitializationBox box )
        {
            box.TitleOptions = GetDefinedValueOptions( Rock.SystemGuid.DefinedType.PERSON_TITLE );
            box.SuffixOptions = GetDefinedValueOptions( Rock.SystemGuid.DefinedType.PERSON_SUFFIX );
            box.MaritalStatusOptions = GetDefinedValueOptions( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS );
            box.PreferredServiceTimeOptions = GetPreferredServiceTimeOptions();
        }

        /// <summary>
        /// Populates the offered opportunities, each with its public connection request attributes, on the box.
        /// </summary>
        private void SetOpportunities( ConnectionRequestEntryInitializationBox box )
        {
            var connectionTypeGuids = GetAttributeValue( AttributeKey.ConnectionTypes ).SplitDelimitedValues().AsGuidList();

            if ( !connectionTypeGuids.Any() )
            {
                box.Opportunities = new List<ConnectionRequestEntryOpportunityBag>();
                return;
            }

            var opportunities = new ConnectionOpportunityService( RockContext )
                .Queryable()
                .Include( o => o.ConnectionType )
                .Where( o =>
                    o.IsActive &&
                    o.ConnectionType.IsActive &&
                    connectionTypeGuids.Contains( o.ConnectionType.Guid ) )
                .OrderBy( o => o.PublicName )
                .ThenBy( o => o.Name )
                .ToList();

            box.Opportunities = opportunities
                .Select( o => new ConnectionRequestEntryOpportunityBag
                {
                    IdKey = o.IdKey,
                    Name = o.PublicName.IsNotNullOrWhiteSpace() ? o.PublicName : o.Name,
                    Description = o.Description,
                    IconCssClass = o.IconCssClass,
                    ConnectionTypeName = o.ConnectionType.Name,
                    ConnectionTypeIconCssClass = o.ConnectionType.IconCssClass,
                    Attributes = GetPublicOpportunityAttributes( o )
                } )
                .ToList();
        }

        /// <summary>
        /// Populates the Additional Information person attributes on the box when a category is configured.
        /// </summary>
        private void SetPersonAttributes( ConnectionRequestEntryInitializationBox box )
        {
            var categoryGuid = GetAttributeValue( AttributeKey.PersonAttributeCategory ).AsGuidOrNull();

            if ( !categoryGuid.HasValue )
            {
                return;
            }

            var category = CategoryCache.Get( categoryGuid.Value );

            if ( category == null )
            {
                return;
            }

            var person = new Person();
            person.LoadAttributes( RockContext );

            box.PersonAttributes = person.Attributes.Values
                .Where( a => a.IsPublic && a.Categories.Any( c => c.Id == category.Id ) )
                .ToDictionary( a => a.Key, a => PublicAttributeHelper.GetPublicAttributeForEdit( a ) );
        }

        /// <summary>
        /// Builds the pre-populated form values from the visitor's Person record.
        /// </summary>
        /// <returns>The pre-filled request bag, or null when no person is logged in.</returns>
        private ConnectionRequestEntryRequestBag GetPrefilledValues( Person person, ConnectionRequestEntryInitializationBox box )
        {
            if ( person == null )
            {
                return null;
            }

            /*
                06/23/26 - JMH

                Only pre-populate fields that are actually shown. A hidden field's value would otherwise be
                serialized into the page configuration and exposed in the browser even though it never renders,
                leaking the visitor's personal data on this public-facing block.

                Reason: Do not send hidden-field values to the client.
            */
            static bool IsShown( ConnectionRequestEntryFieldVisibility visibility )
            {
                return visibility != ConnectionRequestEntryFieldVisibility.Hidden;
            }

            var bag = new ConnectionRequestEntryRequestBag
            {
                FirstName = person.NickName.IsNotNullOrWhiteSpace() ? person.NickName : person.FirstName,
                LastName = person.LastName
            };

            if ( IsShown( box.EmailVisibility ) )
            {
                bag.Email = person.Email;
            }

            if ( IsShown( box.TitleVisibility ) )
            {
                bag.Title = ToDefinedValueListItem( person.TitleValueId );
            }

            if ( IsShown( box.SuffixVisibility ) )
            {
                bag.Suffix = ToDefinedValueListItem( person.SuffixValueId );
            }

            if ( IsShown( box.MaritalStatusVisibility ) )
            {
                bag.MaritalStatus = ToDefinedValueListItem( person.MaritalStatusValueId );
            }

            if ( IsShown( box.GenderVisibility ) )
            {
                bag.Gender = person.Gender.ToString();
            }

            if ( IsShown( box.BirthDateVisibility ) )
            {
                bag.BirthDate = person.BirthDate?.ToString( "s" );
            }

            if ( IsShown( box.MobilePhoneVisibility ) )
            {
                bag.MobilePhone = GetMobilePhoneBag( person );
            }

            if ( IsShown( box.AddressVisibility ) )
            {
                var homeLocation = person.GetHomeLocation( RockContext );
                if ( homeLocation != null )
                {
                    bag.Address = new AddressControlBag
                    {
                        Street1 = homeLocation.Street1,
                        Street2 = homeLocation.Street2,
                        City = homeLocation.City,
                        State = homeLocation.State,
                        Locality = homeLocation.County,
                        PostalCode = homeLocation.PostalCode,
                        Country = homeLocation.Country
                    };
                }
            }

            if ( GetAttributeValue( AttributeKey.PreferredServiceTime ).AsGuidOrNull().HasValue && person.PreferredServiceTimeScheduleId.HasValue )
            {
                var schedule = new ScheduleService( RockContext ).Get( person.PreferredServiceTimeScheduleId.Value );
                if ( schedule != null )
                {
                    bag.PreferredServiceTime = new ListItemBag { Value = schedule.Guid.ToString(), Text = schedule.Name };
                }
            }

            var spouse = person.GetSpouse( RockContext );
            if ( spouse != null )
            {
                if ( IsShown( box.SpouseFirstNameVisibility ) )
                {
                    bag.SpouseFirstName = spouse.NickName.IsNotNullOrWhiteSpace() ? spouse.NickName : spouse.FirstName;
                }

                if ( IsShown( box.SpouseLastNameVisibility ) )
                {
                    bag.SpouseLastName = spouse.LastName;
                }

                if ( IsShown( box.SpouseGenderVisibility ) )
                {
                    bag.SpouseGender = spouse.Gender.ToString();
                }

                if ( IsShown( box.SpouseEmailVisibility ) )
                {
                    bag.SpouseEmail = spouse.Email;
                }

                if ( IsShown( box.SpouseMobilePhoneVisibility ) )
                {
                    bag.SpouseMobilePhone = GetMobilePhoneBag( spouse );
                }
            }

            return bag;
        }

        /// <summary>
        /// Gets the mobile phone control bag for a person, or null when no mobile number is on file.
        /// </summary>
        private PhoneNumberBoxWithSmsControlBag GetMobilePhoneBag( Person person )
        {
            var mobileType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );

            if ( mobileType == null )
            {
                return null;
            }

            var mobile = person.PhoneNumbers?.FirstOrDefault( p => p.NumberTypeValueId == mobileType.Id );

            if ( mobile == null )
            {
                return null;
            }

            return new PhoneNumberBoxWithSmsControlBag
            {
                Number = mobile.NumberFormatted,
                CountryCode = mobile.CountryCode,
                IsMessagingEnabled = mobile.IsMessagingEnabled
            };
        }

        /// <summary>
        /// Converts a defined value identifier into a list item bag carrying its Guid and value.
        /// </summary>
        private ListItemBag ToDefinedValueListItem( int? definedValueId )
        {
            if ( !definedValueId.HasValue )
            {
                return null;
            }

            var definedValue = DefinedValueCache.Get( definedValueId.Value );

            if ( definedValue == null )
            {
                return null;
            }

            return new ListItemBag { Value = definedValue.Guid.ToString(), Text = definedValue.Value };
        }

        /// <summary>
        /// Gets the active defined values of a defined type as list item bags keyed by Guid.
        /// </summary>
        private List<ListItemBag> GetDefinedValueOptions( string definedTypeGuid )
        {
            var definedType = DefinedTypeCache.Get( definedTypeGuid.AsGuid() );

            if ( definedType == null )
            {
                return new List<ListItemBag>();
            }

            return definedType.DefinedValues
                .Where( v => v.IsActive )
                .Select( v => new ListItemBag { Value = v.Guid.ToString(), Text = v.Value } )
                .ToList();
        }

        /// <summary>
        /// Gets the active schedules in the configured Preferred Service Time category as list item bags keyed by Guid.
        /// </summary>
        private List<ListItemBag> GetPreferredServiceTimeOptions()
        {
            var categoryGuid = GetAttributeValue( AttributeKey.PreferredServiceTime ).AsGuidOrNull();

            if ( !categoryGuid.HasValue )
            {
                return new List<ListItemBag>();
            }

            var category = CategoryCache.Get( categoryGuid.Value );

            if ( category == null )
            {
                return new List<ListItemBag>();
            }

            return new ScheduleService( RockContext )
                .Queryable()
                .Where( s => s.IsActive && s.CategoryId.HasValue && s.CategoryId.Value == category.Id )
                .OrderBy( s => s.Name )
                .ToList()
                .Select( s => new ListItemBag { Value = s.Guid.ToString(), Text = s.Name } )
                .ToList();
        }

        /// <summary>
        /// Resolves the selected Preferred Service Time list item to its <see cref="Schedule"/> identifier.
        /// </summary>
        /// <param name="scheduleItem">The selected schedule list item; its value is the Schedule's Guid.</param>
        /// <returns>The Schedule identifier, or <c>null</c> when nothing valid is selected.</returns>
        private int? GetScheduleId( ListItemBag scheduleItem )
        {
            var scheduleGuid = scheduleItem?.Value.AsGuidOrNull();

            if ( !scheduleGuid.HasValue )
            {
                return null;
            }

            return new ScheduleService( RockContext ).GetId( scheduleGuid.Value );
        }

        /// <summary>
        /// Gets the public connection request attributes for an opportunity, for use as inline editors.
        /// </summary>
        private Dictionary<string, PublicAttributeBag> GetPublicOpportunityAttributes( ConnectionOpportunity opportunity )
        {
            // Both identifiers are required for the connection request attribute inheritance to resolve.
            var connectionRequest = new ConnectionRequest
            {
                ConnectionOpportunityId = opportunity.Id,
                ConnectionTypeId = opportunity.ConnectionTypeId
            };

            connectionRequest.LoadAttributes( RockContext );

            return ( connectionRequest.Attributes?.Values ?? Enumerable.Empty<AttributeCache>() )
                .Where( a => a.IsPublic )
                .ToDictionary( a => a.Key, a => PublicAttributeHelper.GetPublicAttributeForEdit( a ) );
        }

        /// <summary>
        /// Reads a Show/Hide/Required setting and maps it to a field-visibility value.
        /// </summary>
        private ConnectionRequestEntryFieldVisibility GetFieldVisibility( string attributeKey )
        {
            var value = GetAttributeValue( attributeKey );

            switch ( value )
            {
                case "Required":
                    return ConnectionRequestEntryFieldVisibility.Required;
                case "Show":
                    return ConnectionRequestEntryFieldVisibility.Optional;
                default:
                    return ConnectionRequestEntryFieldVisibility.Hidden;
            }
        }

        /// <summary>
        /// Indicates whether the field for the given setting is shown on the form (optional or required).
        /// </summary>
        private bool IsFieldShown( string attributeKey )
        {
            return GetFieldVisibility( attributeKey ) != ConnectionRequestEntryFieldVisibility.Hidden;
        }

        /// <summary>
        /// Saves or updates a phone number for the given person and phone type.
        /// </summary>
        private void SavePhone( string number, string countryCode, Person person, Guid phoneTypeGuid, bool isMessagingEnabled )
        {
            var numberType = DefinedValueCache.Get( phoneTypeGuid );

            if ( numberType == null )
            {
                return;
            }

            var newPhoneNumber = PhoneNumber.CleanNumber( number );

            if ( string.IsNullOrWhiteSpace( newPhoneNumber ) )
            {
                return;
            }

            var phone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == numberType.Id );

            if ( phone == null )
            {
                phone = new PhoneNumber { NumberTypeValueId = numberType.Id };
                person.PhoneNumbers.Add( phone );
            }

            phone.CountryCode = PhoneNumber.CleanNumber( countryCode );
            phone.Number = newPhoneNumber;
            phone.IsMessagingEnabled = isMessagingEnabled;
        }

        /// <summary>
        /// Gets the record source identifier to use for new individuals.
        /// </summary>
        /// <returns>The identifier of the Record Source Type <see cref="DefinedValue"/> to use.</returns>
        private int? GetRecordSourceValueId()
        {
            return RecordSourceHelper.GetSessionRecordSourceValueId()
                ?? DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordSource ).AsGuid() )?.Id;
        }

        /// <summary>
        /// Saves the home address for the family group from the submitted address.
        /// </summary>
        private void SaveAddress( AddressControlBag address, Person person )
        {
            if ( !IsFieldShown( AttributeKey.Address ) )
            {
                return;
            }

            if ( address == null || address.Street1.IsNullOrWhiteSpace() )
            {
                return;
            }

            var homeLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
            var family = person.GetFamily( RockContext );

            if ( homeLocationType == null || family == null )
            {
                return;
            }

            var location = new LocationService( RockContext ).Get(
                address.Street1,
                address.Street2,
                address.City,
                address.State,
                address.PostalCode,
                address.Country ?? GlobalAttributesCache.Get().OrganizationCountry,
                new GetLocationArgs
                {
                    CreateNewLocation = true,
                    Group = family,
                    ValidateLocation = false,
                    VerifyLocation = true
                } );

            if ( location == null )
            {
                return;
            }

            var groupLocation = family.GroupLocations
                .FirstOrDefault( l => l.GroupLocationTypeValueId == homeLocationType.Id );

            if ( groupLocation == null || groupLocation.LocationId != location.Id )
            {
                GroupService.AddNewGroupAddress(
                    RockContext,
                    family,
                    homeLocationType.Guid.ToString(),
                    location,
                    moveExistingToPrevious: true,
                    modifiedBy: string.Empty,
                    isMailingLocation: true,
                    isMappedLocation: true );
            }
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Matches or creates the visitor (and optional spouse), saves their information, and creates a connection request per selected opportunity.
        /// </summary>
        /// <param name="bag">The submitted form values.</param>
        /// <returns>The submission result, including any configured redirect URL.</returns>
        [BlockAction]
        public BlockActionResult Save( ConnectionRequestEntryRequestBag bag )
        {
            try
            {
                if ( bag == null )
                {
                    return ActionBadRequest( "Invalid request." );
                }

                var isCaptchaEnabled = GetAttributeValue( AttributeKey.EnableCaptcha ).AsBoolean()
                    && !Captcha.CaptchaService.ShouldDisableCaptcha( false );

                if ( isCaptchaEnabled && !RequestContext.IsCaptchaValid )
                {
                    return ActionBadRequest( "Captcha was not valid." );
                }

                var firstTimeGuestOpportunityGuid = GetAttributeValue( AttributeKey.FirstTimeGuestOpportunity ).AsGuidOrNull();
                var hasSelectedOpportunity = bag.SelectedOpportunities?.Any() == true;
                var isStandaloneFirstTimeGuest = bag.IsFirstTimeGuest && firstTimeGuestOpportunityGuid.HasValue;

                if ( !hasSelectedOpportunity && !isStandaloneFirstTimeGuest )
                {
                    return ActionBadRequest( "Please select at least one opportunity." );
                }

                // First and last name are always-shown required fields; email is required only when the setting demands it.
                if ( bag.FirstName.IsNullOrWhiteSpace() || bag.LastName.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( "First Name and Last Name are required." );
                }

                if ( GetFieldVisibility( AttributeKey.Email ) == ConnectionRequestEntryFieldVisibility.Required && bag.Email.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( "Email is required." );
                }

                var campusId = bag.CampusGuid.HasValue ? CampusCache.Get( bag.CampusGuid.Value )?.Id : null;
                var currentPerson = GetCurrentPerson();

                var person = currentPerson ?? new PersonService( RockContext ).FindPerson(
                    new PersonService.PersonMatchQuery( bag.FirstName, bag.LastName, bag.Email, bag.MobilePhone?.Number ),
                    updatePrimaryEmail: false );

                if ( person == null || !person.PrimaryAliasId.HasValue )
                {
                    person = CreateNewPerson( bag, campusId );
                }

                if ( IsFieldShown( AttributeKey.MobilePhone ) && bag.MobilePhone != null && bag.MobilePhone.Number.IsNotNullOrWhiteSpace() )
                {
                    SavePhone( bag.MobilePhone.Number, bag.MobilePhone.CountryCode, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), bag.MobilePhone.IsMessagingEnabled );
                }

                if ( IsFieldShown( AttributeKey.ProfilePhoto ) && bag.PhotoGuid.IsNotNullOrWhiteSpace() && Guid.TryParse( bag.PhotoGuid, out var photoGuid ) )
                {
                    person.PhotoId = new BinaryFileService( RockContext ).GetId( photoGuid );
                }

                // Persist the preferred service time (a Schedule) only when the field is offered
                // (a Preferred Service Time schedule category is configured).
                if ( GetAttributeValue( AttributeKey.PreferredServiceTime ).AsGuidOrNull().HasValue )
                {
                    person.PreferredServiceTimeScheduleId = GetScheduleId( bag.PreferredServiceTime );
                }

                RockContext.SaveChanges();

                SaveAddress( bag.Address, person );
                SavePersonAttributeValues( bag, person, currentPerson );
                SaveSpouse( bag, person, currentPerson );

                var ( created, redirectUrl ) = CreateConnectionRequests( bag, person, campusId, firstTimeGuestOpportunityGuid );

                if ( !created )
                {
                    return ActionBadRequest( "Please select at least one available opportunity." );
                }

                return ActionOk( new ConnectionRequestEntryResultBag { RedirectUrl = redirectUrl } );
            }
            catch ( Exception ex )
            {
                return ActionInternalServerError( "An unexpected error occurred: " + ex.Message );
            }
        }

        #endregion Block Actions

        #region Save Helpers

        /// <summary>
        /// Creates and persists a new Person (with their family) from the submitted values.
        /// </summary>
        private Person CreateNewPerson( ConnectionRequestEntryRequestBag bag, int? campusId )
        {
            var connectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
            var recordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );

            // Only carry a submitted value onto the new record when its field is shown; a crafted payload
            // could otherwise set fields the form hid (such as Email, Gender, or Birthdate).
            var person = new Person
            {
                FirstName = bag.FirstName,
                LastName = bag.LastName,
                Email = IsFieldShown( AttributeKey.Email ) ? bag.Email : null,
                IsEmailActive = true,
                EmailPreference = EmailPreference.EmailAllowed,
                RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON ).Id,
                ConnectionStatusValueId = connectionStatus?.Id,
                RecordStatusValueId = recordStatus?.Id,
                RecordSourceValueId = GetRecordSourceValueId(),
                TitleValueId = IsFieldShown( AttributeKey.Title ) ? GetDefinedValueId( bag.Title ) : null,
                SuffixValueId = IsFieldShown( AttributeKey.Suffix ) ? GetDefinedValueId( bag.Suffix ) : null,
                MaritalStatusValueId = IsFieldShown( AttributeKey.MaritalStatus ) ? GetDefinedValueId( bag.MaritalStatus ) : null,
                Gender = IsFieldShown( AttributeKey.Gender ) ? ( bag.Gender.ConvertToEnumOrNull<Gender>() ?? Gender.Unknown ) : Gender.Unknown
            };

            var birthDate = bag.BirthDate.AsDateTime();
            if ( IsFieldShown( AttributeKey.Birthdate ) && birthDate.HasValue )
            {
                person.SetBirthDate( birthDate );
            }

            PersonService.SaveNewPerson( person, RockContext, campusId, false );

            return person;
        }

        /// <summary>
        /// Saves the Additional Information person attribute values from the submitted form.
        /// </summary>
        private void SavePersonAttributeValues( ConnectionRequestEntryRequestBag bag, Person person, Person currentPerson )
        {
            if ( bag.PersonAttributeValues == null || !bag.PersonAttributeValues.Any() )
            {
                return;
            }

            // Only the public person attributes in the configured category were offered on the form. Filtering the
            // submitted values to that set keeps a crafted payload from writing arbitrary person attributes.
            var categoryGuid = GetAttributeValue( AttributeKey.PersonAttributeCategory ).AsGuidOrNull();

            if ( !categoryGuid.HasValue )
            {
                return;
            }

            var category = CategoryCache.Get( categoryGuid.Value );

            if ( category == null )
            {
                return;
            }

            person.LoadAttributes( RockContext );

            var allowedKeys = person.Attributes.Values
                .Where( a => a.IsPublic && a.Categories.Any( c => c.Id == category.Id ) )
                .Select( a => a.Key )
                .ToHashSet();

            var allowedValues = bag.PersonAttributeValues
                .Where( kvp => allowedKeys.Contains( kvp.Key ) )
                .ToDictionary( kvp => kvp.Key, kvp => kvp.Value );

            if ( !allowedValues.Any() )
            {
                return;
            }

            person.SetPublicAttributeValues( allowedValues, currentPerson, enforceSecurity: false );
            person.SaveAttributeValues( RockContext );
        }

        /// <summary>
        /// Matches or creates the spouse and persists their information when the visitor is married and spouse details were supplied.
        /// </summary>
        private void SaveSpouse( ConnectionRequestEntryRequestBag bag, Person person, Person currentPerson )
        {
            // A spouse is only created when the form actually offered marital status and the spouse name fields,
            // the submitted status is Married, and both names were provided. The name fields gate the whole block.
            var areSpouseNamesShown = IsFieldShown( AttributeKey.SpouseFirstName ) && IsFieldShown( AttributeKey.SpouseLastName );

            if ( !IsFieldShown( AttributeKey.MaritalStatus ) || !areSpouseNamesShown )
            {
                return;
            }

            var maritalStatusGuid = bag.MaritalStatus?.Value.AsGuidOrNull();
            var isMarried = maritalStatusGuid == Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid();

            if ( !isMarried || bag.SpouseFirstName.IsNullOrWhiteSpace() || bag.SpouseLastName.IsNullOrWhiteSpace() )
            {
                return;
            }

            // For a logged-in visitor whose family already has a spouse, update that spouse in place rather than duplicating.
            var spouse = currentPerson != null ? person.GetSpouse( RockContext ) : null;

            if ( spouse == null )
            {
                var married = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED );

                var spouseEmail = IsFieldShown( AttributeKey.SpouseEmail ) ? bag.SpouseEmail : null;

                spouse = new Person
                {
                    FirstName = bag.SpouseFirstName,
                    LastName = bag.SpouseLastName,
                    Email = spouseEmail,
                    IsEmailActive = spouseEmail.IsNotNullOrWhiteSpace(),
                    EmailPreference = EmailPreference.EmailAllowed,
                    RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON ).Id,
                    ConnectionStatusValueId = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() )?.Id,
                    RecordStatusValueId = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() )?.Id,
                    RecordSourceValueId = GetRecordSourceValueId(),
                    MaritalStatusValueId = married?.Id,
                    Gender = IsFieldShown( AttributeKey.SpouseGender ) ? ( bag.SpouseGender.ConvertToEnumOrNull<Gender>() ?? Gender.Unknown ) : Gender.Unknown
                };

                var family = person.GetFamily( RockContext );
                var adultRoleId = GroupTypeCache.GetFamilyGroupType().Roles
                    .First( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;

                PersonService.AddPersonToFamily( spouse, true, family.Id, adultRoleId, RockContext );
            }
            else
            {
                spouse.FirstName = bag.SpouseFirstName;
                spouse.LastName = bag.SpouseLastName;

                if ( IsFieldShown( AttributeKey.SpouseEmail ) && bag.SpouseEmail.IsNotNullOrWhiteSpace() )
                {
                    spouse.Email = bag.SpouseEmail;
                }

                if ( IsFieldShown( AttributeKey.SpouseGender ) )
                {
                    var spouseGender = bag.SpouseGender.ConvertToEnumOrNull<Gender>();
                    if ( spouseGender.HasValue )
                    {
                        spouse.Gender = spouseGender.Value;
                    }
                }
            }

            if ( IsFieldShown( AttributeKey.SpouseMobilePhone ) && bag.SpouseMobilePhone != null && bag.SpouseMobilePhone.Number.IsNotNullOrWhiteSpace() )
            {
                SavePhone( bag.SpouseMobilePhone.Number, bag.SpouseMobilePhone.CountryCode, spouse, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), bag.SpouseMobilePhone.IsMessagingEnabled );
            }

            RockContext.SaveChanges();
        }

        /// <summary>
        /// Creates one connection request per valid selected opportunity (and the first-time-guest opportunity when applicable) within a single transaction.
        /// </summary>
        /// <returns>A tuple whose <c>created</c> flag is false when no valid request was built, and whose <c>redirectUrl</c> is the resolved Optional Redirect URL (empty when none is configured).</returns>
        private (bool created, string redirectUrl) CreateConnectionRequests( ConnectionRequestEntryRequestBag bag, Person person, int? campusId, Guid? firstTimeGuestOpportunityGuid )
        {
            var currentPerson = GetCurrentPerson();
            var opportunityService = new ConnectionOpportunityService( RockContext );
            var connectionRequestService = new ConnectionRequestService( RockContext );

            /*
                06/23/26 - JMH

                The server cannot trust the submitted opportunity list. A crafted payload could reference an
                inactive opportunity, an opportunity under an inactive connection type, or an opportunity whose
                connection type was never made available through this block's Connection Types setting. Each
                opportunity is validated against the configured set (with its ConnectionType eagerly loaded so
                the active/Guid checks are reliable) before a request is built, and only the public request
                attributes the form actually offered are accepted.

                Reason: Do not trust client-submitted opportunities or attribute values.
            */
            var allowedConnectionTypeGuids = GetAttributeValue( AttributeKey.ConnectionTypes )
                .SplitDelimitedValues()
                .AsGuidList()
                .ToHashSet();

            var selectedOpportunities = bag.SelectedOpportunities ?? new List<ConnectionRequestEntrySelectedOpportunityBag>();
            var requestsToSave = new List<ConnectionRequest>();

            foreach ( var selected in selectedOpportunities )
            {
                var opportunityId = opportunityService.Get( selected.OpportunityIdKey, !PageCache.Layout.Site.DisablePredictableIds )?.Id;

                if ( !opportunityId.HasValue )
                {
                    continue;
                }

                var opportunity = opportunityService.Queryable()
                    .Include( o => o.ConnectionType )
                    .FirstOrDefault( o => o.Id == opportunityId.Value );

                if ( !IsOpportunitySelectable( opportunity, allowedConnectionTypeGuids ) )
                {
                    continue;
                }

                var request = BuildConnectionRequest( opportunity, person, campusId, bag.AdditionalComments );

                request.LoadAttributes( RockContext );

                // Only attributes the form offered for this opportunity may be set.
                var allowedAttributeKeys = GetPublicOpportunityAttributes( opportunity ).Keys.ToHashSet();
                var allowedAttributeValues = ( selected.AttributeValues ?? new Dictionary<string, string>() )
                    .Where( kvp => allowedAttributeKeys.Contains( kvp.Key ) )
                    .ToDictionary( kvp => kvp.Key, kvp => kvp.Value );

                if ( allowedAttributeValues.Any() )
                {
                    request.SetPublicAttributeValues( allowedAttributeValues, currentPerson, enforceSecurity: false );
                }

                requestsToSave.Add( request );
            }

            // The first-time-guest opportunity is admin-configured and therefore trusted; it is exempt from the
            // allowed-Connection-Types membership check but must still be active before a request is added.
            if ( bag.IsFirstTimeGuest && firstTimeGuestOpportunityGuid.HasValue )
            {
                var firstTimeGuestOpportunity = opportunityService.Queryable()
                    .Include( o => o.ConnectionType )
                    .FirstOrDefault( o => o.Guid == firstTimeGuestOpportunityGuid.Value );

                if ( IsOpportunityActive( firstTimeGuestOpportunity ) )
                {
                    var request = BuildConnectionRequest( firstTimeGuestOpportunity, person, campusId, bag.AdditionalComments );
                    request.LoadAttributes( RockContext );
                    requestsToSave.Add( request );
                }
            }

            if ( !requestsToSave.Any() )
            {
                return (false, string.Empty);
            }

            RockContext.WrapTransaction( () =>
            {
                foreach ( var request in requestsToSave )
                {
                    connectionRequestService.Add( request );
                }

                RockContext.SaveChanges();

                foreach ( var request in requestsToSave )
                {
                    request.SaveAttributeValues( RockContext );
                }
            } );

            var redirectUrl = GetAttributeValue( AttributeKey.OptionalRedirectUrl );

            return (true, redirectUrl.IsNotNullOrWhiteSpace() ? redirectUrl : string.Empty);
        }

        /// <summary>
        /// Indicates whether an opportunity is active and belongs to an active connection type.
        /// </summary>
        private bool IsOpportunityActive( ConnectionOpportunity opportunity )
        {
            return opportunity != null
                && opportunity.IsActive
                && opportunity.ConnectionType != null
                && opportunity.ConnectionType.IsActive;
        }

        /// <summary>
        /// Indicates whether an opportunity may be selected through this block: it must be active, under an active
        /// connection type, and that connection type must be one the block's Connection Types setting allows.
        /// </summary>
        private bool IsOpportunitySelectable( ConnectionOpportunity opportunity, HashSet<Guid> allowedConnectionTypeGuids )
        {
            return IsOpportunityActive( opportunity )
                && allowedConnectionTypeGuids.Contains( opportunity.ConnectionType.Guid );
        }

        /// <summary>
        /// Builds a connection request for an opportunity using its connection type's default status and connector.
        /// </summary>
        private ConnectionRequest BuildConnectionRequest( ConnectionOpportunity opportunity, Person person, int? campusId, string comments )
        {
            var defaultStatusId = opportunity.ConnectionType.ConnectionStatuses
                .Where( s => s.IsDefault )
                .Select( s => s.Id )
                .FirstOrDefault();

            return new ConnectionRequest
            {
                PersonAliasId = person.PrimaryAliasId.Value,
                ConnectionOpportunityId = opportunity.Id,
                ConnectionTypeId = opportunity.ConnectionTypeId,
                ConnectionState = ConnectionState.Active,
                ConnectionStatusId = defaultStatusId,
                CampusId = campusId,
                ConnectorPersonAliasId = opportunity.GetDefaultConnectorPersonAliasId( campusId ),
                Comments = comments ?? string.Empty
            };
        }

        /// <summary>
        /// Resolves the defined value identifier from a list item bag carrying a defined value Guid.
        /// </summary>
        private int? GetDefinedValueId( ListItemBag listItem )
        {
            var guid = listItem?.Value.AsGuidOrNull();

            return guid.HasValue ? DefinedValueCache.Get( guid.Value )?.Id : null;
        }

        #endregion Save Helpers
    }
}
