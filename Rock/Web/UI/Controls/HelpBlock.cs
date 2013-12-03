//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays a help icon that will display the Text property when clicked
    /// </summary>
    [ToolboxData( "<{0}:HelpBlock runat=server></{0}:HelpBlock>" )]
    public class HelpBlock : Literal
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            string script = @"
$(document).ready(function() {
    $('a.help').click(function (e) {
        e.preventDefault();
        $(this).siblings('div.alert-info').slideToggle(function(){
            $('.scroll-container').each(function() {
                $(this).tinyscrollbar_update('relative');
            });
        });
    });
});
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "help-block", script, true );

        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Text.Trim() != string.Empty )
            {
                writer.AddAttribute( "class", "help" );
                writer.AddAttribute( "href", "#" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute("class", "fa fa-question-circle");
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.AddAttribute( "class", "alert alert-info" );
                writer.AddAttribute( "style", "display:none" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.RenderBeginTag( HtmlTextWriterTag.Small );
                writer.Write( this.Text.Trim() );
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
        }
    }
}