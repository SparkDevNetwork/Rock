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

using Newtonsoft.Json;

using RestSharp;

using Rock.Data;
using Rock.Model;
using Rock.Store;
using Rock.Web.Cache;

namespace Rock.Utility
{

    /// <summary>
    /// Helper class used to send the spark link notice
    /// </summary>
    public static class SparkLinkHelper
    {
        /// <summary>
        /// Sends to spark.
        /// </summary>
        /// <returns></returns>
        public static List<Notification> SendToSpark( RockContext rockContext )
        {
            var notifications = new List<Notification>();

            var installedPackages = InstalledPackageService.GetInstalledPackages();

            var sparkLinkRequest = new SparkLinkRequestV2();
            sparkLinkRequest.RockInstanceId = Rock.Web.SystemSettings.GetRockInstanceId();
            sparkLinkRequest.VersionIds = installedPackages.Select( i => i.VersionId ).ToList();
            sparkLinkRequest.RockVersion = VersionInfo.VersionInfo.GetRockSemanticVersionNumber();

            var globalAttributes = GlobalAttributesCache.Get();
            sparkLinkRequest.OrganizationName = globalAttributes.GetValue( "OrganizationName" );
            sparkLinkRequest.PublicUrl = globalAttributes.GetValue( "PublicApplicationRoot" );

            sparkLinkRequest.NumberOfActiveRecords = new PersonService( rockContext ).Queryable( includeDeceased: false, includeBusinesses: false ).Count();

            sparkLinkRequest.PluginBlockTypes = new BlockTypeService( rockContext ).Queryable()
                .Where( bt => bt.Path.Contains( "~/Plugins" ) )
                .Select( bt => new PluginBlockType()
                {
                    CreatedDateTime = bt.CreatedDateTime,
                    ModifiedDateTime = bt.ModifiedDateTime,
                    Name = bt.Name,
                    Path = bt.Path,
                } ).ToList();

            // Set FileCreationTime and FileLastWriteTime
            foreach ( var item in sparkLinkRequest.PluginBlockTypes )
            {
                var itemPath = item.Path.Substring( 2 );
                var filePath = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, itemPath );
                var fileInfo = new FileInfo( filePath );

                if ( fileInfo.Exists )
                {
                    item.FileCreationTime = fileInfo.CreationTime;
                    item.FileLastWriteTime = fileInfo.LastWriteTime;
                }
            }

            // Fetch the organization address
            var organizationAddressLocationGuid = globalAttributes.GetValue( "OrganizationAddress" ).AsGuid();
            if ( !organizationAddressLocationGuid.Equals( Guid.Empty ) )
            {
                var location = new LocationService( rockContext ).Get( organizationAddressLocationGuid );
                if ( location != null )
                {
                    sparkLinkRequest.OrganizationLocation = new SparkLinkLocation( location );
                }
            }

            var sparkLinkRequestJson = JsonConvert.SerializeObject( sparkLinkRequest );

            var client = new RestClient( "https://www.rockrms.com/api/SparkLink/update" );
            //var client = new RestClient( "http://localhost:57822/api/SparkLink/update" );
            var request = new RestRequest( Method.POST );
            request.AddParameter( "application/json", sparkLinkRequestJson, ParameterType.RequestBody );
            IRestResponse response = client.Execute( request );
            if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
            {
                foreach ( var notification in JsonConvert.DeserializeObject<List<Notification>>( response.Content ) )
                {
                    notifications.Add( notification );
                }
            }

            if ( sparkLinkRequest.VersionIds.Any() )
            {
                client = new RestClient( "https://www.rockrms.com/api/Packages/VersionNotifications" );
                //client = new RestClient( "http://localhost:57822/api/Packages/VersionNotifications" );
                request = new RestRequest( Method.GET );
                request.AddParameter( "VersionIds", sparkLinkRequest.VersionIds.AsDelimited( "," ) );
                response = client.Execute( request );
                if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
                {
                    foreach ( var notification in JsonConvert.DeserializeObject<List<Notification>>( response.Content ) )
                    {
                        notifications.Add( notification );
                    }
                }
            }

            return notifications;
        }

        #region Helper Class

