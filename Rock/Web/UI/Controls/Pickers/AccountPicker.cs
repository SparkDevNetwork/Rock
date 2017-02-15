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
        private HiddenField _hfLastSelectedAccountId;
        private HyperLink _btnSelectChildAccounts;

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

        /// <summary>
        /// Render any additional picker actions
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderCustomPickerActions( HtmlTextWriter writer )
        {
            base.RenderCustomPickerActions( writer );

            if ( this.AllowMultiSelect )
            {
                _hfLastSelectedAccountId.RenderControl( writer );
                _btnSelectChildAccounts.RenderControl( writer );

                var script = $@"
$('#{this.ClientID}').on('rockTree:selected', function(a,id){{
    $('#{_hfLastSelectedAccountId.ClientID}').val(id);    
}});

$('#{_btnSelectChildAccounts.ClientID}').on('click', function(a){{
    var $btn = $('#{_btnSelectChildAccounts.ClientID}');
    var $rockTree = $btn.closest('.account-picker').find('.rocktree');    

    var lastId = $('#{_hfLastSelectedAccountId.ClientID}').val();
    if (!lastId || lastId == '') {{
        return;
    }}

    var $lastSelectedItemNode = $rockTree.find('[data-id=' + lastId + ']');
    var $lastSelectedNameNode = $lastSelectedItemNode.find('.rocktree-name');
    
    if (!$lastSelectedNameNode.hasClass('selected') || $lastSelectedNameNode.length == 0){{
        // dont do anything if the name node isnt selected
        return;
    }}

    // first set all the child nodes as NOT selected so that we can call 'click' on them to make them selected
    // NOTE: it will only select nodes that are visible
    var $childNameNodes = $lastSelectedItemNode.find('.rocktree-name').filter(':visible').not($lastSelectedNameNode[0]);

    var allChildNodesAlreadySelected = true;
    $childNameNodes.each(function(a) {{
        if (!$(this).hasClass('selected')) {{
            allChildNodesAlreadySelected = false;
        }}
    }});
    
    if (!allChildNodesAlreadySelected) {{
        // mark them all as unselected (just in case some are selected already), then click them to select them 
        $childNameNodes.removeClass('selected');        
        $childNameNodes.click();
    }} else {{
        // if all where already selected, toggle them to unselected
        $childNameNodes.removeClass('selected');        
    }}

    // set the hidden field value back to the original selection since the child ones probably overwrote it 
    $('#{_hfLastSelectedAccountId.ClientID}').val(lastId);
}});
                ";

                ScriptManager.RegisterStartupScript( this, this.GetType(), "rememberLastSelectedAccount", script, true );
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if ( this.AllowMultiSelect )
            {
                _hfLastSelectedAccountId = new HiddenField();
                _hfLastSelectedAccountId.ID = this.ID + "_hfLastSelectedAccountId";
                Controls.Add( _hfLastSelectedAccountId );

                _btnSelectChildAccounts = new HyperLink();
                _btnSelectChildAccounts.ID = this.ID + "_btnSelectChildAccounts";
                _btnSelectChildAccounts.CssClass = "btn btn-xs btn-link pull-right js-selectchildaccounts";
                _btnSelectChildAccounts.Text = "<i class='fa fa-list-ul'></i>";
                _btnSelectChildAccounts.ToolTip = "Select Child Accounts";
                Controls.Add( _btnSelectChildAccounts );
            }
        }
    }
}
