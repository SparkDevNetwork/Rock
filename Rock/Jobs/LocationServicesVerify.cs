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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to keep a heartbeat of the job process so we know when the jobs stop working
    /// </summary>
    [DisplayName( "Location Services Verify" )]
    [Description( "Attempts to standardize and geocode addresses that have not been verified yet. It also attempts to re-verify address that failed in the past after a given wait period." )]

    [IntegerField( "Max Records Per Run", "The maximum number of records to run per run.", true, 1000 )]
    [IntegerField( "Throttle Period", "The number of milliseconds to wait between records. This helps to throttle requests to the lookup services.", true, 500 )]
    [IntegerField( "Retry Period", "The number of days to wait before retrying a unsuccessful address lookup.", true, 200 )]
    public class LocationServicesVerify : RockJob
    {
        
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public LocationServicesVerify()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            int maxRecords = GetAttributeValue( "MaxRecordsPerRun" ).AsIntegerOrNull() ?? 1000;
            int throttlePeriod = GetAttributeValue( "ThrottlePeriod" ).AsIntegerOrNull() ?? 500;
            int retryPeriod = GetAttributeValue( "RetryPeriod" ).AsIntegerOrNull() ?? 200;

            var retryDate = RockDateTime.Now.Subtract( new TimeSpan( retryPeriod, 0, 0, 0 ) );

            var rockContext = new Rock.Data.RockContext();
            LocationService locationService = new LocationService( rockContext );
            var addresses = locationService.Queryable()
                .Where( l => 
                    (
                        ( l.IsGeoPointLocked == null || l.IsGeoPointLocked == false ) &&// don't ever try locked address
                        l.IsActive == true && 
                        l.Street1 != null &&
                        l.Street1 != string.Empty &&
                        (
                            ( l.GeocodedDateTime == null && ( l.GeocodeAttemptedDateTime == null || l.GeocodeAttemptedDateTime < retryDate ) ) || // has not been attempted to be geocoded since retry date
                            ( l.StandardizedDateTime == null && ( l.StandardizeAttemptedDateTime == null || l.StandardizeAttemptedDateTime < retryDate ) ) // has not been attempted to be standardize since retry date
                        )
                    ) )
                .Take( maxRecords ).ToList();

            int attempts = 0;
            int successes = 0;
            foreach ( var address in addresses )
            {
                attempts++;
                if ( locationService.Verify( address, true ) ) 
                {
                    successes++;
                }
                rockContext.SaveChanges();
                System.Threading.Tasks.Task.Delay( throttlePeriod ).Wait();
            }

            this.Result = string.Format( "{0:N0} address verifications attempted; {1:N0} successfully verified", attempts, successes );
        }
    }
}
