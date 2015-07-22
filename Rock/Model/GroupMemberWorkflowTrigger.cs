// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// A Group Member Workflow Trigger defined a workflow that should be triggered to start when certain group member changes are made..
    /// </summary>
    [Table( "GroupMemberWorkflowTrigger" )]
    [DataContract]
    public partial class GroupMemberWorkflowTrigger : Entity<GroupMemberWorkflowTrigger>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if the WorkflowTrigger is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the WorkflowTrigger is active; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        [DataMember]
        public int? GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the WorkflowTypeId of the <see cref="Rock.Model.WorkflowType"/> that is executed by this WorkflowTrigger. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing WorkflowTypeId of the <see cref="Rock.Model.WorkflowType"/> that is executed by the WorkflowTrigger.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int WorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the type of the trigger.
        /// </summary>
        /// <value>
        /// The type of the trigger.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public GroupMemberWorkflowTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the type qualifier.
        /// </summary>
        /// <value>
        /// The type qualifier.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string TypeQualifier { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow trigger.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the workflow trigger.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string WorkflowName { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        [DataMember]
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowType"/> that is executed by this WorkflowTrigger.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowType"/> that is executed by this WorkflowTrigger.
        /// </value>
        [DataMember]
        public virtual WorkflowType WorkflowType { get; set; }

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

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// EntityTypeWorkflowTrigger Configuration class.
    /// </summary>
    public partial class GroupMemberWorkflowTriggerConfiguration : EntityTypeConfiguration<GroupMemberWorkflowTrigger>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberWorkflowTriggerConfiguration"/> class.
        /// </summary>
        public GroupMemberWorkflowTriggerConfiguration()
        {
            this.HasOptional( t => t.GroupType ).WithMany( g => g.GroupMemberWorkflowTriggers ).HasForeignKey( t => t.GroupTypeId ).WillCascadeOnDelete( true );
            this.HasOptional( t => t.Group ).WithMany( g => g.GroupMemberWorkflowTriggers ).HasForeignKey( t => t.GroupId ).WillCascadeOnDelete( true );
            this.HasRequired( t => t.WorkflowType ).WithMany().HasForeignKey( t => t.WorkflowTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Type of workflow trigger
    /// </summary>
    public enum GroupMemberWorkflowTriggerType
    {
        /// <summary>
        /// AddedToGroup
        /// </summary>
        MemberAddedToGroup = 0,

        /// <summary>
        /// RemovedFromGroup
        /// </summary>
        MemberRemovedFromGroup = 1,

        /// <summary>
        /// StatusChanged
        /// </summary>
        MemberStatusChanged = 2,

        /// <summary>
        /// RoleChanged
        /// </summary>
        MemberRoleChanged = 3,

        /// <summary>
        /// Attended
        /// </summary>
        MemberAttendedGroup = 4,

        /// <summary>
        /// Placed Elsewhere
        /// </summary>
        MemberPlacedElsewhere = 5
    }

    #endregion

}
