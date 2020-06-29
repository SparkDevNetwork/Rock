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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a connection request
    /// </summary>
    [RockDomain( "Connection" )]
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
        /// Gets or sets the connector person alias identifier.
        /// </summary>
        /// <value>
        /// The connector person alias identifier.
        /// </value>
        [DataMember]
        public int? ConnectorPersonAliasId { get; set; }

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
        [LavaInclude]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the assigned group.
        /// </summary>
        /// <value>
        /// The assigned group.
        /// </value>
        [LavaInclude]
        public virtual Group AssignedGroup { get; set; }

        /// <summary>
        /// Gets or sets the connector person alias.
        /// </summary>
        /// <value>
        /// The connector person alias.
        /// </value>
        [LavaInclude]
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
        [LavaInclude]
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
        public AnalyticsSourceDate CreatedSourceDate { get; set; }
        #endregion

        #region Methods

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check the default authorization on the current type, and
        /// then the authorization on the Rock.Security.GlobalDefault entity
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.ConnectionOpportunity ?? base.ParentAuthority;
            }
        }

        /// <summary>
        /// Determines whether the specified action is authorized.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>True if the person is authorized; false otherwise.</returns>
        public override bool IsAuthorized( string action, Person person )
        {
            if ( this.ConnectionOpportunity != null
                && this.ConnectionOpportunity.ConnectionType != null
                && this.ConnectionOpportunity.ConnectionType.EnableRequestSecurity
                && this.ConnectorPersonAlias != null
                && this.ConnectorPersonAlias.PersonId == person.Id )
            {
                return true;
            }
            else
            {
                return base.IsAuthorized( action, person );
            }
        }

        /// <summary>
        /// Pres the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, DbEntityEntry entry )
        {
            var transaction = new Rock.Transactions.ConnectionRequestChangeTransaction( entry );
            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Rock.Data.DbContext dbContext )
        {
            if ( ConnectionStatus == null )
            {
                ConnectionStatus = new ConnectionStatusService( ( RockContext ) dbContext ).Get( ConnectionStatusId );
            }

            if ( ConnectionStatus != null && ConnectionStatus.AutoInactivateState && ConnectionState != ConnectionState.Inactive )
            {
                ConnectionState = ConnectionState.Inactive;
                var rockContext = ( RockContext ) dbContext;
                rockContext.SaveChanges();
            }

            base.PostSaveChanges( dbContext );
        }

        /// <summary>
        /// Get a list of all inherited Attributes that should be applied to this entity.
        /// </summary>
        /// <returns>A list of all inherited AttributeCache objects.</returns>
        public override List<AttributeCache> GetInheritedAttributes( Rock.Data.RockContext rockContext )
        {
            var connectionOpportunity = this.ConnectionOpportunity;
            if ( connectionOpportunity == null && this.ConnectionOpportunityId > 0 )
            {
                connectionOpportunity = new ConnectionOpportunityService( rockContext )
                    .Queryable().AsNoTracking()
                    .FirstOrDefault( g => g.Id == this.ConnectionOpportunityId );
            }

            if ( connectionOpportunity != null )
            {
                var connectionType = connectionOpportunity.ConnectionType;

                if ( connectionType != null )
                {
                    return connectionType.GetInheritedAttributesForQualifier( rockContext, TypeId, "ConnectionTypeId" );
                }
            }

            return null;
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

    /// <summary>
    /// Connected
    /// </summary>
    Connected = 3,
}

#endregion