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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

// using RestSharp;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting.Dashboard;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.cc_newspring.Blocks.MinistryMetrics
{
    /// <summary>
    /// Ministry Metrics Block
    /// </summary>
    ///
    [DisplayName( "Ministry Metrics" )]
    [Category( "NewSpring" )]
    [Description( "Custom church metrics block using the Chart.js library" )]
    [CustomDropdownListField( "Number of Columns", "", "1,2,3,4,5,6,7,8,9,10,11,12", false, DefaultValue = "12", Order = 1 )]
    [DefinedValueField( "A5F29054-EC70-4BA3-B181-E2A62D11A929", "Metric Type", "", true, false )]
    //    [DefinedTypeField( "Defined Type Metric", "", true, "", "Metric", 1, "SQLStatement" )]
    //    [CustomDropdownListField(
    //        "Number of Columns",
    //        "",
    //        "1,2,3,4,5,6,7,8,9,10,11,12",
    //        false,
    //        DefaultValue = "12",
    //        Order = 1
    //    )]
    //    [CustomDropdownListField(
    //        "Metric Type",
    //        "",
    //        @"
    //            # Vols Roles Filled,
    //            Roster #,
    //            % of Roster Serving,
    //            Unique Vols Attended,
    //            Goal #,
    //            % of Goal,
    //            MS Student #,
    //            HS Student #,
    //            Total Attendee #,
    //            Sunday Attendance #,
    //            % of Attendee to Sunday #,
    //            Total Vol #,
    //            1st Time Attendee #,
    //            4 week Return %,
    //            Fuse Salvation #,
    //            Salv #,
    //            Bapt #,
    //            Care Room Visit #,
    //            Ownership Class #,
    //            Financial Coaching #,
    //            Group Leader #,
    //            Group Participant #,
    //            Avg Group Attendance #,
    //            % Group participation,
    //            Total Group Members,
    //            Salv Attributes,
    //            % Roster # Changed,
    //            % Unique Vols # Changed,
    //            Vol Assignments per Vol,
    //            1st Serves,
    //            Ratio of Vols to Attendees
    //        ",
    //        false,
    //        Order = 2
    //    )]
    //    [CustomRadioListField(
    //        "Modifier",
    //        "",
    //        @"
    //            By Service,
    //            By Group,
    //            Change 6w,
    //            Change 1y,
    //            YTD,
    //            Change 4w,
    //            Totals
    //        ",
    //        Order = 3
    //    )]
    public partial class MinistryMetrics : Rock.Web.UI.RockBlock
    {
        #region Fields

        /// <summary>
        /// Gets or sets the metric block values.
        /// </summary>
        /// <value>
        /// The metric block values.
        /// </value>

        //protected string metricBlockValues
        //{
        //    get
        //    {
        //        var metricBlockValues = ViewState["metricBlockValues"] as string;

        //        if ( metricBlockValues != null )
        //        {
        //            return metricBlockValues;
        //        }

        //        return string.Empty;
        //    }
        //    set
        //    {
        //        ViewState["metricBlockValues"] = value;
        //    }
        //}

        /// <summary>
        /// Gets or sets a value indicating whether the metric should compare to last year.
        /// </summary>
        /// <value>
        /// <c>true</c> if the metric should compare to last year; otherwise, <c>false</c>.
        /// </value>

        //protected string MetricCompareLastYear
        //{
        //    get
        //    {
        //        var savedValue = ViewState[string.Format( "MetricCompareLastYear_{0}", BlockId )] as string;

        //        if ( savedValue != null )
        //        {
        //            return savedValue.ToString();
        //        }

        //        return string.Empty;
        //    }
        //    set
        //    {
        //        ViewState[string.Format( "MetricCompareLastYear_{0}", BlockId )] = value;
        //    }
        //}

        #endregion Fields

        // <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            //var campusEntityType = EntityTypeCache.Read( "Rock.Model.Campus" );

            //var campus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

            //var campusContext = GetAttributeValue( "RespectCampusContext" ).AsBooleanOrNull();

            // Returns the GUID of the Metric Item
            //var MetricDefinedValueString = GetAttributeValues( "MetricType" ).FirstOrDefault();

            // Guid MetricDefinedValueGuid = new Guid( MetricDefinedValueString );

            // Use the GUID to get the entry

            // var MetricDefinedValue = new DefinedValueService( new RockContext() ).GetByGuid( MetricDefinedValueGuid );

            // MetricDefinedValue.LoadAttributes( new RockContext() );

            // new DefinedValueService( new RockContext() ).GetByGuid( MetricDefinedValueGuid );

            // var MetricDefinedValue = new MetricService( new RockContext() ).GetByIds( churchMetricSource ).FirstOrDefault();

            // Use the GUID to get the entry
            // var things = newMetric.MetricValues.W;

            if ( !Page.IsPostBack )
            {
                var MetricTypeValue = GetAttributeValue( "MetricType" ).AsBooleanOrNull();

                string sqlStatement = string.Empty;

                if ( MetricTypeValue.HasValue )
                {
                    // Define rockContext to speed things up
                    var rockContext = new RockContext();

                    var definedValue = new DefinedValueService( rockContext ).Get( new Guid( GetAttributeValue( "MetricType" ) ) );

                    // Load the attributes
                    definedValue.LoadAttributes( rockContext );

                    // Get the SQLStatement Attribute
                    if ( definedValue.AttributeValues.Any( a => a.Key == "SQLStatement" ) )
                    {
                        sqlStatement = definedValue.AttributeValues["SQLStatement"].Value;
                    }

                    metricTitle.Value = BlockName.ToString();
                    metricWidth.Value = GetAttributeValue( "NumberofColumns" );
                }

                if ( sqlStatement != "" )
                {
                    var sqlResult = DbService.ExecuteCommand( sqlStatement );

                    if ( sqlResult <= 0 )
                    {
                        currentMetricValue.Value = "0";
                    }
                    else
                    {
                        currentMetricValue.Value = sqlResult.ToString();
                    }
                }
            }
        }
    }
}