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

using System.Linq;
using Rock.Data;
using Rock.Tasks;

namespace Rock.Model
{
    public partial class ConnectionRequest
    {
        /// <summary>
        /// Save hook implementation for <see cref="ConnectionRequest"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<ConnectionRequest>
        {
            private History.HistoryChangeList HistoryChangeList { get; set; }
            private History.HistoryChangeList PersonHistoryChangeList { get; set; }

            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                HistoryChangeList = new History.HistoryChangeList();
                PersonHistoryChangeList = new History.HistoryChangeList();
                var connectionRequest = this.Entity as ConnectionRequest;
                var processConnectionRequestChangeMessage = GetProcessConnectionRequestChangeMessage( Entry, connectionRequest );
                processConnectionRequestChangeMessage.Send();

                var rockContext = ( RockContext ) this.RockContext;
                var connectionOpportunity = connectionRequest.ConnectionOpportunity;
                if ( connectionOpportunity == null )
                {
                    connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( connectionRequest.ConnectionOpportunityId );
                }

                switch ( State )
                {
                    case EntityContextState.Added:
                        {
                            HistoryChangeList.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "ConnectionRequest" );

                            History.EvaluateChange( HistoryChangeList, "Connector", string.Empty, History.GetValue<PersonAlias>( connectionRequest.ConnectorPersonAlias, connectionRequest.ConnectorPersonAliasId, rockContext ) );
                            History.EvaluateChange( HistoryChangeList, "ConnectionStatus", string.Empty, History.GetValue<ConnectionStatus>( connectionRequest.ConnectionStatus, connectionRequest.ConnectionStatusId, rockContext ) );
                            History.EvaluateChange( HistoryChangeList, "ConnectionState", null, connectionRequest.ConnectionState );
                            PersonHistoryChangeList.AddChange( History.HistoryVerb.ConnectionRequestAdded, History.HistoryChangeType.Record, connectionOpportunity.Name );
                            if ( connectionRequest.ConnectionState == ConnectionState.Connected )
                            {
                                PersonHistoryChangeList.AddChange( History.HistoryVerb.ConnectionRequestConnected, History.HistoryChangeType.Record, connectionOpportunity.Name );
                            }
                            break;
                        }

                    case EntityContextState.Modified:
                        {
                            var originalConnectorPersonAliasId = Entry.OriginalValues[nameof( ConnectionRequest.ConnectorPersonAliasId )].ToStringSafe().AsIntegerOrNull();
                            string originalConnector = History.GetValue<PersonAlias>( null, Entry.OriginalValues[nameof( ConnectionRequest.ConnectorPersonAliasId )].ToStringSafe().AsIntegerOrNull(), rockContext );
                            string connector = History.GetValue<PersonAlias>( connectionRequest.ConnectorPersonAlias, connectionRequest.ConnectorPersonAliasId, rockContext );
                            History.EvaluateChange( HistoryChangeList, "Connector", originalConnector, connector );

                            int? originalConnectionStatusId = Entry.OriginalValues[nameof( ConnectionRequest.ConnectionStatusId )].ToStringSafe().AsIntegerOrNull();
                            int? connectionStatusId = connectionRequest.ConnectionStatus != null ? connectionRequest.ConnectionStatus.Id : connectionRequest.ConnectionStatusId;
                            if ( !connectionStatusId.Equals( originalConnectionStatusId ) )
                            {
                                string origConnectionStatus = History.GetValue<ConnectionStatus>( null, originalConnectionStatusId, rockContext );
                                string connectionStatus = History.GetValue<ConnectionStatus>( connectionRequest.ConnectionStatus, connectionRequest.ConnectionStatusId, rockContext );
                                History.EvaluateChange( HistoryChangeList, "ConnectionStatus", origConnectionStatus, connectionStatus );
                                PersonHistoryChangeList.AddChange( History.HistoryVerb.ConnectionRequestStatusModify, History.HistoryChangeType.Record, connectionOpportunity.Name );
                            }

                            var originalConnectionState = Entry.OriginalValues[nameof( ConnectionRequest.ConnectionState )].ToStringSafe().ConvertToEnum<ConnectionState>();
                            History.EvaluateChange( HistoryChangeList, "ConnectionState", Entry.OriginalValues[nameof( ConnectionRequest.ConnectionState )].ToStringSafe().ConvertToEnum<ConnectionState>(), connectionRequest.ConnectionState );
                            if ( connectionRequest.ConnectionState != originalConnectionState )
                            {
                                if ( connectionRequest.ConnectionState == ConnectionState.Connected )
                                {
                                    PersonHistoryChangeList.AddChange( History.HistoryVerb.ConnectionRequestConnected, History.HistoryChangeType.Record, connectionOpportunity.Name );
                                }
                                else
                                {
                                    PersonHistoryChangeList.AddChange( History.HistoryVerb.ConnectionRequestStateModify, History.HistoryChangeType.Record, connectionOpportunity.Name );
                                }
                            }

                            break;
                        }

                    case EntityContextState.Deleted:
                        {
                            HistoryChangeList.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "ConnectionRequest" );
                            PersonHistoryChangeList.AddChange( History.HistoryVerb.ConnectionRequestDelete, History.HistoryChangeType.Record, connectionOpportunity.Name );
                            break;
                        }
                }

