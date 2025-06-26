using System.Collections.Generic;

using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Reporting;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    public class InitializationBox : BlockBox
    {
        public CommunicationFlowBag CommunicationFlow { get; set; }
        public GridDefinitionBag GridDefinition { get; set; }
    }
}
