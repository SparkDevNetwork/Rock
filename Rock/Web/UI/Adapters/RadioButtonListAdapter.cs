using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Adapters;

namespace Rock.Web.UI.Adapters
{
    public class RadioButtonListAdapter : WebControlAdapter
    {
        protected override void RenderBeginTag( System.Web.UI.HtmlTextWriter writer )
        {
        }

        protected override void RenderEndTag( System.Web.UI.HtmlTextWriter writer )
        {
        }

        protected override void RenderContents( System.Web.UI.HtmlTextWriter writer )
        {
            RadioButtonList rbl = Control as RadioButtonList;
            if ( rbl != null )
            {
                int i = 0;
                foreach ( ListItem li in rbl.Items )
                {
                    writer.WriteLine();
                    writer.AddAttribute( "class", "radio inline" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Label );

                    string itemId = string.Format( "{0}_{1}", rbl.ClientID, i++ );
                    writer.AddAttribute( "id", itemId );
                    writer.AddAttribute( "type", "radio" );
                    writer.AddAttribute( "name", rbl.UniqueID );
                    writer.AddAttribute( "value", li.Value );
                    if ( li.Selected )
                    {
                        writer.AddAttribute( "checked", "checked" );
                    }
                    writer.RenderBeginTag( HtmlTextWriterTag.Input );
                    writer.RenderEndTag();

                    writer.Write( li.Text );

                    writer.RenderEndTag();

                    if ( Page != null && Page.ClientScript != null )
                    {
                        Page.ClientScript.RegisterForEventValidation( rbl.UniqueID, li.Value );
                    }
                }

                if ( Page != null && Page.ClientScript != null )
                {
                    Page.ClientScript.RegisterForEventValidation( rbl.UniqueID );
                }
            }
        }
    }
}