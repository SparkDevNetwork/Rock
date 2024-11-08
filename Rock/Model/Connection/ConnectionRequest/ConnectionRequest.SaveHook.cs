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

using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Rock.Data;
using Rock.Tasks;

namespace Rock.Model
{
    public partial class ConnectionRequest
    {
        private HashSet<int> _processedConnectionStatusAutomations = null;
        private bool _runAutomationsInPostSaveChanges = true;

        /// <summary>
        /// To help protect again an infinite loop, keep track of the automations
        /// that have already run.
        /// </summary>
        /// <value>The processed connection status automations.</value>
        private HashSet<int> processedConnectionStatusAutomations
        {
            get
            {
                if ( _processedConnectionStatusAutomations == null )
                {
                    _processedConnectionStatusAutomations = new HashSet<int>();
                }

                return _processedConnectionStatusAutomations;
            }
        }

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
                // Get the current person's alias ID from the current context.
                var currentPersonAliasId = DbContext.GetCurrentPersonAlias()?.Id;

                HistoryChangeList = new History.HistoryChangeList();
                PersonHistoryChangeList = new History.HistoryChangeList();
                var connectionRequest = this.Entity as ConnectionRequest;

                var rockContext = ( RockContext ) this.RockContext;
                var connectionOpportunity = connectionRequest.ConnectionOpportunity;
                if ( connectionOpportunity == null )
                {
                    connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( connectionRequest.ConnectionOpportunityId );
                }

