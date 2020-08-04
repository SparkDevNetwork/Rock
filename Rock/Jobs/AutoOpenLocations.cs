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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Jobs
{
    /// <summary>
    /// Job that auto opens closed (inactive) named locations that are designated as a 'Room' location type.
    /// </summary>
    [DisplayName( "Auto Open Locations" )]
    [Description( "Job that auto opens closed (inactive) named locations that are designated as a 'Room' location type." )]

    [LocationField(
        "Parent Location",
        Description = "Optional location that if set will limit which locations are considered for re-opening. If not set, all named locations will be used.",
        IsRequired = false,
        Order = 0,
        CurrentPickerMode = LocationPickerMode.Named,
        AllowedPickerModes = new LocationPickerMode[] { LocationPickerMode.Named },
        Key = AttributeKey.ParentLocation )]
    [IntegerField(
        "Re-open Period (minutes)",
        Key = AttributeKey.ReopenPeriod,
        IsRequired = false,
        Description = "Optional period of time (in minutes) to look for locations that have been closed/inactivated (modified). Only locations modified within the timeframe would be considered. If left empty, the time the location was modified will not be considered.",
        Order = 1 )]
    [DisallowConcurrentExecution]
    public class AutoOpenLocations : IJob
    {
        /// <summary>
        /// Keys for DataMap Field Attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string ReopenPeriod = "ReopenPeriod";
            public const string ParentLocation = "ParentLocation";
        }

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public AutoOpenLocations()
        {
        }

        /// <summary>
        /// Job to get a National Change of Address (NCOA) report for all active people's addresses.
        ///
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            // Get the job setting(s)
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            var rockContext = new RockContext();
            var locationService = new LocationService( rockContext );

            var locationTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.LOCATION_TYPE_ROOM.AsGuid() ).Id;

            var inactiveLocationsQry = locationService.Queryable().Where( a => locationTypeValueId == a.LocationTypeValueId && !a.IsActive );

            // limit to only Named Locations (don't show home addresses, etc)
            inactiveLocationsQry = inactiveLocationsQry.Where( a => a.Name != null && a.Name != string.Empty );

            var reopenPeriod = dataMap.GetString( AttributeKey.ReopenPeriod ).AsIntegerOrNull();

            if ( reopenPeriod.HasValue )
            {
                var reopenDateTime = RockDateTime.Now.AddMinutes( -reopenPeriod.Value );
                // Only consider locations that were modified within the re-open period.
                inactiveLocationsQry = inactiveLocationsQry.Where( a => a.ModifiedDateTime >= reopenDateTime );
            }

            var inactiveLocations = inactiveLocationsQry.AsEnumerable();

            var parentLocationGuid = dataMap.GetString( AttributeKey.ParentLocation ).AsGuidOrNull();
            if ( parentLocationGuid.HasValue )
            {
                var parentLocation = locationService.Get( parentLocationGuid.Value );
                if ( parentLocation != null )
                {
                    var descendentLocations = locationService.GetAllDescendentIds( parentLocation.Id );
                    inactiveLocations = inactiveLocations.Where( a => descendentLocations.Contains( a.Id ) );
                }
            }

            int updatedLocationCount = 0;
            List<string> errors = new List<string>();
            List<Exception> exceptions = new List<Exception>();

            foreach ( var inactiveLocation in inactiveLocations.ToList() )
            {
                try
                {
                    inactiveLocation.IsActive = true;
                    updatedLocationCount++;
                    rockContext.SaveChanges();
                }
                catch ( Exception ex )
                {
                    // Capture and log the exception because we're not going to fail this job
                    // unless all the data views fail.
                    var errorMessage = $"An error occurred while trying to update location '{inactiveLocation.Name}' so it was skipped. Error: {ex.Message}";
                    errors.Add( errorMessage );
                    var ex2 = new Exception( errorMessage, ex );
                    exceptions.Add( ex2 );
                    ExceptionLogService.LogException( ex2, null );
                    continue;
                }
            }

            var results = new StringBuilder();
            // Format the result message
            results.AppendLine( $"Opened {updatedLocationCount} {"location".PluralizeIf( updatedLocationCount != 1 )}" );

            context.Result = results.ToString();

            if ( errors.Any() )
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.Append( "Errors: " );
                errors.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                string errorMessage = sb.ToString();
                context.Result += errorMessage;
                // We're not going to throw an aggregate exception unless there were no successes.
                // Otherwise the status message does not show any of the success messages in
                // the last status message.
                if ( updatedLocationCount == 0 )
                {
                    throw new AggregateException( exceptions.ToArray() );
                }
            }
        }
    }
}
