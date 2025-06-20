using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowMessagePerformance
{
    /// <summary>
    /// Bag containing information about a communication flow instance communication for the Communication Flow Message Performance block.
    /// </summary>
    public class CommunicationFlowMessagePerformanceFlowInstanceCommunicationBag
    {
        /// <summary>
        /// Gets or sets the unique identifier key for the communication flow instance communication.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the list of recipient metrics associated with this communication flow instance communication.
        /// </summary>
        public List<CommunicationFlowMessagePerformanceRecipientMetricsBag> RecipientMetrics { get; set; }
    }
}
