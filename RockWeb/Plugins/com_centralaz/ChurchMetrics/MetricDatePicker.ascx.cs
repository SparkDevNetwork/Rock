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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Plugins.com_centralaz.ChurchMetrics
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Metric Date Picker" )]
    [Category( "com_centralaz > ChurchMetrics" )]
    [Description( "Allows users to select a week to view metrics for" )]
    [CategoryField( "Metric Root Category", required: true, entityTypeName: "Rock.Model.MetricCategory" )]
    [CategoryField( "Schedule Root Category", required: true, entityTypeName: "Rock.Model.Schedule" )]
    [CodeEditorField( "Lava Template", "The lava template to use to format the group list.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, false, "", "", 0 )]
    [BooleanField( "Show Holiday Dropdown", "Whether to show the holiday dropdown list.", false )]
    [BooleanField( "Show Calendar", "Whether to show the calendar.", true )]
    public partial class MetricDatePicker : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            lHoliday.Visible = GetAttributeValue( "ShowHolidayDropdown" ).AsBoolean();
            ddlHoliday.Visible = GetAttributeValue( "ShowHolidayDropdown" ).AsBoolean();            
            calCalendar.Visible = GetAttributeValue( "ShowCalendar" ).AsBoolean( resultIfNullOrEmpty: true );

            if ( !Page.IsPostBack )
            {               
                ShowDetail();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        protected void calCalendar_SelectionChanged( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            if ( !String.IsNullOrWhiteSpace( PageParameter( "CampusId" ) ) )
            {
                qryParams["CampusId"] = PageParameter( "CampusId" );
            }
            qryParams["SundayDate"] = calCalendar.SelectedDate.SundayDate().ToShortDateString();
            NavigateToCurrentPage( qryParams );
        }

        protected void ddlHoliday_SelectedIndexChanged( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            if ( !String.IsNullOrWhiteSpace( PageParameter( "CampusId" ) ) )
            {
                qryParams["CampusId"] = PageParameter( "CampusId" );
            }
            if ( !String.IsNullOrWhiteSpace( PageParameter( "SundayDate" ) ) )
            {
                qryParams["SundayDate"] = PageParameter( "SundayDate" );
            }
            qryParams["Holiday"] = ddlHoliday.SelectedValue;
            NavigateToCurrentPage( qryParams );
        }

        #endregion

        #region Methods

        private void ShowDetail()
        {
            DateTime? sundayDate = PageParameter( "SundayDate" ).AsDateTime();
            if ( sundayDate != null )
            {
                var mondayDate = sundayDate.Value.AddDays( -6 );
                calCalendar.SelectedDates.SelectRange( mondayDate, sundayDate.Value );
                calCalendar.VisibleDate = sundayDate.Value;

                int? campusId = PageParameter( "CampusId" ).AsIntegerOrNull();
                string campusName = "All Church";
                if ( campusId != null )
                {
                    campusName = CampusCache.Read( campusId.Value ).Name;
                }

                if ( !PageParameter( "Holiday" ).IsNullOrWhiteSpace() )
                {
                    ddlHoliday.SelectedValue = PageParameter( "Holiday" );
                }

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.Add( "SundayDate", sundayDate );
                mergeFields.Add( "MondayDate", mondayDate );
                mergeFields.Add( "CampusName", campusName );
                mergeFields.Add( "WeekOfYear", sundayDate.Value.GetWeekOfYear( System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday ) );
                mergeFields.Add( "WeekOfMonth", sundayDate.Value.GetWeekOfMonth( DayOfWeek.Monday ) );
                lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );
            }
            else
            {
                var rockContext = new RockContext();
                var categoryService = new CategoryService( rockContext );
                var scheduleService = new ScheduleService( rockContext );
                var metricValuePartitionService = new MetricValuePartitionService( rockContext );

                var metricCategoryList = new List<Category>();
                var metricGuid = GetAttributeValue( "MetricRootCategory" ).AsGuidOrNull();
                if ( metricGuid != null )
                {
                    metricCategoryList.Add( categoryService.Get( metricGuid.Value ) );
                    metricCategoryList.AddRange( categoryService.GetAllDescendents( metricGuid.Value ) );
                }
                var metricCategoryIdList = metricCategoryList.Select( mc => mc.Id ).ToList();

                var scheduleCategoryList = new List<Category>();
                var scheduleGuid = GetAttributeValue( "ScheduleRootCategory" ).AsGuidOrNull();
                if ( scheduleGuid != null )
                {
                    scheduleCategoryList.Add( categoryService.Get( scheduleGuid.Value ) );
                    scheduleCategoryList.AddRange( categoryService.GetAllDescendents( scheduleGuid.Value ) );
                }
                var scheduleCategoryIdList = scheduleCategoryList.Select( sc => sc.Id ).ToList();

                var scheduleIdList = scheduleService.Queryable().Where( s =>
                s.CategoryId != null &&
                scheduleCategoryIdList.Contains( s.CategoryId.Value )
                )
                .Select( s => s.Id )
                .ToList();

                var scheduleTypeGuid = Rock.SystemGuid.EntityType.SCHEDULE.AsGuid();
                var latestMetricDateTime = metricValuePartitionService.Queryable().Where( mvp =>
                    mvp.MetricPartition.EntityType.Guid == scheduleTypeGuid &&
                    mvp.EntityId != null &&
                    scheduleIdList.Contains( mvp.EntityId.Value ) &&
                    mvp.MetricValue.Metric.MetricCategories.Any( mc => metricCategoryIdList.Contains( mc.CategoryId ) ) &&
                    mvp.MetricValue.YValue != null
                    )
                    .Max( mvp => mvp.MetricValue.MetricValueDateTime );
                if ( latestMetricDateTime.HasValue )
                {
                    sundayDate = latestMetricDateTime.Value.SundayDate();

                    var mondayDate = sundayDate.Value.AddDays( -6 );
                    calCalendar.SelectedDates.SelectRange( mondayDate, sundayDate.Value );
                    calCalendar.VisibleDate = sundayDate.Value;

                    int? campusId = PageParameter( "CampusId" ).AsIntegerOrNull();
                    string campusName = "All Church";
                    if ( campusId != null )
                    {
                        campusName = CampusCache.Read( campusId.Value ).Name;
                    }

                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.Add( "SundayDate", sundayDate );
                    mergeFields.Add( "MondayDate", mondayDate );
                    mergeFields.Add( "CampusName", campusName );
                    mergeFields.Add( "WeekOfYear", sundayDate.Value.GetWeekOfYear( System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday ) );
                    mergeFields.Add( "WeekOfMonth", sundayDate.Value.GetWeekOfMonth( DayOfWeek.Monday ) );
                    lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );
                }
                else
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.Add( "SundayDate", null );
                    mergeFields.Add( "MondayDate", null );
                    mergeFields.Add( "CampusName", null );
                    lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );
                }

            }
        }

        #endregion
    }
}