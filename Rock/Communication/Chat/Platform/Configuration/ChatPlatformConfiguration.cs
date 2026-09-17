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

namespace Rock.Communication.Chat.Platform.Configuration
{
    /// <summary>
    /// The church's chat settings. Two halves: the settings an administrator owns,
    /// and the half the platform issues when chat is enabled, which the administrator
    /// sees but never types.
    /// </summary>
    internal sealed class ChatPlatformConfiguration
    {
        #region Church half

        /// <summary>
        /// Whether a person's profile details are visible to other people by default.
        /// A person's own setting wins where they have one.
        /// </summary>
        public bool AreChatProfilesVisible { get; set; }

        /// <summary>
        /// Whether a person may be direct messaged by anyone by default.
        /// A person's own setting wins where they have one.
        /// </summary>
        public bool IsOpenDirectMessagingAllowed { get; set; }

        /// <summary>
        /// The youngest age that may use chat at all, or null for no limit.
        /// A person whose birthdate is unknown is refused rather than allowed.
        /// </summary>
        public int? MinimumAge { get; set; }

        /// <summary>
        /// A Data View naming the people who may start a direct message, or null when
        /// anyone may.
        /// </summary>
        public Guid? DirectMessageAccessDataViewGuid { get; set; }

        /// <summary>
        /// Data Views whose members carry a badge in chat.
        /// </summary>
        public List<Guid> ChatBadgeDataViewGuids { get; set; }

        /// <summary>
        /// The church's private signing key, as a JWK. Held encrypted at rest and
        /// decrypted only in memory; it never reaches a view model or a log line.
        /// </summary>
        public string PrivateKey { get; set; }

        /// <summary>
        /// The key exactly as it sits in storage, still encrypted, kept beside the
        /// decrypted one so a save can put it back untouched. A database restored onto an
        /// installation with a different encryption key decrypts to nothing, and without
        /// this the first save made from that reading would write the nothing over it.
        /// Never stored itself.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string StoredPrivateKey { get; set; }

        #endregion Church half

        #region Platform half

        /// <summary>
        /// The church's id on the chat platform.
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// The chat platform project this church talks to.
        /// </summary>
        public string ProjectUrl { get; set; }

        /// <summary>
        /// The key the browser presents to that project. Public by design.
        /// </summary>
        public string PublishableKey { get; set; }

        /// <summary>
        /// The id of the key pair registered for this church.
        /// </summary>
        public string Kid { get; set; }

        #endregion Platform half

        /// <summary>
        /// True when chat has everything it needs to run: a church to be, somewhere to
        /// reach, a key the browser may use, and a key to sign with. One question with
        /// one answer, because the configuration screen and the session gates both ask
        /// it and a church that saw two different answers could not be told why.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsConfigured =>
            TenantId.HasValue
            && !string.IsNullOrWhiteSpace( PrivateKey )
            && !string.IsNullOrWhiteSpace( ProjectUrl )
            && !string.IsNullOrWhiteSpace( PublishableKey );
    }
}
