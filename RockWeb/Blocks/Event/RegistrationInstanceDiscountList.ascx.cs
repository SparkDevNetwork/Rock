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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// A Block that displays the discounts related to an event registration instance.
    /// </summary>
    [DisplayName( "Registration Instance - Discount List" )]
    [Category( "Event" )]
    [Description( "Displays the discounts related to an event registration instance." )]
    [Rock.SystemGuid.BlockTypeGuid( "6C8954BF-E221-4B2F-AC3B-612DC16BA27D" )]
    public partial class RegistrationInstanceDiscountList : RegistrationInstanceBlock, ISecondaryBlock
    {
        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The Registration Instance identifier
            /// </summary>
            public const string RegistrationInstanceId = "RegistrationInstanceId";
        }

        #endregion Page Parameter Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            fDiscounts.ApplyFilterClick += fDiscounts_ApplyFilterClick;

            gDiscounts.EmptyDataText = "No Discounts Found";
            gDiscounts.GridRebind += gDiscounts_GridRebind;

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
                ShowDetail();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Gets the display value for a filter field.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fDiscounts_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "DiscountDateRange":
                    e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "DiscountCode":
                    // If that discount code is not in the list, don't show it anymore.
                    if ( ddlDiscountCode.Items.FindByText( e.Value ) == null )
                    {
                        e.Value = string.Empty;
                    }

                    break;

                case "DiscountCodeSearch":
                    break;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the Click event of the ClearFilter button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fDiscounts_ClearFilterClick( object sender, EventArgs e )
        {
            fDiscounts.DeleteFilterPreferences();
            tbDiscountCodeSearch.Enabled = true;
            BindDiscountsFilter();
        }

        /// <summary>
        /// Handles the Click event of the ApplyFilter button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fDiscounts_ApplyFilterClick( object sender, EventArgs e )
        {
            fDiscounts.SetFilterPreference( UserPreferenceKeyBase.GridFilter_DiscountDateRange, "Discount Date Range", sdrpDiscountDateRange.DelimitedValues );
            fDiscounts.SetFilterPreference( UserPreferenceKeyBase.GridFilter_DiscountCode, "Discount Code", ddlDiscountCode.SelectedItem.Text );
            fDiscounts.SetFilterPreference( UserPreferenceKeyBase.GridFilter_DiscountCodeSearch, "Discount Code Search", tbDiscountCodeSearch.Text );

            BindDiscountsGrid();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the DiscountCode selection list.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlDiscountCode_SelectedIndexChanged( object sender, EventArgs e )
        {
            tbDiscountCodeSearch.Enabled = ddlDiscountCode.SelectedIndex == 0 ? true : false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            var registrationInstance = this.RegistrationInstance;

            if ( registrationInstance == null )
            {
                return;
            }

            BindDiscountsFilter();
            BindDiscountsGrid();
        }

        /// <summary>
        /// Bind data to the filter control.
        /// </summary>
        private void BindDiscountsFilter()
        {
            sdrpDiscountDateRange.DelimitedValues = fDiscounts.GetFilterPreference( UserPreferenceKeyBase.GridFilter_DiscountDateRange );
            PopulateDiscountCodeList();
            ddlDiscountCode.SelectedIndex = ddlDiscountCode.Items.IndexOf( ddlDiscountCode.Items.FindByText( fDiscounts.GetFilterPreference( UserPreferenceKeyBase.GridFilter_DiscountCode ) ) );
            tbDiscountCodeSearch.Text = fDiscounts.GetFilterPreference( UserPreferenceKeyBase.GridFilter_DiscountCodeSearch );
        }

        /// <summary>
        /// Bind data to the grid control.
        /// </summary>
        private void BindDiscountsGrid()
        {
            var instanceId = this.RegistrationInstanceId;

            if ( instanceId == null || instanceId == 0 )
            {
                return;
            }

            var registrationTemplateDiscountService = new RegistrationTemplateDiscountService( new RockContext() );

            var data = registrationTemplateDiscountService.GetRegistrationInstanceDiscountCodeReport( ( int ) instanceId );

            // Add Date Range
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpDiscountDateRange.DelimitedValues );
            if ( dateRange.Start.HasValue )
            {
                data = data.Where( r => r.RegistrationDate >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                data = data.Where( r => r.RegistrationDate < dateRange.End.Value );
            }

            // Discount code, use ddl if one is selected, otherwise try the search box.
            if ( ddlDiscountCode.SelectedIndex > 0 )
            {
                data = data.Where( r => r.DiscountCode == ddlDiscountCode.SelectedItem.Text );
            }
            else if ( tbDiscountCodeSearch.Text.IsNotNullOrWhiteSpace() )
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex( tbDiscountCodeSearch.Text.ToLower() );
                data = data.Where( r => regex.IsMatch( r.DiscountCode.ToLower() ) );
            }

            var results = data.ToList();

            SortProperty sortProperty = gDiscounts.SortProperty;
            if ( sortProperty != null )
            {
                results = results.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                results = results.OrderByDescending( d => d.RegistrationDate ).ToList();
            }

            gDiscounts.DataSource = results;
            gDiscounts.DataBind();

            PopulateTotals( results );
        }

        /// <summary>
        /// Bind the grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gDiscounts_GridRebind( object sender, GridRebindEventArgs e )
        {
            var instance = this.RegistrationInstance;

            if ( instance == null )
            {
                return;
            }

            gDiscounts.ExportTitleName = instance.Name + " - Discount Codes";
            gDiscounts.ExportFilename = gDiscounts.ExportFilename ?? instance.Name + "DiscountCodes";
            BindDiscountsGrid();
        }

        /// <summary>
        /// Show a summary of discount entries.
        /// </summary>
        /// <param name="report"></param>
        private void PopulateTotals( List<TemplateDiscountReport> report )
        {
            lTotalTotalCost.Text = report.Sum( r => r.TotalCost ).FormatAsCurrency();
            lTotalDiscountQualifiedCost.Text = report.Sum( r => r.DiscountQualifiedCost ).FormatAsCurrency();
            lTotalDiscounts.Text = report.Sum( r => r.TotalDiscount ).FormatAsCurrency();
            lTotalRegistrationCost.Text = report.Sum( r => r.RegistrationCost ).FormatAsCurrency();
            lTotalRegistrations.Text = report.Count().ToString();
            lTotalRegistrants.Text = report.Sum( r => r.RegistrantCount ).ToString();
        }

        /// <summary>
        /// Load entries into the discount code selection list.
        /// </summary>
        protected void PopulateDiscountCodeList()
        {
            var instanceId = this.RegistrationInstanceId;

            if ( instanceId == null || instanceId == 0 )
            {
                return;
            }

            var discountService = new RegistrationTemplateDiscountService( new RockContext() );
            var discountCodes = discountService.GetDiscountsForRegistrationInstance( instanceId ).AsNoTracking().OrderBy( d => d.Code ).ToList();

            ddlDiscountCode.Items.Clear();
            ddlDiscountCode.Items.Add( new ListItem() );

            foreach ( var discountCode in discountCodes )
            {
                ddlDiscountCode.Items.Add( new ListItem( discountCode.Code, discountCode.Id.ToString() ) );
            }
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visibility of the block.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlDetails.Visible = visible;
        }

        #endregion
    }
}