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
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.Dashboard
{
    /// <summary>
    /// Base class for dashboard widgets that display a chart based on one or more Metrics.
    /// </summary>
    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES,
        Name = "Chart Style",
        Order = 3 )]
    [BooleanField( "Show Legend",
        DefaultBooleanValue = true,
        Order = 7 )]
    [CustomDropdownListField( "Legend Position",
        Description ="Select the position of the Legend (corner)",
        ListSource = "n,ne,e,se,s,sw,w,nw",
        IsRequired = false,
        DefaultValue = "ne",
        Order = 8 )]
    [LinkedPage( "Detail Page",
        Description = "Select the page to navigate to when the chart is clicked",
        IsRequired = false,
        Order = 9 )]
    [TextField( "Metric",
        Description = "NOTE: Weird storage due to backwards compatible",
        IsRequired = false,
        Category = "CustomSetting" )]
    
    [TextField( AttributeKeys.MetricEntityTypeEntityIds,
        IsRequired = false,
        Category = "CustomSetting" )]
    [CustomCheckboxListField( "Metric Value Types",
        Description = "Select which metric value types to display in the chart",
        ListSource = "Goal,Measure",
        IsRequired = false,
        DefaultValue = "Measure",
        Category = "CustomSetting",
        Order = 4 )]
    [SlidingDateRangeField( "Date Range",
        Key = "SlidingDateRange",
        Category = "CustomSetting",
        DefaultValue = "1||4||",
        Order = 6 )]
    public abstract class MetricChartDashboardWidget : DashboardWidget
    {
        /// <summary>
        /// The block setting attribute keys for the <see cref="MetricChartDashboardWidget"/> block.
        /// </summary>
        protected static class AttributeKeys
        {
            /// <summary>
            /// The metric value partitions as a position sensitive comma-delimited list of EntityTypeId|EntityId 
            /// </summary>
            public const string MetricEntityTypeEntityIds = "MetricEntityTypeEntityIds";
            /// <summary>
            /// The identifier of a page that provides additional detail for a chart data point.
            /// </summary>
            public const string DetailPage = "DetailPage";
        }

        #region Fields

        private Panel pnlEditModel
        {
            get
            {
                return this.ControlsOfTypeRecursive<Panel>().FirstOrDefault( a => a.ID == "pnlEditModel" );
            }
        }

        private MetricCategoryPicker mpMetricCategoryPicker
        {
            get
            {
                return this.ControlsOfTypeRecursive<MetricCategoryPicker>().First( a => a.ID == "mpMetricCategoryPicker" );
            }
        }

        private RockRadioButtonList rblSelectOrContext
        {
            get
            {
                return this.ControlsOfTypeRecursive<RockRadioButtonList>().First( a => a.ID == "rblSelectOrContext" );
            }
        }

        private RockCheckBox cbCombineValues
        {
            get
            {
                return this.ControlsOfTypeRecursive<RockCheckBox>().First( a => a.ID == "cbCombineValues" );
            }
        }

        private PlaceHolder phMetricValuePartitions
        {
            get
            {
                return this.ControlsOfTypeRecursive<PlaceHolder>().First( a => a.ID == "phMetricValuePartitions" );
            }
        }

        private ModalDialog mdEdit
        {
            get
            {
                return this.ControlsOfTypeRecursive<ModalDialog>().First( a => a.ID == "mdEdit" );
            }
        }

        private SlidingDateRangePicker drpSlidingDateRange
        {
            get
            {
                return this.ControlsOfTypeRecursive<SlidingDateRangePicker>().First( a => a.ID == "drpSlidingDateRange" );
            }
        }

        #endregion

        #region Control Overrides

        /// <summary>
        /// Gets a flag indicating if the widget has custom configuration edit controls.
        /// </summary>
        protected bool HasEditControls
        {
            get
            {
                return ( this.pnlEditModel != null );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += OnBlockUpdated;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            LoadChart();

            CreateDynamicControls( this.MetricId );
        }

        #endregion

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OnBlockUpdated( object sender, EventArgs e )
        {
            LoadChart();
        }

        #region Settings

        /// <summary>
        /// Gets the settings tool tip.
        /// </summary>
        /// <value>
        /// The settings tool tip.
        /// </value>
        public virtual string SettingsToolTip
        {
            get { return "Settings"; }
        }

        /// <summary>
        /// Adds icons to the configuration area of a <see cref="Rock.Model.Block" /> instance.  Can be overridden to
        /// add additional icons
        /// </summary>
        /// <param name="canConfig">A <see cref="System.Boolean" /> flag that indicates if the user can configure the <see cref="Rock.Model.Block" /> instance.
        /// This value will be <c>true</c> if the user is allowed to configure the <see cref="Rock.Model.Block" /> instance; otherwise <c>false</c>.</param>
        /// <param name="canEdit">A <see cref="System.Boolean" /> flag that indicates if the user can edit the <see cref="Rock.Model.Block" /> instance.
        /// This value will be <c>true</c> if the user is allowed to edit the <see cref="Rock.Model.Block" /> instance; otherwise <c>false</c>.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{Control}" /> containing all the icon <see cref="System.Web.UI.Control">controls</see>
        /// that will be available to the user in the configuration area of the block instance.
        /// </returns>
        public override List<Control> GetAdministrateControls( bool canConfig, bool canEdit )
        {
            List<Control> configControls = new List<Control>();

            if ( canEdit && this.HasEditControls )
            {
                LinkButton lbEdit = new LinkButton();
                lbEdit.CssClass = "edit";
                lbEdit.ToolTip = SettingsToolTip;
                lbEdit.Click += lbEdit_Click;
                configControls.Add( lbEdit );
                HtmlGenericControl iEdit = new HtmlGenericControl( "i" );
                lbEdit.Controls.Add( iEdit );
                lbEdit.CausesValidation = false;
                iEdit.Attributes.Add( "class", "fa fa-edit" );

                // will toggle the block config so they are no longer showing
                lbEdit.Attributes["onclick"] = "Rock.admin.pageAdmin.showBlockConfig()";

                ScriptManager.GetCurrent( this.Page ).RegisterAsyncPostBackControl( lbEdit );
            }

            configControls.AddRange( base.GetAdministrateControls( canConfig, canEdit ) );

            return configControls;
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowSettings();
        }

        /// <summary>
        /// Get the chart control instance displayed by this widget.
        /// </summary>
        /// <returns></returns>
        protected abstract IRockChart GetChartControl();

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected void ShowSettings()
        {
            if ( !this.HasEditControls )
            {
                return;
            }

            var chartControl = GetChartControl();

            pnlEditModel.Visible = true;

            var rockContext = new RockContext();
            var metricCategoryService = new MetricCategoryService( rockContext );
            MetricCategory metricCategory = null;
            if ( this.MetricId.HasValue )
            {
                metricCategory = metricCategoryService.Queryable().Where( a => a.MetricId == this.MetricId ).FirstOrDefault();
                mpMetricCategoryPicker.SetValue( metricCategory );
            }

            if ( this.GetEntityFromContextEnabled )
            {
                rblSelectOrContext.SetValue( "1" );
            }
            else
            {
                rblSelectOrContext.SetValue( "0" );
            }

            cbCombineValues.Checked = this.CombineValues;

            rblSelectOrContext_SelectedIndexChanged( null, null );

            if ( metricCategory != null )
            {
                var entityTypeEntityIds = ( GetAttributeValue( AttributeKeys.MetricEntityTypeEntityIds ) ?? string.Empty ).Split( ',' ).Select( a => a.Split( '|' ) ).Where( a => a.Length == 2 ).Select( a => new
                {
                    EntityTypeId = a[0].AsIntegerOrNull(),
                    EntityId = a[1].AsIntegerOrNull()
                } ).ToList();

                int position = 0;
                foreach ( var metricPartition in metricCategory.Metric.MetricPartitions.OrderBy( a => a.Order ) )
                {
                    var metricPartitionEntityType = EntityTypeCache.Get( metricPartition.EntityTypeId ?? 0 );
                    var controlId = string.Format( "metricPartition{0}_entityTypeEditControl", metricPartition.Id );
                    Control entityTypeEditControl = phMetricValuePartitions.FindControl( controlId );

                    if ( entityTypeEntityIds.Count() > position )
                    {
                        var entry = entityTypeEntityIds[position];

                        if ( metricPartitionEntityType != null && metricPartitionEntityType.SingleValueFieldType != null && metricPartitionEntityType.SingleValueFieldType.Field is IEntityFieldType )
                        {
                            if ( entry != null && entry.EntityTypeId == metricPartitionEntityType.Id)
                            {
                                ( metricPartitionEntityType.SingleValueFieldType.Field as IEntityFieldType ).SetEditValueFromEntityId( entityTypeEditControl, new Dictionary<string, ConfigurationValue>(), entry.EntityId );
                            }
                        }
                    }

                    position++;
                }
            }

            drpSlidingDateRange.DelimitedValues = GetAttributeValue( "SlidingDateRange" );

            mdEdit.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdEdit_SaveClick( object sender, EventArgs e )
        {
            if ( !this.HasEditControls )
            {
                return;
            }

            var metricCategoryId = mpMetricCategoryPicker.SelectedValue.AsIntegerOrNull();
            var metricCategory = new MetricCategoryService( new RockContext() ).Get( metricCategoryId ?? 0 );
            Guid? metricGuid = metricCategory != null ? metricCategory.Metric.Guid : (Guid?)null;
            Guid? metricCategoryGuid = metricCategory != null ? metricCategory.Category.Guid : (Guid?)null; ;

            // NOTE: Weird storage due to backwards compatible
            // value stored as pipe delimited: Metric (as Guid) | EntityId (leave null) | GetEntityFromContextEnabled | CombineValues | Metric's Category (as Guid)
            var metricAttributeValue = string.Format( "{0}|{1}|{2}|{3}|{4}", metricGuid, null, rblSelectOrContext.SelectedValue == "1", cbCombineValues.Checked, metricCategoryGuid );
            SetAttributeValue( "Metric", metricAttributeValue );

            var entityTypeEntityFilters = new List<string>();
            if ( metricCategory != null )
            {
                foreach ( var metricPartition in metricCategory.Metric.MetricPartitions.OrderBy( a => a.Order ) )
                {
                    var metricPartitionEntityType = EntityTypeCache.Get( metricPartition.EntityTypeId ?? 0 );
                    var controlId = string.Format( "metricPartition{0}_entityTypeEditControl", metricPartition.Id );
                    Control entityTypeEditControl = phMetricValuePartitions.FindControl( controlId );

                    int? entityId;

                    if ( metricPartitionEntityType != null && metricPartitionEntityType.SingleValueFieldType != null && metricPartitionEntityType.SingleValueFieldType.Field is IEntityFieldType )
                    {
                        entityId = ( metricPartitionEntityType.SingleValueFieldType.Field as IEntityFieldType ).GetEditValueAsEntityId( entityTypeEditControl, new Dictionary<string, ConfigurationValue>() );

                        entityTypeEntityFilters.Add( string.Format( "{0}|{1}", metricPartitionEntityType.Id, entityId ) );
                    }
                }
            }

            string metricEntityTypeEntityIdsValue = entityTypeEntityFilters.ToList().AsDelimited( "," );
            SetAttributeValue( AttributeKeys.MetricEntityTypeEntityIds, metricEntityTypeEntityIdsValue );

            SetAttributeValue( "SlidingDateRange", drpSlidingDateRange.DelimitedValues);

            SaveAttributeValues();

            mdEdit.Hide();
            pnlEditModel.Visible = false;

            NotifyBlockUpdated();
        }

        /// <summary>
        /// Creates the dynamic controls.
        /// </summary>
        private void CreateDynamicControls( int? metricId )
        {
            if ( !this.HasEditControls )
            {
                return;
            }
            phMetricValuePartitions.Controls.Clear();
            Metric metric = new MetricService( new RockContext() ).Get( metricId ?? 0 );
            if ( metric != null )
            {
                foreach ( var metricPartition in metric.MetricPartitions )
                {
                    if ( metricPartition.EntityTypeId.HasValue )
                    {
                        var entityTypeCache = EntityTypeCache.Get( metricPartition.EntityTypeId.Value );
                        if ( entityTypeCache != null && entityTypeCache.SingleValueFieldType != null )
                        {
                            var fieldType = entityTypeCache.SingleValueFieldType;

                            Dictionary<string, Rock.Field.ConfigurationValue> configurationValues;
                            if ( fieldType.Field is IEntityQualifierFieldType )
                            {
                                configurationValues = ( fieldType.Field as IEntityQualifierFieldType ).GetConfigurationValuesFromEntityQualifier( metricPartition.EntityTypeQualifierColumn, metricPartition.EntityTypeQualifierValue );
                            }
                            else
                            {
                                configurationValues = new Dictionary<string, ConfigurationValue>();
                            }

                            var entityTypeEditControl = fieldType.Field.EditControl( configurationValues, string.Format( "metricPartition{0}_entityTypeEditControl", metricPartition.Id ) );
                            var panelCol4 = new Panel { CssClass = "col-md-4" };

                            if ( entityTypeEditControl != null )
                            {
                                phMetricValuePartitions.Controls.Add( entityTypeEditControl );
                                if ( entityTypeEditControl is IRockControl )
                                {
                                    var entityTypeRockControl = ( entityTypeEditControl as IRockControl );
                                    entityTypeRockControl.Label = metricPartition.Label;
                                }
                            }
                            else
                            {
                                var errorControl = new LiteralControl();
                                errorControl.Text = string.Format( "<span class='label label-danger'>Unable to create Partition control for {0}. Verify that the metric partition settings are set correctly</span>", metricPartition.Label );
                                phMetricValuePartitions.Controls.Add( errorControl );
                            }
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets the metric identifier.
        /// </summary>
        /// <value>
        /// The metric identifier.
        /// </value>
        public int? MetricId
        {
            get
            {
                var valueParts = GetAttributeValue( "Metric" ).Split( '|' );
                if ( valueParts.Length > 1 )
                {
                    Guid metricGuid = valueParts[0].AsGuid();
                    var metric = new Rock.Model.MetricService( new Rock.Data.RockContext() ).Get( metricGuid );
                    if ( metric != null )
                    {
                        return metric.Id;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to get the Entity from the Page context vs. the configured EntityId
        /// </summary>
        /// <value>
        /// <c>true</c> if [get entity from context]; otherwise, <c>false</c>.
        /// </value>
        protected bool GetEntityFromContextEnabled
        {
            get
            {
                var valueParts = GetAttributeValue( "Metric" ).Split( '|' );
                if ( valueParts.Length > 2 )
                {
                    return valueParts[2].AsBoolean();
                }

                // there is no metric, so try to get the EntityFromContext
                return true;
            }
        }

        /// <summary>
        /// Gets the entity identifier either from a specific selection, PageContext, or null, depending on the Metric/Entity selection
        /// NOTE: This is for backwards compatible when only one EntityType per metric was supported
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int? EntityId
        {
            get  
            {
                int? result = null;
                if ( GetEntityFromContextEnabled )
                {
                    MetricPartitionEntityId primaryMetricPartitionEntityId = GetPrimaryMetricPartitionEntityIdFromContext().FirstOrDefault();
                    result = primaryMetricPartitionEntityId?.EntityId;
                }
                else
                {
                    var metricEntityTypeEntityIds = ( GetAttributeValue( AttributeKeys.MetricEntityTypeEntityIds ) ?? string.Empty ).Split( ',' ).Select( a => a.Split( '|' ) ).Where( a => a.Length == 2 ).Select( a => new
                    {
                        EntityTypeId = a[0].AsIntegerOrNull(),
                        EntityId = a[1].AsIntegerOrNull()
                    } ).ToList();

                    if ( metricEntityTypeEntityIds.Count() >= 1 )
                    {
                        // return the EntityId of the first partition
                        result = metricEntityTypeEntityIds[0].EntityId;
                    }
                    else
                    {
                        // backwards compatibility, see if the old "Metric" attribute has an EntityId set
                        var valueParts = GetAttributeValue( "Metric" ).Split( '|' );
                        if ( valueParts.Length > 1 )
                        {
                            result = valueParts[1].AsIntegerOrNull();
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Get a list of identifiers for the entities by which the metric series is partitioned.
        /// An entity is uniquely identified by an entity type and instance id.
        /// </summary>
        /// <returns></returns>
        protected List<MetricService.EntityIdentifierByTypeAndId> GetSelectedPartitionEntityIdentifiers()
        {
            var entityPartitionValues = new List<MetricService.EntityIdentifierByTypeAndId>();

            if ( this.GetEntityFromContextEnabled )
            {
                var metricPartitionEntityIds = GetPrimaryMetricPartitionEntityIdFromContext();
                metricPartitionEntityIds = metricPartitionEntityIds.Where( a => a.MetricPartition != null ).ToList();
                if ( metricPartitionEntityIds.Any() )
                {
                    entityPartitionValues = metricPartitionEntityIds
                        .Select( a => new MetricService.EntityIdentifierByTypeAndId
                        {
                            EntityTypeId = a.MetricPartition.EntityTypeId.GetValueOrDefault( 0 ),
                            EntityId = a.EntityId.GetValueOrDefault( 0 )
                        } )
                        .ToList();
                }
            }
            else
            {
                var metricEntityIdList = GetAttributeValue( AttributeKeys.MetricEntityTypeEntityIds );
                entityPartitionValues = metricEntityIdList.Split( ',' )
                    .Select( a => a.Split( '|' ) )
                    .Where( a => a.Length == 2 )
                    .Select( a => new MetricService.EntityIdentifierByTypeAndId
                    {
                        EntityTypeId = a[0].AsInteger(),
                        EntityId = a[1].AsInteger()
                    } )
                    .ToList();
            }

            return entityPartitionValues;
        }

        /// <summary>
        /// Gets the primary partition entity identifier from context.
        /// </summary>
        /// <returns></returns>
        protected List<MetricPartitionEntityId> GetPrimaryMetricPartitionEntityIdFromContext()
        {
            List<MetricPartitionEntityId> results = new List<MetricPartitionEntityId>();

            if ( !this.MetricId.HasValue )
            {
                return results;
            }

            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var metric = new Rock.Model.MetricService( rockContext ).Get( this.MetricId ?? 0 );
                if ( metric == null )
                {
                    return results;
                }

                foreach ( var mp in metric.MetricPartitions.OrderBy( a => a.Order ) )
                {
                    var result = new MetricPartitionEntityId();

                    result.MetricPartition = mp;

                    // Get the Entity Type for this Partition if it has been assigned.
                    var entityTypeCache = EntityTypeCache.Get( result.MetricPartition.EntityTypeId ?? 0 );

                    // Try to get an Entity of the same type from the current context.
                    if ( entityTypeCache != null && this.ContextEntity( entityTypeCache.Name ) != null )
                    {
                        result.EntityId = this.ContextEntity( entityTypeCache.Name ).Id;
                    }

                    // If there is no matching Entity in the current context, check the page parameters based on the Entity Type name.
                    if ( !result.EntityId.HasValue )
                    {
                        // Figure out what the parameter name should be ("CampusId, GroupId, etc") depending on the Metric's Entity Type.
                        // If this partition does not have a specified Entity Type, look for a generic "EntityId" parameter.
                        var entityParamName = "EntityId";
                        if ( entityTypeCache != null )
                        {
                            entityParamName = entityTypeCache.FriendlyName + "Id";
                        }

                        result.EntityId = this.PageParameter( entityParamName ).AsIntegerOrNull();
                    }

                    // If we still don't have a context entity, check the Page context.
                    if ( !result.EntityId.HasValue )
                    {
                        var contextEntity = this.RockPage.GetCurrentContext( entityTypeCache );
                        result.EntityId = contextEntity?.Id;
                    }

                    results.Add( result );
                }
            }

            return results;
        }

        /// <summary>
        /// Gets the type of the metric value.
        /// </summary>
        /// <value>
        /// The type of the metric value.
        /// </value>
        public MetricValueType? MetricValueType
        {
            get
            {
                string[] metricValueTypes = this.GetAttributeValue( "MetricValueTypes" ).SplitDelimitedValues();
                var selected = metricValueTypes.Select( a => a.ConvertToEnum<MetricValueType>() ).ToArray();
                if ( selected.Length == 1 )
                {
                    // if they picked one, return that one
                    return selected[0];
                }
                else
                {
                    // if they picked both or neither, return null, which indicates to show both
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the chart style.
        /// </summary>
        /// <value>
        /// The chart style.
        /// </value>
        public Guid? ChartStyleDefinedValueGuid
        {
            get
            {
                return this.GetAttributeValue( "ChartStyle" ).AsGuidOrNull();
            }
        }

        /// <summary>
        /// Gets the chart style.
        /// </summary>
        /// <value>
        /// The chart style.
        /// </value>
        public bool ShowLegend
        {
            get
            {
                return this.GetAttributeValue( "ShowLegend" ).AsBooleanOrNull() ?? false;
            }
        }

        /// <summary>
        /// Gets the chart style.
        /// </summary>
        /// <value>
        /// The chart style.
        /// </value>
        public string LegendPosition
        {
            get
            {
                return this.GetAttributeValue( "LegendPosition" ).ToStringSafe();
            }
        }

        /// <summary>
        /// Gets the date range.
        /// </summary>
        /// <value>
        /// The date range.
        /// </value>
        public DateRange DateRange
        {
            get
            {
                return SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( GetAttributeValue( "SlidingDateRange" ) ?? "1||4" );
            }
        }

        /// <summary>
        /// Gets a value indicating whether to combine values with different EntityId values into one series vs. showing each in its own series
        /// </summary>
        /// <value>
        ///   <c>true</c> if [combine values]; otherwise, <c>false</c>.
        /// </value>
        public bool CombineValues
        {
            get
            {
                var valueParts = GetAttributeValue( "Metric" ).Split( '|' );
                if ( valueParts.Length > 3 )
                {
                    return valueParts[3].AsBoolean();
                }

                return false;
            }
        }

        /// <summary>
        /// Override this method to load the chart data.
        /// </summary>
        public void LoadChart()
        {
            // Initialize the chart click event handler.
            var rockChart = this.GetChartControl();

            var detailPageGuid = ( GetAttributeValue( AttributeKeys.DetailPage ) ?? string.Empty ).AsGuidOrNull();
            if ( detailPageGuid.HasValue )
            {
                rockChart.ChartClick += ChartClickHandler;
            }

            // Dashboard widgets have a user-definable width (set as a number of bootstrap columns),
            // but are set to a specific height when the "panel-dashboard" css style is applied.
            // The chart must be set to stretch rather than maintain its aspect ratio,
            // or it will overflow the vertical container boundary and corrupt the page layout.
            // We should consider creating a new "panel-dashboard-chart" css style to omit the height specification
            // and thereby allow vertical resizing to maintain the aspect ratio.
            rockChart.MaintainAspectRatio = false;

            // Load the specific chart instance.
            OnLoadChart();
        }

        /// <summary>
        /// Loads the specific chart instance displayed by this widget.
        /// </summary>
        public abstract void OnLoadChart();

        /// <summary>
        /// Handles a chart click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void ChartClickHandler( object sender, ChartClickArgs e )
        {
            Guid? detailPageGuid = ( GetAttributeValue( AttributeKeys.DetailPage ) ?? string.Empty ).AsGuidOrNull();
            if ( detailPageGuid.HasValue )
            {
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString.Add( "MetricValueId", e.MetricValueId.ToString() );

                qryString.Add( "MetricId", this.MetricId.ToString() );
                qryString.Add( "SeriesId", e.SeriesId );
                qryString.Add( "YValue", e.YValue.ToString() );
                qryString.Add( "DateTimeValue", e.DateTimeValue.ToString( "o" ) );
                NavigateToPage( detailPageGuid.Value, qryString );
            }
        }

        /// <summary>
        /// Handles the SelectItem event of the mpMetricCategoryPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mpMetricCategoryPicker_SelectItem( object sender, EventArgs e )
        {
            var metricCategoryId = mpMetricCategoryPicker.SelectedValue.AsIntegerOrNull();
            var metricCategory = new MetricCategoryService( new RockContext() ).Get( metricCategoryId ?? 0 );
            CreateDynamicControls( metricCategory != null ? metricCategory.MetricId : (int?)null );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblSelectOrContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void rblSelectOrContext_SelectedIndexChanged( object sender, EventArgs e )
        {
            phMetricValuePartitions.Visible = rblSelectOrContext.SelectedValue == "0";
            var metricCategoryId = mpMetricCategoryPicker.SelectedValue.AsIntegerOrNull();
            var metricCategory = new MetricCategoryService( new RockContext() ).Get( metricCategoryId ?? 0 );
            CreateDynamicControls( metricCategory != null ? metricCategory.MetricId : (int?)null );
        }

        #region Support Classes

        /// <summary>
        /// Represents a Metric Partition Value.
        /// </summary>
        public class MetricPartitionEntityId
        {
            /// <summary>
            /// Gets or sets the metric partition.
            /// </summary>
            /// <value>
            /// The metric partition.
            /// </value>
            public MetricPartition MetricPartition { get; set; }

            /// <summary>
            /// Gets or sets the entity identifier.
            /// </summary>
            /// <value>
            /// The entity identifier.
            /// </value>
            public int? EntityId { get; set; }
        }

        #endregion
    }
}