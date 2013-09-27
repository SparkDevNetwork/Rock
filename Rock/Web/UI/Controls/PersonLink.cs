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
        private Literal _personName;

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        public int PersonId
        {
            get { return ViewState["PersonId"] as int? ?? 0; }
            set { ViewState["PersonId"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the person.
        /// </summary>
        /// <value>
        /// The name of the person.
        /// </value>
        public string PersonName
        {
            get { return _personName.Text; }
            set { _personName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the role.
        /// </summary>
        /// <value>
        /// The role.
        /// </value>
        public string Role
        {
            get { return ViewState["Role"] as string; }
            set { ViewState["Role"] = value; }
        }

        /// <summary>
        /// Gets or sets the photo id.
        /// </summary>
        /// <value>
        /// The photo id.
        /// </value>
        public int? PhotoId
        {
            get { return ViewState["PhotoId"] as int?; }
            set { ViewState["PhotoId"] = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonLink"/> class.
        /// </summary>
        public PersonLink()
        {
            _personName = new Literal();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( Page, ResolveUrl( "~/Scripts/jquery.tooltipster.min.js" ) );
        }

        /// <summary>
        /// Renders the <see cref="T:System.Web.UI.HtmlControls.HtmlContainerControl" /> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that receives the <see cref="T:System.Web.UI.HtmlControls.HtmlContainerControl" /> content.</param>
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
            _personName.RenderControl(writer);
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
                var url = Rock.settings.get('baseUrl') + 'api/People/PopupHtml/' +  origin.attr('personId');
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