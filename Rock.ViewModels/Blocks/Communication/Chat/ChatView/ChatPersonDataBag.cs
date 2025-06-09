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
namespace Rock.ViewModels.Blocks.Communication.Chat.ChatView
{
    /// <summary>
    /// Represents a data container for a person's chat-related identification information.
    /// </summary>
    public class ChatPersonDataBag
    {
        /// <summary>
        /// Gets or sets the unique token used for authentication or session tracking in chat.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the individual using the chat system.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the individual is required to complete age verification before using chat features.
        /// </summary>
        public bool IsAgeVerificationRequired { get; set; }

        /// <summary>
        /// Gets or sets the template used for age verification.
        /// </summary>
        public string AgeVerificationTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the individual failed the age verification check.
        /// </summary>
        public bool HasFailedAgeVerification { get; set; }

        /// <summary>
        /// Gets or sets the template used for displaying age restriction messages.
        /// </summary>
        public string AgeRestrictionTemplate { get; set; }
    }
}
