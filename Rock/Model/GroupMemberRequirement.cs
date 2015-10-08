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
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "GroupMemberRequirement" )]
    [DataContract]
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
        /// Gets or sets the group requirement identifier.
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

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the group member.
        /// </summary>
        /// <value>
        /// The group member.
        /// </value>
        public virtual GroupMember GroupMember { get; set; }

        /// <summary>
        /// Gets or sets the group requirement.
        /// </summary>
        /// <value>
        /// The group requirement.
        /// </value>
        public virtual GroupRequirement GroupRequirement { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( this.GroupMember != null && this.GroupRequirement != null )
            {
                return string.Format( "{0}|{1}", this.GroupMember, this.GroupRequirement );
            }
            else
            {
                return base.ToString();
            }
        }

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
        }
    }

    #endregion
}
