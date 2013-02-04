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
        protected override void OnInit( System.EventArgs e )
        {
            string script = @"
Sys.Application.add_load(function () {

    $('a.help').click(function () {
        $(this).siblings('div.alert-info').slideToggle(function(){
            $('.scroll-container').each(function() {
                $(this).tinyscrollbar_update('relative');
            });
        });
    });
});
";
            this.Page.ClientScript.RegisterStartupScript( this.Page.GetType(), "help-block", script, true );
        }
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Text.Trim() != string.Empty )
            {
                writer.AddAttribute( "class", "help" );
                writer.AddAttribute( "href", "#" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute( "class", "icon-question-sign" );
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