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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.PersistedDatasetDetail;
using Rock.ViewModels.Blocks.Core.ScheduleDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

using static Rock.Blocks.Cms.PersistedDatasetDetail;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular persisted dataset.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Persisted Dataset Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a particular persisted dataset." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "b189040b-2914-437f-900d-a54705b22d2e" )]
    [Rock.SystemGuid.BlockTypeGuid( "6035ac10-07a5-4edd-a1e9-10862fc41494" )]
    public class PersistedDatasetDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string PersistedDatasetId = "PersistedDatasetId";
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
                var box = new DetailBlockBox<PersistedDatasetBag, PersistedDatasetDetailOptionsBag>();

                SetBoxInitialEntityState( box, false, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );

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
        private PersistedDatasetDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new PersistedDatasetDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the PersistedDataset for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="persistedDataset">The PersistedDataset to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the PersistedDataset is valid, <c>false</c> otherwise.</returns>
        private bool ValidatePersistedDataset( PersistedDataset persistedDataset, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            var persistedDatasetService = new PersistedDatasetService( rockContext );
            var accessKeyAlreadyExistsForDataSet = persistedDatasetService.Queryable().Where( a => a.AccessKey == persistedDataset.AccessKey && a.Id != persistedDataset.Id ).AsNoTracking().FirstOrDefault();

            if ( accessKeyAlreadyExistsForDataSet != null )
            {
                errorMessage = string.Format( "Access Key must be unique. {0} is using '{1}' as the access key", accessKeyAlreadyExistsForDataSet.Name, persistedDataset.AccessKey );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<PersistedDatasetBag, PersistedDatasetDetailOptionsBag> box, bool loadAttributes, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity != null )
            {
                var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
                box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

                if ( entity.Id != 0 )
                {
                    // Existing entity was found, prepare for view mode by default.
                    if ( isViewable )
                    {
                        box.Entity = GetEntityBagForView( entity, loadAttributes );
                    }
                    else
                    {
                        box.ErrorMessage = EditModeMessage.NotAuthorizedToView( PersistedDataset.FriendlyTypeName );
                    }
                }
                else
                {
                    // New entity is being created, prepare for edit mode by default.
                    if ( box.IsEditable )
                    {
                        box.Entity = GetEntityBagForEdit( entity, loadAttributes );
                    }
                    else
                    {
                        box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( PersistedDataset.FriendlyTypeName );
                    }
                }
            }
            else
            {
                box.ErrorMessage = $"The {PersistedDataset.FriendlyTypeName} was not found.";
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="PersistedDatasetBag"/> that represents the entity.</returns>
        private PersistedDatasetBag GetCommonEntityBag( PersistedDataset entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var rockContext = new RockContext();

            Schedule schedule = null;
            if (entity.PersistedScheduleId.HasValue)
            {
                schedule = new ScheduleService(rockContext).Get(entity.PersistedScheduleId.Value);
            }

            var entityBag = new PersistedDatasetBag
            {
                IdKey = entity.IdKey,
                AccessKey = entity.AccessKey,
                AllowManualRefresh = entity.AllowManualRefresh,
                BuildScript = entity.BuildScript,
                Description = entity.Description,
                EnabledLavaCommands = entity.EnabledLavaCommands.SplitDelimitedValues(",").Select(c => new ListItemBag() { Value = c, Text = c } ).ToList(),
                EntityType = entity.EntityType.ToListItemBag(),
                ExpireDateTime = entity.ExpireDateTime,
                IsSystem = entity.IsSystem,
                IsActive = entity.IsActive,
                LastRefreshDateTime = entity.LastRefreshDateTime,
                TimeToBuildMS = entity.TimeToBuildMS,
                MemoryCacheDurationHours = entity.MemoryCacheDurationMS,
                Name = entity.Name,
                PersistedScheduleId = entity.PersistedScheduleId,
                RefreshInterval = entity.RefreshIntervalMinutes,
                PersistenceType = entity.PersistedScheduleId.HasValue ? "Schedule" : ( entity.RefreshIntervalMinutes.HasValue ? "Interval" : null ),
                PersistedScheduleType = entity.PersistedScheduleId.HasValue && (schedule != null && !schedule.Name.IsNullOrWhiteSpace()) ? "NamedSchedule" : ( entity.PersistedScheduleId.HasValue ? "Unique" : null ),
                PersistedSchedule = schedule?.iCalendarContent,
                FriendlyScheduleText = schedule?.FriendlyScheduleText
            };

            if ( entity.RefreshIntervalMinutes.HasValue )
            {
                entityBag.RefreshIntervalHours = Convert.ToInt32( TimeSpan.FromMinutes( entity.RefreshIntervalMinutes.Value ).TotalHours );
            }

            if ( entity.MemoryCacheDurationMS.HasValue )
            {
                entityBag.MemoryCacheDurationHours = Convert.ToInt32( TimeSpan.FromMilliseconds( entity.MemoryCacheDurationMS.Value ).TotalHours );
            }

            // Set PersistedScheduleIntervalType by RefreshIntervalMinutes
            if ( entity.RefreshIntervalMinutes.HasValue )
            {
                int intervalMinutes = entity.RefreshIntervalMinutes.Value;

                if ( intervalMinutes % 1440 == 0 ) // Check if it's in days
                {
                    entityBag.PersistedScheduleIntervalType = "Days";
                    entityBag.RefreshInterval = intervalMinutes / 1440;
                }
                else if ( intervalMinutes % 60 == 0 ) // Check if it's in hours
                {
                    entityBag.PersistedScheduleIntervalType = "Hours";
                    entityBag.RefreshInterval = intervalMinutes / 60;
                }
                else // It's in minutes
                {
                    entityBag.PersistedScheduleIntervalType = "Mins";
                    entityBag.RefreshInterval = intervalMinutes;
                }
            }
            else
            {
                // If no interval minutes are set, ensure the type is also null
                entityBag.PersistedScheduleIntervalType = null;
            }

            var namedSchedules = LoadNamedSchedules();
            entityBag.NamedSchedules = namedSchedules;

            return entityBag;
        }

        /// <summary>
        /// Gets the bag for viewing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <returns>A <see cref="PersistedDatasetBag"/> that represents the entity.</returns>
        private PersistedDatasetBag GetEntityBagForView( PersistedDataset entity, bool loadAttributes )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <returns>A <see cref="PersistedDatasetBag"/> that represents the entity.</returns>
        private PersistedDatasetBag GetEntityBagForEdit( PersistedDataset entity, bool loadAttributes )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( PersistedDataset entity, DetailBlockBox<PersistedDatasetBag, PersistedDatasetDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            // Update properties
            box.IfValidProperty( nameof( box.Entity.AccessKey ),
                () => entity.AccessKey = box.Entity.AccessKey );

            box.IfValidProperty( nameof( box.Entity.AllowManualRefresh ),
                () => entity.AllowManualRefresh = box.Entity.AllowManualRefresh );

            box.IfValidProperty( nameof( box.Entity.BuildScript ),
                () => entity.BuildScript = box.Entity.BuildScript );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.EnabledLavaCommands ),
                () => entity.EnabledLavaCommands = box.Entity.EnabledLavaCommands.ConvertAll( m => m.Value ).AsDelimited( "," ) );

            box.IfValidProperty( nameof( box.Entity.EntityType ),
                () => entity.EntityTypeId = box.Entity.EntityType.GetEntityId<EntityType>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.ExpireDateTime ),
                () => entity.ExpireDateTime = box.Entity.ExpireDateTime );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.RefreshInterval ),
                () => entity.RefreshIntervalMinutes = box.Entity.RefreshInterval );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.PersistedScheduleId ),
                () => entity.PersistedScheduleId = box.Entity.PersistedScheduleId );

            if ( box.Entity.MemoryCacheDurationHours.HasValue )
            {
                box.IfValidProperty( nameof( box.Entity.MemoryCacheDurationHours ),
                    () => entity.MemoryCacheDurationMS = ( int ) TimeSpan.FromHours( box.Entity.MemoryCacheDurationHours.Value ).TotalMilliseconds );
            }

            if ( box.Entity.RefreshIntervalHours.HasValue )
            {
                box.IfValidProperty( nameof( box.Entity.RefreshIntervalHours ),
                    () => entity.RefreshIntervalMinutes = ( int ) TimeSpan.FromHours( box.Entity.RefreshIntervalHours.Value ).TotalMinutes );
            }

            // Update RefreshIntervalMinutes based on Interval Type
            if ( box.Entity.PersistenceType == "Interval" && box.Entity.RefreshInterval.HasValue )
            {
                int intervalMinutes = box.Entity.RefreshInterval.Value;
                string intervalType = box.Entity.PersistedScheduleIntervalType ?? "Mins";

                switch ( intervalType )
                {
                    case "Days":
                        intervalMinutes *= 1440; // Convert days to minutes
                        break;
                    case "Hours":
                        intervalMinutes *= 60; // Convert hours to minutes
                        break;
                }

                // Update the entity's RefreshIntervalMinutes
                entity.RefreshIntervalMinutes = intervalMinutes;
            }
            else
            {
                // If the persistence type is not 'Interval', reset the interval minutes
                entity.RefreshIntervalMinutes = null;
            }

            return true;
        }

        /// <summary>
        /// Loads the named schedules.
        /// </summary>
        /// <returns>A list of named schedules.</returns>
        private List<ListItemBag> LoadNamedSchedules()
        {
            return NamedScheduleCache.All()
                .Where(a => a.IsActive && !string.IsNullOrWhiteSpace(a.Name))
                .Select(a => new ListItemBag { Value = a.Id.ToString(), Text = a.Name })
                .ToList();
        }
        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="PersistedDataset"/> to be viewed or edited on the page.</returns>
        private PersistedDataset GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<PersistedDataset, PersistedDatasetService>( rockContext, PageParameterKey.PersistedDatasetId );
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

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <param name="entity">The entity being viewed or edited on this block.</param>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( IHasAttributes entity )
        {
            return new Rock.Security.SecurityGrant()
                .AddRulesForAttributes( entity, RequestContext.CurrentPerson )
                .ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out PersistedDataset entity, out BlockActionResult error )
        {
            var entityService = new PersistedDatasetService( rockContext );
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
                entity = new PersistedDataset();
                entity.PersistedScheduleId = null;
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{PersistedDataset.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${PersistedDataset.FriendlyTypeName}." );
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

                var entityBag = GetEntityBagForEdit( entity, true );

                var box = new DetailBlockBox<PersistedDatasetBag, PersistedDatasetDetailOptionsBag>
                {
                    Entity = entityBag
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
        public BlockActionResult Save( DetailBlockBox<PersistedDatasetBag, PersistedDatasetDetailOptionsBag> box, ScheduleBag scheduleBag )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new PersistedDatasetService( rockContext );

                // Determine if we are editing an existing dataset or creating a new one.
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the box.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Manage Schedule if applicable
                if ( scheduleBag != null && !string.IsNullOrEmpty( scheduleBag.iCalendarContent ) )
                {
                    var scheduleService = new ScheduleService( rockContext );
                    Schedule schedule = null;

                    if ( entity.PersistedScheduleId.HasValue )
                    {
                        schedule = scheduleService.Get( entity.PersistedScheduleId.Value );
                    }

                    if ( schedule == null )
                    {
                        schedule = new Schedule
                        {
                            iCalendarContent = scheduleBag.iCalendarContent
                        };
                        scheduleService.Add( schedule );
                        entity.PersistedScheduleId = schedule.Id;
                    }
                    else
                    {
                        schedule.iCalendarContent = scheduleBag.iCalendarContent;
                    }
                }

                // Validate the dataset
                if ( !ValidatePersistedDataset( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                entity.LastRefreshDateTime = null;
                entity.TimeToBuildMS = null;

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.PersistedDatasetId] = entity.IdKey
                    } ) );
                }

                return ActionOk( GetEntityBagForView( entity, true ) );
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
                var entityService = new PersistedDatasetService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<PersistedDatasetBag, PersistedDatasetDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<PersistedDatasetBag, PersistedDatasetDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, true )
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
