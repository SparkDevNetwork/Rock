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
using Rock;
using Rock.Data;
using Rock.Lava;
using Rock.Security;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Workflow Logic
    /// </summary>
    public partial class Workflow
    {
        #region Virtual Properties

        /// <summary>
        /// Gets the workflow type cache.
        /// </summary>
        /// <value>
        /// The workflow type cache.
        /// </value>
        [LavaVisible]
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
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check the default authorization on the current type, and
        /// then the authorization on the Rock.Security.GlobalDefault entity
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                var workflowTypeCache = this.WorkflowTypeCache;
                return workflowTypeCache != null ? workflowTypeCache : base.ParentAuthority;
            }
        }

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

        #endregion Virtual Properties

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

            using ( var diagnosticActivity = Observability.ObservabilityHelper.StartActivity( $"WORKFLOW: Type '{WorkflowTypeCache?.Name}'" ) )
            {
                diagnosticActivity?.AddTag( "rock.workflow.id", Id );
                diagnosticActivity?.AddTag( "rock.workflow.name", Name );
                diagnosticActivity?.AddTag( "rock.workflow.type.id", WorkflowTypeId );
                diagnosticActivity?.AddTag( "rock.workflow.type.name", WorkflowTypeCache?.Name ?? string.Empty );

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
        /// Log Entry
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

#endregion Public Methods

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

        #endregion Private Methods

        #region Static Methods

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

        #endregion Static Methods
    }
}

