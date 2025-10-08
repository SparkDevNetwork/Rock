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

using Rock.Data;
using Rock.Enums.Communication;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class CommunicationFlowService
    {
        private static readonly IEnumerable<CommunicationFlowInstanceRecipientInactiveReason> UnsubscribedInactiveReasons = new List<CommunicationFlowInstanceRecipientInactiveReason>
        {
            CommunicationFlowInstanceRecipientInactiveReason.Unsubscribed,
            CommunicationFlowInstanceRecipientInactiveReason.UnsubscribedFromFlow
        };

        private const string PersonUnsubscribedFromCommunicationFlowStatusNote = "Person unsubscribed from communication flow.";

        /// <summary>
        /// Unsubscribes a person from a flow, preventing them from being a recipient of future communications for this communication flow.
        /// </summary>
        /// <param name="communicationFlowId">The identifier of the communication flow to unsubscribe from.</param>
        /// <param name="personId">The identifier of the person to unsubscribe.</param>
        public void UnsubscribePersonFromFlow( int communicationFlowId, int personId )
        {
            if ( Context is RockContext rockContext )
            {
                var communicationFlowInstanceRecipientService = new CommunicationFlowInstanceRecipientService( rockContext );
                var communicationFlowInstanceService = new CommunicationFlowInstanceService( rockContext );

                var communicationFlowInstanceRecipients = communicationFlowInstanceRecipientService
                    .Queryable()
                    .Where( r =>
                        r.CommunicationFlowInstance.CommunicationFlowId == communicationFlowId
                        && r.RecipientPersonAlias.PersonId == personId
                        && r.Status == CommunicationFlowInstanceRecipientStatus.Active )
                    .ToList();

                foreach ( var communicationFlowInstanceRecipient in communicationFlowInstanceRecipients )
                {
                    DeactivateInstanceRecipient( communicationFlowInstanceRecipient, CommunicationFlowInstanceRecipientInactiveReason.UnsubscribedFromFlow );
                }

                // Cancel pending communications for this flow.
                var communicationFlowInstanceCommunicationRecipients = communicationFlowInstanceService
                    .GetByCommunicationFlow( communicationFlowId )
                    .SelectMany( cfi => cfi.CommunicationFlowInstanceCommunications.SelectMany( cfic => cfic.Communication.Recipients ) )
                    .Where( cr =>
                        cr.PersonAlias.PersonId == personId
                        && !cr.Communication.SendDateTime.HasValue
                        && cr.Communication.FutureSendDateTime.HasValue
                        && cr.Status == CommunicationRecipientStatus.Pending )
                    .ToList();

                foreach ( var communicationRecipient in communicationFlowInstanceCommunicationRecipients )
                {
                    communicationRecipient.Status = CommunicationRecipientStatus.Cancelled;
                    communicationRecipient.StatusNote = PersonUnsubscribedFromCommunicationFlowStatusNote;
                }
            }
        }

        /// <summary>
        /// Resubscribes a person to a flow from which they unsubscribed, allowing them to be a recipient of future communications for this communication flow.
        /// </summary>
        /// <param name="communicationFlowId">The identifier of the communication flow to resubscribe to.</param>
        /// <param name="personId">The identifier of the person to resubscribe.</param>
        public void ResubscribePersonToFlow( int communicationFlowId, int personId )
        {
            if ( Context is RockContext rockContext )
            {
                var communicationFlowInstanceRecipientService = new CommunicationFlowInstanceRecipientService( rockContext );
                var communicationFlowInstanceService = new CommunicationFlowInstanceService( rockContext );

                // Mark canceled communications as pending for this flow.
                var communicationFlowInstanceCommunicationRecipients = communicationFlowInstanceService
                    .GetByCommunicationFlow( communicationFlowId )
                    .SelectMany( cfi => cfi.CommunicationFlowInstanceCommunications.SelectMany( cfic => cfic.Communication.Recipients ) )
                    .Where( cr =>
                        cr.PersonAlias.PersonId == personId
                        && !cr.Communication.SendDateTime.HasValue
                        && cr.Communication.FutureSendDateTime.HasValue
                        && cr.Status == CommunicationRecipientStatus.Cancelled
                        && cr.StatusNote == PersonUnsubscribedFromCommunicationFlowStatusNote )
                    .ToList();

                foreach ( var communicationRecipient in communicationFlowInstanceCommunicationRecipients )
                {
                    communicationRecipient.Status = CommunicationRecipientStatus.Pending;
                    communicationRecipient.StatusNote = null;
                }

                foreach ( var communicationFlowInstanceRecipient in communicationFlowInstanceRecipientService
                    .Queryable()
                    .Where( r =>
                        r.CommunicationFlowInstance.CommunicationFlowId == communicationFlowId
                        && r.RecipientPersonAlias.PersonId == personId
                        && r.Status == CommunicationFlowInstanceRecipientStatus.Inactive
                        && r.InactiveReason.HasValue
                        && UnsubscribedInactiveReasons.Contains( r.InactiveReason.Value ) ) )
                {
                    ActivateInstanceRecipient( communicationFlowInstanceRecipient );
                }
            }
        }

        internal void ActivateInstanceRecipient( CommunicationFlowInstanceRecipient communicationFlowInstanceRecipient )
        {
            communicationFlowInstanceRecipient.Status = CommunicationFlowInstanceRecipientStatus.Active;
            communicationFlowInstanceRecipient.InactiveReason = null;
        }

        internal void DeactivateInstanceRecipient( CommunicationFlowInstanceRecipient communicationFlowInstanceRecipient, CommunicationFlowInstanceRecipientInactiveReason inactiveReason )
        {
            communicationFlowInstanceRecipient.Status = CommunicationFlowInstanceRecipientStatus.Inactive;
            communicationFlowInstanceRecipient.InactiveReason = inactiveReason;
        }

        /// <summary>
        /// Marks the messaging process as closed for the specified communication flow.
        /// </summary>
        /// <remarks>
        /// This indicates that the flow has no additional instances to create and that all 
        /// communications for existing instances have been scheduled or sent. The flow should 
        /// no longer be processed for messaging.
        /// </remarks>
        /// <param name="flow">
        /// The communication flow for which to close messaging. Must not be <c>null</c>.
        /// </param>
        internal void CloseMessaging( CommunicationFlow flow )
        {
            flow.IsMessagingClosed = true;
        }

        /// <summary>
        /// Marks the communication flow instance as having completed its messaging phase.
        /// </summary>
        /// <remarks>
        /// This indicates that all messages for the specified instance have been scheduled
        /// or sent and that the instance should not be processed for additional messaging.
        /// </remarks>
        /// <param name="instance">
        /// The communication flow instance to mark as completed. Must not be <c>null</c>.
        /// </param>
        internal void CompleteMessaging( CommunicationFlowInstance instance )
        {
            instance.IsMessagingCompleted = true;
        }

        /// <summary>
        /// Marks the conversion goal tracking as completed for the specified communication flow instance.
        /// </summary>
        /// <remarks>
        /// This indicates that conversion goal tracking has finished for the given instance 
        /// and should not be processed any further.
        /// </remarks>
        /// <param name="instance">
        /// The communication flow instance to mark as having completed conversion goal tracking. Must not be <c>null</c>.
        /// </param>
        internal void CompleteConversionGoalTracking( CommunicationFlowInstance instance )
        {
            instance.IsConversionGoalTrackingCompleted = true;
        }

        /// <summary>
        /// Marks the conversion goal tracking as closed for the specified communication flow.
        /// </summary>
        /// <remarks>
        /// This indicates that conversion goal tracking has finished for all instances of the flow 
        /// and should no longer be processed.
        /// </remarks>
        /// <param name="flow">
        /// The communication flow for which to close conversion goal tracking. Must not be <c>null</c>.
        /// </param>
        internal void CloseConversionGoalTracking( CommunicationFlow flow )
        {
            flow.IsConversionGoalTrackingClosed = true;
        }

        /// <summary>
        /// Automatically assigns the specified person to an appropriate communication flow instance.
        /// <para>
        ///     If a valid instance exists and no communications have been sent or scheduled, the person will be added to that instance.
        ///     If no suitable instance exists or the existing instance has already sent or scheduled communications,
        ///     a new instance will be created with the person as the initial recipient.
        /// </para>
        /// <para>
        ///     This method does not call <c>SaveChanges</c>; the caller is responsible for persisting any changes.
        /// </para>
        /// </summary>
        /// <param name="communicationFlowId">The identifier of the communication flow to assign the person to.</param>
        /// <param name="personId">The identifier of the person to assign.</param>
        /// <param name="currentPersonAliasId">The current person alias identifier to use when creating new records.</param>
        internal void AutoAssignPersonToOnDemandCommunicationFlowInstance( int communicationFlowId, int personId, int? currentPersonAliasId )
        {
            if ( Context is RockContext rockContext )
            {
                var personAliasService = new PersonAliasService( rockContext );
                var communicationFlowInstanceRecipientService = new CommunicationFlowInstanceRecipientService( rockContext );
                var communicationFlowInstanceService = new CommunicationFlowInstanceService( rockContext );
                var communicationFlowInstanceCommunicationService = new CommunicationFlowInstanceCommunicationService( rockContext );
                var communicationService = new CommunicationService( rockContext );
                var communicationRecipientService = new CommunicationRecipientService( rockContext );

                // Step 1: Get the person alias ID
                var personAliasId = personAliasService.GetPrimaryAliasQuery()
                    .Where( pa => pa.PersonId == personId )
                    .Select( pa => ( int? ) pa.Id )
                    .FirstOrDefault();

                if ( !personAliasId.HasValue )
                {
                    throw new InvalidOperationException( $"Primary alias not found for personId {personId}." );
                }

                // Step 2: Get the communication flow
                var communicationFlow = Get( communicationFlowId );

                if ( communicationFlow == null )
                {
                    throw new InvalidOperationException( $"Communication flow with Id {communicationFlowId} not found." );
                }

                if ( !communicationFlow.IsActive )
                {
                    throw new InvalidOperationException( $"Cannot auto-assign a person to Communication Flow instance (Id: {communicationFlowId}, Name: '{communicationFlow.Name}') because the flow is inactive." );
                }

                if ( communicationFlow.TriggerType != CommunicationFlowTriggerType.OnDemand )
                {
                    throw new InvalidOperationException( $"Cannot auto-assign a person to Communication Flow instance (Id: {communicationFlowId}, Name: '{communicationFlow.Name}') because the flow's trigger type is not On-Demand." );
                }

                // Step 3: Create a new flow instance if there is no instance for today
                var today = RockDateTime.Today;
                var instance = communicationFlowInstanceService
                    .Queryable()
                    .Where( cfi =>
                        cfi.CommunicationFlowId == communicationFlow.Id
                        && DbFunctions.DiffDays( cfi.StartDate, today ) == 0
                    )
                    .FirstOrDefault();

                if ( instance == null )
                {
                    instance = CreateNewOnDemandCommunicationFlowInstance( communicationFlowInstanceService, communicationFlow );
                    AddFlowInstanceRecipient( communicationFlowInstanceRecipientService, instance, personAliasId.Value );
                    return;
                }

                // Step 4: Exit early if the person is already in the instance 
                var isExistingRecipient = communicationFlowInstanceRecipientService
                    .Queryable()
                    .Any( r =>
                        r.CommunicationFlowInstanceId == instance.Id
                        && r.RecipientPersonAlias.PersonId == personId
                    );

                if ( isExistingRecipient )
                {
                    // Do not reactivate the person in case they unsubscribed from the flow, met the goal, etc.
                    //ActivateInstanceRecipient( existingInstanceRecipient );
                    return;
                }

                // Step 5: Add the person to the instance
                AddFlowInstanceRecipient( communicationFlowInstanceRecipientService, instance, personAliasId.Value );

                var instanceCommunicationsForMessageGroupings = communicationFlowInstanceCommunicationService
                    .Queryable()
                    .Include( cfic => cfic.Communication )
                    .Include( cfic => cfic.CommunicationFlowCommunication )
                    .Where( cfic => cfic.CommunicationFlowInstanceId == instance.Id ) // Get all communications for the instance
                    // Exclude instance communications where the person is already a recipient
                    .Where( cfic => !cfic.Communication.Recipients.Any( cr => cr.PersonAliasId == personAliasId.Value ) )
                    .ToList()
                    .GroupBy( cfic => cfic.CommunicationFlowCommunicationId )
                    .Select( g => new
                    {
                        CommunicationFlowCommunicationId = g.Key,
                        CommunicationFlowInstanceCommunications = g
                            .OrderBy( s => s.CommunicationFlowCommunication.Order )
                            .ToList()
                    } )
                    .ToList();

                // Step 6: Send copies of all previously sent communications to the person
                foreach ( var instanceCommunicationsForSentMessageGrouping in instanceCommunicationsForMessageGroupings
                    .Where( g => g.CommunicationFlowInstanceCommunications.Any( cfic => cfic.Communication.SendDateTime.HasValue ) ) )
                {
                    var instanceComunicationsForSentMessage = instanceCommunicationsForSentMessageGrouping.CommunicationFlowInstanceCommunications;

                    // If there is only one communication (original) in the group
                    // or if all other communications (copies) have been sent,
                    // then copy the original communication and send it to the person.
                    if ( instanceComunicationsForSentMessage.Count == 1
                         || instanceComunicationsForSentMessage.All( cfic => cfic.Communication.SendDateTime.HasValue ) )
                    {
                        var originalInstanceCommunication = instanceComunicationsForSentMessage
                            .OrderBy( cfic => cfic.Communication.SendDateTime.Value )
                            .First();
                        var sentCommunication = originalInstanceCommunication.Communication;
                        var copiedCommunication = communicationService
                            .Copy( sentCommunication.Id, currentPersonAliasId, new CommunicationService.CopyArgs
                            {
                                IsRecipientCopyingDisabled = true,
                                IsFutureSendDateCopyingDisabled = true
                            } );
                        copiedCommunication.Status = CommunicationStatus.Approved;

                        AddCommunicationRecipient( communicationRecipientService, copiedCommunication, personAliasId.Value );

                        // Link communication to flow instance.
                        AddCommunicationFlowInstanceCommunication(
                            communicationFlowInstanceCommunicationService,
                            instance,
                            originalInstanceCommunication.CommunicationFlowCommunicationId,
                            copiedCommunication
                        );
                    }
                    // Otherwise, add the person as a recipient to the newest unsent communication.
                    else
                    {
                        var newestUnsentInstanceCommunication = instanceComunicationsForSentMessage
                            .Where( cfic => !cfic.Communication.SendDateTime.HasValue )
                            .OrderByDescending( cfic => cfic.Communication.FutureSendDateTime ?? DateTime.MinValue )
                            .ThenByDescending( cfic => cfic.Communication.Id )
                            .FirstOrDefault();

                        if ( newestUnsentInstanceCommunication != null )
                        {
                            var scheduledCommunication = newestUnsentInstanceCommunication.Communication;
                            AddCommunicationRecipient(
                                communicationRecipientService,
                                scheduledCommunication,
                                personAliasId.Value
                            );
                        }
                    }
                }

                // Step 7: Add the person to all the newest scheduled communications.
                foreach ( var instanceCommunicationsForUnsentMessageGrouping in instanceCommunicationsForMessageGroupings
                    .Where( g => g.CommunicationFlowInstanceCommunications.All( cfic => !cfic.Communication.SendDateTime.HasValue ) ) )
                {
                    var newestUnsentInstanceCommunication = instanceCommunicationsForUnsentMessageGrouping
                        .CommunicationFlowInstanceCommunications
                        .OrderByDescending( cfic => cfic.Communication.FutureSendDateTime ?? DateTime.MinValue )
                        .ThenByDescending( cfic => cfic.Communication.Id )
                        .FirstOrDefault();

                    AddCommunicationRecipient(
                        communicationRecipientService,
                        newestUnsentInstanceCommunication.Communication,
                        personAliasId.Value
                    );
                }
            }
        }

        private int? GetMediumEntityTypeId( CommunicationType communicationType )
        {
            return communicationType == CommunicationType.Email
                ? EntityTypeCache.GetId( SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )
                : communicationType == CommunicationType.SMS
                    ? EntityTypeCache.GetId( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() )
                    : communicationType == CommunicationType.PushNotification
                        ? EntityTypeCache.GetId( SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() )
                        : ( int? ) null;
        }

        private CommunicationFlowInstance CreateNewOnDemandCommunicationFlowInstance( CommunicationFlowInstanceService communicationFlowInstanceService, CommunicationFlow communicationFlow )
        {
            var instance = new CommunicationFlowInstance
            {
                CommunicationFlow = communicationFlow,
                CommunicationFlowId = communicationFlow.Id,
                StartDate = RockDateTime.Today 
            };

            // Add to the service instead of the communicationFlow.CommunicationFlowInstances to avoid the DB read.
            communicationFlowInstanceService.Add( instance );

            return instance;
        }

        /// <summary>
        /// Adds a recipient to the specified communication flow instance using the provided service.
        /// </summary>
        /// <remarks>
        /// This method adds the recipient directly to the service to avoid a database read
        /// operation; it does not modify the <c>instance.CommunicationFlowInstanceRecipients</c> collection.
        /// </remarks>
        /// <param name="communicationFlowInstanceRecipientService"></param>
        /// <param name="instance"></param>
        /// <param name="personAliasId"></param>
        private void AddFlowInstanceRecipient( CommunicationFlowInstanceRecipientService communicationFlowInstanceRecipientService, CommunicationFlowInstance instance, int personAliasId )
        {
            var recipient = new CommunicationFlowInstanceRecipient
            {
                CommunicationFlowInstance = instance,
                CommunicationFlowInstanceId = instance.Id,
                RecipientPersonAliasId = personAliasId,
                Status = CommunicationFlowInstanceRecipientStatus.Active
            };

            // Add to the service instead of the instance.CommunicationFlowInstanceRecipients collection
            // to avoid the DB read.
            communicationFlowInstanceRecipientService.Add( recipient );
        }

        /// <summary>
        /// Adds a recipient to the specified communication using the provided service.
        /// </summary>
        /// <remarks>
        /// This method adds the recipient directly to the service to avoid a database read
        /// operation; it does not modify the <c>communication.Recipients</c> collection.
        /// </remarks>
        /// <param name="communicationRecipientService">The service used to manage communication recipients.</param>
        /// <param name="communication">The communication to which the recipient will be added.</param>
        /// <param name="personAliasId">The identifier of the person alias representing the recipient.</param>
        private void AddCommunicationRecipient( CommunicationRecipientService communicationRecipientService, Communication communication, int personAliasId )
        {
            // Add to the service instead of the communication.Recipients collection
            // to avoid the DB read.
            communicationRecipientService.Add( new CommunicationRecipient
            {
                Communication = communication,
                CommunicationId = communication.Id,
                MediumEntityTypeId = GetMediumEntityTypeId( communication.CommunicationType ),
                PersonAliasId = personAliasId,
                Status = CommunicationRecipientStatus.Pending
            } );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="communicationFlowInstanceCommunicationService"></param>
        /// <param name="instance"></param>
        /// <param name="flowCommunicationId"></param>
        /// <param name="communication"></param>
        private void AddCommunicationFlowInstanceCommunication(
            CommunicationFlowInstanceCommunicationService communicationFlowInstanceCommunicationService,
            CommunicationFlowInstance instance,
            int flowCommunicationId,
            Communication communication )
        {
            var instanceCommunication = new CommunicationFlowInstanceCommunication
            {
                CommunicationFlowInstance = instance,
                CommunicationFlowInstanceId = instance.Id,
                CommunicationFlowCommunicationId = flowCommunicationId,
                Communication = communication,
                CommunicationId = communication.Id
            };
            // Add to the service instead of the instance.CommunicationFlowInstanceCommunications collection
            // to avoid the DB read.
            communicationFlowInstanceCommunicationService.Add( instanceCommunication );
        }
    }
}
