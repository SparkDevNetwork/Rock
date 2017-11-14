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
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Calendar Dimension Settings" )]
    [Category( "Reporting" )]
    [Description( "Helps configure and generate the AnalyticsSourceDate table for BI Analytics" )]

    [DateField( "StartDate", "", false, "", "CustomSetting", 0 )]
    [DateField( "EndDate", "", false, "", "CustomSetting", 0 )]
    [IntegerField( "FiscalStartMonth", "", false, 1, "CustomSetting", 0 )]
    [BooleanField( "GivingMonthUseSundayDate", "", false, "CustomSetting", 1 )]
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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail();
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        public void ShowDetail()
        {
            LoadDropDowns();

            DateTime? startDate = this.GetAttributeValue( "StartDate" ).AsDateTime() ?? new DateTime( RockDateTime.Today.AddYears( -150 ).Year, 1, 1 );
            DateTime? endDate = this.GetAttributeValue( "EndDate" ).AsDateTime() ?? new DateTime( RockDateTime.Today.AddYears( 101 ).Year, 1, 1 ).AddDays( -1 );
            
            dpStartDate.SelectedDate = startDate;
            dpEndDate.SelectedDate = endDate;

            int? fiscalStartMonth = this.GetAttributeValue( "FiscalStartMonth" ).AsIntegerOrNull() ?? 1;
            monthDropDownList.SetValue( fiscalStartMonth );

            bool givingMonthUseSundayDate = this.GetAttributeValue( "GivingMonthUseSundayDate" ).AsBoolean();
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
            this.SetAttributeValue( "StartDate", dpStartDate.SelectedDate.Value.ToString( "o" ) );
            this.SetAttributeValue( "EndDate", dpEndDate.SelectedDate.Value.ToString( "o" ) );
            this.SetAttributeValue( "FiscalStartMonth", monthDropDownList.SelectedValue );
            this.SetAttributeValue( "GivingMonthUseSundayDate", cbGivingMonthUseSundayDate.Checked.ToTrueFalse() );

            int fiscalStartMonth = monthDropDownList.SelectedValue.AsIntegerOrNull() ?? 1;

            bool givingMonthUseSundayDate = cbGivingMonthUseSundayDate.Checked;

            AnalyticsSourceDate.GenerateAnalyticsSourceDateData( fiscalStartMonth, givingMonthUseSundayDate, dpStartDate.SelectedDate.Value, dpEndDate.SelectedDate.Value );

            nbGenerateSuccess.Text = "Successfully generated AnalyticsSourceDate records";
        }

        #endregion
    }
}