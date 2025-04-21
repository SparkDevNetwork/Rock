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
using System.IO;
using System.Linq;
using System.Net;

using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;

using RestSharp;
using RestSharp.Extensions;

using Rock.Model;
using Rock.Observability;
using Rock.Web.Cache;

namespace Rock.Net.Geolocation
{
    /// <summary>
    /// IP Geolocation Lookup helper.
    /// </summary>
    internal sealed class IpGeoLookup
    {
        #region Fields

        private const string CacheKeyPrefix = "Rock.IpGeolocation-";

        private const string UpdateUrl = "https://rockrms.blob.core.windows.net/resources/ip-geo/ip-geo-current.mmdb";

        private static readonly string _directoryPath = System.Web.Hosting.HostingEnvironment.MapPath( "~/App_Data/Geolocation" );
        private static readonly string _eTagPath = Path.Combine( _directoryPath, "ip-geo-current-etag.txt" );
        private static readonly string _databasePath = Path.Combine( _directoryPath, "ip-geo-current.mmdb" );

        private readonly HashSet<string> _skipIpPrefixes = new HashSet<string>
        {
            "10.",
            "127.",
            "169.254.",
            "172.16.",
            "192.168.",
            "localhost",
            "::1",
            "fe80::"
        };

        private readonly object _updateLock = new object();

        private string _previousEtagValue;
        private DatabaseReader _locationDatabase;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Whether the database file exists on disk.
        /// </summary>
        private bool DatabaseFileExists => File.Exists( _databasePath );

        /// <summary>
        /// The region defined type cache.
        /// </summary>
        private DefinedTypeCache RegionDefinedTypeCache => DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.LOCATION_ADDRESS_STATE );

