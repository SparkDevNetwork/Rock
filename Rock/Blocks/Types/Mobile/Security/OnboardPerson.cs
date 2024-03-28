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
using Rock.Common.Mobile;
using Rock.Common.Mobile.Blocks.Security.OnboardPerson;
using Rock.Common.Mobile.Enums;
using Rock.Communication;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.Utility.Enums;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Security
{
    /// <summary>
    /// Provides an interface for the user to safely identify themselves and create a login.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Onboard Person" )]
    [Category( "Mobile > Security" )]
    [Description( "Provides an interface for the user to safely identify themselves and create a login." )]
    [IconCssClass( "fa fa-plane-departure" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [BooleanField( "Allow Skip of Onboarding",
        Description = "Allows the user to skip the onboarding process and go straight to the homepage.",
        IsRequired = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = true,
        Key = AttributeKeys.AllowSkipOfOnboarding,
        Order = 0 )]

    [DefinedValueField( "Default Connection Status",
        Description = "The connection status to use for new individuals (default = 'Prospect'.)",
        DefinedTypeGuid = SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT,
        Key = AttributeKeys.DefaultConnectionStatus,
        Order = 1 )]

    [DefinedValueField( "Default Record Status",
        Description = "The record status to use for new individuals (default = 'Pending'.)",
        DefinedTypeGuid = SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
        Key = AttributeKeys.DefaultRecordStatus,
        Order = 2 )]

    [SystemCommunicationField( "System Communication",
        Description = "The communication that will be used to send the SMS or email to the user.",
        IsRequired = true,
        Key = AttributeKeys.SystemCommunication,
        Order = 3 )]

    [GroupCategoryField( "Communication List Categories",
        Description = "The category of communication lists that will be made available to the user as topics of interest.",
        GroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST,
        IsRequired = false,
        AllowMultiple = true,
        Key = AttributeKeys.CommunicationListCategories,
        Order = 4 )]

    [IntegerField( "Verification Time Limit",
        Description = "The number of minutes that the user has to enter the verification code.",
        DefaultIntegerValue = 5,
        Key = AttributeKeys.VerificationTimeLimit,
        Order = 5 )]

    [IntegerField( "IP Throttle Limit",
        Description = "The number of times a single IP address can submit phone numbers for verification per day.",
        DefaultIntegerValue = 5000,
        Key = AttributeKeys.IpThrottleLimit,
        Order = 6 )]

    [IntegerField( "Validation Code Attempts",
        Description = "The number of times a validation code verification can be re-tried before failing permanently.",
        DefaultIntegerValue = IdentityVerification.DefaultMaxFailedMatchAttemptCount,
        Key = AttributeKeys.ValidationCodeAttempts,
        Order = 7 )]

    #region Campus Block Attributes

    [DefinedValueField( "Display Campus Types",
        Description = "The campus types that will be included in the list of campuses for the user to choose from.",
        DefinedTypeGuid = SystemGuid.DefinedType.CAMPUS_TYPE,
        IsRequired = true,
        DefaultValue = SystemGuid.DefinedValue.CAMPUS_TYPE_PHYSICAL,
        AllowMultiple = true,
        Category = AttributeCategories.Campus,
        Key = AttributeKeys.DisplayCampusTypes,
        Order = 0 )]

    [DefinedValueField( "Display Campus Statuses",
        Description = "The campus types that will be included in the list of campuses for the user to choose from.",
        DefinedTypeGuid = SystemGuid.DefinedType.CAMPUS_STATUS,
        IsRequired = true,
        DefaultValue = SystemGuid.DefinedValue.CAMPUS_STATUS_OPEN,
        AllowMultiple = true,
        Category = AttributeCategories.Campus,
        Key = AttributeKeys.DisplayCampusStatuses,
        Order = 1 )]

    [CampusField( name: "Online Campus",
        description: "The campus to pick for the user if they press the 'Online Campus' button.",
        required: false,
        includeInactive: false,
        category: AttributeCategories.Campus,
        key: AttributeKeys.OnlineCampus,
        order: 2,
        ForceVisible = true )]

    [CampusField( name: "Do Not Attend Campus",
        description: "The campus to pick for the user if they press the 'Do Not Attend' button.",
        required: false,
        includeInactive: false,
        category: AttributeCategories.Campus,
        key: AttributeKeys.DoNotAttendCampus,
        order: 3,
        ForceVisible = true )]

    #endregion

    #region Pages Block Attributes

    [LinkedPage( "Completed Page",
        Description = "The page to redirect the user to after the onboarding process has finished.",
        IsRequired = true,
        Category = AttributeCategories.Pages,
        Key = AttributeKeys.CompletedPage,
        Order = 0 )]

    [LinkedPage( "Login Page",
        Description = "The page to use when allowing log in by existing account credentials.",
        IsRequired = false,
        Category = AttributeCategories.Pages,
        Key = AttributeKeys.LoginPage,
        Order = 1 )]

    #endregion

    #region Titles Block Attributes

    [TextField( "Hello Screen Title",
        Description = "The title to display at the top of the Hello screen. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "Hello!",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.HelloScreenTitle,
        Order = 0 )]

    [TextField( "Hello Screen Subtitle",
        Description = "The text to display at the top of the Hello screen underneath the title. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "Welcome to the {{ 'Global' | Attribute:'OrganizationName' }} mobile app. Please sign-in so we can personalize your experience.",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.HelloScreenSubtitle,
        Order = 1 )]

    [TextField( "Code Sent Screen Title",
        Description = "The title to display at the top of the Code Sent screen. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "Code Sent...",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.CodeSentScreenTitle,
        Order = 2 )]

    [TextField( "Code Sent Screen Subtitle",
        Description = "The text to display at the top of the Code Sent screen underneath the title. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "You should be receiving a verification code from us shortly. When it arrives type or paste it below.",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.CodeSentScreenSubtitle,
        Order = 3 )]

    [TextField( "Name Screen Title",
        Description = "The title to display at the top of the Name screen. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "Let’s Get to Know You",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.NameScreenTitle,
        Order = 4 )]

    [TextField( "Name Screen Subtitle",
        Description = "The text to display at the top of the Name screen underneath the title. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "To maximize your experience we’d like to know a little about you.",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.NameScreenSubtitle,
        Order = 5 )]

    [TextField( "Personal Information Screen Title",
        Description = "The title to display at the top of the Personal Information screen. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "Tell Us More",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.PersonalInformationScreenTitle,
        Order = 6 )]

    [TextField( "Personal Information Screen Subtitle",
        Description = "The text to display at the top of the Personal Information screen underneath the title. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "The more we know the more we can tailor our ministry to you.",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.PersonalInformationScreenSubtitle,
        Order = 7 )]

    [TextField( "Contact Information Screen Title",
        Description = "The title to display at the top of the Contact Information screen. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "Stay Connected",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.ContactInformationScreenTitle,
        Order = 8 )]

    [TextField( "Contact Information Screen Subtitle",
        Description = "The text to display at the top of the Contact Information screen underneath the title. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "Help us keep you in the loop by providing your contact information.",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.ContactInformationScreenSubtitle,
        Order = 9 )]

    [TextField( "Interests Screen Title",
        Description = "The title to display at the top of the Interests screen. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "Topics Of Interest",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.InterestsScreenTitle,
        Order = 10 )]

    [TextField( "Interests Screen Subtitle",
        Description = "The text to display at the top of the Interests screen underneath the title. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "What topics are you most interested in.",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.InterestsScreenSubtitle,
        Order = 11 )]

    [TextField( "Notifications Screen Title",
        Description = "The title to display at the top of the Notifications screen. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "Enable Notifications",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.NotificationsScreenTitle,
        Order = 12 )]

    [TextField( "Notifications Screen Subtitle",
        Description = "The text to display at the top of the Notifications screen underneath the title. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "We’d like to keep you in the loop with important alerts and notifications.",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.NotificationsScreenSubtitle,
        Order = 13 )]

    [TextField( "Campus Screen Title",
        Description = "The title to display at the top of the Campus screen. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "Find Your Campus",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.CampusScreenTitle,
        Order = 14 )]

    [TextField( "Campus Screen Subtitle",
        Description = "The text to display at the top of the Campus screen underneath the title. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "Select the campus you attend to get targets news and information about events.",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.CampusScreenSubtitle,
        Order = 15 )]

    [TextField( "Create Login Screen Title",
        Description = "The title to display at the top of the Create Login screen. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "Create Login",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.CreateLoginScreenTitle,
        Order = 16 )]

    [TextField( "Create Login Screen Subtitle",
        Description = "The text to display at the top of the Create Login screen underneath the title. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "Create a login to help signing in quicker in the future.",
        Category = AttributeCategories.Titles,
        Key = AttributeKeys.CreateLoginScreenSubtitle,
        Order = 17 )]

    #endregion

    #region Visibility Block Attributes

    [EnumField( "Gender",
        Description = "Determines if the Gender field should be hidden, optional or required.",
        EnumSourceType = typeof( VisibilityTriState ),
        IsRequired = true,
        DefaultEnumValue = ( int ) VisibilityTriState.Optional,
        Category = AttributeCategories.Visibility,
        Key = AttributeKeys.GenderVisibility,
        Order = 0 )]

    [BooleanField( "Hide Gender if Known",
        Description = "Hides the Gender field if a value is already known.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Category = AttributeCategories.Visibility,
        Key = AttributeKeys.HideGenderIfKnown,
        Order = 1 )]

    [EnumField( "Birth Date",
        Description = "Determines if the Birth Date field should be hidden, optional or required.",
        EnumSourceType = typeof( VisibilityTriState ),
        IsRequired = true,
        DefaultEnumValue = ( int ) VisibilityTriState.Optional,
        Category = AttributeCategories.Visibility,
        Key = AttributeKeys.BirthDateVisibility,
        Order = 2 )]

    [BooleanField( "Hide Birth Date if Known",
        Description = "Hides the Birth Date field if a value is already known.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Category = AttributeCategories.Visibility,
        Key = AttributeKeys.HideBirthDateIfKnown,
        Order = 3 )]

    [EnumField( "Mobile Phone",
        Description = "Determines if the Mobile Phone field should be hidden, optional or required.",
        EnumSourceType = typeof( VisibilityTriState ),
        IsRequired = true,
        DefaultEnumValue = ( int ) VisibilityTriState.Optional,
        Category = AttributeCategories.Visibility,
        Key = AttributeKeys.MobilePhoneVisibility,
        Order = 4 )]

    [BooleanField( "Hide Mobile Phone if Known",
        Description = "Hides the Mobile Phone field if a value is already known.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Category = AttributeCategories.Visibility,
        Key = AttributeKeys.HideMobilePhoneIfKnown,
        Order = 5 )]

    [EnumField( "Email",
        Description = "Determines if the Email field should be hidden, optional or required.",
        EnumSourceType = typeof( VisibilityTriState ),
        IsRequired = true,
        DefaultEnumValue = ( int ) VisibilityTriState.Optional,
        Category = AttributeCategories.Visibility,
        Key = AttributeKeys.EmailVisibility,
        Order = 6 )]

    [BooleanField( "Hide Email if Known",
        Description = "Hides the Email field if a value is already known.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Category = AttributeCategories.Visibility,
        Key = AttributeKeys.HideEmailIfKnown,
        Order = 7 )]

    [BooleanField( "Show Notifications Request",
        Description = "Shows the screen that will request the user to grant permission to send notifications to them.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = AttributeCategories.Visibility,
        Key = AttributeKeys.ShowNotificationsRequest,
        Order = 8 )]

    [BooleanField( "Hide Campus if Known",
        Description = "Hides the Campus field if a value is already known.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Category = AttributeCategories.Visibility,
        Key = AttributeKeys.HideCampusIfKnown,
        Order = 9 )]

    [EnumField( "Create Login",
        Description = "Determines if the Create Login screen should be hidden, optional or required.",
        EnumSourceType = typeof( VisibilityTriState ),
        IsRequired = true,
        DefaultEnumValue = ( int ) VisibilityTriState.Optional,
        Category = AttributeCategories.Visibility,
        Key = AttributeKeys.CreateLoginVisibility,
        Order = 10 )]

    #endregion

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_SECURITY_ONBOARD_PERSON )]
    [Rock.SystemGuid.BlockTypeGuid( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47")]
    public class OnboardPerson : RockBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The attribute categories for the <see cref="OnboardPerson"/> block.
        /// </summary>
        private static class AttributeCategories
        {
            /// <summary>
            /// The campus category.
            /// </summary>
            public const string Campus = "Campus";

            /// <summary>
            /// The pages category.
            /// </summary>
            public const string Pages = "Pages";

            /// <summary>
            /// The visibility category.
            /// </summary>
            public const string Visibility = "Visibility";

            /// <summary>
            /// The titles category.
            /// </summary>
            public const string Titles = "Titles";
        }

        /// <summary>
        /// The block setting attribute keys for the <see cref="OnboardPerson"/> block.
        /// </summary>
        private static class AttributeKeys
        {
            /// <summary>
            /// The allow skip of on-boarding key.
            /// </summary>
            public const string AllowSkipOfOnboarding = "AllowSkipOfOnboarding";

            /// <summary>
            /// The default connection status key.
            /// </summary>
            public const string DefaultConnectionStatus = "DefaultConnectionStatus";

            /// <summary>
            /// The default record status key.
            /// </summary>
            public const string DefaultRecordStatus = "DefaultRecordStatus";

            /// <summary>
            /// The system communication key.
            /// </summary>
            public const string SystemCommunication = "SystemCommunication";

            /// <summary>
            /// The communication list category key.
            /// </summary>
            public const string CommunicationListCategories = "CommunicationListCategory";

            /// <summary>
            /// The verification time limit key.
            /// </summary>
            public const string VerificationTimeLimit = "VerificationTimeLimit";

            /// <summary>
            /// The IP throttle limit key.
            /// </summary>
            public const string IpThrottleLimit = "IpThrottleLimit";

            /// <summary>
            /// The validation code attempts key.
            /// </summary>
            public const string ValidationCodeAttempts = "ValidationCodeAttempts";

            /// <summary>
            /// The display campus types key.
            /// </summary>
            public const string DisplayCampusTypes = "DisplayCampusTypes";

            /// <summary>
            /// The display campus statuses key.
            /// </summary>
            public const string DisplayCampusStatuses = "DisplayCampusStatuses";

            /// <summary>
            /// The online campus key.
            /// </summary>
            public const string OnlineCampus = "OnlineCampus";

            /// <summary>
            /// The do not attend campus key.
            /// </summary>
            public const string DoNotAttendCampus = "DoNotAttendCampus";

            /// <summary>
            /// The completed page key.
            /// </summary>
            public const string CompletedPage = "CompletedPage";

            /// <summary>
            /// The registration page key.
            /// </summary>
            public const string LoginPage = "LoginPage";

            /// <summary>
            /// The gender visibility key.
            /// </summary>
            public const string GenderVisibility = "GenderVisibility";

            /// <summary>
            /// The hide gender if known key.
            /// </summary>
            public const string HideGenderIfKnown = "HideGenderIfKnown";

            /// <summary>
            /// The birth date visibility key.
            /// </summary>
            public const string BirthDateVisibility = "BirthDateVisibility";

            /// <summary>
            /// The hide birth date if known key.
            /// </summary>
            public const string HideBirthDateIfKnown = "HideBirthDateIfKnown";

            /// <summary>
            /// The mobile phone visibility key.
            /// </summary>
            public const string MobilePhoneVisibility = "MobilePhoneVisibility";

            /// <summary>
            /// The hide mobile phone if known key.
            /// </summary>
            public const string HideMobilePhoneIfKnown = "HideMobilePhoneIfKnown";

            /// <summary>
            /// The email visibility key.
            /// </summary>
            public const string EmailVisibility = "EmailVisibility";

            /// <summary>
            /// The hide email if known key.
            /// </summary>
            public const string HideEmailIfKnown = "HideEmailIfKnown";

            /// <summary>
            /// The show notifications request key.
            /// </summary>
            public const string ShowNotificationsRequest = "ShowNotificationsRequest";

            /// <summary>
            /// The hide campus if known key.
            /// </summary>
            public const string HideCampusIfKnown = "HideCampusIfKnown";

            /// <summary>
            /// The create login visibility key.
            /// </summary>
            public const string CreateLoginVisibility = "CreateLoginVisibility";

            /// <summary>
            /// The hello screen title key.
            /// </summary>
            public const string HelloScreenTitle = "HelloScreenTitle";

            /// <summary>
            /// The hello screen subtitle key.
            /// </summary>
            public const string HelloScreenSubtitle = "HelloScreenSubtitle";

            /// <summary>
            /// The code sent screen title key.
            /// </summary>
            public const string CodeSentScreenTitle = "CodeSentScreenTitle";

            /// <summary>
            /// The code sent screen subtitle key.
            /// </summary>
            public const string CodeSentScreenSubtitle = "CodeSentScreenSubtitle";

            /// <summary>
            /// The name screen title key.
            /// </summary>
            public const string NameScreenTitle = "NameScreenTitle";

            /// <summary>
            /// The name screen subtitle key.
            /// </summary>
            public const string NameScreenSubtitle = "NameScreenSubtitle";

            /// <summary>
            /// The personal information screen title key.
            /// </summary>
            public const string PersonalInformationScreenTitle = "PersonalInformationScreenTitle";

            /// <summary>
            /// The personal information screen subtitle key.
            /// </summary>
            public const string PersonalInformationScreenSubtitle = "PersonalInformationScreenSubtitle";

            /// <summary>
            /// The contact information screen title key.
            /// </summary>
            public const string ContactInformationScreenTitle = "ContactInformationScreenTitle";

            /// <summary>
            /// The contact information screen subtitle key.
            /// </summary>
            public const string ContactInformationScreenSubtitle = "ContactInformationScreenSubtitle";

            /// <summary>
            /// The interests screen title key.
            /// </summary>
            public const string InterestsScreenTitle = "InterestsScreenTitle";

            /// <summary>
            /// The interests screen subtitle key.
            /// </summary>
            public const string InterestsScreenSubtitle = "InterestsScreenSubtitle";

            /// <summary>
            /// The notifications screen title key.
            /// </summary>
            public const string NotificationsScreenTitle = "NotificationsScreenTitle";

            /// <summary>
            /// The notifications screen subtitle key.
            /// </summary>
            public const string NotificationsScreenSubtitle = "NotificationsScreenSubtitle";

            /// <summary>
            /// The campus screen title key.
            /// </summary>
            public const string CampusScreenTitle = "CampusScreenTitle";

            /// <summary>
            /// The campus screen subtitle key.
            /// </summary>
            public const string CampusScreenSubtitle = "CampusScreenSubtitle";

            /// <summary>
            /// The create login screen title key.
            /// </summary>
            public const string CreateLoginScreenTitle = "CreateLoginScreenTitle";

            /// <summary>
            /// The create login screen subtitle key.
            /// </summary>
            public const string CreateLoginScreenSubtitle = "CreateLoginScreenSubtitle";
        }

        /// <summary>
        /// Gets a value indicating whether user can skip on-boarding process.
        /// </summary>
        /// <value>
        ///   <c>true</c> if user can skip on-boarding process; otherwise, <c>false</c>.
        /// </value>
        public bool AllowSkipOfOnboarding => GetAttributeValue( AttributeKeys.AllowSkipOfOnboarding ).AsBoolean();

        /// <summary>
        /// Gets the default connection status unique identifier.
        /// </summary>
        /// <value>
        /// The default connection status unique identifier.
        /// </value>
        public Guid DefaultConnectionStatusGuid => GetAttributeValue( AttributeKeys.DefaultConnectionStatus ).AsGuid();

        /// <summary>
        /// Gets the default record status unique identifier.
        /// </summary>
        /// <value>
        /// The default record status unique identifier.
        /// </value>
        public Guid DefaultRecordStatusGuid => GetAttributeValue( AttributeKeys.DefaultRecordStatus ).AsGuid();

        /// <summary>
        /// Gets the system communication unique identifier.
        /// </summary>
        /// <value>
        /// The system communication unique identifier.
        /// </value>
        public Guid? SystemCommunicationGuid => GetAttributeValue( AttributeKeys.SystemCommunication ).AsGuidOrNull();

        /// <summary>
        /// Gets the communication list category guids.
        /// </summary>
        /// <value>
        /// The communication list category guids.
        /// </value>
        public List<Guid> CommunicationListCategoryGuids => GetAttributeValue( AttributeKeys.CommunicationListCategories ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the verification time limit in minutes.
        /// </summary>
        /// <value>
        /// The verification time limit in minutes.
        /// </value>
        public int VerificationTimeLimit => GetAttributeValue( AttributeKeys.VerificationTimeLimit ).AsInteger();

        /// <summary>
        /// Gets the per-IP throttle limit.
        /// </summary>
        /// <value>
        /// The per-IP throttle limit.
        /// </value>
        public int IPThrottleLimit => GetAttributeValue( AttributeKeys.IpThrottleLimit ).AsInteger();

        /// <summary>
        /// Gets the maximum validation code attempts.
        /// </summary>
        /// <value>
        /// The maximum validation code attempts.
        /// </value>
        public int ValidationCodeAttempts => GetAttributeValue( AttributeKeys.ValidationCodeAttempts ).AsInteger();

        /// <summary>
        /// Gets the protection profiles that will be used to prevent matching.
        /// </summary>
        /// <value>The protection profiles that will be used to prevent matching.</value>
        public List<AccountProtectionProfile> DisableMatchingProtectionProfiles => new SecuritySettingsService().SecuritySettings.DisablePasswordlessSignInForAccountProtectionProfiles;

        /// <summary>
        /// Gets the display campus type guids.
        /// </summary>
        /// <value>
        /// The display campus type guids.
        /// </value>
        public List<Guid> DisplayCampusTypeGuids => GetAttributeValue( AttributeKeys.DisplayCampusTypes ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the display campus status guids.
        /// </summary>
        /// <value>
        /// The display campus status guids.
        /// </value>
        public List<Guid> DisplayCampusStatusGuids => GetAttributeValue( AttributeKeys.DisplayCampusStatuses ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the online campus unique identifier.
        /// </summary>
        /// <value>
        /// The online campus unique identifier.
        /// </value>
        public Guid? OnlineCampusGuid => GetAttributeValue( AttributeKeys.OnlineCampus ).AsGuidOrNull();

        /// <summary>
        /// Gets the do not attend campus unique identifier.
        /// </summary>
        /// <value>
        /// The do not attend campus unique identifier.
        /// </value>
        public Guid? DoNotAttendCampusGuid => GetAttributeValue( AttributeKeys.DoNotAttendCampus ).AsGuidOrNull();

        /// <summary>
        /// Gets the gender visibility.
        /// </summary>
        /// <value>
        /// The gender visibility.
        /// </value>
        public VisibilityTriState GenderVisibility => GetAttributeValue( AttributeKeys.GenderVisibility ).ConvertToEnum<VisibilityTriState>();

        /// <summary>
        /// Gets a value indicating whether the gender field should be hidden if the value is known.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the gender field should be hidden if the value is known; otherwise, <c>false</c>.
        /// </value>
        public bool HideGenderIfKnown => GetAttributeValue( AttributeKeys.HideGenderIfKnown ).AsBoolean();

        /// <summary>
        /// Gets the birth date visibility.
        /// </summary>
        /// <value>
        /// The birth date visibility.
        /// </value>
        public VisibilityTriState BirthDateVisibility => GetAttributeValue( AttributeKeys.BirthDateVisibility ).ConvertToEnum<VisibilityTriState>();

        /// <summary>
        /// Gets a value indicating whether the birth date field should be hidden if the value is known.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the birth date field should be hidden if the value is known; otherwise, <c>false</c>.
        /// </value>
        public bool HideBirthDateIfKnown => GetAttributeValue( AttributeKeys.HideBirthDateIfKnown ).AsBoolean();

        /// <summary>
        /// Gets the mobile phone visibility.
        /// </summary>
        /// <value>
        /// The mobile phone visibility.
        /// </value>
        public VisibilityTriState MobilePhoneVisibility => GetAttributeValue( AttributeKeys.MobilePhoneVisibility ).ConvertToEnum<VisibilityTriState>();

        /// <summary>
        /// Gets a value indicating whether the mobile phone field should be hidden if value is known.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the mobile phone field should be hidden if value is known; otherwise, <c>false</c>.
        /// </value>
        public bool HideMobilePhoneIfKnown => GetAttributeValue( AttributeKeys.HideMobilePhoneIfKnown ).AsBoolean();

        /// <summary>
        /// Gets the email visibility.
        /// </summary>
        /// <value>
        /// The email visibility.
        /// </value>
        public VisibilityTriState EmailVisibility => GetAttributeValue( AttributeKeys.EmailVisibility ).ConvertToEnum<VisibilityTriState>();

        /// <summary>
        /// Gets a value indicating whether the email field should be hidden if value is known.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the email field should be hidden if value is known; otherwise, <c>false</c>.
        /// </value>
        public bool HideEmailIfKnown => GetAttributeValue( AttributeKeys.HideEmailIfKnown ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether the notification screen should be shown.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the notification screen should be shown; otherwise, <c>false</c>.
        /// </value>
        public bool ShowNotificationsRequest => GetAttributeValue( AttributeKeys.ShowNotificationsRequest ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether the campus field should be hidden if value is known.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the campus field should be hidden if value is known; otherwise, <c>false</c>.
        /// </value>
        public bool HideCampusIfKnown => GetAttributeValue( AttributeKeys.HideCampusIfKnown ).AsBoolean();

        /// <summary>
        /// Gets the create login visibility.
        /// </summary>
        /// <value>
        /// The create login visibility.
        /// </value>
        public VisibilityTriState CreateLoginVisibility => GetAttributeValue( AttributeKeys.CreateLoginVisibility ).ConvertToEnum<VisibilityTriState>();

        /// <summary>
        /// Gets the completed page unique identifier.
        /// </summary>
        /// <value>
        /// The completed page unique identifier.
        /// </value>
        public Guid? CompletedPageGuid => GetAttributeValue( AttributeKeys.CompletedPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the login page unique identifier.
        /// </summary>
        /// <value>
        /// The login page unique identifier.
        /// </value>
        public Guid? LoginPageGuid => GetAttributeValue( AttributeKeys.LoginPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the hello screen title.
        /// </summary>
        /// <value>
        /// The hello screen title.
        /// </value>
        public string HelloScreenTitle => GetAttributeValue( AttributeKeys.HelloScreenTitle );

        /// <summary>
        /// Gets the hello screen subtitle.
        /// </summary>
        /// <value>
        /// The hello screen subtitle.
        /// </value>
        public string HelloScreenSubtitle => GetAttributeValue( AttributeKeys.HelloScreenSubtitle );

        /// <summary>
        /// Gets the code sent screen title.
        /// </summary>
        /// <value>
        /// The code sent screen title.
        /// </value>
        public string CodeSentScreenTitle => GetAttributeValue( AttributeKeys.CodeSentScreenTitle );

        /// <summary>
        /// Gets the code sent screen subtitle.
        /// </summary>
        /// <value>
        /// The code sent screen subtitle.
        /// </value>
        public string CodeSentScreenSubtitle => GetAttributeValue( AttributeKeys.CodeSentScreenSubtitle );

        /// <summary>
        /// Gets the name screen title.
        /// </summary>
        /// <value>
        /// The name screen title.
        /// </value>
        public string NameScreenTitle => GetAttributeValue( AttributeKeys.NameScreenTitle );

        /// <summary>
        /// Gets the name screen subtitle.
        /// </summary>
        /// <value>
        /// The name screen subtitle.
        /// </value>
        public string NameScreenSubtitle => GetAttributeValue( AttributeKeys.NameScreenSubtitle );

        /// <summary>
        /// Gets the personal information screen title.
        /// </summary>
        /// <value>
        /// The personal information screen title.
        /// </value>
        public string PersonalInformationScreenTitle => GetAttributeValue( AttributeKeys.PersonalInformationScreenTitle );

        /// <summary>
        /// Gets the personal information screen subtitle.
        /// </summary>
        /// <value>
        /// The personal information screen subtitle.
        /// </value>
        public string PersonalInformationScreenSubtitle => GetAttributeValue( AttributeKeys.PersonalInformationScreenSubtitle );

        /// <summary>
        /// Gets the contact information screen title.
        /// </summary>
        /// <value>
        /// The contact information screen title.
        /// </value>
        public string ContactInformationScreenTitle => GetAttributeValue( AttributeKeys.ContactInformationScreenTitle );

        /// <summary>
        /// Gets the contact information screen subtitle.
        /// </summary>
        /// <value>
        /// The contact information screen subtitle.
        /// </value>
        public string ContactInformationScreenSubtitle => GetAttributeValue( AttributeKeys.ContactInformationScreenSubtitle );

        /// <summary>
        /// Gets the interests screen title.
        /// </summary>
        /// <value>
        /// The interests screen title.
        /// </value>
        public string InterestsScreenTitle => GetAttributeValue( AttributeKeys.InterestsScreenTitle );

        /// <summary>
        /// Gets the interests screen subtitle.
        /// </summary>
        /// <value>
        /// The interests screen subtitle.
        /// </value>
        public string InterestsScreenSubtitle => GetAttributeValue( AttributeKeys.InterestsScreenSubtitle );

        /// <summary>
        /// Gets the notifications screen title.
        /// </summary>
        /// <value>
        /// The notifications screen title.
        /// </value>
        public string NotificationsScreenTitle => GetAttributeValue( AttributeKeys.NotificationsScreenTitle );

        /// <summary>
        /// Gets the notifications screen subtitle.
        /// </summary>
        /// <value>
        /// The notifications screen subtitle.
        /// </value>
        public string NotificationsScreenSubtitle => GetAttributeValue( AttributeKeys.NotificationsScreenSubtitle );

        /// <summary>
        /// Gets the campus screen title.
        /// </summary>
        /// <value>
        /// The campus screen title.
        /// </value>
        public string CampusScreenTitle => GetAttributeValue( AttributeKeys.CampusScreenTitle );

        /// <summary>
        /// Gets the campus screen subtitle.
        /// </summary>
        /// <value>
        /// The campus screen subtitle.
        /// </value>
        public string CampusScreenSubtitle => GetAttributeValue( AttributeKeys.CampusScreenSubtitle );

        /// <summary>
        /// Gets the create login screen title.
        /// </summary>
        /// <value>
        /// The create login screen title.
        /// </value>
        public string CreateLoginScreenTitle => GetAttributeValue( AttributeKeys.CreateLoginScreenTitle );

        /// <summary>
        /// Gets the create login screen subtitle.
        /// </summary>
        /// <value>
        /// The create login screen subtitle.
        /// </value>
        public string CreateLoginScreenSubtitle => GetAttributeValue( AttributeKeys.CreateLoginScreenSubtitle );

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 2 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            using ( var rockContext = new RockContext() )
            {
                var systemCommunication = new SystemCommunicationService( rockContext ).Get( SystemCommunicationGuid ?? Guid.Empty );

                return new Rock.Common.Mobile.Blocks.Security.OnboardPerson.Configuration
                {
                    CanAuthenticateByEmail = MediumContainer.HasActiveEmailTransport() && systemCommunication != null,
                    CanAuthenticateBySms = MediumContainer.HasActiveSmsTransport() && systemCommunication?.SmsFromSystemPhoneNumberId != null,
                    AllowSkipOfOnboarding = AllowSkipOfOnboarding,
                    CompletedPageGuid = CompletedPageGuid ?? Guid.Empty,
                    LoginPageGuid = LoginPageGuid,
                    EmailAddressVisibility = EmailVisibility,
                    HideEmailAddressIfKnown = HideEmailIfKnown,
                    MobilePhoneVisibility = MobilePhoneVisibility,
                    HideMobilePhoneIfKnown = HideMobilePhoneIfKnown,
                    GenderVisibility = GenderVisibility,
                    HideGenderIfKnown = HideGenderIfKnown,
                    BirthDateVisibility = BirthDateVisibility,
                    HideBirthDateIfKnown = HideBirthDateIfKnown,
                    Interests = GetInterests( rockContext ),
                    RequestNotificationPermission = ShowNotificationsRequest,
                    Campuses = GetDisplayCampuses( rockContext ),
                    OnlineCampusGuid = OnlineCampusGuid,
                    DoNotAttendCampusGuid = DoNotAttendCampusGuid,
                    CreateLoginVisibility = CreateLoginVisibility,
                    ScreenSettings = GetScreenSettings(),
                    HeaderXaml = null
                };
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the list of campuses that should be displayed to the user to pick from.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// A list of <see cref="MobileCampus" /> objects.
        /// </returns>
        private List<MobileCampus> GetDisplayCampuses( RockContext rockContext )
        {
            // Get the campus status identifiers that will be used for
            // filtering.
            var campusStatusIds = DisplayCampusStatusGuids
                .Select( a => DefinedValueCache.Get( a, rockContext )?.Id )
                .Where( a => a != null )
                .Cast<int>()
                .ToList();

            // Get the campus type identifiers that will be used for filtering.
            var campusTypeIds = DisplayCampusTypeGuids
                .Select( a => DefinedValueCache.Get( a, rockContext )?.Id )
                .Where( a => a != null )
                .Cast<int>()
                .ToList();

            // Get all the campuses that match the filters and then cast them
            // to a MobileCampus type.
            return CampusCache.All()
                .Where( a => a.CampusStatusValueId.HasValue && campusStatusIds.Contains( a.CampusStatusValueId.Value ) )
                .Where( a => a.CampusTypeValueId.HasValue && campusTypeIds.Contains( a.CampusTypeValueId.Value ) )
                .Select( a => new MobileCampus
                {
                    Guid = a.Guid,
                    Name = a.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the topic interest areas that the user can pick from.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<InterestTopic> GetInterests( RockContext rockContext )
        {
            var categoryGuids = CommunicationListCategoryGuids;

            if ( !categoryGuids.Any() )
            {
                return new List<InterestTopic>();
            }

            int communicationListGroupTypeId = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST ).Id;
            int? communicationListGroupTypeDefaultRoleId = GroupTypeCache.Get( communicationListGroupTypeId ).DefaultGroupRoleId;

            // Get a list of syncs for the communication list groups where
            // the default role is sync'd. This is used to filter out the
            // list of communication lists.
            var commGroupSyncsForDefaultRole = new GroupSyncService( rockContext )
                .Queryable()
                .Where( a => a.Group.GroupTypeId == communicationListGroupTypeId )
                .Where( a => a.GroupTypeRoleId == communicationListGroupTypeDefaultRoleId )
                .Select( a => a.GroupId )
                .ToList();

            // Get the communication lists that are not syncing the default
            // member role.
            var communicationLists = new GroupService( rockContext )
               .Queryable()
               .Where( a => a.GroupTypeId == communicationListGroupTypeId && !commGroupSyncsForDefaultRole.Contains( a.Id ) )
               .ToList();

            communicationLists.LoadAttributes( rockContext );

            // Include only any communication lists that match a selected
            // category.
            communicationLists = communicationLists
                .Where( a => categoryGuids.Intersect( a.GetAttributeValue( "Category" ).SplitDelimitedValues().AsGuidList() ).Any() )
                .ToList();

            // Convert the communication lists into InterestTopics for
            // the mobile application to use.
            return communicationLists
                .Select( a => new InterestTopic
                {
                    Type = "CommunicationList",
                    Key = a.Guid.ToString(),
                    Title = a.GetAttributeValue( "PublicName" ).IfEmpty( a.Name )
                } )
                .OrderBy( a => a.Title )
                .ToList();
        }

        /// <summary>
        /// Gets the screen settings.
        /// </summary>
        /// <returns></returns>
        private List<OnboardScreenSettings> GetScreenSettings()
        {
            var mergeFields = RequestContext.GetCommonMergeFields();

            return new List<OnboardScreenSettings>
            {
                new OnboardScreenSettings
                {
                    Screen = OnboardScreen.Hello,
                    Title = HelloScreenTitle.ResolveMergeFields( mergeFields ),
                    Subtitle = HelloScreenSubtitle.ResolveMergeFields( mergeFields )
                },

                new OnboardScreenSettings
                {
                    Screen = OnboardScreen.CodeSent,
                    Title = CodeSentScreenTitle.ResolveMergeFields( mergeFields ),
                    Subtitle = CodeSentScreenSubtitle.ResolveMergeFields( mergeFields )
                },

                new OnboardScreenSettings
                {
                    Screen = OnboardScreen.Name,
                    Title = NameScreenTitle.ResolveMergeFields( mergeFields ),
                    Subtitle = NameScreenSubtitle.ResolveMergeFields( mergeFields )
                },

                new OnboardScreenSettings
                {
                    Screen = OnboardScreen.PersonalInformation,
                    Title = PersonalInformationScreenTitle.ResolveMergeFields( mergeFields ),
                    Subtitle = PersonalInformationScreenSubtitle.ResolveMergeFields( mergeFields )
                },

                new OnboardScreenSettings
                {
                    Screen = OnboardScreen.ContactInformation,
                    Title = ContactInformationScreenTitle.ResolveMergeFields( mergeFields ),
                    Subtitle = ContactInformationScreenSubtitle.ResolveMergeFields( mergeFields )
                },

                new OnboardScreenSettings
                {
                    Screen = OnboardScreen.Interests,
                    Title = InterestsScreenTitle.ResolveMergeFields( mergeFields ),
                    Subtitle = InterestsScreenSubtitle.ResolveMergeFields( mergeFields )
                },

                new OnboardScreenSettings
                {
                    Screen = OnboardScreen.Notifications,
                    Title = NotificationsScreenTitle.ResolveMergeFields( mergeFields ),
                    Subtitle = NotificationsScreenSubtitle.ResolveMergeFields( mergeFields )
                },

                new OnboardScreenSettings
                {
                    Screen = OnboardScreen.Campus,
                    Title = CampusScreenTitle.ResolveMergeFields( mergeFields ),
                    Subtitle = CampusScreenSubtitle.ResolveMergeFields( mergeFields )
                },

                new OnboardScreenSettings
                {
                    Screen = OnboardScreen.CreateLogin,
                    Title = CreateLoginScreenTitle.ResolveMergeFields( mergeFields ),
                    Subtitle = CreateLoginScreenSubtitle.ResolveMergeFields( mergeFields )
                }
            };
        }

        /// <summary>
        /// Gets the initial values for the person object.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns>The details to pass to the mobile application.</returns>
        private OnboardDetails GetInitialValues( Person person )
        {
            int mobileNumberValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;

            return new OnboardDetails
            {
                FirstName = person?.FirstName,
                LastName = person?.LastName,
                BirthDate = person?.BirthDate,
                CampusGuid = person?.GetCampus()?.Guid,
                Email = person?.Email,
                MobilePhone = person?.PhoneNumbers.FirstOrDefault( a => a.NumberTypeValueId == mobileNumberValueId ).ToStringSafe(),
                Gender = person?.Gender.ToMobile(),
                Interests = new List<string>()
            };
        }

        /// <summary>
        /// Converts to mobile Gender value to the web version.
        /// </summary>
        /// <param name="gender">The mobile gender.</param>
        /// <returns>The web gender value.</returns>
        private Rock.Model.Gender? ToWeb( Rock.Common.Mobile.Enums.Gender? gender )
        {
            if ( !gender.HasValue )
            {
                return null;
            }

            switch ( gender.Value )
            {
                case Common.Mobile.Enums.Gender.Male:
                    return Rock.Model.Gender.Male;

                case Common.Mobile.Enums.Gender.Female:
                    return Rock.Model.Gender.Female;

                default:
                    return Rock.Model.Gender.Unknown;
            }
        }

        /// <summary>
        /// Creates the new person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="details">The details.</param>
        /// <returns>The <see cref="Person"/> record that was created.</returns>
        private Person CreateNewPerson( OnboardDetails details, RockContext rockContext )
        {
            var dvcConnectionStatus = DefinedValueCache.Get( DefaultConnectionStatusGuid );
            var dvcRecordStatus = DefinedValueCache.Get( DefaultRecordStatusGuid );

            var person = new Person
            {
                FirstName = details.FirstName,
                LastName = details.LastName,
                Email = details.Email,
                Gender = ToWeb( details.Gender ) ?? Rock.Model.Gender.Unknown,
                IsEmailActive = true,
                EmailPreference = Rock.Model.EmailPreference.EmailAllowed,
                RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id,
                ConnectionStatusValueId = dvcConnectionStatus?.Id,
                RecordStatusValueId = dvcRecordStatus?.Id
            };

            if ( details.BirthDate.HasValue )
            {
                // Special case for birth dates. They don't get time zone
                // offsets applied, so just take the raw date value.
                var absBirthDate = details.BirthDate.Value.Date;

                person.BirthMonth = absBirthDate.Month;
                person.BirthDay = absBirthDate.Day;
                if ( absBirthDate.Year != DateTime.MinValue.Year )
                {
                    person.BirthYear = absBirthDate.Year;
                }
            }

            if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( details.MobilePhone ) ) )
            {
                int phoneNumberTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id;

                var phoneNumber = new PhoneNumber
                {
                    NumberTypeValueId = phoneNumberTypeId,
                    Number = PhoneNumber.CleanNumber( details.MobilePhone )
                };
                person.PhoneNumbers.Add( phoneNumber );
            }

            int? campusId = null;

            if ( details.CampusGuid.HasValue )
            {
                campusId = CampusCache.Get( details.CampusGuid.Value )?.Id;
            }

            PersonService.SaveNewPerson( person, rockContext, campusId, false );

            return person;
        }

        /// <summary>
        /// Updates the existing person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="details">The details.</param>
        /// <param name="rockContext">The rock context.</param>
        private void UpdateExistingPerson( Person person, OnboardDetails details, RockContext rockContext )
        {
            // Update the BirthDate value if they provided one. The user
            // will always provide a year to this method.
            if ( details.BirthDate.HasValue )
            {
                // Special case for birth dates. They don't get time zone
                // offsets applied, so just take the raw date value.
                var absBirthDate = details.BirthDate.Value.Date;

                person.BirthDay = absBirthDate.Day;
                person.BirthMonth = absBirthDate.Month;
                person.BirthYear = absBirthDate.Year;
            }
            else
            {
                person.BirthDay = null;
                person.BirthMonth = null;
                person.BirthYear = null;
            }

            // Check if they entered a mobile number and if we have on
            // already that needs to be updated/removed.
            int phoneNumberTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id;
            var existingPhoneNumber = person.PhoneNumbers.FirstOrDefault( a => a.NumberTypeValueId == phoneNumberTypeId );
            var cleanMobilePhone = PhoneNumber.CleanNumber( details.MobilePhone );

            if ( !string.IsNullOrWhiteSpace( cleanMobilePhone ) )
            {
                if ( existingPhoneNumber == null )
                {
                    var phoneNumber = new PhoneNumber
                    {
                        NumberTypeValueId = phoneNumberTypeId,
                        Number = cleanMobilePhone
                    };

                    person.PhoneNumbers.Add( phoneNumber );
                }
                else
                {
                    existingPhoneNumber.Number = cleanMobilePhone;
                }
            }
            else if ( existingPhoneNumber != null )
            {
                // Remove the existing phone number.
                person.PhoneNumbers.Remove( existingPhoneNumber );
            }

            person.Gender = ToWeb( details.Gender ) ?? Rock.Model.Gender.Unknown;
            person.Email = details.Email;

            var familyGroup = person.GetFamily( rockContext );
            if ( familyGroup != null && details.CampusGuid.HasValue )
            {
                familyGroup.CampusId = CampusCache.Get( details.CampusGuid.Value ).Id;
            }
        }

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="username">The account user name.</param>
        /// <param name="password">The account password.</param>
        /// <param name="confirmed">if set to <c>true</c> the account is considered confirmed.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// The <see cref="UserLogin" /> record that was created.
        /// </returns>
        private UserLogin CreateUser( Person person, string username, string password, bool confirmed, RockContext rockContext )
        {
            return UserLoginService.Create(
                rockContext,
                person,
                AuthenticationServiceType.Internal,
                EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                username,
                password,
                confirmed );
        }

        /// <summary>
        /// Updates the person's interests.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="topicGuids">The topic guids.</param>
        /// <param name="rockContext">The rock context.</param>
        private void UpdatePersonInterests( Person person, IEnumerable<Guid> topicGuids, RockContext rockContext )
        {
            foreach ( var groupGuid in topicGuids )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var group = new GroupService( rockContext ).Get( groupGuid );
                var groupMemberRecordsForPerson = groupMemberService.Queryable()
                    .Where( a => a.GroupId == group.Id && a.PersonId == person.Id ).ToList();

                if ( groupMemberRecordsForPerson.Any() )
                {
                    // normally there would be at most 1 group member record
                    // for the person, but just in case, mark them all
                    foreach ( var groupMember in groupMemberRecordsForPerson )
                    {
                        if ( groupMember.GroupMemberStatus == GroupMemberStatus.Inactive )
                        {
                            groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                            if ( groupMember.Note == "Unsubscribed" )
                            {
                                groupMember.Note = string.Empty;
                            }
                        }
                    }
                }
                else
                {
                    // they are not currently in the Group
                    var groupMember = new GroupMember
                    {
                        PersonId = person.Id,
                        GroupId = group.Id
                    };

                    int? defaultGroupRoleId = GroupTypeCache.Get( group.GroupTypeId ).DefaultGroupRoleId;
                    if ( defaultGroupRoleId.HasValue )
                    {
                        groupMember.GroupRoleId = defaultGroupRoleId.Value;
                    }
                    else
                    {
                        continue;
                    }

                    groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                    groupMember.CommunicationPreference = Model.CommunicationType.RecipientPreference;

                    if ( groupMember.IsValidGroupMember( rockContext ) )
                    {
                        groupMemberService.Add( groupMember );
                    }
                }
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the merge fields.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="person">The person that was matched.</param>
        /// <returns></returns>
        private Dictionary<string, object> GetMergeFields( string code, Person person )
        {
            // Make the Person available so that if we matched it
            // to a person the e-mail can include something personal
            // when telling them they requested an authentication code.
            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Person", person );
            mergeFields.Add( "Code", code );

            return mergeFields;
        }

        /// <summary>
        /// Sends the email code.
        /// </summary>
        /// <param name="systemCommunication">The system communication.</param>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="code">The code.</param>
        /// <param name="person">The person that probably owns this email.</param>
        /// <returns><c>true</c> if the code sent; otherwise <c>false</c>.</returns>
        private bool SendEmailCode( SystemCommunication systemCommunication, string emailAddress, string code, Person person )
        {
            var recipients = new List<RockEmailMessageRecipient>
            {
                RockEmailMessageRecipient.CreateAnonymous( emailAddress, GetMergeFields( code, person ) )
            };

            var emailMessage = new RockEmailMessage( systemCommunication );
            emailMessage.SetRecipients( recipients );
            emailMessage.CreateCommunicationRecord = false;

            return emailMessage.Send( out _ );
        }

        /// <summary>
        /// Sends the SMS code.
        /// </summary>
        /// <param name="systemCommunication">The system communication.</param>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="code">The code.</param>
        /// <param name="person">The person that probably owns this number.</param>
        /// <returns><c>true</c> if the code sent; otherwise <c>false</c>.</returns>
        private bool SendSmsCode( SystemCommunication systemCommunication, string phoneNumber, string code, Person person )
        {
            var recipients = new List<RockSMSMessageRecipient>
            {
                RockSMSMessageRecipient.CreateAnonymous( phoneNumber, GetMergeFields( code, person ) )
            };

            var smsMessage = new RockSMSMessage( systemCommunication );
            smsMessage.SetRecipients( recipients );
            smsMessage.CreateCommunicationRecord = false;

            return smsMessage.Send( out _ );
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Sends a one-time use verification code to the email or phone number.
        /// </summary>
        /// <param name="sendSms"><c>true</c> if the code should be sent via SMS.</param>
        /// <param name="sendEmail"><c>true</c> if the code should be sent via e-mail.</param>
        /// <param name="phoneNumber">The phone number to send the code to.</param>
        /// <param name="email">The e-mail address to send the code to.</param>
        /// <returns>A <see cref="SendCodeResponse"/> object or an error message.</returns>
        /// <remarks>This can be removed once all mobile apps are on shell v3 or later.</remarks>
        [RockObsolete( "1.13" )]
        [Obsolete]
        [BlockAction( "SendCode" )]
        public BlockActionResult SendCodeLegacy( bool sendSms, bool sendEmail, string phoneNumber, string email )
        {
            var request = new SendCodeRequest
            {
                SendSms = sendSms,
                SendEmail = sendEmail,
                PhoneNumber = phoneNumber,
                Email = email
            };

            return SendCode( request );
        }

        /// <summary>
        /// Sends a one-time use verification code to the email or phone number.
        /// </summary>
        /// <param name="request">The code request.</param>
        /// <returns>A <see cref="SendCodeResponse"/> object or an error message.</returns>
        [BlockAction]
        public BlockActionResult SendCode( SendCodeRequest request )
        {
            using ( var rockContext = new RockContext() )
            {
                var identityVerificationService = new IdentityVerificationService( rockContext );
                var systemCommunication = new SystemCommunicationService( rockContext ).Get( this.SystemCommunicationGuid ?? Guid.Empty );
                Person person;
                string refNumber;

                if ( systemCommunication == null )
                {
                    return ActionInternalServerError( "Invalid configuration." );
                }

                // Determine if we are sending an email or an SMS.
                if ( request.SendEmail && request.Email.IsNotNullOrWhiteSpace() )
                {
                    refNumber = request.Email;
                    var people = new PersonService( rockContext ).Queryable()
                        .AsNoTracking()
                        .Where( a => a.Email == request.Email )
                        .ToList();

                    // Only a single match is taken as a quality match. If
                    // there were more than 1 then we don't match.
                    person = people.Count == 1 ? people[0] : null;
                }
                else if ( request.SendSms && request.PhoneNumber.IsNotNullOrWhiteSpace() )
                {
                    refNumber = request.PhoneNumber;

                    var personIds = new PhoneNumberService( rockContext )
                        .GetPersonIdsByNumber( request.PhoneNumber ).ToList();

                    // Only a single match is taken as a quality match. If
                    // there were more than 1 then we don't match.
                    person = personIds.Count == 1 ? new PersonService( rockContext ).Get( personIds[0] ) : null;
                }
                else
                {
                    return ActionBadRequest( "Missing required information to send code." );
                }

                // If we have a matching person, check if it is a protected
                // profile account that must not be used.
                if ( person != null )
                {
                    if ( DisableMatchingProtectionProfiles.Contains( person.AccountProtectionProfile ) )
                    {
                        return ActionBadRequest( "It appears you have an account in our system that has security access which requires you to log in with a username and password." );
                    }
                }

                // Generate the identify verification code that the user will need to enter.
                var identityVerification = identityVerificationService.CreateIdentityVerificationRecord( RequestContext.ClientInformation.IpAddress, IPThrottleLimit, refNumber );
                bool success = false;

                if ( request.SendEmail && request.Email.IsNotNullOrWhiteSpace() )
                {
                    success = SendEmailCode( systemCommunication, request.Email, identityVerification.IdentityVerificationCode.Code, person );
                }
                else if ( request.SendSms && request.PhoneNumber.IsNotNullOrWhiteSpace() )
                {
                    success = SendSmsCode( systemCommunication, request.PhoneNumber, identityVerification.IdentityVerificationCode.Code, person );
                }

                // It's not much, but give the user some idea of what is going
                // on in the event of an error.
                if ( !success )
                {
                    return ActionInternalServerError( "Unable to send code." );
                }

                // Create our encrypted state to track where we are in the process.
                var state = new EncryptedState
                {
                    IdentityVerificationId = identityVerification.Id,
                    MatchedPersonId = person?.Id
                };

                return ActionOk( new SendCodeResponse
                {
                    IsSuccess = true,
                    State = Rock.Security.Encryption.EncryptString( state.ToJson() )
                } );
            }
        }

        /// <summary>
        /// Verifies the code entered by the user.
        /// </summary>
        /// <param name="state">The custom state data that was sent to the client.</param>
        /// <param name="code">The code entered by the individual.</param>
        /// <returns>A <see cref="VerifyCodeResponse"/> object or an error message.</returns>
        /// <remarks>This can be removed once all mobile apps are on shell v3 or later.</remarks>
        [RockObsolete( "1.13" )]
        [Obsolete]
        [BlockAction( "VerifyCode" )]
        public BlockActionResult VerifyCodeLegacy( string state, string code )
        {
            var request = new VerifyCodeRequest
            {
                State = state,
                Code = code
            };

            return VerifyCode( request );
        }

        /// <summary>
        /// Verifies the code entered by the user.
        /// </summary>
        /// <param name="request">The code request.</param>
        /// <returns>A <see cref="VerifyCodeResponse"/> object or an error message.</returns>
        [BlockAction]
        public BlockActionResult VerifyCode( VerifyCodeRequest request )
        {
            using ( var rockContext = new RockContext() )
            {
                var identityVerificationService = new IdentityVerificationService( rockContext );
                var state = Rock.Security.Encryption.DecryptString( request.State ).FromJsonOrThrow<EncryptedState>();

                bool isCodeValid = identityVerificationService.VerifyIdentityVerificationCode( state.IdentityVerificationId, VerificationTimeLimit, request.Code, ValidationCodeAttempts );

                if ( !isCodeValid )
                {
                    return new BlockActionResult( System.Net.HttpStatusCode.Unauthorized )
                    {
                        Error = "Unable to verify your code."
                    };
                }

                Person person = null;

                if ( state.MatchedPersonId.HasValue )
                {
                    person = new PersonService( rockContext ).Get( state.MatchedPersonId.Value );
                }

                return ActionOk( new VerifyCodeResponse
                {
                    State = Rock.Security.Encryption.EncryptString( state.ToJson() ),
                    CreateLogin = person == null ? true : !person.Users.Any( a => ( a.IsConfirmed ?? true ) && !( a.IsLockedOut ?? false ) ),
                    InitialValues = GetInitialValues( person )
                } );
            }
        }

        /// <summary>
        /// Attempts to perform final log in of the person.
        /// </summary>
        /// <param name="state">The custom state data that was sent to the client.</param>
        /// <param name="personalDeviceGuid">The personal device unique identifier that the client has been assigned.</param>
        /// <param name="details">The details that the individual filled out.</param>
        /// <returns>A <see cref="CreatePersonResponse"/> that contains the log in result or an error object.</returns>
        /// <remarks>This can be removed once all mobile apps are on shell v3 or later.</remarks>
        [RockObsolete( "1.13" )]
        [Obsolete]
        [BlockAction( "CreatePerson" )]
        public BlockActionResult CreatePersonLegacy( string state, Guid? personalDeviceGuid, OnboardDetails details )
        {
            var request = new CreatePersonRequest
            {
                State = state,
                PersonalDeviceGuid = personalDeviceGuid,
                Details = details
            };

            return CreatePerson( request );
        }

        /// <summary>
        /// Attempts to perform final log in of the person.
        /// </summary>
        /// <param name="request">The details of the request.</param>
        /// <returns>A <see cref="CreatePersonResponse"/> that contains the log in result or an error object.</returns>
        [BlockAction]
        public BlockActionResult CreatePerson( CreatePersonRequest request )
        {
            using ( var rockContext = new RockContext() )
            {
                string username = null;
                Person person = null;
                var siteCache = PageCache.Layout.Site;
                var personService = new PersonService( rockContext );
                var state = Rock.Security.Encryption.DecryptString( request.State ).FromJsonOrThrow<EncryptedState>();

                if ( state.MatchedPersonId.HasValue )
                {
                    person = personService.Get( state.MatchedPersonId.Value );
                }

                // If we will be creating a login later then verify those
                // values now before we start creating the person.
                if ( request.Details.UserName.IsNotNullOrWhiteSpace() )
                {
                    if ( !UserLoginService.IsPasswordValid( request.Details.Password ) )
                    {
                        return ActionBadRequest( UserLoginService.FriendlyPasswordRules() );
                    }

                    var userLoginService = new UserLoginService( rockContext );
                    var userLogin = userLoginService.GetByUserName( request.Details.UserName );
                    if ( userLogin != null )
                    {
                        return ActionBadRequest( "An account with that name already exists." );
                    }

                    username = request.Details.UserName;
                }

                // Check if the person is the same. This is calculated by
                // checking if the FirstName and LastName have been modified.
                // Also if the database record has a BirthDate already and
                // it has been changed then the record is no longer the same.
                // Also if the database record has a valid Gender and it has
                // been changed then the record is no longer the same.
                bool isSamePerson = person != null;

                if ( isSamePerson && ( person.FirstName != request.Details.FirstName || person.LastName != request.Details.LastName ) )
                {
                    isSamePerson = false;
                }

                if ( isSamePerson && person.Gender != Rock.Model.Gender.Unknown && person.Gender != ToWeb( request.Details.Gender ) )
                {
                    isSamePerson = false;
                }

                if ( isSamePerson && person.BirthDate.HasValue && person.BirthDate.Value != request.Details.BirthDate?.Date )
                {
                    isSamePerson = false;
                }

                if ( isSamePerson && username == null )
                {
                    username = person.Users.FirstOrDefault( a => ( a.IsConfirmed ?? true ) && !( a.IsLockedOut ?? false ) )?.UserName;
                }

                try
                {
                    // Run in a transaction so we don't create a person but then
                    // fail on the login account and end up with duplicate person
                    // records later.
                    rockContext.WrapTransaction( () =>
                    {
                        // Either create a new person or update the existing one.
                        if ( !isSamePerson )
                        {
                            person = CreateNewPerson( request.Details, rockContext );
                        }
                        else
                        {
                            UpdateExistingPerson( person, request.Details, rockContext );
                        }

                        if ( request.Details.UserName.IsNotNullOrWhiteSpace() )
                        {
                            CreateUser( person, request.Details.UserName, request.Details.Password, true, rockContext );
                        }
                        else if ( username == null )
                        {
                            // No existing username, no manually created username.
                            // We need to generate a user so that we can properly
                            // log them in.
                            var newPassword = Password.GeneratePassword();
                            username = Rock.Security.Authentication.Database.GenerateUsername( person.NickName, person.LastName );

                            CreateUser( person, username, newPassword, true, rockContext );
                        }

                        if ( request.Details.Interests.Any() )
                        {
                            UpdatePersonInterests( person, request.Details.Interests.AsGuidList(), rockContext );
                        }

                        // Update the personal device to indicate it's owned by this
                        // person.
                        if ( request.PersonalDeviceGuid.HasValue )
                        {
                            var personalDevice = new PersonalDeviceService( rockContext ).Get( request.PersonalDeviceGuid.Value );

                            if ( personalDevice != null )
                            {
                                personalDevice.PersonAliasId = person.PrimaryAliasId;
                                if ( ShowNotificationsRequest )
                                {
                                    personalDevice.DeviceRegistrationId = request.Details.PushToken;
                                    personalDevice.NotificationsEnabled = request.Details.PushToken.IsNotNullOrWhiteSpace();
                                }

                                rockContext.SaveChanges();
                            }
                        }
                    } );

                    var mobilePerson = MobileHelper.GetMobilePerson( person, siteCache );

                    // Set the authentication token to either a normal token so
                    // they can log in.
                    mobilePerson.AuthToken = MobileHelper.GetAuthenticationToken( username );

                    return ActionOk( new CreatePersonResponse
                    {
                        IsSuccess = true,
                        Person = mobilePerson
                    } );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );

                    return ActionOk( new CreatePersonResponse
                    {
                        IsSuccess = false,
                        Message = "We were unable to complete your request at this time."
                    } );
                }
            }
        }

        /// <summary>
        /// Updates the current person and returns the person's details.
        /// </summary>
        /// <param name="details">The details to be updated.</param>
        /// <param name="personalDeviceGuid">The personal device unique identifier.</param>
        /// <returns>The result of the block action.</returns>
        [BlockAction]
        public BlockActionResult UpdateCurrentPerson( UpdateCurrentPersonDetails details, Guid? personalDeviceGuid )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionUnauthorized( "Must be logged in to perform this action." );
            }

            using ( var rockContext = new RockContext() )
            {
                var person = new PersonService( rockContext ).Get( RequestContext.CurrentPerson.Id );
                var username = RequestContext.CurrentUser.UserName;

                if ( username == null )
                {
                    username = person.Users.FirstOrDefault( a => ( a.IsConfirmed ?? true ) && !( a.IsLockedOut ?? false ) )?.UserName;
                }

                rockContext.WrapTransaction( () =>
                {
                    // Update the campus if we can.
                    if ( details.CampusGuid != null )
                    {
                        var campusId = CampusCache.Get( details.CampusGuid.Value )?.Id;

                        if ( campusId.HasValue )
                        {
                            person.GetFamily( rockContext ).CampusId = campusId;
                        }
                    }

                    if ( details.Interests.Any() )
                    {
                        UpdatePersonInterests( person, details.Interests.AsGuidList(), rockContext );
                    }

                    // Update the personal device to indicate it's owned by this
                    // person.
                    if ( personalDeviceGuid.HasValue )
                    {
                        var personalDevice = new PersonalDeviceService( rockContext ).Get( personalDeviceGuid.Value );

                        if ( personalDevice != null )
                        {
                            personalDevice.PersonAliasId = person.PrimaryAliasId;
                            if ( ShowNotificationsRequest )
                            {
                                personalDevice.DeviceRegistrationId = details.PushToken;
                                personalDevice.NotificationsEnabled = details.PushToken.IsNotNullOrWhiteSpace();
                            }
                        }
                    }

                    if ( username == null )
                    {
                        // No existing username. We need to generate a user
                        // so that we can properly log them in.
                        var newPassword = Password.GeneratePassword();
                        username = Rock.Security.Authentication.Database.GenerateUsername( person.NickName, person.LastName );

                        CreateUser( person, username, newPassword, true, rockContext );
                    }

                    rockContext.SaveChanges();
                } );

                var mobilePerson = MobileHelper.GetMobilePerson( person, PageCache.Layout.Site );

                // Set the authentication token so they get/stay logged in.
                mobilePerson.AuthToken = MobileHelper.GetAuthenticationToken( username );

                return ActionOk( new
                {
                    Person = mobilePerson
                } );
            }
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Encrypted state that is sent to the client to ensure
        /// no tampering happens.
        /// </summary>
        private class EncryptedState
        {
            /// <summary>
            /// Gets or sets the identity verification identifier.
            /// </summary>
            /// <value>
            /// The identity verification identifier.
            /// </value>
            public int IdentityVerificationId { get; set; }

            /// <summary>
            /// Gets or sets the matched person identifier.
            /// </summary>
            /// <value>
            /// The matched person identifier.
            /// </value>
            public int? MatchedPersonId { get; set; }
        }

        #endregion

        #region Action Classes

        /// <summary>
        /// The data that should be updated on the person's record for the
        /// UpdateCurrentPerson action.
        /// </summary>
        public class UpdateCurrentPersonDetails
        {
            /// <summary>
            /// Gets or sets the campus unique identifier.
            /// </summary>
            /// <value>
            /// The campus unique identifier.
            /// </value>
            public Guid? CampusGuid { get; set; }

            /// <summary>
            /// Gets or sets the interests.
            /// </summary>
            /// <value>
            /// The interests.
            /// </value>
            public List<string> Interests { get; set; }

            /// <summary>
            /// Gets or sets the push notification token.
            /// </summary>
            /// <value>
            /// The push notification token.
            /// </value>
            public string PushToken { get; set; }
        }

        #endregion
    }
}
