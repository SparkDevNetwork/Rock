﻿// <copyright>
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

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Web.Cache;

namespace Rock.Utility
{
    /// <summary>
    /// Campaign Connection Helper
    /// </summary>
    public class CampaignConnectionHelper
    {
        /// <summary>
        /// creates or updates the entity set on the basis of campaign connection configuration, and returns the Id of the entitySetId
        /// </summary>
        /// <param name="campaignConfiguration">The campaign configuration.</param>
        /// <returns></returns>
        public static int GetEntitySet( CampaignItem campaignConfiguration )
        {
            var rockContext = new RockContext();

            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
            var connectionRequestService = new ConnectionRequestService( rockContext );
            var entitySetService = new Rock.Model.EntitySetService( rockContext );

            var connectionOpportunity = connectionOpportunityService.Get( campaignConfiguration.OpportunityGuid );

            // list of person on the basis of Dataview result and optout group.
            var filteredPersonIds = GetFilteredPersonIds( campaignConfiguration, rockContext );

            // get the last connection datetime.
            var lastConnectionDateTime = RockDateTime.Now.AddDays( -campaignConfiguration.DaysBetweenConnection );

            // if DaysBetweenConnection is 0 then check for connection request for any time period.
            if ( campaignConfiguration.DaysBetweenConnection == default( int ) )
            {
                lastConnectionDateTime = DateTime.MinValue;
            }

            // list of person that has active connection request OR has connection closed in the past number of days between connection.  
            var excludedPersonIds = connectionRequestService
                .Queryable()
                .Where( a =>
                        a.ConnectionOpportunityId == connectionOpportunity.Id && (
                        a.ConnectionState == ConnectionState.Active
                        || a.ConnectionState == ConnectionState.FutureFollowUp
                        || ( ( a.ConnectionState == ConnectionState.Connected || a.ConnectionState == ConnectionState.Inactive ) && a.ModifiedDateTime > lastConnectionDateTime ) ) )
                .Select( a => a.PersonAlias.PersonId )
                .ToList();

            // filtered list of person removing all the personIds found in excludedPersonIds List
            filteredPersonIds = filteredPersonIds.Where( a => !excludedPersonIds.Contains( a ) ).ToList();

            // get the ordered list of personIds based on the oldest previous connection request and connection opportunity
            /* 2020-05-06 MDP
             * If there are many filteredPersonIds, we'll get a SQL Exception, so let's get *all* the Connected connection Requests first,
             * and then use C# to filter.
             */

            var orderedLastCompletedRequestForPerson = connectionRequestService
                .Queryable()
                .Where( a => a.ConnectionOpportunityId == connectionOpportunity.Id
                            && a.ConnectionState == ConnectionState.Connected )
                .GroupBy( a => a.PersonAlias.PersonId )
                .Select( a => new
                {
                    PersonId = a.Key,
                    LastConnectionDateTime = a.OrderByDescending( b => b.ModifiedDateTime ).Select( b => b.ModifiedDateTime ).FirstOrDefault()
                } )
                .OrderBy( a => a.LastConnectionDateTime )
                .Select( a => a.PersonId ).ToList();

            // Use C# to filter persons so we can avoid a SQL Exception 
            orderedLastCompletedRequestForPerson = orderedLastCompletedRequestForPerson.Where( a => filteredPersonIds.Contains( a ) ).ToList();

            var random = new Random();

            //// get the final ordered list of personIds based on the oldest previous connection request and
            //// connection opportunity otherwise order randomly for the person who don't have any previous connection request.
            var orderedPersonIds = filteredPersonIds
                .OrderBy( a =>
                {
                    var index = orderedLastCompletedRequestForPerson.IndexOf( a );
                    if ( index == -1 )
                    {
                        return random.Next( orderedLastCompletedRequestForPerson.Count, int.MaxValue );
                    }
                    else
                    {
                        return index;
                    }
                } ).ToList();

            EntitySet entitySet = null;
            if ( campaignConfiguration.EntitySetId != default( int ) )
            {
                entitySet = entitySetService.Get( campaignConfiguration.EntitySetId );
            }

            List<Rock.Model.EntitySetItem> entitySetItems = new List<Rock.Model.EntitySetItem>();
            var personEntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
            if ( entitySet == null || entitySet.EntityTypeId != personEntityTypeId )
            {
                entitySet = new Rock.Model.EntitySet();
                entitySet.EntityTypeId = personEntityTypeId;
                entitySet.ExpireDateTime = null;
                entitySetService.Add( entitySet );
            }
            else
            {
                var entitySetItemQry = new EntitySetItemService( rockContext )
                       .Queryable().AsNoTracking()
                       .Where( i => i.EntitySetId == entitySet.Id );
                rockContext.BulkDelete( entitySetItemQry );
            }

            // Update the EntitySet name
            entitySet.Name = campaignConfiguration.Name;

            var orderIndex = 0;
            foreach ( var personId in orderedPersonIds )
            {
                try
                {
                    var item = new Rock.Model.EntitySetItem();
                    item.Order = orderIndex++;
                    item.EntityId = personId;
                    entitySetItems.Add( item );
                }
                catch
                {
                    // ignore
                }
            }

            rockContext.SaveChanges();
            entitySetItems.ForEach( a =>
            {
                a.EntitySetId = entitySet.Id;
            } );

            rockContext.BulkInsert( entitySetItems );

            return entitySet.Id;
        }

