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

using Rock.Attribute;
using Rock.Model;

namespace Rock.Communication.Chat
{
    /// <summary>
    /// Chat configuration to dictate how Rock's chat feature behaves; some configuration values will be synchronized
    /// with the external chat application.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "17.0", true )]
    public class ChatConfiguration
    {
        /// <summary>
        /// Gets or sets the API key for Rock to use when interacting with the external chat application.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the API secret for Rock to use when interacting with the external chat application.
        /// </summary>
        public string ApiSecret { get; set; }

        /// <summary>
        /// Gets or sets the system default for whether individuals' profiles are visible in the external chat application.
        /// This can be overridden per individual, by the value of <see cref="Person.IsChatProfilePublic"/>.
        /// </summary>
        public bool AreChatProfilesVisible { get; set; }

        /// <summary>
        /// Gets or sets the system default for whether individuals can receive direct messages from anybody in the system.
        /// This can be overridden per individual, by the value of <see cref="Person.IsChatOpenDirectMessageAllowed"/>.
        /// </summary>
        public bool IsOpenDirectMessagingAllowed { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="Rock.Model.Workflow"/> type that will be launched every time a chat user is created.
        /// </summary>
        public Guid? WelcomeWorkflowTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets a list of unique identifiers, for <see cref="DataView"/>s that will be used to populate badges
        /// in the external chat application.
        /// </summary>
        public List<Guid> ChatBadgeDataViewGuids { get; set; }
    }
}