// <copyright>
// Copyright by Central Christian Church
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
using System.Data.Entity;
using System.Linq;
using com.centralaz.RoomManagement.Model;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace com.centralaz.RoomManagement.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for the SQL Query to complete. Leave blank to use the default for this job (3600). Note, it could take several minutes, so you might want to set it at 3600 (60 minutes) or higher", false, 60 * 60, "General", 1, "CommandTimeout" )]
    public class PopulateFirstLastOccurrenceDateTimes : IJob
    {
        private int _commandTimeout = 0;

        private int _remainingReservationRecords = 0;
        private int _reservationRecordsLoaded = 0;
        private int _reservationRecordsRead = 0;
        private int _reservationRecordsUpdated = 0;

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>

        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            _commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull() ?? 3600;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = _commandTimeout;
                _remainingReservationRecords = rockContext.Database.SqlQuery<int>( $@"
                    SELECT COUNT(*) FROM [_com_centralaz_RoomManagement_Reservation] WHERE [FirstOccurrenceStartDateTime] IS NULL OR [LastOccurrenceEndDateTime] IS NULL
                " ).First();

                if ( _remainingReservationRecords == 0 )
                {
                    // delete job if there are no unlined attendance records
                    var jobId = context.GetJobId();
                    var jobService = new ServiceJobService( rockContext );
                    var job = jobService.Get( jobId );
                    if ( job != null )
                    {
                        jobService.Delete( job );
                        rockContext.SaveChanges();
                        return;
                    }
                }
            }

            PopulateOccurrenceData( context );

            context.UpdateLastStatusMessage( $@"Remaining Reservation Records: {_remainingReservationRecords},
                    Reservation Records Loaded: {_reservationRecordsLoaded}, 
                    Reservation Records Read: {_reservationRecordsRead}, 
                    Reservation Records Updated: { _reservationRecordsUpdated}
                    " );
        }

        /// <summary>
        /// Migrates the page views data.
        /// </summary>
        /// <param name="context">The context.</param>
        private void PopulateOccurrenceData( IJobExecutionContext context )
        {
            List<int> reservationIdList = new List<int>();
            _reservationRecordsUpdated = 0;
            _reservationRecordsRead = 0;
            using ( var rockContext = new RockContext() )
            {
                reservationIdList = new ReservationService( rockContext ).Queryable()
                    .AsNoTracking()
                    .Where( r => r.FirstOccurrenceStartDateTime == null || r.LastOccurrenceEndDateTime == null )
                    .Select( r => r.Id )
                    .ToList();
            }
            _reservationRecordsLoaded = reservationIdList.Count();

            try
            {
                foreach ( var reservationId in reservationIdList )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        rockContext.Database.CommandTimeout = _commandTimeout;
                        var reservationService = new ReservationService( rockContext );
                        var reservation = reservationService.Get( reservationId );

                        if ( reservation != null )
                        {
                            _reservationRecordsRead++;

                            try
                            {
                                reservation = reservationService.SetFirstLastOccurrenceDateTimes( reservation );
                                rockContext.SaveChanges();
                                _reservationRecordsUpdated++;
                            }
                            catch
                            {

                            }

                        }
                    }
                }
            }
            catch
            {

            }
        }
    }
}
