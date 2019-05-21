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
        #region Controls

        /// <summary>
        /// The Select All button
        /// </summary>
        private HyperLink _btnSelectAll;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountPicker"/> class.
        /// </summary>
        public AccountPicker() : base()
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
            get
            {
                return ViewState["DisplayActiveOnly"] as bool? ?? false;
            }

            set
            {
                ViewState["DisplayActiveOnly"] = value;
                SetExtraRestParams();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [display public name].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display public name]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayPublicName
        {
            get
            {
                return ViewState["DisplayPublicName"] as bool? ?? false;
            }

            set
            {
                ViewState["DisplayPublicName"] = value;
                SetExtraRestParams();
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _btnSelectAll = new HyperLink();
            _btnSelectAll.ID = "_btnSelectAll";
            _btnSelectAll.CssClass = "btn btn-default btn-xs js-select-all pull-right";
            _btnSelectAll.Text = "Select All";

            this.Controls.Add( _btnSelectAll );
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
                _btnSelectAll.RenderControl( writer );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            SetExtraRestParams();
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
                ItemName = this.DisplayPublicName ? account.PublicName : account.Name;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Returns a list of the ancestor FinancialAccounts of the specified FinancialAccount.
        /// If the ParentFinancialAccount property of the FinancialAccount is not populated, it is assumed to be a top-level node.
        /// </summary>
        /// <param name="financialAccount">The financial account.</param>
        /// <param name="ancestorFinancialAccountIds">The ancestor financial account ids.</param>
        /// <returns></returns>
        private List<int> GetFinancialAccountAncestorsIdList( FinancialAccount financialAccount, List<int> ancestorFinancialAccountIds = null )
        {
            if ( ancestorFinancialAccountIds == null )
            {
                ancestorFinancialAccountIds = new List<int>();
            }

            if ( financialAccount == null )
            {
                return ancestorFinancialAccountIds;
            }

            // If we have encountered this node previously in our tree walk, there is a recursive loop in the tree.
            if ( ancestorFinancialAccountIds.Contains( financialAccount.Id ) )
            {
                return ancestorFinancialAccountIds;
            }

            // Create or add this node to the history stack for this tree walk.
            ancestorFinancialAccountIds.Insert( 0, financialAccount.Id );

            ancestorFinancialAccountIds = this.GetFinancialAccountAncestorsIdList( financialAccount.ParentAccount, ancestorFinancialAccountIds );

            return ancestorFinancialAccountIds;
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
                var parentIds = new List<int>();

                foreach ( var account in accounts )
                {
                    if ( account != null )
                    {
                        ids.Add( account.Id.ToString() );
                        names.Add( this.DisplayPublicName ? account.PublicName : account.Name );
                        var parentAccount = account.ParentAccount;
                        var accountParentIds = GetFinancialAccountAncestorsIdList( parentAccount );
                        foreach ( var accountParentId in accountParentIds )
                        {
                            if ( !parentIds.Contains( accountParentId ) )
                            {
                                parentIds.Add( accountParentId );
                            }
                        }
                    }
                }

                // NOTE: Order is important (parents before children)
                InitialItemParentIds = parentIds.AsDelimited( "," );
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
        /// Sets the extra rest parameters.
        /// </summary>
        private void SetExtraRestParams()
        {
            var extraParams = new System.Text.StringBuilder();
            extraParams.Append( $"/{this.DisplayActiveOnly.ToString()}/{this.DisplayPublicName.ToString()}" );
            ItemRestUrlExtraParams = extraParams.ToString();
        }
    }
}