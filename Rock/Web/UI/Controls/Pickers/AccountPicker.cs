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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class AccountPicker : ItemPicker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountPicker"/> class.
        /// </summary>
        public AccountPicker(): base()
        {
            this.ShowSelectChildren = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [display active only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display active only]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayActiveOnly
        {
            get { return ViewState["DisplayActiveOnly"] as bool? ?? false; }
            set 
            {
                ViewState["DisplayActiveOnly"] = value;  
                this.ItemRestUrlExtraParams = "/" + value.ToString();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.ItemRestUrlExtraParams = "/" + DisplayActiveOnly.ToString();
            this.IconCssClass = "fa fa-building-o";
            this.CssClass = "picker-lg";
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
                List<int> parentAccountIds = new List<int>();
                var parentAccount = account.ParentAccount;

                while ( parentAccount != null )
                {
                    if ( parentAccountIds.Contains( parentAccount.Id ) )
                    {
                        // infinite recursion
                        break;
                    }

                    parentAccountIds.Insert( 0, parentAccount.Id );
                    parentAccount = parentAccount.ParentAccount;
                }

                InitialItemParentIds = parentAccountIds.AsDelimited( "," );
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
                List<int> parentAccountIds = new List<int>();

                foreach ( var account in financialAccounts )
                {
                    if ( account != null )
                    {
                        ids.Add( account.Id.ToString() );
                        names.Add( account.PublicName );
                        var parentAccount = account.ParentAccount;

                        while ( parentAccount != null )
                        {
                            if ( parentAccountIds.Contains( parentAccount.Id ) )
                            {
                                // infinite recursion
                                break;
                            }

                            parentAccountIds.Insert( 0, parentAccount.Id );
                            parentAccount = parentAccount.ParentAccount;
                        }
                    }
                }

                InitialItemParentIds = parentAccountIds.AsDelimited( "," );
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
            var item = new FinancialAccountService( new RockContext() ).Get( int.Parse( ItemId ) );
            this.SetValue( item );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void SetValuesOnSelect()
        {
            var itemIds = ItemIds.Select( int.Parse );
            var items = new FinancialAccountService( new RockContext() ).Queryable().Where( i => itemIds.Contains( i.Id ) );
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
