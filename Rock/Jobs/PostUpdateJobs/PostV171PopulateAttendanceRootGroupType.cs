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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs.PostUpdateJobs
{
    /// <summary>
    /// Run once job for v17.1 to populate the new RootGroupTypeId property on existing Attendance Occurrence records.
    /// </summary>
    [DisplayName( "Rock Update Helper v17.1 - Populate Attendance Root Group Type" )]
    [Description( "This job will populate the new RootGroupTypeId property on existing Attendance Occurrence records." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    public class PostV171PopulateAttendanceRootGroupType : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            // Get the configured timeout, or default to 240 minutes if it is blank.
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;

            var updateMap = GetUpdateMap( commandTimeout );

            foreach ( var update in updateMap )
            {
                UpdateAttendanceOccurrenceRecords( update.Key, update.Value, commandTimeout );
            }

            DeleteJob();
        }

        /// <summary>
        /// Gets a map that represents all the GroupId values for the rows
        /// that should have their RootGroupTypeId value updated.
        /// </summary>
        /// <param name="commandTimeout">The SQL command timeout in seconds.</param>
        /// <returns>A dictionary whose key is the GroupId and value is the RootGroupTypeId.</returns>
        private Dictionary<int, int> GetUpdateMap( int commandTimeout )
        {
            var rootGroupTypeMap = new Dictionary<int, int>();

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                // Get all the distinct group identifiers that have attendance
                // associated with them. This has an index on it so it should be
                // extremely fast.
                var groupIdMap = new AttendanceOccurrenceService( rockContext )
                    .Queryable()
                    .Select( g => new
                    {
                        g.GroupId,
                        g.Group.GroupTypeId
                    } )
                    .Where( g => g.GroupId.HasValue )
                    .Distinct()
                    .ToList();

                foreach ( var idMap in groupIdMap )
                {
                    // Get the group type for this group
                    var groupType = GroupTypeCache.Get( idMap.GroupTypeId, rockContext );

                    // Get the first root group type.
                    var rootGroupType = groupType?.GetRootGroupTypes( rockContext ).FirstOrDefault();

                    if ( rootGroupType != null )
                    {
                        rootGroupTypeMap.TryAdd( idMap.GroupId.Value, rootGroupType.Id );
                    }
                }
            }

            return rootGroupTypeMap;
        }

        /// <summary>
        /// Updates all <see cref="AttendanceOccurrence"/> records for the
        /// <paramref name="groupId"/> with the new <paramref name="rootGroupTypeId"/>
        /// value.
        /// </summary>
        /// <param name="groupId">The group identifier that will be used to filter the records to update.</param>
        /// <param name="rootGroupTypeId">The group type identifier that will be used when updated the records.</param>
        /// <param name="commandTimeout">The SQL command timeout in seconds.</param>
        private void UpdateAttendanceOccurrenceRecords( int groupId, int rootGroupTypeId, int commandTimeout )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                // Get all the attendance occurrences for this group
                var attendanceOccurrenceQry = new AttendanceOccurrenceService( rockContext )
                    .Queryable()
                    .Where( a => a.GroupId == groupId );

                rockContext.BulkUpdate( attendanceOccurrenceQry, ao => new AttendanceOccurrence
                {
                    RootGroupTypeId = rootGroupTypeId
                } );
            }
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        private void DeleteJob()
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( GetJobId() );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }
    }
}
