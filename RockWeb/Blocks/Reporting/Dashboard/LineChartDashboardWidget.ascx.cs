// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock.Model;
using Rock.Reporting.Dashboard;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting.Dashboard
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

    // TODO MEtric/EntityType, etc filters in CustomSettings
    [TextField( "Metric", "NOTE: Weird storage due to backwards compatible", false, "", "CustomSetting" )]
    [TextField( "MetricEntityTypeEntityIds", "", false, "", "CustomSetting" )]

    [CustomCheckboxListField( "Metric Value Types", "Select which metric value types to display in the chart", "Goal,Measure", false, "Measure", "CustomSetting", Order = 4 )]
    [SlidingDateRangeField( "Date Range", Key = "SlidingDateRange", Category = "CustomSetting", DefaultValue = "1||4||", Order = 6 )]
    public partial class LineChartDashboardWidget : DashboardWidget
    {
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
        /// <exception cref="System.NotImplementedException"></exception>
        protected void ShowSettings()
        {
            pnlEditModel.Visible = true;

            var rockContext = new RockContext();
            var metricCategoryService = new MetricCategoryService( rockContext );
            if ( this.MetricId.HasValue )
            {
                var metricCategory = metricCategoryService.Queryable().Where( a => a.MetricId == this.MetricId ).FirstOrDefault();
                mpMetricCategoryPicker.SetValue( metricCategory );
            }
            
            if (this.GetEntityFromContext)
            {
                rblSelectOrContext.SetValue( "1" );
            }
            else
            {
                rblSelectOrContext.SetValue( "0" );
            }

            cbCombineValues.Checked = this.CombineValues;

            // TODO Populate MetricPartitionValues

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
            // value stored as pipe delimited: Metric (as Guid) | EntityId | GetEntityFromContext | CombineValues | Metric's Category (as Guid)
            var metricAttributeValue = string.Format( "{0}|{1}|{2}|{3}|{4}", metricGuid, null, rblSelectOrContext.SelectedValue == "1", cbCombineValues.Checked, metricCategoryGuid );
            SetAttributeValue( "Metric", metricAttributeValue );
            SaveAttributeValues();

            mdEdit.Hide();
            pnlEditModel.Visible = false;

            // TODO Get selected MetricPartitionValues and set EntityId on FirstPartition for backwards compatibiliy


            LoadChart();
        }

        /// <summary>
        /// Creates the dynamic controls.
        /// </summary>
        private void CreateDynamicControls( int? metricId )
        {
            Metric metric = new MetricService( new RockContext() ).Get( metricId ?? 0 );
            if ( metric != null )
            {
                foreach ( var metricPartition in metric.MetricPartitions )
                {
                    if ( metricPartition.EntityTypeId.HasValue )
                    {
                        var entityTypeCache = EntityTypeCache.Read( metricPartition.EntityTypeId.Value );
                        if ( entityTypeCache != null && entityTypeCache.SingleValueFieldType != null )
                        {
                            var fieldType = entityTypeCache.SingleValueFieldType;
                            var entityTypeEditControl = fieldType.Field.EditControl( new Dictionary<string, Rock.Field.ConfigurationValue>(), string.Format( "metricPartition{0}_entityTypeEditControl", metricPartition.Id ) );
                            var panelCol4 = new Panel { CssClass = "col-md-4" };

                            phMetricValuePartitions.Controls.Add( entityTypeEditControl );
                            if ( entityTypeEditControl is IRockControl )
                            {
                                var entityTypeRockControl = ( entityTypeEditControl as IRockControl );
                                entityTypeRockControl.Label = metricPartition.Label;
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
        private bool GetEntityFromContext
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
                if ( GetEntityFromContext )
                {
                    EntityTypeCache entityTypeCache = null;
                    if ( this.MetricId.HasValue )
                    {
                        using ( var rockContext = new Rock.Data.RockContext() )
                        {
                            var metric = new Rock.Model.MetricService( rockContext ).Get( this.MetricId ?? 0 );
                            if ( metric != null )
                            {
                                // for backwards compatibily, get the first metric partition
                                entityTypeCache = EntityTypeCache.Read( metric.MetricPartitions.OrderBy( a => a.Order ).First().EntityTypeId ?? 0 );
                            }
                        }
                    }

                    if ( entityTypeCache != null && this.ContextEntity( entityTypeCache.Name ) != null )
                    {
                        result = this.ContextEntity( entityTypeCache.Name ).Id;
                    }

                    // if Getting the EntityFromContext, and we didn't get it from ContextEntity, get it from the Page Param
                    if ( !result.HasValue )
                    {
                        // figure out what the param name should be ("CampusId, GroupId, etc") depending on metric's entityType
                        var entityParamName = "EntityId";
                        if ( entityTypeCache != null )
                        {
                            entityParamName = entityTypeCache.Name + "Id";
                        }

                        result = this.PageParameter( entityParamName ).AsIntegerOrNull();
                    }
                }
                else
                {
                    var valueParts = GetAttributeValue( "Metric" ).Split( '|' );
                    if ( valueParts.Length > 1 )
                    {
                        result = valueParts[1].AsIntegerOrNull();
                    }
                }

                return result;
            }
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
            var floatChartControl = lcChart;
            floatChartControl.StartDate = this.DateRange.Start;
            floatChartControl.EndDate = this.DateRange.End;
            floatChartControl.MetricValueType = this.MetricValueType;

            floatChartControl.CombineValues = this.CombineValues;
            floatChartControl.ShowTooltip = true;

            Guid? detailPageGuid = ( GetAttributeValue( "DetailPage" ) ?? string.Empty ).AsGuidOrNull();
            if ( detailPageGuid.HasValue )
            {
                floatChartControl.ChartClick += lcExample_ChartClick;
            }

            floatChartControl.Options.SetChartStyle( this.ChartStyleDefinedValueGuid );

            floatChartControl.Options.legend = floatChartControl.Options.legend ?? new Legend();
            floatChartControl.Options.legend.show = this.GetAttributeValue( "ShowLegend" ).AsBooleanOrNull();
            floatChartControl.Options.legend.position = this.GetAttributeValue( "LegendPosition" );

            floatChartControl.MetricId = this.MetricId;
            floatChartControl.EntityId = this.EntityId;
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