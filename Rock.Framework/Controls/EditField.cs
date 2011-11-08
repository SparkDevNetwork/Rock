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
    [ToolboxData( "<{0}:EditField runat=server></{0}:EditField>" )]
    public class EditField : TemplateField
    {
        public override bool Initialize( bool sortingEnabled, Control control )
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.ItemStyle.CssClass = "grid-icon-cell edit";

            EditFieldTemplate editFieldTemplate = new EditFieldTemplate();
            editFieldTemplate.LinkButtonClick += new RowEventHandler( editFieldTemplate_LinkButtonClick );
            this.ItemTemplate = editFieldTemplate;

            return base.Initialize( sortingEnabled, control );
        }

        void editFieldTemplate_LinkButtonClick( object sender, RowEventArgs e )
        {
            OnClick( e );
        }

        public event RowEventHandler Click;
        public virtual void OnClick( RowEventArgs e )
        {
            if ( Click != null )
                Click( this, e );
        }
    }

    public class EditFieldTemplate : ITemplate
    {
        public void InstantiateIn( Control container )
        {
            DataControlFieldCell cell = container as DataControlFieldCell;
            if ( cell != null )
            {
                LinkButton lbEdit = new LinkButton();
                lbEdit.Text = "Edit";
                lbEdit.Click += new EventHandler( lbEdit_Click );

                cell.Controls.Add( lbEdit );
            }
        }

        void lbEdit_Click( object sender, EventArgs e )
        {
            if ( LinkButtonClick != null )
            {
                GridViewRow row = ( GridViewRow )( ( LinkButton )sender ).Parent.Parent;
                RowEventArgs args = new RowEventArgs( row.RowIndex );
                LinkButtonClick( sender, args );
            }
        }

        internal event RowEventHandler LinkButtonClick;
    }
}