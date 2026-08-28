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
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /*
        8/24/2026 - CLAUDE

        Until the bot check was moved ahead of CreateAnonymousVisitorAlias, every
        page view from a JavaScript-capable crawler committed a PersonAlias row
        and then had its interaction discarded by the crawler filter at
        queue-flush time. Because a crawler that does not persist cookies looks
        like a new first-time visitor on every request, this produced one
        orphaned alias per bot page view rather than one per bot.

        RockCleanup already removes these, but only after the stale anonymous
        visitor retention period, which defaults to 365 days. This job clears the
        accumulated backlog immediately instead of waiting out that window.

        Reason: One-time cleanup of aliases created by a defect that is fixed in
        this same release.
    */

    /// <summary>
    /// Run once job for v20.0 to remove Anonymous Visitor person alias records
    /// that have no interactions.
    /// </summary>
    [DisplayName( "Rock Update Helper v20.0 - Remove Orphaned Anonymous Visitor Aliases" )]
    [Description( "This job removes Anonymous Visitor PersonAlias records that were created for bot traffic whose page view interaction was subsequently discarded as a crawler. After all the operations are done, this job will delete itself." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with many anonymous visitors, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    public class PostV20RemoveOrphanedAnonymousVisitorAliases : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// How recent an alias must be to be left alone. RegisterPageInteraction
        /// commits the alias and then enqueues the interaction, so there is a
        /// window where a perfectly legitimate alias has no interaction yet
        /// simply because the transaction queue has not flushed. Without this
        /// buffer the job would race that window and delete live visitors.
        /// </summary>
        private static readonly TimeSpan RecencyBuffer = TimeSpan.FromHours( 24 );

        /// <summary>
        /// The command timeout, in seconds, resolved once per run.
        /// </summary>
        private int _commandTimeout;

        /// <inheritdoc />
        public override void Execute()
        {
            // Get the configured timeout, or default to 240 minutes if it is blank.
            _commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;

            var orphanedPersonAliasIds = GetOrphanedPersonAliasIds();

            if ( !orphanedPersonAliasIds.Any() )
            {
                Result = "No orphaned Anonymous Visitor aliases were found.";

                DeleteJob();

                return;
            }

            UpdateLastStatusMessage( $"Removing {orphanedPersonAliasIds.Count:N0} orphaned Anonymous Visitor aliases." );

            // No pre-batch step is passed here. These aliases have no
            // interactions by definition, so there is nothing to null out.
            var deleteCount = PersonAliasService.DeletePersonAliasesInBatches(
                orphanedPersonAliasIds,
                CreateRockContext,
                Logger );

            var skippedCount = orphanedPersonAliasIds.Count - deleteCount;

            Result = skippedCount > 0
                ? $"Removed {deleteCount:N0} orphaned Anonymous Visitor aliases. {skippedCount:N0} could not be removed and have the reason recorded in their InternalMessage."
                : $"Removed {deleteCount:N0} orphaned Anonymous Visitor aliases.";

            DeleteJob();
        }

        /// <summary>
        /// Gets the identifiers of every Anonymous Visitor alias that has no
        /// interactions, is old enough to not be racing the transaction queue,
        /// and has not already failed a previous delete attempt.
        /// </summary>
        /// <returns>The list of person alias identifiers to remove.</returns>
        private System.Collections.Generic.List<int> GetOrphanedPersonAliasIds()
        {
            using ( var rockContext = CreateRockContext() )
            {
                var anonymousVisitorId = new PersonService( rockContext ).GetId( SystemGuid.Person.ANONYMOUS_VISITOR.AsGuid() );

                if ( !anonymousVisitorId.HasValue )
                {
                    // The Anonymous Visitor person record is missing, so there is
                    // nothing that can be safely identified as an orphan.
                    return new System.Collections.Generic.List<int>();
                }

                var cutoffDateTime = RockDateTime.Now.Subtract( RecencyBuffer );

                /*
                    Built as an unexecuted IQueryable so EF emits a NOT IN
                    subquery. Materializing the referenced alias ids first would
                    produce a WHERE IN clause large enough to exceed the batch
                    size limit on any real database.
                */
                var referencedPersonAliasIdQuery = new InteractionService( rockContext ).Queryable()
                    .Where( i => i.PersonAliasId.HasValue )
                    .Select( i => i.PersonAliasId.Value );

                /*
                    A null LastVisitDateTime compares as false here and is
                    therefore skipped, which is what we want. An alias with no
                    recorded visit time cannot be shown to be old enough to
                    delete safely.
                */
                return new PersonAliasService( rockContext )
                    .Queryable()
                    .Where( a => a.PersonId == anonymousVisitorId.Value
                        && string.IsNullOrEmpty( a.InternalMessage )
                        && a.LastVisitDateTime < cutoffDateTime
                        && !referencedPersonAliasIdQuery.Contains( a.Id ) )
                    .Select( a => a.Id )
                    .ToList();
            }
        }

        /// <summary>
        /// Creates a context configured with this job's command timeout.
        /// </summary>
        /// <returns>A new <see cref="RockContext"/>.</returns>
        private RockContext CreateRockContext()
        {
            var rockContext = new RockContext();

            rockContext.Database.CommandTimeout = _commandTimeout;

            return rockContext;
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
