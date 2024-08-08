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
using Rock;
using Rock.Model;

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// Helps configure and generate the AnalyticsSourceDate table for BI Analytics.
    /// </summary>
    [DisplayName( "Calendar Dimension Settings" )]
    [Category( "Reporting" )]
    [Description( "Helps configure and generate the AnalyticsSourceDate table for BI Analytics" )]

    [Rock.SystemGuid.BlockTypeGuid( "7711EAE9-5CF0-46E4-A4E6-26C05A71FE43" )]
    public partial class CalendarDimensionSettings : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

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
            if ( !Page.IsPostBack )
            {
                ShowDetail();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        public void ShowDetail()
        {
            LoadDropDowns();

            var startDate = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.ANALYTICS_CALENDAR_DIMENSION_START_DATE ).AsDateTime()
                ?? new DateTime( RockDateTime.Today.AddYears( -150 ).Year, 1, 1 );
            dpStartDate.SelectedDate = startDate;

            var endDate = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.ANALYTICS_CALENDAR_DIMENSION_END_DATE ).AsDateTime()
                ?? new DateTime( RockDateTime.Today.AddYears( 101 ).Year, 1, 1 ).AddDays( -1 );
            dpEndDate.SelectedDate = endDate;

            var fiscalStartMonth = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.ANALYTICS_CALENDAR_DIMENSION_FISCAL_START_MONTH ).AsIntegerOrNull() ?? 1;
            monthDropDownList.SetValue( fiscalStartMonth );

            var givingMonthUseSundayDate = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.ANALYTICS_CALENDAR_DIMENSION_GIVING_MONTH_USE_SUNDAY_DATE ).AsBoolean();
            cbGivingMonthUseSundayDate.Checked = givingMonthUseSundayDate;
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            monthDropDownList.Items.Clear();
            monthDropDownList.Items.Add( new ListItem( string.Empty, string.Empty ) );
            DateTime date = new DateTime( 2000, 1, 1 );
            for ( int i = 0; i <= 11; i++ )
            {
                monthDropDownList.Items.Add( new ListItem( date.AddMonths( i ).ToString( "MMMM" ), ( i + 1 ).ToString() ) );
            }
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
            //
        }

        /// <summary>
        /// Handles the Click event of the btnGenerate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGenerate_Click( object sender, EventArgs e )
        {
            var startDate = dpStartDate.SelectedDate.Value;
            var maximumStartDate = RockDateTime.Now.AddYears( -120 ).Date;
            if ( startDate > maximumStartDate )
            {
                nbGenerateWarning.Text = $"The latest the Start Date may be is 120 years before today's date ({maximumStartDate:d}). The data in this Calendar Dimensions table is used for various calculations including determining a person's age.";
                nbGenerateWarning.Visible = true;
                return;
            }
            else
            {
                nbGenerateWarning.Visible = false;
            }

            Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.ANALYTICS_CALENDAR_DIMENSION_START_DATE, startDate.ToString( "o" ) );

            var endDate = dpEndDate.SelectedDate.Value;
            Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.ANALYTICS_CALENDAR_DIMENSION_END_DATE, endDate.ToString( "o" ) );

            var fiscalStartMonth = monthDropDownList.SelectedValue.AsIntegerOrNull() ?? 1;
            Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.ANALYTICS_CALENDAR_DIMENSION_FISCAL_START_MONTH, fiscalStartMonth.ToString() );

            var givingMonthUseSundayDate = cbGivingMonthUseSundayDate.Checked;
            Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.ANALYTICS_CALENDAR_DIMENSION_GIVING_MONTH_USE_SUNDAY_DATE, givingMonthUseSundayDate.ToString() );

            AnalyticsSourceDate.GenerateAnalyticsSourceDateData( fiscalStartMonth, givingMonthUseSundayDate, startDate, endDate );

            nbGenerateSuccess.Text = "Successfully generated AnalyticsSourceDate records";
        }

        #endregion
    }
}