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
using Rock.Data;
using Rock.Lava;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// WorkflowActivity Logic
    /// </summary>
    public partial class WorkflowActivity
    {
        #region Virtual Properties

        /// <summary>
        /// Gets the activity type cache.
        /// </summary>
        /// <value>
        /// The activity type cache.
        /// </value>
        [LavaVisible]
        public virtual WorkflowActivityTypeCache ActivityTypeCache
        {
            get
            {
                if ( ActivityTypeId > 0 )
                {
                    return WorkflowActivityTypeCache.Get( ActivityTypeId );
                }
                else if ( ActivityType != null )
                {
                    return WorkflowActivityTypeCache.Get( ActivityType.Id );
                }
                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this WorkflowActivity instance is active.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual bool IsActive
        {
            get
            {
                return ( this.ActivityType?.IsActive ?? true ) && ActivatedDateTime.HasValue && !CompletedDateTime.HasValue;
            }
            private set { }
        }

        /// <summary>
        /// Gets an enumerable collection containing the active <see cref="Rock.Model.WorkflowAction">WorkflowActions</see> for this WorkflowActivity, ordered by their order property.
        /// </summary>
        /// <value>
        /// An enumerable collection containing the active <see cref="Rock.Model.WorkflowAction">WorkflowActions</see> for this WorkflowActivity.
        /// </value>
        [LavaVisible]
        public virtual IEnumerable<WorkflowAction> ActiveActions
        {
            get
            {
                return this.Actions
                    .Where( a => a.IsActive && !a.CompletedDateTime.HasValue )
                    .ToList()
                    .OrderBy( a => a.ActionTypeCache.Order );
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
                return this.Workflow != null ? this.Workflow : base.ParentAuthority;
            }
        }

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
                if ( propertyKey == "ActivityType" )
                {
                    return ActivityTypeCache;
                }
                return base[key];
            }
        }

        #endregion Virtual Properties

        #region Public Methods

        /// <summary>
        /// Processes this WorkflowAction
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="entity">The entity that work is being performed against.</param>
        /// <param name="errorMessages">A 
        /// <see cref="System.Collections.Generic.List{String}" /> that will contain any error messages that are
        /// returned while processing this WorkflowActivity</param>
        /// <returns>
        /// A <see cref="System.Boolean" /> value that is <c>true</c> if the WorkflowActivity processes successfully; otherwise <c>false</c>.
        /// </returns>
        internal virtual bool Process( RockContext rockContext, Object entity, out List<string> errorMessages )
        {
            AddLogEntry( "Processing..." );

            using ( var diagnosticActivity = Observability.ObservabilityHelper.StartActivity( $"WORKFLOW: Activity '{ActivityTypeCache?.Name}'" ) )
            {
                diagnosticActivity?.AddTag( "rock.workflow.activitytype.id", ActivityTypeId );
                diagnosticActivity?.AddTag( "rock.workflow.activitytype.name", ActivityTypeCache?.Name ?? string.Empty );

                errorMessages = new List<string>();

                foreach ( var action in this.ActiveActions )
                {
                    List<string> actionErrorMessages;
                    bool actionSuccess = action.Process( rockContext, entity, out actionErrorMessages );
                    if ( actionErrorMessages.Any() )
                    {
                        errorMessages.Add( string.Format( "Error in Activity: {0}; Action: {1} ({2} action type)", this.ActivityTypeCache.Name, action.ActionTypeCache.Name, action.ActionTypeCache.WorkflowAction.EntityType.FriendlyName ) );
                        errorMessages.AddRange( actionErrorMessages );
                    }

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
                    if ( this.Workflow == null || !this.Workflow.IsActive )
                    {
                        break;
                    }
                }

                this.LastProcessedDateTime = RockDateTime.Now;

                AddLogEntry( "Processing Complete" );

                if ( !this.ActiveActions.Any() )
                {
                    MarkComplete();
                }

                return errorMessages.Count == 0;
            }
        }

        /// <summary>
        /// Adds a <see cref="Rock.Model.WorkflowLog" /> entry.
        /// </summary>
        /// <param name="logEntry">A <see cref="System.String" /> representing the body of the log entry.</param>
        /// <param name="force">if set to <c>true</c> will ignore logging level and always add the entry.</param>
        public virtual void AddLogEntry( string logEntry, bool force = false )
        {

            if ( this.Workflow != null )
            {
                var workflowType = this.Workflow.WorkflowTypeCache;
                if ( force || (
                    workflowType != null && (
                    workflowType.LoggingLevel == WorkflowLoggingLevel.Activity ||
                    workflowType.LoggingLevel == WorkflowLoggingLevel.Action ) ) )
                {
                    string idStr = Id > 0 ? "(" + Id.ToString() + ")" : "";
                    this.Workflow.AddLogEntry( string.Format( "{0} Activity {1}: {2}", this.ToString(), idStr, logEntry ), force );
                }
            }
        }

        /// <summary>
        /// Marks this WorkflowActivity as complete.
        /// </summary>
        public virtual void MarkComplete()
        {
            CompletedDateTime = RockDateTime.Now;
            AddLogEntry( "Completed" );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this WorkflowActivity.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this WorkflowActivity.
        /// </returns>
        public override string ToString()
        {
            var activityType = this.ActivityTypeCache;
            if ( activityType != null )
            {
                return activityType.ToStringSafe();
            }
            return base.ToString();
        }

        #region Static Methods

        /// <summary>
        /// Activates the specified WorkflowActivity
        /// </summary>
        /// <param name="activityTypeCache">The activity type cache.</param>
        /// <param name="workflow">The persisted <see cref="Rock.Model.Workflow" /> instance that this Workflow activity belongs to.</param>
        /// <returns>
        /// The activated <see cref="Rock.Model.WorkflowActivity" />.
        /// </returns>
        public static WorkflowActivity Activate( WorkflowActivityTypeCache activityTypeCache, Workflow workflow )
        {
            using ( var rockContext = new RockContext() )
            {
                return Activate( activityTypeCache, workflow, rockContext );
            }
        }

        /// <summary>
        /// Activates the specified WorkflowActivity
        /// </summary>
        /// <param name="activityTypeCache">The activity type cache.</param>
        /// <param name="workflow">The persisted <see cref="Rock.Model.Workflow" /> instance that this Workflow activity belongs to.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// The activated <see cref="Rock.Model.WorkflowActivity" />.
        /// </returns>
        public static WorkflowActivity Activate( WorkflowActivityTypeCache activityTypeCache, Workflow workflow, RockContext rockContext )
        {
            if ( !workflow.IsActive )
            {
                return null;
            }

            var activity = new WorkflowActivity();
            activity.Workflow = workflow;
            activity.ActivityTypeId = activityTypeCache.Id;
            activity.ActivatedDateTime = RockDateTime.Now;
            activity.LoadAttributes( rockContext );

            activity.AddLogEntry( "Activated" );

            foreach ( var actionType in activityTypeCache.ActionTypes )
            {
                activity.Actions.Add( WorkflowAction.Activate( actionType, activity, rockContext ) );
            }

            workflow.Activities.Add( activity );

            return activity;
        }

        #endregion

        #endregion Public Methods
    }
}

