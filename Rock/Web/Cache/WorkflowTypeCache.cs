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

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a WorkflowType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class WorkflowTypeCache : CachedModel<WorkflowType>
    {
        #region Constructors

        private WorkflowTypeCache( WorkflowType model )
        {
            CopyFromModel( model );
        }

        #endregion

        #region Properties

        private object _obj = new object();

        /// <summary>
        /// Gets or sets a flag indicating if this WorkflowType is part of the Rock core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the WorkflowType is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this WorkflowType is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the WorkflowType is active; otherwise <c>false</c>.
        /// </value>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the workflow identifier prefix.
        /// </summary>
        /// <value>
        /// The workflow identifier prefix.
        /// </value>
        public string WorkflowIdPrefix { get; set; }

        /// <summary>
        /// Gets or sets the friendly Name of the WorkflowType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the friendly Name of the WorkflowType.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a user defined description or summary about the WorkflowType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a description/summary of the WorkflowType.
        /// </value>
        public string Description { get; set; }


        /// <summary>
        /// Gets or sets the CategoryId of the <see cref="Rock.Model.Category"/> that this WorkflowType belongs to. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CategoryId of the <see cref="Rock.Model.Category"/> that the WorkflowType belongs to. 
        /// If the WorkflowType does not belong to a category, this value will be null.
        /// </value>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the display order of the WorkFlowType, the lower the number the higher up that the WorkflowType will display in the workflow list. This
        /// property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the display order of the WorkflowType.  
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the work term for the WorkflowType. This is the action that is being performed by this WorkflowType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the WorkItem that is being performed by this WorkflowType
        /// </value>
        public string WorkTerm { get; set; }

        /// <summary>
        /// Gets or sets the minimum length of time, in seconds, before a persisted <see cref="Rock.Model.Workflow"/> instance that implements this 
        /// WorkflowType can be re-executed.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the minimum length of time, in seconds, that must pass before the same persisted <see cref="Rock.Model.Workflow"/> instance
        /// that implements this WorkflowType can be re-executed.
        /// </value>
        public int? ProcessingIntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="Rock.Model.Workflow"/> instances of this WorkflowType are persisted.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if <see cref="Rock.Model.Workflow"/> instances of this WorkflowType are persisted; otherwise <c>false</c>.
        /// </value>
        public bool IsPersisted { get; set; }

        /// <summary>
        /// Gets or sets the summary view text to be displayed when a workflow of this type has no form or has been completed. This field supports Lava.
        /// </summary>
        /// <value>
        /// The summary view text.
        /// </value>
        public string SummaryViewText { get; set; }

        /// <summary>
        /// Gets or sets the text to be displayed when a workflow of this type workflow is active, but does not have an active form. This field supports Lava.
        /// </summary>
        /// <value>
        /// The summary view text.
        /// </value>
        public string NoActionMessage { get; set; }

        /// <summary>
        /// Gets or sets the log retention period in days.
        /// </summary>
        /// <value>
        /// The log retention period in days.
        /// </value>
        public int? LogRetentionPeriod { get; set; }

        /// <summary>
        /// Gets or sets the completed workflow rention period in days.
        /// </summary>
        /// <value>
        /// The completed workflow rention period in days.
        /// </value>
        public int? CompletedWorkflowRetentionPeriod { get; set; }

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
        public WorkflowLoggingLevel LoggingLevel { get; set; }

        /// <summary>
        /// Gets or sets the name of the icon CSS class. This property is only used for CSS based icons.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the icon CSS class. This property will be null if a file based icon is being used.
        /// </value>
        public string IconCssClass { get; set; }

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
                    return CategoryCache.Read( CategoryId.Value );
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

                lock ( _obj )
                { 
                    if ( activityTypeIds == null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            activityTypeIds = new Model.WorkflowActivityTypeService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( a => a.WorkflowTypeId == this.Id )
                                .Select( v => v.Id )
                                .ToList();
                        }
                    }
                }

                foreach ( int id in activityTypeIds )
                {
                    var activityType = WorkflowActivityTypeCache.Read( id );
                    if ( activityType != null )
                    {
                        activityTypes.Add( activityType );
                    }
                }

                return activityTypes;
            }
        }
        private List<int> activityTypeIds = null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is WorkflowType )
            {
                var WorkflowType = (WorkflowType)model;
                this.IsSystem = WorkflowType.IsSystem;
                this.IsActive = WorkflowType.IsActive;
                this.WorkflowIdPrefix = WorkflowType.WorkflowIdPrefix;
                this.Name = WorkflowType.Name;
                this.Description = WorkflowType.Description;
                this.CategoryId = WorkflowType.CategoryId;
                this.Order = WorkflowType.Order;
                this.WorkTerm = WorkflowType.WorkTerm;
                this.ProcessingIntervalSeconds = WorkflowType.ProcessingIntervalSeconds;
                this.IsPersisted = WorkflowType.IsPersisted;
                this.SummaryViewText = WorkflowType.SummaryViewText;
                this.NoActionMessage = WorkflowType.NoActionMessage;
                this.LogRetentionPeriod = WorkflowType.LogRetentionPeriod;
                this.CompletedWorkflowRetentionPeriod = WorkflowType.CompletedWorkflowRetentionPeriod;
                this.LoggingLevel = WorkflowType.LoggingLevel;
                this.IconCssClass = WorkflowType.IconCssClass;

                // set activityTypeIds to null so it load them all at once on demand
                this.activityTypeIds = null;
            }
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

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:WorkflowType:{0}", id );
        }

        /// <summary>
        /// Returns WorkflowType object from cache.  If WorkflowType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowTypeCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( WorkflowTypeCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static WorkflowTypeCache LoadById( int id, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadById2( id, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadById2( id, rockContext2 );
            }
        }

        private static WorkflowTypeCache LoadById2( int id, RockContext rockContext )
        {
            var WorkflowTypeService = new WorkflowTypeService( rockContext );
            var WorkflowTypeModel = WorkflowTypeService
                .Queryable()
                .Where( t => t.Id == id )
                .FirstOrDefault();

            if ( WorkflowTypeModel != null )
            {
                return new WorkflowTypeCache( WorkflowTypeModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowTypeCache Read( Guid guid, RockContext rockContext = null )
        {
            int id = GetOrAddExisting( guid.ToString(),
                () => LoadByGuid( guid, rockContext ) );

            return Read( id, rockContext );
        }

        private static int LoadByGuid( Guid guid, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadByGuid2( guid, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadByGuid2( guid, rockContext2 );
            }
        }

        private static int LoadByGuid2( Guid guid, RockContext rockContext )
        {
            var WorkflowTypeService = new WorkflowTypeService( rockContext );
            return WorkflowTypeService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Reads the specified defined type model.
        /// </summary>
        /// <param name="WorkflowTypeModel">The defined type model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowTypeCache Read( WorkflowType WorkflowTypeModel, RockContext rockContext = null )
        {
            return GetOrAddExisting( WorkflowTypeCache.CacheKey( WorkflowTypeModel.Id ),
                () => LoadByModel( WorkflowTypeModel ) );
        }

        private static WorkflowTypeCache LoadByModel( WorkflowType WorkflowTypeModel )
        {
            if ( WorkflowTypeModel != null )
            {
                return new WorkflowTypeCache( WorkflowTypeModel );
            }
            return null;
        }

        /// <summary>
        /// Removes WorkflowType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            var workflowType = WorkflowTypeCache.Read( id );
            if ( workflowType != null && workflowType.activityTypeIds != null )
            {
                foreach ( int activityTypeId in workflowType.activityTypeIds )
                {
                    WorkflowActivityTypeCache.Flush( activityTypeId );
                }
            }
            FlushCache( WorkflowTypeCache.CacheKey( id ) );
        }

        #endregion
    }
}