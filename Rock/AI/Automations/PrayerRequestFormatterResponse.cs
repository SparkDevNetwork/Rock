namespace Rock.AI.Automations
{
    /// <summary>
    /// The expected response format for an AI Completion response to a request for text changes.
    /// </summary>
    public class PrayerRequestFormatterResponse
    {
        /// <summary>
        /// The modified text.
        /// </summary>
        public string Content { get; set; }
    }
}