                base.PreSave();
            }

            /// <summary>
            /// Called after the save operation has been executed
            /// </summary>
            /// <remarks>
            /// This method is only called if <see cref="M:Rock.Data.EntitySaveHook`1.PreSave" /> returns
            /// without error.
            /// </remarks>
            protected override void PostSave()
            {
                var rockContext = ( RockContext ) this.RockContext;
                if ( Entity.ConnectionStatus == null )
                {
                    Entity.ConnectionStatus = new ConnectionStatusService( rockContext ).Get( Entity.ConnectionStatusId );
                }

                if ( Entity.ConnectionStatus != null && Entity.ConnectionStatus.AutoInactivateState && Entity.ConnectionState != ConnectionState.Inactive )
                {
                    Entity.ConnectionState = ConnectionState.Inactive;
                    rockContext.SaveChanges();
                }

                if ( HistoryChangeList?.Any() == true )
                {
                    HistoryService.SaveChanges( rockContext, typeof( ConnectionRequest ), Rock.SystemGuid.Category.HISTORY_CONNECTION_REQUEST.AsGuid(), Entity.Id, HistoryChangeList, true, Entity.ModifiedByPersonAliasId );
                }

                if ( PersonHistoryChangeList?.Any() == true )
                {
                    var personAlias = Entity.PersonAlias ?? new PersonAliasService( rockContext ).Get( Entity.PersonAliasId );
                    HistoryService.SaveChanges(
                                rockContext,
                                typeof( Person ),
                                Rock.SystemGuid.Category.HISTORY_PERSON_CONNECTION_REQUEST.AsGuid(),
                                personAlias.PersonId,
                                PersonHistoryChangeList,
                                "Request",
                                typeof( ConnectionRequest ),
                                Entity.Id,
                                true,
                                Entity.ModifiedByPersonAliasId,
                                rockContext.SourceOfChange );
                }

                base.PostSave();
            }

            private ProcessConnectionRequestChange.Message GetProcessConnectionRequestChangeMessage( IEntitySaveEntry entry, ConnectionRequest connectionRequest )
            {
                var transaction = new ProcessConnectionRequestChange.Message();

                if ( connectionRequest != null )
                {
                    transaction.State = entry.State;

                    // If this isn't a deleted connection request, get the connection request guid
                    if ( transaction.State != EntityContextState.Deleted )
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

                        if ( transaction.State == EntityContextState.Modified )
                        {
                            transaction.PreviousConnectionOpportunityId = entry.OriginalValues[nameof( ConnectionRequest.ConnectionOpportunityId )].ToStringSafe().AsIntegerOrNull();
                            transaction.PreviousConnectorPersonAliasId = entry.OriginalValues[nameof( ConnectionRequest.ConnectorPersonAliasId )].ToStringSafe().AsIntegerOrNull();
                            transaction.ConnectionState = entry.OriginalValues[nameof( ConnectionRequest.ConnectionState )].ToStringSafe().ConvertToEnum<ConnectionState>();
                            transaction.PreviousConnectionStatusId = entry.OriginalValues[nameof( ConnectionRequest.ConnectionStatusId )].ToStringSafe().AsInteger();
                            transaction.PreviousAssignedGroupId = entry.OriginalValues[nameof( ConnectionRequest.AssignedGroupId )].ToStringSafe().AsIntegerOrNull();
                        }
                    }
                }

                return transaction;
            }
        }
    }
}
