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
using Rock.ViewModels.Blocks;

namespace Rock.ViewModels.Blocks.Communication.Chat.ChatConfiguration
{
    /// <summary>
    /// What the chat configuration screen is given when it opens.
    /// </summary>
    public class ChatConfigurationInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets whether chat has everything it needs to run. False means the
        /// screen shows how to enable chat rather than settings for it.
        /// </summary>
        public bool IsChatConfigured { get; set; }

        /// <summary>
        /// Gets or sets whether this organization was enabled and this Rock server cannot
        /// read the chat credentials it was given. The screen then says so instead of
        /// pointing back at Connected Services, which has nothing to offer it.
        /// </summary>
        public bool IsCredentialUnreadable { get; set; }

        /// <summary>
        /// Gets or sets the settings, when there are any to show.
        /// </summary>
        public ChatConfigurationBag Configuration { get; set; }

        /// <summary>
        /// Gets or sets where an administrator goes to enable chat.
        /// </summary>
        public string ConnectedServicesUrl { get; set; }
    }
}
