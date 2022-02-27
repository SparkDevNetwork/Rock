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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using com.minecartstudio.WistiaIntegration.Model;
using System.Collections.Generic;
using Rock.Model;
using System.Data.Entity;
using Rock.Web.Cache;

namespace RockWeb.Plugins.com_mineCartStudio.WistiaIntegration
{
    /// <summary>
    /// Lists all the Wistia accounts and allows for managing them.
    /// </summary>
    [DisplayName( "Account List" )]
    [Category( "Mine Cart Studio > Wistia Integration" )]
    [Description( "Lists all the Wistia accounts and allows for managing them." )]

    [LinkedPage( "Detail Page" )]
    public partial class AccountList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gAccount.DataKeyNames = new string[] { "Id" };
            gAccount.Actions.ShowAdd = true;
            gAccount.Actions.AddClick += gAccount_Add;
            gAccount.GridRebind += gAccount_GridRebind;
            gAccount.ShowConfirmDeleteDialog = false;
            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gAccount.Actions.ShowAdd = canAddEditDelete;
            gAccount.IsDeleteEnabled = canAddEditDelete;

            string deleteScript = @"
    $('table.js-grid-accounts a.grid-delete-button').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Deleting this account will also delete related projects and media in Rock. These files will not be deleted from Wistia. Are you sure you wish to proceed ?', function (result) {
            if (result) {
                        window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                }
            });
    });
";
            ScriptManager.RegisterStartupScript( gAccount, gAccount.GetType(), "deleteInstanceScript", deleteScript, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
               gAccount_Bind();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAccount_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "AccountId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAccount_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "AccountId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAccount_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var accountService = new WistiaAccountService( rockContext );

            var account = accountService.Get( e.RowKeyId );

            if ( account != null )
            {
                string errorMessage;
                if ( !accountService.CanDelete( account, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                accountService.Delete( account );

                rockContext.SaveChanges();
            }

            gAccount_Bind();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAccount_GridRebind( object sender, EventArgs e )
        {
            gAccount_Bind();
        }

        protected void gAccount_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var accountResult = e.Row.DataItem as WistiaAccount;

                Literal lMediaFileCount = e.Row.FindControl( "lMediaFileCount" ) as Literal;
                if ( lMediaFileCount != null )
                {
                    if ( accountResult.AccountDataInfo.MediaCount.HasValue )
                    {
                        lMediaFileCount.Text = accountResult.AccountDataInfo.MediaCount.ToString();
                    }
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid for defined types.
        /// </summary>
        private void gAccount_Bind()
        {
            using ( var rockContext = new RockContext() )
            {
                var accounts = new WistiaAccountService( rockContext ).Queryable().AsNoTracking();

                SortProperty sortProperty = gAccount.SortProperty;
                if ( sortProperty != null )
                {
                    accounts = accounts.Sort( sortProperty );
                }
                else
                {
                    accounts = accounts.OrderBy( a => a.Name );
                }

                gAccount.DataSource = accounts.ToList();
                gAccount.DataBind();
            }
        }

        #endregion
    }

    
}