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
using System.Linq;
using System.Web.Http;

using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Personal Devices REST API
    /// </summary>
    public partial class PersonalDevicesController
    {
        /// <summary>
        /// Deletes the specified Block with extra logic to flush caches.
        /// </summary>
        /// <param name="macAddress">The MAC address of the device.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="devicePlatform">The device platform.</param>
        /// <param name="deviceVersion">The device version.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/PersonalDevices/UpdateByMACAddress/{macAddress}" )]
        public PersonalDevice UpdateByMACAddress( string macAddress, string deviceIdentifier = "", string devicePlatform = "", string deviceVersion = "", int? personAliasId = null )
        {
            var rockContext = new Data.RockContext();
            var service = new PersonalDeviceService( rockContext );

            // MAC address
            var personalDevice = service.GetByMACAddress( macAddress );
            if ( personalDevice == null )
            {
                personalDevice = new PersonalDevice();
                personalDevice.MACAddress = macAddress;
                service.Add( personalDevice );
            }

            // unique identifier
            if ( deviceIdentifier.IsNotNullOrWhitespace() )
            {
                personalDevice.DeviceUniqueIdentifier = deviceIdentifier;
            }

            // Platform
            if ( devicePlatform.IsNotNullOrWhitespace() )
            {
                var dt = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSONAL_DEVICE_PLATFORM.AsGuid() );
                DefinedValueCache dv = null;
                if ( dt != null )
                {
                    dv = dt.DefinedValues.FirstOrDefault( v => v.Value == devicePlatform );
                }
                if ( dv == null )
                {
                    dv = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSONAL_DEVICE_PLATFORM_OTHER.AsGuid() );
                }
                personalDevice.PlatformValueId = dv != null ? dv.Id : (int?)null;
            }

            // Version
            if ( deviceVersion.IsNotNullOrWhitespace() )
            {
                personalDevice.DeviceVersion = deviceVersion;
            }

            // Person
            if ( personAliasId.HasValue )
            {
                var person = new PersonAliasService( rockContext ).GetPerson( personAliasId.Value );
                if ( person != null )
                {
                    if ( personalDevice.PersonAlias == null || personalDevice.PersonAlias.PersonId != person.Id )
                    {
                        personalDevice.PersonAliasId = personAliasId.Value;
                    }

                    // Update any associated interaction records for the device that do not have an associated person.
                    if ( personalDevice.Id != 0 )
                    {
                        var interactionService = new InteractionService( rockContext );
                        foreach ( var interaction in interactionService.Queryable()
                            .Where( i =>
                                i.PersonalDeviceId == personalDevice.Id &&
                                !i.PersonAliasId.HasValue ) )
                        {
                            interaction.PersonAliasId = personAliasId.Value;
                        }
                    }
                }
            }

            rockContext.SaveChanges();

            return GetById( personalDevice.Id );
        }

    }
}
