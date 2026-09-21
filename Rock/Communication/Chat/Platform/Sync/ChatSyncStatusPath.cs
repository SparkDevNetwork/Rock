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
namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Works out what a run reports from what it was told.
    /// </summary>
    internal static class ChatSyncStatusPath
    {
        /// <summary>
        /// Resolves the run's own result.
        /// </summary>
        /// <param name="acknowledgement">What came back from the submission.</param>
        /// <param name="polled">What a status read found, or null where none was made.</param>
        /// <returns>The result.</returns>
        public static ChatSyncRunResult Resolve( ChatSyncAcknowledgement acknowledgement, ChatSyncOutcome polled )
        {
            if ( polled != null && polled.Status.HasValue )
            {
                return new ChatSyncRunResult
                {
                    IsFailure = !ChatSyncOutcomeMapper.IsJobSuccess( polled.Status.Value ),
                    Message = "the restatement is " + ChatSyncOutcomeMapper.WireValueFor( polled.Status.Value )
                        + ( polled.ErrorCode.IsNullOrWhiteSpace() ? string.Empty : " (" + polled.ErrorCode + ")" )
                };
            }

            return new ChatSyncRunResult
            {
                IsFailure = !acknowledgement.IsJobSuccess,
                Message = acknowledgement.ErrorCode
            };
        }
    }
}
