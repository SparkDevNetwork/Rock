//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// ActivityType POCO Entity.
    /// </summary>
    [Table( "WorkflowActivityType" )]
    [DataContract( IsReference = true )]
    public partial class WorkflowActivityType : Model<WorkflowActivityType>, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the Active.
        /// </summary>
        /// <value>
        /// Determines is the job is currently active..
        /// </value>
        [DataMember]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the workflow type id.
        /// </summary>
        /// <value>
        /// The workflow type id.
        /// </value>
        [DataMember]
        public int WorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Friendly name for the job..
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Notes about the job..
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is activatedd with workflow.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is activatedd with workflow; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActivatedWithWorkflow { get; set; }

        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the workflow.
        /// </summary>
        /// <value>
        /// The type of the workflow.
        /// </value>
        [DataMember]
        public virtual WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the action types.
        /// </summary>
        /// <value>
        /// The action types.
        /// </value>
        [DataMember]
        public virtual ICollection<WorkflowActionType> ActionTypes
        {
            get { return _actionTypes ?? ( _actionTypes = new Collection<WorkflowActionType>() ); }
            set { _actionTypes = value; }
        }
        private ICollection<WorkflowActionType> _actionTypes;

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.WorkflowType;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// ActivityType Configuration class.
    /// </summary>
    public partial class WorkflowActivityTypeConfiguration : EntityTypeConfiguration<WorkflowActivityType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActivityTypeConfiguration"/> class.
        /// </summary>
        public WorkflowActivityTypeConfiguration()
        {
            this.HasRequired( m => m.WorkflowType ).WithMany( m => m.ActivityTypes ).HasForeignKey( m => m.WorkflowTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}

