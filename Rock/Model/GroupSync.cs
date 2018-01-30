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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// A collection of group and groupTypeRole values used to sync a group's roles to a dataview.
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupSync" )]
    [DataContract]
    public partial class GroupSync : Model<GroupSync>
    {
        #region Entity Properties
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IX_GroupIdGroupTypeRoleId", 0, IsUnique = true )]
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group type role identifier.
        /// </summary>
        /// <value>
        /// The group type role identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IX_GroupIdGroupTypeRoleId", 1, IsUnique = true )]
        public int GroupTypeRoleId { get; set; }

        /// <summary>
        /// Gets or sets the synchronize data view identifier.
        /// </summary>
        /// <value>
        /// The synchronize data view identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int SyncDataViewId { get; set; }

        /// <summary>
        /// Gets or sets the welcome system email identifier.
        /// </summary>
        /// <value>
        /// The welcome system email identifier.
        /// </value>
        [DataMember]
        public int? WelcomeSystemEmailId { get; set; }

        /// <summary>
        /// Gets or sets the exit system email identifier.
        /// </summary>
        /// <value>
        /// The exit system email identifier.
        /// </value>
        [DataMember]
        public int? ExitSystemEmailId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [add user accounts during synchronize].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [add user accounts during synchronize]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AddUserAccountsDuringSync { get; set; }

        #endregion

        #region Virtual Properties
        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the group type role.
        /// </summary>
        /// <value>
        /// The group type role.
        /// </value>
        [DataMember]
        public virtual GroupTypeRole GroupTypeRole { get; set; }

        /// <summary>
        /// Gets or sets the synchronize data view.
        /// </summary>
        /// <value>
        /// The synchronize data view.
        /// </value>
        [DataMember]
        public virtual Rock.Model.DataView SyncDataView { get; set; }

        /// <summary>
        /// Gets or sets the welcome system email.
        /// </summary>
        /// <value>
        /// The welcome system email.
        /// </value>
        [DataMember]
        public virtual Rock.Model.SystemEmail WelcomeSystemEmail { get; set; }

        /// <summary>
        /// Gets or sets the exit system email.
        /// </summary>
        /// <value>
        /// The exit system email.
        /// </value>
        [DataMember]
        public virtual SystemEmail ExitSystemEmail { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// GroupSync configuration class
    /// </summary>
    public partial class GroupSyncConfiguration : EntityTypeConfiguration<GroupSync>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupSyncConfiguration"/> class.
        /// </summary>
        public GroupSyncConfiguration()
        {
            HasRequired( g => g.Group ).WithMany( g => g.GroupSyncs ).HasForeignKey( g => g.GroupId ).WillCascadeOnDelete( true );
            HasRequired( g => g.GroupTypeRole ).WithMany().HasForeignKey( g => g.GroupTypeRoleId ).WillCascadeOnDelete( false );
            HasRequired( g => g.SyncDataView ).WithMany().HasForeignKey( g => g.SyncDataViewId ).WillCascadeOnDelete( false );
            HasOptional( g => g.WelcomeSystemEmail ).WithMany().HasForeignKey( g => g.WelcomeSystemEmailId ).WillCascadeOnDelete( false );
            HasOptional( g => g.ExitSystemEmail ).WithMany().HasForeignKey( g => g.ExitSystemEmailId ).WillCascadeOnDelete( false );
        }
    }
    #endregion
}
