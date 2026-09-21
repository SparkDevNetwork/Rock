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

namespace Rock.Communication.Chat.Platform.Sync
{
    internal static class ChatSyncOutcomeMapper
    {
        public static int HttpStatusFor( ChatSyncSubmissionStatus status )
        {
            switch ( status )
            {
                case ChatSyncSubmissionStatus.Accepted:
                case ChatSyncSubmissionStatus.Applied:
                    return 200;

                case ChatSyncSubmissionStatus.Refused:
                case ChatSyncSubmissionStatus.Failed:
                    return 422;

                default:
                    throw new ArgumentOutOfRangeException( nameof( status ), status, "no HTTP status is mapped for this recorded status" );
            }
        }

        public static bool IsJobSuccess( ChatSyncSubmissionStatus status )
        {
            switch ( status )
            {
                case ChatSyncSubmissionStatus.Accepted:
                case ChatSyncSubmissionStatus.Applied:
                    return true;

                case ChatSyncSubmissionStatus.Refused:
                case ChatSyncSubmissionStatus.Failed:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException( nameof( status ), status, "no run outcome is mapped for this recorded status" );
            }
        }

        public static string WireValueFor( ChatSyncSubmissionStatus status )
        {
            return status.ToString().ToLowerInvariant();
        }

        public static ChatSyncSubmissionStatus? ParseStatus( string value )
        {
            if ( value.IsNullOrWhiteSpace() )
            {
                return null;
            }

            ChatSyncSubmissionStatus parsed;
            if ( !Enum.TryParse( value, true, out parsed ) || !Enum.IsDefined( typeof( ChatSyncSubmissionStatus ), parsed ) )
            {
                return null;
            }

            // Enum.TryParse takes a number as well as a name, and a number is not a label the wire
            // could ever have carried.
            return WireValueFor( parsed ) == value.ToLowerInvariant() ? parsed : ( ChatSyncSubmissionStatus? ) null;
        }
    }
}
