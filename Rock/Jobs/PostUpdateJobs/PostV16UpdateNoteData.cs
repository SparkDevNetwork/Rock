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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v16 to update note data for new format.
    /// </summary>
    [DisplayName( "Rock Update Helper v16.0 - Update Note Data." )]
    [Description( "This job updates note data to use new format." )]

    [IntegerField(
    "Command Timeout",
    Key = AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of notes, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = 14400 )]

    [IntegerField(
    "Start At Id",
    Key = AttributeKey.StartAtId,
    Description = "The Id of the record to start or resume the execution of the job.",
    IsRequired = false,
    DefaultIntegerValue = 0 )]
    public class PostV16UpdateNoteData : PostUpdateJobs.PostUpdateJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
            public const string StartAtId = "StartAtId";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            UpdateNoteTypeColor();

            DeleteDeniedNotes();

            DeleteJob();
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

        /// <summary>
        /// Updates all note types to move the background color to the color
        /// property. This will also trigger an update of the legacy values.
        /// </summary>
        private void UpdateNoteTypeColor()
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;

                var noteTypeService = new NoteTypeService( rockContext );
                var notes = noteTypeService.Queryable().ToList();

                foreach ( var note in notes )
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    if ( note.Color.IsNullOrWhiteSpace() && note.BackgroundColor.IsNotNullOrWhiteSpace() )
                    {
                        note.Color = note.BackgroundColor;
                    }
#pragma warning restore CS0618 // Type or member is obsolete
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Delete any notes that are denied. Also any that are pending and require approval.
        /// </summary>
        private void DeleteDeniedNotes()
        {
            int? firstId;
            int? lastId;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;

                var noteService = new NoteService( rockContext );

#pragma warning disable CS0618 // Type or member is obsolete
                firstId = noteService.Queryable()
                    .Where( n => n.ApprovalStatus == NoteApprovalStatus.Denied
                        || ( n.NoteType.RequiresApprovals && n.ApprovalStatus == NoteApprovalStatus.PendingApproval ) )
                    .Select( n => ( int? ) n.Id )
                    .Min();

                lastId = noteService.Queryable()
                    .Where( n => n.ApprovalStatus == NoteApprovalStatus.Denied
                        || ( n.NoteType.RequiresApprovals && n.ApprovalStatus == NoteApprovalStatus.PendingApproval ) )
                    .Select( n => ( int? ) n.Id )
                    .Max();
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if ( firstId.HasValue && lastId.HasValue )
            {
                var sql = @"
DELETE [N]
FROM [Note] AS [N]
INNER JOIN [NoteType] AS [NT] ON [NT].[Id] = [N].[NoteTypeId]
WHERE [N].[Id] >= @StartId AND [N].[Id] < @StartId + @BatchSize
  AND ([NT].[RequiresApprovals] = 1 AND [N].[ApprovalStatus] = 0) OR [N].[ApprovalStatus] = 2
";

                BulkUpdateRecords( sql, AttributeKey.StartAtId, lastId.Value, firstId.Value );
            }
        }
    }
}
