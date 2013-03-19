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
    /// Action POCO Entity.
    /// </summary>
    [Table( "WorkflowAction" )]
    [DataContract]
    public partial class WorkflowAction : Model<WorkflowAction>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the activity id.
        /// </summary>
        /// <value>
        /// The activity id.
        /// </value>
        [DataMember]
        public int ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the activity type id.
        /// </summary>
        /// <value>
        /// The activity type id.
        /// </value>
        [DataMember]
        public int ActionTypeId { get; set; }

        /// <summary>
        /// Gets or sets the last processed date time.
        /// </summary>
        /// <value>
        /// The last processed date time.
        /// </value>
        [DataMember]
        public DateTime? LastProcessedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the completed date time.
        /// </summary>
        /// <value>
        /// The completed date time.
        /// </value>
        [DataMember]
        public DateTime? CompletedDateTime { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the  activity.
        /// </summary>
        /// <value>
        /// The activity.
        /// </value>
        [DataMember]
        public virtual WorkflowActivity Activity { get; set; }

        /// <summary>
        /// Gets or sets the type of the activity.
        /// </summary>
        /// <value>
        /// The type of the activity.
        /// </value>
        [DataMember]
        public virtual WorkflowActionType ActionType { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsActive
        {
            get
            {
                return !CompletedDateTime.HasValue;
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
                return this.Activity;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes this instance.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.SystemException"></exception>
        internal virtual bool Process( IEntity entity, out List<string> errorMessages )
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
        /// Adds a log entry.
        /// </summary>
        /// <param name="logEntry">The log entry.</param>
        public virtual void AddLogEntry( string logEntry )
        {
            if ( this.Activity != null &&
                this.Activity.Workflow != null )
            {
                this.Activity.Workflow.AddLogEntry( string.Format( "'{0}' Action: {1}", this.ToString(), logEntry ) );
            }
        }

        /// <summary>
        /// Marks this action as complete.
        /// </summary>
        public virtual void MarkComplete()
        {
            CompletedDateTime = DateTime.Now;
            AddSystemLogEntry( "Completed" );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
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
        /// <param name="logEntry">The log entry.</param>
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
        /// Activates the specified action type.
        /// </summary>
        /// <param name="actionType">Type of the action.</param>
        /// <param name="activity">The activity.</param>
        /// <returns></returns>
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

