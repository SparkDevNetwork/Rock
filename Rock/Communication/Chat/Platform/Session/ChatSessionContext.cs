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
using System.Collections.Generic;

using Rock.Communication.Chat.Platform.Configuration;

namespace Rock.Communication.Chat.Platform.Session
{
    /// <summary>
    /// What the session gates need for one open: the church's settings, and the
    /// people the Direct Message Access Data View resolved to. The second is a
    /// lookup result rather than a setting, which is why it is here and not on the
    /// configuration, and it is resolved once at session open rather than per gate.
    /// </summary>
    internal sealed class ChatSessionContext
    {
        /// <summary>
        /// The church's settings, as stored, with the signing key decrypted.
        /// </summary>
        public ChatPlatformConfiguration Configuration { get; set; }

        /// <summary>
        /// The people the Direct Message Access Data View resolved to, or null when
        /// the church named no Data View and anyone may start a direct message.
        /// </summary>
        public ISet<int> DirectMessageAccessPersonIds { get; set; }
    }
}
