using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowInstanceMessageMetrics
{
    /// <summary>
    /// Bag containing the high-level flow communication (blueprint) information needed for the Communication Flow Instance Message Metrics block.
    /// </summary>
    public class CommunicationFlowInstanceMessageMetricsFlowCommunicationBag
    {
        /// <summary>
        /// Gets or sets the identifier key for this high-level flow communication.
        /// </summary>
        public string FlowCommunicationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the flow communication (blueprint).
        /// </summary>
        public string FlowCommunicationName { get; set; }

        /// <summary>
        /// Gets or sets the collection of flow instance communications that are associated with this high-level flow communication (blueprint).
        /// </summary>
        /// <remarks></remarks>
        public List<CommunicationFlowInstanceMessageMetricsFlowInstanceCommunicationBag> FlowInstanceCommunications { get; set; }
    }
}
