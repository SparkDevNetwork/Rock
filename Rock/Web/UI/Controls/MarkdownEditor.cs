using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// An editor for Markdown
    /// </summary>
    /// <seealso cref="Rock.Web.UI.Controls.RockTextBox" />
    [ToolboxData( "<{0}:MarkdownEditor runat=server></{0}:MarkdownEditor>" )]
    public class MarkdownEditor : RockTextBox
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.TextMode = TextBoxMode.MultiLine;
            this.Rows = 3;
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderBaseControl( System.Web.UI.HtmlTextWriter writer )
        {
            base.RenderBaseControl( writer );
            writer.Write( "<a href='http://commonmark.org/help/' class='btn btn-xs pull-right text-muted' target='_blank'>markdown</a>" );
        }
    }
}
