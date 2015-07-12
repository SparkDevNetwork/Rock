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
    /// Represents a connection request
    /// </summary>
    [Table( "ConnectionRequest" )]
    [DataContract]
    public partial class ConnectionRequest : Model<ConnectionRequest>
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
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [Required]
        [DataMember]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        /// <value>
        /// The comments.
        /// </value>
        [DataMember]
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the connection status identifier.
        /// </summary>
        /// <value>
        /// The connection status identifier.
        /// </value>
        [Required]
        [DataMember]
        public int ConnectionStatusId { get; set; }

        /// <summary>
        /// Gets or sets the state of the connection.
        /// </summary>
        /// <value>
        /// The state of the connection.
        /// </value>
        [Required]
        [DataMember]
        public ConnectionState ConnectionState { get; set; }

        /// <summary>
        /// Gets or sets the followup date.
        /// </summary>
        /// <value>
        /// The followup date.
        /// </value>
        [DataMember]
        public DateTime? FollowupDate { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the assigned group identifier.
        /// </summary>
        /// <value>
        /// The assigned group identifier.
        /// </value>
        [DataMember]
        public int? AssignedGroupId { get; set; }

        /// <summary>
        /// Gets or sets the connector person alias identifier.
        /// </summary>
        /// <value>
        /// The connector person alias identifier.
        /// </value>
        [DataMember]
        public int? ConnectorPersonAliasId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        /// <value>
        /// The connection status.
        /// </value>
        [DataMember]
        public virtual ConnectionStatus ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunity.
        /// </summary>
        /// <value>
        /// The connection opportunity.
        /// </value>
        [DataMember]
        public virtual ConnectionOpportunity ConnectionOpportunity { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        [DataMember]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the assigned group.
        /// </summary>
        /// <value>
        /// The assigned group.
        /// </value>
        [DataMember]
        public virtual Group AssignedGroup { get; set; }

        /// <summary>
        /// Gets or sets the connector person alias.
        /// </summary>
        /// <value>
        /// The connector person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ConnectorPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionRequestWorkflow">ConnectionRequestWorkflows</see> who are associated with the ConnectionRequest.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionRequestWorkflow">ConnectionRequestWorkflows</see> who are associated with the ConnectionRequest.
        /// </value>
        [DataMember]
        public virtual ICollection<ConnectionRequestWorkflow> ConnectionRequestWorkflows
        {
            get { return _connectionRequestWorkflows; }
            set { _connectionRequestWorkflows = value; }
        }

        private ICollection<ConnectionRequestWorkflow> _connectionRequestWorkflows;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionRequestActivity">ConnectionRequestActivities</see> who are associated with the ConnectionRequest.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionRequestActivity">ConnectionRequestActivities</see> who are associated with the ConnectionRequest.
        /// </value>
        [DataMember]
        public virtual ICollection<ConnectionRequestActivity> ConnectionRequestActivities
        {
            get { return _connectionRequestActivities; }
            set { _connectionRequestActivities = value; }
        }

        private ICollection<ConnectionRequestActivity> _connectionRequestActivities;

        #endregion

        #region Methods

        /// <summary>
        /// Pres the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( DbContext dbContext, System.Data.Entity.Infrastructure.DbEntityEntry entry )
        {
            var transaction = new Rock.Transactions.ConnectionRequestChangeTransaction( entry );
            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );

            base.PreSaveChanges( dbContext, entry );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// ConnectionRequest Configuration class.
    /// </summary>
    public partial class ConnectionRequestConfiguration : EntityTypeConfiguration<ConnectionRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionRequestConfiguration" /> class.
        /// </summary>
        public ConnectionRequestConfiguration()
        {
            this.HasRequired( p => p.ConnectionOpportunity ).WithMany( p => p.ConnectionRequests ).HasForeignKey( p => p.ConnectionOpportunityId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.PersonAlias ).WithMany().HasForeignKey( p => p.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ConnectorPersonAlias ).WithMany().HasForeignKey( p => p.ConnectorPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.AssignedGroup ).WithMany().HasForeignKey( p => p.AssignedGroupId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.ConnectionStatus ).WithMany().HasForeignKey( p => p.ConnectionStatusId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}

#region Enumerations

/// <summary>
/// Type of connection state
/// </summary>
public enum ConnectionState
{
    /// <summary>
    /// Active
    /// </summary>
    Active = 0,

    /// <summary>
    /// Inactive
    /// </summary>
    Inactive = 1,

    /// <summary>
    /// Future Follow-up
    /// </summary>
    FutureFollowUp = 2,

}

#endregion