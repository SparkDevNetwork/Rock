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

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.WebFarm.WebFarmNodeDetail;
using Rock.ViewModels.Blocks.WebFarm.WebFarmSettings;
using Rock.Web;
using Rock.WebFarm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using static Rock.Model.WebFarmNodeMetricService;

namespace Rock.Blocks.WebFarm
{
    /// <summary>
    /// Displays the details of a web farm.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />
    [DisplayName( "Web Farm Settings" )]
    [Category( "WebFarm" )]
    [Description( "Displays the details of the Web Farm." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Farm Node Detail Page",
        Key = AttributeKey.NodeDetailPage,
        Description = "The page where the node details can be seen",
        DefaultValue = Rock.SystemGuid.Page.WEB_FARM_NODE,
        Order = 1 )]
    [IntegerField(
        "Node CPU Chart Hours",
        Key = AttributeKey.CpuChartHours,
        Description = "The amount of hours represented by the width of the Node CPU charts.",
        DefaultIntegerValue = 4,
        Order = 2 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "d9510038-0547-45f3-9eca-c2ca85e64416" )]
    [Rock.SystemGuid.EntityTypeGuid( "3AA0CC1E-3C16-4AB2-BF03-9EA2FD3239E9")]
    public class WebFarmSettings : RockDetailBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string NodeDetailPage = "NodeDetailPage";
            public const string CpuChartHours = "CpuChartHours";
        }

        private static class PageParameterKey
        {
            public const string WebFarmNodeId = "WebFarmNodeId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        private static readonly int _cpuMetricSampleCount = 50;
        private DateTime ChartMaxDate { get => RockDateTime.Now; }

        private DateTime? _chartMinDate = null;

        private DateTime ChartMinDate
        {
            get
            {
                if ( !_chartMinDate.HasValue )
                {
                    var hours = GetAttributeValue( AttributeKey.CpuChartHours ).AsInteger();
                    _chartMinDate = ChartMaxDate.AddHours( 0 - hours );
                }

                return _chartMinDate.Value;
            }
        }

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<WebFarmSettingsBag, WebFarmSettingsDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private WebFarmSettingsDetailOptionsBag GetBoxOptions()
        {
            var options = new WebFarmSettingsDetailOptionsBag();
            return options;
        }

        /// <summary>
        /// Validates the WebFarm for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="bag">The WebFarm to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the WebFarm is valid, <c>false</c> otherwise.</returns>
        private bool ValidateWebFarmSettings( WebFarmSettingsBag bag, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<WebFarmSettingsBag, WebFarmSettingsDetailOptionsBag> box, RockContext rockContext )
        {
            var isViewable = BlockCache.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

            // Existing entity was found, prepare for view mode by default.
            if ( isViewable )
            {
                box.Entity = GetEntityBagForView( rockContext );
                box.SecurityGrantToken = GetSecurityGrantToken();
            }
            else
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToView( "Web Farm Settings" );
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <returns>A <see cref="WebFarmSettingsBag"/> that represents the entity.</returns>
        private WebFarmSettingsBag GetCommonEntityBag()
        {
            return new WebFarmSettingsBag
            {
                IsActive = RockWebFarm.IsEnabled(),
                WebFarmKey = SystemSettings.GetValue( Rock.SystemKey.SystemSetting.WEBFARM_KEY ),
                LowerPollingLimit = RockWebFarm.GetLowerPollingLimitSeconds(),
                UpperPollingLimit = RockWebFarm.GetUpperPollingLimitSeconds(),
                MinimumPollingDifference = RockWebFarm.GetMinimumPollingDifferenceSeconds(),
                MaxPollingWaitSeconds = RockWebFarm.GetMaxPollingWaitSeconds(),
                IsInMemoryTransport = Rock.Bus.RockMessageBus.IsInMemoryTransport,
                HasValidKey = RockWebFarm.HasValidKey(),
                IsRunning = RockWebFarm.IsRunning()
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A <see cref="WebFarmSettingsBag"/> that represents the entity.</returns>
        private WebFarmSettingsBag GetEntityBagForView( RockContext rockContext )
        {
            var bag = GetCommonEntityBag();

            bag.Nodes = GetNodes( rockContext );
            bag.WebFarmKey = bag.WebFarmKey.Masked();

            return bag;
        }

        /// <summary>
        /// Gets the nodes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private List<WebFarmNodeBag> GetNodes( RockContext rockContext )
        {
            var webFarmNodeService = new WebFarmNodeService( rockContext );

            const int unresponsiveMinutes = 10;
            var unresponsiveDateTime = RockDateTime.Now.AddMinutes( 0 - unresponsiveMinutes );

            var nodes = webFarmNodeService.Queryable()
                .AsNoTracking()
                .Select( wfn => new WebFarmNodeBag
                {
                    IdKey = wfn.Id.ToString(),
                    CurrentLeadershipPollingIntervalSeconds = wfn.CurrentLeadershipPollingIntervalSeconds,
                    IsCurrentJobRunner = wfn.IsCurrentJobRunner,
                    IsActive = wfn.IsActive,
                    IsUnresponsive = wfn.IsActive && !wfn.StoppedDateTime.HasValue && wfn.LastSeenDateTime < unresponsiveDateTime,
                    IsLeader = wfn.IsLeader,
                    NodeName = wfn.NodeName,
                    LastSeenDateTime = wfn.LastSeenDateTime,
                    WebFarmNodeMetrics = wfn.WebFarmNodeMetrics
                        .Where( wfnm =>
                            wfnm.MetricType == WebFarmNodeMetric.TypeOfMetric.CpuUsagePercent &&
                            wfnm.MetricValueDateTime >= ChartMinDate &&
                            wfnm.MetricValueDateTime <= ChartMaxDate )
                        .Select( wfnm => new WebFarmMetricBag
                        {
                            MetricValueDateTime = wfnm.MetricValueDateTime,
                            MetricValue = wfnm.MetricValue
                        } )
                        .ToList(),
                } )
                .ToList();

            foreach ( var node in nodes )
            {
                node.NodeDetailPageUrl = this.GetLinkedPageUrl( AttributeKey.NodeDetailPage, new Dictionary<string, string> {
                    { PageParameterKey.WebFarmNodeId, node.IdKey }
                } );
                node.HumanReadableLastSeen = WebFarmNodeService.GetHumanReadablePastTimeDifference( node.LastSeenDateTime );

                if ( node.IsActive && !node.IsUnresponsive && node.WebFarmNodeMetrics.Count > 1 )
                {
                    var metrics = node.WebFarmNodeMetrics.ConvertAll( x => new MetricViewModel() { MetricValue = x.MetricValue, MetricValueDateTime = x.MetricValueDateTime } );
                    var samples = WebFarmNodeMetricService.CalculateMetricSamples( metrics, _cpuMetricSampleCount, ChartMinDate, ChartMaxDate );
                    node.ChartData = GetChartData( samples );
                }
            }

            return nodes;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <returns>A <see cref="WebFarmSettingsBag"/> that represents the entity.</returns>
        private WebFarmSettingsBag GetEntityBagForEdit()
        {
            return GetCommonEntityBag();
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( DetailBlockBox<WebFarmSettingsBag, WebFarmSettingsDetailOptionsBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => RockWebFarm.SetIsEnabled( box.Entity.IsActive ) );

            box.IfValidProperty( nameof( box.Entity.WebFarmKey ),
                () => SystemSettings.SetValue( Rock.SystemKey.SystemSetting.WEBFARM_KEY, box.Entity.WebFarmKey ) );

            box.IfValidProperty( nameof( box.Entity.LowerPollingLimit ),
                () => SystemSettings.SetValue( Rock.SystemKey.SystemSetting.WEBFARM_LEADERSHIP_POLLING_INTERVAL_LOWER_LIMIT_SECONDS,
                ( box.Entity.LowerPollingLimit == 0 ? RockWebFarm.DefaultValue.DefaultLeadershipPollingIntervalLowerLimitSeconds : box.Entity.LowerPollingLimit ).ToString() ) );

            box.IfValidProperty( nameof( box.Entity.UpperPollingLimit ),
                () => SystemSettings.SetValue( Rock.SystemKey.SystemSetting.WEBFARM_LEADERSHIP_POLLING_INTERVAL_UPPER_LIMIT_SECONDS,
                ( box.Entity.UpperPollingLimit == 0 ? RockWebFarm.DefaultValue.DefaultLeadershipPollingIntervalUpperLimitSeconds : box.Entity.UpperPollingLimit ).ToString() ) );

            box.IfValidProperty( nameof( box.Entity.MaxPollingWaitSeconds ),
                () => SystemSettings.SetValue( Rock.SystemKey.SystemSetting.WEBFARM_LEADERSHIP_MAX_WAIT_SECONDS,
                ( box.Entity.MaxPollingWaitSeconds == 0 ? RockWebFarm.DefaultValue.DefaultPollingMaxWaitSeconds : box.Entity.MaxPollingWaitSeconds ).ToString() ) );

            box.IfValidProperty( nameof( box.Entity.MinimumPollingDifference ),
                () => SystemSettings.SetValue( Rock.SystemKey.SystemSetting.WEBFARM_LEADERSHIP_MIN_POLLING_DIFFERENCE_SECONDS,
                ( box.Entity.MinimumPollingDifference == 0 ? RockWebFarm.DefaultValue.DefaultMinimumPollingDifferenceSeconds : box.Entity.MinimumPollingDifference ).ToString() ) );

            return true;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            return GetSecurityGrantToken();
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken()
        {
            var securityGrant = new Rock.Security.SecurityGrant();
            return securityGrant.ToToken();
        }

        /// <summary>
        /// Gets the chart data.
        /// </summary>
        /// <returns></returns>
        private string GetChartData( decimal[] samples )
        {
            if ( samples == null || samples.Length <= 1 )
            {
                return string.Empty;
            }

            return string.Format(
@"{{
            ""labels"": [{0}],
            ""datasets"": [{{
                ""data"": [{1}],
                ""fill"": true,
                ""backgroundColor"": ""rgba(128, 205, 241, 0.25)"",
                ""borderColor"": ""#009CE3"",
                ""borderWidth"": 2,
                ""pointRadius"": 0,
                ""pointHoverRadius"": 0,
                ""tension"": 0.5
            }}]
        }}",
                samples.Select( s => "\"\"" ).JoinStrings( "," ),
                samples.Select( s => ( ( int ) s ).ToString() ).JoinStrings( "," )
            );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit()
        {
            var box = new DetailBlockBox<WebFarmSettingsBag, WebFarmSettingsDetailOptionsBag>
            {
                Entity = GetEntityBagForEdit(),
                Options = GetBoxOptions()
            };

            return ActionOk( box );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<WebFarmSettingsBag, WebFarmSettingsDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                // Ensure everything is valid before saving.
                if ( !ValidateWebFarmSettings( box.Entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( box ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                return ActionOk( GetEntityBagForView( rockContext ) );
            }
        }

        #endregion Block Actions
    }
}