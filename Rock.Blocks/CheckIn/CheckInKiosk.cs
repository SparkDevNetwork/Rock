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

        private static class PageParameterKey
        {
            public const string KioskId = "KioskId";
            public const string CheckinConfigId = "CheckinConfigId";
            public const string GroupTypeIds = "GroupTypeIds";
            public const string GroupIds = "GroupIds";
            //public const string FamilyId = "FamilyId";
            //public const string CameraIndex = "CameraIndex";
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
            var urlGroupIds = ( PageParameter( PageParameterKey.GroupIds ) ?? string.Empty ).SplitDelimitedValues().AsIntegerList();

            // Rock check-in will set configuration using Group identifiers or
            // GroupType identifiers but not both. This is to remove the
            // possiblilty of a Group/GroupType mismatch. Check for groups
            // first since the GroupTypes of those groups will overwrite the
            // URL provided GroupType identifiers.
            if ( urlGroupIds.Any() )
            {
                // Determine the GroupType(s) from the provided Group identifiers
                // and add them to the configuration, replacing explicit ones if
                // they were provided.
                urlAreaIds = new GroupService( RockContext ).Queryable()
                    .Where( g => urlGroupIds.Contains( g.Id ) )
                    .Select( g => g.GroupTypeId )
                    .Distinct()
                    .ToList();

                // TODO: Do we need this, see Asana task https://app.asana.com/0/1198840255983422/1206332550528719
                // this.LocalDeviceConfig.CurrentGroupIds = urlGroupIds;
            }

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
                    Guid = a.Guid,
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
                    Guid = a.Guid,
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
