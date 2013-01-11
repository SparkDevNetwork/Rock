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
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Workflow POCO Entity.
    /// </summary>
    [Table( "Workflow" )]
    [DataContract( IsReference = true )]
    public partial class Workflow : Model<Workflow>
    {

        #region Entity Properties

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
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is processing.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is processing; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsProcessing { get; set; }

        /// <summary>
        /// Gets or sets the activated date time.
        /// </summary>
        /// <value>
        /// The activated date time.
        /// </value>
        [DataMember]
        public DateTime? ActivatedDateTime { get; set; }

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
        /// Gets or sets the type of the workflow.
        /// </summary>
        /// <value>
        /// The type of the workflow.
        /// </value>
        [DataMember]
        public virtual WorkflowType WorkflowType { get; set; }

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
        [DataMember]
        public virtual ICollection<WorkflowActivity> Activities 
        {
            get { return _activities ?? ( _activities = new Collection<WorkflowActivity>() ); }
            set { _activities = value; }
        }
        private ICollection<WorkflowActivity> _activities;

        /// <summary>
        /// Gets the active activities.
        /// </summary>
        /// <value>
        /// The active activities.
        /// </value>
        public virtual IEnumerable<WorkflowActivity> ActiveActivities
        {
            get
            {
                return this.Activities
                    .Where( a => a.IsActive )
                    .OrderBy( a => a.ActivityType.Order );
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has active activities.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has active activities; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasActiveActivities
        {
            get
            {
                return this.Activities.Any( a => a.IsActive );
            }
        }

        /// <summary>
        /// Gets or sets the log entries.
        /// </summary>
        /// <value>
        /// The log entries.
        /// </value>
        [DataMember]
        public virtual ICollection<WorkflowLog> LogEntries
        {
            get { return _logEntries ?? ( _logEntries = new Collection<WorkflowLog>() ); }
            set { _logEntries = value; }
        }
        private ICollection<WorkflowLog> _logEntries;

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

        #region Public Methods

        /// <summary>
        /// Processes this instance.
        /// </summary>
        /// <returns></returns>
        public virtual bool Process( out List<string> errorMessages)
        {
            bool result = Process( null, out errorMessages );
            return result;
        }

        /// <summary>
        /// Processes this instance.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public virtual bool Process( IEntity entity, out List<string> errorMessages )
        {
            AddSystemLogEntry( "Processing..." );

            DateTime processStartTime = DateTime.Now;

            while ( ProcessActivity( processStartTime, entity, out errorMessages )
                && errorMessages.Count == 0 ) { }

            this.LastProcessedDateTime = DateTime.Now;

            AddSystemLogEntry( "Processing Complete" );

            if ( !this.HasActiveActivities )
            {
                MarkComplete();
            }

            return errorMessages.Count == 0;
        }

        /// <summary>
        /// Adds a log entry.
        /// </summary>
        /// <param name="logEntry">The log entry.</param>
        public virtual void AddLogEntry( string logEntry )
        {
            var workflowLog = new WorkflowLog();
            workflowLog.LogDateTime = DateTime.Now;
            workflowLog.LogText = logEntry;

            this.LogEntries.Add( workflowLog );
        }

        /// <summary>
        /// Marks this workflow as complete.
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
            return this.Name;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Processes the activity.
        /// </summary>
        /// <param name="processStartTime">The process start time.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        private bool ProcessActivity( DateTime processStartTime, IEntity entity, out List<string> errorMessages )
        {
            if ( this.IsActive )
            {
                foreach ( var activity in this.ActiveActivities )
                {
                    if ( !activity.LastProcessedDateTime.HasValue ||
                        activity.LastProcessedDateTime.Value.CompareTo( processStartTime ) < 0 )
                    {
                        return activity.Process( entity, out errorMessages );
                    }
                }
            }

            errorMessages = new List<string>();
            return false;
        }

        /// <summary>
        /// adds a system log entry
        /// </summary>
        /// <param name="logEntry">The log entry.</param>
        private void AddSystemLogEntry( string logEntry )
        {
            if ( this.WorkflowType != null && (
                this.WorkflowType.LoggingLevel == WorkflowLoggingLevel.Workflow ||
                this.WorkflowType.LoggingLevel == WorkflowLoggingLevel.Activity ||
                this.WorkflowType.LoggingLevel == WorkflowLoggingLevel.Action ))
            {
                AddLogEntry( logEntry );
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Activates the specified workflow type.
        /// </summary>
        /// <param name="workflowType">Type of the workflow.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static Workflow Activate( WorkflowType workflowType, string name )
        {
            var workflow = new Workflow();
            workflow.WorkflowTypeId = workflowType.Id;
            workflow.Name = name;
            workflow.Status = "Activated";
            workflow.IsProcessing = false;
            workflow.ActivatedDateTime = DateTime.Now;
            workflow.LoadAttributes();

            workflow.AddSystemLogEntry( "Activated" );

            foreach ( var activityType in workflowType.ActivityTypes.OrderBy( a => a.Order ) )
            {
                if ( activityType.IsActivatedWithWorkflow)
                {
                    WorkflowActivity.Activate(activityType, workflow);
                }
            }

            return workflow;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Workflow Configuration class.
    /// </summary>
    public partial class WorkflowConfiguration : EntityTypeConfiguration<Workflow>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowConfiguration"/> class.
        /// </summary>
        public WorkflowConfiguration()
        {
            this.HasRequired( m => m.WorkflowType ).WithMany().HasForeignKey( m => m.WorkflowTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}

