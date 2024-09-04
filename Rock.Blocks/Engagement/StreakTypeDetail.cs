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

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Tasks;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.StreakTypeDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays the details of a particular streak type.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Streak Type Detail" )]
    [Category( "Engagement" )]
    [Description( "Displays the details of a particular streak type." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Map Editor Page",
        Description = "Page used for editing the streak type map.",
        Key = AttributeKey.MapEditorPage,
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Exclusions Page",
        Description = "Page used for viewing a list of streak type exclusions.",
        Key = AttributeKey.ExclusionsPage,
        IsRequired = false,
        Order = 2 )]

    [LinkedPage(
        "Achievements Page",
        Description = "Page used for viewing a list of streak type achievement types.",
        Key = AttributeKey.AchievementsPage,
        IsRequired = false,
        Order = 3 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "8a8c5bea-6293-4ac0-8c2e-d89f541043aa" )]
    [Rock.SystemGuid.BlockTypeGuid( "a83a1f49-10a6-4362-acc3-8027224a2120" )]
    public class StreakTypeDetail : RockDetailBlockType, IBreadCrumbBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// Key for the Map Editor Page
            /// </summary>
            public const string MapEditorPage = "MapEditorPage";

            /// <summary>
            /// Key for the Exclusions Page
            /// </summary>
            public const string ExclusionsPage = "ExclusionsPage";

            /// <summary>
            /// The achievements page
            /// </summary>
            public const string AchievementsPage = "AchievementsPage";
        }


        private static class PageParameterKey
        {
            public const string StreakTypeId = "StreakTypeId";
            public const string StreakId = "StreakId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string AchievementsPage = "AchievementsPage";
            public const string MapEditorPage = "MapEditorPage";
            public const string ExclusionsPage = "ExclusionsPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<StreakTypeBag, StreakTypeDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                if ( box?.Entity != null )
                {
                    box.NavigationUrls = GetBoxNavigationUrls( box.Entity.IdKey );
                    box.Options = GetBoxOptions( box.IsEditable, rockContext );
                    box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<StreakType>();
                }

                return box;
            }
        }

        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            using ( var rockContext = new RockContext() )
            {
                var streakId = pageReference.GetPageParameter( PageParameterKey.StreakId );
                var streakTypeId = new StreakService( new RockContext() ).Get( streakId )?.StreakTypeId.ToString();
                if ( streakTypeId == null )
                {
                    streakTypeId = pageReference.GetPageParameter( PageParameterKey.StreakTypeId );
                }
                var streakType = StreakTypeCache.Get( streakTypeId, true );

                var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageReference.Parameters );
                var breadCrumb = new BreadCrumbLink( streakType?.Name ?? "New Streak Type", breadCrumbPageRef );

                return new BreadCrumbResult
                {
                    BreadCrumbs = new List<IBreadCrumb>
                   {
                       breadCrumb
                   }
                };
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private StreakTypeDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var checkInPurposeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );
            var options = new StreakTypeDetailOptionsBag
            {
                StreakOccurrenceFrequencies = typeof( StreakOccurrenceFrequency ).ToEnumListItemBag(),
                StreakStructureTypes = typeof( StreakStructureType ).ToEnumListItemBag(),
                AttendanceCheckInConfigGroupTypesGuids = GroupTypeCache.All()
                    .Where( gt => gt.GroupTypePurposeValueId == checkInPurposeId )
                    .OrderBy( gt => gt.Name )
                    .Select( gt => gt.Guid )
                    .ToList()
            };
            return options;
        }

        /// <summary>
        /// Validates the StreakType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="streakType">The StreakType to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the StreakType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateStreakType( StreakType streakType, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            return streakType.IsValid;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<StreakTypeBag, StreakTypeDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {StreakType.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( StreakType.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity, rockContext );
                    // this will set the sync linked activity checkbox to true by default in the frontend
                    box.Entity.EnableAttendance = true;
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( StreakType.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="StreakTypeBag"/> that represents the entity.</returns>
        private StreakTypeBag GetCommonEntityBag( StreakType entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new StreakTypeBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                EnableAttendance = entity.EnableAttendance,
                IsActive = entity.IsActive,
                Name = entity.Name,
                OccurrenceFrequency = entity.OccurrenceFrequency,
                RequiresEnrollment = entity.RequiresEnrollment,
                StartDate = entity.StartDate,
                FirstDayOfWeek = ( int ) ( entity.FirstDayOfWeek ?? 0 ),
                IncludeChildAccounts = entity.StructureSettings.IncludeChildAccounts,
                StructureType = entity.StructureType
            };
            bag.StructureEntity = GetStructureEntityIdListItemBag( entity, rockContext, bag );
            return bag;
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="StreakTypeBag"/> that represents the entity.</returns>
        private StreakTypeBag GetEntityBagForView( StreakType entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity, rockContext );

            var structureTypeString = "";
            if ( entity.StructureType.HasValue )
            {
                var structureName = ( new StreakTypeService( rockContext ) ).GetStructureName( entity.StructureType, entity.StructureEntityId );
                structureTypeString = string.Format( "{0}{1}",
                        entity.StructureType.Value.GetDescription() ?? "",
                        string.Format( "{0}{1}",
                            structureName.IsNullOrWhiteSpace() ? string.Empty : " - ",
                            structureName
                        ) );
            }
            bag.StructureTypeDisplay = structureTypeString;

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="StreakTypeBag"/> that represents the entity.</returns>
        private StreakTypeBag GetEntityBagForEdit( StreakType entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity, rockContext );
            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        private static ListItemBag GetStructureEntityIdListItemBag( StreakType entity, RockContext rockContext, StreakTypeBag bag )
        {
            switch ( entity.StructureType )
            {
                case StreakStructureType.GroupType:
                    return GroupTypeCache.Get( entity.StructureEntityId ?? 0 ).ToListItemBag();
                case StreakStructureType.Group:
                    return new GroupService( rockContext ).Get( entity.StructureEntityId ?? 0 ).ToListItemBag();
                case StreakStructureType.GroupTypePurpose:
                    return DefinedValueCache.Get( entity.StructureEntityId ?? 0 ).ToListItemBag();
                case StreakStructureType.CheckInConfig:
                    return GroupTypeCache.Get( entity.StructureEntityId ?? 0 ).ToListItemBag();
                case StreakStructureType.InteractionChannel:
                    return InteractionChannelCache.Get( entity.StructureEntityId ?? 0 ).ToListItemBag();
                case StreakStructureType.InteractionComponent:
                    InteractionComponentCache interactionComponent = InteractionComponentCache.Get( entity.StructureEntityId ?? 0 );
                    if ( interactionComponent != null )
                    {
                        bag.InteractionComponentInteractionChannel = InteractionChannelCache.Get( interactionComponent.InteractionChannelId )
                            .ToListItemBag();
                    }
                    else
                    {
                        bag.InteractionComponentInteractionChannel = new ListItemBag();
                    }
                    return interactionComponent.ToListItemBag();
                case StreakStructureType.InteractionMedium:
                    return DefinedValueCache.Get( entity.StructureEntityId ?? 0 ).ToListItemBag();
                case StreakStructureType.FinancialTransaction:
                    return FinancialAccountCache.Get( entity.StructureEntityId ?? 0 ).ToListItemBag();
            }
            return null;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( StreakType entity, DetailBlockBox<StreakTypeBag, StreakTypeDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.EnableAttendance ),
                () => entity.EnableAttendance = box.Entity.EnableAttendance );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.RequiresEnrollment ),
                () => entity.RequiresEnrollment = box.Entity.RequiresEnrollment );

            if ( entity.Id == 0 )
            {
                // Update of Start Date and Occurrence Frequency should be permitted only when creating a new streak type.

                box.IfValidProperty( nameof( box.Entity.OccurrenceFrequency ),
                    () => entity.OccurrenceFrequency = box.Entity.OccurrenceFrequency );

                if ( entity.OccurrenceFrequency == StreakOccurrenceFrequency.Daily )
                {
                    box.IfValidProperty( nameof( box.Entity.StartDate ),
                        () => entity.StartDate = box.Entity?.StartDate ?? RockDateTime.Today );
                }
                else
                {
                    box.IfValidProperty( nameof( box.Entity.StartDate ),
                        () => entity.StartDate = ( box.Entity?.StartDate ?? RockDateTime.Today ).SundayDate() );
                }
            }

            if ( entity.OccurrenceFrequency == StreakOccurrenceFrequency.Weekly )
            {
                box.IfValidProperty( nameof( box.Entity.FirstDayOfWeek ),
                    () => entity.FirstDayOfWeek = ( DayOfWeek? ) box.Entity.FirstDayOfWeek );
            }

            box.IfValidProperty( nameof( box.Entity.StructureType ),
                () => entity.StructureType = box.Entity.StructureType );

            box.IfValidProperty( nameof( box.Entity.StructureEntity ),
                () =>
                {
                    entity.StructureEntityId = GetStructureEntityId( entity, box, rockContext ).ToIntSafe();
                } );

            if ( entity.StructureType == StreakStructureType.FinancialTransaction )
            {
                box.IfValidProperty( nameof( box.Entity.IncludeChildAccounts ),
                () => entity.StructureSettings.IncludeChildAccounts = box.Entity.IncludeChildAccounts );
            }

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        private static int? GetStructureEntityId( StreakType entity, DetailBlockBox<StreakTypeBag, StreakTypeDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.Entity.StructureEntity == null )
            {
                return null;
            }
            switch ( entity.StructureType )
            {
                case StreakStructureType.GroupType:
                    return GroupTypeCache.GetId( box.Entity.StructureEntity.Value.AsGuid() );
                case StreakStructureType.Group:
                    return new GroupService( rockContext ).GetId( box.Entity.StructureEntity.Value.AsGuid() );
                case StreakStructureType.GroupTypePurpose:
                    return DefinedValueCache.GetId( box.Entity.StructureEntity.Value.AsGuid() );
                case StreakStructureType.CheckInConfig:
                    return GroupTypeCache.GetId( box.Entity.StructureEntity.Value.AsGuid() );
                case StreakStructureType.InteractionChannel:
                    return InteractionChannelCache.GetId( box.Entity.StructureEntity.Value.AsGuid() );
                case StreakStructureType.InteractionComponent:
                    if ( box.Entity.StructureEntity == null )
                    {
                        return 0;
                    }
                    return InteractionComponentCache.GetId( box.Entity.StructureEntity.Value.AsGuid() );
                case StreakStructureType.InteractionMedium:
                    return DefinedValueCache.GetId( box.Entity.StructureEntity.Value.AsGuid() );
                case StreakStructureType.FinancialTransaction:
                    return FinancialAccountCache.GetId( box.Entity.StructureEntity.Value.AsGuid() );
            }
            return null;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="StreakType"/> to be viewed or edited on the page.</returns>
        private StreakType GetInitialEntity( RockContext rockContext )
        {
            var streak = GetInitialEntity<Streak, StreakService>( rockContext, PageParameterKey.StreakId );
            var streakType = streak?.StreakType;
            if ( streakType == null && streak != null && streak.Id != 0 )
            {
                streakType = new StreakTypeService( rockContext ).Get( streak.StreakTypeId );
            }

            // If no streak type was found in the streak with the id passed in by the StreakId page parameter,
            // try to get the same from the StreakTypeId page parameter key
            if ( streakType == null )
            {
                return GetInitialEntity<StreakType, StreakTypeService>( rockContext, PageParameterKey.StreakTypeId );
            }
            return streakType;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( string idKey )
        {
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.StreakTypeId] = IdHasher.Instance.GetId( idKey ).ToStringSafe()
            };
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl(),
                [NavigationUrlKey.AchievementsPage] = this.GetLinkedPageUrl( AttributeKey.AchievementsPage, queryParams ),
                [NavigationUrlKey.ExclusionsPage] = this.GetLinkedPageUrl( AttributeKey.ExclusionsPage, queryParams ),
                [NavigationUrlKey.MapEditorPage] = this.GetLinkedPageUrl( AttributeKey.MapEditorPage, queryParams ),
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
        private string GetSecurityGrantToken( StreakType entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out StreakType entity, out BlockActionResult error )
        {
            var entityService = new StreakTypeService( rockContext );
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
                entity = new StreakType();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{StreakType.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${StreakType.FriendlyTypeName}." );
                return false;
            }

            return true;
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

                var box = new DetailBlockBox<StreakTypeBag, StreakTypeDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<StreakTypeBag, StreakTypeDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new StreakTypeService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateStreakType( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.StreakTypeId] = entity.IdKey
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
                var entityService = new StreakTypeService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "You are not authorized to delete this item." );
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
        public BlockActionResult RefreshAttributes( DetailBlockBox<StreakTypeBag, StreakTypeDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<StreakTypeBag, StreakTypeDetailOptionsBag>
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


        /// <summary>
        /// Rebuild the streak type and enrollment data
        /// </summary>
        [BlockAction]
        public BlockActionResult Rebuild( string key )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                return ActionNotFound();
            }
            var streakType = StreakTypeCache.Get( key, false );

            if ( !streakType.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionUnauthorized( "You are not authorized to rebuild this item." );
            }

            new ProcessRebuildStreakType.Message { StreakTypeId = streakType.Id }.Send();
            return ActionOk( "The streak type rebuild has been started." );
        }
    }
}
