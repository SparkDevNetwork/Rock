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
    /// Represents a reaction that can be used in a chat message, including its key, optional image, and display text.
    /// </summary>
    public class ChatReactionBag
    {
        /// <summary>
        /// Gets or sets the unique key that identifies the reaction.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the image URL.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the medium image url.
        /// </summary>
        public string ImageUrlMedium { get; set; }

        /// <summary>
        /// Gets or sets the small image URL, which is a scaled down version of the main <see cref="ImageUrl" />.
        /// </summary>
        public string ImageUrlSmall { get; set; }

        /// <summary>
        /// Gets or sets the text that represents the reaction (e.g 😲).
        /// </summary>
        public string ReactionText { get; set; }
    }
}
