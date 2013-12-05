//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Workflow;

namespace Rock.Model
{
    /// <summary>
    /// Represents a persisted WorkflowAction in RockChMS.
    /// </summary>
    [Table( "WorkflowAction" )]
    [DataContract]
    public partial class WorkflowAction : Model<WorkflowAction>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the WorkflowActivityId of the <see cref="Rock.Model.WorkflowActivity"/> that this WorkflowAction is a part of.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> represents the WorflowActivityId that this WorkflowAction is a part of.
        /// </value>
        [DataMember]
        public int ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the ActionTypeId of the <see cref="Rock.Model.WorkflowAction"/> that is being executed by this instance.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the ActionTypeId of the <see cref="Rock.Model.WorkflowActionType"/> that is being executed on this instance.
        /// </value>
        [DataMember]
        public int ActionTypeId { get; set; }

        /// <summary>
        /// Gets or sets the date and time that this WorkflowAction was last processed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that this WorkflowAction was last processed.
        /// </value>
        [DataMember]
        public DateTime? LastProcessedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the WorkflowAction completed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the WorkflowAction completed.
        /// </value>
        [DataMember]
        public DateTime? CompletedDateTime { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowActivity"/> that contains the WorkflowAction.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowActivity"/> that contains this WorkflowAction.
        /// </value>
        [DataMember]
        public virtual WorkflowActivity Activity { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowActionType"/> that is being executed by this WorkflowAction.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowActionType"/> that is being executed.
        /// </value>
        [DataMember]
        public virtual WorkflowActionType ActionType { get; set; }

        /// <summary>
        /// Gets a value indicating whether this WorkflowAction is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this WorkflowAction is active; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsActive
        {
            get
            {
                return !CompletedDateTime.HasValue;
            }
        }

        /// <summary>
        /// Gets the parent security authority for this WorkflowAction.
        /// </summary>
        /// <value>
        /// The parent security authority for this WorkflowAction.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.Activity;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes this WorkflowAction.
        /// </summary>
        /// <param name="entity">The entity that the WorkflowAction is operating against.</param>
        /// <param name="errorMessages">A <see cref="System.Collections.Generic.List{String}"/> that will contain any error messages that occur while processing the WorkflowAction.</param>
        /// <returns>A <see cref="System.Boolean"/> value that is <c>true</c> if the process completed successfully; otherwise <c>false</c>.</returns>
        /// <exception cref="System.SystemException"></exception>
        internal virtual bool Process( Object entity, out List<string> errorMessages )
        {
            AddSystemLogEntry( "Processing..." );

            ActionComponent workflowAction = this.ActionType.WorkflowAction;
            if ( workflowAction == null )
            {
                throw new SystemException( string.Format( "The '{0}' component does not exist, or is not active", workflowAction));
            }

            this.ActionType.LoadAttributes();

            bool success = workflowAction.Execute( this, entity, out errorMessages );

            AddSystemLogEntry( string.Format( "Processing Complete (Success:{0})", success.ToString() ) );

            if ( success )
            {
                if ( this.ActionType.IsActionCompletedOnSuccess )
                {
                    this.MarkComplete();
                }

                if ( this.ActionType.IsActivityCompletedOnSuccess )
                {
                    this.Activity.MarkComplete();
                }
            }

            return success;
        }

        /// <summary>
        /// Adds a <see cref="Rock.Model.WorkflowLog"/> entry.
        /// </summary>
        /// <param name="logEntry">A <see cref="System.String"/> representing the  log entry.</param>
        public virtual void AddLogEntry( string logEntry )
        {
            if ( this.Activity != null &&
                this.Activity.Workflow != null )
            {
                this.Activity.Workflow.AddLogEntry( string.Format( "'{0}' Action: {1}", this.ToString(), logEntry ) );
            }
        }

        /// <summary>
        /// Marks this WorkflowAction as complete.
        /// </summary>
        public virtual void MarkComplete()
        {
            CompletedDateTime = DateTime.Now;
            AddSystemLogEntry( "Completed" );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this WorkflowAction.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this WorkflowAction..
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0}[{1}]", this.ActionType.ToString(), this.Id );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Logs a system event.
        /// </summary>
        /// <param name="logEntry">A <see cref="System.String"/>representing the log entry.</param>
        private void AddSystemLogEntry( string logEntry )
        {
            if ( this.Activity != null &&
                this.Activity.Workflow != null &&
                this.Activity.Workflow.WorkflowType != null &&
                this.Activity.Workflow.WorkflowType.LoggingLevel == WorkflowLoggingLevel.Action )
            {
                AddLogEntry( logEntry );
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Activates the specified <see cref="Rock.Model.WorkflowAction"/>.
        /// </summary>
        /// <param name="actionType">The <see cref="Rock.Model.WorkflowActionType"/> to be activated.</param>
        /// <param name="activity">The <see cref="Rock.Model.WorkflowActivity"/> that this WorkflowAction belongs to..</param>
        /// <returns>The <see cref="Rock.Model.WorkflowAction"/></returns>
        internal static WorkflowAction Activate( WorkflowActionType actionType, WorkflowActivity activity)
        {
            var action = new WorkflowAction();
            action.Activity = activity;
            action.ActionType = actionType;
            action.LoadAttributes();

            action.AddSystemLogEntry( "Activated" );

            return action;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Action Configuration class.
    /// </summary>
    public partial class WorkflowActionConfiguration : EntityTypeConfiguration<WorkflowAction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActionConfiguration"/> class.
        /// </summary>
        public WorkflowActionConfiguration()
        {
            this.HasRequired( m => m.Activity ).WithMany( m => m.Actions ).HasForeignKey( m => m.ActivityId ).WillCascadeOnDelete( true );
            this.HasRequired( m => m.ActionType ).WithMany().HasForeignKey( m => m.ActionTypeId).WillCascadeOnDelete( false );
        }
    }

    #endregion

}

