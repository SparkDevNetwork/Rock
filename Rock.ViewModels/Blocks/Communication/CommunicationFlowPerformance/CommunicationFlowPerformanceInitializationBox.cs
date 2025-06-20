using System.Collections.Generic;

using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Reporting;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    public class CommunicationFlowPerformanceInitializationBox : BlockBox
    {
        public CommunicationFlowPerformanceFlowBag CommunicationFlow { get; set; }
        public GridDefinitionBag GridDefinition { get; set; }
    }
}
