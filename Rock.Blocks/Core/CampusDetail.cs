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
using System.Text.RegularExpressions;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.CampusDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular campus.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianDetailBlockType" />

    [DisplayName( "Campus Detail" )]
    [Category( "Obsidian > Core" )]
    [Description( "Displays the details of a particular campus." )]
    [IconCssClass( "fa fa-building" )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "A61EAF51-5DB4-451E-9F88-9D4C6ACCE73B")]
    [Rock.SystemGuid.BlockTypeGuid( "507F5108-FB55-48F0-A66E-CC3D5185D35D")]
    public class CampusDetail : RockObsidianDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string CampusId = "CampusId";
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
                var box = new DetailBlockBox<CampusBag, CampusDetailOptionsBag>();

                SetBoxInitialEntityState( box, true, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable );
                box.QualifiedAttributeProperties = GetAttributeQualifiedColumns<Campus>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private CampusDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new CampusDetailOptionsBag
            {
                IsMultiTimeZoneSupported = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.ENABLE_MULTI_TIME_ZONE_SUPPORT ).AsBoolean()
            };

            // Get all the time zone options that will be available for the
            // individual to make their selection from. This is also used
            // when viewing to render the friendly time zone name.
            if ( options.IsMultiTimeZoneSupported )
            {
                options.TimeZoneOptions = TimeZoneInfo.GetSystemTimeZones()
                    .Select( tz => new ListItemBag
                    {
                        Value = tz.Id,
                        Text = tz.DisplayName
                    } )
                    .ToList();
            }

            return options;
        }

        /// <summary>
        /// Parses the <see cref="Campus.ServiceTimes"/> value into a format that
        /// can be used by the client.
        /// </summary>
        /// <param name="serviceTimes">The campus service times.</param>
        /// <returns>A collection of <see cref="ListItemBag"/> objects that represent the service times.</returns>
        private static List<ListItemBag> ConvertServiceTimesToBags( string serviceTimes )
        {
            if ( serviceTimes.IsNullOrWhiteSpace() )
            {
                return new List<ListItemBag>();
            }

            var bags = new List<ListItemBag>();

            // Format is "Day 1^Time 1|Day 2^Time 2"
            var services = serviceTimes.Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( var service in services )
            {
                var segments = service.Split( '^' );

                if ( segments.Length >= 2 )
                {
                    bags.Add( new ListItemBag
                    {
                        Value = segments[0],
                        Text = segments[1]
                    } );
                }
            }

            return bags;
        }

        /// <summary>
        /// Converts the service times from bags into the text string
        /// stored in <see cref="Campus.ServiceTimes"/>.
        /// </summary>
        /// <param name="bags">The packs that represent the service times.</param>
        /// <returns>A custom formatted <see cref="string"/> that contains the service times.</returns>
        private static string ConvertServiceTimesFromBags( List<ListItemBag> bags )
        {
            return bags
                .Select( s => $"{s.Value}^{s.Text}" )
                .JoinStrings( "|" );
        }

        /// <summary>
        /// Converts the campus schedules to bags to represent the custom
        /// data that needs to be included.
        /// </summary>
        /// <param name="campusSchedules">The campus schedules.</param>
        /// <returns>A collection of <see cref="CampusScheduleBag"/> objects that represent the schedules.</returns>
        private static List<CampusScheduleBag> ConvertCampusSchedulesToBags( IEnumerable<CampusSchedule> campusSchedules )
        {
            if ( campusSchedules == null )
            {
                return new List<CampusScheduleBag>();
            }

            return campusSchedules
                .Select( cs => new CampusScheduleBag
                {
                    Guid = cs.Guid,
                    Schedule = cs.Schedule.ToListItemBag(),
                    ScheduleTypeValue = cs.ScheduleTypeValue.ToListItemBag()
                } )
                .ToList();
        }

        /// <summary>
        /// Updates the campus schedules from the data contained in the bags.
        /// </summary>
        /// <param name="campus">The campus instance to be updated.</param>
        /// <param name="bags">The bags that represent the schedules.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the schedules were valid and updated; otherwise <c>false</c>.</returns>
        private bool UpdateCampusSchedulesFromBags( Campus campus, IEnumerable<CampusScheduleBag> bags, RockContext rockContext )
        {
            if ( bags == null )
            {
                return false;
            }

            // Remove any CampusSchedules that were removed in the UI.
            var selectedSchedules = bags.Select( s => s.Guid );
            var locationsToRemove = campus.CampusSchedules.Where( s => !selectedSchedules.Contains( s.Guid ) ).ToList();

            if ( locationsToRemove.Any() )
            {
                var campusScheduleService = new CampusScheduleService( rockContext );

                foreach ( var campusSchedule in locationsToRemove )
                {
                    campus.CampusSchedules.Remove( campusSchedule );
                    campusScheduleService.Delete( campusSchedule );
                }
            }

            // Add or update any schedules that are still selected in the UI.
            int order = 0;
            foreach ( var campusScheduleViewModel in bags )
            {
                var scheduleId = campusScheduleViewModel.Schedule.GetEntityId<Schedule>( rockContext );

                if ( !scheduleId.HasValue )
                {
                    return false;
                }

                var campusSchedule = campus.CampusSchedules.Where( s => s.Guid == campusScheduleViewModel.Guid ).FirstOrDefault();

                if ( campusSchedule == null )
                {
                    campusSchedule = new CampusSchedule()
                    {
                        CampusId = campus.Id,
                        Guid = Guid.NewGuid()
                    };
                    campus.CampusSchedules.Add( campusSchedule );
                }

                campusSchedule.ScheduleId = scheduleId.Value;
                campusSchedule.ScheduleTypeValueId = campusScheduleViewModel.ScheduleTypeValue.GetEntityId<DefinedValue>( rockContext );
                campusSchedule.Order = order++;
            }

            return true;
        }

        /// <summary>
        /// Validates the Campus for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="campus">The Campus to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Campus is valid, <c>false</c> otherwise.</returns>
        private bool ValidateCampus( Campus campus, RockContext rockContext, out string errorMessage )
        {
            // Verify the location is selected and a valid location type.
            var campusLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.LOCATION_TYPE_CAMPUS.AsGuid() );
            var location = new LocationService( rockContext ).Get( campus.LocationId ?? 0 );

            if ( location == null || campusLocationType.Id != location.LocationTypeValueId )
            {
                errorMessage = $"The named location \"{location?.Name}\" is not a 'Campus' location type.";
                return false;
            }

            // Verify the campus name is unique.
            var existingCampus = campus.Id == 0
                ? CampusCache.All( true ).Where( c => c.Name == campus.Name ).FirstOrDefault()
                : CampusCache.All( true ).Where( c => c.Name == campus.Name && c.Id != campus.Id ).FirstOrDefault();

            if ( existingCampus != null )
            {
                var activeString = existingCampus.IsActive ?? false ? "active" : "inactive";

                errorMessage = $"The campus name \"{campus.Name}\" is already in use for an existing {activeString} campus.";
                return false;
            }

            // Verify the phone number is valid.
            if ( !IsPhoneNumberValid( campus.PhoneNumber ) )
            {
                errorMessage = $"The phone number '{campus.PhoneNumber}' is not a valid phone number.";
                return false;
            }

            // Verify the campus URL is valid.
            if ( !IsUrlValid( campus.Url ) )
            {
                errorMessage = $"The URL '{campus.Url}' is not a valid URL.";
                return false;
            }

            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Determines whether the string in <paramref name="phoneNumber"/> is a valid phone number.
        /// Uses the RegEx match string attributes in the defined values for the defined type Communication Phone Country Code.
        /// If there is nothing to match (<paramref name="phoneNumber"/> is null or empty) or match with (Missing defined values or MatchRegEx attribute) then true is returned.
        /// </summary>
        /// <param name="phoneNumber">The phone number to be validated.</param>
        /// <remarks>Taken from PhoneNumberBox UI control.</remarks>
        /// <returns>
        ///   <c>true</c> if <paramref name="phoneNumber"/> is a valid phone number otherwise, <c>false</c>.
        /// </returns>
        private static bool IsPhoneNumberValid( string phoneNumber )
        {
            // No number is a valid number, let the required field validator handle this.
            if ( phoneNumber.IsNullOrWhiteSpace() )
            {
                return true;
            }

            // This is the list of valid phone number formats, it must match one of them.
            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.COMMUNICATION_PHONE_COUNTRY_CODE.AsGuid() );
            if ( definedType == null )
            {
                // If there is nothing to match against then return true
                return true;
            }

            foreach ( var definedValue in definedType.DefinedValues )
            {
                string matchRegEx = definedValue.GetAttributeValue( "MatchRegEx" );
                if ( matchRegEx.IsNullOrWhiteSpace() )
                {
                    // No available pattern so move on
                    continue;
                }

                if ( System.Text.RegularExpressions.Regex.IsMatch( phoneNumber.RemoveAllNonNumericCharacters(), matchRegEx ) )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the string is a valid URL.
        /// </summary>
        /// <param name="url">The URL to be validated.</param>
        /// <returns><c>true</c> if the url is valid; otherwise, <c>false</c>.</returns>
        private static bool IsUrlValid( string url )
        {
            if ( url.IsNullOrWhiteSpace() )
            {
                return true;
            }

            var urlRegex = @"^(http[s]?:\/\/)?[^\s([" + '"' + @" <,>]*\.?[^\s[" + '"' + @",><]*\/$";

            return Regex.IsMatch( url, urlRegex );
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<CampusBag, CampusDetailOptionsBag> box, bool loadAttributes, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity != null )
            {
                var isViewable = entity.IsAuthorized( Security.Authorization.VIEW, RequestContext.CurrentPerson );
                box.IsEditable = entity.IsAuthorized( Security.Authorization.EDIT, RequestContext.CurrentPerson );

                if ( loadAttributes )
                {
                    entity.LoadAttributes( rockContext );
                }

                if ( entity.Id != 0 )
                {
                    // Existing entity was found, prepare for view mode by default.
                    if ( isViewable )
                    {
                        box.Entity = GetEntityBagForView( entity, loadAttributes );
                        box.SecurityGrantToken = GetSecurityGrantToken( entity );
                    }
                    else
                    {
                        box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Campus.FriendlyTypeName );
                    }
                }
                else
                {
                    // New entity is being created, prepare for edit mode by default.
                    if ( box.IsEditable )
                    {
                        box.Entity = GetEntityBagForEdit( entity, loadAttributes );
                        box.SecurityGrantToken = GetSecurityGrantToken( entity );
                    }
                    else
                    {
                        box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Campus.FriendlyTypeName );
                    }
                }
            }
            else
            {
                box.ErrorMessage = $"The {Campus.FriendlyTypeName} was not found.";
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="CampusBag"/> that represents the entity.</returns>
        private CampusBag GetCommonEntityBag( Campus entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new CampusBag
            {
                IdKey = entity.IdKey,
                CampusSchedules = ConvertCampusSchedulesToBags( entity.CampusSchedules ),
                CampusStatusValue = entity.CampusStatusValue.ToListItemBag(),
                //CampusTopics = entity.CampusTopics.ToListItemBagList(),
                CampusTypeValue = entity.CampusTypeValue.ToListItemBag(),
                Description = entity.Description,
                IsActive = entity.IsActive,
                IsSystem = entity.IsSystem,
                LeaderPersonAlias = entity.LeaderPersonAlias.ToListItemBag(),
                Location = entity.Location.ToListItemBag(),
                Name = entity.Name,
                PhoneNumber = entity.PhoneNumber,
                ServiceTimes = ConvertServiceTimesToBags( entity.ServiceTimes ),
                ShortCode = entity.ShortCode,
                TimeZoneId = entity.TimeZoneId,
                Url = entity.Url
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <returns>A <see cref="CampusBag"/> that represents the entity.</returns>
        private CampusBag GetEntityBagForView( Campus entity, bool loadAttributes )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( loadAttributes )
            {
                bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );
            }

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <returns>A <see cref="CampusBag"/> that represents the entity.</returns>
        private CampusBag GetEntityBagForEdit( Campus entity, bool loadAttributes )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( loadAttributes )
            {
                bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );
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
        private bool UpdateEntityFromBox( Campus entity, DetailBlockBox<CampusBag, CampusDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            var isSchedulesValid = box.IfValidProperty( nameof( box.Entity.CampusSchedules ),
                () => UpdateCampusSchedulesFromBags( entity, box.Entity.CampusSchedules, rockContext ),
                true );

            if ( !isSchedulesValid )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.CampusStatusValue ),
                () => entity.CampusStatusValueId = box.Entity.CampusStatusValue.GetEntityId<DefinedValue>( rockContext ) );

            //box.IfValidProperty( nameof( box.Entity.CampusTopics ),
            //    () => entity.CampusTopics = box.Entity./* TODO: Unknown property type 'ICollection<CampusTopic>' for conversion to bag. */ );

            box.IfValidProperty( nameof( box.Entity.CampusTypeValue ),
                () => entity.CampusTypeValueId = box.Entity.CampusTypeValue.GetEntityId<DefinedValue>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.IsSystem ),
                () => entity.IsSystem = box.Entity.IsSystem );

            box.IfValidProperty( nameof( box.Entity.LeaderPersonAlias ),
                () => entity.LeaderPersonAliasId = box.Entity.LeaderPersonAlias.GetEntityId<PersonAlias>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.Location ),
                () => entity.LocationId = box.Entity.Location.GetEntityId<Location>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.PhoneNumber ),
                () => entity.PhoneNumber = box.Entity.PhoneNumber );

            box.IfValidProperty( nameof( box.Entity.ServiceTimes ),
                () => entity.ServiceTimes = ConvertServiceTimesFromBags( box.Entity.ServiceTimes ) );

            box.IfValidProperty( nameof( box.Entity.ShortCode ),
                () => entity.ShortCode = box.Entity.ShortCode );

            box.IfValidProperty( nameof( box.Entity.TimeZoneId ),
                () => entity.TimeZoneId = box.Entity.TimeZoneId );

            box.IfValidProperty( nameof( box.Entity.Url ),
                () => entity.Url = box.Entity.Url );

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
        /// <returns>The <see cref="Campus"/> to be viewed or edited on the page.</returns>
        private Campus GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<Campus, CampusService>( rockContext, PageParameterKey.CampusId );
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out Campus entity, out BlockActionResult error )
        {
            var entityService = new CampusService( rockContext );
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
                entity = new Campus();
                entityService.Add( entity );

                var maxOrder = entityService.Queryable()
                    .Select( t => ( int? ) t.Order )
                    .Max();

                entity.Order = maxOrder.HasValue ? maxOrder.Value + 1 : 0;
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Campus.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Campus.FriendlyTypeName}." );
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

                var box = new DetailBlockBox<CampusBag, CampusDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, true )
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
        public BlockActionResult Save( DetailBlockBox<CampusBag, CampusDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new CampusService( rockContext );

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
                if ( !ValidateCampus( entity, rockContext, out var validationMessage ) )
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
                        [PageParameterKey.CampusId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );
                entity.LoadAttributes( rockContext );

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
                var entityService = new CampusService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Don't allow deleting the last campus.
                if ( !entityService.Queryable().Where( c => c.Id != entity.Id ).Any() )
                {
                    return ActionBadRequest( $"{entity.Name} is the only campus and cannot be deleted (Rock requires at least one campus)." );
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
        public BlockActionResult RefreshAttributes( DetailBlockBox<CampusBag, CampusDetailOptionsBag> box )
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

                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<CampusBag, CampusDetailOptionsBag>
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
