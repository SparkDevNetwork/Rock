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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;
using Rock.Workflow;

namespace Rock.Model
{
    /// <summary>
    /// Represents an <see cref="Rock.Model.WorkflowActionType"/> (action or task) that is performed as part of a <see cref="Rock.Model.WorkflowActionType"/>.
    /// </summary>
    [RockDomain( "Workflow" )]
    [Table( "WorkflowActionType" )]
    [DataContract]
    public partial class WorkflowActionType : Model<WorkflowActionType>, IOrdered, ICacheable
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the ActivityTypeId of the <see cref="Rock.Model.WorkflowActivityType"/> that performs this Action Type.
        /// </summary>
        /// <value>
        /// The activity type id.
        /// </value>
        [DataMember]
        public int ActivityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the ActionType
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the ActionType.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order of the ActionType in the <see cref="Rock.Model.WorkflowActivityType" />
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that the action is operating against.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that the action is operating against.
        /// </value>
        [DataMember]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is action completed on success.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is action completed on success; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActionCompletedOnSuccess { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is activity completed on success.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this instance is activity completed on success; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActivityCompletedOnSuccess { get; set; }

        /// <summary>
        /// Gets or sets the workflow form identifier.
        /// </summary>
        /// <value>
        /// The workflow form identifier.
        /// </value>
        [DataMember]
        public int? WorkflowFormId { get; set; }

        /// <summary>
        /// Gets or sets the criteria attribute unique identifier.
        /// </summary>
        /// <value>
        /// The criteria attribute unique identifier.
        /// </value>
        [DataMember]
        public Guid? CriteriaAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the type of the criteria comparison.
        /// </summary>
        /// <value>
        /// The type of the criteria comparison.
        /// </value>
        [DataMember]
        public ComparisonType CriteriaComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the criteria value.
        /// </summary>
        /// <value>
        /// The criteria value.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string CriteriaValue { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowActivityType"/> that performs this ActionType.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowActivityType" /> that performs this ActionType.
        /// </value>
        [LavaInclude]
        public virtual WorkflowActivityType ActivityType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of that this ActionType is running against.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

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
        /// Gets or sets the workflow form.
        /// </summary>
        /// <value>
        /// The workflow form.
        /// </value>
        [DataMember]
        public virtual WorkflowActionForm WorkflowForm { get; set; }

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

        #endregion

        #region Methods

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

        #endregion


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

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// ActionType Configuration class.
    /// </summary>
    public partial class WorkflowActionTypeConfiguration : EntityTypeConfiguration<WorkflowActionType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActionTypeConfiguration"/> class.
        /// </summary>
        public WorkflowActionTypeConfiguration()
        {
            this.HasRequired( m => m.ActivityType ).WithMany( m => m.ActionTypes ).HasForeignKey( m => m.ActivityTypeId ).WillCascadeOnDelete( true );
            this.HasOptional( m => m.WorkflowForm ).WithMany().HasForeignKey( m => m.WorkflowFormId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}

