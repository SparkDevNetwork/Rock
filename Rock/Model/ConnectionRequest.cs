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
using Rock.Tasks;
using Rock.Web.Cache;
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

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the history change list.
        /// </summary>
        /// <value>
        /// The history change list.
        /// </value>
        [NotMapped]
        public virtual History.HistoryChangeList HistoryChangeList { get; set; }

        #endregion

        #region Virtual Properties

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
            var processConnectionRequestChangeMessage = GetProcessConnectionRequestChangeMessage( entry );
            processConnectionRequestChangeMessage.Send();

            var rockContext = ( RockContext ) dbContext;

            HistoryChangeList = new History.HistoryChangeList();

            switch ( entry.State )
            {
                case EntityState.Added:
                    {
                        HistoryChangeList.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "ConnectionRequest" );

                        History.EvaluateChange( HistoryChangeList, "Connector", string.Empty, History.GetValue<PersonAlias>( ConnectorPersonAlias, ConnectorPersonAliasId, rockContext ) );
                        History.EvaluateChange( HistoryChangeList, "ConnectionStatus", string.Empty, History.GetValue<ConnectionStatus>( ConnectionStatus, ConnectionStatusId, rockContext ) );
                        History.EvaluateChange( HistoryChangeList, "ConnectionState", null, ConnectionState );
                        break;
                    }

                case EntityState.Modified:
                    {
                        string originalConnector = History.GetValue<PersonAlias>( null, entry.OriginalValues["ConnectorPersonAliasId"].ToStringSafe().AsIntegerOrNull(), rockContext );
                        string connector = History.GetValue<PersonAlias>( ConnectorPersonAlias, ConnectorPersonAliasId, rockContext );
                        History.EvaluateChange( HistoryChangeList, "Connector", originalConnector, connector );

                        int? originalConnectionStatusId = entry.OriginalValues["ConnectionStatusId"].ToStringSafe().AsIntegerOrNull();
                        int? connectionStatusId = ConnectionStatus != null ? ConnectionStatus.Id : ConnectionStatusId;
                        if ( !connectionStatusId.Equals( originalConnectionStatusId ) )
                        {
                            string origConnectionStatus = History.GetValue<ConnectionStatus>( null, originalConnectionStatusId, rockContext );
                            string connectionStatus = History.GetValue<ConnectionStatus>( ConnectionStatus, ConnectionStatusId, rockContext );
                            History.EvaluateChange( HistoryChangeList, "ConnectionStatus", origConnectionStatus, connectionStatus );
                        }

                        History.EvaluateChange( HistoryChangeList, "ConnectionState", entry.OriginalValues["ConnectionState"].ToStringSafe().ConvertToEnum<ConnectionState>(), ConnectionState );
                        break;
                    }

                case EntityState.Deleted:
                    {
                        HistoryChangeList.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "ConnectionRequest" );
                        break;
                    }
            }

            base.PreSaveChanges( dbContext, entry );
        }

        private ProcessConnectionRequestChange.Message GetProcessConnectionRequestChangeMessage( DbEntityEntry entry )
        {
            var transaction = new ProcessConnectionRequestChange.Message();

            // If entity was a connection request, save the values
            var connectionRequest = entry.Entity as ConnectionRequest;
            if ( connectionRequest != null )
            {
                transaction.State = entry.State;

                // If this isn't a deleted connection request, get the connection request guid
                if ( transaction.State != EntityState.Deleted )
                {
                    transaction.ConnectionRequestGuid = connectionRequest.Guid;

                    if ( connectionRequest.PersonAlias != null )
                    {
                        transaction.PersonId = connectionRequest.PersonAlias.PersonId;
                    }
                    else if ( connectionRequest.PersonAliasId != default )
                    {
                        transaction.PersonId = new PersonAliasService( new RockContext() ).GetPersonId( connectionRequest.PersonAliasId );
                    }

                    if ( connectionRequest.ConnectionOpportunity != null )
                    {
                        transaction.ConnectionTypeId = connectionRequest.ConnectionOpportunity.ConnectionTypeId;
                    }

                    transaction.ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId;
                    transaction.ConnectorPersonAliasId = connectionRequest.ConnectorPersonAliasId;
                    transaction.ConnectionState = connectionRequest.ConnectionState;
                    transaction.ConnectionStatusId = connectionRequest.ConnectionStatusId;
                    transaction.AssignedGroupId = connectionRequest.AssignedGroupId;

                    if ( transaction.State == EntityState.Modified )
                    {
                        var dbOpportunityIdProperty = entry.Property( "ConnectionOpportunityId" );
                        if ( dbOpportunityIdProperty != null )
                        {
                            transaction.PreviousConnectionOpportunityId = dbOpportunityIdProperty.OriginalValue as int?;
                        }

                        var dbConnectorPersonAliasIdProperty = entry.Property( "ConnectorPersonAliasId" );
                        if ( dbConnectorPersonAliasIdProperty != null )
                        {
                            transaction.PreviousConnectorPersonAliasId = dbConnectorPersonAliasIdProperty.OriginalValue as int?;
                        }

                        var dbStateProperty = entry.Property( "ConnectionState" );
                        if ( dbStateProperty != null )
                        {
                            transaction.PreviousConnectionState = ( ConnectionState ) dbStateProperty.OriginalValue;
                        }

                        var dbStatusProperty = entry.Property( "ConnectionStatusId" );
                        if ( dbStatusProperty != null )
                        {
                            transaction.PreviousConnectionStatusId = ( int ) dbStatusProperty.OriginalValue;
                        }

                        var dbAssignedGroupIdProperty = entry.Property( "AssignedGroupId" );
                        if ( dbAssignedGroupIdProperty != null )
                        {
                            transaction.PreviousAssignedGroupId = dbAssignedGroupIdProperty.OriginalValue as int?;
                        }
                    }
                }
            }

            return transaction;
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

            if ( HistoryChangeList?.Any() == true )
            {
                HistoryService.SaveChanges( ( RockContext ) dbContext, typeof( ConnectionRequest ), Rock.SystemGuid.Category.HISTORY_CONNECTION_REQUEST.AsGuid(), this.Id, HistoryChangeList, true, this.ModifiedByPersonAliasId );
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