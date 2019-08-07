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
    /// 
    /// </summary>
    [DisplayName( "Line Chart" )]
    [Category( "Reporting > Dashboard" )]
    [Description( "Line Chart Dashboard Widget" )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES, "Chart Style", Order = 3 )]

    [BooleanField( "Show Legend", "", true, Order = 7 )]
    [CustomDropdownListField( "Legend Position", "Select the position of the Legend (corner)", "ne,nw,se,sw", false, "ne", Order = 8 )]
    [LinkedPage( "Detail Page", "Select the page to navigate to when the chart is clicked", false, Order = 9 )]

    [TextField( "Metric", "NOTE: Weird storage due to backwards compatible", false, "", "CustomSetting" )]

    // The metric value partitions as a position sensitive comma-delimited list of EntityTypeId|EntityId
    [TextField( "MetricEntityTypeEntityIds", "", false, "", "CustomSetting" )]

    [CustomCheckboxListField( "Metric Value Types", "Select which metric value types to display in the chart", "Goal,Measure", false, "Measure", "CustomSetting", Order = 4 )]
    [SlidingDateRangeField( "Date Range", Key = "SlidingDateRange", Category = "CustomSetting", DefaultValue = "1||4||", Order = 6 )]
    public abstract class LineBarPointsChartDashboardWidget : DashboardWidget
    {
        private Panel pnlEditModel
        {
            get
            {
                return this.ControlsOfTypeRecursive<Panel>().First( a => a.ID == "pnlEditModel" );
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

        private FlotChart flotChart
        {
            get
            {
                return this.ControlsOfTypeRecursive<FlotChart>().First( a => a.ID == "flotChart" );
            }
        }

        private NotificationBox nbMetricWarning
        {
            get
            {
                return this.ControlsOfTypeRecursive<NotificationBox>().First( a => a.ID == "nbMetricWarning" );
            }
        }

        private Panel pnlDashboardTitle
        {
            get
            {
                return this.ControlsOfTypeRecursive<Panel>().First( a => a.ID == "pnlDashboardTitle" );
            }
        }

        private Panel pnlDashboardSubtitle
        {
            get
            {
                return this.ControlsOfTypeRecursive<Panel>().First( a => a.ID == "pnlDashboardSubtitle" );
            }
        }

        private Literal lDashboardTitle
        {
            get
            {
                return this.ControlsOfTypeRecursive<Literal>().First( a => a.ID == "lDashboardTitle" );
            }
        }

        private Literal lDashboardSubtitle
        {
            get
            {
                return this.ControlsOfTypeRecursive<Literal>().First( a => a.ID == "lDashboardSubtitle" );
            }
        }

        private SlidingDateRangePicker drpSlidingDateRange
        {
            get
            {
                return this.ControlsOfTypeRecursive<SlidingDateRangePicker>().First( a => a.ID == "drpSlidingDateRange" );
            }
        }
        
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;
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

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadChart();
        }

        #region custom settings

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

            if ( canEdit )
            {
                LinkButton lbEdit = new LinkButton();
                lbEdit.CssClass = "edit";
                lbEdit.ToolTip = SettingsToolTip;
                lbEdit.Click += lbEdit_Click;
                configControls.Add( lbEdit );
                HtmlGenericControl iEdit = new HtmlGenericControl( "i" );
                lbEdit.Controls.Add( iEdit );
                lbEdit.CausesValidation = false;
                iEdit.Attributes.Add( "class", "fa fa-pencil-square-o" );

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
        /// Shows the settings.
        /// </summary>
        protected void ShowSettings()
        {
            flotChart.Visible = false;
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
                var entityTypeEntityIds = ( GetAttributeValue( "MetricEntityTypeEntityIds" ) ?? string.Empty ).Split( ',' ).Select( a => a.Split( '|' ) ).Where( a => a.Length == 2 ).Select( a => new
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
            SetAttributeValue( "MetricEntityTypeEntityIds", metricEntityTypeEntityIdsValue );

            SetAttributeValue( "SlidingDateRange", drpSlidingDateRange.DelimitedValues);

            SaveAttributeValues();

            mdEdit.Hide();
            pnlEditModel.Visible = false;

            LoadChart();
        }

        /// <summary>
        /// Creates the dynamic controls.
        /// </summary>
        private void CreateDynamicControls( int? metricId )
        {
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
        private bool GetEntityFromContextEnabled
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
                    var metricEntityTypeEntityIds = ( GetAttributeValue( "MetricEntityTypeEntityIds" ) ?? string.Empty ).Split( ',' ).Select( a => a.Split( '|' ) ).Where( a => a.Length == 2 ).Select( a => new
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
        /// Gets the primary partition entity identifier from context.
        /// </summary>
        /// <returns></returns>
        private List<MetricPartitionEntityId> GetPrimaryMetricPartitionEntityIdFromContext()
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
                    var entityTypeCache = EntityTypeCache.Get( result.MetricPartition.EntityTypeId ?? 0 );

                    if ( entityTypeCache != null && this.ContextEntity( entityTypeCache.Name ) != null )
                    {
                        result.EntityId = this.ContextEntity( entityTypeCache.Name ).Id;
                    }

                    // if Getting the EntityFromContext, and we didn't get it from ContextEntity, get it from the Page Param
                    if ( !result.EntityId.HasValue )
                    {
                        // figure out what the param name should be ("CampusId, GroupId, etc") depending on metric's entityType
                        var entityParamName = "EntityId";
                        if ( entityTypeCache != null )
                        {
                            entityParamName = entityTypeCache.Name + "Id";
                        }

                        result.EntityId = this.PageParameter( entityParamName ).AsIntegerOrNull();
                    }

                    results.Add( result );
                }
            }

            return results;
        }

        /// <summary>
        /// 
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
        /// Loads the chart.
        /// </summary>
        public void LoadChart()
        {
            flotChart.Visible = true;
            var flotChartControl = flotChart;
            flotChartControl.StartDate = this.DateRange.Start;
            flotChartControl.EndDate = this.DateRange.End;
            flotChartControl.MetricValueType = this.MetricValueType;

            flotChartControl.CombineValues = this.CombineValues;
            flotChartControl.ShowTooltip = true;

            Guid? detailPageGuid = ( GetAttributeValue( "DetailPage" ) ?? string.Empty ).AsGuidOrNull();
            if ( detailPageGuid.HasValue )
            {
                flotChartControl.ChartClick += lcExample_ChartClick;
            }

            flotChartControl.Options.SetChartStyle( this.ChartStyleDefinedValueGuid );

            flotChartControl.Options.legend = flotChartControl.Options.legend ?? new Legend();
            flotChartControl.Options.legend.show = this.GetAttributeValue( "ShowLegend" ).AsBooleanOrNull();
            flotChartControl.Options.legend.position = this.GetAttributeValue( "LegendPosition" );

            flotChartControl.MetricId = this.MetricId;
            
            if ( this.GetEntityFromContextEnabled )
            {
                var metricPartitionEntityIds = GetPrimaryMetricPartitionEntityIdFromContext();
                metricPartitionEntityIds = metricPartitionEntityIds.Where( a => a.MetricPartition != null ).ToList();
                if ( metricPartitionEntityIds.Any() )
                {
                    flotChartControl.MetricValuePartitionEntityIds = string.Join( ",",
                        metricPartitionEntityIds.Select( a => string.Format( "{0}|{1}", a.MetricPartition.EntityTypeId, a.EntityId ) ) );
                }
            }
            else
            {
                flotChartControl.MetricValuePartitionEntityIds = GetAttributeValue( "MetricEntityTypeEntityIds" );
            }

            nbMetricWarning.Visible = !this.MetricId.HasValue;

            pnlDashboardTitle.Visible = !string.IsNullOrEmpty( this.Title );
            pnlDashboardSubtitle.Visible = !string.IsNullOrEmpty( this.Subtitle );
            lDashboardTitle.Text = this.Title;
            lDashboardSubtitle.Text = this.Subtitle;
        }

        /// <summary>
        /// Lcs the example_ chart click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void lcExample_ChartClick( object sender, ChartClickArgs e )
        {
            Guid? detailPageGuid = ( GetAttributeValue( "DetailPage" ) ?? string.Empty ).AsGuidOrNull();
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
    }
}