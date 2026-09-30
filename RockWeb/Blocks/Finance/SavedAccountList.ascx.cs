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
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
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

            if ( savedAccount == null || CurrentPerson == null )
            {
                return;
            }

            // Only allow deleting saved accounts owned by the current person, or by a
            // person the current person is authorized to edit.
            var owner = savedAccount.PersonAlias?.Person;

            if ( owner == null || ( owner.Id != CurrentPerson.Id && !owner.IsAuthorized( Authorization.EDIT, CurrentPerson ) ) )
            {
                mdGridWarning.Show( $"Not authorized to delete {FinancialPersonSavedAccount.FriendlyTypeName}.", ModalAlertType.Warning );
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
            var personId = GetAuthorizedPerson()?.Id;

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

        /// <summary>
        /// Gets the person whose saved accounts the current person is allowed to view or manage.
        /// The context person is only honored when it is the current person or the current person
        /// is authorized to edit the context person; otherwise the current person is used.
        /// </summary>
        /// <returns>The authorized owning person, or <c>null</c> if there is no current person.</returns>
        private Person GetAuthorizedPerson()
        {
            if ( CurrentPerson == null )
            {
                return null;
            }

            var contextPerson = this.ContextEntity() as Person;
            if ( contextPerson == null || contextPerson.Id == CurrentPerson.Id )
            {
                return CurrentPerson;
            }

            return contextPerson.IsAuthorized( Authorization.EDIT, CurrentPerson ) ? contextPerson : CurrentPerson;
        }
    }
}