using System;
using System.Collections.Generic;

using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Reporting;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    /// <summary>
    /// Initialization box for the Communication Flow Performance block.
    /// </summary>
    public class InitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the grid definition.
        /// </summary>
        public GridDefinitionBag GridDefinition { get; set; }

        /// <summary>
        /// Gets or sets the flow performance.
        /// </summary>
        public PerformanceBag FlowPerformance { get; set; }
    }
}
