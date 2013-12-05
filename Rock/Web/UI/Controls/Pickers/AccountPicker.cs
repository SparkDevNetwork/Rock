//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class AccountPicker : ItemPicker
    {

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.IconCssClass = "fa fa-building-o";
        }
        
        
        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="account">The account.</param>
        public void SetValue( FinancialAccount account )
        {
            if ( account != null )
            {
                ItemId = account.Id.ToString();
                var parentAccountIds = string.Empty;
                var parentAccount = account.ParentAccount;

                while ( parentAccount != null )
                {
                    parentAccountIds = parentAccount.Id + "," + parentAccountIds;
                    parentAccount = parentAccount.ParentAccount;
                }

                InitialItemParentIds = parentAccountIds.TrimEnd( new[] { ',' } );
                ItemName = account.PublicName;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="accounts">The accounts.</param>
        public void SetValues( IEnumerable<FinancialAccount> accounts )
        {
            var financialAccounts = accounts.ToList();

            if ( financialAccounts.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentAccountIds = string.Empty;

                foreach ( var account in financialAccounts )
                {
                    if ( account != null )
                    {
                        ids.Add( account.Id.ToString() );
                        names.Add( account.PublicName );
                        var parentAccount = account.ParentAccount;

                        while ( parentAccount != null )
                        {
                            parentAccountIds += parentAccount.Id.ToString() + ",";
                            parentAccount = parentAccount.ParentAccount;
                        }
                    }
                }

                InitialItemParentIds = parentAccountIds.TrimEnd( new[] { ',' } );
                ItemIds = ids;
                ItemNames = names;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the value on select.
        /// </summary>
        protected override void SetValueOnSelect()
        {
            var item = new FinancialAccountService().Get( int.Parse( ItemId ) );
            this.SetValue( item );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void SetValuesOnSelect()
        {
            var itemIds = ItemIds.Select( int.Parse );
            var items = new FinancialAccountService().Queryable().Where( i => itemIds.Contains( i.Id ) );
            this.SetValues( items );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/financialaccounts/getchildren/"; }
        }

    }
}
