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
namespace Rock.Enums.Communication
{
    /// <summary>
    /// The filter applied to the GetCommunicationConversation query
    /// </summary>
    public enum CommunicationMessageFilter
    {
        /// <summary>
        /// When this option is selected, only messages that have actual replies (and with IsRead=false) are considered
        /// </summary>
        ShowUnreadReplies = 0,

        /// <summary>
        /// When this option is selected, it would show messages that have any replies/responses (IsRead true or false)
        /// </summary>
        ShowAllReplies = 1,

        /// <summary>
        /// When this option is selected, all messages are shown regardless of any replies/responses.
        /// </summary>
        ShowAllMessages = 2,
    }
}
