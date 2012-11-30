//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Rock.Data;

namespace Rock.Util
{
    /// <summary>
    /// Action POCO Entity.
    /// </summary>
    [Table( "WorkflowAction" )]
    public partial class Action : Model<Action>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the activity id.
        /// </summary>
        /// <value>
        /// The activity id.
        /// </value>
        public int ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the activity type id.
        /// </summary>
        /// <value>
        /// The activity type id.
        /// </value>
        public int ActionTypeId { get; set; }

        /// <summary>
        /// Gets or sets the last processed date time.
        /// </summary>
        /// <value>
        /// The last processed date time.
        /// </value>
        public DateTime? LastProcessedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the completed date time.
        /// </summary>
        /// <value>
        /// The completed date time.
        /// </value>
        public DateTime? CompletedDateTime { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the  activity.
        /// </summary>
        /// <value>
        /// The activity.
        /// </value>
        public virtual Activity Activity { get; set; }

        /// <summary>
        /// Gets or sets the type of the activity.
        /// </summary>
        /// <value>
        /// The type of the activity.
        /// </value>
        public virtual ActionType ActionType { get; set; }

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

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return this.ToDto(); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes this instance.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.SystemException"></exception>
        internal virtual bool Process( IDto dto, out List<string> errorMessages )
        {
            AddSystemLogEntry( "Processing..." );

            WorkflowActionComponent workflowAction = this.ActionType.WorkflowAction;
            if ( workflowAction == null )
            {
                throw new SystemException( string.Format( "The '{0}' component does not exist, or is not active", workflowAction));
            }

            this.ActionType.LoadAttributes();

            bool success = workflowAction.Execute( this, dto, out errorMessages );

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
        internal static Action Activate( ActionType actionType, Activity activity)
        {
            var action = new Action();
            action.Activity = activity;
            action.ActionType = actionType;

            action.AddSystemLogEntry( "Activated" );

            return action;
        }

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static Action Read( int id )
        {
            return Read<Action>( id );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static Action Read( Guid guid )
        {
            return Read<Action>( guid );
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Action Configuration class.
    /// </summary>
    public partial class ActionConfiguration : EntityTypeConfiguration<Action>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionConfiguration"/> class.
        /// </summary>
        public ActionConfiguration()
        {
            this.HasRequired( m => m.Activity ).WithMany( m => m.Actions ).HasForeignKey( m => m.ActivityId ).WillCascadeOnDelete( true );
            this.HasRequired( m => m.ActionType ).WithMany().HasForeignKey( m => m.ActionTypeId).WillCascadeOnDelete( false );
        }
    }

    #endregion

}

