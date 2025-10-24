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

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Personalization;
using Rock.Personalization.SegmentFilters;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.PersonalizationSegmentDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular personalization segment.
    /// </summary>

    [DisplayName( "Personalization Segment Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a particular personalization segment." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "B1595326-592E-415B-ADE3-984F2FBA1980" )]
    [Rock.SystemGuid.BlockTypeGuid( "AA6203A7-E1D5-427D-98FB-76F969642906" )]
    public class PersonalizationSegmentDetail : RockEntityDetailBlockType<PersonalizationSegment, PersonalizationSegmentBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string PersonalizationSegmentId = "PersonalizationSegmentId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class PersistenceType
        {
            public const string Schedule = "Schedule";
            public const string Interval = "Interval";
        }
        private static class ScheduleType
        {
            public const string Named = "Named";
            public const string Unique = "Unique";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<PersonalizationSegmentBag, PersonalizationSegmentDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private PersonalizationSegmentDetailOptionsBag GetBoxOptions()
        {
            var options = new PersonalizationSegmentDetailOptionsBag();

            options.Sites = SiteCache.GetAllActiveSites()
                .Select( s => s.ToListItemBag() )
                .ToList();

            return options;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<PersonalizationSegmentBag, PersonalizationSegmentDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {PersonalizationSegment.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( PersonalizationSegment.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( PersonalizationSegment.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="PersonalizationSegmentBag"/> that represents the entity.</returns>
        private PersonalizationSegmentBag GetCommonEntityBag( PersonalizationSegment entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var isNamedSchedule = false;
            var iCalendarContent = "";

            if ( entity.PersistedScheduleId.HasValue )
            {
                var schedule = new ScheduleService( RockContext ).Get( entity.PersistedScheduleId.Value );
                if ( schedule != null )
                {
                    if ( schedule.Name.IsNotNullOrWhiteSpace() )
                    {
                        isNamedSchedule = true;
                    }
                    else
                    {
                        iCalendarContent = schedule.iCalendarContent;
                    }
                }
            }

            return new PersonalizationSegmentBag
            {
                IdKey = entity.IdKey,
                AdditionalFilterConfiguration = MapAdditionalFilterConfigurationToBag( entity.AdditionalFilterConfiguration ),
                Categories = entity.Categories.ToListItemBagList(),
                Description = entity.Description,
                FilterDataView = entity.FilterDataView.ToListItemBag(),
                FilterDataViewId = entity.FilterDataViewId,
                IsActive = entity.IsActive,
                Name = entity.Name,
                PersistedLastRefreshDateTime = entity.PersistedLastRefreshDateTime,
                PersistedLastRunDurationMilliseconds = entity.PersistedLastRunDurationMilliseconds,
                PersistenceType = entity.PersistedScheduleId.HasValue ? PersistenceType.Schedule : ( entity.PersistedScheduleIntervalMinutes.HasValue ? PersistenceType.Interval : null ),
                PersistenceScheduleType = isNamedSchedule ? ScheduleType.Named : ScheduleType.Unique,
                PersistedSchedule = entity.PersistedSchedule.ToListItemBag(),
                UniqueScheduleICalendarContent = iCalendarContent,
                PersistedScheduleIntervalMinutes = entity.PersistedScheduleIntervalMinutes,
                SegmentKey = entity.SegmentKey,
                TimeToUpdateDurationMilliseconds = entity.TimeToUpdateDurationMilliseconds
            };
        }

        /// <inheritdoc/>
        protected override PersonalizationSegmentBag GetEntityBagForView( PersonalizationSegment entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <inheritdoc/>
        protected override PersonalizationSegmentBag GetEntityBagForEdit( PersonalizationSegment entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( PersonalizationSegment entity, ValidPropertiesBox<PersonalizationSegmentBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            var scheduleService = new ScheduleService( RockContext );
            var oldSchedule = entity.PersistedScheduleId.HasValue ? scheduleService.Get( entity.PersistedScheduleId.Value ) : null;

            var oldName = entity.Name;
            var oldSegmentKey = entity.SegmentKey;
            var oldIsActive = entity.IsActive;
            var oldFilterDataViewId = entity.FilterDataViewId;
            var oldAdditionalFilterConfiguration = entity.AdditionalFilterConfiguration;

            box.IfValidProperty( nameof( box.Bag.AdditionalFilterConfiguration ),
                () => entity.AdditionalFilterConfiguration = MapAdditionalFilterConfigurationFromBag( box.Bag.AdditionalFilterConfiguration ) );

            box.IfValidProperty( nameof( box.Bag.Categories ),
                () => UpdateCategories( box.Bag, entity ) );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.FilterDataView ),
                () => entity.FilterDataViewId = box.Bag.FilterDataView.GetEntityId<DataView>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.PersistedLastRefreshDateTime ),
                () => entity.PersistedLastRefreshDateTime = box.Bag.PersistedLastRefreshDateTime );

            box.IfValidProperty( nameof( box.Bag.PersistedLastRunDurationMilliseconds ),
                () => entity.PersistedLastRunDurationMilliseconds = box.Bag.PersistedLastRunDurationMilliseconds );

            box.IfValidProperty( nameof( box.Bag.PersistedSchedule ),
                () =>
                {
                    if ( box.Bag.PersistenceType == PersistenceType.Schedule )
                    {
                        if ( box.Bag.PersistenceScheduleType == ScheduleType.Named )
                        {
                            entity.PersistedScheduleId = box.Bag.PersistedSchedule.GetEntityId<Schedule>( RockContext );
                        }
                        else if ( box.Bag.PersistenceScheduleType == ScheduleType.Unique )
                        {
                            var newSchedule = new Schedule
                            {
                                iCalendarContent = box.Bag.UniqueScheduleICalendarContent
                            };

                            scheduleService.Add( newSchedule );
                            entity.PersistedScheduleId = newSchedule.Id;
                        }
                        else
                        {
                            entity.PersistedScheduleId = null;
                        }
                    }
                    else
                    {
                        entity.PersistedScheduleId = null;
                    }
                } );

            box.IfValidProperty( nameof( box.Bag.PersistedScheduleIntervalMinutes ),
                () =>
                {
                    if ( box.Bag.PersistenceType == PersistenceType.Interval )
                    {
                        entity.PersistedScheduleIntervalMinutes = box.Bag.PersistedScheduleIntervalMinutes;
                    }
                    else
                    {
                        entity.PersistedScheduleIntervalMinutes = null;
                    }

                } );

            box.IfValidProperty( nameof( box.Bag.SegmentKey ),
                () => entity.SegmentKey = box.Bag.SegmentKey );

            box.IfValidProperty( nameof( box.Bag.TimeToUpdateDurationMilliseconds ),
                () => entity.TimeToUpdateDurationMilliseconds = box.Bag.TimeToUpdateDurationMilliseconds );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            // If the schedule was changed/removed, and the old schedule was a unique schedule (no name)... then delete the old schedule.
            // This prevents orphaned unique schedules from accumulating in the database.
            if ( oldSchedule != null && oldSchedule.Name.IsNullOrWhiteSpace() && oldSchedule.Id != ( entity.PersistedScheduleId ?? 0 ) )
            {
                scheduleService.Delete( oldSchedule );
            }

            bool isSegmentDefinitionChanged =
                oldFilterDataViewId != entity.FilterDataViewId ||
                oldIsActive != entity.IsActive ||
                oldName != entity.Name ||
                oldSegmentKey != entity.SegmentKey ||
                oldAdditionalFilterConfiguration != entity.AdditionalFilterConfiguration;

            // Mark segment as dirty to signal the PostSave hook to update the sometimes long running Personalization data on a background task.
            if ( isSegmentDefinitionChanged )
            {
                entity.IsDirty = true;
            }

            return true;
        }

        /// <inheritdoc/>
        protected override PersonalizationSegment GetInitialEntity()
        {
            return GetInitialEntity<PersonalizationSegment, PersonalizationSegmentService>( RockContext, PageParameterKey.PersonalizationSegmentId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out PersonalizationSegment entity, out BlockActionResult error )
        {
            var entityService = new PersonalizationSegmentService( RockContext );
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
                entity = new PersonalizationSegment();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{PersonalizationSegment.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${PersonalizationSegment.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        #endregion

        #region Private Methods
        private static AdditionalFilterConfigurationBag MapAdditionalFilterConfigurationToBag( PersonalizationSegmentAdditionalFilterConfiguration configuration )
        {
            if ( configuration == null )
            {
                return null;
            }

            return new AdditionalFilterConfigurationBag
            {
                SessionFilterExpressionType = ( int ) configuration.SessionFilterExpressionType,
                SessionSegmentFilters = configuration.SessionSegmentFilters?.Select( f => new SessionCountSegmentFilterBag
                {
                    Guid = f.Guid,
                    ComparisonType = ( int ) f.ComparisonType,
                    ComparisonValue = f.ComparisonValue,
                    SiteGuids = f.SiteGuids?.ToList(),
                    SlidingDateRangeDelimitedValues = f.SlidingDateRangeDelimitedValues,
                    Description = f.GetDescription()
                } ).ToList(),

                PageViewFilterExpressionType = ( int ) configuration.PageViewFilterExpressionType,
                PageViewSegmentFilters = configuration.PageViewSegmentFilters?.Select( f => new PageViewSegmentFilterBag
                {
                    Guid = f.Guid,
                    ComparisonType = ( int ) f.ComparisonType,
                    ComparisonValue = f.ComparisonValue,
                    SiteGuids = f.SiteGuids?.ToList(),
                    PageGuids = f.GetSelectedPages()?.Select( p => p.ToListItemBag() ).ToList(),
                    SlidingDateRangeDelimitedValues = f.SlidingDateRangeDelimitedValues,
                    PageUrlComparisonType = ( int ) f.PageUrlComparisonType,
                    PageUrlComparisonValue = f.PageUrlComparisonValue,
                    PageReferrerComparisonType = ( int ) f.PageReferrerComparisonType,
                    PageReferrerComparisonValue = f.PageReferrerComparisonValue,
                    SourceComparisonType = ( int ) f.SourceComparisonType,
                    SourceComparisonValue = f.SourceComparisonValue,
                    MediumComparisonType = ( int ) f.MediumComparisonType,
                    MediumComparisonValue = f.MediumComparisonValue,
                    CampaignComparisonType = ( int ) f.CampaignComparisonType,
                    CampaignComparisonValue = f.CampaignComparisonValue,
                    ContentComparisonType = ( int ) f.ContentComparisonType,
                    ContentComparisonValue = f.ContentComparisonValue,
                    TermComparisonType = ( int ) f.TermComparisonType,
                    TermComparisonValue = f.TermComparisonValue,
                    IncludeChildPages = f.IncludeChildPages,
                    Description = f.GetDescription()
                } ).ToList(),

                InteractionFilterExpressionType = ( int ) configuration.InteractionFilterExpressionType,
                InteractionSegmentFilters = configuration.InteractionSegmentFilters?.Select( f => new InteractionSegmentFilterBag
                {
                    Guid = f.Guid,
                    ComparisonType = ( int ) f.ComparisonType,
                    ComparisonValue = f.ComparisonValue,
                    InteractionChannel = f.InteractionChannelGuid != Guid.Empty ? InteractionChannelCache.Get( f.InteractionChannelGuid )?.ToListItemBag() : null,
                    InteractionComponent = f.InteractionComponentGuid.HasValue ? InteractionComponentCache.Get( f.InteractionComponentGuid.Value )?.ToListItemBag() : null,
                    Operation = f.Operation,
                    SlidingDateRangeDelimitedValues = f.SlidingDateRangeDelimitedValues,
                } ).ToList()
            };
        }

        private static SessionCountSegmentFilter MapSessionCountSegmentFilterFromBag( SessionCountSegmentFilterBag filterBag )
        {
            var filter = new SessionCountSegmentFilter
            {
                Guid = filterBag.Guid == Guid.Empty ? Guid.NewGuid() : filterBag.Guid,
                ComparisonType = ( ComparisonType ) filterBag.ComparisonType,
                ComparisonValue = filterBag.ComparisonValue,
                SiteGuids = filterBag.SiteGuids ?? new List<Guid>(),
                SlidingDateRangeDelimitedValues = filterBag.SlidingDateRangeDelimitedValues
            };
            return filter;
        }
        private static PageViewSegmentFilter MapPageViewSegmentFilterFromBag( PageViewSegmentFilterBag filterBag )
        {
            var filter = new PageViewSegmentFilter
            {
                Guid = filterBag.Guid,
                ComparisonType = ( ComparisonType ) filterBag.ComparisonType,
                ComparisonValue = filterBag.ComparisonValue,
                SiteGuids = filterBag.SiteGuids?.ToList() ?? new List<Guid>(),
                PageGuids = filterBag.PageGuids?.Select( pg => pg.Value.AsGuid() ).Where( g => g != Guid.Empty ).ToList() ?? new List<Guid>(),
                SlidingDateRangeDelimitedValues = filterBag.SlidingDateRangeDelimitedValues,
                PageUrlComparisonType = ( ComparisonType ) filterBag.PageUrlComparisonType,
                PageUrlComparisonValue = filterBag.PageUrlComparisonValue,
                PageReferrerComparisonType = ( ComparisonType ) filterBag.PageReferrerComparisonType,
                PageReferrerComparisonValue = filterBag.PageReferrerComparisonValue,
                SourceComparisonType = ( ComparisonType ) filterBag.SourceComparisonType,
                SourceComparisonValue = filterBag.SourceComparisonValue,
                MediumComparisonType = ( ComparisonType ) filterBag.MediumComparisonType,
                MediumComparisonValue = filterBag.MediumComparisonValue,
                CampaignComparisonType = ( ComparisonType ) filterBag.CampaignComparisonType,
                CampaignComparisonValue = filterBag.CampaignComparisonValue,
                ContentComparisonType = ( ComparisonType ) filterBag.ContentComparisonType,
                ContentComparisonValue = filterBag.ContentComparisonValue,
                TermComparisonType = ( ComparisonType ) filterBag.TermComparisonType,
                TermComparisonValue = filterBag.TermComparisonValue,
                IncludeChildPages = filterBag.IncludeChildPages
            };
            return filter;
        }
        private static InteractionSegmentFilter MapInteractionSegmentFilterFromBag( InteractionSegmentFilterBag filterBag )
        {
            var filter = new InteractionSegmentFilter
            {
                Guid = filterBag.Guid,
                ComparisonType = ( ComparisonType ) filterBag.ComparisonType,
                ComparisonValue = filterBag.ComparisonValue,
                InteractionChannelGuid = filterBag.InteractionChannel.Value.AsGuid(),
                InteractionComponentGuid = filterBag.InteractionComponent?.Value.AsGuidOrNull(),
                Operation = filterBag.Operation,
                SlidingDateRangeDelimitedValues = filterBag.SlidingDateRangeDelimitedValues
            };
            return filter;
        }
        private static PersonalizationSegmentAdditionalFilterConfiguration MapAdditionalFilterConfigurationFromBag( AdditionalFilterConfigurationBag bag )
        {
            if ( bag == null )
            {
                return null;
            }

            var configuration = new PersonalizationSegmentAdditionalFilterConfiguration
            {
                SessionFilterExpressionType = ( FilterExpressionType ) bag.SessionFilterExpressionType,
                PageViewFilterExpressionType = ( FilterExpressionType ) bag.PageViewFilterExpressionType,
                InteractionFilterExpressionType = ( FilterExpressionType ) bag.InteractionFilterExpressionType
            };

            if ( bag.SessionSegmentFilters != null )
            {
                configuration.SessionSegmentFilters = bag.SessionSegmentFilters.Select( filterBag => MapSessionCountSegmentFilterFromBag( filterBag ) ).ToList();
            }
            if ( bag.PageViewSegmentFilters != null )
            {
                configuration.PageViewSegmentFilters = bag.PageViewSegmentFilters.Select( filterBag => MapPageViewSegmentFilterFromBag( filterBag ) ).ToList();
            }
            if ( bag.InteractionSegmentFilters != null )
            {
                configuration.InteractionSegmentFilters = bag.InteractionSegmentFilters
                    .Where( f => f.InteractionChannel != null && f.InteractionChannel.Value.AsGuidOrNull().HasValue )
                    .Select( filterBag => MapInteractionSegmentFilterFromBag( filterBag ) )
                    .ToList();
            }

            return configuration;
        }

        /// <summary>
        /// Updates the categories for the personalization segment.
        /// </summary>
        /// <param name="bag">The bag containing the category information.</param>
        /// <param name="entity">The personalization segment entity.</param>
        private void UpdateCategories( PersonalizationSegmentBag bag, PersonalizationSegment entity )
        {
            bool hasChanges = false;
            var categoryService = new CategoryService( RockContext );

            var incomingCategoryGuids = bag.Categories?.Select( c => c.Value.AsGuid() ).ToList() ?? new List<Guid>();

            var categoriesToRemove = entity.Categories
                .Where( c => !incomingCategoryGuids.Contains( c.Guid ) )
                .ToList();

            foreach ( var category in categoriesToRemove )
            {
                entity.Categories.Remove( category );
                hasChanges = true;
            }

            foreach ( var categoryGuid in incomingCategoryGuids )
            {
                if ( !entity.Categories.Any( c => c.Guid == categoryGuid ) )
                {
                    var category = categoryService.Get( categoryGuid );
                    if ( category != null )
                    {
                        entity.Categories.Add( category );
                        hasChanges = true;
                    }
                }
            }

            if ( hasChanges )
            {
                entity.ModifiedDateTime = RockDateTime.Now;
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Returns the server-generated description for a session segment filter.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetSessionFilterDescription( SessionCountSegmentFilterBag filterBag )
        {
            if ( filterBag == null )
            {
                return ActionBadRequest( "Invalid session filter." );
            }
            var filter = MapSessionCountSegmentFilterFromBag( filterBag );
            var description = filter.GetDescription();
            return ActionOk( new FilterDescriptionResultBag { Description = description } );
        }

        /// <summary>
        /// Returns the server-generated description for a page view segment filter.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetPageViewFilterDescription( PageViewSegmentFilterBag filterBag )
        {
            if ( filterBag == null )
            {
                return ActionBadRequest( "Invalid page view filter." );
            }
            var filter = MapPageViewSegmentFilterFromBag( filterBag );
            var description = filter.GetDescription();
            return ActionOk( new FilterDescriptionResultBag { Description = description } );
        }

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

            return ActionOk( new ValidPropertiesBox<PersonalizationSegmentBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<PersonalizationSegmentBag> box )
        {
            var entityService = new PersonalizationSegmentService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            var isNew = entity.Id == 0;

            var isDuplicate = new PersonalizationSegmentService( RockContext ).Queryable().Any( a => a.Id != entity.Id && a.SegmentKey == entity.SegmentKey );
            if ( isDuplicate )
            {
                return ActionBadRequest( $"A request filter with the key '{entity.SegmentKey}' already exists." );
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
                    [PageParameterKey.PersonalizationSegmentId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<PersonalizationSegmentBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
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
            var entityService = new PersonalizationSegmentService( RockContext );

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

            return ActionOk( this.GetParentPageUrl() );
        }

        #endregion
    }
}
