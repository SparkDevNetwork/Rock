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
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Rock.Blocks.WebFarm
{
    /// <summary>
    /// Displays the details of a particular web farm node.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockEntityDetailBlockType" />
    [DisplayName( "Web Farm Node Detail" )]
    [Category( "WebFarm" )]
    [Description( "Displays the details of a particular web farm node." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [IntegerField(
        "Node CPU Chart Hours",
        Key = AttributeKey.CpuChartHours,
        Description = "The amount of hours represented by the width of the Node CPU chart.",
        DefaultIntegerValue = 24,
        Order = 2 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "8471bf7f-6d0d-411b-899f-cd853f496bb9" )]
    [Rock.SystemGuid.BlockTypeGuid( "6bba1fc0-ac56-4e58-9e99-eb20da7aa415" )]
    public class WebFarmNodeDetail : RockEntityDetailBlockType<WebFarmNode, WebFarmNodeBag>
    {
        #region Keys

        private static class AttributeKey
        {
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

        private static readonly int _cpuMetricSampleCount = 200;
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
            var box = new DetailBlockBox<WebFarmNodeBag, WebFarmNodeDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private WebFarmNodeDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new WebFarmNodeDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the WebFarmNode for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="webFarmNode">The WebFarmNode to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the WebFarmNode is valid, <c>false</c> otherwise.</returns>
        private bool ValidateWebFarmNode( WebFarmNode webFarmNode, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<WebFarmNodeBag, WebFarmNodeDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {WebFarmNode.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( WebFarmNode.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( WebFarmNode.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="WebFarmNodeBag"/> that represents the entity.</returns>
        private WebFarmNodeBag GetCommonEntityBag( WebFarmNode entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var unresponsiveMinutes = 10;
            var unresponsiveDateTime = RockDateTime.Now.AddMinutes( 0 - unresponsiveMinutes );

            var bag = new WebFarmNodeBag
            {
                IdKey = entity.IdKey,
                CurrentLeadershipPollingIntervalSeconds = entity.CurrentLeadershipPollingIntervalSeconds,
                IsActive = entity.IsActive,
                IsCurrentJobRunner = entity.IsCurrentJobRunner,
                IsLeader = entity.IsLeader,
                IsUnresponsive = entity.IsActive && !entity.StoppedDateTime.HasValue && entity.LastSeenDateTime < unresponsiveDateTime,
                LastSeenDateTime = entity.LastSeenDateTime,
                NodeName = entity.NodeName,
                WebFarmNodeMetrics = entity.WebFarmNodeMetrics
                .Where( wfnm =>
                    wfnm.MetricType == WebFarmNodeMetric.TypeOfMetric.CpuUsagePercent &&
                    wfnm.MetricValueDateTime >= ChartMinDate &&
                    wfnm.MetricValueDateTime <= ChartMaxDate )
                .Select( wfnm => new WebFarmMetricBag { MetricValue = wfnm.MetricValue, MetricValueDateTime = wfnm.MetricValueDateTime } )
                .ToList()
            };

            if ( bag.IsActive && !bag.IsUnresponsive && bag.WebFarmNodeMetrics.Count > 1 )
            {
                var metrics = bag.WebFarmNodeMetrics.ConvertAll( x => new Rock.Model.WebFarmNodeMetricService.MetricViewModel() { MetricValue = x.MetricValue, MetricValueDateTime = x.MetricValueDateTime } );
                var samples = WebFarmNodeMetricService.CalculateMetricSamples( metrics, _cpuMetricSampleCount, ChartMinDate, ChartMaxDate );
                bag.ChartData = GetChartData( samples );
            }

            return bag;
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="WebFarmNodeBag"/> that represents the entity.</returns>
        protected override WebFarmNodeBag GetEntityBagForView( WebFarmNode entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            const int unresponsiveMinutes = 10;
            var unresponsiveDateTime = RockDateTime.Now.AddMinutes( 0 - unresponsiveMinutes );
            bag.IsUnresponsive = entity.IsActive && !entity.StoppedDateTime.HasValue && entity.LastSeenDateTime < unresponsiveDateTime;

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="WebFarmNodeBag"/> that represents the entity.</returns>
        protected override WebFarmNodeBag GetEntityBagForEdit( WebFarmNode entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        protected override bool UpdateEntityFromBox( WebFarmNode entity, ValidPropertiesBox<WebFarmNodeBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.IsLeader ),
                () => entity.IsLeader = box.Bag.IsLeader );

            box.IfValidProperty( nameof( box.Bag.NodeName ),
                () => entity.NodeName = box.Bag.NodeName );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <returns>The <see cref="WebFarmNode"/> to be viewed or edited on the page.</returns>
        protected override WebFarmNode GetInitialEntity()
        {
            return GetInitialEntity<WebFarmNode, WebFarmNodeService>( RockContext, PageParameterKey.WebFarmNodeId );
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

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        protected override bool TryGetEntityForEditAction( string idKey, out WebFarmNode entity, out BlockActionResult error )
        {
            var entityService = new WebFarmNodeService( RockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new WebFarmNode();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{WebFarmNode.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${WebFarmNode.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the chart HTML.
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
                samples.Select( _ => "\"\"" ).JoinStrings( "," ),
                samples.Select( s => ( ( int ) s ).ToString() ).JoinStrings( "," )
            );
        }

        /// <summary>
        /// Gets the valid, editable properties.
        /// </summary>
        /// <returns></returns>
        private List<string> GetValidProperties()
        {
            return typeof( WebFarmNodeBag ).GetProperties().Select( p => p.Name ).ToList();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var box = new ValidPropertiesBox<WebFarmNodeBag>
            {
                Bag = GetEntityBagForEdit( entity ),
                ValidProperties = GetValidProperties()
            };

            return ActionOk( box );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<WebFarmNodeBag> box )
        {
            var entityService = new WebFarmNodeService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateWebFarmNode( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.WebFarmNodeId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            return ActionOk( new ValidPropertiesBox<WebFarmNodeBag>
            {
                Bag = GetEntityBagForView( entity ),
                ValidProperties = GetValidProperties()
            } );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new WebFarmNodeService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        #endregion Block Actions
    }
}