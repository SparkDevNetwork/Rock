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
using System.Diagnostics;
using System.Linq;

using AngleSharp.Common;

using DocumentFormat.OpenXml.Wordprocessing;

using Microsoft.Ajax.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Communication;
using Rock.Model;
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
            var sw = Stopwatch.StartNew();
            var now = RockDateTime.Now;

            Log( Microsoft.Extensions.Logging.LogLevel.Information, "Starting job...", start: now );

            List<int> activeFlowIds;
            using ( var rockContext = new RockContext() )
            {
                activeFlowIds = new CommunicationFlowService( rockContext )
                    .Queryable()
                    .Where( f => f.IsActive )
                    .Select( f => f.Id )
                    .ToList();
            }

            foreach ( var flowId in activeFlowIds )
            {
                using ( var rockContext = new RockContext() )
                {
                    // Turn off AutoDetectChanges for performance and manually call DetectChanges() once before saving.
                    rockContext.Configuration.AutoDetectChangesEnabled = false;

                    // A. Read the entire flow object graph.
                    var flow = new CommunicationFlowService( rockContext )
                        .Queryable()
                        .Include( f => f.Schedule )
                        .Include( f =>
                            f.CommunicationFlowCommunications.Select( c =>
                                c.CommunicationTemplate ) )
                        .Include( f =>
                            f.CommunicationFlowInstances.Select( i =>
                                i.CommunicationFlowInstanceCommunications.Select( ic =>
                                    ic.Communication.Recipients ) ) )
                        .Include( f =>
                            f.CommunicationFlowInstances.Select( i =>
                                i.CommunicationFlowInstanceRecipients ) )
                        .FirstOrDefault( f => f.Id == flowId );

                    if ( flow == null || !flow.IsActive ) // Check again if flow is active in case it changed while this job was running.
                    {
                        continue;
                    }

                    // B. Process the flow unsubscribes, conversions, next-send logic.
                    var flowHasChanges = ProcessFlowUnsubscribes( flow );
                    flowHasChanges |= ProcessFlowConversions( rockContext, flow );
                    flowHasChanges |= ProcessFlowNextCommunications( rockContext, flow, out var sendImmediatelyQueue );

                    // C. Save once for the whole flow.
                    if ( flowHasChanges )
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
                }
            }

            Log( Microsoft.Extensions.Logging.LogLevel.Information, "Stopping job...", elapsedMs: sw.ElapsedMilliseconds );
        }

        private bool ProcessFlowUnsubscribes( CommunicationFlow flow )
        {
            var hasUnsubscribes = false;

            foreach ( var instance in flow.CommunicationFlowInstances )
            {
                // Get the initial recipients for this flow instance.
                var instanceRecipientLookup = instance.CommunicationFlowInstanceRecipients
                    .ToDictionary( cfir => cfir.RecipientPersonAliasId );
                
                // Check if this instance has any unsubscribes.
                var unsubscribedRecipients = instance.CommunicationFlowInstanceCommunications
                    .SelectMany( ic => ic.Communication.Recipients )
                    .Where( r => r.UnsubscribeDateTime.HasValue )
                    .ToList();

                foreach ( var recipient in unsubscribedRecipients )
                {
                    var recipientPersonAliasId = recipient.PersonAliasId;

                    if ( recipientPersonAliasId.HasValue
                        && instanceRecipientLookup.TryGetValue( recipientPersonAliasId.Value, out var instanceRecipient )
                        && instanceRecipient.UnsubscribeCommunicationRecipientId != recipient.Id )
                    {
                        instanceRecipient.UnsubscribeCommunicationRecipient = recipient;
                        // TODO JMH Do we need to deal with flow-level unsubscribes?
                        instanceRecipient.UnsubscribeScope = CommunicationFlowInstanceRecipientUnsubscribeScope.CommunicationFlowInstance;
                        hasUnsubscribes = true;
                    }
                }                
            }

            return hasUnsubscribes;
        }

        private bool ProcessFlowConversions( RockContext rockContext, CommunicationFlow flow )
        {
            if ( !flow.ConversionGoalType.HasValue
                 || !flow.ConversionGoalTimeframeInDays.HasValue )
            {
                // TODO JMH Log a message?
                // No conversion goal set, so no conversion processing needed for this flow.
                return false;
            }

            var conversionGoalProcessor = ConversionGoalProcessorFactory.Create( rockContext, flow );
            
            if ( conversionGoalProcessor == null )
            {
                // The conversion goal type is unknown, so skip conversion processing for this flow.
                // TODO JMH Log a warning/exception?
                return false;
            }

            var hasChanges = false;

            foreach ( var instance in flow.CommunicationFlowInstances )
            {
                hasChanges |= conversionGoalProcessor.AddConversions( instance );
            }

            return hasChanges;
        }

        private bool ProcessFlowNextCommunications( RockContext rockContext, CommunicationFlow flow, out List<Model.Communication> sendImmediatelyQueue )
        {
            var triggerProcessor = TriggerTypeProcessorFactory.Create( rockContext, flow );

            if ( triggerProcessor == null )
            {
                // TODO JMH Log a warning/exception?
                sendImmediatelyQueue = null;
                return false;
            }

            var hasChanges = triggerProcessor.EnsureFlowHasInitialInstance( flow );

            var instanceCommunicationHelper = InstanceCommunicationHelper.Create( new CommunicationService( rockContext ) );
            sendImmediatelyQueue = new List<Model.Communication>();

            // TODO JMH Implement CommunicationFlowInstance.CompletedDateTime so we can easily identify when an instance has completed.
            foreach ( var instance in flow.CommunicationFlowInstances.Where( i => !i.CompletedDateTime.HasValue ) )
            {
                hasChanges |= ExitConditionHelper.PruneRecipients( instance );
                hasChanges |= instanceCommunicationHelper.CreateNextCommunication( instance, out var communication );

                if ( communication != null )
                {
                    // Decide whether it should be sent immediately.
                    if ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value <= RockDateTime.Now )
                    {
                        sendImmediatelyQueue.Add( communication );
                    }
                }
            }

            return hasChanges;
        }

        #region Conversion Goal Processors

        private interface IConversionGoalProcessor
        {
            bool AddConversions( CommunicationFlowInstance instance );
        }

        private static class ConversionGoalProcessorFactory
        {
            public static IConversionGoalProcessor Create( RockContext rockContext, CommunicationFlow flow )
            {
                switch ( flow.ConversionGoalType )
                {
                    case ConversionGoalType.CompletedForm: return new CompletedFormConversionGoalProcessor( new WorkflowService( rockContext ) );
                    case ConversionGoalType.EnteredDataView: return new EnteredDataViewConversionGoalProcessor( new DataViewService( rockContext ), new PersonService( rockContext ) );
                    case ConversionGoalType.JoinedGroupType: return new JoinedGroupTypeConversionGoalProcessor( new GroupMemberService( rockContext ) );
                    case ConversionGoalType.JoinedGroup: return new JoinedGroupConversionGoalProcessor( new GroupMemberService( rockContext ) );
                    case ConversionGoalType.Registered: return new RegisteredConversionGoalProcessor( new RegistrationRegistrantService( rockContext ) );
                    case ConversionGoalType.TookStep: return new TookStepConversionGoalProcessor( new StepService( rockContext ) );
                    default: return null;
                }
            }
        }

        private abstract class ConversionGoalProcessorBase : IConversionGoalProcessor
        {
            protected delegate CommunicationFlowCommunication GetConversionCommunicationDelegate( int personAliasId, DateTime conversionDateTime );

            public bool AddConversions( CommunicationFlowInstance instance )
            {
                // TODO JMH This is going to play a role in deduping conversions across multiple messages in the same instance.
                // TODO JMH I think we need to figure out which messages need conversion processing
                // and do them one at a time.
                var recipientInfo = instance.CommunicationFlowInstanceCommunications
                    .SelectMany( c =>
                        c.Communication
                        .Recipients
                        .Where( r => r.PersonAliasId.HasValue && ( r.SendDateTime.HasValue || c.Communication.SendDateTime.HasValue ) )
                        .Select( r => new
                        {
                            c.CommunicationFlowCommunication,
                            PersonAliasId = r.PersonAliasId.Value,
                            SentDateTime =  r.SendDateTime ?? c.Communication.SendDateTime.Value 
                        } )
                    )
                    // Order the recipient info by the intended send order of the messages ( newest first ), then by the actual send date/time of the message ( newest first ).
                    .OrderByDescending( r => r.CommunicationFlowCommunication.Order )
                    .ThenByDescending( r => r.SentDateTime )
                    .ToList();

                var personAliasIds = recipientInfo
                    .Select( r => r.PersonAliasId )
                    .Distinct()
                    .ToList();

                if ( personAliasIds.Any() != true )
                {
                    return false;
                }

                var flowInstanceStartDateTime = instance.StartDateTime;
                var flowInstanceConversionEndDateTime = instance.StartDateTime.AddDays( instance.CommunicationFlow.ConversionGoalTimeframeInDays.Value );

                CommunicationFlowCommunication GetConversionCommunication( int personAliasId, DateTime conversionDateTime )
                {
                    // Find the first communication that was sent on or before the conversion date/time.
                    return recipientInfo
                        .Where( r => r.PersonAliasId == personAliasId && r.SentDateTime <= conversionDateTime )
                        .Select( r => r.CommunicationFlowCommunication )
                        .FirstOrDefault();
                }

                // Call the template method implemented by extending classes to get the conversions.
                var conversionHistories = GetConversionHistories( instance, flowInstanceStartDateTime, flowInstanceConversionEndDateTime, personAliasIds, GetConversionCommunication );

                if ( conversionHistories?.Any() != true )
                {
                    return false;
                }

                foreach ( var conversionHistory in conversionHistories )
                {
                    instance.CommunicationFlowInstanceConversionHistories.Add( conversionHistory );
                }

                return true;
            }

            protected abstract List<CommunicationFlowInstanceConversionHistory> GetConversionHistories( CommunicationFlowInstance instance, DateTime flowInstanceStartDateTime, DateTime flowInstanceConversionEndDateTime, IEnumerable<int> personAliasIds, GetConversionCommunicationDelegate getConversionCommunication );
        }

        private sealed class CompletedFormConversionGoalProcessor : ConversionGoalProcessorBase
        {
            private readonly WorkflowService _workflowService;

            public CompletedFormConversionGoalProcessor( WorkflowService workflowService )
            {
                _workflowService = workflowService ?? throw new ArgumentNullException( nameof( workflowService ) );
            }

            protected override List<CommunicationFlowInstanceConversionHistory> GetConversionHistories( CommunicationFlowInstance instance, DateTime flowInstanceStartDateTime, DateTime flowInstanceConversionEndDateTime, IEnumerable<int> personAliasIds, GetConversionCommunicationDelegate getConversionCommunication )
            {
                var workflowTypeGuid = instance.CommunicationFlow.GetConversionGoalSettings()?.CompletedFormSettings?.WorkflowTypeGuid;

                if ( !workflowTypeGuid.HasValue )
                {
                    return null;
                }

                var completedFormConversions = _workflowService
                    .Queryable()
                    .Where( w =>
                        w.WorkflowType.Guid == workflowTypeGuid.Value
                        && w.CompletedDateTime.HasValue
                        && flowInstanceStartDateTime <= w.CompletedDateTime.Value
                        && w.CompletedDateTime.Value <= flowInstanceConversionEndDateTime
                        && w.InitiatorPersonAliasId.HasValue
                        && personAliasIds.Contains( w.InitiatorPersonAliasId.Value )
                    )
                    .Select( p => new
                    {
                        PersonAliasId = p.InitiatorPersonAliasId.Value,
                        ConversionDateTime = p.CompletedDateTime.Value
                    } )
                    .ToList();

                if ( !completedFormConversions.Any() )
                {
                    return null;
                }

                // TODO JMH Don't allow conversions to be duplicated across subsequent instance messages in this job execution run.
                // This could happen if multiple messages for the same instance
                // have not been processed since the last time the job ran.
                return completedFormConversions
                    .Select( c =>
                    {
                        var conversionCommunication = getConversionCommunication( c.PersonAliasId, c.ConversionDateTime );

                        if ( conversionCommunication == null )
                        {
                            return null;
                        }

                        return new CommunicationFlowInstanceConversionHistory
                        {
                            CommunicationFlowCommunicationId = conversionCommunication.Id,
                            CommunicationFlowCommunication = conversionCommunication,
                            CommunicationFlowInstanceId = instance.Id,
                            CommunicationFlowInstance = instance,
                            Date = c.ConversionDateTime,
                            PersonAliasId = c.PersonAliasId
                        };
                    } )
                    // Filter out conversions that already exist in the current instance.
                    .Where( conversionHistory =>
                        conversionHistory != null
                        && !instance.CommunicationFlowInstanceConversionHistories.Any( h =>
                            h.CommunicationFlowCommunicationId == conversionHistory.CommunicationFlowCommunicationId
                            && h.CommunicationFlowInstanceId == conversionHistory.CommunicationFlowInstanceId
                            && h.PersonAliasId == conversionHistory.PersonAliasId
                            && h.Date == conversionHistory.Date
                        )
                    )
                    .ToList();
            }
        }

        private sealed class EnteredDataViewConversionGoalProcessor : ConversionGoalProcessorBase
        {
            private readonly DataViewService _dataViewService;
            private readonly PersonService _personService;

            public EnteredDataViewConversionGoalProcessor( DataViewService dataViewService, PersonService personService )
            {
                _dataViewService = dataViewService ?? throw new ArgumentNullException( nameof( dataViewService ) );
                _personService = personService ?? throw new ArgumentNullException( nameof( personService ) );
            }

            protected override List<CommunicationFlowInstanceConversionHistory> GetConversionHistories( CommunicationFlowInstance instance, DateTime flowInstanceStartDateTime, DateTime flowInstanceConversionEndDateTime, IEnumerable<int> personAliasIds, GetConversionCommunicationDelegate getConversionCommunication )
            {
                var dataViewGuid = instance.CommunicationFlow.GetConversionGoalSettings()?.EnteredDataViewSettings?.DataViewGuid;

                if ( !dataViewGuid.HasValue )
                {
                    return null;
                }

                var dataView = _dataViewService.Get( dataViewGuid.Value );

                // Only process the data view if it exists and has a last run date that is within the flow instance's start and end dates.
                if ( dataView == null
                     || !dataView.LastRunDateTime.HasValue
                     || flowInstanceStartDateTime > dataView.LastRunDateTime
                     || dataView.LastRunDateTime > flowInstanceConversionEndDateTime )
                {
                    return null;
                }

                // TODO JMH Test this. I'm not convinced that this will work.
                var enteredDataViewConversions = _personService.GetQueryUsingDataView( dataView )
                    .Where( p =>
                        p.PrimaryAliasId.HasValue
                        && personAliasIds.Contains( p.PrimaryAliasId.Value )
                    )
                    .Select( p => new
                    {
                        PersonAliasId = p.PrimaryAliasId.Value
                    } )
                    .ToList();

                if ( !enteredDataViewConversions.Any() )
                {
                    return null;
                }

                return enteredDataViewConversions
                    .Select( c =>
                    {
                        var conversionCommunication = getConversionCommunication( c.PersonAliasId, dataView.LastRunDateTime.Value );

                        if ( conversionCommunication == null )
                        {
                            return null;
                        }

                        return new CommunicationFlowInstanceConversionHistory
                        {
                            CommunicationFlowCommunicationId = conversionCommunication.Id,
                            CommunicationFlowCommunication = conversionCommunication,
                            CommunicationFlowInstanceId = instance.Id,
                            CommunicationFlowInstance = instance,
                            Date = dataView.LastRunDateTime.Value,
                            PersonAliasId = c.PersonAliasId
                        };
                    } )
                    // Filter out conversions that already exist in the current conversions.
                    .Where( conversionHistory =>
                        conversionHistory != null
                        && !instance.CommunicationFlowInstanceConversionHistories.Any( h =>
                            h.CommunicationFlowCommunicationId == conversionHistory.CommunicationFlowCommunicationId
                            && h.CommunicationFlowInstanceId == conversionHistory.CommunicationFlowInstanceId
                            && h.PersonAliasId == conversionHistory.PersonAliasId
                            // Don't check Date here because once we have a conversion history
                            // for a given CommunicationFlowCommunicationId, CommunicationFlowInstanceId, and PersonAliasId,
                            // we don't care about the date anymore.
                            // We only care about the first conversion date for the Entered Data View goal.
                            // && h.Date == conversionHistory.Date 
                        )
                    )
                    .ToList();
            }
        }

        private sealed class JoinedGroupConversionGoalProcessor : ConversionGoalProcessorBase
        {
            private readonly GroupMemberService _groupMemberService;

            public JoinedGroupConversionGoalProcessor( GroupMemberService groupMemberService )
            {
                _groupMemberService = groupMemberService ?? throw new ArgumentNullException( nameof( groupMemberService ) );
            }

            protected override List<CommunicationFlowInstanceConversionHistory> GetConversionHistories( CommunicationFlowInstance instance, DateTime flowInstanceStartDateTime, DateTime flowInstanceConversionEndDateTime, IEnumerable<int> personAliasIds, GetConversionCommunicationDelegate getConversionCommunication )
            {
                var groupGuid = instance.CommunicationFlow.GetConversionGoalSettings()?.JoinedGroupSettings?.GroupGuid;

                if ( !groupGuid.HasValue )
                {
                    return null;
                }

                var joinedGroupConversions = _groupMemberService
                    .Queryable()
                    .Where( gm =>
                        gm.Group.Guid == groupGuid.Value
                        && gm.DateTimeAdded.HasValue
                        && flowInstanceStartDateTime <= gm.DateTimeAdded.Value
                        && gm.DateTimeAdded.Value <= flowInstanceConversionEndDateTime
                        && gm.Person.PrimaryAliasId.HasValue
                        && personAliasIds.Contains( gm.Person.PrimaryAliasId.Value )
                    )
                    .Select( gm => new
                    {
                        PersonAliasId = gm.Person.PrimaryAliasId.Value,
                        ConversionDateTime = gm.DateTimeAdded.Value
                    } )
                    .ToList();

                if ( !joinedGroupConversions.Any() )
                {
                    return null;
                }

                return joinedGroupConversions
                    .Select( c =>
                    {
                        var conversionCommunication = getConversionCommunication( c.PersonAliasId, c.ConversionDateTime );

                        if ( conversionCommunication == null )
                        {
                            return null;
                        }

                        return new CommunicationFlowInstanceConversionHistory
                        {
                            CommunicationFlowCommunicationId = conversionCommunication.Id,
                            CommunicationFlowCommunication = conversionCommunication,
                            CommunicationFlowInstanceId = instance.Id,
                            CommunicationFlowInstance = instance,
                            Date = c.ConversionDateTime,
                            PersonAliasId = c.PersonAliasId
                        };
                    } )
                    // Filter out conversions that already exist in the current conversions.
                    .Where( conversionHistory =>
                        conversionHistory != null
                        && !instance.CommunicationFlowInstanceConversionHistories.Any( h =>
                            h.CommunicationFlowCommunicationId == conversionHistory.CommunicationFlowCommunicationId
                            && h.CommunicationFlowInstanceId == conversionHistory.CommunicationFlowInstanceId
                            && h.PersonAliasId == conversionHistory.PersonAliasId
                            && h.Date == conversionHistory.Date 
                        )
                    )
                    .ToList();
            }
        }

        private sealed class JoinedGroupTypeConversionGoalProcessor : ConversionGoalProcessorBase
        {
            private readonly GroupMemberService _groupMemberService;

            public JoinedGroupTypeConversionGoalProcessor( GroupMemberService groupMemberService )
            {
                _groupMemberService = groupMemberService ?? throw new ArgumentNullException( nameof( groupMemberService ) );
            }

            protected override List<CommunicationFlowInstanceConversionHistory> GetConversionHistories( CommunicationFlowInstance instance, DateTime flowInstanceStartDateTime, DateTime flowInstanceConversionEndDateTime, IEnumerable<int> personAliasIds, GetConversionCommunicationDelegate getConversionCommunication )
            {
                var groupTypeGuid = instance.CommunicationFlow.GetConversionGoalSettings()?.JoinedGroupTypeSettings?.GroupTypeGuid;

                if ( !groupTypeGuid.HasValue )
                {
                    return null;
                }

                var joinedGroupTypeConversions = _groupMemberService
                    .Queryable()
                    .Where( gm =>
                        gm.Group.GroupType.Guid == groupTypeGuid.Value
                        && gm.DateTimeAdded.HasValue
                        && flowInstanceStartDateTime <= gm.DateTimeAdded.Value
                        && gm.DateTimeAdded.Value <= flowInstanceConversionEndDateTime
                        && gm.Person.PrimaryAliasId.HasValue
                        && personAliasIds.Contains( gm.Person.PrimaryAliasId.Value )
                    )
                    .Select( gm => new
                    {
                        PersonAliasId = gm.Person.PrimaryAliasId.Value,
                        ConversionDateTime = gm.DateTimeAdded.Value
                    } )
                    .ToList();

                if ( !joinedGroupTypeConversions.Any() )
                {
                    return null;
                }

                return joinedGroupTypeConversions
                    .Select( c =>
                    {
                        var conversionCommunication = getConversionCommunication( c.PersonAliasId, c.ConversionDateTime );

                        if ( conversionCommunication == null )
                        {
                            return null;
                        }

                        return new CommunicationFlowInstanceConversionHistory
                        {
                            CommunicationFlowCommunicationId = conversionCommunication.Id,
                            CommunicationFlowCommunication = conversionCommunication,
                            CommunicationFlowInstanceId = instance.Id,
                            CommunicationFlowInstance = instance,
                            Date = c.ConversionDateTime,
                            PersonAliasId = c.PersonAliasId
                        };
                    } )
                    // Filter out conversions that already exist in the current conversions.
                    .Where( conversionHistory =>
                        conversionHistory != null
                        && !instance.CommunicationFlowInstanceConversionHistories.Any( h =>
                            h.CommunicationFlowCommunicationId == conversionHistory.CommunicationFlowCommunicationId
                            && h.CommunicationFlowInstanceId == conversionHistory.CommunicationFlowInstanceId
                            && h.PersonAliasId == conversionHistory.PersonAliasId
                            && h.Date == conversionHistory.Date 
                        )
                    )
                    .ToList();
            }
        }
        
        private sealed class RegisteredConversionGoalProcessor : ConversionGoalProcessorBase
        {
            private readonly RegistrationRegistrantService _registrationRegistrantService;

            public RegisteredConversionGoalProcessor( RegistrationRegistrantService registrationRegistrantService )
            {
                _registrationRegistrantService = registrationRegistrantService ?? throw new ArgumentNullException( nameof( registrationRegistrantService ) );
            }

            protected override List<CommunicationFlowInstanceConversionHistory> GetConversionHistories( CommunicationFlowInstance instance, DateTime flowInstanceStartDateTime, DateTime flowInstanceConversionEndDateTime, IEnumerable<int> personAliasIds, GetConversionCommunicationDelegate getConversionCommunication )
            {
                var registrationInstanceGuid = instance.CommunicationFlow.GetConversionGoalSettings()?.RegisteredSettings?.RegistrationInstanceGuid;

                if ( !registrationInstanceGuid.HasValue )
                {
                    return null;
                }

                var registeredConversions = _registrationRegistrantService
                    .Queryable()
                    .Where( rr =>
                        rr.Registration.RegistrationInstance.Guid == registrationInstanceGuid.Value
                        && rr.CreatedDateTime.HasValue
                        && flowInstanceStartDateTime <= rr.CreatedDateTime
                        && rr.CreatedDateTime <= flowInstanceConversionEndDateTime
                        && rr.PersonAliasId.HasValue
                        && personAliasIds.Contains( rr.PersonAliasId.Value )
                    )
                    .Select( rr => new
                    {
                        PersonAliasId = rr.PersonAliasId.Value,
                        ConversionDateTime = rr.CreatedDateTime.Value
                    } )
                    .ToList();

                if ( !registeredConversions.Any() )
                {
                    return null;
                }

                return registeredConversions
                    .Select( c =>
                    {
                        var conversionCommunication = getConversionCommunication( c.PersonAliasId, c.ConversionDateTime );

                        if ( conversionCommunication == null )
                        {
                            return null;
                        }

                        return new CommunicationFlowInstanceConversionHistory
                        {
                            CommunicationFlowCommunicationId = conversionCommunication.Id,
                            CommunicationFlowCommunication = conversionCommunication,
                            CommunicationFlowInstanceId = instance.Id,
                            CommunicationFlowInstance = instance,
                            Date = c.ConversionDateTime,
                            PersonAliasId = c.PersonAliasId
                        };
                    } )
                    // Filter out conversions that already exist in the current conversions.
                    .Where( conversionHistory =>
                        conversionHistory != null
                        && !instance.CommunicationFlowInstanceConversionHistories.Any( h =>
                            h.CommunicationFlowCommunicationId == conversionHistory.CommunicationFlowCommunicationId &&
                            h.CommunicationFlowInstanceId == conversionHistory.CommunicationFlowInstanceId &&
                            h.PersonAliasId == conversionHistory.PersonAliasId &&
                            h.Date == conversionHistory.Date
                        )
                    ).ToList();
            }
        }

        private sealed class TookStepConversionGoalProcessor : ConversionGoalProcessorBase
        {
            private readonly StepService _stepService;

            public TookStepConversionGoalProcessor( StepService stepService )
            {
                _stepService = stepService ?? throw new ArgumentNullException( nameof( stepService ) );
            }

            protected override List<CommunicationFlowInstanceConversionHistory> GetConversionHistories( CommunicationFlowInstance instance, DateTime flowInstanceStartDateTime, DateTime flowInstanceConversionEndDateTime, IEnumerable<int> personAliasIds, GetConversionCommunicationDelegate getConversionCommunication )
            {
                var stepTypeGuid = instance.CommunicationFlow.GetConversionGoalSettings()?.TookStepSettings?.StepTypeGuid;

                if ( !stepTypeGuid.HasValue )
                {
                    return null;
                }

                var tookStepConversions = _stepService
                    .Queryable()
                    .Where( s =>
                        s.StepType.Guid == stepTypeGuid.Value
                        && s.CompletedDateTime.HasValue
                        && flowInstanceStartDateTime <= s.CompletedDateTime.Value
                        && s.CompletedDateTime.Value <= flowInstanceConversionEndDateTime
                        && s.StepStatus.IsCompleteStatus
                        && personAliasIds.Contains( s.PersonAliasId )
                    )
                    .Select( s => new
                    {
                        s.PersonAliasId,
                        ConversionDateTime = s.CompletedDateTime.Value
                    } )
                    .ToList();

                if ( !tookStepConversions.Any() )
                {
                    return null;
                }

                return tookStepConversions
                    .Select( c =>
                    {
                        var conversionCommunication = getConversionCommunication( c.PersonAliasId, c.ConversionDateTime );

                        if ( conversionCommunication == null )
                        {
                            return null;
                        }

                        return new CommunicationFlowInstanceConversionHistory
                        {
                            CommunicationFlowCommunicationId = conversionCommunication.Id,
                            CommunicationFlowCommunication = conversionCommunication,
                            CommunicationFlowInstanceId = instance.Id,
                            CommunicationFlowInstance = instance,
                            Date = c.ConversionDateTime,
                            PersonAliasId = c.PersonAliasId
                        };
                    } )
                    // Filter out conversions that already exist in the current conversions.
                    .Where( conversionHistory =>
                        conversionHistory != null
                        && !instance.CommunicationFlowInstanceConversionHistories.Any( h =>
                            h.CommunicationFlowCommunicationId == conversionHistory.CommunicationFlowCommunicationId &&
                            h.CommunicationFlowInstanceId == conversionHistory.CommunicationFlowInstanceId &&
                            h.PersonAliasId == conversionHistory.PersonAliasId &&
                            h.Date == conversionHistory.Date
                        )
                    ).ToList();
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
            bool EnsureFlowHasInitialInstance( CommunicationFlow flow ); 
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

            public abstract bool EnsureFlowHasInitialInstance( CommunicationFlow flow );

            /// <summary>
            /// Populates <paramref name="instance"/> with CommunicationFlowInstanceRecipients
            /// pulled from flow.TargetAudienceDataViewId. Returns <c>true</c> if it added any.
            /// </summary>
            protected bool AddInitialInstanceRecipients(
                CommunicationFlow flow,
                CommunicationFlowInstance instance
            )
            {
                if ( flow.TargetAudienceDataView == null )
                {
                    return false; // No DataView; nothing to seed.
                }

                // Build the person query exactly as Rock does in the UI
                var personQry = PersonService.GetQueryUsingDataView( flow.TargetAudienceDataView );

                // We only need the PrimaryAliasId for inserts
                var primaryPersonAliasIds = personQry
                    .Where( p => p.PrimaryAliasId.HasValue )
                    .Select( p => p.PrimaryAliasId.Value )
                    .ToList();

                if ( !primaryPersonAliasIds.Any() )
                {
                    return false;
                }

                foreach ( var paId in primaryPersonAliasIds )
                {
                    instance.CommunicationFlowInstanceRecipients.Add(
                        new CommunicationFlowInstanceRecipient
                        {
                            CommunicationFlowInstance = instance,
                            RecipientPersonAliasId = paId,
                            Status = CommunicationFlowInstanceRecipientStatus.Active
                        }
                    );
                }

                return true;
            }
        }

        private sealed class OnDemandTriggerTypeProcessor : ITriggerTypeProcessor
        {
            /// <summary>
            /// This job cannot create On-Demand flows so this method always returns <see langword="false" />.
            /// </summary>
            /// <returns><see langword="false" /></returns>
            public bool EnsureFlowHasInitialInstance( CommunicationFlow flow ) => false;
        }

        private sealed class OneTimeTriggerTypeProcessor : TriggerTypeProcessorBase
        {
            public OneTimeTriggerTypeProcessor( PersonService personService )
                : base( personService )
            { }

            public override bool EnsureFlowHasInitialInstance( CommunicationFlow flow )
            {
                if ( flow.CommunicationFlowInstances.Any() )
                {
                    // Only one instance is allowed for one-time flows.
                    if ( flow.CommunicationFlowInstances.Any( i => i.CommunicationFlowInstanceRecipients.Any() ) )
                    {
                        return false;
                    }
                    else
                    {
                        // Try to regenerate the initial recipients for the last instance.
                        var lastInstance = flow.CommunicationFlowInstances.OrderByDescending( i => i.StartDateTime ).FirstOrDefault();
                        if ( lastInstance != null )
                        {
                            AddInitialInstanceRecipients( flow, lastInstance );

                            if ( lastInstance.CommunicationFlowInstanceRecipients.Any() )
                            {
                                return true;
                            }

                            // Failed to create any recipients. The target data view may not be set or may not have any results.
                            return false;
                        }
                    }
                }

                var schedule = flow.Schedule;
                if ( schedule == null )
                {
                    return false;
                }

                var firstStartDateTime = schedule.FirstStartDateTime ?? RockDateTime.Now;

                var instance = new CommunicationFlowInstance
                {
                    // Need to set parent relationship here since AutoDetectChanges is off,
                    // and adding below will not set these automatically.
                    CommunicationFlowId = flow.Id,
                    CommunicationFlow = flow,
                    StartDateTime = firstStartDateTime
                };

                flow.CommunicationFlowInstances.Add( instance );

                AddInitialInstanceRecipients( flow, instance );

                return true;
            }
        }

        private sealed class RecurringTriggerTypeProcessor : TriggerTypeProcessorBase
        {
            public RecurringTriggerTypeProcessor( PersonService personService )
                : base( personService )
            { }

            public override bool EnsureFlowHasInitialInstance( CommunicationFlow flow )
            {
                var schedule = flow.Schedule;
                if ( schedule == null )
                {
                    return false;
                }

                var lastStartDateTime = flow.CommunicationFlowInstances
                    .OrderByDescending( i => i.StartDateTime )
                    .Select( i => ( DateTime? ) i.StartDateTime ) // cast as DateTime? so lastStart can be null if there are no instances.
                    .FirstOrDefault();

                var nextStartDateTime = lastStartDateTime.HasValue ? schedule.GetNextStartDateTime( lastStartDateTime.Value ) : schedule.FirstStartDateTime;
                if ( !nextStartDateTime.HasValue )
                {
                    // There are no more recurring instances to process for this flow.
                    return false;
                }

                // TODO JMH Should we process if the next instance is within 1 day of now?
                var isFutureInstance = RockDateTime.Now.AddDays( 1 ) < nextStartDateTime.Value;
                if ( isFutureInstance )
                {
                    // Let a future job execution process the future instance.
                    return false;
                }

                var existingInstance = flow.CommunicationFlowInstances.FirstOrDefault( i => i.StartDateTime == nextStartDateTime.Value );
                if ( existingInstance != null )
                {
                    // Ensure the instance has initial recipients.
                    if ( !existingInstance.CommunicationFlowInstanceRecipients.Any() )
                    {
                        AddInitialInstanceRecipients( flow, existingInstance );
                        return existingInstance.CommunicationFlowInstanceRecipients.Any();
                    }

                    return false;
                }

                var instance = new CommunicationFlowInstance
                {
                    // Need to set parent relationship here since AutoDetectChanges is off,
                    // and adding below will not set these automatically.
                    CommunicationFlowId = flow.Id,
                    CommunicationFlow = flow,
                    StartDateTime = nextStartDateTime.Value
                };

                flow.CommunicationFlowInstances.Add( instance );

                AddInitialInstanceRecipients( flow, instance );

                return true;
            }
        }

        #endregion Trigger Type Processors

        #region Helper Types

        private static class ExitConditionHelper
        {
            public static bool PruneRecipients( CommunicationFlowInstance instance )
            {
                var hasChanges = false;

                foreach ( var recipient in instance.CommunicationFlowInstanceRecipients
                    .Where( r => r.Status == CommunicationFlowInstanceRecipientStatus.Active && ShouldPruneRecipient( r ) ) )
                {
                    recipient.Status = CommunicationFlowInstanceRecipientStatus.Inactive;
                    hasChanges |= true;
                }

                return hasChanges;
            }

            private static bool ShouldPruneRecipient( CommunicationFlowInstanceRecipient recipient )
            {
                // Regardless of exit condition, the recipient should exit if they have unsubscribed.
                if ( recipient.UnsubscribeScope.HasValue )
                {
                    return true;
                }

                var exitConditionType = recipient.CommunicationFlowInstance.CommunicationFlow.ExitConditionType;

                switch ( exitConditionType )
                {
                    case ExitConditionType.LastMessageSent:
                        return recipient.CommunicationFlowInstance.CommunicationFlowInstanceCommunications.Count >=
                               recipient.CommunicationFlowInstance.CommunicationFlow.CommunicationFlowCommunications.Count;

                    case ExitConditionType.AnyEmailOpened:
                        // TODO JMH
                        return false; // recipient.HasOpenedAny;

                    case ExitConditionType.AnyEmailClickedThrough:
                        // TODO JMH
                        return false; // recipient.HasClickedAny; 

                    case ExitConditionType.ConversionAchieved:
                        return recipient.CommunicationFlowInstance.CommunicationFlowInstanceConversionHistories.Any( ch => ch.PersonAliasId == recipient.RecipientPersonAliasId );

                    default:
                        return false;
                }
            }
        }

        private sealed class InstanceCommunicationHelper
        {
            private readonly CommunicationService _communicationService;

            private InstanceCommunicationHelper( CommunicationService communicationService )
            {
                _communicationService = communicationService ?? throw new ArgumentNullException( nameof( communicationService ) );
            }

            public static InstanceCommunicationHelper Create( CommunicationService communicationService )
            {
                return new InstanceCommunicationHelper( communicationService );
            }

            public bool CreateNextCommunication( CommunicationFlowInstance instance, out Model.Communication communication )
            {
                /* 1. Pick next unsent communication blueprint */
                var sentIds = instance
                    .CommunicationFlowInstanceCommunications
                    .Select( ic => ic.CommunicationFlowCommunicationId )
                    .ToHashSet();

                var nextBlueprint = instance
                    .CommunicationFlow
                    .CommunicationFlowCommunications
                    .Where( fc => !sentIds.Contains( fc.Id ) )
                    .OrderBy( fc => fc.Order )
                    .FirstOrDefault();

                if ( nextBlueprint == null )
                {
                    // Nothing left to send.
                    communication = null;
                    return false;
                }

                var due = instance.StartDateTime.Date
                    .AddDays( nextBlueprint.DaysToWait )
                    .Add( nextBlueprint.TimeToSend );

                // TODO JMH Should we make this pass when it's within 1 day of the instance start date
                // so we don't miss sending the communication on time?
                if ( RockDateTime.Now.AddDays( 1 ) < due )
                {
                    // Not yet time.
                    communication = null;
                    return false;
                }

                DateTime? futureSendDateTime = null;

                if ( RockDateTime.Now < due )
                {
                    futureSendDateTime = due;
                }

                /* 2. Gather active recipients. */
                var activeRecipientPersonAliasIds = instance
                    .CommunicationFlowInstanceRecipients
                   .Where( r => r.Status == CommunicationFlowInstanceRecipientStatus.Active )
                   .Select( r => r.RecipientPersonAliasId )
                   .ToList();

                /* 2a. No one left to send to. */
                if ( !activeRecipientPersonAliasIds.Any() )
                {
                    // Add an instance communication without sending an actual communication.
                    instance.CommunicationFlowInstanceCommunications.Add(
                        new CommunicationFlowInstanceCommunication
                        {
                            CommunicationFlowInstanceId = instance.Id,
                            CommunicationFlowCommunicationId = nextBlueprint.Id
                        } );
                    
                    communication = null;
                    return true; // Context modified even though no communication is needed.
                }

                /* 3. Create Communication (no SaveChanges here) */
                communication = CreateCommunicationFromTemplate( nextBlueprint, activeRecipientPersonAliasIds, futureSendDateTime );

                /* Link IC */
                instance.CommunicationFlowInstanceCommunications.Add(
                    new CommunicationFlowInstanceCommunication
                    {
                        CommunicationFlowInstanceId = instance.Id,
                        CommunicationFlowInstance = instance,
                        CommunicationFlowCommunicationId = nextBlueprint.Id,
                        CommunicationFlowCommunication = nextBlueprint,
                        Communication = communication
                    } );

                return true;
            }

            private Model.Communication CreateCommunicationFromTemplate(
                CommunicationFlowCommunication blueprint,
                List<int> activeRecipientPersonAliasIds,
                DateTime? scheduledSendDateTime
            )
            {
                var template = blueprint.CommunicationTemplate;

                if ( blueprint.CommunicationType == Model.CommunicationType.Email )
                {
                    return _communicationService.CreateEmailCommunication( new CommunicationService.CreateEmailCommunicationArgs
                    {
                        BulkCommunication = false,
                        CommunicationTemplateId = template.Id,
                        FromAddress = template.FromEmail,
                        FromName = template.FromName,
                        FutureSendDateTime = scheduledSendDateTime,
                        Message = template.Message,
                        Name = $"{blueprint.CommunicationFlow.Name} – {blueprint.Name}",
                        RecipientPrimaryPersonAliasIds = activeRecipientPersonAliasIds,
                        RecipientStatus = CommunicationRecipientStatus.Pending,
                        ReplyTo = template.ReplyToEmail,
                        SendDateTime = null, // This is actually the "sent" value and must be null here.
                        SenderPersonAliasId = template.SenderPersonAliasId,
                        Subject = template.Subject,
                        SystemCommunicationId = null
                    } );
                }
                else if ( blueprint.CommunicationType == Model.CommunicationType.SMS )
                {
                    return _communicationService.CreateSMSCommunication( new CommunicationService.CreateSMSCommunicationArgs
                    {
                        CommunicationName = $"{blueprint.CommunicationFlow.Name} – {blueprint.Name}",
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
                else if ( blueprint.CommunicationType == Model.CommunicationType.PushNotification )
                {
                    // TODO JMH Implement.
                    return null;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion Helper Types
    }
}