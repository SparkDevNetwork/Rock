//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Workflow;

namespace Rock.Model
{
    /// <summary>
    /// ActionType POCO Entity.
    /// </summary>
    [Table( "WorkflowActionType" )]
    [DataContract( IsReference = true )]
    public partial class WorkflowActionType : Model<WorkflowActionType>, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the activity type id.
        /// </summary>
        /// <value>
        /// The activity type id.
        /// </value>
        [DataMember]
        public int ActivityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Friendly name for the job..
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        [Required]
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        [DataMember]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is action completed on success.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is action completed on success; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActionCompletedOnSuccess { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is activity completed on success.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is activity completed on success; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActivityCompletedOnSuccess { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the activity.
        /// </summary>
        /// <value>
        /// The type of the activity.
        /// </value>
        [DataMember]
        public virtual WorkflowActivityType ActivityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets the workflow action.
        /// </summary>
        /// <value>
        /// The workflow action.
        /// </value>
        public virtual ActionComponent WorkflowAction
        {
            get
            {
                if ( this.EntityType != null )
                {
                    foreach ( var serviceEntry in WorkflowActionContainer.Instance.Components )
                    {
                        var component = serviceEntry.Value.Value;
                        string componentName = component.GetType().FullName;
                        if ( componentName == this.EntityType.Name )
                        {
                            return component;
                        }
                    }
                }
                return null;
            }
        }

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
                return this.ActivityType;
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
    /// ActionType Configuration class.
    /// </summary>
    public partial class WorkflowActionTypeConfiguration : EntityTypeConfiguration<WorkflowActionType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActionTypeConfiguration"/> class.
        /// </summary>
        public WorkflowActionTypeConfiguration()
        {
            this.HasRequired( m => m.ActivityType ).WithMany( m => m.ActionTypes ).HasForeignKey( m => m.ActivityTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}

