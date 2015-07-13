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
    /// Represents a role that a <see cref="Rock.Model.GroupMember"/> can have in a <see cref="Rock.Model.Group"/>.  In Rock each member of a group has one 
    /// or more <see cref="Rock.Model.GroupTypeRole">GroupRoles</see> assigned to them (for instance the leader of a group can have both a leader and member role). Examples
    /// of roles include leader, member, team leader, coach, host, etc.
    /// </summary>
    [Table( "GroupTypeRole" )]
    [DataContract]
    public partial class GroupTypeRole : Model<GroupTypeRole>, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this GroupRole is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this GroupRole is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.GroupType"/> that this GroupRole belongs to. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupType"/> that this GroupRole belongs to.
        /// </value>
        [DataMember]
        public int? GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Name of the GroupRole. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the Name of the GroupRole.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user defined description of the GroupRole. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the user defined description of the GroupRole.
        /// </value>
        [DataMember( IsRequired = true )]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the sort order position of the GroupRole.  The lower the SortOrder the higher the GroupRole shows in lists/controls.  
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the sort order of the GroupRole.
        /// </value>
        [DataMember( IsRequired = true )]
        public int Order { get; set; }


        /// <summary>
        /// Gets or sets the maximum count of <see cref="Rock.Model.GroupMember">GroupMembers</see> that a <see cref="Rock.Model.Group"/> can have who 
        /// belong to this GroupRole.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the maximum count of <see cref="Rock.Model.GroupMember">GroupMembers</see> that a <see cref="Rock.Model.Group"/> can have
        /// who belong to this GroupRole. If there isn't a maximum, this value is null.
        /// </value>
        [DataMember]
        public int? MaxCount { get; set; }

        /// <summary>
        /// Gets or sets the minimum count of <see cref="Rock.Model.GroupMember">GroupMembers</see> that a <see cref="Rock.Model.Group"/> can have who belong to this GroupRole.
        /// </summary>
        /// <value>
        /// The min count of <see cref="Rock.Model.GroupMember">GroupMebers</see> that a <see cref="Rock.Model.Group"/> can have who belong to this GroupRole. If there is no minimum
        /// this value is null.
        /// </value>
        [DataMember]
        public int? MinCount { get; set; }


        /// <summary>
        /// Gets or sets a flag indicating if this is a group leader role.  
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this is a GroupLeader role; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsLeader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this role should receive requirements notifications].
        /// </summary>
        /// <value>
        /// <c>true</c> if the role should receive requirements notifications; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ReceiveRequirementsNotifications { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can view.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can view; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CanView { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can edit.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can edit; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CanEdit { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupType"/> that this GroupRole belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.GroupType"/> that this GroupRole belongs to.
        /// </value>
        public virtual GroupType GroupType { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing Name of the GroupRole that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Name of the GroupRole that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Group Role Configuration class.
    /// </summary>
    public partial class GroupRoleConfiguration : EntityTypeConfiguration<GroupTypeRole>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupRoleConfiguration"/> class.
        /// </summary>
        public GroupRoleConfiguration()
        {
            this.HasRequired( p => p.GroupType ).WithMany( p => p.Roles ).HasForeignKey( p => p.GroupTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}
