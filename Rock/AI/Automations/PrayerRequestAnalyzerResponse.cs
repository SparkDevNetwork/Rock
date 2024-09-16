namespace Rock.AI.Automations
{
    /// <summary>
    /// The expected response format for an AI Completion response to a request for analysis of text.
    /// </summary>
    public class PrayerRequestAnalyzerResponse
    {
        /// <summary>
        /// The identifier of the sentiment chosen by the AI completion.
        /// </summary>
        public int? SentimentId;

        /// <summary>
        /// The identifier of the category chosen by the AI completion.
        /// </summary>
        public int? CategoryId;

        /// <summary>
        /// <see langword="true"/> if the text was determined to be appropriate for public viewing; otherwise, <see langword="false"/>.
        /// </summary>
        public bool? IsAppropriateForPublic;
    }
}
