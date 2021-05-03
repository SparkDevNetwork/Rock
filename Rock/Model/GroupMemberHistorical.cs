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
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a snapshot of some of the group member values at a point in history
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupMemberHistorical" )]
    [DataContract]
    public class GroupMemberHistorical : Model<GroupMemberHistorical>, IHistoricalTracking
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the group member id of the group member for this group member historical record
        /// </summary>
        /// <value>
        /// The group member identifier.
        /// </value>
        [DataMember]
        public int GroupMemberId { get; set; }

        /// <summary>
        /// Gets or sets <see cref="Rock.Model.Group">GroupId</see> for this group member record at this point in history
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group role id for this group member at this point in history
        /// </summary>
        /// <value>
        /// The group role identifier.
        /// </value>
        [DataMember]
        public int GroupRoleId { get; set; }

        /// <summary>
        /// Gets or sets the group role name at this point in history
        /// </summary>
        /// <value>
        /// The name of the group role.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string GroupRoleName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group member was IsLeader (which is determined by GroupRole.IsLeader) at this point in history
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is leader; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsLeader { get; set; }

        /// <summary>
        /// Gets or sets the group member status of this group member at this point in history
        /// </summary>
        /// <value>
        /// The group member status.
        /// </value>
        [DataMember]
        public GroupMemberStatus GroupMemberStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this group member was archived at this point in history
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is archived; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsArchived { get; set; }

        /// <summary>
        /// Gets or sets the archived date time value of this group member at this point in history
        /// </summary>
        /// <value>
        /// The archived date time.
        /// </value>
        [DataMember]
        public DateTime? ArchivedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId that archived (soft deleted) this group member at this point in history
        /// </summary>
        /// <value>
        /// The archived by person alias identifier.
        /// </value>
        [DataMember]
        public int? ArchivedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the InActiveDateTime value of the group member at this point in history (the time when the group member status was changed to GroupMemberStatus.Inactive)
        /// </summary>
        /// <value>
        /// The in active date time.
        /// </value>
        [DataMember]
        public DateTime? InactiveDateTime { get; set; }

        #endregion

        #region IHistoricalTracking

        /// <summary>
        /// Gets or sets the effective date.
        /// This is the starting date that the tracked record had the values reflected in this record
        /// </summary>
        /// <value>
        /// The effective date.
        /// </value>
        [DataMember]
        public DateTime EffectiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets the expire date time
        /// This is the last date that the tracked record had the values reflected in this record
        /// For example, if a tracked record's Name property changed on '2016-07-14', the ExpireDate of the previously current record will be '2016-07-13', and the EffectiveDate of the current record will be '2016-07-14'
        /// If this is most current record, the ExpireDate will be '9999-01-01'
        /// </summary>
        /// <value>
        /// The expire date.
        /// </value>
        [DataMember]
        public DateTime ExpireDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [current row indicator].
        /// This will be True if this represents the same values as the current tracked record for this
        /// </summary>
        /// <value>
        ///   <c>true</c> if [current row indicator]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CurrentRowIndicator { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupMember"/> for this group member historical record
        /// </summary>
        /// <value>
        /// The group member.
        /// </value>
        [DataMember]
        public virtual GroupMember GroupMember { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> for this group member record at this point in history
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> that archived (soft deleted) this group member at this point in history
        /// </summary>
        /// <value>
        /// The archived by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ArchivedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupTypeRole"/> for this group member at this point in history
        /// </summary>
        /// <value>
        /// The group role.
        /// </value>
        [DataMember]
        public virtual GroupTypeRole GroupRole { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a GroupMemberHistorical with CurrentRowIndicator = true for the specified groupmember
        /// </summary>
        /// <param name="groupMember">The group member.</param>
        /// <param name="effectiveDateTime">The effective date time.</param>
        /// <returns></returns>
        public static GroupMemberHistorical CreateCurrentRowFromGroupMember( GroupMember groupMember, DateTime effectiveDateTime )
        {
            var groupMemberHistoricalCurrent = new GroupMemberHistorical
            {
                GroupMemberId = groupMember.Id,
                GroupId = groupMember.GroupId,
                GroupRoleId = groupMember.GroupRoleId,
                GroupRoleName = groupMember.GroupRole.Name,
                IsLeader = groupMember.GroupRole.IsLeader,
                GroupMemberStatus = groupMember.GroupMemberStatus,
                IsArchived = groupMember.IsArchived,
                ArchivedDateTime = groupMember.ArchivedDateTime,
                ArchivedByPersonAliasId = groupMember.ArchivedByPersonAliasId,
                InactiveDateTime = groupMember.InactiveDateTime,

                // Set the Modified/Created fields for GroupMemberHistorical to be the current values from GroupMember table
                ModifiedDateTime = groupMember.ModifiedDateTime,
                ModifiedByPersonAliasId = groupMember.ModifiedByPersonAliasId,
                CreatedByPersonAliasId = groupMember.CreatedByPersonAliasId,
                CreatedDateTime = groupMember.CreatedDateTime,

                // Set HistoricalTracking fields
                CurrentRowIndicator = true,
                EffectiveDateTime = effectiveDateTime,
                ExpireDateTime = HistoricalTracking.MaxExpireDateTime
            };

            return groupMemberHistoricalCurrent;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class GroupMemberHistoricalConfiguration : EntityTypeConfiguration<GroupMemberHistorical>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberHistoricalConfiguration"/> class.
        /// </summary>
        public GroupMemberHistoricalConfiguration()
        {
            this.HasRequired( p => p.GroupMember ).WithMany().HasForeignKey( p => p.GroupMemberId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.Group ).WithMany().HasForeignKey( p => p.GroupId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.GroupRole ).WithMany().HasForeignKey( p => p.GroupRoleId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ArchivedByPersonAlias ).WithMany().HasForeignKey( p => p.ArchivedByPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
