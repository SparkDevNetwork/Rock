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
using Rock.Web.Cache;
using Rock.Workflow;
using System.Data.Entity;

namespace Rock.Model
{
    /// <summary>
    /// WorkflowActionType Logic
    /// </summary>
    public partial class WorkflowActionType
    {
        #region Virtual Properties

        /// <summary>
        /// Gets the <see cref="Rock.Workflow.ActionComponent"/>
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Workflow.ActionComponent"/>
        /// </value>
        public virtual ActionComponent WorkflowAction
        {
            get
            {
                return GetWorkflowAction( this.EntityTypeId );
            }
        }

        /// <summary>
        /// Gets the parent security authority for this ActionType.
        /// </summary>
        /// <value>
        /// The parent security authority for this ActionType.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.ActivityType != null ? this.ActivityType : base.ParentAuthority;
            }
        }

        #endregion Virtual Properties

        #region Public Methods

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
        /// Gets the workflow action.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        public static ActionComponent GetWorkflowAction( int entityTypeId )
        {
            var entityType = EntityTypeCache.Get( entityTypeId );
            if ( entityType != null )
            {
                foreach ( var serviceEntry in ActionContainer.Instance.Components )
                {
                    var component = serviceEntry.Value.Value;
                    string componentName = component.GetType().FullName;
                    if ( componentName == entityType.Name )
                    {
                        return component;
                    }
                }
            }
            return null;
        }

        #endregion Public Methods

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return WorkflowActionTypeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            var workflowTypeId = WorkflowActivityTypeCache.Get( this.ActivityTypeId, dbContext as RockContext )?.WorkflowTypeId;
            if ( workflowTypeId.HasValue )
            {
                WorkflowTypeCache.UpdateCachedEntity( workflowTypeId.Value, EntityState.Modified );
            }

            WorkflowActivityTypeCache.UpdateCachedEntity( this.ActivityTypeId, EntityState.Modified ); 
            WorkflowActionTypeCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion ICacheable
    }
}

