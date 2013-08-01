//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.ComponentModel;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control with numerical validation 
    /// </summary>
    [ToolboxData( "<{0}:PersonLink runat=server></{0}:PersonLink>" )]
    public class PersonLink : HtmlAnchor
    {
        Literal personName;

        public int PersonId
        {
            get { return ViewState["PersonId"] as int? ?? 0; }
            set { ViewState["PersonId"] = value; }
        }

        public string PersonName
        {
            get { return personName.Text; }
            set { personName.Text = value; }
        }

        public string Role
        {
            get { return ViewState["Role"] as string; }
            set { ViewState["Role"] = value; }
        }

        public int? PhotoId
        {
            get { return ViewState["PhotoId"] as int?; }
            set { ViewState["PhotoId"] = value; }
        }

        public PersonLink()
        {
            personName = new Literal();
        }

        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( Page, ResolveUrl( "~/Scripts/jquery.tooltipster.min.js" ) );
        }

        protected override void Render( HtmlTextWriter writer )
        {
            this.AddCssClass( "popover-person" );

            HRef = Path.Combine( System.Web.VirtualPathUtility.ToAbsolute( "~" ), string.Format( "Person/{0}", PersonId ) );

            this.Attributes["personId"] = PersonId.ToString();
            base.RenderBeginTag( writer );

            if ( PhotoId.HasValue && PhotoId.Value != 0 )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "icon-circle" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Strong );
            personName.RenderControl(writer);
            writer.RenderEndTag();

            base.RenderEndTag( writer );

            if ( !string.IsNullOrWhiteSpace( Role ) )
            {
                writer.Write( " (" );
                writer.Write( Role );
                writer.Write( ")" );
            }

            string script = @"
    $('.popover-person').tooltipster({
        content: 'Loading...',
        position: 'right', 
        interactive: true, 
        interactiveTolerance: 350,
        updateAnimation: false,
        functionBefore: function(origin, continueTooltip) {
            continueTooltip();
            if (origin.data('ajax') !== 'cached') {
                var url = rock.baseUrl + 'api/People/PopupHtml/' +  origin.attr('personId');
                $.get(
                    url,
                    function(data) {
                        origin.tooltipster('update', data.PickerItemDetailsHtml).data('ajax', 'cached');
                    }
                );
            }
        }
    });
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "person-link-popover", script, true );

        }
    }
}