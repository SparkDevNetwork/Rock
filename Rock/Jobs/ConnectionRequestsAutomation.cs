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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Quartz;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Connection Requests Automation" )]
    [Description( "This job will perform routine operations on active connection requests. This includes processing any configured Status Automation rules that are configured on the Connection Type." )]

    [DisallowConcurrentExecution]
    public class ConnectionRequestsAutomation : IJob
    {
        #region Constructors

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public ConnectionRequestsAutomation()
        {
        }

        #endregion Constructors

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            // Use concurrent safe data structures to track the count and errors
            var errors = new ConcurrentBag<string>();
            var updatedResults = new ConcurrentBag<int>();
            var connectionTypeViews = GetConnectionTypeViewsWithOrderedStatuses();

            // Loop through each connection type
            Parallel.ForEach(
                connectionTypeViews,
                connectionTypeView =>
                {
                    ProcessConnectionType( connectionTypeView, updatedResults, out var errorsFromThisConnectionType );

                    if ( errorsFromThisConnectionType != null && errorsFromThisConnectionType.Any() )
                    {
                        errorsFromThisConnectionType.ForEach( errors.Add );
                    }
                } );

            var totalUpdated = updatedResults.Sum();
            context.Result = $"{totalUpdated} Connection Request{( totalUpdated == 1 ? "" : "s" )} updated.";

            if ( errors.Any() )
            {
                ThrowErrors( context, errors );
            }
        }

        /// <summary>
        /// Processes the connection type.
        /// </summary>
        /// <param name="connectionTypeView">The connection type view.</param>
        /// <param name="updatedResults">The updated results.</param>
        /// <param name="errorMessages">The error message.</param>
        private void ProcessConnectionType(
            ConnectionTypeView connectionTypeView,
            ConcurrentBag<int> updatedResults,
            out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var groupViews = new List<GroupView>();
            foreach ( var connectionStatus in connectionTypeView.ConnectionStatuses )
            {
                foreach ( var connectionStatusAutomation in connectionStatus.ConnectionStatusAutomations )
                {
                    var rockContext = new RockContext();
                    var connectionRequestService = new ConnectionRequestService( rockContext );
                    var connectionRequestQry = connectionRequestService.Queryable().Include( a => a.AssignedGroup.Members ).Where( a => a.ConnectionStatusId == connectionStatus.Id );
                    if ( connectionStatusAutomation.DataViewId.HasValue )
                    {
                        // Get the dataview configured for the connection request
                        var dataViewService = new DataViewService( rockContext );
                        var dataview = dataViewService.Get( connectionStatusAutomation.DataViewId.Value );

                        if ( dataview == null )
                        {
                            errorMessages.Add( $"The dataview {connectionStatusAutomation.DataViewId} for Connection Type {connectionTypeView.ConnectionTypeId} did not resolve" );
                            continue;
                        }

                        // Now we'll filter our connection request query to only include the ones that are in the configured data view.
                        var dataViewGetQueryArgs = new DataViewGetQueryArgs
                        {
                            DbContext = rockContext
                        };

                        IQueryable<ConnectionRequest> dataviewQuery;
                        try
                        {
                            dataviewQuery = dataview.GetQuery( dataViewGetQueryArgs ) as IQueryable<ConnectionRequest>;
                        }
                        catch ( Exception ex )
                        {
                            errorMessages.Add( ex.Message );
                            ExceptionLogService.LogException( ex );
                            continue;
                        }

                        if ( dataviewQuery == null )
                        {
                            errorMessages.Add( $"Generating a query for dataview {connectionStatusAutomation.DataViewId} for Status {connectionStatus.Id} in Connection Type {connectionTypeView.ConnectionTypeId} was not successful" );
                            continue;
                        }

                        connectionRequestQry = connectionRequestQry.Where( a => dataviewQuery.Any( b => b.Id == a.Id ) );
                    }

                    var eligibleConnectionRequests = new List<ConnectionRequest>();
                    if ( connectionStatusAutomation.GroupRequirementsFilter != GroupRequirementsFilter.Ignore )
                    {
                        var connectionRequests = connectionRequestQry.ToList();
                        foreach ( var connectionRequest in connectionRequests )
                        {
                            var isRequirementMet = true;
                            // Group Requirement can't be met when either placement group or placement group role id is missing
                            if ( !connectionRequest.AssignedGroupId.HasValue || !connectionRequest.AssignedGroupMemberRoleId.HasValue )
                            {
                                isRequirementMet = false;
                            }
                            else
                            {
                                var groupView = GetGroupView( connectionRequest, groupViews, rockContext );
                                if ( groupView != null && groupView.HasGroupRequirement )
                                {
                                    isRequirementMet = IsGroupRequirementMet( connectionRequest, groupView, rockContext );
                                }
                            }

                            // connection request based on if group requirement is met or not is added to list for status update
                            if ( ( connectionStatusAutomation.GroupRequirementsFilter == GroupRequirementsFilter.DoesNotMeet && !isRequirementMet ) ||
                                ( connectionStatusAutomation.GroupRequirementsFilter == GroupRequirementsFilter.MustMeet && isRequirementMet ) )
                            {
                                eligibleConnectionRequests.Add( connectionRequest );
                            }
                        }
                    }
                    else
                    {
                        eligibleConnectionRequests = connectionRequestQry.ToList();
                    }

                    var updatedCount = 0;
                    foreach ( var connectionRequest in eligibleConnectionRequests )
                    {
                        connectionRequest.ConnectionStatusId = connectionStatusAutomation.DestinationStatusId;
                        updatedCount++;
                    }

                    rockContext.SaveChanges();
                    updatedResults.Add( updatedCount );
                }
            }
        }

        #region Data Helpers

        /// <summary>
        /// Gets the connection type view query.  The ConnectionStatues within each type
        /// should be processed by their ConnectionStatus.Order to avoid non-ideal
        /// conditions from arising.
        /// </summary>
        /// <returns></returns>
        private List<ConnectionTypeView> GetConnectionTypeViewsWithOrderedStatuses()
        {
            var rockContext = new RockContext();
            var connectionTypeService = new ConnectionTypeService( rockContext );

            var views = connectionTypeService.Queryable().AsNoTracking()
                .Where( ct => ct.IsActive && ct.ConnectionStatuses.Any( b => b.ConnectionStatusAutomations.Count > 0 ) )
                .Select( ct => new ConnectionTypeView
                {
                    ConnectionTypeId = ct.Id,
                    ConnectionStatuses = ct.ConnectionStatuses
                    .Where( cs => cs.IsActive && cs.ConnectionStatusAutomations.Any() )
                    .OrderBy( cs => cs.Order )
                } )
                .ToList();

            return views;
        }

        private GroupView GetGroupView( ConnectionRequest connectionRequest, List<GroupView> groupViews, RockContext rockContext )
        {
            GroupView groupView = groupViews.FirstOrDefault( a => a.GroupId == connectionRequest.AssignedGroupId.Value );
            if ( groupView == null )
            {
                var groupService = new GroupService( rockContext );
                var group = groupService.Get( connectionRequest.AssignedGroupId.Value );
                groupView = new GroupView()
                {
                    GroupId = group.Id,
                    Group = group
                };

                groupView.HasGroupRequirement = new GroupRequirementService( rockContext ).Queryable().Where( a => ( a.GroupId.HasValue && a.GroupId == group.Id ) || ( a.GroupTypeId.HasValue && a.GroupTypeId == group.GroupTypeId ) ).Any();
            }

            return groupView;
        }

        private bool IsGroupRequirementMet( ConnectionRequest connectionRequest, GroupView groupView, RockContext rockContext )
        {
            var requirementMeet = true;
            if ( groupView != null )
            {
                var requirementsResults = groupView.Group.PersonMeetsGroupRequirements(
                    rockContext,
                    connectionRequest.PersonAlias.PersonId,
                    connectionRequest.AssignedGroupMemberRoleId.Value );

                if ( requirementsResults != null && requirementsResults
                    .Where( a => a.MeetsGroupRequirement != MeetsGroupRequirement.NotApplicable )
                    .Any( r =>
                        r.MeetsGroupRequirement != MeetsGroupRequirement.Meets && r.MeetsGroupRequirement != MeetsGroupRequirement.MeetsWithWarning )
                    )
                {
                    requirementMeet = false;
                }
            }

            return requirementMeet;
        }

        #endregion Data Helpers

        #region Job State Helpers

        /// <summary>
        /// Throws the errors.
        /// </summary>
        /// <param name="jobExecutionContext">The job execution context.</param>
        /// <param name="errors">The errors.</param>
        private void ThrowErrors( IJobExecutionContext jobExecutionContext, IEnumerable<string> errors )
        {
            var sb = new StringBuilder();

            if ( !jobExecutionContext.Result.ToStringSafe().IsNullOrWhiteSpace() )
            {
                sb.AppendLine();
            }

            sb.AppendLine( string.Format( "{0} Errors: ", errors.Count() ) );

            foreach ( var error in errors )
            {
                sb.AppendLine( error );
            }

            var errorMessage = sb.ToString();
            jobExecutionContext.Result += errorMessage;

            var exception = new Exception( errorMessage );
            var httpContext = HttpContext.Current;
            ExceptionLogService.LogException( exception, httpContext );

            throw exception;
        }

        #endregion Job State Helpers

        #region Views

        /// <summary>
        /// Selected properties from a <see cref="ConnectionType"/> related query
        /// </summary>
        private class ConnectionTypeView
        {
            /// <summary>
            /// Gets or sets the connection type identifier.
            /// </summary>
            /// <value>
            /// The connection type identifier.
            /// </value>
            public int ConnectionTypeId { get; set; }

            /// <summary>
            /// Gets or sets the connection status with data automation rule.
            /// </summary>
            /// <value>
            /// The connection status with data automation rule.
            /// </value>
            public IEnumerable<ConnectionStatus> ConnectionStatuses { get; set; }
        }

        /// <summary>
        /// Selected properties from a <see cref="ConnectionType"/> related query
        /// </summary>
        private class GroupView
        {
            /// <summary>
            /// Gets or sets the group identifier.
            /// </summary>
            /// <value>
            /// The group identifier.
            /// </value>
            public int GroupId { get; set; }

            /// <summary>
            /// Gets or sets a flag indicating if this Group has any group requirements.
            /// </summary>
            /// <value>
            /// A <see cref="System.Boolean"/> value that is <c>true</c> if this Group has any group requirements; otherwise <c>false</c>.
            /// </value>
            public bool HasGroupRequirement { get; set; }

            /// <summary>
            /// Gets or sets the group.
            /// </summary>
            /// <value>
            /// The group identifier.
            /// </value>
            public Group Group { get; set; }
        }

        #endregion SQL Views
    }
}
