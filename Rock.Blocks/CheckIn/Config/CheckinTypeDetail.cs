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
using System.Reflection;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.CheckIn.Config.CheckinTypeDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn.Config
{
    /// <summary>
    /// Displays the details of a particular Check-in Type.
    /// </summary>
    [DisplayName( "Check-in Type Detail" )]
    [Category( "Check-in > Configuration" )]
    [Description( "Displays the details of a particular Check-in Type." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Schedule Page",
        Key = AttributeKey.SchedulePage,
        Description = "Page used to manage schedules for the check-in type." )]

    #endregion

    [SystemGuid.EntityTypeGuid( "7d1dec32-3a94-45b4-b567-48d9478041b9" )]
    [SystemGuid.BlockTypeGuid( "7ea2e093-2f33-4213-a33e-9e9a7a760181" )]
    public class CheckinTypeDetail : RockEntityDetailBlockType<GroupType, CheckinTypeBag>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string SchedulePage = "SchedulePage";
        }

        private static class PageParameterKey
        {
            public const string CheckinTypeId = "CheckinTypeId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string LinkedPage = "LinkedPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<CheckinTypeBag, CheckinTypeDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.Options = GetBoxOptions();
            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private CheckinTypeDetailOptionsBag GetBoxOptions()
        {
            var options = new CheckinTypeDetailOptionsBag
            {
                AchievementTypeOptions = GetAchievementTypeOptions(),
                DisplayOptions = GetDisplayOptions(),
                FamilyAttributeOptions = GetFamilyAttributeOptions(),
                PersonAttributeOptions = GetPersonAttributeOptions(),
                RelationshipTypeOptions = GetRelationshipTypeOptions(),
                SearchTypeOptions = GetSearchTypeOptions(),
                TemplateDisplayOptions = typeof( SuccessLavaTemplateDisplayMode ).ToEnumListItemBag(),
                PromotionsContentChannelTypeOptions = GetPromotionsContentChannelTypeOptions(),
                HidePanel = string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.CheckinTypeId ) ),
                NameSearch = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME.AsGuid() ).ToListItemBag(),
                ValidProperties = GetValidProperties( new CheckinTypeBag() )
            };

            return options;
        }

        /// <summary>
        /// Gets the promotions content channel type options.
        /// </summary>
        /// <returns></returns>
        private static List<ListItemBag> GetPromotionsContentChannelTypeOptions()
        {
            return ContentChannelCache.All()
                .Where( cc => cc.ContentChannelType.ShowInChannelList == true )
                .OrderBy( cc => cc.Name )
                .AsEnumerable()
                .ToListItemBagList();
        }

        /// <summary>
        /// Gets the content channels.
        /// </summary>
        /// <returns></returns>
        private static List<ListItemBag> GetAchievementTypeOptions()
        {
            return AchievementTypeCache.All()
                .Where( stat => stat.IsActive )
                .OrderBy( stat => stat.Name )
                .AsEnumerable()
                .ToListItemBagList();
        }

        /// <summary>
        /// Gets the search type options.
        /// </summary>
        /// <returns></returns>
        private List<ListItemBag> GetSearchTypeOptions()
        {
            var options = new List<ListItemBag>();
            var searchTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CHECKIN_SEARCH_TYPE.AsGuid() );
            if ( searchTypes != null )
            {
                foreach ( var searchType in searchTypes.DefinedValues )
                {
                    if ( searchType.GetAttributeValue( "UserSelectable" ).AsBooleanOrNull() ?? true )
                    {
                        options.Add( new ListItemBag() { Text = searchType.Value, Value = searchType.Guid.ToString() } );
                    }
                }
            }

            return options;
        }

        /// <summary>
        /// Gets the relationship type options.
        /// </summary>
        /// <returns></returns>
        private List<ListItemBag> GetRelationshipTypeOptions()
        {
            var options = new List<ListItemBag>()
            {
                new ListItemBag { Text = "Child", Value = "0" }
            };

            foreach ( var knownRelationShipRole in GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS ).Roles.Where( a => a.Name != "Child" ) )
            {
                options.Add( new ListItemBag { Text = knownRelationShipRole.Name, Value = knownRelationShipRole.Id.ToString() } );
            }

            return options;
        }

        /// <summary>
        /// Gets the family attribute options.
        /// </summary>
        /// <returns></returns>
        private List<ListItemBag> GetFamilyAttributeOptions()
        {
            var fakeFamily = new Rock.Model.Group { GroupTypeId = GroupTypeCache.GetFamilyGroupType().Id };
            fakeFamily.LoadAttributes();
            return fakeFamily.Attributes.Select( a => new ListItemBag { Text = a.Value.Name, Value = a.Value.Guid.ToString() } ).ToList();
        }

        /// <summary>
        /// Gets the person attribute options.
        /// </summary>
        /// <returns></returns>
        private List<ListItemBag> GetPersonAttributeOptions()
        {
            var fakePerson = new Person();
            fakePerson.LoadAttributes();
            return fakePerson.Attributes.Select( a => new ListItemBag { Text = a.Value.Name, Value = a.Value.Guid.ToString() } ).ToList();
        }

        /// <summary>
        /// Gets the display options.
        /// </summary>
        /// <returns></returns>
        private List<ListItemBag> GetDisplayOptions()
        {
            return new List<ListItemBag>()
            {
                new ListItemBag() { Text = ControlOptions.HIDE, Value = ControlOptions.HIDE },
                new ListItemBag() { Text = ControlOptions.OPTIONAL, Value = ControlOptions.OPTIONAL },
                new ListItemBag() { Text = ControlOptions.REQUIRED, Value = ControlOptions.REQUIRED },
            };
        }

        /// <summary>
        /// Validates the GroupType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="groupType">The GroupType to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the GroupType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateGroupType( GroupType groupType, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<CheckinTypeBag, CheckinTypeDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {GroupType.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) || BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( GroupType.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( GroupType.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="CheckinTypeBag"/> that represents the entity.</returns>
        private CheckinTypeBag GetCommonEntityBag( GroupType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new CheckinTypeBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                IconCssClass = entity.IconCssClass,
                Name = entity.Name
            };
        }

        /// <inheritdoc/>
        protected override CheckinTypeBag GetEntityBagForView( GroupType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.ScheduledTimes = GetScheduleTimes( entity );

            if ( entity.AttributeValues.ContainsKey( "core_checkin_CheckInType" ) )
            {
                bag.CheckInType = entity.AttributeValues["core_checkin_CheckInType"].ValueFormatted;
            }

            if ( entity.AttributeValues.ContainsKey( "core_checkin_SearchType" ) )
            {
                var searchType = entity.AttributeValues["core_checkin_SearchType"];
                bag.SearchType = searchType.ValueFormatted;

                var searchTypeGuid = searchType.Value.AsGuid();
                if ( searchTypeGuid.Equals( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME_AND_PHONE.AsGuid() ) ||
                    searchTypeGuid.Equals( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER.AsGuid() ) )
                {
                    bag.PhoneNumberCompare = entity.AttributeValues["core_checkin_PhoneSearchType"].ValueFormatted;
                }
            }

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: IsAttributeIncluded );

            return bag;
        }

        /// <summary>
        /// Gets the schedule times.
        /// </summary>
        /// <param name="groupType">The GroupType entity.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private string GetScheduleTimes( GroupType groupType )
        {
            var descendantGroupTypeIds = new GroupTypeService( RockContext ).GetCheckinAreaDescendants( groupType.Id ).Select( a => a.Id );
            return new GroupLocationService( RockContext )
                .Queryable().AsNoTracking()
                .Where( a =>
                    a.Group.GroupType.Id == groupType.Id ||
                    descendantGroupTypeIds.Contains( a.Group.GroupTypeId ) )
                .SelectMany( a => a.Schedules )
                .Where( s => s.IsActive )
                .Select( s => s.Name )
                .Distinct()
                .OrderBy( s => s )
                .ToList()
                .AsDelimited( ", " );
        }

        /// <inheritdoc/>
        protected override CheckinTypeBag GetEntityBagForEdit( GroupType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.AdvancedSettings = GetAdvancedSettings( entity );
            bag.BarcodeSettings = GetBarcodeSettings( entity );
            bag.DisplaySettings = GetDisplaySettings( entity );
            bag.GeneralSettings = GetGeneralSettings( entity );
            bag.HeaderText = GetHeaderText( entity );
            bag.RegistrationSettings = GetRegistrationSettings( entity );
            bag.SearchSettings = GetSearchSettings( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: IsAttributeIncluded );

            return bag;
        }

        /// <summary>
        /// Gets the search settings.
        /// </summary>
        /// <param name="groupType">The GroupType entity.</param>
        /// <returns></returns>
        private CheckInSearchSettingsBag GetSearchSettings( GroupType groupType )
        {
            return new CheckInSearchSettingsBag()
            {
                MaxPhoneLength = groupType.GetAttributeValue( "core_checkin_MaximumPhoneSearchLength" ) ?? "10",
                MaxResults = groupType.GetAttributeValue( "core_checkin_MaxSearchResults" ) ?? "100",
                MinPhoneLength = groupType.GetAttributeValue( "core_checkin_MinimumPhoneSearchLength" ) ?? "4",
                PhoneSearchType = groupType.GetAttributeValue( "core_checkin_PhoneSearchType" ),
                SearchType = groupType.GetAttributeValue( "core_checkin_SearchType" )
            };
        }

        /// <summary>
        /// Gets the registration settings.
        /// </summary>
        /// <param name="groupType">The GroupType entity.</param>
        /// <returns></returns>
        private CheckInRegistrationSettingsBag GetRegistrationSettings( GroupType groupType )
        {
            var workflowTypeService = new WorkflowTypeService( RockContext );

            var bag = new CheckInRegistrationSettingsBag()
            {
                CanCheckInKnownRelationshipTypes = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_CANCHECKINKNOWNRELATIONSHIPTYPES ).SplitDelimitedValues().ToList(),
                EnableCheckInAfterRegistration = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ENABLECHECKINAFTERREGISTRATION ).AsBoolean(),
                KnownRelationshipTypes = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_KNOWNRELATIONSHIPTYPES ).SplitDelimitedValues().ToList(),
                RegistrationAddFamilyWorkflowTypes = workflowTypeService.GetByGuids( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDFAMILYWORKFLOWTYPES ).SplitDelimitedValues().AsGuidList() ).ToListItemBagList(),
                RegistrationAddPersonWorkflowTypes = workflowTypeService.GetByGuids( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDPERSONWORKFLOWTYPES ).SplitDelimitedValues().AsGuidList() ).ToListItemBagList(),
                RegistrationDisplayAlternateIdFieldForAdults = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORADULTS ).AsBoolean(),
                RegistrationDisplayAlternateIdFieldForChildren = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORCHILDREN ).AsBoolean(),
                RegistrationDisplayBirthdateOnAdults = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONADULTS ),
                RegistrationDisplayBirthdateOnChildren = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONCHILDREN ),
                RegistrationDisplayEthnicityOnAdults = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS ),
                RegistrationDisplayEthnicityOnChildren = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONCHILDREN ),
                RegistrationDisplayGradeOnChildren = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYGRADEONCHILDREN ),
                RegistrationDisplayRaceOnAdults = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONADULTS ),
                RegistrationDisplayRaceOnChildren = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONCHILDREN ),
                RegistrationDisplaySmsEnabled = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYSMSBUTTON ).AsBoolean(),
                RegistrationOptionalAttributesForAdults = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORADULTS ).SplitDelimitedValues().ToList(),
                RegistrationOptionalAttributesForChildren = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORCHILDREN ).SplitDelimitedValues().ToList(),
                RegistrationOptionalAttributesForFamilies = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORFAMILIES ).SplitDelimitedValues().ToList(),
                RegistrationRequiredAttributesForAdults = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORADULTS ).SplitDelimitedValues().ToList(),
                RegistrationRequiredAttributesForChildren = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORCHILDREN ).SplitDelimitedValues().ToList(),
                RegistrationRequiredAttributesForFamilies = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORFAMILIES ).SplitDelimitedValues().ToList(),
                RegistrationSmsEnabledByDefault = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTSMSENABLED ).AsBoolean(),
                SameFamilyKnownRelationshipTypes = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_SAMEFAMILYKNOWNRELATIONSHIPTYPES ).SplitDelimitedValues().ToList()
            };

            Guid? defaultPersonConnectionStatusValueGuid = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTPERSONCONNECTIONSTATUS ).AsGuidOrNull();
            if ( defaultPersonConnectionStatusValueGuid.HasValue )
            {
                var defaultPersonRecordStatusValue = DefinedValueCache.Get( defaultPersonConnectionStatusValueGuid.Value );
                if ( defaultPersonRecordStatusValue != null )
                {
                    bag.RegistrationDefaultPersonConnectionStatus = defaultPersonRecordStatusValue.ToListItemBag();
                }
            }

            return bag;
        }

        /// <summary>
        /// Gets the header text.
        /// </summary>
        /// <param name="groupType">The GroupType entity.</param>
        /// <returns></returns>
        private CheckInHeaderTextBag GetHeaderText( GroupType groupType )
        {
            return new CheckInHeaderTextBag()
            {
                AbilityLevelSelectHeaderTemplate = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ABILITY_LEVEL_SELECT_HEADER_LAVA_TEMPLATE ),
                ActionSelectHeaderTemplate = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ACTION_SELECT_HEADER_LAVA_TEMPLATE ),
                CheckoutPersonSelectHeaderTemplate = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_CHECKOUT_PERSON_SELECT_HEADER_LAVA_TEMPLATE ),
                GroupSelectHeaderTemplate = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_SELECT_HEADER_LAVA_TEMPLATE ),
                GroupTypeSelectHeaderTemplate = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_TYPE_SELECT_HEADER_LAVA_TEMPLATE ),
                LocationSelectHeaderTemplate = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_LOCATION_SELECT_HEADER_LAVA_TEMPLATE ),
                MultiPersonSelectHeaderTemplate = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_MULTI_PERSON_SELECT_HEADER_LAVA_TEMPLATE ),
                PersonSelectHeaderTemplate = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_HEADER_LAVA_TEMPLATE ),
                TimeSelectHeaderTemplate = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_TIME_SELECT_HEADER_LAVA_TEMPLATE )
            };
        }

        /// <summary>
        /// Gets the general settings.
        /// </summary>
        /// <param name="groupType">The GroupType entity.</param>
        /// <returns></returns>
        private CheckInGeneralSettingsBag GetGeneralSettings( GroupType groupType )
        {
            return new CheckInGeneralSettingsBag()
            {
                AchievementTypes = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES ).SplitDelimitedValues().ToList(),
                AllowCheckoutAtKiosk = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_KIOSK ).AsBoolean(),
                AllowCheckoutInManager = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER ).AsBoolean(),
                AutoSelectDaysBack = groupType.GetAttributeValue( "core_checkin_AutoSelectDaysBack" ) ?? "10",
                AutoSelectOptions = groupType.GetAttributeValue( "core_checkin_AutoSelectOptions" ),
                CheckInType = groupType.GetAttributeValue( "core_checkin_CheckInType" ),
                EnableManager = groupType.GetAttributeValue( "core_checkin_EnableManagerOption" ).AsBoolean( true ),
                EnableOverride = groupType.GetAttributeValue( "core_checkin_EnableOverride" ).AsBoolean( true ),
                EnablePresence = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE ).AsBoolean(),
                PreventDuplicateCheckin = groupType.GetAttributeValue( "core_checkin_PreventDuplicateCheckin" ).AsBoolean( true ),
                PreventInactivePeople = groupType.GetAttributeValue( "core_checkin_PreventInactivePeople" ).AsBoolean( true ),
                UseSameOptions = groupType.GetAttributeValue( "core_checkin_UseSameOptions" ).AsBoolean( false ),
                PromotionsContentChannelTypes = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_PROMOTIONS_CONTENT_CHANNEL ),
                EnableRemoveFamilyKiosk = groupType.GetAttributeValue(Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_REMOVE_FROM_FAMILY_KIOSK ).AsBoolean(),
                EnableProximityCheckin = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PROXIMITY_CHECKIN ).AsBoolean()
            };
        }

        /// <summary>
        /// Gets the display settings.
        /// </summary>
        /// <param name="groupType">The GroupType entity.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private CheckInDisplaySettingsBag GetDisplaySettings( GroupType groupType )
        {
            var displayMode = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE_OVERRIDE_DISPLAY_MODE );
            return new CheckInDisplaySettingsBag()
            {
                FamilySelectTemplate = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_FAMILYSELECT_LAVA_TEMPLATE ),
                HidePhotos = groupType.GetAttributeValue( "core_checkin_HidePhotos" ).AsBoolean( true ),
                PersonSelectTemplate = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_ADDITIONAL_INFORMATION_LAVA_TEMPLATE ),
                StartTemplate = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_START_LAVA_TEMPLATE ),
                SuccessTemplate = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE ),
                SuccessTemplateOverrideDisplayMode = string.IsNullOrWhiteSpace( displayMode ) ? SuccessLavaTemplateDisplayMode.Never.ConvertToInt().ToString() : displayMode,
            };
        }

        /// <summary>
        /// Gets the barcode settings.
        /// </summary>
        /// <param name="groupType">The GroupType entity.</param>
        /// <returns></returns>
        private CheckInBarcodeSettingsBag GetBarcodeSettings( GroupType groupType )
        {
            return new CheckInBarcodeSettingsBag()
            {
                CodeAlphaLength = groupType.GetAttributeValue( "core_checkin_SecurityCodeAlphaLength" ),
                CodeAlphaNumericLength = groupType.GetAttributeValue( "core_checkin_SecurityCodeLength" ),
                CodeNumericLength = groupType.GetAttributeValue( "core_checkin_SecurityCodeNumericLength" ),
                CodeRandom = groupType.GetAttributeValue( "core_checkin_SecurityCodeNumericRandom" ).AsBoolean( true ),
                ReuseCode = groupType.GetAttributeValue( "core_checkin_ReuseSameCode" ).AsBoolean( false )
            };
        }

        /// <summary>
        /// Gets the advanced settings.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <returns></returns>
        private CheckInAdvancedSettingsBag GetAdvancedSettings( GroupType groupType )
        {
            var bag = new CheckInAdvancedSettingsBag()
            {
                AbilityLevelDetermination = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ABILITY_LEVEL_DETERMINATION ),
                AgeRequired = groupType.GetAttributeValue( "core_checkin_AgeRequired" ).AsBoolean( true ),
                DisplayLocCount = groupType.GetAttributeValue( "core_checkin_DisplayLocationCount" ).AsBoolean( true ),
                GradeRequired = groupType.GetAttributeValue( "core_checkin_GradeRequired" ).AsBoolean( true ),
                RefreshInterval = groupType.GetAttributeValue( "core_checkin_RefreshInterval" ),
                SearchRegex = groupType.GetAttributeValue( "core_checkin_RegularExpressionFilter" )
            };

            bag.SpecialNeedsValues = new List<string>();
            if ( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_REMOVE_SPECIAL_NEEDS_GROUPS ).AsBoolean() )
            {
                bag.SpecialNeedsValues.Add( "special-needs" );
            }
            if ( groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_REMOVE_NON_SPECIAL_NEEDS_GROUPS ).AsBoolean() )
            {
                bag.SpecialNeedsValues.Add( "non-special-needs" );
            }

            return bag;
        }

        /// <summary>
        /// Determines if the attribute should be included in the block.
        /// </summary>
        /// <param name="attribute">The attribute to be checked.</param>
        /// <returns><c>true</c> if the attribute should be included, <c>false</c> otherwise.</returns>
        private bool IsAttributeIncluded( AttributeCache attribute )
        {
            var excludeList = BuildAttributeExcludeList();
            return !excludeList.Contains( attribute.Key );
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( GroupType entity, ValidPropertiesBox<CheckinTypeBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            // This pattern of nested bags and using ValidProperties as a flat list of the properties inside the nested bag should not be followed.
            // Later we will come up with a pattern for how to handle this safely.
            // The concern is that two sub-bags might have the same property name which could lead to data loss during a save operation from an older client.
            // However, because this block is rather complex/specific, it would not be a supported configuration to have a non-Obsidian
            // implementation talk to the server so the risk for this specific block is minimal.

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IconCssClass ),
                () => entity.IconCssClass = box.Bag.IconCssClass );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            // Advanced Settings
            box.IfValidProperty( nameof( box.Bag.AdvancedSettings.AbilityLevelDetermination ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ABILITY_LEVEL_DETERMINATION, box.Bag.AdvancedSettings.AbilityLevelDetermination ) );

            box.IfValidProperty( nameof( box.Bag.AdvancedSettings.AgeRequired ),
                () => entity.SetAttributeValue( "core_checkin_AgeRequired", box.Bag.AdvancedSettings.AgeRequired.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.AdvancedSettings.DisplayLocCount ),
                () => entity.SetAttributeValue( "core_checkin_DisplayLocationCount", box.Bag.AdvancedSettings.DisplayLocCount.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.AdvancedSettings.GradeRequired ),
                () => entity.SetAttributeValue( "core_checkin_GradeRequired", box.Bag.AdvancedSettings.GradeRequired.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.AdvancedSettings.RefreshInterval ),
                () => entity.SetAttributeValue( "core_checkin_RefreshInterval", box.Bag.AdvancedSettings.RefreshInterval.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.AdvancedSettings.SearchRegex ),
                () => entity.SetAttributeValue( "core_checkin_RegularExpressionFilter", box.Bag.AdvancedSettings.SearchRegex ) );

            box.IfValidProperty( nameof( box.Bag.AdvancedSettings.SpecialNeedsValues ),
                () =>
                {
                    var specialNeedsValues = box.Bag.AdvancedSettings.SpecialNeedsValues ?? new List<string>();
                    entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_REMOVE_SPECIAL_NEEDS_GROUPS, specialNeedsValues.Contains( "special-needs" ).ToString() );
                    entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_REMOVE_NON_SPECIAL_NEEDS_GROUPS, specialNeedsValues.Contains( "non-special-needs" ).ToString() );
                } );

            // Barcode Settings
            box.IfValidProperty( nameof( box.Bag.BarcodeSettings.CodeAlphaLength ),
                () => entity.SetAttributeValue( "core_checkin_SecurityCodeAlphaLength", box.Bag.BarcodeSettings.CodeAlphaLength.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.BarcodeSettings.CodeAlphaNumericLength ),
                () => entity.SetAttributeValue( "core_checkin_SecurityCodeLength", box.Bag.BarcodeSettings.CodeAlphaNumericLength.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.BarcodeSettings.CodeNumericLength ),
                () => entity.SetAttributeValue( "core_checkin_SecurityCodeNumericLength", box.Bag.BarcodeSettings.CodeNumericLength.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.BarcodeSettings.CodeRandom ),
                () => entity.SetAttributeValue( "core_checkin_SecurityCodeNumericRandom", box.Bag.BarcodeSettings.CodeRandom.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.BarcodeSettings.ReuseCode ),
                () => entity.SetAttributeValue( "core_checkin_ReuseSameCode", box.Bag.BarcodeSettings.ReuseCode.ToString() ) );

            // Display Settings
            box.IfValidProperty( nameof( box.Bag.DisplaySettings.FamilySelectTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_FAMILYSELECT_LAVA_TEMPLATE, box.Bag.DisplaySettings.FamilySelectTemplate ) );

            box.IfValidProperty( nameof( box.Bag.DisplaySettings.HidePhotos ),
                () => entity.SetAttributeValue( "core_checkin_HidePhotos", box.Bag.DisplaySettings.HidePhotos.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.DisplaySettings.PersonSelectTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_ADDITIONAL_INFORMATION_LAVA_TEMPLATE, box.Bag.DisplaySettings.PersonSelectTemplate ) );

            box.IfValidProperty( nameof( box.Bag.DisplaySettings.StartTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_START_LAVA_TEMPLATE, box.Bag.DisplaySettings.StartTemplate ) );

            box.IfValidProperty( nameof( box.Bag.DisplaySettings.SuccessTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE, box.Bag.DisplaySettings.SuccessTemplate ) );

            box.IfValidProperty( nameof( box.Bag.DisplaySettings.SuccessTemplateOverrideDisplayMode ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE_OVERRIDE_DISPLAY_MODE, box.Bag.DisplaySettings.SuccessTemplateOverrideDisplayMode ) );

            // General Settings
            box.IfValidProperty( nameof( box.Bag.GeneralSettings.AchievementTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES, box.Bag.GeneralSettings.AchievementTypes.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.GeneralSettings.AllowCheckoutAtKiosk ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_KIOSK, box.Bag.GeneralSettings.AllowCheckoutAtKiosk.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.GeneralSettings.AllowCheckoutInManager ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER, box.Bag.GeneralSettings.AllowCheckoutInManager.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.GeneralSettings.AutoSelectDaysBack ),
                () => entity.SetAttributeValue( "core_checkin_AutoSelectDaysBack", box.Bag.GeneralSettings.AutoSelectDaysBack ) );

            box.IfValidProperty( nameof( box.Bag.GeneralSettings.AutoSelectOptions ),
                () => entity.SetAttributeValue( "core_checkin_AutoSelectOptions", box.Bag.GeneralSettings.AutoSelectOptions ) );

            box.IfValidProperty( nameof( box.Bag.GeneralSettings.CheckInType ),
                () => entity.SetAttributeValue( "core_checkin_CheckInType", box.Bag.GeneralSettings.CheckInType ) );

            box.IfValidProperty( nameof( box.Bag.GeneralSettings.EnableManager ),
                () => entity.SetAttributeValue( "core_checkin_EnableManagerOption", box.Bag.GeneralSettings.EnableManager.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.GeneralSettings.EnableOverride ),
                () => entity.SetAttributeValue( "core_checkin_EnableOverride", box.Bag.GeneralSettings.EnableOverride.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.GeneralSettings.EnablePresence ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE, box.Bag.GeneralSettings.EnablePresence.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.GeneralSettings.PreventDuplicateCheckin ),
                () => entity.SetAttributeValue( "core_checkin_PreventDuplicateCheckin", box.Bag.GeneralSettings.PreventDuplicateCheckin.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.GeneralSettings.PreventInactivePeople ),
                () => entity.SetAttributeValue( "core_checkin_PreventInactivePeople", box.Bag.GeneralSettings.PreventInactivePeople.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.GeneralSettings.UseSameOptions ),
                () => entity.SetAttributeValue( "core_checkin_UseSameOptions", box.Bag.GeneralSettings.UseSameOptions.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.GeneralSettings.PromotionsContentChannelTypes ),
            () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_PROMOTIONS_CONTENT_CHANNEL, box.Bag.GeneralSettings.PromotionsContentChannelTypes.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.GeneralSettings.EnableRemoveFamilyKiosk ),
            () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_REMOVE_FROM_FAMILY_KIOSK, box.Bag.GeneralSettings.EnableRemoveFamilyKiosk.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.GeneralSettings.EnableProximityCheckin ),
            () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PROXIMITY_CHECKIN, box.Bag.GeneralSettings.EnableProximityCheckin.ToString() ) );

            // Header Text
            box.IfValidProperty( nameof( box.Bag.HeaderText.AbilityLevelSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ABILITY_LEVEL_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.HeaderText.AbilityLevelSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Bag.HeaderText.ActionSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ACTION_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.HeaderText.ActionSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Bag.HeaderText.CheckoutPersonSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_CHECKOUT_PERSON_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.HeaderText.CheckoutPersonSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Bag.HeaderText.GroupSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.HeaderText.GroupSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Bag.HeaderText.GroupTypeSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_TYPE_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.HeaderText.GroupTypeSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Bag.HeaderText.LocationSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_LOCATION_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.HeaderText.LocationSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Bag.HeaderText.MultiPersonSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_MULTI_PERSON_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.HeaderText.MultiPersonSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Bag.HeaderText.PersonSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.HeaderText.PersonSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Bag.HeaderText.TimeSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_TIME_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.HeaderText.TimeSelectHeaderTemplate ) );

            // Registration Settings
            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.CanCheckInKnownRelationshipTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_CANCHECKINKNOWNRELATIONSHIPTYPES, box.Bag.RegistrationSettings.CanCheckInKnownRelationshipTypes.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.EnableCheckInAfterRegistration ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ENABLECHECKINAFTERREGISTRATION, box.Bag.RegistrationSettings.EnableCheckInAfterRegistration.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.KnownRelationshipTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_KNOWNRELATIONSHIPTYPES, box.Bag.RegistrationSettings.KnownRelationshipTypes.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationAddFamilyWorkflowTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDFAMILYWORKFLOWTYPES, box.Bag.RegistrationSettings.RegistrationAddFamilyWorkflowTypes.ConvertAll( wft => wft.Value ).AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationAddPersonWorkflowTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDPERSONWORKFLOWTYPES, box.Bag.RegistrationSettings.RegistrationAddPersonWorkflowTypes.ConvertAll( wft => wft.Value ).AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationDisplayAlternateIdFieldForAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORADULTS, box.Bag.RegistrationSettings.RegistrationDisplayAlternateIdFieldForAdults.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationDisplayAlternateIdFieldForChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORCHILDREN, box.Bag.RegistrationSettings.RegistrationDisplayAlternateIdFieldForChildren.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationDisplayBirthdateOnAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONADULTS, box.Bag.RegistrationSettings.RegistrationDisplayBirthdateOnAdults ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationDisplayBirthdateOnChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONCHILDREN, box.Bag.RegistrationSettings.RegistrationDisplayBirthdateOnChildren ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationDisplayEthnicityOnAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS, box.Bag.RegistrationSettings.RegistrationDisplayEthnicityOnAdults ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationDisplayEthnicityOnChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONCHILDREN, box.Bag.RegistrationSettings.RegistrationDisplayEthnicityOnChildren ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationDisplayGradeOnChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYGRADEONCHILDREN, box.Bag.RegistrationSettings.RegistrationDisplayGradeOnChildren ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationDisplayRaceOnAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONADULTS, box.Bag.RegistrationSettings.RegistrationDisplayRaceOnAdults ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationDisplayRaceOnChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONCHILDREN, box.Bag.RegistrationSettings.RegistrationDisplayRaceOnChildren ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationDisplaySmsEnabled ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYSMSBUTTON, box.Bag.RegistrationSettings.RegistrationDisplaySmsEnabled.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationOptionalAttributesForAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORADULTS, box.Bag.RegistrationSettings.RegistrationOptionalAttributesForAdults.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationOptionalAttributesForChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORCHILDREN, box.Bag.RegistrationSettings.RegistrationOptionalAttributesForChildren.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationOptionalAttributesForFamilies ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORFAMILIES, box.Bag.RegistrationSettings.RegistrationOptionalAttributesForFamilies.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationRequiredAttributesForAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORADULTS, box.Bag.RegistrationSettings.RegistrationRequiredAttributesForAdults.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationRequiredAttributesForChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORCHILDREN, box.Bag.RegistrationSettings.RegistrationRequiredAttributesForChildren.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationRequiredAttributesForFamilies ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORFAMILIES, box.Bag.RegistrationSettings.RegistrationRequiredAttributesForFamilies.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationSmsEnabledByDefault ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTSMSENABLED, box.Bag.RegistrationSettings.RegistrationSmsEnabledByDefault.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.SameFamilyKnownRelationshipTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_SAMEFAMILYKNOWNRELATIONSHIPTYPES, box.Bag.RegistrationSettings.SameFamilyKnownRelationshipTypes.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationSettings.RegistrationDefaultPersonConnectionStatus ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTPERSONCONNECTIONSTATUS, box.Bag.RegistrationSettings.RegistrationDefaultPersonConnectionStatus.Value ) );

            // Search Settings
            box.IfValidProperty( nameof( box.Bag.SearchSettings.MaxPhoneLength ),
                () => entity.SetAttributeValue( "core_checkin_MaximumPhoneSearchLength", box.Bag.SearchSettings.MaxPhoneLength.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.SearchSettings.MaxResults ),
                () => entity.SetAttributeValue( "core_checkin_MaxSearchResults", box.Bag.SearchSettings.MaxResults.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.SearchSettings.MinPhoneLength ),
                () => entity.SetAttributeValue( "core_checkin_MinimumPhoneSearchLength", box.Bag.SearchSettings.MinPhoneLength.ToString() ) );

            box.IfValidProperty( nameof( box.Bag.SearchSettings.PhoneSearchType ),
                () => entity.SetAttributeValue( "core_checkin_PhoneSearchType", box.Bag.SearchSettings.PhoneSearchType ) );

            box.IfValidProperty( nameof( box.Bag.SearchSettings.SearchType ),
                () => entity.SetAttributeValue( "core_checkin_SearchType", box.Bag.SearchSettings.SearchType ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: IsAttributeIncluded );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override GroupType GetInitialEntity()
        {
            var entity = GetInitialEntity<GroupType, GroupTypeService>( RockContext, PageParameterKey.CheckinTypeId );

            if ( entity?.Id == 0 )
            {
                var templatePurpose = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );
                entity.GroupTypePurposeValueId = templatePurpose?.Id;
            }

            return entity;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var qryParams = new Dictionary<string, string>
            {
                { "GroupTypeId", PageParameter( PageParameterKey.CheckinTypeId ) }
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl(),
                [NavigationUrlKey.LinkedPage] = this.GetLinkedPageUrl( AttributeKey.SchedulePage, qryParams ),
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out GroupType entity, out BlockActionResult error )
        {
            var entityService = new GroupTypeService( RockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new GroupType();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{GroupType.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${GroupType.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        private List<string> BuildAttributeExcludeList()
        {
            var excludeList = new List<string>
            {
                "core_checkin_AgeRequired",
                "core_checkin_GradeRequired",
                "core_checkin_HidePhotos",
                "core_checkin_PreventDuplicateCheckin",
                "core_checkin_PreventInactivePeople",
                "core_checkin_CheckInType",
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ABILITY_LEVEL_DETERMINATION,
                "core_checkin_DisplayLocationCount",
                "core_checkin_EnableManagerOption",
                "core_checkin_EnableOverride",
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_PROMOTIONS_CONTENT_CHANNEL,
                "core_checkin_MaximumPhoneSearchLength",
                "core_checkin_MaxSearchResults",
                "core_checkin_MinimumPhoneSearchLength",
                "core_checkin_UseSameOptions",
                "core_checkin_PhoneSearchType",
                "core_checkin_RefreshInterval",
                "core_checkin_RegularExpressionFilter",
                "core_checkin_ReuseSameCode",
                "core_checkin_SearchType",
                "core_checkin_SecurityCodeLength",
                "core_checkin_SecurityCodeAlphaLength",
                "core_checkin_SecurityCodeNumericLength",
                "core_checkin_SecurityCodeNumericRandom",
                "core_checkin_AutoSelectDaysBack",
                "core_checkin_AutoSelectOptions",
#pragma warning disable CS0618 // Type or member is obsolete
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT,
#pragma warning restore CS0618 // Type or member is obsolete
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_REMOVE_FROM_FAMILY_KIOSK,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PROXIMITY_CHECKIN,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_KIOSK,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_CANCHECKINKNOWNRELATIONSHIPTYPES,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORADULTS,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORCHILDREN,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYSMSBUTTON,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTSMSENABLED,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ENABLECHECKINAFTERREGISTRATION,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_KNOWNRELATIONSHIPTYPES,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORADULTS,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORCHILDREN,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORFAMILIES,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORADULTS,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORCHILDREN,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORFAMILIES,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_SAMEFAMILYKNOWNRELATIONSHIPTYPES,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDFAMILYWORKFLOWTYPES,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDPERSONWORKFLOWTYPES,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTPERSONCONNECTIONSTATUS,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONCHILDREN,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONADULTS,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYGRADEONCHILDREN,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ACTION_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_CHECKOUT_PERSON_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_MULTI_PERSON_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_TYPE_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_TIME_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ABILITY_LEVEL_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_LOCATION_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_START_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_FAMILYSELECT_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE_OVERRIDE_DISPLAY_MODE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_ADDITIONAL_INFORMATION_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONCHILDREN,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONCHILDREN,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONADULTS,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS,

                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_REMOVE_SPECIAL_NEEDS_GROUPS,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_REMOVE_NON_SPECIAL_NEEDS_GROUPS
            };

            return excludeList;
        }

        /// <summary>
        /// Searchs the bag's properties and nested properties for values that can be updated.
        /// </summary>
        /// <param name="bag">The CheckinTypeBag</param>
        /// <returns></returns>
        private List<string> GetValidProperties( CheckinTypeBag bag )
        {
            var validProperties = new List<string>();

            var properties = bag.GetType().GetProperties();

            AddValidProperties( validProperties, properties );

            return validProperties;
        }

        /// <summary>
        /// Adds the property names of the selected properties, if any if them is a nested class its property names are added.
        /// </summary>
        /// <param name="validProperties">The current selection of valid properties</param>
        /// <param name="properties">The properties to be evaluated</param>
        /// <returns></returns>
        private static void AddValidProperties( List<string> validProperties, PropertyInfo[] properties )
        {
            foreach ( var propertyInfo in properties )
            {
                var propertyType = propertyInfo.PropertyType;
                if ( propertyType.IsClass && !propertyType.IsGenericType && propertyType != typeof( string ) && propertyType != typeof( ListItemBag ) )
                {
                    AddValidProperties( validProperties, propertyInfo.PropertyType.GetProperties() );
                }

                validProperties.Add( propertyInfo.Name );
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<CheckinTypeBag>()
            {
                Bag = bag,
                ValidProperties = GetValidProperties( bag )
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<CheckinTypeBag> box )
        {
            var entityService = new GroupTypeService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var isNew = entity.Id == 0;

            if ( isNew )
            {
                var templatePurpose = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );
                if ( templatePurpose != null )
                {
                    entity.GroupTypePurposeValueId = templatePurpose.Id;
                }
            }

            entity.LoadAttributes( RockContext );

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateGroupType( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.CheckinTypeId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<CheckinTypeBag>()
            {
                Bag = bag,
                ValidProperties = GetValidProperties( bag )
            } );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new GroupTypeService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            var pageRef = new Rock.Web.PageReference( PageCache.Id );
            var routeId = PageCache.PageRoutes.FirstOrDefault()?.Id;

            if ( routeId.HasValue )
            {
                pageRef.RouteId = routeId.Value;
            }

            return ActionOk( pageRef.BuildUrl() );
        }

        #endregion
    }
}