                //Just because connection opportunity is always loaded, we are populating ConnectionTypeId all the times except delete.
                if ( this.State != EntityContextState.Deleted && connectionOpportunity != null )
                {
                    this.Entity.ConnectionTypeId = connectionOpportunity.ConnectionTypeId;
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
                // Get the current person's alias ID from the current context.
                var currentPersonAliasId = DbContext.GetCurrentPersonAlias()?.Id;
                var connectionRequest = this.Entity as ConnectionRequest;

                // Create and send the change notification message now that the connection request has been saved.
                var processConnectionRequestChangeMessage = GetProcessConnectionRequestChangeMessage( Entry, connectionRequest, currentPersonAliasId );
                processConnectionRequestChangeMessage.SendWhen( this.DbContext.WrappedTransactionCompletedTask );

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

                switch ( State )
                {
                    case EntityContextState.Added:
                        {
                            var connectionRequestService = new ConnectionRequestService( rockContext );
                            var requestsOfStatus = connectionRequestService.Queryable()
                            .Where( r =>
                                r.ConnectionStatusId == connectionRequest.ConnectionStatusId &&
                                r.ConnectionOpportunityId == connectionRequest.ConnectionOpportunityId &&
                                r.Id != connectionRequest.Id );

                            if ( connectionRequest.Order > 0 )
                            {
                                requestsOfStatus = requestsOfStatus.Where( r => r.Order >= connectionRequest.Order );
                            }

                            rockContext.BulkUpdate( requestsOfStatus, r => new ConnectionRequest { Order = r.Order + 1, ModifiedDateTime = r.ModifiedDateTime } );
                            break;
                        }
                    case EntityContextState.Deleted:
                        {
                            var connectionRequestService = new ConnectionRequestService( rockContext );
                            var requestsOfStatus = connectionRequestService.Queryable()
                            .Where( r =>
                                r.ConnectionStatusId == connectionRequest.ConnectionStatusId &&
                                r.ConnectionOpportunityId == connectionRequest.ConnectionOpportunityId &&
                                r.Order > connectionRequest.Order &&
                                r.Id != connectionRequest.Id );
                            rockContext.BulkUpdate( requestsOfStatus, r => new ConnectionRequest { Order = r.Order - 1, ModifiedDateTime = r.ModifiedDateTime } );
                            break;
                        }
                }

                var connectionStatusAutomationsQuery = new ConnectionStatusAutomationService( rockContext ).Queryable().Where( a => a.SourceStatusId == Entity.ConnectionStatusId );

                if ( this.Entity._runAutomationsInPostSaveChanges && connectionStatusAutomationsQuery.Any() )
                {
                    var connectionStatusAutomationsList = connectionStatusAutomationsQuery.AsNoTracking().OrderBy( a => a.Order ).ThenBy( a => a.AutomationName ).ToList();
                    var connectionStatusAutomations = connectionStatusAutomationsList;
                    int changedStatusCount = 0;
                    foreach ( var connectionStatusAutomation in connectionStatusAutomations )
                    {
                        if ( changedStatusCount > 0 )
                        {
                            // Updated Connection Status Automation logic to process statuses in order. If there is a match, no other automations are considered for that status.
                            break;
                        }

                        if ( this.Entity.processedConnectionStatusAutomations.Contains( connectionStatusAutomation.Id ) )
                        {
                            // to avoid recursion, skip over automations that have already been processed in this thread.
                            continue;
                        }

                        if ( Entity.ConnectionStatusId == connectionStatusAutomation.DestinationStatusId )
                        {
                            // If already have this status, no need to figure out if it needs to be set to this status,
                            // or to set the status.
                            this.Entity.processedConnectionStatusAutomations.Add( connectionStatusAutomation.Id );
                            continue;
                        }

                        bool isAutomationMatched = true;
                        if ( connectionStatusAutomation.DataViewId.HasValue )
                        {
                            // Get the dataview configured for the connection request
                            var dataViewService = new DataViewService( rockContext );
                            var dataview = dataViewService.Get( connectionStatusAutomation.DataViewId.Value );

                            if ( dataview != null )
                            {
                                var dataViewQuery = new ConnectionRequestService( rockContext ).GetQueryUsingDataView( dataview );
                                isAutomationMatched = dataViewQuery.Any( a => a.Id == Entity.Id );
                            }
                        }

                        if ( isAutomationMatched && connectionStatusAutomation.GroupRequirementsFilter != GroupRequirementsFilter.Ignore )
                        {
                            // Group Requirement can't be meet when either placement group or placement group role id is missing
                            if ( !Entity.AssignedGroupId.HasValue || !Entity.AssignedGroupMemberRoleId.HasValue )
                            {
                                isAutomationMatched = false;
                            }
                            else
                            {
                                var isRequirementMeet = true;
                                var group = new GroupService( rockContext ).Get( Entity.AssignedGroupId.Value );
                                var hasGroupRequirement = new GroupRequirementService( rockContext ).Queryable().Where( a => ( a.GroupId.HasValue && a.GroupId == group.Id ) || ( a.GroupTypeId.HasValue && a.GroupTypeId == group.GroupTypeId ) ).Any();
                                if ( hasGroupRequirement )
                                {
                                    var requirementsResults = group.PersonMeetsGroupRequirements(
                                        rockContext,
                                        Entity.PersonAlias.PersonId,
                                        Entity.AssignedGroupMemberRoleId.Value );

                                    if ( requirementsResults != null && requirementsResults
                                        .Where( a => a.MeetsGroupRequirement != MeetsGroupRequirement.NotApplicable )
                                        .Any( r =>
                                            r.MeetsGroupRequirement != MeetsGroupRequirement.Meets && r.MeetsGroupRequirement != MeetsGroupRequirement.MeetsWithWarning )
                                        )
                                    {
                                        isRequirementMeet = false;
                                    }
                                }

                                // connection request based on if group requirement is meet or not is added to list for status update
                                isAutomationMatched = ( connectionStatusAutomation.GroupRequirementsFilter == GroupRequirementsFilter.DoesNotMeet && !isRequirementMeet ) ||
                                    ( connectionStatusAutomation.GroupRequirementsFilter == GroupRequirementsFilter.MustMeet && isRequirementMeet );
                            }
                        }

                        if ( isAutomationMatched )
                        {
                            if ( Entity.SetConnectionStatusFromAutomationLoop( connectionStatusAutomation ) )
                            {
                                changedStatusCount++;
                                rockContext.SaveChanges();
                            }
                        }
                    }
                }

                var hasHistoryChanges = HistoryChangeList?.Any() == true;
                var hasPersonHistoryChanges = PersonHistoryChangeList?.Any() == true;
                if ( hasHistoryChanges || hasPersonHistoryChanges )
                {
                    using ( var historyRockContext = new RockContext() )
                    {
                        if ( hasHistoryChanges )
                        {
                            HistoryService.SaveChanges( historyRockContext, typeof( ConnectionRequest ), Rock.SystemGuid.Category.HISTORY_CONNECTION_REQUEST.AsGuid(), Entity.Id, HistoryChangeList, false, Entity.ModifiedByPersonAliasId );
                        }

                        if ( hasPersonHistoryChanges )
                        {
                            var personId = Entity.PersonAlias?.PersonId ?? new PersonAliasService( rockContext ).GetPersonId( Entity.PersonAliasId );
                            if ( personId.HasValue )
                            {
                                HistoryService.SaveChanges(
                                            historyRockContext,
                                            typeof( Person ),
                                            Rock.SystemGuid.Category.HISTORY_PERSON_CONNECTION_REQUEST.AsGuid(),
                                            personId.Value,
                                            PersonHistoryChangeList,
                                            "Request",
                                            typeof( ConnectionRequest ),
                                            Entity.Id,
                                            false,
                                            Entity.ModifiedByPersonAliasId,
                                            rockContext.SourceOfChange );
                            }
                        }

                        historyRockContext.SaveChanges( false );
                    }
                }

                base.PostSave();
            }

