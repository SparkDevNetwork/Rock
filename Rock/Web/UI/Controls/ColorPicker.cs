using System.Web.UI;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:ColorPicker runat=server></{0}:ColorPicker>" )]
    public class ColorPicker : RockTextBox
    {

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            this.Text = this.Value;

            this.AppendText = "<i></i>";
            this.AddCssClass( "rock-colorpicker-input" );

            string script = "$('.rock-colorpicker-input').colorpicker();";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "rock-colorpicker", script, true );
            base.RenderControl( writer );

        }
    }
}
