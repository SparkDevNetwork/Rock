using System;

using Rock.Enums.Communication;
using Rock.Model;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    /// <summary>
    /// Bag containing information about a specific message that was sent as part of a communication flow.
    /// </summary>
    public class MessageBag
    {
        /// <summary>
        /// Gets or sets the communication flow communication identifier key.
        /// </summary>
        public string CommunicationFlowCommunicationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the communication flow instance identifier key.
        /// </summary>
        public string CommunicationFlowInstanceIdKey { get; set; }

        /// <summary>
        /// Gets or sets the communication flow instance communication identifier key.
        /// </summary>
        public string CommunicationFlowInstanceCommunicationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the instance start date.
        /// </summary>
        public DateTime CommunicationFlowInstanceStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that this Communication Flow Instance was completed.
        /// </summary>
        /// <value>When this is not <see langword="null"/>, then no more messages will be sent for this flow.</value>
        public DateTime? CommunicationFlowInstanceCompletedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the last time the communication flow instance was processed by the system.
        /// </summary>
        public DateTime? CommunicationFlowInstanceLastProcessedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier key.
        /// </summary>
        public string PersonAliasIdKey { get; set; }

        /// <summary>
        /// Gets or sets the date/time the message was sent.
        /// </summary>
        public DateTime? SentDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date/time the message was opened.
        /// </summary>
        public DateTime? OpenedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date/time the message was clicked.
        /// </summary>
        public DateTime? ClickedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date/time the person unsubscribed.
        /// </summary>
        public DateTime? UnsubscribedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date/time the person converted.
        /// </summary>
        public DateTime? ConvertedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the unsubscribe level.
        /// </summary>
        public UnsubscribeLevel? UnsubscribeLevel { get; set; }

        /// <summary>
        /// Gets or sets the name of the communication flow communication.
        /// </summary>
        public string CommunicationFlowCommunicationName { get; set; }

        /// <summary>
        /// Gets or sets the type of the communication.
        /// </summary>
        public CommunicationType CommunicationType { get; set; }
    }
}
