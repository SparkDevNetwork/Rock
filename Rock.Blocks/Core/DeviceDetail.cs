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
using System.Data.Entity.Spatial;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.DeviceDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular device.
    /// </summary>

    [DisplayName( "Device Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given device." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [DefinedValueField( "Map Style",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.MAP_STYLES,
        Description = "The map theme that should be used for styling the GeoPicker map.",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK,
        Key = AttributeKey.MapStyle )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "69638956-3539-44a6-9b66-520133ed6489" )]
    [Rock.SystemGuid.BlockTypeGuid( "e3b5db5c-280f-461c-a6e3-64462c9b329d" )]
    public class DeviceDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string DeviceId = "DeviceId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        public static class AttributeKey
        {
            public const string MapStyle = "MapStyle";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<DeviceBag, DeviceDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<Device>();

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
        private DeviceDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new DeviceDetailOptionsBag();

            options.PrintFromOptions = typeof( PrintFrom ).ToEnumListItemBag();
            options.PrinterOptions = new DeviceService( rockContext )
                .GetByDeviceTypeGuid( new Guid( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER ) )
                .OrderBy( d => d.Name )
                .ToListItemBagList();
            options.KioskTypeOptions = typeof( KioskType ).ToEnumListItemBag();
            options.CameraBarcodeConfigurationOptions = typeof( CameraBarcodeConfiguration ).ToEnumListItemBag();
            options.PrintToOptions = new List<ListItemBag>()
            {
                new ListItemBag() { Text = "Group Type", Value = "0" },
                new ListItemBag() { Text = "Device Printer", Value = "1" },
                new ListItemBag() { Text = "Location Printer", Value = "2" }
            };

            options.MapStyleValueGuid = GetAttributeValue( AttributeKey.MapStyle );

            return options;
        }

        /// <summary>
        /// Validates the Device for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="device">The Device to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Device is valid, <c>false</c> otherwise.</returns>
        private bool ValidateDevice( Device device, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            if ( device.Id == 0 )
            {
                var deviceService = new DeviceService( rockContext );
                // Check for existing
                var existingDevice = deviceService.Queryable()
                    .Where( d => d.Name == device.Name )
                    .FirstOrDefault();

                if ( existingDevice != null )
                {
                    errorMessage = string.Format( "A device already exists with the name '{0}'. Please use a different device name.", existingDevice.Name );
                    return false;
                }
            }

            if ( !VerifyUniqueIpAddress( device.Id, device.DeviceTypeValueId, device.IPAddress ) )
            {
                errorMessage = "IP address must be unique to the device type.";
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
        private void SetBoxInitialEntityState( DetailBlockBox<DeviceBag, DeviceDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Device.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Device.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Device.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="DeviceBag"/> that represents the entity.</returns>
        private DeviceBag GetCommonEntityBag( Device entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new DeviceBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                DeviceType = entity.DeviceType.ToListItemBag(),
                IpAddress = entity.IPAddress,
                IsActive = entity.IsActive,
                Location = entity.Location.ToListItemBag(),
                Name = entity.Name,
                PrinterDevice = entity.PrinterDevice.ToListItemBag(),
                PrintFrom = entity.PrintFrom,
                PrintToOverride = entity.PrintToOverride,
                KioskType = entity.KioskType,
                CameraBarcodeConfigurationType = entity.CameraBarcodeConfigurationType,
                HasCamera = entity.HasCamera,
                GeoFence = entity.Location?.GeoFence?.AsText(),
                GeoPoint = entity.Location?.GeoPoint?.AsText(),
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="DeviceBag"/> that represents the entity.</returns>
        private DeviceBag GetEntityBagForView( Device entity )
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
        /// <returns>A <see cref="DeviceBag"/> that represents the entity.</returns>
        private DeviceBag GetEntityBagForEdit( Device entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            bag.Locations = GetLocations( entity );

            return bag;
        }

        private List<ListItemBag> GetLocations( Device entity )
        {
            var locations = new List<ListItemBag>();
            foreach ( var location in entity.Locations )
            {
                string path = location.Name;
                var parentLocation = location.ParentLocation;
                while ( parentLocation != null )
                {
                    path = $"{parentLocation.Name} > {path}";
                    parentLocation = parentLocation.ParentLocation;
                }

                locations.Add( new ListItemBag() { Value = location.Guid.ToString(), Text = path } );
            }

            return locations;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( Device entity, DetailBlockBox<DeviceBag, DeviceDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.DeviceType ),
                () => entity.DeviceTypeValueId = box.Entity.DeviceType.GetEntityId<DefinedValue>( rockContext ) ?? 0 );

            box.IfValidProperty( nameof( box.Entity.IpAddress ),
                () => entity.IPAddress = box.Entity.IpAddress );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.PrinterDevice ),
                () => entity.PrinterDeviceId = box.Entity.PrinterDevice.GetEntityId<Device>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.PrintFrom ),
                () => entity.PrintFrom = box.Entity.PrintFrom );

            box.IfValidProperty( nameof( box.Entity.PrintToOverride ),
                () => entity.PrintToOverride = box.Entity.PrintToOverride );

            box.IfValidProperty( nameof( box.Entity.CameraBarcodeConfigurationType ),
                () => entity.CameraBarcodeConfigurationType = box.Entity.CameraBarcodeConfigurationType );

            box.IfValidProperty( nameof( box.Entity.KioskType ),
                () => entity.KioskType = box.Entity.KioskType );

            box.IfValidProperty( nameof( box.Entity.HasCamera ),
                () => entity.HasCamera = box.Entity.HasCamera );

            box.IfValidProperty( nameof( box.Entity.Locations ),
                () => SaveLocations( box.Entity, entity, rockContext ) );

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
        /// <returns>The <see cref="Device"/> to be viewed or edited on the page.</returns>
        private Device GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<Device, DeviceService>( rockContext, PageParameterKey.DeviceId );
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
        private string GetSecurityGrantToken( Device entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out Device entity, out BlockActionResult error )
        {
            var entityService = new DeviceService( rockContext );
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
                entity = new Device();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Device.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Device.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Verifies the selected ip is Unique for the device type.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <returns></returns>
        private bool VerifyUniqueIpAddress( int deviceId, int deviceTypeValueId, string ipAddress )
        {
            bool isValid = true;
            if ( !string.IsNullOrWhiteSpace( ipAddress ) )
            {
                var rockContext = new RockContext();
                bool ipExists = new DeviceService( rockContext ).Queryable()
                    .Any( d => d.IPAddress.Equals( ipAddress )
                        && d.DeviceTypeValueId == deviceTypeValueId
                        && d.Id != deviceId );
                isValid = !ipExists;
            }

            return isValid;
        }

        /// <summary>
        /// Saves the locations.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void SaveLocations( DeviceBag bag, Device entity, RockContext rockContext )
        {
            var locationService = new LocationService( rockContext );

            if ( entity.Location == null )
            {
                entity.Location = new Location();
            }

            if ( bag.GeoPoint.IsNotNullOrWhiteSpace() )
            {
                entity.Location.GeoPoint = DbGeography.FromText( bag.GeoPoint );
            }

            if ( bag.GeoFence.IsNotNullOrWhiteSpace() )
            {
                entity.Location.GeoFence = DbGeography.FromText( bag.GeoFence );
            }

            var locationGuids = bag.Locations.ConvertAll( l => l.Value.AsGuid() );
            // Remove any deleted locations
            foreach ( var location in entity.Locations
                .Where( l => !locationGuids.Contains( l.Guid ) )
                .ToList() )
            {
                entity.Locations.Remove( location );
            }

            // Add any new locations
            var existingLocationIDs = entity.Locations.Select( l => l.Id ).ToList();
            foreach ( var location in locationService.Queryable()
                .Where( l =>
                    locationGuids.Contains( l.Guid ) &&
                    !existingLocationIDs.Contains( l.Id ) ) )
            {
                entity.Locations.Add( location );
            }
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

                var box = new DetailBlockBox<DeviceBag, DeviceDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<DeviceBag, DeviceDetailOptionsBag> box )
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

                // Ensure everything is valid before saving.
                if ( !ValidateDevice( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                return ActionOk( this.GetParentPageUrl() );
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
                var entityService = new DeviceService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<DeviceBag, DeviceDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<DeviceBag, DeviceDetailOptionsBag>
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
