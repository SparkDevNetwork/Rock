using System;

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowMessagePerformance
{
    /// <summary>
    /// Bag containing the metrics for a recipient of a communication flow message.
    /// </summary>
    public class CommunicationFlowMessagePerformanceRecipientMetricsBag
    {
        /// <summary>
        /// Gets or sets the unique identifier key for the recipient person.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier key for the parent Communication Flow Instance Communication.
        /// </summary>
        public string FlowInstanceCommunicationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the person associated with the recipient metrics.
        /// </summary>
        public PersonFieldBag Person { get; set; }

        /// <summary>
        /// Gets or sets the date the communication was sent to the recipient.
        /// </summary>
        public DateTime? SentDate { get; set; }

        /// <summary>
        /// Gets or sets the latest date the communication was opened by the recipient.
        /// </summary>
        public DateTime? OpenedDate { get; set; }

        /// <summary>
        /// Gets or sets the latest date the recipient clicked on a link in the communication.
        /// </summary>
        public DateTime? ClickedDate { get; set; }

        /// <summary>
        /// Gets or sets the date when the recipient achieved the conversion goal for the communication flow instance.
        /// </summary>
        public DateTime? ConversionDate { get; set; }

        /// <summary>
        /// Gets or sets the date when the recipient unsubscribed from the communication.
        /// </summary>
        public DateTime? UnsubscribeDate { get; set; }
    }
}
