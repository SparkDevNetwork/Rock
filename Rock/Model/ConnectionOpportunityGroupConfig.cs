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
    /// Represents a connection opportunity group configuration
    /// </summary>
    [RockDomain( "Connection" )]
    [Table( "ConnectionOpportunityGroupConfig" )]
    [DataContract]
    public partial class ConnectionOpportunityGroupConfig : Model<ConnectionOpportunityGroupConfig>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the connection opportunity identifier.
        /// </summary>
        /// <value>
        /// The connection opportunity identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int ConnectionOpportunityId { get; set; }

        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        [Required]
        [DataMember]
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the group member role identifier.
        /// </summary>
        /// <value>
        /// The group member role identifier.
        /// </value>
        [DataMember]
        public int? GroupMemberRoleId { get; set; }

        /// <summary>
        /// Gets or sets the group member status.
        /// </summary>
        /// <value>
        /// The group member status.
        /// </value>
        [DataMember]
        public GroupMemberStatus GroupMemberStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use all groups of type].
        /// </summary>
        /// <value>
        /// <c>true</c> if [use all groups of type]; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember]
        public bool UseAllGroupsOfType { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the connection opportunity.
        /// </summary>
        /// <value>
        /// The connection opportunity.
        /// </value>
        [LavaInclude]
        public virtual ConnectionOpportunity ConnectionOpportunity { get; set; }

        /// <summary>
        /// Gets or sets the type of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        [LavaInclude]
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the group member role.
        /// </summary>
        /// <value>
        /// The group member role.
        /// </value>
        [LavaInclude]
        public virtual GroupTypeRole GroupMemberRole { get; set; }

        #endregion

        #region Methods

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// ConnectionOpportunityGroupConfig Configuration class.
    /// </summary>
    public partial class ConnectionOpportunityGroupConfigConfiguration : EntityTypeConfiguration<ConnectionOpportunityGroupConfig>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionOpportunityGroupConfigConfiguration" /> class.
        /// </summary>
        public ConnectionOpportunityGroupConfigConfiguration()
        {
            this.HasRequired( p => p.ConnectionOpportunity ).WithMany( p => p.ConnectionOpportunityGroupConfigs ).HasForeignKey( p => p.ConnectionOpportunityId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.GroupMemberRole ).WithMany().HasForeignKey( p => p.GroupMemberRoleId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.GroupType ).WithMany().HasForeignKey( p => p.GroupTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}