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
using System.Data.Entity;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Data;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Used to look at pledges using various criteria.
    /// </summary>
    [DisplayName( "Pledge Analytics" )]
    [Category( "Finance" )]
    [Description( "Used to look at pledges using various criteria." )]

    [IntegerField(
        "Database Timeout",
        Key = AttributeKeys.DatabaseTimeoutSeconds,
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Order = 0 )]

    [Rock.SystemGuid.BlockTypeGuid( "72B4BBC0-1E8A-46B7-B956-A399624F513C" )]
    public partial class PledgeAnalytics : Rock.Web.UI.RockBlock
    {
        private static class AttributeKeys
        {
            public const string DatabaseTimeoutSeconds = "DatabaseTimeoutSeconds";
        }

        #region Fields

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gList.DataKeyNames = new string[] { "Id" };
            gList.GridRebind += gList_GridRebind;

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
            if ( !Page.IsPostBack )
            {
                LoadSettingsFromUserPreferences();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnApply control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApply_Click(object sender, EventArgs e)
        {
            SaveSettingsToUserPreferences();
            BindGrid();
        }


        protected void gList_RowSelected( object sender, RowEventArgs e )
        {
            int personId = e.RowKeyId;
            Response.Redirect( string.Format( "~/Person/{0}/Contributions", personId ), false );
            Context.ApplicationInstance.CompleteRequest();
            return;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            pnlUpdateMessage.Visible = false;
            pnlResults.Visible = true;
            pnlSummary.Visible = true;

            int[] accountIds = apAccounts.SelectedIds;
            if ( accountIds.Length == 0 )
            {
                return;
            }

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );
            DateTime? start = dateRange.Start ?? (DateTime?) System.Data.SqlTypes.SqlDateTime.MinValue;
            DateTime? end = dateRange.End ?? ( DateTime? ) System.Data.SqlTypes.SqlDateTime.MaxValue;

            var minPledgeAmount = nrePledgeAmount.LowerValue;
            var maxPledgeAmount = nrePledgeAmount.UpperValue;

            var minComplete = nrePercentComplete.LowerValue;
            var maxComplete = nrePercentComplete.UpperValue;

            var minGiftAmount = nreAmountGiven.LowerValue;
            var maxGiftAmount = nreAmountGiven.UpperValue;

            int includeOption = rblInclude.SelectedValueAsInt() ?? 0;
            bool includePledges = includeOption != 1;
            bool includeGifts = includeOption != 0;

            var rockContextAnalytics = new RockContextAnalytics();
            rockContextAnalytics.Database.CommandTimeout = this.GetAttributeValue( AttributeKeys.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;

            DataSet ds = new FinancialPledgeService( rockContextAnalytics ).GetPledgeAnalyticsDataSet( accountIds, start, end,
                minPledgeAmount, maxPledgeAmount, minComplete, maxComplete, minGiftAmount, maxGiftAmount,
                includePledges, includeGifts );
            System.Data.DataView dv = ds.Tables[0].DefaultView;

            if ( gList.SortProperty != null )
            {
                try
                {
                    var sortProperties = new List<string>();
                    foreach ( string prop in gList.SortProperty.Property.SplitDelimitedValues( false ) )
                    {
                        sortProperties.Add( string.Format( "[{0}] {1}", prop, gList.SortProperty.DirectionString ) );
                    }
                    dv.Sort = sortProperties.AsDelimited( ", " );
                }
                catch
                {
                    dv.Sort = "[LastName] ASC, [NickName] ASC";
                }
            }
            else
            {
                dv.Sort = "[LastName] ASC, [NickName] ASC";
            }

            gList.DataSource = dv;
            gList.DataBind();

            decimal pledgeTotal = 0;
            decimal totalGivingAmount = 0;
            foreach ( DataRow row in ds.Tables[0].Rows )
            {
                if ( !DBNull.Value.Equals( row["PledgeAmount"] ) )
                {
                    pledgeTotal += ( decimal ) row["PledgeAmount"];
                }

                if ( !DBNull.Value.Equals( row["GiftAmount"] ) )
                {
                    totalGivingAmount += ( decimal ) row["GiftAmount"];
                }
            }

            lPledgeTotal.Text = pledgeTotal.FormatAsCurrencyWithDecimalPlaces( 2 );
            lTotalGivingAmount.Text = totalGivingAmount.FormatAsCurrencyWithDecimalPlaces( 2 );
        }

        /// <summary>
        /// Saves the attendance reporting settings to user preferences.
        /// </summary>
        private void SaveSettingsToUserPreferences()
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( "apAccounts", apAccounts.SelectedIds.ToList().AsDelimited( "," ) );

            preferences.SetValue( "drpDateRange", drpSlidingDateRange.DelimitedValues );

            preferences.SetValue( "nrePledgeAmount", nrePledgeAmount.DelimitedValues );
            preferences.SetValue( "nrePercentComplete", nrePercentComplete.DelimitedValues );
            preferences.SetValue( "nreAmountGiven", nreAmountGiven.DelimitedValues );

            preferences.SetValue( "Include", rblInclude.SelectedValue );

            preferences.Save();
        }

        /// <summary>
        /// Loads the attendance reporting settings from user preferences.
        /// </summary>
        private void LoadSettingsFromUserPreferences()
        {
            var preferences = GetBlockPersonPreferences();

            var accountSettings = preferences.GetValue( "apAccounts" ).SplitDelimitedValues().AsIntegerList();

            apAccounts.SetValues( accountSettings );

            string slidingDateRangeSettings = preferences.GetValue( "drpDateRange" );
            if ( string.IsNullOrWhiteSpace( slidingDateRangeSettings ) )
            {
                // default to current year
                drpSlidingDateRange.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.Current;
                drpSlidingDateRange.TimeUnit = SlidingDateRangePicker.TimeUnitType.Year;
            }
            else
            {
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( slidingDateRangeSettings );
                if ( !dateRange.Start.HasValue && !dateRange.End.HasValue )
                {
                    // default to current year
                    drpSlidingDateRange.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.Current;
                    drpSlidingDateRange.TimeUnit = SlidingDateRangePicker.TimeUnitType.Year;
                }
                else
                {
                    drpSlidingDateRange.DelimitedValues = slidingDateRangeSettings;
                }
            }

            nrePledgeAmount.DelimitedValues = preferences.GetValue( "nrePledgeAmount" );
            nrePercentComplete.DelimitedValues = preferences.GetValue( "nrePercentComplete" );
            nreAmountGiven.DelimitedValues = preferences.GetValue( "nreAmountGiven" );

            string includeSetting = preferences.GetValue( "Include" );
            if ( !string.IsNullOrWhiteSpace( includeSetting ) )
            {
                rblInclude.SetValue( Int32.Parse( includeSetting ) );
            }
        }

        /// <summary>
        /// Formats the name.
        /// </summary>
        /// <param name="lastname">The lastname.</param>
        /// <param name="nickname">The nickname.</param>
        /// <returns></returns>
        protected string FormatName( object lastname, object nickname )
        {
            string result = string.Empty;

            if ( lastname != null )
            {
                result = lastname.ToString();
            }

            if ( nickname != null )
            {
                if ( !string.IsNullOrWhiteSpace( result ) )
                {
                    result += ", ";
                }

                result += nickname;
            }

            return result;
        }

        #endregion
    }
}