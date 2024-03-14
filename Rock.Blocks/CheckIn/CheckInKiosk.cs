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
        "Enable Kiosk Match By Name",
        Key = AttributeKey.EnableKioskMatchByName,
        Description = "Enable a kiosk match by computer name by doing reverseIP lookup to get computer name based on IP address",
        DefaultBooleanValue = false,
        Order = 8 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "b208cafe-2194-4308-aa52-a920c516805a" )]
    [Rock.SystemGuid.BlockTypeGuid( "a27fd0aa-67ee-44c3-9e5f-3289c6a210f3" )]
    public class CheckInKiosk : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string EnableKioskMatchByName = "EnableKioskMatchByName";
        }

        #endregion

        #region Fields

        /// <summary>
        /// The web host environment for this block.
        /// </summary>
        private readonly IWebHostEnvironment _environment;

        #endregion

        #region Constructors

        public CheckInKiosk( IWebHostEnvironment environment )
        {
            _environment = environment;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var config = GetConfigurationByIpOrName( rockContext );

                return new
                {
                    Campuses = GetCampusesAndKiosks( rockContext ),
                    Themes = GetThemes(),
                    DefaultTheme = PageCache.Layout?.Site?.Theme
                };
            }
        }

        /// <summary>
        /// Gets the campuses and associated kiosks that can be selected during
        /// manual configuration.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A collection of <see cref="CampusBag"/> objects.</returns>
        private List<CampusBag> GetCampusesAndKiosks( RockContext rockContext )
        {
            var kioskDeviceTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK.AsGuid(), rockContext )?.Id;

            if ( !kioskDeviceTypeValueId.HasValue )
            {
                throw new Exception( "Device type Check-in Kiosk defined value not found." );
            }

            var campuses = CampusCache.All( rockContext )
                .Where( c => c.IsActive == true )
                .OrderBy( c => c.Order )
                .ToList();

            var kiosks = DeviceCache.All( rockContext )
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
                    Kiosks = kiosks[0].Select( k => new CheckInItemBag
                    {
                        Guid = k.Kiosk.Guid,
                        Name = k.Kiosk.Name
                    } )
                    .ToList()
                } );
            }

            foreach ( var campus in campuses )
            {
                var campusKiosks = kiosks.GetValueOrNull( campus.Id );

                campusBags.Add( new CampusBag
                {
                    Guid = campus.Guid,
                    Name = campus.Name,
                    Kiosks = campusKiosks?.Select( k => new CheckInItemBag
                    {
                        Guid = k.Kiosk.Guid,
                        Name = k.Kiosk.Name
                    } )
                    .ToList() ?? new List<CheckInItemBag>()
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
                    Text = d.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Attempts to find the Device record for this kiosk by looking for a
        /// matching Device by IP Address, and optional host name if it can't
        /// be found from IP Address.
        /// </summary>
        /// <returns>An instance of <see cref="DeviceCache"/> or <c>null</c>.</returns>
        private DeviceCache GetKioskFromIpOrName()
        {
            var checkInDeviceTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ).Id;
            var enableNameLookup = GetAttributeValue( AttributeKey.EnableKioskMatchByName ).AsBoolean();

            if ( enableNameLookup )
            {
                // TODO: GetByIPAddressOrNameAsync
                return DeviceCache.GetByIPAddress( RequestContext.ClientInformation.IpAddress, checkInDeviceTypeId );
            }
            else
            {
                return DeviceCache.GetByIPAddress( RequestContext.ClientInformation.IpAddress, checkInDeviceTypeId );
            }
        }

        /// <summary>
        /// Attempts to match a known kiosk based on the IP address of the client.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>The configuration object or <c>null</c> if it could not be determined.</returns>
        private object GetConfigurationByIpOrName( RockContext rockContext )
        {
            var kiosk = GetKioskFromIpOrName();

            if ( kiosk == null )
            {
                return null;
            }

            var director = new CheckInDirector( rockContext );
            var areas = director.GetKioskAreas( kiosk );
            var template = GetPrimaryTemplate( areas );

            if ( template == null )
            {
                return null;
            }

            var config = new
            {
                Kiosk = kiosk.ToListItemBag(),
                Areas = areas.ToListItemBagList(),
                Template = template.ToListItemBag()
            };

            return config;
        }

        /// <summary>
        /// Gets the primary configuration template to use from the list
        /// of areas.
        /// </summary>
        /// <param name="areas">The areas that will be used to determine the configuration template.</param>
        /// <returns>An instance of <see cref="GroupTypeCache"/> or <c>null</c>.</returns>
        private GroupTypeCache GetPrimaryTemplate( ICollection<GroupTypeCache> areas )
        {
            foreach ( var groupType in areas )
            {
                var template = groupType.GetCheckInConfigurationType();

                if ( template != null )
                {
                    return template;
                }
            }

            return null;
        }

        #endregion

        #region Block Actions

        #endregion

        private class CampusBag : CheckInItemBag
        {
            public List<CheckInItemBag> Kiosks { get; set; }
        }
    }
}
