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
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.RequestFilterDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Personalization;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular request filter.
    /// </summary>

    [DisplayName( "Request Filter Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a particular request filter." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "0E340E27-D6D9-4870-9835-401545C44801" )]
    [Rock.SystemGuid.BlockTypeGuid( "59E6D50E-70A8-4695-97A4-2DE33DD09ECF" )]
    public class RequestFilterDetail : RockEntityDetailBlockType<RequestFilter, RequestFilterBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string RequestFilterId = "RequestFilterId";
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
            var box = new DetailBlockBox<RequestFilterBag, RequestFilterDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private RequestFilterDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new RequestFilterDetailOptionsBag();

            // Populate active sites for dropdown, include blank option client-side.
            options.Sites = SiteCache.GetAllActiveSites()
                .Select( s => s.ToListItemBag() )
                .ToList();

            return options;
        }

        /// <summary>
        /// Validates the RequestFilter for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="requestFilter">The RequestFilter to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the RequestFilter is valid, <c>false</c> otherwise.</returns>
        private bool ValidateRequestFilter( RequestFilter requestFilter, out string errorMessage )
        {
            errorMessage = null;

            var environment = requestFilter?.FilterConfiguration?.EnvironmentRequestFilter;
            if ( environment?.BeginningTimeOfDay.HasValue == true && environment?.EndingTimeOfDay.HasValue == true )
            {
                if ( environment.EndingTimeOfDay < environment.BeginningTimeOfDay )
                {
                    errorMessage = "Time of Day Beginning must always be smaller than Ending.";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<RequestFilterBag, RequestFilterDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {RequestFilter.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( RequestFilter.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( RequestFilter.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="RequestFilterBag"/> that represents the entity.</returns>
        private RequestFilterBag GetCommonEntityBag( RequestFilter entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new RequestFilterBag
            {
                IdKey = entity.IdKey,
                AdditionalFilterConfiguration = MapAdditionalFilterConfigurationToBag( entity.FilterConfiguration ),
                IsActive = entity.IsActive,
                Name = entity.Name,
                RequestFilterKey = entity.RequestFilterKey,
                Site = entity.Site.ToListItemBag(),
                SiteId = entity.SiteId
            };
        }

        /// <inheritdoc/>
        protected override RequestFilterBag GetEntityBagForView( RequestFilter entity )
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

        //// <inheritdoc/>
        protected override RequestFilterBag GetEntityBagForEdit( RequestFilter entity )
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
        protected override bool UpdateEntityFromBox( RequestFilter entity, ValidPropertiesBox<RequestFilterBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.AdditionalFilterConfiguration ),
                () => entity.FilterConfiguration = MapFilterConfigurationFromBag( box.Bag.AdditionalFilterConfiguration ) );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.RequestFilterKey ),
                () => entity.RequestFilterKey = box.Bag.RequestFilterKey );

            box.IfValidProperty( nameof( box.Bag.Site ),
                () => entity.SiteId = box.Bag.Site.GetEntityId<Site>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.SiteId ),
                () => entity.SiteId = box.Bag.SiteId );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override RequestFilter GetInitialEntity()
        {
            return GetInitialEntity<RequestFilter, RequestFilterService>( RockContext, PageParameterKey.RequestFilterId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out RequestFilter entity, out BlockActionResult error )
        {
            var entityService = new RequestFilterService( RockContext );
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
                entity = new RequestFilter();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{RequestFilter.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${RequestFilter.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        #endregion Methods

        #region Helper Methods

        /// <summary>
        /// Maps the domain configuration into the additional filter configuration bag used by the UI.
        /// </summary>
        /// <param name="configuration">The domain personalization request filter configuration.</param>
        /// <returns>An <see cref="AdditionalFilterConfigurationBag"/> for client consumption.</returns>
        private AdditionalFilterConfigurationBag MapAdditionalFilterConfigurationToBag( PersonalizationRequestFilterConfiguration configuration )
        {
            if ( configuration == null )
            {
                return null;
            }

            var bag = new AdditionalFilterConfigurationBag
            {
                PreviousActivityRequestFilter = new PreviousActivityRequestFilterBag
                {
                    PreviousActivityTypes = configuration.PreviousActivityRequestFilter?.PreviousActivityTypes?.Select( t => ( int ) t ).ToList()
                },
                DeviceTypeRequestFilter = new DeviceTypeRequestFilterBag
                {
                    DeviceTypes = configuration.DeviceTypeRequestFilter?.DeviceTypes?.Select( t => ( int ) t ).ToList()
                },
                QueryStringRequestFilterExpressionType = ( int ) configuration.QueryStringRequestFilterExpressionType,
                QueryStringRequestFilters = configuration.QueryStringRequestFilters?.Select( q => new QueryStringRequestFilterBag
                {
                    Key = q.Key,
                    ComparisonType = ( int ) q.ComparisonType,
                    ComparisonValue = q.ComparisonValue,
                    Guid = q.Guid
                } ).ToList(),
                CookieRequestFilterExpressionType = ( int ) configuration.CookieRequestFilterExpressionType,
                CookieRequestFilters = configuration.CookieRequestFilters?.Select( c => new CookieRequestFilterBag
                {
                    Key = c.Key,
                    ComparisonType = ( int ) c.ComparisonType,
                    ComparisonValue = c.ComparisonValue,
                    Guid = c.Guid
                } ).ToList(),
                BrowserRequestFilters = configuration.BrowserRequestFilters?.Select( b => new BrowserRequestFilterBag
                {
                    BrowserFamily = ( int ) b.BrowserFamily,
                    VersionComparisonType = ( int ) b.VersionComparisonType,
                    MajorVersion = b.MajorVersion,
                    Guid = b.Guid
                } ).ToList(),
                IPAddressRequestFilters = configuration.IPAddressRequestFilters?.Select( i => new IPAddressRequestFilterBag
                {
                    MatchType = ( int ) i.MatchType,
                    BeginningIPAddress = i.BeginningIPAddress,
                    EndingIPAddress = i.EndingIPAddress,
                    Guid = i.Guid
                } ).ToList(),
                GeolocationRequestFilters = configuration.GeolocationRequestFilters?.Select( g => new GeolocationRequestFilterBag
                {
                    ComparisonType = ( int ) g.ComparisonType,
                    LocationComponent = ( int ) g.LocationComponent,
                    Value = g.Value,
                    Guid = g.Guid
                } ).ToList(),
                EnvironmentRequestFilter = new EnvironmentRequestFilterBag
                {
                    DaysOfWeek = configuration.EnvironmentRequestFilter?.DaysOfWeek?.Select( d => ( int ) d ).ToList(),
                    BeginningTimeOfDay = ToTimePickerValueBag( configuration.EnvironmentRequestFilter?.BeginningTimeOfDay ),
                    EndingTimeOfDay = ToTimePickerValueBag( configuration.EnvironmentRequestFilter?.EndingTimeOfDay )
                }
            };

            return bag;
        }

        /// <summary>
        /// Maps the additional filter configuration bag from the UI back into the domain configuration model.
        /// </summary>
        /// <param name="bag">The additional filter configuration bag.</param>
        /// <returns>A populated <see cref="PersonalizationRequestFilterConfiguration"/>.</returns>
        private PersonalizationRequestFilterConfiguration MapFilterConfigurationFromBag( AdditionalFilterConfigurationBag bag )
        {
            if ( bag == null )
            {
                return new PersonalizationRequestFilterConfiguration();
            }

            var config = new PersonalizationRequestFilterConfiguration
            {
                PreviousActivityRequestFilter = new PreviousActivityRequestFilter
                {
                    PreviousActivityTypes = ( bag.PreviousActivityRequestFilter?.PreviousActivityTypes ?? new List<int>() )
                        .Select( i => ( PreviousActivityRequestFilter.PreviousActivityType ) i )
                        .ToArray()
                },
                DeviceTypeRequestFilter = new DeviceTypeRequestFilter
                {
                    DeviceTypes = ( bag.DeviceTypeRequestFilter?.DeviceTypes ?? new List<int>() )
                        .Select( i => ( DeviceTypeRequestFilter.DeviceType ) i )
                        .ToArray()
                },
                QueryStringRequestFilterExpressionType = ( Rock.Model.FilterExpressionType ) bag.QueryStringRequestFilterExpressionType,
                CookieRequestFilterExpressionType = ( Rock.Model.FilterExpressionType ) bag.CookieRequestFilterExpressionType,
                EnvironmentRequestFilter = new EnvironmentRequestFilter
                {
                    DaysOfWeek = ( bag.EnvironmentRequestFilter?.DaysOfWeek ?? new List<int>() )
                        .Select( i => ( DayOfWeek ) i )
                        .ToArray(),
                    BeginningTimeOfDay = FromTimePickerValueBag( bag.EnvironmentRequestFilter?.BeginningTimeOfDay ),
                    EndingTimeOfDay = FromTimePickerValueBag( bag.EnvironmentRequestFilter?.EndingTimeOfDay )
                }
            };

            // Collections
            config.QueryStringRequestFilters = ( bag.QueryStringRequestFilters ?? new List<QueryStringRequestFilterBag>() )
                .Select( q => new QueryStringRequestFilter
                {
                    Key = q.Key,
                    ComparisonType = ( Rock.Model.ComparisonType ) q.ComparisonType,
                    ComparisonValue = q.ComparisonValue,
                    Guid = q.Guid
                } ).ToList();

            config.CookieRequestFilters = ( bag.CookieRequestFilters ?? new List<CookieRequestFilterBag>() )
                .Select( c => new CookieRequestFilter
                {
                    Key = c.Key,
                    ComparisonType = ( Rock.Model.ComparisonType ) c.ComparisonType,
                    ComparisonValue = c.ComparisonValue,
                    Guid = c.Guid
                } ).ToList();

            config.BrowserRequestFilters = ( bag.BrowserRequestFilters ?? new List<BrowserRequestFilterBag>() )
                .Select( b => new BrowserRequestFilter
                {
                    BrowserFamily = ( BrowserRequestFilter.BrowserFamilyEnum ) b.BrowserFamily,
                    VersionComparisonType = ( Rock.Model.ComparisonType ) b.VersionComparisonType,
                    MajorVersion = b.MajorVersion,
                    Guid = b.Guid
                } ).ToList();

            config.IPAddressRequestFilters = ( bag.IPAddressRequestFilters ?? new List<IPAddressRequestFilterBag>() )
                .Select( i => new IPAddressRequestFilter
                {
                    MatchType = ( IPAddressRequestFilter.RangeType ) i.MatchType,
                    BeginningIPAddress = i.BeginningIPAddress,
                    EndingIPAddress = i.EndingIPAddress,
                    Guid = i.Guid
                } ).ToList();

            config.GeolocationRequestFilters = ( bag.GeolocationRequestFilters ?? new List<GeolocationRequestFilterBag>() )
                .Select( g => new GeolocationRequestFilter
                {
                    ComparisonType = ( Rock.Model.ComparisonType ) g.ComparisonType,
                    LocationComponent = ( GeolocationRequestFilter.LocationComponentEnum ) g.LocationComponent,
                    Value = g.Value,
                    Guid = g.Guid
                } ).ToList();

            return config;
        }

        /// <summary>
        /// Converts a <see cref="TimeSpan"/> to a <see cref="TimePickerValueBag"/>.
        /// </summary>
        /// <param name="time">The time span value.</param>
        /// <returns>A <see cref="TimePickerValueBag"/> or <c>null</c> if no time is provided.</returns>
        private TimePickerValueBag ToTimePickerValueBag( TimeSpan? time )
        {
            if ( !time.HasValue )
            {
                return null;
            }

            return new TimePickerValueBag
            {
                Hour = time.Value.Hours,
                Minute = time.Value.Minutes
            };
        }

        /// <summary>
        /// Converts a <see cref="TimePickerValueBag"/> to a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="value">The time picker value bag.</param>
        /// <returns>A <see cref="TimeSpan"/> or <c>null</c> if the bag is incomplete.</returns>
        private TimeSpan? FromTimePickerValueBag( TimePickerValueBag value )
        {
            if ( value == null || !value.Hour.HasValue || !value.Minute.HasValue )
            {
                return null;
            }

            return new TimeSpan( value.Hour.Value, value.Minute.Value, 0 );
        }

        #endregion Helper Methods

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
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<RequestFilterBag>
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
        public BlockActionResult Save( ValidPropertiesBox<RequestFilterBag> box )
        {
            var entityService = new RequestFilterService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateRequestFilter( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            var isDuplicate = new RequestFilterService(RockContext).Queryable().Any(a => a.Id != entity.Id && a.RequestFilterKey == entity.RequestFilterKey);
            if (isDuplicate)
            {
                return ActionBadRequest($"A request filter with the key '{entity.RequestFilterKey}' already exists.");
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
                    [PageParameterKey.RequestFilterId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<RequestFilterBag>
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
            var entityService = new RequestFilterService( RockContext );

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

        #endregion Block Actions
    }
}
