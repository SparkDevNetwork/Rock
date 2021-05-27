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

using System;
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Update.Interfaces;
using Rock.Update.Models;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Update
{
    /// <summary>
    /// Class used to send statistics to the Rock site.
    /// </summary>
    public class RockInstanceImpactStatistics
    {
        private readonly IRockImpactService _rockImpactService;
        private readonly RockContext _rockContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RockInstanceImpactStatistics"/> class.
        /// </summary>
        /// <param name="rockImpactService">The rock impact service.</param>
        /// <param name="rockContext">The rock context.</param>
        public RockInstanceImpactStatistics( IRockImpactService rockImpactService, RockContext rockContext )
        {
            _rockImpactService = rockImpactService;
            _rockContext = rockContext;
        }

        /// <summary>
        /// Sends the impact statistics to spark.
        /// </summary>
        /// <param name="includeOrganizationData">if set to <c>true</c> [include organization data].</param>
        /// <param name="version">The version.</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="environmentData">The environment data.</param>
        public void SendImpactStatisticsToSpark( bool includeOrganizationData, string version, string ipAddress, string environmentData )
        {
            ImpactLocation organizationLocation = null;
            var publicUrl = string.Empty;
            var organizationName = string.Empty;

            var numberOfActiveRecords = new PersonService( _rockContext ).Queryable( includeDeceased: false, includeBusinesses: false ).Count();

            if ( numberOfActiveRecords <= 100 || SystemSettings.GetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE ).AsDateTime().HasValue )
            {
                return;
            }

            if ( includeOrganizationData )
            {
                var globalAttributes = GlobalAttributesCache.Get();

                // Fetch the organization address
                var organizationAddressLocationGuid = globalAttributes.GetValue( "OrganizationAddress" ).AsGuid();
                if ( !organizationAddressLocationGuid.Equals( Guid.Empty ) )
                {
                    var location = new LocationService( _rockContext ).Get( organizationAddressLocationGuid );
                    if ( location != null )
                    {
                        organizationLocation = new ImpactLocation( location );
                    }
                }

                organizationName = globalAttributes.GetValue( "OrganizationName" );
                publicUrl = globalAttributes.GetValue( "PublicApplicationRoot" );
            }
            else
            {
                numberOfActiveRecords = 0;
            }

            ImpactStatistic impactStatistic = new ImpactStatistic()
            {
                RockInstanceId = SystemSettings.GetRockInstanceId(),
                Version = version,
                IpAddress = ipAddress,
                PublicUrl = publicUrl,
                OrganizationName = organizationName,
                OrganizationLocation = organizationLocation,
                NumberOfActiveRecords = numberOfActiveRecords,
                EnvironmentData = environmentData
            };

            _rockImpactService.SendImpactStatisticsToSpark( impactStatistic );
        }
    }
}
