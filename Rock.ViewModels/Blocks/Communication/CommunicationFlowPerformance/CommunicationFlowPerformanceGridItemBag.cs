using Rock.Enums.Communication;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    public class CommunicationFlowPerformanceGridItemBag
    {
        public object IdKey { get; set; }
        public string MessageName { get; set; }
        public CommunicationType CommunicationType { get; set; }
        public int Sent { get; set; }
        public int? Conversions { get; set; }
        public int Unsubscribes { get; set; }
        public decimal Opens { get; set; }
        public decimal Clicks { get; set; }
    }
}
