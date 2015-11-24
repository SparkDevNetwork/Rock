// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System.Web;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block that can be used to set the default date range context for the site
    /// </summary>
    [DisplayName( "Date Range Context Setter" )]
    [Category( "Core" )]
    [Description( "Block that can be used to set the default date range for the site." )]
    [CustomRadioListField( "Context Scope", "The scope of context to set", "Site,Page", true, "Site", order: 0 )]
    [TextField( "No Date Range Text", "The text to show when there is no date range in the context.", true, "Select Date Range", order: 1 )]
    [IntegerFieldAttribute("Default Range", "The number of days to include from the current date. The default is 7 (previous week)", false, 7, order: 2)]
    [TextField( "Clear Selection Text", "The text displayed when a date range can be unselected. This will not display when the text is empty.", true, "", order: 3 )]
    public partial class DateRangeContextSetter : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // repaint the screen after block settings are updated
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

            LoadDropdowns();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadDropdowns();
        }

        /// <summary>
        /// Loads the schedules
        /// </summary>
        private void LoadDropdowns()
        {
            var dateRangeEntityType = EntityTypeCache.Read( typeof( DateRangeItem ) );
            var currentRange = RockPage.GetCurrentContext( dateRangeEntityType ) as DateRangeItem;

            drpSlidingDateRange.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.Current;
            drpSlidingDateRange.TimeUnit = SlidingDateRangePicker.TimeUnitType.Week;

            var startDateString = Request.QueryString["startDate"];
            var endDateString = Request.QueryString["endDate"];
            if ( startDateString != null && endDateString != null )
            {
                var queryDateRange = new DateRangeItem()  
                {
                    Start = Convert.ToDateTime(startDateString), 
                    End = Convert.ToDateTime(endDateString)
                };

                if ( currentRange != null && currentRange.Start != queryDateRange.Start && currentRange.End != queryDateRange.End )
                {
                    currentRange = SetDateRangeContext( queryDateRange, false );
                }
            }

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );


            lCurrentSelection.Text = GetAttributeValue( "NoDateRangeText" );

            if ( currentRange != null )
            {   
                //lCurrentSelection.Text = GetAttributeValue( "CurrentItemTemplate" ).ResolveMergeFields( mergeObjects );
            }
            else
            {
                //drpDateRange.DelimitedValues = gfPledges.GetUserPreference( "Date Range" );
            }
        }

        /// <summary>
        /// Sets the schedule context.
        /// </summary>
        /// <param name="queryDateRange">The schedule identifier.</param>
        /// <param name="refreshPage">if set to <c>true</c> [refresh page].</param>
        /// <returns></returns>
        protected DateRangeItem SetDateRangeContext( DateRangeItem queryDateRange, bool refreshPage = false )
        {
            bool pageScope = GetAttributeValue( "ContextScope" ) == "Page";

            // set context and refresh below with the correct query string if needed
            //RockPage.SetContextCookie( queryDateRange, pageScope, false );

            if ( refreshPage )
            {
                // Only redirect if refreshPage is true
                if ( !string.IsNullOrWhiteSpace( PageParameter( "startDate" ) ) )
                {
                    var queryString = HttpUtility.ParseQueryString( Request.QueryString.ToStringSafe() );
                    queryString.Set( "startDate", queryDateRange.Start.ToString() );
                    queryString.Set( "startDate", queryDateRange.End.ToString() );
                    Response.Redirect( string.Format( "{0}?{1}", Request.Url.AbsolutePath, queryString ), false );
                }
                else
                {
                    Response.Redirect( Request.RawUrl, false );
                }

                Context.ApplicationInstance.CompleteRequest();
            }

            return queryDateRange;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the Click event of the btnSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSelect_Click( object sender, EventArgs e )
        {
            // set the context here when the user is finished editing
        }


        
        #endregion

        /// <summary>
        /// Date Range Item
        /// </summary>
        public class DateRangeItem
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the start date.
            /// </summary>
            /// <value>
            /// The start.
            /// </value>
            public DateTime Start { get; set; }

            /// <summary>
            /// Gets or sets the end date.
            /// </summary>
            /// <value>
            /// The end.
            /// </value>
            public DateTime End { get; set; }

        }
       
}
}