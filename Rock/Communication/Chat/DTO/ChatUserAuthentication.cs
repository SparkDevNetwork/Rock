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
namespace Rock.Communication.Chat.DTO
{
    /// <summary>
    /// Represents the token - as well as the <see cref="ChatUser.Key"/> that was used to obtain the token - to be used
    /// when authenticating with the chat provider.
    /// </summary>
    internal class ChatUserAuthentication
    {
        /// <summary>
        /// Gets or sets the token to be used when authenticating with the chat provider.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ChatUser.Key"/> that was used to obtain the token.
        /// </summary>
        public string ChatUserKey { get; set; }
    }
}
