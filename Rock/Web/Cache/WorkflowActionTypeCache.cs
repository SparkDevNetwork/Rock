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

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cached WorkflowActionType
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CacheWorkflowActionType instead" )]
    public class WorkflowActionTypeCache : CachedModel<WorkflowActionType>
    {
        #region Constructors

        private WorkflowActionTypeCache( CacheWorkflowActionType cacheItem )
        {
            CopyFromNewCache( cacheItem );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the ActivityTypeId of the <see cref="Rock.Model.WorkflowActivityType"/> that performs this Action Type.
        /// </summary>
        /// <value>
        /// The activity type id.
        /// </value>
        public int ActivityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the ActionType
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the ActionType.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order of the ActionType in the <see cref="Rock.Model.WorkflowActivityType" />
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that the action is operating against.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that the action is operating against.
        /// </value>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is action completed on success.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is action completed on success; otherwise, <c>false</c>.
        /// </value>
        public bool IsActionCompletedOnSuccess { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is activity completed on success.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this instance is activity completed on success; otherwise, <c>false</c>.
        /// </value>
        public bool IsActivityCompletedOnSuccess { get; set; }

        /// <summary>
        /// Gets or sets the workflow form identifier.
        /// </summary>
        /// <value>
        /// The workflow form identifier.
        /// </value>
        public int? WorkflowFormId { get; set; }

        /// <summary>
        /// Gets or sets the criteria attribute unique identifier.
        /// </summary>
        /// <value>
        /// The criteria attribute unique identifier.
        /// </value>
        public Guid? CriteriaAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the type of the criteria comparison.
        /// </summary>
        /// <value>
        /// The type of the criteria comparison.
        /// </value>
        public ComparisonType CriteriaComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the criteria value.
        /// </summary>
        /// <value>
        /// The criteria value.
        /// </value>
        public string CriteriaValue { get; set; }

        /// <summary>
        /// Gets the type of the activity.
        /// </summary>
        /// <value>
        /// The type of the activity.
        /// </value>
        public WorkflowActivityTypeCache ActivityType => WorkflowActivityTypeCache.Read( ActivityTypeId );

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public EntityTypeCache EntityType => EntityTypeCache.Read( EntityTypeId );

        /// <summary>
        /// Gets the workflow action.
        /// </summary>
        /// <value>
        /// The workflow action.
        /// </value>
        public Workflow.ActionComponent WorkflowAction => WorkflowActionType.GetWorkflowAction( EntityTypeId );

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public WorkflowActionFormCache WorkflowForm => WorkflowFormId.HasValue ? WorkflowActionFormCache.Read( WorkflowFormId.Value ) : null;

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
        public override Security.ISecured ParentAuthority => ActivityType ?? base.ParentAuthority;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is WorkflowActionType ) ) return;

            var workflowActionType = (WorkflowActionType)model;
            ActivityTypeId = workflowActionType.ActivityTypeId;
            Name = workflowActionType.Name;
            Order = workflowActionType.Order;
            EntityTypeId = workflowActionType.EntityTypeId;
            IsActionCompletedOnSuccess = workflowActionType.IsActionCompletedOnSuccess;
            IsActivityCompletedOnSuccess = workflowActionType.IsActivityCompletedOnSuccess;
            WorkflowFormId = workflowActionType.WorkflowFormId;
            CriteriaAttributeGuid = workflowActionType.CriteriaAttributeGuid;
            CriteriaComparisonType = workflowActionType.CriteriaComparisonType;
            CriteriaValue = workflowActionType.CriteriaValue;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheWorkflowActionType ) ) return;

            var workflowActionType = (CacheWorkflowActionType)cacheEntity;
            ActivityTypeId = workflowActionType.ActivityTypeId;
            Name = workflowActionType.Name;
            Order = workflowActionType.Order;
            EntityTypeId = workflowActionType.EntityTypeId;
            IsActionCompletedOnSuccess = workflowActionType.IsActionCompletedOnSuccess;
            IsActivityCompletedOnSuccess = workflowActionType.IsActivityCompletedOnSuccess;
            WorkflowFormId = workflowActionType.WorkflowFormId;
            CriteriaAttributeGuid = workflowActionType.CriteriaAttributeGuid;
            CriteriaComparisonType = workflowActionType.CriteriaComparisonType;
            CriteriaValue = workflowActionType.CriteriaValue;
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
        /// Returns WorkflowActionType object from cache.  If workflowActionType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionTypeCache Read( int id, RockContext rockContext = null )
        {
            return new WorkflowActionTypeCache( CacheWorkflowActionType.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionTypeCache Read( Guid guid, RockContext rockContext = null )
        {
            return new WorkflowActionTypeCache( CacheWorkflowActionType.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Reads the specified defined type model.
        /// </summary>
        /// <param name="workflowActionTypeModel">The defined type model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionTypeCache Read( WorkflowActionType workflowActionTypeModel, RockContext rockContext = null )
        {
            return new WorkflowActionTypeCache( CacheWorkflowActionType.Get( workflowActionTypeModel ) );
        }

        /// <summary>
        /// Removes workflowActionType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheWorkflowActionType.Remove( id );
        }

        #endregion
    }
}