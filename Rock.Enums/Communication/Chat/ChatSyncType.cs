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
namespace Rock.Enums.Communication.Chat
{
    /// <summary>
    /// The types of chat synchronization operations that can be performed.
    /// </summary>
    [Enums.EnumDomain( "Communication" )]
    public enum ChatSyncType
    {
        /// <summary>
        /// Skip, as no changes are needed.
        /// </summary>
        Skip = 0,

        /// <summary>
        /// Create a new record.
        /// </summary>
        Create = 1,

        /// <summary>
        /// Update an existing record.
        /// </summary>
        Update = 2,

        /// <summary>
        /// Delete an existing record.
        /// </summary>
        Delete = 3,

        /// <summary>
        /// Mute an existing record.
        /// </summary>
        Mute = 4,

        /// <summary>
        /// Unmute an existing record.
        /// </summary>
        Unmute = 5,

        /// <summary>
        /// Ban an existing record.
        /// </summary>
        Ban = 6,

        /// <summary>
        /// Unban an existing record.
        /// </summary>
        Unban = 7
    }
}
