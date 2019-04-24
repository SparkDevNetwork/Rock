// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

using Rock;
using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a persisted <see cref="Rock.Model.Workflow"/> execution/instance in Rock.
    /// </summary>
    [RockDomain( "Workflow" )]
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
        /// A type specific number to uniquely identify a workflow.
        /// </summary>
        /// <value>
        /// The type identifier number.
        /// </value>
        [DataMember]
        public int WorkflowIdNumber { get; set; }

        /// <summary>
        /// Gets the workflow identifier.
        /// </summary>
        /// <value>
        /// The workflow identifier.
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        public virtual string WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets a friendly name for this Workflow instance. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing a friendly name of this Workflow instance.
        /// </value>
        [Required]
        [MaxLength( 250 )]
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
        [NotAudited]
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
        [NotAudited]
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
        [LavaInclude]
        public virtual WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// Gets the workflow type cache.
        /// </summary>
        /// <value>
        /// The workflow type cache.
        /// </value>
        [LavaInclude]
        public virtual WorkflowTypeCache WorkflowTypeCache
        {
            get
            {
                if ( WorkflowTypeId > 0 )
                {
                    return WorkflowTypeCache.Get( WorkflowTypeId );
                }
                else if ( WorkflowType != null )
                {
                    return WorkflowTypeCache.Get( WorkflowType.Id );
                }
                return null;
            }
        }

        /// <summary>
        /// Gets or sets the initiator person alias.
        /// </summary>
        /// <value>
        /// The initiator person alias.
        /// </value>
        [DataMember]
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
                    .ToList()
                    .OrderBy( a => a.ActivityTypeCache.Order );
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
                return ActiveActivities.Select( a => a.ActivityTypeCache.Name ).ToList().AsDelimited( "<br/>" );
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
        /// Gets the parent security authority for this Workflow instance.
        /// </summary>
        /// <value>
        /// The parent authority for this Workflow instance.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                var workflowTypeCache = this.WorkflowTypeCache;
                return workflowTypeCache != null ? workflowTypeCache : base.ParentAuthority;
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

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public override object this[object key]
        {
            get
            {
                string propertyKey = key.ToStringSafe();
                if ( propertyKey == "WorkflowType" )
                {
                    return WorkflowTypeCache;
                }
                return base[key];
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Processes the activities.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        internal bool ProcessActivities( RockContext rockContext, Object entity, out List<string> errorMessages )
        {
            AddLogEntry( "Workflow Processing..." );

            DateTime processStartTime = RockDateTime.Now;

            if ( Attributes == null )
            {
                this.LoadAttributes( rockContext );
            }

            SetInitiator();

            while ( ProcessActivity( rockContext, processStartTime, entity, out errorMessages )
                && errorMessages.Count == 0 )
            { }

            this.LastProcessedDateTime = RockDateTime.Now;

            AddLogEntry( "Workflow Processing Complete" );

            if ( !this.HasActiveActivities )
            {
                MarkComplete();
            }

            return errorMessages.Count == 0;
        }

        /// <summary>
        /// Sets the initiator.
        /// </summary>
        internal void SetInitiator()
        {
            if ( !InitiatorPersonAliasId.HasValue &&
                HttpContext.Current != null &&
                HttpContext.Current.Items.Contains( "CurrentPerson" ) )
            {
                var currentPerson = HttpContext.Current.Items["CurrentPerson"] as Person;
                if ( currentPerson != null )
                {
                    InitiatorPersonAliasId = currentPerson.PrimaryAliasId;
                }
            }
        }

        /// <summary>
        /// Marks the complete.
        /// </summary>
        public virtual void MarkComplete()
        {
            MarkComplete( CompletedDateTime.HasValue ? "" : "Completed" );
        }

        /// <summary>
        /// Marks this Workflow as complete.
        /// </summary>
        public virtual void MarkComplete( string status )
        {
            foreach ( var activity in this.Activities )
            {
                activity.MarkComplete();
            }

            CompletedDateTime = RockDateTime.Now;
            if ( !string.IsNullOrWhiteSpace( status ) )
            {
                Status = status;
            }

            AddLogEntry( "Completed" );
        }

        /// <summary>
        /// Determines whether this workflow instance has an active entry form for the selected person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public bool HasActiveEntryForm( Person person )
        {
            if ( IsActive )
            {
                var canEdit = IsAuthorized( Authorization.EDIT, person );

                // Find first active action form
                int personId = person != null ? person.Id : 0;
                foreach ( var activity in Activities
                    .Where( a =>
                        a.IsActive &&
                        (
                            ( canEdit ) ||
                            ( !a.AssignedGroupId.HasValue && !a.AssignedPersonAliasId.HasValue ) ||
                            ( a.AssignedPersonAlias != null && a.AssignedPersonAlias.PersonId == personId ) ||
                            ( a.AssignedGroup != null && a.AssignedGroup.Members.Any( m => m.PersonId == personId ) )
                        )
                    )
                    .ToList()
                    .OrderBy( a => a.ActivityTypeCache.Order ) )
                {
                    if ( canEdit || ( activity.ActivityTypeCache.IsAuthorized( Authorization.VIEW, person ) ) )
                    {
                        foreach ( var action in activity.ActiveActions )
                        {
                            var actionType = action.ActionTypeCache;
                            if ( actionType != null && actionType.WorkflowForm != null && action.IsCriteriaValid )
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
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

        /// <summary>
        /// Gets the unsaved log entries.
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<LogEntry> GetUnsavedLogEntries()
        {
            return ( _logEntries ?? new List<LogEntry>() ).ToList().AsReadOnly();
        }

        /// <summary>
        /// 
        /// </summary>
        public class LogEntry
        {
            /// <summary>
            /// Gets or sets the log date time.
            /// </summary>
            /// <value>
            /// The log date time.
            /// </value>
            public DateTime LogDateTime { get; set; }

            /// <summary>
            /// Gets or sets the log text.
            /// </summary>
            /// <value>
            /// The log text.
            /// </value>
            public string LogText { get; set; }
        }

        /// <summary>
        /// The log entries cache that will get saved in PreSaveChanges
        /// </summary>
        private ICollection<LogEntry> _logEntries;

        /// <summary>
        /// Adds a <see cref="Rock.Model.WorkflowLog" /> entry.
        /// </summary>
        /// <param name="logText">The log text.</param>
        /// <param name="force">if set to <c>true</c> will ignore logging level and always add the entry.</param>
        public virtual void AddLogEntry( string logText, bool force = false )
        {
            var workflowType = this.WorkflowTypeCache;
            if ( force || (
                workflowType != null && (
                workflowType.LoggingLevel == WorkflowLoggingLevel.Workflow ||
                workflowType.LoggingLevel == WorkflowLoggingLevel.Activity ||
                workflowType.LoggingLevel == WorkflowLoggingLevel.Action ) ) )
            {
                LogEntry logEntry = new LogEntry();
                logEntry.LogDateTime = RockDateTime.Now;
                logEntry.LogText = logText;

                if ( _logEntries == null )
                {
                    _logEntries = new List<LogEntry>();
                }

                this._logEntries.Add( logEntry );
            }
        }

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, EntityState state )
        {
            if ( _logEntries != null )
            {
                if ( _logEntries.Any() )
                {
                    if ( dbContext is RockContext )
                    {
                        var workflowLogService = new WorkflowLogService( ( dbContext as RockContext ) );
                        foreach ( var logEntry in _logEntries )
                        {
                            workflowLogService.Add( new WorkflowLog { LogDateTime = logEntry.LogDateTime, LogText = logEntry.LogText, WorkflowId = this.Id } );
                        }

                        _logEntries.Clear();
                    }
                }
            }

            // Set the workflow number
            if ( state == EntityState.Added )
            {
                int maxNumber = new WorkflowService( dbContext as RockContext )
                    .Queryable().AsNoTracking()
                    .Where( w => w.WorkflowTypeId == this.WorkflowTypeId )
                    .Max( w => (int?)w.WorkflowIdNumber ) ?? 0;
                this.WorkflowIdNumber = maxNumber + 1;
            }

            base.PreSaveChanges( dbContext, state );
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
                        if ( activity.Attributes == null )
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
        [RockObsolete( "1.7" )]
        [Obsolete( "For improved performance, use the Activate method that takes a WorkflowTypeCache parameter instead. IMPORTANT NOTE: When using the new method, the Workflow object that is returned by that method will not have the WorkflowType property set. If you are referencing the WorkflowType property on a Workflow returned by that method, you will get a Null Reference Exception! You should use the new WorkflowTypeCache property on the workflow instead.", true )]
        public static Workflow Activate( WorkflowType workflowType, string name )
        {
            using ( var rockContext = new RockContext() )
            {
                return Activate( workflowType, name, rockContext );
            }
        }

        /// <summary>
        /// Activates the specified <see cref="Rock.Model.WorkflowType" />.
        /// </summary>
        /// <param name="workflowType">The <see cref="Rock.Model.WorkflowType" />  being activated.</param>
        /// <param name="name">A <see cref="System.String" /> representing the name of the <see cref="Rock.Model.Workflow" /> instance.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// The <see cref="Rock.Model.Workflow" /> instance.
        /// </returns>
        [RockObsolete( "1.7" )]
        [Obsolete( "For improved performance, use the Activate method that takes a WorkflowTypeCache parameter instead. IMPORTANT NOTE: When using the new method, the Workflow object that is returned by that method will not have the WorkflowType property set. If you are referencing the WorkflowType property on a Workflow returned by that method, you will get a Null Reference Exception! You should use the new WorkflowTypeCache property on the workflow instead.", true )]
        public static Workflow Activate( WorkflowType workflowType, string name, RockContext rockContext )
        {
            if ( workflowType != null )
            {
                var workflowTypeCache = WorkflowTypeCache.Get( workflowType.Id );
                var workflow = Activate( workflowTypeCache, name, rockContext );
                if ( workflow != null )
                {
                    workflow.WorkflowType = workflowType;
                }
                return workflow;
            }

            return null;
        }

        /// <summary>
        /// Activates the specified workflow type cache.
        /// </summary>
        /// <param name="workflowTypeCache">The workflow type cache.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static Workflow Activate( WorkflowTypeCache workflowTypeCache, string name )
        {
            using ( var rockContext = new RockContext() )
            {
                return Activate( workflowTypeCache, name, rockContext );
            }
        }

        /// <summary>
        /// Activates the specified workflow type cache.
        /// </summary>
        /// <param name="workflowTypeCache">The workflow type cache.</param>
        /// <param name="name">The name.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static Workflow Activate( WorkflowTypeCache workflowTypeCache, string name, RockContext rockContext )
        {
            var workflow = new Workflow();
            workflow.WorkflowTypeId = workflowTypeCache.Id;

            if ( !string.IsNullOrWhiteSpace( name ) )
            {
                workflow.Name = name;
            }
            else
            {
                workflow.Name = workflowTypeCache.Name;
            }

            workflow.Status = "Active";
            workflow.IsProcessing = false;
            workflow.ActivatedDateTime = RockDateTime.Now;
            workflow.LoadAttributes( rockContext );

            workflow.AddLogEntry( "Activated" );

            foreach ( var activityType in workflowTypeCache.ActivityTypes.OrderBy( a => a.Order ) )
            {
                if ( activityType.IsActivatedWithWorkflow )
                {
                    WorkflowActivity.Activate( activityType, workflow, rockContext );
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

