using Rock.Enums.Communication;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    /// <summary>
    /// Bag containing information for a message grid item in the Communication Flow Performance block.
    /// </summary>
    public class MessageGridItemBag
    {
        /// <summary>
        /// Gets or sets the unique identifier key for the Communication Flow Communication.
        /// </summary>
        public string CommunicationFlowCommunicationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier key for the Communication Flow Instance Communication.
        /// </summary>
        public string CommunicationFlowInstanceCommunicationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the communication name.
        /// </summary>
        public string CommunicationName { get; set; }

        /// <summary>
        /// Gets or sets the communication type for the message.
        /// </summary>
        public CommunicationType CommunicationType { get; set; }

        /// <summary>
        /// Gets or sets the number of sent communications for this Communication Flow Instance Communication.
        /// </summary>
        public int Sent { get; set; }

        /// <summary>
        /// Gets or sets the number of recipient conversions for this Communication Flow Instance Communication.
        /// </summary>
        public int? Conversions { get; set; }

        /// <summary>
        /// Gets or sets the number of recipient unsubscribes for this Communication Flow Instance Communication.
        /// </summary>
        public int Unsubscribes { get; set; }

        /// <summary>
        /// Gets or sets the number of times this Communication Flow Instance Communication was opened.
        /// </summary>
        public int Opens { get; set; }
        
        /// <summary>
        /// Gets or sets the number of times this Communication Flow Instance Communication was clicked.
        /// </summary>
        public int Clicks { get; set; }
    }
}
