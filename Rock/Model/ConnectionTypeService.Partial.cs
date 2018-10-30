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

namespace Rock.Model
{

    /// <summary>
    /// 
    /// </summary>
    public partial class ConnectionTypeService
    {
        /// <summary>
        /// Clear ID, guid, modified and created properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        private void InitModel<T>( ref T model ) where T : Rock.Data.IModel
        {
            model.CreatedByPersonAlias = null;
            model.CreatedByPersonAliasId = null;
            model.CreatedDateTime = RockDateTime.Now;
            model.ModifiedByPersonAlias = null;
            model.ModifiedByPersonAliasId = null;
            model.ModifiedDateTime = RockDateTime.Now;
            model.Id = 0;
            model.Guid = Guid.NewGuid();
        }

        /// <summary>
        /// Copies the connection opportunities.
        /// </summary>
        /// <param name="connectionType">Source connectionType.</param>
        /// <param name="newConnectionType">Destination connectionType.</param>
        private void CopyConnectionOpportunities( ConnectionType connectionType, ConnectionType newConnectionType )
        {
            RockContext rockContext = ( RockContext ) Context;

            foreach ( ConnectionOpportunity connectionOpportunity in connectionType.ConnectionOpportunities )
            {
                ConnectionOpportunity newConnectionOpportunity = new ConnectionOpportunity();
                newConnectionOpportunity.CopyPropertiesFrom( connectionOpportunity );
                InitModel( ref newConnectionOpportunity );
                newConnectionOpportunity.ConnectionTypeId = newConnectionType.Id;
                newConnectionType.ConnectionOpportunities.Add( newConnectionOpportunity );
                rockContext.SaveChanges();

                foreach ( var connectionWorkflow in connectionOpportunity.ConnectionWorkflows )
                {
                    ConnectionWorkflow newConnectionWorkflow = new ConnectionWorkflow();
                    newConnectionWorkflow.CopyPropertiesFrom( connectionWorkflow );
                    InitModel( ref newConnectionWorkflow );
                    newConnectionWorkflow.ConnectionOpportunityId = newConnectionOpportunity.Id;
                    newConnectionOpportunity.ConnectionWorkflows.Add( newConnectionWorkflow );
                }
                foreach ( var connectionWorkflow in connectionOpportunity.ConnectionType.ConnectionWorkflows )
                {
                    ConnectionWorkflow newConnectionWorkflow = new ConnectionWorkflow();
                    newConnectionWorkflow.CopyPropertiesFrom( connectionWorkflow );
                    InitModel( ref newConnectionWorkflow );
                    newConnectionWorkflow.ConnectionOpportunityId = newConnectionOpportunity.Id;
                    newConnectionOpportunity.ConnectionType.ConnectionWorkflows.Add( newConnectionWorkflow );
                }

                foreach ( var opportunityGroup in connectionOpportunity.ConnectionOpportunityGroups )
                {
                    ConnectionOpportunityGroup newOpportunityGroup = new ConnectionOpportunityGroup();
                    newOpportunityGroup.CopyPropertiesFrom( opportunityGroup );
                    InitModel( ref newOpportunityGroup );
                    newOpportunityGroup.ConnectionOpportunityId = newConnectionOpportunity.Id;
                    newConnectionOpportunity.ConnectionOpportunityGroups.Add( newOpportunityGroup );
                }

                foreach ( var groupConfig in connectionOpportunity.ConnectionOpportunityGroupConfigs )
                {
                    ConnectionOpportunityGroupConfig newGroupConfig = new ConnectionOpportunityGroupConfig();
                    newGroupConfig.CopyPropertiesFrom( groupConfig );
                    InitModel( ref newGroupConfig );
                    newGroupConfig.ConnectionOpportunityId = newConnectionOpportunity.Id;
                    newConnectionOpportunity.ConnectionOpportunityGroupConfigs.Add( newGroupConfig );
                }

                foreach ( var connectorGroup in connectionOpportunity.ConnectionOpportunityConnectorGroups )
                {
                    ConnectionOpportunityConnectorGroup newConnectorGroup = new ConnectionOpportunityConnectorGroup();
                    newConnectorGroup.CopyPropertiesFrom( connectorGroup );
                    InitModel( ref newConnectorGroup );
                    newConnectorGroup.ConnectionOpportunityId = newConnectionOpportunity.Id;
                    newConnectionOpportunity.ConnectionOpportunityConnectorGroups.Add( newConnectorGroup );
                }

                newConnectionOpportunity.PhotoId = connectionOpportunity.PhotoId;

                foreach ( var campus in connectionOpportunity.ConnectionOpportunityCampuses )
                {
                    ConnectionOpportunityCampus newCampus = new ConnectionOpportunityCampus();
                    newCampus.CopyPropertiesFrom( campus );
                    InitModel( ref newCampus );
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
            RockContext rockContext = ( RockContext ) Context;
            int newConnectionTypeId = 0;
            AttributeService attributeService = new AttributeService( rockContext );
            var authService = new AuthService( rockContext );

            // Get current Opportunity attributes 
            var opportunityAttributes = attributeService
                .GetByEntityTypeId( new ConnectionOpportunity().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "ConnectionTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( connectionType.Id.ToString() ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            ConnectionType newConnectionType = new ConnectionType();
            rockContext.WrapTransaction( () =>
            {

                newConnectionType.CopyPropertiesFrom( connectionType );
                InitModel( ref newConnectionType );
                newConnectionType.Name = connectionType.Name + " - Copy";
                this.Add( newConnectionType );
                rockContext.SaveChanges();
                newConnectionTypeId = newConnectionType.Id;

                foreach ( var connectionActivityTypeState in connectionType.ConnectionActivityTypes )
                {
                    ConnectionActivityType newConnectionActivityType = new ConnectionActivityType();
                    newConnectionActivityType.CopyPropertiesFrom( connectionActivityTypeState );
                    InitModel( ref newConnectionActivityType );
                    newConnectionType.ConnectionActivityTypes.Add( newConnectionActivityType );
                }

                foreach ( var connectionStatusState in connectionType.ConnectionStatuses )
                {
                    ConnectionStatus newConnectionStatus = new ConnectionStatus();
                    newConnectionStatus.CopyPropertiesFrom( connectionStatusState );
                    InitModel( ref newConnectionStatus );
                    newConnectionType.ConnectionStatuses.Add( newConnectionStatus );
                    newConnectionStatus.ConnectionTypeId = newConnectionType.Id;
                }

                foreach ( ConnectionWorkflow connectionWorkflowState in connectionType.ConnectionWorkflows )
                {
                    ConnectionWorkflow newConnectionWorkflow = new ConnectionWorkflow();
                    newConnectionWorkflow.CopyPropertiesFrom( connectionWorkflowState );
                    InitModel( ref newConnectionWorkflow );
                    newConnectionType.ConnectionWorkflows.Add( newConnectionWorkflow );
                    newConnectionWorkflow.ConnectionTypeId = newConnectionType.Id;
                }

                rockContext.SaveChanges();

                // Clone the Opportunity attributes
                List<Attribute> newAttributesState = new List<Attribute>();
                foreach ( var attribute in opportunityAttributes )
                {
                    var newAttribute = attribute.Clone( false );
                    InitModel( ref newAttribute );
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
    }
}