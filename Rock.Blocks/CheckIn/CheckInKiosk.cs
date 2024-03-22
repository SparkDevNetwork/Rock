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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;

using Rock.Attribute;
using Rock.CheckIn.v2;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.CheckIn;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn
{
    /// <summary>
    /// The standard Rock block for performing check-in at a kiosk.
    /// </summary>

    [DisplayName( "Check-in Kiosk" )]
    [Category( "Check-in" )]
    [Description( "The standard Rock block for performing check-in at a kiosk." )]
    [IconCssClass( "fa fa-clipboard-check" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField(
        "Allow Manual Setup",
        Key = AttributeKey.AllowManualSetup,
        Description = "If enabled, the block will allow the kiosk to be setup manually if it was not set via other means.",
        Category = AttributeCategory.Configuration,
        DefaultBooleanValue = true,
        Order = 0 )]

    [BooleanField(
        "Enable Location Sharing",
        Key = AttributeKey.EnableLocationSharing,
        Description = "If enabled, the block will attempt to determine the kiosk's location via location sharing geocode.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Configuration,
        Order = 1 )]

    [IntegerField(
        "Time to Cache Kiosk GeoLocation",
        Key = AttributeKey.TimeToCacheKioskLocation,
        Description = "Time in minutes to cache the coordinates of the kiosk. A value of zero (0) means cache forever. Default 20 minutes.",
        IsRequired = false,
        DefaultIntegerValue = 20,
        Category = AttributeCategory.Configuration,
        Order = 2 )]

    [BooleanField(
        "Enable Kiosk Match By Name",
        Key = AttributeKey.EnableKioskMatchByName,
        Description = "Enable a kiosk match by computer name by doing reverse IP lookup to get computer name based on IP address",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Configuration,
        Order = 3 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "b208cafe-2194-4308-aa52-a920c516805a" )]
    [Rock.SystemGuid.BlockTypeGuid( "a27fd0aa-67ee-44c3-9e5f-3289c6a210f3" )]
    public class CheckInKiosk : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string AllowManualSetup = "AllowManualSetup";
            public const string EnableLocationSharing = "EnableLocationSharing";
            public const string TimeToCacheKioskLocation = "TimeToCacheKioskLocation";
            public const string EnableKioskMatchByName = "EnableKioskMatchByName";
        }

        private static class AttributeCategory
        {
            public const string Configuration = "Configuration";
        }

        //private static class PageParameterKey
        //{
        //    public const string KioskId = "KioskId";
        //    public const string CheckinConfigId = "CheckinConfigId";
        //    public const string GroupTypeIds = "GroupTypeIds";
        //    public const string GroupIds = "GroupIds";
        //    public const string FamilyId = "FamilyId";
        //    public const string CameraIndex = "CameraIndex";
        //    public const string Theme = "Theme";
        //}

        #endregion

        #region Fields

        /// <summary>
        /// The web host environment for this block.
        /// </summary>
        private readonly IWebHostEnvironment _environment;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInKiosk"/> class.
        /// </summary>
        /// <param name="environment">The environment.</param>
        public CheckInKiosk( IWebHostEnvironment environment )
        {
            _environment = environment;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override async Task<object> GetObsidianBlockInitializationAsync()
        {
            var director = new CheckInDirector( RockContext );
            var config = await GetConfigurationByIpOrNameAsync( director );

            return new
            {
                IsManualSetupAllowed = GetAttributeValue( AttributeKey.AllowManualSetup ).AsBoolean(),
                IsConfigureByLocationEnabled = GetAttributeValue( AttributeKey.EnableLocationSharing ).AsBoolean(),
                GeoLocationCacheInMinutes = GetAttributeValue( AttributeKey.TimeToCacheKioskLocation ).AsInteger(),
                Campuses = GetCampusesAndKiosks(),
                DefaultTheme = PageCache.Layout?.Site?.Theme,
                Templates = director.GetConfigurationTemplateBags(),
                Themes = GetThemes()
            };
        }

        /// <summary>
        /// Gets the kiosk bag that represents the specified kiosk device.
        /// </summary>
        /// <param name="kiosk">The kiosk device.</param>
        /// <returns>A new instance of <see cref="KioskBag"/>.</returns>
        private KioskBag GetKioskBag( DeviceCache kiosk )
        {
            var bag = new KioskBag
            {
                Guid = kiosk.Guid,
                Name = kiosk.Name,
                Type = kiosk.KioskType,
                IsCameraEnabled = kiosk.HasCamera,
                PrintFrom = kiosk.PrintFrom,
                PrintTo = kiosk.PrintToOverride,
                IsRegistrationModeEnabled = kiosk.GetAttributeValue( "core_device_RegistrationMode" ).AsBoolean()
            };

            if ( kiosk.PrinterDeviceId.HasValue )
            {
                var printer = DeviceCache.Get( kiosk.PrinterDeviceId.Value, RockContext );

                if ( printer != null )
                {
                    bag.Printer = new CheckInItemBag
                    {
                        Guid = printer.Guid,
                        Name = printer.Name
                    };
                }
            }

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
                    Guid = Guid.Empty,
                    Name = string.Empty,
                    Kiosks = kiosks[0].Select( k => GetKioskBag( k.Kiosk ) ).ToList()
                } );
            }

            foreach ( var campus in campuses )
            {
                var campusKiosks = kiosks.GetValueOrNull( campus.Id );

                campusBags.Add( new CampusBag
                {
                    Guid = campus.Guid,
                    Name = campus.Name,
                    Kiosks = campusKiosks?.Select( k => GetKioskBag( k.Kiosk ) ).ToList()
                        ?? new List<KioskBag>()
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
        private async Task<object> GetConfigurationByIpOrNameAsync( CheckInDirector director )
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
                    Guid = a.Guid,
                    Name = a.Name
                } ).ToList(),
                Template = template,
                Theme = new ListItemBag
                {
                    Value = PageCache.Layout.Site.Theme.ToLower(),
                    Text = PageCache.Layout.Site.Theme.SplitCase()
                }
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
        public DeviceCache GetCurrentKioskByGeoFencing( double latitude, double longitude )
        {
            var checkInDeviceTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ).Id;

            return DeviceCache.GetByGeocode( latitude, longitude, checkInDeviceTypeId, RockContext );
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

        #endregion

        private class CampusBag : CheckInItemBag
        {
            public List<KioskBag> Kiosks { get; set; }
        }
    }
}
