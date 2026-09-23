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

namespace Rock.ViewModels.Blocks.Communication.Chat.ChatShell
{
    /// <summary>
    /// A short-lived church token for the person, which the shell exchanges with the chat
    /// platform for its own session token, or the gate that refused it.
    /// </summary>
    public class ChatChurchTokenBag
    {
        /// <summary>
        /// Gets or sets the outcome of the session gates as a stable snake_case code, "ok" when a
        /// token was signed.
        /// </summary>
        public string Gate { get; set; }

        /// <summary>
        /// Gets or sets the signed church token, when the gates passed.
        /// </summary>
        public string ChurchToken { get; set; }

        /// <summary>
        /// Gets or sets when the church token expires, when the gates passed.
        /// </summary>
        public DateTimeOffset? ExpiresAt { get; set; }
    }
}