        /// <summary>
        /// get the campaign connection configurations
        /// </summary>
        /// <param name="campaignGuid">The campaign identifier.</param>
        /// <returns></returns>
        public static CampaignItem GetCampaignConfiguration( Guid campaignGuid )
        {
            var campaignConnectionItems = Rock.Web.SystemSettings.GetValue( CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION ).FromJsonOrNull<List<CampaignItem>>() ?? new List<CampaignItem>();
            return campaignConnectionItems.FirstOrDefault( a => a.Guid == campaignGuid );
        }

        /// <summary>
        /// add or updates the campaign connection configuration
        /// </summary>
        /// <param name="campaignGuid">The campaign identifier.</param>
        /// <param name="campaignConfiguration">The campaign configuration.</param>
        /// <returns></returns>
        public static void AddOrUpdateCampaignConfiguration( Guid campaignGuid, CampaignItem campaignConfiguration )
        {
            var campaignConnectionItems = Rock.Web.SystemSettings.GetValue( CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION ).FromJsonOrNull<List<CampaignItem>>() ?? new List<CampaignItem>();

            var campaignConnectionItem = campaignConnectionItems.FirstOrDefault( a => a.Guid == campaignGuid );
            if ( campaignConnectionItem == null )
            {
                campaignConnectionItems.Add( campaignConfiguration );
            }
            else
            {
                // Remove the existing item and add the given one as new.
                campaignConnectionItems.Remove( campaignConnectionItem );
                campaignConnectionItems.Add( campaignConfiguration );
            }

            // Since this is saving the entire list of campaign configurations, they all must be included
            Rock.Web.SystemSettings.SetValue( Rock.SystemKey.CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION, campaignConnectionItems.ToJson() );
        }

        /// <summary>
        /// remove the campaign connection configuration
        /// </summary>
        /// <param name="campaignGuid">The campaign identifier.</param>
        /// <returns></returns>
        public static void RemoveCampaignConfiguration( Guid campaignGuid )
        {
            var campaignConnectionItems = Rock.Web.SystemSettings.GetValue( CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION ).FromJsonOrNull<List<CampaignItem>>() ?? new List<CampaignItem>();
            campaignConnectionItems.RemoveAll( a => a.Guid == campaignGuid );
            Rock.Web.SystemSettings.SetValue( CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION, campaignConnectionItems.ToJson() );
        }

