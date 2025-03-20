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
using Newtonsoft.Json; // We need this implementation of `JsonIgnoreAttribute` to ignore certain properties when logging via Serilog.

using Rock.Enums.Communication.Chat;

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// Represents the base class for a command that synchronizes data from the external chat system to Rock.
    /// </summary>
    internal abstract class ChatToRockSyncCommand
    {
        /// <summary>
        /// Gets or sets whether this sync command was completed.
        /// </summary>
        public bool WasCompleted { get; private set; }

        /// <summary>
        /// Gets or sets the number of times this webhook has been attempted to be processed.
        /// </summary>
        public int Attempts { get; private set; }

        /// <summary>
        /// Gets or sets the maximum number of times to attempt a chat-to-Rock sync command before giving up.
        /// </summary>
        public int AttemptLimit { get; private set; }

        /// <summary>
        /// Gets or sets the type of synchronization to perform.
        /// </summary>
        public virtual ChatSyncType ChatSyncType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the command should be retried.
        /// </summary>
        [JsonIgnore]
        public bool ShouldRetry => !WasCompleted && Attempts < AttemptLimit;

        /// <summary>
        /// Gets the reason why the synchronization operation failed.
        /// </summary>
        public string FailureReason { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the synchronization operation failed.
        /// </summary>
        [JsonIgnore]
        public bool HasFailureReason => FailureReason.IsNotNullOrWhiteSpace();

        /// <summary>
        /// Resets the command for a new synchronization attempt.
        /// </summary>
        public void ResetForSyncAttempt()
        {
            WasCompleted = false;
            Attempts++;
            FailureReason = null;
        }

        /// <summary>
        /// Marks the command as completed and clears any existing failure reason.
        /// </summary>
        public void MarkAsCompleted()
        {
            WasCompleted = true;
            FailureReason = null;
        }

        /// <summary>
        /// Marks the command as skipped.
        /// </summary>
        /// <remarks>
        /// This should be used when the command should not be attempted or logged.
        /// </remarks>
        public void MarkAsSkipped()
        {
            WasCompleted = false;
            Attempts = AttemptLimit;
            FailureReason = null;
        }

        /// <summary>
        /// Marks the command as recoverable with a failure reason.
        /// </summary>
        /// <param name="failureReason">The reason why the command failed.</param>
        public void MarkAsRecoverable( string failureReason )
        {
            WasCompleted = false;

            var attemptCount = $"(attempt {Attempts}/{AttemptLimit})";
            FailureReason = $"{( failureReason.IsNotNullOrWhiteSpace() ? $"{failureReason} " : string.Empty )}{attemptCount}";
        }

        /// <summary>
        /// Marks the command as unrecoverable with a failure reason.
        /// </summary>
        /// <param name="failureReason">The reason why the command failed.</param>
        public void MarkAsUnrecoverable( string failureReason )
        {
            WasCompleted = false;
            Attempts = AttemptLimit;
            FailureReason = failureReason;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatToRockSyncCommand"/> class.
        /// </summary>
        /// <param name="attemptLimit">The maximum number of times to attempt a chat-to-Rock sync command before giving up.</param>
        /// <param name="chatSyncType">The type of synchronization to perform.</param>
        public ChatToRockSyncCommand( int attemptLimit, ChatSyncType chatSyncType )
        {
            AttemptLimit = attemptLimit;
            ChatSyncType = chatSyncType;
        }
    }
}
