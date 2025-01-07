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
using System.Data.Entity.Spatial;
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
using Rock.ViewModels.Utility;
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
    public class LocationDetail : RockEntityDetailBlockType<Location, LocationBag>
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
            public const string ParentLocationId = "ParentLocationId";
            public const string ExpandedIds = "ExpandedIds";
            public const string PersonId = "PersonId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string CurrentPage = "CurrentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<LocationBag, LocationDetailOptionsBag>();

            box.Options = GetBoxOptions( box.IsEditable );

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private LocationDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new LocationDetailOptionsBag();
            var deviceItems = new Rock.Model.DeviceService( RockContext )
                .GetByDeviceTypeGuid( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER.AsGuid() )
                .OrderBy( d => d.Name ).ToListItemBagList();

            options.PrinterDeviceOptions = deviceItems;
            options.HasPersonId = PageParameter( PageParameterKey.PersonId ).IsNotNullOrWhiteSpace();
            options.HasParentLocationId = PageParameter( PageParameterKey.ParentLocationId ).IsNotNullOrWhiteSpace();
            options.MapStyleGuid = GetAttributeValue( AttributeKey.MapStyle ).AsGuid();
            options.IsPersonIdAvailable = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull().HasValue;
            return options;
        }

        /// <summary>
        /// Validates the Location for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="location">The Location to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Location is valid, <c>false</c> otherwise.</returns>
        private bool ValidateLocation( Location location, out string errorMessage )
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
        private void SetBoxInitialEntityState( DetailBlockBox<LocationBag, LocationDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                var globalAttributesCache = GlobalAttributesCache.Get();

                entity = new Location
                {
                    Id = 0,
                    IsActive = true,
                    ParentLocationId = PageParameter( PageParameterKey.ParentLocationId ).AsIntegerOrNull(),
                    State = globalAttributesCache.OrganizationState,
                    Country = globalAttributesCache.OrganizationCountry
                };
            }

            var isViewable = entity.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );
            box.Options.PanelTitle = entity.ToString( true );
            box.Options.CanAdministrate = entity.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson );

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
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
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Location.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
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
                FirmRoomThreshold = entity.FirmRoomThreshold.ToString(),
                Image = entity.Image.ToListItemBag(),
                ImageUrlParam = GetImageIdOrHash( entity.ImageId ),
                IsActive = entity.IsActive,
                IsGeoPointLocked = entity.IsGeoPointLocked,
                LocationTypeValue = entity.LocationTypeValue.ToListItemBag(),
                Name = entity.Name,
                ParentLocation = ToListItemBag( entity.ParentLocation ),
                PrinterDevice = entity.PrinterDevice.ToListItemBag(),
                SoftRoomThreshold = entity.SoftRoomThreshold.ToString(),
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
        /// Converts the location to ListItemBag, the to string method of Location prefers the full address when available,
        /// and since the ToListItemBag extension method calls ToString() this method is used to ensure the name is preferred.
        /// </summary>
        /// <param name="location">The parent location.</param>
        /// <returns></returns>
        private ListItemBag ToListItemBag( Location location )
        {
            if ( location == null )
            {
                return new ListItemBag();
            }

            return new ListItemBag()
            {
                Text = location.ToString( true ),
                Value = location.Guid.ToString()
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

        /// <inheritdoc/>
        protected override LocationBag GetEntityBagForView( Location entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override LocationBag GetEntityBagForEdit( Location entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );
            var parentLocationId = PageParameter( PageParameterKey.ParentLocationId ).AsIntegerOrNull();

            if ( entity.Id == 0 && parentLocationId.HasValue )
            {
                var parentLocation = new LocationService( RockContext ).Get( parentLocationId.Value );
                bag.ParentLocation = ToListItemBag( parentLocation );
            }

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( Location entity, ValidPropertiesBox<LocationBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.FirmRoomThreshold ),
                () => entity.FirmRoomThreshold = box.Bag.FirmRoomThreshold.AsIntegerOrNull() );

            box.IfValidProperty( nameof( box.Bag.Image ),
                () => entity.ImageId = box.Bag.Image.GetEntityId<BinaryFile>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.IsGeoPointLocked ),
                () => entity.IsGeoPointLocked = box.Bag.IsGeoPointLocked );

            box.IfValidProperty( nameof( box.Bag.LocationTypeValue ),
                () => entity.LocationTypeValueId = box.Bag.LocationTypeValue.GetEntityId<DefinedValue>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.ParentLocation ),
                () => entity.ParentLocationId = box.Bag.ParentLocation.GetEntityId<Location>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.PrinterDevice ),
                () => entity.PrinterDeviceId = box.Bag.PrinterDevice.GetEntityId<Device>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.SoftRoomThreshold ),
                () => entity.SoftRoomThreshold = box.Bag.SoftRoomThreshold.AsIntegerOrNull() );

            box.IfValidProperty( nameof( box.Bag.AddressFields ),
                () =>
                {
                    entity.Street1 = box.Bag.AddressFields.Street1;
                    entity.Street2 = box.Bag.AddressFields.Street2;
                    entity.City = box.Bag.AddressFields.City;
                    entity.Country = box.Bag.AddressFields.Country;
                    entity.PostalCode = box.Bag.AddressFields.PostalCode;
                    entity.State = box.Bag.AddressFields.State;
                } );

            box.IfValidProperty( nameof( box.Bag.GeoPoint_WellKnownText ),
                () => entity.GeoPoint = box.Bag.GeoPoint_WellKnownText.IsNullOrWhiteSpace() ? null : DbGeography.FromText( box.Bag.GeoPoint_WellKnownText ) );

            box.IfValidProperty( nameof( box.Bag.GeoFence_WellKnownText ),
                () => entity.GeoFence = box.Bag.GeoFence_WellKnownText.IsNullOrWhiteSpace() ? null : DbGeography.PolygonFromText( box.Bag.GeoFence_WellKnownText, DbGeography.DefaultCoordinateSystemId ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="Location"/> to be viewed or edited on the page.</returns>
        protected override Location GetInitialEntity()
        {
            return GetInitialEntity<Location, LocationService>( RockContext, PageParameterKey.LocationId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var queryParams = new Dictionary<string, string>();
            var parentLocationId = PageParameter( PageParameterKey.ParentLocationId );
            var expandedIds = PageParameter( PageParameterKey.ExpandedIds );
            var personId = PageParameter( PageParameterKey.PersonId );

            if ( personId.IsNotNullOrWhiteSpace() )
            {
                queryParams.Add( PageParameterKey.PersonId, personId );
            }
            else
            {
                // If parentLocationId was passed in URL odds are block is in treeview mode.
                // The parentLocationId is set as the currentId for the CurrentPage url so
                // on cancel the page is reloaded with the parent location as the location.
                if ( parentLocationId.IsNotNullOrWhiteSpace() )
                {
                    queryParams.Add( PageParameterKey.LocationId, parentLocationId );
                }

                if ( expandedIds.IsNotNullOrWhiteSpace() )
                {
                    queryParams.Add( PageParameterKey.ExpandedIds, expandedIds );
                }
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( queryParams ),
                [NavigationUrlKey.CurrentPage] = this.GetCurrentPageUrl( queryParams ),
            };
        }

        // <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out Location entity, out BlockActionResult error )
        {
            var entityService = new LocationService( RockContext );
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

            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
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
                    },
                    GeoPointWellKnownText = location.GeoPoint?.AsText(),
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
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );
            var box = new ValidPropertiesBox<LocationBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            };

            return ActionOk( box );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<LocationBag> box )
        {
            var entityService = new LocationService( RockContext );

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
            if ( !ValidateLocation( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
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
            entity.LoadAttributes( RockContext );

            var personId = PageParameter( PageParameterKey.PersonId );

            if ( personId.IsNotNullOrWhiteSpace() )
            {
                return ActionOk( this.GetParentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.PersonId] = personId
                } ) );
            }

            else
            {
                var bag = GetEntityBagForView( entity );
                return ActionOk( new ValidPropertiesBox<LocationBag>
                {
                    Bag = bag,
                    ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
                } );
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
            var entityService = new LocationService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            var parentLocationId = entity.ParentLocationId;
            entityService.Delete( entity );
            RockContext.SaveChanges();

            Rock.CheckIn.KioskDevice.Clear();

            var qryParams = new Dictionary<string, string>();
            if ( parentLocationId != null )
            {
                qryParams["LocationId"] = parentLocationId.ToString();
            }

            qryParams[PageParameterKey.ExpandedIds] = PageParameter( PageParameterKey.ExpandedIds );

            return ActionOk( this.GetCurrentPageUrl( qryParams ) );
        }

        #endregion
    }
}
