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
    [Description( "Block that can be used to set a user specific date range preference." )]

    [TextField( "No Date Range Text",
        Description = "The text to show when there is no date range in the context.",
        IsRequired = true,
        DefaultValue = "Select Date Range",
        Order = 0,
        Key = AttributeKey.NoDateRangeText )]

    [SlidingDateRangeField( "Default Date Range",
        Description = "The default range to start with if context and query string have not been set",
        Order = 1,
        Key = AttributeKey.DefaultDateRange )]

    [BooleanField( "Display Query Strings",
        Description = "Select to always display query strings. Default behavior will only display the query string when it's passed to the page.",
        Order = 2,
        Key = AttributeKey.DisplayQueryStrings )]

    public partial class DateRangeContextSetter : Rock.Web.UI.RockBlock
    {
        public static class AttributeKey
        {
            public const string NoDateRangeText = "NoDateRangeText";
            public const string DefaultDateRange = "DefaultDateRange";
            public const string DisplayQueryStrings = "DisplayQueryStrings";
        }

        /// <summary>
        /// The context preference name
        /// </summary>
        protected static string ContextPreferenceName = "context-date-range";

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

            if ( !Page.IsPostBack )
            {
                LoadDropdowns();
            }
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
            var currentRange = RockPage.GetUserPreference( ContextPreferenceName );
            var dateRangeString = Request.QueryString["SlidingDateRange"];
            if ( !string.IsNullOrEmpty( dateRangeString ) && currentRange != dateRangeString )
            {
                // set context to query string
                SetDateRangeContext( dateRangeString, false );
                currentRange = dateRangeString;
            }

            // if current range is selected, show a tooltip, otherwise show the default
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( currentRange );
            if ( dateRange != null && dateRange.Start != null && dateRange.End != null )
            {
                lCurrentSelection.Text = dateRange.ToStringAutomatic();
                drpSlidingDateRange.DelimitedValues = currentRange;
            }
            else
            {
                lCurrentSelection.Text = GetAttributeValue( AttributeKey.NoDateRangeText );
                drpSlidingDateRange.DelimitedValues = GetAttributeValue( AttributeKey.DefaultDateRange );
            }
        }

        /// <summary>
        /// Sets the schedule context.
        /// </summary>
        /// <param name="queryDateRange">The schedule identifier.</param>
        /// <param name="refreshPage">if set to <c>true</c> [refresh page].</param>
        /// <returns></returns>
        protected void SetDateRangeContext( string dateRangeValues, bool refreshPage = false )
        {
            // set context and refresh below with the correct query string if needed
            RockPage.SetUserPreference( ContextPreferenceName, dateRangeValues, true );

            if ( refreshPage )
            {
                // Only redirect if refreshPage is true
                if ( !string.IsNullOrWhiteSpace( PageParameter( "SlidingDateRange" ) ) || GetAttributeValue( AttributeKey.DisplayQueryStrings ).AsBoolean() )
                {
                    var queryString = HttpUtility.ParseQueryString( Request.QueryString.ToStringSafe() );
                    queryString.Set( "SlidingDateRange", dateRangeValues );
                    Response.Redirect( string.Format( "{0}?{1}", Request.Url.AbsolutePath, queryString ), false );
                }
                else
                {
                    Response.Redirect( Request.RawUrl, false );
                }

                Context.ApplicationInstance.CompleteRequest();
            }
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
            // set context here when the user is finished editing
            var dateRange = drpSlidingDateRange.DelimitedValues;
            if ( dateRange != null )
            {
                SetDateRangeContext( dateRange, true );
            }
        }

        #endregion

    }
}