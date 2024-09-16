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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;

using Rock.Attribute;
using Rock.CheckIn.v2;
using Rock.Model;
using Rock.Utility.ExtensionMethods;
using Rock.ViewModels.Blocks.CheckIn.CheckInKiosk;
using Rock.ViewModels.CheckIn;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn
{
    /// <summary>
    /// Sets kiosk options and then starts the kiosk to allow self check-in.
    /// </summary>

    [DisplayName( "Check-in Kiosk Setup" )]
    [Category( "Check-in" )]
    [Description( "Sets kiosk options and then starts the kiosk to allow self check-in." )]
    [IconCssClass( "fa fa-clipboard" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Kiosk Page",
        Key = AttributeKey.KioskPage,
        Description = "The page to redirect to after configuration has been set.",
        IsRequired = true,
        Order = 0 )]

    [BooleanField(
        "Allow Manual Setup",
        Key = AttributeKey.AllowManualSetup,
        Description = "If enabled, the block will allow the kiosk to be setup manually if it was not set via other means.",
        DefaultBooleanValue = true,
        Order = 1 )]

    [BooleanField(
        "Enable Location Sharing",
        Key = AttributeKey.EnableLocationSharing,
        Description = "If enabled, the block will attempt to determine the kiosk's location via location sharing geocode.",
        DefaultBooleanValue = false,
        Order = 2 )]

    [IntegerField(
        "Time to Cache Kiosk GeoLocation",
        Key = AttributeKey.TimeToCacheKioskLocation,
        Description = "Time in minutes to cache the coordinates of the kiosk. A value of zero (0) means cache forever. Default 20 minutes.",
        IsRequired = false,
        DefaultIntegerValue = 20,
        Order = 3 )]

    [BooleanField(
        "Enable Kiosk Match By Name",
        Key = AttributeKey.EnableKioskMatchByName,
        Description = "Enable a kiosk match by computer name by doing reverse IP lookup to get computer name based on IP address",
        DefaultBooleanValue = false,
        Order = 4 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "b252d7df-d0c1-4126-a36a-f0b1e2063372" )]
    [Rock.SystemGuid.BlockTypeGuid( "d42352a2-c48d-443b-a51d-31ea4ce0c5a4" )]
    public class CheckInKioskSetup : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string KioskPage = "KioskPage";
            public const string AllowManualSetup = "AllowManualSetup";
            public const string EnableLocationSharing = "EnableLocationSharing";
            public const string TimeToCacheKioskLocation = "TimeToCacheKioskLocation";
            public const string EnableKioskMatchByName = "EnableKioskMatchByName";
        }

        private static class PageParameterKey
        {
            public const string KioskId = "KioskId";
            public const string CheckinConfigId = "CheckinConfigId";
            public const string GroupTypeIds = "GroupTypeIds";
            public const string GroupIds = "GroupIds";
            public const string Theme = "Theme";
        }

        #endregion

        #region Fields

        /// <summary>
        /// The web host environment for this block.
        /// </summary>
        private readonly IWebHostEnvironment _environment;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInKioskSetup"/> class.
        /// </summary>
        /// <param name="environment">The environment.</param>
        public CheckInKioskSetup( IWebHostEnvironment environment )
        {
            _environment = environment;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override async Task<object> GetObsidianBlockInitializationAsync()
        {
            RequestContext.Response.AddCssLink( RequestContext.ResolveRockUrl( "~/Styles/Blocks/Checkin/CheckInKiosk.css" ), true );

            var director = new CheckInDirector( RockContext );

            var kioskConfiguration = await GetConfigurationByUrlAsync( director )
                ?? await GetConfigurationByIpOrNameAsync( director );

            return new
            {
                KioskConfiguration = kioskConfiguration,
                IsManualSetupAllowed = GetAttributeValue( AttributeKey.AllowManualSetup ).AsBoolean(),
                IsConfigureByLocationEnabled = GetAttributeValue( AttributeKey.EnableLocationSharing ).AsBoolean(),
                GeoLocationCacheInMinutes = GetAttributeValue( AttributeKey.TimeToCacheKioskLocation ).AsInteger(),
                Campuses = GetCampusesAndKiosks(),
                CurrentTheme = PageParameter( PageParameterKey.Theme )?.ToLower()
                    .IfEmpty( PageCache.Layout?.Site?.Theme?.ToLower() ),
                Templates = director.GetConfigurationTemplateBags(),
                Themes = GetThemes(),
                SavedConfigurations = GetSavedConfigurations(),
                KioskPageRoute = this.GetLinkedPageUrl( AttributeKey.KioskPage )
            };
        }

        /// <summary>
        /// Gets the kiosk bag that represents the specified kiosk device.
        /// </summary>
        /// <param name="kiosk">The kiosk device.</param>
        /// <returns>A new instance of <see cref="KioskBag"/>.</returns>
        internal static WebKioskBag GetKioskBag( DeviceCache kiosk )
        {
            var bag = new WebKioskBag
            {
                Id = kiosk.IdKey,
                IdNumber = kiosk.Id,
                Name = kiosk.Name,
                Type = kiosk.KioskType,
                IsCameraEnabled = kiosk.HasCamera,
                IsRegistrationModeEnabled = kiosk.GetAttributeValue( "core_device_RegistrationMode" ).AsBoolean()
            };

            return bag;
        }

        /// <summary>
        /// Gets the campuses and associated kiosks that can be selected during
        /// manual configuration.
        /// </summary>
        /// <returns>A collection of <see cref="CampusBag"/> objects.</returns>
        private List<CampusBag> GetCampusesAndKiosks()
        {
            var kioskDeviceTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK.AsGuid(), RockContext )?.Id;

            if ( !kioskDeviceTypeValueId.HasValue )
            {
                throw new Exception( "Device type Check-in Kiosk defined value not found." );
            }

            var campuses = CampusCache.All( RockContext )
                .Where( c => c.IsActive == true )
                .OrderBy( c => c.Order )
                .ToList();

            var kiosks = DeviceCache.All( RockContext )
                .Where( k => k.IsActive && k.DeviceTypeValueId == kioskDeviceTypeValueId.Value )
                .OrderBy( k => k.Name )
                .Select( k => new
                {
                    Kiosk = k,
                    CampusId = k.GetCampusId() ?? 0
                } )
                .GroupBy( k => k.CampusId )
                .ToDictionary( k => k.Key, k => k.ToList() );

            var campusBags = new List<CampusBag>();

            if ( kiosks.ContainsKey( 0 ) )
            {
                campusBags.Add( new CampusBag
                {
                    Id = string.Empty,
                    Name = string.Empty,
                    Kiosks = kiosks[0].Select( k => GetKioskBag( k.Kiosk ) ).ToList()
                } );
            }

            foreach ( var campus in campuses )
            {
                var campusKiosks = kiosks.GetValueOrNull( campus.Id );

                campusBags.Add( new CampusBag
                {
                    Id = campus.IdKey,
                    Name = campus.Name,
                    Kiosks = campusKiosks?.Select( k => GetKioskBag( k.Kiosk ) ).ToList()
                        ?? new List<WebKioskBag>()
                } );
            }

            return campusBags;
        }

        /// <summary>
        /// Gets all the themes that can be used for check-in.
        /// </summary>
        /// <returns>A collection of <see cref="ListItemBag"/> objects.</returns>
        private List<ListItemBag> GetThemes()
        {
            var di = new DirectoryInfo( Path.Combine( _environment.WebRootPath, "Themes" ) );

            return di.EnumerateDirectories()
                .OrderBy( d => d.Name )
                .Select( d => new ListItemBag
                {
                    Value = d.Name.ToLower(),
                    Text = d.Name.SplitCase()
                } )
                .ToList();
        }

        /// <summary>
        /// Attempts to configure the kiosk from query string parameters in
        /// the URL.
        /// </summary>
        /// <param name="director">The check-in director to use.</param>
        /// <returns>An instance of <see cref="KioskConfigurationBag"/> or <c>null</c>.</returns>
        private async Task<KioskConfigurationBag> GetConfigurationByUrlAsync( CheckInDirector director )
        {
            var urlKioskId = PageParameter( PageParameterKey.KioskId ).AsIntegerOrNull();
            var urlTemplateId = PageParameter( PageParameterKey.CheckinConfigId ).AsIntegerOrNull();
            var urlAreaIds = ( PageParameter( PageParameterKey.GroupTypeIds ) ?? string.Empty ).SplitDelimitedValues().AsIntegerList();

            // Get all the data from cache.
            var urlKiosk = urlKioskId.HasValue
                ? DeviceCache.Get( urlKioskId.Value )
                : null;
            var urlAreas = urlAreaIds.Select( id => GroupTypeCache.Get( id, RockContext ) )
                .Where( gt => gt != null )
                .ToList();
            ConfigurationTemplateBag urlTemplate = null;

            if ( urlTemplateId.HasValue )
            {
                var templateGroupType = GroupTypeCache.Get( urlTemplateId.Value, RockContext );

                if ( templateGroupType != null )
                {
                    urlTemplate = director.GetConfigurationTemplateBag( templateGroupType );
                }
            }

            /*
                2021-04-30 MSB
                There is a route that supports not passing in the check-in type
                id. If that route is used we need to try to get the check-in
                type id from the selected group types.
            */
            if ( urlKiosk != null && urlAreas.Any() && urlTemplate == null )
            {
                urlTemplate = GetPrimaryTemplate( director, urlAreas );
            }

            // Need to display the admin UI if Rock didn't find the check-in type.
            if ( urlTemplate == null )
            {
                return null;
            }

            /*
                2020-09-10 MDP
                If both PageParameterKey.CheckinConfigId and PageParameterKey.GroupTypeIds
                are specified, set the local device configuration from those.
                Then if PageParameterKey.KioskId is also specified set the
                KioskId from that, otherwise determine it from the IP Address.
                See https://app.asana.com/0/1121505495628584/1191546188992881/f
            */

            // If the kiosk device ID wasn't provided in the URL attempt
            // to get it using the IPAddress.
            if ( urlAreaIds.Any() && urlKiosk == null )
            {
                urlKiosk = await GetKioskFromIpOrNameAsync();
            }

            // If we didn't get everything, then we can't auto-configure.
            if ( urlKiosk == null || urlTemplate == null || !urlAreas.Any() )
            {
                return null;
            }

            return new KioskConfigurationBag
            {
                Kiosk = GetKioskBag( urlKiosk ),
                Template = urlTemplate,
                Areas = urlAreas.Select( a => new CheckInItemBag
                {
                    Id = a.IdKey,
                    Name = a.Name
                } ).ToList()
            };
        }

        /// <summary>
        /// Attempts to find the Device record for this kiosk by looking for a
        /// matching Device by IP Address, and optional host name if it can't
        /// be found from IP Address.
        /// </summary>
        /// <returns>An instance of <see cref="DeviceCache"/> or <c>null</c>.</returns>
        private Task<DeviceCache> GetKioskFromIpOrNameAsync()
        {
            var checkInDeviceTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK.AsGuid(), RockContext ).Id;
            var enableNameLookup = GetAttributeValue( AttributeKey.EnableKioskMatchByName ).AsBoolean();

            if ( enableNameLookup )
            {
                return DeviceCache.GetByIPAddressOrNameAsync( RequestContext.ClientInformation.IpAddress, checkInDeviceTypeId, RockContext );
            }
            else
            {
                return Task.FromResult( DeviceCache.GetByIPAddress( RequestContext.ClientInformation.IpAddress, checkInDeviceTypeId, RockContext ) );
            }
        }

        /// <summary>
        /// Attempts to match a known kiosk based on the IP address of the client.
        /// </summary>
        /// <param name="director">The check-in director for this operation.</param>
        /// <returns>The configuration object or <c>null</c> if it could not be determined.</returns>
        private async Task<KioskConfigurationBag> GetConfigurationByIpOrNameAsync( CheckInDirector director )
        {
            var kiosk = await GetKioskFromIpOrNameAsync();

            if ( kiosk == null )
            {
                return null;
            }

            return GetKioskConfiguration( director, kiosk );
        }

        /// <summary>
        /// Gets the primary configuration template to use from the list
        /// of areas.
        /// </summary>
        /// <param name="director">The check-in director for this operation.</param>
        /// <param name="areas">The areas that will be used to determine the configuration template.</param>
        /// <returns>An instance of <see cref="GroupTypeCache"/> or <c>null</c>.</returns>
        private ConfigurationTemplateBag GetPrimaryTemplate( CheckInDirector director, ICollection<GroupTypeCache> areas )
        {
            foreach ( var groupType in areas )
            {
                var template = groupType.GetCheckInConfigurationType();

                if ( template != null )
                {
                    return director.GetConfigurationTemplateBag( template );
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the automatic configuration for the specified kiosk.
        /// </summary>
        /// <param name="director">The check-in director for this operation.</param>
        /// <param name="kiosk">The kiosk whose configuration should be retrieved.</param>
        /// <returns>An instance of <see cref="KioskConfigurationBag"/> if the kiosk was valid; otherwise <c>null</c>.</returns>
        private KioskConfigurationBag GetKioskConfiguration( CheckInDirector director, DeviceCache kiosk )
        {
            var areas = director.GetKioskAreas( kiosk );
            var template = GetPrimaryTemplate( director, areas );

            if ( template == null )
            {
                return null;
            }

            var config = new KioskConfigurationBag
            {
                Kiosk = GetKioskBag( kiosk ),
                Areas = areas.Select( a => new CheckInItemBag
                {
                    Id = a.IdKey,
                    Name = a.Name
                } ).ToList(),
                Template = template
            };

            return config;
        }

        /// <summary>
        /// Returns a kiosk based on finding a geo location match for the
        /// given latitude and longitude.
        /// </summary>
        /// <param name="latitude">The latitude of the physical device.</param>
        /// <param name="longitude">The longitude of the physical device.</param>
        /// <returns></returns>
        private DeviceCache GetCurrentKioskByGeoFencing( double latitude, double longitude )
        {
            var checkInDeviceTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ).Id;

            return DeviceCache.GetByGeocode( latitude, longitude, checkInDeviceTypeId, RockContext );
        }

        /// <summary>
        /// Gets the saved check-in configurations.
        /// </summary>
        /// <returns>A collection of <see cref="SavedCheckInConfigurationBag"/> objects.</returns>
        private IReadOnlyCollection<SavedCheckInConfigurationBag> GetSavedConfigurations()
        {
            var savedConfigurationCache = DefinedTypeCache.Get( SystemGuid.DefinedType.SAVED_CHECKIN_CONFIGURATIONS.AsGuid(), RockContext );

            if ( savedConfigurationCache == null )
            {
                return Array.Empty<SavedCheckInConfigurationBag>();
            }

            return savedConfigurationCache.DefinedValues
                .Select( GetSavedConfigurationBag )
                .ToList();
        }

        /// <summary>
        /// Gets the saved configuration bag from the defined value.
        /// </summary>
        /// <param name="definedValue">The defined value.</param>
        /// <returns>A new <see cref="SavedCheckInConfigurationBag"/> instance.</returns>
        private SavedCheckInConfigurationBag GetSavedConfigurationBag( DefinedValueCache definedValue )
        {
            return new SavedCheckInConfigurationBag
            {
                Id = definedValue.IdKey,
                Name = definedValue.Value,
                Description = definedValue.Description,
                Campuses = definedValue.GetAttributeValue( "Campuses" )
                    .SplitDelimitedValues()
                    .AsGuidList()
                    .Select( g => CampusCache.Get( g, RockContext ) )
                    .Where( c => c != null )
                    .ToListItemBagList(),
                Settings = definedValue.GetAttributeValue( "SettingsJson" ).FromJsonOrNull<SavedCheckInConfigurationSettingsBag>()
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// A request from the client to determine the kiosk configuration from
        /// the geo-location specified by the latitude and longitude.
        /// </summary>
        /// <param name="latitude">The latitude of the physical device that wants to be a kiosk.</param>
        /// <param name="longitude">The longitude of the physical device that wants to be a kiosk.</param>
        /// <returns>The result of the operation.</returns>
        [BlockAction]
        public BlockActionResult GetConfigurationByLocation( double latitude, double longitude )
        {
            var kiosk = GetCurrentKioskByGeoFencing( latitude, longitude );

            if ( kiosk == null )
            {
                return ActionNotFound( GetAttributeValue( AttributeKey.AllowManualSetup ).AsBoolean()
                    ? "We could not automatically determine your configuration."
                    : "You are too far. Try again later." );
            }

            var director = new CheckInDirector( RockContext );
            var config = GetKioskConfiguration( director, kiosk );

            if ( config == null )
            {
                return ActionNotFound( "Invalid kiosk configuration." );
            }

            return ActionOk( config );
        }

        /// <summary>
        /// Gets the promotion list defined for the template and kiosk.
        /// </summary>
        /// <param name="templateGuid">The check-in template unique identifier.</param>
        /// <param name="kioskGuid">The kiosk unique identifier.</param>
        /// <returns>A list of <see cref="PromotionBag"/> objects.</returns>
        [BlockAction]
        public BlockActionResult GetPromotionList( Guid templateGuid, Guid kioskGuid )
        {
            var kiosk = DeviceCache.Get( kioskGuid, RockContext );
            var contentChannel = new ContentChannelService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( cc => cc.Items )
                .Where( cc => cc.Id == 8 )
                .FirstOrDefault();

            if ( kiosk == null || contentChannel == null )
            {
                return ActionOk( new List<PromotionBag>() );
            }

            contentChannel.Items.LoadAttributes( RockContext );

            var now = RockDateTime.Now;
            var campusId = kiosk.GetCampusId();
            var campusGuid = campusId.HasValue
                ? CampusCache.Get( campusId.Value )?.Guid ?? Guid.Empty
                : Guid.Empty;

            // Filter items by date.
            var promotionItems = contentChannel.Items
                .Where( item => item.StartDateTime <= now
                    && ( !item.ExpireDateTime.HasValue || item.ExpireDateTime >= now ) );

            // Filter items by approval.
            if ( contentChannel.RequiresApproval )
            {
                promotionItems = promotionItems.Where( item => item.Status == ContentChannelItemStatus.Approved );
            }

            // Filter items by kiosk campus.
            promotionItems = promotionItems
                .Where( item => item.GetAttributeValue( "Campuses" ).IsNullOrWhiteSpace()
                    || item.GetAttributeValue( "Campuses" ).SplitDelimitedValues().AsGuidList().Contains( campusGuid ) );

            // Order the items.
            promotionItems = contentChannel.ItemsManuallyOrdered
                ? contentChannel.Items.OrderBy( item => item.Order )
                : contentChannel.Items.OrderBy( item => item.StartDateTime );

            var promotions = promotionItems
                .Select( item => new PromotionBag
                {
                    Url = $"/GetImage.ashx?guid={item.GetAttributeValue( "Image" )}",
                    Duration = item.GetAttributeValue( "DisplayDuration" ).AsIntegerOrNull() ?? 15
                } )
                .ToList();

            return ActionOk( promotions );
        }

        /// <summary>
        /// Saves the configuration as a new saved configuration item.
        /// </summary>
        /// <param name="box">The box that contains the configuration data.</param>
        /// <returns>A response that indicates success or failure.</returns>
        [BlockAction]
        public BlockActionResult SaveConfiguration( ValidPropertiesBox<SavedCheckInConfigurationBag> box )
        {
            var savedConfigurationDefinedTypeId = DefinedTypeCache.Get( SystemGuid.DefinedType.SAVED_CHECKIN_CONFIGURATIONS.AsGuid(), RockContext )?.Id;

            if ( box.Bag.Id.IsNotNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Editing existing saved configurations is not supported." );
            }

            if ( !savedConfigurationDefinedTypeId.HasValue )
            {
                return ActionInternalServerError( "Saved configuration data does not exist in the database yet." );
            }

            var definedValueService = new DefinedValueService( RockContext );
            var definedValue = new DefinedValue
            {
                DefinedTypeId = savedConfigurationDefinedTypeId.Value
            };

            definedValue.LoadAttributes( RockContext );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => definedValue.Value = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => definedValue.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.Campuses ),
                () => definedValue.SetAttributeValue( "Campuses", box.Bag.Campuses.Select( c => c.Value ).JoinStrings( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.Settings ),
                () => definedValue.SetAttributeValue( "SettingsJson", box.Bag.Settings.ToJson() ) );

            definedValueService.Add( definedValue );

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                definedValue.SaveAttributeValues( RockContext );
            } );

            return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
        }

        #endregion

        private class CampusBag : CheckInItemBag
        {
            public List<WebKioskBag> Kiosks { get; set; }
        }
    }
}
