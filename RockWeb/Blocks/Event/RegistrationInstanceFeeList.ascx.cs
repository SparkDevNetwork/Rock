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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// A Block that displays the fees related to an event registration instance.
    /// </summary>
    [DisplayName( "Registration Instance - Fee List" )]
    [Category( "Event" )]
    [Description( "Displays the fees related to an event registration instance." )]

    [Rock.SystemGuid.BlockTypeGuid( "41CD9629-9327-40D4-846A-1BB8135D130C" )]
    public partial class RegistrationInstanceFeeList : RegistrationInstanceBlock, ISecondaryBlock
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

            fFees.ApplyFilterClick += fFees_ApplyFilterClick;

            gFees.EmptyDataText = "No Fees Found";
            gFees.GridRebind += gFees_GridRebind;

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

        #endregion

        #region Events

        /// <summary>
        /// Gets the display value for a filter field.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fFees_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "FeeDateRange":
                    e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "FeeName":
                    break;

                case "FeeOptions":
                    var values = new List<string>();
                    foreach ( string value in e.Value.Split( ';' ) )
                    {
                        var item = cblFeeOptions.Items.FindByValue( value );
                        if ( item != null )
                        {
                            values.Add( item.Text );
                        }
                    }

                    e.Value = values.AsDelimited( ", " );
                    break;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the ClearFilterCick event of the fFees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fFees_ClearFilterCick( object sender, EventArgs e )
        {
            fFees.DeleteFilterPreferences();

            BindFeesFilter();
        }

        protected void fFees_ApplyFilterClick( object sender, EventArgs e )
        {
            fFees.SetFilterPreference( UserPreferenceKeyBase.GridFilter_FeeDateRange, "Fee Date Range", sdrpFeeDateRange.DelimitedValues );
            fFees.SetFilterPreference( UserPreferenceKeyBase.GridFilter_FeeName, "Fee Name", ddlFeeName.SelectedItem.Text );
            fFees.SetFilterPreference( UserPreferenceKeyBase.GridFilter_FeeOptions, "Fee Options", cblFeeOptions.SelectedValues.AsDelimited( ";" ) );

            BindFeesGrid();
        }

        protected void ddlFeeName_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( ddlFeeName.SelectedIndex > 0 )
            {
                Populate_cblFeeOptions();
                cblFeeOptions.Visible = true;
            }
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

            pnlDetails.Visible = true;

            BindFeesFilter();
            BindFeesGrid();
        }

        /// <summary>
        /// Binds the fees filter.
        /// </summary>
        private void BindFeesFilter()
        {
            sdrpFeeDateRange.DelimitedValues = fFees.GetFilterPreference( UserPreferenceKeyBase.GridFilter_FeeDateRange );
            Populate_ddlFeeName();
            ddlFeeName.SelectedIndex = ddlFeeName.Items.IndexOf( ddlFeeName.Items.FindByText( fFees.GetFilterPreference( UserPreferenceKeyBase.GridFilter_FeeName ) ) );
            Populate_cblFeeOptions();
        }

        /// <summary>
        /// Binds the fees grid.
        /// </summary>
        private void BindFeesGrid()
        {
            var instanceId = this.RegistrationInstanceId;

            if ( instanceId.GetValueOrDefault( 0 ) == 0 )
            {
                return;
            }

            var registrationTemplateFeeService = new RegistrationTemplateFeeService( new RockContext() );

            var data = registrationTemplateFeeService.GetRegistrationTemplateFeeReport( instanceId.Value );

            // Add Date Range
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpFeeDateRange.DelimitedValues );
            if ( dateRange.Start.HasValue )
            {
                data = data.Where( r => r.RegistrationDate >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                data = data.Where( r => r.RegistrationDate < dateRange.End.Value );
            }

            // Fee Name
            if ( ddlFeeName.SelectedIndex > 0 )
            {
                data = data.Where( r => r.FeeName == ddlFeeName.SelectedItem.Text );
            }

            // Fee Options
            if ( cblFeeOptions.SelectedValues.Count > 0 )
            {
                data = data.Where( r => cblFeeOptions.SelectedValues.Any( v => v.Equals( r.FeeItem.Guid.ToString(), StringComparison.OrdinalIgnoreCase ) ) );
            }

            SortProperty sortProperty = gFees.SortProperty;
            if ( sortProperty != null )
            {
                data = data.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                data = data.OrderByDescending( f => f.RegistrationDate ).ToList();
            }

            gFees.DataSource = data;
            gFees.DataBind();
        }

        /// <summary>
        /// Handles the GridRebind event of the gFees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gFees_GridRebind( object sender, GridRebindEventArgs e )
        {
            var instance = this.RegistrationInstance;

            if ( instance == null )
            {
                return;
            }

            gFees.ExportTitleName = instance.Name + " - Registration Fees";
            gFees.ExportFilename = gFees.ExportFilename ?? instance.Name + "RegistrationFees";

            BindFeesGrid();
        }

        /// <summary>
        /// Populates ddlFeeName with the name of the DDL fee.
        /// </summary>
        private void Populate_ddlFeeName()
        {
            int? instanceId = this.RegistrationInstanceId;

            if ( instanceId.GetValueOrDefault( 0 ) == 0 )
            {
                return;
            }

            var rockContext = new RockContext();
            var registrationInstanceService = new RegistrationInstanceService( rockContext );
            var templateId = registrationInstanceService.Get( ( int ) instanceId ).RegistrationTemplateId;

            var registrationTemplateFeeService = new RegistrationTemplateFeeService( new RockContext() );
            var templateFees = registrationTemplateFeeService.Queryable().Where( f => f.RegistrationTemplateId == templateId ).ToList();

            ddlFeeName.Items.Add( new ListItem() );

            foreach ( var templateFee in templateFees )
            {
                ddlFeeName.Items.Add( new ListItem( templateFee.Name, templateFee.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Populates cblFeeOptions with fee options.
        /// </summary>
        private void Populate_cblFeeOptions()
        {
            cblFeeOptions.Items.Clear();

            int? feeId = ddlFeeName.SelectedValue.AsIntegerOrNull();
            if ( feeId.HasValue )
            {
                var feeItems = new RegistrationTemplateFeeItemService( new RockContext() ).Queryable().Where( a => a.RegistrationTemplateFeeId == feeId );

                foreach ( var feeItem in feeItems )
                {
                    cblFeeOptions.Items.Add( new ListItem( feeItem.Name, feeItem.Guid.ToString() ) );
                }

                string feeOptionValues = fFees.GetFilterPreference( UserPreferenceKeyBase.GridFilter_FeeOptions );
                if ( !string.IsNullOrWhiteSpace( feeOptionValues ) )
                {
                    cblFeeOptions.SetValues( feeOptionValues.Split( ';' ).ToList() );
                }

                cblFeeOptions.Visible = true;
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