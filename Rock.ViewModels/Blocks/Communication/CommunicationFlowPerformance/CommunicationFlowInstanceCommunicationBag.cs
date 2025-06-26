
using Rock.Enums.Communication;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    /// <summary>
    /// Bag containing Communication Flow Instance Communication information for the Communication Flow Performance block.
    /// </summary>
    public class CommunicationFlowInstanceCommunicationBag
    {
        /// <summary>
        /// Gets or sets the unique identifier key for the Communication Flow Instance Communication.
        /// </summary>
        public string CommunicationFlowInstanceCommunicationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier key for the parent Communication Flow Communication.
        /// </summary>
        public string CommunicationFlowCommunicationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the communication type for this communication.
        /// </summary>
        public CommunicationType CommunicationType { get; set; }

        /// <summary>
        /// Gets or sets the number of recipients that were sent this communication.
        /// </summary>
        public int SentCount { get; set; }

        /// <summary>
        /// Gets or sets the number of conversions that occurred as a result of this communication.
        /// </summary>
        public int ConversionCount { get; set; }

        public int Unsubscribes { get; set; }
    }
}
