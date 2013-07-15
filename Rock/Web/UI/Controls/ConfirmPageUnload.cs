//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Renders the title of a page
    /// </summary>
    [ToolboxData( "<{0}:ConfirmPageUnload runat=server></{0}:ConfirmPageUnload>" )]
    public class ConfirmPageUnload : Control
    {
        /// <summary>
        /// Gets or sets the confirmation message.
        /// </summary>
        /// <value>The confirmation message.</value>
        public string ConfirmationMessage
        {
            get { return ViewState["ConfirmationMessage"] as string ?? string.Empty; }
            set { ViewState["ConfirmationMessage"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ConfirmPageUnload"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get { return ViewState["Enabled"] as bool? ?? false; }
            set { ViewState["Enabled"] = value; }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            string script = string.Format( @"
    window.addEventListener('beforeunload', function(e) {{
        confirmationMessage = $('#{0}').val();
        (e || window.event).returnValue = confirmationMessage;
        return confirmationMessage;
    }});
", this.ClientID );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "ConfirmPageUnload", script, true );
        }

        public override void RenderControl( HtmlTextWriter writer )
        {
            base.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Type, "hidden" );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
            writer.AddAttribute( HtmlTextWriterAttribute.Value, Enabled ? ConfirmationMessage : "" );
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }
    }
}