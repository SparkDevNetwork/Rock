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
namespace Rock.Migrations
{
    /// <summary>
    /// A migration that adds the default Chat Reactions defined type with the populated values.
    /// </summary>
    public partial class AddDefaultChatReactions : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// A guid for the text reaction attribute.
        /// </summary>
        private readonly string _textReactionGuid = "C3F37466-CBC4-44FD-9ACF-1FE79DFC0B56";  

        /// <summary>
        /// The guid for the image reaction attribute.
        /// </summary>
        private readonly string _imageReactionGuid = "86ECA0E7-440D-463C-A2AE-2F1F5674E1F8";

        /// <inheritdoc />
        public override void Up()
        {
            // 1) Create the defined type
            RockMigrationHelper.AddDefinedType(
                "Communication",
                "Chat Reactions",
                "Defines the set of reactions available in the Chat View block. " +
                "Each reaction can be a text emoji (e.g. 👍) or an image (recommended 48×48 px SVG). " +
                "If both are provided, the image takes precedence.",
                Rock.SystemGuid.DefinedType.CHAT_REACTION,
                "Category for chat message reactions."
            );

            // 2) Add attributes for text vs. image reaction
            RockMigrationHelper.AddDefinedTypeAttribute(
                Rock.SystemGuid.DefinedType.CHAT_REACTION,
                SystemGuid.FieldType.TEXT,
                "Text Emoji",
                "TextReaction",
                "Unicode or emoji character to use for this reaction (e.g. 👍).",
                0,
                string.Empty,
                _textReactionGuid
            );

            RockMigrationHelper.AddDefinedTypeAttribute(
                Rock.SystemGuid.DefinedType.CHAT_REACTION,
                SystemGuid.FieldType.IMAGE,
                "Image Reaction",
                "ImageReaction",
                "Image file to use for this reaction (recommended 48×48 px; supports SVG, PNG, JPG).",
                1,
                string.Empty,
                _imageReactionGuid
            );

            // 3) Seed standard text-based reactions
            AddChatReaction(
                "love",
                "Heart reaction to show love.",
                Rock.SystemGuid.DefinedValue.CHAT_REACTION_LOVE,
                "❤️"
            );

            AddChatReaction(
                "haha",
                "Laugh reaction for something funny.",
                Rock.SystemGuid.DefinedValue.CHAT_REACTION_HAHA,
                "😂"
            );

            AddChatReaction(
                "like",
                "Thumbs-up reaction to show approval.",
                Rock.SystemGuid.DefinedValue.CHAT_REACTION_LIKE,
                "👍"
            );

            AddChatReaction(
                "sad",
                "Crying reaction to express sadness.",
                Rock.SystemGuid.DefinedValue.CHAT_REACTION_SAD,
                "😢"
            );

            AddChatReaction(
                "wow",
                "Surprised reaction for something amazing.",
                Rock.SystemGuid.DefinedValue.CHAT_REACTION_WOW,
                "😮"
            );
        }

        /// <inheritdoc />
        public override void Down()
        {
            // Remove defined values in reverse order
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.CHAT_REACTION_WOW );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.CHAT_REACTION_SAD );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.CHAT_REACTION_LIKE );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.CHAT_REACTION_HAHA );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.CHAT_REACTION_LOVE );

            RockMigrationHelper.DeleteAttribute( _imageReactionGuid );
            RockMigrationHelper.DeleteAttribute( _textReactionGuid );

            // Remove the defined type itself
            RockMigrationHelper.DeleteDefinedType( Rock.SystemGuid.DefinedType.CHAT_REACTION );
        }

        /// <summary>
        /// Adds a single chat‐reaction defined value and sets its text-emoji attribute.
        /// </summary>
        private void AddChatReaction( string key, string description, string valueGuid, string emoji )
        {
            RockMigrationHelper.AddDefinedValue(
                Rock.SystemGuid.DefinedType.CHAT_REACTION,
                key,
                description,
                valueGuid,
                true
            );

            RockMigrationHelper.AddDefinedValueAttributeValue(
                valueGuid,
                _textReactionGuid,
                emoji
            );
        }
    }
}
