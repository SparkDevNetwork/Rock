using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Controls
{
    [ToolboxData( "<{0}:DeleteField runat=server></{0}:DeleteField>" )]
    public class DeleteField : TemplateField
    {
        public override bool Initialize( bool sortingEnabled, Control control )
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.ItemStyle.CssClass = "grid-icon-cell delete";

            DeleteFieldTemplate deleteFieldTemplate = new DeleteFieldTemplate();
            deleteFieldTemplate.LinkButtonClick += deleteFieldTemplate_LinkButtonClick;
            this.ItemTemplate = deleteFieldTemplate;

            return base.Initialize( sortingEnabled, control );
        }

        void deleteFieldTemplate_LinkButtonClick( object sender, RowEventArgs e )
        {
            OnClick( e );
        }

        public event EventHandler<RowEventArgs> Click;
        public virtual void OnClick( RowEventArgs e )
        {
            if ( Click != null )
                Click( this, e );
        }
    }

    public class DeleteFieldTemplate : ITemplate
    {
        public void InstantiateIn( Control container )
        {
            DataControlFieldCell cell = container as DataControlFieldCell;
            if ( cell != null )
            {
                LinkButton lbDelete = new LinkButton();
                lbDelete.Text = "Delete";
                lbDelete.Click += lbDelete_Click;

                cell.Controls.Add( lbDelete );
            }
        }

        void lbDelete_Click( object sender, EventArgs e )
        {
            if ( LinkButtonClick != null )
            {
                GridViewRow row = ( GridViewRow )( ( LinkButton )sender ).Parent.Parent;
                RowEventArgs args = new RowEventArgs( row.RowIndex );
                LinkButtonClick( sender, args );
            }
        }

        internal event EventHandler<RowEventArgs> LinkButtonClick;
    }
}