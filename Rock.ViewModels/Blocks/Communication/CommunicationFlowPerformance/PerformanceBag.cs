using System;
using System.Collections.Generic;

using Rock.Enums.Communication;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    /// <summary>
    /// Bag containing information about the performance of a communication flow.
    /// </summary>
    public class PerformanceBag
    {
        /// <summary>
        /// Gets or sets the communication flow identifier key.
        /// </summary>
        public string CommunicationFlowIdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the communication flow.
        /// </summary>
        public string CommunicationFlowName { get; set; }

        /// <summary>
        /// Gets or sets the type of the trigger that kicks off new communication flow instances.
        /// </summary>
        public CommunicationFlowTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the type of the conversion goal.
        /// </summary>
        public ConversionGoalType? ConversionGoalType { get; set; }

        /// <summary>
        /// Gets or sets the conversion goal target percent.
        /// </summary>
        public decimal? ConversionGoalTargetPercent { get; set; }

        /// <summary>
        /// Gets or sets the conversion goal timeframe in days.
        /// </summary>
        public int? ConversionGoalTimeframeInDays { get; set; }

        /// <summary>
        /// Gets or sets the conversion goal settings.
        /// </summary>
        public ConversionGoalSettingsBag ConversionGoalSettings { get; set; }

        /// <summary>
        /// Gets or sets the messages that are part of this communication flow's instances.
        /// </summary>
        public List<MessageBag> Messages { get; set; }
    }
}
