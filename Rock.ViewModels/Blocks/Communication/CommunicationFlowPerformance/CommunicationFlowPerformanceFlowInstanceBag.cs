
using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    public class CommunicationFlowPerformanceFlowInstanceBag
    {
        public string IdKey { get; set; }

        public DateTime? StartDate { get; set; }

        public List<CommunicationFlowPerformanceConversionHistoryBag> Conversions { get; set; }

        public List<CommunicationFlowPerformanceRecipientBag> Recipients { get; set; }

        public bool HasSentCommunications { get; set; }

        public bool HasUnsentCommunications { get; set; }

        public List<CommunicationFlowPerformanceInstanceCommunicationBag> Communications { get; set; }
    }
}
