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
namespace Rock.ViewModels.Blocks.Communication.Chat.ChatShell
{
    /// <summary>
    /// What happened when a person gave chat their birthdate, and the session as it now
    /// stands, so the shell can carry on from it without reloading.
    /// </summary>
    public class ChatBirthdateResultBag
    {
        /// <summary>
        /// Gets or sets the outcome as a stable snake_case code: "saved" when the birthdate was
        /// written, otherwise the reason nothing was.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the session after the save, exactly as the block's initialization
        /// would describe it now.
        /// </summary>
        public ChatShellSessionBag Session { get; set; }
    }
}
