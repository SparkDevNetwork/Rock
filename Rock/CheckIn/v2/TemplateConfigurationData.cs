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
using System.Linq;
using System.Text.RegularExpressions;

using Rock.Data;
using Rock.Enums.CheckIn;
using Rock.Enums.Controls;
using Rock.SystemKey;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// This provides the server check-in configuration data for a single
    /// check-in template group type. This should not be sent down to clients
    /// as it contains additional data they should not see.
    /// </summary>
    internal class TemplateConfigurationData
    {
        #region Configuration Properties

        /// <summary>
        /// Gets a value that determines how check-in should gather the
        /// ability level of the current individual.
        /// </summary>
        /// <value>How check-in should gather the ability level.</value>
        public virtual AbilityLevelDeterminationMode AbilityLevelDetermination { get; }

        /// <summary>
        /// Gets the achievement type unique identifiers that will be used
        /// for check-in celebrations.
        /// </summary>
        /// <value>The achievement type unique identifiers.</value>
        public virtual IReadOnlyCollection<Guid> AchievementTypeGuids { get; }

        /// <summary>
        /// Defines the age restriction for this check-in configuration. This applies
        /// a filter when considering which people can be listed on the family
        /// member selection screen. Using one of the Hide modes will hide those
        /// people from the screen even if there is a valid opportunity for them to
        /// check into.
        /// </summary>
        public virtual AgeRestrictionMode AgeRestriction { get; }

        /// <summary>
        /// Gets a value indicating whether groups not marked as special needs
        /// should be removed from a person's opportunity list if the person
        /// <strong>is</strong> marked as special needs.
        /// </summary>
        public virtual bool AreNonSpecialNeedsGroupsRemoved { get; }

        /// <summary>
        /// Gets a value indicating whether groups marked as special needs
        /// should be removed from a person's opportunity list if the person
        /// is <strong>not</strong> marked as special needs.
        /// </summary>
        public virtual bool AreSpecialNeedsGroupsRemoved { get; }

        /// <summary>
        /// Gets the number of days back the AutoSelect feature will use to
        /// determine automatic selections. A value of <c>0</c> will disable.
        /// </summary>
        /// <value>The number of days back.</value>
        public virtual int AutoSelectDaysBack { get; }

        /// <summary>
        /// Gets the options that should be automatically selected if an
        /// individual has previously checked-in.
        /// </summary>
        /// <value>The options to automatically select.</value>
        public virtual AutoSelectMode AutoSelect { get; }

        /// <summary>
        /// Gets the type of check-in experience to use. Family check-in allows
        /// more than one person in the family to be checked in at a time.
        /// </summary>
        /// <value>The type of check-in experience to use.</value>
        public virtual KioskCheckInMode KioskCheckInType { get; }

        /// <summary>
        /// Gets or sets the type of the family search configured for
        /// the configuration.
        /// </summary>
        /// <value>The type of the family search.</value>
        public virtual FamilySearchMode FamilySearchType { get; }

        /// <summary>
        /// The matching behavior that will be used when matching on Grade
        /// and Age (Age Range and Birthdate Range) for groups.
        /// </summary>
        public virtual GradeAndAgeMatchingMode GradeAndAgeMatchingBehavior { get; }

        /// <summary>
        /// Gets a value indicating whether age is required for check-in.
        /// If an area and/or group has an age requirement, enabling this
        /// option will prevent people without an age from checking in to
        /// that area or group.
        /// </summary>
        /// <value><c>true</c> if age is required for check-in; otherwise, <c>false</c>.</value>
        public virtual bool IsAgeRequired { get; }

        /// <summary>
        /// Gets a value indicating whether self checkout is allowed at the kiosk.
        /// </summary>
        /// <value><c>true</c> if self checkout is allowed at the kiosk; otherwise, <c>false</c>.</value>
        public virtual bool IsCheckoutAtKioskAllowed { get; }

        /// <summary>
        /// Gets a value indicating whether checkout is allowed in the manager.
        /// </summary>
        /// <value><c>true</c> if checkout is allowed in the manager; otherwise, <c>false</c>.</value>
        public virtual bool IsCheckoutInManagerAllowed { get; }

        /// <summary>
        /// Gets a value indicating whether to prevent people from being
        /// checked in to a specific schedule more than once.
        /// </summary>
        /// <value><c>true</c> if duplicate check-ins are prevented; otherwise, <c>false</c>.</value>
        public virtual bool IsDuplicateCheckInPrevented { get; }

        /// <summary>
        /// Gets a value indicating whether a grade is required for check-in.
        /// If an area and/or group has a grade requirement, enabling this
        /// option will prevent people without a grade from checking in to
        /// that area or group.
        /// </summary>
        /// <value><c>true</c> if grade is required; otherwise, <c>false</c>.</value>
        public virtual bool IsGradeRequired { get; }

        /// <summary>
        /// Gets or sets a value indicating whether inactive people should
        /// be excluded from the check-in process.
        /// </summary>
        /// <value><c>true</c> if inactive people should be excluded; otherwise, <c>false</c>.</value>
        public virtual bool IsInactivePersonExcluded { get; }

        /// <summary>
        /// Gets a value indicating whether the current location occupancy
        /// counts should be displayed when selecting a location.
        /// </summary>
        /// <value><c>true</c> if current location occupancy counts should be displayed; otherwise, <c>false</c>.</value>
        public virtual bool IsLocationCountDisplayed { get; }

        /// <summary>
        /// Gets a value indicating whether the numeric only portion of the
        /// security code is randomized. Alpha and alpha-numeric portions are
        /// always randomized.
        /// </summary>
        /// <value><c>true</c> if numeric only portion of the security code is randomized; otherwise, <c>false</c>.</value>
        public virtual bool IsNumericSecurityCodeRandom { get; }

        /// <summary>
        /// Gets a value indicating whether an override option is available in
        /// the kiosk supervisor screen. This allows an authorized support
        /// person to bypass check-in requirements.
        /// </summary>
        /// <value><c>true</c> if the kiosk override option is available; otherwise, <c>false</c>.</value>
        public virtual bool IsOverrideAvailable { get; }

        /// <summary>
        /// Gets a value indicating whether individual photos should be hidden
        /// on public kiosks.
        /// </summary>
        /// <value><c>true</c> if photos should be hidden; otherwise, <c>false</c>.</value>
        public virtual bool IsPhotoHidden { get; }

        /// <summary>
        /// Gets a value indicating whether the presence system is enabled.
        /// When enabled a checked in person will not be marked as present
        /// automatically.
        /// </summary>
        /// <value><c>true</c> if the presence system is enabled; otherwise, <c>false</c>.</value>
        public virtual bool IsPresenceEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether removing people with a "can check-in"
        /// relationship from the family is allowed. This does not allow
        /// full family members to be removed.
        /// </summary>
        /// <value><c>true</c> if can check-in relationship can be removed; otherwise, <c>false</c>.</value>
        public virtual bool IsRemoveFromFamilyAtKioskAllowed { get; }

        /// <summary>
        /// Gets a value indicating whether the same security code should be
        /// re-used for all people checked in during a single family check-in
        /// session.
        /// </summary>
        /// <value><c>true</c> if the same security could should be used for a single family check-in session; otherwise, <c>false</c>.</value>
        public virtual bool IsSameCodeUsedForFamily { get; }

        /// <summary>
        /// Gets a value indicating whether to attempt to use the same options
        /// from the first service when a person is checking into more than one
        /// service schedule.
        /// </summary>
        /// <value><c>true</c> if the same options from the first service will be used; otherwise, <c>false</c>.</value>
        public virtual bool IsSameOptionUsed { get; }

        /// <summary>
        /// Gets a value indicating whether the supervisor screen is available
        /// to kiosks after entering a pin number.
        /// </summary>
        /// <value><c>true</c> if the supervisor screen is available; otherwise, <c>false</c>.</value>
        public virtual bool IsSupervisorEnabled { get; }

        /// <summary>
        /// Gets or sets the maximum number of family search results.
        /// </summary>
        /// <value>The maximum number of family search results.</value>
        public virtual int? MaximumNumberOfResults { get; }

        /// <summary>
        /// Gets or sets the maximum length of the phone number during
        /// family search.
        /// </summary>
        /// <value>The maximum length of the phone number.</value>
        public virtual int? MaximumPhoneNumberLength { get; }

        /// <summary>
        /// Gets or sets the minimum length of the phone number during
        /// family search.
        /// </summary>
        /// <value>The minimum length of the phone number.</value>
        public virtual int? MinimumPhoneNumberLength { get; }

        /// <summary>
        /// Gets or sets the type of the phone search used in family search.
        /// </summary>
        /// <value>The type of the phone search used in family search.</value>
        public virtual PhoneSearchMode PhoneSearchType { get; }

        /// <summary>
        /// The unique identifier of the content channel that will provide
        /// the promotions displayed on the welcome screen.
        /// </summary>
        public virtual Guid? PromotionContentChannelGuid { get; }

        /// <summary>
        /// Gets the kiosk refresh interval. This is used by some kiosks to
        /// determine how often, in seconds, they will check with the server
        /// to see if any configuration data has changed.
        /// </summary>
        /// <value>The kiosk refresh interval in seconds.</value>
        public virtual int RefreshInterval { get; }

        /// <summary>
        /// Gets the regular expression filter that will be run against the
        /// search term before a phone number search is performed.
        /// </summary>
        /// <value>The regular expression filter.</value>
        public virtual string PhoneNumberPattern { get; }

        /// <summary>
        /// Gets the regular expression that will be run against the search
        /// term before a phone number search is performed. If it matches then
        /// the phone number will be replaced with the first match group.
        /// </summary>
        /// <value>The regular expression for phone number searches.</value>
        public Regex PhoneNumberRegex { get; }

        /// <summary>
        /// Gets the length of the alpha character sequence for the security
        /// codes generated during check-in.
        /// </summary>
        /// <value>The length of the security code alpha character sequence.</value>
        public virtual int SecurityCodeAlphaLength { get; }

        /// <summary>
        /// Gets the length of the alpha-numeric character sequence for the
        /// security codes generated during check-in.
        /// </summary>
        /// <value>The length of the security code alpha-numeric character sequence.</value>
        public virtual int SecurityCodeAlphaNumericLength { get; }

        /// <summary>
        /// Gets the length of the numeric character sequence for the security
        /// codes generated during check-in.
        /// </summary>
        /// <value>The length of the security code numeric character sequence.</value>
        public virtual int SecurityCodeNumericLength { get; }

        /// <summary>
        /// Gets a value that determines how the custom success lava template
        /// will be displayed at the end of the check-in session.
        /// </summary>
        /// <value>A value that determines how the custom success lava tempalte will be displayed.</value>
        public virtual Enums.CheckIn.SuccessLavaTemplateDisplayMode SuccessLavaTemplateDisplay { get; }

        #endregion

        #region Lava Template Properties

        /// <summary>
        /// Gets the legacy ability level select header lava template. This is
        /// used by the WebForms blocks to render the page header on the
        /// Ability Level Select screen.
        /// </summary>
        /// <value>The ability level select header lava template.</value>
        public virtual string AbilityLevelSelectHeaderLavaTemplate { get; }

        /// <summary>
        /// Gets the legacy action select header lava template. This is used
        /// by the WebForms blocks to render the page header on the
        /// Action Select screen where you pick if you want to check-in another
        /// person or check-out somebody already checked in.
        /// </summary>
        /// <value>The action select header lava template.</value>
        public virtual string ActionSelectHeaderLavaTemplate { get; }

        /// <summary>
        /// Gets the legacy checkout person select header lava template. This
        /// is used by the WebForms blocks to render the page header on the
        /// Checkout Person Select screen.
        /// </summary>
        /// <value>The checkout person select header lava template.</value>
        public virtual string CheckoutPersonSelectHeaderLavaTemplate { get; }

        /// <summary>
        /// Gets the legacy family select button lava template. This is used
        /// by the WebForms blocks to render the entire button that represents
        /// each family on the Family Select screen.
        /// </summary>
        /// <value>The family select button lava template.</value>
        public virtual string FamilySelectButtonLavaTemplate { get; }

        /// <summary>
        /// Gets the legacy group select header lava template. This is
        /// used by the WebForms blocks to render the page header on the
        /// Group Select screen.
        /// </summary>
        /// <value>The group select header lava template.</value>
        public virtual string GroupSelectHeaderLavaTemplate { get; }

        /// <summary>
        /// Gets the legacy group type select header lava template. This is
        /// used by the WebForms blocks to render the page header on the
        /// Group Type Select screen.
        /// </summary>
        /// <value>The group type select header lava template.</value>
        public virtual string GroupTypeSelectHeaderLavaTemplate { get; }

        /// <summary>
        /// Gets the legacy location select header lava template. This is
        /// used by the WebForms blocks to render the page header on the
        /// Location Select screen.
        /// </summary>
        /// <value>The location select header lava template.</value>
        public virtual string LocationSelectHeaderLavaTemplate { get; }

        /// <summary>
        /// Gets the legacy multi-person select header lava template. This is
        /// used by the WebForms blocks to render the page header on the
        /// Multi-Person Select screen.
        /// </summary>
        /// <value>The multi-person select header lava template.</value>
        public virtual string MultiPersonSelectHeaderLavaTemplate { get; }

        /// <summary>
        /// Gets the legacy lava template used by WebForms blocks to render
        /// additional information inside a person button on the Person
        /// Select screen.
        /// </summary>
        /// <value>The lava template used to render additional person information.</value>
        public virtual string PersonSelectAdditionalInformationLavaTemplate { get; }

        /// <summary>
        /// Gets the legacy person select header lava template. This is
        /// used by the WebForms blocks to render the page header on the
        /// Person Select screen.
        /// </summary>
        /// <value>The person select header lava template.</value>
        public virtual string PersonSelectHeaderLavaTemplate { get; }

        /// <summary>
        /// Gets the legacy welcome screen lava template. This is used by
        /// the WebForms blocks to render the "start" buttons on the
        /// Welcome screen.
        /// </summary>
        /// <value>The welcome screen lava template.</value>
        public virtual string StartLavaTemplate { get; }

        /// <summary>
        /// Gets the legacy success lava template used by WebForms blocks.
        /// This will either replace or be appended to the standard success
        /// content depending on the value of <see cref="SuccessLavaTemplateDisplay"/>.
        /// </summary>
        /// <value>The success lava template.</value>
        public virtual string SuccessLavaTemplate { get; }

        /// <summary>
        /// Gets the legacy time select header lava template. This is
        /// used by the WebForms blocks to render the page header on the
        /// Time Select screen.
        /// </summary>
        /// <value>The time select header lava template.</value>
        public virtual string TimeSelectHeaderLavaTemplate { get; }

        #endregion

        #region Registration Properties

        /// <summary>
        /// Gets the workflow type unique identifiers that will be launched when
        /// a new family is added from the kiosk registration screen.
        /// </summary>
        /// <value>The workflow type unique identifiers.</value>
        public virtual IReadOnlyCollection<Guid> AddFamilyWorkflowTypeGuids { get; }

        /// <summary>
        /// Gets the workflow type unique identifiers that will be launched when
        /// a new person is added from the kiosk registration screen.
        /// </summary>
        /// <value>The workflow type unique identifiers.</value>
        public virtual IReadOnlyCollection<Guid> AddPersonWorkflowTypeGuids { get; }

        /// <summary>
        /// Gets the relationship role unique identifiers that will indicate
        /// the child is not part of the family on the kiosk registration
        /// screen.
        /// </summary>
        /// <value>The relationship role unique identifiers.</value>
        public virtual IReadOnlyCollection<Guid> CanCheckInKnownRelationshipRoleGuids { get; }

        /// <summary>
        /// Gets the default person connection status unique identifier when
        /// adding a new person on the kiosk registration screen.
        /// </summary>
        /// <value>The default person connection status unique identifier.</value>
        public virtual Guid DefaultPersonConnectionStatusGuid { get; }

        /// <summary>
        /// Gets a value indicating if the birthdate field is visible and/or
        /// required for adults on the kiosk registration screen.
        /// </summary>
        /// <value>A value indicating how to display the birthdate field for adults.</value>
        public virtual RequirementLevel DisplayBirthdateForAdults { get; }

        /// <summary>
        /// Gets a value indicating if the birthdate field is visible and/or
        /// required for children on the kiosk registration screen.
        /// </summary>
        /// <value>A value indicating how to display the birthdate field for children.</value>
        public virtual RequirementLevel DisplayBirthdateForChildren { get; }

        /// <summary>
        /// Gets a value indicating if the grade field is visible and/or
        /// required for children on the kiosk registration screen.
        /// </summary>
        /// <value>A value indicating how to display the grade field for children.</value>
        public virtual RequirementLevel DisplayGradeForChildren { get; }

        /// <summary>
        /// Gets a value indicating if the ethnicity field is visible and/or
        /// required for adults on the kiosk registration screen.
        /// </summary>
        /// <value>A value indicating how to display the ethnicity field for adults.</value>
        public virtual RequirementLevel DisplayEthnicityForAdults { get; }

        /// <summary>
        /// Gets a value indicating if the ethnicity field is visible and/or
        /// required for children on the kiosk registration screen.
        /// </summary>
        /// <value>A value indicating how to display the ethnicity field for adults.</value>
        public virtual RequirementLevel DisplayEthnicityForChildren { get; }

        /// <summary>
        /// Gets a value indicating if the race field is visible and/or
        /// required for adults on the kiosk registration screen.
        /// </summary>
        /// <value>A value indicating how to display the race field for adults.</value>
        public virtual RequirementLevel DisplayRaceForAdults { get; }

        /// <summary>
        /// Gets a value indicating if the race field is visible and/or
        /// required for children on the kiosk registration screen.
        /// </summary>
        /// <value>A value indicating how to display the race field for children.</value>
        public virtual RequirementLevel DisplayRaceForChildren { get; }

        /// <summary>
        /// Gets a value indicating whether the alternate identifier field
        /// is visible for adults on the kiosk registration screen.
        /// </summary>
        /// <value><c>true</c> if the alternate identifier field is visible; otherwise, <c>false</c>.</value>
        public virtual bool IsAlternateIdFieldVisibleForAdults { get; }

        /// <summary>
        /// Gets a value indicating whether the alternate identifier field
        /// is visible for children on the kiosk registration screen.
        /// </summary>
        /// <value><c>true</c> if the alternate identifier field is visible; otherwise, <c>false</c>.</value>
        public virtual bool IsAlternateIdFieldVisibleForChildren { get; }

        /// <summary>
        /// Gets a value indicating whether check-in should be offered to the
        /// family immediately after kiosk registration has completed.
        /// </summary>
        /// <value><c>true</c> if check-in should be allowed as part of the kiosk registraiton process; otherwise, <c>false</c>.</value>
        public virtual bool IsCheckInAfterRegistrationAllowed { get; }

        /// <summary>
        /// Gets a value indicating whether the SMS button should default to
        /// being checked when adding a new person on the kiosk registration
        /// screen.
        /// </summary>
        /// <value><c>true</c> if the SMS button should default to checked; otherwise, <c>false</c>.</value>
        public virtual bool IsSmsButtonCheckedByDefault { get; }

        /// <summary>
        /// Gets a value indicating whether the SMS button is visible for
        /// mobile phone numbers on the kiosk registration screen.
        /// </summary>
        /// <value><c>true</c> if the SMS button should be visible; otherwise, <c>false</c>.</value>
        public virtual bool IsSmsButtonVisible { get; }

        /// <summary>
        /// Gets the relationship role unique identifiers that will be available
        /// to be selected for a child on the kiosk registration screen.
        /// </summary>
        /// <value>The relationship role unique identifiers.</value>
        public virtual IReadOnlyCollection<Guid> KnownRelationshipRoleGuids { get; }

        /// <summary>
        /// Gets the attribute unique identifiers that will be optional for adults
        /// on the kiosk registration screen.
        /// </summary>
        /// <value>The attribute unique identifiers.</value>
        public virtual IReadOnlyCollection<Guid> OptionalAttributeGuidsForAdults { get; }

        /// <summary>
        /// Gets the attribute unique identifiers that will be optional for children
        /// on the kiosk registration screen.
        /// </summary>
        /// <value>The attribute unique identifiers.</value>
        public virtual IReadOnlyCollection<Guid> OptionalAttributeGuidsForChildren { get; }

        /// <summary>
        /// Gets the attribute unique identifiers that will be optional for families
        /// on the kiosk registration screen.
        /// </summary>
        /// <value>The attribute unique identifiers.</value>
        public virtual IReadOnlyCollection<Guid> OptionalAttributeGuidsForFamilies { get; }

        /// <summary>
        /// Gets the attribute unique identifiers that will be required for adults
        /// on the kiosk registration screen.
        /// </summary>
        /// <value>The attribute unique identifiers.</value>
        public virtual IReadOnlyCollection<Guid> RequiredAttributeGuidsForAdults { get; }

        /// <summary>
        /// Gets the attribute unique identifiers that will be required for children
        /// on the kiosk registration screen.
        /// </summary>
        /// <value>The attribute unique identifiers.</value>
        public virtual IReadOnlyCollection<Guid> RequiredAttributeGuidsForChildren { get; }

        /// <summary>
        /// Gets the attribute unique identifiers that will be required for families
        /// on the kiosk registration screen.
        /// </summary>
        /// <value>The attribute unique identifiers.</value>
        public virtual IReadOnlyCollection<Guid> RequiredAttributeGuidsForFamilies { get; }

        /// <summary>
        /// Gets the relationship role unique identifiers that indicate a person
        /// is in the same family on the kiosk registration screen.
        /// </summary>
        /// <value>The relationship role unique identifiers.</value>
        public virtual IReadOnlyCollection<Guid> SameFamilyKnownRelationshipRoleGuids { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateConfigurationData"/> class.
        /// </summary>
        /// <remarks>
        /// This is intended to be used by unit tests only.
        /// </remarks>
        protected TemplateConfigurationData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateConfigurationData"/> class.
        /// </summary>
        /// <param name="groupTypeCache">The group type cache to get the attribute values from.</param>
        /// <param name="rockContext">The context to use if database access is required to load data from cache.</param>
        internal TemplateConfigurationData( GroupTypeCache groupTypeCache, RockContext rockContext )
        {
            AbilityLevelDetermination = ( AbilityLevelDeterminationMode ) groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ABILITY_LEVEL_DETERMINATION ).AsInteger();
            AchievementTypeGuids = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES ).SplitDelimitedValues().AsGuidList();
            AgeRestriction = ( AgeRestrictionMode ) groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_GROUPTYPE_AGE_RESTRICTION ).AsInteger();
            AreNonSpecialNeedsGroupsRemoved = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_GROUPTYPE_REMOVE_NON_SPECIAL_NEEDS_GROUPS ).AsBoolean();
            AreSpecialNeedsGroupsRemoved = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_GROUPTYPE_REMOVE_SPECIAL_NEEDS_GROUPS ).AsBoolean();
            AutoSelectDaysBack = groupTypeCache.GetAttributeValue( "core_checkin_AutoSelectDaysBack" ).AsInteger();
            AutoSelect = ( AutoSelectMode ) groupTypeCache.GetAttributeValue( "core_checkin_AutoSelectOptions" ).AsInteger();
            KioskCheckInType = groupTypeCache.GetAttributeValue( "core_checkin_CheckInType" ) == "1" ? KioskCheckInMode.Family : KioskCheckInMode.Individual;
            FamilySearchType = GetFamilySearchType( groupTypeCache.GetAttributeValue( "core_checkin_SearchType" ).AsGuid() );
            GradeAndAgeMatchingBehavior = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_GROUPTYPE_GRADE_AND_AGE_MATCHING_BEHAVIOR ).ConvertToEnum<GradeAndAgeMatchingMode>( GradeAndAgeMatchingMode.GradeAndAgeMustMatch );
            IsAgeRequired = groupTypeCache.GetAttributeValue( "core_checkin_AgeRequired" ).AsBoolean( true );
            IsCheckoutAtKioskAllowed = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_KIOSK ).AsBoolean();
            IsCheckoutInManagerAllowed = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER ).AsBoolean();
            IsDuplicateCheckInPrevented = groupTypeCache.GetAttributeValue( "core_checkin_PreventDuplicateCheckin" ).AsBoolean( true );
            IsGradeRequired = groupTypeCache.GetAttributeValue( "core_checkin_GradeRequired" ).AsBoolean( true );
            IsInactivePersonExcluded = groupTypeCache.GetAttributeValue( "core_checkin_PreventInactivePeople" ).AsBoolean( true );
            IsLocationCountDisplayed = groupTypeCache.GetAttributeValue( "core_checkin_DisplayLocationCount" ).AsBoolean( true );
            IsNumericSecurityCodeRandom = groupTypeCache.GetAttributeValue( "core_checkin_SecurityCodeNumericRandom" ).AsBoolean( true );
            IsOverrideAvailable = groupTypeCache.GetAttributeValue( "core_checkin_EnableOverride" ).AsBoolean( true );
            IsPhotoHidden = groupTypeCache.GetAttributeValue( "core_checkin_HidePhotos" ).AsBoolean( true );
            IsPresenceEnabled = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE ).AsBoolean();
            IsRemoveFromFamilyAtKioskAllowed = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_REMOVE_FROM_FAMILY_KIOSK ).AsBoolean();
            IsSameCodeUsedForFamily = groupTypeCache.GetAttributeValue( "core_checkin_ReuseSameCode" ).AsBoolean( false );
            IsSameOptionUsed = groupTypeCache.GetAttributeValue( "core_checkin_UseSameOptions" ).AsBoolean( false );
            IsSupervisorEnabled = groupTypeCache.GetAttributeValue( "core_checkin_EnableManagerOption" ).AsBoolean( true );
            MaximumNumberOfResults = groupTypeCache.GetAttributeValue( "core_checkin_MaxSearchResults" ).AsIntegerOrNull();
            MaximumPhoneNumberLength = groupTypeCache.GetAttributeValue( "core_checkin_MaximumPhoneSearchLength" ).AsIntegerOrNull();
            MinimumPhoneNumberLength = groupTypeCache.GetAttributeValue( "core_checkin_MinimumPhoneSearchLength" ).AsIntegerOrNull();
            PhoneSearchType = ( PhoneSearchMode ) groupTypeCache.GetAttributeValue( "core_checkin_PhoneSearchType" ).AsInteger();
            PromotionContentChannelGuid = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_GROUPTYPE_PROMOTIONS_CONTENT_CHANNEL ).AsGuidOrNull();
            RefreshInterval = groupTypeCache.GetAttributeValue( "core_checkin_RefreshInterval" ).AsInteger();
            PhoneNumberPattern = groupTypeCache.GetAttributeValue( "core_checkin_RegularExpressionFilter" ) ?? string.Empty;
            PhoneNumberRegex = GetRegexOrNull( PhoneNumberPattern );
            SecurityCodeAlphaLength = groupTypeCache.GetAttributeValue( "core_checkin_SecurityCodeAlphaLength" ).AsInteger();
            SecurityCodeAlphaNumericLength = groupTypeCache.GetAttributeValue( "core_checkin_SecurityCodeLength" ).AsInteger();
            SecurityCodeNumericLength = groupTypeCache.GetAttributeValue( "core_checkin_SecurityCodeNumericLength" ).AsInteger();
            SuccessLavaTemplateDisplay = ( Enums.CheckIn.SuccessLavaTemplateDisplayMode ) groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE_OVERRIDE_DISPLAY_MODE ).AsInteger();

            // Lava templates.
            AbilityLevelSelectHeaderLavaTemplate = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_ABILITY_LEVEL_SELECT_HEADER_LAVA_TEMPLATE ) ?? string.Empty;
            ActionSelectHeaderLavaTemplate = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_ACTION_SELECT_HEADER_LAVA_TEMPLATE ) ?? string.Empty;
            CheckoutPersonSelectHeaderLavaTemplate = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_CHECKOUT_PERSON_SELECT_HEADER_LAVA_TEMPLATE ) ?? string.Empty;
            FamilySelectButtonLavaTemplate = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_FAMILYSELECT_LAVA_TEMPLATE ) ?? string.Empty;
            GroupSelectHeaderLavaTemplate = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_GROUP_SELECT_HEADER_LAVA_TEMPLATE ) ?? string.Empty;
            GroupTypeSelectHeaderLavaTemplate = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_GROUP_TYPE_SELECT_HEADER_LAVA_TEMPLATE ) ?? string.Empty;
            LocationSelectHeaderLavaTemplate = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_LOCATION_SELECT_HEADER_LAVA_TEMPLATE ) ?? string.Empty;
            MultiPersonSelectHeaderLavaTemplate = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_MULTI_PERSON_SELECT_HEADER_LAVA_TEMPLATE ) ?? string.Empty;
            PersonSelectAdditionalInformationLavaTemplate = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_ADDITIONAL_INFORMATION_LAVA_TEMPLATE ) ?? string.Empty;
            PersonSelectHeaderLavaTemplate = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_HEADER_LAVA_TEMPLATE ) ?? string.Empty;
            StartLavaTemplate = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_START_LAVA_TEMPLATE ) ?? string.Empty;
            SuccessLavaTemplate = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE ) ?? string.Empty;
            TimeSelectHeaderLavaTemplate = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_TIME_SELECT_HEADER_LAVA_TEMPLATE ) ?? string.Empty;

            // Registration settings.
            AddFamilyWorkflowTypeGuids = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDFAMILYWORKFLOWTYPES ).SplitDelimitedValues().AsGuidList();
            AddPersonWorkflowTypeGuids = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDPERSONWORKFLOWTYPES ).SplitDelimitedValues().AsGuidList();
            CanCheckInKnownRelationshipRoleGuids = GetRelationshipRoleGuids( groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_CANCHECKINKNOWNRELATIONSHIPTYPES ), rockContext );
            DefaultPersonConnectionStatusGuid = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTPERSONCONNECTIONSTATUS ).AsGuidOrNull() ?? SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid();
            DisplayBirthdateForAdults = GetRequirementLevel( groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONADULTS ) );
            DisplayBirthdateForChildren = GetRequirementLevel( groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONCHILDREN ) );
            DisplayGradeForChildren = GetRequirementLevel( groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYGRADEONCHILDREN ) );
            DisplayEthnicityForAdults = GetRequirementLevel( groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS ) );
            DisplayEthnicityForChildren = GetRequirementLevel( groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONCHILDREN ) );
            DisplayRaceForAdults = GetRequirementLevel( groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONADULTS ) );
            DisplayRaceForChildren = GetRequirementLevel( groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONCHILDREN ) );
            IsAlternateIdFieldVisibleForAdults = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORADULTS ).AsBoolean();
            IsAlternateIdFieldVisibleForChildren = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORCHILDREN ).AsBoolean();
            IsCheckInAfterRegistrationAllowed = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_ENABLECHECKINAFTERREGISTRATION ).AsBoolean();
            IsSmsButtonCheckedByDefault = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTSMSENABLED ).AsBoolean();
            IsSmsButtonVisible = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYSMSBUTTON ).AsBoolean();
            KnownRelationshipRoleGuids = GetRelationshipRoleGuids( groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_KNOWNRELATIONSHIPTYPES ), rockContext );
            OptionalAttributeGuidsForAdults = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORADULTS ).SplitDelimitedValues().AsGuidList();
            OptionalAttributeGuidsForChildren = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORCHILDREN ).SplitDelimitedValues().AsGuidList();
            OptionalAttributeGuidsForFamilies = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORFAMILIES ).SplitDelimitedValues().AsGuidList();
            RequiredAttributeGuidsForAdults = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORADULTS ).SplitDelimitedValues().AsGuidList();
            RequiredAttributeGuidsForChildren = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORCHILDREN ).SplitDelimitedValues().AsGuidList();
            RequiredAttributeGuidsForFamilies = groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORFAMILIES ).SplitDelimitedValues().AsGuidList();
            SameFamilyKnownRelationshipRoleGuids = GetRelationshipRoleGuids( groupTypeCache.GetAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_SAMEFAMILYKNOWNRELATIONSHIPTYPES ), rockContext );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the requirement level from the attribute value.
        /// </summary>
        /// <param name="value">The attribute value.</param>
        /// <returns>A <see cref="RequirementLevel"/> value.</returns>
        private static RequirementLevel GetRequirementLevel( string value )
        {
            if ( value == ControlOptions.REQUIRED )
            {
                return RequirementLevel.Required;
            }
            else if ( value == ControlOptions.OPTIONAL )
            {
                return RequirementLevel.Optional;
            }
            else
            {
                return RequirementLevel.Unavailable;
            }
        }

        /// <summary>
        /// Gets the family search type from the defined value unique identifier.
        /// </summary>
        /// <param name="value">The defined value unique identifier.</param>
        /// <returns>A <see cref="FamilySearchType"/> value.</returns>
        private static FamilySearchMode GetFamilySearchType( Guid value )
        {
            if ( value == SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER.AsGuid() )
            {
                // Don't make this the final else since this is a common
                // value, this way we can avoid some extra .AsGuid() calls.
                return FamilySearchMode.PhoneNumber;
            }
            else if ( value == SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME.AsGuid() )
            {
                return FamilySearchMode.Name;
            }
            else if ( value == SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME_AND_PHONE.AsGuid() )
            {
                return FamilySearchMode.NameAndPhone;
            }
            else if ( value == SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_SCANNED_ID.AsGuid() )
            {
                return FamilySearchMode.ScannedId;
            }
            else if ( value == SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_FAMILY_ID.AsGuid() )
            {
                return FamilySearchMode.FamilyId;
            }
            else
            {
                return FamilySearchMode.PhoneNumber;
            }
        }

        /// <summary>
        /// Gets the known relationship role unique identifiers from the raw
        /// value stored in the group type attribute.
        /// </summary>
        /// <param name="value">The raw value of the attribute.</param>
        /// <param name="rockContext">The database context to use for cache lookups.</param>
        /// <returns>A collection of unique identifiers.</returns>
        private static IReadOnlyCollection<Guid> GetRelationshipRoleGuids( string value, RockContext rockContext )
        {
            var roleIds = value.SplitDelimitedValues().AsIntegerList();
            var knownRelationShipRoles = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid(), rockContext )?.Roles;
            var results = new List<Guid>();

            if ( knownRelationShipRoles == null )
            {
                return results;
            }

            // The default value uses "0" to specify Child instead of the actual
            // identifier number.
            if ( roleIds.Contains( 0 ) )
            {
                results.Add( SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CHILD.AsGuid() );
            }

            foreach ( var role in knownRelationShipRoles.Where( a => roleIds.Contains( a.Id ) ) )
            {
                results.Add( role.Guid, true );
            }

            return results;
        }

        /// <summary>
        /// Gets a compiled regular expression for the pattern. This catches
        /// any exceptions and returns <c>null</c> instead.
        /// </summary>
        /// <param name="pattern">The pattern of the regular expression.</param>
        /// <returns>An <see cref="Regex"/> instance of <c>null</c> if <paramref name="pattern"/> was not valid.</returns>
        private static Regex GetRegexOrNull( string pattern )
        {
            if ( pattern.IsNullOrWhiteSpace() )
            {
                return null;
            }

            try
            {
                // A compiled expression will take about 0.2ms to compile to
                // native code for the expected complexity. It then executes
                // 3x faster. Since this is likely to be called hundreds if
                // not thousands of times per service, that is a good tradeoff.
                return new Regex( pattern, RegexOptions.Compiled );
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
