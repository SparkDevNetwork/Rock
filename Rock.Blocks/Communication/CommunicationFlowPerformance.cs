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
using System.Linq;
using System.Linq.Dynamic.Core;

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

        private string CommunicationFlowPageParameter => PageParameter( PageParameterKey.CommunicationFlow );

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new InitializationBox();

            var allowIntegerIds = !this.PageCache.Layout.Site.DisablePredictableIds;

            box.NavigationUrls = GetBoxNavigationUrls();
            box.GridDefinition = GetGridBuilder().BuildDefinition();
            //box.MessageGridItems = GetMessageGridItems( box.CommunicationFlow );
            box.FlowPerformance = GetFlowPerformance( RockContext, CommunicationFlowPageParameter, allowIntegerIds );
            
            if ( box.FlowPerformance != null )
            {
                this.ResponseContext.SetPageTitle( box.FlowPerformance.CommunicationFlowName.ToStringOrDefault( "Flow Performance" ) );
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

        private PerformanceBag GetFlowPerformance( RockContext rockContext, string flowKey, bool allowIntegerIds )
        {
            var commChannelId = InteractionChannelCache
                    .Get( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() )?.Id ?? 0;

            var ctx = new Ctx( rockContext );

            var ( communicationFlowPerformanceBag, communicationFlowId ) = ctx.CommunicationFlows
                .WhereKeyIs( flowKey, allowIntegerIds )
                .Select( cf => new
                {
                    CommunicationFlowId = cf.Id,
                    cf.AdditionalSettingsJson,
                    cf.ConversionGoalType,
                    cf.ConversionGoalTargetPercent,
                    cf.ConversionGoalTimeframeInDays,
                    cf.TriggerType,
                    cf.Name
                })
                .ToList()
                .Select( cf =>
                    (
                        new PerformanceBag
                        {
                            CommunicationFlowIdKey = IdHasher.Instance.GetHash( cf.CommunicationFlowId ),
                            ConversionGoalSettings = GetConversionGoalSettingsBag( cf.AdditionalSettingsJson.AsAdditionalSettings().GetAdditionalSettings<CommunicationFlow.ConversionGoalSettings>() ),
                            ConversionGoalType = cf.ConversionGoalType,
                            ConversionGoalTargetPercent = cf.ConversionGoalTargetPercent,
                            ConversionGoalTimeframeInDays = cf.ConversionGoalTimeframeInDays,
                            TriggerType = cf.TriggerType,
                            CommunicationFlowName = cf.Name
                        },
                        cf.CommunicationFlowId
                    )
                )
                .FirstOrDefault();

            if ( communicationFlowPerformanceBag?.ConversionGoalType == null )
            {
                // TODO JMH If there is no conversion goal, move on without processing further?
                return communicationFlowPerformanceBag;
            }

            // Get message instances. These are the actual messages that were (or will be) sent to recipients.
            communicationFlowPerformanceBag.Messages = ctx.CommunicationFlowInstanceCommunications
                .Where( ic => ic.CommunicationFlowCommunication.CommunicationFlowId == communicationFlowId )
                .SelectMany( ic => ic.Communication.Recipients
                    .Select( cr => new
                    {
                        ic.CommunicationFlowCommunicationId,
                        ic.CommunicationFlowInstanceId,
                        CommunicationFlowInstanceCompletedDateTime = ic.CommunicationFlowInstance.CompletedDateTime,
                        CommunicationFlowInstanceCommunicationId = ic.Id,
                        cr.PersonAliasId,
                        Sent = cr.SendDateTime,
                        Opened = cr.OpenedDateTime,
                        Unsubscribed = cr.UnsubscribeDateTime,
                        cr.UnsubscribeLevel,
                        CommunicationFlowCommunicationName = ic.CommunicationFlowCommunication.Name,
                        cr.Communication.CommunicationType,
                        CommunicationFlowInstanceStartDateTime = ic.CommunicationFlowInstance.StartDateTime,
                        CommunicationFlowInstanceLastProcessedDateTime = ic.CommunicationFlowInstance.LastProcessedDateTime,

                        Clicked =
                            ctx.Interactions
                               .Where( ix =>
                                       ix.InteractionComponent.InteractionChannelId == commChannelId
                                       && ix.InteractionComponent.EntityId == ic.CommunicationId
                                       && ix.Operation == "Click"
                                       && ix.EntityId == cr.Id )
                               .OrderByDescending( ix => ix.InteractionDateTime )
                               .Select( ix => ( DateTime? ) ix.InteractionDateTime )
                               .FirstOrDefault(),

                        Converted =
                            ctx.CommunicationFlowInstanceConversionHistories
                               .Where( ch =>
                                       ch.CommunicationFlowInstanceId == ic.CommunicationFlowInstanceId
                                       && ch.PersonAliasId == cr.PersonAliasId )
                               .OrderBy( ch => ch.Date )
                               .Select( ch => ( DateTime? ) ch.Date )
                               .FirstOrDefault()
                    } )
                )
                .ToList()
                .Select( cr => new MessageBag
                {
                    CommunicationFlowCommunicationIdKey = IdHasher.Instance.GetHash( cr.CommunicationFlowCommunicationId ),
                    CommunicationFlowInstanceIdKey = IdHasher.Instance.GetHash( cr.CommunicationFlowInstanceId ),
                    CommunicationFlowInstanceCommunicationIdKey = IdHasher.Instance.GetHash( cr.CommunicationFlowInstanceCommunicationId ),
                    PersonAliasIdKey = cr.PersonAliasId.HasValue ? IdHasher.Instance.GetHash( cr.PersonAliasId.Value ) : string.Empty,
                    CommunicationFlowCommunicationName = cr.CommunicationFlowCommunicationName,
                    CommunicationType = ( Enums.Communication.CommunicationType ) ( int ) cr.CommunicationType,
                    SentDateTime = cr.Sent,
                    OpenedDateTime = cr.Opened,
                    UnsubscribedDateTime = cr.Unsubscribed,
                    UnsubscribeLevel = cr.UnsubscribeLevel,
                    ClickedDateTime = cr.Clicked,
                    ConvertedDateTime = cr.Converted,
                    CommunicationFlowInstanceStartDateTime = cr.CommunicationFlowInstanceStartDateTime,
                    CommunicationFlowInstanceCompletedDateTime = cr.CommunicationFlowInstanceCompletedDateTime,
                } )
                .ToList();

            return communicationFlowPerformanceBag;
        }

        private ConversionGoalSettingsBag GetConversionGoalSettingsBag( CommunicationFlow.ConversionGoalSettings settings )
        {
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
                var registrationInstance = new RegistrationInstanceService( RockContext )
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
            var title = new CommunicationFlowService( RockContext )
                .GetSelect( entityKey, f => f.Name, arePredictableIdsEnabled );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb>
                {
                    new BreadCrumbLink( title ?? "Flow Performance", pageReference )
                }
            };
        }

        private GridBuilder<GridRowBag> GetGridBuilder()
        {
            return new GridBuilder<GridRowBag>()
                .WithBlock( this )
                .AddField( nameof( GridRowBag.CommunicationFlowCommunicationIdKey ).ToCamelCase(), b => b.CommunicationFlowCommunicationIdKey )
                .AddField( nameof( GridRowBag.CommunicationFlowInstanceCommunicationIdKey ).ToCamelCase(), b => b.CommunicationFlowInstanceCommunicationIdKey )
                .AddField( nameof( GridRowBag.CommunicationFlowInstanceIdKey ).ToCamelCase(), b => b.CommunicationFlowInstanceIdKey )
                .AddTextField( nameof( GridRowBag.CommunicationFlowCommunicationName ).ToCamelCase(), b => b.CommunicationFlowCommunicationName )
                .AddField( nameof( GridRowBag.CommunicationType ).ToCamelCase(), b => b.CommunicationType )
                .AddField(  nameof( GridRowBag.Sent).ToCamelCase(), b => b.Sent )
                .AddField(  nameof( GridRowBag.Conversions ).ToCamelCase(), b => b.Conversions )
                .AddField( nameof( GridRowBag.Unsubscribes ).ToCamelCase(), b => b.Unsubscribes )
                .AddField( nameof( GridRowBag.Opens ).ToCamelCase(), b => b.Opens )
                .AddField( nameof( GridRowBag.Clicks ).ToCamelCase(), b => b.Clicks )
                .AddField( nameof( GridRowBag.CommunicationFlowInstanceStartDate ).ToCamelCase(), b => b.CommunicationFlowInstanceStartDate );
        }

        #endregion

        #region Helper Classes

        private class Ctx
        {
            private readonly RockContext _rockContext;

            public Ctx( RockContext rockContext )
            {
                _rockContext = rockContext;
            }

            public IQueryable<CommunicationFlow> CommunicationFlows
            {
                get
                {
                    return new CommunicationFlowService( _rockContext ).Queryable();
                }
            }

            public IQueryable<CommunicationFlowInstanceCommunication> CommunicationFlowInstanceCommunications
            {
                get
                {
                    return new CommunicationFlowInstanceCommunicationService( _rockContext ).Queryable();
                }
            }

            public IQueryable<Interaction> Interactions
            {
                get
                {
                    return new InteractionService( _rockContext ).Queryable();
                }
            }

            public IQueryable<CommunicationFlowInstanceConversionHistory> CommunicationFlowInstanceConversionHistories
            {
                get
                {
                    return new CommunicationFlowInstanceConversionHistoryService( _rockContext ).Queryable();
                }
            }

            public IQueryable<CommunicationFlowInstance> CommunicationFlowInstances
            {
                get
                {
                    return new CommunicationFlowInstanceService( _rockContext ).Queryable();
                }
            }

            public IQueryable<CommunicationFlowCommunication> CommunicationFlowCommunications
            {
                get
                {
                    return new CommunicationFlowCommunicationService( _rockContext ).Queryable();
                }
            }
        }

        #endregion
    }
    internal static class CtxExtensions
    {
        internal static IQueryable<T> WhereKeyIs<T>( this IQueryable<T> source, string key, bool allowIntegerIdentifier ) where T : class, Rock.Data.IEntity, new()
        {
            var id = allowIntegerIdentifier ? key.AsIntegerOrNull() : null;

            if ( !id.HasValue )
            {
                var guid = key.AsGuidOrNull();

                if ( guid.HasValue )
                {
                    return source.Where( e => e.Guid == guid.Value );
                }

                id = IdHasher.Instance.GetId( key );
            }

            return id.HasValue ? source.Where( e => e.Id == id.Value ) : source.Where( e => false );
        }

        internal static IHasAdditionalSettings AsAdditionalSettings( this string additionalSettingsJson )
        {
            return new HasAdditionalSettings
            {
                AdditionalSettingsJson = additionalSettingsJson
            };
        }

        internal class HasAdditionalSettings : IHasAdditionalSettings
        {
            public string AdditionalSettingsJson { get; set; }
        }
    }
}
