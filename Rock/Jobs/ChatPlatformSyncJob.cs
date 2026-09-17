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
using System.Net.Http;

using Rock.Attribute;
using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Session;
using Rock.Communication.Chat.Platform.Sync;
using Rock.Data;

namespace Rock.Jobs
{
    /// <summary>
    /// Restates this church's people, channels, memberships and badges onto
    /// the chat platform. The church owns the cadence; 24 hours is the cap.
    /// </summary>
    [DisplayName( "Chat Restatement" )]
    [Description( "Sends this church's people, channels, memberships and badges to the chat platform." )]

    [IntegerField(
        "Command Timeout",
        Description = "Maximum seconds to wait for the projection queries. Leave blank to use 3600.",
        IsRequired = false,
        DefaultIntegerValue = 3600,
        Order = 0,
        Key = AttributeKey.CommandTimeout )]

    public class ChatPlatformSyncJob : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            var configuration = ChatPlatformConfigurationService.Read();
            if ( !configuration.IsConfigured )
            {
                throw new Exception( "Chat is not configured." );
            }

            ClampCadence();

            var isManual = ChatSyncNow.ConsumeUrgent() || IsRunNow();
            var backoff = ChatSyncNow.ReadBackoffUntil();
            if ( ChatSyncRunner.ShouldDelay( backoff, isManual, DateTime.UtcNow ) )
            {
                Result = string.Format( "Waiting until {0:u} as advised by the chat platform.", backoff );
                return;
            }

            var mint = ChatSessionHelper.TryMintSyncToken( new ChatSessionContext { Configuration = configuration } );
            if ( mint.Gate != ChatMintGate.Ok || string.IsNullOrWhiteSpace( mint.ChurchToken ) )
            {
                throw new Exception( "Chat could not sign a sync credential." );
            }

            var timeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;
            var readAt = ChatSyncPayloadWriter.TruncateToMicroseconds( DateTime.UtcNow );
            var submissionId = Guid.NewGuid();

            UpdateLastStatusMessage( "Reading people, channels, memberships and badges." );

            IDictionary<string, IList<IDictionary<string, object>>> sections;
            IDictionary<string, long> identity;
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = timeout;
                sections = ChatProjection.Read( rockContext, configuration, timeout );
                identity = ChatProjection.ReadIdentityMarks( rockContext );
            }

            var lengths = ChatSyncPayloadWriter.SectionLengths( sections );
            var body = ChatSyncPayloadWriter.WriteBody( sections );
            var counts = ChatSyncPayloadWriter.BuildCounts( lengths );
            var marks = ChatSyncPayloadWriter.BuildMarks( identity );

            UpdateLastStatusMessage( "Submitting the restatement." );

            ChatSyncOutcome submit;
            using ( var http = new HttpClient() )
            {
                var client = new ChatSyncClient( http, configuration, mint.ChurchToken );
                submit = client.SubmitAsync( new ChatSyncSubmitRequest
                {
                    SubmissionId = submissionId,
                    ReadAt = ChatSyncPayloadWriter.FormatReadAt( readAt ),
                    Body = body,
                    CountsJson = ChatSyncPayloadWriter.WriteHeaderObject( counts ),
                    MarksJson = ChatSyncPayloadWriter.WriteHeaderObject( marks ),
                    RockVersion = Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber(),
                    IsUrgent = isManual
                } ).GetAwaiter().GetResult();

                ChatSyncNow.WriteBackoffUntil( submit.SyncBackoffUntil );

                if ( submit.IsFailure )
                {
                    throw new Exception( submit.ToJobResult() );
                }

                if ( submit.IsApplied )
                {
                    Result = submit.ToJobResult();
                    return;
                }

                UpdateLastStatusMessage( "Waiting for the platform to apply the restatement." );

                var settled = ChatSyncRunner.PollUntilSettledAsync(
                    () => client.StatusAsync( submissionId ),
                    delay => System.Threading.Thread.Sleep( delay ),
                    () => DateTime.UtcNow ).GetAwaiter().GetResult();

                if ( settled == null )
                {
                    throw new RockJobWarningException( "Chat sync was submitted and the platform has not applied it yet." );
                }

                ChatSyncNow.WriteBackoffUntil( settled.SyncBackoffUntil );

                if ( settled.IsApplied )
                {
                    Result = settled.ToJobResult();
                    return;
                }

                if ( settled.IsFailure )
                {
                    throw new Exception( settled.ToJobResult() );
                }

                throw new RockJobWarningException( settled.ToJobResult() + " The platform has not applied it yet." );
            }
        }

        private void ClampCadence()
        {
            if ( ServiceJob == null || string.IsNullOrWhiteSpace( ServiceJob.CronExpression ) )
            {
                return;
            }

            var clamped = ChatSyncRunner.ClampCronExpression( ServiceJob.CronExpression, DateTimeOffset.Now );
            if ( clamped == ServiceJob.CronExpression )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var job = new Rock.Model.ServiceJobService( rockContext ).Get( ServiceJob.Id );
                if ( job == null )
                {
                    return;
                }

                job.CronExpression = clamped;
                rockContext.SaveChanges();
                ServiceJob.CronExpression = clamped;
            }
        }

        private bool IsRunNow()
        {
            return Scheduler != null
                && Scheduler.SchedulerName != null
                && Scheduler.SchedulerName.StartsWith( "RunNow:", StringComparison.OrdinalIgnoreCase );
        }
    }
}
