//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a persisted WorkflowActivity in RockChMS
    /// </summary>
    [Table( "WorkflowActivity" )]
    [DataContract]
    public partial class WorkflowActivity : Model<WorkflowActivity>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the WorkflowId of the <see cref="Rock.Model.Workflow"/> instance that is performing this WorkflowActivity.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the WorkflowId of the <see cref="Rock.Model.Workflow"/> instance that is performing this WorkflowActivity.
        /// </value>
        [DataMember]
        public int WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the ActivityTypeId of the <see cref="Rock.Model.WorkflowActivityType"/> that is being executed.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the ActivityTypeID of the <see cref="Rock.Model.WorkflowActivity"/> that is being performed.
        /// </value>
        [DataMember]
        public int ActivityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the date and time that this WorkflowActivity was activated.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that this WorkflowActivity was activated.
        /// </value>
        [DataMember]
        public DateTime? ActivatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that this WorkflowActivity was last processed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that this WorkflowActivity was last processed.
        /// </value>
        [DataMember]
        public DateTime? LastProcessedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that this WorkflowActivity completed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that this WorkflowActivity completed.
        /// </value>
        [DataMember]
        public DateTime? CompletedDateTime { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Workflow"/> instance that is performing this WorkflowActivity.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Workflow"/> instance that is performing this WorkflowActivity.
        /// </value>
        [DataMember]
        public virtual Workflow Workflow { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowActivityType"/> that is being performed by this WorkflowActivity instance.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowActivityType"/> that is being performed by this WorkflowActivity instance.
        /// </value>
        [DataMember]
        public virtual WorkflowActivityType ActivityType { get; set; }

        /// <summary>
        /// Gets a value indicating whether this WorkflowActivity instance is active.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsActive
        {
            get
            {
                return ActivatedDateTime.HasValue && !CompletedDateTime.HasValue;
            }
        }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.WorkflowAction">WorkflowActions</see> that are run by this WorkflowActivity.
        /// </summary>
        /// <value>
        /// A collection containing the <see cref="Rock.Model.WorkflowAction">WorkflowActions</see> that are being run by this WorkflowActivity.
        /// </value>
        [DataMember]
        public virtual ICollection<WorkflowAction> Actions
        {
            get { return _actions ?? ( _actions = new Collection<WorkflowAction>() ); }
            set { _actions = value; }
        }
        private ICollection<WorkflowAction> _actions;

        /// <summary>
        /// Gets an enumerable collection containing the active <see cref="Rock.Model.WorkflowAction">WorkflowActions</see> for this WorkflowActivity, ordered by their order property.
        /// </summary>
        /// <value>
        /// An enumerable collection containing the active <see cref="Rock.Model.WorkflowAction">WorkflowActions</see> for this WorkflowActivity.
        /// </value>
        public virtual IEnumerable<Rock.Model.WorkflowAction> ActiveActions
        {
            get
            {
                return this.Actions
                    .Where( a => a.IsActive )
                    .OrderBy( a => a.ActionType.Order );
            }
        }

        /// <summary>
        /// Gets the parent security authority for this WorkflowAction.
        /// </summary>
        /// <value>
        /// The parent security authority for this Workflow action.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.Workflow;
            }
        }

        #endregion

        #region Public Methods


        /// <summary>
        /// Processes this WorkflowAction
        /// </summary>
        /// <param name="entity">The entity that work is being performed against.</param>
        /// <param name="errorMessages">A <see cref="System.Collections.Generic.List{String}"/> that will contain any error messages that are 
        /// returned while processing this WorkflowActivity</param>
        /// <returns>A <see cref="System.Boolean"/> vlaue that is <c>true</c> if the WorkflowActivity processes successfully; otherwise <c>false</c>.</returns>
        internal virtual bool Process( Object entity, out List<string> errorMessages )
        {
            AddSystemLogEntry( "Processing..." );

            this.LastProcessedDateTime = DateTime.Now;

            errorMessages = new List<string>();

            foreach ( var action in this.ActiveActions )
            {
                List<string> actionErrorMessages;
                bool actionSuccess = action.Process( entity, out actionErrorMessages );
                errorMessages.AddRange( actionErrorMessages );

                // If action was not successful, exit
                if ( !actionSuccess )
                {
                    break;
                }

                // If action completed this activity, exit
                if ( !this.IsActive )
                {
                    break;
                }

                // If action completed this workflow, exit
                if ( !this.Workflow.IsActive )
                {
                    break;
                }
            }

            AddSystemLogEntry( "Processing Complete" );

            return errorMessages.Count == 0;
        }

        /// <summary>
        /// Adds a <see cref="Rock.Model.WorkflowLog"/> entry.
        /// </summary>
        /// <param name="logEntry">A <see cref="System.String"/> representing the body of the log entry.</param>
        public virtual void AddLogEntry( string logEntry )
        {
            if ( this.Workflow != null )
            {
                this.Workflow.AddLogEntry( string.Format( "'{0}' Activity: {1}", this.ToString(), logEntry ) );
            }
        }

        /// <summary>
        /// Marks this WorkflowActivity as complete.
        /// </summary>
        public virtual void MarkComplete()
        {
            CompletedDateTime = DateTime.Now;
            AddSystemLogEntry( "Completed" );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this WorkflowActivity.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this WorkflowActivity.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0}[{1}]", this.ActivityType.ToString(), this.Id );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Logs a system event to the <see cref="Rock.Model.WorkflowLog"/>
        /// </summary>
        /// <param name="logEntry">A <see cref="System.String"/> representing the log entry.</param>
        private void AddSystemLogEntry( string logEntry )
        {
            if ( this.Workflow != null &&
                this.Workflow.WorkflowType != null &&
                ( this.Workflow.WorkflowType.LoggingLevel == WorkflowLoggingLevel.Activity ||
                this.Workflow.WorkflowType.LoggingLevel == WorkflowLoggingLevel.Action ) )
            {
                AddLogEntry( logEntry );
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Activates the specified WorkflowActivity
        /// </summary>
        /// <param name="activityType">The <see cref="Rock.Model.WorkflowActivityType"/> to activate.</param>
        /// <param name="workflow">The persisted <see cref="Rock.Model.Workflow"/> instance that this Workflow activity belongs to.</param>
        /// <returns>The activated <see cref="Rock.Model.WorkflowActivity"/>.</returns>
        public static WorkflowActivity Activate( WorkflowActivityType activityType, Workflow workflow )
        {
            var activity = new WorkflowActivity();
            activity.Workflow = workflow;
            activity.ActivityType = activityType;
            activity.ActivatedDateTime = DateTime.Now;
            activity.LoadAttributes();

            activity.AddSystemLogEntry( "Activated" );

            foreach ( var actionType in activityType.ActionTypes )
            {
                activity.Actions.Add( WorkflowAction.Activate(actionType, activity) );
            }

            workflow.Activities.Add( activity );

            return activity;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// WorkflowActivity Configuration class.
    /// </summary>
    public partial class WorkflowActivityConfiguration : EntityTypeConfiguration<WorkflowActivity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActivityConfiguration"/> class.
        /// </summary>
        public WorkflowActivityConfiguration()
        {
            this.HasRequired( m => m.Workflow ).WithMany( m => m.Activities).HasForeignKey( m => m.WorkflowId ).WillCascadeOnDelete( true );
            this.HasRequired( m => m.ActivityType ).WithMany().HasForeignKey( m => m.ActivityTypeId).WillCascadeOnDelete( false );
        }
    }

    #endregion

}

