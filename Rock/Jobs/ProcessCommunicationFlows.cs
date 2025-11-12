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
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Communication;
using Rock.Model;
using Rock.Observability;
using Rock.Tasks;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// This job will send communication flow instance communications and update conversions and unsubscribes.
    /// </summary>
    [DisplayName( "Process Communication Flows" )]
    [Description( "Automates Communication Flow Instances by processing unsubscribes, conversions, and sending the next round of eligible messages." )]

    #region Job Attributes

    [IntegerField(
        "SQL Command Timeout",
        Key = AttributeKey.CommandTimeoutSeconds,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. Leave blank to use the default for this job (300 seconds). ",
        IsRequired = false,
        DefaultIntegerValue = 300,
        Category = "General",
        Order = 0 )]

    #endregion

    public class ProcessCommunicationFlows : RockJob
    {
        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string CommandTimeoutSeconds = "CommandTimeoutSeconds";
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {            
            var startDateTime = RockDateTime.Now;

            List<int> activeFlowIds;
            using ( var rockContext = CreateRockContext() )
            {
                var now = RockDateTime.Now;
                // Get active flows that have instance messages to send
                // or conversions to process
                activeFlowIds = new CommunicationFlowService( rockContext )
                    .Queryable()
                    .Where( f =>
                        f.IsActive
                        && (
                            !f.IsMessagingClosed
                            || !f.IsConversionGoalTrackingClosed
                        )
                    )
                    .Select( f => f.Id )
                    .ToList();
            }

            var jobStatus = new JobStatus( this, activeFlowIds.Count );

            var flowNumber = 0;
            foreach ( var flowId in activeFlowIds )
            {
                try
                {
                    flowNumber++;
                    var flowUnsubscribeCount = 0;
                    var flowConversionCount = 0;
                    var flowSendCount = 0;
                    List<Model.Communication> sendImmediatelyQueue = null;

                    using ( var rockContext = CreateRockContext() )
                    {
                        CommunicationFlow communicationFlow;
                        using ( var activity = ObservabilityHelper.StartActivity( $"Retrieve full object graph for flow {flowId}" ) )
                        {
                            // A. Read the entire flow object graph (except Communications and CommunicationRecipients).
                            communicationFlow = new CommunicationFlowService( rockContext )
                                .Queryable()
                                .Include( f => f.Schedule )
                                .Include( f =>
                                    f.CommunicationFlowCommunications.Select( c =>
                                        c.CommunicationTemplate ) )
                                .Include( f =>
                                    f.CommunicationFlowInstances.Select( i =>
                                        i.CommunicationFlowInstanceCommunications ) )
                                .Include( f => f.CommunicationFlowInstances )
                                .FirstOrDefault( f => f.Id == flowId );
                        }

                        // Check again if flow is active in case it changed while this job was running.
                        if ( communicationFlow == null || !communicationFlow.IsActive )
                        {
                            return;
                        }

                        var interactionQuery = new InteractionService( rockContext ).Queryable();
                        var commChannelId = InteractionChannelCache.Get( SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() )?.Id ?? 0;

                        // B. Process the flow unsubscribes, conversions, next-send logic.
                        using ( ObservabilityHelper.StartActivity( "Process Flow Unsubscribes" ) )
                        {
                            ProcessCommunicationFlowInstanceRecipientsUnsubscribedFromCommunications(
                                rockContext,
                                communicationFlow.Id,
                                out flowUnsubscribeCount
                            );
                        }

                        var conversionGoalProcessor = GetConversionGoalProcessor( rockContext, communicationFlow.ConversionGoalType );

                        using ( ObservabilityHelper.StartActivity( "Process Flow Conversions" ) )
                        {
                            ProcessFlowConversions(
                                rockContext,
                                communicationFlow,
                                conversionGoalProcessor,
                                out flowConversionCount
                            );
                        }

                        using ( ObservabilityHelper.StartActivity( "Process Flow Next Communications" ) )
                        {
                            ProcessFlowNextCommunications(
                                rockContext,
                                communicationFlow,
                                conversionGoalProcessor,
                                out sendImmediatelyQueue,
                                out flowSendCount
                            );
                        }

                        // D. Send immediate Communications.
                        if ( sendImmediatelyQueue != null )
                        {
                            foreach ( var communication in sendImmediatelyQueue )
                            {
                                var msg = new ProcessSendCommunication.Message
                                {
                                    CommunicationId = communication.Id
                                };
                                msg.Send();
                            }
                        }

                        jobStatus.ReportFlowProgress( flowNumber, flowUnsubscribeCount, flowConversionCount, flowSendCount );
                    }
                }
                catch ( Exception ex )
                {
                    jobStatus.Fail( ex );
                    throw;
                }
            }

            jobStatus.Complete();
        }

        /// <summary>
        /// Creates a new RockContext configured for use by this service job.
        /// </summary>
        /// <returns></returns>
        private RockContext CreateRockContext()
        {
            var rockContext = new RockContext();
            rockContext.Database.CommandTimeout = GetAttributeValue( AttributeKey.CommandTimeoutSeconds ).AsIntegerOrNull() ?? 300;
            return rockContext;
        }

        private void ProcessCommunicationFlowInstanceRecipientsUnsubscribedFromCommunications( RockContext rockContext, int communicationFlowId, out int flowUnsubscribeCount )
        {
            var communicationFlowInstanceService = new CommunicationFlowInstanceService( rockContext );
            flowUnsubscribeCount = 0;

            // Get instance ids once
            List<int> instanceIds;
            instanceIds = communicationFlowInstanceService
                .GetByCommunicationFlow( communicationFlowId )
                .Where( cfi => cfi.CommunicationFlow.IsActive )
                .Select( cfi => cfi.Id )
                .ToList();

            if ( instanceIds.Count == 0 )
            {
                return;
            }

            const string sql = @"
UPDATE cfir
SET
    cfir.[UnsubscribeCommunicationRecipientId] = unsub.[Id],
    cfir.[Status] = @InactiveStatus,
    cfir.[InactiveReason] = @InactiveReason
FROM [dbo].[CommunicationFlowInstanceRecipient] AS cfir
CROSS APPLY (
    SELECT TOP (1) cr.[Id]
    FROM [dbo].[CommunicationRecipient] cr
    WHERE
        cr.[PersonAliasId] = cfir.[RecipientPersonAliasId]
        AND cr.[UnsubscribeDateTime] IS NOT NULL
        AND cr.[CommunicationId] IN (
            SELECT cfic.[CommunicationId]
            FROM [dbo].[CommunicationFlowInstanceCommunication] cfic
            WHERE cfic.[CommunicationFlowInstanceId] = @InstanceId
        )
    ORDER BY cr.[UnsubscribeDateTime] DESC, cr.[Id] DESC
) AS unsub
WHERE
    cfir.[CommunicationFlowInstanceId] = @InstanceId
    AND (
        cfir.[UnsubscribeCommunicationRecipientId] IS NULL
        OR cfir.[UnsubscribeCommunicationRecipientId] <> unsub.[Id]
    );";

            foreach ( var instanceId in instanceIds )
            {
                var affected = rockContext.Database.ExecuteSqlCommand(
                    sql,
                    new SqlParameter( "@InactiveStatus", ( int ) CommunicationFlowInstanceRecipientStatus.Inactive ),
                    new SqlParameter( "@InactiveReason", ( int ) CommunicationFlowInstanceRecipientInactiveReason.Unsubscribed ),
                    new SqlParameter( "@InstanceId", instanceId )
                );

                flowUnsubscribeCount += Math.Max( 0, affected );
            }
        }

        private IConversionGoalProcessor GetConversionGoalProcessor( RockContext rockContext, ConversionGoalType? conversionGoalType )
        {
            if ( !conversionGoalType.HasValue )
            {
                // No conversion goal set, so no conversion processing needed for this flow.
                return NullConversionGoalProcessor.Instance;
            }

            return ConversionGoalProcessorFactory.Create( rockContext, conversionGoalType.Value );
        }

        private void ProcessFlowConversions(
            RockContext rockContext,
            CommunicationFlow flow,
            IConversionGoalProcessor conversionGoalProcessor,
            out int flowConversionCount )
        {
            var communicationFlowService = new CommunicationFlowService( rockContext );
            flowConversionCount = 0;
            
            if ( !flow.ConversionGoalType.HasValue )
            {
                // Conversion goal tracking is not enabled for this communication flow so exit early.
                return;
            }

            if ( conversionGoalProcessor == NullConversionGoalProcessor.Instance )
            {
                // The conversion goal type is unknown, so skip conversion processing for this flow.
                Logger.LogWarning( $"Flow {flow.Id} cannot be processed. It has an unsupported conversion goal type {flow.ConversionGoalType.Value}." );
                return;
            }

            var isContextDirty = false;
            foreach ( var flowInstance in flow.CommunicationFlowInstances.Where( cfi => !cfi.IsConversionGoalTrackingCompleted ) )
            {
                using ( ObservabilityHelper.StartActivity( $"Processing Conversions For Instance {flowInstance.Id}" ) )
                {
                    if ( conversionGoalProcessor.AddConversions( flowInstance, out var addedConversions ) )
                    {
                        flowConversionCount += addedConversions?.Count ?? 0;
                        isContextDirty = true;
                    }

                    if ( flowInstance.StartDate.AddDays( flow.ConversionGoalTimeframeInDays.Value ) < RockDateTime.Now.Date )
                    {
                        communicationFlowService.CompleteConversionGoalTracking( flowInstance );
                    }
                }
            }

            if ( flow.CommunicationFlowInstances.All( cfi => cfi.IsConversionGoalTrackingCompleted ) )
            {
                communicationFlowService.CloseConversionGoalTracking( flow );
            }
            
            if ( isContextDirty )
            {
                rockContext.SaveChanges();
            }
        }

        private void ProcessFlowNextCommunications(
            RockContext rockContext,
            CommunicationFlow flow,
            IConversionGoalProcessor conversionGoalProcessor,
            out List<Model.Communication> sendImmediatelyQueue,
            out int flowSendCount
        )
        {
            var isContextDirty = false;
            ITriggerTypeProcessor triggerProcessor;
            using ( ObservabilityHelper.StartActivity( "Creating Trigger Type Processor" ) )
            {
                triggerProcessor = TriggerTypeProcessorFactory.Create( rockContext, flow );
            }

            flowSendCount = 0;

            if ( triggerProcessor == null )
            {
                Logger.LogWarning( $"Flow {flow.Id} cannot be processed. It has an unsupported trigger type {flow.TriggerType}." );
                sendImmediatelyQueue = null;
                return;
            }

            using ( ObservabilityHelper.StartActivity( "Ensuring Flow Has Latest Instance" ) )
            {
                triggerProcessor.EnsureFlowHasLatestInstance( flow );
            }

            InstanceCommunicationHelper instanceCommunicationHelper;
            using ( ObservabilityHelper.StartActivity( "Creating Instance Communication Helper" ) )
            {
                instanceCommunicationHelper = InstanceCommunicationHelper.Create(
                    new CommunicationFlowService( rockContext ),
                    new CommunicationService( rockContext ),
                    new PersonService( rockContext ),
                    new CommunicationFlowInstanceRecipientService( rockContext ),
                    conversionGoalProcessor,
                    new BulkInsertService( rockContext ),
                    new SaveChangesService( rockContext )
                );
            }

            sendImmediatelyQueue = new List<Model.Communication>();

            using ( ObservabilityHelper.StartActivity( $"Creating Communications For Flow {flow.Id}" ) )
            {
                foreach ( var instance in flow.CommunicationFlowInstances.Where( i => !i.IsMessagingCompleted ) )
                {
                    using ( ObservabilityHelper.StartActivity( $"Creating Communications For Flow Instance {instance.Id}" ) )
                    {
                        using ( ObservabilityHelper.StartActivity( "Pruning Recipients" ) )
                        {
                            ExitConditionHelper.PruneRecipients( rockContext, instance );
                        }

                        Model.Communication communication;
                        using ( ObservabilityHelper.StartActivity( "Creating Next Communication" ) )
                        {
                            instanceCommunicationHelper.CreateNextCommunication( instance, out communication );
                        }

                        if ( communication != null )
                        {
                            flowSendCount++;

                            // Decide whether it should be sent immediately.
                            if ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value <= RockDateTime.Now )
                            {
                                sendImmediatelyQueue.Add( communication );
                            }
                        }
                    }
                }

                isContextDirty |= triggerProcessor.MarkFlowMessagingClosedIfWarranted( flow );
            }

            if ( isContextDirty )
            {
                rockContext.SaveChanges();
            }
        }

        #region Conversion Goal Processors

        private interface IConversionGoalProcessor
        {
            bool AddConversions( CommunicationFlowInstance instance, out List<CommunicationFlowInstanceCommunicationConversion> addedConversions );
            IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIds, DateTime conversionStartDateTime, DateTime conversionEndDateTime );
        }

        private class ConversionInfo
        {
            public int PersonId { get; set; }

            public int PersonAliasId { get; set; }

            public DateTime ConversionDateTime { get; set; }
        }

        private static class ConversionGoalProcessorFactory
        {
            public static IConversionGoalProcessor Create( RockContext rockContext, ConversionGoalType conversionGoalType )
            {
                var communicationFlowInstanceService = new CommunicationFlowInstanceService( rockContext );
                var communicationFlowInstanceCommunicationConversionService = new CommunicationFlowInstanceCommunicationConversionService( rockContext );
                var bulkInsertService = new BulkInsertService( rockContext );

                switch ( conversionGoalType )
                {
                    case ConversionGoalType.CompletedForm: return new CompletedFormConversionGoalProcessor( communicationFlowInstanceService, communicationFlowInstanceCommunicationConversionService, bulkInsertService, new WorkflowService( rockContext ) );
                    case ConversionGoalType.EnteredDataView: return new EnteredDataViewConversionGoalProcessor( communicationFlowInstanceService, communicationFlowInstanceCommunicationConversionService, bulkInsertService, new DataViewService( rockContext ), new PersonService( rockContext ), new CommunicationFlowInstanceRecipientService( rockContext ) );
                    case ConversionGoalType.JoinedGroupType: return new JoinedGroupTypeConversionGoalProcessor( communicationFlowInstanceService, communicationFlowInstanceCommunicationConversionService, bulkInsertService, new GroupMemberService( rockContext ) );
                    case ConversionGoalType.JoinedGroup: return new JoinedGroupConversionGoalProcessor( communicationFlowInstanceService, communicationFlowInstanceCommunicationConversionService, bulkInsertService, new GroupMemberService( rockContext ) );
                    case ConversionGoalType.Registered: return new RegisteredConversionGoalProcessor( communicationFlowInstanceService, communicationFlowInstanceCommunicationConversionService, bulkInsertService, new RegistrationRegistrantService( rockContext ) );
                    case ConversionGoalType.TookStep: return new TookStepConversionGoalProcessor( communicationFlowInstanceService, communicationFlowInstanceCommunicationConversionService, bulkInsertService, new StepService( rockContext ) );
                    default: return NullConversionGoalProcessor.Instance;
                }
            }
        }

        private class NullConversionGoalProcessor : IConversionGoalProcessor
        {
            public static NullConversionGoalProcessor Instance { get; } = new NullConversionGoalProcessor();

            private NullConversionGoalProcessor()
            {
                // Only allow using the singleton Instance.
            }

            public bool AddConversions( CommunicationFlowInstance instance, out List<CommunicationFlowInstanceCommunicationConversion> addedConversions )
            {
                addedConversions = new List<CommunicationFlowInstanceCommunicationConversion>();
                return false;
            }

            public IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIds )
            {
                return Enumerable.Empty<ConversionInfo>().AsQueryable();
            }

            public IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIds, DateTime conversionStartDateTime, DateTime conversionEndDateTime )
            {
                return Enumerable.Empty<ConversionInfo>().AsQueryable();
            }
        }

        private abstract class ConversionGoalProcessorBase : IConversionGoalProcessor
        {
            private readonly BulkInsertService _bulkInsertService;
            private readonly CommunicationFlowInstanceCommunicationConversionService _communicationFlowInstanceCommunicationConversionService;
                            

            protected CommunicationFlowInstanceService CommunicationFlowInstanceService { get; }

            protected delegate ( int CommunicationFlowInstanceCommunicationId, int CommunicationRecipientId ) GetInstanceCommunicationForConversionDelegate( ConversionInfo conversionInfo );

            protected ConversionGoalProcessorBase(
                CommunicationFlowInstanceService communicationFlowInstanceService,
                CommunicationFlowInstanceCommunicationConversionService communicationFlowInstanceCommunicationConversionService,
                BulkInsertService bulkInsertService )
            {
                CommunicationFlowInstanceService = communicationFlowInstanceService ?? throw new ArgumentNullException( nameof( communicationFlowInstanceService ) );
                _communicationFlowInstanceCommunicationConversionService = communicationFlowInstanceCommunicationConversionService ?? throw new ArgumentNullException( nameof( communicationFlowInstanceCommunicationConversionService ) );
                _bulkInsertService = bulkInsertService ?? throw new ArgumentNullException( nameof( bulkInsertService ) );
            }

            public bool AddConversions( CommunicationFlowInstance instance, out List<CommunicationFlowInstanceCommunicationConversion> addedConversions )
            {
                List<PersonConversionInfo> peopleWhoReceivedInstanceCommunications;
                IQueryable<int> personIdWhoReceivedInstanceCommunicationQuery;
                using ( ObservabilityHelper.StartActivity( "Get RecipientInfos Who Received Comms" ) )
                {
                    var peopleWhoReceivedInstanceCommunicationsQuery = CommunicationFlowInstanceService
                        .Queryable()
                        .Where( cfi => cfi.Id == instance.Id )
                        .SelectMany( cfi => cfi.CommunicationFlowInstanceCommunications
                            .SelectMany( cfic => cfic.Communication.Recipients
                                .Where( cr => cr.Status == CommunicationRecipientStatus.Delivered || cr.Status == CommunicationRecipientStatus.Opened )
                                .Where( cr => cr.SendDateTime.HasValue || cfic.Communication.SendDateTime.HasValue )
                                .Select( cr => new PersonConversionInfo
                                {
                                    CommunicationFlowCommunicationOrder = cfic.CommunicationFlowCommunication.Order,
                                    CommunicationFlowInstanceCommunicationId = cfic.Id,
                                    CommunicationRecipientId = cr.Id,
                                    PersonId = cr.PersonAlias.PersonId,
                                    SentDateTime = cr.SendDateTime ?? cfic.Communication.SendDateTime.Value
                                } ) ) );

                    peopleWhoReceivedInstanceCommunications = peopleWhoReceivedInstanceCommunicationsQuery
                        .ToList();

                    personIdWhoReceivedInstanceCommunicationQuery = peopleWhoReceivedInstanceCommunicationsQuery
                        .Select( p => p.PersonId )
                        .Distinct();
                }

                var conversionWindowStart = instance.StartDate;
                var conversionWindowEnd = instance.StartDate
                    .AddDays( instance.CommunicationFlow.ConversionGoalTimeframeInDays.Value )
                    .Date;

                List<ConversionInfo> conversions;
                using ( ObservabilityHelper.StartActivity( "Get ConversionInfos" ) )
                {
                    conversions = GetConversionQuery(
                            instance,
                            personIdWhoReceivedInstanceCommunicationQuery,
                            conversionWindowStart,
                            conversionWindowEnd )
                        .ToList();
                }

                if ( !conversions.Any() )
                {
                    addedConversions = null;
                    return false;
                }

                var groupedPersonConversionInfo = peopleWhoReceivedInstanceCommunications
                    .GroupBy( p => p.PersonId )
                    .ToDictionary( g => g.Key, g => g
                        .OrderByDescending( r => r.CommunicationFlowCommunicationOrder )
                        .ThenByDescending( r => r.SentDateTime ).ToList() );

                // Build Conversion objects (not added to EF yet).
                List<ConversionCandidate> conversionCandidates;
                using ( ObservabilityHelper.StartActivity( "Convert ConversionInfos to ConversionCandidates" ) )
                {
                    conversionCandidates = conversions
                        .Select( c =>
                        {
                            if ( !groupedPersonConversionInfo.TryGetValue( c.PersonId, out var personConversionInfos ) )
                            {
                                return null; // Nulls are filtered after projection.
                            }

                            var personConversionInfo = personConversionInfos
                                .Where( p => p.SentDateTime <= c.ConversionDateTime
                                    || p.SentDateTime.Date == c.ConversionDateTime.Date ) // fallback to date for conversion goals like Took Step which only track date (w/o time)
                                .FirstOrDefault();

                            if ( personConversionInfo == null )
                            {
                                return null; // Nulls are filtered after projection.
                            }

                            var communicationFlowInstanceCommunication = instance.CommunicationFlowInstanceCommunications
                                .FirstOrDefault( cfic => cfic.Id == personConversionInfo.CommunicationFlowInstanceCommunicationId );

                            if ( communicationFlowInstanceCommunication == null )
                            {
                                return null; // Nulls are filtered after projection.
                            }

                            return new ConversionCandidate
                            {
                                PersonId = c.PersonId,
                                Conversion = new CommunicationFlowInstanceCommunicationConversion
                                {
                                    CommunicationFlowInstanceCommunicationId = personConversionInfo.CommunicationFlowInstanceCommunicationId,
                                    CommunicationRecipientId = personConversionInfo.CommunicationRecipientId,
                                    Date = c.ConversionDateTime,
                                    PersonAliasId = c.PersonAliasId
                                }
                            };
                        } )
                        .Where( h => h?.Conversion != null )
                        .ToList();
                }

                if ( !conversionCandidates.Any() )
                {
                    addedConversions = null;
                    return false;
                }

                var existingKeys = _communicationFlowInstanceCommunicationConversionService
                    .GetByCommunicationFlowInstance( instance.Id )
                    .Select( h => new
                    {
                        h.CommunicationFlowInstanceCommunicationId,
                        h.CommunicationRecipientId,
                        h.PersonAlias.PersonId
                        // h.Date - add this in the future if we ever need to track ALL conversions for a person per instance.
                    } )
                    .ToHashSet();

                List<ConversionCandidate> newConversions;
                using ( ObservabilityHelper.StartActivity( "Get New ConversionCandidates" ) )
                {
                    newConversions = conversionCandidates
                        .Where( h => !existingKeys.Contains( new
                        {
                            h.Conversion.CommunicationFlowInstanceCommunicationId,
                            h.Conversion.CommunicationRecipientId,
                            h.PersonId
                            // h.Date - add this in the future if we ever need to track ALL conversions for a person per instance.
                        } ) )
                        .ToList();
                }

                if ( !newConversions.Any() )
                {
                    addedConversions = null;
                    return false;
                }

                using ( ObservabilityHelper.StartActivity( "Insert Conversions" ) )
                {
                    var conversionsToInsert = newConversions.Select( c => c.Conversion ).ToList();
                    _bulkInsertService.BulkInsert( conversionsToInsert );
                    addedConversions = conversionsToInsert;
                }

                return addedConversions.Any();
            }

            public abstract IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIds, DateTime conversionStartDateTime, DateTime conversionEndDateTime );

            private class PersonConversionInfo
            {
                public int CommunicationFlowInstanceCommunicationId { get; set; }
                public int CommunicationRecipientId { get; set; }
                public int PersonId { get; set; }
                public DateTime SentDateTime { get; set; }
                public int CommunicationFlowCommunicationOrder { get; internal set; }
            }
        }

        private sealed class CompletedFormConversionGoalProcessor : ConversionGoalProcessorBase
        {
            private readonly WorkflowService _workflowService;


            public CompletedFormConversionGoalProcessor(
                CommunicationFlowInstanceService communicationFlowInstanceService,
                CommunicationFlowInstanceCommunicationConversionService communicationFlowInstanceCommunicationConversionService,
                BulkInsertService bulkInsertService,
                WorkflowService workflowService )
                : base( communicationFlowInstanceService, communicationFlowInstanceCommunicationConversionService, bulkInsertService )
            {
                _workflowService = workflowService ?? throw new ArgumentNullException( nameof( workflowService ) );
            }

            public override IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIdQuery, DateTime conversionGoalStartDate, DateTime conversionGoalEndDate )
            {
                var workflowTypeGuid = communicationFlowInstance.CommunicationFlow.GetConversionGoalSettings()?.CompletedFormSettings?.WorkflowTypeGuid;
                var workflowTypeId = workflowTypeGuid.HasValue ? WorkflowTypeCache.GetId( workflowTypeGuid.Value ) : null;

                if ( !workflowTypeId.HasValue )
                {
                    return Enumerable.Empty<ConversionInfo>().AsQueryable();
                }

                return _workflowService
                    .Queryable()
                    .Where( w =>
                        w.WorkflowType.Id == workflowTypeId.Value
                        && w.CompletedDateTime.HasValue
                        && w.InitiatorPersonAlias != null
                        && personIdQuery.Contains( w.InitiatorPersonAlias.PersonId )
                        && conversionGoalStartDate <= w.CompletedDateTime.Value && w.CompletedDateTime.Value < conversionGoalEndDate
                    )
                    .Select( p => new ConversionInfo
                    {
                        PersonAliasId = p.InitiatorPersonAlias.Id,
                        PersonId = p.InitiatorPersonAlias.PersonId,
                        ConversionDateTime = p.CompletedDateTime.Value
                    } );
            }
        }

        private sealed class EnteredDataViewConversionGoalProcessor : ConversionGoalProcessorBase
        {
            private readonly DataViewService _dataViewService;
            private readonly PersonService _personService;
            private readonly CommunicationFlowInstanceRecipientService _communicationFlowInstanceRecipientService;

            public EnteredDataViewConversionGoalProcessor(
                CommunicationFlowInstanceService communicationFlowInstanceService, 
                CommunicationFlowInstanceCommunicationConversionService communicationFlowInstanceCommunicationConversionService,
                BulkInsertService bulkInsertService,
                DataViewService dataViewService, PersonService personService, CommunicationFlowInstanceRecipientService communicationFlowInstanceRecipientService )
                : base( communicationFlowInstanceService, communicationFlowInstanceCommunicationConversionService, bulkInsertService )
            {
                _dataViewService = dataViewService ?? throw new ArgumentNullException( nameof( dataViewService ) );
                _personService = personService ?? throw new ArgumentNullException( nameof( personService ) );
                _communicationFlowInstanceRecipientService = communicationFlowInstanceRecipientService ?? throw new ArgumentNullException( nameof( communicationFlowInstanceRecipientService ) );
            }

            public override IQueryable<ConversionInfo> GetConversionQuery(CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIdQuery, DateTime conversionGoalStartDate, DateTime conversionGoalEndDate )
            {
                /*
                    7/23/2025 - JMH

                    DataViews don't have a reliable DateTime to determine when a person entered the DataView.
                    At the start of the CommunicationFlowInstance, we will check if the instance recipients are already in the DataView,
                    in which case they will be marked as WasConversionGoalPreMet. Then when we process the DataView,
                    we will check if the recipient was not already in the DataView at the start of the instance,
                    and if they are in the DataView now, we will consider that a conversion.
                 */

                var personIdsAlreadyInDataViewPriorToFlowQuery = _communicationFlowInstanceRecipientService
                    .Queryable()
                    .Where( cfir =>
                        cfir.CommunicationFlowInstanceId == communicationFlowInstance.Id 
                        && cfir.WasConversionGoalPreMet )
                    .Select( cfir => cfir.RecipientPersonAlias.PersonId )
                    .Distinct();

                var dataViewGuid = communicationFlowInstance.CommunicationFlow.GetConversionGoalSettings()?.EnteredDataViewSettings?.DataViewGuid;

                if ( !dataViewGuid.HasValue )
                {
                    return null;
                }

                var dataView = _dataViewService.Get( dataViewGuid.Value );

                // Only process the data view if it exists and has a last run date that is within the flow instance's start and end dates.
                if ( dataView == null )
                {
                    return null;
                }

                return _personService
                    .GetQueryUsingDataView( dataView )
                    .Where( p =>
                        p.PrimaryAliasId.HasValue
                        && personIdQuery.Contains( p.Id )
                        // Ignore people who were already in the dataview prior to the first flow instance communication.
                        && !personIdsAlreadyInDataViewPriorToFlowQuery.Contains( p.Id )
                        && dataView.LastRunDateTime.HasValue
                        && conversionGoalStartDate <= dataView.LastRunDateTime.Value && dataView.LastRunDateTime.Value < conversionGoalEndDate
                    )
                    .Select( p => new ConversionInfo
                    {
                        PersonId = p.Id,
                        PersonAliasId = p.PrimaryAliasId.Value,
                        ConversionDateTime = dataView.LastRunDateTime.Value
                    } );
            }
        }

        private sealed class JoinedGroupConversionGoalProcessor : ConversionGoalProcessorBase
        {
            private readonly GroupMemberService _groupMemberService;

            public JoinedGroupConversionGoalProcessor(
                CommunicationFlowInstanceService communicationFlowInstanceService, 
                CommunicationFlowInstanceCommunicationConversionService communicationFlowInstanceCommunicationConversionService,
                BulkInsertService bulkInsertService,
                GroupMemberService groupMemberService )
                : base( communicationFlowInstanceService, communicationFlowInstanceCommunicationConversionService, bulkInsertService )
            {
                _groupMemberService = groupMemberService ?? throw new ArgumentNullException( nameof( groupMemberService ) );
            }

            public override IQueryable<ConversionInfo> GetConversionQuery(CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIdQuery, DateTime conversionGoalStartDate, DateTime conversionGoalEndDate )
            {
                var groupGuid = communicationFlowInstance.CommunicationFlow.GetConversionGoalSettings()?.JoinedGroupSettings?.GroupGuid;
                var groupId = groupGuid.HasValue ? GroupCache.GetId( groupGuid.Value ) : null;

                if ( !groupId.HasValue )
                {
                    return null;
                }

                return _groupMemberService
                    .Queryable()
                    .Where( gm =>
                        gm.Group.Id == groupId.Value
                        && gm.DateTimeAdded.HasValue
                        && gm.Person.PrimaryAliasId.HasValue
                        && personIdQuery.Contains( gm.PersonId )
                        && conversionGoalStartDate <= gm.DateTimeAdded.Value && gm.DateTimeAdded.Value < conversionGoalEndDate
                    )
                    .Select( gm => new ConversionInfo
                    {
                        PersonId = gm.PersonId,
                        PersonAliasId = gm.Person.PrimaryAliasId.Value,
                        ConversionDateTime = gm.DateTimeAdded.Value
                    } );
            }
        }

        private sealed class JoinedGroupTypeConversionGoalProcessor : ConversionGoalProcessorBase
        {
            private readonly GroupMemberService _groupMemberService;

            public JoinedGroupTypeConversionGoalProcessor(
                CommunicationFlowInstanceService communicationFlowInstanceService, 
                CommunicationFlowInstanceCommunicationConversionService communicationFlowInstanceCommunicationConversionService,
                BulkInsertService bulkInsertService,
                GroupMemberService groupMemberService )
                : base(communicationFlowInstanceService, communicationFlowInstanceCommunicationConversionService, bulkInsertService )
            {
                _groupMemberService = groupMemberService ?? throw new ArgumentNullException( nameof( groupMemberService ) );
            }

            public override IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIdQuery, DateTime conversionGoalStartDate, DateTime conversionGoalEndDate )
            {
                var groupTypeGuid = communicationFlowInstance.CommunicationFlow.GetConversionGoalSettings()?.JoinedGroupTypeSettings?.GroupTypeGuid;
                var groupTypeId = groupTypeGuid.HasValue ? GroupTypeCache.GetId( groupTypeGuid.Value ) : null;

                if ( !groupTypeId.HasValue )
                {
                    return null;
                }

                return _groupMemberService
                    .Queryable()
                    .Where( gm =>
                        gm.Group.GroupType.Id == groupTypeId.Value
                        && gm.DateTimeAdded.HasValue
                        && gm.Person.PrimaryAliasId.HasValue
                        && personIdQuery.Contains( gm.PersonId )
                        && conversionGoalStartDate <= gm.DateTimeAdded.Value && gm.DateTimeAdded.Value < conversionGoalEndDate
                    )
                    .Select( gm => new ConversionInfo
                    {
                        PersonId = gm.PersonId,
                        PersonAliasId = gm.Person.PrimaryAliasId.Value,
                        ConversionDateTime = gm.DateTimeAdded.Value
                    } );
            }
        }
        
        private sealed class RegisteredConversionGoalProcessor : ConversionGoalProcessorBase
        {
            private readonly RegistrationRegistrantService _registrationRegistrantService;

            public RegisteredConversionGoalProcessor(
                CommunicationFlowInstanceService communicationFlowInstanceService, 
                CommunicationFlowInstanceCommunicationConversionService communicationFlowInstanceCommunicationConversionService,
                BulkInsertService bulkInsertService,
                RegistrationRegistrantService registrationRegistrantService )
                : base(communicationFlowInstanceService, communicationFlowInstanceCommunicationConversionService, bulkInsertService )
            {
                _registrationRegistrantService = registrationRegistrantService ?? throw new ArgumentNullException( nameof( registrationRegistrantService ) );
            }

            public override IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIdQuery, DateTime conversionGoalStartDate, DateTime conversionGoalEndDate )
            {
                var registrationInstanceGuid = communicationFlowInstance.CommunicationFlow.GetConversionGoalSettings()?.RegisteredSettings?.RegistrationInstanceGuid;
                
                if ( !registrationInstanceGuid.HasValue )
                {
                    return null;
                }

                IQueryable<ConversionInfo> query;

                using ( ObservabilityHelper.StartActivity( "Create Conversion Query" ) )
                {
                    query = _registrationRegistrantService
                        .Queryable()
                        .Where( rr =>
                            rr.Registration.RegistrationInstance.Guid == registrationInstanceGuid.Value
                            && rr.CreatedDateTime.HasValue
                            && rr.PersonAliasId.HasValue
                            && personIdQuery.Contains( rr.PersonAlias.PersonId )
                            && conversionGoalStartDate <= rr.CreatedDateTime.Value && rr.CreatedDateTime.Value < conversionGoalEndDate
                        )
                        .Select( rr => new ConversionInfo
                        {
                            PersonId = rr.PersonAlias.PersonId,
                            PersonAliasId = rr.PersonAliasId.Value,
                            ConversionDateTime = rr.CreatedDateTime.Value
                        } );
                }

                return query;
            }
        }

        private sealed class TookStepConversionGoalProcessor : ConversionGoalProcessorBase
        {
            private readonly StepService _stepService;

            public TookStepConversionGoalProcessor(
                CommunicationFlowInstanceService communicationFlowInstanceService, 
                CommunicationFlowInstanceCommunicationConversionService communicationFlowInstanceCommunicationConversionService,
                BulkInsertService bulkInsertService,
                StepService stepService )
                : base(communicationFlowInstanceService, communicationFlowInstanceCommunicationConversionService, bulkInsertService )
            {
                _stepService = stepService ?? throw new ArgumentNullException( nameof( stepService ) );
            }

            public override IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIdQuery, DateTime conversionStartDateTime, DateTime conversionEndDateTime )
            {
                var stepTypeGuid = communicationFlowInstance.CommunicationFlow.GetConversionGoalSettings()?.TookStepSettings?.StepTypeGuid;
                var stepTypeId = stepTypeGuid.HasValue ? StepTypeCache.GetId( stepTypeGuid.Value ) : null;

                if ( !stepTypeId.HasValue )
                {
                    return null;
                }
                
                // The CompletedDateTime doesn't actually store the time
                // so check if the conversion occurred at any time between the dates.
                conversionStartDateTime = conversionStartDateTime.StartOfDay();
                conversionEndDateTime = conversionEndDateTime.AddDays( 1 ).StartOfDay();

                return _stepService
                    .Queryable()
                    .Where( s =>
                        s.StepTypeId == stepTypeId.Value
                        && s.CompletedDateTime.HasValue
                        && s.StepStatus.IsCompleteStatus
                        && conversionStartDateTime <= s.CompletedDateTime && s.CompletedDateTime < conversionEndDateTime
                        && personIdQuery.Contains( s.PersonAlias.PersonId )
                    )
                    .Select( s => new ConversionInfo
                    {
                        PersonId = s.PersonAlias.PersonId,
                        PersonAliasId = s.PersonAliasId,
                        // The CompletedDateTime doesn't actually store the time
                        // so project this to the end of the day for conversion goal purposes.
                        ConversionDateTime = s.CompletedDateTime.Value
                    } );
            }
        }

        #endregion Conversion Goal Processors

        #region Trigger Type Processors

        private interface ITriggerTypeProcessor
        {
            /// <summary>
            /// Ensures an initial instance exists if warranted.
            /// </summary>
            /// <returns>Returns <c>true</c> when the context has been modified.</returns>
            void EnsureFlowHasLatestInstance( CommunicationFlow flow );

            /// <summary>
            /// Marks the flow complete if warranted.
            /// </summary>
            /// <param name="flow">The flow to mark as complete if needed.</param>
            /// <returns>Returns <c>true</c> when the context has been modified.</returns>
            bool MarkFlowMessagingClosedIfWarranted( CommunicationFlow flow );
        }

        private class SaveChangesService
        {
            private readonly RockContext _rockContext;

            public SaveChangesService( RockContext rockContext )
            {
                _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            }

            public void SaveChanges()
            {
                _rockContext.SaveChanges();
            }
        }

        private static class TriggerTypeProcessorFactory
        {
            public static ITriggerTypeProcessor Create( RockContext rockContext, CommunicationFlow flow )
            {
                switch ( flow.TriggerType )
                {
                    case CommunicationFlowTriggerType.OneTime:   return new OneTimeTriggerTypeProcessor( new SaveChangesService( rockContext ), new CommunicationFlowService( rockContext ) );
                    case CommunicationFlowTriggerType.Recurring: return new RecurringTriggerTypeProcessor( new SaveChangesService( rockContext ), new CommunicationFlowService( rockContext ) );
                    case CommunicationFlowTriggerType.OnDemand:  return new OnDemandTriggerTypeProcessor();
                    default: return null;
                }
            }
        }

        private sealed class OnDemandTriggerTypeProcessor : ITriggerTypeProcessor
        {
            /// <summary>
            /// This job cannot create On-Demand flows so this method always returns <see langword="false" />.
            /// </summary>
            /// <returns><see langword="false" /></returns>
            public void EnsureFlowHasLatestInstance( CommunicationFlow flow ) { }

            /// <summary>
            /// On-Demand flows are never automatically marked complete by this job.
            /// </summary>
            /// <returns><see langword="false" /></returns>
            public bool MarkFlowMessagingClosedIfWarranted( CommunicationFlow flow ) => false;
        }

        private sealed class OneTimeTriggerTypeProcessor : ITriggerTypeProcessor
        {
            private readonly SaveChangesService _saveChangesService;
            private readonly CommunicationFlowService _communicationFlowService;

            public OneTimeTriggerTypeProcessor( SaveChangesService saveChangesService, CommunicationFlowService communicationFlowService )
            {
                _saveChangesService = saveChangesService ?? throw new ArgumentNullException( nameof( saveChangesService ) ); 
                _communicationFlowService = communicationFlowService ?? throw new ArgumentNullException( nameof( communicationFlowService ) );
            }

            public void EnsureFlowHasLatestInstance( CommunicationFlow flow )
            {
                if ( flow.CommunicationFlowInstances.Any() )
                {
                    // There is already a one-time flow instance so return.
                    return;
                }

                // No instances yet so try to create one.
                var schedule = flow.Schedule;
                if ( schedule == null )
                {
                    // The schedule has not been set so an instance can not be created.
                    return;
                }

                var firstStartDateTime = schedule.FirstStartDateTime;
                if ( !firstStartDateTime.HasValue )
                {
                    // The schedule does not have a first start date so an instance can not be created.
                    return;
                }

                var instance = new CommunicationFlowInstance
                {
                    CommunicationFlowId = flow.Id,
                    // Need to set parent object relationship manually since AutoDetectChanges is off,
                    // and adding ID above will not set the reference automatically.
                    CommunicationFlow = flow,
                    StartDate = firstStartDateTime.Value
                };

                flow.CommunicationFlowInstances.Add( instance );
                _saveChangesService.SaveChanges();
            }

            public bool MarkFlowMessagingClosedIfWarranted( CommunicationFlow flow )
            {
                var isContextDirty = false;

                if ( flow.CommunicationFlowInstances.Any()
                     && flow.CommunicationFlowInstances.All( i => i.IsMessagingCompleted ) )
                {
                    // There is already a one-time flow instance and it has completed.
                    _communicationFlowService.CloseMessaging( flow );
                    isContextDirty = true;
                    return isContextDirty;
                }

                return isContextDirty;
            }
        }

        private sealed class RecurringTriggerTypeProcessor : ITriggerTypeProcessor
        {
            private readonly SaveChangesService _saveChangesService;
            private readonly CommunicationFlowService _communicationFlowService;

            public RecurringTriggerTypeProcessor( SaveChangesService saveChangesService, CommunicationFlowService communicationFlowService )
            {
                _saveChangesService = saveChangesService ?? throw new ArgumentNullException( nameof( saveChangesService ) );
                _communicationFlowService = communicationFlowService ?? throw new ArgumentNullException( nameof( communicationFlowService ) );
            }

            public void EnsureFlowHasLatestInstance( CommunicationFlow flow )
            {
                var schedule = flow.Schedule;
                if ( schedule == null )
                {
                    return;
                }

                var lastInstanceStartDateTime = flow.CommunicationFlowInstances
                    .OrderByDescending( i => i.StartDate )
                    .Select( i => ( DateTime? ) i.StartDate ) // cast as DateTime? so lastStart can be null if there are no instances.
                    .FirstOrDefault();

                var nextInstanceStartDateTime = !lastInstanceStartDateTime.HasValue
                    ? schedule.FirstStartDateTime
                    : schedule.GetNextStartDateTime( lastInstanceStartDateTime.Value.AddDays( 1 ) );

                // Find the next future start datetime.
                DateTime? temp = null;

                while ( nextInstanceStartDateTime != temp // prevent infinite loops
                    && nextInstanceStartDateTime.HasValue
                    && nextInstanceStartDateTime.Value < RockDateTime.Now.Date )
                {
                    temp = nextInstanceStartDateTime;
                    nextInstanceStartDateTime = schedule.GetNextStartDateTime( nextInstanceStartDateTime.Value.AddDays( 1 ) );
                }

                if ( !nextInstanceStartDateTime.HasValue )
                {
                    // The schedule is completed; no more instances to create.
                    return;
                }

                if ( RockDateTime.Now.Date != nextInstanceStartDateTime.Value.Date )
                {
                    // The next instance start date isn't today,
                    // so let the next job execution try to process the flow.
                    return;
                }

                var instance = new CommunicationFlowInstance
                {
                    // Need to set parent relationship here since AutoDetectChanges is off,
                    // and adding below will not set these automatically.
                    CommunicationFlowId = flow.Id,
                    CommunicationFlow = flow,
                    StartDate = nextInstanceStartDateTime.Value.Date
                };

                flow.CommunicationFlowInstances.Add( instance );
                _saveChangesService.SaveChanges();
            }

            public bool MarkFlowMessagingClosedIfWarranted( CommunicationFlow flow )
            {
                var isContextDirty = false;

                var schedule = flow.Schedule;
                if ( schedule == null )
                {
                    // Invalid flow; schedule is missing.
                    return isContextDirty;
                }

                var lastInstanceStartDateTime = flow.CommunicationFlowInstances
                    .OrderByDescending( i => i.StartDate )
                    .Select( i => ( DateTime? ) i.StartDate ) // cast as DateTime? so lastStart can be null if there are no instances.
                    .FirstOrDefault();

                var nextInstanceStartDateTime = !lastInstanceStartDateTime.HasValue
                    ? schedule.FirstStartDateTime
                    : schedule.GetNextStartDateTime( lastInstanceStartDateTime.Value.AddDays( 1 ) );

                // Find the next future start datetime.
                DateTime? temp = null;

                while ( nextInstanceStartDateTime != temp // prevent infinite loops
                    && nextInstanceStartDateTime.HasValue
                    && nextInstanceStartDateTime.Value < RockDateTime.Now.Date )
                {
                    temp = nextInstanceStartDateTime;
                    nextInstanceStartDateTime = schedule.GetNextStartDateTime( nextInstanceStartDateTime.Value.AddDays( 1 ) );
                }

                if ( !nextInstanceStartDateTime.HasValue
                     && flow.CommunicationFlowInstances.Any()
                     && flow.CommunicationFlowInstances.All( i => i.IsMessagingCompleted ) )
                {
                    // There is at least one flow instance,
                    // no more instances to create,
                    // and all instances have completed
                    // so mark the flow messaging as closed.
                    _communicationFlowService.CloseMessaging( flow );
                    isContextDirty = true;
                    return isContextDirty;
                }

                return isContextDirty;
            }
        }

        #endregion Trigger Type Processors

        #region Helper Types

        private class BulkInsertService
        {
            private readonly RockContext _rockContext;

            public BulkInsertService( RockContext rockContext )
            {
                _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            }

            public void BulkInsert<T>( IEnumerable<T> records ) where T : class, IEntity
            { 
                foreach ( var batch in Batch( records ) )
                {
                    _rockContext.BulkInsert( batch );
                }
            }

            private static IEnumerable<List<T>> Batch<T>( IEnumerable<T> source, int size = 4000 )
            {
                var bucket = new List<T>( size );
                foreach ( var item in source )
                {
                    bucket.Add( item );

                    if ( bucket.Count == size )
                    {
                        yield return bucket;
                        bucket = new List<T>( size );
                    }
                }
                if ( bucket.Count > 0 )
                {
                    yield return bucket;
                }
            }
        }

        private class BulkUpdateService
        {
            private readonly RockContext _rockContext;

            public BulkUpdateService( RockContext rockContext )
            {
                _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            }

            /// <summary>
            /// Performs a bulk update on entities of type <typeparamref name="T"/> in batches,
            /// using the specified filter and update factory expression.
            /// </summary>
            /// <typeparam name="T">
            /// The entity type, which must implement <see cref="IEntity"/>.
            /// </typeparam>
            /// <param name="query">
            /// The base query that identifies the candidate entities to evaluate for update.
            /// </param>
            /// <param name="whereRecordsHaveNotBeenUpdated">
            /// A predicate expression that defines which records from <paramref name="query"/>
            /// should be included for updating. This ensures only records that have not already
            /// been updated are considered.
            /// </param>
            /// <param name="updateFactory">
            /// An expression that defines the update operation to apply to matching records.
            /// </param>
            /// <param name="batchSize">
            /// The maximum number of records to process in each batch. Defaults to 4000.
            /// </param>
            /// <returns>
            /// The total number of records updated across all processed batches.
            /// </returns>
            /// <remarks>
            /// This method limits each batch to 4000 records to avoid generating an
            /// excessively large <c>WHERE IN</c> clause, which could otherwise exceed
            /// SQL Server's query plan limits. The update process will continue iterating
            /// until no additional records meet the update condition.
            /// </remarks>
            public int BulkUpdate<T>(
                IQueryable<T> query,
                Expression<Func<T, bool>> whereRecordsHaveNotBeenUpdated,
                Expression<Func<T, T>> updateFactory,
                int batchSize = 4000 ) where T : class, IEntity
            {
                var totalUpdatedCount = 0;

                while ( true )
                {
                    var idToUpdateQuery = query
                        .Where( whereRecordsHaveNotBeenUpdated )
                        .OrderBy( e => e.Id )
                        .Select( e => e.Id )
                        .Take( batchSize );

                    var batchedUpdateQuery = _rockContext
                        .Set<T>()
                        .Where( e => idToUpdateQuery.Contains( e.Id ) );

                    var batchUpdatedCount = _rockContext.BulkUpdate( batchedUpdateQuery, updateFactory );

                    if ( batchUpdatedCount <= 0 )
                    {
                        break;
                    }

                    totalUpdatedCount += batchUpdatedCount;
                }

                return totalUpdatedCount;
            }
        }

        private class RecipientInfo
        {
            public int CommunicationRecipientId { get; set; }
            public int? PersonId { get; set; }
            public CommunicationRecipientStatus Status { get; set; }
            public DateTime? UnsubscribeDateTime { get; set; }
            public DateTime? OpenedDateTime { get; set; }
            public DateTime? ClickedDateTime { get; set; }
            public int CommunicationFlowInstanceId { get; set; }
            public int CommunicationFlowInstanceCommunicationId { get; set; }
            public CommunicationFlowInstanceCommunication CommunicationFlowInstanceCommunication { get; set; }
            public int CommunicationId { get; set; }
            public DateTime? CommunicationSendDateTime { get; set; }
            public DateTime? CommunicationRecipientSendDateTime { get; set; }
            public int CommunicationFlowCommunicationOrder { get; set; }
        }

        private static class ExitConditionHelper
        {
            public static void PruneRecipients( RockContext rockContext, CommunicationFlowInstance communicationFlowInstance )
            {
                var communicationFlowService = new CommunicationFlowService( rockContext );
                var communicationFlowInstanceRecipientService = new CommunicationFlowInstanceRecipientService( rockContext );
                var communicationFlowInstanceCommunicationService = new CommunicationFlowInstanceCommunicationService( rockContext );
                var communicationFlowInstanceCommunicationConversionService = new CommunicationFlowInstanceCommunicationConversionService( rockContext );
                var communicationService = new CommunicationService( rockContext );
                var bulkUpdateService = new BulkUpdateService( rockContext );

                var activeInstanceRecipientQuery = communicationFlowInstanceRecipientService
                    .GetByCommunicationFlowInstance( communicationFlowInstance.Id )
                    .Where( r => r.Status == CommunicationFlowInstanceRecipientStatus.Active );

                var activeInstanceRecipientPersonIdQuery = activeInstanceRecipientQuery
                    .Select( cfir => cfir.RecipientPersonAlias.PersonId )
                    .Distinct();

                var communicationFlow = communicationFlowInstance?.CommunicationFlow;

                if ( communicationFlow == null )
                {
                    // Unable to check exit condition to determine if recipients should be pruned.
                    return;
                }

                // Ensure flow-unsubscribed recipients are pruned from this flow instance.
                var unsubscribedFromFlowPersonIdQuery = communicationFlowInstanceRecipientService
                    .GetByCommunicationFlow( communicationFlow.Id )
                    .WhereUnsubscribedFromCommunicationFlow()
                    .Where( r => r.CommunicationFlowInstanceId != communicationFlowInstance.Id && r.RecipientPersonAlias != null )
                    .Select( r => r.RecipientPersonAlias.PersonId )
                    .Distinct();

                bulkUpdateService.BulkUpdate(
                    activeInstanceRecipientQuery.Where( cfir => unsubscribedFromFlowPersonIdQuery.Contains( cfir.RecipientPersonAlias.PersonId ) ),
                    cfir => !(
                        cfir.Status == CommunicationFlowInstanceRecipientStatus.Inactive
                        && cfir.InactiveReason == CommunicationFlowInstanceRecipientInactiveReason.UnsubscribedFromFlow
                    ),
                    updates => new CommunicationFlowInstanceRecipient
                    {
                        Status = CommunicationFlowInstanceRecipientStatus.Inactive,
                        InactiveReason = CommunicationFlowInstanceRecipientInactiveReason.UnsubscribedFromFlow
                    }
                );

                switch ( communicationFlow.ExitConditionType )
                {
                    case ExitConditionType.LastMessageSent:
                        {
                            // If the instance is completed, then all active recipients should be marked as inactive.
                            if ( communicationFlowInstance.IsMessagingCompleted )
                            {
                                bulkUpdateService.BulkUpdate(
                                    activeInstanceRecipientQuery,
                                    cfir => !(
                                        cfir.Status == CommunicationFlowInstanceRecipientStatus.Inactive
                                        && cfir.InactiveReason == CommunicationFlowInstanceRecipientInactiveReason.LastCommunicationSent
                                    ),
                                    updates => new CommunicationFlowInstanceRecipient
                                    {
                                        Status = CommunicationFlowInstanceRecipientStatus.Inactive,
                                        InactiveReason = CommunicationFlowInstanceRecipientInactiveReason.LastCommunicationSent
                                    }
                                );
                            }

                            break;
                        }

                    case ExitConditionType.AnyEmailOpened:
                        {
                            var communicationIdQuery = communicationFlowInstanceCommunicationService
                                .GetByCommunicationFlowInstance( communicationFlowInstance.Id )
                                .Select( cfic => cfic.CommunicationId )
                                .Distinct();

                            var openedCommunicationPersonIdQuery = communicationService
                                .GetOpenedInteractions( communicationIdQuery, activeInstanceRecipientPersonIdQuery )
                                .Select( i => i.PersonAlias.PersonId );

                            bulkUpdateService.BulkUpdate(
                                activeInstanceRecipientQuery.Where( cfir => openedCommunicationPersonIdQuery.Contains( cfir.RecipientPersonAlias.PersonId ) ),
                                cfir => !(
                                    cfir.Status == CommunicationFlowInstanceRecipientStatus.Inactive
                                    && cfir.InactiveReason == CommunicationFlowInstanceRecipientInactiveReason.OpenedCommunication
                                ),
                                updates => new CommunicationFlowInstanceRecipient
                                {
                                    Status = CommunicationFlowInstanceRecipientStatus.Inactive,
                                    InactiveReason = CommunicationFlowInstanceRecipientInactiveReason.OpenedCommunication
                                }
                            );

                            break;
                        }

                    case ExitConditionType.AnyEmailClickedThrough:
                        {
                            var communicationIdQuery = communicationFlowInstanceCommunicationService
                                .GetByCommunicationFlowInstance( communicationFlowInstance.Id )
                                .Select( cfic => cfic.CommunicationId )
                                .Distinct();

                            var clickedCommunicationPersonIdQuery = communicationService
                                .GetClickInteractions( communicationIdQuery, activeInstanceRecipientPersonIdQuery )
                                .Select( i => i.PersonAlias.PersonId );

                            bulkUpdateService.BulkUpdate(
                                activeInstanceRecipientQuery.Where( cfir => clickedCommunicationPersonIdQuery.Contains( cfir.RecipientPersonAlias.PersonId ) ),
                                cfir => !(
                                    cfir.Status == CommunicationFlowInstanceRecipientStatus.Inactive
                                    && cfir.InactiveReason == CommunicationFlowInstanceRecipientInactiveReason.ClickedCommunication
                                ),
                                updates => new CommunicationFlowInstanceRecipient
                                {
                                    Status = CommunicationFlowInstanceRecipientStatus.Inactive,
                                    InactiveReason = CommunicationFlowInstanceRecipientInactiveReason.ClickedCommunication
                                }
                            );

                            break;
                        }

                    case ExitConditionType.ConversionAchieved:
                        {
                            var personIdWhoAchievedConversionGoalQuery = communicationFlowInstanceCommunicationConversionService
                                .GetByCommunicationFlowInstance( communicationFlowInstance.Id )
                                .Select( cfich => cfich.PersonAlias.PersonId )
                                .Distinct();

                            bulkUpdateService.BulkUpdate(
                                activeInstanceRecipientQuery.Where( cfir => personIdWhoAchievedConversionGoalQuery.Contains( cfir.RecipientPersonAlias.PersonId ) ),
                                cfir => !(
                                    cfir.Status == CommunicationFlowInstanceRecipientStatus.Inactive
                                    && cfir.InactiveReason == CommunicationFlowInstanceRecipientInactiveReason.ConversionGoalMet
                                ),
                                updates => new CommunicationFlowInstanceRecipient
                                {
                                    Status = CommunicationFlowInstanceRecipientStatus.Inactive,
                                    InactiveReason = CommunicationFlowInstanceRecipientInactiveReason.ConversionGoalMet
                                }
                            );

                            break;
                        }

                    default:
                        break;
                }
            }
        }

        private sealed class InstanceCommunicationHelper
        {
            private readonly CommunicationFlowService _communicationFlowService;
            private readonly CommunicationService _communicationService;
            private readonly PersonService _personService;
            private readonly CommunicationFlowInstanceRecipientService _communicationFlowInstanceRecipientService;
            private readonly IConversionGoalProcessor _conversionGoalProcessor;
            private readonly BulkInsertService _bulkInsertService;
            private readonly SaveChangesService _saveChangesService;

            private InstanceCommunicationHelper(
                CommunicationFlowService communicationFlowService,
                CommunicationService communicationService,
                PersonService personService,
                CommunicationFlowInstanceRecipientService communicationFlowInstanceRecipientService,
                IConversionGoalProcessor conversionGoalProcessor,
                BulkInsertService bulkInsertService,
                SaveChangesService saveChangesService )
            {
                _communicationFlowService = communicationFlowService ?? throw new ArgumentNullException( nameof( communicationFlowService ) );
                _communicationService = communicationService ?? throw new ArgumentNullException( nameof( communicationService ) );
                _personService = personService ?? throw new ArgumentNullException( nameof( personService ) );
                _communicationFlowInstanceRecipientService = communicationFlowInstanceRecipientService ?? throw new ArgumentNullException( nameof( communicationFlowInstanceRecipientService ) );
                _conversionGoalProcessor = conversionGoalProcessor ?? throw new ArgumentNullException( nameof( conversionGoalProcessor ) );
                _bulkInsertService = bulkInsertService ?? throw new ArgumentNullException( nameof( bulkInsertService ) );
                _saveChangesService = saveChangesService ?? throw new ArgumentNullException( nameof( saveChangesService ) );
            }

            public static InstanceCommunicationHelper Create(
                CommunicationFlowService communicationFlowService,
                CommunicationService communicationService,
                PersonService personService,
                CommunicationFlowInstanceRecipientService communicationFlowInstanceRecipientService,
                IConversionGoalProcessor conversionGoalProcessor,
                BulkInsertService bulkInsertService,
                SaveChangesService saveChangesService
            )
            {
                return new InstanceCommunicationHelper(
                    communicationFlowService,
                    communicationService,
                    personService,
                    communicationFlowInstanceRecipientService,
                    conversionGoalProcessor,
                    bulkInsertService,
                    saveChangesService );
            }

            public void CreateNextCommunication( CommunicationFlowInstance instance, out Model.Communication communication )
            {
                // 1. Determine the next communication to send.
                var sentCommunicationsWithBlueprints = instance
                    .CommunicationFlowInstanceCommunications
                    .Select( ic => new
                    {
                        CommunicationFlowInstanceCommunication = ic,
                        CommunicationFlowCommunication = instance
                            .CommunicationFlow
                            .CommunicationFlowCommunications
                            .FirstOrDefault( cfc => cfc.Id == ic.CommunicationFlowCommunicationId )
                    } )
                    .Where( c => c.CommunicationFlowCommunication != null )
                    .OrderBy( c => c.CommunicationFlowCommunication.Order ) // Don't change this sorting unless you also change the logic below.
                    .ToList();

                var nextUnsentBlueprint = instance
                    .CommunicationFlow
                    .CommunicationFlowCommunications
                    .Where( cfc => !sentCommunicationsWithBlueprints.Select( s => s.CommunicationFlowCommunication ).Contains( cfc ) )
                    .OrderBy( cfc => cfc.Order )
                    .FirstOrDefault();

                if ( nextUnsentBlueprint == null )
                {
                    // No more communications to send.
                    _communicationFlowService.CompleteMessaging( instance );
                    _saveChangesService.SaveChanges();

                    communication = null;

                    return;
                }

                // Calculate the send date and time for the next communication.
                DateTime nextCommunicationSendDateTime;
                if ( sentCommunicationsWithBlueprints.Count == 0 )
                {
                    // No communications have been sent yet so use the instance start date.
                    nextCommunicationSendDateTime = instance
                        .StartDate
                        .AddDays( nextUnsentBlueprint.DaysToWait )
                        .Add( nextUnsentBlueprint.TimeToSend );
                }
                else
                {
                    // Some communications have been sent (or are scheduled) so use the last communication's send date.
                    var lastCommunication = sentCommunicationsWithBlueprints.LastOrDefault(); // sorted by CommunicationFlowCommunication.Order
                    var lastCommunicationSendDateTime =
                        lastCommunication?.CommunicationFlowInstanceCommunication.Communication?.SendDateTime // datetime communication was sent
                        ?? lastCommunication?.CommunicationFlowInstanceCommunication.Communication?.FutureSendDateTime; // datetime communication will be sent

                    if ( !lastCommunicationSendDateTime.HasValue )
                    {
                        // The last communication didn't send, nor is it scheduled to be sent.
                        // Let the next job execution process the next communication
                        // so there is time to send the last communication.
                        communication = null;
                        return;
                    }
                    else
                    {
                        nextCommunicationSendDateTime = lastCommunicationSendDateTime.Value
                            .Date
                            .AddDays( nextUnsentBlueprint.DaysToWait )
                            .Add( nextUnsentBlueprint.TimeToSend );
                    }
                }

                if ( nextCommunicationSendDateTime.Date != RockDateTime.Now.Date )
                {
                    // Let the next job execution process the communication
                    // so there is time to accurately build a recipients list
                    // of people who have not exited the flow.
                    communication = null;
                    return;
                }

                DateTime? futureSendDateTime = null;

                if ( RockDateTime.Now < nextCommunicationSendDateTime )
                {
                    futureSendDateTime = nextCommunicationSendDateTime;
                }

                if ( !instance.CommunicationFlowInstanceCommunications.Any() )
                {
                    // Add the instance recipients as close to sending the FIRST communication as possible
                    // to make sure the best matching target recipients for the communication.
                    if ( !AddInitialInstanceRecipients( instance ) )
                    {
                        // Failed to add initial recipients before first communication was created
                        // so indicate that no changes have been made.
                        communication = null;
                        return;
                    }
                }

                // B. Gather active recipients.
                var activeRecipientPersonAliasIdQuery = _communicationFlowInstanceRecipientService
                    .GetByCommunicationFlowInstance( instance.Id )
                    .Where( r => r.Status == CommunicationFlowInstanceRecipientStatus.Active )
                    .Select( r => r.RecipientPersonAliasId );

                // B.1. No one left to send to.
                if ( !activeRecipientPersonAliasIdQuery.Any() )
                {
                    // This flow instance should be marked complete.
                    _communicationFlowService.CompleteMessaging( instance );
                    _saveChangesService.SaveChanges();
                    
                    communication = null;
                    return; 
                }

                // C. Create communication (no SaveChanges here).
                CreateCommunicationFromTemplate( nextUnsentBlueprint, activeRecipientPersonAliasIdQuery, futureSendDateTime, out communication );

                if ( communication == null )
                {
                    // Unable to create communication.
                    return;
                }

                // D. Add instance communication link.
                instance.CommunicationFlowInstanceCommunications.Add(
                    new CommunicationFlowInstanceCommunication
                    {
                        CommunicationFlowInstanceId = instance.Id,
                        CommunicationFlowInstance = instance,
                        CommunicationFlowCommunicationId = nextUnsentBlueprint.Id,
                        CommunicationFlowCommunication = nextUnsentBlueprint,
                        Communication = communication,
                        CommunicationId = communication.Id
                    } );

                _saveChangesService.SaveChanges();

                return ;
            }

            private void CreateCommunicationFromTemplate(
                CommunicationFlowCommunication communicationBlueprint,
                IQueryable<int> activeRecipientPersonAliasIdQuery,
                DateTime? scheduledSendDateTime,
                out Model.Communication communication
            )
            {
                var template = communicationBlueprint.CommunicationTemplate;

                if ( communicationBlueprint.CommunicationType == Model.CommunicationType.Email )
                {
                    communication = _communicationService.CreateEmailCommunication( new CommunicationService.CreateEmailCommunicationArgs
                    {
                        BulkCommunication = true,
                        CommunicationTemplateId = template.Id,
                        FromAddress = template.FromEmail,
                        FromName = template.FromName,
                        FutureSendDateTime = scheduledSendDateTime,
                        Message = template.Message,
                        Name = $"{communicationBlueprint.CommunicationFlow.Name} – {communicationBlueprint.Name}",
                        RecipientPrimaryPersonAliasIds = activeRecipientPersonAliasIdQuery.ToList(),
                        RecipientStatus = CommunicationRecipientStatus.Pending,
                        ReplyTo = template.ReplyToEmail,
                        SendDateTime = null, // This is actually the "sent" value and must be null here.
                        SenderPersonAliasId = template.SenderPersonAliasId,
                        Subject = template.Subject,
                        SystemCommunicationId = null
                    } );
                }
                else if ( communicationBlueprint.CommunicationType == Model.CommunicationType.SMS )
                {
                    communication = _communicationService.CreateSMSCommunication( new CommunicationService.CreateSMSCommunicationArgs
                    {
                        CommunicationName = $"{communicationBlueprint.CommunicationFlow.Name} – {communicationBlueprint.Name}",
                        CommunicationTemplateId = template.Id,
                        FromPrimaryPersonAliasId = template.SenderPersonAliasId,
                        FromSystemPhoneNumber = SystemPhoneNumberCache.Get( template.SmsFromSystemPhoneNumberId.Value ),
                        FutureSendDateTime = scheduledSendDateTime,
                        Message = template.SMSMessage,
                        ResponseCode = null,
                        SystemCommunicationId = null,
                        ToPrimaryPersonAliasIds = activeRecipientPersonAliasIdQuery.ToList()
                    } );
                }
                else if ( communicationBlueprint.CommunicationType == Model.CommunicationType.PushNotification )
                {
                    communication = _communicationService.CreatePushCommunication( new CommunicationService.CreatePushCommunicationArgs
                    {
                        Name = $"{communicationBlueprint.CommunicationFlow.Name} – {communicationBlueprint.Name}",
                        CommunicationTemplateId = template.Id,
                        FromPersonAliasId = template.SenderPersonAliasId,
                        FutureSendDateTime = scheduledSendDateTime,
                        PushMessage = template.PushMessage,
                        ToPersonAliasIds = activeRecipientPersonAliasIdQuery.ToList(),
                        PushData = template.PushData,
                        PushImageBinaryFileId = template.PushImageBinaryFileId,
                        PushOpenAction = template.PushOpenAction,
                        PushOpenMessage = template.PushOpenMessage,
                        PushOpenMessageJson = template.PushOpenMessageJson,
                        PushTitle = template.PushTitle
                    } );
                }
                else
                {
                    communication = null;
                }

                if ( communication != null )
                {
                    // Always set Communication Flow Communications as bulk.
                    communication.IsBulkCommunication = true;

                    foreach ( var attachment in template.Attachments.ToList() )
                    {
                        var newAttachment = new CommunicationAttachment
                        {
                            BinaryFileId = attachment.BinaryFileId,
                            CommunicationType = attachment.CommunicationType
                        };
                        communication.Attachments.Add( newAttachment );
                    }

                    // Separate the CommunicationRecipients from the Communication
                    // so the Communication can be saved without recipients.
                    // Then we'll save the recipients using a bulk insert.
                    var communicationRecipients = communication.Recipients;
                    communication.Recipients = new List<CommunicationRecipient>();
                    foreach ( var communicationRecipient in communicationRecipients )
                    {
                        communicationRecipient.Communication = null;
                        _communicationService.Context.Entry( communicationRecipient ).State = EntityState.Detached;
                    }

                    // Save the communication.
                    _saveChangesService.SaveChanges();

                    foreach ( var communicationRecipient in communicationRecipients )
                    {
                        // Update the communication ID.
                        communicationRecipient.CommunicationId = communication.Id;
                    }

                    _bulkInsertService.BulkInsert( communicationRecipients );
                }
            }

            /// <summary>
            /// Populates <paramref name="instance"/> with CommunicationFlowInstanceRecipients
            /// pulled from flow.TargetAudienceDataViewId. Returns <c>true</c> if it added any.
            /// </summary>
            private bool AddInitialInstanceRecipients( CommunicationFlowInstance instance )
            {
                using ( ObservabilityHelper.StartActivity( "Add Initial Instance Recipients" ) )
                {
                    if ( instance.CommunicationFlow.TriggerType == CommunicationFlowTriggerType.OnDemand )
                    {
                        // On-Demand flows do not auto-seed recipients from a DataView.
                        // They should already be added by the time the first communication is created.
                        return true;
                    }

                    if ( instance.CommunicationFlow.TargetAudienceDataView == null )
                    {
                        // No DataView; nothing to seed.
                        return false;
                    }

                    var dataViewQuery = _personService.GetQueryUsingDataView( instance.CommunicationFlow.TargetAudienceDataView );

                    var personIdQuery = dataViewQuery.Select( p => p.Id );
                    var personAndPersonAliasIds = dataViewQuery
                        .Where( p => p.PrimaryAliasId.HasValue )
                        .Select( p => new
                        {
                            PersonId = p.Id,
                            PersonAliasId = p.PrimaryAliasId.Value
                        } )
                        .ToList();

                    var conversionGoalStartDate = instance.StartDate;
                    var conversionGoalEndDate = conversionGoalStartDate.AddDays( instance.CommunicationFlow.ConversionGoalTimeframeInDays ?? 0 ).Date;

                    var preMetPersonIds = _conversionGoalProcessor
                        .GetConversionQuery( instance, personIdQuery, DateTime.MinValue, conversionGoalStartDate )
                        .Select( h => h.PersonId )
                        .ToList();

                    var unsubscribedFromFlowPersonIds = _communicationFlowInstanceRecipientService
                        .GetByCommunicationFlow( instance.CommunicationFlowId )
                        .WhereUnsubscribedFromCommunicationFlow()
                        .Where( r => r.CommunicationFlowInstanceId != instance.Id && r.RecipientPersonAlias != null )
                        .Select( r => r.RecipientPersonAlias.PersonId )
                        .Distinct()
                        .ToList();

                    using ( ObservabilityHelper.StartActivity( $"Creating {personAndPersonAliasIds.Count} Communication Instance Recipients" ) )
                    {
                        var communicationFlowInstanceRecipients = new List<CommunicationFlowInstanceRecipient>();

                        foreach ( var ids in personAndPersonAliasIds )
                        {
                            var isUnsubscribed = unsubscribedFromFlowPersonIds.Contains( ids.PersonId );

                            communicationFlowInstanceRecipients.Add(
                                new CommunicationFlowInstanceRecipient
                                {
                                    CommunicationFlowInstance = instance,
                                    CommunicationFlowInstanceId = instance.Id,
                                    RecipientPersonAliasId = ids.PersonAliasId,
                                    Status = !isUnsubscribed ? CommunicationFlowInstanceRecipientStatus.Active : CommunicationFlowInstanceRecipientStatus.Inactive,
                                    InactiveReason = !isUnsubscribed ? default : CommunicationFlowInstanceRecipientInactiveReason.UnsubscribedFromFlow,
                                    WasConversionGoalPreMet = preMetPersonIds.Contains( ids.PersonId )
                                }
                            );
                        }

                        if ( communicationFlowInstanceRecipients.Any() )
                        {
                            _bulkInsertService.BulkInsert( communicationFlowInstanceRecipients );

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }

        private class JobStatus
        {
            private readonly ProcessCommunicationFlows _job;
            private readonly int _totalFlows;
            private int _flowsProcessed;
            private int _totalUnsubscribeCount;
            private int _totalConversionCount;
            private int _totalSendCount;

            public JobStatus( ProcessCommunicationFlows job, int totalFlows )
            {
                _job = job ?? throw new ArgumentNullException( nameof( job ) );
                _totalFlows = totalFlows;
            }

            public void ReportFlowProgress( int currentFlowNumber, int flowUnsubscribeCount, int flowConversionCount, int flowSendCount )
            {
                _flowsProcessed = currentFlowNumber;
                _totalUnsubscribeCount += flowUnsubscribeCount;
                _totalConversionCount += flowConversionCount;
                _totalSendCount += flowSendCount;

                _job.UpdateLastStatusMessage( $"Flow {_flowsProcessed}/{_totalFlows} | Unsubscribes:{flowUnsubscribeCount} Conversions:{flowConversionCount} Communications:{flowSendCount}" );
            }

            public void Complete()
            {
                _job.UpdateLastStatusMessage( $"Processed {_totalFlows} flow(s) | Unsubscribes:{_totalUnsubscribeCount} Conversions:{_totalConversionCount} Communications:{_totalSendCount}" );
            }

            public void Fail( Exception ex )
            {
                _job.UpdateLastStatusMessage( $"Failed after flow {_flowsProcessed}/{_totalFlows} | Unsubscribes:{_totalUnsubscribeCount} Conversions:{_totalConversionCount} Communications:{_totalSendCount} | {ex.GetType().Name}" );
            }
        }

        private class ConversionCandidate
        {
            public int PersonId { get; set; }

            public CommunicationFlowInstanceCommunicationConversion Conversion { get; set; }
        }

        #endregion Helper Types
    }
}