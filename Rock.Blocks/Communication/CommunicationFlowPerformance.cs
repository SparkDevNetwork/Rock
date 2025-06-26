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
            var box = new InitializationBox();

            box.NavigationUrls = GetBoxNavigationUrls();
            box.CommunicationFlow = GetCommunicationFlow( this.RockContext );
            box.GridDefinition = GetGridBuilder().BuildDefinition();
            box.MessageGridItems = GetMessageGridItems( box.CommunicationFlow );
            
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

        private CommunicationFlowBag GetCommunicationFlow( RockContext rockContext )
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

            var bag = new CommunicationFlowBag
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

        private List<CommunicationFlowInstanceBag> GetCommunicationFlowInstanceBags( IEnumerable<CommunicationFlowInstance> communicationFlowInstances )
        {
            var communicationFlowInstanceBags = new List<CommunicationFlowInstanceBag>();

            // Process instance data.
            foreach ( var communicationFlowInstance in communicationFlowInstances )
            {
                var instanceBag = new CommunicationFlowInstanceBag
                {
                    CommunicationFlowInstanceIdKey = communicationFlowInstance.IdKey,
                    StartDate = communicationFlowInstance.StartDate,
                    Conversions = GetCommunicationFlowInstanceConversionHistoryBags( communicationFlowInstance.CommunicationFlowInstanceConversionHistories ),
                    InitialRecipients = GetCommunicationFlowInstanceRecipientBags( communicationFlowInstance.CommunicationFlowInstanceRecipients ),
                    Communications = GetCommunicationFlowInstanceCommunicationBags( communicationFlowInstance.CommunicationFlowInstanceCommunications ),
                    HasSentCommunications = communicationFlowInstance.CommunicationFlowInstanceCommunications.Any( c => c.Communication.SendDateTime.HasValue && RockDateTime.Now > c.Communication.SendDateTime.Value ),
                    HasUnsentCommunications = communicationFlowInstance.CommunicationFlowInstanceCommunications.Count < communicationFlowInstance.CommunicationFlow.CommunicationFlowCommunications.Count
                };

                communicationFlowInstanceBags.Add( instanceBag );
            }

            return communicationFlowInstanceBags;
        }

        private List<CommunicationFlowInstanceCommunicationBag> GetCommunicationFlowInstanceCommunicationBags( ICollection<CommunicationFlowInstanceCommunication> communicationFlowInstanceCommunications )
        {
            var communicationBags = new List<CommunicationFlowInstanceCommunicationBag>();

            foreach ( var communicationFlowInstanceCommunication in communicationFlowInstanceCommunications )
            {
                communicationBags.Add( new CommunicationFlowInstanceCommunicationBag
                {
                    CommunicationFlowInstanceCommunicationIdKey = communicationFlowInstanceCommunication.IdKey,
                    CommunicationFlowCommunicationIdKey = communicationFlowInstanceCommunication.CommunicationFlowCommunication.IdKey
                } );
            }

            return communicationBags;
        }

        private List<CommunicationFlowInstanceRecipientBag> GetCommunicationFlowInstanceRecipientBags( ICollection<CommunicationFlowInstanceRecipient> communicationFlowInstanceRecipients )
        {
            var recipientBags = new List<CommunicationFlowInstanceRecipientBag>();

            foreach ( var communicationFlowInstanceRecipient in communicationFlowInstanceRecipients )
            {
                var recipientBag = new CommunicationFlowInstanceRecipientBag
                {
                    CausedUnsubscribe = communicationFlowInstanceRecipient.UnsubscribeCommunicationRecipientId.HasValue,
                    UnsubscribeDateTime = communicationFlowInstanceRecipient.UnsubscribeCommunicationRecipient?.UnsubscribeDateTime,
                    UnsubscribeLevel = communicationFlowInstanceRecipient.UnsubscribeCommunicationRecipient?.UnsubscribeLevel
                };

                recipientBags.Add( recipientBag );
            }

            return recipientBags;
        }

        private List<CommunicationFlowInstanceConversionHistoryBag> GetCommunicationFlowInstanceConversionHistoryBags( IEnumerable<CommunicationFlowInstanceConversionHistory> conversionHistories )
        {
            var conversionHistoryBags = new List<CommunicationFlowInstanceConversionHistoryBag>();

            foreach ( var conversionHistory in conversionHistories )
            {
                var conversionHistoryBag = new CommunicationFlowInstanceConversionHistoryBag
                {
                    Date = conversionHistory.Date
                };

                conversionHistoryBags.Add( conversionHistoryBag );
            }

            return conversionHistoryBags;
        }

        private ConversionGoalSettingsBag GetConversionGoalSettingsBag( CommunicationFlow entity )
        {
            var settings = entity.GetConversionGoalSettings();

            if ( settings == null )
            {
                return null;
            }

            var bag = new ConversionGoalSettingsBag();

            if ( settings.CompletedFormSettings != null )
            {
                bag.CompletedFormSettings = new CompletedFormSettingsBag
                {
                    WorkflowType = WorkflowTypeCache.Get( settings.CompletedFormSettings.WorkflowTypeGuid ).ToListItemBag(),
                };
            }

            if ( settings.JoinedGroupTypeSettings != null )
            {
                bag.JoinedGroupTypeSettings = new JoinedGroupTypeSettingsBag
                {
                    GroupType = GroupTypeCache.Get( settings.JoinedGroupTypeSettings.GroupTypeGuid ).ToListItemBag()
                };
            }

            if ( settings.JoinedGroupSettings != null )
            {
                bag.JoinedGroupSettings = new JoinedGroupSettingsBag
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

                bag.RegisteredSettings = new RegisteredSettingsBag
                {
                    RegistrationInstance = registrationInstance
                };
            }

            if ( settings.TookStepSettings != null )
            {
                bag.TookStepSettings = new TookStepSettingsBag
                {
                    StepType = StepTypeCache.Get( settings.TookStepSettings.StepTypeGuid ).ToListItemBag()
                };
            }

            if ( settings.EnteredDataViewSettings != null )
            {
                bag.EnteredDataViewSettings = new EnteredDataViewSettingsBag
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

        private GridBuilder<MessageGridItemBag> GetGridBuilder()
        {
            return new GridBuilder<MessageGridItemBag>()
                .WithBlock( this )
                .AddField( nameof( MessageGridItemBag.CommunicationFlowCommunicationIdKey ).ToCamelCase(), b => b.CommunicationFlowCommunicationIdKey )
                .AddField( nameof( MessageGridItemBag.CommunicationFlowInstanceCommunicationIdKey ).ToCamelCase(), b => b.CommunicationFlowInstanceCommunicationIdKey )
                .AddTextField( nameof( MessageGridItemBag.CommunicationName ).ToCamelCase(), b => b.CommunicationName )
                .AddField( nameof( MessageGridItemBag.CommunicationType ).ToCamelCase(), b => b.CommunicationType )
                .AddField( nameof( MessageGridItemBag.Sent ).ToCamelCase(), b => b.Sent )
                .AddField( nameof( MessageGridItemBag.Conversions ).ToCamelCase(), b => b.Conversions )
                .AddField( nameof( MessageGridItemBag.Unsubscribes ).ToCamelCase(), b => b.Unsubscribes )
                .AddField( nameof( MessageGridItemBag.Opens ).ToCamelCase(), b => b.Opens )
                .AddField( nameof( MessageGridItemBag.Clicks ).ToCamelCase(), b => b.Clicks );
        }

        private List<MessageGridItemBag> GetMessageGridItems( CommunicationFlow communicationFlow )
        {
            var interactionChannelId = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() ) ?? 0;
            var interactionQuery = new InteractionService( this.RockContext ).Queryable();

            return new CommunicationFlowService( this.RockContext )
                .GetQueryableByKey( communicationFlow.IdKey, !this.PageCache.Layout.Site.DisablePredictableIds )
                .SelectMany( f => f.CommunicationFlowCommunications )
                .SelectMany( fc => fc.CommunicationFlowInstanceCommunications )
                .Select( fic => new
                {
                    CommunicationFlowInstanceCommunicationId = fic.Id,
                    CommunicationFlowCommunicationId = fic.Communication.Id,
                    MessageName = fic.CommunicationFlowCommunication.Name,
                    CommunicationType = ( Enums.Communication.CommunicationType ) ( int ) fic.CommunicationFlowCommunication.CommunicationType,

                    // Sent = distinct CommunicationRecipients underneath this blueprint message.
                    Sent = fic.Communication.Recipients
                        .Select( cr => cr.Id )
                        .Distinct()
                        .Count(),

                    // Conversions = distinct ConversionHistory rows tied to any instance that contains this message.
                    Conversions = fic.CommunicationFlowInstance.CommunicationFlowInstanceConversionHistories
                        .Select( ch => ch.Id )
                        .Distinct()
                        .Count(),

                    // Unsubscribes = instance recipients that actually unsubscribed.
                    Unsubscribes = fic.CommunicationFlowInstance.CommunicationFlowInstanceRecipients
                        .Where( r => r.UnsubscribeCommunicationRecipient.CommunicationId == fic.CommunicationId )
                        .Select( r => r.Id )
                        .Distinct()
                        .Count(),

                    // Opens = open interactions
                    Opens =
                        // "Real" opens
                        interactionQuery
                            .Where( ix =>
                                ix.InteractionComponent.InteractionChannelId == interactionChannelId
                                && ix.InteractionComponent.EntityId == fic.Communication.Id
                                && ix.Operation == "Opened"
                                && ix.EntityId != null
                            )
                            .Select( ix => ix.EntityId.Value )
                        // "Inferred" opens - communication recipients with "Click" but no "Open" interactions.
                        .Union(
                            interactionQuery
                                .Where( ix =>
                                        ix.InteractionComponent.InteractionChannelId == interactionChannelId
                                        && ix.InteractionComponent.EntityId == fic.CommunicationId
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
                        .Distinct()
                        .Count(),

                    // Clicks = click interactions
                    Clicks = interactionQuery
                        .Where( ix =>
                            ix.InteractionComponent.InteractionChannelId == interactionChannelId
                            && ix.InteractionComponent.EntityId == fic.Communication.Id
                            && ix.Operation == "Click"
                            && ix.EntityId != null
                        )
                        .Select( ix => ix.EntityId.Value )
                        .Distinct()
                        .Count()
                } )
                // Execute query then transform to final data structure.
                .ToList()
                .Select( d =>
                {
                    return new MessageGridItemBag
                    {
                        Clicks = d.Clicks,
                        CommunicationType = d.CommunicationType,
                        Conversions = d.Conversions,
                        CommunicationFlowCommunicationIdKey = d.CommunicationFlowCommunicationId != 0 ? IdHasher.Instance.GetHash( d.CommunicationFlowCommunicationId ) : string.Empty,
                        CommunicationFlowInstanceCommunicationIdKey = d.CommunicationFlowInstanceCommunicationId != 0 ? IdHasher.Instance.GetHash( d.CommunicationFlowInstanceCommunicationId ) : string.Empty,
                        CommunicationName = d.MessageName,
                        Opens = d.Opens,
                        Sent = d.Sent,
                        Unsubscribes = d.Unsubscribes
                    };
                } )
                .ToList();
        }

        #endregion
    }
}
