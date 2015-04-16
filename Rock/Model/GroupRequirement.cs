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
    /// 
    /// </summary>
    [Table( "GroupRequirement" )]
    [DataContract]
    public partial class GroupRequirement : Model<GroupRequirement>
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
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group requirement type identifier.
        /// </summary>
        /// <value>
        /// The group requirement type identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int GroupRequirementTypeId { get; set; }

        /// <summary>
        /// The specific GroupRoleId that this requirement is for. NULL means this requirement applies to all roles.
        /// </summary>
        /// <value>
        /// The group role identifier.
        /// </value>
        [DataMember]
        public int? GroupRoleId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the type of the group requirement.
        /// </summary>
        /// <value>
        /// The type of the group requirement.
        /// </value>
        [DataMember]
        public virtual GroupRequirementType GroupRequirementType { get; set; }

        /// <summary>
        /// The specific Group Role that this requirement is for. NULL means this requirement applies to all roles.
        /// </summary>
        /// <value>
        /// The group type role.
        /// </value>
        public virtual GroupTypeRole GroupRole { get; set; }

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
            if ( this.GroupRequirementType != null && this.Group != null )
            {
                return string.Format( "{0}|{1}", this.GroupRequirementType, this.Group );
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
    /// 
    /// </summary>
    public partial class GroupRequirementConfiguration : EntityTypeConfiguration<GroupRequirement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupRequirementConfiguration"/> class.
        /// </summary>
        public GroupRequirementConfiguration()
        {
            // NOTE: would be nice if this would cascade delete, but doing so results in a "may cause cycles or multiple cascade paths" error
            this.HasRequired( a => a.Group ).WithMany(a => a.GroupRequirements).HasForeignKey( a => a.GroupId ).WillCascadeOnDelete( false );
            
            this.HasRequired( a => a.GroupRequirementType ).WithMany().HasForeignKey( a => a.GroupRequirementTypeId ).WillCascadeOnDelete( true );
            this.HasRequired( a => a.GroupRole ).WithMany().HasForeignKey( a => a.GroupRoleId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}
