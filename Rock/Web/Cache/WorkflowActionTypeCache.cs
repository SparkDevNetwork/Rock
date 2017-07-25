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
    /// Cached WorkflowActionType
    /// </summary>
    [Serializable]
    public class WorkflowActionTypeCache : CachedModel<WorkflowActionType>
    {
        #region Constructors

        private WorkflowActionTypeCache( WorkflowActionType model )
        {
            CopyFromModel( model );
        }

        #endregion

        #region Properties

        private object _obj = new object();

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
        public WorkflowActivityTypeCache ActivityType
        {
            get
            {
                return WorkflowActivityTypeCache.Read( ActivityTypeId );
            }
        }

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public EntityTypeCache EntityType
        {
            get
            {
                return EntityTypeCache.Read( EntityTypeId );
            }
        }

        /// <summary>
        /// Gets the workflow action.
        /// </summary>
        /// <value>
        /// The workflow action.
        /// </value>
        public Workflow.ActionComponent WorkflowAction
        {
            get
            {
                return WorkflowActionType.GetWorkflowAction( this.EntityTypeId );
            }
        }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public WorkflowActionFormCache WorkflowForm
        {
            get
            {
                if ( WorkflowFormId.HasValue )
                {
                    return WorkflowActionFormCache.Read( WorkflowFormId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.ActivityType != null ? this.ActivityType : base.ParentAuthority;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is WorkflowActionType )
            {
                var workflowActionType = (WorkflowActionType)model;
                this.ActivityTypeId = workflowActionType.ActivityTypeId;
                this.Name = workflowActionType.Name;
                this.Order = workflowActionType.Order;
                this.EntityTypeId = workflowActionType.EntityTypeId;
                this.IsActionCompletedOnSuccess = workflowActionType.IsActionCompletedOnSuccess;
                this.IsActivityCompletedOnSuccess = workflowActionType.IsActivityCompletedOnSuccess;
                this.WorkflowFormId = workflowActionType.WorkflowFormId;
                this.CriteriaAttributeGuid = workflowActionType.CriteriaAttributeGuid;
                this.CriteriaComparisonType = workflowActionType.CriteriaComparisonType;
                this.CriteriaValue = workflowActionType.CriteriaValue;
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
            return string.Format( "Rock:WorkflowActionType:{0}", id );
        }

        /// <summary>
        /// Returns WorkflowActionType object from cache.  If workflowActionType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionTypeCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( WorkflowActionTypeCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static WorkflowActionTypeCache LoadById( int id, RockContext rockContext )
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

        private static WorkflowActionTypeCache LoadById2( int id, RockContext rockContext )
        {
            var workflowActionTypeService = new WorkflowActionTypeService( rockContext );
            var workflowActionTypeModel = workflowActionTypeService
                .Queryable()
                .Where( t => t.Id == id )
                .FirstOrDefault();

            if ( workflowActionTypeModel != null )
            {
                return new WorkflowActionTypeCache( workflowActionTypeModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionTypeCache Read( Guid guid, RockContext rockContext = null )
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
            var workflowActionTypeService = new WorkflowActionTypeService( rockContext );
            return workflowActionTypeService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Reads the specified defined type model.
        /// </summary>
        /// <param name="workflowActionTypeModel">The defined type model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionTypeCache Read( WorkflowActionType workflowActionTypeModel, RockContext rockContext = null )
        {
            return GetOrAddExisting( WorkflowActionTypeCache.CacheKey( workflowActionTypeModel.Id ),
                () => LoadByModel( workflowActionTypeModel ) );
        }

        private static WorkflowActionTypeCache LoadByModel( WorkflowActionType workflowActionTypeModel )
        {
            if ( workflowActionTypeModel != null )
            {
                return new WorkflowActionTypeCache( workflowActionTypeModel );
            }
            return null;
        }

        /// <summary>
        /// Removes workflowActionType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            var actionType = WorkflowActionTypeCache.Read( id );
            if ( actionType != null && actionType.WorkflowFormId.HasValue )
            {
                WorkflowActionFormCache.Flush( actionType.WorkflowFormId.Value );
            }
            FlushCache( WorkflowActionTypeCache.CacheKey( id ) );
        }

        #endregion
    }
}