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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// "Handles displaying account search results and redirects to the accounts page when only one match is found.
    /// </summary>
    [DisplayName( "Account Search" )]
    [Category( "Accounts" )]
    [Description( "Handles displaying account search results and redirects to the accounts page when only one match is found." )]
    [BooleanField( "Show Account Type",
        Description = "Displays the account type in the grid.",
        DefaultBooleanValue = true,
        Category = "Grid Settings",
         Order = 1,
        Key = AttributeKey.ShowAccountType,
        IsRequired = true )]
    [BooleanField( "Show Description",
        Description = "Displays the account description in the grid.",
        DefaultBooleanValue = true,
        Category = "Grid Settings",
         Order = 2,
        Key = AttributeKey.ShowAccountDescription,
        IsRequired = true )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.FINANCIAL_ACCOUNT_SEARCH )]
    public partial class FinancialAccountSearch : RockBlock
    {
        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            BlockUpdated += Block_BlockUpdated;
            gAccounts.GridRebind += gAccounts_GridRebind;
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

        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var pageParams = PageParameters().ToDictionary( k => k.Key, k => k.Value.ToString() );
            NavigateToCurrentPage( pageParams );
        }

        private void gAccounts_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.WebControls.GridView.RowDataBound"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Web.UI.WebControls.GridViewRowEventArgs"/> that contains event data.</param>
        protected void gAccounts_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                // Controls
                var pnlPublicName = ( Panel ) e.Row.FindControl( "pnlPublicName" );

                // Grid Data
                var obj = ( dynamic ) e.Row.DataItem;

                if ( obj != null )
                {
                    var publicName = obj.PublicName ?? "";
                    var name = obj.Name ?? "";

                    pnlPublicName.Visible = obj.PublicName?.Length > 0 && publicName.ToUpper() != name.ToUpper();
                }
            }
        }

        /// <summary>
        /// Handles the Command event of the lnkAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CommandEventArgs"/> instance containing the event data.</param>
        protected void lnkAccount_Command( object sender, CommandEventArgs e )
        {
            if ( e.CommandName == LinkCommand.AccountClick )
            {
                var accountService = new FinancialAccountService( new RockContext() );

                var queryParams = new Dictionary<string, string>();

                var accountId = e.CommandArgument.ToString();

                if ( accountId.IsNotNullOrWhiteSpace() )
                {
                    var accountIdNum = accountId.AsInteger();
                    var parentIds = accountService.GetAllAncestorIds( accountIdNum ).Reverse()?.ToList();
                    if ( parentIds == null )
                    {
                        parentIds = new List<int>();
                    }

                    queryParams.Add( "AccountId", accountId );
                    queryParams.Add( "ExpandedIds", parentIds.Select( v => v.ToString() ).JoinStrings( "," ) );
                    NavigateToPage( Rock.SystemGuid.Page.ACCOUNTS.AsGuid(), queryParams );
                }
            }
        }

        #endregion

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ShowAccountType = "ShowAccountType";
            public const string ShowAccountDescription = "ShowAccountDescription";
        }

        private static class LinkCommand
        {
            public const string AccountClick = "AccountClick";
        }
        #endregion Attribute Keys

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            string searchTerm = PageParameter( "SearchTerm" );

            var accountService = new FinancialAccountService( new RockContext() );
            var accounts = new List<FinancialAccount>();

            var glCodeStart = searchTerm.LastIndexOf( '(' );

            if ( glCodeStart > -1 )
            {
                searchTerm = searchTerm.Substring( glCodeStart ).Replace( "(", "" ).Replace( ")", "" );
            }

            searchTerm = searchTerm.Trim();

            accounts = accountService.GetAccountsBySearchTerm( searchTerm )?.ToList();

            if ( accounts?.Count == 1 )
            {
                var pageService = new PageService( new RockContext() );

                var queryParams = new Dictionary<string, string>();

                var accountId = accounts.First().Id;
                var parentIds = accountService.GetAllAncestorIds( accountId ).Reverse()?.ToList();
                if ( parentIds == null )
                {
                    parentIds = new List<int>();
                }
                queryParams.Add( "AccountId", accountId.ToString() );
                queryParams.Add( "ExpandedIds", parentIds.Select( v => v.ToString() ).JoinStrings( "," ) );
                NavigateToPage( Rock.SystemGuid.Page.ACCOUNTS.AsGuid(), queryParams );
            }
            else
            {
                gAccounts.EntityTypeId = EntityTypeCache.Get<FinancialAccount>().Id;
                gAccounts.DataSource = accounts?
                    .Select( act => new
                    {
                        act.Id,
                        Name = act.Name,
                        PublicName = act.PublicName.IsNullOrWhiteSpace() ? "" : act.PublicName,
                        GlCode = act.GlCode,
                        AccountType = act.AccountTypeValue?.Value,
                        Description = act.Description,
                        Path = accountService.GetDelimitedAccountHierarchy( act, FinancialAccountService.AccountHierarchyDirection.CurrentAccountToParent )?.Replace( "^", " > " ),
                        Campus = act.Campus?.Name
                    } )
                    .ToList();

                gAccounts.DataBind();

                foreach ( DataControlField dcf in gAccounts.Columns )
                {
                    var rtf = dcf as RockTemplateField;
                    var rtfAccountDescription = dcf as RockTemplateField;

                    if ( rtf != null )
                    {
                        if ( rtf.ID == "rtfAccountType" )
                        {
                            rtf.Visible = this.GetAttributeValue( AttributeKey.ShowAccountType ).AsBoolean();
                        }
                    }

                    if ( rtfAccountDescription != null )
                    {
                        if ( rtfAccountDescription.ID == "rtfAccountDescription" )
                        {
                            rtfAccountDescription.Visible = this.GetAttributeValue( AttributeKey.ShowAccountDescription ).AsBoolean();
                        }
                    }
                }

            }
        }

        #endregion
    }
}