        /// <summary>
        /// 
        /// </summary>
        public class SparkLinkRequest
        {
            /// <summary>
            /// Gets or sets the rock instance identifier.
            /// </summary>
            /// <value>
            /// The rock instance identifier.
            /// </value>
            public Guid RockInstanceId { get; set; }

            /// <summary>
            /// Gets or sets the name of the organization.
            /// </summary>
            /// <value>
            /// The name of the organization.
            /// </value>
            public string OrganizationName { get; set; }

            /// <summary>
            /// Gets or sets the rock version.
            /// </summary>
            /// <value>
            /// The rock version.
            /// </value>
            public string RockVersion { get; set; }

            /// <summary>
            /// Gets or sets the version ids.
            /// </summary>
            /// <value>
            /// The version ids.
            /// </value>
            public List<int> VersionIds { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="Rock.Utility.SparkLinkHelper.SparkLinkRequest" />
        public class SparkLinkRequestV2 : SparkLinkRequest
        {
            /// <summary>
            /// Gets or sets the ip address.
            /// </summary>
            /// <value>
            /// The ip address.
            /// </value>
            public string IpAddress { get; set; }

            /// <summary>
            /// Gets or sets the public URL.
            /// </summary>
            /// <value>
            /// The public URL.
            /// </value>
            public string PublicUrl { get; set; }

            /// <summary>
            /// Gets or sets the organization location.
            /// </summary>
            /// <value>
            /// The organization location.
            /// </value>
            public SparkLinkLocation OrganizationLocation { get; set; }

            /// <summary>
            /// Gets or sets the number of active records.
            /// </summary>
            /// <value>
            /// The number of active records.
            /// </value>
            public int NumberOfActiveRecords { get; set; }

            /// <summary>
            /// Gets or sets the plugin block types.
            /// </summary>
            /// <value>
            /// The plugin block types.
            /// </value>
            public List<PluginBlockType> PluginBlockTypes { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class SparkLinkLocation
        {
            /// <summary>
            /// Gets or sets the street1.
            /// </summary>
            /// <value>
            /// The street1.
            /// </value>
            public string Street1 { get; set; }

            /// <summary>
            /// Gets or sets the street2.
            /// </summary>
            /// <value>
            /// The street2.
            /// </value>
            public string Street2 { get; set; }

            /// <summary>
            /// Gets or sets the city.
            /// </summary>
            /// <value>
            /// The city.
            /// </value>
            public string City { get; set; }

            /// <summary>
            /// Gets or sets the state.
            /// </summary>
            /// <value>
            /// The state.
            /// </value>
            public string State { get; set; }

            /// <summary>
            /// Gets or sets the postal code.
            /// </summary>
            /// <value>
            /// The postal code.
            /// </value>
            public string PostalCode { get; set; }

            /// <summary>
            /// Gets or sets the country.
            /// </summary>
            /// <value>
            /// The country.
            /// </value>
            public string Country { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SparkLinkLocation"/> class.
            /// </summary>
            public SparkLinkLocation()
            {

            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SparkLinkLocation" /> class.
            /// </summary>
            /// <param name="location">The location.</param>
            public SparkLinkLocation( Location location )
            {
                Street1 = location.Street1;
                Street2 = location.Street2;
                City = location.City;
                State = location.State;
                PostalCode = location.PostalCode;
                Country = location.Country;
            }
        }

        /// <summary>
        /// Details of BlockTypes from plugins
        /// </summary>
        public class PluginBlockType
        {
            /// <summary>
            /// Gets or sets the path.
            /// </summary>
            /// <value>
            /// The path.
            /// </value>
            public string Path { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the created date time.
            /// </summary>
            /// <value>
            /// The created date time.
            /// </value>
            public DateTime? CreatedDateTime { get; set; }

            /// <summary>
            /// Gets or sets the modified date time.
            /// </summary>
            /// <value>
            /// The modified date time.
            /// </value>
            public DateTime? ModifiedDateTime { get; set; }

            /// <summary>
            /// Gets or sets the file creation time of the block's file on disk.
            /// </summary>
            /// <value>
            /// The file creation time.
            /// </value>
            public DateTime? FileCreationTime { get; set; }

            /// <summary>
            /// Gets or sets the file last write time of the block's file on disk.
            /// </summary>
            /// <value>
            /// The file last write time.
            /// </value>
            public DateTime? FileLastWriteTime { get; set; }
        }

        #endregion

    }
}