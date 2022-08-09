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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupMemberRequirement" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "FF1B2C4B-0F2D-4D9B-9E85-7336CCC24A62" )]
    public partial class GroupMemberRequirement : Model<GroupMemberRequirement>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the group member identifier.
        /// </summary>
        /// <value>
        /// The group member identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int GroupMemberId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupRequirement"/> identifier.
        /// </summary>
        /// <value>
        /// The group requirement identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int GroupRequirementId { get; set; }

        /// <summary>
        /// Gets or sets the requirement met date time.
        /// </summary>
        /// <value>
        /// The requirement met date time.
        /// </value>
        [DataMember]
        public DateTime? RequirementMetDateTime { get; set; }

        /// <summary>
        /// Gets or sets the requirement fail date time.
        /// </summary>
        /// <value>
        /// The requirement fail date time.
        /// </value>
        [DataMember]
        public DateTime? RequirementFailDateTime { get; set; }

        /// <summary>
        /// Gets or sets the requirement warning date time.
        /// </summary>
        /// <value>
        /// The requirement warning date time.
        /// </value>
        [DataMember]
        public DateTime? RequirementWarningDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last requirement check date time.
        /// </summary>
        /// <value>
        /// The last requirement check date time.
        /// </value>
        [DataMember]
        public DateTime? LastRequirementCheckDateTime { get; set; }

        /// <summary>
        /// Gets or sets the "Does Not Meet" <see cref="Rock.Model.Workflow"/> identifier for the group member's requirement.
        /// </summary>
        /// <value>
        /// The workflow identifier.
        /// </value>
        [DataMember]
        public int? DoesNotMeetWorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the "Warning" <see cref="Rock.Model.Workflow"/> identifier for the group member's requirement.
        /// </summary>
        /// <value>
        /// The workflow identifier.
        /// </value>
        [DataMember]
        public int? WarningWorkflowId { get; set; }

        /// <summary>
        /// Gets or sets whether the member requirement was manually completed.
        /// </summary>
        [DataMember]
        [DefaultValue( false )]
        public bool WasManuallyCompleted { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias">PersonAliasId</see> that manually completed this member requirement.
        /// </summary>
        /// <value>
        /// The manually completed by person alias identifier.
        /// </value>
        [DataMember]
        public int? ManuallyCompletedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the manually completed date for the group member requirement.
        /// </summary>
        /// <value>
        /// The manually completed date time.
        /// </value>
        [DataMember]
        public DateTime? ManuallyCompletedDateTime { get; set; }

        /// <summary>
        /// Gets or sets whether the member requirement was overridden.
        /// </summary>
        [DataMember]
        [DefaultValue( false )]
        public bool WasOverridden { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias">PersonAliasId</see> that overrode this member requirement.
        /// </summary>
        /// <value>
        /// The overridden by person alias identifier.
        /// </value>
        [DataMember]
        public int? OverriddenByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the overridden date for the group member requirement.
        /// </summary>
        /// <value>
        /// The overridden date time.
        /// </value>
        [DataMember]
        public DateTime? OverriddenDateTime { get; set; }

        /// <summary>
        /// Gets or sets the due date for the group member requirement.
        /// </summary>
        /// <remarks>
        /// The due date would be:
        /// <br />
        /// 1. Provided by a group administrator when the <see cref="Rock.Model.GroupRequirementType.DueDateType"/> is <see cref="DueDateType.ConfiguredDate"/>.<br />
        /// 2. Calculated from GroupAttribute. Would be equal to the selected date in the attribute plus the <see cref="GroupRequirementType.DueDateOffsetInDays"/>.<br />
        /// OR<br />
        /// 3. Calculated from <see cref="DueDateType.DaysAfterJoining"/>. Would be equal to the date the individual was added to the group plus the <see cref="GroupRequirementType.DueDateOffsetInDays"/><br />
        /// </remarks>
        [DataMember]
        public DateTime? DueDate { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupMember"/>.
        /// </summary>
        /// <value>
        /// The group member.
        /// </value>
        [LavaVisible]
        public virtual GroupMember GroupMember { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupRequirement"/>.
        /// </summary>
        /// <value>
        /// The group requirement.
        /// </value>
        [LavaVisible]
        public virtual GroupRequirement GroupRequirement { get; set; }

        /// <summary>
        /// Gets or sets the "Does Not Meet" <see cref="Rock.Model.Workflow"/>.
        /// </summary>
        /// <value>
        /// The "Does Not Meet" workflow.
        /// </value>
        [LavaVisible]
        public virtual Workflow DoesNotMeetWorkflow { get; set; }

        /// <summary>
        /// Gets or sets the "Warning" <see cref="Rock.Model.Workflow"/>.
        /// </summary>
        /// <value>
        /// The "Warning" workflow.
        /// </value>
        [LavaVisible]
        public virtual Workflow WarningWorkflow { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> of the person who manually completed the member requirement.
        /// </summary>
        /// <value>
        /// The manually completed by person alias.
        /// </value>
        public virtual PersonAlias ManuallyCompletedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> of the person who overrode the member requirement.
        /// </summary>
        /// <value>
        /// The overridden by person alias.
        /// </value>
        public virtual PersonAlias OverriddenByPersonAlias { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// GroupMember Requirement configuration
    /// </summary>
    public partial class GroupMemberRequirementConfiguration : EntityTypeConfiguration<GroupMemberRequirement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberRequirementConfiguration"/> class.
        /// </summary>
        public GroupMemberRequirementConfiguration()
        {
            this.HasRequired( a => a.GroupRequirement ).WithMany().HasForeignKey( a => a.GroupRequirementId ).WillCascadeOnDelete( true );
            this.HasRequired( a => a.GroupMember ).WithMany( a => a.GroupMemberRequirements ).HasForeignKey( a => a.GroupMemberId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.DoesNotMeetWorkflow ).WithMany().HasForeignKey( a => a.DoesNotMeetWorkflowId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.WarningWorkflow ).WithMany().HasForeignKey( a => a.WarningWorkflowId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.ManuallyCompletedByPersonAlias ).WithMany().HasForeignKey( a => a.ManuallyCompletedByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.OverriddenByPersonAlias ).WithMany().HasForeignKey( a => a.OverriddenByPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}