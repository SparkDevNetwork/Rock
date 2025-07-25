namespace Rock.Enums.Communication.Chat
{
    /// <summary>
    /// Represents the visual style of the chat experience.
    /// </summary>
    public enum ChatViewStyle
    {
        /// <summary>
        /// A simple, text-message-style layout.
        /// This mode emphasizes linear conversations and is ideal for one-on-one or small group chats.
        /// Messages appear in a flowing, bubble-style format similar to traditional SMS or mobile messaging.
        /// </summary>
        Conversational,

        /// <summary>
        /// A more structured layout for larger group communication.
        /// This mode supports organized discussions with clear visual separation,
        /// making it easier to follow multiple topics or conversations within a group.
        /// Best suited for teams, communities, or discussions with multiple participants.
        /// </summary>
        Community
    }
}
