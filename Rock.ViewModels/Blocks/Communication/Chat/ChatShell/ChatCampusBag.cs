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
    /// One of the church's active campuses, as the chat names it wherever a room or a person
    /// carries a campus.
    /// </summary>
    public class ChatCampusBag
    {
        /// <summary>
        /// Gets or sets the campus's guid, the value a room or a person carries.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the campus's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the campus's short code, or null when it has none.
        /// </summary>
        public string ShortCode { get; set; }
    }
}
