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

using Rock.Configuration.ConnectedServices;

namespace Rock.Communication.Chat.Platform.Configuration
{
    /// <summary>
    /// The credentials a church is given when chat is enabled, read off the connected
    /// services entry they arrive on. This is also the set of fields Enable Chat owns:
    /// the writer that stores them takes this type and nothing wider, so it cannot
    /// reach a setting an administrator owns.
    /// </summary>
    internal sealed class ConnectedServicesChatEntry
    {
        /// <summary>
        /// The church's id on the chat platform.
        /// </summary>
        public Guid TenantId { get; set; }

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

        /// <summary>
        /// The church's private signing key, as a JWK, in the clear. It is encrypted
        /// on the way into storage and never reaches a view model or a log line.
        /// </summary>
        public string PrivateKey { get; set; }

        /// <summary>
        /// Reads the credentials off a connected services entry, or returns null when
        /// any of them is missing or the signing key is not one that can be signed
        /// with. A church that is given three of the four, or a key nothing can use,
        /// reads as set up and fails at its first token, with nothing on any screen to
        /// say why and no second Enable to press, so a set like that is refused whole
        /// rather than stored and discovered later by a gate.
        /// </summary>
        /// <param name="entry">The service entry the gateway answered an enable with.</param>
        /// <returns>The credentials, or null when the entry does not carry a usable set.</returns>
        public static ConnectedServicesChatEntry FromEntry( ServiceEntry entry )
        {
            var configuration = entry?.GetConfiguration<ServiceEntryConfiguration>();

            if ( configuration == null )
            {
                return null;
            }

            var tenantId = configuration.TenantId.AsGuidOrNull();

            if ( !tenantId.HasValue
                || configuration.ProjectUrl.IsNullOrWhiteSpace()
                || configuration.PublishableKey.IsNullOrWhiteSpace()
                || !ChatSigningKey.IsUsable( configuration.PrivateKey ) )
            {
                return null;
            }

            return new ConnectedServicesChatEntry
            {
                TenantId = tenantId.Value,
                ProjectUrl = configuration.ProjectUrl,
                PublishableKey = configuration.PublishableKey,
                Kid = configuration.Kid,
                PrivateKey = configuration.PrivateKey
            };
        }

        /// <summary>
        /// The configuration object the gateway puts on the service entry. This is
        /// what the wire carries; the type above is what Rock keeps.
        /// </summary>
        private class ServiceEntryConfiguration
        {
            /// <inheritdoc cref="ConnectedServicesChatEntry.TenantId" />
            public string TenantId { get; set; }

            /// <inheritdoc cref="ConnectedServicesChatEntry.ProjectUrl" />
            public string ProjectUrl { get; set; }

            /// <inheritdoc cref="ConnectedServicesChatEntry.PublishableKey" />
            public string PublishableKey { get; set; }

            /// <inheritdoc cref="ConnectedServicesChatEntry.Kid" />
            public string Kid { get; set; }

            /// <inheritdoc cref="ConnectedServicesChatEntry.PrivateKey" />
            public string PrivateKey { get; set; }
        }
    }
}
