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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a WorkflowType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class WorkflowTypeCache : ModelCache<WorkflowTypeCache, WorkflowType>
    {

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets a flag indicating if this WorkflowType is part of the Rock core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the WorkflowType is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets or sets a flag indicating if this WorkflowType is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the WorkflowType is active; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsActive { get; private set; }

        /// <summary>
        /// Gets or sets the workflow identifier prefix.
        /// </summary>
        /// <value>
        /// The workflow identifier prefix.
        /// </value>
        [DataMember]
        public string WorkflowIdPrefix { get; private set; }

        /// <summary>
        /// Gets or sets the friendly Name of the WorkflowType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the friendly Name of the WorkflowType.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets a user defined description or summary about the WorkflowType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a description/summary of the WorkflowType.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the CategoryId of the <see cref="Rock.Model.Category"/> that this WorkflowType belongs to. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CategoryId of the <see cref="Rock.Model.Category"/> that the WorkflowType belongs to. 
        /// If the WorkflowType does not belong to a category, this value will be null.
        /// </value>
        [DataMember]
        public int? CategoryId { get; private set; }

        /// <summary>
        /// Gets or sets the display order of the WorkFlowType, the lower the number the higher up that the WorkflowType will display in the workflow list. This
        /// property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the display order of the WorkflowType.  
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets the work term for the WorkflowType. This is the action that is being performed by this WorkflowType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the WorkItem that is being performed by this WorkflowType
        /// </value>
        [DataMember]
        public string WorkTerm { get; private set; }

        /// <summary>
        /// Gets or sets the minimum length of time, in seconds, before a persisted <see cref="Rock.Model.Workflow"/> instance that implements this 
        /// WorkflowType can be re-executed.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the minimum length of time, in seconds, that must pass before the same persisted <see cref="Rock.Model.Workflow"/> instance
        /// that implements this WorkflowType can be re-executed.
        /// </value>
        [DataMember]
        public int? ProcessingIntervalSeconds { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="Rock.Model.Workflow"/> instances of this WorkflowType are persisted.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if <see cref="Rock.Model.Workflow"/> instances of this WorkflowType are persisted; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsPersisted { get; private set; }

        /// <summary>
        /// Gets or sets the summary view text to be displayed when a workflow of this type has no form or has been completed. This field supports Lava.
        /// </summary>
        /// <value>
        /// The summary view text.
        /// </value>
        [DataMember]
        public string SummaryViewText { get; private set; }

        /// <summary>
        /// Gets or sets the text to be displayed when a workflow of this type workflow is active, but does not have an active form. This field supports Lava.
        /// </summary>
        /// <value>
        /// The summary view text.
        /// </value>
        [DataMember]
        public string NoActionMessage { get; private set; }

        /// <summary>
        /// Gets or sets the log retention period in days.
        /// </summary>
        /// <value>
        /// The log retention period in days.
        /// </value>
        [DataMember]
        public int? LogRetentionPeriod { get; private set; }

        /// <summary>
        /// Gets or sets the completed workflow retention period in days.
        /// </summary>
        /// <value>
        /// The completed workflow retention period in days.
        /// </value>
        [DataMember]
        public int? CompletedWorkflowRetentionPeriod { get; private set; }

        /// <summary>
        /// Gets or sets the logging level.
        /// Gets or sets the <see cref="Rock.Model.WorkflowLoggingLevel"/> indicating the level of detail that should be logged when instances of this WorkflowType are executed.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.WorkflowLoggingLevel"/> value indicating the level of detail that should be logged when instances of this WorkflowType are executed.
        /// When <c>WorkflowLoggingLevel.None</c> no details of the Workflow instance's execution will be logged.
        /// When <c>WorkflowLoggingLevel.Workflow</c> only workflow events will be logged (i.e. begin and end).
        /// When <c>WorkflowLoggingLevel.Activity</c> workflow and activity events will be logged.
        /// When <c>WorkflowLoggingLevel.Action</c> workflow, activity and action events will be logged.
        /// </value>
        [DataMember]
        public WorkflowLoggingLevel LoggingLevel { get; private set; }

        /// <summary>
        /// Gets or sets the name of the icon CSS class. This property is only used for CSS based icons.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the icon CSS class. This property will be null if a file based icon is being used.
        /// </value>
        [DataMember]
        public string IconCssClass { get; private set; }

        /// <inheritdoc cref="WorkflowType.MaxWorkflowAgeDays"/>
        [DataMember]
        public int? MaxWorkflowAgeDays { get; private set; }

        /// <inheritdoc cref="WorkflowType.FormBuilderTemplateId"/>
        [DataMember]
        public int? FormBuilderTemplateId { get; private set; }

        /// <inheritdoc cref="WorkflowType.IsFormBuilder"/>
        [DataMember]
        public bool IsFormBuilder { get; private set; }

        /// <inheritdoc cref="WorkflowType.FormBuilderSettingsJson"/>
        [DataMember]
        public string FormBuilderSettingsJson
        {
            get => FormBuilderSettings?.ToJson();
            private set => FormBuilderSettings = value?.FromJsonOrNull<Rock.Workflow.FormBuilder.FormSettings>();
        }

        /// <inheritdoc cref="WorkflowType.FormStartDateTime"/>
        [DataMember]
        public DateTime? FormStartDateTime { get; private set; }

        /// <inheritdoc cref="WorkflowType.FormEndDateTime"/>
        [DataMember]
        public DateTime? FormEndDateTime { get; private set; }

        /// <inheritdoc cref="WorkflowType.WorkflowExpireDateTime"/>
        [DataMember]
        public DateTime? WorkflowExpireDateTime { get; private set; }

        /// <inheritdoc cref="WorkflowType.IsLoginRequired"/>
        [DataMember]
        public bool IsLoginRequired { get; private set; }

        /// <inheritdoc cref="WorkflowType.FormBuilderTemplate"/>
        public WorkflowFormBuilderTemplateCache FormBuilderTemplate => FormBuilderTemplateId.HasValue ? WorkflowFormBuilderTemplateCache.Get( FormBuilderTemplateId.Value ) : null;

        /// <inheritdoc cref="Rock.Workflow.FormBuilder.FormSettings"/>
        public Rock.Workflow.FormBuilder.FormSettings FormBuilderSettings { get; private set; }

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public CategoryCache Category
        {
            get
            {
                if ( CategoryId.HasValue )
                {
                    return CategoryCache.Get( CategoryId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the defined values.
        /// </summary>
        /// <value>
        /// The defined values.
        /// </value>
        public List<WorkflowActivityTypeCache> ActivityTypes
        {
            get
            {
                var activityTypes = new List<WorkflowActivityTypeCache>();

                if ( _activityTypeIds == null )
                {
                    lock ( _obj )
                    {
                        if ( _activityTypeIds == null )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                _activityTypeIds = new WorkflowActivityTypeService( rockContext )
                                    .Queryable().AsNoTracking()
                                    .Where( a => a.WorkflowTypeId == Id )
                                    .Select( v => v.Id )
                                    .ToList();
                            }
                        }
                    }
                }

                if ( _activityTypeIds == null )
                    return activityTypes;

                foreach ( var id in _activityTypeIds )
                {
                    var activityType = WorkflowActivityTypeCache.Get( id );
                    if ( activityType != null )
                    {
                        activityTypes.Add( activityType );
                    }
                }

                return activityTypes;
            }
        }
        private List<int> _activityTypeIds;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var workflowType = entity as WorkflowType;
            if ( workflowType == null )
            {
                return;
            }

            IsSystem = workflowType.IsSystem;
            IsActive = workflowType.IsActive;
            WorkflowIdPrefix = workflowType.WorkflowIdPrefix;
            Name = workflowType.Name;
            Description = workflowType.Description;
            CategoryId = workflowType.CategoryId;
            Order = workflowType.Order;
            WorkTerm = workflowType.WorkTerm;
            ProcessingIntervalSeconds = workflowType.ProcessingIntervalSeconds;
            IsPersisted = workflowType.IsPersisted;
            SummaryViewText = workflowType.SummaryViewText;
            NoActionMessage = workflowType.NoActionMessage;
            LogRetentionPeriod = workflowType.LogRetentionPeriod;
            CompletedWorkflowRetentionPeriod = workflowType.CompletedWorkflowRetentionPeriod;
            LoggingLevel = workflowType.LoggingLevel;
            IconCssClass = workflowType.IconCssClass;
            MaxWorkflowAgeDays = workflowType.MaxWorkflowAgeDays;
            FormBuilderTemplateId = workflowType.FormBuilderTemplateId;
            IsFormBuilder = workflowType.IsFormBuilder;
            FormBuilderSettingsJson = workflowType.FormBuilderSettingsJson;
            FormStartDateTime = workflowType.FormStartDateTime;
            FormEndDateTime = workflowType.FormEndDateTime;
            WorkflowExpireDateTime = workflowType.WorkflowExpireDateTime;
            IsLoginRequired = workflowType.IsLoginRequired;

            // set activityTypeIds to null so it load them all at once on demand
            _activityTypeIds = null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Removes a WorkflowActionForm from cache.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public new static void Remove( int id )
        {
            var workflowType = Get( id );
            if ( workflowType != null )
            {
                foreach ( var activityType in workflowType.ActivityTypes )
                {
                    WorkflowActivityTypeCache.Remove( activityType.Id );
                }
            }

            Remove( id.ToString() );
        }

        #endregion
    }
}