// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a persisted <see cref="Rock.Model.Workflow"/> execution/instance in Rock.
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

        /// <summary>
        /// Gets or sets the initiator person alias identifier.
        /// </summary>
        /// <value>
        /// The initiator person alias identifier.
        /// </value>
        [DataMember]
        public int? InitiatorPersonAliasId { get; set; }
        
        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowType"/> that is being executed in this persisted Workflow instance.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowType"/> that is being executed in this persisted Workflow instance.
        /// </value>
        public virtual WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the initiator person alias.
        /// </summary>
        /// <value>
        /// The initiator person alias.
        /// </value>
        public virtual PersonAlias InitiatorPersonAlias { get; set; }

        /// <summary>
        /// Gets a flag indicating whether this Workflow instance is active.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> value that is <c>true</c> if this Workflow instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual bool IsActive
        {
            get
            {
                return ActivatedDateTime.HasValue && !CompletedDateTime.HasValue;
            }
            private set { }
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
        [NotMapped]
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
        /// Gets the active activity names.
        /// </summary>
        /// <value>
        /// The active activity names.
        /// </value>
        [NotMapped]
        public virtual string ActiveActivityNames
        {
            get
            {
                return ActiveActivities.Select( a => a.ActivityType.Name ).ToList().AsDelimited( "<br/>" );
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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is persisted.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is persisted; otherwise, <c>false</c>.
        /// </value>
        [NotMapped]
        [DataMember]
        public virtual bool IsPersisted 
        {
            get
            {
                return _isPersisted || Id > 0;
            }
            set
            {
                _isPersisted = value;
            }
        }
        private bool _isPersisted = false;

        #endregion

        #region Public Methods

        /// <summary>
        /// Processes this Workflow instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>
        /// A <see cref="System.Boolean" /> value that is <c>true</c> if the Workflow processed successfully; otherwise <c>false</c>.
        /// </returns>
        public virtual bool Process( RockContext rockContext, out List<string> errorMessages)
        {
            if (!InitiatorPersonAliasId.HasValue &&
                HttpContext.Current != null && 
                HttpContext.Current.Items.Contains( "CurrentPerson" ) )
            {
                var currentPerson = HttpContext.Current.Items["CurrentPerson"] as Person;
                if ( currentPerson != null )
                {
                    InitiatorPersonAliasId = currentPerson.PrimaryAliasId;
                }
            }

            return Process( rockContext, null, out errorMessages );
        }

        /// <summary>
        /// Processes this instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="entity">The entity that work is being performed against.</param>
        /// <param name="errorMessages">A 
        /// <see cref="System.Collections.Generic.List{String}" /> that will contain and any error messages that occur
        /// while the Workflow is being processed.</param>
        /// <returns>
        /// A <see cref="System.Boolean" /> that is <c>true</c> if the workflow processed sucessfully.
        /// </returns>
        public virtual bool Process( RockContext rockContext, Object entity, out List<string> errorMessages )
        {
            AddLogEntry( "Workflow Processing..." );

            DateTime processStartTime = RockDateTime.Now;

            while ( ProcessActivity( rockContext, processStartTime, entity, out errorMessages )
                && errorMessages.Count == 0 ) { }

            this.LastProcessedDateTime = RockDateTime.Now;

            AddLogEntry( "Workflow Processing Complete" );

            if ( !this.HasActiveActivities )
            {
                MarkComplete();
            }

            return errorMessages.Count == 0;
        }

        /// <summary>
        /// Adds a <see cref="Rock.Model.WorkflowLog" /> entry.
        /// </summary>
        /// <param name="logEntry">A <see cref="System.String" />containing the log entry.</param>
        /// <param name="force">if set to <c>true</c> will ignore logging level and always add the entry.</param>
        public virtual void AddLogEntry( string logEntry, bool force = false )
        {
            if ( force || (
                this.WorkflowType != null && (
                this.WorkflowType.LoggingLevel == WorkflowLoggingLevel.Workflow ||
                this.WorkflowType.LoggingLevel == WorkflowLoggingLevel.Activity ||
                this.WorkflowType.LoggingLevel == WorkflowLoggingLevel.Action ) ) )
            {
                var workflowLog = new WorkflowLog();
                workflowLog.LogDateTime = RockDateTime.Now;
                workflowLog.LogText = logEntry;

                this.LogEntries.Add( workflowLog );
            } 
        }

        /// <summary>
        /// Marks this Workflow as complete.
        /// </summary>
        public virtual void MarkComplete()
        {
            foreach( var activity in this.Activities)
            {
                activity.MarkComplete();
            }

            CompletedDateTime = RockDateTime.Now;
            Status = "Completed";
            AddLogEntry( "Completed" );
        }

        /// <summary>
        /// Creates a DotLiquid compatible dictionary that represents the current entity object.
        /// </summary>
        /// <param name="debug">if set to <c>true</c> the entire object tree will be parsed immediately.</param>
        /// <returns>
        /// DotLiquid compatible dictionary.
        /// </returns>
        public override object ToLiquid( bool debug )
        {
            var mergeFields = base.ToLiquid( debug ) as Dictionary<string, object>;
            if ( debug )
            {
                mergeFields.Add( "WorkflowType", this.WorkflowType.ToLiquid( true ) );
            }
            else
            {
                mergeFields.Add( "WorkflowType", this.WorkflowType );
            }

            return mergeFields;
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
        /// <param name="rockContext">The rock context.</param>
        /// <param name="processStartTime">A <see cref="System.DateTime" /> that represents the process start time.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">A 
        /// <see cref="System.Collections.Generic.List{String}" /> containing error messages for any
        /// errors that occurred while the activity was being processed..</param>
        /// <returns>
        /// A <see cref="System.Boolean" /> value that is <c>true</c> if the activity processed successfully; otherwise <c>false</c>.
        /// </returns>
        private bool ProcessActivity( RockContext rockContext, DateTime processStartTime, Object entity, out List<string> errorMessages )
        {
            if ( this.IsActive )
            {
                foreach ( var activity in this.ActiveActivities )
                {
                    if ( !activity.LastProcessedDateTime.HasValue ||
                        activity.LastProcessedDateTime.Value.CompareTo( processStartTime ) < 0 )
                    {
                        if ( activity.Attributes == null)
                        {
                            activity.LoadAttributes( rockContext );
                        }

                        return activity.Process( rockContext, entity, out errorMessages );
                    }
                }
            }

            errorMessages = new List<string>();
            return false;
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
            workflow.WorkflowType = workflowType;
            workflow.WorkflowTypeId = workflowType.Id;
            
            if ( !string.IsNullOrWhiteSpace( name ) )
            {
                workflow.Name = name;
            }
            else
            {
                workflow.Name = workflowType.Name;
            }

            workflow.Status = "Active";
            workflow.IsProcessing = false;
            workflow.ActivatedDateTime = RockDateTime.Now;
            workflow.LoadAttributes();

            workflow.AddLogEntry( "Activated" );

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
            this.HasRequired( w => w.WorkflowType ).WithMany().HasForeignKey( w => w.WorkflowTypeId ).WillCascadeOnDelete( true );
            this.HasOptional( w => w.InitiatorPersonAlias ).WithMany().HasForeignKey( w => w.InitiatorPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}

