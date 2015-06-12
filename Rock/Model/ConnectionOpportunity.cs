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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a connection opportunity
    /// </summary>
    [Table( "ConnectionOpportunity" )]
    [DataContract]
    public partial class ConnectionOpportunity : Model<ConnectionOpportunity>
    {

        #region Entity Properties

        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string PublicName { get; set; }

        [Required]
        [DataMember( IsRequired = true )]
        public string Description { get; set; }

        [DataMember]
        public int? PhotoId { get; set; }

        [Required]
        [DataMember( IsRequired = true )]
        public int ConnectionTypeId { get; set; }

        [Required]
        [DataMember]
        public int GroupTypeId { get; set; }

        [DataMember]
        public int? ConnectorGroupId { get; set; }

        [DataMember]
        public string IconCssClass { get; set; }

        [Required]
        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public int? GroupMemberRoleId { get; set; }

        [DataMember]
        public int? GroupMemberStatusId { get; set; }

        [Required]
        [DataMember]
        public bool UseAllGroupsOfType { get; set; }

        #endregion

        #region Virtual Properties

        [DataMember]
        public virtual ConnectionType ConnectionType { get; set; }

        [DataMember]
        public virtual Group ConnectorGroup { get; set; }

        [DataMember]
        public virtual GroupType GroupType { get; set; }

        [DataMember]
        public virtual GroupTypeRole GroupMemberRole { get; set; }

        [DataMember]
        public virtual GroupMemberStatus GroupMemberStatus { get; set; }

        [DataMember]
        public virtual ICollection<ConnectionOpportunityGroup> ConnectionOpportunityGroups
        {
            get { return _connectionOpportunityGroups ?? ( _connectionOpportunityGroups = new Collection<ConnectionOpportunityGroup>() ); }
            set { _connectionOpportunityGroups = value; }
        }

        private ICollection<ConnectionOpportunityGroup> _connectionOpportunityGroups;

        [DataMember]
        public virtual ICollection<ConnectionWorkflow> ConnectionWorkflows
        {
            get { return _connectionWorkflows ?? ( _connectionWorkflows = new Collection<ConnectionWorkflow>() ); }
            set { _connectionWorkflows = value; }
        }

        private ICollection<ConnectionWorkflow> _connectionWorkflows;

        [DataMember]
        public virtual ICollection<ConnectionRequest> ConnectionRequests
        {
            get { return _connectionRequests ?? ( _connectionRequests = new Collection<ConnectionRequest>() ); }
            set { _connectionRequests = value; }
        }

        private ICollection<ConnectionRequest> _connectionRequests;

        [DataMember]
        public virtual ICollection<ConnectionOpportunityCampus> ConnectionOpportunityCampuses
        {
            get { return _connectionOpportunityCampuses ?? ( _connectionOpportunityCampuses = new Collection<ConnectionOpportunityCampus>() ); }
            set { _connectionOpportunityCampuses = value; }
        }

        private ICollection<ConnectionOpportunityCampus> _connectionOpportunityCampuses;

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// ConnectionOpportunity Configuration class.
    /// </summary>
    public partial class ConnectionOpportunityConfiguration : EntityTypeConfiguration<ConnectionOpportunity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionOpportunityConfiguration" /> class.
        /// </summary>
        public ConnectionOpportunityConfiguration()
        {
            this.HasOptional( p => p.GroupMemberRole ).WithMany().HasForeignKey( p => p.GroupMemberRoleId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ConnectorGroup ).WithMany().HasForeignKey( p => p.ConnectorGroupId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.GroupType ).WithMany().HasForeignKey( p => p.GroupTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.ConnectionType ).WithMany( p => p.ConnectionOpportunities ).HasForeignKey( p => p.ConnectionTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}