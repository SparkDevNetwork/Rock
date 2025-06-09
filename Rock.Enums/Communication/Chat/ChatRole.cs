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
using System.ComponentModel;

namespace Rock.Enums.Communication.Chat
{
    /// <summary>
    /// The role of a chat individual, to be synchronized with the external chat system.
    /// </summary>
    [Enums.EnumDomain( "Communication" )]
    public enum ChatRole
    {
        /// <summary>
        /// Represents a rock_user role in the external chat system.
        /// </summary>
        [Description( "rock_user" )]
        User = 0,

        /// <summary>
        /// Represents a rock_moderator role in the external chat system.
        /// </summary>
        [Description( "rock_moderator" )]
        Moderator = 1,

        /// <summary>
        /// Represents a rock_admin role in the external chat system.
        /// </summary>
        [Description( "rock_admin" )]
        Administrator = 2
    }
}
