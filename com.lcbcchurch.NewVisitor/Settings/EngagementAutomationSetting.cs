using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.lcbcchurch.NewVisitor.Settings
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class EngagementAutomationSetting
    {
        public EngagementAutomationSetting()
        {
            ScoringItems = new List<ScoringItem>();
        }

        /// <summary>
        /// Gets or sets the end date attribute identifier.
        /// </summary>
        /// <value>
        /// The end date attribute identifier.
        /// </value>
        public Guid? BeginDateAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the weeks in engagement window.
        /// </summary>
        /// <value>
        /// The weeks in engagement window.
        /// </value>
        public int WeeksInEngagementWindow { get; set; } = 16;

        /// <summary>
        /// Gets or sets the score attribute identifier.
        /// </summary>
        /// <value>
        /// The score attribute identifier.
        /// </value>
        public Guid? ScoreAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the scoring items.
        /// </summary>
        /// <value>
        /// The scoring items.
        /// </value>
        public List<ScoringItem> ScoringItems { get; set; }
    }
}
