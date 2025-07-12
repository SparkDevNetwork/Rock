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

using Rock.Attribute;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.Communication.CommunicationFlowInstanceMessageMetrics;
using Rock.ViewModels.Core.Grid;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Displays the metrics of each message that the flow instance sends out.
    /// </summary>

    [DisplayName( "Communication Flow Instance Message Metrics" )]
    [Category( "Communication" )]
    [Description( "Displays the metrics of each message that the flow instance sends out." )]
    [IconCssClass( "fa fa-line-chart" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]
    
    [Rock.SystemGuid.EntityTypeGuid( "91D70135-87DA-4748-B459-CCE7F60F3D67" )]
    [Rock.SystemGuid.BlockTypeGuid( "039ADBBE-4158-47C8-AE05-181DF42E990C" )]
    public class CommunicationFlowInstanceMessageMetrics : RockBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string CommunicationFlow = "CommunicationFlow";
            public const string CommunicationFlowInstance = "CommunicationFlowInstance";
            public const string CommunicationFlowInstanceCommunication = "CommunicationFlowInstanceCommunication";
        }

        #endregion Keys

        public override object GetObsidianBlockInitialization()
        {
            var instanceCommunicationKey = PageParameter( PageParameterKey.CommunicationFlowInstanceCommunication );
            var arePredictableIdsEnabled = !this.PageCache.Layout.Site.DisablePredictableIds;
            var title = new CommunicationFlowInstanceCommunicationService( this.RockContext )
                .GetSelect( instanceCommunicationKey, c => c.CommunicationFlowCommunication.Name, arePredictableIdsEnabled );
            this.ResponseContext.SetPageTitle( title.ToStringOrDefault( "Communication Flow Instance Message Metrics" ) );

            var box = new CommunicationFlowInstanceMessageMetricsInitializationBox();

            var interactionChannelId = InteractionChannelCache
                    .Get( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() )?.Id ?? 0;

            var interactionQuery = new InteractionService( this.RockContext ).Queryable();
            var conversionHistoryQuery = new CommunicationFlowInstanceConversionHistoryService( this.RockContext ).Queryable();
            var communicationFlowCommunication = new CommunicationFlowInstanceCommunicationService( this.RockContext )
                .GetQueryableByKey( instanceCommunicationKey )
                .Select( c => new
                {
                    c.CommunicationFlowCommunicationId,
                    c.CommunicationFlowCommunication.Name
                } )
                .FirstOrDefault();

            if ( communicationFlowCommunication != null )
            {
                var recipientMetrics = new CommunicationFlowInstanceCommunicationService( this.RockContext )
                    // Get all sibling "instance" communications associated with this one.
                    //  1. Get the blueprint associated with this "instance" communication.
                    //  2. Get ALL "instance" communications associated with the blueprint.
                    .GetQueryableByKey( instanceCommunicationKey, !this.PageCache.Layout.Site.DisablePredictableIds )
                    .Select( ic => ic.CommunicationFlowCommunication )
                    .SelectMany( c => c.CommunicationFlowInstanceCommunications )
                    .SelectMany( ic =>
                        ic.Communication.Recipients
                            .Select( cr => new
                            {
                                FlowInstanceCommunicationId = ic.Id,

                                Person = cr.PersonAlias.Person,
                                SentDate = cr.SendDateTime,
                                OpenedDate = cr.OpenedDateTime,
                                UnsubscribeDate = cr.UnsubscribeDateTime,

                                // Newest clicked date for this recipient and communication.
                                ClickedDate =
                                    interactionQuery
                                        .Where( ix =>
                                            ix.InteractionComponent.InteractionChannelId == interactionChannelId
                                            && ix.InteractionComponent.EntityId == ic.CommunicationId
                                            && ix.Operation == "Click"
                                            && ix.EntityId == cr.Id
                                        )
                                        .OrderByDescending( ix => ix.InteractionDateTime ) // This is needed here to get the latest click.
                                        .Select( ix => ( DateTime? ) ix.InteractionDateTime ) // Cast as nullable so `null` is returned instead of `DateTime.MinValue` if the recipient never clicked the communication.
                                        .FirstOrDefault(),


                                // Newest Conversion for this recipient and instance.
                                ConversionDate =
                                    conversionHistoryQuery
                                        .Where( ch =>
                                            ch.CommunicationFlowInstanceCommunicationId == ic.Id
                                            && ch.PersonAliasId == cr.PersonAliasId
                                        )
                                        .OrderByDescending( ch => ch.Date ) // This is needed here to get the latest conversion.
                                        .Select( ch => ( DateTime? ) ch.Date ) // Cast as nullable so `null` is returned instead of `DateTime.MinValue` if the recipient never achieved the conversion goal for this flow instance.
                                        .FirstOrDefault()
                            }
                        )
                    )
                    .ToList(); // Execute the query.

                box.FlowCommunication = new CommunicationFlowInstanceMessageMetricsFlowCommunicationBag
                {
                    FlowCommunicationIdKey = IdHasher.Instance.GetHash( communicationFlowCommunication.CommunicationFlowCommunicationId ),
                    FlowCommunicationName = communicationFlowCommunication.Name,
                    FlowInstanceCommunications = recipientMetrics
                        .GroupBy( r => r.FlowInstanceCommunicationId )
                        .Select( g => new CommunicationFlowInstanceMessageMetricsFlowInstanceCommunicationBag
                        {
                            FlowInstanceCommunicationIdKey = IdHasher.Instance.GetHash( g.Key ),
                            RecipientMetrics = g
                                .Select( r => new CommunicationFlowInstanceMessageMetricsRecipientMetricsBag
                                {
                                    PersonAliasIdKey = r.Person.IdKey,
                                    ClickedDate = r.ClickedDate,
                                    ConversionDate = r.ConversionDate,
                                    FlowInstanceCommunicationIdKey = IdHasher.Instance.GetHash( r.FlowInstanceCommunicationId ),
                                    OpenedDate = r.OpenedDate,
                                    Person = new PersonFieldBag
                                    {
                                        ConnectionStatus = r.Person.ConnectionStatusValue.Value,
                                        IdKey = r.Person.IdKey,
                                        LastName = r.Person.LastName,
                                        NickName = r.Person.NickName,
                                        PhotoUrl = r.Person.PhotoUrl
                                    },
                                    SentDate = r.SentDate,
                                    UnsubscribeDate = r.UnsubscribeDate
                                } )
                                .ToList()
                        } )
                        .ToList()
                };
            }

            return box;
        }

        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            // Get the message name from the communication blueprint (CommunicationFlowCommunication)
            // since the instance communication doesn't have a separate name.
            var instanceCommunicationKey = pageReference.GetPageParameter( PageParameterKey.CommunicationFlowInstanceCommunication );
            var arePredictableIdsEnabled = !this.PageCache.Layout.Site.DisablePredictableIds;
            var title = new CommunicationFlowInstanceCommunicationService( this.RockContext )
                .GetSelect( instanceCommunicationKey, c => c.CommunicationFlowCommunication.Name, arePredictableIdsEnabled );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb>
                {
                    new BreadCrumbLink( title ?? "Flow Communication Instance Message Metrics", pageReference )
                }
            };
        }
    }
}
