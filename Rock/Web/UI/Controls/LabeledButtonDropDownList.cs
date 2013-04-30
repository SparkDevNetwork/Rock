using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    [ToolboxData( "<{0}:LabeledButtonDropDownList runat=server></{0}:LabeledButtonDropDownList>" )]
    public class LabeledButtonDropDownList : ButtonDropDownList, ILabeledControl
    {
        protected Label label;

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string LabelText
        {
            get
            {
                base.EnsureChildControls();
                return label.Text;
            }
            set
            {
                base.EnsureChildControls();
                label.Text = value;
            }
        }


        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            EnsureChildControls();                                    
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            label = new Label();
            Controls.Add( label );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        protected override void Render( HtmlTextWriter writer )
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
