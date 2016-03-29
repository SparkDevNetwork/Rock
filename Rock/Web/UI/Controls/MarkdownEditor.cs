using System;
using System.Collections.Generic;
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
        /// Initializes a new instance of the <see cref="MarkdownEditor"/> class.
        /// </summary>
        public MarkdownEditor()
            : base()
        {
            this.Rows = 3;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.TextMode = TextBoxMode.MultiLine;
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderBaseControl( System.Web.UI.HtmlTextWriter writer )
        {
            base.RenderBaseControl( writer );
            writer.Write( "<small class='pull-right text-muted'><strong>**bold**</strong> &nbsp;<em>*italics*</em> &nbsp;>quote &nbsp;[link text](link address) &nbsp;<a href='http://commonmark.org/help/' class='btn btn-xs text-muted' target='_blank'>more</a></small>" );
        }
    }
}
