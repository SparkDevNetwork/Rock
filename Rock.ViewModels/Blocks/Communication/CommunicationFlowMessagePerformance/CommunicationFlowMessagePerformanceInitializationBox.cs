using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowMessagePerformance
{
    /// <summary>
    /// Box containing the data needed to initialize the Communication Flow Message Performance block.
    /// </summary>
    public class CommunicationFlowMessagePerformanceInitializationBox
    {
        /// <summary>
        /// Gets or sets the high-level flow communication details.
        /// </summary>
        public CommunicationFlowMessagePerformanceFlowCommunicationBag FlowCommunication { get; set; }
    }
}
