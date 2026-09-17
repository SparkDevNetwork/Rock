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
using System.Linq;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Tasks;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Queues a restatement now. Lives here rather than on a block so a test
    /// can pin it: the unit suite cannot see block classes.
    /// </summary>
    internal static class ChatSyncNow
    {
        /// <summary>
        /// True when chat is configured and the restatement job is registered.
        /// </summary>
        internal static bool CanRequest( RockContext rockContext )
        {
            return CanRequest( rockContext, ChatPlatformConfigurationService.Read() );
        }

        /// <summary>
        /// True when chat is configured and the restatement job is registered.
        /// </summary>
        internal static bool CanRequest( RockContext rockContext, ChatPlatformConfiguration configuration )
        {
            if ( configuration == null || !configuration.IsConfigured )
            {
                return false;
            }

            return FindJob( rockContext ) != null;
        }

        /// <summary>
        /// Marks the next run urgent and queues it. Returns false with a reason
        /// when the job is missing or chat is not configured.
        /// </summary>
        internal static bool TryRequest( RockContext rockContext, out string error )
        {
            return TryRequest( rockContext, ChatPlatformConfigurationService.Read(), out error, null );
        }

        /// <summary>
        /// Marks the next run urgent and queues it. The enqueue action is the
        /// production bus send, and a test substitutes a recorder so this does
        /// not need a running message bus.
        /// </summary>
        internal static bool TryRequest( RockContext rockContext, ChatPlatformConfiguration configuration, out string error, Action<int> enqueue )
        {
            error = null;

            if ( configuration == null || !configuration.IsConfigured )
            {
                error = "Chat is not configured.";
                return false;
            }

            var job = FindJob( rockContext );
            if ( job == null )
            {
                error = "The chat restatement job is not registered.";
                return false;
            }

            if ( enqueue != null )
            {
                enqueue( job.Id );
            }
            else
            {
                Rock.Web.SystemSettings.SetValue( SystemSetting.CHAT_PLATFORM_SYNC_URGENT, "true" );
                new ProcessRunJobNow.Message { JobId = job.Id }.Send();
            }

            return true;
        }

        /// <summary>
        /// Consumes and clears the urgent flag. A missing setting is not urgent.
        /// </summary>
        internal static bool ConsumeUrgent()
        {
            var raw = Rock.Web.SystemSettings.GetValue( SystemSetting.CHAT_PLATFORM_SYNC_URGENT );
            if ( !raw.AsBoolean() )
            {
                return false;
            }

            Rock.Web.SystemSettings.SetValue( SystemSetting.CHAT_PLATFORM_SYNC_URGENT, "false" );
            return true;
        }

        /// <summary>
        /// Stored backpressure, or null when none.
        /// </summary>
        internal static DateTime? ReadBackoffUntil()
        {
            var raw = Rock.Web.SystemSettings.GetValue( SystemSetting.CHAT_PLATFORM_SYNC_BACKOFF_UNTIL );
            DateTime parsed;
            if ( DateTime.TryParse( raw, null, System.Globalization.DateTimeStyles.RoundtripKind, out parsed ) )
            {
                return parsed.ToUniversalTime();
            }

            return null;
        }

        /// <summary>
        /// Stores backpressure from an acknowledgement.
        /// </summary>
        internal static void WriteBackoffUntil( DateTime? utc )
        {
            Rock.Web.SystemSettings.SetValue(
                SystemSetting.CHAT_PLATFORM_SYNC_BACKOFF_UNTIL,
                utc.HasValue ? utc.Value.ToUniversalTime().ToString( "o" ) : string.Empty );
        }

        private static ServiceJob FindJob( RockContext rockContext )
        {
            if ( rockContext == null )
            {
                return null;
            }

            var guid = Guid.Parse( Rock.SystemGuid.ServiceJob.CHAT_PLATFORM_SYNC );
            return rockContext.Set<ServiceJob>().FirstOrDefault( j => j.Guid == guid );
        }
    }
}
