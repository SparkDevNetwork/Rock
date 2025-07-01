using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    /// <summary>
    /// Bag containing a row of data for the grid in the Communication Flow Performance block.
    /// </summary>
    public class GridRowBag
    {
        /// <summary>
        /// Gets or sets the communication flow instance identifier key.
        /// </summary>
        public string CommunicationFlowCommunicationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the communication flow instance identifier key.
        /// </summary>
        public string CommunicationFlowInstanceIdKey { get; set; }

        /// <summary>
        /// Gets or sets the instance start date.
        /// </summary>
        public DateTime CommunicationFlowInstanceStartDate { get; set; }

        /// <summary>
        /// Gets or sets the communication flow instance communication identifier key.
        /// </summary>
        public string CommunicationFlowInstanceCommunicationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the communication flow communication.
        /// </summary>
        public string CommunicationFlowCommunicationName { get; set; }

        /// <summary>
        /// Gets or sets the type of the communication.
        /// </summary>
        public Enums.Communication.CommunicationType CommunicationType { get; set; }

        /// <summary>
        /// Gets or sets the number of clicks.
        /// </summary>
        public int Clicks { get; set; }

        /// <summary>
        /// Gets or sets the number of opens.
        /// </summary>
        public int Opens { get; set; }

        /// <summary>
        /// Gets or sets the number of messages sent.
        /// </summary>
        public int Sent { get; set; }

        /// <summary>
        /// Gets or sets the number of unsubscribes.
        /// </summary>
        public int Unsubscribes { get; set; }

        /// <summary>
        /// Gets or sets the number of conversions.
        /// </summary>
        public int Conversions { get; set; }
    }
}
