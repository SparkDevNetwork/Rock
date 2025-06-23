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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Utility;
using Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Displays the performance of a particular communication flow.
    /// </summary>

    [DisplayName( "Communication Flow Performance" )]
    [Category( "Communication" )]
    [Description( "Displays the performance of a particular communication flow." )]
    [IconCssClass( "fa fa-line-chart" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Message Metrics Page",
        Description = "The page that will show the communication flow instance message metrics.",
        Key = AttributeKey.MessageMetricsPage )]

    #endregion
    
    [Rock.SystemGuid.EntityTypeGuid( "53FFF4C5-1F30-415C-BE65-53FCA7F2CD64" )]
    [Rock.SystemGuid.BlockTypeGuid( "72B92AA4-2AA7-4FDD-9CD7-7DD84018B21E" )]
    public class CommunicationFlowPerformance : RockBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string MessageMetricsPage = "MessageMetricsPage";
        }

        private static class PageParameterKey
        {
            public const string CommunicationFlow = "CommunicationFlow";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string MessageMetricsPage = "MessageMetricsPage";
            public const string PersonProfilePage = "PersonProfilePage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CommunicationFlowPerformanceInitializationBox();

            box.NavigationUrls = GetBoxNavigationUrls();
            box.CommunicationFlow = GetFlow( this.RockContext );
            box.GridDefinition = GetGridBuilder().BuildDefinition();
            
            if ( box.CommunicationFlow != null )
            {
                this.ResponseContext.SetPageTitle( box.CommunicationFlow.Name.ToStringOrDefault( "Flow Performance" ) );
            }
            else
            {
                this.ResponseContext.SetPageTitle( "Flow Performance" );
            }

            return box;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl(),
                [NavigationUrlKey.MessageMetricsPage] = this.GetLinkedPageUrl( AttributeKey.MessageMetricsPage, new Dictionary<string, string>
                {
                    { "CommunicationFlow", PageParameter( PageParameterKey.CommunicationFlow ) },
                    { "CommunicationFlowInstance", "((CommunicationFlowInstanceKey))" },
                    { "CommunicationFlowInstanceCommunication", "((Key))" }
                } )
            };
        }

        private CommunicationFlowPerformanceFlowBag GetFlow( RockContext rockContext )
        {
            var flowKey = PageParameter( PageParameterKey.CommunicationFlow );
            if ( flowKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var flow = new CommunicationFlowService( rockContext )
                .GetQueryableByKey( flowKey, !this.PageCache.Layout.Site.DisablePredictableIds )
                .FirstOrDefault(); // Zero Include() for fast and lean query.

            if ( flow == null )
            {
                return null;
            }

            // Attach the conversion histories.
            rockContext.Entry( flow )
                .Collection( f => f.CommunicationFlowInstances )
                .Query()
                .Include( i => i.CommunicationFlowInstanceConversionHistories )
                .Load();

            // Attach the recipients.
            foreach ( var instance in flow.CommunicationFlowInstances )
            {
                rockContext.Entry( instance )
                    .Collection( i => i.CommunicationFlowInstanceRecipients )
                    .Query()
                    .Include( i => i.UnsubscribeCommunicationRecipient )
                    .Load();
            }

            var bag = new CommunicationFlowPerformanceFlowBag
            {
                IdKey = flow.IdKey,
                ConversionGoalType = flow.ConversionGoalType,
                ConversionGoalTargetPercent = flow.ConversionGoalTargetPercent,
                ConversionGoalTimeframeInDays = flow.ConversionGoalTimeframeInDays,
                Name = flow.Name,
                TriggerType = flow.TriggerType,
                ConversionGoalSettings = GetConversionGoalSettingsBag( flow ),
                Instances = GetCommunicationFlowInstanceBags( flow.CommunicationFlowInstances )
            };

            return bag;
        }

        private List<CommunicationFlowPerformanceFlowInstanceBag> GetCommunicationFlowInstanceBags( IEnumerable<CommunicationFlowInstance> communicationFlowInstances )
        {
            var communicationFlowInstanceBags = new List<CommunicationFlowPerformanceFlowInstanceBag>();

            // Process instance data.
            foreach ( var communicationFlowInstance in communicationFlowInstances )
            {
                var instanceBag = new CommunicationFlowPerformanceFlowInstanceBag
                {
                    IdKey = communicationFlowInstance.IdKey,
                    StartDate = communicationFlowInstance.StartDate,
                    Conversions = GetCommunicationFlowInstanceConversionHistoryBags( communicationFlowInstance.CommunicationFlowInstanceConversionHistories ),
                    Recipients = GetCommunicationFlowInstanceRecipientBags( communicationFlowInstance.CommunicationFlowInstanceRecipients ),
                    Communications = GetCommunicationFlowInstanceCommunicationBags( communicationFlowInstance.CommunicationFlowInstanceCommunications ),
                    HasSentCommunications = communicationFlowInstance.CommunicationFlowInstanceCommunications.Any( c => c.Communication.SendDateTime.HasValue && RockDateTime.Now > c.Communication.SendDateTime.Value ),
                    HasUnsentCommunications = communicationFlowInstance.CommunicationFlowInstanceCommunications.Count < communicationFlowInstance.CommunicationFlow.CommunicationFlowCommunications.Count
                };

                communicationFlowInstanceBags.Add( instanceBag );
            }

            return communicationFlowInstanceBags;
        }

        private List<CommunicationFlowPerformanceInstanceCommunicationBag> GetCommunicationFlowInstanceCommunicationBags( ICollection<CommunicationFlowInstanceCommunication> communicationFlowInstanceCommunications )
        {
            var communicationBags = new List<CommunicationFlowPerformanceInstanceCommunicationBag>();

            foreach ( var communicationFlowInstanceCommunication in communicationFlowInstanceCommunications )
            {
                communicationBags.Add( new CommunicationFlowPerformanceInstanceCommunicationBag
                {
                    IdKey = communicationFlowInstanceCommunication.IdKey,
                    CommunicationFlowCommunicationIdKey = communicationFlowInstanceCommunication.CommunicationFlowCommunication.IdKey
                } );
            }

            return communicationBags;
        }

        private List<CommunicationFlowPerformanceRecipientBag> GetCommunicationFlowInstanceRecipientBags( ICollection<CommunicationFlowInstanceRecipient> communicationFlowInstanceRecipients )
        {
            var recipientBags = new List<CommunicationFlowPerformanceRecipientBag>();

            foreach ( var communicationFlowInstanceRecipient in communicationFlowInstanceRecipients )
            {
                var recipientBag = new CommunicationFlowPerformanceRecipientBag
                {
                    CausedUnsubscribe = communicationFlowInstanceRecipient.UnsubscribeCommunicationRecipientId.HasValue,
                    UnsubscribeDateTime = communicationFlowInstanceRecipient.UnsubscribeCommunicationRecipient?.UnsubscribeDateTime,
                    UnsubscribeLevel = communicationFlowInstanceRecipient.UnsubscribeCommunicationRecipient?.UnsubscribeLevel
                };

                recipientBags.Add( recipientBag );
            }

            return recipientBags;
        }

        private List<CommunicationFlowPerformanceConversionHistoryBag> GetCommunicationFlowInstanceConversionHistoryBags( IEnumerable<CommunicationFlowInstanceConversionHistory> conversionHistories )
        {
            var conversionHistoryBags = new List<CommunicationFlowPerformanceConversionHistoryBag>();

            foreach ( var conversionHistory in conversionHistories )
            {
                var conversionHistoryBag = new CommunicationFlowPerformanceConversionHistoryBag
                {
                    Date = conversionHistory.Date
                };

                conversionHistoryBags.Add( conversionHistoryBag );
            }

            return conversionHistoryBags;
        }

        private CommunicationFlowPerformanceConversionGoalSettingsBag GetConversionGoalSettingsBag( CommunicationFlow entity )
        {
            var settings = entity.GetConversionGoalSettings();

            if ( settings == null )
            {
                return null;
            }

            var bag = new CommunicationFlowPerformanceConversionGoalSettingsBag();

            if ( settings.CompletedFormSettings != null )
            {
                bag.CompletedFormSettings = new CommunicationFlowPerformanceCompletedFormSettingsBag
                {
                    WorkflowType = WorkflowTypeCache.Get( settings.CompletedFormSettings.WorkflowTypeGuid ).ToListItemBag(),
                };
            }

            if ( settings.JoinedGroupTypeSettings != null )
            {
                bag.JoinedGroupTypeSettings = new CommunicationFlowPerformanceJoinedGroupTypeSettingsBag
                {
                    GroupType = GroupTypeCache.Get( settings.JoinedGroupTypeSettings.GroupTypeGuid ).ToListItemBag()
                };
            }

            if ( settings.JoinedGroupSettings != null )
            {
                bag.JoinedGroupSettings = new CommunicationFlowPerformanceJoinedGroupSettingsBag
                {
                    Group = GroupCache.Get( settings.JoinedGroupSettings.GroupGuid ).ToListItemBag()
                };
            }

            if ( settings.RegisteredSettings != null )
            {
                var registrationInstance = new RegistrationInstanceService( this.RockContext )
                    .Queryable()
                    .Where( ri => ri.Guid == settings.RegisteredSettings.RegistrationInstanceGuid )
                    .Select( ri => new ListItemBag
                    {
                        Text = ri.Name,
                        Value = ri.Guid.ToString()
                    } )
                    .FirstOrDefault();

                bag.RegisteredSettings = new CommunicationFlowPerformanceRegisteredSettingsBag
                {
                    RegistrationInstance = registrationInstance
                };
            }

            if ( settings.TookStepSettings != null )
            {
                bag.TookStepSettings = new CommunicationFlowPerformanceTookStepSettingsBag
                {
                    StepType = StepTypeCache.Get( settings.TookStepSettings.StepTypeGuid ).ToListItemBag()
                };
            }

            if ( settings.EnteredDataViewSettings != null )
            {
                bag.EnteredDataViewSettings = new CommunicationFlowPerformanceEnteredDataViewSettingsBag
                {
                    DataView = DataViewCache.Get( settings.EnteredDataViewSettings.DataViewGuid ).ToListItemBag()
                };
            }

            return bag;
        }
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var entityKey = pageReference.GetPageParameter( PageParameterKey.CommunicationFlow );
            var arePredictableIdsEnabled = !this.PageCache.Layout.Site.DisablePredictableIds;
            var title = new CommunicationFlowService( this.RockContext )
                .GetSelect( entityKey, f => f.Name, arePredictableIdsEnabled );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb>
                {
                    new BreadCrumbLink( title ?? "Flow Performance", pageReference )
                }
            };
        }

        private GridBuilder<CommunicationFlowPerformanceGridItemBag> GetGridBuilder()
        {
            return new GridBuilder<CommunicationFlowPerformanceGridItemBag>()
                .WithBlock( this )
                .AddField( "idKey", b => b.IdKey )
                .AddTextField( "messageName", b => b.MessageName )
                .AddField( "communicationType", b => b.CommunicationType )
                .AddField( "sent", b => b.Sent )
                .AddField( "conversions", b => b.Conversions )
                .AddField( "unsubscribes", b => b.Unsubscribes )
                .AddField( "opens", b => b.Opens )
                .AddField( "clicks", b => b.Clicks );
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult GetRowData()
        {
            var flowKey = PageParameter( PageParameterKey.CommunicationFlow );
            if ( flowKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var interactionChannelId = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() ) ?? 0;
            var interactionQuery = new InteractionService( this.RockContext ).Queryable();

            var data = new CommunicationFlowService( this.RockContext )
                .GetQueryableByKey( flowKey, !this.PageCache.Layout.Site.DisablePredictableIds )
                .SelectMany( f => f.CommunicationFlowCommunications )
                .Select( fc => new
                {
                    Id = fc.Id,
                    MessageName = fc.Name,
                    CommunicationType = ( Enums.Communication.CommunicationType ) ( int ) fc.CommunicationType,

                    // Sent = distinct CommunicationRecipients underneath this blueprint message.
                    Sent = fc.CommunicationFlowInstanceCommunications
                        .SelectMany( ic => ic.Communication.Recipients )
                        .Select( cr => cr.Id )
                        .Distinct()
                        .Count(),

                    // Conversions = distinct ConversionHistory rows tied to any instance that contains this message.
                    Conversions = fc.CommunicationFlowInstanceCommunications
                        .SelectMany( ic => ic.CommunicationFlowInstance.CommunicationFlowInstanceConversionHistories )
                        .Select( ch => ch.Id )
                        .Distinct()
                        .Count(),

                    // Unsubscribes = instance recipients that actually unsubscribed.
                    Unsubscribes = fc.CommunicationFlowInstanceCommunications
                        .SelectMany( ic =>
                            ic.CommunicationFlowInstance
                                .CommunicationFlowInstanceRecipients
                                .Where( r => r.UnsubscribeCommunicationRecipientId != null )
                        )
                        .Select( r => r.Id )
                        .Distinct()
                        .Count(),

                    // Opens = open interactions
                    Opens = fc.CommunicationFlowInstanceCommunications
                        .SelectMany( ic =>
                            // "Real" opens
                            interactionQuery
                                .Where( ix =>
                                    ix.InteractionComponent.InteractionChannelId == interactionChannelId
                                    && ix.InteractionComponent.EntityId == ic.Communication.Id
                                    && ix.Operation == "Opened"
                                    && ix.EntityId != null
                                )
                                .Select( ix => ix.EntityId.Value )
                            // "Inferred" opens - communication recipients with "Click" but no "Open" interactions.
                            .Union(
                                interactionQuery
                                    .Where( ix =>
                                            ix.InteractionComponent.InteractionChannelId == interactionChannelId
                                            && ix.InteractionComponent.EntityId == ic.CommunicationId
                                            && ix.Operation == "Click"
                                            && ix.EntityId != null
                                            // No matching "Opened" for the same communication recipient.
                                            && !interactionQuery.Any( ox =>
                                                ox.InteractionComponent.InteractionChannelId == interactionChannelId
                                                && ox.InteractionComponent.EntityId == ix.InteractionComponent.EntityId
                                                && ox.Operation == "Opened"
                                                && ox.EntityId == ix.EntityId
                                            )
                                    )
                                    .Select( ix => ix.EntityId.Value )
                            )
                        )
                        .Distinct()
                        .Count(),

                    // Clicks = click interactions
                    Clicks = fc.CommunicationFlowInstanceCommunications
                        .SelectMany( ic =>
                            interactionQuery
                                .Where( ix =>
                                    ix.InteractionComponent.InteractionChannelId == interactionChannelId
                                    && ix.InteractionComponent.EntityId == ic.Communication.Id
                                    && ix.Operation == "Click"
                                    && ix.EntityId != null
                                )
                                .Select( ix => ix.EntityId.Value )
                        )
                        .Distinct()
                        .Count()
                } )
                // Execute query then transform to final data structure.
                .ToList()
                .Select( d =>
                {
                    return new CommunicationFlowPerformanceGridItemBag
                    {
                        Clicks = d.Clicks,
                        CommunicationType = d.CommunicationType,
                        Conversions = d.Conversions,
                        IdKey = d.Id != 0 ? IdHasher.Instance.GetHash( d.Id ) : string.Empty,
                        MessageName = d.MessageName,
                        Opens = d.Opens,
                        Sent = d.Sent,
                        Unsubscribes = d.Unsubscribes
                    };
                } )
                .ToList();

            var builder = GetGridBuilder();
            var gridDataBag = builder.Build( data );
        
            return ActionOk( gridDataBag );
        }

        #endregion
    }
}
