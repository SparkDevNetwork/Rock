using System;

using Rock.Model;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    public class CommunicationFlowPerformanceRecipientBag
    {
        public bool CausedUnsubscribe { get; set; }

        public UnsubscribeLevel? UnsubscribeLevel { get; set; }

        public DateTime? UnsubscribeDateTime { get; set; }
    }
}
