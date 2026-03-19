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
using System.Linq.Expressions;

using Rock.Data;
using Rock.Model.Connection.ConnectionType.DTO;
using Rock.Model.Connection.ConnectionType.Options;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ConnectionTypeService
    {
        #region Default Options

        /// <summary>
        /// The default options to use if not specified. This saves a few
        /// CPU cycles from having to create a new one each time.
        /// </summary>
        private static readonly ConnectionTypeQueryOptions DefaultGetConnectionTypesOptions = new ConnectionTypeQueryOptions();

        #endregion

        /// <summary>
        /// Copies the connection opportunities.
        /// </summary>
        /// <param name="connectionType">Source connectionType.</param>
        /// <param name="newConnectionType">Destination connectionType.</param>
        private void CopyConnectionOpportunities( ConnectionType connectionType, ConnectionType newConnectionType )
        {
            var rockContext = ( RockContext ) Context;

            foreach ( var connectionOpportunity in connectionType.ConnectionOpportunities )
            {
                var newConnectionOpportunity = connectionOpportunity.CloneWithoutIdentity();
                newConnectionOpportunity.ConnectionTypeId = newConnectionType.Id;
                newConnectionType.ConnectionOpportunities.Add( newConnectionOpportunity );
                rockContext.SaveChanges();

                foreach ( var connectionWorkflow in connectionOpportunity.ConnectionWorkflows )
                {
                    var newConnectionWorkflow = connectionWorkflow.CloneWithoutIdentity();
                    newConnectionWorkflow.ConnectionOpportunityId = newConnectionOpportunity.Id;
                    newConnectionOpportunity.ConnectionWorkflows.Add( newConnectionWorkflow );
                }

                foreach ( var opportunityGroup in connectionOpportunity.ConnectionOpportunityGroups )
                {
                    var newOpportunityGroup = opportunityGroup.CloneWithoutIdentity();
                    newOpportunityGroup.ConnectionOpportunityId = newConnectionOpportunity.Id;
                    newConnectionOpportunity.ConnectionOpportunityGroups.Add( newOpportunityGroup );
                }

                foreach ( var groupConfig in connectionOpportunity.ConnectionOpportunityGroupConfigs )
                {
                    var newGroupConfig = groupConfig.CloneWithoutIdentity();
                    newGroupConfig.ConnectionOpportunityId = newConnectionOpportunity.Id;
                    newConnectionOpportunity.ConnectionOpportunityGroupConfigs.Add( newGroupConfig );
                }

                foreach ( var connectorGroup in connectionOpportunity.ConnectionOpportunityConnectorGroups )
                {
                    var newConnectorGroup = connectorGroup.CloneWithoutIdentity();
                    newConnectorGroup.ConnectionOpportunityId = newConnectionOpportunity.Id;
                    newConnectionOpportunity.ConnectionOpportunityConnectorGroups.Add( newConnectorGroup );
                }

                newConnectionOpportunity.PhotoId = connectionOpportunity.PhotoId;

                foreach ( var campus in connectionOpportunity.ConnectionOpportunityCampuses )
                {
                    var newCampus = campus.CloneWithoutIdentity();
                    newCampus.ConnectionOpportunityId = newConnectionOpportunity.Id;
                    newConnectionOpportunity.ConnectionOpportunityCampuses.Add( newCampus );
                }

                rockContext.SaveChanges();

                // Copy attributes
                connectionOpportunity.LoadAttributes( rockContext );
                newConnectionOpportunity.LoadAttributes();

                if ( connectionOpportunity.Attributes != null && connectionOpportunity.Attributes.Any() )
                {
                    foreach ( var attributeKey in connectionOpportunity.Attributes.Select( a => a.Key ) )
                    {
                        string value = connectionOpportunity.GetAttributeValue( attributeKey );
                        newConnectionOpportunity.SetAttributeValue( attributeKey, value );
                    }
                }

                newConnectionOpportunity.SaveAttributeValues( rockContext );
            }
        }

        /// <summary>
        /// Copies the specified connection type.
        /// </summary>
        /// <param name="connectionTypeId">The connection type identifier.</param>
        /// <returns>
        /// Return the new ConnectionType ID
        /// </returns>
        public int Copy( int connectionTypeId )
        {
            var connectionType = this.Get( connectionTypeId );
            var rockContext = ( RockContext ) Context;
            var attributeService = new AttributeService( rockContext );
            var authService = new AuthService( rockContext );
            int newConnectionTypeId = 0;

            // Get current Opportunity attributes 
            var opportunityAttributes = attributeService
                .GetByEntityTypeId( new ConnectionOpportunity().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "ConnectionTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( connectionType.Id.ToString() ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            var newConnectionType = new ConnectionType();
            rockContext.WrapTransaction( () =>
            {

                newConnectionType = connectionType.CloneWithoutIdentity();
                newConnectionType.Name = connectionType.Name + " - Copy";
                this.Add( newConnectionType );
                rockContext.SaveChanges();
                newConnectionTypeId = newConnectionType.Id;

                foreach ( var connectionActivityTypeState in connectionType.ConnectionActivityTypes )
                {
                    var newConnectionActivityType = connectionActivityTypeState.CloneWithoutIdentity();
                    newConnectionType.ConnectionActivityTypes.Add( newConnectionActivityType );
                }

                foreach ( var connectionStatusState in connectionType.ConnectionStatuses )
                {
                    var newConnectionStatus = connectionStatusState.CloneWithoutIdentity();
                    newConnectionType.ConnectionStatuses.Add( newConnectionStatus );
                    newConnectionStatus.ConnectionTypeId = newConnectionType.Id;
                }

                foreach ( ConnectionWorkflow connectionWorkflowState in connectionType.ConnectionWorkflows )
                {
                    var newConnectionWorkflow = connectionWorkflowState.CloneWithoutIdentity();
                    newConnectionType.ConnectionWorkflows.Add( newConnectionWorkflow );
                    newConnectionWorkflow.ConnectionTypeId = newConnectionType.Id;
                }

                rockContext.SaveChanges();

                // Clone the Opportunity attributes
                List<Attribute> newAttributesState = new List<Attribute>();
                foreach ( var attribute in opportunityAttributes )
                {
                    var newAttribute = attribute.CloneWithoutIdentity();
                    newAttribute.IsSystem = false;
                    newAttributesState.Add( newAttribute );

                    foreach ( var qualifier in attribute.AttributeQualifiers )
                    {
                        var newQualifier = qualifier.Clone( false );
                        newQualifier.Id = 0;
                        newQualifier.Guid = Guid.NewGuid();
                        newQualifier.IsSystem = false;
                        newAttribute.AttributeQualifiers.Add( qualifier );
                    }
                }

                // Save Attributes
                string qualifierValue = newConnectionType.Id.ToString();
                Rock.Attribute.Helper.SaveAttributeEdits( newAttributesState, new ConnectionOpportunity().TypeId, "ConnectionTypeId", qualifierValue, rockContext );

                // Copy Security
                Rock.Security.Authorization.CopyAuthorization( connectionType, newConnectionType, rockContext );
            } );

            CopyConnectionOpportunities( connectionType, newConnectionType );
            ConnectionWorkflowService.RemoveCachedTriggers();
            return newConnectionTypeId;
        }

        /// <summary>
        /// Gets the connection opportunities queryable that will provide the results.
        /// This method returns a queryable of <see cref="ConnectionType"/> objects
        /// that can then have additional custom filters applied before the results are
        /// materialized from the database. If no filters are applied then all
        /// connection types are returned.
        /// </summary>
        /// <param name="options">The options that describe the filters to apply to the query.</param>
        /// <returns>A queryable of <see cref="ConnectionType"/> objects.</returns>
        /// <exception cref="System.InvalidOperationException">Context is not a RockContext.</exception>
        public IQueryable<ConnectionType> GetConnectionTypesQuery( ConnectionTypeQueryOptions options = null )
        {
            if ( !( Context is RockContext rockContext ) )
            {
                throw new InvalidOperationException( "Context is not a RockContext." );
            }

            options = options ?? DefaultGetConnectionTypesOptions;

            var qry = Queryable();

            if ( options.ConnectorPersonIds != null && options.ConnectorPersonIds.Any() )
            {
                var connectorRequestsQry = new ConnectionRequestService( rockContext ).Queryable()
                    .Where( r => r.ConnectionState != ConnectionState.Connected
                        && r.ConnectorPersonAliasId.HasValue
                        && options.ConnectorPersonIds.Contains( r.ConnectorPersonAlias.PersonId ) )
                    .Select( r => r.Id );

                qry = qry.Where( t => t.ConnectionOpportunities.SelectMany( o => o.ConnectionRequests ).Any( r => connectorRequestsQry.Contains( r.Id ) ) );
            }

            if ( !options.IncludeInactive )
            {
                qry = qry.Where( t => t.IsActive && t.IsActive );
            }

            return qry;
        }

        /// <summary>
        /// Filters the collection of <see cref="ConnectionType"/> objects to those
        /// that <paramref name="person"/> is authorized to view. This handles special
        /// security considerations such as <see cref="ConnectionType.EnableRequestSecurity"/>.
        /// </summary>
        /// <param name="connectionTypes">The connection types to be filtered.</param>
        /// <param name="person">The person that will be used for the authorization check.</param>
        /// <returns>A list of <see cref="ConnectionType"/> objects that the person is allowed to see.</returns>
        /// <exception cref="System.InvalidOperationException">Context is not a RockContext.</exception>
        public List<ConnectionType> GetViewAuthorizedConnectionTypes( IEnumerable<ConnectionType> connectionTypes, Person person )
        {
            if ( !( Context is RockContext rockContext ) )
            {
                throw new InvalidOperationException( "Context is not a RockContext." );
            }

            // Make a list of any type identifiers that are configured
            // for request security and the person is assigned as the
            // connector to any request.
            var currentPersonId = person?.Id;
            var selfAssignedSecurityTypes = new ConnectionRequestService( rockContext )
                .Queryable()
                .Where( r => r.ConnectorPersonAlias.PersonId == currentPersonId
                    && r.ConnectionOpportunity.ConnectionType.EnableRequestSecurity )
                .Select( r => r.ConnectionOpportunity.ConnectionTypeId )
                .Distinct()
                .ToList();

            // Put all the types in memory so we can check security.
            var types = connectionTypes.ToList()
                .Where( o => o.IsAuthorized( Authorization.VIEW, person )
                    || selfAssignedSecurityTypes.Contains( o.Id ) )
                .ToList();

            return types;
        }

        /// <summary>
        /// Retrieves health count statistics for connection requests associated with the specified connection types.
        /// </summary>
        /// <param name="connectionTypeQuery">A queryable collection of connection types for which to retrieve request health counts. Cannot be null.</param>
        /// <param name="options">The options that describe the filters to apply to the query.</param>
        /// <returns>A queryable collection of health count results for each specified connection type.</returns>
        internal IQueryable<ConnectionRequestHealthSnapshot> GetConnectionRequestHealthSnapshot(
            IQueryable<ConnectionType> connectionTypeQuery,
            ConnectionRequestHealthSnapshotQueryOptions options
        )
        {
            var today = RockDateTime.Today;
            var campusGuid = options?.CampusGuid;
            var connectionOpportunityGuid = options?.ConnectionOpportunityGuid;

            var baseQuery =
                from ct in connectionTypeQuery

                from co in ct.ConnectionOpportunities
                    .Where(co =>
                        !connectionOpportunityGuid.HasValue
                        || co.Guid == connectionOpportunityGuid.Value
                    )

                from cr in co.ConnectionRequests

                where !campusGuid.HasValue
                      || cr.Campus.Guid == campusGuid.Value
                      || (cr.Campus == null && campusGuid == null)

                select new
                {
                    ct.Id,

                    IsActive = cr.ConnectionState == ConnectionState.Active,

                    IsDueSoon =
                        cr.ConnectionState == ConnectionState.Active
                        && cr.DueSoonDate.HasValue
                        && DbFunctions.TruncateTime(cr.DueSoonDate.Value) <= today
                        && !(
                            cr.DueDate.HasValue
                            && DbFunctions.TruncateTime(cr.DueDate.Value) < today
                        ),

                    IsOverdue =
                        cr.ConnectionState == ConnectionState.Active
                        && cr.DueDate.HasValue
                        && DbFunctions.TruncateTime(cr.DueDate.Value) < today,

                    IsUnassigned =
                        cr.ConnectionState == ConnectionState.Active
                        && !cr.ConnectorPersonAliasId.HasValue
                };

            return
                from x in baseQuery
                group x by x.Id into g
                select new ConnectionRequestHealthSnapshot
                {
                    ConnectionTypeId = g.Key,

                    ActiveCount = g.Sum(x => x.IsActive ? 1 : 0),
                    DueSoonCount = g.Sum(x => x.IsDueSoon ? 1 : 0),
                    OverdueCount = g.Sum(x => x.IsOverdue ? 1 : 0),
                    UnassignedCount = g.Sum(x => x.IsUnassigned ? 1 : 0),

                    OnTrackCount =
                        g.Sum(x => x.IsActive ? 1 : 0)
                        - g.Sum(x => x.IsDueSoon ? 1 : 0)
                        - g.Sum(x => x.IsOverdue ? 1 : 0)
                };
        }


        /// <summary>
        /// Retrieves a queryable collection of connection request status distributions.
        /// </summary>
        /// <param name="connectionTypeQuery">An <see cref="IQueryable{T}"/> sequence of connection types to include in the status distribution calculation.</param>
        /// <param name="options">The options used to filter the results by campus or connection opportunity. If <c>null</c>, no filtering is applied.</param>
        /// <returns>An <see cref="IQueryable{T}"/> sequence of <see cref="ConnectionRequestStatusDistribution"/> objects,
        /// each representing the count and color for a specific connection request status.</returns>
        internal IQueryable<ConnectionRequestStatusDistribution> GetConnectionRequestStatusDistributions(
            IQueryable<ConnectionType> connectionTypeQuery,
            ConnectionRequestStatusDistributionQueryOptions options
        )
        {
            var campusGuid = options?.CampusGuid;
            var connectionOpportunityGuid = options?.ConnectionOpportunityGuid;

            return
                from ct in connectionTypeQuery
                from co in ct.ConnectionOpportunities
                    .Where(co =>
                        !connectionOpportunityGuid.HasValue
                        || co.Guid == connectionOpportunityGuid.Value
                    )
                from cr in co.ConnectionRequests
                where !campusGuid.HasValue
                      || cr.Campus.Guid == campusGuid.Value
                      || (cr.Campus == null && campusGuid == null)
                group cr by new
                {
                    cr.ConnectionStatusId,
                    cr.ConnectionStatus.Order,
                    cr.ConnectionStatus.Name,
                    cr.ConnectionStatus.HighlightColor
                }
                into g
                orderby g.Key.Order, g.Key.Name
                select new ConnectionRequestStatusDistribution
                {
                    Status = g.Key.Name,
                    Color = g.Key.HighlightColor,
                    Count = g.Count()
                };
        }


        /// <summary>
        /// Gets upcoming connection request follow up counts for the specified
        /// connection types, grouped into standard future time windows.
        /// </summary>
        /// <param name="connectionTypeQuery">
        /// An <see cref="IQueryable{ConnectionType}"/> that defines which connection
        /// types should be included. Callers are responsible for applying any desired
        /// filtering before invoking this method.
        /// </param>
        /// <param name="options">The options that describe the filters to apply to the query.</param>
        /// <returns>
        /// An <see cref="IQueryable{ConnectionRequestUpcomingFollowUpWindow}"/> where
        /// each row represents a single future time window for a connection type.
        /// </returns>
        internal IQueryable<ConnectionRequestUpcomingFollowUpWindow> GetConnectionRequestUpcomingFollowUpWindows(
            IQueryable<ConnectionType> connectionTypeQuery,
            ConnectionRequestUpcomingFollowUpWindowQueryOptions options
        )
        {
            var today = RockDateTime.Today;
            var maxFollowUpDate = today.AddDays( 28 );
            var campusGuid = options?.CampusGuid;
            var connectionOpportunityGuid = options?.ConnectionOpportunityGuid;

            var query =
                from cr in connectionTypeQuery
                    .SelectMany( ct =>
                        ct.ConnectionOpportunities
                            .Where( co =>
                                !connectionOpportunityGuid.HasValue
                                || co.Guid == connectionOpportunityGuid.Value
                            )
                            .SelectMany( co =>
                                co.ConnectionRequests
                                    .Where( cr =>
                                        !campusGuid.HasValue
                                        || cr.Campus.Guid == campusGuid.Value
                                    )
                            )
                    )
                where cr.FollowupDate.HasValue
                      && cr.FollowupDate > today
                      && cr.FollowupDate <= maxFollowUpDate
                let dayOffset = DbFunctions.DiffDays( today, cr.FollowupDate.Value )
                let window =
                    dayOffset <= 3 ? new { Start = 0, End = 3 } :
                    dayOffset <= 7 ? new { Start = 3, End = 7 } :
                                     new { Start = 7, End = 28 }
                group cr by new
                {
                    cr.ConnectionTypeId,
                    window.Start,
                    window.End
                }
                into g
                select new ConnectionRequestUpcomingFollowUpWindow
                {
                    ConnectionTypeId = g.Key.ConnectionTypeId,
                    StartOffsetDays = g.Key.Start,
                    EndOffsetDays = g.Key.End,
                    Count = g.Count()
                };

            return query
                .OrderBy( w => w.ConnectionTypeId )
                .ThenBy( w => w.StartOffsetDays );
        }

        /// <summary>
        /// Retrieves a summary of connection request completion metrics for the specified connection types and date range.
        /// </summary>
        /// <remarks>The returned query is not executed until enumerated. Filtering by campus or
        /// connection opportunity is applied if specified in the <paramref name="options"/> parameter.</remarks>
        /// <param name="connectionTypeQuery">An <see cref="IQueryable{T}"/> sequence of connection types to include in the metrics calculation.</param>
        /// <param name="startDate">The start date of the date range for which to calculate metrics. Only requests modified on or after this
        /// date are included.</param>
        /// <param name="endDate">The end date of the date range for which to calculate metrics. Only requests modified before the day after
        /// this date are included.</param>
        /// <param name="options">An object containing additional filtering options.</param>
        /// <returns>An <see cref="IQueryable{T}"/> sequence of <see cref="ConnectionRequestCompletionMetricsSummary"/> objects,
        /// each representing completion metrics for a connection type within the specified date range and filters.</returns>
        internal IQueryable<ConnectionRequestCompletionMetricsSummary> GetConnectionRequestCompletionMetricsSummary(
            IQueryable<ConnectionType> connectionTypeQuery,
            DateTime startDate,
            DateTime endDate,
            ConnectionRequestCompletionMetricsQueryOptions options
        )
        {
            var rangeStart = startDate.Date;
            var rangeEnd = endDate.Date.AddDays( 1 );
            var campusGuid = options?.CampusGuid;
            var connectionOpportunityGuid = options?.ConnectionOpportunityGuid;

            var query =
                from cr in connectionTypeQuery
                    .SelectMany( ct =>
                        ct.ConnectionOpportunities
                            .Where( co =>
                                !connectionOpportunityGuid.HasValue
                                || co.Guid == connectionOpportunityGuid.Value
                            )
                            .SelectMany( co => co.ConnectionRequests )
                    )
                where cr.ModifiedDateTime.HasValue
                    && cr.ModifiedDateTime >= rangeStart
                    && cr.ModifiedDateTime < rangeEnd
                    && ( !campusGuid.HasValue
                        || cr.Campus.Guid == campusGuid.Value 
                    )
                let firstActivityDate =
                    cr.ConnectionRequestActivities
                        .OrderBy( a => a.CreatedDateTime )
                        .Select( a => a.CreatedDateTime )
                        .FirstOrDefault()
                group new
                {
                    cr,
                    firstActivityDate
                }
                by cr.ConnectionTypeId
                into g
                let connectedCount = g.Count( x => x.cr.ConnectionState == ConnectionState.Connected ) 
                select new ConnectionRequestCompletionMetricsSummary
                {
                    ConnectionTypeId = g.Key,

                    RequestsCompletedCount = connectedCount,

                    TimelinessPercent = connectedCount == 0
                        ? 0
                        : (
                            g.Count( x =>
                                x.cr.ConnectionState == ConnectionState.Connected
                                && x.cr.ConnectedDateTime.HasValue
                                && (
                                    !x.cr.DueDate.HasValue
                                    || x.cr.ConnectedDateTime.Value <= x.cr.DueDate.Value
                                )
                            )
                            / connectedCount
                        ),

                    AverageResponsivenessDays = g
                        .Where( x =>
                            x.cr.CreatedDateTime.HasValue
                            && x.firstActivityDate.HasValue
                        )
                        .Select( x => ( decimal ) DbFunctions.DiffDays( x.cr.CreatedDateTime.Value, x.firstActivityDate.Value ) )
                        .DefaultIfEmpty()
                        .Average(),

                    AverageCompletionDays = g
                        .Where( x =>
                            x.cr.CreatedDateTime.HasValue
                            && x.cr.ConnectionState == ConnectionState.Connected
                            && x.cr.ConnectedDateTime.HasValue
                        )
                        .Select( x => ( decimal )DbFunctions.DiffDays( x.cr.CreatedDateTime.Value, x.cr.ConnectedDateTime.Value ) )
                        .DefaultIfEmpty()
                        .Average()
                };

            return query;
        }

        /// <summary>
        /// Compares connection request completion metrics for the specified date range with the immediately preceding
        /// period for each connection type.
        /// </summary>
        /// <remarks>The previous period is defined as the same length as the current period, immediately
        /// preceding the specified start date. For example, if the current period is 7 days, the previous period will
        /// be the 7 days before the start date. The returned query is not executed until enumerated.</remarks>
        /// <param name="connectionTypeQuery">A queryable collection of connection types to include in the comparison. Only metrics for these connection
        /// types are considered.</param>
        /// <param name="startDate">The start date of the current period for which to calculate metrics. The time component is ignored.</param>
        /// <param name="endDate">The end date of the current period for which to calculate metrics. The time component is ignored.</param>
        /// <param name="options">Options that control how the completion metrics are calculated and filtered.</param>
        /// <returns>An <see cref="IQueryable{ConnectionRequestCompletionMetricsComparison}"/> containing comparison results for
        /// each connection type, including metrics for the current and previous periods and their differences.</returns>
        internal IQueryable<ConnectionRequestCompletionMetricsComparison> GetConnectionRequestCompletionMetricsComparison(
            IQueryable<ConnectionType> connectionTypeQuery,
            DateTime startDate,
            DateTime endDate,
            ConnectionRequestCompletionMetricsQueryOptions options
        )
        {
            var rangeLength = ( endDate.Date - startDate.Date ).Days + 1;

            var previousStartDate = startDate.Date.AddDays( -rangeLength );
            var previousEndDate = startDate.Date.AddDays( -1 );

            var currentPeriodSummary =
                GetConnectionRequestCompletionMetricsSummary(
                    connectionTypeQuery,
                    startDate,
                    endDate,
                    options );

            var previousPeriodSummary =
                GetConnectionRequestCompletionMetricsSummary(
                    connectionTypeQuery,
                    previousStartDate,
                    previousEndDate,
                    options );

            var comparison =
                from current in currentPeriodSummary
                join previous in previousPeriodSummary
                    on current.ConnectionTypeId equals previous.ConnectionTypeId
                    into previousJoin
                from previous in previousJoin.DefaultIfEmpty()
                select new ConnectionRequestCompletionMetricsComparison
                {
                    ConnectionTypeId = current.ConnectionTypeId,

                    Current = current,
                    Previous = previous,

                    TimelinessPercentDelta =
                        current.TimelinessPercent - ( previous != null ? previous.TimelinessPercent : 0 ),

                    AverageResponsivenessDaysDelta =
                        current.AverageResponsivenessDays - ( previous != null ? previous.AverageResponsivenessDays : 0 ),

                    RequestsCompletedCountDelta =
                        current.RequestsCompletedCount - ( previous != null ? previous.RequestsCompletedCount : 0 ),

                    AverageCompletionDaysDelta =
                        current.AverageCompletionDays - ( previous != null ? previous.AverageCompletionDays : 0 )
                };

            return comparison;
        }
    }
}