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

namespace Rock.ViewModels.Blocks.Cms.ContentChannelItemView
{
    /// <summary>
    /// The configuration values supplied to the <see cref="Rock.Blocks.Cms.ContentChannelItemView"/> Obsidian client.
    /// </summary>
    public class ContentChannelItemViewOptionsBag
    {
        /// <summary>
        /// Gets or sets an opaque, encrypted token the client passes back when registering a "View"
        /// interaction for this page render. The token carries a server-issued interaction Guid,
        /// the content channel item Id, and an expiration timestamp; the client cannot read or
        /// modify it. <c>null</c> when this request is not eligible to log an interaction (logging
        /// disabled, request from a known crawler, logged-in-only with no current person, or no
        /// content channel item resolved). The client also uses the token value as its
        /// sessionStorage de-duplication key so browser-back navigation doesn't double-count.
        /// </summary>
        public string InteractionToken { get; set; }

        /// <summary>
        /// Gets or sets the lifetime, in seconds, of the <see cref="InteractionToken"/>. The
        /// client uses this as the TTL on its de-duplication cache entries so they expire on the
        /// same schedule the server would reject the token. Always a positive value when
        /// <see cref="InteractionToken"/> is set.
        /// </summary>
        public int InteractionTokenLifetimeSeconds { get; set; }
    }
}
