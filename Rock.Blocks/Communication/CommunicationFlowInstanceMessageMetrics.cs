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
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Displays the metrics of each message that the flow instance sends out.
    /// </summary>

    [DisplayName( "Communication Flow Instance Message Metrics" )]
    [Category( "Communication" )]
    [Description( "Displays the metrics of each message that the flow instance sends out." )]
    [IconCssClass( "ti ti-chart-line" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]
    
    [Rock.SystemGuid.EntityTypeGuid( "91D70135-87DA-4748-B459-CCE7F60F3D67" )]
    [Rock.SystemGuid.BlockTypeGuid( "039ADBBE-4158-47C8-AE05-181DF42E990C" )]
    public class CommunicationFlowInstanceMessageMetrics : RockBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            /// <summary>
            /// Used in routes:
            /// <list type="bullet">
            ///     <item>
            ///         <term>/CommunicationFlow/{CommunicationFlow}/Instance/{CommunicationFlowInstance}/Messages/{CommunicationFlowInstanceCommunication}/Metrics</term>
            ///         <term>/CommunicationFlow/{CommunicationFlow}/Instance/{CommunicationFlowInstance}/Templates/{CommunicationFlowCommunication}/Messages/Metrics?StartDateRange={StartDateRange}</term>
            ///     </item>
            /// </list>
            /// </summary>
            public const string CommunicationFlow = "CommunicationFlow";

            /// <summary>
            /// Used in routes:
            /// <list type="bullet">
            ///     <item>
            ///         <term>/CommunicationFlow/{CommunicationFlow}/Instance/{CommunicationFlowInstance}/Messages/{CommunicationFlowInstanceCommunication}/Metrics</term>
            ///         <term>/CommunicationFlow/{CommunicationFlow}/Instance/{CommunicationFlowInstance}/Templates/{CommunicationFlowCommunication}/Messages/Metrics?StartDateRange={StartDateRange}</term>
            ///     </item>
            /// </list>
            /// </summary>
            public const string CommunicationFlowInstance = "CommunicationFlowInstance";

            /// <summary>
            /// Used in routes:
            /// <list type="bullet">
            ///     <item>
            ///         <term>/CommunicationFlow/{CommunicationFlow}/Instance/{CommunicationFlowInstance}/Messages/{CommunicationFlowInstanceCommunication}/Metrics</term>
            ///     </item>
            /// </list>
            /// </summary>
            public const string CommunicationFlowInstanceCommunication = "CommunicationFlowInstanceCommunication";

            /// <summary>
            /// Used in routes:
            /// <list type="bullet">
            ///     <item>
            ///         <term>/CommunicationFlow/{CommunicationFlow}/Instance/{CommunicationFlowInstance}/Templates/{CommunicationFlowCommunication}/Messages/Metrics?StartDateRange={StartDateRange}</term>
            ///     </item>
            /// </list>
            /// </summary>
            public const string CommunicationFlowCommunication = "CommunicationFlowCommunication";
            
            /// <summary>
            /// Used in routes:
            /// <list type="bullet">
            ///     <item>
            ///         <term>/CommunicationFlow/{CommunicationFlow}/Instance/{CommunicationFlowInstance}/Templates/{CommunicationFlowCommunication}/Messages/Metrics?StartDateRange={StartDateRange}</term>
            ///     </item>
            /// </list>
            /// </summary>
            public const string StartDateRange = "StartDateRange";
        }

        #endregion Keys

        #region Properties

        private DateRange StartDateRangePageParameter
        {
            get
            {
                var startDateRange = PageParameter( PageParameterKey.StartDateRange );

                if ( startDateRange.IsNullOrWhiteSpace() )
                {
                    return null;
                }

                return SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( startDateRange );
            }
        }

        private bool AreIntegerIdentifiersAllowed => !PageCache.Layout.Site.DisablePredictableIds;

        #endregion Properties

        public override object GetObsidianBlockInitialization()
        {
            var communicationFlowInstanceCommunicationConversionService = new CommunicationFlowInstanceCommunicationConversionService( RockContext );
            var interactionService = new InteractionService( RockContext );

            var interactionChannelId = InteractionChannelCache.Get( SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() )?.Id ?? 0;
            var interactionQuery = interactionService.Queryable();
            var conversionQuery = communicationFlowInstanceCommunicationConversionService.Queryable();
            var communicationFlowCommunicationQuery = GetCommunicationFlowCommunicationQuery();

            var communicationFlowCommunication =
                communicationFlowCommunicationQuery
                    .Select( cfc => new
                    {
                        CommunicationFlowCommunicationId = cfc.Id,
                        cfc.Name,
                        cfc.CommunicationFlowId,
                        IsConversionGoalTrackingEnabled = cfc.CommunicationFlow.ConversionGoalType.HasValue
                    } )
                    .FirstOrDefault();

            this.ResponseContext.SetPageTitle( communicationFlowCommunication?.Name.ToStringOrDefault( "Communication Flow Instance Message Metrics" ) );
            
            var box = new InitializationBox();

            if ( communicationFlowCommunication != null )
            {
                box.IsConversionGoalTrackingEnabled = communicationFlowCommunication.IsConversionGoalTrackingEnabled;

                var recipientMetrics = communicationFlowCommunicationQuery
                    // Get all sibling "instance" communications associated with this one.
                    //  1. Get the blueprint associated with this "instance" communication.
                    //  2. Get ALL "instance" communications associated with the blueprint.
                    // Purpose: "How does this instance of this message compare across all instances of this flow?"
                    .SelectMany( cfc => cfc.CommunicationFlowInstanceCommunications )
                    .SelectMany( cfic =>
                        cfic.Communication.Recipients
                            .Select( cr => new
                            {
                                FlowInstanceId = cfic.CommunicationFlowInstanceId,
                                FlowInstanceCommunicationId = cfic.Id,

                                Person = cr.PersonAlias.Person,
                                SentDate = cr.SendDateTime,
                                OpenedDate = cr.OpenedDateTime,
                                UnsubscribeDate = cr.UnsubscribeDateTime,

                                // Newest clicked date for this recipient and communication.
                                ClickedDate =
                                    interactionQuery
                                        .Where( ix =>
                                            ix.InteractionComponent.InteractionChannelId == interactionChannelId
                                            && ix.InteractionComponent.EntityId == cfic.CommunicationId
                                            && ix.Operation == "Click"
                                            && ix.EntityId == cr.Id
                                        )
                                        .OrderByDescending( ix => ix.InteractionDateTime ) // This is needed here to get the latest click.
                                        .Select( ix => ( DateTime? ) ix.InteractionDateTime ) // Cast as nullable so `null` is returned instead of `DateTime.MinValue` if the recipient never clicked the communication.
                                        .FirstOrDefault(),


                                // Newest Conversion for this recipient and instance.
                                ConversionDate =
                                    conversionQuery
                                        .Where( ch =>
                                            ch.CommunicationFlowInstanceCommunicationId == cfic.Id
                                            && ch.PersonAliasId == cr.PersonAliasId
                                        )
                                        .OrderByDescending( ch => ch.Date ) // This is needed here to get the latest conversion.
                                        .Select( ch => ( DateTime? ) ch.Date ) // Cast as nullable so `null` is returned instead of `DateTime.MinValue` if the recipient never achieved the conversion goal for this flow instance.
                                        .FirstOrDefault()
                            }
                        )
                    )
                    .ToList(); // Execute the query.

                box.CommunicationFlowCommunication = new CommunicationFlowCommunicationBag
                {
                    CommunicationFlowCommunicationIdKey = IdHasher.Instance.GetHash( communicationFlowCommunication.CommunicationFlowCommunicationId ),
                    CommunicationFlowCommunicationName = communicationFlowCommunication.Name,
                    CommunicationFlowInstanceCommunications = recipientMetrics
                        .GroupBy( r => r.FlowInstanceCommunicationId )
                        .Select( g => new CommunicationFlowInstanceCommunicationBag
                        {
                            CommunicationFlowInstanceIdKey = IdHasher.Instance.GetHash( g.Select( r => r.FlowInstanceId ).FirstOrDefault() ),
                            CommunicationFlowInstanceCommunicationIdKey = IdHasher.Instance.GetHash( g.Key ),
                            RecipientMetrics = g
                                .Select( r => new RecipientMetricsBag
                                {
                                    PersonAliasIdKey = r.Person.IdKey,
                                    ClickedDate = r.ClickedDate,
                                    ConversionDate = r.ConversionDate,
                                    CommunicationFlowInstanceCommunicationIdKey = IdHasher.Instance.GetHash( r.FlowInstanceCommunicationId ),
                                    OpenedDate = r.OpenedDate,
                                    Person = new PersonFieldBag
                                    {
                                        ConnectionStatus = r.Person.ConnectionStatusValue?.Value,
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

                // Get instance details.
                box.CommunicationFlowInstances = new CommunicationFlowInstanceService( RockContext )
                    .Queryable()
                    .Where( cfi => cfi.CommunicationFlowId == communicationFlowCommunication.CommunicationFlowId )
                    .Select( cfi => new
                    {
                        CommunicationFlowInstanceId = cfi.Id,
                        StartDate = cfi.StartDate,
                        UniquePersonCount = cfi.CommunicationFlowInstanceRecipients.Select( cfir => cfir.RecipientPersonAlias.PersonId ).Distinct().Count()
                    } )
                    .ToList()
                    .Select( cfi => new CommunicationFlowInstanceBag
                    {
                        CommunicationFlowInstanceIdKey = IdHasher.Instance.GetHash( cfi.CommunicationFlowInstanceId ),
                        StartDate = cfi.StartDate,
                        UniquePersonCount = cfi.UniquePersonCount
                    } )
                    .ToList();

                box.UniquePersonCount = GetUniquePersonCount( communicationFlowCommunication.CommunicationFlowId );
                box.AllUniquePersonCount = GetAllUniquePersonCount( communicationFlowCommunication.CommunicationFlowId );
            }

            return box;
        }

        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            // Get the message name from the communication blueprint (CommunicationFlowCommunication)
            // since the instance communication doesn't have a separate name.
            var title = GetCommunicationFlowCommunicationQuery()
                .Select( cfc => cfc.Name )
                .FirstOrDefault();

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb>
                {
                    new BreadCrumbLink( title ?? "Flow Communication Instance Message Metrics", pageReference )
                }
            };
        }

        private IQueryable<CommunicationFlowCommunication> GetCommunicationFlowCommunicationQuery()
        {
            var communicationFlowCommunicationService = new CommunicationFlowCommunicationService( RockContext );
            var communicationFlowInstanceCommunicationService = new CommunicationFlowInstanceCommunicationService( RockContext );

            var communicationFlowCommunicationKey = PageParameter( PageParameterKey.CommunicationFlowCommunication );
            var communicationFlowInstanceCommunicationKey = PageParameter( PageParameterKey.CommunicationFlowInstanceCommunication );

            if ( communicationFlowInstanceCommunicationKey.IsNotNullOrWhiteSpace() )
            {
                return communicationFlowInstanceCommunicationService
                    .GetQueryableByKey( communicationFlowInstanceCommunicationKey, AreIntegerIdentifiersAllowed )
                    .Select( cfic => cfic.CommunicationFlowCommunication );
            }
            else if ( communicationFlowCommunicationKey.IsNotNullOrWhiteSpace() )
            {
                return communicationFlowCommunicationService
                    .GetQueryableByKey( communicationFlowCommunicationKey, AreIntegerIdentifiersAllowed );
            }
            else
            {
                return Enumerable
                    .Empty<CommunicationFlowCommunication>()
                    .AsQueryable();
            }
        }

        private int GetUniquePersonCount( int communicationFlowId )
        {
            var communicationFlowInstanceService = new CommunicationFlowInstanceService( RockContext );

            var communicationFlowInstanceIdKey = PageParameter( PageParameterKey.CommunicationFlowInstance );

            if ( communicationFlowInstanceIdKey.IsNotNullOrWhiteSpace() )
            {
                return communicationFlowInstanceService
                    .GetQueryableByKey( communicationFlowInstanceIdKey, AreIntegerIdentifiersAllowed )
                    .SelectMany( cfi => cfi
                        .CommunicationFlowInstanceRecipients
                        .Select( cfir => cfir.RecipientPersonAlias.PersonId )
                    )
                    .Distinct()
                    .Count();
            }

            var startDateRange = StartDateRangePageParameter;

            if ( startDateRange != null )
            {
                return communicationFlowInstanceService
                    .Queryable()
                    .Where( cfi => cfi.CommunicationFlowId == communicationFlowId )
                    .SelectMany( cfi => cfi
                        .CommunicationFlowInstanceRecipients
                        .Select( cfir => cfir.RecipientPersonAlias.PersonId )
                    )
                    .Distinct()
                    .Count();
            }

            return 0;
        }

        private int GetAllUniquePersonCount( int communicationFlowId )
        {
            return new CommunicationFlowService( RockContext )
                .Queryable()
                .Where( cf => cf.Id == communicationFlowId )
                .SelectMany( cf => cf.CommunicationFlowInstances
                    .SelectMany(cfi => cfi.CommunicationFlowInstanceRecipients
                        .Select( cfir => cfir.RecipientPersonAlias.PersonId )
                    )
                )
                .Distinct()
                .Count();
        }
    }
}
