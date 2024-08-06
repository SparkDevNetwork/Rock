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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cached WorkflowActionType
    /// </summary>
    [Serializable]
    [DataContract]
    public class WorkflowActionTypeCache : ModelCache<WorkflowActionTypeCache, WorkflowActionType>
    {

        #region Properties

        /// <summary>
        /// Gets or sets the ActivityTypeId of the <see cref="Rock.Model.WorkflowActivityType"/> that performs this Action Type.
        /// </summary>
        /// <value>
        /// The activity type id.
        /// </value>
        [DataMember]
        public int ActivityTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the friendly name of the ActionType
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the ActionType.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the order of the ActionType in the <see cref="Rock.Model.WorkflowActivityType" />
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that the action is operating against.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that the action is operating against.
        /// </value>
        [DataMember]
        public int EntityTypeId { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is action completed on success.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is action completed on success; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActionCompletedOnSuccess { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is activity completed on success.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this instance is activity completed on success; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActivityCompletedOnSuccess { get; private set; }

        /// <summary>
        /// Gets or sets the workflow form identifier.
        /// </summary>
        /// <value>
        /// The workflow form identifier.
        /// </value>
        [DataMember]
        public int? WorkflowFormId { get; private set; }

        /// <summary>
        /// Gets or sets the criteria attribute unique identifier.
        /// </summary>
        /// <value>
        /// The criteria attribute unique identifier.
        /// </value>
        [DataMember]
        public Guid? CriteriaAttributeGuid { get; private set; }

        /// <summary>
        /// Gets or sets the type of the criteria comparison.
        /// </summary>
        /// <value>
        /// The type of the criteria comparison.
        /// </value>
        [DataMember]
        public ComparisonType CriteriaComparisonType { get; private set; }

        /// <summary>
        /// Gets or sets the criteria value.
        /// </summary>
        /// <value>
        /// The criteria value.
        /// </value>
        [DataMember]
        public string CriteriaValue { get; private set; }

        /// <summary>
        /// Gets or sets the boolean value that determines if an action should be completed if criteria is unmet.
        /// </summary>
        /// <value>
        /// The boolean value determining if an action should be completed if criteria is unmet.
        /// </value>
        [DataMember]
        public bool IsActionCompletedIfCriteriaUnmet { get; set; }

        /// <summary>
        /// Gets the type of the activity.
        /// </summary>
        /// <value>
        /// The type of the activity.
        /// </value>
        public WorkflowActivityTypeCache ActivityType => WorkflowActivityTypeCache.Get( ActivityTypeId );

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public EntityTypeCache EntityType => EntityTypeCache.Get( EntityTypeId );

        /// <summary>
        /// Gets the workflow action.
        /// </summary>
        /// <value>
        /// The workflow action.
        /// </value>
        public Workflow.ActionComponent WorkflowAction => WorkflowActionType.GetWorkflowAction( EntityTypeId );

        /// <summary>
        /// If this Workflow Action Type is a <see cref="Rock.Workflow.Action.UserEntryForm"/>, this is the <see cref="WorkflowActionForm"/> for this workflow ction
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public WorkflowActionFormCache WorkflowForm => WorkflowFormId.HasValue ? WorkflowActionFormCache.Get( WorkflowFormId.Value ) : null;

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
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var workflowActionType = entity as WorkflowActionType;
            if ( workflowActionType == null ) return;

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
            IsActionCompletedIfCriteriaUnmet = workflowActionType.IsActionCompletedIfCriteriaUnmet;
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
            var actionType = Get( id );
            if ( actionType != null && actionType.WorkflowFormId.HasValue )
            {
                WorkflowActionFormCache.Remove( actionType.WorkflowFormId.Value );
            }

            Remove( id.ToString() );
        }

        #endregion
    }
}