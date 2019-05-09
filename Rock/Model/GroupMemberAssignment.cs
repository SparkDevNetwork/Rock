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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupMemberAssignment" )]
    [DataContract]
    public class GroupMemberAssignment : Model<GroupMemberAssignment>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the group member identifier.
        /// </summary>
        /// <value>
        /// The group member identifier.
        /// </value>
        [DataMember]
        [Index( "IX_GroupMemberIdLocationIdScheduleId", IsUnique = true, Order = 0 )]
        public int GroupMemberId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        [DataMember]
        [Index( "IX_GroupMemberIdLocationIdScheduleId", IsUnique = true, Order = 1 )]
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        [DataMember]
        [Index( "IX_GroupMemberIdLocationIdScheduleId", IsUnique = true, Order = 2 )]
        public int? ScheduleId { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the group member.
        /// </summary>
        /// <value>
        /// The group member.
        /// </value>
        public virtual GroupMember GroupMember { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        [DataMember]
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [DataMember]
        public virtual Schedule Schedule { get; set; }

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
            return $"{GroupMember} in {this.GroupMember.Group} is assigned to {Location.ToString() ?? "any location"} at {Schedule.ToString() ?? "any schedule"}. ";
        }

        #endregion
    }

    /// <summary>
    /// GroupMemberAssignment EntityTypeConfiguration
    /// </summary>
    public class GroupMemberAssignmentConfiguration : EntityTypeConfiguration<GroupMemberAssignment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberAssignmentConfiguration"/> class.
        /// </summary>
        public GroupMemberAssignmentConfiguration()
        {
            this.HasRequired( a => a.GroupMember ).WithMany( a => a.GroupMemberAssignments ).HasForeignKey( a => a.GroupMemberId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.Schedule ).WithMany().HasForeignKey( a => a.ScheduleId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.Location ).WithMany().HasForeignKey( a => a.LocationId ).WillCascadeOnDelete( false );
        }
    }
}
