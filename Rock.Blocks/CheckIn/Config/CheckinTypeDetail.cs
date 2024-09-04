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
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Schedule Page",
        Key = AttributeKey.SchedulePage,
        Description = "Page used to manage schedules for the check-in type." )]

    #endregion

    [SystemGuid.EntityTypeGuid( "7d1dec32-3a94-45b4-b567-48d9478041b9" )]
    [SystemGuid.BlockTypeGuid( "7ea2e093-2f33-4213-a33e-9e9a7a760181" )]
    public class CheckinTypeDetail : RockDetailBlockType
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
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<CheckinTypeBag, CheckinTypeDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.Options = GetBoxOptions();
                box.NavigationUrls = GetBoxNavigationUrls();
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<GroupType>();

                return box;
            }
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
                HidePanel = string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.CheckinTypeId ) ),
                NameSearch = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME.AsGuid() ).ToListItemBag(),
            };

            return options;
        }

        /// <summary>
        /// Gets the achievement type options.
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
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the GroupType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateGroupType( GroupType groupType, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<CheckinTypeBag, CheckinTypeDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {GroupType.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) || BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            entity.LoadAttributes( rockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity, rockContext );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
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
                    box.Entity = GetEntityBagForEdit( entity, rockContext );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( GroupType.FriendlyTypeName );
                }
            }
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

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <param name="rockContext">The rockContext.</param>
        /// <returns>A <see cref="CheckinTypeBag"/> that represents the entity.</returns>
        private CheckinTypeBag GetEntityBagForView( GroupType entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.ScheduledTimes = GetScheduleTimes( entity, rockContext );

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

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, attributeFilter: IsAttributeIncluded );

            return bag;
        }

        /// <summary>
        /// Gets the schedule times.
        /// </summary>
        /// <param name="groupType">The GroupType entity.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private string GetScheduleTimes( GroupType groupType, RockContext rockContext )
        {
            var descendantGroupTypeIds = new GroupTypeService( rockContext ).GetCheckinAreaDescendants( groupType.Id ).Select( a => a.Id );
            return new GroupLocationService( rockContext )
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

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A <see cref="CheckinTypeBag"/> that represents the entity.</returns>
        private CheckinTypeBag GetEntityBagForEdit( GroupType entity, RockContext rockContext )
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
            bag.RegistrationSettings = GetRegistrationSettings( entity, rockContext );
            bag.SearchSettings = GetSearchSettings( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, attributeFilter: IsAttributeIncluded );

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
                MaxPhoneLength = groupType.GetAttributeValue( "core_checkin_MaximumPhoneSearchLength" ).AsIntegerOrNull() ?? 10,
                MaxResults = groupType.GetAttributeValue( "core_checkin_MaxSearchResults" ).AsIntegerOrNull() ?? 100,
                MinPhoneLength = groupType.GetAttributeValue( "core_checkin_MinimumPhoneSearchLength" ).AsIntegerOrNull() ?? 4,
                PhoneSearchType = groupType.GetAttributeValue( "core_checkin_PhoneSearchType" ),
                SearchType = groupType.GetAttributeValue( "core_checkin_SearchType" )
            };
        }

        /// <summary>
        /// Gets the registration settings.
        /// </summary>
        /// <param name="groupType">The GroupType entity.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private CheckInRegistrationSettingsBag GetRegistrationSettings( GroupType groupType, RockContext rockContext )
        {
            var workflowTypeService = new WorkflowTypeService( rockContext );

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
                AutoSelectDaysBack = groupType.GetAttributeValue( "core_checkin_AutoSelectDaysBack" ).AsIntegerOrNull() ?? 10,
                AutoSelectOptions = groupType.GetAttributeValue( "core_checkin_AutoSelectOptions" ),
                CheckInType = groupType.GetAttributeValue( "core_checkin_CheckInType" ),
                EnableManager = groupType.GetAttributeValue( "core_checkin_EnableManagerOption" ).AsBoolean( true ),
                EnableOverride = groupType.GetAttributeValue( "core_checkin_EnableOverride" ).AsBoolean( true ),
                EnablePresence = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE ).AsBoolean(),
                PreventDuplicateCheckin = groupType.GetAttributeValue( "core_checkin_PreventDuplicateCheckin" ).AsBoolean( true ),
                PreventInactivePeople = groupType.GetAttributeValue( "core_checkin_PreventInactivePeople" ).AsBoolean( true ),
                UseSameOptions = groupType.GetAttributeValue( "core_checkin_UseSameOptions" ).AsBoolean( false )
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
                CodeAlphaLength = groupType.GetAttributeValue( "core_checkin_SecurityCodeAlphaLength" ).AsInteger(),
                CodeAlphaNumericLength = groupType.GetAttributeValue( "core_checkin_SecurityCodeLength" ).AsInteger(),
                CodeNumericLength = groupType.GetAttributeValue( "core_checkin_SecurityCodeNumericLength" ).AsInteger(),
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
            return new CheckInAdvancedSettingsBag()
            {
                AbilityLevelDetermination = groupType.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ABILITY_LEVEL_DETERMINATION ),
                AgeRequired = groupType.GetAttributeValue( "core_checkin_AgeRequired" ).AsBoolean( true ),
                DisplayLocCount = groupType.GetAttributeValue( "core_checkin_DisplayLocationCount" ).AsBoolean( true ),
                GradeRequired = groupType.GetAttributeValue( "core_checkin_GradeRequired" ).AsBoolean( true ),
                RefreshInterval = groupType.GetAttributeValue( "core_checkin_RefreshInterval" ).AsIntegerOrNull(),
                SearchRegex = groupType.GetAttributeValue( "core_checkin_RegularExpressionFilter" )
            };
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

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( GroupType entity, DetailBlockBox<CheckinTypeBag, CheckinTypeDetailOptionsBag> box, RockContext rockContext )
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

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.IconCssClass ),
                () => entity.IconCssClass = box.Entity.IconCssClass );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            // Advanced Settings
            box.IfValidProperty( nameof( box.Entity.AdvancedSettings.AbilityLevelDetermination ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ABILITY_LEVEL_DETERMINATION, box.Entity.AdvancedSettings.AbilityLevelDetermination ) );

            box.IfValidProperty( nameof( box.Entity.AdvancedSettings.AgeRequired ),
                () => entity.SetAttributeValue( "core_checkin_AgeRequired", box.Entity.AdvancedSettings.AgeRequired.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.AdvancedSettings.DisplayLocCount ),
                () => entity.SetAttributeValue( "core_checkin_DisplayLocationCount", box.Entity.AdvancedSettings.DisplayLocCount.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.AdvancedSettings.GradeRequired ),
                () => entity.SetAttributeValue( "core_checkin_GradeRequired", box.Entity.AdvancedSettings.GradeRequired.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.AdvancedSettings.RefreshInterval ),
                () => entity.SetAttributeValue( "core_checkin_RefreshInterval", box.Entity.AdvancedSettings.RefreshInterval.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.AdvancedSettings.SearchRegex ),
                () => entity.SetAttributeValue( "core_checkin_RegularExpressionFilter", box.Entity.AdvancedSettings.SearchRegex ) );

            // Barcode Settings
            box.IfValidProperty( nameof( box.Entity.BarcodeSettings.CodeAlphaLength ),
                () => entity.SetAttributeValue( "core_checkin_SecurityCodeAlphaLength", box.Entity.BarcodeSettings.CodeAlphaLength.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.BarcodeSettings.CodeAlphaNumericLength ),
                () => entity.SetAttributeValue( "core_checkin_SecurityCodeLength", box.Entity.BarcodeSettings.CodeAlphaNumericLength.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.BarcodeSettings.CodeNumericLength ),
                () => entity.SetAttributeValue( "core_checkin_SecurityCodeNumericLength", box.Entity.BarcodeSettings.CodeNumericLength.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.BarcodeSettings.CodeRandom ),
                () => entity.SetAttributeValue( "core_checkin_SecurityCodeNumericRandom", box.Entity.BarcodeSettings.CodeRandom.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.BarcodeSettings.ReuseCode ),
                () => entity.SetAttributeValue( "core_checkin_ReuseSameCode", box.Entity.BarcodeSettings.ReuseCode.ToString() ) );

            // Display Settings
            box.IfValidProperty( nameof( box.Entity.DisplaySettings.FamilySelectTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_FAMILYSELECT_LAVA_TEMPLATE, box.Entity.DisplaySettings.FamilySelectTemplate ) );

            box.IfValidProperty( nameof( box.Entity.DisplaySettings.HidePhotos ),
                () => entity.SetAttributeValue( "core_checkin_HidePhotos", box.Entity.DisplaySettings.HidePhotos.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.DisplaySettings.PersonSelectTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_ADDITIONAL_INFORMATION_LAVA_TEMPLATE, box.Entity.DisplaySettings.PersonSelectTemplate ) );

            box.IfValidProperty( nameof( box.Entity.DisplaySettings.StartTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_START_LAVA_TEMPLATE, box.Entity.DisplaySettings.StartTemplate ) );

            box.IfValidProperty( nameof( box.Entity.DisplaySettings.SuccessTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE, box.Entity.DisplaySettings.SuccessTemplate ) );

            box.IfValidProperty( nameof( box.Entity.DisplaySettings.SuccessTemplateOverrideDisplayMode ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE_OVERRIDE_DISPLAY_MODE, box.Entity.DisplaySettings.SuccessTemplateOverrideDisplayMode ) );

            // General Settings
            box.IfValidProperty( nameof( box.Entity.GeneralSettings.AchievementTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES, box.Entity.GeneralSettings.AchievementTypes.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Entity.GeneralSettings.AllowCheckoutAtKiosk ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_KIOSK, box.Entity.GeneralSettings.AllowCheckoutAtKiosk.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.GeneralSettings.AllowCheckoutInManager ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER, box.Entity.GeneralSettings.AllowCheckoutInManager.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.GeneralSettings.AutoSelectDaysBack ),
                () => entity.SetAttributeValue( "core_checkin_AutoSelectDaysBack", box.Entity.GeneralSettings.AutoSelectDaysBack ) );

            box.IfValidProperty( nameof( box.Entity.GeneralSettings.AutoSelectOptions ),
                () => entity.SetAttributeValue( "core_checkin_AutoSelectOptions", box.Entity.GeneralSettings.AutoSelectOptions ) );

            box.IfValidProperty( nameof( box.Entity.GeneralSettings.CheckInType ),
                () => entity.SetAttributeValue( "core_checkin_CheckInType", box.Entity.GeneralSettings.CheckInType ) );

            box.IfValidProperty( nameof( box.Entity.GeneralSettings.EnableManager ),
                () => entity.SetAttributeValue( "core_checkin_EnableManagerOption", box.Entity.GeneralSettings.EnableManager.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.GeneralSettings.EnableOverride ),
                () => entity.SetAttributeValue( "core_checkin_EnableOverride", box.Entity.GeneralSettings.EnableOverride.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.GeneralSettings.EnablePresence ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE, box.Entity.GeneralSettings.EnablePresence.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.GeneralSettings.PreventDuplicateCheckin ),
                () => entity.SetAttributeValue( "core_checkin_PreventDuplicateCheckin", box.Entity.GeneralSettings.PreventDuplicateCheckin.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.GeneralSettings.PreventInactivePeople ),
                () => entity.SetAttributeValue( "core_checkin_PreventInactivePeople", box.Entity.GeneralSettings.PreventInactivePeople.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.GeneralSettings.UseSameOptions ),
                () => entity.SetAttributeValue( "core_checkin_UseSameOptions", box.Entity.GeneralSettings.UseSameOptions.ToString() ) );

            // Header Text
            box.IfValidProperty( nameof( box.Entity.HeaderText.AbilityLevelSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ABILITY_LEVEL_SELECT_HEADER_LAVA_TEMPLATE, box.Entity.HeaderText.AbilityLevelSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Entity.HeaderText.ActionSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ACTION_SELECT_HEADER_LAVA_TEMPLATE, box.Entity.HeaderText.ActionSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Entity.HeaderText.CheckoutPersonSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_CHECKOUT_PERSON_SELECT_HEADER_LAVA_TEMPLATE, box.Entity.HeaderText.CheckoutPersonSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Entity.HeaderText.GroupSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_SELECT_HEADER_LAVA_TEMPLATE, box.Entity.HeaderText.GroupSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Entity.HeaderText.GroupTypeSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_TYPE_SELECT_HEADER_LAVA_TEMPLATE, box.Entity.HeaderText.GroupTypeSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Entity.HeaderText.LocationSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_LOCATION_SELECT_HEADER_LAVA_TEMPLATE, box.Entity.HeaderText.LocationSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Entity.HeaderText.MultiPersonSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_MULTI_PERSON_SELECT_HEADER_LAVA_TEMPLATE, box.Entity.HeaderText.MultiPersonSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Entity.HeaderText.PersonSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_HEADER_LAVA_TEMPLATE, box.Entity.HeaderText.PersonSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Entity.HeaderText.TimeSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_TIME_SELECT_HEADER_LAVA_TEMPLATE, box.Entity.HeaderText.TimeSelectHeaderTemplate ) );

            // Registration Settings
            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.CanCheckInKnownRelationshipTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_CANCHECKINKNOWNRELATIONSHIPTYPES, box.Entity.RegistrationSettings.CanCheckInKnownRelationshipTypes.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.EnableCheckInAfterRegistration ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ENABLECHECKINAFTERREGISTRATION, box.Entity.RegistrationSettings.EnableCheckInAfterRegistration.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.KnownRelationshipTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_KNOWNRELATIONSHIPTYPES, box.Entity.RegistrationSettings.KnownRelationshipTypes.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationAddFamilyWorkflowTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDFAMILYWORKFLOWTYPES, box.Entity.RegistrationSettings.RegistrationAddFamilyWorkflowTypes.ConvertAll( wft => wft.Value ).AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationAddPersonWorkflowTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDPERSONWORKFLOWTYPES, box.Entity.RegistrationSettings.RegistrationAddPersonWorkflowTypes.ConvertAll( wft => wft.Value ).AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationDisplayAlternateIdFieldForAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORADULTS, box.Entity.RegistrationSettings.RegistrationDisplayAlternateIdFieldForAdults.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationDisplayAlternateIdFieldForChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORCHILDREN, box.Entity.RegistrationSettings.RegistrationDisplayAlternateIdFieldForChildren.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationDisplayBirthdateOnAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONADULTS, box.Entity.RegistrationSettings.RegistrationDisplayBirthdateOnAdults ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationDisplayBirthdateOnChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONCHILDREN, box.Entity.RegistrationSettings.RegistrationDisplayBirthdateOnChildren ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationDisplayEthnicityOnAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS, box.Entity.RegistrationSettings.RegistrationDisplayEthnicityOnAdults ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationDisplayEthnicityOnChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONCHILDREN, box.Entity.RegistrationSettings.RegistrationDisplayEthnicityOnChildren ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationDisplayGradeOnChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYGRADEONCHILDREN, box.Entity.RegistrationSettings.RegistrationDisplayGradeOnChildren ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationDisplayRaceOnAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONADULTS, box.Entity.RegistrationSettings.RegistrationDisplayRaceOnAdults ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationDisplayRaceOnChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONCHILDREN, box.Entity.RegistrationSettings.RegistrationDisplayRaceOnChildren ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationDisplaySmsEnabled ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYSMSBUTTON, box.Entity.RegistrationSettings.RegistrationDisplaySmsEnabled.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationOptionalAttributesForAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORADULTS, box.Entity.RegistrationSettings.RegistrationOptionalAttributesForAdults.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationOptionalAttributesForChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORCHILDREN, box.Entity.RegistrationSettings.RegistrationOptionalAttributesForChildren.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationOptionalAttributesForFamilies ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORFAMILIES, box.Entity.RegistrationSettings.RegistrationOptionalAttributesForFamilies.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationRequiredAttributesForAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORADULTS, box.Entity.RegistrationSettings.RegistrationRequiredAttributesForAdults.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationRequiredAttributesForChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORCHILDREN, box.Entity.RegistrationSettings.RegistrationRequiredAttributesForChildren.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationRequiredAttributesForFamilies ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORFAMILIES, box.Entity.RegistrationSettings.RegistrationRequiredAttributesForFamilies.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationSmsEnabledByDefault ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTSMSENABLED, box.Entity.RegistrationSettings.RegistrationSmsEnabledByDefault.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.SameFamilyKnownRelationshipTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_SAMEFAMILYKNOWNRELATIONSHIPTYPES, box.Entity.RegistrationSettings.SameFamilyKnownRelationshipTypes.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationSettings.RegistrationDefaultPersonConnectionStatus ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTPERSONCONNECTIONSTATUS, box.Entity.RegistrationSettings.RegistrationDefaultPersonConnectionStatus.Value ) );

            // Search Settings
            box.IfValidProperty( nameof( box.Entity.SearchSettings.MaxPhoneLength ),
                () => entity.SetAttributeValue( "core_checkin_MaximumPhoneSearchLength", box.Entity.SearchSettings.MaxPhoneLength.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.SearchSettings.MaxResults ),
                () => entity.SetAttributeValue( "core_checkin_MaxSearchResults", box.Entity.SearchSettings.MaxResults.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.SearchSettings.MinPhoneLength ),
                () => entity.SetAttributeValue( "core_checkin_MinimumPhoneSearchLength", box.Entity.SearchSettings.MinPhoneLength.ToString() ) );

            box.IfValidProperty( nameof( box.Entity.SearchSettings.PhoneSearchType ),
                () => entity.SetAttributeValue( "core_checkin_PhoneSearchType", box.Entity.SearchSettings.PhoneSearchType ) );

            box.IfValidProperty( nameof( box.Entity.SearchSettings.SearchType ),
                () => entity.SetAttributeValue( "core_checkin_SearchType", box.Entity.SearchSettings.SearchType ) );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson, attributeFilter: IsAttributeIncluded );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="GroupType"/> to be viewed or edited on the page.</returns>
        private GroupType GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<GroupType, GroupTypeService>( rockContext, PageParameterKey.CheckinTypeId );
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
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                if ( entity != null )
                {
                    entity.LoadAttributes( rockContext );
                }

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( GroupType entity )
        {
            var securityGrant = new SecurityGrant();

            securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out GroupType entity, out BlockActionResult error )
        {
            var entityService = new GroupTypeService( rockContext );
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
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS
            };

            return excludeList;
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
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<CheckinTypeBag, CheckinTypeDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<CheckinTypeBag, CheckinTypeDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new GroupTypeService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
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

                entity.LoadAttributes( rockContext );

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateGroupType( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
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
                entity.LoadAttributes( rockContext );

                return ActionOk( GetEntityBagForView( entity, rockContext ) );
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new GroupTypeService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<CheckinTypeBag, CheckinTypeDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<CheckinTypeBag, CheckinTypeDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
                };

                var oldAttributeGuids = box.Entity.Attributes.Values.Select( a => a.AttributeGuid ).ToList();
                var newAttributeGuids = refreshedBox.Entity.Attributes.Values.Select( a => a.AttributeGuid );

                // If the attributes haven't changed then return a 204 status code.
                if ( oldAttributeGuids.SequenceEqual( newAttributeGuids ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
                }

                // Replace any values for attributes that haven't changed with
                // the value sent by the client. This ensures any unsaved attribute
                // value changes are not lost.
                foreach ( var kvp in refreshedBox.Entity.Attributes )
                {
                    if ( oldAttributeGuids.Contains( kvp.Value.AttributeGuid ) )
                    {
                        refreshedBox.Entity.AttributeValues[kvp.Key] = box.Entity.AttributeValues[kvp.Key];
                    }
                }

                return ActionOk( refreshedBox );
            }
        }

        #endregion
    }
}
