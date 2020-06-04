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
using System.Linq;
using Quartz;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Utility;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to create connection request based on campaign connection configuration and auto assign the request if configured. 
    /// </summary>
    [DisallowConcurrentExecution]
    public class CampaignManager : IJob
    {
        #region Constructor

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public CampaignManager()
        {
        }

        #endregion Constructor

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var allCampaignItems = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION ).FromJsonOrNull<List<CampaignItem>>() ?? new List<CampaignItem>();
            if ( !allCampaignItems.Any() )
            {
                context.UpdateLastStatusMessage( $@"No Campaign Connection Configuration found." );
                return;
            }

            List<Exception> exceptions = new List<Exception>();

            int createConnectionRequestsResultCount = 0;

            foreach ( var campaignItem in allCampaignItems.Where( a => a.IsActive ) )
            {
                try
                {
                    ProcessCampaignConfigurationEntitySet( context, campaignItem );
                    createConnectionRequestsResultCount += CreateConnectionRequests( context, campaignItem );

                    if ( createConnectionRequestsResultCount > 0 )
                    {
                        // If records were created, update the EntitySet
                        ProcessCampaignConfigurationEntitySet( context, campaignItem );
                    }
                }
                catch ( Exception ex )
                {
                    exceptions.Add( new Exception( $"Error occurred when processing {campaignItem.Name} : {ex.Message}", ex ) );
                }
            }

            if ( createConnectionRequestExceptions.Any() )
            {
                exceptions.AddRange( createConnectionRequestExceptions );
            }

            if ( exceptions.Any() )
            {
                string exceptionMessage = $"{exceptions.Count} errors occurred";
                if ( createConnectionRequestsResultCount > 0 )
                {
                    exceptionMessage = $"{createConnectionRequestsResultCount} connections created successfully, but {createConnectionRequestExceptions.Count} errors occurred";
                }

                if ( exceptions.Count == 0 )
                {
                    throw new Exception( exceptionMessage, exceptions[0] );
                }
                else
                {
                    throw new AggregateException( exceptionMessage, exceptions );
                }
            }

            context.UpdateLastStatusMessage( $"{createConnectionRequestsResultCount} connection requests were created. " );
        }

        #region Process Campaign Configuration Entity Set

        /// <summary>
        /// Processes the campaign configuration entity set.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="campaignItem">The campaign item.</param>
        private void ProcessCampaignConfigurationEntitySet( IJobExecutionContext context, CampaignItem campaignItem )
        {
            context.UpdateLastStatusMessage( $"Processing entity set for {campaignItem.Name}." );

            campaignItem.EntitySetId = CampaignConnectionHelper.GetEntitySet( campaignItem );
            CampaignConnectionHelper.AddOrUpdateCampaignConfiguration( campaignItem.Guid, campaignItem );
        }

        #endregion

        #region Create Connection Requests

        private List<Exception> createConnectionRequestExceptions = new List<Exception>();

        /// <summary>
        /// Creates the connection requests.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="campaignItem">The campaign item.</param>
        /// <returns></returns>
        private int CreateConnectionRequests( IJobExecutionContext context, CampaignItem campaignItem )
        {
            context.UpdateLastStatusMessage( $"Processing create connection requests for {campaignItem.Name}" );

            // Skip creating connection requests if set to "AsNeeded" and DailyLimitAssigned is 0 or null
            if ( campaignItem.CreateConnectionRequestOption == CreateConnectionRequestOptions.AsNeeded &&
                ( campaignItem.DailyLimitAssigned <= 0 || campaignItem.DailyLimitAssigned == null ) )
            {
                return 0;
            }

            int recordsProcessed = 0;

            var rockContext = new RockContext();
            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );
            var connectionRequestService = new ConnectionRequestService( rockContext );

            var connectionOpportunity = connectionOpportunityService.Get( campaignItem.OpportunityGuid );
            if ( connectionOpportunity == null )
            {
                return 0;
            }

            // get cutoff for the last connection datetime.
            var lastConnectionDateTime = RockDateTime.Now.AddDays( -campaignItem.DaysBetweenConnection );

            // if DaysBetweenConnection is 0 then check for connection request for any time period.
            if ( campaignItem.DaysBetweenConnection == default( int ) )
            {
                lastConnectionDateTime = DateTime.MinValue;
            }

            var entitySetItemService = new EntitySetItemService( rockContext );

            var entitySetItemsQry = entitySetItemService
                                .Queryable()
                                .Where( a => a.EntitySetId == campaignItem.EntitySetId )
                                .OrderBy( a => a.Order );

            bool autoAssignment = false;
            var eligibleConnectors = new List<ConnectionConnector>();
            if ( campaignItem.CreateConnectionRequestOption == CreateConnectionRequestOptions.AsNeeded &&
                campaignItem.DailyLimitAssigned.HasValue &&
                campaignItem.DailyLimitAssigned != default( int ) )
            {
                autoAssignment = true;
                eligibleConnectors = GetEligibleConnectorWithLimit( connectionOpportunity.Id, rockContext, campaignItem.DailyLimitAssigned.Value );
            }

            int? connectionStatusId = connectionOpportunity.ConnectionType.ConnectionStatuses
                    .Where( s => s.IsDefault )
                    .Select( s => ( int? ) s.Id )
                    .FirstOrDefault();

            // If opportunity doesn't have a default status, something is wrong
            if ( connectionStatusId == null )
            {
                ExceptionLogService.LogException( new Exception( $"Unable to determine default connection status for {connectionOpportunity.Name} while processing campaigns", null ) );
                return 0;
            }

            var dayofWeek = RockDateTime.Now.DayOfWeek;
            var entitySetItemList = entitySetItemsQry.ToList();
            foreach ( var entitySetItem in entitySetItemList )
            {
                var personAlias = personAliasService.GetPrimaryAlias( entitySetItem.EntityId );
                var defaultCampus = personAlias.Person.GetCampus();
                int? connectorPersonAliasId = null;
                if ( autoAssignment )
                {
                    int? connectorPersonId = null;
                    if ( campaignItem.PreferPreviousConnector )
                    {
                        var personIds = eligibleConnectors
                            .Where( a => a.Limit - a.Current > 0 &&
                                        ( !a.DaysOfWeek.Any() || a.DaysOfWeek.Contains( dayofWeek ) ) &&
                                        ( !a.CampusId.HasValue || ( a.CampusId.HasValue && defaultCampus != null && defaultCampus.Id == a.CampusId.Value ) ) )
                                        .Select( a => a.PersonId )
                                        .ToList();

                        if ( personIds.Any() )
                        {
                            var person = connectionRequestService
                                .Queryable()
                                .Where( a => a.ConnectionOpportunityId == connectionOpportunity.Id &&
                                    a.PersonAlias.PersonId == personAlias.PersonId &&
                                    a.ConnectionState == ConnectionState.Connected &&
                                    a.ConnectorPersonAliasId.HasValue &&
                                    personIds.Contains( a.ConnectorPersonAlias.PersonId ) )
                                    .Select( a => a.ConnectorPersonAlias.Person )
                                    .FirstOrDefault();

                            if ( person != null )
                            {
                                connectorPersonId = person.Id;
                            }
                        }
                    }

                    if ( !connectorPersonId.HasValue )
                    {
                        var eligibleConnector = eligibleConnectors
                            .Where( a => a.Limit - a.Current > 0 &&
                            ( !a.DaysOfWeek.Any() || a.DaysOfWeek.Contains( dayofWeek ) ) &&
                            ( !a.CampusId.HasValue || ( a.CampusId.HasValue && defaultCampus != null && defaultCampus.Id == a.CampusId.Value ) ) )
                            .OrderBy( a => a.Current )      // order from least assigned to most assigned
                            .ThenBy( x => Guid.NewGuid() )  // and then randomize
                            .FirstOrDefault();

                        if ( eligibleConnector != null )
                        {
                            connectorPersonId = eligibleConnector.PersonId;
                        }
                    }

                    if ( !connectorPersonId.HasValue )
                    {
                        continue;
                    }

                    foreach ( var connectionConnector in eligibleConnectors.Where( a => a.PersonId == connectorPersonId.Value ) )
                    {
                        connectorPersonAliasId = connectionConnector.PersonAliasId;
                        connectionConnector.Current += 1;
                    }
                }

                using ( var insertRockContext = new RockContext() )
                {
                    try
                    {
                        /*
                            3/30/2020 - NA 

                            When setting the connection request's Requester, we have to use the PrimaryAlias
                            to set the connectionRequest.PersonAlias property because the ConnectionRequestChangeTransaction
                            https://github.com/SparkabilityGroup/Rock/blob/a556a9285b7fdfe5594441286242f4feaa5847f2/Rock/Transactions/ConnectionRequestChangeTransaction.cs#L123
                            (which handles triggered workflows) expects it.  Also, it needs to be tracked by
                            the current rockContext...

                            In other words, this will not work correctly:
                            PersonAliasId = personAlias.Id,

                            Reason: This plug-in cannot change Rock core assembly code.
                        */

                        var personPrimaryAlias = new PersonAliasService( insertRockContext ).GetPrimaryAlias( entitySetItem.EntityId );
                        var connectionRequestActivityService = new ConnectionRequestActivityService( insertRockContext );
                        var insertConnectionRequestService = new ConnectionRequestService( insertRockContext );

                        // double check that they haven't already been added
                        var personAlreadyHasConnectionRequest = CampaignConnectionHelper.PersonAlreadyHasConnectionRequest( connectionOpportunity.Id, rockContext, lastConnectionDateTime, entitySetItem.EntityId );

                        if ( personAlreadyHasConnectionRequest )
                        {
                            continue;
                        }

                        var connectionRequest = new ConnectionRequest()
                        {
                            ConnectionOpportunityId = connectionOpportunity.Id,
                            PersonAlias = personPrimaryAlias,
                            ConnectionState = ConnectionState.Active,
                            ConnectorPersonAliasId = connectorPersonAliasId,
                            CampusId = defaultCampus?.Id,
                            ConnectionStatusId = connectionStatusId.Value,
                        };

                        if ( campaignItem.RequestCommentsLavaTemplate.IsNotNullOrWhiteSpace() )
                        {
                            var mergeFields = new Dictionary<string, object>();
                            mergeFields.Add( "Person", personAlias.Person );
                            mergeFields.Add( "Family", personAlias.Person.GetFamily() );
                            connectionRequest.Comments = campaignItem.RequestCommentsLavaTemplate.ResolveMergeFields( mergeFields );
                        }

                        insertConnectionRequestService.Add( connectionRequest );

                        if ( connectorPersonAliasId.HasValue )
                        {
                            var connectionActivityTypeAssignedGuid = Rock.SystemGuid.ConnectionActivityType.ASSIGNED.AsGuid();
                            int? assignedActivityId = new ConnectionActivityTypeService( rockContext ).GetId( connectionActivityTypeAssignedGuid );
                            if ( assignedActivityId.HasValue )
                            {
                                var connectionRequestActivity = new ConnectionRequestActivity();
                                connectionRequestActivity.ConnectionRequest = connectionRequest;
                                connectionRequestActivity.ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId;
                                connectionRequestActivity.ConnectionActivityTypeId = assignedActivityId.Value;
                                connectionRequestActivity.ConnectorPersonAliasId = connectorPersonAliasId;
                                connectionRequestActivityService.Add( connectionRequestActivity );
                            }
                        }

                        insertRockContext.SaveChanges();
                    }
                    catch ( Exception ex )
                    {
                        // Log exception and keep on trucking.
                        var exception = new Exception( $"Exception occurred trying to create connection request:{personAlias.Id}.", ex );
                        createConnectionRequestExceptions.Add( exception );
                        ExceptionLogService.LogException( exception, null );
                    }
                }

                recordsProcessed += 1;
            }

            return recordsProcessed;
        }

        /// <summary>
        /// Gets the eligible connector with limit considering how many non-closed requests the connector may already have.
        /// </summary>
        /// <param name="connectionOpportunityId">The connection opportunity identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="globalLimit">The global limit.</param>
        /// <returns></returns>
        private List<ConnectionConnector> GetEligibleConnectorWithLimit( int connectionOpportunityId, RockContext rockContext, int globalLimit )
        {
            var eligibleConnectors = new List<ConnectionConnector>();
            var qryConnectionOpportunityConnectorGroups = new ConnectionOpportunityConnectorGroupService( rockContext ).Queryable()
                        .Where( a => a.ConnectionOpportunityId == connectionOpportunityId
                                    && a.ConnectorGroup.IsActive
                                    && !a.ConnectorGroup.IsArchived );

            // Get all connection requests that are active for the given opportunity that have a connector
            var activeConnectionRequestsWithConnector = new ConnectionRequestService( rockContext )
                        .Queryable()
                        .Where( a => a.ConnectionOpportunityId == connectionOpportunityId &&
                                    a.ConnectorPersonAlias != null &&
                                    a.ConnectionState == ConnectionState.Active );

            foreach ( var opportunityConnectionGroup in qryConnectionOpportunityConnectorGroups )
            {
                var members = opportunityConnectionGroup.ConnectorGroup.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).ToList();
                members.LoadAttributes();
                foreach ( var member in members )
                {
                    ConnectionConnector connectionConnector = new ConnectionConnector()
                    {
                        PersonAliasId = member.Person.PrimaryAliasId,
                        PersonId = member.PersonId,
                        CampusId = opportunityConnectionGroup.CampusId,
                    };
                    eligibleConnectors.Add( connectionConnector );

                    var campaignDailyLimit = member.GetAttributeValue( "CampaignDailyLimit" ).AsIntegerOrNull();
                    connectionConnector.Limit = campaignDailyLimit.HasValue ? campaignDailyLimit.Value : globalLimit;
                    connectionConnector.Current = activeConnectionRequestsWithConnector.Where( a => a.ConnectorPersonAlias.PersonId == member.PersonId ).Count();

                    var campaignScheduleDays = member.GetAttributeValue( "CampaignScheduleDays" );
                    connectionConnector.DaysOfWeek = new List<DayOfWeek>();
                    if ( !string.IsNullOrWhiteSpace( campaignScheduleDays ) )
                    {
                        connectionConnector.DaysOfWeek = campaignScheduleDays.Split( ',' ).Select( a => ( DayOfWeek ) a.AsInteger() ).ToList();
                    }
                }
            }

            return eligibleConnectors;
        }

        #endregion

        #region Helper Classes

        private class ConnectionConnector
        {
            public int? PersonAliasId { get; set; }

            public int PersonId { get; set; }

            public int Limit { get; set; }

            public int Current { get; set; }

            public int? CampusId { get; set; }

            public List<DayOfWeek> DaysOfWeek { get; set; }
        }

        #endregion
    }
}