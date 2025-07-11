using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowInstanceMessageMetrics
{
    /// <summary>
    /// Bag containing information about a communication flow instance communication for the Communication Flow Instance Message Metrics block.
    /// </summary>
    public class CommunicationFlowInstanceMessageMetricsFlowInstanceCommunicationBag
    {
        /// <summary>
        /// Gets or sets the unique identifier key for the communication flow instance communication.
        /// </summary>
        public string FlowInstanceCommunicationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the list of recipient metrics associated with this communication flow instance communication.
        /// </summary>
        public List<CommunicationFlowInstanceMessageMetricsRecipientMetricsBag> RecipientMetrics { get; set; }
    }
}
