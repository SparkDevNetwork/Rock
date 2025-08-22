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
using System.Linq;

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
                activeFlowIds = new CommunicationFlowService( rockContext )
                    .Queryable()
                    .Where( f => f.IsActive )
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

                    using ( var rockContext = CreateRockContext() )
                    {
                        // Turn off AutoDetectChanges for performance and manually call DetectChanges() once before saving.
                        rockContext.Configuration.AutoDetectChangesEnabled = false;

                        CommunicationFlow flow;
                        using ( var activity = ObservabilityHelper.StartActivity( $"Retrieve full object graph for flow {flowId}" ) )
                        {
                            // A. Read the entire flow object graph (except Communications and CommunicationRecipients).
                            flow = new CommunicationFlowService( rockContext )
                                .Queryable()
                                .Include( f => f.Schedule )
                                .Include( f =>
                                    f.CommunicationFlowCommunications.Select( c =>
                                        c.CommunicationTemplate ) )
                                .Include( f =>
                                    f.CommunicationFlowInstances.Select( i =>
                                        i.CommunicationFlowInstanceCommunications.Select( c =>
                                            c.CommunicationFlowInstanceCommunicationConversions.Select( h =>
                                                h.PersonAlias ) ) ) )
                                .Include( f =>
                                    f.CommunicationFlowInstances.Select( i =>
                                        i.CommunicationFlowInstanceRecipients.Select( r =>
                                            r.RecipientPersonAlias ) ) )
                                .FirstOrDefault( f => f.Id == flowId );
                        }
                       
                        if ( flow == null || !flow.IsActive ) // Check again if flow is active in case it changed while this job was running.
                        {
                            continue;
                        }

                        var interactionQuery = new InteractionService( rockContext ).Queryable();
                        var commChannelId = InteractionChannelCache.Get( SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() )?.Id ?? 0;

                        var recipientInfoQuery = new CommunicationFlowInstanceService( rockContext )
                            .Queryable()
                            .AsNoTracking()
                            .Where( cfi => cfi.CommunicationFlowId == flowId )
                            .SelectMany( cfi => cfi.CommunicationFlowInstanceCommunications
                                .SelectMany( cfic => cfic.Communication.Recipients
                                    .Select( r => new RecipientInfo
                                    {
                                        CommunicationRecipientId = r.Id,
                                        CommunicationRecipientSendDateTime = r.SendDateTime,
                                        CommunicationFlowCommunicationOrder = cfic.CommunicationFlowCommunication.Order,
                                        CommunicationSendDateTime = cfic.Communication.SendDateTime,
                                        CommunicationFlowInstanceId = cfi.Id,
                                        CommunicationFlowInstanceCommunicationId = cfic.Id,
                                        CommunicationId = r.CommunicationId,
                                        PersonId = r.PersonAlias.PersonId,
                                        Status = r.Status,
                                        UnsubscribeDateTime = r.UnsubscribeDateTime,
                                        OpenedDateTime = r.OpenedDateTime,
                                        ClickedDateTime = interactionQuery // newest click
                                            .Where( i =>
                                                i.InteractionComponent.InteractionChannelId == commChannelId
                                                && i.InteractionComponent.EntityId == r.CommunicationId
                                                && i.EntityId == r.Id
                                                && i.Operation == "Click" )
                                            .Select( ix => ix.InteractionDateTime )
                                            .Max()
                                    } )
                                )
                            );

                        var flowCommunicationRecipients = recipientInfoQuery.ToList();

                        // Attach the CommunicationFlowInstanceCommunications
                        // from the flow object graph (this avoids reading the CommunicationFlowInstanceCommunications twice).
                        var cficLookup = flow.CommunicationFlowInstances?.SelectMany( cfi => cfi.CommunicationFlowInstanceCommunications ).ToDictionary( cfic => cfic.Id, cfic => cfic ) ?? new Dictionary<int, CommunicationFlowInstanceCommunication>();
                        foreach ( var flowCommunicationRecipient in flowCommunicationRecipients )
                        {
                            if ( cficLookup.TryGetValue( flowCommunicationRecipient.CommunicationFlowInstanceCommunicationId, out var cfic ) )
                            {
                                flowCommunicationRecipient.CommunicationFlowInstanceCommunication = cfic;
                            }
                        }

                        // B. Process the flow unsubscribes, conversions, next-send logic.
                        var isContextDirty = false;
                        using ( ObservabilityHelper.StartActivity( "Process Flow Unsubscribes" ) )
                        {
                            isContextDirty = ProcessFlowUnsubscribes( flow, flowCommunicationRecipients, out flowUnsubscribeCount );
                        }

                        IConversionGoalProcessor conversionGoalProcessor;
                        using ( ObservabilityHelper.StartActivity( "Process Flow Conversions" ) )
                        {
                            isContextDirty |= ProcessFlowConversions( rockContext, flow, recipientInfoQuery, flowCommunicationRecipients, out conversionGoalProcessor, out flowConversionCount );
                        }
                        List<Model.Communication> sendImmediatelyQueue;
                        using ( ObservabilityHelper.StartActivity( "Process Flow Next Communications" ) )
                        {
                            isContextDirty |= ProcessFlowNextCommunications( rockContext, flow, conversionGoalProcessor, out sendImmediatelyQueue, out flowSendCount );
                        }

                        // C. Save once for the whole flow.
                        if ( isContextDirty )
                        {
                            // One context scan for changes.
                            rockContext.ChangeTracker.DetectChanges();
                            rockContext.SaveChanges();
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

        private bool ProcessFlowUnsubscribes( CommunicationFlow flow, List<RecipientInfo> recipientInfos, out int flowUnsubscribeCount )
        {
            var isContextDirty = false;
            flowUnsubscribeCount = 0;

            foreach ( var instance in flow.CommunicationFlowInstances )
            {
                // Get the initial recipients for this flow instance.
                var personIdToInstanceRecipientLookup = instance.CommunicationFlowInstanceRecipients
                    .ToDictionary( cfir => cfir.RecipientPersonAlias.PersonId );
                
                // Check if this instance has any unsubscribes.
                var unsubscribedRecipients = recipientInfos
                    .Where( r => r.CommunicationFlowInstanceId == instance.Id && r.UnsubscribeDateTime.HasValue )
                    .ToList();

                foreach ( var recipient in unsubscribedRecipients )
                {
                    var recipientPersonId = recipient.PersonId;

                    if ( recipientPersonId.HasValue
                        && personIdToInstanceRecipientLookup.TryGetValue( recipientPersonId.Value, out var instanceRecipient )
                        && instanceRecipient.UnsubscribeCommunicationRecipientId != recipient.CommunicationRecipientId )
                    {
                        instanceRecipient.MarkUnsubscribedFromCommunication( recipient.CommunicationRecipientId );

                        flowUnsubscribeCount++;
                        isContextDirty = true;
                    }
                }                
            }

            return isContextDirty;
        }

        private bool ProcessFlowConversions(
            RockContext rockContext,
            CommunicationFlow flow,
            IQueryable<RecipientInfo> recipientInfoQuery,
            List<RecipientInfo> communicationRecipients,
            out IConversionGoalProcessor conversionGoalProcessor,
            out int flowConversionCount )
        {
            var isContextDirty = false;
            flowConversionCount = 0;

            if ( !flow.ConversionGoalType.HasValue
                 || !flow.ConversionGoalTimeframeInDays.HasValue )
            {
                // No conversion goal set, so no conversion processing needed for this flow.
                conversionGoalProcessor = NullConversionGoalProcessor.Instance;
                return isContextDirty;
            }

            using ( ObservabilityHelper.StartActivity( "Create Conversion Goal Processor" ) )
            {
                conversionGoalProcessor = ConversionGoalProcessorFactory.Create( rockContext, flow );
            }

            if ( conversionGoalProcessor == NullConversionGoalProcessor.Instance )
            {
                // The conversion goal type is unknown, so skip conversion processing for this flow.
                Logger.LogWarning( $"Flow {flow.Id} cannot be processed. It has an unsupported conversion goal type {flow.ConversionGoalType.Value}." );
                return isContextDirty;
            }

            foreach ( var flowInstance in flow.CommunicationFlowInstances )
            {
                using ( ObservabilityHelper.StartActivity( $"Processing Conversions For Instance {flowInstance.Id}" ) )
                {
                    if ( conversionGoalProcessor.AddConversions( flowInstance, out var addedConversions ) )
                    {
                        flowConversionCount += addedConversions?.Count ?? 0;
                        isContextDirty = true;
                    }
                }
            }
            
            return isContextDirty;
        }

        private bool ProcessFlowNextCommunications(
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
                return isContextDirty;
            }

            using ( ObservabilityHelper.StartActivity( "Ensuring Flow Has Latest Instance" ) )
            {
                isContextDirty |= triggerProcessor.EnsureFlowHasLatestInstance( flow );
            }

            InstanceCommunicationHelper instanceCommunicationHelper;
            using ( ObservabilityHelper.StartActivity( "Creating Instance Communication Helper" ) )
            {
                instanceCommunicationHelper = InstanceCommunicationHelper.Create(
                    new CommunicationService( rockContext ),
                    new PersonService( rockContext ),
                    conversionGoalProcessor
                );
            }

            sendImmediatelyQueue = new List<Model.Communication>();

            using ( ObservabilityHelper.StartActivity( $"Creating Communications For Flow {flow.Id}" ) )
            {
                foreach ( var instance in flow.CommunicationFlowInstances.Where( i => i.CommunicationFlowInstanceCommunications.Count != flow.CommunicationFlowCommunications.Count ) )
                {
                    using ( ObservabilityHelper.StartActivity( $"Creating Communications For Flow Instance {instance.Id}" ) )
                    {
                        using ( ObservabilityHelper.StartActivity( "Pruning Recipients" ) )
                        {
                            isContextDirty |= ExitConditionHelper.PruneRecipients( rockContext, instance );
                        }

                        Model.Communication communication;
                        using ( ObservabilityHelper.StartActivity( "Creating Next Communication" ) )
                        {
                            isContextDirty |= instanceCommunicationHelper.CreateNextCommunication( instance, out communication );
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
            }

            return isContextDirty;
        }

        #region Conversion Goal Processors

        private interface IConversionGoalProcessor
        {
            bool AddConversions( CommunicationFlowInstance instance, out List<CommunicationFlowInstanceCommunicationConversion> addedConversions );
            IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIds );
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
            public static IConversionGoalProcessor Create( RockContext rockContext, CommunicationFlow flow )
            {
                switch ( flow.ConversionGoalType )
                {
                    case ConversionGoalType.CompletedForm: return new CompletedFormConversionGoalProcessor( new CommunicationFlowInstanceService( rockContext ), new WorkflowService( rockContext ) );
                    case ConversionGoalType.EnteredDataView: return new EnteredDataViewConversionGoalProcessor( new CommunicationFlowInstanceService( rockContext ), new DataViewService( rockContext ), new PersonService( rockContext ), new CommunicationFlowInstanceRecipientService( rockContext ) );
                    case ConversionGoalType.JoinedGroupType: return new JoinedGroupTypeConversionGoalProcessor( new CommunicationFlowInstanceService( rockContext ), new GroupMemberService( rockContext ) );
                    case ConversionGoalType.JoinedGroup: return new JoinedGroupConversionGoalProcessor( new CommunicationFlowInstanceService( rockContext ), new GroupMemberService( rockContext ) );
                    case ConversionGoalType.Registered: return new RegisteredConversionGoalProcessor( new CommunicationFlowInstanceService( rockContext ), new RegistrationRegistrantService( rockContext ) );
                    case ConversionGoalType.TookStep: return new TookStepConversionGoalProcessor( new CommunicationFlowInstanceService( rockContext ), new StepService( rockContext ) );
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
            protected CommunicationFlowInstanceService CommunicationFlowInstanceService { get; }

            protected delegate ( int CommunicationFlowInstanceCommunicationId, int CommunicationRecipientId ) GetInstanceCommunicationForConversionDelegate( ConversionInfo conversionInfo );

            protected ConversionGoalProcessorBase( CommunicationFlowInstanceService communicationFlowInstanceService )
            {
                CommunicationFlowInstanceService = communicationFlowInstanceService ?? throw new ArgumentNullException( nameof( communicationFlowInstanceService ) );
            }

            public bool AddConversions( CommunicationFlowInstance instance, out List<CommunicationFlowInstanceCommunicationConversion> addedConversions )
            {
                var isContextDirty = false;

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
                    return isContextDirty;
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
                                    CommunicationFlowInstanceCommunication = communicationFlowInstanceCommunication,
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
                    return isContextDirty;
                }

                var existingKeys = instance.CommunicationFlowInstanceCommunications
                    .SelectMany( cfic => cfic.CommunicationFlowInstanceCommunicationConversions )
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
                    return isContextDirty;
                }

                // Attach - stays in memory, counted by EF.
                using ( ObservabilityHelper.StartActivity( "Attach Conversions" ) )
                {
                    foreach ( var conversion in newConversions )
                    {
                        conversion.Conversion
                            .CommunicationFlowInstanceCommunication
                            .CommunicationFlowInstanceCommunicationConversions
                            .Add( conversion.Conversion );
                        isContextDirty = true;
                    }

                    addedConversions = newConversions
                        .Select( c => c.Conversion )
                        .ToList();
                }

                return isContextDirty;
            }

            public abstract IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIds );

            public virtual IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIds, DateTime conversionStartDateTime, DateTime conversionEndDateTime )
            {
                return GetConversionQuery( communicationFlowInstance, personIds )
                    .Where( c => conversionStartDateTime <= c.ConversionDateTime && c.ConversionDateTime < conversionEndDateTime );
            }

            private static (CommunicationFlowInstanceCommunication Cfic, int? RecipientId) FindInstanceCommunicationForConversion(
                IEnumerable<RecipientInfo> flowCommunicationRecipientsWhomReceivedComms,
                int personId,
                DateTime conversionDateTime )
            {
                return flowCommunicationRecipientsWhomReceivedComms
                    .OrderByDescending( r => r.CommunicationFlowCommunicationOrder )
                    .ThenByDescending( r => r.CommunicationRecipientSendDateTime ?? r.CommunicationSendDateTime )
                    .Where( r => r.PersonId == personId
                             && ( ( r.CommunicationRecipientSendDateTime.HasValue
                                    && r.CommunicationRecipientSendDateTime.Value <= conversionDateTime )
                                  || ( r.CommunicationSendDateTime.HasValue
                                       && r.CommunicationSendDateTime.Value <= conversionDateTime ) ) )
                    .Select( r => ( r.CommunicationFlowInstanceCommunication, (int?)r.CommunicationRecipientId ) )
                    .FirstOrDefault();
            }

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

            public CompletedFormConversionGoalProcessor( CommunicationFlowInstanceService communicationFlowInstanceService, WorkflowService workflowService )
                : base( communicationFlowInstanceService )
            {
                _workflowService = workflowService ?? throw new ArgumentNullException( nameof( workflowService ) );
            }

            public override IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIdQuery )
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
                    )
                    .Select( p => new ConversionInfo
                    {
                        PersonAliasId = p.InitiatorPersonAliasId.Value,
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

            public EnteredDataViewConversionGoalProcessor( CommunicationFlowInstanceService communicationFlowInstanceService, DataViewService dataViewService, PersonService personService, CommunicationFlowInstanceRecipientService communicationFlowInstanceRecipientService )
                : base( communicationFlowInstanceService )
            {
                _dataViewService = dataViewService ?? throw new ArgumentNullException( nameof( dataViewService ) );
                _personService = personService ?? throw new ArgumentNullException( nameof( personService ) );
                _communicationFlowInstanceRecipientService = communicationFlowInstanceRecipientService ?? throw new ArgumentNullException( nameof( communicationFlowInstanceRecipientService ) );
            }

            public override IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIdQuery )
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

            public JoinedGroupConversionGoalProcessor( CommunicationFlowInstanceService communicationFlowInstanceService, GroupMemberService groupMemberService )
                : base( communicationFlowInstanceService )
            {
                _groupMemberService = groupMemberService ?? throw new ArgumentNullException( nameof( groupMemberService ) );
            }

            public override IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIdQuery )
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

            public JoinedGroupTypeConversionGoalProcessor( CommunicationFlowInstanceService communicationFlowInstanceService, GroupMemberService groupMemberService )
                : base( communicationFlowInstanceService )
            {
                _groupMemberService = groupMemberService ?? throw new ArgumentNullException( nameof( groupMemberService ) );
            }

            public override IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIdQuery )
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

            public RegisteredConversionGoalProcessor( CommunicationFlowInstanceService communicationFlowInstanceService, RegistrationRegistrantService registrationRegistrantService )
                : base( communicationFlowInstanceService )
            {
                _registrationRegistrantService = registrationRegistrantService ?? throw new ArgumentNullException( nameof( registrationRegistrantService ) );
            }

            public override IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIdQuery )
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

            public TookStepConversionGoalProcessor( CommunicationFlowInstanceService communicationFlowInstanceService, StepService stepService )
                : base( communicationFlowInstanceService )
            {
                _stepService = stepService ?? throw new ArgumentNullException( nameof( stepService ) );
            }

            public override IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIdQuery )
            {
                var stepTypeGuid = communicationFlowInstance.CommunicationFlow.GetConversionGoalSettings()?.TookStepSettings?.StepTypeGuid;
                var stepTypeId = stepTypeGuid.HasValue ? StepTypeCache.GetId( stepTypeGuid.Value ) : null;

                if ( !stepTypeId.HasValue )
                {
                    return null;
                }

                return _stepService
                    .Queryable()
                    .Where( s =>
                        s.StepType.Id == stepTypeId.Value
                        && s.CompletedDateTime.HasValue
                        && s.StepStatus.IsCompleteStatus
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

            public override IQueryable<ConversionInfo> GetConversionQuery( CommunicationFlowInstance communicationFlowInstance, IQueryable<int> personIdQuery, DateTime conversionStartDateTime, DateTime conversionEndDateTime )
            {
                // The CompletedDateTime doesn't actually store the time
                // so check if the conversion occurred at any time between the dates.
                conversionStartDateTime = conversionStartDateTime.StartOfDay();
                conversionEndDateTime = conversionEndDateTime.AddDays( 1 ).StartOfDay();

                return GetConversionQuery( communicationFlowInstance, personIdQuery )
                    .Where( c => conversionStartDateTime <= c.ConversionDateTime && c.ConversionDateTime < conversionEndDateTime );
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
            bool EnsureFlowHasLatestInstance( CommunicationFlow flow ); 
        }

        private static class TriggerTypeProcessorFactory
        {
            public static ITriggerTypeProcessor Create( RockContext rockContext, CommunicationFlow flow )
            {
                switch ( flow.TriggerType )
                {
                    case CommunicationFlowTriggerType.OneTime:   return new OneTimeTriggerTypeProcessor( new PersonService( rockContext ) );
                    case CommunicationFlowTriggerType.Recurring: return new RecurringTriggerTypeProcessor( new PersonService( rockContext ) );
                    case CommunicationFlowTriggerType.OnDemand:  return new OnDemandTriggerTypeProcessor();
                    default: return null;
                }
            }
        }

        private abstract class TriggerTypeProcessorBase : ITriggerTypeProcessor
        {
            protected PersonService PersonService { get; }

            protected TriggerTypeProcessorBase( PersonService personService )
            {
                PersonService = personService ?? throw new ArgumentNullException( nameof( personService ) );
            }

            public abstract bool EnsureFlowHasLatestInstance( CommunicationFlow flow );
        }

        private sealed class OnDemandTriggerTypeProcessor : ITriggerTypeProcessor
        {
            /// <summary>
            /// This job cannot create On-Demand flows so this method always returns <see langword="false" />.
            /// </summary>
            /// <returns><see langword="false" /></returns>
            public bool EnsureFlowHasLatestInstance( CommunicationFlow flow ) => false;
        }

        private sealed class OneTimeTriggerTypeProcessor : TriggerTypeProcessorBase
        {
            public OneTimeTriggerTypeProcessor( PersonService personService )
                : base( personService )
            { }

            public override bool EnsureFlowHasLatestInstance( CommunicationFlow flow )
            {
                var isContextDirty = false;

                if ( flow.CommunicationFlowInstances.Any() )
                {
                    // There is already a one-time flow instance so return.
                    return isContextDirty;
                }

                // No instances yet so try to create one.
                var schedule = flow.Schedule;
                if ( schedule == null )
                {
                    // The schedule has not been set so an instance can not be created.
                    return isContextDirty;
                }

                var firstStartDateTime = schedule.FirstStartDateTime;
                if ( !firstStartDateTime.HasValue )
                {
                    // The schedule does not have a first start date so an instance can not be created.
                    return isContextDirty;
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
                isContextDirty = true;

                return isContextDirty;
            }
        }

        private sealed class RecurringTriggerTypeProcessor : TriggerTypeProcessorBase
        {
            public RecurringTriggerTypeProcessor( PersonService personService )
                : base( personService )
            { }

            public override bool EnsureFlowHasLatestInstance( CommunicationFlow flow )
            {
                var isContextDirty = false;

                var schedule = flow.Schedule;
                if ( schedule == null )
                {
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

                if ( !nextInstanceStartDateTime.HasValue )
                {
                    // The schedule is completed; no more instances to create.
                    return isContextDirty;
                }

                if ( RockDateTime.Now.Date != nextInstanceStartDateTime.Value.Date )
                {
                    // The next instance start date isn't today,
                    // so let the next job execution try to process the flow.
                    return isContextDirty;
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
                isContextDirty = true;

                return isContextDirty;
            }
        }

        #endregion Trigger Type Processors

        #region Helper Types

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
            public static bool PruneRecipients( RockContext rockContext, CommunicationFlowInstance communicationFlowInstance )
            {
                var isContextDirty = false;

                var recipients = communicationFlowInstance.CommunicationFlowInstanceRecipients
                    .Where( r => r.Status == CommunicationFlowInstanceRecipientStatus.Active );

                var communicationFlow = communicationFlowInstance?.CommunicationFlow;

                if ( communicationFlow == null )
                {
                    // Unable to check exit condition to determine if recipients should be pruned.
                    return isContextDirty;
                }
                
                switch ( communicationFlow.ExitConditionType )
                {
                    case ExitConditionType.LastMessageSent:
                        {
                            var expectedCommunicationsCount = communicationFlow.CommunicationFlowCommunications?.Count ?? 0;
                            var actualCommunicationsCount = communicationFlowInstance.CommunicationFlowInstanceCommunications?.Count ?? 0;

                            var hasSentLastFlowCommunication = expectedCommunicationsCount > 0
                                && expectedCommunicationsCount <= actualCommunicationsCount;

                            if ( hasSentLastFlowCommunication )
                            {
                                foreach ( var recipient in recipients )
                                {
                                    recipient.Status = CommunicationFlowInstanceRecipientStatus.Inactive;
                                    recipient.InactiveReason = CommunicationFlowInstanceRecipientInactiveReason.LastCommunicationSent;
                                    isContextDirty = true;
                                }
                            }

                            break;
                        }

                    case ExitConditionType.AnyEmailOpened:
                        {
                            var communicationIdQuery = new CommunicationFlowInstanceCommunicationService( rockContext )
                                .Queryable()
                                .Where( cfic => cfic.CommunicationFlowInstanceId == communicationFlowInstance.Id )
                                .Select( cfic => cfic.CommunicationId )
                                .Distinct();

                            var personIdQuery = new CommunicationFlowInstanceRecipientService( rockContext )
                                .Queryable()
                                .Where( cfir =>
                                    cfir.CommunicationFlowInstanceId == communicationFlowInstance.Id
                                    && cfir.Status == CommunicationFlowInstanceRecipientStatus.Active )
                                .Select( cfir => cfir.RecipientPersonAlias.PersonId )
                                .Distinct();

                            var recipientPersonIdsWhoOpenedComm = new CommunicationService( rockContext )
                                .GetOpenedInteractions( communicationIdQuery, personIdQuery )
                                .Select( i => i.PersonAlias.PersonId )
                                .ToList();

                            var recipientsWhoOpenedComm = recipients
                                .Where( r => recipientPersonIdsWhoOpenedComm.Contains( r.GetPersonId() ) )
                                .ToList();

                            foreach ( var recipient in recipientsWhoOpenedComm )
                            {
                                recipient.Status = CommunicationFlowInstanceRecipientStatus.Inactive;
                                recipient.InactiveReason = CommunicationFlowInstanceRecipientInactiveReason.OpenedCommunication;
                                isContextDirty = true;
                            }

                            break;
                        }

                    case ExitConditionType.AnyEmailClickedThrough:
                        {
                            var communicationIdQuery = new CommunicationFlowInstanceCommunicationService( rockContext )
                                .Queryable()
                                .Where( cfic => cfic.CommunicationFlowInstanceId == communicationFlowInstance.Id )
                                .Select( cfic => cfic.CommunicationId )
                                .Distinct();

                            var personIdQuery = new CommunicationFlowInstanceRecipientService( rockContext )
                                .Queryable()
                                .Where( cfir =>
                                    cfir.CommunicationFlowInstanceId == communicationFlowInstance.Id
                                    && cfir.Status == CommunicationFlowInstanceRecipientStatus.Active )
                                .Select( cfir => cfir.RecipientPersonAlias.PersonId )
                                .Distinct();

                            var recipientPersonIdsWhoClickedComm = new CommunicationService( rockContext )
                                .GetClickInteractions( communicationIdQuery, personIdQuery )
                                .Select( i => i.PersonAlias.PersonId )
                                .ToList();

                            var recipientsWhoClickedComm = recipients
                                .Where( r => recipientPersonIdsWhoClickedComm.Contains( r.GetPersonId() ) )
                                .ToList();

                            foreach ( var recipient in recipientsWhoClickedComm )
                            {
                                recipient.Status = CommunicationFlowInstanceRecipientStatus.Inactive;
                                recipient.InactiveReason = CommunicationFlowInstanceRecipientInactiveReason.ClickedCommunication;
                                isContextDirty = true;
                            }

                            break;
                        }

                    case ExitConditionType.ConversionAchieved:
                        {
                            var personIdsWhoAchievedConversionGoal = communicationFlowInstance
                                .CommunicationFlowInstanceCommunications
                                .SelectMany( cfic => cfic.CommunicationFlowInstanceCommunicationConversions.Select( cfich => cfich.PersonAlias.PersonId ).Distinct() )
                                .Distinct()
                                .ToList();

                            var recipientsWhoAchievedConversionGoal = recipients
                                .Where( r => personIdsWhoAchievedConversionGoal.Contains( r.GetPersonId() ) )
                                .ToList();

                            foreach ( var recipient in recipientsWhoAchievedConversionGoal )
                            {
                                recipient.Status = CommunicationFlowInstanceRecipientStatus.Inactive;
                                recipient.InactiveReason = CommunicationFlowInstanceRecipientInactiveReason.ConversionGoalMet;
                                isContextDirty = true;
                            }

                            break;
                        }

                    default:
                        break;
                }

                return isContextDirty;
            }
        }

        private sealed class InstanceCommunicationHelper
        {
            private readonly CommunicationService _communicationService;
            private readonly PersonService _personService;
            private readonly IConversionGoalProcessor _conversionGoalProcessor;

            private InstanceCommunicationHelper( CommunicationService communicationService, PersonService personService, IConversionGoalProcessor conversionGoalProcessor )
            {
                _communicationService = communicationService ?? throw new ArgumentNullException( nameof( communicationService ) );
                _personService = personService ?? throw new ArgumentNullException( nameof( personService ) );
                _conversionGoalProcessor = conversionGoalProcessor ?? throw new ArgumentNullException( nameof( conversionGoalProcessor ) );
            }

            public static InstanceCommunicationHelper Create( CommunicationService communicationService, PersonService personService, IConversionGoalProcessor conversionGoalProcessor )
            {
                return new InstanceCommunicationHelper( communicationService, personService, conversionGoalProcessor );
            }

            public bool CreateNextCommunication( CommunicationFlowInstance instance, out Model.Communication communication )
            {
                var isContextDirty = false;

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
                    communication = null;
                    return isContextDirty;
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
                        return isContextDirty;
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
                    return isContextDirty;
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
                    isContextDirty |= AddInitialInstanceRecipients( instance );
                    if ( !instance.CommunicationFlowInstanceRecipients.Any() )
                    {
                        // Failed to add initial recipients before first communication was created
                        // so indicate that no changes have been made.
                        communication = null;
                        return isContextDirty;
                    }
                }

                // B. Gather active recipients.
                var activeRecipientPersonAliasIds = instance
                    .CommunicationFlowInstanceRecipients
                   .Where( r => r.Status == CommunicationFlowInstanceRecipientStatus.Active )
                   .Select( r => r.RecipientPersonAliasId )
                   .ToList();

                // B.1. No one left to send to.
                if ( !activeRecipientPersonAliasIds.Any() )
                {
                    // Add an instance communication without sending an actual communication.
                    instance.CommunicationFlowInstanceCommunications.Add(
                        new CommunicationFlowInstanceCommunication
                        {
                            CommunicationFlowInstanceId = instance.Id,
                            CommunicationFlowCommunicationId = nextUnsentBlueprint.Id
                        } );
                    isContextDirty = true; // Context modified even though no communication was needed.
                    
                    communication = null;
                    return isContextDirty; 
                }

                // C. Create communication (no SaveChanges here).
                isContextDirty |= CreateCommunicationFromTemplate( nextUnsentBlueprint, activeRecipientPersonAliasIds, futureSendDateTime, out communication );

                if ( communication == null )
                {
                    // Unable to create communication.
                    return isContextDirty;
                }

                // D. Add instance communication link.
                instance.CommunicationFlowInstanceCommunications.Add(
                    new CommunicationFlowInstanceCommunication
                    {
                        CommunicationFlowInstanceId = instance.Id,
                        CommunicationFlowInstance = instance,
                        CommunicationFlowCommunicationId = nextUnsentBlueprint.Id,
                        CommunicationFlowCommunication = nextUnsentBlueprint,
                        Communication = communication
                    } );

                isContextDirty = true;

                return isContextDirty;
            }

            private bool CreateCommunicationFromTemplate(
                CommunicationFlowCommunication communicationBlueprint,
                List<int> activeRecipientPersonAliasIds,
                DateTime? scheduledSendDateTime,
                out Model.Communication communication
            )
            {
                var isContextDirty = false;
                var template = communicationBlueprint.CommunicationTemplate;

                if ( communicationBlueprint.CommunicationType == Model.CommunicationType.Email )
                {
                    communication = _communicationService.CreateEmailCommunication( new CommunicationService.CreateEmailCommunicationArgs
                    {
                        BulkCommunication = false,
                        CommunicationTemplateId = template.Id,
                        FromAddress = template.FromEmail,
                        FromName = template.FromName,
                        FutureSendDateTime = scheduledSendDateTime,
                        Message = template.Message,
                        Name = $"{communicationBlueprint.CommunicationFlow.Name} – {communicationBlueprint.Name}",
                        RecipientPrimaryPersonAliasIds = activeRecipientPersonAliasIds,
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
                        ToPrimaryPersonAliasIds = activeRecipientPersonAliasIds
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
                        ToPersonAliasIds = activeRecipientPersonAliasIds,
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
                
                isContextDirty |= communication != null;
                return isContextDirty;
            }

            /// <summary>
            /// Populates <paramref name="instance"/> with CommunicationFlowInstanceRecipients
            /// pulled from flow.TargetAudienceDataViewId. Returns <c>true</c> if it added any.
            /// </summary>
            private bool AddInitialInstanceRecipients( CommunicationFlowInstance instance )
            {
                var isContextDirty = false;

                if ( instance.CommunicationFlow.TargetAudienceDataView == null )
                {
                    // No DataView; nothing to seed.
                    return isContextDirty;
                }

                var dataViewQuery = _personService
                    .GetQueryUsingDataView( instance.CommunicationFlow.TargetAudienceDataView );

                var personIdQuery = dataViewQuery.Select( p => p.Id );
                var personAndPersonAliasIds = dataViewQuery
                    .Where( p => p.PrimaryAliasId.HasValue )
                    .Select( p => new
                    {
                        PersonId = p.Id,
                        PersonAliasId = p.PrimaryAliasId.Value
                    } )
                    .ToList();

                var preMetPersonIds = _conversionGoalProcessor.GetConversionQuery( instance, personIdQuery )
                        .Select( h => h.PersonId )
                        .ToList();

                foreach ( var ids in personAndPersonAliasIds )
                {
                    instance.CommunicationFlowInstanceRecipients.Add(
                        new CommunicationFlowInstanceRecipient
                        {
                            CommunicationFlowInstance = instance,
                            RecipientPersonAliasId = ids.PersonAliasId,
                            Status = CommunicationFlowInstanceRecipientStatus.Active,
                            WasConversionGoalPreMet = preMetPersonIds.Contains( ids.PersonId )
                        }
                    );
                    isContextDirty = true;
                }

                return isContextDirty;
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