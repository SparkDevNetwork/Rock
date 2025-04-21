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
using Rock.Model;
using Rock.Utilities;

namespace Rock.Communication.Chat.DTO
{
    /// <summary>
    /// Represents a chat badge in the external chat system and membership within a <see cref="DataView"/> in Rock.
    /// </summary>
    internal class ChatBadge
    {
        /// <summary>
        /// Gets or sets the badge key.
        /// </summary>
        /// <value>
        /// The IdKey of the corresponding, chat-related <see cref="DataView"/>.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the badge name.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="DataView.Name"/>.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the badge icon CSS class.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="DataView.IconCssClass"/>.
        /// </value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the badge background color.
        /// </summary>
        /// <value>
        /// The <see cref="ColorPair.BackgroundColor"/> from the corresponding <see cref="DataView.HighlightColor"/>.
        /// </value>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the badge foreground color.
        /// </summary>
        /// <value>
        /// The <see cref="ColorPair.ForegroundColor"/> from the corresponding <see cref="DataView.HighlightColor"/>.
        /// </value>
        public string ForegroundColor { get; set; }
    }
}
