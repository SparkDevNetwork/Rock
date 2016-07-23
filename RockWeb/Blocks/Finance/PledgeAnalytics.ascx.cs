﻿// <copyright>
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
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Pledge Analytics" )]
    [Category( "Finance" )]
    [Description( "Used to look at pledges using various criteria." )]
    public partial class PledgeAnalytics : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        private RockContext _rockContext = null;
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

            _rockContext = new RockContext();
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
                LoadSettingsFromUserPreferences();
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

        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
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

            int? accountId = apAccount.SelectedValue.AsIntegerOrNull();
            if ( accountId.HasValue )
            {
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpDateRange.DelimitedValues );
                var start = dateRange.Start;
                var end = dateRange.End;

                var minPledgeAmount = nrePledgeAmount.LowerValue;
                var maxPledgeAmount = nrePledgeAmount.UpperValue;

                var minComplete = nrePercentComplete.LowerValue;
                var maxComplete = nrePercentComplete.UpperValue;

                var minGiftAmount = nreAmountGiven.LowerValue;
                var maxGiftAmount = nreAmountGiven.UpperValue;

                int includeOption = rblInclude.SelectedValueAsInt() ?? 0;
                bool includePledges = includeOption != 1;
                bool includeGifts = includeOption != 0;

                DataSet ds = FinancialPledgeService.GetPledgeAnalytics( accountId.Value, start, end,
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
            }
        }

        /// <summary>
        /// Saves the attendance reporting settings to user preferences.
        /// </summary>
        private void SaveSettingsToUserPreferences()
        {
            string keyPrefix = string.Format("pledge-analytics-{0}-", this.BlockId);

            this.SetUserPreference(keyPrefix + "apAccount", apAccount.SelectedValue);

            this.SetUserPreference(keyPrefix + "drpDateRange", drpDateRange.DelimitedValues);

            this.SetUserPreference(keyPrefix + "nrePledgeAmount", nrePledgeAmount.DelimitedValues);
            this.SetUserPreference(keyPrefix + "nrePercentComplete", nrePercentComplete.DelimitedValues);
            this.SetUserPreference(keyPrefix + "nreAmountGiven", nreAmountGiven.DelimitedValues);

            this.SetUserPreference(keyPrefix + "Include", rblInclude.SelectedValue);
        }

        /// <summary>
        /// Loads the attendance reporting settings from user preferences.
        /// </summary>
        private void LoadSettingsFromUserPreferences()
        {
            string keyPrefix = string.Format("pledge-analytics-{0}-", this.BlockId);


            string accountSetting = this.GetUserPreference(keyPrefix + "apAccount");
            if ( !string.IsNullOrWhiteSpace(accountSetting) )
            {
                apAccount.SetValue(Int32.Parse(accountSetting));
            }
            
            
            drpDateRange.DelimitedValues = this.GetUserPreference(keyPrefix + "drpDateRange");

            nrePledgeAmount.DelimitedValues = this.GetUserPreference(keyPrefix + "nrePledgeAmount");
            nrePercentComplete.DelimitedValues = this.GetUserPreference(keyPrefix + "nrePercentComplete");
            nreAmountGiven.DelimitedValues = this.GetUserPreference(keyPrefix + "nreAmountGiven");

            string includeSetting = this.GetUserPreference(keyPrefix + "Include");
            if ( !string.IsNullOrWhiteSpace(includeSetting) )
            {
                rblInclude.SetValue(Int32.Parse(includeSetting));
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

            if ( nickname != null )
            {
                result = nickname.ToString();
            }

            if ( lastname != null )
            {
                if ( !string.IsNullOrWhiteSpace( result ) )
                {
                    result += " ";
                }

                result += lastname;
            }

            return result;
        }

        #endregion


}
}