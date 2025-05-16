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
    //[SupportedSiteTypes( Model.SiteType.Web )]

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
            // TODO - Probably should update preference names to idkey
            public const string PlacementConfigurationJSONRegistrationInstanceId = "PlacementConfigurationJSON_RegistrationInstanceId_{0}";
            public const string PlacementConfigurationJSONRegistrationTemplateId = "PlacementConfigurationJSON_RegistrationTemplateId_{0}";
            public const string PlacementConfigurationJSONSourceGroupId = "PlacementConfigurationJSON_SourceGroupId_{0}";
            public const string PersonAttributeFilterRegistrationInstanceId = "PersonAttributeFilter_RegistrationInstanceId_{0}";
            public const string PersonAttributeFilterRegistrationTemplateId = "PersonAttributeFilter_RegistrationTemplateId_{0}";
            public const string PersonAttributeFilterSourceGroupId = "PersonAttributeFilter_SourceGroupId_{0}";
            public const string GroupAttributeFilterGroupTypeId = "GroupAttributeFilter_GroupTypeId_{0}";
            public const string GroupMemberAttributeFilterGroupTypeId = "GroupMemberAttributeFilter_GroupTypeId_{0}";
            public const string RegistrantFeeItemValuesForFiltersJSONRegistrationInstanceId = "RegistrantFeeItemValuesForFiltersJSON_RegistrationInstanceId_{0}";
            public const string RegistrantFeeItemValuesForFiltersJSONRegistrationTemplateId = "RegistrantFeeItemValuesForFiltersJSON_RegistrationTemplateId_{0}";
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

        #region Classes

        private class DestinationGroupResult
        {
            public int GroupId { get; set; }
            public string GroupName { get; set; }
            public Guid GroupGuid { get; set; }
            public int? GroupCapacity { get; set; }
            public int GroupTypeId { get; set; }
            public int GroupOrder { get; set; }
            public bool IsShared { get; set; }
            public int? RegistrationInstanceId { get; set; }
        }

        private class PlacementPeopleResult
        {
            // Group Member info
            public int? GroupId { get; set; }
            public int? GroupTypeId { get; set; }
            public int? GroupMemberId { get; set; }
            public int? GroupRoleId { get; set; }

            // Person info
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string NickName { get; set; }
            public string LastName { get; set; }
            public Gender Gender { get; set; }
            public int? PhotoId { get; set; }
            public int Age { get; set; }
            public int? RecordTypeValueId { get; set; }
            public AgeClassification AgeClassification { get; set; }

            // Registrant info
            public int? RegistrantId { get; set; }
            public string RegistrationInstanceName { get; set; }
            public int? RegistrationInstanceId { get; set; }
            public DateTime? CreatedDateTime { get; set; }
            public string FeeName { get; set; }
            public string Option { get; set; }
            public int? Quantity { get; set; }
            public decimal? Cost { get; set; }
            public RegistrationFeeType? FeeType { get; set; }
            public int? FeeItemId { get; set; }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            GroupPlacementInitializationBox box = GetInitializationBox();

            return box;
        }

        private GroupPlacementInitializationBox GetInitializationBox()
        {
            GroupPlacementInitializationBox box = new GroupPlacementInitializationBox();
            box.DestinationGroupType = new DestinationGroupTypeBag();
            box.GroupPlacementKeys = new GroupPlacementKeysBag();
            box.Title = "Group Placement"; // TODO - convert to proper title
            box.NavigationUrls = GetBoxNavigationUrls();
            box.BackPageUrl = GetBackPageUrl();
            box.PlacementConfigurationSettingOptions = new PlacementConfigurationSettingOptionsBag();

            var sourcePersonId = GetIdFromPageParameter( PageParameterKey.SourcePerson );
            if ( sourcePersonId.HasValue )
            {
                var sourcePerson = new PersonService( RockContext ).Get( sourcePersonId.Value );
                box.SourcePerson = new PersonBag
                {
                    PersonIdKey = sourcePerson.IdKey,
                    FirstName = sourcePerson.FirstName,
                    Nickname = sourcePerson.NickName,
                    LastName = sourcePerson.LastName,
                    Gender = sourcePerson.Gender
                };
            }

            int? destinationGroupTypeId = null;
            var sourceGroupId = GetIdFromPageParameter( PageParameterKey.SourceGroup );
            var entitySetId = GetIdFromPageParameter( PageParameterKey.EntitySet );

            if ( sourceGroupId.HasValue || entitySetId.HasValue )
            {
                box.IsPlacementAllowingMultiple = PageParameter( PageParameterKey.AllowMultiplePlacements ).AsBoolean();


                destinationGroupTypeId = GetIdFromPageParameter( PageParameterKey.DestinationGroupType );

                if ( sourceGroupId.HasValue )
                {
                    var sourceGroup = GroupCache.Get( sourceGroupId.Value );

                    if ( !destinationGroupTypeId.HasValue )
                    {
                        destinationGroupTypeId = sourceGroup.GroupTypeId;
                    }

                    Rock.Model.Group fakeSourceGroup = new Rock.Model.Group
                    {
                        GroupTypeId = sourceGroup.GroupTypeId
                    };

                    box.PlacementConfigurationSettingOptions.SourceAttributes = box.PlacementConfigurationSettingOptions.DestinationGroupMemberAttributes = GetGroupMemberAttributesAsListItems( fakeSourceGroup );
                    box.Title = $"Group Placement - {sourceGroup.Name}";
                    box.GroupPlacementKeys.SourceGroupIdKey = IdHasher.Instance.GetHash( sourceGroupId.Value );
                    box.GroupPlacementKeys.SourceGroupTypeIdKey = IdHasher.Instance.GetHash( sourceGroup.GroupTypeId ); // TODO - Verify if I need this or if I just use the one on realtime update message.
                    box.GroupPlacementKeys.SourceGroupGuid = sourceGroup.Guid;
                    box.PlacementMode = PlacementMode.GroupMode;
                }
                else
                {
                    box.GroupPlacementKeys.EntitySetIdKey = IdHasher.Instance.GetHash( entitySetId.Value );
                    box.PlacementMode = PlacementMode.EntitySetMode;

                    var entitySet = new EntitySetService( RockContext ).Get( entitySetId.Value );

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
                box.ErrorMessage = "Group Type was not found.";
                return box;
            }

            // If destinationGroupTypeId was null an error message would be returned before this, so we can use .Value here.
            var groupType = GroupTypeCache.Get( destinationGroupTypeId.Value );

            // TODO - simplify this
            box.DestinationGroupType.IdKey = destinationGroupTypeId.HasValue ? IdHasher.Instance.GetHash( destinationGroupTypeId.Value ) : null; // TODO - Probably throw an error if not provided.
            box.GroupPlacementKeys.DestinationGroupTypeIdKey = destinationGroupTypeId.HasValue ? IdHasher.Instance.GetHash( destinationGroupTypeId.Value ) : null;

            box.DestinationGroupType.Roles = groupType?.Roles
                .Select( r => new DestinationGroupTypeRoleBag
                {
                    IdKey = IdHasher.Instance.GetHash( r.Id ),
                    Name = r.Name,
                    MaxCount = r.MaxCount,
                    MinCount = r.MinCount,
                    IsLeader = r.IsLeader
                } )
                .ToList();


            box.DestinationGroupType.IconCSSClass = groupType?.IconCssClass;
            box.DestinationGroupType.Name = groupType?.GroupTerm?.Pluralize();

            Rock.Model.Group fakeGroup;
            box.PlacementConfigurationSettingOptions.DestinationGroupAttributes = GetGroupAttributesAsListItems( groupType.Id, out fakeGroup );
            box.PlacementConfigurationSettingOptions.DestinationGroupMemberAttributes = GetGroupMemberAttributesAsListItems( fakeGroup );

            // TODO - optimize fetching attributes (I'm getting this data 3 different ways currently)
            box.AttributesForGroupAdd = fakeGroup.GetPublicAttributesForEdit( GetCurrentPerson(), true );

            return box;
        }

        private GroupPlacementInitializationBox GetBoxForRegistrationPlacement( GroupPlacementInitializationBox box, out int? destinationGroupTypeId )
        {
            var registrationTemplatePlacementId = GetIdFromPageParameter( PageParameterKey.RegistrationTemplatePlacementId );
            var registrationInstanceId = GetIdFromPageParameter( PageParameterKey.RegistrationInstanceId );

            var registrationTemplateService = new RegistrationTemplateService( RockContext );
            var registrationInstanceService = new RegistrationInstanceService( RockContext );

            destinationGroupTypeId = null;

            // Pull Registration Template from Block Attribute if available.
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
                if ( registrationInstance != null )
                {
                    registrationTemplateId = registrationInstance.RegistrationTemplateId;

                    box.GroupPlacementKeys.RegistrationInstanceGuid = registrationInstance.Guid;
                }
            }

            // make sure a valid RegistrationTemplate specified (or determined from RegistrationInstanceId )
            var registrationTemplate = registrationTemplateId.HasValue
                ? registrationTemplateService.Get( registrationTemplateId.Value )
                : null;

            if ( registrationTemplate == null )
            {
                box.ErrorMessage = "Invalid Registration Template";
                return box;
            }

            RegistrationTemplatePlacement registrationTemplatePlacement = null;
            if ( registrationTemplatePlacementId.HasValue )
            {
                registrationTemplatePlacement = new RegistrationTemplatePlacementService( RockContext ).Get( registrationTemplatePlacementId.Value );

                if ( registrationTemplatePlacement == null || registrationTemplatePlacement.RegistrationTemplateId != registrationTemplateId )
                {
                    // if the registration template placement is for a different registration template, don't use it
                    registrationTemplatePlacement = null;
                    registrationTemplatePlacementId = null;
                }
                else
                {
                    destinationGroupTypeId = registrationTemplatePlacement.GroupTypeId;
                }
            }

            // Binds the selectable placements ( Buses, Cabins, etc. )
            var registrationTemplatePlacements = registrationTemplateService.GetSelect( registrationTemplateId.Value, s => s.Placements ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();

            // TODO - check if needed.
            if ( registrationTemplatePlacement == null )
            {
                if ( !registrationTemplatePlacements.Any() )
                {
                    box.ErrorMessage = "No Placement Types available for this registration template.";
                    return box;
                }

                destinationGroupTypeId = registrationTemplatePlacements.First().GroupTypeId;
            }

            if ( !destinationGroupTypeId.HasValue )
            {
                box.ErrorMessage = "Group Type was not found.";
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

            box.IsPlacementAllowingMultiple = registrationTemplatePlacement?.AllowMultiplePlacements ?? false; // todo this shouldn't need a null check.

            box.GroupPlacementKeys.RegistrationInstanceIdKey = registrationInstanceId.HasValue ? IdHasher.Instance.GetHash( registrationInstanceId.Value ) : null;
            box.GroupPlacementKeys.RegistrationTemplateIdKey = registrationTemplateId.HasValue ? IdHasher.Instance.GetHash( registrationTemplateId.Value ) : null;
            box.GroupPlacementKeys.RegistrationTemplateGuid = registrationTemplate.Guid;
            box.GroupPlacementKeys.RegistrationTemplatePlacementIdKey = registrationTemplatePlacementId.HasValue ? IdHasher.Instance.GetHash( registrationTemplatePlacementId.Value ) : null;

            box.PlacementConfigurationSettingOptions.SourceAttributes = GetRegistrantAttributesAsListItems( registrationTemplateId );

            if ( box.PlacementMode == PlacementMode.TemplateMode )
            {
                // Get Registration instances... TODO - Optimize to only query once and get both variations of instances.

                var registrationTemplateInstances = registrationInstanceService.Queryable()
                    .Where( a => a.RegistrationTemplateId == registrationTemplateId )
                    .OrderBy( a => a.Name )
                    .ToList()
                    .Select( a => new ListItemBag
                    {
                        Value = a.IdKey,
                        Text = a.Name
                    } )
                    .ToList();

                box.PlacementConfigurationSettingOptions.RegistrationInstances = registrationTemplateInstances;
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

        private List<ListItemBag> GetRegistrantAttributesAsListItems( int? registrationTemplateId )
        {
            var listItems = new List<ListItemBag>();

            if ( !registrationTemplateId.HasValue )
            {
                return listItems;
            }

            using ( var rockContext = new RockContext() )
            {
                var templateForms = new RegistrationTemplateService( rockContext )
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
                                Value = IdHasher.Instance.GetHash( attribute.Id )
                            } );
                        }
                    }
                }
            }

            return listItems;
        }

        private List<ListItemBag> GetGroupAttributesAsListItems( int? registrationTemplatePlacementGroupTypeId, out Rock.Model.Group fakeGroup )
        {
            var listItems = new List<ListItemBag>();

            if ( !registrationTemplatePlacementGroupTypeId.HasValue )
            {
                fakeGroup = null;
                return listItems;
            }

            fakeGroup = new Rock.Model.Group { GroupTypeId = registrationTemplatePlacementGroupTypeId.Value };
            fakeGroup.LoadAttributes();

            listItems = fakeGroup.Attributes
                .Select( a => a.Value )
                .Where( a => a != null )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new ListItemBag
                {
                    Text = a.Name,
                    Value = IdHasher.Instance.GetHash( a.Id )
                } )
                .ToList();

            return listItems;
        }

        public List<ListItemBag> GetGroupMemberAttributesAsListItems( Rock.Model.Group group )
        {
            var listItems = new List<ListItemBag>();

            if ( group == null )
            {
                return listItems;
            }
            // TODO this seems overkill
            var fakeGroupMember = new GroupMember
            {
                Group = group,
                GroupId = group.Id
            };

            fakeGroupMember.LoadAttributes();

            listItems = fakeGroupMember.GetAuthorizedAttributes( Authorization.VIEW, GetCurrentPerson() )
                .Select( a => a.Value )
                .Where( a => a != null )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new ListItemBag
                {
                    Text = a.Name,
                    Value = IdHasher.Instance.GetHash( a.Id )
                } )
                .ToList();

            return listItems;
        }


        private AttributeDataBag GetGroupAttributesAndValues( int groupId, int groupTypeId, HashSet<int> displayedAttributeIds )
        {
            if ( displayedAttributeIds == null )
            {
                // don't spend time loading attributes if there aren't any to be displayed
                return null;
            }

            Rock.Model.Group group = new Rock.Model.Group
            {
                Id = groupId,
                GroupTypeId = groupTypeId,
            };

            if ( group.AttributeValues == null )
            {
                group.LoadAttributes();
            }

            var attributeDataBag = new AttributeDataBag
            {
                Attributes = group.GetPublicAttributesForView( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) ),
                AttributeValues = group.GetPublicAttributeValuesForView( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) )
            };

            return attributeDataBag;
        }

        private AttributeDataBag GetGroupMemberAttributesAndValues( int? groupMemberId, int? groupTypeId, HashSet<int> displayedAttributeIds )
        {
            if ( displayedAttributeIds == null || !groupMemberId.HasValue || !groupTypeId.HasValue )
            {
                // don't spend time loading attributes if there aren't any to be displayed
                return null;
            }

            GroupMember groupMember = new GroupMember
            {
                Id = groupMemberId.Value,
                GroupTypeId = groupTypeId.Value,
            };

            if ( groupMember.AttributeValues == null )
            {
                groupMember.LoadAttributes();
            }

            var attributeDataBag = new AttributeDataBag
            {
                Attributes = groupMember.GetPublicAttributesForView( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) ),
                AttributeValues = groupMember.GetPublicAttributeValuesForView( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) )
            };

            return attributeDataBag;
        }

        private AttributeDataBag GetRegistrantAttributesAndValues( int? registrantId, int? registrationTemplateId, HashSet<int> displayedAttributeIds )
        {
            if ( displayedAttributeIds == null || !registrantId.HasValue || !registrationTemplateId.HasValue)
            {
                // don't spend time loading attributes if there aren't any to be displayed
                return null;
            }

            RegistrationRegistrant registrant = new RegistrationRegistrant
            {
                Id = registrantId.Value,
                RegistrationTemplateId = registrationTemplateId.Value,
            };

            if ( registrant.AttributeValues == null )
            {
                registrant.LoadAttributes();
            }

            var attributeDataBag = new AttributeDataBag
            {
                Attributes = registrant.GetPublicAttributesForView( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) ),
                AttributeValues = registrant.GetPublicAttributeValuesForView( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) )
            };

            return attributeDataBag;
        }

        /// <summary>
        /// Gets the placement configuration.
        /// </summary>
        /// <returns></returns>
        private PlacementConfigurationSettingsBag GetPlacementConfiguration( GroupPlacementKeysBag groupPlacementKeys )
        {
            if ( _placementConfiguration == null )
            {
                var preferences = GetBlockPersonPreferences();

                string placementConfigurationJSON = string.Empty;
                if ( groupPlacementKeys.RegistrationInstanceIdKey.IsNotNullOrWhiteSpace() )
                {
                    placementConfigurationJSON = preferences.GetValue( string.Format( PreferenceKey.PlacementConfigurationJSONRegistrationInstanceId, groupPlacementKeys.RegistrationInstanceIdKey ) );
                }
                else if ( groupPlacementKeys.RegistrationTemplateIdKey.IsNotNullOrWhiteSpace() )
                {
                    placementConfigurationJSON = preferences.GetValue( string.Format( PreferenceKey.PlacementConfigurationJSONRegistrationTemplateId, groupPlacementKeys.RegistrationTemplateIdKey ) );
                }
                else if ( groupPlacementKeys.SourceGroupIdKey.IsNotNullOrWhiteSpace() )
                {
                    placementConfigurationJSON = preferences.GetValue( string.Format( PreferenceKey.PlacementConfigurationJSONSourceGroupId, groupPlacementKeys.SourceGroupIdKey ) );
                }

                _placementConfiguration = placementConfigurationJSON.FromJsonOrNull<PlacementConfigurationSettingsBag>() ?? new PlacementConfigurationSettingsBag();
            }

            return _placementConfiguration;
        }

        private PlacementConfigurationSettingsBag _placementConfiguration = null;

        private List<string> GetRegistrantFeeItemValuesForFilters( GroupPlacementKeysBag groupPlacementKeys )
        {
            var preferences = GetBlockPersonPreferences();

            string registrantFeeItemValuesJSON = string.Empty;
            if ( groupPlacementKeys.RegistrationInstanceIdKey.IsNotNullOrWhiteSpace() )
            {
                registrantFeeItemValuesJSON = preferences.GetValue( string.Format( PreferenceKey.RegistrantFeeItemValuesForFiltersJSONRegistrationInstanceId, groupPlacementKeys.RegistrationInstanceIdKey ) );
            }
            else if ( groupPlacementKeys.RegistrationTemplateIdKey.IsNotNullOrWhiteSpace() )
            {
                registrantFeeItemValuesJSON = preferences.GetValue( string.Format( PreferenceKey.RegistrantFeeItemValuesForFiltersJSONRegistrationTemplateId, groupPlacementKeys.RegistrationTemplateIdKey ) );
            }

            return registrantFeeItemValuesJSON.FromJsonOrNull<List<string>>() ?? new List<string>();
        }

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

            if ( destinationGroupTypeId != group.GroupTypeId )
            {
                errorMessage = "Specified group's group type does not match the group type of the Destination Group.";
                return false;
            }

            // TODO - Logic to check if allow multiple conditions are met.

            errorMessage = string.Empty;
            return true;
        }

        private bool IsGroupRoleCapacityAvailable( int roleId, int groupId, int pendingGroupMemberCount, out string errorMessage )
        {
            var group = GroupCache.Get( groupId );
            var role = GroupTypeCache.Get( group.GroupTypeId )?.Roles.FirstOrDefault( r => r.Id == roleId );

            if ( role?.MaxCount is int maxCount )
            {
                var currentCount = new GroupMemberService( RockContext )
                    .Queryable()
                    .Count( gm => gm.GroupId == group.Id && gm.GroupRoleId == roleId );

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

        private Dictionary<string, ListItemBag> GetFormattedFeesForRegistrant(
            IEnumerable<PlacementPeopleResult> personGroup,
            int registrantId )
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
                        var costText = f.Quantity > 1
                            ? $"{f.Quantity} at {f.Cost?.FormatAsCurrency()}" // TODO - check for null
                            : ( ( f.Quantity ?? 0 ) * ( f.Cost ?? 0 ) ).FormatAsCurrency();

                        return new ListItemBag
                        {
                            Text = feeLabel,
                            Value = costText
                        };
                    }
                );
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

        #endregion Helper Methods

        #region Block Actions

        /// <summary>
        /// Subscribes to the real-time AttendanceOccurrence channels.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="groupGuid">The Group unique identifier.</param>
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
                // Authorize the current user.
                if ( group == null )
                {
                    return ActionNotFound( "Group not found." );
                }

                if ( !group.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.Forbidden );
                }
                await topicChannels.AddToChannelAsync( realTimeConnectionKeysBag.ConnectionId, GroupPlacementTopic.GetGroupMemberChannelForGroup( group.Guid ) );
                await topicChannels.AddToChannelAsync( realTimeConnectionKeysBag.ConnectionId, GroupPlacementTopic.GetGroupMemberDeletedChannel() );
            }

            // TODO - should this be an else if? Do I ever want both channels added?
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

        [BlockAction]
        public BlockActionResult GetPlacementPeople( GroupPlacementKeysBag groupPlacementKeys, Boolean isPlacementAllowingMultiple )
        {
            int registrationTemplatePlacementEntityTypeId = EntityTypeCache.Get<Rock.Model.RegistrationTemplatePlacement>().Id;
            int registrationInstanceEntityTypeId = EntityTypeCache.Get<Rock.Model.RegistrationInstance>().Id;
            int personEntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
            int sourceEntityTypeId;
            int targetEntityTypeId = EntityTypeCache.Get<Rock.Model.Group>().Id;
            int sourceEntityId;
            string placementMode;
            string purposeKey;
            string registrationTemplatePurposeKey = RelatedEntityPurposeKey.RegistrationTemplateGroupPlacementTemplate;
            string registrationInstancePurposeKey = RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement;
            List<PlacementPeopleResult> placementPeopleResults;

            var registrationInstanceId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.RegistrationInstanceIdKey );
            var registrationTemplateId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.RegistrationTemplateIdKey );
            var registrationTemplatePlacementId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.RegistrationTemplatePlacementIdKey );
            var sourceGroupId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.SourceGroupIdKey );
            var entitySetId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.EntitySetIdKey );
            var destinationGroupTypeId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.DestinationGroupTypeIdKey );

            if ( !destinationGroupTypeId.HasValue )
            {
                // TODO - Determine if this is valid message.
                return ActionBadRequest( "Could not find Destination Group Type Id" );
            }

            //if ( !registrationTemplateId.HasValue || !registrationTemplatePlacementId.HasValue )
            //{
            //    // TODO - refactor how ids are retrieved.
            //    return ActionBadRequest("Could not find Registration Template Id");
            //}

            var placementConfiguration = GetPlacementConfiguration( groupPlacementKeys );
            var includedRegistrationInstanceIds = ( placementConfiguration.IncludedRegistrationInstanceIds?.Any() == true )
                ? string.Join(
                    ",",
                    placementConfiguration.IncludedRegistrationInstanceIds
                        .Select( idKey => Rock.Utility.IdHasher.Instance.GetId( idKey ) )   // TODO - convert to helper method? I do this in a lot of places.
                        .Where( id => id.HasValue )
                        .Select( id => id.Value )
                )
                : null;

            var includeFees = placementConfiguration.AreFeesDisplayed;
            var includedFeeItemIdsFromPreferences = GetRegistrantFeeItemValuesForFilters( groupPlacementKeys );
            var includedFeeItemIds = includedFeeItemIdsFromPreferences.Any() ? string.Join( ",", includedFeeItemIdsFromPreferences ) : null;

            string displayedCampusGuid = null;
            if ( placementConfiguration.DisplayedCampus?.Value.IsNotNullOrWhiteSpace() == true)
            {
                displayedCampusGuid = placementConfiguration.DisplayedCampus.Value;
            }

            var destinationGroupIds = GetIdsFromDestinationGroupParam();

            // If registration instance id has a value than we are in instance mode
            if ( registrationInstanceId.HasValue )
            {
                placementMode = "InstanceMode";

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
                    new SqlParameter( nameof( sourceEntityTypeId ), DBNull.Value ),
                    new SqlParameter( nameof( targetEntityTypeId ), targetEntityTypeId ),
                    new SqlParameter( nameof( registrationTemplateId ), registrationTemplateId ),
                    new SqlParameter( nameof( registrationInstanceId ), registrationInstanceId.Value ),
                    new SqlParameter( nameof( registrationTemplatePlacementId ), registrationTemplatePlacementId ),
                    new SqlParameter( nameof( sourceEntityId ), DBNull.Value ),
                    new SqlParameter( nameof( placementMode ), placementMode ),
                    new SqlParameter( nameof( includedRegistrationInstanceIds ), ( object ) includedRegistrationInstanceIds ?? DBNull.Value ),
                    new SqlParameter( nameof( includeFees ), includeFees ),
                    new SqlParameter( nameof( includedFeeItemIds ), ( object ) includedFeeItemIds ?? DBNull.Value ),
                    new SqlParameter( nameof( destinationGroupTypeId ), destinationGroupTypeId.Value ),
                    new SqlParameter( nameof( destinationGroupIds ), DBNull.Value ),
                    new SqlParameter( nameof( displayedCampusGuid ), ( object ) displayedCampusGuid ?? DBNull.Value ),
                    new SqlParameter( nameof( purposeKey ), DBNull.Value ),
                    new SqlParameter( nameof( registrationTemplatePurposeKey ), registrationTemplatePurposeKey ),
                    new SqlParameter( nameof( registrationInstancePurposeKey ), registrationInstancePurposeKey )
                ).ToList();
            }
            else if ( registrationTemplateId.HasValue )
            {
                placementMode = "TemplateMode";

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
                    new SqlParameter( nameof( sourceEntityTypeId ), DBNull.Value ),
                    new SqlParameter( nameof( targetEntityTypeId ), targetEntityTypeId ),
                    new SqlParameter( nameof( registrationTemplateId ), registrationTemplateId.Value ),
                    new SqlParameter( nameof( registrationInstanceId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationTemplatePlacementId ), registrationTemplatePlacementId ),
                    new SqlParameter( nameof( sourceEntityId ), DBNull.Value ),
                    new SqlParameter( nameof( placementMode ), placementMode ),
                    new SqlParameter( nameof( includedRegistrationInstanceIds ), ( object ) includedRegistrationInstanceIds ?? DBNull.Value ),
                    new SqlParameter( nameof( includeFees ), includeFees ),
                    new SqlParameter( nameof( includedFeeItemIds ), ( object ) includedFeeItemIds ?? DBNull.Value ),
                    new SqlParameter( nameof( destinationGroupTypeId ), destinationGroupTypeId.Value ),
                    new SqlParameter( nameof( destinationGroupIds ), DBNull.Value ),
                    new SqlParameter( nameof( displayedCampusGuid ), ( object ) displayedCampusGuid ?? DBNull.Value ),
                    new SqlParameter( nameof( purposeKey ), DBNull.Value ),
                    new SqlParameter( nameof( registrationTemplatePurposeKey ), registrationTemplatePurposeKey ),
                    new SqlParameter( nameof( registrationInstancePurposeKey ), registrationInstancePurposeKey )
                ).ToList();
            }
            else if ( sourceGroupId.HasValue )
            {
                placementMode = "GroupMode";
                sourceEntityId = sourceGroupId.Value;
                purposeKey = RelatedEntityPurposeKey.GroupPlacement;

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
                    new SqlParameter( nameof( registrationTemplatePlacementEntityTypeId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationInstanceEntityTypeId ), DBNull.Value ),
                    new SqlParameter( nameof( personEntityTypeId ), personEntityTypeId ),
                    new SqlParameter( nameof( sourceEntityTypeId ), targetEntityTypeId ), // We know our target entity type id is set to Group.
                    new SqlParameter( nameof( targetEntityTypeId ), targetEntityTypeId ),
                    new SqlParameter( nameof( registrationTemplateId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationInstanceId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationTemplatePlacementId ), DBNull.Value ),
                    new SqlParameter( nameof( sourceEntityId ), sourceEntityId ),
                    new SqlParameter( nameof( placementMode ), placementMode ),
                    new SqlParameter( nameof( includedRegistrationInstanceIds ), DBNull.Value ),
                    new SqlParameter( nameof( includeFees ), DBNull.Value ),
                    new SqlParameter( nameof( includedFeeItemIds ), DBNull.Value ),
                    new SqlParameter( nameof( destinationGroupTypeId ), destinationGroupTypeId.Value ),
                    new SqlParameter( nameof( destinationGroupIds ), ( object ) destinationGroupIds ?? DBNull.Value ),
                    new SqlParameter( nameof( displayedCampusGuid ), ( object ) displayedCampusGuid ?? DBNull.Value ),
                    new SqlParameter( nameof( purposeKey ), purposeKey ),
                    new SqlParameter( nameof( registrationTemplatePurposeKey ), DBNull.Value ),
                    new SqlParameter( nameof( registrationInstancePurposeKey ), DBNull.Value )
                ).ToList();
            }
            else if ( entitySetId.HasValue )
            {
                placementMode = "EntitySetMode";
                sourceEntityTypeId = EntityTypeCache.Get<Rock.Model.EntitySet>().Id;
                sourceEntityId = entitySetId.Value;
                purposeKey = RelatedEntityPurposeKey.EntitySetPlacement;

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
                    new SqlParameter( nameof( registrationTemplatePlacementEntityTypeId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationInstanceEntityTypeId ), DBNull.Value ),
                    new SqlParameter( nameof( personEntityTypeId ), personEntityTypeId ),
                    new SqlParameter( nameof( sourceEntityTypeId ), sourceEntityTypeId ),
                    new SqlParameter( nameof( targetEntityTypeId ), targetEntityTypeId ),
                    new SqlParameter( nameof( registrationTemplateId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationInstanceId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationTemplatePlacementId ), DBNull.Value ),
                    new SqlParameter( nameof( sourceEntityId ), sourceEntityId ),
                    new SqlParameter( nameof( placementMode ), placementMode ),
                    new SqlParameter( nameof( includedRegistrationInstanceIds ), DBNull.Value ),
                    new SqlParameter( nameof( includeFees ), DBNull.Value ),
                    new SqlParameter( nameof( includedFeeItemIds ), DBNull.Value ),
                    new SqlParameter( nameof( destinationGroupTypeId ), destinationGroupTypeId.Value ),
                    new SqlParameter( nameof( destinationGroupIds ), ( object ) destinationGroupIds ?? DBNull.Value ),
                    new SqlParameter( nameof( displayedCampusGuid ), ( object ) displayedCampusGuid ?? DBNull.Value ),
                    new SqlParameter( nameof( purposeKey ), purposeKey ),
                    new SqlParameter( nameof( registrationTemplatePurposeKey ), DBNull.Value ),
                    new SqlParameter( nameof( registrationInstancePurposeKey ), DBNull.Value )
                ).ToList();
            }
            else
            {
                return ActionNotFound( "Missing required keys to get Placement People." );
            }

            HashSet<int> displayedSourceAttributeIds = null;
            HashSet<int> displayedGroupMemberAttributeIds = null;

            if ( placementConfiguration.SourceAttributesToDisplay?.Any() == true )
            {
                displayedSourceAttributeIds = placementConfiguration.SourceAttributesToDisplay
                    .Select( idKey => Rock.Utility.IdHasher.Instance.GetId( idKey ) )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();
            }

            if ( placementConfiguration.DestinationGroupMemberAttributesToDisplay?.Any() == true )
            {
                displayedGroupMemberAttributeIds = placementConfiguration.DestinationGroupMemberAttributesToDisplay
                    .Select( idKey => Rock.Utility.IdHasher.Instance.GetId( idKey ) )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();
            }

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
                    Nickname = firstResult.NickName,
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

                // TODO - Institute PlacementMode Enum here.
                if ( registrationInstanceId.HasValue || registrationTemplateId.HasValue )
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

                            var attributesAndValues = GetRegistrantAttributesAndValues( r.RegistrantId, registrationTemplateId, displayedSourceAttributeIds );
                            if ( attributesAndValues != null )
                            {
                                registrantBag.Attributes = attributesAndValues.Attributes;
                                registrantBag.AttributeValues = attributesAndValues.AttributeValues;
                            }

                            return registrantBag;
                        } )
                        .ToList();
                }
                else if ( sourceGroupId.HasValue )
                {
                    var sourceGroup = GroupCache.Get( sourceGroupId.Value );

                    personBag.SourceGroupMembers = personGroup
                        .Where( p => p.GroupMemberId.HasValue && !p.GroupId.HasValue )
                        .DistinctBy( p => p.GroupMemberId )
                        .Select( p => {
                            var groupMemberBag = new GroupMemberBag
                            {
                                GroupMemberIdKey = IdHasher.Instance.GetHash( p.GroupMemberId.Value ),
                            };

                            var attributeDataBag = GetGroupMemberAttributesAndValues( p.GroupMemberId, sourceGroup.GroupTypeId, displayedSourceAttributeIds );
                            if ( attributeDataBag != null )
                            {
                                groupMemberBag.Attributes = attributeDataBag.Attributes;
                                groupMemberBag.AttributeValues = attributeDataBag.AttributeValues;
                            }

                            return groupMemberBag;
                        } )
                        .ToList();
                }

                // If GroupId exists, put them in TempGroups
                //if ( firstResult.GroupId.HasValue )
                //{
                //    var groupBag = placementPeopleBag.TempGroups.FirstOrDefault( g => g.GroupIdKey == IdHasher.Instance.GetHash( firstResult.GroupId.Value ) ); // TODO - Hashing seems overkill here.

                //    if ( groupBag == null )
                //    {
                //        groupBag = new PlacementGroupBag
                //        {
                //            GroupIdKey = IdHasher.Instance.GetHash( firstResult.GroupId.Value ),
                //            GroupMembers = new List<GroupMemberBag>()
                //        };
                //        placementPeopleBag.TempGroups.Add( groupBag );
                //    }

                //    var groupMemberBag = new GroupMemberBag
                //    {
                //        GroupMemberIdKey = firstResult.GroupMemberId.HasValue ? IdHasher.Instance.GetHash( firstResult.GroupMemberId.Value ) : null,
                //        GroupRoleIdKey = firstResult.GroupRoleId.HasValue ? IdHasher.Instance.GetHash( firstResult.GroupRoleId.Value ) : null,
                //        Attributes = GetGroupMemberAttributes( firstResult.GroupMemberId, firstResult.GroupTypeId, displayedGroupMemberAttributeIds ),
                //        AttributeValues = GetGroupMemberAttributeValues( firstResult.GroupMemberId, firstResult.GroupTypeId, displayedGroupMemberAttributeIds ),
                //        Person = personBag
                //    };

                //    groupBag.GroupMembers.Add( groupMemberBag );
                //}
                //else
                //{
                //    // No GroupId > Add directly to PeopleToPlace
                //    placementPeopleBag.PeopleToPlace.Add( personBag );
                //}

                var addedGroupMemberIds = new HashSet<int>();

                // Check for all group placements
                foreach ( var row in personGroup.Where( r => r.GroupId.HasValue && r.GroupMemberId.HasValue ) )
                {
                    var groupId = row.GroupId.Value;
                    var groupMemberId = row.GroupMemberId.Value;

                    if ( !addedGroupMemberIds.Add( groupMemberId ) )
                    {
                        // Already added — skip
                        continue;
                    }

                    var groupBag = placementPeopleBag.TempGroups
                        .FirstOrDefault( g => g.GroupIdKey == IdHasher.Instance.GetHash( groupId ) );

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

                    var attributeDataBag = GetGroupMemberAttributesAndValues( groupMemberId, row.GroupTypeId, displayedGroupMemberAttributeIds );

                    groupBag.GroupMembers.Add( new GroupMemberBag
                    {
                        GroupMemberId = groupMemberId,
                        GroupMemberIdKey = IdHasher.Instance.GetHash( groupMemberId ),
                        GroupRoleIdKey = row.GroupRoleId.HasValue
                            ? IdHasher.Instance.GetHash( row.GroupRoleId.Value )
                            : null,
                        Attributes = attributeDataBag?.Attributes,
                        AttributeValues = attributeDataBag?.AttributeValues,
                        Person = personBag
                    } );
                }

                // If the person has no placements at all
                var hasPlacements = personGroup.Any( r => r.GroupId.HasValue && r.GroupMemberId.HasValue );
                // TODO - check allow multiple here.
                if ( !hasPlacements || isPlacementAllowingMultiple )
                {
                    placementPeopleBag.PeopleToPlace.Add( personBag );
                }

            }

            return ActionOk( placementPeopleBag );

            //return placementGroupDetails;
        }

        [BlockAction]
        public BlockActionResult GetDestinationGroups( GroupPlacementKeysBag groupPlacementKeys )
        {
            int registrationTemplatePlacementEntityTypeId = EntityTypeCache.Get<Rock.Model.RegistrationTemplatePlacement>().Id;
            int registrationInstanceEntityTypeId = EntityTypeCache.Get<Rock.Model.RegistrationInstance>().Id;
            int sourceEntityTypeId;
            int targetEntityTypeId = EntityTypeCache.Get<Rock.Model.Group>().Id;
            int sourceEntityId;
            string placementMode;
            string purposeKey;
            string registrationTemplatePurposeKey = RelatedEntityPurposeKey.RegistrationTemplateGroupPlacementTemplate;
            string registrationInstancePurposeKey = RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement;
            List<DestinationGroupResult> destinationGroupResults;

            var registrationInstanceId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.RegistrationInstanceIdKey );
            var registrationTemplateId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.RegistrationTemplateIdKey );
            var registrationTemplatePlacementId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.RegistrationTemplatePlacementIdKey );
            var sourceGroupId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.SourceGroupIdKey );
            var entitySetId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.EntitySetIdKey );
            var destinationGroupTypeId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.DestinationGroupTypeIdKey );

            if ( !destinationGroupTypeId.HasValue )
            {
                // TODO - Determine if this is valid message.
                return ActionBadRequest( "Could not find Destination Group Type Id" );
            }

            //if ( !registrationTemplatePlacementId.HasValue )
            //{
            //    // TODO - refactor how ids are retrieved.
            //    return ActionBadRequest( "Could not find Registration Template Id" );
            //}

            var placementConfiguration = GetPlacementConfiguration( groupPlacementKeys );
            var includedRegistrationInstanceIds = ( placementConfiguration.IncludedRegistrationInstanceIds?.Any() == true )
                ? string.Join(
                    ",",
                    placementConfiguration.IncludedRegistrationInstanceIds
                        .Select( idKey => Rock.Utility.IdHasher.Instance.GetId( idKey ) )   // TODO - convert to helper method? I do this in a lot of places.
                        .Where( id => id.HasValue )
                        .Select( id => id.Value )
                )
                : null;

            string displayedCampusGuid = null;
            if ( placementConfiguration.DisplayedCampus?.Value.IsNotNullOrWhiteSpace() == true )
            {
                displayedCampusGuid = placementConfiguration.DisplayedCampus.Value;
            }

            var destinationGroupIds = GetIdsFromDestinationGroupParam();

            // If registration instance id has a value than we are in instance mode
            if ( registrationInstanceId.HasValue )
            {
                placementMode = "InstanceMode";

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
                        @{ nameof( includedRegistrationInstanceIds )},
                        @{ nameof( destinationGroupTypeId )},
                        @{ nameof( destinationGroupIds )},
                        @{ nameof( displayedCampusGuid )},
                        @{ nameof( purposeKey )},
                        @{nameof( registrationTemplatePurposeKey )},
                        @{nameof( registrationInstancePurposeKey )}",
                    new SqlParameter( nameof( registrationTemplatePlacementEntityTypeId ), registrationTemplatePlacementEntityTypeId ),
                    new SqlParameter( nameof( registrationInstanceEntityTypeId ), registrationInstanceEntityTypeId ),
                    new SqlParameter( nameof( sourceEntityTypeId ), DBNull.Value ),
                    new SqlParameter( nameof( targetEntityTypeId ), targetEntityTypeId ),
                    new SqlParameter( nameof( registrationTemplateId ), registrationTemplateId ),
                    new SqlParameter( nameof( registrationInstanceId ), registrationInstanceId.Value ),
                    new SqlParameter( nameof( registrationTemplatePlacementId ), registrationTemplatePlacementId ),
                    new SqlParameter( nameof( sourceEntityId ), DBNull.Value ),
                    new SqlParameter( nameof( placementMode ), placementMode ),
                    new SqlParameter( nameof( includedRegistrationInstanceIds ), ( object ) includedRegistrationInstanceIds ?? DBNull.Value ),
                    new SqlParameter( nameof( destinationGroupTypeId ), destinationGroupTypeId.Value ),
                    new SqlParameter( nameof( destinationGroupIds ), DBNull.Value ),
                    new SqlParameter( nameof( displayedCampusGuid ), ( object ) displayedCampusGuid ?? DBNull.Value ),
                    new SqlParameter( nameof( purposeKey ), DBNull.Value ),
                    new SqlParameter( nameof( registrationTemplatePurposeKey ), registrationTemplatePurposeKey ),
                    new SqlParameter( nameof( registrationInstancePurposeKey ), registrationInstancePurposeKey )
                ).ToList();
            }
            else if ( registrationTemplateId.HasValue )
            {
                placementMode = "TemplateMode";

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
                        @{ nameof( destinationGroupTypeId )},
                        @{nameof( destinationGroupIds )},
                        @{nameof( displayedCampusGuid )},
                        @{nameof( purposeKey )},
                        @{nameof( registrationTemplatePurposeKey )},
                        @{nameof( registrationInstancePurposeKey )}",
                    new SqlParameter( nameof( registrationTemplatePlacementEntityTypeId ), registrationTemplatePlacementEntityTypeId ),
                    new SqlParameter( nameof( registrationInstanceEntityTypeId ), registrationInstanceEntityTypeId ),
                    new SqlParameter( nameof( sourceEntityTypeId ), DBNull.Value ),
                    new SqlParameter( nameof( targetEntityTypeId ), targetEntityTypeId ),
                    new SqlParameter( nameof( registrationTemplateId ), registrationTemplateId.Value ),
                    new SqlParameter( nameof( registrationInstanceId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationTemplatePlacementId ), registrationTemplatePlacementId ),
                    new SqlParameter( nameof( sourceEntityId ), DBNull.Value ),
                    new SqlParameter( nameof( placementMode ), placementMode ),
                    new SqlParameter( nameof( includedRegistrationInstanceIds ), ( object ) includedRegistrationInstanceIds ?? DBNull.Value ),
                    new SqlParameter( nameof( destinationGroupTypeId ), destinationGroupTypeId.Value ),
                    new SqlParameter( nameof( destinationGroupIds ), DBNull.Value ),
                    new SqlParameter( nameof( displayedCampusGuid ), ( object ) displayedCampusGuid ?? DBNull.Value ),
                    new SqlParameter( nameof( purposeKey ), DBNull.Value ),
                    new SqlParameter( nameof( registrationTemplatePurposeKey ), registrationTemplatePurposeKey ),
                    new SqlParameter( nameof( registrationInstancePurposeKey ), registrationInstancePurposeKey )
                ).ToList();
            }
            else if ( sourceGroupId.HasValue )
            {
                placementMode = "GroupMode";
                sourceEntityId = sourceGroupId.Value;
                purposeKey = RelatedEntityPurposeKey.GroupPlacement;

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
                    new SqlParameter( nameof( registrationTemplatePlacementEntityTypeId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationInstanceEntityTypeId ), DBNull.Value ),
                    new SqlParameter( nameof( sourceEntityTypeId ), targetEntityTypeId ), // We know our target entity type id is set to Group.
                    new SqlParameter( nameof( targetEntityTypeId ), targetEntityTypeId ),
                    new SqlParameter( nameof( registrationTemplateId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationInstanceId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationTemplatePlacementId ), DBNull.Value ),
                    new SqlParameter( nameof( sourceEntityId ), sourceEntityId ),
                    new SqlParameter( nameof( placementMode ), placementMode ),
                    new SqlParameter( nameof( includedRegistrationInstanceIds ), DBNull.Value ),
                    new SqlParameter( nameof( destinationGroupTypeId ), destinationGroupTypeId.Value ),
                    new SqlParameter( nameof( destinationGroupIds ), ( object ) destinationGroupIds ?? DBNull.Value ),
                    new SqlParameter( nameof( displayedCampusGuid ), ( object ) displayedCampusGuid ?? DBNull.Value ),
                    new SqlParameter( nameof( purposeKey ), purposeKey ),
                    new SqlParameter( nameof( registrationTemplatePurposeKey ), DBNull.Value ),
                    new SqlParameter( nameof( registrationInstancePurposeKey ), DBNull.Value )
                ).ToList();
            }
            else if ( entitySetId.HasValue )
            {
                placementMode = "EntitySetMode";
                sourceEntityTypeId = EntityTypeCache.Get<Rock.Model.EntitySet>().Id;
                sourceEntityId = entitySetId.Value;
                purposeKey = RelatedEntityPurposeKey.EntitySetPlacement;

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
                    new SqlParameter( nameof( registrationTemplatePlacementEntityTypeId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationInstanceEntityTypeId ), DBNull.Value ),
                    new SqlParameter( nameof( sourceEntityTypeId ), sourceEntityTypeId ),
                    new SqlParameter( nameof( targetEntityTypeId ), targetEntityTypeId ),
                    new SqlParameter( nameof( registrationTemplateId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationInstanceId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationTemplatePlacementId ), DBNull.Value ),
                    new SqlParameter( nameof( sourceEntityId ), sourceEntityId ),
                    new SqlParameter( nameof( placementMode ), placementMode ),
                    new SqlParameter( nameof( includedRegistrationInstanceIds ), DBNull.Value ),
                    new SqlParameter( nameof( destinationGroupTypeId ), destinationGroupTypeId.Value ),
                    new SqlParameter( nameof( destinationGroupIds ), ( object ) destinationGroupIds ?? DBNull.Value ),
                    new SqlParameter( nameof( displayedCampusGuid ), ( object ) displayedCampusGuid ?? DBNull.Value ),
                    new SqlParameter( nameof( purposeKey ), purposeKey ),
                    new SqlParameter( nameof( registrationTemplatePurposeKey ), DBNull.Value ),
                    new SqlParameter( nameof( registrationInstancePurposeKey ), DBNull.Value )
                ).ToList();
            }
            else
            {
                return ActionNotFound( "Missing required keys to get Destination Groups." );
            }

            HashSet<int> groupIdsToDisplay = new HashSet<int>();

            if ( placementConfiguration.DestinationGroupAttributesToDisplay?.Any() == true )
            {
                groupIdsToDisplay = placementConfiguration.DestinationGroupAttributesToDisplay
                    .Select( idKey => Rock.Utility.IdHasher.Instance.GetId( idKey ) )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();
            }

            var placementGroupBags = destinationGroupResults
                .GroupBy( g => g.GroupId )
                .Select( g => {
                    var placementBag = new PlacementGroupBag
                    {
                        GroupId = g.Key,
                        GroupIdKey = IdHasher.Instance.GetHash( g.Key ),
                        GroupName = g.First().GroupName,
                        GroupGuid = g.First().GroupGuid,
                        GroupCapacity = g.First().GroupCapacity,
                        GroupTypeIdKey = IdHasher.Instance.GetHash( g.First().GroupTypeId ),
                        GroupOrder = g.First().GroupOrder,
                        RegistrationInstanceIdKey = g.First().RegistrationInstanceId.HasValue ? IdHasher.Instance.GetHash( g.First().RegistrationInstanceId.Value ) : string.Empty,
                        IsShared = g.First().IsShared
                    };

                    var attributeDataBag = GetGroupAttributesAndValues( g.Key, g.First().GroupTypeId, groupIdsToDisplay );
                    if ( attributeDataBag != null )
                    {
                        placementBag.Attributes = attributeDataBag.Attributes;
                        placementBag.AttributeValues = attributeDataBag.AttributeValues;
                    }

                    return placementBag;
                } ).ToList();

            return ActionOk( placementGroupBags );
        }

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
            string errorMessage = string.Empty;

            var canPlacePeople = CanPlacePeople( personIds, addGroupMembersBag.GroupPlacementKeys.DestinationGroupTypeIdKey, addGroupMembersBag.TargetGroup.GroupIdKey, out errorMessage );

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
                groupRoleId.HasValue &&
                !IsGroupRoleCapacityAvailable( groupRoleId.Value, groupId.Value, addGroupMembersBag.PendingGroupMembers.Count, out errorMessage )
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

                // TODO - may need to populate attributes here

                if ( !groupMember.IsValid )
                {
                    errorMessage = groupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                    return ActionBadRequest( errorMessage );
                }


                groupMemberService.Add( groupMember );

                memberPairs.Add( (bag, groupMember) );
            }

            RockContext.SaveChanges();

            foreach ( var (bag, entity) in memberPairs )
            {
                bag.GroupMemberId = entity.Id;
                bag.GroupMemberIdKey = entity.IdKey;
            }

            return ActionOk( addGroupMembersBag.PendingGroupMembers );
        }

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

            groupMemberService.Delete( groupMember );
            RockContext.SaveChanges();
            return ActionOk();
        }

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

                newPlacementGroup.SaveAttributeValues();
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

                string errorMessage;
                if ( !HasValidChildGroups( parentGroup.Id, groupTypeId.Value, out errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                var existingPlacementGroups = groupService.Queryable().Where( a => a.ParentGroupId == parentGroup.Id && a.IsActive == true ).ToList();
                placementGroups = existingPlacementGroups;
            }

            // TODO - update with groups placement logic
            var registrationInstanceService = new RegistrationInstanceService( RockContext );
            var registrationTemplatePlacementService = new RegistrationTemplatePlacementService( RockContext );

            foreach ( var placementGroup in placementGroups )
            {
                if ( registrationInstanceId.HasValue )
                {
                    var registrationInstance = registrationInstanceService.Get( registrationInstanceId.Value );
                    registrationInstanceService.GetRegistrationInstancePlacementGroupsByPlacement( registrationInstanceId.Value, registrationTemplatePlacementId.Value );

                    // in RegistrationInstanceMode
                    registrationInstanceService.AddRegistrationInstancePlacementGroup( registrationInstance, placementGroup, registrationTemplatePlacementId.Value );
                }
                else if ( registrationTemplatePlacementId.HasValue )
                {
                    var registrationTemplatePlacement = registrationTemplatePlacementService.Get( registrationTemplatePlacementId.Value );
                    registrationTemplatePlacementService.AddRegistrationTemplatePlacementPlacementGroup( registrationTemplatePlacement, placementGroup );
                }
                else if ( sourceGroupId.HasValue )
                {
                    var sourceGroup = groupService.Get( sourceGroupId.Value );
                    groupService.AddGroupPlacementPlacementGroup( sourceGroup, placementGroup );
                }
                else if ( entitySetId.HasValue )
                {
                    var entitySetService = new EntitySetService( RockContext );
                    var entitySet = entitySetService.Get( entitySetId.Value );

                    entitySetService.AddEntitySetPlacementGroup( entitySet, placementGroup );
                }
            }

            RockContext.SaveChanges();

            // TODO - return the added groups
            return ActionOk();
        }

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

                if ( registrationInstanceId.HasValue )
                {
                    var registrationInstanceService = new RegistrationInstanceService( RockContext );
                    var registrationInstance = registrationInstanceService.Get( registrationInstanceId.Value );

                    if ( registrationInstance == null )
                    {
                        return ActionNotFound( "Specified registration instance not found." );
                    }

                    registrationInstanceService.DeleteRegistrationInstancePlacementGroup( registrationInstance, group, registrationTemplatePlacementId.Value );
                }
                else
                {
                    var registrationTemplatePlacementService = new RegistrationTemplatePlacementService( RockContext );
                    var registrationTemplatePlacement = registrationTemplatePlacementService.Get( registrationTemplatePlacementId.Value );

                    if ( registrationTemplatePlacement == null )
                    {
                        return ActionNotFound( "Specified registration template placement not found." );
                    }

                    registrationTemplatePlacementService.DeleteRegistrationTemplatePlacementPlacementGroup( registrationTemplatePlacement, group );
                }
            }
            else if ( detachGroupBag.PlacementMode == PlacementMode.GroupMode )
            {
                var sourceGroupId = Rock.Utility.IdHasher.Instance.GetId( detachGroupBag.GroupPlacementKeys.SourceGroupIdKey );

                if ( !sourceGroupId.HasValue )
                {
                    return ActionNotFound( "Specified Source Group not found." );
                }

                var sourceGroup = groupService.Get( sourceGroupId.Value );

                groupService.DetachDestinationGroupFromSourceGroup( sourceGroup, group );
            }
            else if ( detachGroupBag.PlacementMode == PlacementMode.EntitySetMode )
            {
                var entitySetId = Rock.Utility.IdHasher.Instance.GetId( detachGroupBag.GroupPlacementKeys.EntitySetIdKey );

                if ( !entitySetId.HasValue )
                {
                    return ActionNotFound( "Specified Entity Set not found." );
                }

                var entitySetService = new EntitySetService( RockContext );
                var entitySet = entitySetService.Get( entitySetId.Value );
                entitySetService.DetachDestinationGroupFromEntitySet( entitySet, group );                
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

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

        [BlockAction]
        public BlockActionResult PopulateAttributeFilters( GroupPlacementKeysBag groupPlacementKeys )
        {
            var placementConfiguration = GetPlacementConfiguration( groupPlacementKeys );
            HashSet<int> displayedSourceAttributeIds = new HashSet<int>();
            HashSet<int> groupAttributeIds = new HashSet<int>();
            HashSet<int> groupMemberAttributeIds = new HashSet<int>();
            AttributeFiltersBag attributeFiltersBag = new AttributeFiltersBag();

            // TODO - I could also pull from page params.
            var sourceGroupId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.SourceGroupIdKey );
            var registrationTemplateId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.RegistrationTemplateIdKey );
            //var registrationInstanceId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.RegistrationInstanceIdKey );
            var registrationTemplatePlacementId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.RegistrationTemplatePlacementIdKey );
            var destinationGroupTypeId = Rock.Utility.IdHasher.Instance.GetId( groupPlacementKeys.DestinationGroupTypeIdKey );

            Rock.Model.Group fakeGroup = null;
            if ( destinationGroupTypeId.HasValue )
            {
                fakeGroup = new Rock.Model.Group
                {
                    GroupTypeId = destinationGroupTypeId.Value
                };
            }

            if ( placementConfiguration.SourceAttributesToDisplay?.Any() == true )
            {
                displayedSourceAttributeIds = placementConfiguration.SourceAttributesToDisplay
                    .Select( idKey => Rock.Utility.IdHasher.Instance.GetId( idKey ) )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();

                if ( sourceGroupId.HasValue )
                {
                    var fakeGroupMember = new GroupMember
                    {
                        Group = fakeGroup,
                        GroupId = fakeGroup.Id
                    };

                    fakeGroupMember.LoadAttributes();

                    attributeFiltersBag.SourceAttributesForFilter = fakeGroupMember.GetPublicAttributesForEdit( GetCurrentPerson(), true, attributeFilter: a => displayedSourceAttributeIds.Contains( a.Id ) );
                    attributeFiltersBag.SourceAttributeValuesForFilter = fakeGroupMember.GetPublicAttributeValuesForEdit( GetCurrentPerson(), true, attributeFilter: a => displayedSourceAttributeIds.Contains( a.Id ) );
                }
                else if ( registrationTemplateId.HasValue ) // TODO - Think of scenarios where this might be null but we still want data.
                {
                    RegistrationRegistrant fakeRegistrant = new RegistrationRegistrant
                    {
                        RegistrationTemplateId = registrationTemplateId.Value
                    };

                    fakeRegistrant.LoadAttributes();

                    // TODO - Maybe use our method instead.
                    attributeFiltersBag.SourceAttributesForFilter = fakeRegistrant.GetPublicAttributesForEdit( GetCurrentPerson(), true, attributeFilter: a => displayedSourceAttributeIds.Contains( a.Id ) );
                    attributeFiltersBag.SourceAttributeValuesForFilter = fakeRegistrant.GetPublicAttributeValuesForEdit( GetCurrentPerson(), true, attributeFilter: a => displayedSourceAttributeIds.Contains( a.Id ) );
                }
            }

            if ( placementConfiguration.DestinationGroupAttributesToDisplay?.Any() == true && fakeGroup != null )
            {
                groupAttributeIds = placementConfiguration.DestinationGroupAttributesToDisplay
                    .Select( idKey => Rock.Utility.IdHasher.Instance.GetId( idKey ) )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();

                // TODO - Need to talk to Jon about potential Destination Group Id Key from query string.
                
                fakeGroup.LoadAttributes();

                attributeFiltersBag.DestinationGroupAttributesForFilter = fakeGroup.GetPublicAttributesForEdit( GetCurrentPerson(), true, attributeFilter: a => groupAttributeIds.Contains( a.Id ) );
                attributeFiltersBag.DestinationGroupAttributeValuesForFilter = fakeGroup.GetPublicAttributeValuesForEdit( GetCurrentPerson(), true, attributeFilter: a => groupAttributeIds.Contains( a.Id ) );
            }

            if ( placementConfiguration.DestinationGroupMemberAttributesToDisplay?.Any() == true && fakeGroup != null )
            {
                groupMemberAttributeIds = placementConfiguration.DestinationGroupMemberAttributesToDisplay
                    .Select( idKey => Rock.Utility.IdHasher.Instance.GetId( idKey ) )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();

                var fakeGroupMember = new GroupMember
                {
                    Group = fakeGroup,
                    GroupId = fakeGroup.Id // TODO - do i even have this??
                };

                fakeGroupMember.LoadAttributes();

                attributeFiltersBag.DestinationGroupMemberAttributesForFilter = fakeGroupMember.GetPublicAttributesForEdit( GetCurrentPerson(), true, attributeFilter: a => groupMemberAttributeIds.Contains( a.Id ) );
                attributeFiltersBag.DestinationGroupMemberAttributeValuesForFilter = fakeGroupMember.GetPublicAttributeValuesForEdit( GetCurrentPerson(), true, attributeFilter: a => groupMemberAttributeIds.Contains( a.Id ) );
            }

            if ( placementConfiguration.AreFeesDisplayed && registrationTemplateId.HasValue )
            {
                var feeItemList = new RegistrationTemplateFeeService( RockContext )
                    .Queryable()
                    .Where( f => f.RegistrationTemplateId == registrationTemplateId.Value )
                    .ToList()                                                               // TODO - to list here is hurting performance potentially.
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
            HashSet<int> displayedGroupMemberAttributeIds = null;

            if ( placementConfiguration.SourceAttributesToDisplay?.Any() == true )
            {
                displayedSourceAttributeIds = placementConfiguration.SourceAttributesToDisplay
                    .Select( idKey => Rock.Utility.IdHasher.Instance.GetId( idKey ) )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();
            }

            if ( placementConfiguration.DestinationGroupMemberAttributesToDisplay?.Any() == true )
            {
                displayedGroupMemberAttributeIds = placementConfiguration.DestinationGroupMemberAttributesToDisplay
                    .Select( idKey => Rock.Utility.IdHasher.Instance.GetId( idKey ) )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();
            }

            if ( sourceAndDestinationEntityKeys.PlacementMode == PlacementMode.GroupMode )
            {
                foreach ( var sourceEntity in sourceAndDestinationEntityKeys.SourceEntities )
                {
                    var entityId = IdHasher.Instance.GetId( sourceEntity.EntityIdKey );
                    var typeId = IdHasher.Instance.GetId( sourceEntity.EntityTypeIdKey );

                    resultBag.SourceEntityAttributes[sourceEntity.EntityIdKey] = GetGroupMemberAttributesAndValues( entityId, typeId, displayedSourceAttributeIds );
                }
            }
            else if ( sourceAndDestinationEntityKeys.PlacementMode == PlacementMode.TemplateMode || sourceAndDestinationEntityKeys.PlacementMode == PlacementMode.InstanceMode )
            {
                foreach ( var sourceEntity in sourceAndDestinationEntityKeys.SourceEntities )
                {
                    var entityId = IdHasher.Instance.GetId( sourceEntity.EntityIdKey );
                    var typeId = IdHasher.Instance.GetId( sourceEntity.EntityTypeIdKey );

                    resultBag.SourceEntityAttributes[sourceEntity.EntityIdKey] = GetRegistrantAttributesAndValues( entityId, typeId, displayedSourceAttributeIds );
                }
            }

            foreach ( var destinationEntity in sourceAndDestinationEntityKeys.DestinationEntities )
            {
                var entityId = IdHasher.Instance.GetId( destinationEntity.EntityIdKey );
                var typeId = IdHasher.Instance.GetId( destinationEntity.EntityTypeIdKey );
                resultBag.DestinationEntityAttributes[destinationEntity.EntityIdKey] = GetGroupMemberAttributesAndValues( entityId, typeId, displayedGroupMemberAttributeIds );
            }

            return ActionOk( resultBag );
        }

        #endregion Block Actions
    }
}
