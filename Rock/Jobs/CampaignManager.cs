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
using System.Data.Entity;
using System.Linq;
using System.Web;
using Rock.Utility;
using Quartz;
using Rock;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to create connection request based on campaign connection configuration and auto assign the request if configured. 
    /// </summary>
    [DisallowConcurrentExecution]
    public class CampaignManager : IJob
    {
        private HttpContext _httpContext = null;

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
        /// <exception cref="System.NotImplementedException"></exception>
        public void Execute( IJobExecutionContext context )
        {
            _httpContext = HttpContext.Current;

            var allCampaignItems = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION ).FromJsonOrNull<List<CampaignItem>>() ?? new List<CampaignItem>();
            if ( !allCampaignItems.Any() )
            {
                context.UpdateLastStatusMessage( $@"No Campaign Connection Configuration found." );
                return;
            }

            string processCampaignConfigurationEntitySetResult = ProcessCampaignConfigurationEntitySet( context, allCampaignItems );

            int createConnectionRequestsResultCount = CreateConnectionRequests( context, allCampaignItems.Where( a=>a.IsActive).ToList() );

            // If records were created, update the EntitySet
            if ( createConnectionRequestsResultCount > 0 )
            {
                ProcessCampaignConfigurationEntitySet( context, allCampaignItems );
            }

            context.UpdateLastStatusMessage( $@"Process Campaign Configuration EntitySet: {processCampaignConfigurationEntitySetResult} Create Connection Requests: {createConnectionRequestsResultCount:N0} connection requests were created. " );
        }

        #region Process Campaign Configuration Entity Set

        /// <summary>
        /// process the campaign connection configuration entity set
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="allCampaignConfigurations">All of the Campaign Configuration items (active and inactive).</param>
        /// <returns></returns>
        private string ProcessCampaignConfigurationEntitySet( IJobExecutionContext context, List<CampaignItem> allCampaignConfigurations )
        {
            try
            {
                context.UpdateLastStatusMessage( $"Processing campaign configuration entity set." );

                int recordsProcessed = 0;

                foreach ( var campaignConnectionConfiguration in allCampaignConfigurations.Where( a => a.IsActive ) )
                {
                    campaignConnectionConfiguration.EntitySetId = CampaignConnectionHelper.GetEntitySet( campaignConnectionConfiguration );
                    CampaignConnectionHelper.AddOrUpdateCampaignConfiguration( campaignConnectionConfiguration.Guid, campaignConnectionConfiguration );
                    recordsProcessed += 1;
                }

                // Format the result message
                return $"{recordsProcessed:N0} campaign configuration entity set were processed;";
            }
            catch ( Exception ex )
            {
                // Log exception and return the exception messages.
                ExceptionLogService.LogException( ex, _httpContext );

                return ex.Messages().AsDelimited( "; " );
            }
        }

        #endregion

        #region Create Connection Requests

        /// <summary>
        /// create the connection request from entity set
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="activeCampaignConfigurations">The active Campaign Configuration items.</param>
        /// <returns>count of how many connection requests were created.</returns>
        private int CreateConnectionRequests( IJobExecutionContext context, List<CampaignItem> activeCampaignConfigurations )
        {
            context.UpdateLastStatusMessage( $"Processing create connection requests." );

            int recordsProcessed = 0;

            foreach ( var campaignConnectionConfiguration in activeCampaignConfigurations )
            {
                // Skip creating connection requests if set to "AsNeeded" and DailyLimitAssigned is 0 or null
                if ( campaignConnectionConfiguration.CreateConnectionRequestOption == CreateConnectionRequestOptions.AsNeeded &&
                    ( campaignConnectionConfiguration.DailyLimitAssigned <= 0 || campaignConnectionConfiguration.DailyLimitAssigned == null ) )
                {
                    continue;
                }

                // Try to process as many campaigns as possible, log any exceptions and move to next one when possible.
                try
                {
                    var rockContext = new RockContext();
                    var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
                    var personAliasService = new PersonAliasService( rockContext );
                    var connectionRequestService = new ConnectionRequestService( rockContext );

                    var connectionOpportunity = connectionOpportunityService.Get( campaignConnectionConfiguration.OpportunityGuid );
                    if ( connectionOpportunity == null )
                    {
                        continue;
                    }

                    var entitySetItemsQry = new EntitySetItemService( rockContext )
                                        .Queryable()
                                        .Where( a => a.EntitySetId == campaignConnectionConfiguration.EntitySetId )
                                        .OrderBy( a => a.Order );

                    bool autoAssignment = false;
                    var eligibleConnectors = new List<ConnectionConnector>();
                    if ( campaignConnectionConfiguration.CreateConnectionRequestOption == CreateConnectionRequestOptions.AsNeeded &&
                        campaignConnectionConfiguration.DailyLimitAssigned.HasValue &&
                        campaignConnectionConfiguration.DailyLimitAssigned != default( int ) )
                    {
                        autoAssignment = true;
                        eligibleConnectors = GetEligibleConnectorWithLimit( connectionOpportunity.Id, rockContext, campaignConnectionConfiguration.DailyLimitAssigned.Value );
                    }

                    int? connectionStatusId = connectionOpportunity.ConnectionType.ConnectionStatuses
                            .Where( s => s.IsDefault )
                            .Select( s => ( int? ) s.Id )
                            .FirstOrDefault();

                    // If opportunity doesn't have a default status, something is wrong
                    if ( connectionStatusId == null )
                    {
                        ExceptionLogService.LogException( new Exception( $"Unable to determine default connection status for {connectionOpportunity.Name} while processing Campaigns", null ) );
                        continue;
                    }

                    var dayofWeek = RockDateTime.Now.DayOfWeek;
                    foreach ( var entitySetItem in entitySetItemsQry )
                    {
                        var personAlias = personAliasService.GetPrimaryAlias( entitySetItem.EntityId );
                        var defaultCampus = personAlias.Person.GetCampus();
                        int? connectorPersonAliasId = null;
                        if ( autoAssignment )
                        {
                            int? connectorPersonId = null;
                            if ( campaignConnectionConfiguration.PreferPreviousConnector )
                            {
                                var personIds = eligibleConnectors
                                    .Where( a => a.Limit - a.Current > 0 &&
                                                (!a.DaysOfWeek.Any() || a.DaysOfWeek.Contains( dayofWeek ) ) &&
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
                                var connectionRequest = new ConnectionRequest()
                                {
                                    ConnectionOpportunityId = connectionOpportunity.Id,
                                    PersonAlias = personPrimaryAlias,
                                    ConnectionState = ConnectionState.Active,
                                    ConnectorPersonAliasId = connectorPersonAliasId,
                                    CampusId = defaultCampus?.Id,
                                    ConnectionStatusId = connectionStatusId.Value,
                                };

                                if ( campaignConnectionConfiguration.RequestCommentsLavaTemplate.IsNotNullOrWhiteSpace() )
                                {
                                    var mergeFields = new Dictionary<string, object>();
                                    mergeFields.Add( "Person", personAlias.Person );
                                    mergeFields.Add( "Family", personAlias.Person.GetFamily() );
                                    connectionRequest.Comments = campaignConnectionConfiguration.RequestCommentsLavaTemplate.ResolveMergeFields( mergeFields );
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
                                ExceptionLogService.LogException( new Exception( $"Exception occurred trying to create connection request:{personAlias.Id}.", ex ), _httpContext );
                            }

                        }

                        recordsProcessed += 1;
                    }
                }
                catch ( Exception ex )
                {
                    // Log exception and continue until there are no more
                    ExceptionLogService.LogException( ex, _httpContext );
                }
            }

            // return the number of records created
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
                        .Where( a => a.ConnectionOpportunityId == connectionOpportunityId );

            // Get all connection requests that are active for the given opportunity that have a connector
            var activeConnectionRequestsWithConnector = new ConnectionRequestService( rockContext )
                        .Queryable()
                        .Where( a => a.ConnectionOpportunityId == connectionOpportunityId &&
                                    a.ConnectorPersonAlias != null &&
                                    a.ConnectionState == ConnectionState.Active
                                    );

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
                        connectionConnector.DaysOfWeek = campaignScheduleDays.Split( ',' ).Select( a => ( DayOfWeek ) ( a.AsInteger() ) ).ToList();
                    }
                }
            }
            return eligibleConnectors;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// 
        /// </summary>
        public class ConnectionConnector
        {
            /// <summary>
            /// Gets or sets the person alias identifier.
            /// </summary>
            /// <value>
            /// The person alias identifier.
            /// </value>
            public int? PersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the limit.
            /// </summary>
            /// <value>
            /// The limit.
            /// </value>
            public int Limit { get; set; }

            /// <summary>
            /// Gets or sets the current.
            /// </summary>
            /// <value>
            /// The current.
            /// </value>
            public int Current { get; set; }

            /// <summary>
            /// Gets or sets the campus identifier.
            /// </summary>
            /// <value>
            /// The campus identifier.
            /// </value>
            public int? CampusId { get; set; }

            /// <summary>
            /// Gets or sets the days of week.
            /// </summary>
            /// <value>
            /// The days of week.
            /// </value>
            public List<DayOfWeek> DaysOfWeek { get; set; }
        }

        #endregion

    }
}