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
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.RealTime.Topics;
using Rock.RealTime;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.ViewModels.Blocks.Group.GroupPlacement;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays the details of a particular group.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Group Placement" )]
    [Category( "Group" )]
    [Description( "Block to manage group placements" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [RegistrationTemplateField(
        "Registration Template",
        Description = "If provided, this Registration Template will override any Registration Template specified in a URL parameter.",
        Key = AttributeKey.RegistrationTemplate,
        Order = 0,
        IsRequired = false
        )]

    [LinkedPage(
        "Group Detail Page",
        Key = AttributeKey.GroupDetailPage,
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER,
        Order = 1
        )]

    [LinkedPage(
        "Group Member Detail Page",
        Key = AttributeKey.GroupMemberDetailPage,
        DefaultValue = Rock.SystemGuid.Page.GROUP_MEMBER_DETAIL_GROUP_VIEWER,
        Order = 2
        )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "0AA9BF5D-D72C-41DB-9719-253CE2500122" )]
    [Rock.SystemGuid.BlockTypeGuid( "5FA6DDB6-2A99-4882-8E49-562781D69ECA" )]
    public class GroupPlacement : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string RegistrationTemplate = "RegistrationTemplate";
            public const string GroupDetailPage = "GroupDetailPage";
            public const string GroupMemberDetailPage = "GroupMemberDetailPage";
        }

        private static class PreferenceKey
        {
            public const string PlacementConfigurationJSONRegistrationInstanceIdKey = "PlacementConfigurationJSON_RegistrationInstanceIdKey_{0}";
            public const string PlacementConfigurationJSONRegistrationTemplateIdKey = "PlacementConfigurationJSON_RegistrationTemplateIdKey_{0}";
            public const string PlacementConfigurationJSONSourceGroupIdKey = "PlacementConfigurationJSON_SourceGroupIdKey_{0}";
            public const string PlacementConfigurationJSONEntitySetIdKey = "PlacementConfigurationJSON_EntitySetIdKey_{0}";
            public const string PersonAttributeFilterRegistrationInstanceIdKey = "PersonAttributeFilter_RegistrationInstanceIdKey_{0}";
            public const string PersonAttributeFilterRegistrationTemplateIdKey = "PersonAttributeFilter_RegistrationTemplateIdKey_{0}";
            public const string PersonAttributeFilterSourceGroupIdKey = "PersonAttributeFilter_SourceGroupIdKey_{0}";
            public const string GroupAttributeFilterGroupTypeIdKey = "GroupAttributeFilter_GroupTypeIdKey_{0}";
            public const string GroupMemberAttributeFilterGroupTypeIdKey = "GroupMemberAttributeFilter_GroupTypeIdKey_{0}";
            public const string RegistrantFeeItemValuesForFiltersJSONRegistrationInstanceIdKey = "RegistrantFeeItemValuesForFiltersJSON_RegistrationInstanceIdKey_{0}";
            public const string RegistrantFeeItemValuesForFiltersJSONRegistrationTemplateIdKey = "RegistrantFeeItemValuesForFiltersJSON_RegistrationTemplateIdKey_{0}";
            public const string SortOrderRegistrationInstanceIdKey = "SortOrder_RegistrationInstanceIdKey_{0}";
            public const string SortOrderRegistrationTemplateIdKey = "SortOrder_RegistrationTemplateIdKey_{0}";
            public const string SortOrderSourceGroupIdKey = "SortOrder_SourceGroupIdKey_{0}";
            public const string SortOrderEntitySetIdKey = "SortOrder_EntitySetIdKey_{0}";
            public const string IsGenderHighlightingRegistrationInstanceIdKey = "IsGenderHighlighting_RegistrationInstanceIdKey_{0}";
            public const string IsGenderHighlightingRegistrationTemplateIdKey = "IsGenderHighlighting_RegistrationTemplateIdKey_{0}";
            public const string IsGenderHighlightingSourceGroupIdKey = "IsGenderHighlighting_SourceGroupIdKey_{0}";
            public const string IsGenderHighlightingEntitySetIdKey = "IsGenderHighlighting_EntitySetIdKey_{0}";
            public const string FallbackRegistrationTemplatePlacement = "FallbackRegistrationTemplatePlacement";
        }

        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
            public const string RegistrationTemplateId = "RegistrationTemplateId";
            public const string RegistrationTemplatePlacementId = "RegistrationTemplatePlacementId";
            public const string PromptForTemplatePlacement = "PromptForTemplatePlacement";
            public const string SourcePerson = "SourcePerson";
            public const string SourceGroup = "SourceGroup";
            public const string ReturnUrl = "ReturnUrl";
            public const string AllowMultiplePlacements = "AllowMultiplePlacements";
            public const string DestinationGroup = "DestinationGroup";
            public const string DestinationGroupType = "DestinationGroupType";
            public const string EntitySet = "EntitySet";
        }

        private static class NavigationUrlKey
        {
            public const string GroupDetailPage = "GroupDetailPage";
            public const string GroupMemberDetailPage = "GroupMemberDetailPage";
        }

        #endregion Keys

        #region Properties

        protected Guid? FallbackRegistrationTemplatePlacement => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FallbackRegistrationTemplatePlacement )
            .AsGuidOrNull();

        #endregion Properties

        #region Classes

        /// <summary>
        /// Represents the result of the destination group query, including group details and registration context.
        /// </summary>
        private class DestinationGroupResult
        {
            /// <summary>
            /// Gets or sets the unique identifier for the group.
            /// </summary>
            public int GroupId { get; set; }

            /// <summary>
            /// Gets or sets the display name of the group.
            /// </summary>
            public string GroupName { get; set; }

            /// <summary>
            /// Gets or sets the globally unique identifier (GUID) for the group.
            /// </summary>
            public Guid GroupGuid { get; set; }

            /// <summary>
            /// Gets or sets the optional capacity limit for the group.
            /// </summary>
            public int? GroupCapacity { get; set; }

            /// <summary>
            /// Gets or sets the identifier for the group type.
            /// </summary>
            public int GroupTypeId { get; set; }

            /// <summary>
            /// Gets or sets the order or priority of the group within a list.
            /// </summary>
            public int GroupOrder { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the group is shared across contexts.
            /// </summary>
            public bool IsShared { get; set; }

            /// <summary>
            /// Gets or sets the optional identifier for the associated registration instance.
            /// </summary>
            public int? RegistrationInstanceId { get; set; }

            /// <summary>
            /// Gets or sets the optional identifier of the parent group, if this group is nested.
            /// </summary>
            public int? ParentGroupId { get; set; }

            /// <summary>
            /// Gets or sets the optional identifier for the associated campus.
            /// </summary>
            public int? CampusId { get; set; }
        }


        /// <summary>
        /// Represents the result of a group placement people query, including person, group membership, and registration fee details.
        /// </summary>
        private class PlacementPeopleResult
        {
            #region Group Member Info

            /// <summary>
            /// Gets or sets the identifier of the group the person is associated with.
            /// </summary>
            public int? GroupId { get; set; }

            /// <summary>
            /// Gets or sets the type identifier of the group.
            /// </summary>
            public int? GroupTypeId { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the group member record.
            /// </summary>
            public int? GroupMemberId { get; set; }

            /// <summary>
            /// Gets or sets the role identifier within the group.
            /// </summary>
            public int? GroupRoleId { get; set; }

            #endregion

            #region Person Info

            /// <summary>
            /// Gets or sets the identifier of the person.
            /// </summary>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the first name of the person.
            /// </summary>
            public string FirstName { get; set; }

            /// <summary>
            /// Gets or sets the nickname of the person.
            /// </summary>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the last name of the person.
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the gender of the person.
            /// </summary>
            public Gender Gender { get; set; }

            /// <summary>
            /// Gets or sets the photo ID associated with the person's profile picture.
            /// </summary>
            public int? PhotoId { get; set; }

            /// <summary>
            /// Gets or sets the age of the person.
            /// </summary>
            public int? Age { get; set; }

            /// <summary>
            /// Gets or sets the record type value ID, used for differentiating record types (e.g., person vs. business).
            /// </summary>
            public int? RecordTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets the age classification of the person (e.g., child, adult).
            /// </summary>
            public AgeClassification AgeClassification { get; set; }

            #endregion

            #region Registrant Info

            /// <summary>
            /// Gets or sets the identifier of the registrant.
            /// </summary>
            public int? RegistrantId { get; set; }

            /// <summary>
            /// Gets or sets the name of the registration instance.
            /// </summary>
            public string RegistrationInstanceName { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the registration instance.
            /// </summary>
            public int? RegistrationInstanceId { get; set; }

            /// <summary>
            /// Gets or sets the date and time the registration or record was created.
            /// </summary>
            public DateTime? CreatedDateTime { get; set; }

            /// <summary>
            /// Gets or sets the name of the fee associated with the registration.
            /// </summary>
            public string FeeName { get; set; }

            /// <summary>
            /// Gets or sets the selected option or value for the fee.
            /// </summary>
            public string Option { get; set; }

            /// <summary>
            /// Gets or sets the quantity selected for the fee option.
            /// </summary>
            public int? Quantity { get; set; }

            /// <summary>
            /// Gets or sets the cost associated with the fee selection.
            /// </summary>
            public decimal? Cost { get; set; }

            /// <summary>
            /// Gets or sets the type of fee (e.g., per person, per item).
            /// </summary>
            public RegistrationFeeType? FeeType { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the selected fee item.
            /// </summary>
            public int? FeeItemId { get; set; }

            #endregion
        }


        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            GroupPlacementInitializationBox box = GetInitializationBox();

            return box;
        }

        /// <summary>
        /// Gets the Initialization Box for Group Placements
        /// </summary>
        /// <returns>The box</returns>
        private GroupPlacementInitializationBox GetInitializationBox()
        {
            GroupPlacementInitializationBox box = new GroupPlacementInitializationBox
            {
                GroupPlacementKeys = new GroupPlacementKeysBag(),
                NavigationUrls = GetBoxNavigationUrls(),
                BackPageUrl = GetBackPageUrl(),
                PlacementConfigurationSettingOptions = new PlacementConfigurationSettingOptionsBag()
            };

            var sourcePersonId = GetIdFromPageParameter( PageParameterKey.SourcePerson );
            if ( sourcePersonId.HasValue )
            {
                var sourcePerson = new PersonService( RockContext ).Get( sourcePersonId.Value );
                box.SourcePerson = new PersonBag
                {
                    PersonIdKey = sourcePerson.IdKey,
                    FirstName = sourcePerson.FirstName,
                    NickName = sourcePerson.NickName,
                    LastName = sourcePerson.LastName,
                    Gender = sourcePerson.Gender
                };
            }

            int? destinationGroupTypeId = null;
            var sourceGroupId = GetIdFromPageParameter( PageParameterKey.SourceGroup );
            var entitySetId = GetIdFromPageParameter( PageParameterKey.EntitySet );

            // We determine the Placement Mode based on the Ids provided. If there is a Source Group Id or an
            // Entity Set Id than we know we are in one of those two modes.
            if ( sourceGroupId.HasValue || entitySetId.HasValue )
            {
                box.IsPlacementAllowingMultiple = PageParameter( PageParameterKey.AllowMultiplePlacements ).AsBoolean();
                destinationGroupTypeId = GetIdFromPageParameter( PageParameterKey.DestinationGroupType );

                if ( sourceGroupId.HasValue )
                {
                    var sourceGroup = GroupCache.Get( sourceGroupId.Value );

                    if ( sourceGroup == null )
                    {
                        box.ErrorMessage = "The Source Group was not found.";
                        return box;
                    }

                    // If the Destination Group Type Id was not provided than we will use the Group Type of
                    // the Source Group.
                    if ( !destinationGroupTypeId.HasValue )
                    {
                        destinationGroupTypeId = sourceGroup.GroupTypeId;
                    }

                    GroupMember fakeSourceGroupMember = new GroupMember
                    {
                        GroupTypeId = sourceGroup.GroupTypeId
                    };

                    fakeSourceGroupMember.LoadAttributes();

                    box.PlacementConfigurationSettingOptions.SourceAttributes = fakeSourceGroupMember.GetPublicAttributeListItemsForView( GetCurrentPerson(), true );
                    box.Title = $"Group Placement - {sourceGroup.Name}";
                    box.GroupPlacementKeys.SourceGroupIdKey = IdHasher.Instance.GetHash( sourceGroupId.Value );
                    box.GroupPlacementKeys.SourceGroupTypeIdKey = IdHasher.Instance.GetHash( sourceGroup.GroupTypeId );
                    box.GroupPlacementKeys.SourceGroupGuid = sourceGroup.Guid;
                    box.PlacementMode = PlacementMode.GroupMode;
                }
                else
                {
                    box.GroupPlacementKeys.EntitySetIdKey = IdHasher.Instance.GetHash( entitySetId.Value );
                    box.PlacementMode = PlacementMode.EntitySetMode;

                    var entitySet = new EntitySetService( RockContext ).Get( entitySetId.Value );

                    if ( entitySet == null )
                    {
                        box.ErrorMessage = "The Entity Set was not found.";
                        return box;
                    }

                    box.Title = $"Selected: {entitySet.Items.Count()} {( entitySet.Items.Count() == 1 ? "Person" : "People" )}";
                }
            }
            else
            {
                box = GetBoxForRegistrationPlacement( box, out destinationGroupTypeId );
            }

            if ( box.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                return box;
            }

            if ( !destinationGroupTypeId.HasValue )
            {
                box.ErrorMessage = "The Destination Group Type was not found.";
                return box;
            }

            // If destinationGroupTypeId was null an error message would be returned before this, so we can use .Value here.
            var destinationGroupType = GroupTypeCache.Get( destinationGroupTypeId.Value );

            box.GroupPlacementKeys.DestinationGroupTypeIdKey = IdHasher.Instance.GetHash( destinationGroupTypeId.Value );

            box.DestinationGroupTypeRoles = destinationGroupType?.Roles
                .Select( r => new DestinationGroupTypeRoleBag
                {
                    IdKey = IdHasher.Instance.GetHash( r.Id ),
                    Name = r.Name,
                    MaxCount = r.MaxCount,
                    MinCount = r.MinCount,
                } )
                .ToList();

            Rock.Model.Group fakeDestinationGroup = new Rock.Model.Group
            {
                GroupTypeId = destinationGroupType.Id,
            };

            fakeDestinationGroup.LoadAttributes();

            box.PlacementConfigurationSettingOptions.DestinationGroupAttributes = fakeDestinationGroup.GetPublicAttributeListItemsForView( GetCurrentPerson(), true );
            box.AttributesForGroupAdd = fakeDestinationGroup.GetPublicAttributesForEdit( GetCurrentPerson(), true );
            box.IsPersonPermittedToEditGroupType = fakeDestinationGroup.IsAuthorized( Authorization.EDIT, GetCurrentPerson() );

            GroupMember fakeDestinationGroupMember = new GroupMember
            {
                GroupTypeId = destinationGroupType.Id,
            };

            fakeDestinationGroupMember.LoadAttributes();

            box.PlacementConfigurationSettingOptions.DestinationGroupMemberAttributes = fakeDestinationGroupMember.GetPublicAttributeListItemsForView( GetCurrentPerson(), true );

            return box;
        }

        /// <summary>
        /// Gets the Group Placement Initialization Box if we are handling Registration Group Placements.
        /// </summary>
        /// <param name="box">The Group Placement Initialization Box</param>
        /// <param name="destinationGroupTypeId">The Destination Group Type Id</param>
        /// <returns>The box.</returns>
        private GroupPlacementInitializationBox GetBoxForRegistrationPlacement( GroupPlacementInitializationBox box, out int? destinationGroupTypeId )
        {
            var registrationTemplatePlacementId = GetIdFromPageParameter( PageParameterKey.RegistrationTemplatePlacementId );
            var registrationInstanceId = GetIdFromPageParameter( PageParameterKey.RegistrationInstanceId );

            var registrationTemplateService = new RegistrationTemplateService( RockContext );
            var registrationInstanceService = new RegistrationInstanceService( RockContext );

            destinationGroupTypeId = null;

            // Pull the Registration Template from the Block Attribute if available.
            int? registrationTemplateId;
            Guid? registrationTemplateGuid = GetAttributeValue( AttributeKey.RegistrationTemplate ).AsGuidOrNull();
            if ( registrationTemplateGuid.HasValue )
            {
                registrationTemplateId = registrationTemplateService.GetId( registrationTemplateGuid.Value );
            }
            else
            {
                registrationTemplateId = GetIdFromPageParameter( PageParameterKey.RegistrationTemplateId );
            }

            // If we are in Registration Instance mode then we'll use the Registration Template from the instance.
            RegistrationInstance registrationInstance = null;
            if ( registrationInstanceId.HasValue )
            {
                registrationInstance = registrationInstanceService.Get( registrationInstanceId.Value );
                if ( registrationInstance == null )
                {
                    box.ErrorMessage = "Invalid Registration Instance";
                    return box;
                }

                registrationTemplateId = registrationInstance.RegistrationTemplateId;
                box.GroupPlacementKeys.RegistrationInstanceGuid = registrationInstance.Guid;
            }

            // Make sure a valid RegistrationTemplate specified (or determined from RegistrationInstanceId )
            var registrationTemplate = registrationTemplateId.HasValue
                ? registrationTemplateService.Get( registrationTemplateId.Value )
                : null;

            if ( registrationTemplate == null )
            {
                box.ErrorMessage = "Invalid Registration Template";
                return box;
            }

            RegistrationTemplatePlacement registrationTemplatePlacement = null;
            RegistrationTemplatePlacementService registrationTemplatePlacementService = new RegistrationTemplatePlacementService( RockContext );
            if ( registrationTemplatePlacementId.HasValue )
            {
                registrationTemplatePlacement = registrationTemplatePlacementService.Get( registrationTemplatePlacementId.Value );

                // If the registration template placement is not valid or belongs to a different registration
                // template, don't use it.
                if ( registrationTemplatePlacement == null || registrationTemplatePlacement.RegistrationTemplateId != registrationTemplateId )
                {
                    registrationTemplatePlacement = null;
                    registrationTemplatePlacementId = null;
                }
                else
                {
                    destinationGroupTypeId = registrationTemplatePlacement.GroupTypeId;
                }
            }

            if ( registrationTemplatePlacement == null )
            {
                // Binds the selectable placements ( Buses, Cabins, etc. )
                var registrationTemplatePlacements = registrationTemplateService
                    .GetSelect( registrationTemplateId.Value, s => s.Placements )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .ToList();

                if ( !registrationTemplatePlacements.Any() )
                {
                    box.ErrorMessage = "No Placement Types available for this Registration Template.";
                    return box;
                }

                box.RegistrationTemplatePlacements = registrationTemplatePlacements.Select( r => new ListItemBag
                {
                    Text = r.Name,
                    Value = r.Guid.ToString()
                } ).ToList();

                // If the person has a fallback Registration Template Placement stored in Person Preferences than attempt to use it.
                if ( FallbackRegistrationTemplatePlacement.HasValue )
                {
                    var fallbackRegistrationTemplatePlacement = registrationTemplatePlacementService.Get( FallbackRegistrationTemplatePlacement.Value );
                    if ( fallbackRegistrationTemplatePlacement?.RegistrationTemplateId == registrationTemplateId )
                    {
                        registrationTemplatePlacement = fallbackRegistrationTemplatePlacement;
                        destinationGroupTypeId = fallbackRegistrationTemplatePlacement.GroupTypeId;
                    }
                }

                // If there is still no placement selected, use the first available one
                if ( registrationTemplatePlacement == null )
                {
                    registrationTemplatePlacement = registrationTemplatePlacements.First();
                    destinationGroupTypeId = registrationTemplatePlacements.First().GroupTypeId;
                }
            }

            if ( !destinationGroupTypeId.HasValue )
            {
                box.ErrorMessage = "Destination Group Type was not found.";
                return box;
            }

            if ( !registrationInstanceId.HasValue )
            {
                box.PlacementMode = PlacementMode.TemplateMode;
                box.Title = $"Group Placement - {registrationTemplate.Name} - {registrationTemplatePlacement.Name}";
            }
            else
            {
                box.PlacementMode = PlacementMode.InstanceMode;
                box.Title = $"Group Placement - {registrationInstance.Name} - {registrationTemplatePlacement.Name}";
            }

            box.IsPlacementAllowingMultiple = registrationTemplatePlacement.AllowMultiplePlacements;
            box.GroupPlacementKeys.RegistrationInstanceIdKey = registrationInstanceId.HasValue ? IdHasher.Instance.GetHash( registrationInstanceId.Value ) : null;
            box.GroupPlacementKeys.RegistrationTemplateIdKey = registrationTemplateId.HasValue ? IdHasher.Instance.GetHash( registrationTemplateId.Value ) : null;
            box.GroupPlacementKeys.RegistrationTemplateGuid = registrationTemplate.Guid;
            box.GroupPlacementKeys.RegistrationTemplatePlacementIdKey = registrationTemplatePlacement?.Id != null ? IdHasher.Instance.GetHash( registrationTemplatePlacement.Id ) : null;
            box.PlacementConfigurationSettingOptions.SourceAttributes = GetRegistrantAttributesAsListItems( registrationTemplateId );

            if ( box.PlacementMode == PlacementMode.TemplateMode )
            {
                box.PlacementConfigurationSettingOptions.RegistrationInstances = registrationInstanceService.Queryable()
                    .Where( a => a.RegistrationTemplateId == registrationTemplateId )
                    .OrderBy( a => a.Name )
                    .ToList()
                    .Select( a => new ListItemBag
                    {
                        Value = a.IdKey,
                        Text = a.Name
                    } )
                    .ToList();
            }

            return box;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.GroupDetailPage] = this.GetLinkedPageUrl( AttributeKey.GroupDetailPage, new Dictionary<string, string> { ["GroupId"] = "((Key))", ["autoEdit"] = "true", ["returnUrl"] = this.GetCurrentPageUrl() } ),
                [NavigationUrlKey.GroupMemberDetailPage] = this.GetLinkedPageUrl( AttributeKey.GroupMemberDetailPage, new Dictionary<string, string> { ["GroupMemberId"] = "((Key))", ["autoEdit"] = "true", ["returnUrl"] = this.GetCurrentPageUrl() } ),
            };
        }

        /// <summary>
        /// Gets the "Back" page URL.
        /// </summary>
        private string GetBackPageUrl()
        {
            var returnUrl = this.PageParameter( PageParameterKey.ReturnUrl );
            if ( returnUrl.IsNotNullOrWhiteSpace() )
            {
                return returnUrl;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets Registrant Attributes as a List of ListItemBags
        /// </summary>
        /// <param name="registrationTemplateId">The Registration Template Id</param>
        /// <returns>Registrant Attribute List of ListItemBags</returns>
        private List<ListItemBag> GetRegistrantAttributesAsListItems( int? registrationTemplateId )
        {
            var listItems = new List<ListItemBag>();

            if ( !registrationTemplateId.HasValue )
            {
                return listItems;
            }

            var templateForms = new RegistrationTemplateService( RockContext )
                .GetSelect( registrationTemplateId.Value, t => t.Forms )?
                .ToList();

            if ( templateForms == null )
            {
                return listItems;
            }

            foreach ( var form in templateForms )
            {
                var fields = form.Fields
                    .OrderBy( f => f.Order )
                    .Where( f => f.FieldSource == RegistrationFieldSource.RegistrantAttribute && f.AttributeId.HasValue )
                    .ToList();

                foreach ( var field in fields )
                {
                    var attribute = AttributeCache.Get( field.AttributeId.Value );
                    if ( attribute != null )
                    {
                        listItems.Add( new ListItemBag
                        {
                            Text = attribute.Name,
                            Value = attribute.Guid.ToString(),
                        } );
                    }
                }
            }

            return listItems;
        }

        /// <summary>
        /// Gets the Placement Configuration.
        /// </summary>
        /// <returns>The Placement Configuration</returns>
        private PlacementConfigurationSettingsBag GetPlacementConfiguration( GroupPlacementKeysBag groupPlacementKeys )
        {
            var preferences = GetBlockPersonPreferences();

            string placementConfigurationJSON = string.Empty;
            if ( groupPlacementKeys.RegistrationInstanceIdKey.IsNotNullOrWhiteSpace() )
            {
                placementConfigurationJSON = preferences.GetValue( string.Format( PreferenceKey.PlacementConfigurationJSONRegistrationInstanceIdKey, groupPlacementKeys.RegistrationInstanceIdKey ) );
            }
            else if ( groupPlacementKeys.RegistrationTemplateIdKey.IsNotNullOrWhiteSpace() )
            {
                placementConfigurationJSON = preferences.GetValue( string.Format( PreferenceKey.PlacementConfigurationJSONRegistrationTemplateIdKey, groupPlacementKeys.RegistrationTemplateIdKey ) );
            }
            else if ( groupPlacementKeys.SourceGroupIdKey.IsNotNullOrWhiteSpace() )
            {
                placementConfigurationJSON = preferences.GetValue( string.Format( PreferenceKey.PlacementConfigurationJSONSourceGroupIdKey, groupPlacementKeys.SourceGroupIdKey ) );
            }
            else if ( groupPlacementKeys.EntitySetIdKey.IsNotNullOrWhiteSpace() )
            {
                placementConfigurationJSON = preferences.GetValue( string.Format( PreferenceKey.PlacementConfigurationJSONEntitySetIdKey, groupPlacementKeys.EntitySetIdKey ) );
            }

            return placementConfigurationJSON.FromJsonOrNull<PlacementConfigurationSettingsBag>() ?? new PlacementConfigurationSettingsBag();
        }

        /// <summary>
        /// Gets the selected Registrant Fee Items from preferences for filtering.
        /// </summary>
        /// <param name="groupPlacementKeys">The Group Placement Keys</param>
        /// <returns>The Registrant Fee Items</returns>
        private List<string> GetRegistrantFeeItemValuesForFilters( GroupPlacementKeysBag groupPlacementKeys )
        {
            var preferences = GetBlockPersonPreferences();

            string registrantFeeItemValuesJSON = string.Empty;
            if ( groupPlacementKeys.RegistrationInstanceIdKey.IsNotNullOrWhiteSpace() )
            {
                registrantFeeItemValuesJSON = preferences.GetValue( string.Format( PreferenceKey.RegistrantFeeItemValuesForFiltersJSONRegistrationInstanceIdKey, groupPlacementKeys.RegistrationInstanceIdKey ) );
            }
            else if ( groupPlacementKeys.RegistrationTemplateIdKey.IsNotNullOrWhiteSpace() )
            {
                registrantFeeItemValuesJSON = preferences.GetValue( string.Format( PreferenceKey.RegistrantFeeItemValuesForFiltersJSONRegistrationTemplateIdKey, groupPlacementKeys.RegistrationTemplateIdKey ) );
            }

            return registrantFeeItemValuesJSON.FromJsonOrNull<List<string>>() ?? new List<string>();
        }

        /// <summary>
        /// Determines if people can be placed in the specified Placement Group.
        /// </summary>
        /// <param name="personIds">The pending placement people</param>
        /// <param name="destinationGroupTypeIdKey">The Destination Group Type IdKey</param>
        /// <param name="groupIdKey">The specified Placement Group's IdKey</param>
        /// <param name="errorMessage">The error message</param>
        /// <returns>A boolean determining if the people can or cannot be placed.</returns>
        private bool CanPlacePeople( List<int?> personIds, string destinationGroupTypeIdKey, string groupIdKey, out string errorMessage )
        {
            var destinationGroupTypeId = Rock.Utility.IdHasher.Instance.GetId( destinationGroupTypeIdKey );

            if ( !destinationGroupTypeId.HasValue )
            {
                errorMessage = "Specified Destination Group Type not found.";
                return false;
            }

            var group = new GroupService( RockContext ).Get( groupIdKey );

            if ( group == null )
            {
                errorMessage = "Specified Group not found.";
                return false;
            }

            var peopleInfo = new PersonService( RockContext )
                .Queryable()
                .Where( p => personIds.Contains( p.Id ) )
                .Select( p => new
                {
                    p.Id,
                } )
                .ToList();

            if ( !peopleInfo.Any() )
            {
                errorMessage = "No valid people to place found.";
                return false;
            }

            if ( !group.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) && !group.IsAuthorized( Authorization.MANAGE_MEMBERS, GetCurrentPerson() ) )
            {
                errorMessage = "You are not authorized to place people in this group.";
                return false;
            }

            if ( destinationGroupTypeId != group.GroupTypeId )
            {
                errorMessage = "The specified group's Group Type does not match the group type of the Destination Group.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Determines if the group has enough available capacity for the pending Placement People
        /// </summary>
        /// <param name="roleId">The specified Role Id that the people are being placed into</param>
        /// <param name="groupId">The specified Placement Group</param>
        /// <param name="pendingGroupMemberCount">The total amount of pending placement people</param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private bool IsGroupRoleCapacityAvailable( int? roleId, int groupId, int pendingGroupMemberCount, out string errorMessage )
        {
            if (!roleId.HasValue)
            {
                errorMessage = "The specified Role Id is not valid.";
                return false;
            }

            var group = GroupCache.Get( groupId );
            var role = GroupTypeCache.Get( group.GroupTypeId )?.Roles.FirstOrDefault( r => r.Id == roleId.Value );

            if ( role?.MaxCount is int maxCount )
            {
                var currentCount = new GroupMemberService( RockContext )
                    .Queryable()
                    .Count( gm => gm.GroupId == group.Id && gm.GroupRoleId == roleId.Value );

                if ( currentCount + pendingGroupMemberCount > maxCount )
                {
                    errorMessage = $"The number of {role.Name.Pluralize().ToLower()} for this group is above its maximum allowed limit of {maxCount} active members.";
                    return false;
                }
            }
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Determines whether [is valid parent group] [the specified parent group].
        /// </summary>
        /// <param name="parentGroup">The parent group.</param>
        /// <param name="childGroupTypeId">The child group type identifier.</param>
        /// <returns>
        ///   <c>true</c> if [is valid parent group] [the specified parent group]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidParentGroup( Rock.Model.Group parentGroup, int childGroupTypeId )
        {
            bool isValidParentGroup = true;

            if ( parentGroup != null )
            {
                var parentGroupGroupType = GroupTypeCache.Get( parentGroup.GroupTypeId );
                isValidParentGroup = parentGroupGroupType.AllowAnyChildGroupType || parentGroupGroupType.ChildGroupTypes.Any( a => a.Id == childGroupTypeId );
            }

            return isValidParentGroup;
        }

        /// <summary>
        /// Determines whether [is valid existing group] [the specified selected group].
        /// </summary>
        /// <param name="selectedGroup">The selected group.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns>
        ///   <c>true</c> if [is valid existing group] [the specified selected group]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidExistingGroup( Rock.Model.Group selectedGroup, int groupTypeId )
        {
            return selectedGroup?.GroupTypeId == groupTypeId;
        }

        /// <summary>
        /// Determines whether [has valid child groups] [the specified parent group identifier].
        /// </summary>
        /// <param name="parentGroupId">The parent group identifier.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if [has valid child groups] [the specified parent group identifier]; otherwise, <c>false</c>.
        /// </returns>
        private bool HasValidChildGroups( int parentGroupId, int groupTypeId, out string errorMessage )
        {
            var childPlacementGroups = new GroupService( new RockContext() ).Queryable().Where( a => a.ParentGroupId == parentGroupId && a.IsActive == true ).ToList();
            if ( childPlacementGroups.Count() == 0 )
            {
                errorMessage = "The selected parent group does not have any active child groups.";
                return false;
            }

            List<Rock.Model.Group> invalidGroups = new List<Rock.Model.Group>();
            foreach ( var childPlacementGroup in childPlacementGroups )
            {
                if ( !IsValidExistingGroup( childPlacementGroup, groupTypeId ) )
                {
                    invalidGroups.Add( childPlacementGroup );
                }
            }

            if ( invalidGroups.Any() )
            {
                var groupType = GroupTypeCache.Get( groupTypeId );
                errorMessage = string.Format( "The child groups of this parent group must be {0} groups.", groupType );
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Returns a dictionary of formatted registration fees for a specific registrant,
        /// including a description and total cost per unique fee item.
        /// </summary>
        /// <param name="personGroup">A collection of <see cref="PlacementPeopleResult"/> objects representing registrants and their associated fees.</param>
        /// <param name="registrantId">The ID of the registrant to filter fee items for.</param>
        /// <returns>
        /// A dictionary where each key is a hashed FeeItemId, and each value is a <see cref="ListItemBag"/>
        /// containing a formatted label and cost string for the fee.
        /// </returns>
        private Dictionary<string, ListItemBag> GetFormattedFeesForRegistrant( IEnumerable<PlacementPeopleResult> personGroup, int registrantId )
        {
            return personGroup
                .Where( f => f.RegistrantId == registrantId && f.Quantity > 0 && f.FeeItemId.HasValue )
                .DistinctBy( f => f.FeeItemId )
                .ToDictionary(
                    f => IdHasher.Instance.GetHash( f.FeeItemId.Value ),
                    f =>
                    {
                        var feeLabel = f.FeeType == RegistrationFeeType.Multiple && f.Option.IsNotNullOrWhiteSpace()
                            ? $"{f.FeeName}-{f.Option}"
                            : f.Option;
                        var cost = f.Cost ?? 0;
                        var costText = f.Quantity.Value > 1
                            ? $"{f.Quantity.Value} at {cost.FormatAsCurrency()}"
                            : ( f.Quantity.Value * cost ).FormatAsCurrency();

                        return new ListItemBag
                        {
                            Text = feeLabel,
                            Value = costText
                        };
                    }
                );
        }

        /// <summary>
        /// Generates a list of entities (either <see cref="RegistrationRegistrant"/> or <see cref="GroupMember"/>) from placement results,
        /// with only the specified attributes loaded for each entity. The result depends on the generic type <typeparamref name="T"/> and other filter conditions.
        /// </summary>
        /// <typeparam name="T">
        /// The entity type to return, which must implement <see cref="IEntity"/>. Expected values are <see cref="RegistrationRegistrant"/> or <see cref="GroupMember"/>.
        /// </typeparam>
        /// <param name="placementPeopleResults">The list of <see cref="PlacementPeopleResult"/> records to derive entities from.</param>
        /// <param name="registrationTemplateId">The registration template ID used to instantiate <see cref="RegistrationRegistrant"/> entities.</param>
        /// <param name="sourceGroupId">The ID of the source group, used to determine which <see cref="GroupMember"/> records to include or exclude.</param>
        /// <param name="isFetchingDestinationGroupMembers">Indicates whether to fetch group members from a destination group rather than the source group.</param>
        /// <param name="displayedAttributeIds">A set of attribute IDs to load for each entity. If null, no entities are returned.</param>
        /// <returns>A list of entities of type <typeparamref name="T"/> with the specified attributes loaded.</returns>
        private List<T> GetPlacementPeopleEntities<T>(
            List<PlacementPeopleResult> placementPeopleResults,
            int? registrationTemplateId,
            int? sourceGroupId,
            bool isFetchingDestinationGroupMembers,
            HashSet<int> displayedAttributeIds )
            where T : IEntity
        {
            List<T> entitiesWithLoadedAttributes = new List<T>();

            // If there are no attributes to display then return an empty list.
            if ( displayedAttributeIds == null )
            {
                return entitiesWithLoadedAttributes;
            }

            if ( typeof( T ) == typeof( RegistrationRegistrant ) )
            {
                if ( registrationTemplateId.HasValue )
                {
                    var registrants = placementPeopleResults
                        .Where( p => p.RegistrantId.HasValue )
                        .DistinctBy( p => p.RegistrantId )
                        .Select( p => new RegistrationRegistrant
                        {
                            Id = p.RegistrantId.Value,
                            RegistrationTemplateId = registrationTemplateId.Value,
                        } )
                        .ToList();

                    registrants.LoadFilteredAttributes( a => displayedAttributeIds.Contains( a.Id ) );
                    entitiesWithLoadedAttributes = registrants.Cast<T>().ToList();
                }
            }
            else if ( typeof( T ) == typeof( GroupMember ) )
            {
                if ( sourceGroupId.HasValue && !isFetchingDestinationGroupMembers )
                {
                    var members = placementPeopleResults
                        .Where( p => p.GroupMemberId.HasValue && p.GroupRoleId.HasValue && p.GroupTypeId.HasValue )
                        .DistinctBy( p => p.GroupMemberId )
                        .Select( p => new GroupMember
                        {
                            Id = p.GroupMemberId.Value,
                            GroupRoleId = p.GroupRoleId.Value,
                            GroupTypeId = p.GroupTypeId.Value,
                            GroupId = sourceGroupId.Value
                        } )
                        .ToList();

                    members.LoadFilteredAttributes( a => displayedAttributeIds.Contains( a.Id ) );
                    entitiesWithLoadedAttributes = members.Cast<T>().ToList();
                }
                else
                {
                    var members = placementPeopleResults
                        .Where( p => p.GroupId.HasValue && p.GroupMemberId.HasValue && p.GroupRoleId.HasValue && p.GroupTypeId.HasValue && p.GroupId.Value != sourceGroupId )
                        .DistinctBy( p => p.GroupMemberId )
                        .Select( p => new GroupMember
                        {
                            Id = p.GroupMemberId.Value,
                            GroupRoleId = p.GroupRoleId.Value,
                            GroupTypeId = p.GroupTypeId.Value,
                            GroupId = p.GroupId.Value
                        } )
                        .ToList();

                    members.LoadFilteredAttributes( a => displayedAttributeIds.Contains( a.Id ) );
                    entitiesWithLoadedAttributes = members.Cast<T>().ToList();
                }
            }

            return entitiesWithLoadedAttributes;
        }

        #endregion Methods

        #region Helper Methods

        /// <summary>
        /// Gets the ID from the page parameter accounting for idKeys (Obsidian) or plain ids (Webforms).
        /// </summary>
        /// <param name="pageParameterKey">The key of the page parameter.</param>
        /// <returns>The ID as an integer if found; otherwise, null.</returns>
        private int? GetIdFromPageParameter( string pageParameterKey )
        {
            var idParam = PageParameter( pageParameterKey );
            return Rock.Utility.IdHasher.Instance.GetId( idParam ) ?? idParam.AsIntegerOrNull();
        }

        /// <summary>
        /// Retrieves and parses the destination group IDs from the page parameter, converting hashed or plain string values into
        /// a comma-separated list of numeric group IDs.
        /// </summary>
        /// <returns>A comma-separated string of valid destination group IDs, or <c>null</c> if no valid IDs are found.</returns>
        private string GetIdsFromDestinationGroupParam()
        {
            var destinationGroupIdParams = PageParameter( PageParameterKey.DestinationGroup ).Split( ',' );
            var destinationGroupIds = destinationGroupIdParams
                .Select( param => Rock.Utility.IdHasher.Instance.GetId( param ) ?? param.AsIntegerOrNull() )
                .Where( id => id.HasValue )
                .Select( id => id.Value.ToString() )
                .ToList();

            return destinationGroupIds.Any() ? string.Join( ",", destinationGroupIds ) : null;
        }

        /// <summary>
        /// Converts a list of attribute GUID strings to a HashSet of attribute IDs,
        /// skipping any invalid or non-existent GUIDs.
        /// </summary>
        /// <param name="attributeGuids">A collection of attribute GUID strings.</param>
        /// <returns>A HashSet of attribute IDs corresponding to the provided GUIDs.</returns>
        public static HashSet<int> GetAttributeIdsFromGuids( IEnumerable<string> attributeGuids )
        {
            if ( attributeGuids == null )
            {
                return new HashSet<int>();
            }

            return attributeGuids
                .Select( guidString =>
                {
                    if ( Guid.TryParse( guidString, out var guid ) )
                    {
                        var attribute = AttributeCache.Get( guid );
                        return attribute?.Id;
                    }

                    return null;
                } )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToHashSet();
        }

        #endregion Helper Methods

        #region Block Actions

        /// <summary>
        /// Subscribes the client (based on the provided connection ID) to one or more real-time topic channels related to group placement updates.
        /// These may include channels for specific groups, registration instances, or registration templates,
        /// enabling the client to receive live updates for group member or registrant changes.
        /// </summary>
        /// <param name="realTimeConnectionKeysBag">A bag of real-time connection keys.</param>
        [BlockAction( "SubscribeToRealTime" )]
        public async Task<BlockActionResult> SubscribeToRealTime( RealTimeConnectionKeysBag realTimeConnectionKeysBag )
        {
            var groupService = new GroupService( RockContext );
            List<Rock.Model.Group> groups = new List<Rock.Model.Group>();

            if ( realTimeConnectionKeysBag.GroupGuids.Any() )
            {
                groups = groupService
                    .Queryable()
                    .Where( g => realTimeConnectionKeysBag.GroupGuids.Contains( g.Guid ) )
                    .ToList();
            }

            var topicChannels = RealTimeHelper.GetTopicContext<IGroupPlacement>().Channels;

            foreach ( var group in groups )
            {
                if ( group == null )
                {
                    return ActionNotFound( "Group not found." );
                }

                // Authorize the current user.
                if ( !group.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.Forbidden );
                }
                await topicChannels.AddToChannelAsync( realTimeConnectionKeysBag.ConnectionId, GroupPlacementTopic.GetGroupMemberChannelForGroup( group.Guid ) );
                await topicChannels.AddToChannelAsync( realTimeConnectionKeysBag.ConnectionId, GroupPlacementTopic.GetGroupMemberDeletedChannel() );
            }

            if ( realTimeConnectionKeysBag.RegistrationInstanceGuid.HasValue )
            {
                await topicChannels.AddToChannelAsync( realTimeConnectionKeysBag.ConnectionId, GroupPlacementTopic.GetRegistrantChannelForRegistrationInstance( realTimeConnectionKeysBag.RegistrationInstanceGuid.Value ) );
                await topicChannels.AddToChannelAsync( realTimeConnectionKeysBag.ConnectionId, GroupPlacementTopic.GetRegistrantDeletedChannel() );
            }
            else if ( realTimeConnectionKeysBag.RegistrationTemplateGuid.HasValue )
            {
                await topicChannels.AddToChannelAsync( realTimeConnectionKeysBag.ConnectionId, GroupPlacementTopic.GetRegistrantChannelForRegistrationTemplate( realTimeConnectionKeysBag.RegistrationTemplateGuid.Value ) );
                await topicChannels.AddToChannelAsync( realTimeConnectionKeysBag.ConnectionId, GroupPlacementTopic.GetRegistrantDeletedChannel() );
            }

            return ActionOk();
        }

        /// <summary>
        /// Gets the Placement People (placed and unplaced) for the specified Group Placement Keys and Placement Mode.
        /// </summary>
        /// <param name="groupPlacementKeys">A bag of Group Placement Keys</param>
        /// <param name="isPlacementAllowingMultiple">The boolean value indicating if this configuration will allow for people being placed multiple times.</param>
        /// <param name="placementMode">The mode of placement being performed (TemplateMode, InstanceMode, GroupMode, EntitySetMode).</param>
        [BlockAction]
        public BlockActionResult GetPlacementPeople( GroupPlacementKeysBag groupPlacementKeys, Boolean isPlacementAllowingMultiple, PlacementMode placementMode )
        {
            int registrationTemplatePlacementEntityTypeId = EntityTypeCache.Get<Rock.Model.RegistrationTemplatePlacement>().Id;
            int registrationInstanceEntityTypeId = EntityTypeCache.Get<Rock.Model.RegistrationInstance>().Id;
            int personEntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
            int? sourceEntityTypeId = null;
            int targetEntityTypeId = EntityTypeCache.Get<Rock.Model.Group>().Id;
            int? sourceEntityId = null;
            string purposeKey = null;
            string includedRegistrationInstanceIds = null;
            bool includeFees = false;
            string includedFeeItemIds = null;
            string registrationTemplatePurposeKey = RelatedEntityPurposeKey.RegistrationTemplateGroupPlacementTemplate;
            string registrationInstancePurposeKey = RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement;
            List<PlacementPeopleResult> placementPeopleResults;

            var registrationInstanceId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys?.RegistrationInstanceIdKey );
            var registrationTemplateId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys?.RegistrationTemplateIdKey );
            var registrationTemplatePlacementId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys?.RegistrationTemplatePlacementIdKey );
            var sourceGroupId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys?.SourceGroupIdKey );
            var entitySetId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys?.EntitySetIdKey );
            var destinationGroupTypeId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys?.DestinationGroupTypeIdKey );

            // If all of these ID's do not have a value than we cannot retrieve Placement People.
            if ( !registrationTemplateId.HasValue && !registrationInstanceId.HasValue && !sourceGroupId.HasValue && !entitySetId.HasValue )
            {
                return ActionNotFound( "Missing required keys to retrieve Placement People." );
            }

            // If there is no specified Destination Group Type than we cannot retrieve Placement People.
            if ( !destinationGroupTypeId.HasValue )
            {
                return ActionNotFound( "Could not find Destination Group Type Id" );
            }

            var destinationGroupIds = GetIdsFromDestinationGroupParam();
            var placementConfiguration = GetPlacementConfiguration( groupPlacementKeys );

            string displayedCampusGuid = null;
            if ( placementConfiguration.DisplayedCampus?.Value.IsNotNullOrWhiteSpace() == true)
            {
                displayedCampusGuid = placementConfiguration.DisplayedCampus.Value;
            }

            if ( placementMode == PlacementMode.TemplateMode || placementMode == PlacementMode.InstanceMode )
            {
                if ( !registrationTemplatePlacementId.HasValue )
                {
                    return ActionBadRequest( "Could not find Registration Template Placement Id" );
                }

                includeFees = placementConfiguration.AreFeesDisplayed;
                var includedFeeItemIdsFromPreferences = GetRegistrantFeeItemValuesForFilters( groupPlacementKeys );
                includedFeeItemIds = includedFeeItemIdsFromPreferences.Any() ? string.Join( ",", includedFeeItemIdsFromPreferences ) : null;

                if ( placementMode == PlacementMode.TemplateMode )
                {
                    includedRegistrationInstanceIds = ( placementConfiguration.IncludedRegistrationInstanceIds?.Any() == true )
                        ? string.Join(
                            ",",
                            placementConfiguration.IncludedRegistrationInstanceIds
                                .Select( idKey => Rock.Utility.IdHasher.Instance.GetId( idKey ) )
                                .Where( id => id.HasValue )
                                .Select( id => id.Value )
                        )
                        : null;
                }
            }
            else if ( placementMode == PlacementMode.GroupMode )
            {
                sourceEntityTypeId = EntityTypeCache.Get<Rock.Model.Group>().Id;
                sourceEntityId = sourceGroupId;
                purposeKey = RelatedEntityPurposeKey.GroupPlacement;
            }
            else if ( placementMode == PlacementMode.EntitySetMode )
            {
                sourceEntityTypeId = EntityTypeCache.Get<Rock.Model.EntitySet>().Id;
                sourceEntityId = entitySetId;
                purposeKey = RelatedEntityPurposeKey.EntitySetPlacement;
            }

            placementPeopleResults = RockContext.Database.SqlQuery<PlacementPeopleResult>(
                $@"EXEC [dbo].[spGetGroupPlacementPeople] 
                                @{nameof( registrationTemplatePlacementEntityTypeId )}, 
                                @{nameof( registrationInstanceEntityTypeId )}, 
                                @{nameof( personEntityTypeId )}, 
                                @{nameof( sourceEntityTypeId )}, 
                                @{nameof( targetEntityTypeId )}, 
                                @{nameof( registrationTemplateId )}, 
                                @{nameof( registrationInstanceId )}, 
                                @{nameof( registrationTemplatePlacementId )},
                                @{nameof( sourceEntityId )},
                                @{nameof( placementMode )},
                                @{nameof( includedRegistrationInstanceIds )},
                                @{nameof( includeFees )},
                                @{nameof( includedFeeItemIds )},
                                @{nameof( destinationGroupTypeId )},
                                @{nameof( destinationGroupIds )},
                                @{nameof( displayedCampusGuid )},
                                @{nameof( purposeKey )},
                                @{nameof( registrationTemplatePurposeKey )},
                                @{nameof( registrationInstancePurposeKey )}",
                new SqlParameter( nameof( registrationTemplatePlacementEntityTypeId ), registrationTemplatePlacementEntityTypeId ),
                new SqlParameter( nameof( registrationInstanceEntityTypeId ), registrationInstanceEntityTypeId ),
                new SqlParameter( nameof( personEntityTypeId ), personEntityTypeId ),
                new SqlParameter( nameof( sourceEntityTypeId ), ( object ) sourceEntityTypeId ?? DBNull.Value ),
                new SqlParameter( nameof( targetEntityTypeId ), targetEntityTypeId ),
                new SqlParameter( nameof( registrationTemplateId ), ( object ) registrationTemplateId ?? DBNull.Value ),
                new SqlParameter( nameof( registrationInstanceId ), ( object ) registrationInstanceId ?? DBNull.Value ),
                new SqlParameter( nameof( registrationTemplatePlacementId ), ( object ) registrationTemplatePlacementId ?? DBNull.Value ),
                new SqlParameter( nameof( sourceEntityId ), ( object ) sourceEntityId ?? DBNull.Value ),
                new SqlParameter( nameof( placementMode ), placementMode.ToString() ),
                new SqlParameter( nameof( includedRegistrationInstanceIds ), ( object ) includedRegistrationInstanceIds ?? DBNull.Value ),
                new SqlParameter( nameof( includeFees ), includeFees ),
                new SqlParameter( nameof( includedFeeItemIds ), ( object ) includedFeeItemIds ?? DBNull.Value ),
                new SqlParameter( nameof( destinationGroupTypeId ), destinationGroupTypeId.Value ),
                new SqlParameter( nameof( destinationGroupIds ), ( object ) destinationGroupIds ?? DBNull.Value ),
                new SqlParameter( nameof( displayedCampusGuid ), ( object ) displayedCampusGuid ?? DBNull.Value ),
                new SqlParameter( nameof( purposeKey ), ( object ) purposeKey ?? DBNull.Value ),
                new SqlParameter( nameof( registrationTemplatePurposeKey ), registrationTemplatePurposeKey ),
                new SqlParameter( nameof( registrationInstancePurposeKey ), registrationInstancePurposeKey )
            ).ToList();

            HashSet<int> displayedSourceAttributeIds = null;
            HashSet<int> displayedDestinationGroupMemberAttributeIds = null;

            if ( placementConfiguration.SourceAttributesToDisplay?.Any() == true )
            {
                displayedSourceAttributeIds = GetAttributeIdsFromGuids( placementConfiguration.SourceAttributesToDisplay );
            }

            if ( placementConfiguration.DestinationGroupMemberAttributesToDisplay?.Any() == true )
            {
                displayedDestinationGroupMemberAttributeIds = GetAttributeIdsFromGuids( placementConfiguration.DestinationGroupMemberAttributesToDisplay );
            }

            List<RegistrationRegistrant> registrants = new List<RegistrationRegistrant>();
            List<GroupMember> sourceGroupMembers = new List<GroupMember>();

            if ( placementMode == PlacementMode.TemplateMode || placementMode == PlacementMode.InstanceMode )
            {
                // Get the Registrant entities from Placement People
                registrants = GetPlacementPeopleEntities<RegistrationRegistrant>(
                    placementPeopleResults,
                    registrationTemplateId,
                    null,
                    false,
                    displayedSourceAttributeIds
                );
            }
            else if ( placementMode == PlacementMode.GroupMode )
            {
                // Get the GroupMember entities from Placement People for the source group.
                sourceGroupMembers = GetPlacementPeopleEntities<GroupMember>(
                    placementPeopleResults,
                    null,
                    sourceGroupId,
                    false,
                    displayedSourceAttributeIds
                );
            }

            // Get the GroupMember entities from Placement People for the destination group(s).
            List<GroupMember> destinationGroupMembers = GetPlacementPeopleEntities<GroupMember>(
                placementPeopleResults,
                null,
                sourceGroupId,
                true,
                displayedDestinationGroupMemberAttributeIds
            );

            var placementPeopleBag = new PlacementPeopleBag
            {
                TempGroups = new List<PlacementGroupBag>(),
                PeopleToPlace = new List<PersonBag>()
            };

            // Group flat results by PersonId
            var peopleGroups = placementPeopleResults
                .GroupBy( r => r.PersonId )
                .ToList();

            foreach ( var personGroup in peopleGroups )
            {
                var firstResult = personGroup.First();

                var personBag = new PersonBag
                {
                    PersonIdKey = IdHasher.Instance.GetHash( firstResult.PersonId ),
                    FirstName = firstResult.FirstName,
                    NickName = firstResult.NickName,
                    LastName = firstResult.LastName,
                    Gender = firstResult.Gender,
                    PhotoUrl = Rock.Model.Person.GetPersonPhotoUrl(
                        $"{firstResult.NickName.Truncate( 1, false )}{firstResult.LastName.Truncate( 1, false )}",
                        firstResult.PhotoId,
                        firstResult.Age,
                        firstResult.Gender,
                        firstResult.RecordTypeValueId,
                        firstResult.AgeClassification
                    )
                };

                if ( placementMode == PlacementMode.InstanceMode || placementMode == PlacementMode.TemplateMode )
                {
                    personBag.Registrants = personGroup
                        .Where( r => r.RegistrantId.HasValue )
                        .DistinctBy( r => r.RegistrantId )
                        .Select( r => {
                            var registrantBag = new RegistrantBag
                            {
                                RegistrantIdKey = IdHasher.Instance.GetHash( r.RegistrantId.Value ),
                                RegistrationInstanceName = r.RegistrationInstanceName,
                                RegistrationInstanceIdKey = r.RegistrationInstanceId.HasValue ? IdHasher.Instance.GetHash( r.RegistrationInstanceId.Value ) : null,
                                CreatedDateTime = r.CreatedDateTime?.ToRockDateTimeOffset(),
                                Fees = GetFormattedFeesForRegistrant( personGroup, r.RegistrantId.Value ),
                            };

                            // Grab our registrant from the list of registrants.
                            var registrant = registrants.FirstOrDefault( x => x.Id == r.RegistrantId );

                            // Get the public attributes and attribute values for the registrant.
                            registrantBag.Attributes = registrant.GetPublicAttributesForView( GetCurrentPerson(), true, attributeFilter: a => displayedSourceAttributeIds.Contains( a.Id ) );
                            registrantBag.AttributeValues = registrant.GetPublicAttributeValuesForView( GetCurrentPerson(), true, attributeFilter: a => displayedSourceAttributeIds.Contains( a.Id ) );

                            return registrantBag;
                        } )
                        .ToList();
                }
                else if ( placementMode == PlacementMode.GroupMode )
                {
                    personBag.SourceGroupMembers = personGroup
                        .Where( p => p.GroupMemberId.HasValue && p.GroupId == sourceGroupId.Value )
                        .DistinctBy( p => p.GroupMemberId )
                        .Select( p => {
                            var groupMemberBag = new GroupMemberBag
                            {
                                GroupMemberIdKey = IdHasher.Instance.GetHash( p.GroupMemberId.Value ),
                                CreatedDateTime = p.CreatedDateTime
                            };

                            // Grab our group member from the list of source group members.
                            var sourceGroupMember = sourceGroupMembers.FirstOrDefault( gm => gm.Id == p.GroupMemberId.Value );

                            // Get the public attributes and attribute values for the group member.
                            groupMemberBag.Attributes = sourceGroupMember.GetPublicAttributesForView( GetCurrentPerson(), true, attributeFilter: a => displayedSourceAttributeIds.Contains( a.Id ) );
                            groupMemberBag.AttributeValues = sourceGroupMember.GetPublicAttributeValuesForView( GetCurrentPerson(), true, attributeFilter: a => displayedSourceAttributeIds.Contains( a.Id ) );

                            return groupMemberBag;
                        } )
                        .ToList();
                }

                var addedGroupMemberIds = new HashSet<int>();

                // Process each person and their associated placement records to determine the destination groups they have been placed in (if any).
                foreach ( var row in personGroup.Where( r => r.GroupId.HasValue && r.GroupMemberId.HasValue && r.GroupId != sourceGroupId ) )
                {
                    var groupId = row.GroupId.Value;
                    var groupMemberId = row.GroupMemberId.Value;

                    // Skip if we've already added this group member.
                    if ( !addedGroupMemberIds.Add( groupMemberId ) )
                    {
                        continue;
                    }

                    // Try to find the destination group in the temp group list.
                    var groupBag = placementPeopleBag.TempGroups
                        .FirstOrDefault( g => g.GroupIdKey == IdHasher.Instance.GetHash( groupId ) );

                    // If it doesn't exist, create and add it.
                    if ( groupBag == null )
                    {
                        groupBag = new PlacementGroupBag
                        {
                            GroupId = groupId,
                            GroupIdKey = IdHasher.Instance.GetHash( groupId ),
                            GroupMembers = new List<GroupMemberBag>()
                        };

                        placementPeopleBag.TempGroups.Add( groupBag );
                    }

                    // Grab our group member from the list of destination group members.
                    var destinationGroupMember = destinationGroupMembers.FirstOrDefault( gm => gm.Id == groupMemberId );

                    // Add the destination group member.
                    groupBag.GroupMembers.Add( new GroupMemberBag
                    {
                        GroupMemberId = groupMemberId,
                        GroupMemberIdKey = IdHasher.Instance.GetHash( groupMemberId ),
                        GroupRoleIdKey = row.GroupRoleId.HasValue
                            ? IdHasher.Instance.GetHash( row.GroupRoleId.Value )
                            : null,
                        Attributes = destinationGroupMember.GetPublicAttributesForView( GetCurrentPerson(), true, attributeFilter: a => displayedDestinationGroupMemberAttributeIds.Contains( a.Id ) ),
                        AttributeValues = destinationGroupMember.GetPublicAttributeValuesForView( GetCurrentPerson(), true, attributeFilter: a => displayedDestinationGroupMemberAttributeIds.Contains( a.Id ) ),
                        Person = personBag
                    } );
                }

                var hasPlacements = personGroup.Any( r => r.GroupId.HasValue && r.GroupMemberId.HasValue && r.GroupId != sourceGroupId );

                // If the person has no placements or we are allowing multiple placements than add the person to PeopleToPlace.
                if ( !hasPlacements || isPlacementAllowingMultiple )
                {
                    placementPeopleBag.PeopleToPlace.Add( personBag );
                }
            }

            return ActionOk( placementPeopleBag );
        }

        /// <summary>
        /// Gets the Destination Groups for the specified Group Placement Keys and Placement Mode.
        /// </summary>
        /// <param name="groupPlacementKeys">A bag of Group Placement Keys</param>
        /// <param name="placementMode">The mode of placement being performed (TemplateMode, InstanceMode, GroupMode, EntitySetMode).</param>
        [BlockAction]
        public BlockActionResult GetDestinationGroups( GroupPlacementKeysBag groupPlacementKeys, PlacementMode placementMode )
        {
            int registrationTemplatePlacementEntityTypeId = EntityTypeCache.Get<Rock.Model.RegistrationTemplatePlacement>().Id;
            int registrationInstanceEntityTypeId = EntityTypeCache.Get<Rock.Model.RegistrationInstance>().Id;
            int? sourceEntityTypeId = null;
            int targetEntityTypeId = EntityTypeCache.Get<Rock.Model.Group>().Id;
            int? sourceEntityId = null;
            string purposeKey = null;
            string includedRegistrationInstanceIds = null;
            string registrationTemplatePurposeKey = RelatedEntityPurposeKey.RegistrationTemplateGroupPlacementTemplate;
            string registrationInstancePurposeKey = RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement;
            List<DestinationGroupResult> destinationGroupResults;

            var registrationInstanceId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys?.RegistrationInstanceIdKey );
            var registrationTemplateId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys?.RegistrationTemplateIdKey );
            var registrationTemplatePlacementId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys?.RegistrationTemplatePlacementIdKey );
            var sourceGroupId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys?.SourceGroupIdKey );
            var entitySetId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys?.EntitySetIdKey );
            var destinationGroupTypeId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys?.DestinationGroupTypeIdKey );

            if ( !registrationTemplateId.HasValue && !registrationInstanceId.HasValue && !sourceGroupId.HasValue && !entitySetId.HasValue )
            {
                return ActionNotFound( "Missing required keys to retrieve Destination Groups." );
            }

            if ( !destinationGroupTypeId.HasValue )
            {
                return ActionBadRequest( "Could not find Destination Group Type Id" );
            }

            var destinationGroupIds = GetIdsFromDestinationGroupParam();
            var placementConfiguration = GetPlacementConfiguration( groupPlacementKeys );

            string displayedCampusGuid = null;
            if ( placementConfiguration.DisplayedCampus?.Value.IsNotNullOrWhiteSpace() == true )
            {
                displayedCampusGuid = placementConfiguration.DisplayedCampus.Value;
            }

            if ( placementMode == PlacementMode.TemplateMode || placementMode == PlacementMode.InstanceMode )
            {
                if ( !registrationTemplatePlacementId.HasValue )
                {
                    return ActionBadRequest( "Could not find Registration Template Placement Id" );
                }

                if ( placementMode == PlacementMode.TemplateMode )
                {
                    includedRegistrationInstanceIds = ( placementConfiguration.IncludedRegistrationInstanceIds?.Any() == true )
                        ? string.Join(
                            ",",
                            placementConfiguration.IncludedRegistrationInstanceIds
                                .Select( idKey => Rock.Utility.IdHasher.Instance.GetId( idKey ) )
                                .Where( id => id.HasValue )
                                .Select( id => id.Value )
                        )
                        : null;
                }
            }
            else if ( placementMode == PlacementMode.GroupMode )
            {
                sourceEntityTypeId = EntityTypeCache.Get<Rock.Model.Group>().Id;
                sourceEntityId = sourceGroupId;
                purposeKey = RelatedEntityPurposeKey.GroupPlacement;
            }
            else if ( placementMode == PlacementMode.EntitySetMode )
            {
                sourceEntityTypeId = EntityTypeCache.Get<Rock.Model.EntitySet>().Id;
                sourceEntityId = entitySetId;
                purposeKey = RelatedEntityPurposeKey.EntitySetPlacement;
            }

            destinationGroupResults = RockContext.Database.SqlQuery<DestinationGroupResult>(
                $@"EXEC [dbo].[spGetDestinationGroups] 
                                    @{nameof( registrationTemplatePlacementEntityTypeId )}, 
                                    @{nameof( registrationInstanceEntityTypeId )},
                                    @{nameof( sourceEntityTypeId )}, 
                                    @{nameof( targetEntityTypeId )}, 
                                    @{nameof( registrationTemplateId )}, 
                                    @{nameof( registrationInstanceId )}, 
                                    @{nameof( registrationTemplatePlacementId )},
                                    @{nameof( sourceEntityId )},
                                    @{nameof( placementMode )},
                                    @{nameof( includedRegistrationInstanceIds )},
                                    @{nameof( destinationGroupTypeId )},
                                    @{nameof( destinationGroupIds )},
                                    @{nameof( displayedCampusGuid )},
                                    @{nameof( purposeKey )},
                                    @{nameof( registrationTemplatePurposeKey )},
                                    @{nameof( registrationInstancePurposeKey )}",
                new SqlParameter( nameof( registrationTemplatePlacementEntityTypeId ), registrationTemplatePlacementEntityTypeId ),
                new SqlParameter( nameof( registrationInstanceEntityTypeId ), registrationInstanceEntityTypeId ),
                new SqlParameter( nameof( sourceEntityTypeId ), ( object ) sourceEntityTypeId ?? DBNull.Value ),
                new SqlParameter( nameof( targetEntityTypeId ), targetEntityTypeId ),
                new SqlParameter( nameof( registrationTemplateId ), ( object ) registrationTemplateId ?? DBNull.Value ),
                new SqlParameter( nameof( registrationInstanceId ), ( object ) registrationInstanceId ?? DBNull.Value ),
                new SqlParameter( nameof( registrationTemplatePlacementId ), ( object ) registrationTemplatePlacementId ?? DBNull.Value ),
                new SqlParameter( nameof( sourceEntityId ), ( object ) sourceEntityId ?? DBNull.Value ),
                new SqlParameter( nameof( placementMode ), placementMode.ToString() ),
                new SqlParameter( nameof( includedRegistrationInstanceIds ), ( object ) includedRegistrationInstanceIds ?? DBNull.Value ),
                new SqlParameter( nameof( destinationGroupTypeId ), destinationGroupTypeId.Value ),
                new SqlParameter( nameof( destinationGroupIds ), ( object ) destinationGroupIds ?? DBNull.Value ),
                new SqlParameter( nameof( displayedCampusGuid ), ( object ) displayedCampusGuid ?? DBNull.Value ),
                new SqlParameter( nameof( purposeKey ), ( object ) purposeKey ?? DBNull.Value ),
                new SqlParameter( nameof( registrationTemplatePurposeKey ), registrationTemplatePurposeKey ),
                new SqlParameter( nameof( registrationInstancePurposeKey ), registrationInstancePurposeKey )
            ).ToList();

            HashSet<int> displayedGroupAttributeIds = new HashSet<int>();
            List<Rock.Model.Group> groups = new List<Rock.Model.Group>();

            if ( placementConfiguration.DestinationGroupAttributesToDisplay?.Any() == true )
            {
                displayedGroupAttributeIds = GetAttributeIdsFromGuids( placementConfiguration.DestinationGroupAttributesToDisplay );

                groups = destinationGroupResults.DistinctBy( g => g.GroupId )
                    .Select( g => new Rock.Model.Group
                    {
                        Id = g.GroupId,
                        GroupTypeId = g.GroupTypeId,
                        CampusId = g.CampusId,
                        ParentGroupId = g.ParentGroupId
                    } )
                    .ToList();

                groups.LoadFilteredAttributes( a => displayedGroupAttributeIds.Contains( a.Id ) );
            }

            var placementGroupBags = destinationGroupResults
                .GroupBy( g => g.GroupId )
                .Select( g => {
                    var group = groups.FirstOrDefault( x => x.Id == g.Key );
                    var groupEntry = g.OrderByDescending( x => x.IsShared ).First();

                    var placementBag = new PlacementGroupBag
                    {
                        GroupId = g.Key,
                        GroupIdKey = IdHasher.Instance.GetHash( g.Key ),
                        GroupName = groupEntry.GroupName,
                        GroupGuid = groupEntry.GroupGuid,
                        GroupCapacity = groupEntry.GroupCapacity,
                        GroupTypeIdKey = IdHasher.Instance.GetHash( groupEntry.GroupTypeId ),
                        GroupOrder = groupEntry.GroupOrder,
                        RegistrationInstanceIdKey = groupEntry.RegistrationInstanceId.HasValue ? IdHasher.Instance.GetHash( groupEntry.RegistrationInstanceId.Value ) : string.Empty,
                        IsShared = groupEntry.IsShared,
                        Attributes = group.GetPublicAttributesForView( GetCurrentPerson(), true, attributeFilter: a => displayedGroupAttributeIds.Contains( a.Id ) ),
                        AttributeValues = group.GetPublicAttributeValuesForView( GetCurrentPerson(), true, attributeFilter: a => displayedGroupAttributeIds.Contains( a.Id ) )
                    };

                    return placementBag;
                } ).ToList();

            return ActionOk( placementGroupBags );
        }

        /// <summary>
        /// Adds a collection of people to a specified group as new group members. Validates permissions,
        /// group capacity limits, and registrant-to-group alignment before committing any changes.
        /// </summary>
        /// <param name="addGroupMembersBag">
        /// A bag containing the target group details, the group placement context, the list of pending group members to add,
        /// and placement mode configuration.
        /// </param>
        [BlockAction]
        public BlockActionResult AddGroupMembersToGroup( AddGroupMembersBag addGroupMembersBag )
        {
            var groupId = Rock.Utility.IdHasher.Instance.GetId( addGroupMembersBag.TargetGroup.GroupIdKey );

            if ( !groupId.HasValue )
            {
                return ActionNotFound( "The specified Target Group was not found." );
            }

            var personIds = addGroupMembersBag.PendingGroupMembers
                .Where( gm => gm?.Person != null )
                .Select( gm => Rock.Utility.IdHasher.Instance.GetId( gm.Person.PersonIdKey ) )
                .ToList();

            // Check if the people can be placed into the specified destination Group by the current person.
            var canPlacePeople = CanPlacePeople( personIds, addGroupMembersBag.GroupPlacementKeys.DestinationGroupTypeIdKey, addGroupMembersBag.TargetGroup.GroupIdKey, out string errorMessage );

            // If the people cannot be placed, return a bad request with the error message.
            if ( !canPlacePeople )
            {
                return ActionBadRequest( errorMessage );
            }

            var groupMemberService = new GroupMemberService( RockContext );
            var memberPairs = new List<(GroupMemberBag Bag, GroupMember Entity)>();
            var groupRoleId = Rock.Utility.IdHasher.Instance.GetId( addGroupMembersBag.PendingGroupMembers[0].GroupRoleIdKey );

            // Handle edge case where adding multiple group members could exceed the max count and throw an exception
            if (
                addGroupMembersBag.PendingGroupMembers.Count > 1 &&
                !IsGroupRoleCapacityAvailable( groupRoleId, groupId.Value, addGroupMembersBag.PendingGroupMembers.Count, out errorMessage )
            ) {
                return ActionBadRequest( errorMessage );
            }

            if ( addGroupMembersBag.PlacementMode == PlacementMode.TemplateMode && !addGroupMembersBag.TargetGroup.IsShared )
            {
                foreach ( var groupMember in addGroupMembersBag.PendingGroupMembers )
                {
                    var hasMatchingRegistrant = groupMember.Person?.Registrants
                        ?.Any( r => r.RegistrationInstanceIdKey == addGroupMembersBag.TargetGroup.RegistrationInstanceIdKey ) == true;

                    if ( !hasMatchingRegistrant )
                    {
                        return ActionBadRequest( $"{addGroupMembersBag.TargetGroup.GroupName} is not a placement group for this registration template or instance." );
                    }
                }
            }

            foreach ( var bag in addGroupMembersBag.PendingGroupMembers )
            {
                var personId = Rock.Utility.IdHasher.Instance.GetId( bag.Person.PersonIdKey );
                if ( !personId.HasValue || !groupRoleId.HasValue || !groupId.HasValue )
                {
                    continue;
                }

                var groupMember = new GroupMember
                {
                    GroupId = groupId.Value,
                    GroupRoleId = groupRoleId.Value,
                    PersonId = personId.Value,
                    GroupMemberStatus = GroupMemberStatus.Active,
                };

                if ( !groupMember.IsValid )
                {
                    errorMessage = groupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                    return ActionBadRequest( errorMessage );
                }

                groupMemberService.Add( groupMember );

                // Add the newly created group member to memberPairs.
                memberPairs.Add( (bag, groupMember) );
            }

            RockContext.SaveChanges();

            // Loop through memberPairs to set the GroupMemberId and GroupMemberIdKey on each bag.
            foreach ( var (bag, entity) in memberPairs )
            {
                bag.GroupMemberId = entity.Id;
                bag.GroupMemberIdKey = entity.IdKey;
            }

            return ActionOk( addGroupMembersBag.PendingGroupMembers );
        }

        /// <summary>
        /// Removes a group member from a group based on the provided group member ID key and group ID keys.
        /// </summary>
        /// <param name="groupMemberIdKey">The group member ID key to remove</param>
        /// <param name="groupIdKey">The group ID key to remove the member from.</param>
        [BlockAction]
        public BlockActionResult RemoveGroupMemberFromGroup( string groupMemberIdKey, string groupIdKey )
        {
            var groupMemberService = new GroupMemberService( RockContext );
            var groupMember = groupMemberService.Get( groupMemberIdKey );
            var groupId = Rock.Utility.IdHasher.Instance.GetId( groupIdKey );

            if ( groupMember == null || groupMember.GroupId != groupId )
            {
                return ActionBadRequest( "The specified Group Member was not found." );
            }

            var group = new GroupService( RockContext ).Get( groupIdKey );

            if ( !group.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) && !group.IsAuthorized( Authorization.MANAGE_MEMBERS, GetCurrentPerson() ) )
            {
                return ActionBadRequest( "You are not authorized to remove people from this group." );
            }

            groupMemberService.Delete( groupMember );
            RockContext.SaveChanges();
            return ActionOk();
        }

        /// <summary>
        /// Adds a new placement group based on the provided configuration in the AddGroupBag.
        /// </summary>
        /// <param name="addGroupBag">The bag that conains the details needed to add a Group.</param>
        [BlockAction]
        public BlockActionResult AddPlacementGroup( AddGroupBag addGroupBag )
        {
            List<Rock.Model.Group> placementGroups;
            var groupService = new GroupService( RockContext );
            var groupTypeId = Rock.Utility.IdHasher.Instance.GetId( addGroupBag.GroupTypeIdKey );
            var sourceGroupId = Rock.Utility.IdHasher.Instance.GetId( addGroupBag.GroupPlacementKeys.SourceGroupIdKey );
            var entitySetId = Rock.Utility.IdHasher.Instance.GetId( addGroupBag.GroupPlacementKeys.EntitySetIdKey );
            var registrationInstanceId = Rock.Utility.IdHasher.Instance.GetId( addGroupBag.GroupPlacementKeys.RegistrationInstanceIdKey );
            var registrationTemplatePlacementId = Rock.Utility.IdHasher.Instance.GetId( addGroupBag.GroupPlacementKeys.RegistrationTemplatePlacementIdKey );

            if ( !groupTypeId.HasValue )
            {
                return ActionNotFound( "Group Type Id not found." );
            }

            if ( addGroupBag.SelectedGroupOption == "Add New Group" )
            {
                var newPlacementGroup = new Rock.Model.Group
                {
                    Name = addGroupBag.GroupName,
                    GroupCapacity = addGroupBag.GroupCapacity,
                    Description = addGroupBag.GroupDescription,
                    GroupTypeId = groupTypeId.Value,
                };

                if ( addGroupBag.ParentGroupForNewGroup != null && addGroupBag.ParentGroupForNewGroup.Value.IsNotNullOrWhiteSpace() )
                {
                    var parentGroup = groupService.Get( addGroupBag.ParentGroupForNewGroup.Value.AsGuid() );
                    if ( parentGroup != null )
                    {
                        if ( !IsValidParentGroup( parentGroup, groupTypeId.Value ) )
                        {
                            var groupType = GroupTypeCache.Get( groupTypeId.Value );
                            var errorMessage = string.Format( "The selected parent group doesn't allow adding {0} child groups", groupType );
                            return ActionBadRequest( errorMessage );
                        }

                        newPlacementGroup.ParentGroupId = parentGroup.Id;
                        newPlacementGroup.ParentGroup = parentGroup;
                    }
                }

                if ( addGroupBag.GroupCampus != null )
                {
                    var campus = CampusCache.Get( addGroupBag.GroupCampus.Value );
                    if ( campus != null )
                    {
                        newPlacementGroup.CampusId = campus.Id;
                    }
                }

                groupService.Add( newPlacementGroup );

                if ( !newPlacementGroup.IsAuthorized( Rock.Security.Authorization.EDIT, GetCurrentPerson() ) )
                {
                    return ActionBadRequest( "You are not authorized to save group with the selected group type and/or parent group." );
                }

                RockContext.SaveChanges();

                newPlacementGroup.LoadAttributes( RockContext );
                newPlacementGroup.SetPublicAttributeValues( addGroupBag.NewGroupAttributeValues, GetCurrentPerson(), enforceSecurity: true );
                newPlacementGroup.SaveAttributeValues( RockContext );

                placementGroups = new List<Rock.Model.Group>
                {
                    newPlacementGroup
                };
            }
            else if ( addGroupBag.SelectedGroupOption == "Add Existing Group(s)" )
            {
                placementGroups = new List<Rock.Model.Group>();
                var existingGroups = addGroupBag.ExistingGroupsToAdd;

                if ( existingGroups == null )
                {
                    return ActionBadRequest( "Please select an existing group." );
                }

                foreach ( var group in existingGroups )
                {
                    if ( group.Value.IsNullOrWhiteSpace() )
                    {
                        continue;
                    }

                    var existingPlacementGroup = groupService.Get( group.Value.AsGuid() );
                    if ( existingPlacementGroup == null )
                    {
                        continue;
                    }

                    if ( groupTypeId != existingPlacementGroup.GroupTypeId )
                    {
                        var groupType = GroupTypeCache.Get( groupTypeId.Value );
                        var errorMessage = string.Format( "The selected groups must be {0} groups", groupType );
                        return ActionBadRequest( errorMessage );
                    }

                    placementGroups.Add( existingPlacementGroup );
                }
            }
            else
            {
                var parentGroupGuid = addGroupBag.ParentGroupForChildren?.Value?.AsGuidOrNull();
                var parentGroup = parentGroupGuid.HasValue ? groupService.Get( parentGroupGuid.Value ) : null;

                if ( parentGroup == null )
                {
                    return ActionBadRequest( "Please select a parent group to add the groups." );
                }

                if ( !HasValidChildGroups( parentGroup.Id, groupTypeId.Value, out string errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                placementGroups = groupService.Queryable().Where( a => a.ParentGroupId == parentGroup.Id && a.IsActive == true ).ToList();
            }

            var registrationInstanceService = new RegistrationInstanceService( RockContext );
            var registrationTemplatePlacementService = new RegistrationTemplatePlacementService( RockContext );
            List<Guid> addedGroupGuids = new List<Guid>();

            // Loop through our added placement groups and attempt to add them to Related Entities.
            foreach ( var placementGroup in placementGroups )
            {
                if ( registrationInstanceId.HasValue )
                {
                    var registrationInstance = registrationInstanceService.Get( registrationInstanceId.Value );
                    registrationInstanceService.GetRegistrationInstancePlacementGroupsByPlacement( registrationInstanceId.Value, registrationTemplatePlacementId.Value );

                    if ( registrationInstanceService.AddRegistrationInstancePlacementGroup( registrationInstance, placementGroup, registrationTemplatePlacementId.Value ) )
                    {
                        addedGroupGuids.Add( placementGroup.Guid );
                    }

                }
                else if ( registrationTemplatePlacementId.HasValue )
                {
                    var registrationTemplatePlacement = registrationTemplatePlacementService.Get( registrationTemplatePlacementId.Value );
                    if ( registrationTemplatePlacementService.AddRegistrationTemplatePlacementPlacementGroup( registrationTemplatePlacement, placementGroup ) )
                    {
                        addedGroupGuids.Add( placementGroup.Guid );
                    }
                }
                else if ( sourceGroupId.HasValue && placementGroup.Id != sourceGroupId.Value ) // Added logic to skip adding placement group if it is the current source group.
                {
                    var sourceGroup = groupService.Get( sourceGroupId.Value );
                    
                    if ( groupService.AddGroupPlacementPlacementGroup( sourceGroup, placementGroup ) )
                    {
                        addedGroupGuids.Add( placementGroup.Guid );
                    }
                }
                else if ( entitySetId.HasValue )
                {
                    var entitySetService = new EntitySetService( RockContext );
                    var entitySet = entitySetService.Get( entitySetId.Value );

                    if ( entitySetService.AddEntitySetPlacementGroup( entitySet, placementGroup ) )
                    {
                        addedGroupGuids.Add( placementGroup.Guid );
                    }
                }
            }

            RockContext.SaveChanges();

            return ActionOk( addedGroupGuids );
        }

        /// <summary>
        /// Detach a placement group
        /// </summary>
        /// <param name="detachGroupBag">The bag that contains the info needed to detach a Placement Group.</param>
        [BlockAction]
        public BlockActionResult DetachPlacementGroup( DetachGroupBag detachGroupBag )
        {
            var groupId = Rock.Utility.IdHasher.Instance.GetId( detachGroupBag.GroupIdKey );
            var groupService = new GroupService( RockContext );
            Rock.Model.Group group = null;

            if ( groupId.HasValue )
            {
                group = groupService.Get( groupId.Value );
            }
            else if ( group == null )
            {
                return ActionNotFound( "Specified group not found." );
            }

            if ( detachGroupBag.PlacementMode == PlacementMode.TemplateMode || detachGroupBag.PlacementMode == PlacementMode.InstanceMode )
            {
                var registrationTemplatePlacementId = Rock.Utility.IdHasher.Instance.GetId( detachGroupBag.GroupPlacementKeys.RegistrationTemplatePlacementIdKey );
                var registrationInstanceId = Rock.Utility.IdHasher.Instance.GetId( detachGroupBag.GroupPlacementKeys.RegistrationInstanceIdKey );

                if (!registrationTemplatePlacementId.HasValue)
                {
                    return ActionNotFound( "Specified registration template placement not found." );
                }

                var registrationInstanceService = new RegistrationInstanceService( RockContext );

                var registrationInstance = registrationInstanceId.HasValue
                    ? registrationInstanceService.Get( registrationInstanceId.Value )
                    : null;

                if ( registrationInstance == null )
                {
                    return ActionNotFound( "Specified registration instance not found." );
                }

                registrationInstanceService.DeleteRegistrationInstancePlacementGroup( registrationInstance, group, registrationTemplatePlacementId.Value );
            }
            else if ( detachGroupBag.PlacementMode == PlacementMode.GroupMode )
            {
                var sourceGroupId = Rock.Utility.IdHasher.Instance.GetId( detachGroupBag.GroupPlacementKeys.SourceGroupIdKey );

                var sourceGroup = sourceGroupId.HasValue
                    ? groupService.Get( sourceGroupId.Value )
                    : null;

                if ( sourceGroup == null )
                {
                    return ActionNotFound( "Specified Source Group not found." );
                }

                groupService.DetachDestinationGroupFromSourceGroup( sourceGroup, group );
            }
            else if ( detachGroupBag.PlacementMode == PlacementMode.EntitySetMode )
            {
                var entitySetId = Rock.Utility.IdHasher.Instance.GetId( detachGroupBag.GroupPlacementKeys.EntitySetIdKey );
                var entitySetService = new EntitySetService( RockContext );

                var entitySet = entitySetId.HasValue
                    ? entitySetService.Get( entitySetId.Value )
                    : null;

                if ( entitySet == null )
                {
                    return ActionNotFound( "Specified Entity Set not found." );
                }

                entitySetService.DetachDestinationGroupFromEntitySet( entitySet, group );                
            }

            RockContext.SaveChanges();

            return ActionOk( group.Guid );
        }

        /// <summary>
        /// Deletes a placement group identified by the provided group ID key.
        /// </summary>
        /// <param name="groupIdKey">The group ID key to delete</param>
        [BlockAction]
        public BlockActionResult DeletePlacementGroup( string groupIdKey )
        {
            var groupService = new GroupService( RockContext );
            var group = groupService.Get( groupIdKey );

            if ( group == null )
            {
                return ActionNotFound( "Specified group not found." );
            }

            if ( !group.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
            {
                return ActionBadRequest( "You are not authorized to delete this group." );
            }

            if ( !groupService.CanDelete( group, out string errorMessage, true ) )
            {
                return ActionBadRequest( errorMessage );
            }

            groupService.Delete( group );

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Populates the attribute filters for the specified group placement keys.
        /// </summary>
        /// <param name="groupPlacementKeys">The specified Group Placement Keys.</param>
        [BlockAction]
        public BlockActionResult PopulateAttributeFilters( GroupPlacementKeysBag groupPlacementKeys )
        {
            var placementConfiguration = GetPlacementConfiguration( groupPlacementKeys );
            HashSet<int> displayedSourceAttributeIds = new HashSet<int>();
            HashSet<int> groupAttributeIds = new HashSet<int>();
            HashSet<int> groupMemberAttributeIds = new HashSet<int>();
            AttributeFiltersBag attributeFiltersBag = new AttributeFiltersBag();

            var sourceGroupId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.SourceGroupIdKey );
            var sourceGroupTypeId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.SourceGroupTypeIdKey );
            var registrationTemplateId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.RegistrationTemplateIdKey );
            var registrationTemplatePlacementId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.RegistrationTemplatePlacementIdKey );
            var destinationGroupTypeId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.DestinationGroupTypeIdKey );

            Rock.Model.Group fakeDestinationGroup = null;
            if ( destinationGroupTypeId.HasValue )
            {
                fakeDestinationGroup = new Rock.Model.Group
                {
                    GroupTypeId = destinationGroupTypeId.Value
                };
            }

            if ( placementConfiguration.SourceAttributesToDisplay?.Any() == true )
            {
                displayedSourceAttributeIds = GetAttributeIdsFromGuids( placementConfiguration.SourceAttributesToDisplay );

                if ( sourceGroupId.HasValue && sourceGroupTypeId.HasValue )
                {
                    var fakeSourceGroupMember = new GroupMember
                    {
                        GroupId = sourceGroupId.Value,
                        GroupTypeId = sourceGroupTypeId.Value
                    };

                    fakeSourceGroupMember.LoadAttributes();

                    attributeFiltersBag.SourceAttributesForFilter = fakeSourceGroupMember.GetPublicAttributesForEdit( GetCurrentPerson(), true, attributeFilter: a => displayedSourceAttributeIds.Contains( a.Id ) );
                }
                else if ( registrationTemplateId.HasValue )
                {
                    var fakeRegistrant = new RegistrationRegistrant
                    {
                        RegistrationTemplateId = registrationTemplateId.Value
                    };

                    fakeRegistrant.LoadAttributes();

                    attributeFiltersBag.SourceAttributesForFilter = fakeRegistrant.GetPublicAttributesForEdit( GetCurrentPerson(), true, attributeFilter: a => displayedSourceAttributeIds.Contains( a.Id ) );
                }
            }

            if ( placementConfiguration.DestinationGroupAttributesToDisplay?.Any() == true && fakeDestinationGroup != null )
            {
                groupAttributeIds = GetAttributeIdsFromGuids( placementConfiguration.DestinationGroupAttributesToDisplay );

                fakeDestinationGroup.LoadAttributes();

                attributeFiltersBag.DestinationGroupAttributesForFilter = fakeDestinationGroup.GetPublicAttributesForEdit( GetCurrentPerson(), true, attributeFilter: a => groupAttributeIds.Contains( a.Id ) );
            }

            if ( placementConfiguration.DestinationGroupMemberAttributesToDisplay?.Any() == true && fakeDestinationGroup != null )
            {
                groupMemberAttributeIds = GetAttributeIdsFromGuids( placementConfiguration.DestinationGroupMemberAttributesToDisplay );

                var fakeDestinationGroupMember = new GroupMember
                {
                    Group = fakeDestinationGroup
                };

                fakeDestinationGroupMember.LoadAttributes();

                attributeFiltersBag.DestinationGroupMemberAttributesForFilter = fakeDestinationGroupMember.GetPublicAttributesForEdit( GetCurrentPerson(), true, attributeFilter: a => groupMemberAttributeIds.Contains( a.Id ) );
            }

            if ( placementConfiguration.AreFeesDisplayed && registrationTemplateId.HasValue )
            {
                var feeItemList = new RegistrationTemplateFeeService( RockContext )
                    .Queryable()
                    .Where( f => f.RegistrationTemplateId == registrationTemplateId.Value )
                    .ToList()
                    .SelectMany( fee => fee.FeeItems.Select( item => new ListItemBag
                    {
                        Value = item.IdKey,
                        Text = fee.Name + " - " + item.Name
                    } ) )
                    .ToList();

                attributeFiltersBag.RegistrantFeeItemsForFilter = feeItemList;
            }

            return ActionOk( attributeFiltersBag );
        }

        /// <summary>
        /// Populates the attributes for Registrant and Group Member entities when they are added via realtime.
        /// </summary>
        /// <param name="sourceAndDestinationEntityKeys">The source and destination keys for Group Member and Registrant entities</param>
        [BlockAction]
        public BlockActionResult PopulateAttributes( SourceAndDestinationEntityKeysBag sourceAndDestinationEntityKeys )
        {
            var resultBag = new SourceAndDestinationEntityAttributesBag
            {
                SourceEntityAttributes = new Dictionary<string, AttributeDataBag>(),
                DestinationEntityAttributes = new Dictionary<string, AttributeDataBag>()
            };

            var placementConfiguration = GetPlacementConfiguration( sourceAndDestinationEntityKeys.GroupPlacementKeysBag );

            HashSet<int> displayedSourceAttributeIds = null;
            HashSet<int> displayedDestinationGroupMemberAttributeIds = null;

            if ( placementConfiguration.SourceAttributesToDisplay?.Any() == true )
            {
                displayedSourceAttributeIds = GetAttributeIdsFromGuids( placementConfiguration.SourceAttributesToDisplay );
            }

            if ( placementConfiguration.DestinationGroupMemberAttributesToDisplay?.Any() == true )
            {
                displayedDestinationGroupMemberAttributeIds = GetAttributeIdsFromGuids( placementConfiguration.DestinationGroupMemberAttributesToDisplay );
            }

            if ( sourceAndDestinationEntityKeys.PlacementMode == PlacementMode.GroupMode )
            {
                // Gets a list of Group Member entities for our source group members.
                var sourceGroupMembers = sourceAndDestinationEntityKeys.SourceGroupMembers.Select( gm => {
                    var groupMemberId = IdHasher.Instance.GetId( gm.GroupMemberIdKey );
                    var groupId = IdHasher.Instance.GetId( gm.GroupIdKey );
                    var groupTypeId = IdHasher.Instance.GetId( gm.GroupTypeIdKey );
                    var groupRoleId = IdHasher.Instance.GetId( gm.GroupRoleIdKey );
                    var groupMember = new GroupMember();

                    if ( groupMemberId.HasValue && groupId.HasValue && groupTypeId.HasValue && groupRoleId.HasValue )
                    {
                        groupMember = new GroupMember
                        {
                            Id = groupMemberId.Value,
                            GroupId = groupId.Value,
                            GroupTypeId = groupTypeId.Value,
                            GroupRoleId = groupRoleId.Value
                        };
                    }

                    return groupMember;
                } ).ToList();

                if ( displayedSourceAttributeIds != null )
                {
                    // Load the attributes for the source group members based on the displayed attribute IDs.
                    sourceGroupMembers.LoadFilteredAttributes( a => displayedSourceAttributeIds.Contains( a.Id ) );
                }

                foreach ( var sourceGroupMember in sourceGroupMembers )
                {
                    // Create a new AttributeDataBag for each source group member.
                    resultBag.SourceEntityAttributes[sourceGroupMember.IdKey] = new AttributeDataBag()
                    {
                        Attributes = sourceGroupMember.GetPublicAttributesForView( GetCurrentPerson(), true, attributeFilter: a => displayedSourceAttributeIds.Contains( a.Id ) ),
                        AttributeValues = sourceGroupMember.GetPublicAttributeValuesForView( GetCurrentPerson(), true, attributeFilter: a => displayedSourceAttributeIds.Contains( a.Id ) )
                    };
                }
            }
            else if ( sourceAndDestinationEntityKeys.PlacementMode == PlacementMode.TemplateMode || sourceAndDestinationEntityKeys.PlacementMode == PlacementMode.InstanceMode )
            {
                // Gets a list of Registrant entities for our source registrants.
                var sourceRegistrants = sourceAndDestinationEntityKeys.SourceRegistrants.Select( r => {
                    var registrantId = IdHasher.Instance.GetId( r.RegistrantIdKey );
                    var registrationTemplateId = IdHasher.Instance.GetId( r.RegistrationTemplateIdKey );
                    var registrant = new RegistrationRegistrant();

                    if ( registrantId.HasValue && registrationTemplateId.HasValue )
                    {
                        registrant = new RegistrationRegistrant
                        {
                            Id = registrantId.Value,
                            RegistrationTemplateId = registrationTemplateId.Value
                        };
                    }

                    return registrant;
                } ).ToList();

                if ( displayedSourceAttributeIds != null )
                {
                    // Load the attributes for the source registrants based on the displayed attribute IDs.
                    sourceRegistrants.LoadFilteredAttributes( a => displayedSourceAttributeIds.Contains( a.Id ) );
                }

                foreach ( var sourceRegistrant in sourceRegistrants )
                {
                    // Create a new AttributeDataBag for each source registrant.
                    resultBag.SourceEntityAttributes[sourceRegistrant.IdKey] = new AttributeDataBag()
                    {
                        Attributes = sourceRegistrant.GetPublicAttributesForView( GetCurrentPerson(), true, attributeFilter: a => displayedSourceAttributeIds.Contains( a.Id ) ),
                        AttributeValues = sourceRegistrant.GetPublicAttributeValuesForView( GetCurrentPerson(), true, attributeFilter: a => displayedSourceAttributeIds.Contains( a.Id ) )
                    };
                }
            }

            // Gets a list of Group Member entities for our destination group members.
            var destinationGroupMembers = sourceAndDestinationEntityKeys.DestinationGroupMembers.Select( gm => {
                var groupMemberId = IdHasher.Instance.GetId( gm.GroupMemberIdKey );
                var groupId = IdHasher.Instance.GetId( gm.GroupIdKey );
                var groupTypeId = IdHasher.Instance.GetId( gm.GroupTypeIdKey );
                var groupRoleId = IdHasher.Instance.GetId( gm.GroupRoleIdKey );
                var groupMember = new GroupMember();

                if ( groupMemberId.HasValue && groupId.HasValue && groupTypeId.HasValue && groupRoleId.HasValue )
                {
                    groupMember = new GroupMember
                    {
                        Id = groupMemberId.Value,
                        GroupId = groupId.Value,
                        GroupTypeId = groupTypeId.Value,
                        GroupRoleId = groupRoleId.Value
                    };
                }

                return groupMember;
            } ).ToList();

            if ( displayedDestinationGroupMemberAttributeIds != null )
            {
                // Load the attributes for the destination group members based on the displayed attribute IDs.
                destinationGroupMembers.LoadFilteredAttributes( a => displayedDestinationGroupMemberAttributeIds.Contains( a.Id ) );
            }

            foreach ( var destinationGroupMember in destinationGroupMembers )
            {
                // Create a new AttributeDataBag for each destination group member.
                resultBag.DestinationEntityAttributes[destinationGroupMember.IdKey] = new AttributeDataBag()
                {
                    Attributes = destinationGroupMember.GetPublicAttributesForView( GetCurrentPerson(), true, attributeFilter: a => displayedDestinationGroupMemberAttributeIds.Contains( a.Id ) ),
                    AttributeValues = destinationGroupMember.GetPublicAttributeValuesForView( GetCurrentPerson(), true, attributeFilter: a => displayedDestinationGroupMemberAttributeIds.Contains( a.Id ) )
                };
            }

            return ActionOk( resultBag );
        }

        #endregion Block Actions
    }
}