        /// <summary>
        /// Gets the number of pending connection request based on existing request that don't have a connector plus the ones in the CampaignConnectionItem's EntitySet
        /// </summary>
        /// <param name="campaignConnectionItem">The campaign connection item.</param>
        /// <param name="connectorPerson">The connector person.</param>
        /// <returns></returns>
        public static int GetPendingConnectionCount( CampaignItem campaignConnectionItem, Person connectorPerson )
        {
            var rockContext = new RockContext();
            var entitySetService = new EntitySetService( rockContext );
            var entitySetId = GetEntitySet( campaignConnectionItem );

            var pendingPersonPrimaryCampusIdList = entitySetService.GetEntityQuery<Person>( entitySetId )
                .Select( a => new
                {
                    a.PrimaryCampusId
                } )
                .ToList();

            int pendingCount = 0;
            var connectorCampusIds = GetConnectorCampusIds( campaignConnectionItem, connectorPerson );
            foreach ( var pendingPerson in pendingPersonPrimaryCampusIdList )
            {
                int? entitySetPersonPrimaryCampusId = pendingPerson.PrimaryCampusId;

                if ( IsValidCampus( connectorCampusIds, entitySetPersonPrimaryCampusId ) )
                {
                    pendingCount++;
                }
            }

            pendingCount += CampaignConnectionHelper.GetConnectionRequestsWithoutConnectorQuery( rockContext, campaignConnectionItem, connectorPerson ).Count();
            return pendingCount;
        }

        /// <summary>
        /// Gets the filter person Ids based on dataview and optout group
        /// </summary>
        /// <param name="campaignConfiguration">The campaign configuration.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static List<int> GetFilteredPersonIds( CampaignItem campaignConfiguration, RockContext rockContext )
        {
            var dataView = new DataViewService( rockContext ).Get( campaignConfiguration.DataViewGuid );
            var personService = new PersonService( rockContext );
            int recordStatusInactiveId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;
            var filteredPersonIds = new List<int>();

            var dataViewGetQueryArgs = new DataViewGetQueryArgs();
            var personQuery = dataView.GetQuery( dataViewGetQueryArgs ).OfType<Rock.Model.Person>().Where( a => a.RecordStatusValueId != recordStatusInactiveId );

            if ( campaignConfiguration.FamilyLimits == FamilyLimits.HeadOfHouse )
            {
                var familyMembersQuery = personQuery
                    .Where( a => a.PrimaryFamily != null )
                    .SelectMany( a => a.PrimaryFamily.Members )
                    .Distinct();

                //// Get all family group Id and all it's family member in dictionary.
                //// We will all the family members to both figure out if might be opted out
                //// and to figure out the head of household
                var familyWithMembers = familyMembersQuery.AsNoTracking()
                    .Select( a => new
                    {
                        a.GroupId,
                        a.PersonId,
                        PersonIsActive = a.Person.RecordStatusValueId != recordStatusInactiveId,
                        PersonIsDeceased = a.Person.IsDeceased,
                        GroupRoleOrder = a.GroupRole.Order,
                        PersonGender = a.Person.Gender
                    } )
                    .ToList()
                    .GroupBy( a => a.GroupId )
                    .ToDictionary( k => k.Key, v => v );

                if ( campaignConfiguration.OptOutGroupGuid.HasValue )
                {
                    var optOutGroup = new GroupService( rockContext ).Get( campaignConfiguration.OptOutGroupGuid.Value );
                    if ( optOutGroup != null )
                    {
                        var personIds = optOutGroup.ActiveMembers().Select( a => a.PersonId ).ToList();

                        // exclude families in which any member is part of optOut Group.
                        familyWithMembers = familyWithMembers.Where( a => !a.Value.Any( b => personIds.Contains( b.PersonId ) ) ).ToDictionary( a => a.Key, b => b.Value );
                    }
                }

                foreach ( var familyId in familyWithMembers.Keys )
                {
                    /* 2020-05-07 MDP
                     * It is possible that a person is the Head Of Household in more than one family. For example: 
                     * -- Alex is in Ted Decker family. Ted Decker is Head of Household
                     * -- Ted Decker is in Grandpa Decker family, and also the head of household for that one too

                     * We'll deal with that by putting a Distinct on the filteredPersonIds
                     */

                    // Get all the head of house personIds of leftout family.
                    var headOfHouse = familyWithMembers[familyId]
                          .Where( m => !m.PersonIsDeceased && m.PersonIsActive )
                          .OrderBy( m => m.GroupRoleOrder )
                          .ThenBy( m => m.PersonGender )
                          .Select( a => a.PersonId )
                          .FirstOrDefault();

                    if ( headOfHouse != default( int ) )
                    {
                        filteredPersonIds.Add( headOfHouse );
                    }
                }
            }
            else
            {
                var personIdList = personQuery.Select( a => a.Id ).ToList();
                if ( campaignConfiguration.OptOutGroupGuid.HasValue )
                {
                    var optOutGroup = new GroupService( rockContext ).Get( campaignConfiguration.OptOutGroupGuid.Value );
                    if ( optOutGroup != null )
                    {
                        var personIds = optOutGroup.ActiveMembers().Select( a => a.PersonId ).ToList();
                        personIdList = personIdList.Where( a => !personIds.Contains( a ) ).ToList();
                    }
                }

                filteredPersonIds = personIdList;
            }

            // just in case the same person is in multiple times (for example, head of household in multiple families), get just the distinct person ids
            filteredPersonIds = filteredPersonIds.Distinct().ToList();

            return filteredPersonIds;
        }

