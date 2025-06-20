using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowMessagePerformance
{
    /// <summary>
    /// Bag containing the high-level flow communication (blueprint) information needed for the Communication Flow Message Performance block.
    /// </summary>
    public class CommunicationFlowMessagePerformanceFlowCommunicationBag
    {
        /// <summary>
        /// Gets or sets the identifier key for this high-level flow communication.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the flow communication (blueprint).
        /// </summary>
        public string FlowCommunicationName { get; set; }

        /// <summary>
        /// Gets or sets the collection of flow instance communications that are associated with this high-level flow communication (blueprint).
        /// </summary>
        /// <remarks></remarks>
        public List<CommunicationFlowMessagePerformanceFlowInstanceCommunicationBag> FlowInstanceCommunications { get; set; }
    }
}
