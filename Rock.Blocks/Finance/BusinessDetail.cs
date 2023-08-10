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
using System.Linq;
using System.Text;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Search.Person;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.BusinessDetail;
using Rock.ViewModels.Controls;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays the details of a particular business.
    /// </summary>
    /// <seealso cref="RockObsidianDetailBlockType" />

    [DisplayName( "Business Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the given business." )]
    [IconCssClass( "fa fa-question" )]

    #region Block Attributes

    [LinkedPage( "Communication Page",
        Description = "The communication page to use for when the business email address is clicked. Leave this blank to use the default.",
        Key = AttributeKey.CommunicationPage,
        IsRequired = false,
        Order = 0 )]

    [BadgesField(
        "Badges",
        Key = AttributeKey.Badges,
        Description = "The label badges to display in this block.",
        IsRequired = false,
        Order = 1 )]

    [BooleanField(
        "Display Tags",
        Description = "Should tags be displayed?",
        Key = AttributeKey.DisplayTags,
        DefaultBooleanValue = true,
        Order = 2 )]

    [CategoryField(
        "Tag Category",
        Description = "Optional category to limit the tags to. If specified all new personal tags will be added with this category.",
        Key = AttributeKey.TagCategory,
        AllowMultiple = false,
        EntityType = typeof( Rock.Model.Tag ),
        IsRequired = false,
        Order = 3 )]

    [CustomEnhancedListField(
        "Search Key Types",
        Key = AttributeKey.SearchKeyTypes,
        Description = "Optional list of search key types to limit the display in search keys grid. No selection will show all.",
        ListSource = ListSource.SearchKeyTypes,
        IsRequired = false,
        Order = 4 )]

    [AttributeField(
        "Business Attributes",
        Key = AttributeKey.BusinessAttributes,
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        Description = "The person attributes that should be displayed / edited for adults.",
        IsRequired = false,
        AllowMultiple = true,
        Order = 5 )]

    [WorkflowTypeField(
        "Workflow Actions",
        Key = AttributeKey.WorkflowActions,
        Description = "The workflows to make available as actions.",
        AllowMultiple = true,
        IsRequired = false,
        Order = 6 )]

    [CodeEditorField(
        "Additional Custom Actions",
        Key = AttributeKey.AdditionalCustomActions,
        Description = BlockAttributeDescription.AdditionalCustomActions,
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        Order = 7 )]

    #endregion BlockAttributes

    [SystemGuid.EntityTypeGuid( "d54d7307-40f2-4beb-819d-8112dfbfbb12" )]
    [SystemGuid.BlockTypeGuid( "729e1953-4cff-46f0-8715-9d7892badb4e" )]
    public class BusinessDetail : RockDetailBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Badges = "Badges";
            public const string BusinessAttributes = "BusinessAttributes";
            public const string CommunicationPage = "CommunicationPage";
            public const string DisplayTags = "DisplayTags";
            public const string SearchKeyTypes = "SearchKeyTypes";
            public const string TagCategory = "TagCategory";
            public const string WorkflowActions = "WorkflowActions";
            public const string AdditionalCustomActions = "AdditionalCustomActions";
        }

        private static class BlockAttributeDescription
        {
            public const string AdditionalCustomActions = @"
Additional custom actions (will be displayed after the list of workflow actions). Any instance of '{0}' will be replaced with the current business's id.
Because the contents of this setting will be rendered inside a &lt;ul&gt; element, it is recommended to use an 
&lt;li&gt; element for each available action.  Example:
<pre>
    &lt;li&gt;&lt;a href='~/WorkflowEntry/4?PersonId={0}' tabindex='0'&gt;Fourth Action&lt;/a&gt;&lt;/li&gt;
</pre>";
        }

        private static class ListSource
        {
            public const string SearchKeyTypes = @"
                SELECT CAST( V.[Guid] as varchar(40) ) AS [Value], V.[Value] AS [Text]
                FROM [DefinedType] T
                INNER JOIN [DefinedValue] V ON V.[DefinedTypeId] = T.[Id]
                LEFT OUTER JOIN [AttributeValue] AV ON AV.[EntityId] = V.[Id]
	                AND AV.[AttributeId] = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '15C419AA-76A9-4105-AB99-8384AB0E9B44')
	                AND AV.[Value] = 'False'
                WHERE T.[Guid] = '61BDD0E3-173D-45AB-9E8C-1FBB9FA8FDF3'
	                AND AV.[Id] IS NULL
                ORDER BY V.[Order]";
        }

        private static class PageParameterKey
        {
            public const string BusinessId = "BusinessId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<BusinessDetailBag, BusinessDetailOptionsBag>();

                var entity = SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext, entity );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<Person>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="business">The busniess entity.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private BusinessDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext, Person business )
        {
            var options = new BusinessDetailOptionsBag();

            if ( business != null )
            {
                var validSearchTypes = GetValidSearchKeyTypes();
                var dvAlternateId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() );
                var searchKeys = business.GetPersonSearchKeys()
                    .Where( a => validSearchTypes.Contains( a.SearchTypeValue.Guid ) && a.SearchTypeValueId != dvAlternateId.Id )
                    .ToList();

                var searchValueTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_SEARCH_KEYS ).DefinedValues;
                var searchTypesList = searchValueTypes.Where( dv => validSearchTypes.Contains( dv.Guid ) && dv.Id != dvAlternateId.Id ).ToList();

                options.SearchKeys = searchKeys.ConvertAll(a => new SearchKeyBag() { Guid = a.Guid, SearchType = a.SearchTypeValue.ToListItemBag(), SearchValue = a.SearchValue });
                options.SearchTypesList = searchTypesList.ConvertAll( dv => new ViewModels.Utility.ListItemBag { Text = dv.Value, Value = dv.Guid.ToString() } );

                options.TagCategoryGuid = GetAttributeValue( AttributeKey.TagCategory ).AsGuidOrNull();
            }

            return options;
        }

        /// <summary>
        /// Validates the business for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="business">The business to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Business is valid, <c>false</c> otherwise.</returns>
        private bool ValidateBusiness( Person business, RockContext rockContext, out string errorMessage )
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
        private Person SetBoxInitialEntityState( DetailBlockBox<BusinessDetailBag, BusinessDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = "The business was not found.";
                return null;
            }

            var isViewable = entity.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

            // Existing entity was found, prepare for view mode by default.
            if ( isViewable )
            {
                box.Entity = GetEntityBagForView( entity, rockContext );
                box.SecurityGrantToken = GetSecurityGrantToken( entity );
            }
            else
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Person.FriendlyTypeName );
            }

            return entity;
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="BusinessDetailBag"/> that represents the entity.</returns>
        private BusinessDetailBag GetCommonEntityBag( Person entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new BusinessDetailBag
            {
                IdKey = entity.IdKey,
                BusinessName = entity.LastName,
                EmailAddress = entity.Email,
                RecordStatus = entity.RecordStatusValue.ToListItemBag(),
                RecordStatusReason = entity.RecordStatusReasonValue.ToListItemBag(),
                Campus = entity.PrimaryCampus.ToListItemBag(),
                CustomActions = CreateActionMenu( entity.Id )
            };

            // Get Phone Number
            var workPhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
            if ( workPhoneType != null )
            {
                var phoneNumber = entity.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == workPhoneType.Id );
                if ( phoneNumber != null )
                {
                    bag.PhoneNumber = phoneNumber.ToString();
                    bag.IsSmsChecked = phoneNumber.IsMessagingEnabled;
                    bag.IsUnlistedChecked = phoneNumber.IsUnlisted;
                    bag.CountryCode = phoneNumber.CountryCode;
                }
            }

            return bag;
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="BusinessDetailBag"/> that represents the entity.</returns>
        private BusinessDetailBag GetEntityBagForView( Person entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            // Get addresses
            var workLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() );
            if ( workLocationType != null && entity.GivingGroup != null )
            {
                var location = entity.GivingGroup.GroupLocations
                    .Where( gl => gl.GroupLocationTypeValueId == workLocationType.Id )
                    .Select( gl => gl.Location )
                    .FirstOrDefault();

                if ( location != null )
                {
                    bag.AddressAsHtml = location.GetFullStreetAddress().ConvertCrLfToHtmlBr();
                }
            }

            var attributeGuidList = GetAttributeValue( AttributeKey.BusinessAttributes ).SplitDelimitedValues().AsGuidList();

            if ( attributeGuidList.Any() )
            {
                bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, attributeFilter: a => attributeGuidList.Any( ag => a.Guid == ag  )  );
            }

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="BusinessDetailBag"/> that represents the entity.</returns>
        private BusinessDetailBag GetEntityBagForEdit( Person entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.EmailPreference = entity.EmailPreference.ToString();

            // Get addresses
            var workLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() );
            if ( workLocationType != null && entity.GivingGroup != null )
            {
                var location = entity.GivingGroup.GroupLocations
                    .Where( gl => gl.GroupLocationTypeValueId == workLocationType.Id )
                    .Select( gl => gl.Location )
                    .FirstOrDefault();

                if ( location != null )
                {
                    bag.Address = new AddressControlBag
                    {
                        Street1 = location.Street1,
                        Street2 = location.Street2,
                        City = location.City,
                        State = location.State,
                        PostalCode = location.PostalCode,
                        Country = location.Country,
                    };
                }

                // Get Previous Addresses
                var previousLocations = new GroupLocationHistoricalService( rockContext )
                    .Queryable()
                    .Where( h => h.GroupId == entity.GivingGroup.Id && h.GroupLocationTypeValueId == workLocationType.Id )
                    .OrderBy( h => h.EffectiveDateTime )
                    .Select( h => h.Location )
                    .ToList();

                foreach ( var previousLocation in previousLocations )
                {
                    bag.PreviousAddress = previousLocation.GetFullStreetAddress().ConvertCrLfToHtmlBr();
                }
            }

            var attributeGuidList = GetAttributeValue( AttributeKey.BusinessAttributes ).SplitDelimitedValues().AsGuidList();

            if ( attributeGuidList.Any() )
            {
                bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, attributeFilter: a => attributeGuidList.Any( ag => a.Guid == ag ) );
            }

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( Person entity, DetailBlockBox<BusinessDetailBag, BusinessDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.BusinessName ),
                () => entity.LastName = box.Entity.BusinessName );

            box.IfValidProperty( nameof( box.Entity.RecordStatus ),
                () => entity.RecordStatusValueId = box.Entity.RecordStatus.GetEntityId<DefinedValue>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.EmailAddress ),
                () => entity.Email = box.Entity.EmailAddress );

            box.IfValidProperty( nameof( box.Entity.EmailPreference ),
                () => entity.EmailPreference = box.Entity.EmailPreference.ConvertToEnum<EmailPreference>() );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="Person"/> to be viewed or edited on the page.</returns>
        private Person GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<Person, PersonService>( rockContext, PageParameterKey.BusinessId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
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
        private string GetSecurityGrantToken( Person entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out Person entity, out BlockActionResult error )
        {
            var entityService = new PersonService( rockContext );
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
                entity = new Person();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Person.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Person.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the valid search key types.
        /// </summary>
        /// <returns></returns>
        private List<Guid> GetValidSearchKeyTypes()
        {
            var searchKeyTypes = new List<Guid> { Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() };

            var dt = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_SEARCH_KEYS );
            if ( dt != null )
            {
                var values = dt.DefinedValues;
                var searchTypesList = this.GetAttributeValue( AttributeKey.SearchKeyTypes ).SplitDelimitedValues().AsGuidList();
                if ( searchTypesList.Any() )
                {
                    values = values.Where( v => searchTypesList.Contains( v.Guid ) ).ToList();
                }
                searchKeyTypes.AddRange( values.Where( dv => dv.GetAttributeValue( "UserSelectable" ).AsBoolean() ).Select( dv => dv.Guid ) );
            }

            return searchKeyTypes;

        }

        /// <summary>
        /// Updates the group member.
        /// </summary>
        /// <param name="businessId">The business identifier.</param>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private GroupMember UpdateGroupMember( int businessId, GroupTypeCache groupType, string groupName, int? campusId, int groupRoleId, RockContext rockContext )
        {
            var groupMemberService = new GroupMemberService( rockContext );

            GroupMember groupMember = groupMemberService.Queryable( "Group" )
                .Where( m =>
                    m.PersonId == businessId &&
                    m.GroupRoleId == groupRoleId )
                .FirstOrDefault();

            if ( groupMember == null )
            {
                groupMember = new GroupMember();
                groupMember.Group = new Rock.Model.Group();
            }

            groupMember.PersonId = businessId;
            groupMember.GroupRoleId = groupRoleId;
            groupMember.GroupMemberStatus = GroupMemberStatus.Active;

            groupMember.Group.GroupTypeId = groupType.Id;
            groupMember.Group.Name = groupName;
            groupMember.Group.CampusId = campusId;

            if ( groupMember.Id == 0 )
            {
                groupMemberService.Add( groupMember );

                /*
                     6/20/2022 - SMC

                     We need to save the new Group to the database so that an Id is assigned.  This
                     Id is necessary to calculate the correct GivingId for the business, otherwise
                     all new businesses are given a GivingId of "G0" until the Rock Cleanup Job runs,
                     which causes the transactions to appear on any new records (because they all
                     have the same GivingId).

                     Reason: Transactions showing up on records they don't belong to.
                */
                rockContext.SaveChanges();
            }

            return groupMember;
        }

        private string CreateActionMenu( int businessId )
        {
            if ( businessId == 0 )
            {
                return string.Empty;
            }

            var sbActions = new StringBuilder();
            var hasCustomActions = false;
            var hasWorkflowActions = false;

            // First list the actions manually entered as html in the block settting
            var actions = GetAttributeValue( AttributeKey.AdditionalCustomActions );
            if ( !string.IsNullOrWhiteSpace( actions ) )
            {
                hasCustomActions = true;
                string appRoot = this.RequestContext.ResolveRockUrl( "~/" );
                string themeRoot = this.RequestContext.ResolveRockUrl( "~~/" );
                actions = actions.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

                if ( actions.Contains( "{0}" ) )
                {
                    actions = string.Format( actions, businessId );
                }

                sbActions.Append( actions );
            }

            // Next list the workflow actions selected in the picker
            var workflowActions = GetAttributeValue( AttributeKey.WorkflowActions );
            if ( !string.IsNullOrWhiteSpace( workflowActions ) )
            {
                hasWorkflowActions = true;
                List<WorkflowType> workflowTypes = new List<WorkflowType>();

                using ( var rockContext = new RockContext() )
                {
                    var workflowTypeService = new WorkflowTypeService( rockContext );
                    foreach ( string guidValue in workflowActions.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                    {
                        Guid? guid = guidValue.AsGuidOrNull();
                        if ( guid.HasValue )
                        {
                            var workflowType = workflowTypeService.Get( guid.Value );
                            if ( workflowType != null && ( workflowType.IsActive ?? true ) && workflowType.IsAuthorized( Rock.Security.Authorization.VIEW, GetCurrentPerson() ) )
                            {
                                workflowTypes.Add( workflowType );
                            }
                        }
                    }
                }

                workflowTypes = workflowTypes.OrderBy( w => w.Name ).ToList();

                if ( hasCustomActions && workflowTypes.Count > 0 )
                {
                    sbActions.Append( "<li role=\"separator\" class=\"divider\"></li>" );
                }

                foreach ( var workflowType in workflowTypes )
                {
                    string url = string.Format( "~/WorkflowEntry/{0}?PersonId={1}", workflowType.Id, businessId );
                    sbActions.AppendFormat(
                        "<li><a href='{0}'><i class='fa-fw {1}'></i> {2}</a></li>",
                        this.RequestContext.ResolveRockUrl( url ),
                        workflowType.IconCssClass,
                        workflowType.Name );
                    sbActions.AppendLine();
                }
            }

            return hasCustomActions || hasWorkflowActions ? sbActions.ToString() : null;
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

                var box = new DetailBlockBox<BusinessDetailBag, BusinessDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<BusinessDetailBag, BusinessDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new PersonService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var business, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( business, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                var isNew = business.Id == 0;

                // Phone Number
                var businessPhoneTypeId = new DefinedValueService( rockContext ).GetByGuid( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ) ).Id;

                var phoneNumber = business.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == businessPhoneTypeId );

                if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( box.Entity.PhoneNumber ) ) )
                {
                    if ( phoneNumber == null )
                    {
                        phoneNumber = new PhoneNumber { NumberTypeValueId = businessPhoneTypeId };
                        business.PhoneNumbers.Add( phoneNumber );
                    }

                    phoneNumber.CountryCode = PhoneNumber.CleanNumber( box.Entity.CountryCode );
                    phoneNumber.Number = PhoneNumber.CleanNumber( box.Entity.PhoneNumber );
                    phoneNumber.IsMessagingEnabled = box.Entity.IsSmsChecked;
                    phoneNumber.IsUnlisted = box.Entity.IsUnlistedChecked;
                }
                else
                {
                    if ( phoneNumber != null )
                    {
                        business.PhoneNumbers.Remove( phoneNumber );
                        new PhoneNumberService( rockContext ).Delete( phoneNumber );
                    }
                }

                // Record Type - this is always "business". it will never change.
                business.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

                // Record Status Reason
                int? newRecordStatusReasonId = null;
                if ( business.RecordStatusValueId.HasValue && business.RecordStatusValueId.Value == DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id )
                {
                    newRecordStatusReasonId = box.Entity.RecordStatusReason.GetEntityId<DefinedValue>( rockContext );
                }
                business.RecordStatusReasonValueId = newRecordStatusReasonId;

                // Email
                business.IsEmailActive = true;

                // Ensure everything is valid before saving.
                if ( !ValidateBusiness( business, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    // Add/Update Family Group
                    var familyGroupType = GroupTypeCache.GetFamilyGroupType();
                    int adultRoleId = familyGroupType.Roles
                        .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) )
                        .Select( r => r.Id )
                        .FirstOrDefault();
                    var adultFamilyMember = UpdateGroupMember( business.Id, familyGroupType, business.LastName + " Business", box.Entity.Campus.GetEntityId<Campus>( rockContext ), adultRoleId, rockContext );
                    business.GivingGroup = adultFamilyMember.Group;

                    // Add/Update Known Relationship Group Type
                    var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
                    int knownRelationshipOwnerRoleId = knownRelationshipGroupType.Roles
                        .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ) )
                        .Select( r => r.Id )
                        .FirstOrDefault();
                    var knownRelationshipOwner = UpdateGroupMember( business.Id, knownRelationshipGroupType, "Known Relationship", null, knownRelationshipOwnerRoleId, rockContext );

                    // Add/Update Implied Relationship Group Type
                    var impliedRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_PEER_NETWORK.AsGuid() );
                    int impliedRelationshipOwnerRoleId = impliedRelationshipGroupType.Roles
                        .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_PEER_NETWORK_OWNER.AsGuid() ) )
                        .Select( r => r.Id )
                        .FirstOrDefault();
                    var impliedRelationshipOwner = UpdateGroupMember( business.Id, impliedRelationshipGroupType, "Implied Relationship", null, impliedRelationshipOwnerRoleId, rockContext );

                    rockContext.SaveChanges();

                    // Location
                    int workLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK ).Id;

                    var groupLocationService = new GroupLocationService( rockContext );
                    var workLocation = groupLocationService.Queryable( "Location" )
                        .Where( gl =>
                            gl.GroupId == adultFamilyMember.Group.Id &&
                            gl.GroupLocationTypeValueId == workLocationTypeId )
                        .FirstOrDefault();

                    if ( string.IsNullOrWhiteSpace( box.Entity.Address.Street1 ) )
                    {
                        if ( workLocation != null )
                        {
                            if ( box.Entity.SaveFormerAddressAsPreviousAddress )
                            {
                                GroupLocationHistorical.CreateCurrentRowFromGroupLocation( workLocation, RockDateTime.Now );
                            }

                            groupLocationService.Delete( workLocation );
                        }
                    }
                    else
                    {
                        var newLocation = new LocationService( rockContext ).Get( box.Entity.Address.Street1, box.Entity.Address.Street2, box.Entity.Address.City, box.Entity.Address.State, box.Entity.Address.PostalCode, box.Entity.Address.Country );
                        if ( workLocation == null )
                        {
                            workLocation = new GroupLocation();
                            groupLocationService.Add( workLocation );
                            workLocation.GroupId = adultFamilyMember.Group.Id;
                            workLocation.GroupLocationTypeValueId = workLocationTypeId;
                        }
                        else
                        {
                            // Save this to history if the box is checked and the new info is different than the current one.
                            if ( box.Entity.SaveFormerAddressAsPreviousAddress && newLocation.Id != workLocation.Location.Id )
                            {
                                new GroupLocationHistoricalService( rockContext ).Add( GroupLocationHistorical.CreateCurrentRowFromGroupLocation( workLocation, RockDateTime.Now ) );
                            }
                        }

                        workLocation.Location = newLocation;
                        workLocation.IsMailingLocation = true;
                    }

                    rockContext.SaveChanges();
                } );

                /* Ethan Drotning 2022-01-11
                 * Need save the PersonSearchKeys outside of the transaction since the DB might not have READ_COMMITTED_SNAPSHOT enabled.
                 */

                // PersonSearchKey
                if ( !isNew )
                {
                    var personSearchKeyService = new PersonSearchKeyService( rockContext );
                    var validSearchTypes = GetValidSearchKeyTypes();
                    var databaseSearchKeys = personSearchKeyService.Queryable().Where( a => a.PersonAlias.PersonId == business.Id && validSearchTypes.Contains( a.SearchTypeValue.Guid ) ).ToList();

                    foreach ( var deletedSearchKey in databaseSearchKeys.Where( a => !box.Entity.SearchKeys.Any( p => p.Guid == a.Guid ) ) )
                    {
                        personSearchKeyService.Delete( deletedSearchKey );
                    }

                    foreach ( var searchKeyBag in box.Entity.SearchKeys.Where( a => !databaseSearchKeys.Any( d => d.Guid == a.Guid ) ) )
                    {
                        personSearchKeyService.Add( new PersonSearchKey { SearchValue = searchKeyBag.SearchValue, SearchTypeValueId = searchKeyBag.SearchType.GetEntityId<DefinedValue>(rockContext) ?? 0, Guid = Guid.NewGuid(), PersonAliasId = business.PrimaryAliasId.Value } );
                    }
                }

                rockContext.SaveChanges();

                business.SaveAttributeValues();

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    business.SaveAttributeValues( rockContext );
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.BusinessId] = business.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                business = entityService.Get( business.Id );
                business.LoadAttributes( rockContext );

                return ActionOk( GetEntityBagForView( business, rockContext ) );
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
                var entityService = new PersonService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<BusinessDetailBag, BusinessDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<BusinessDetailBag, BusinessDetailOptionsBag>
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