        /// <summary>
        /// A lock object that should be used when assigning Connection Requests from the entity set, to help prevent duplicates.
        /// </summary>
        private static object addConnectionRequestsLockObject = new object();

        /// <summary>
        /// Adds connection request(s) for the specified connectorPerson
        /// </summary>
        /// <param name="selectedCampaignItem">The selected campaign item.</param>
        /// <param name="connectorPerson">The connector person.</param>
        /// <param name="numberOfRequests">The number of requests.</param>
        /// <param name="numberOfRequestsRemaining">The number of requests remaining.</param>
        public static void AddConnectionRequestsForPerson( CampaignItem selectedCampaignItem, Person connectorPerson, int numberOfRequests, out int numberOfRequestsRemaining )
        {
            var rockContext = new RockContext();
            numberOfRequestsRemaining = numberOfRequests;

            /* To assign requests, do the following in this order
            - a) Get current connectionRequest records that don't have a connector and have a connection state of Active or a pastdue FutureFollowUp.
            - b) If that runs out, get persons from the EntitySet of the selectedCampaignConnectionItem
            - If there aren't enough from 'a' or 'b'
             */

            IQueryable<ConnectionRequest> connectionRequestsWithoutConnectorQuery = GetConnectionRequestsWithoutConnectorQuery( rockContext, selectedCampaignItem, connectorPerson );

            numberOfRequestsRemaining = numberOfRequests;
            var connectionRequestsWithoutConnectorList = connectionRequestsWithoutConnectorQuery
                .OrderBy( a => a.CreatedDateTime )
                .ToList();

            int connectorPersonAliasId = connectorPerson.PrimaryAliasId.Value;

            var connectionActivityTypeAssignedGuid = Rock.SystemGuid.ConnectionActivityType.ASSIGNED.AsGuid();
            int? assignedActivityId = new ConnectionActivityTypeService( rockContext ).GetId( connectionActivityTypeAssignedGuid );
            var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );

            foreach ( var connectionRequest in connectionRequestsWithoutConnectorList )
            {
                connectionRequest.ConnectorPersonAliasId = connectorPersonAliasId;
                if ( selectedCampaignItem.RequestCommentsLavaTemplate.IsNotNullOrWhiteSpace() && connectionRequest.Comments.IsNullOrWhiteSpace() )
                {
                    var mergeFields = new Dictionary<string, object>();
                    mergeFields.Add( "Person", connectionRequest.PersonAlias.Person );
                    mergeFields.Add( "Family", connectionRequest.PersonAlias.Person.GetFamily() );
                    connectionRequest.Comments = selectedCampaignItem.RequestCommentsLavaTemplate.ResolveMergeFields( mergeFields );
                }

                /*
                    4/1/2020 - NA 

                    You cannot attached the connectorPerson.PrimaryAlias to the connectionRequest.ConnectorPersonAlias
                    because they are tracked by two different contexts.  Therefore this will not work:
                    connectionRequest.ConnectorPersonAlias = connectorPerson.PrimaryAlias;
                */

                if ( assignedActivityId.HasValue )
                {
                    var connectionRequestActivity = new ConnectionRequestActivity();
                    connectionRequestActivity.ConnectionRequestId = connectionRequest.Id;
                    connectionRequestActivity.ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId;
                    connectionRequestActivity.ConnectionActivityTypeId = assignedActivityId.Value;
                    connectionRequestActivity.ConnectorPersonAliasId = connectorPersonAliasId;
                    connectionRequestActivityService.Add( connectionRequestActivity );
                }

                numberOfRequestsRemaining--;
                if ( numberOfRequestsRemaining <= 0 )
                {
                    break;
                }
            }

