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
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Transaction Yearly Summary Lava" )]
    [Category( "Finance" )]
    [Description( "Presents a summary of financial transactions broke out by year and account using lava" )]

    [ContextAware( typeof( Person ) )]
    [CodeEditorField( "Lava Template", "The lava template to use to format the transaction summary.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "{% include '~~/Assets/Lava/TransactionYearlySummary.lava' %}", "", 1 )]
    public partial class TransactionYearlySummaryLava : RockBlock, ISecondaryBlock
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
                BindGrid();
            }

            base.OnLoad( e );
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
            BindGrid();
        }

        /// <summary>
        /// 
        /// </summary>
        private class SummaryRecord
        {
            public int Year { get; set; }
            public int AccountId { get; set; }
            public decimal TotalAmount { get; set; }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var contributionType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            if ( contributionType != null )
            {
                var rockContext = new RockContext();
                var transactionDetailService = new FinancialTransactionDetailService( rockContext );
                var qry = transactionDetailService.Queryable().AsNoTracking()
                    .Where( a =>
                        a.Transaction.TransactionTypeValueId == contributionType.Id &&
                        a.Transaction.TransactionDateTime.HasValue );

                var targetPerson = this.ContextEntity<Person>();
                if ( targetPerson != null )
                {
                    qry = qry.Where( t => t.Transaction.AuthorizedPersonAlias.Person.GivingId == targetPerson.GivingId );
                }

                List<SummaryRecord> summaryList;

                using ( new Rock.Data.QueryHintScope( rockContext, QueryHintType.RECOMPILE ) )
                {
                    summaryList = qry
                        .GroupBy( a => new { a.Transaction.TransactionDateTime.Value.Year, a.AccountId } )
                        .Select( t => new SummaryRecord
                        {
                            Year = t.Key.Year,
                            AccountId = t.Key.AccountId,
                            TotalAmount = t.Sum( d => d.Amount )
                        } ).OrderByDescending( a => a.Year )
                        .ToList();
                }

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                var financialAccounts = new FinancialAccountService( rockContext ).Queryable().Select( a => new { a.Id, a.Name } ).ToDictionary( k => k.Id, v => v.Name );

                var yearsMergeObjects = new List<Dictionary<string, object>>();
                foreach ( var item in summaryList.GroupBy( a => a.Year ) )
                {
                    var year = item.Key;
                    var accountsList = new List<object>();
                    foreach ( var a in item )
                    {
                        var accountDictionary = new Dictionary<string, object>();
                        accountDictionary.Add( "Account", financialAccounts.ContainsKey( a.AccountId ) ? financialAccounts[a.AccountId] : string.Empty );
                        accountDictionary.Add( "TotalAmount", a.TotalAmount );
                        accountsList.Add( accountDictionary );
                    }

                    var yearDictionary = new Dictionary<string, object>();
                    yearDictionary.Add( "Year", year );
                    yearDictionary.Add( "SummaryRows", accountsList );

                    yearsMergeObjects.Add( yearDictionary );
                }

                mergeFields.Add( "Rows", yearsMergeObjects );

                lLavaOutput.Text = string.Empty;

                string template = GetAttributeValue( "LavaTemplate" );

                lLavaOutput.Text += template.ResolveMergeFields( mergeFields ).ResolveClientIds( upnlContent.ClientID );
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on it's page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion
    }
}
