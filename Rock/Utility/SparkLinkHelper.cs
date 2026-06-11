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
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;

using Newtonsoft.Json;

using RestSharp;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Store;
using Rock.SystemKey;
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

            var sparkLinkRequest = new SparkLinkRequestV2();
            sparkLinkRequest.RockInstanceId = Rock.Web.SystemSettings.GetRockInstanceId();
            sparkLinkRequest.RockVersion = VersionInfo.VersionInfo.GetRockSemanticVersionNumber();
            sparkLinkRequest.InstallDateTime = Rock.Web.SystemSettings.GetRockInstallationDateTime();

            var globalAttributes = GlobalAttributesCache.Get();
            sparkLinkRequest.OrganizationName = globalAttributes.GetValue( "OrganizationName" );
            sparkLinkRequest.PublicUrl = globalAttributes.GetValue( "PublicApplicationRoot" );

            // Compute "active" person count (non-deceased, non-business) for rough instance sizing.
            sparkLinkRequest.NumberOfActiveRecords = new PersonService( rockContext ).Queryable( includeDeceased: false, includeBusinesses: false ).AsNoTracking().Count();

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

            sparkLinkRequest.SparkLinkAdditionalSettings = CollectAdditionalSettings( rockContext, sparkLinkRequest );

            var sparkLinkRequestJson = JsonConvert.SerializeObject( sparkLinkRequest );

            // Dev Note: Test by changing the SparkApiUrl in your web.config to point to a local endpoint (such as http://localhost:57822/). 
            var client = new RestClient( $"{ConfigurationManager.AppSettings["SparkApiUrl"].EnsureTrailingForwardslash()}api/SparkLink/update" );

            // POST instance/package/plugin metadata to Rock's SparkLink API.
            // Response contains a list of "Notification" objects (e.g., advisories, messages, alerts).
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

            // If there are RockShop package version IDs, call a second endpoint to get package-version-specific notifications.
            if ( sparkLinkRequest.SparkLinkAdditionalSettings.Event.SparkLink.PluginVersionIds.Any() )
            {
                client = new RestClient( $"{ConfigurationManager.AppSettings["SparkApiUrl"].EnsureTrailingForwardslash()}api/Packages/VersionNotifications" );
                request = new RestRequest( Method.GET );
                request.AddParameter( "VersionIds", sparkLinkRequest.SparkLinkAdditionalSettings.Event.SparkLink.PluginVersionIds.AsDelimited( "," ) );
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

        private static SparkLinkAdditionalSettings CollectAdditionalSettings( RockContext rockContext, SparkLinkRequestV2 sparkLinkRequest )
        {
            var installedPackages = InstalledPackageService.GetInstalledPackages();

            var experienceMode = ExperienceMode.Trailblazer; // default unless next setting is false.
            if ( !Rock.Web.SystemSettings.GetValue( SystemSetting.TRAILBLAZER_MODE ).AsBoolean() )
            {
                experienceMode = ExperienceMode.Essentials;
            }

            var sparkLinkAdditionalSettings = new SparkLinkAdditionalSettings
            {
                Event = new SparkLinkEvent
                {
                    SparkLink = new SparkLink
                    {
                        ExperienceMode = experienceMode,
                        PluginVersionIds = installedPackages.Select( i => i.VersionId ).ToList(),
                        PluginBlockTypes = new List<PluginBlockType>()
                    }
                }
            };

            // Collect block types that live under ~/Plugins (i.e., custom/plugin blocks).
            // We can use this to detect plugin inventory and potentially flag outdated/modified plugins.
            sparkLinkAdditionalSettings.Event.SparkLink.PluginBlockTypes = new BlockTypeService( rockContext ).Queryable()
                .Where( bt => bt.Path.Contains( "~/Plugins" ) )
                .Select( bt => new PluginBlockType()
                {
                    CreatedDateTime = bt.CreatedDateTime,
                    ModifiedDateTime = bt.ModifiedDateTime,
                    Name = bt.Name,
                    Path = bt.Path,
                } ).ToList();

            // Enrich PluginBlockTypes list with underlying file timestamps from disk.
            // This adds another signal for "what's deployed" and whether files changed.
            foreach ( var item in sparkLinkAdditionalSettings.Event.SparkLink.PluginBlockTypes )
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

            return sparkLinkAdditionalSettings;
        }

        #region Helper Class

        /// <summary>
        /// Represents a request to the SparkLink API (original version).
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
            /// Gets or sets the RockShop plugin version ids on this Rock instance.
            /// </summary>
            /// <remarks>
            /// See also: org.sparkdevnetwork.Core.Rest.Controllers.SparkLinkController
            /// </remarks>
            /// <value>
            /// The RockShop plugin version ids.
            /// </value>
            [Obsolete( "See SparkLinkAdditionalSettings.Event.SparkLink.PluginVersionIds.  NOTE: We cannot remove this from Spark until all Rock [supported] instances are v19 or higher because it is used by the org.sparkdevnetwork.Core SparkLinkController.cs for pre-v19 versions of Rock." )]
            [RockObsolete( "19.0" )]
            public List<int> VersionIds { get; set; }
        }

        /// <summary>
        /// Represents a request to the SparkLink API (version 2).
        /// </summary>
        /// <seealso cref="Rock.Utility.SparkLinkHelper.SparkLinkRequest" />
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "19.0" )]
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
            /// <remarks>
            /// See also: org.sparkdevnetwork.Core.Rest.Controllers.SparkLinkController
            /// </remarks>
            /// <value>
            /// The plugin block types.
            /// </value>
            [Obsolete( "See SparkLinkAdditionalSettings.Event.SparkLink.PluginBlockTypes. NOTE: We cannot remove this from Spark until all Rock [supported] instances are v19 or higher because it is used by the org.sparkdevnetwork.Core SparkLinkController.cs for pre-v19 versions of Rock." )]
            [RockObsolete( "19.0" )]
            public List<PluginBlockType> PluginBlockTypes { get; set; }

            /// <summary>
            /// The date and time the Rock instance was installed.
            /// </summary>
            public DateTime InstallDateTime { get; set; }

            /// <summary>
            /// Holds additional settings (JSON) data to send via SparkLink.
            /// </summary>
            /// <remarks>
            ///     <para>
            ///         <strong>This is an internal API</strong> that supports the Rock
            ///         infrastructure and not subject to the same compatibility standards
            ///         as public APIs. It may be changed or removed without notice in any
            ///         release and should therefore not be directly used in any plug-ins.
            ///     </para>
            /// </remarks>
            [RockInternal( "19.0" )]
            public SparkLinkAdditionalSettings SparkLinkAdditionalSettings { get; set; }
        }

        /// <summary>
        /// The location of the Rock instance for use with SparkLink's OrganizationLocation
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
        /// Represents the AdditionalSettings event payload for use with the SparkLink system.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "19.0" )]
        public sealed class SparkLinkAdditionalSettings
        {
            /// <summary>
            /// 
            /// </summary>
            public SparkLinkEvent Event { get; set; }
        }

        /// <summary>
        /// Represents an event payload for use with the SparkLink system.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "19.0" )]
        public sealed class SparkLinkEvent
        {
            /// <summary>
            /// Holds SparkLink-type data for the SparkLink event.
            /// </summary>
            public SparkLink SparkLink { get; set; }
        }

        /// <summary>
        /// Represents an event payload for use with the SparkLink system.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "19.0" )]
        public sealed class SparkLink
        {
            /// <summary>
            /// Gets or sets the experience mode for the current Rock instance.
            /// </summary>
            public ExperienceMode ExperienceMode { get; set; }

            /// <summary>
            /// Gets or sets the RockShop plugin version ids on this Rock instance.
            /// </summary>
            public List<int> PluginVersionIds { get; set; }

            /// <summary>
            /// Gets or sets the list of plugin block types associated with this Rock instance.
            /// </summary>
            public List<PluginBlockType> PluginBlockTypes { get; set; }
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

        /// <summary>
        /// Specifies the available experience modes.
        /// <seealso cref="SystemSetting.TRAILBLAZER_MODE" />
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "19.0" )]
        public enum ExperienceMode
        {
            /// <summary>
            /// The Essentials experience mode (TRAILBLAZER_MODE false).
            /// </summary>
            Essentials = 1,

            /// <summary>
            /// The Trailblazer experience mode (TRAILBLAZER_MODE true).
            /// </summary>
            Trailblazer = 2
        }

        #endregion

    }
}