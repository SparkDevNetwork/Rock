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

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cached WorkflowActivityType
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CacheWorkflowActivityTyp instead" )]
    public class WorkflowActivityTypeCache : CachedModel<WorkflowActivityType>
    {
        #region Constructors

        private WorkflowActivityTypeCache( CacheWorkflowActivityType cacheItem )
        {
            CopyFromNewCache( cacheItem );
        }

        #endregion

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets a flag indicating if this WorkflowActivityType is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the WorkflowActivityType is active; otherwise <c>false</c>.
        /// </value>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the WorkflowTypeId of the <see cref="Rock.Model.WorkflowType"/> that this WorkflowActivityType belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the WorkflowTypeId of the <see cref="Rock.Model.WorkflowType"/> that this WorkflowActivityType belongs to.
        /// </value>
        public int WorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the friendly Name of this WorkflowActivityType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the friendly name of this WorkflowActivityType
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description or summary about this WorkflowActivityType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing a description or summary about this WorkflowActivityType.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this WorkflowActivityType is activated with the workflow.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is activated with workflow; otherwise, <c>false</c>.
        /// </value>
        public bool IsActivatedWithWorkflow { get; set; }

        /// <summary>
        /// Gets or sets the order that this WorkflowActivityType will be executed in the WorkflowType's process. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> indicating the order that this Activity will be executed in the Workflow.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets the type of the workflow.
        /// </summary>
        /// <value>
        /// The type of the workflow.
        /// </value>
        public WorkflowTypeCache WorkflowType => WorkflowTypeCache.Read( WorkflowTypeId );

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
        public override Security.ISecured ParentAuthority => WorkflowType ?? base.ParentAuthority;

        /// <summary>
        /// Gets the defined values.
        /// </summary>
        /// <value>
        /// The defined values.
        /// </value>
        public List<WorkflowActionTypeCache> ActionTypes
        {
            get
            {
                var actionTypes = new List<WorkflowActionTypeCache>();

                lock ( _obj )
                {
                    if ( actionTypeIds == null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            actionTypeIds = new WorkflowActionTypeService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( a => a.ActivityTypeId == Id )
                                .Select( a => a.Id )
                                .ToList();
                        }
                    }
                }

                foreach ( var id in actionTypeIds )
                {
                    var actionType = WorkflowActionTypeCache.Read( id );
                    if ( actionType != null )
                    {
                        actionTypes.Add( actionType );
                    }
                }

                return actionTypes;
            }
        }
        private List<int> actionTypeIds;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is WorkflowActivityType ) ) return;

            var workflowActivityType = (WorkflowActivityType)model;
            IsActive = workflowActivityType.IsActive;
            WorkflowTypeId = workflowActivityType.WorkflowTypeId;
            Name = workflowActivityType.Name;
            Description = workflowActivityType.Description;
            IsActivatedWithWorkflow = workflowActivityType.IsActivatedWithWorkflow;
            Order = workflowActivityType.Order;

            // set actionTypeIds to null so it load them all at once on demand
            actionTypeIds = null;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheWorkflowActivityType ) ) return;

            var workflowActivityType = (CacheWorkflowActivityType)cacheEntity;
            IsActive = workflowActivityType.IsActive;
            WorkflowTypeId = workflowActivityType.WorkflowTypeId;
            Name = workflowActivityType.Name;
            Description = workflowActivityType.Description;
            IsActivatedWithWorkflow = workflowActivityType.IsActivatedWithWorkflow;
            Order = workflowActivityType.Order;

            // set actionTypeIds to null so it load them all at once on demand
            actionTypeIds = null;
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
        /// Returns WorkflowActivityType object from cache.  If workflowActivityType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActivityTypeCache Read( int id, RockContext rockContext = null )
        {
            return new WorkflowActivityTypeCache( CacheWorkflowActivityType.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActivityTypeCache Read( Guid guid, RockContext rockContext = null )
        {
            return new WorkflowActivityTypeCache( CacheWorkflowActivityType.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Reads the specified defined type model.
        /// </summary>
        /// <param name="workflowActivityTypeModel">The defined type model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActivityTypeCache Read( WorkflowActivityType workflowActivityTypeModel, RockContext rockContext = null )
        {
            return new WorkflowActivityTypeCache( CacheWorkflowActivityType.Get( workflowActivityTypeModel ) );
        }

        /// <summary>
        /// Removes workflowActivityType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheWorkflowActivityType.Remove( id );
        }

        #endregion
    }
}