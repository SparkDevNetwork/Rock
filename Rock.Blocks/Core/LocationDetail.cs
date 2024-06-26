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
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.LocationDetail;
using Rock.ViewModels.Controls;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular location.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Location Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a particular location." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CodeEditorField( "Map HTML",
        Description = "The HTML to use for displaying group location maps. Lava syntax is used to render data from the following data structure: points[type, latitude, longitude], polygons[type, polygon_wkt, google_encoded_polygon]",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 300,
        IsRequired = false,
        DefaultValue = @"{% if point or polygon %}
    <div class='group-location-map'>
        <img class='img-thumbnail' src='//maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60{% if point %}&markers=color:0x779cb1|{{ point.latitude }},{{ point.longitude }}{% endif %}{% if polygon %}&path=fillcolor:0x779cb155|color:0xFFFFFF00|enc:{{ polygon.google_encoded_polygon }}{% endif %}&visual_refresh=true'/>
    </div>
{% endif %}",
        Key = AttributeKey.MapHtml )]

    [DefinedValueField( "Map Style",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.MAP_STYLES,
        Description = "The map theme that should be used for styling the GeoPicker map.",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK,
        Key = AttributeKey.MapStyle )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "862067b0-8764-452e-9b4f-dc3e0cf5f876" )]
    [Rock.SystemGuid.BlockTypeGuid( "d0203b97-5856-437e-8700-8846309f8eed" )]
    public class LocationDetail : RockDetailBlockType
    {
        #region Keys

        public static class AttributeKey
        {
            public const string MapStyle = "MapStyle";
            public const string MapHtml = "MapHTML";
        }

        private static class PageParameterKey
        {
            public const string LocationId = "LocationId";
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
                var box = new DetailBlockBox<LocationBag, LocationDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<Location>();

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
        private LocationDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new LocationDetailOptionsBag();
            var deviceItems = new Rock.Model.DeviceService( rockContext )
                .GetByDeviceTypeGuid( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER.AsGuid() )
                .OrderBy( d => d.Name ).ToListItemBagList();

            options.PrinterDeviceOptions = deviceItems;

            return options;
        }

        /// <summary>
        /// Validates the Location for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="location">The Location to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Location is valid, <c>false</c> otherwise.</returns>
        private bool ValidateLocation( Location location, RockContext rockContext, out string errorMessage )
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
        private void SetBoxInitialEntityState( DetailBlockBox<LocationBag, LocationDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Location.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized(Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized(Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Location.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Location.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="LocationBag"/> that represents the entity.</returns>
        private LocationBag GetCommonEntityBag( Location entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var geoPointAndGeoFenceImageHtml = GetGeoPointAndGeoFenceImageHtml( entity );

            return new LocationBag
            {
                IdKey = entity.IdKey,
                FirmRoomThreshold = entity.FirmRoomThreshold,
                ImageId = entity.ImageId,
                ImageUrlParam = GetImageIdOrHash( entity.ImageId ),
                IsActive = entity.IsActive,
                IsGeoPointLocked = entity.IsGeoPointLocked,
                LocationTypeValue = entity.LocationTypeValue.ToListItemBag(),
                Name = entity.Name,
                ParentLocation = entity.ParentLocation.ToListItemBag(),
                PrinterDevice = entity.PrinterDevice.ToListItemBag(),
                SoftRoomThreshold = entity.SoftRoomThreshold,
                Guid = entity.Guid,
                AddressFields = new AddressControlBag
                {
                    Street1 = entity.Street1 ?? string.Empty,
                    Street2 = entity.Street2 ?? string.Empty,
                    City = entity.City ?? string.Empty,
                    // County = entity.County,
                    Country = entity.Country ?? string.Empty,
                    State = entity.State ?? string.Empty,
                    PostalCode = entity.PostalCode ?? string.Empty
                },

                FormattedHtmlAddress = entity.FormattedHtmlAddress,
                GeoPointImageHtml = geoPointAndGeoFenceImageHtml.GeoPointImageHtml,
                GeoFenceImageHtml = geoPointAndGeoFenceImageHtml.GeoFenceImageHtml,

                // Temporary code until GeoPicker is ready
                GeoPoint_WellKnownText = entity.GeoPoint?.AsText(),
                GeoFence_WellKnownText = entity.GeoFence?.AsText()
            };
        }

        /// <summary>
        /// Gets the geo point and geo fence image HTML.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>System.ValueTuple&lt;System.String, System.String&gt;.</returns>
        private (string GeoPointImageHtml, string GeoFenceImageHtml) GetGeoPointAndGeoFenceImageHtml( Location location )
        {
            var mapStyleValue = DefinedValueCache.Get( GetAttributeValue( AttributeKey.MapStyle ) );
            var googleAPIKey = GlobalAttributesCache.Get().GetValue( "GoogleAPIKey" );

            if ( mapStyleValue == null )
            {
                mapStyleValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK );
            }

            string geoPointImageHtml = string.Empty;
            string geoFenceImageHtml = string.Empty;

            if ( mapStyleValue != null && !string.IsNullOrWhiteSpace( googleAPIKey ) )
            {
                string mapStyle = mapStyleValue.GetAttributeValue( "StaticMapStyle" );

                if ( !string.IsNullOrWhiteSpace( mapStyle ) )
                {
                    if ( location.GeoPoint != null )
                    {
                        string markerPoints = string.Format( "{0},{1}", location.GeoPoint.Latitude, location.GeoPoint.Longitude );
                        string mapLink = System.Text.RegularExpressions.Regex.Replace( mapStyle, @"\{\s*MarkerPoints\s*\}", markerPoints );
                        mapLink = System.Text.RegularExpressions.Regex.Replace( mapLink, @"\{\s*PolygonPoints\s*\}", string.Empty );
                        mapLink += "&sensor=false&size=350x200&zoom=13&format=png&key=" + googleAPIKey;
                        geoPointImageHtml = string.Format( "<div class='group-location-map'><img class='img-thumbnail' src='{0}'/></div>", mapLink );
                    }

                    if ( location.GeoFence != null )
                    {
                        string polygonPoints = "enc:" + location.EncodeGooglePolygon();
                        string mapLink = System.Text.RegularExpressions.Regex.Replace( mapStyle, @"\{\s*MarkerPoints\s*\}", string.Empty );
                        mapLink = System.Text.RegularExpressions.Regex.Replace( mapLink, @"\{\s*PolygonPoints\s*\}", polygonPoints );
                        mapLink += "&sensor=false&size=350x200&format=png&key=" + googleAPIKey;
                        geoFenceImageHtml = string.Format( "<div class='group-location-map'><img class='img-thumbnail' src='{0}'/></div>", mapLink );
                    }
                }
            }

            return (geoPointImageHtml, geoFenceImageHtml);
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="LocationBag"/> that represents the entity.</returns>
        private LocationBag GetEntityBagForView( Location entity )
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
        /// <returns>A <see cref="LocationBag"/> that represents the entity.</returns>
        private LocationBag GetEntityBagForEdit( Location entity )
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
        private bool UpdateEntityFromBox( Location entity, DetailBlockBox<LocationBag, LocationDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.FirmRoomThreshold ),
                () => entity.FirmRoomThreshold = box.Entity.FirmRoomThreshold );

            box.IfValidProperty( nameof( box.Entity.ImageId ),
                () => entity.ImageId = box.Entity.ImageId );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.IsGeoPointLocked ),
                () => entity.IsGeoPointLocked = box.Entity.IsGeoPointLocked );

            box.IfValidProperty( nameof( box.Entity.LocationTypeValue ),
                () => entity.LocationTypeValueId = box.Entity.LocationTypeValue.GetEntityId<DefinedValue>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.ParentLocation ),
                () => entity.ParentLocationId = box.Entity.ParentLocation.GetEntityId<Location>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.PrinterDevice ),
                () => entity.PrinterDeviceId = box.Entity.PrinterDevice.GetEntityId<Device>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.SoftRoomThreshold ),
                () => entity.SoftRoomThreshold = box.Entity.SoftRoomThreshold );

            box.IfValidProperty( nameof( box.Entity.AddressFields ),
                () =>
                {
                    entity.Street1 = box.Entity.AddressFields.Street1;
                    entity.Street2 = box.Entity.AddressFields.Street2;
                    entity.City = box.Entity.AddressFields.City;
                    entity.Country = box.Entity.AddressFields.Country;
                    entity.PostalCode = box.Entity.AddressFields.PostalCode;
                    entity.State = box.Entity.AddressFields.State;
                } );

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
        /// <returns>The <see cref="Location"/> to be viewed or edited on the page.</returns>
        private Location GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<Location, LocationService>( rockContext, PageParameterKey.LocationId );
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
        private string GetSecurityGrantToken( Location entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out Location entity, out BlockActionResult error )
        {
            var entityService = new LocationService( rockContext );
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
                entity = new Location();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Location.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized(Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Location.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        private string GetImageIdOrHash( int? imageId )
        {
            if ( !imageId.HasValue )
            {
                return null;
            }

            var securityService = new SecuritySettingsService();
            var securitySettings = securityService.SecuritySettings;

            if ( securitySettings.DisablePredictableIds )
            {
                return IdHasher.Instance.GetHash( imageId.Value );
            }

            return imageId.Value.ToString();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Performs Address Verification on the specified addressFields and returns a <see cref="AddressStandardizationResultBag"/> with standardized
        /// address values for the addressFields.
        /// </summary>
        /// <param name="addressFields">The address fields.</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult StandardizeLocation( AddressControlBag addressFields )
        {
            using ( var rockContext = new RockContext() )
            {
                var locationService = new LocationService( rockContext );
                var location = new Location
                {
                    Street1 = addressFields.Street1,
                    Street2 = addressFields.Street2,
                    City = addressFields.City,
                    State = addressFields.State,
                    PostalCode = addressFields.PostalCode,
                    Country = addressFields.Country,
                };

                locationService.Verify( location, true );

                var result = new AddressStandardizationResultBag
                {
                    StandardizeAttemptedResult = location.StandardizeAttemptedResult,
                    GeocodeAttemptedResult = location.GeocodeAttemptedResult,
                    AddressFields = new AddressControlBag
                    {
                        Street1 = location.Street1,
                        Street2 = location.Street2,
                        City = location.City,
                        State = location.State,
                        PostalCode = location.PostalCode,
                        Country = location.Country,
                    }
                };

                return ActionOk( result );
            }
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
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<LocationBag, LocationDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<LocationBag, LocationDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new LocationService( rockContext );

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
                if ( !ValidateLocation( entity, rockContext, out var validationMessage ) )
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
                        [PageParameterKey.LocationId] = entity.IdKey
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
                var entityService = new LocationService( rockContext );

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

                Rock.CheckIn.KioskDevice.Clear();

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<LocationBag, LocationDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<LocationBag, LocationDetailOptionsBag>
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
