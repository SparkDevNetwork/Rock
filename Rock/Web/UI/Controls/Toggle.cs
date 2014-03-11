// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
    public class Toggle : RockCheckBox
    {
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
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderBaseControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( "class", "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "switch " + this.CssClass );
            if (!string.IsNullOrWhiteSpace( OnText ) )
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
            if ( onClick.EndsWith( ", 0)" ) )
            {
                onClick = onClick.Substring( 0, onClick.Length - 2 ) + "225)";
            }
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

            writer.RenderEndTag();  // switch
            writer.RenderEndTag();  // controls
        }
    }
}