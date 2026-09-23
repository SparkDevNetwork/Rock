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

using Rock.ViewModels.Blocks;

namespace Rock.ViewModels.Blocks.Communication.Chat.ChatShell
{
    /// <summary>
    /// What the chat shell is given when it opens.
    /// </summary>
    public class ChatShellInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets whether chat may open for this person and, if so, where the platform is.
        /// </summary>
        public ChatShellSessionBag Session { get; set; }

        /// <summary>
        /// Gets or sets the channel named by the ChannelGuid page parameter, which opens first
        /// when present, or null.
        /// </summary>
        public Guid? ChannelGuid { get; set; }
    }
}
