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
    /// The types of chat messages events that can be reported by the external chat system.
    /// </summary>
    [Enums.EnumDomain( "Communication" )]
    public enum ChatMessageEventType
    {
        /// <summary>
        /// A new message was added.
        /// </summary>
        New = 0,

        /// <summary>
        /// An existing message was updated.
        /// </summary>
        Update = 1
    }
}
