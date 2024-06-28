using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2;
using Rock.Data;
using Rock.Enums.CheckIn;
using Rock.Enums.Controls;
using Rock.Model;
using Rock.SystemKey;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2
{
    /// <summary>
    /// This suite checks the area configuration data objects to make sure
    /// they continue to work as expected. This primarily covers just the
    /// constructors as they currently have no other logic.
    /// </summary>
    /// <seealso cref="TemplateConfigurationData"/>
    [TestClass]
    public class TemplateConfigurationDataTests : CheckInMockDatabase
    {
        #region Constants

        private const int KnownRelationshipTypeOneId = 11;
        private static readonly Guid KnownRelationshipTypeOneGuid = new Guid( "7536b0aa-8060-475d-8264-d7937e33932b" );

        private const int KnownRelationshipTypeTwoId = 12;
        private static readonly Guid KnownRelationshipTypeTwoGuid = new Guid( "c8221f22-80f7-494b-9b61-e30aa33d5e09" );

        private const int KnownRelationshipTypeThreeId = 13;
        private static readonly Guid KnownRelationshipTypeThreeGuid = new Guid( "2a0074ca-82cf-4b9f-ad41-0d92e1214e94" );

        #endregion

        #region Constructor Tests

        // Start Configuration Properties section.
        [DataRow( nameof( TemplateConfigurationData.AbilityLevelDetermination ), AbilityLevelDeterminationMode.DoNotAsk, GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ABILITY_LEVEL_DETERMINATION )]
        [DataRow( nameof( TemplateConfigurationData.AchievementTypeGuids ), "d6b38cb2-eb9a-4f73-a4bc-978cf6b9d1a2,dc71b47f-35e9-4d26-b012-d7d6a6141c55", GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES )]
        [DataRow( nameof( TemplateConfigurationData.AutoSelectDaysBack ), 14, "core_checkin_AutoSelectDaysBack" )]
        [DataRow( nameof( TemplateConfigurationData.AutoSelect ), AutoSelectMode.PeopleAndAreaGroupLocation, "core_checkin_AutoSelectOptions" )]
        // KioskCheckInType has its own test.
        // FamilySearchType has its own test.
        [DataRow( nameof( TemplateConfigurationData.IsAgeRequired ), true, "core_checkin_AgeRequired" )]
        [DataRow( nameof( TemplateConfigurationData.IsCheckoutAtKioskAllowed ), true, GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_KIOSK )]
        [DataRow( nameof( TemplateConfigurationData.IsCheckoutInManagerAllowed ), true, GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER )]
        [DataRow( nameof( TemplateConfigurationData.IsDuplicateCheckInPrevented ), true, "core_checkin_PreventDuplicateCheckin" )]
        [DataRow( nameof( TemplateConfigurationData.IsGradeRequired ), true, "core_checkin_GradeRequired" )]
        [DataRow( nameof( TemplateConfigurationData.IsInactivePersonExcluded ), true, "core_checkin_PreventInactivePeople" )]
        [DataRow( nameof( TemplateConfigurationData.IsLocationCountDisplayed ), true, "core_checkin_DisplayLocationCount" )]
        [DataRow( nameof( TemplateConfigurationData.IsNumericSecurityCodeRandom ), true, "core_checkin_SecurityCodeNumericRandom" )]
        [DataRow( nameof( TemplateConfigurationData.IsOverrideAvailable ), true, "core_checkin_EnableOverride" )]
        [DataRow( nameof( TemplateConfigurationData.IsPhotoHidden ), true, "core_checkin_HidePhotos" )]
        [DataRow( nameof( TemplateConfigurationData.IsPresenceEnabled ), true, GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE )]
        [DataRow( nameof( TemplateConfigurationData.IsSameCodeUsedForFamily ), true, "core_checkin_ReuseSameCode" )]
        [DataRow( nameof( TemplateConfigurationData.IsSameOptionUsed ), true, "core_checkin_UseSameOptions" )]
        [DataRow( nameof( TemplateConfigurationData.IsSupervisorEnabled ), true, "core_checkin_EnableManagerOptions" )]
        [DataRow( nameof( TemplateConfigurationData.MaximumNumberOfResults ), 5, "core_checkin_MaxSearchResults" )]
        [DataRow( nameof( TemplateConfigurationData.MaximumPhoneNumberLength ), 10, "core_checkin_MaximumPhoneSearchLength" )]
        [DataRow( nameof( TemplateConfigurationData.MinimumPhoneNumberLength ), 7, "core_checkin_MinimumPhoneSearchLength" )]
        [DataRow( nameof( TemplateConfigurationData.PhoneSearchType ), PhoneSearchMode.EndsWith, "core_checkin_PhoneSearchType" )]
        [DataRow( nameof( TemplateConfigurationData.RefreshInterval ), 60, "core_checkin_RefreshInterval" )]
        [DataRow( nameof( TemplateConfigurationData.RegularExpressionFilter ), "^test$", "core_checkin_RegularExpressionFilter" )]
        [DataRow( nameof( TemplateConfigurationData.SecurityCodeAlphaLength ), 3, "core_checkin_SecurityCodeAlphaLength" )]
        [DataRow( nameof( TemplateConfigurationData.SecurityCodeAlphaNumericLength ), 2, "core_checkin_SecurityCodeLength" )]
        [DataRow( nameof( TemplateConfigurationData.SecurityCodeNumericLength ), 1, "core_checkin_SecurityCodeNumericLength" )]
        [DataRow( nameof( TemplateConfigurationData.SuccessLavaTemplateDisplay ), SuccessLavaTemplateDisplayMode.Append, GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE_OVERRIDE_DISPLAY_MODE )]
        // Start Lava templates section.
        [DataRow( nameof( TemplateConfigurationData.AbilityLevelSelectHeaderLavaTemplate ), "testtemplate", GroupTypeAttributeKey.CHECKIN_ABILITY_LEVEL_SELECT_HEADER_LAVA_TEMPLATE )]
        [DataRow( nameof( TemplateConfigurationData.ActionSelectHeaderLavaTemplate ), "testtemplate", GroupTypeAttributeKey.CHECKIN_ACTION_SELECT_HEADER_LAVA_TEMPLATE )]
        [DataRow( nameof(TemplateConfigurationData.CheckoutPersonSelectHeaderLavaTemplate ), "testtemplate", GroupTypeAttributeKey.CHECKIN_CHECKOUT_PERSON_SELECT_HEADER_LAVA_TEMPLATE )]
        [DataRow( nameof( TemplateConfigurationData.FamilySelectButtonLavaTemplate ), "testtemplate", GroupTypeAttributeKey.CHECKIN_FAMILYSELECT_LAVA_TEMPLATE )]
        [DataRow( nameof( TemplateConfigurationData.GroupSelectHeaderLavaTemplate ), "testtemplate", GroupTypeAttributeKey.CHECKIN_GROUP_SELECT_HEADER_LAVA_TEMPLATE )]
        [DataRow( nameof( TemplateConfigurationData.GroupTypeSelectHeaderLavaTemplate ), "testtemplate", GroupTypeAttributeKey.CHECKIN_GROUP_TYPE_SELECT_HEADER_LAVA_TEMPLATE )]
        [DataRow( nameof( TemplateConfigurationData.LocationSelectHeaderLavaTemplate ), "testtemplate", GroupTypeAttributeKey.CHECKIN_LOCATION_SELECT_HEADER_LAVA_TEMPLATE )]
        [DataRow( nameof( TemplateConfigurationData.MultiPersonSelectHeaderLavaTemplate ), "testtemplate", GroupTypeAttributeKey.CHECKIN_MULTI_PERSON_SELECT_HEADER_LAVA_TEMPLATE )]
        [DataRow( nameof( TemplateConfigurationData.PersonSelectAdditionalInformationLavaTemplate ), "testtemplate", GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_ADDITIONAL_INFORMATION_LAVA_TEMPLATE )]
        [DataRow( nameof( TemplateConfigurationData.PersonSelectHeaderLavaTemplate ), "testtemplate", GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_HEADER_LAVA_TEMPLATE )]
        [DataRow( nameof( TemplateConfigurationData.StartLavaTemplate ), "testtemplate", GroupTypeAttributeKey.CHECKIN_START_LAVA_TEMPLATE )]
        [DataRow( nameof( TemplateConfigurationData.SuccessLavaTemplate ), "testtemplate", GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE )]
        [DataRow( nameof( TemplateConfigurationData.TimeSelectHeaderLavaTemplate ), "testtemplate", GroupTypeAttributeKey.CHECKIN_TIME_SELECT_HEADER_LAVA_TEMPLATE )]
        // Start Registration settings section.
        [DataRow( nameof( TemplateConfigurationData.AddFamilyWorkflowTypeGuids ), "54cd8101-8e40-4dc6-950d-727f671ece71,3a4b0725-1a85-42bc-85b0-0b0d27040a9a", GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDFAMILYWORKFLOWTYPES )]
        [DataRow( nameof( TemplateConfigurationData.AddPersonWorkflowTypeGuids ), "491201c8-9f82-4644-8d88-9f4068d5a246,4c19cb07-9ce9-4e61-965e-e8cf57ed8951", GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDPERSONWORKFLOWTYPES )]
        // CanCheckInKnownRelationshipRoleGuids has its own test.
        [DataRow( nameof( TemplateConfigurationData.DefaultPersonConnectionStatusGuid ), "89f15ffa-cefa-4040-ab7e-becceb3e800d", GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTPERSONCONNECTIONSTATUS )]
        // DisplayBirthdateForAdults has its own test.
        // DisplayBirthdateForChildren has its own test.
        // DisplayGradeForChildren has its own test.
        // DisplayEthnicityForAdults has its own test.
        // DisplayEthnicityForChildren has its own test.
        // DisplayRaceForAdults has its own test.
        [DataRow( nameof( TemplateConfigurationData.IsAlternateIdFieldVisibleForAdults ), true, GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORADULTS )]
        [DataRow( nameof( TemplateConfigurationData.IsAlternateIdFieldVisibleForChildren ), true, GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORCHILDREN )]
        [DataRow( nameof( TemplateConfigurationData.IsCheckInAfterRegistrationAllowed ), true, GroupTypeAttributeKey.CHECKIN_REGISTRATION_ENABLECHECKINAFTERREGISTRATION )]
        [DataRow( nameof( TemplateConfigurationData.IsSmsButtonCheckedByDefault ), true, GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTSMSENABLED )]
        [DataRow( nameof( TemplateConfigurationData.IsSmsButtonVisible ), true, GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYSMSBUTTON )]
        // KnownRelationshipRoleGuids has its own test.
        [DataRow( nameof( TemplateConfigurationData.OptionalAttributeGuidsForAdults ), "604f3ce6-ef1c-4fd2-bfe3-a84572c06b9a,3e70b9c0-b2a2-42c4-9443-9c4dabc7f64d", GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORADULTS )]
        [DataRow( nameof( TemplateConfigurationData.OptionalAttributeGuidsForChildren ), "99604a8b-f26e-43cf-8279-82d298a0906f,dd558d46-e371-4fb1-ad07-daf8dc747c13", GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORCHILDREN )]
        [DataRow( nameof( TemplateConfigurationData.OptionalAttributeGuidsForFamilies ), "4571ca29-1cae-42cf-b775-37f329477f22,48662e32-f65e-476a-ab2b-72850f570f14", GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORFAMILIES )]
        [DataRow( nameof( TemplateConfigurationData.RequiredAttributeGuidsForAdults ), "1da54b47-e9c8-43c5-956a-d61f31dfe746,8b180920-902d-4d0f-9ea6-b30b59b129fd", GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORADULTS )]
        [DataRow( nameof( TemplateConfigurationData.RequiredAttributeGuidsForChildren ), "4492a016-0a37-4856-bb32-fa856736c03e,a5d22122-649d-4b97-81d5-dbb93289ecbb", GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORCHILDREN )]
        [DataRow( nameof( TemplateConfigurationData.RequiredAttributeGuidsForFamilies ), "89d52c4c-7c9c-4b47-8481-40925c629412,9b98833f-1231-40fa-983b-da1c2053de94", GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORFAMILIES )]
        // SameFamilyKnownRelationshipRoleGuids has its own test.
        [TestMethod]
        public void Constructor_WithSingleAttributeValue_InitializesProperty( string propertyName, object expectedValue, string attributeKey )
        {
            var rockContextMock = GetRockContextMock();
            rockContextMock.SetupDbSet<GroupType>();

            var groupType = CreateEntityMock<GroupType>( 1, new Guid( "4b8fd000-2043-4f4b-a2f6-31d58e26123c" ) );

            if ( expectedValue.GetType().IsEnum )
            {
                groupType.SetMockAttributeValue( attributeKey, ( ( int ) expectedValue ).ToString() );
            }
            else
            {
                groupType.SetMockAttributeValue( attributeKey, expectedValue.ToString() );
            }

            var groupTypeCache = new GroupTypeCache();
            groupTypeCache.SetFromEntity( groupType.Object );

            var instance = new TemplateConfigurationData( groupTypeCache, rockContextMock.Object );

            var propertyValue = instance.GetPropertyValue( propertyName );

            // Convert the expected value if it is a special type that can't
            // be represented at compile time.
            if ( propertyValue is Guid propertyGuidValue )
            {
                expectedValue = new Guid( expectedValue.ToString() );

                Assert.AreEqual( expectedValue, propertyGuidValue );
            }
            else if ( propertyValue is IReadOnlyCollection<Guid> propertyGuidValues )
            {
                var expectedValues = expectedValue.ToString().SplitDelimitedValues().AsGuidList();

                CollectionAssert.AreEqual( expectedValues, propertyGuidValues.ToList() );
            }
            else
            {
                Assert.AreEqual( expectedValue, propertyValue );
            }
        }

        [DataRow( nameof( TemplateConfigurationData.DisplayBirthdateForAdults ), GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONADULTS )]
        [DataRow( nameof( TemplateConfigurationData.DisplayBirthdateForChildren ), GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONCHILDREN )]
        [DataRow( nameof( TemplateConfigurationData.DisplayGradeForChildren ), GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYGRADEONCHILDREN )]
        [DataRow( nameof( TemplateConfigurationData.DisplayEthnicityForAdults ), GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS )]
        [DataRow( nameof( TemplateConfigurationData.DisplayEthnicityForChildren ), GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONCHILDREN )]
        [DataRow( nameof( TemplateConfigurationData.DisplayRaceForAdults ), GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONADULTS )]
        [DataRow( nameof( TemplateConfigurationData.DisplayRaceForChildren ), GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONCHILDREN )]
        [TestMethod]
        public void Constructor_WithSingleAttributeValue_InitializesDisplayBirthdateForAdultsProperty( string propertyName, string attributeKey )
        {
            var expectedValue = RequirementLevel.Required;
            var attributeValue = global::Rock.CheckIn.ControlOptions.REQUIRED;

            var rockContextMock = GetRockContextMock();
            rockContextMock.SetupDbSet<GroupType>();

            var groupType = CreateEntityMock<GroupType>( 1, new Guid( "4b8fd000-2043-4f4b-a2f6-31d58e26123c" ) );

            groupType.SetMockAttributeValue( attributeKey, attributeValue );

            var groupTypeCache = new GroupTypeCache();
            groupTypeCache.SetFromEntity( groupType.Object );

            var instance = new TemplateConfigurationData( groupTypeCache, rockContextMock.Object );

            var propertyValue = instance.GetPropertyValue( propertyName );

            Assert.AreEqual( expectedValue, propertyValue );
        }

        [DataRow( nameof( TemplateConfigurationData.FamilySearchType ), FamilySearchMode.PhoneNumber )]
        [DataRow( nameof( TemplateConfigurationData.KioskCheckInType ), KioskCheckInMode.Individual )]
        [DataRow( nameof( TemplateConfigurationData.RegularExpressionFilter ), "" )]
        [DataRow( nameof( TemplateConfigurationData.AbilityLevelSelectHeaderLavaTemplate ), "" )]
        [DataRow( nameof( TemplateConfigurationData.ActionSelectHeaderLavaTemplate ), "" )]
        [DataRow( nameof( TemplateConfigurationData.CheckoutPersonSelectHeaderLavaTemplate ), "" )]
        [DataRow( nameof( TemplateConfigurationData.FamilySelectButtonLavaTemplate ), "" )]
        [DataRow( nameof( TemplateConfigurationData.GroupSelectHeaderLavaTemplate ), "" )]
        [DataRow( nameof( TemplateConfigurationData.GroupTypeSelectHeaderLavaTemplate ), "" )]
        [DataRow( nameof( TemplateConfigurationData.LocationSelectHeaderLavaTemplate ), "" )]
        [DataRow( nameof( TemplateConfigurationData.MultiPersonSelectHeaderLavaTemplate ), "" )]
        [DataRow( nameof( TemplateConfigurationData.PersonSelectAdditionalInformationLavaTemplate ), "" )]
        [DataRow( nameof( TemplateConfigurationData.PersonSelectHeaderLavaTemplate ), "" )]
        [DataRow( nameof( TemplateConfigurationData.StartLavaTemplate ), "" )]
        [DataRow( nameof( TemplateConfigurationData.SuccessLavaTemplate ), "" )]
        [DataRow( nameof( TemplateConfigurationData.TimeSelectHeaderLavaTemplate ), "" )]
        [TestMethod]
        public void Constructor_WithEmptyGroupTypeCache_InitializesDefaultPropertyValue( string propertyName, object expectedValue )
        {
            var rockContextMock = GetRockContextMock();
            rockContextMock.SetupDbSet<GroupType>();

            var groupType = CreateEntityMock<GroupType>( 1, new Guid( "4b8fd000-2043-4f4b-a2f6-31d58e26123c" ) );

            var groupTypeCache = new GroupTypeCache();
            groupTypeCache.SetFromEntity( groupType.Object );

            var instance = new TemplateConfigurationData( groupTypeCache, rockContextMock.Object );

            var propertyValue = instance.GetPropertyValue( propertyName );

            Assert.AreEqual( expectedValue, propertyValue );
        }

        [TestMethod]
        public void Constructor_WithAchievementTypeAttributeValue_InitializesAchievementTypeGuids()
        {
            var expectedValue = new List<Guid>( new[] { Guid.NewGuid(), Guid.NewGuid() } );

            var rockContextMock = GetRockContextMock();
            rockContextMock.SetupDbSet<GroupType>();

            var groupType = CreateEntityMock<GroupType>( 1, new Guid( "4b8fd000-2043-4f4b-a2f6-31d58e26123c" ) );

            groupType.SetMockAttributeValue( GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES, expectedValue.AsDelimited( ", " ) );

            var groupTypeCache = new GroupTypeCache();
            groupTypeCache.SetFromEntity( groupType.Object );

            var instance = new TemplateConfigurationData( groupTypeCache, rockContextMock.Object );

            CollectionAssert.AreEquivalent( expectedValue, instance.AchievementTypeGuids.ToList() );
        }

        [TestMethod]
        public void Constructor_WithKioskCheckInTypeAttributeValue_InitializesKioskCheckInType()
        {
            var expectedValue = KioskCheckInMode.Family;

            var rockContextMock = GetRockContextMock();
            rockContextMock.SetupDbSet<GroupType>();

            var groupType = CreateEntityMock<GroupType>( 1, new Guid( "4b8fd000-2043-4f4b-a2f6-31d58e26123c" ) );

            groupType.SetMockAttributeValue( "core_checkin_CheckInType", "1" );

            var groupTypeCache = new GroupTypeCache();
            groupTypeCache.SetFromEntity( groupType.Object );

            var instance = new TemplateConfigurationData( groupTypeCache, rockContextMock.Object );

            Assert.AreEqual( expectedValue, instance.KioskCheckInType );
        }

        [DataRow( FamilySearchMode.PhoneNumber, SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER )]
        [DataRow( FamilySearchMode.Name, SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME )]
        [DataRow( FamilySearchMode.NameAndPhone, SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME_AND_PHONE )]
        [DataRow( FamilySearchMode.ScannedId, SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_SCANNED_ID )]
        [DataRow( FamilySearchMode.FamilyId, SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_FAMILY_ID )]
        [TestMethod]
        public void Constructor_WithFamilySearchTypeAttributeValue_InitializesFamilySearchType( FamilySearchMode expectedValue, string attributeValue )
        {
            var rockContextMock = GetRockContextMock();
            rockContextMock.SetupDbSet<GroupType>();

            var groupType = CreateEntityMock<GroupType>( 1, new Guid( "4b8fd000-2043-4f4b-a2f6-31d58e26123c" ) );

            groupType.SetMockAttributeValue( "core_checkin_SearchType", attributeValue );

            var groupTypeCache = new GroupTypeCache();
            groupTypeCache.SetFromEntity( groupType.Object );

            var instance = new TemplateConfigurationData( groupTypeCache, rockContextMock.Object );

            Assert.AreEqual( expectedValue, instance.FamilySearchType );
        }

        [TestMethod]
        [DataRow( RequirementLevel.Unavailable, "" )]
        [DataRow( RequirementLevel.Optional, global::Rock.CheckIn.ControlOptions.OPTIONAL )]
        [DataRow( RequirementLevel.Required, global::Rock.CheckIn.ControlOptions.REQUIRED )]
        [DataRow( RequirementLevel.Unavailable, global::Rock.CheckIn.ControlOptions.HIDE )]
        public void Constructor_WithControlOption_ConvertsToRequirementLevel( RequirementLevel expectedRequirementLevel, string controlOption )
        {
            var rockContextMock = GetRockContextMock();
            rockContextMock.SetupDbSet<GroupType>();

            var groupType = CreateEntityMock<GroupType>( 1, new Guid( "4b8fd000-2043-4f4b-a2f6-31d58e26123c" ) );

            groupType.SetMockAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONADULTS, controlOption );

            var groupTypeCache = new GroupTypeCache();
            groupTypeCache.SetFromEntity( groupType.Object );

            var instance = new TemplateConfigurationData( groupTypeCache, rockContextMock.Object );

            Assert.AreEqual( expectedRequirementLevel, instance.DisplayBirthdateForAdults );
        }

        [TestMethod]
        [DataRow( nameof( TemplateConfigurationData.CanCheckInKnownRelationshipRoleGuids ), GroupTypeAttributeKey.CHECKIN_REGISTRATION_CANCHECKINKNOWNRELATIONSHIPTYPES )]
        [DataRow( nameof( TemplateConfigurationData.KnownRelationshipRoleGuids ), GroupTypeAttributeKey.CHECKIN_REGISTRATION_KNOWNRELATIONSHIPTYPES )]
        [DataRow( nameof( TemplateConfigurationData.SameFamilyKnownRelationshipRoleGuids ), GroupTypeAttributeKey.CHECKIN_REGISTRATION_SAMEFAMILYKNOWNRELATIONSHIPTYPES )]
        public void Constructor_WithRelationshipRoleAttributeValue_InitializesProperty( string propertyName, string attributeKey )
        {
            var expectedGuids = new List<Guid>
            {
                KnownRelationshipTypeOneGuid,
                KnownRelationshipTypeTwoGuid
            };

            var rockContextMock = GetRockContextMock();
            rockContextMock.SetupDbSet<GroupType>();

            SetupGroupTypeRoleMocks( rockContextMock );

            var groupType = CreateEntityMock<GroupType>( 1, new Guid( "4b8fd000-2043-4f4b-a2f6-31d58e26123c" ) );
            groupType.SetMockAttributeValue( attributeKey, $"{KnownRelationshipTypeOneId},{KnownRelationshipTypeTwoId}" );

            var groupTypeCache = new GroupTypeCache();
            groupTypeCache.SetFromEntity( groupType.Object );

            var instance = new TemplateConfigurationData( groupTypeCache, rockContextMock.Object );

            var propertyValue = instance.GetPropertyValue( propertyName );

            CollectionAssert.AreEquivalent( expectedGuids, ( ( IReadOnlyCollection<Guid> ) propertyValue ).ToList() );
        }

        [TestMethod]
        public void Constructor_WithZeroRelationshipRoleAttributeValue_InitializesPropertyWithChildRole()
        {
            var expectedGuids = new List<Guid>
            {
                SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CHILD.AsGuid()
            };

            var rockContextMock = GetRockContextMock();
            rockContextMock.SetupDbSet<GroupType>();

            SetupGroupTypeRoleMocks( rockContextMock );

            var groupType = CreateEntityMock<GroupType>( 1, new Guid( "4b8fd000-2043-4f4b-a2f6-31d58e26123c" ) );
            groupType.SetMockAttributeValue( GroupTypeAttributeKey.CHECKIN_REGISTRATION_CANCHECKINKNOWNRELATIONSHIPTYPES, "0" );

            var groupTypeCache = new GroupTypeCache();
            groupTypeCache.SetFromEntity( groupType.Object );

            var instance = new TemplateConfigurationData( groupTypeCache, rockContextMock.Object );

            CollectionAssert.AreEquivalent( expectedGuids, instance.CanCheckInKnownRelationshipRoleGuids.ToList() );
        }

        [TestMethod]
        public void DeclaredType_HasExpectedPropertyCount()
        {
            // This is a simple test to help us know when new properties are
            // added so we can update the other tests to check for those
            // properties.
            var type = typeof( TemplateConfigurationData );
            var expectedPropertyCount = 67;

            var propertyCount = type.GetProperties().Length;

            Assert.AreEqual( expectedPropertyCount, propertyCount );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Sets up the group type role mocks that will be used by tests in
        /// this suite.
        /// </summary>
        /// <param name="rockContextMock">The rock context mock.</param>
        private void SetupGroupTypeRoleMocks( Mock<RockContext> rockContextMock )
        {
            var knownRelationshipsGroupType = CreateEntityMock<GroupType>( 2, SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );

            rockContextMock.SetupDbSet( knownRelationshipsGroupType.Object );

            var knownRelationshipsGroupTypeCache = GroupTypeCache.Get( knownRelationshipsGroupType.Object.Id, rockContextMock.Object );
            var roles = new List<GroupTypeRoleCache>
            {
                new GroupTypeRoleCache( new GroupTypeRole
                {
                    Id = KnownRelationshipTypeOneId,
                    Guid = KnownRelationshipTypeOneGuid
                } ),
                new GroupTypeRoleCache( new GroupTypeRole
                {
                    Id = KnownRelationshipTypeTwoId,
                    Guid = KnownRelationshipTypeTwoGuid
                } ),
                new GroupTypeRoleCache( new GroupTypeRole
                {
                    Id = KnownRelationshipTypeThreeId,
                    Guid = KnownRelationshipTypeThreeGuid
                } )
            };

            // Use reflection to set the backing field since we can't currently
            // override the rock context used by the Roles property.
            var internalRolesField = typeof( GroupTypeCache ).GetField( "_roles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );

            internalRolesField.SetValue( knownRelationshipsGroupTypeCache, roles );
        }

        #endregion
    }
}
