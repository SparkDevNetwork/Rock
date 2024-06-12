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
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Saved Account List" )]
    [Category( "Finance" )]
    [Description( "List of a person's saved accounts that can be used to delete an account." )]
    [Rock.SystemGuid.BlockTypeGuid( "CE9F1E41-33E6-4FED-AA08-BD9DCA061498" )]
    [ContextAware( typeof( Person ) )]
    public partial class SavedAccountList : RockBlock, ICustomGridColumns
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gSavedAccounts.DataKeyNames = new[] { "id" };
            gSavedAccounts.ShowActionRow = false;
            gSavedAccounts.GridRebind += gSavedAccounts_GridRebind;
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
                BindGrid();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gSavedAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gSavedAccounts_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gSavedAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gSavedAccounts_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new FinancialPersonSavedAccountService( rockContext );
            var savedAccount = service.Get( e.RowKeyId );
            string errorMessage;

            if ( savedAccount == null )
            {
                return;
            }

            if ( !service.CanDelete( savedAccount, out errorMessage ) )
            {
                mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            service.Delete( savedAccount );
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var contextEntity = this.ContextEntity() as Person;
            var personId = contextEntity?.Id ?? CurrentPerson?.Id;

            if ( personId.HasValue )
            {
                var rockContext = new RockContext();
                gSavedAccounts.DataSource = new FinancialPersonSavedAccountService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        a.FinancialPaymentDetail != null &&
                        a.PersonAlias != null &&
                        a.PersonAlias.PersonId == personId.Value )
                    .OrderBy( a => a.Name )
                    .ToList()
                    .Select( a => new
                    {
                        a.Id,
                        a.Name,
                        AccountNumber = a.FinancialPaymentDetail.AccountNumberMasked,
                        AccountType = a.FinancialPaymentDetail.CurrencyAndCreditCardType
                    } )
                    .ToList();
                gSavedAccounts.DataBind();
            }
        }
    }
}