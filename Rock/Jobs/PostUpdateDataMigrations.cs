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

namespace Rock.Jobs
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Rock Update Helper" )]
    [Description( "This job will run any post data migrations that haven't completed yet." )]

    [IntegerField(
        "Command Timeout",
        AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 60 )]

    public class PostUpdateDataMigrations : IJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // get the configured timeout, or default to 60 minutes if it is blank
            var commandTimeoutSeconds = dataMap.GetString( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;

            var postUpdateTypes = Reflection.FindTypes( typeof( PostUpdateMigration ) ).Values.ToList();
            var postUpdateList = postUpdateTypes.Select( a => Activator.CreateInstance( a ) as PostUpdateMigration );
            var pendingUpdates = postUpdateList.Where( a => !a.IsComplete() );

            HashSet<Task<PostUpdateMigration>> startedUpdates = new HashSet<Task<PostUpdateMigration>>();

            foreach ( var postUpdate in pendingUpdates )
            {
                if ( postUpdate.IsRunning )
                {
                    continue;
                }

                postUpdate.CommandTimeoutSeconds = commandTimeoutSeconds;
                var postUpdateMigrationTask = postUpdate.StartUpdate();
                startedUpdates.Add( postUpdateMigrationTask );
            }

            HashSet<PostUpdateMigration> finishedUpdates = new HashSet<PostUpdateMigration>();

            Task.WhenAny( startedUpdates.ToArray() ).ContinueWith( ( a ) =>
            {
                var completedTask = a.Result;
                var completedUpdate = completedTask.Result;
                finishedUpdates.Add( completedUpdate );
                context.UpdateLastStatusMessage( $"Completed { completedUpdate }..." );
            } );

            Task.WaitAll( startedUpdates.ToArray() );

            // Some PostUpdateMigrations might require multiple runs before they are complete.
            var remainingUpdates = pendingUpdates.Where( a => !a.IsComplete() );

            var resultMessageBuilder = new StringBuilder();
            if ( finishedUpdates.Any() )
            {
                resultMessageBuilder.AppendLine( "Completed: " + finishedUpdates.Select( a => a.ToString() ).ToList().AsDelimited( ", ", " and " ) + "." );
            }

            if ( remainingUpdates.Any() )
            {
                resultMessageBuilder.AppendLine( "Remaining: " + finishedUpdates.Select( a => a.ToString() ).ToList().AsDelimited( ", ", " and " ) + "." );
            }

            context.UpdateLastStatusMessage( resultMessageBuilder.ToString() );
        }

    }
}
