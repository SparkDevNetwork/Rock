//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using Rock.Data;

namespace Rock.Util
{
    /// <summary>
    /// Activity POCO Entity.
    /// </summary>
    [Table( "utilActivity" )]
    public partial class Activity : Model<Activity>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the workflow id.
        /// </summary>
        /// <value>
        /// The workflow id.
        /// </value>
        public int WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the activity type id.
        /// </summary>
        /// <value>
        /// The activity type id.
        /// </value>
        public int ActivityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the activated date time.
        /// </summary>
        /// <value>
        /// The activated date time.
        /// </value>
        public DateTime? ActivatedDateTime { get; set; }

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
        /// Gets or sets the  workflow.
        /// </summary>
        /// <value>
        /// The workflow.
        /// </value>
        public virtual Workflow Workflow { get; set; }

        /// <summary>
        /// Gets or sets the type of the activity.
        /// </summary>
        /// <value>
        /// The type of the activity.
        /// </value>
        public virtual ActivityType ActivityType { get; set; }

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
                return ActivatedDateTime.HasValue && !CompletedDateTime.HasValue;
            }
        }

        /// <summary>
        /// Gets or sets the activities.
        /// </summary>
        /// <value>
        /// The activities.
        /// </value>
        public virtual ICollection<Action> Actions
        {
            get { return _actions ?? ( _actions = new Collection<Action>() ); }
            set { _actions = value; }
        }
        private ICollection<Action> _actions;

        /// <summary>
        /// Gets the active actions.
        /// </summary>
        /// <value>
        /// The active actions.
        /// </value>
        public virtual IEnumerable<Rock.Util.Action> ActiveActions
        {
            get
            {
                return this.Actions
                    .Where( a => a.IsActive )
                    .OrderBy( a => a.ActionType.Order );
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
                return this.Workflow;
            }
        }

        #endregion

        #region Public Methods


        /// <summary>
        /// Processes this instance.
        /// </summary>
        public virtual void Process()
        {
            AddSystemLogEntry( "Processing..." );

            this.LastProcessedDateTime = DateTime.Now;
            
            foreach ( var action in this.ActiveActions )
            {
                action.Process();

                // If action inactivated this activity, exit
                if ( !this.IsActive )
                    break;
            }

            AddSystemLogEntry( "Processing Complete" );

        }

        /// <summary>
        /// Adds a log entry.
        /// </summary>
        /// <param name="logEntry">The log entry.</param>
        public virtual void AddLogEntry( string logEntry )
        {
            if ( this.Workflow != null )
            {
                this.Workflow.AddLogEntry( string.Format( "{0}: {1}", this.ToString(), logEntry ) );
            }
        }

        /// <summary>
        /// Marks this activity as complete.
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
            return string.Format( "{0}[{1}]", this.ActivityType.ToString(), this.Id );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Logs a system event.
        /// </summary>
        /// <param name="logEntry">The log entry.</param>
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
        /// Activates the specified activity type.
        /// </summary>
        /// <param name="activityType">Type of the activity.</param>
        /// <param name="workflow">The workflow.</param>
        /// <returns></returns>
        internal static Activity Activate( ActivityType activityType, Workflow workflow )
        {
            var activity = new Activity();
            activity.Workflow = workflow;
            activity.ActivityType = activityType;
            activity.ActivatedDateTime = DateTime.Now;

            activity.AddSystemLogEntry( "Activated" );

            foreach ( var actionType in activityType.ActionTypes )
            {
                activity.Actions.Add( Action.Activate(actionType, activity) );
            }

            return activity;
        }

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static Activity Read( int id )
        {
            return Read<Activity>( id );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static Activity Read( Guid guid )
        {
            return Read<Activity>( guid );
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Activity Configuration class.
    /// </summary>
    public partial class ActivityConfiguration : EntityTypeConfiguration<Activity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityConfiguration"/> class.
        /// </summary>
        public ActivityConfiguration()
        {
            this.HasRequired( m => m.Workflow ).WithMany( m => m.Activities).HasForeignKey( m => m.WorkflowId ).WillCascadeOnDelete( true );
            this.HasRequired( m => m.ActivityType ).WithMany().HasForeignKey( m => m.ActivityTypeId).WillCascadeOnDelete( false );
        }
    }

    #endregion

}