            rockContext.SaveChanges();

            if ( numberOfRequestsRemaining == 0 )
            {
                // we were able to assign enough from connectionRequestsWithoutConnectorList, so save those changes and return
                return;
            }

            lock ( addConnectionRequestsLockObject )
            {
                AssignConnectionRequestsFromEntitySet( rockContext, selectedCampaignItem, ref numberOfRequestsRemaining, connectorPerson );
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets any (active or past due FutureFollowUp) connection requests that don't have a connector
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="selectedCampaignItem">The selected campaign item.</param>
        /// <param name="connectorPerson">The connector person.</param>
        /// <returns></returns>
        public static IQueryable<ConnectionRequest> GetConnectionRequestsWithoutConnectorQuery( RockContext rockContext, CampaignItem selectedCampaignItem, Person connectorPerson )
        {
            var connectionRequestService = new ConnectionRequestService( rockContext );
            var currentDate = RockDateTime.Today;

            var connectorCampusIds = GetConnectorCampusIds( selectedCampaignItem, connectorPerson );

            // first, get any (active or past due FutureFollowUp) connection requests that don't have a connector
            var connectionRequestsWithoutConnectorQuery = connectionRequestService.Queryable()
                .Where( a => a.ConnectionOpportunity.Guid == selectedCampaignItem.OpportunityGuid
                    && a.ConnectorPersonAliasId == null
                    && ( a.ConnectionState == ConnectionState.Active || ( a.ConnectionState == ConnectionState.FutureFollowUp && a.FollowupDate < currentDate ) ) )

                // only include pending connection requests that have a null campus, or the campus matches the connector's campus
                .Where( a => ( a.CampusId == null ) || connectorCampusIds.Any( connectorCampusId => connectorCampusId == null || a.CampusId.Value == connectorCampusId ) );

            return connectionRequestsWithoutConnectorQuery;
        }

        /// <summary>
        /// Gets the connector campus ids, if the connectorPerson is not a member of the connecter groups for the opportunity, this wil return an empty lkist
        /// </summary>
        /// <param name="selectedCampaignItem">The selected campaign item.</param>
        /// <param name="connectorPerson">The connector person.</param>
        /// <returns></returns>
        public static List<int?> GetConnectorCampusIds( CampaignItem selectedCampaignItem, Person connectorPerson )
        {
            if ( connectorPerson == null )
            {
                // if no connector person is specified, we can return list that just an "All" (null) campus
                var result = new List<int?>();
                result.Add( ( int? ) null );
                return result;
            }

            var rockContext = new RockContext();
            List<int?> connectorCampusIds;
            var opportunityService = new ConnectionOpportunityService( rockContext );

            IQueryable<ConnectionOpportunityConnectorGroup> opportunityConnecterGroupQuery = opportunityService.Queryable()
                .Where( a => a.IsActive && a.Guid == selectedCampaignItem.OpportunityGuid )
                .SelectMany( a => a.ConnectionOpportunityConnectorGroups );

            int connectorPersonId = connectorPerson.Id;

            // get the campusid of the connector's connector group) of this opportunity
            // If the person is a member in more than one of opportunity groups, get all the campus ids that the connector can work with
            connectorCampusIds = opportunityConnecterGroupQuery.Where( a => a.ConnectorGroup.Members.Any( m => m.GroupMemberStatus == GroupMemberStatus.Active && m.PersonId == connectorPersonId ) ).Select( a => a.CampusId ).Distinct().ToList();

            // NOTE: if connectorPerson isn't in a ConnectionOpportunityConnectorGroup, there will be no campus ids. The AddCampaignRequests block shouldn't of let them request connections for this campaign
            return connectorCampusIds;
        }

        /// <summary>
        /// Assigns the connection requests from the SelectedCampaign's entity set.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="selectedCampaignItem">The selected campaign item.</param>
        /// <param name="numberOfRequestsRemaining">The number of requests remaining.</param>
        /// <param name="connectorPerson">The connector person.</param>
        private static void AssignConnectionRequestsFromEntitySet( RockContext rockContext, CampaignItem selectedCampaignItem, ref int numberOfRequestsRemaining, Person connectorPerson )
        {
            var opportunityService = new ConnectionOpportunityService( rockContext );
            ConnectionOpportunity opportunity = opportunityService.Get( selectedCampaignItem.OpportunityGuid );
            if ( opportunity == null || !opportunity.IsActive )
            {
                return;
            }

            int? defaultStatusId = opportunity.ConnectionType.ConnectionStatuses
                    .Where( s => s.IsDefault )
                    .Select( s => ( int? ) s.Id )
                    .FirstOrDefault();

            // If opportunity doesn't have a default status, something is wrong
            if ( defaultStatusId == null )
            {
                return;
            }

            var connectorCampusIds = GetConnectorCampusIds( selectedCampaignItem, connectorPerson );

            var connectionRequestService = new ConnectionRequestService( rockContext );
            var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );

            // get previous connections for the connector that have the same campus of the connector, or if the person's campus or connector person's campus is null
            var previousConnectedPersonIdsForCurrentPerson = connectionRequestService.Queryable()
                .Where( a => a.ConnectionOpportunityId == opportunity.Id )
                .Where( a => a.ConnectorPersonAlias.PersonId == connectorPerson.Id )
                .Where( a => ( a.CampusId == null ) || connectorCampusIds.Any( connectorCampusId => connectorCampusId == null || a.CampusId.Value == connectorCampusId ) )
                .Select( a => a.PersonAlias.PersonId ).Distinct().ToList();

            var entitySetId = CampaignConnectionHelper.GetEntitySet( selectedCampaignItem );
            var entitySetItemService = new EntitySetItemService( rockContext );
            var entitySetItemList = entitySetItemService.Queryable().Where( a => a.EntitySetId == entitySetId ).OrderBy( a => a.Order ).Select( a => new
            {
                PersonId = a.EntityId,
                EntityItemOrder = a.Order
            } ).ToList();

            if ( selectedCampaignItem.PreferPreviousConnector )
            {
                // sort them by any where the current person was assigned to this person before
                entitySetItemList = entitySetItemList
                    .OrderBy( a => previousConnectedPersonIdsForCurrentPerson.Any( x => x == a.PersonId ) )
                    .ThenBy( a => a.EntityItemOrder ).ToList();
            }
            else
            {
                entitySetItemList = entitySetItemList.OrderBy( a => a.EntityItemOrder ).ToList();
            }

            var personService = new PersonService( rockContext );

            // get the last connection datetime.
            var lastConnectionDateTime = RockDateTime.Now.AddDays( -selectedCampaignItem.DaysBetweenConnection );

            // if DaysBetweenConnection is 0 then check for connection request for any time period.
            if ( selectedCampaignItem.DaysBetweenConnection == default( int ) )
            {
                lastConnectionDateTime = DateTime.MinValue;
            }

            foreach ( var entitySetItem in entitySetItemList )
            {
                var entitySetPerson = personService.Get( entitySetItem.PersonId );
                if ( entitySetPerson == null )
                {
                    continue;
                }

                var entitySetPersonPrimaryCampusId = entitySetPerson.PrimaryCampusId;

                bool validCampus = IsValidCampus( connectorCampusIds, entitySetPersonPrimaryCampusId );
                if ( !validCampus )
                {
                    continue;
                }

                // double check that they haven't already been added
                bool personAlreadyHasConnectionRequest = PersonAlreadyHasConnectionRequest( opportunity.Id, rockContext, lastConnectionDateTime, entitySetPerson.Id );

                if ( personAlreadyHasConnectionRequest )
                {
                    continue;
                }

                var connectionRequest = new ConnectionRequest();
                connectionRequest.ConnectionOpportunityId = opportunity.Id;

                /*
                    3/30/2020 - NA 

                    When setting the connection request's Requester, we have to use the PrimaryAlias
                    to set the connectionRequest.PersonAlias property because the ConnectionRequestChangeTransaction
                    https://github.com/SparkabilityGroup/Rock/blob/a556a9285b7fdfe5594441286242f4feaa5847f2/Rock/Transactions/ConnectionRequestChangeTransaction.cs#L123
                    (which handles triggered workflows) expects it.  Also, it needs to be tracked by
                    the current rockContext... hence the change from GetAsNoTracking() to just Get() above:
                    var entitySetPerson = personService.Get( entitySetItem.PersonId );

                    In other words, this will not work correctly:
                    connectionRequest.PersonAliasId = entitySetPerson.PrimaryAliasId.Value;

                    Reason: This plug-in cannot change Rock core assembly code.
                */

                connectionRequest.PersonAlias = entitySetPerson.PrimaryAlias;
                connectionRequest.ConnectionState = ConnectionState.Active;
                connectionRequest.ConnectorPersonAliasId = connectorPerson.PrimaryAliasId;
                connectionRequest.CampusId = entitySetPersonPrimaryCampusId;
                connectionRequest.ConnectionStatusId = defaultStatusId.Value;

                if ( selectedCampaignItem.RequestCommentsLavaTemplate.IsNotNullOrWhiteSpace() )
                {
                    var mergeFields = new Dictionary<string, object>();
                    mergeFields.Add( "Person", entitySetPerson );
                    mergeFields.Add( "Family", entitySetPerson.GetFamily() );
                    connectionRequest.Comments = selectedCampaignItem.RequestCommentsLavaTemplate.ResolveMergeFields( mergeFields );
                }

                connectionRequestService.Add( connectionRequest );

                var connectionActivityTypeAssignedGuid = Rock.SystemGuid.ConnectionActivityType.ASSIGNED.AsGuid();
                int? assignedActivityId = new ConnectionActivityTypeService( rockContext ).GetId( connectionActivityTypeAssignedGuid );
                if ( assignedActivityId.HasValue )
                {
                    var connectionRequestActivity = new ConnectionRequestActivity();
                    connectionRequestActivity.ConnectionRequest = connectionRequest;
                    connectionRequestActivity.ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId;
                    connectionRequestActivity.ConnectionActivityTypeId = assignedActivityId.Value;
                    connectionRequestActivity.ConnectorPersonAliasId = connectorPerson.PrimaryAliasId;
                    connectionRequestActivityService.Add( connectionRequestActivity );
                }

                numberOfRequestsRemaining--;
                if ( numberOfRequestsRemaining <= 0 )
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Returns true if the person already has connection request.
        /// </summary>
        /// <param name="connectionOpportunityId">The connection opportunity identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="lastConnectionDateTime">The last connection date time.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        internal static bool PersonAlreadyHasConnectionRequest( int connectionOpportunityId, RockContext rockContext, DateTime lastConnectionDateTime, int personId )
        {
            return new ConnectionRequestService( rockContext )
                .Queryable()
                .Where( a => a.PersonAlias.PersonId == personId && a.ConnectionOpportunityId == connectionOpportunityId
                    && ( a.ConnectionState == ConnectionState.Active
                            || a.ConnectionState == ConnectionState.FutureFollowUp
                            || ( ( a.ConnectionState == ConnectionState.Connected || a.ConnectionState == ConnectionState.Inactive ) && a.ModifiedDateTime > lastConnectionDateTime ) ) )
                .Any();
        }

        /// <summary>
        /// Determines whether [is valid campus] [the specified connector campus ids].
        /// </summary>
        /// <param name="connectorCampusIds">The connector campus ids.</param>
        /// <param name="requestPrimaryCampusId">The request primary campus identifier.</param>
        /// <returns>
        ///   <c>true</c> if [is valid campus] [the specified connector campus ids]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsValidCampus( List<int?> connectorCampusIds, int? requestPrimaryCampusId )
        {
            // only assign the connection if the person doesn't have campus,
            // or the if the connector is in a connector group that doesn't have a campus,
            // or if the person's campus is one of the campusIds of the connector's connect groups
            return requestPrimaryCampusId == null || connectorCampusIds.Any( connectorCampusId => connectorCampusId == null || connectorCampusId.Value == requestPrimaryCampusId.Value );
        }
    }
}