        /// <summary>
        /// The country defined type cache.
        /// </summary>
        private DefinedTypeCache CountryDefinedTypeCache => DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.LOCATION_COUNTRIES );

        #endregion Properties

        #region Singleton

        /// <summary>
        /// Singleton instance.
        /// </summary>
        private static readonly Lazy<IpGeoLookup> instance
            = new Lazy<IpGeoLookup>( () => new IpGeoLookup() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static IpGeoLookup Instance => instance.Value;

        /// <summary>
        /// Default constructor.
        /// </summary>
        private IpGeoLookup()
        {
            InitializeDatabase();
        }

        #endregion Singleton

        #region Public Methods

        /// <summary>
        /// Updates the geolocation database.
        /// </summary>
        /// <exception cref="IpGeoException">If unable to update database.</exception>
        public void UpdateDatabase()
        {
            lock ( _updateLock )
            {
                var exceptionPrefix = "Unable to update IP Geolocation Lookup database.";

                try
                {
                    var restClient = new RestClient( UpdateUrl );

                    // Start by requesting only the headers so we can compare the ETag.
                    var restRequest = new RestRequest( Method.HEAD );
                    var restResponse = restClient.Execute( restRequest );

                    if ( restResponse.StatusCode != HttpStatusCode.OK )
                    {
                        throw new IpGeoException( $"{exceptionPrefix} Update URL: '{UpdateUrl}'. Status code returned: '{restResponse.StatusCode}'.{( restResponse.ErrorMessage.IsNotNullOrWhiteSpace() ? $" Error message: {restResponse.ErrorMessage}" : string.Empty )}" );
                    }

                    var eTagHeaderValue = restResponse.Headers
                        ?.FirstOrDefault( h => h.Name == "ETag" )
                        ?.Value?.ToString();

                    if ( this.DatabaseFileExists && eTagHeaderValue.IsNotNullOrWhiteSpace() )
                    {
                        // Check the latest ETag header value against the previous in-memory
                        // value to see if we already have the latest database.
                        if ( eTagHeaderValue == _previousEtagValue )
                        {
                            return;
                        }

                        // Check the latest ETag header value against the previously-saved
                        // value to see if we already have the latest database.
                        if ( File.Exists( _eTagPath ) && eTagHeaderValue == File.ReadAllText( _eTagPath ) )
                        {
                            _previousEtagValue = eTagHeaderValue;
                            return;
                        }
                    }

                    // Ensure the directory exists.
                    Directory.CreateDirectory( _directoryPath );

                    // Set client to timeout after 5 minutes.
                    restClient.Timeout = 3000;
                    restRequest = new RestRequest( Method.GET );

                    // Download the latest database.
                    using ( var activity = ObservabilityHelper.StartActivity( "GEO: Downloading Geolocation Database" ) )
                    {
                        restClient.DownloadData( restRequest ).SaveAs( _databasePath );
                    }

                    // Save the latest ETag value.
                    if ( eTagHeaderValue.IsNotNullOrWhiteSpace() )
                    {
                        _previousEtagValue = eTagHeaderValue;
                        File.WriteAllText( _eTagPath, eTagHeaderValue );
                    }
                }
                catch ( Exception ex )
                {
                    if ( ex is IpGeoException )
                    {
                        throw;
                    }

                    throw new IpGeoException( exceptionPrefix, ex );
                }

                InitializeDatabase();
            }
        }

        /// <summary>
        /// Gets geolocation data for the provided IP address.
        /// </summary>
        /// <param name="ipAddress">The IP address for which to get geolocation data.</param>
        /// <returns>Geolocation data.</returns>
        public IpGeolocation GetGeolocation( string ipAddress )
        {
            if ( _locationDatabase == null || ipAddress.IsNullOrWhiteSpace() )
            {
                return null;
            }

            IpGeolocation ipGeolocation = null;
            var cacheKey = $"{CacheKeyPrefix}{ipAddress}";

            try
            {
                ipGeolocation = RockCache.Get( cacheKey ) as IpGeolocation;
                if ( ipGeolocation != null )
                {
                    return ipGeolocation;
                }

                if ( _skipIpPrefixes.Any( prefix => ipAddress.StartsWith( prefix ) ) )
                {
                    ipGeolocation = new IpGeolocation
                    {
                        IpAddress = IpGeolocationErrorCode.ReservedAddress.ToString()
                    };
                }
                else
                {
                    var cityResponse = _locationDatabase.City( ipAddress );
                    if ( cityResponse == null )
                    {
                        ipGeolocation = new IpGeolocation
                        {
                            IpAddress = IpGeolocationErrorCode.InvalidAddress.ToString()
                        };
                    }
                    else
                    {
                        var region = cityResponse.Subdivisions?.FirstOrDefault();

                        ipGeolocation = new IpGeolocation
                        {
                            IpAddress = ipAddress,
                            City = cityResponse.City?.Name,
                            RegionName = region?.Name,
                            RegionCode = region?.IsoCode,
                            CountryCode = cityResponse.Country?.IsoCode,
                            PostalCode = cityResponse.Postal?.Code,
                            Latitude = cityResponse.Location?.Latitude,
                            Longitude = cityResponse.Location?.Longitude
                        };

                        if ( this.RegionDefinedTypeCache != null && ipGeolocation.RegionCode.IsNotNullOrWhiteSpace() )
                        {
                            ipGeolocation.RegionValueId = this.RegionDefinedTypeCache.GetDefinedValueFromValue( ipGeolocation.RegionCode )?.Id;
                        }

                        if ( this.CountryDefinedTypeCache != null && ipGeolocation.CountryCode.IsNotNullOrWhiteSpace() )
                        {
                            ipGeolocation.CountryValueId = this.CountryDefinedTypeCache.GetDefinedValueFromValue( ipGeolocation.CountryCode )?.Id;
                        }
                    }
                }
            }
            catch ( Exception ex ) when ( ex is AddressNotFoundException || ex is GeoIP2Exception )
            {
                ipGeolocation = new IpGeolocation
                {
                    IpAddress = IpGeolocationErrorCode.InvalidAddress.ToString()
                };
            }
            catch
            {
                // Fail silently.
            }

            if ( ipGeolocation != null )
            {
                var now = RockDateTime.Now;
                ipGeolocation.LookupDateTime = now;
                RockCache.AddOrUpdate( cacheKey, null, ipGeolocation, now.AddSeconds( 300 ) );
            }

            return ipGeolocation;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Initializes the geolocation database reader.
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                if ( !this.DatabaseFileExists )
                {
                    return;
                }

                using ( var activity = ObservabilityHelper.StartActivity( "GEO: Initializing Geolocation Database" ) )
                {
                    _locationDatabase = new DatabaseReader( _databasePath, MaxMind.Db.FileAccessMode.Memory );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new IpGeoException( "Unable to initialize IP Geolocation Lookup database.", ex ) );
            }
        }

        #endregion Private Methods
    }
}
