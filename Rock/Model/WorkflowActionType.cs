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
    /// Represents an <see cref="Rock.Model.WorkflowActionType"/> (action or task) that is performed as part of a <see cref="Rock.Model.WorkflowActionType"/>.
    /// </summary>
    [Table( "WorkflowActionType" )]
    [DataContract]
    public partial class WorkflowActionType : Model<WorkflowActionType>, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the ActivityTypeId of the <see cref="Rock.Model.WorkflowActivityType"/> that performs this Action Type.
        /// </summary>
        /// <value>
        /// The activity type id.
        /// </value>
        [DataMember]
        public int ActivityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the ActionType
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the ActionType.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order of the ActionType in the <see cref="Rock.Model.WorkflowActivityType" />
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that the action is operating against.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that the action is operating against.
        /// </value>
        [DataMember]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is action completed on success.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is action completed on success; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActionCompletedOnSuccess { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is activity completed on success.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this instance is activity completed on success; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActivityCompletedOnSuccess { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowActivityType"/> that performs this ActionType.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowActivityType" /> that performs this ActionType.
        /// </value>
        [DataMember]
        public virtual WorkflowActivityType ActivityType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of that this ActionType is running against.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets the <see cref="Rock.Workflow.ActionComponent"/>
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Workflow.ActionComponent"/>
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
        /// Gets the parent security authority for this ActionType.
        /// </summary>
        /// <value>
        /// The parent security authority for this ActionType.
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

