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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents a connection request
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "ConnectionRequest" )]
    [DataContract]
    public partial class ConnectionRequest : Model<ConnectionRequest>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionOpportunity"/> identifier.
        /// </summary>
        /// <value>
        /// The connection opportunity identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int ConnectionOpportunityId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> identifier.
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
        /// Gets or sets the <see cref="Rock.Model.ConnectionStatus"/> identifier.
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
        /// Gets or sets the <see cref="Rock.Model.Campus"/> identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the assigned <see cref="Rock.Model.Group"/> identifier.
        /// </summary>
        /// <value>
        /// The assigned group identifier.
        /// </value>
        [DataMember]
        public int? AssignedGroupId { get; set; }

        /// <summary>
        /// Gets or sets the assigned group member role identifier.
        /// </summary>
        /// <value>
        /// The assigned group member role identifier.
        /// </value>
        [DataMember]
        public int? AssignedGroupMemberRoleId { get; set; }

        /// <summary>
        /// Gets or sets the assigned group member status.
        /// </summary>
        /// <value>
        /// The assigned group member status.
        /// </value>
        [DataMember]
        public GroupMemberStatus? AssignedGroupMemberStatus { get; set; }

        /// <summary>
        /// Gets or sets the assigned group member attribute values.
        /// </summary>
        /// <value>
        /// The assigned group member attribute values.
        /// </value>
        [DataMember]
        public string AssignedGroupMemberAttributeValues { get; set; }

        /// <summary>
        /// Gets or sets the connector <see cref="Rock.Model.PersonAlias"/> identifier.
        /// </summary>
        /// <value>
        /// The connector person alias identifier.
        /// </value>
        [DataMember]
        public int? ConnectorPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets the created date key.
        /// </summary>
        /// <value>
        /// The created date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int? CreatedDateKey
        {
            get => ( CreatedDateTime == null || CreatedDateTime.Value == default ) ?
                        ( int? ) null :
                        CreatedDateTime.Value.ToString( "yyyyMMdd" ).AsInteger();
            private set { }
        }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionStatus"/>.
        /// </summary>
        /// <value>
        /// The connection status.
        /// </value>
        [DataMember]
        public virtual ConnectionStatus ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionOpportunity"/>.
        /// </summary>
        /// <value>
        /// The connection opportunity.
        /// </value>
        [DataMember]
        public virtual ConnectionOpportunity ConnectionOpportunity { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/>.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        [LavaVisible]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the assigned <see cref="Rock.Model.Group"/>.
        /// </summary>
        /// <value>
        /// The assigned group.
        /// </value>
        [LavaVisible]
        public virtual Group AssignedGroup { get; set; }

        /// <summary>
        /// Gets or sets the connector <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The connector person alias.
        /// </value>
        [LavaVisible]
        public virtual PersonAlias ConnectorPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionRequestWorkflow">ConnectionRequestWorkflows</see> who are associated with the ConnectionRequest.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionRequestWorkflow">ConnectionRequestWorkflows</see> who are associated with the ConnectionRequest.
        /// </value>
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
        [LavaVisible]
        public virtual ICollection<ConnectionRequestActivity> ConnectionRequestActivities
        {
            get { return _connectionRequestActivities; }
            set { _connectionRequestActivities = value; }
        }

        private ICollection<ConnectionRequestActivity> _connectionRequestActivities;

        /// <summary>
        /// Gets or sets the created source date.
        /// </summary>
        /// <value>
        /// The created source date.
        /// </value>
        [DataMember]
        public virtual AnalyticsSourceDate CreatedSourceDate { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Connection Request's Name that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Person's FullName that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{ ConnectionOpportunity } Connection Request for { PersonAlias.Person }";
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

            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier OccurrenceDates that aren't in the AnalyticsSourceDate table
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            this.HasOptional( r => r.CreatedSourceDate ).WithMany().HasForeignKey( r => r.CreatedDateKey ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}