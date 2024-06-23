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
using Rock.Event.InteractiveExperiences;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.InteractiveExperiences.InteractiveExperienceDetail;
using Rock.ViewModels.Event.InteractiveExperiences;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Event.InteractiveExperiences
{
    /// <summary>
    /// Displays the details of a particular interactive experience.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Interactive Experience Detail" )]
    [Category( "Event > Interactive Experiences" )]
    [Description( "Displays the details of a particular interactive experience." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "e9e76f40-3e00-40e1-bd9d-3156e9208557" )]
    [Rock.SystemGuid.BlockTypeGuid( "dc997692-3bb4-470c-a2ee-83cb87d246b1" )]
    public class InteractiveExperienceDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string InteractiveExperienceId = "InteractiveExperienceId";
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
                var box = new DetailBlockBox<InteractiveExperienceBag, InteractiveExperienceDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<InteractiveExperience>();

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
        private InteractiveExperienceDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new InteractiveExperienceDetailOptionsBag
            {
                ActionTypes = GetAvailableActionTypes(),
                VisualizerTypes = GetAvailableVisualizerTypes()
            };

            return options;
        }

        /// <summary>
        /// Validates the InteractiveExperience for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="interactiveExperience">The InteractiveExperience to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the InteractiveExperience is valid, <c>false</c> otherwise.</returns>
        private bool ValidateInteractiveExperience( InteractiveExperience interactiveExperience, RockContext rockContext, out string errorMessage )
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
        private void SetBoxInitialEntityState( DetailBlockBox<InteractiveExperienceBag, InteractiveExperienceDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {InteractiveExperience.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( InteractiveExperience.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( InteractiveExperience.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="InteractiveExperienceBag"/> that represents the entity.</returns>
        private InteractiveExperienceBag GetCommonEntityBag( InteractiveExperience entity )
        {
            if ( entity == null )
            {
                return null;
            }

            if ( entity.InteractiveExperienceActions != null && entity.InteractiveExperienceActions.Count > 0 )
            {
                entity.InteractiveExperienceActions.LoadAttributes();
            }

            var settings = entity.ExperienceSettingsJson.FromJsonOrNull<ExperienceSettingsBag>() ?? new ExperienceSettingsBag();
            var defaultCampus = settings.DefaultCampusId.HasValue
                ? CampusCache.Get( settings.DefaultCampusId.Value )
                : null;

            return new InteractiveExperienceBag
            {
                IdKey = entity.IdKey,
                ActionBackgroundColor = entity.ActionBackgroundColor,
                ActionBackgroundImageBinaryFile = entity.ActionBackgroundImageBinaryFile.ToListItemBag(),
                ActionCustomCss = entity.ActionCustomCss,
                ActionPrimaryButtonColor = entity.ActionPrimaryButtonColor,
                ActionPrimaryButtonTextColor = entity.ActionPrimaryButtonTextColor,
                ActionSecondaryButtonColor = entity.ActionSecondaryButtonColor,
                ActionSecondaryButtonTextColor = entity.ActionSecondaryButtonTextColor,
                ActionTextColor = entity.ActionTextColor,
                Actions = entity.InteractiveExperienceActions
                    ?.OrderBy( a => a.Order )
                    .ThenBy( a => a.Id )
                    .Select( a => GetActionBag( a ) )
                    .ToList(),
                AudienceAccentColor = entity.AudienceAccentColor,
                AudienceBackgroundColor = entity.AudienceBackgroundColor,
                AudienceBackgroundImageBinaryFile = entity.AudienceBackgroundImageBinaryFile.ToListItemBag(),
                AudienceCustomCss = entity.AudienceCustomCss,
                AudiencePrimaryColor = entity.AudiencePrimaryColor,
                AudienceSecondaryColor = entity.AudienceSecondaryColor,
                AudienceTextColor = entity.AudienceTextColor,
                CampusBehavior = settings.CampusBehavior,
                DefaultCampus = defaultCampus != null ?
                    new ListItemBag
                    {
                        Value = defaultCampus.Guid.ToString(),
                        Text = defaultCampus.Name
                    }
                    : null,
                Description = entity.Description,
                ExperienceEndedTemplate = settings.ExperienceEndedTemplate ?? string.Empty,
                IsActive = entity.IsActive,
                Name = entity.Name,
                NoActionHeaderImageBinaryFile = entity.NoActionHeaderImageBinaryFile.ToListItemBag(),
                NoActionMessage = entity.NoActionMessage,
                NoActionTitle = entity.NoActionTitle,
                PhotoBinaryFile = entity.PhotoBinaryFile.ToListItemBag(),
                PublicLabel = entity.PublicLabel,
                PushNotificationDetail = entity.PushNotificationDetail,
                PushNotificationTitle = entity.PushNotificationTitle,
                PushNotificationType = entity.PushNotificationType,
                Schedules = entity.InteractiveExperienceSchedules?.Select( s => GetScheduleBag( s ) ).ToList(),
                WelcomeHeaderImageBinaryFile = entity.WelcomeHeaderImageBinaryFile.ToListItemBag(),
                WelcomeMessage = entity.WelcomeMessage,
                WelcomeTitle = entity.WelcomeTitle
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="InteractiveExperienceBag"/> that represents the entity.</returns>
        private InteractiveExperienceBag GetEntityBagForView( InteractiveExperience entity )
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
        /// Gets the bag for editing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A <see cref="InteractiveExperienceBag"/> that represents the entity.</returns>
        private InteractiveExperienceBag GetEntityBagForEdit( InteractiveExperience entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            InteractiveExperienceSchedule schedule = null;

            entity.InteractiveExperienceSchedules.LoadAttributes( rockContext );

            bag.Schedules = entity.InteractiveExperienceSchedules
                ?.Select( s =>
                {
                    var scheduleBag = GetScheduleBag( s );

                    scheduleBag.AttributeValues = s.GetPublicAttributeValuesForEdit( RequestContext.CurrentPerson );

                    if ( bag.ScheduleAttributes == null )
                    {
                        bag.ScheduleAttributes = s.GetPublicAttributesForEdit( RequestContext.CurrentPerson );
                    }

                    return scheduleBag;
                } ).ToList();

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            // If we didn't have anys chedules to load attributes from, use a fake one.
            if ( bag.ScheduleAttributes == null )
            {
                schedule = new InteractiveExperienceSchedule
                {
                    InteractiveExperienceId = entity.Id
                };

                schedule.LoadAttributes( rockContext );

                bag.ScheduleAttributes = entity.GetPublicAttributesForEdit( RequestContext.CurrentPerson );
            }

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">Contains any custom error message if the update was not successful.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( InteractiveExperience entity, DetailBlockBox<InteractiveExperienceBag, InteractiveExperienceDetailOptionsBag> box, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            if ( box.ValidProperties == null )
            {
                return false;
            }

            var settings = entity.ExperienceSettingsJson.FromJsonOrNull<ExperienceSettingsBag>() ?? new ExperienceSettingsBag();

            box.IfValidProperty( nameof( box.Entity.ActionBackgroundColor ),
                () => entity.ActionBackgroundColor = box.Entity.ActionBackgroundColor );

            box.IfValidProperty( nameof( box.Entity.ActionBackgroundImageBinaryFile ),
                () => entity.ActionBackgroundImageBinaryFileId = box.Entity.ActionBackgroundImageBinaryFile.GetEntityId<BinaryFile>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.ActionCustomCss ),
                () => entity.ActionCustomCss = box.Entity.ActionCustomCss );

            box.IfValidProperty( nameof( box.Entity.ActionPrimaryButtonColor ),
                () => entity.ActionPrimaryButtonColor = box.Entity.ActionPrimaryButtonColor );

            box.IfValidProperty( nameof( box.Entity.ActionPrimaryButtonTextColor ),
                () => entity.ActionPrimaryButtonTextColor = box.Entity.ActionPrimaryButtonTextColor );

            box.IfValidProperty( nameof( box.Entity.ActionSecondaryButtonColor ),
                () => entity.ActionSecondaryButtonColor = box.Entity.ActionSecondaryButtonColor );

            box.IfValidProperty( nameof( box.Entity.ActionSecondaryButtonTextColor ),
                () => entity.ActionSecondaryButtonTextColor = box.Entity.ActionSecondaryButtonTextColor );

            box.IfValidProperty( nameof( box.Entity.ActionTextColor ),
                () => entity.ActionTextColor = box.Entity.ActionTextColor );

            box.IfValidProperty( nameof( box.Entity.AudienceAccentColor ),
                () => entity.AudienceAccentColor = box.Entity.AudienceAccentColor );

            box.IfValidProperty( nameof( box.Entity.AudienceBackgroundColor ),
                () => entity.AudienceBackgroundColor = box.Entity.AudienceBackgroundColor );

            box.IfValidProperty( nameof( box.Entity.AudienceBackgroundImageBinaryFile ),
                () => entity.AudienceBackgroundImageBinaryFileId = box.Entity.AudienceBackgroundImageBinaryFile.GetEntityId<BinaryFile>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.AudienceCustomCss ),
                () => entity.AudienceCustomCss = box.Entity.AudienceCustomCss );

            box.IfValidProperty( nameof( box.Entity.AudiencePrimaryColor ),
                () => entity.AudiencePrimaryColor = box.Entity.AudiencePrimaryColor );

            box.IfValidProperty( nameof( box.Entity.AudienceSecondaryColor ),
                () => entity.AudienceSecondaryColor = box.Entity.AudienceSecondaryColor );

            box.IfValidProperty( nameof( box.Entity.AudienceTextColor ),
                () => entity.AudienceTextColor = box.Entity.AudienceTextColor );

            box.IfValidProperty( nameof( box.Entity.CampusBehavior ),
                () => settings.CampusBehavior = box.Entity.CampusBehavior );

            box.IfValidProperty( nameof( box.Entity.DefaultCampus ),
                () => settings.DefaultCampusId = box.Entity.DefaultCampus.GetEntityId<Campus>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.ExperienceEndedTemplate ),
                () => settings.ExperienceEndedTemplate = box.Entity.ExperienceEndedTemplate );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.NoActionHeaderImageBinaryFile ),
                () => entity.NoActionHeaderImageBinaryFileId = box.Entity.NoActionHeaderImageBinaryFile.GetEntityId<BinaryFile>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.NoActionMessage ),
                () => entity.NoActionMessage = box.Entity.NoActionMessage );

            box.IfValidProperty( nameof( box.Entity.NoActionTitle ),
                () => entity.NoActionTitle = box.Entity.NoActionTitle );

            box.IfValidProperty( nameof( box.Entity.PhotoBinaryFile ),
                () => entity.PhotoBinaryFileId = box.Entity.PhotoBinaryFile.GetEntityId<BinaryFile>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.PublicLabel ),
                () => entity.PublicLabel = box.Entity.PublicLabel );

            box.IfValidProperty( nameof( box.Entity.PushNotificationDetail ),
                () => entity.PushNotificationDetail = box.Entity.PushNotificationDetail );

            box.IfValidProperty( nameof( box.Entity.PushNotificationTitle ),
                () => entity.PushNotificationTitle = box.Entity.PushNotificationTitle );

            box.IfValidProperty( nameof( box.Entity.PushNotificationType ),
                () => entity.PushNotificationType = box.Entity.PushNotificationType );

            string scheduleError = null;

            var schedulesOk = box.IfValidProperty( nameof( box.Entity.Schedules ),
                () => UpdateSchedules( entity, box.Entity.Schedules, rockContext, out scheduleError ),
                true );

            if ( !schedulesOk )
            {
                errorMessage = scheduleError;
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.WelcomeHeaderImageBinaryFile ),
                () => entity.WelcomeHeaderImageBinaryFileId = box.Entity.WelcomeHeaderImageBinaryFile.GetEntityId<BinaryFile>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.WelcomeMessage ),
                () => entity.WelcomeMessage = box.Entity.WelcomeMessage );

            box.IfValidProperty( nameof( box.Entity.WelcomeTitle ),
                () => entity.WelcomeTitle = box.Entity.WelcomeTitle );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            entity.ExperienceSettingsJson = settings.ToJson();

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="InteractiveExperience"/> to be viewed or edited on the page.</returns>
        private InteractiveExperience GetInitialEntity( RockContext rockContext )
        {
            var entity = GetInitialEntity<InteractiveExperience, InteractiveExperienceService>( rockContext, PageParameterKey.InteractiveExperienceId );

            if ( entity.Id == 0 )
            {
                entity.IsActive = true;
                entity.PushNotificationTitle = "New Experience Activity";
                entity.PushNotificationDetail = "The {{ InteractiveExperience.Name }} experience has a new activity showing.";
            }

            return entity;
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
        private string GetSecurityGrantToken( InteractiveExperience entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out InteractiveExperience entity, out BlockActionResult error )
        {
            var entityService = new InteractiveExperienceService( rockContext );
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
                entity = new InteractiveExperience
                {
                    IsActive = true,
                    PushNotificationTitle = "New Experience Activity",
                    PushNotificationDetail = "The {{ InteractiveExperience.Name }} experience has a new activity showing."
                };
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{InteractiveExperience.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${InteractiveExperience.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the schedule bag for the specified experience schedule.
        /// </summary>
        /// <param name="experienceSchedule">The experience schedule that needs to be bagged.</param>
        /// <returns>A new <see cref="InteractiveExperienceScheduleBag"/> that represents <paramref name="experienceSchedule"/>.</returns>
        private static InteractiveExperienceScheduleBag GetScheduleBag( InteractiveExperienceSchedule experienceSchedule )
        {
            return new InteractiveExperienceScheduleBag
            {
                Guid = experienceSchedule.Guid,
                Schedule = new ListItemBag
                {
                    Value = experienceSchedule.Schedule.iCalendarContent,
                    Text = experienceSchedule.Schedule.ToString( true )
                },
                EnableMinutesBefore = experienceSchedule.Schedule.CheckInStartOffsetMinutes,
                Campuses = experienceSchedule.InteractiveExperienceScheduleCampuses
                    .Select( c => c.Campus )
                    .OrderBy( c => c.Order )
                    .ThenBy( c => c.Name )
                    .ToListItemBagList(),
                DataView = experienceSchedule.DataView.ToListItemBag(),
                Group = experienceSchedule.Group.ToListItemBag()
            };
        }

        /// <summary>
        /// Updates the schedules of the experience with the data provided
        /// in the bags.
        /// </summary>
        /// <param name="interactiveExperience">The interactive experience to be updated.</param>
        /// <param name="scheduleBags">The schedule bags that contain the new schedule information.</param>
        /// <param name="rockContext">The rock context to operate in.</param>
        /// <param name="errorMessage">The error message if the schedules could not be updated.</param>
        /// <returns><c>true</c> if the schedules were updated, <c>false</c> otherwise.</returns>
        private bool UpdateSchedules( InteractiveExperience interactiveExperience, List<InteractiveExperienceScheduleBag> scheduleBags, RockContext rockContext, out string errorMessage )
        {
            var experienceScheduleService = new InteractiveExperienceScheduleService( rockContext );
            var experienceAnswerService = new InteractiveExperienceAnswerService( rockContext );
            var incomingScheduleGuids = scheduleBags?.Select( s => s.Guid ).ToList() ?? new List<Guid>();

            errorMessage = null;

            interactiveExperience.InteractiveExperienceSchedules.LoadAttributes( rockContext );

            // Delete any existing schedules that have been removed.
            foreach ( var schedule in interactiveExperience.InteractiveExperienceSchedules.ToList() )
            {
                if ( !incomingScheduleGuids.Contains( schedule.Guid ) )
                {
                    if ( !experienceScheduleService.CanDelete( schedule, out errorMessage ) )
                    {
                        return false;
                    }

                    var answersToDelete = experienceAnswerService
                        .Queryable()
                        .Where( a => a.InteractiveExperienceOccurrence.InteractiveExperienceScheduleId == schedule.Id )
                        .ToList();

                    experienceAnswerService.DeleteRange( answersToDelete );

                    interactiveExperience.InteractiveExperienceSchedules.Remove( schedule );
                    experienceScheduleService.Delete( schedule );
                }
            }

            // Add or update any schedules that were newly added.
            if ( scheduleBags != null && scheduleBags.Count > 0 )
            {
                foreach ( var scheduleBag in scheduleBags )
                {
                    var schedule = interactiveExperience.InteractiveExperienceSchedules.FirstOrDefault( s => s.Guid == scheduleBag.Guid );

                    if ( schedule == null )
                    {
                        schedule = new InteractiveExperienceSchedule
                        {
                            Guid = scheduleBag.Guid,
                            InteractiveExperienceId = interactiveExperience.Id,
                            Schedule = new Schedule()
                        };

                        schedule.LoadAttributes( rockContext );

                        interactiveExperience.InteractiveExperienceSchedules.Add( schedule );
                    }

                    if ( !UpdateScheduleCampuses( schedule, scheduleBag.Campuses, rockContext, out errorMessage ) )
                    {
                        return false;
                    }

                    schedule.Schedule.iCalendarContent = scheduleBag.Schedule.Value;
                    schedule.Schedule.CheckInStartOffsetMinutes = scheduleBag.EnableMinutesBefore;
                    schedule.DataViewId = scheduleBag.DataView.GetEntityId<DataView>( rockContext );
                    schedule.GroupId = scheduleBag.Group.GetEntityId<Rock.Model.Group>( rockContext );
                    schedule.SetPublicAttributeValues( scheduleBag.AttributeValues, RequestContext.CurrentPerson );

                    // Force the cache to refresh if the only thing that
                    // changed was the schedule content.
                    schedule.ModifiedDateTime = RockDateTime.Now;
                }
            }

            return true;
        }

        /// <summary>
        /// Updates the experience schedule campuses to match what is provided
        /// in the bags.
        /// </summary>
        /// <param name="experienceSchedule">The experience schedule to be updated.</param>
        /// <param name="campusBags">The campus bags that identify the campuses.</param>
        /// <param name="rockContext">The rock context to operate in.</param>
        /// <param name="errorMessage">The error message if the experience schedule could not be updated.</param>
        /// <returns><c>true</c> if the experience schedule was updated, <c>false</c> otherwise.</returns>
        private static bool UpdateScheduleCampuses( InteractiveExperienceSchedule experienceSchedule, List<ListItemBag> campusBags, RockContext rockContext, out string errorMessage )
        {
            var experienceScheduleCampusService = new InteractiveExperienceScheduleCampusService( rockContext );
            var incomingCampusGuids = campusBags?.Select( c => c.Value.AsGuid() ).ToList() ?? new List<Guid>();

            errorMessage = null;

            // Delete any existing campuses that have been removed.
            foreach ( var scheduleCampus in experienceSchedule.InteractiveExperienceScheduleCampuses.ToList() )
            {
                if ( !incomingCampusGuids.Contains( scheduleCampus.Campus.Guid ) )
                {
                    if ( !experienceScheduleCampusService.CanDelete( scheduleCampus, out errorMessage ) )
                    {
                        return false;
                    }

                    experienceSchedule.InteractiveExperienceScheduleCampuses.Remove( scheduleCampus );
                    experienceScheduleCampusService.Delete( scheduleCampus );
                }
            }

            // Add or update any campuses that were newly added.
            if ( campusBags != null && campusBags.Count > 0 )
            {
                foreach ( var campusBag in campusBags )
                {
                    var campusId = CampusCache.GetId( campusBag.Value.AsGuid() );

                    if ( !campusId.HasValue )
                    {
                        continue;
                    }

                    var scheduleCampus = experienceSchedule.InteractiveExperienceScheduleCampuses.FirstOrDefault( s => s.Campus.Guid == campusBag.Value.AsGuid() );

                    if ( scheduleCampus == null )
                    {
                        scheduleCampus = new InteractiveExperienceScheduleCampus
                        {
                            CampusId = campusId.Value
                        };

                        experienceSchedule.InteractiveExperienceScheduleCampuses.Add( scheduleCampus );
                    }
                    else
                    {
                        scheduleCampus.CampusId = campusId.Value;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the action bag for the specified action.
        /// </summary>
        /// <param name="action">The action to be converted to a bag.</param>
        /// <returns>A new instance of <see cref="InteractiveExperienceActionBag"/> that represents <paramref name="action"/>.</returns>
        private static InteractiveExperienceActionBag GetActionBag( InteractiveExperienceAction action )
        {
            var actionEntityCache = EntityTypeCache.Get( action.ActionEntityTypeId );
            var actionComponent = ActionTypeContainer.GetComponent( actionEntityCache.Name );

            return new InteractiveExperienceActionBag
            {
                Guid = action.Guid,
                Title = actionComponent?.GetDisplayTitle( action ) ?? "Unknown action",
                ActionType = action.ActionEntityType.ToListItemBag(),
                IsMultipleSubmissionsAllowed = action.IsMultipleSubmissionAllowed,
                IsModerationRequired = action.IsModerationRequired,
                IsResponseAnonymous = action.IsResponseAnonymous,
                ResponseVisualizer = action.ResponseVisualEntityType.ToListItemBag(),
                AttributeValues = action.GetPublicAttributeValuesForEdit( null, false, IsAttributeValueIncluded )
            };
        }

        /// <summary>
        /// Determines whether if the attribute is an action attribute.
        /// </summary>
        /// <param name="attribute">The attribute to be checked.</param>
        /// <returns><c>true</c> if the attribute is an action attribute; otherwise, <c>false</c>.</returns>
        private static bool IsActionAttributeIncluded( AttributeCache attribute )
        {
            return attribute.Key.StartsWith( "action" );
        }

        /// <summary>
        /// Determines whether if the attribute is an visualizer attribute.
        /// </summary>
        /// <param name="attribute">The attribute to be checked.</param>
        /// <returns><c>true</c> if the attribute is an visualizer attribute; otherwise, <c>false</c>.</returns>
        private static bool IsVisualizerAttributeIncluded( AttributeCache attribute )
        {
            return attribute.Key.StartsWith( "visualizer" );
        }

        /// <summary>
        /// Determines whether the attribute value associated with the attribute
        /// should be included in the bag.
        /// </summary>
        /// <param name="attribute">The attribute the value is for.</param>
        /// <returns><c>true</c> if the attribute value should be included; otherwise, <c>false</c>.</returns>
        private static bool IsAttributeValueIncluded( AttributeCache attribute )
        {
            return IsActionAttributeIncluded( attribute ) || IsVisualizerAttributeIncluded( attribute );
        }

        /// <summary>
        /// Adds a new or updates an existing action to the interactive experience.
        /// </summary>
        /// <param name="interactiveExperience">The interactive experience to add or update the action on.</param>
        /// <param name="box">The box that contains the action bag.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On return will contain any error message generated.</param>
        /// <returns><c>true</c> if the action was added or updated, <c>false</c> otherwise.</returns>
        private static bool AddOrUpdateActionFromBox( InteractiveExperience interactiveExperience, ValidPropertiesBox<InteractiveExperienceActionBag> box, RockContext rockContext, out string errorMessage )
        {
            var experienceActionService = new InteractiveExperienceActionService( rockContext );

            errorMessage = null;

            if ( !box.IsValidProperty( nameof( box.Bag.Guid ) ) )
            {
                errorMessage = "Action must specify the Guid property.";
                return false;
            }

            var action = interactiveExperience.InteractiveExperienceActions.FirstOrDefault( a => a.Guid == box.Bag.Guid );

            if ( action == null )
            {
                action = new InteractiveExperienceAction
                {
                    Guid = box.Bag.Guid
                };

                interactiveExperience.InteractiveExperienceActions.Add( action );
            }

            if ( box.IsValidProperty( nameof( box.Bag.ActionType ) ) )
            {
                var actionEntityTypeId = box.Bag.ActionType.GetEntityId<EntityType>( rockContext );

                if ( !actionEntityTypeId.HasValue )
                {
                    errorMessage = $"Invalid action type '${box.Bag.ActionType.Text}' specified.";
                    return false;
                }

                action.ActionEntityTypeId = actionEntityTypeId.Value;
            }

            box.IfValidProperty( nameof( box.Bag.IsMultipleSubmissionsAllowed ),
                () => action.IsMultipleSubmissionAllowed = box.Bag.IsMultipleSubmissionsAllowed );

            box.IfValidProperty( nameof( box.Bag.IsModerationRequired ),
                () => action.IsModerationRequired = box.Bag.IsModerationRequired );

            box.IfValidProperty( nameof( box.Bag.IsResponseAnonymous ),
                () => action.IsResponseAnonymous = box.Bag.IsResponseAnonymous );

            if ( box.IsValidProperty( nameof( box.Bag.ResponseVisualizer ) ) )
            {
                var visualizerEntityTypeId = box.Bag.ResponseVisualizer.GetEntityId<EntityType>( rockContext );

                action.ResponseVisualEntityTypeId = visualizerEntityTypeId;
            }

            if ( action.Id == 0 )
            {
                var lastOrder = experienceActionService.Queryable()
                    .Where( a => a.InteractiveExperienceId == interactiveExperience.Id )
                    .Select( a => ( int? ) a.Order )
                    .Max();

                action.Order = lastOrder.HasValue ? lastOrder.Value + 1 : 0;
            }

            if ( box.IsValidProperty( nameof( box.Bag.AttributeValues ) ) )
            {
                action.LoadAttributes( rockContext );

                action.SetPublicAttributeValues( box.Bag.AttributeValues, null, false );
            }

            return true;
        }

        /// <summary>
        /// Gets the available action types that can be selected.
        /// </summary>
        /// <returns>A collection of <see cref="InteractiveExperienceActionTypeBag"/> objects that represent that action types.</returns>
        private static List<InteractiveExperienceActionTypeBag> GetAvailableActionTypes()
        {
            var actionTypes = new List<InteractiveExperienceActionTypeBag>();

            foreach ( var component in ActionTypeContainer.Instance.AllComponents )
            {
                var actionTypeCache = EntityTypeCache.Get( component.GetType() );

                var actionType = new InteractiveExperienceActionTypeBag
                {
                    Value = actionTypeCache.Guid.ToString(),
                    Text = ActionTypeContainer.GetComponentName( component ),
                    IconCssClass = component.IconCssClass,
                    IsModerationSupported = component.IsModerationSupported,
                    IsMultipleSubmissionSupported = component.IsMultipleSubmissionSupported,
                    IsQuestionSupported = component.IsQuestionSupported
                };

                var action = new InteractiveExperienceAction
                {
                    ActionEntityTypeId = actionTypeCache.Id
                };

                action.LoadAttributes( null );
                actionType.Attributes = action.GetPublicAttributesForEdit( null, false, IsActionAttributeIncluded );
                actionType.DefaultAttributeValues = action.GetPublicAttributeValuesForEdit( null, false, IsVisualizerAttributeIncluded );

                actionTypes.Add( actionType );
            }

            return actionTypes.OrderBy( at => at.Text ).ToList();
        }

        /// <summary>
        /// Gets the available visualizer types that can be selected.
        /// </summary>
        /// <returns>A collection of <see cref="InteractiveExperienceVisualizerTypeBag"/> objects that represent that visualizer types.</returns>
        private static List<InteractiveExperienceVisualizerTypeBag> GetAvailableVisualizerTypes()
        {
            var visualizerTypes = new List<InteractiveExperienceVisualizerTypeBag>();

            foreach ( var component in VisualizerTypeContainer.Instance.AllComponents )
            {
                var visualizerTypeCache = EntityTypeCache.Get( component.GetType() );

                var visualizerType = new InteractiveExperienceVisualizerTypeBag
                {
                    Value = visualizerTypeCache.Guid.ToString(),
                    Text = VisualizerTypeContainer.GetComponentName( component )
                };

                var action = new InteractiveExperienceAction
                {
                    ResponseVisualEntityTypeId = visualizerTypeCache.Id
                };

                action.LoadAttributes( null );
                visualizerType.Attributes = action.GetPublicAttributesForEdit( null, false, IsVisualizerAttributeIncluded );
                visualizerType.DefaultAttributeValues = action.GetPublicAttributeValuesForEdit( null, false, IsVisualizerAttributeIncluded );

                visualizerTypes.Add( visualizerType );
            }

            return visualizerTypes.OrderBy( at => at.Text ).ToList();
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

                var box = new DetailBlockBox<InteractiveExperienceBag, InteractiveExperienceDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<InteractiveExperienceBag, InteractiveExperienceDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new InteractiveExperienceService( rockContext );
                var binaryFileService = new BinaryFileService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Record all the old binary file identifiers in case any
                // get deleted by the update operation.
                var oldBinaryFileIds = new List<int>()
                {
                    entity.ActionBackgroundImageBinaryFileId ?? 0,
                    entity.AudienceBackgroundImageBinaryFileId ?? 0,
                    entity.NoActionHeaderImageBinaryFileId ?? 0,
                    entity.PhotoBinaryFileId ?? 0,
                    entity.WelcomeHeaderImageBinaryFileId ?? 0
                }.Where( fileId => fileId != 0 ).ToList();

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext, out var updateMessage ) )
                {
                    return ActionBadRequest( updateMessage ?? "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateInteractiveExperience( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;
                var binaryFileIds = new List<int>
                    {
                        entity.ActionBackgroundImageBinaryFileId ?? 0,
                        entity.AudienceBackgroundImageBinaryFileId ?? 0,
                        entity.NoActionHeaderImageBinaryFileId ?? 0,
                        entity.PhotoBinaryFileId ?? 0,
                        entity.WelcomeHeaderImageBinaryFileId ?? 0
                    }.Where( fileId => fileId != 0 ).ToList();
                var removedBinaryFileIds = oldBinaryFileIds.Except( binaryFileIds ).ToList();

                // Ensure all the current binary files are marked as permanent.
                binaryFileService.Queryable()
                    .Where( bf => binaryFileIds.Contains( bf.Id ) && bf.IsTemporary )
                    .ToList()
                    .ForEach( bf => bf.IsTemporary = false );

                // Ensure all the removed binary files are marked as temporary.
                binaryFileService.Queryable()
                    .Where( bf => removedBinaryFileIds.Contains( bf.Id ) && !bf.IsTemporary )
                    .ToList()
                    .ForEach( bf => bf.IsTemporary = true );

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    entity.SaveAttributeValues( rockContext );

                    foreach ( var schedule in entity.InteractiveExperienceSchedules )
                    {
                        schedule.SaveAttributeValues( rockContext );
                    }
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.InteractiveExperienceId] = entity.IdKey
                    } ) );
                }

                // Using a second context isn't normal, but because we do such deep
                // manipulation of entities we needed this. Otherwise the entity
                // loaded from the old RockContext would pull a non-proxy object
                // from cache which causes ToListItemBag() to fail.
                using ( var freshRockContext = new RockContext() )
                {
                    entity = new InteractiveExperienceService( freshRockContext ).Get( entity.Id );
                    entity.LoadAttributes( rockContext );

                    return ActionOk( GetEntityBagForView( entity ) );
                }
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
                var entityService = new InteractiveExperienceService( rockContext );
                var interactiveExperienceAnswerService = new InteractiveExperienceAnswerService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                var answers = interactiveExperienceAnswerService.Queryable()
                    .Where( a => a.InteractiveExperienceAction.InteractiveExperienceId == entity.Id )
                    .ToList();

                rockContext.WrapTransaction( () =>
                {
                    interactiveExperienceAnswerService.DeleteRange( answers );
                    rockContext.SaveChanges();

                    entityService.Delete( entity );
                    rockContext.SaveChanges();
                } );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<InteractiveExperienceBag, InteractiveExperienceDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext, out var updateMessage ) )
                {
                    return ActionBadRequest( updateMessage ?? "Invalid data." );
                }

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<InteractiveExperienceBag, InteractiveExperienceDetailOptionsBag>
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

        /// <summary>
        /// Creates or updates the action contained in the box.
        /// </summary>
        /// <param name="idKey">The identifier key of the interactive experience this action belongs to.</param>
        /// <param name="box">The box that contains the information about the action.</param>
        /// <returns>An instance of <see cref="InteractiveExperienceActionBag"/> that represents the action.</returns>
        [BlockAction]
        public BlockActionResult SaveAction( string idKey, ValidPropertiesBox<InteractiveExperienceActionBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( idKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !AddOrUpdateActionFromBox( entity, box, rockContext, out var updateMessage ) )
                {
                    return ActionBadRequest( updateMessage ?? "Invalid data." );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    var editedAction = entity.InteractiveExperienceActions.SingleOrDefault( a => a.Guid == box.Bag.Guid );

                    if ( editedAction.Attributes != null )
                    {
                        editedAction.SaveAttributeValues( rockContext );
                    }
                } );

                // Using a second context isn't normal, but we need to ensure we get
                // a proxy object back if we created a new action rather than
                // the cached non-proxy version.
                using ( var freshRockContext = new RockContext() )
                {
                    var freshAction = new InteractiveExperienceActionService( freshRockContext ).Get( box.Bag.Guid );

                    freshAction.LoadAttributes( freshRockContext );

                    return ActionOk( GetActionBag( freshAction ) );
                }
            }
        }

        /// <summary>
        /// Reorders the actions for the interactive experience so that the
        /// specified action is directly before the other action.
        /// </summary>
        /// <param name="idKey">The identifier key of the interactive experience that these actions belong to.</param>
        /// <param name="actionGuid">The action unique identifier of the action to be moved.</param>
        /// <param name="beforeActionGuid">The action unique identifier of the action that it should be moved before; or <c>null</c> to move it to the end.</param>
        /// <returns>An empty 200 OK result if it was moved or an error response if it could not be moved.</returns>
        [BlockAction]
        public BlockActionResult ReorderAction( string idKey, Guid actionGuid, Guid? beforeActionGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( idKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.InteractiveExperienceActions
                    .ToList()
                    .ReorderEntity( actionGuid.ToString(), beforeActionGuid?.ToString() );

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Deletes the action from the interactive experience.
        /// </summary>
        /// <param name="idKey">The identifier key of the interactive experience that contains the action.</param>
        /// <param name="actionGuid">The unique identifier of the action to be deleted.</param>
        /// <returns>A 200 OK response if the action was deleted or an error response if it could not be deleted.</returns>
        [BlockAction]
        public BlockActionResult DeleteAction( string idKey, Guid actionGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var actionService = new InteractiveExperienceActionService( rockContext );
                var answerService = new InteractiveExperienceAnswerService( rockContext );
                var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );

                if ( !TryGetEntityForEditAction( idKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                var action = entity.InteractiveExperienceActions.SingleOrDefault( a => a.Guid == actionGuid );

                if ( action == null )
                {
                    return ActionNotFound( "The specified action was not found." );
                }

                var answersToDelete = answerService
                    .Queryable()
                    .Where( a => a.InteractiveExperienceActionId == action.Id )
                    .ToList();

                answerService.DeleteRange( answersToDelete );

                var occurrencesToUpdate = occurrenceService
                    .Queryable()
                    .Where( o => o.CurrentlyShownActionId == action.Id )
                    .ToList();

                occurrencesToUpdate.ForEach( o => o.CurrentlyShownActionId = null );

                actionService.Delete( action );

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion
    }
}
