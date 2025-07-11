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
