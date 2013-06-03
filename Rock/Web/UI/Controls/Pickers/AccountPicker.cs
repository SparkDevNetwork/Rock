//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Collections.Generic;
using System.Linq;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class AccountPicker : ItemPicker, ILabeledControl
    {
        private Label label;

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        public string LabelText
        {
            get { return label.Text; }
            set 
            { 
                label.Text = value;
                base.FieldName = label.Text;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountPicker" /> class.
        /// </summary>
        public AccountPicker()
            : base()
        {
            label = new Label();
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
            var items = new FinancialAccountService().Queryable().Where( i => ItemIds.Contains( i.ToString() ) );
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
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Add( label );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( string.IsNullOrEmpty( LabelText ) )
            {
                base.RenderControl( writer );
            }
            else
            {
                writer.AddAttribute( "class", "control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                label.AddCssClass( "control-label" );

                label.RenderControl( writer );

                writer.AddAttribute( "class", "controls" );

                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                base.Render( writer );

                writer.RenderEndTag();

                writer.RenderEndTag();
            }
        }
    }
}
