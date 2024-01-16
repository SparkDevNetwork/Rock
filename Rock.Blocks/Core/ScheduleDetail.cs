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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.ScheduleDetail;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular schedule.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Schedule Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a particular schedule." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "ce4859a1-3e47-442f-8442-2671a89a5656" )]
    [Rock.SystemGuid.BlockTypeGuid( "7c10240a-7ee5-4720-aac9-5c162e9f5aac" )]
    public class ScheduleDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string ScheduleId = "ScheduleId";
            public const string ParentCategoryId = "ParentCategoryId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string CancelLink = "CancelLink";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            if ( PageParameter( PageParameterKey.ScheduleId ).IsNullOrWhiteSpace() )
            {
                return new DetailBlockBox<ScheduleBag, ScheduleDetailOptionsBag>();
            }
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<ScheduleBag, ScheduleDetailOptionsBag>();
                var entity = GetInitialEntity( rockContext );

                if ( entity == null )
                {
                    return null;
                }

                SetBoxInitialEntityState( box, rockContext, entity );
                var categoryId = PageParameter( PageParameterKey.ParentCategoryId ).AsIntegerOrNull();
                if ( categoryId.HasValue )
                {
                    box.Entity.Category = CategoryCache.Get( categoryId.Value )
                        .ToListItemBag();
                }
                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext, entity );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<Schedule>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private ScheduleDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext, Schedule entity )
        {
            var options = new ScheduleDetailOptionsBag();

            options.HasScheduleWarning = entity.HasScheduleWarning();

            string errorMessage = string.Empty;
            options.CanDelete = new ScheduleService( rockContext ).CanDelete( entity, out errorMessage );

            options.HasAttendance = entity.Id > 0 && new AttendanceService( rockContext )
                .Queryable()
                .Where( a => a.Occurrence != null && a.Occurrence.ScheduleId.HasValue && a.Occurrence.ScheduleId == entity.Id )
                .Any();

            options.HelpText = ScheduleService.CreatePreviewHTML( entity );

            if ( entity.CategoryId.HasValue )
            {
                var today = RockDateTime.Today;
                var nextYear = today.AddYears( 1 );
                options.Exclusions = new ScheduleCategoryExclusionService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( e =>
                            e.CategoryId == entity.CategoryId.Value &&
                            e.EndDate >= today &&
                            e.StartDate < nextYear )
                        .OrderBy( e => e.StartDate )
                        .ToList()
                        .Select( e => new ScheduleExclusionBag
                        {
                            Title = e.Title,
                            StartDate = e.StartDate.ToRockDateTimeOffset(),
                            EndDate = e.EndDate.ToRockDateTimeOffset()
                        } )
                        .ToList();
            }
            return options;
        }

        /// <summary>
        /// Validates the Schedule for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="schedule">The Schedule to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Schedule is valid, <c>false</c> otherwise.</returns>
        private bool ValidateSchedule( Schedule schedule, RockContext rockContext, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( schedule.CategoryId == 0 )
            {
                errorMessage = "Category is invalid";
                return false;
            }

            if ( !schedule.IsValid )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<ScheduleBag, ScheduleDetailOptionsBag> box, RockContext rockContext, Schedule entity )
        {
            if ( entity == null )
            {
                box.ErrorMessage = $"The {Schedule.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Schedule.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Schedule.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="ScheduleBag"/> that represents the entity.</returns>
        private ScheduleBag GetCommonEntityBag( Schedule entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new ScheduleBag
            {
                IdKey = entity.IdKey,
                AbbreviatedName = entity.AbbreviatedName,
                AutoInactivateWhenComplete = entity.AutoInactivateWhenComplete,
                Category = entity.Category.ToListItemBag(),
                CheckInEndOffsetMinutes = entity.CheckInEndOffsetMinutes,
                CheckInStartOffsetMinutes = entity.CheckInStartOffsetMinutes,
                Description = entity.Description,
                EffectiveEndDate = entity.EffectiveEndDate,
                EffectiveStartDate = entity.EffectiveStartDate,
                FriendlyScheduleText = entity.FriendlyScheduleText,
                iCalendarContent = entity.iCalendarContent,
                IsActive = entity.IsActive,
                IsPublic = entity.IsPublic,
                Name = entity.Name,
                NextOccurrence = entity.GetNextStartDateTime( RockDateTime.Now )?.ToString( "g" ) ?? string.Empty
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="ScheduleBag"/> that represents the entity.</returns>
        private ScheduleBag GetEntityBagForView( Schedule entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="ScheduleBag"/> that represents the entity.</returns>
        private ScheduleBag GetEntityBagForEdit( Schedule entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( Schedule entity, DetailBlockBox<ScheduleBag, ScheduleDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.AbbreviatedName ),
                () => entity.AbbreviatedName = box.Entity.AbbreviatedName );

            box.IfValidProperty( nameof( box.Entity.AutoInactivateWhenComplete ),
                () => entity.AutoInactivateWhenComplete = box.Entity.AutoInactivateWhenComplete );

            box.IfValidProperty( nameof( box.Entity.Category ),
                () => entity.CategoryId = box.Entity.Category.GetEntityId<Category>( rockContext ).ToIntSafe() );

            box.IfValidProperty( nameof( box.Entity.CheckInEndOffsetMinutes ),
                () => entity.CheckInEndOffsetMinutes = box.Entity.CheckInEndOffsetMinutes );

            box.IfValidProperty( nameof( box.Entity.CheckInStartOffsetMinutes ),
                () => entity.CheckInStartOffsetMinutes = box.Entity.CheckInStartOffsetMinutes );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.EffectiveEndDate ),
                () => entity.EffectiveEndDate = box.Entity.EffectiveEndDate?.DateTime );

            box.IfValidProperty( nameof( box.Entity.EffectiveStartDate ),
                () => entity.EffectiveStartDate = box.Entity.EffectiveStartDate?.DateTime );

            box.IfValidProperty( nameof( box.Entity.iCalendarContent ),
                () => entity.iCalendarContent = box.Entity.iCalendarContent );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.IsPublic ),
                () => entity.IsPublic = box.Entity.IsPublic );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

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
        /// <returns>The <see cref="Schedule"/> to be viewed or edited on the page.</returns>
        private Schedule GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<Schedule, ScheduleService>( rockContext, PageParameterKey.ScheduleId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl(),
                [NavigationUrlKey.CancelLink] = GetCancelLink()
            };
        }

        private string GetCancelLink()
        {
            var parentCategoryId = PageParameter( PageParameterKey.ParentCategoryId ).AsIntegerOrNull();
            if ( parentCategoryId.HasValue )
            {
                // Cancelling on Add, and we know the parentCategoryId, so we are probably in treeview mode, so navigate to the current page
                var qryParams = new Dictionary<string, string>();
                qryParams["CategoryId"] = parentCategoryId.ToString();
                return this.GetCurrentPageUrl( qryParams );
            }
            else
            {
                return this.GetParentPageUrl();
            }
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
        private string GetSecurityGrantToken( Schedule entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out Schedule entity, out BlockActionResult error )
        {
            var entityService = new ScheduleService( rockContext );
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
                entity = new Schedule();
                entityService.Add( entity );

                var maxOrder = entityService.Queryable()
                    .Select( t => ( int? ) t.Order )
                    .Max();

                entity.Order = maxOrder.HasValue ? maxOrder.Value + 1 : 0;
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Schedule.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Schedule.FriendlyTypeName}." );
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

                var box = new DetailBlockBox<ScheduleBag, ScheduleDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
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
        public BlockActionResult Save( DetailBlockBox<ScheduleBag, ScheduleDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new ScheduleService( rockContext );

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
                if ( !ValidateSchedule( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                Rock.CheckIn.KioskDevice.Clear();

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.ScheduleId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );
                entity.LoadAttributes( rockContext );

                return ActionOk( GetEntityBagForView( entity ) );
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
                var entityService = new ScheduleService( rockContext );

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

                // reload page, selecting the deleted data view's parent
                var qryParams = new Dictionary<string, string>();
                if ( entity.CategoryId != null )
                {
                    qryParams["CategoryId"] = entity.CategoryId.ToString();
                }

                qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );

                return ActionOk( ( new Rock.Web.PageReference( this.PageCache.Guid.ToString(), qryParams ) ).BuildUrl() );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<ScheduleBag, ScheduleDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<ScheduleBag, ScheduleDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
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
