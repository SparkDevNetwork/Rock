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
    /// Cached WorkflowActivityType
    /// </summary>
    [Serializable]
    public class WorkflowActivityTypeCache : CachedModel<WorkflowActivityType>
    {
        #region Constructors

        private WorkflowActivityTypeCache( WorkflowActivityType model )
        {
            CopyFromModel( model );
        }

        #endregion

        #region Properties

        private object _obj = new object();

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
        public WorkflowTypeCache WorkflowType
        {
            get
            {
                return WorkflowTypeCache.Read( WorkflowTypeId );
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
                return this.WorkflowType != null ? this.WorkflowType : base.ParentAuthority;
            }
        }

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
                            actionTypeIds = new Model.WorkflowActionTypeService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( a => a.ActivityTypeId == this.Id )
                                .Select( a => a.Id )
                                .ToList();
                        }
                    }
                }

                foreach ( int id in actionTypeIds )
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
        private List<int> actionTypeIds = null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is WorkflowActivityType )
            {
                var workflowActivityType = (WorkflowActivityType)model;
                this.IsActive = workflowActivityType.IsActive;
                this.WorkflowTypeId = workflowActivityType.WorkflowTypeId;
                this.Name = workflowActivityType.Name;
                this.Description = workflowActivityType.Description;
                this.IsActivatedWithWorkflow = workflowActivityType.IsActivatedWithWorkflow;
                this.Order = workflowActivityType.Order;

                // set actionTypeIds to null so it load them all at once on demand
                this.actionTypeIds = null;
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
            return string.Format( "Rock:WorkflowActivityType:{0}", id );
        }

        /// <summary>
        /// Returns WorkflowActivityType object from cache.  If workflowActivityType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActivityTypeCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( WorkflowActivityTypeCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static WorkflowActivityTypeCache LoadById( int id, RockContext rockContext )
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

        private static WorkflowActivityTypeCache LoadById2( int id, RockContext rockContext )
        {
            var workflowActivityTypeService = new WorkflowActivityTypeService( rockContext );
            var workflowActivityTypeModel = workflowActivityTypeService
                .Queryable()
                .Where( t => t.Id == id )
                .FirstOrDefault();

            if ( workflowActivityTypeModel != null )
            {
                return new WorkflowActivityTypeCache( workflowActivityTypeModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActivityTypeCache Read( Guid guid, RockContext rockContext = null )
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
            var workflowActivityTypeService = new WorkflowActivityTypeService( rockContext );
            return workflowActivityTypeService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Reads the specified defined type model.
        /// </summary>
        /// <param name="workflowActivityTypeModel">The defined type model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActivityTypeCache Read( WorkflowActivityType workflowActivityTypeModel, RockContext rockContext = null )
        {
            return GetOrAddExisting( WorkflowActivityTypeCache.CacheKey( workflowActivityTypeModel.Id ),
                () => LoadByModel( workflowActivityTypeModel ) );
        }

        private static WorkflowActivityTypeCache LoadByModel( WorkflowActivityType workflowActivityTypeModel )
        {
            if ( workflowActivityTypeModel != null )
            {
                return new WorkflowActivityTypeCache( workflowActivityTypeModel );
            }
            return null;
        }

        /// <summary>
        /// Removes workflowActivityType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            var activityType = WorkflowActivityTypeCache.Read( id );
            if ( activityType != null )
            {
                foreach ( var actionType in activityType.ActionTypes )
                {
                    WorkflowActionTypeCache.Flush( actionType.Id );
                }
            }
            FlushCache( WorkflowActivityTypeCache.CacheKey( id ) );
        }

        #endregion
    }
}