using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace com.lcbcchurch.NewVisitor.Settings
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ScoringItem
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Icon CssClass.
        /// </summary>
        /// <value>
        /// The Icon CssClass.
        /// </value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the score.
        /// </summary>
        /// <value>
        /// The score.
        /// </value>
        public int Score { get; set; }

        /// <summary>
        /// Gets or sets the scoring item type.
        /// </summary>
        /// <value>
        /// The scoring item type.
        /// </value>
        public ScoringItemType Type { get; set; }

        /// <summary>
        /// Gets or sets the entity item identifier.
        /// </summary>
        /// <value>
        /// The entity item identifier.
        /// </value>
        public List<Guid> EntityItemsGuid { get; set; }

        /// <summary>
        /// Gets or sets the entity item qualifier value.
        /// </summary>
        /// <value>
        /// The entity item qualifier value.
        /// </value>
        public string EntityItemQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether children are included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if children are included; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeChildren { get; set; }
    }
}
