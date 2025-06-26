
using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    public class CommunicationFlowInstanceBag
    {
        /// <summary>
        /// Gets or sets the unique identifier key for the Communication Flow Instance.
        /// </summary>
        public string CommunicationFlowInstanceIdKey { get; set; }

        /// <summary>
        /// Gets or sets the start date for this Communication Flow Instance.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the successful conversions for this Communication Flow Instance.
        /// </summary>
        public List<CommunicationFlowInstanceConversionHistoryBag> Conversions { get; set; }

        /// <summary>
        /// Gets or sets the initial recipients at the start of this Communication Flow Instance.
        /// </summary>
        public List<CommunicationFlowInstanceRecipientBag> InitialRecipients { get; set; }

        public bool HasSentCommunications { get; set; }

        public bool HasUnsentCommunications { get; set; }

        /// <summary>
        /// Gets or sets the list of initial and follow-up communications for this Communication Flow Instance.
        /// </summary>
        public List<CommunicationFlowInstanceCommunicationBag> Communications { get; set; }
    }
}
