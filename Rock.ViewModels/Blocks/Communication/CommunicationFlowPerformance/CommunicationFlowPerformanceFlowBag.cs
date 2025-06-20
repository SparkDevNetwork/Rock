
using System.Collections.Generic;

using Rock.Enums.Communication;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    public class CommunicationFlowPerformanceFlowBag
    {
        public ConversionGoalType? ConversionGoalType { get; set; }

        public decimal? ConversionGoalTargetPercent { get; set; }

        public int? ConversionGoalTimeframeInDays { get; set; }

        public CommunicationFlowPerformanceConversionGoalSettingsBag ConversionGoalSettings { get; set; }

        public CommunicationFlowTriggerType TriggerType { get; set; }

        public List<CommunicationFlowPerformanceFlowInstanceBag> Instances { get; set; }

        public string Name { get; set; }

        public string IdKey { get; set; }
    }
}
