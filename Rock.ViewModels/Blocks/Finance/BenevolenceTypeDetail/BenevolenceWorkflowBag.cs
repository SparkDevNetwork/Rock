using Rock.ViewModels.Utility;
using System;

namespace Rock.ViewModels.Blocks.Finance.BenevolenceTypeDetail
{
    public class BenevolenceWorkflowBag
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public string Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow type.
        /// </summary>
        /// <value>
        /// The name of the workflow type.
        /// </value>
        public string WorkflowTypeName { get; set; }

        /// <summary>
        /// Gets or sets the type of the workflow.
        /// </summary>
        /// <value>
        /// The type of the workflow.
        /// </value>
        public ListItemBag WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the trigger.
        /// </summary>
        /// <value>
        /// The triger.
        /// </value>
        public string Trigger { get; set; }

        /// <summary>
        /// Gets or sets the benevolence type identifier.
        /// </summary>
        /// <value>
        /// The benevolence type identifier.
        /// </value>
        public int? BenevolenceTypeId { get; set; }

        /// <summary>
        /// Gets or sets the qualifier.
        /// </summary>
        /// <value>
        /// The qualifier.
        /// </value>
        public string PrimaryQualifier { get; set; }

        /// <summary>
        /// Gets or sets the secondary qualifier.
        /// </summary>
        /// <value>
        /// The secondary qualifier.
        /// </value>
        public string SecondaryQualifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is inherited.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is inherited; otherwise, <c>false</c>.
        /// </value>
        public bool IsInherited { get; set; }
    }
}
