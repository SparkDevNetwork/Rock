// <copyright>
// Copyright 2015 by NewSpring Church
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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.cc_newspring.Blocks.Metrics
{
    /// <summary>
    /// All Church Metrics Block
    /// </summary>
    [DisplayName( "Ministry Metrics" )]
    [Category( "NewSpring" )]
    [Description( "Custom church metrics block using the Chart.js library" )]
    [CustomDropdownListField( "Number of Columns", "", "1,2,3,4,5,6,7,8,9,10,11,12", false, DefaultValue = "4", Order = 1 )]
    [MetricCategoriesField( "Metric Source", "Select the primary metric(s) to include in this chart.", false, Order = 2 )]
    [CustomRadioListField( "Aggregation", "Which calculation method should be used", "Sum,Avg,Min,Max,Median,Latest", true, "Sum", order: 3 )]
    [BooleanField( "Respect Campus Context", "Respect the group context even if the Campus context selector isn't included on the page.", true, Order = 4 )]
    [BooleanField( "Respect Group Context", "Respect the group context even if the GroupType context selector isn't included on the page.", true, Order = 5 )]
    [BooleanField( "Respect Date Context", "Respect the date context even if the DateRange context selector isn't included on the page.", true, Order = 6 )]
    [BooleanField( "Respect Schedule Context", "Respect the schedule context even if the Schedule context selector isn't included on the page.", true, Order = 7 )]
    [MetricCategoriesField( "Comparison Metric Source", "Select the metric(s) to calculate against the Primary Source/Key.", false, Order = 10 )]
    [CustomRadioListField( "Comparison Aggregation", "Which calculation method should be used", "Sum,Avg,Min,Max,Median,Latest", true, "Sum", order: 11 )]
    [CustomRadioListField( "Display Comparison As", "Choose to display the comparison result as an integer or percentage", "Integer,Percentage", true, "Integer", order: 12 )]
    [BooleanField( "Comparison Respect Campus Context", "Respect the group context even if the Campus context selector isn't included on the page.", true, Order = 13 )]
    [BooleanField( "Comparison Respect Group Context", "Respect the group context even if the GroupType context selector isn't included on the page.", true, Order = 14 )]
    [BooleanField( "Comparison Respect Date Context", "Respect the date context even if the DateRange context selector isn't included on the page.", true, Order = 15 )]
    [BooleanField( "Comparison Respect Schedule Context", "Respect the schedule context even if the Schedule context selector isn't included on the page.", true, Order = 16 )]
    public partial class Metrics : RockBlock
    {
        #region Fields

        private List<Guid> MetricSourceGuids = null;
        private List<Guid> ComparisonMetricSourceGuids = null;

        /// <summary>
        /// The string used for setting a Date Range Context
        /// Set via RockWeb.Blocks.Core.DateRangeContextSetter
        /// </summary>
        protected static string ContextPreferenceName = "context-date-range";

        #endregion

        #region Control Methods

        // <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Page.IsPostBack )
            {
                return;
            }

            var rockContext = new RockContext();
            var dvService = new DefinedValueService( rockContext );

            LoadSourceGuids();

            // Output variables direct to the ascx
            metricBlockNumber.Value = BlockId.ToString();
            metricBlockId.Value = BlockName.Replace( " ", "" ).ToString();
            metricTitle.Value = BlockName;
            metricWidth.Value = GetAttributeValue( "NumberofColumns" );

            DisplayTextValue();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Formats the values.
        /// </summary>
        /// <param name="primaryMetricSource">The primary metric source.</param>
        /// <param name="percentageMetricSource">The percentage metric source.</param>
        /// <param name="dateRange">The date range.</param>
        /// <returns></returns>
        protected decimal FormatValues()
        {
            var primaryMetricValues = GetMetricValues( true );
            var primaryValue = GetValueAggregate( primaryMetricValues, true );

            var comparisonMetricGuids = GetAttributeValue( "ComparisonMetricSource" ).SplitDelimitedValues().AsGuidOrNullList();
            // if comparing values, make sure we have a valid percentage source
            if ( primaryValue > 0 && comparisonMetricGuids.Any() )
            {
                var comparisonMetricValues = GetMetricValues( false );
                var comparisonValue = GetValueAggregate( comparisonMetricValues, false );

                if ( comparisonValue > 0 )
                {
                    decimal comparison = ( primaryValue * 1.0M ) / ( comparisonValue * 1.0M );

                    if ( GetAttributeValue( "DisplayComparisonAs" ).Equals( "Integer" ) )
                    {
                        return comparison;
                    }
                    else
                    {
                        return comparison * 100;
                    }
                }
            }

            return primaryValue;
        }

        /// <summary>
        /// Gets the value aggregate.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="isPrimary">if set to <c>true</c> [is primary].</param>
        /// <returns></returns>
        protected decimal GetValueAggregate( List<MetricValue> values, bool isPrimary )
        {
            if ( values == null || !values.Any() )
            {
                return 0;
            }

            var preKey = isPrimary ? string.Empty : "Comparison";
            decimal? aggregateValue = null;

            switch ( GetAttributeValue( preKey + "Aggregation" ) )
            {
                case "Avg":
                    aggregateValue = values.Select( v => v.YValue ).Average();
                    break;

                case "Min":
                    aggregateValue = values.Select( v => v.YValue ).Min();
                    break;

                case "Max":
                    aggregateValue = values.Select( v => v.YValue ).Max();
                    break;

                case "Median":
                    aggregateValue = values.Select( v => v.YValue ).OrderBy( v => v ).ElementAt( values.Count() / 2 );
                    break;

                case "Latest":
                    var latestDate = values.Where( v => v.MetricValueDateTime.HasValue ).OrderByDescending( v => v.MetricValueDateTime ).First().MetricValueDateTime.Value.Date;
                    aggregateValue = values.Where( v => v.MetricValueDateTime.HasValue && v.MetricValueDateTime.Value.Date == latestDate ).Select( v => v.YValue ).Sum();
                    break;

                default:
                    aggregateValue = values.Select( v => v.YValue ).Sum();
                    break;
            }

            if ( !aggregateValue.HasValue )
            {
                aggregateValue = 0;
            }

            return aggregateValue.Value;
        }

        /// <summary>
        /// Gets the metric values.
        /// </summary>
        /// <param name="isPrimary">if set to <c>true</c> [is primary].</param>
        /// <returns></returns>
        protected List<MetricValue> GetMetricValues( bool isPrimary )
        {
            var rockContext = new RockContext();
            var metricService = new MetricService( rockContext );
            List<Guid> sourceGuids = null;
            var preKey = isPrimary ? string.Empty : "Comparison";
            IQueryable<MetricValue> metricValues = null;

            var attributeValue = GetAttributeValue( preKey + "MetricSource" );

            if ( string.IsNullOrWhiteSpace( attributeValue ) )
            {
                attributeValue = string.Empty;
            }

            var pairs = MetricCategoriesFieldAttribute.GetValueAsGuidPairs( attributeValue );
            sourceGuids = pairs.Select( p => p.MetricGuid ).ToList();

            if ( sourceGuids.Any() )
            {
                metricValues = metricService.GetByGuids( sourceGuids ).SelectMany( m => m.MetricValues );
            }
            else
            {
                nbMetricWarning.Visible = true;
                pnlMetricDisplay.Visible = false;
                return null;
            }

            if ( GetAttributeValue( preKey + "RespectCampusContext" ).AsBoolean() )
            {
                var campusContext = RockPage.GetCurrentContext( EntityTypeCache.Read( typeof( Campus ) ) );

                if ( campusContext != null )
                {
                    metricValues = FilterMetricValuesByPartition( metricValues, "Campus", campusContext.Id );
                }
            }

            if ( GetAttributeValue( preKey + "RespectGroupContext" ).AsBoolean() )
            {
                var groupTypeContext = RockPage.GetCurrentContext( EntityTypeCache.Read( typeof( GroupType ) ) );
                var groupContext = RockPage.GetCurrentContext( EntityTypeCache.Read( typeof( Group ) ) );

                if ( groupContext != null )
                {
                    metricValues = FilterMetricValuesByPartition( metricValues, "Group", groupContext.Id );
                }
                else if ( groupTypeContext != null )
                {
                    var groupTypeIds = new GroupTypeService( rockContext ).GetAllAssociatedDescendents( groupTypeContext.Id ).Select( gt => gt.Id );
                    var groupIds = new GroupService( rockContext ).Queryable().Where( g => groupTypeIds.Contains( g.GroupTypeId ) ).Select( g => g.Id );
                    metricValues = metricValues.Where( a => a.MetricValuePartitions.Any( mvp => mvp.MetricPartition.Label == "Group" && groupIds.Any( i => i == mvp.EntityId ) ) );
                }
            }

            if ( GetAttributeValue( preKey + "RespectDateContext" ).AsBoolean() )
            {
                var dateRangeString = RockPage.GetUserPreference( ContextPreferenceName );

                if ( !string.IsNullOrWhiteSpace( dateRangeString ) )
                {
                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( dateRangeString );
                    metricValues = metricValues.Where( v => v.MetricValueDateTime >= dateRange.Start && v.MetricValueDateTime <= dateRange.End );
                }
            }

            if ( GetAttributeValue( preKey + "RespectScheduleContext" ).AsBoolean() )
            {
                var scheduleContext = RockPage.GetCurrentContext( EntityTypeCache.Read( typeof( Schedule ) ) );

                if ( scheduleContext != null )
                {
                    metricValues = FilterMetricValuesByPartition( metricValues, "Schedule", scheduleContext.Id );
                }
            }

            return metricValues.ToList();
        }

        /// <summary>
        /// Filters the metric values by partition.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="partitionName">Name of the partition.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="filterOnNull">if set to <c>true</c> [filter on null].</param>
        /// <returns></returns>
        protected IQueryable<MetricValue> FilterMetricValuesByPartition( IQueryable<MetricValue> values, string partitionName, int? entityId, bool filterOnNull = false )
        {
            if ( !entityId.HasValue && !filterOnNull )
            {
                return values;
            }

            return values.Where( m => m.MetricValuePartitions.Any( mvp => mvp.MetricPartition.Label == partitionName && mvp.EntityId == entityId ) );
        }

        #endregion

        #region Display Methods

        /// <summary>
        /// Displays the text value.
        /// </summary>
        /// <param name="dateRange">The date range.</param>
        /// <param name="primaryMetricSource">The primary metric source.</param>
        /// <param name="comparisonMetricSource">The comparison metric source.</param>
        private void DisplayTextValue()
        {
            // this may be a little complicated to compare date ranges while accepting two metric keys/sources
            decimal currentMetricValues = FormatValues();

            var metricComparisonSymbol = string.Empty;
            var comparisonMetricGuids = GetAttributeValue( "ComparisonMetricSource" ).SplitDelimitedValues().AsGuidOrNullList();
            if ( comparisonMetricGuids.Any() && GetAttributeValue( "DisplayComparisonAs" ).Equals( "Percentage" ) )
            {
                metricComparisonSymbol = "%";
            }

            metricValue.Value = string.Format( "{0:0.#}{1}", currentMetricValues, metricComparisonSymbol );
        }

        #endregion

        /// <summary>
        /// Loads the source guids.
        /// </summary>
        private void LoadSourceGuids()
        {
            var metricSourceString = GetAttributeValue( "MetricSource" );
            MetricSourceGuids = string.IsNullOrWhiteSpace( metricSourceString ) ? null : metricSourceString.SplitDelimitedValues().AsGuidList();

            metricSourceString = GetAttributeValue( "ComparisonMetricSource" );
            ComparisonMetricSourceGuids = string.IsNullOrWhiteSpace( metricSourceString ) ? null : metricSourceString.SplitDelimitedValues().AsGuidList();
        }

        #region Classes

        /// <summary>
        /// Metric information used to bind the selected grid.
        /// </summary>
        [Serializable]
        protected class MetricValueItem
        {
            public int value { get; set; }

            public string color { get; set; }

            public string highlight { get; set; }

            public string label { get; set; }
        }

        /// <summary>
        /// Metric information as a JSON object
        /// </summary>
        [Serializable]
        protected class MetricJson
        {
            public System.DateTime date { get; set; }

            public string value { get; set; }

            public int week { get; set; }

            public int year { get; set; }
        }

        #endregion Classes
    }
}