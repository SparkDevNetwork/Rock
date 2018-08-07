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
    /// Cached WorkflowActivityType
    /// </summary>
    [Serializable]
    [DataContract]
    public class WorkflowActivityTypeCache : ModelCache<WorkflowActivityTypeCache, WorkflowActivityType>
    {

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets a flag indicating if this WorkflowActivityType is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the WorkflowActivityType is active; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsActive { get; private set; }

        /// <summary>
        /// Gets or sets the WorkflowTypeId of the <see cref="Rock.Model.WorkflowType"/> that this WorkflowActivityType belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the WorkflowTypeId of the <see cref="Rock.Model.WorkflowType"/> that this WorkflowActivityType belongs to.
        /// </value>
        [DataMember]
        public int WorkflowTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the friendly Name of this WorkflowActivityType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the friendly name of this WorkflowActivityType
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description or summary about this WorkflowActivityType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing a description or summary about this WorkflowActivityType.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating if this WorkflowActivityType is activated with the workflow.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is activated with workflow; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActivatedWithWorkflow { get; private set; }

        /// <summary>
        /// Gets or sets the order that this WorkflowActivityType will be executed in the WorkflowType's process. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> indicating the order that this Activity will be executed in the Workflow.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets the type of the workflow.
        /// </summary>
        /// <value>
        /// The type of the workflow.
        /// </value>
        public WorkflowTypeCache WorkflowType => WorkflowTypeCache.Get( WorkflowTypeId );

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

                if ( _actionTypeIds == null )
                {
                    lock ( _obj )
                    {
                        if ( _actionTypeIds == null )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                _actionTypeIds = new WorkflowActionTypeService( rockContext )
                                    .Queryable().AsNoTracking()
                                    .Where( a => a.ActivityTypeId == Id )
                                    .Select( a => a.Id )
                                    .ToList();
                            }
                        }
                    }
                }

                if ( _actionTypeIds == null ) return actionTypes;

                foreach ( var id in _actionTypeIds )
                {
                    var actionType = WorkflowActionTypeCache.Get( id );
                    if ( actionType != null )
                    {
                        actionTypes.Add( actionType );
                    }
                }

                return actionTypes;
            }
        }
        private List<int> _actionTypeIds;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var workflowActivityType = entity as WorkflowActivityType;
            if ( workflowActivityType == null ) return;

            IsActive = workflowActivityType.IsActive;
            WorkflowTypeId = workflowActivityType.WorkflowTypeId;
            Name = workflowActivityType.Name;
            Description = workflowActivityType.Description;
            IsActivatedWithWorkflow = workflowActivityType.IsActivatedWithWorkflow;
            Order = workflowActivityType.Order;

            // set actionTypeIds to null so it load them all at once on demand
            _actionTypeIds = null;
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
            var activityType = Get( id );
            if ( activityType != null )
            {
                foreach ( var actionType in activityType.ActionTypes )
                {
                    WorkflowActionTypeCache.Remove( actionType.Id );
                }
            }

            Remove( id.ToString() );
        }

        #endregion
    }
}