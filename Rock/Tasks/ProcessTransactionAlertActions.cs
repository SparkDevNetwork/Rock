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

using Rock.Bus.Message;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Handles any actions for the specified <see cref="FinancialTransactionAlert" /> that trigger after it the alert has been created.
    /// </summary>
    public sealed class ProcessTransactionAlertActions : BusStartedTask<ProcessTransactionAlertActions.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                // Load the alert and alert type
                var financialTransactionAlertService = new FinancialTransactionAlertService( rockContext );
                var alert = financialTransactionAlertService.Queryable()
                    .AsNoTracking()
                    .Include( a => a.FinancialTransactionAlertType )
                    .FirstOrDefault( a => a.Id == message.FinancialTransactionAlertId );

                var alertType = alert?.FinancialTransactionAlertType;

                // If the alert or alert type are no longer in the database, then there is nothing that can be done
                if ( alertType == null )
                {
                    return;
                }

                // Get the person that this alert was generated for. Several of the items below use this
                var personAliasService = new PersonAliasService( rockContext );
                var person = personAliasService.Queryable()
                    .AsNoTracking()
                    .Where( pa => pa.Id == alert.PersonAliasId )
                    .Select( pa => pa.Person )
                    .FirstOrDefault();

                // Generate the merge objects for the lava templates used in the items below
                var isoDate = alert.AlertDateTime.ToISO8601DateString();
                var alertsPageId = PageCache.Get( SystemGuid.Page.GIVING_ALERTS ).Id;
                var relativeAlertLink = $"page/{alertsPageId}?StartDate={isoDate}&EndDate={isoDate}&AlertTypeId={alertType.Id}";

                var mergeObjects = new Dictionary<string, object> {
                    { nameof(FinancialTransactionAlert), alert },
                    { nameof(FinancialTransactionAlertType), alertType },
                    { nameof(Person), person },
                    { "RelativeAlertLink", relativeAlertLink }
                };

                // Launch workflow if configured
                if ( alertType.WorkflowTypeId.HasValue )
                {
                    var workflowAttributeValues = new Dictionary<string, string>();
                    workflowAttributeValues.Add( nameof( FinancialTransactionAlert ), alert.Guid.ToString() );
                    workflowAttributeValues.Add( nameof( FinancialTransactionAlertType ), alertType.Guid.ToString() );
                    workflowAttributeValues.Add( "FinancialTransactionId", alert.FinancialTransaction.Id.ToStringSafe() );
                    workflowAttributeValues.Add( nameof( Person ), person.Guid.ToString() );
                    alert.LaunchWorkflow( alertType.WorkflowTypeId, string.Empty, workflowAttributeValues, alert.PersonAliasId );
                }

                // Add the person to a connection opportunity if configured
                if ( alertType.ConnectionOpportunityId.HasValue )
                {
                    var connectionRequestService = new ConnectionRequestService( rockContext );

                    int personAliasId = alert.PersonAliasId;
                    var request = connectionRequestService.CreateConnectionRequestWithDefaultConnector( alertType.ConnectionOpportunityId.Value, personAliasId, alertType.CampusId, rockContext: rockContext );
                    if ( alert.TransactionId.HasValue )
                    {
                        request.LoadAttributes();
                        request.SetAttributeValue( "FinancialTransactionId", alert.TransactionId.Value.ToString() );
                    }

                    connectionRequestService.Add( request );
                    rockContext.SaveChanges();
                    request.SaveAttributeValues( rockContext );
                }
               
                // Send a bus event if configured
                if ( alertType.SendBusEvent )
                {
                    new TransactionWasAlertedMessage
                    {
                        FinancialTransactionAlertId = alert.Id
                    }.Publish();
                }

                // Send a communication if configured
                if ( alertType.SystemCommunicationId.HasValue )
                {
                    var systemCommunicationService = new SystemCommunicationService( rockContext );
                    var systemCommunication = systemCommunicationService.Get( alertType.SystemCommunicationId.Value );

                    if ( person != null && systemCommunication != null )
                    {
                        CommunicationHelper.SendMessage( person, ( int ) person.CommunicationPreference, systemCommunication, mergeObjects );
                    }
                }

                // Send a communication to account followers if an Account Participant System Communication and Account is specified
                // for this alert type
                if ( alertType.AccountParticipantSystemCommunicationId.HasValue && alertType.FinancialAccountId.HasValue )
                {
                    var systemCommunicationService = new SystemCommunicationService( rockContext );
                    var financialAccountService = new FinancialAccountService( rockContext );
                    var accountParticipantSystemCommunication = systemCommunicationService.Get( alertType.AccountParticipantSystemCommunicationId.Value );
                    if ( accountParticipantSystemCommunication != null )
                    {
                        var accountFollowers = financialAccountService
                            .GetAccountParticipants( alertType.FinancialAccountId.Value, RelatedEntityPurposeKey.FinancialAccountGivingAlert )
                            .Select( a => a.Person );

                        foreach ( var accountFollower in accountFollowers )
                        {
                            CommunicationHelper.SendMessage( accountFollower, ( int ) accountFollower.CommunicationPreference, accountParticipantSystemCommunication, mergeObjects );
                        }
                    }
                }

                // Send a notification to a group if configured
                if ( alertType.AlertSummaryNotificationGroupId.HasValue )
                {
                    var systemEmailGuid = SystemGuid.SystemCommunication.FINANCIAL_TRANSACTION_ALERT_NOTIFICATION_SUMMARY.AsGuid();
                    var systemCommunicationService = new SystemCommunicationService( rockContext );
                    var systemCommunication = systemCommunicationService.Get( systemEmailGuid );

                    CommunicationHelper.SendMessage( alertType.AlertSummaryNotificationGroupId.Value, systemCommunication, mergeObjects );
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the financial transaction alert identifier.
            /// </summary>
            /// <value>
            /// The financial transaction alert identifier.
            /// </value>
            public int FinancialTransactionAlertId { get; set; }
        }
    }
}