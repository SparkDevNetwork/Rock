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
using Rock.Data;
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

    }
}