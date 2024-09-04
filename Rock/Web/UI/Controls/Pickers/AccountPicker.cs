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
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select a financial account
    /// </summary>
    public class AccountPicker : ItemPicker
    {

        #region Controls

        private HiddenFieldWithClass _hfSearchValue;
        private HiddenFieldWithClass _hfPickerShowActive;
        private HiddenFieldWithClass _hfViewMode;

        /// <summary>
        /// The Select All button
        /// </summary>
        private HyperLink _btnSelectAll;

        /// <summary>
        /// The checkbox to show inactive groups
        /// </summary>
        private RockCheckBox _cbShowInactiveAccounts;
        #endregion Controls

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountPicker"/> class.
        /// </summary>
        public AccountPicker() : base()
        {
            //Assembly.GetCallingAssembly()
            this.ShowSelectChildren = true;
        }
        #endregion Constructors

        #region Properties
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
        /// Gets or sets the selected account id.
        /// </summary>
        /// <value>
        /// The account id.
        /// </value>
        public int? AccountId
        {
            get
            {
                int selectedId = this.SelectedValue.AsInteger();
                if ( selectedId > 0 )
                {
                    return selectedId;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                SetValue( value );
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ItemPicker"/> should allow a search when used for single select
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enhance for long list]; otherwise, <c>false</c>.
        /// </value>
        public bool EnhanceForLongLists
        {
            get
            {
                return ViewState["EnhanceForLongLists"] as bool? ?? true;
            }

            set
            {
                ViewState["EnhanceForLongLists"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ItemPicker"/> should display the child count item count label on the parent item.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display child item count label]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayChildItemCountLabel
        {
            get
            {
                return ViewState["DisplayChildItemCountLabel"] as bool? ?? false;
            }

            set
            {
                ViewState["DisplayChildItemCountLabel"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the custom data items that will be serialized as a json object that is used to add custom properties to the itemPicker.js node object.
        /// This value should be specified as a json array (i.e. "[{\"itemKey\":\"jsClientPropName\",\"itemValueKey\":\"ServerPropName\"}]").
        /// The json properties "itemKey" and "itemValueKey" must be in camel-case.
        /// </summary>
        /// <value>The custom data items.</value>
        public string CustomDataItems { get; set; }

        #endregion Properties

        #region Methods
        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected override void RegisterJavaScript()
        {
            // Don't call base we have our own JS
            //base.RegisterJavaScript();

            // This json object is used to add custom properties to the itemPicker.js node object
            var customDataItems = CustomDataItems.IsNotNullOrWhiteSpace() ? CustomDataItems : "[]";

            var treeViewScript =
$@"Rock.controls.accountPicker.initialize({{ 
    controlId: '{this.ClientID}',
    restUrl: '{this.ResolveUrl( ItemRestUrl )}',
    searchRestUrl:'{this.ResolveUrl( SearchRestUrl )}',
    getParentIdsUrl:'{this.ResolveUrl( GetParentIdsCollectionUrl )}',
    allowMultiSelect: {this.AllowMultiSelect.ToString().ToLower()},
    allowCategorySelection: {this.UseCategorySelection.ToString().ToLower()},
    categoryPrefix: '{CategoryPrefix}',
    defaultText: '{this.DefaultText}',
    restParams: $('#{ItemRestUrlExtraParamsControl.ClientID}').val(),
    expandedIds: [{this.InitialItemParentIds}],
    expandedCategoryIds: [{this.ExpandedCategoryIds}],
    showSelectChildren: {this.ShowSelectChildren.ToString().ToLower()},
    enhanceForLongLists: {this.EnhanceForLongLists.ToString().ToLower()},
    customDataItems: {customDataItems}
}});

function doPostBack() {{
    {Page.ClientScript.GetPostBackEventReference( this, string.Empty )}
}}
";

            ScriptManager.RegisterStartupScript( this, this.GetType(), "item_picker-treeviewscript_" + this.ClientID, treeViewScript, true );
        }
        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _btnSelectAll = new HyperLink
            {
                ID = this.ID + "_btnSelectAll",
                CssClass = "btn btn-link btn-xs js-select-all",
                Text = "Select All"
            };

            this.Controls.Add( _btnSelectAll );


            _cbShowInactiveAccounts = new RockCheckBox
            {
                ID = this.ID + "_cbShowInactiveAccounts",
                Text = "Show Inactive",
                CssClass = "picker-show-inactive",
                ContainerCssClass = "js-picker-show-inactive",
                SelectedIconCssClass = "fa fa-check-square-o",
                UnSelectedIconCssClass = "fa fa-square-o",
                CausesValidation = false,
                AutoPostBack = true,
            };
            _cbShowInactiveAccounts.CheckedChanged += _cbShowInactiveAccounts_CheckedChanged;
            this.Controls.Add( _cbShowInactiveAccounts );

            _hfSearchValue = new HiddenFieldWithClass
            {
                ID = this.ID + "_hfSearchValue",
                CssClass = "js-existing-search-value"
            };

            this.Controls.Add( _hfSearchValue );

            _hfPickerShowActive = new HiddenFieldWithClass
            {
                ID = this.ID + "_hfPickerShowActive",
                CssClass = "js-picker-showactive-value"
            };
            this.Controls.Add( _hfPickerShowActive );

            _hfViewMode = new HiddenFieldWithClass
            {
                ID = this.ID + "_hfViewMode",
                CssClass = "js-picker-view-mode"
            };

            this.Controls.Add( _hfViewMode );
        }

        private void _cbShowInactiveAccounts_CheckedChanged( object sender, EventArgs e )
        {
            ShowDropDown = true;
            SetExtraRestParams();
        }

        /// <summary>
        /// Render any additional picker actions
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderCustomPickerActions( HtmlTextWriter writer )
        {
            if ( EnhanceForLongLists && _hfSearchValue != null )
            {
                _hfSearchValue.RenderControl( writer );
            }

            if ( _hfPickerShowActive != null )
            {
                _hfPickerShowActive.RenderControl( writer );
            }

            if ( _hfViewMode != null )
            {
                _hfViewMode.RenderControl( writer );
            }

            if ( this.AllowMultiSelect )
            {
                _btnSelectAll.RenderControl( writer );

                writer.Write( "<a class='btn btn-xs btn-link picker-cancel mr-auto' id='btnCancel_{0}'>Cancel</a>", this.ClientID );

                writer.Write( "<a class='btn btn-link btn-xs picker-preview' id='btnPreviewSelection_{0}' title='Preview'><i class='fa fa-list'></i></a>", this.ClientID );
                writer.Write( "<a class='btn btn-link btn-xs picker-treeview' id='btnTreeView_{0}' title='Tree View'><i class='fa fa-sitemap'></i></a>", this.ClientID );
            }
            else
            {
                writer.Write( "<a class='btn btn-xs btn-link picker-cancel mr-auto' id='btnCancel_{0}'>Cancel</a>", this.ClientID );
            }


            if ( !DisplayActiveOnly )
            {
                _cbShowInactiveAccounts.RenderControl( writer );
            }
            else
            {
                this.Controls.Remove( _cbShowInactiveAccounts );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            SetExtraRestParams();
            base.OnInit( e );
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            SetExtraRestParams();
            base.OnLoad( e );
        }

        /// <summary>
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderBaseControl( HtmlTextWriter writer )
        {
            if ( EnableFullWidth )
            {
                this.RemoveCssClass( "picker-lg" );
            }
            else
            {
                this.AddCssClass( "picker-lg" );
            }

            base.IconCssClass = "fa fa-building-o";
            base.PickerMenuCssClasses = "picker-menu dropdown-menu picker-menu-w500 styled-scroll";
            CustomDataItems = "[{\"itemKey\":\"glCode\",\"itemValueKey\":\"GlCode\"},{\"itemKey\":\"path\",\"itemValueKey\":\"Path\"}]";

            // NOTE: The base ItemPicker.RenderBaseControl will do additional CSS class additions.
            base.RenderBaseControl( writer );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="account">The account.</param>
        public void SetValue( FinancialAccount account )
        {
            FinancialAccountCache financialAccountCache;
            if ( account != null )
            {
                financialAccountCache = FinancialAccountCache.Get( account.Id );
            }
            else
            {
                financialAccountCache = null;
            }

            SetValueFromCache( financialAccountCache );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="account">The account.</param>
        public void SetValueFromCache( FinancialAccountCache account )
        {
            if ( account != null )
            {
                ItemId = account.Id.ToString();

                var parentGroupIds = GetFinancialAccountAncestorsIdList( account.ParentAccountId );
                InitialItemParentIds = parentGroupIds.AsDelimited( "," );
                ItemName = account.Name;
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
        /// <param name="financialAccountId">The financial account identifier.</param>
        /// <param name="ancestorFinancialAccountIds">The ancestor financial account ids.</param>
        /// <returns>List&lt;System.Int32&gt;.</returns>
        private List<int> GetFinancialAccountAncestorsIdList( int? financialAccountId, List<int> ancestorFinancialAccountIds = null )
        {
            if ( ancestorFinancialAccountIds == null )
            {
                ancestorFinancialAccountIds = new List<int>();
            }


            if ( financialAccountId == null )
            {
                return ancestorFinancialAccountIds;
            }


            var financialAccount = FinancialAccountCache.Get( financialAccountId.Value );


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

            ancestorFinancialAccountIds = this.GetFinancialAccountAncestorsIdList( financialAccount.ParentAccountId, ancestorFinancialAccountIds );

            return ancestorFinancialAccountIds;
        }

        /// <summary>
        /// Sets the selected values to the specified Financial Accounts
        /// </summary>
        /// <param name="accounts"></param>
        public void SetValues( IEnumerable<FinancialAccount> accounts )
        {
            if ( accounts != null )
            {
                var financialAccountsCache = FinancialAccountCache.GetByIds( accounts?.Select( a => a.Id ) );
                SetValuesFromCache( financialAccountsCache );
            }
            else
            {
                SetValuesFromCache( null );
            }
        }

        /// <summary>
        /// Sets the selected values to the specified Financial Accounts
        /// </summary>
        /// <param name="financialAccountsCache"></param>
        public void SetValuesFromCache( IEnumerable<FinancialAccountCache> financialAccountsCache )
        {
            var financialAccounts = financialAccountsCache?.ToList();

            if ( financialAccounts != null && financialAccounts.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentAccountIds = new List<int>();

                foreach ( var account in financialAccountsCache )
                {
                    if ( account != null )
                    {
                        ids.Add( account.Id.ToString() );
                        names.Add( this.DisplayPublicName ? account.PublicName : account.Name );
                        if ( account.ParentAccount != null && !parentAccountIds.Contains( account.ParentAccount.Id ) )
                        {
                            var parentAccountId = account.ParentAccountId;
                            var accountParentIds = GetFinancialAccountAncestorsIdList( parentAccountId );
                            foreach ( var accountParentId in accountParentIds )
                            {
                                if ( !parentAccountIds.Contains( accountParentId ) )
                                {
                                    parentAccountIds.Add( accountParentId );
                                }
                            }
                        }
                    }
                }

                // NOTE: Order is important (parents before children) since the GroupTreeView loads on demand
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
            var accountId = ItemId.AsIntegerOrNull();
            FinancialAccountCache account = null;
            if ( accountId.HasValue && accountId > 0 )
            {
                account = FinancialAccountCache.Get( accountId.Value );
            }

            SetValueFromCache( account );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void SetValuesOnSelect()
        {
            var accountIds = ItemIds.Where( i => i != "0" ).AsIntegerList();
            if ( accountIds.Any() )
            {
                var accounts = FinancialAccountCache.GetByIds( accountIds );
                this.SetValuesFromCache( accounts );
            }
            else
            {
                this.SetValues( null );
            }
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
        /// Gets the search rest URL.
        /// </summary>
        /// <value>The search rest URL.</value>
        public string SearchRestUrl
        {
            get { return "~/api/financialaccounts/getchildrenbysearchterm/"; }
        }

        /// <summary>
        /// Gets the get parent ids URL.
        /// </summary>
        /// <value>The get parent ids URL.</value>
        public string GetParentIdsCollectionUrl
        {
            get { return "~/api/financialaccounts/getparentidscollection/"; }
        }

        /// <summary>
        /// Sets the extra rest parameters.
        /// </summary>
        private void SetExtraRestParams()
        {
            var activeOnly = this.DisplayActiveOnly;

            if ( !this.DisplayActiveOnly && _cbShowInactiveAccounts != null )
            {
                activeOnly = !_cbShowInactiveAccounts.Checked;
            }

            var extraParams = new System.Text.StringBuilder();
            extraParams.Append( "?activeOnly=" ).Append( activeOnly ).Append( "&displayPublicName=" ).Append( this.DisplayPublicName ).Append( "&countsType=" ).Append( DisplayChildItemCountLabel ? AccountTreeViewItem.GetCountsType.ChildGroups : AccountTreeViewItem.GetCountsType.None );
            ItemRestUrlExtraParams = extraParams.ToString();
        }
        #endregion Methods
    }
}