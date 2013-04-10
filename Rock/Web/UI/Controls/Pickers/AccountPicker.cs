//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class AccountPicker : ItemPicker
    {
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
        /// Sets the value on select.
        /// </summary>
        protected override void SetValueOnSelect()
        {
            var item = new FinancialAccountService().Get( int.Parse( ItemId ) );
            this.SetValue( item );
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
