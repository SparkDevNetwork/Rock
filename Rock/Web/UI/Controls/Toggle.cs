//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
// Based on the prototype Bootstrap switch by Mattia Larentis at http://www.larentis.eu/switch/
// https://github.com/nostalgiaz/bootstrap-switch/commit/265bfbf0e2d7f390e231249fa457c4d3d34d9b42

using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:Toggle runat=server></{0}:Toggle>" )]
    public class Toggle : CheckBox
    {
        /// <summary>
        /// The label
        /// </summary>
        protected Literal label;

        /// <summary>
        /// The help block
        /// </summary>
        protected HelpBlock helpBlock;

        /// <summary>
        /// Gets or sets the help tip.
        /// </summary>
        /// <value>
        /// The help tip.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help tip." )
        ]
        public string Tip
        {
            get { return ViewState["Tip"] as string ?? string.Empty; }
            set { ViewState["Tip"] = value; }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get { return helpBlock.Text; }
            set { helpBlock.Text = value; }
        }

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
            get { return label.Text; }
            set { label.Text = value; }
        }

        /// <summary>
        /// Gets or sets the on text.
        /// </summary>
        /// <value>
        /// The on text.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "On" ),
        Description( "The text to display when selected." )
        ]
        public string OnText
        {
            get { return ViewState["OnText"] as string ?? string.Empty; }
            set { ViewState["OnText"] = value; }
        }

        /// <summary>
        /// Gets or sets the off text.
        /// </summary>
        /// <value>
        /// The off text.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "Off" ),
        Description( "The text to display when not selected." )
        ]
        public string OffText
        {
            get { return ViewState["OffText"] as string ?? string.Empty; }
            set { ViewState["OffText"] = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Toggle" /> class.
        /// </summary>
        public Toggle()
            : base()
        {
            label = new Literal();
            helpBlock = new HelpBlock();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( this.Page, "~/scripts/jquery.switch.js" );

            // Switch does not automatically initialize again after a partial-postback.  This script 
            // looks for any switch elements that have not been initialized and re-intializes them.
            string script = @"
$(document).ready(function() {
    $('.switch > input').each( function () {
        $(this).parent().switch('init');
    });
});
";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "toggle-switch-init", script, true );

        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            Controls.Add( label );
            Controls.Add( helpBlock );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            bool renderWithLabel = ( !string.IsNullOrEmpty( LabelText ) ) ||
                ( !string.IsNullOrEmpty( Help ) );

            if ( renderWithLabel )
            {
                writer.AddAttribute( "class", "control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                if ( label.Text.Trim() != string.Empty )
                {
                    writer.AddAttribute( "class", "control-label" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    label.RenderControl( writer );
                    helpBlock.RenderControl( writer );
                    writer.RenderEndTag();
                }

                writer.AddAttribute( "class", "controls" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
            }

            writer.AddAttribute( "class", "switch " + this.CssClass );
            if (!string.IsNullOrWhiteSpace(OnText))
            {
                writer.AddAttribute( "data-on-label", OnText );
            }

            if ( !string.IsNullOrWhiteSpace( OffText ) )
            {
                writer.AddAttribute( "data-off-label", OffText );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( this.ClientID != null )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
            }
            writer.AddAttribute( HtmlTextWriterAttribute.Type, "checkbox" );
            if ( this.UniqueID != null )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Name, this.UniqueID );
            }
            if ( this.Checked )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Checked, "checked" );
            }
            if ( !base.IsEnabled && this.SupportsDisabledAttribute )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Disabled, "disabled" );
            }
 
            PostBackOptions postBackOption = new PostBackOptions( this, string.Empty );
            if ( this.CausesValidation && this.Page.GetValidators( this.ValidationGroup ).Count > 0 )
            {
                postBackOption.PerformValidation = true;
                postBackOption.ValidationGroup = this.ValidationGroup;
            }
            if ( this.Page.Form != null )
            {
                postBackOption.AutoPostBack = true;
            }
            string onClick = this.Page.ClientScript.GetPostBackEventReference( postBackOption, true );
            writer.AddAttribute( HtmlTextWriterAttribute.Onchange, onClick );
            
            string accessKey = this.AccessKey;
            if ( accessKey.Length > 0 )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Accesskey, accessKey );
            }
            int tabIndex = this.TabIndex;
            if ( tabIndex != 0 )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Tabindex, tabIndex.ToString( NumberFormatInfo.InvariantInfo ) );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Input );
            writer.RenderEndTag();

            writer.RenderEndTag();

            if ( renderWithLabel )
            {
                if ( label.Text.Trim() == string.Empty )
                {
                    helpBlock.RenderControl( writer );
                }

                if ( Tip.Trim() != string.Empty )
                {
                    writer.AddAttribute( "class", "help-tip" );
                    writer.AddAttribute( "href", "#" );
                    writer.RenderBeginTag( HtmlTextWriterTag.A );
                    writer.RenderBeginTag( HtmlTextWriterTag.Span );
                    writer.Write( Tip.Trim() );
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }

                writer.RenderEndTag();

                writer.RenderEndTag();
            }
        }
    }
}