            private ProcessConnectionRequestChange.Message GetProcessConnectionRequestChangeMessage( IEntitySaveEntry entry, ConnectionRequest connectionRequest, int? currentPersonAliasId )
            {
                var message = new ProcessConnectionRequestChange.Message();
                if ( connectionRequest != null )
                {
                    message.State = entry.State;

                    // If the current person alias has a value, set that value for the message.
                    if ( currentPersonAliasId.HasValue )
                    {
                        message.InitiatorPersonAliasId = currentPersonAliasId;
                    }

                    // If this isn't a deleted connection request, get the connection request guid
                    if ( message.State != EntityContextState.Deleted )
                    {
                        message.ConnectionRequestGuid = connectionRequest.Guid;

                        if ( connectionRequest.PersonAlias != null )
                        {
                            message.PersonId = connectionRequest.PersonAlias.PersonId;
                        }
                        else if ( connectionRequest.PersonAliasId != default )
                        {
                            message.PersonId = new PersonAliasService( new RockContext() ).GetPersonId( connectionRequest.PersonAliasId );
                        }

                        if ( connectionRequest.ConnectionOpportunity != null )
                        {
                            message.ConnectionTypeId = connectionRequest.ConnectionOpportunity.ConnectionTypeId;
                        }

                        message.ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId;
                        message.ConnectorPersonAliasId = connectionRequest.ConnectorPersonAliasId;
                        message.ConnectionState = connectionRequest.ConnectionState;
                        message.ConnectionStatusId = connectionRequest.ConnectionStatusId;
                        message.AssignedGroupId = connectionRequest.AssignedGroupId;

                        if ( message.State == EntityContextState.Modified )
                        {
                            message.PreviousConnectionOpportunityId = entry.OriginalValues[nameof( ConnectionRequest.ConnectionOpportunityId )].ToStringSafe().AsIntegerOrNull();
                            message.PreviousConnectorPersonAliasId = entry.OriginalValues[nameof( ConnectionRequest.ConnectorPersonAliasId )].ToStringSafe().AsIntegerOrNull();
                            message.PreviousConnectionState = entry.OriginalValues[nameof( ConnectionRequest.ConnectionState )].ToStringSafe().ConvertToEnum<ConnectionState>();
                            message.PreviousConnectionStatusId = entry.OriginalValues[nameof( ConnectionRequest.ConnectionStatusId )].ToStringSafe().AsInteger();
                            message.PreviousAssignedGroupId = entry.OriginalValues[nameof( ConnectionRequest.AssignedGroupId )].ToStringSafe().AsIntegerOrNull();
                        }
                    }
                }

                return message;
            }
        }
    }
}
