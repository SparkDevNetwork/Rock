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
    /// Represents a persisted <see cref="Rock.Model.Workflow"/> execution/instance in RockChMS.
    /// </summary>
    [Table( "Workflow" )]
    [DataContract]
    public partial class Workflow : Model<Workflow>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the WorkflowTypeId of the <see cref="Rock.Model.WorkflowType"/> that this Workflow instance is executing.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the WorkflowTypeId fo the <see cref="Rock.Model.WorkflowType"/> that is being executed.
        /// </value>
        [DataMember]
        public int WorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets a friendly name for this Workflow instance. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing a friendly name of this Workflow instance.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description or summary about this Workflow instance.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description or summary about this Workflow instance.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the status of this Workflow instance. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the status of this Workflow instance.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether this instance is processing.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is processing; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsProcessing { get; set; }

        /// <summary>
        /// Gets or sets the date and time that this Workflow instance was activated.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> that represents the date and time that this Workflow instance was activated.
        /// </value>
        [DataMember]
        public DateTime? ActivatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the Workflow was last processed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> that represents when the Workflow was last processed.
        /// </value>
        [DataMember]
        public DateTime? LastProcessedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the Workflow completed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the Workflow completed.
        /// </value>
        [DataMember]
        public DateTime? CompletedDateTime { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowType"/> that is being executed in this persisted Workflow instance.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowType"/> that is being executed in this persisted Workflow instance.
        /// </value>
        [DataMember]
        public virtual WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// Gets a flag indicating whether this Workflow instance is active.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> value that is <c>true</c> if this Workflow instance is active; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsActive
        {
            get
            {
                return ActivatedDateTime.HasValue && !CompletedDateTime.HasValue;
            }
        }

        /// <summary>
        /// Gets or sets a collection containing all the <see cref="Rock.Model.WorkflowActivity">WorkflowActivities</see> that are a part of this Workflow instance.
        /// </summary>
        /// <value>
        /// A collection containing the <see cref="Rock.Model.WorkflowActivity">WorkflowActivities</see> that are a part of this Workflow instance.
        /// </value>
        [DataMember]
        public virtual ICollection<WorkflowActivity> Activities 
        {
            get { return _activities ?? ( _activities = new Collection<WorkflowActivity>() ); }
            set { _activities = value; }
        }
        private ICollection<WorkflowActivity> _activities;

        /// <summary>
        /// Gets an enumerable collection of the Active <see cref="Rock.Model.WorkflowActivity">WorkflowActivities</see> for this Workflow instance, ordered by their order value.
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
        /// Gets a flag indicating whether this instance has active activities.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance has active activities; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasActiveActivities
        {
            get
            {
                return this.Activities.Any( a => a.IsActive );
            }
        }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.WorkflowLog" /> entries for this Workflow instance.
        /// </summary>
        /// <value>
        /// A collection containing the <see cref="Rock.Model.WorkflowLog"/> entries for this Workflow instance.
        /// </value>
        [DataMember]
        public virtual ICollection<WorkflowLog> LogEntries
        {
            get { return _logEntries ?? ( _logEntries = new Collection<WorkflowLog>() ); }
            set { _logEntries = value; }
        }
        private ICollection<WorkflowLog> _logEntries;

        /// <summary>
        /// Gets the parent security authority for this Workflow instance.
        /// </summary>
        /// <value>
        /// The parent authority for this Workflow instance.
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
        /// Processes this Workflow instance.
        /// </summary>
        /// <returns>A <see cref="System.Boolean"/> value that is <c>true</c> if the Workflow processed successfully; otherwise <c>false</c>.</returns>
        public virtual bool Process( out List<string> errorMessages)
        {
            bool result = Process( null, out errorMessages );
            return result;
        }

        /// <summary>
        /// Processes this instance.
        /// </summary>
        /// <param name="entity">The entity that work is being performed against.</param>
        /// <param name="errorMessages">A <see cref="System.Collections.Generic.List{String}"/> that will contain and any error messages that occur
        /// while the Workflow is being processed.</param>
        /// <returns>A <see cref="System.Boolean"/> that is <c>true</c> if the workflow processed sucessfully.</returns>
        public virtual bool Process( Object entity, out List<string> errorMessages )
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
        /// Adds a <see cref="Rock.Model.WorkflowLog"/> entry.
        /// </summary>
        /// <param name="logEntry">A <see cref="System.String"/>containing the log entry.</param>
        public virtual void AddLogEntry( string logEntry )
        {
            var workflowLog = new WorkflowLog();
            workflowLog.LogDateTime = DateTime.Now;
            workflowLog.LogText = logEntry;

            this.LogEntries.Add( workflowLog );
        }

        /// <summary>
        /// Marks this Workflow as complete.
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
        /// <param name="processStartTime">A <see cref="System.DateTime"/> that represents the process start time.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">A <see cref="System.Collections.Generic.List{String}"/> containing error messages for any 
        ///  errors that occurred while the activity was being processed..</param>
        /// <returns>A <see cref="System.Boolean"/> value that is <c>true</c> if the activity processed successfully; otherwise <c>false</c>.</returns>
        private bool ProcessActivity( DateTime processStartTime, Object entity, out List<string> errorMessages )
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
        /// <param name="logEntry">A <see cref="System.String"/> containing the log entry.</param>
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
        /// Activates the specified <see cref="Rock.Model.WorkflowType"/>.
        /// </summary>
        /// <param name="workflowType">The <see cref="Rock.Model.WorkflowType"/>  being activated.</param>
        /// <param name="name">A <see cref="System.String"/> representing the name of the <see cref="Rock.Model.Workflow"/> instance.</param>
        /// <returns>The <see cref="Rock.Model.Workflow"/> instance.</returns>
        public static Workflow Activate( WorkflowType workflowType, string name )
        {
            var workflow = new Workflow();
            workflow.WorkflowTypeId = workflowType.Id;
            workflow.Name = name ?? workflowType.Name;
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

