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
    /// Works out what a sync run has to say for itself.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Three answers, in order of how much they are worth. What became of this submission,
    ///         where the queue reached it while the run was still waiting. What became of the
    ///         previous one, where it did not, which the acknowledgement carries for exactly this
    ///         reason. And, where there is neither, a plain statement that the restatement is
    ///         stored and queued.
    ///     </para>
    ///     <para>
    ///         That last answer is not a failure. The queue runs on its own schedule and very often
    ///         has not reached a submission by the time the run that made it finishes, so a run
    ///         that failed over it would be red on most cycles at a healthy church and would teach
    ///         its administrator to stop reading the job.
    ///     </para>
    ///     <para>
    ///         The fallback names the submission it is talking about. Reporting the previous
    ///         cycle's result as though it were this one's would read as a success on the run after
    ///         a failure, and as a failure on the run after a fix.
    ///     </para>
    /// </remarks>
    internal static class ChatSyncStatusPath
    {
        /// <summary>
        /// Resolves the run's own result.
        /// </summary>
        /// <param name="acknowledgement">What came back from the submission.</param>
        /// <param name="polled">What a status read found, or null where none could be made.</param>
        /// <returns>The result. Never null, and always carrying a sentence.</returns>
        public static ChatSyncRunResult Resolve( ChatSyncAcknowledgement acknowledgement, ChatSyncOutcome polled )
        {
            if ( acknowledgement == null )
            {
                return new ChatSyncRunResult
                {
                    IsFailure = true,
                    Message = "the submission was never made"
                };
            }

            if ( acknowledgement.IsTransportFailure )
            {
                return new ChatSyncRunResult
                {
                    IsFailure = true,
                    Message = "the chat platform could not be reached: "
                        + ( acknowledgement.TransportDetail.IsNullOrWhiteSpace()
                            ? "no reason was given"
                            : acknowledgement.TransportDetail )
                };
            }

            if ( !acknowledgement.Status.HasValue )
            {
                return new ChatSyncRunResult
                {
                    IsFailure = true,
                    Message = "the chat platform answered with nothing this version of Rock can read"
                        + Reason( acknowledgement.ErrorCode )
                };
            }

            // A submission the platform turned away never reaches the queue, so there is nothing to
            // poll for and nothing a previous cycle could say that would matter more.
            if ( acknowledgement.Status.Value == ChatSyncSubmissionStatus.Refused )
            {
                return new ChatSyncRunResult
                {
                    IsFailure = true,
                    Message = "the chat platform refused this restatement" + Reason( acknowledgement.ErrorCode )
                };
            }

            if ( polled != null && polled.Status.HasValue && polled.Status.Value != ChatSyncSubmissionStatus.Accepted )
            {
                return new ChatSyncRunResult
                {
                    IsFailure = !ChatSyncOutcomeMapper.IsJobSuccess( polled.Status.Value ),
                    Message = "this restatement was " + ChatSyncOutcomeMapper.WireValueFor( polled.Status.Value )
                        + Reason( polled.ErrorCode )
                };
            }

            var previous = acknowledgement.PreviousOutcome;
            if ( previous != null && previous.Status.HasValue )
            {
                return new ChatSyncRunResult
                {
                    IsFailure = !ChatSyncOutcomeMapper.IsJobSuccess( previous.Status.Value ),
                    Message = "this restatement was submitted, not yet applied. The previous submission, "
                        + previous.SubmissionId + ", was "
                        + ChatSyncOutcomeMapper.WireValueFor( previous.Status.Value )
                        + Reason( previous.ErrorCode )
                };
            }

            return new ChatSyncRunResult
            {
                IsFailure = false,
                Message = "this restatement was submitted, not yet applied"
            };
        }

        /// <summary>
        /// The named reason, where there is one, as a clause rather than a bare code.
        /// </summary>
        /// <param name="errorCode">The code, or null.</param>
        /// <returns>The clause, or an empty string.</returns>
        private static string Reason( string errorCode )
        {
            return errorCode.IsNullOrWhiteSpace() ? string.Empty : ": " + errorCode;
        }
    }
}
