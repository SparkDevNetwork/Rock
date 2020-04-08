// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
    /// A Bootstrap LinkButton as per http://getbootstrap.com/javascript/#buttons can 
    /// disable itself on click and display some loading text. Useful for preventing
    /// a button click action from happening more than once.
    /// </summary>
    [ToolboxData( "<{0}:BootstrapButton runat=server></{0}:BootstrapButton>" )]
    public class BootstrapButton : LinkButton
    {
        /// <summary>
        /// Gets or sets text to use when the button has been clicked.
        /// </summary>
        /// <value>
        /// The button text
        /// </value>
        [
        Bindable( true ),
        Description( "The text to use when the button is disabled and loading." )
        ]
        public string DataLoadingText
        {
            get { return ViewState["DataLoadingText"] as string; }
            set { ViewState["DataLoadingText"] = value; }
        }

        /// <summary>
        /// Gets or sets the time in seconds to display the completed text and message before reverting back to the original text.
        /// </summary>
        /// <value>
        /// The button text
        /// </value>
        [
        Bindable( true ),
        Description( "The time in seconds to display the completed text and message before reverting back to the original text." )
        ]
        public string CompletedDuration
        {
            get { return ViewState["CompletedDuration"] as string ?? string.Empty; }
            set { ViewState["CompletedDuration"] = value; }
        }

        /// <summary>
        /// Gets or sets the text to use for the button when the postback is completed.
        /// </summary>
        /// <value>
        /// The button text
        /// </value>
        [
        Bindable( true ),
        Description( "The text to use for the button when the postback is completed." )
        ]
        public string CompletedText
        {
            get { return ViewState["CompletedText"] as string ?? string.Empty; }
            set { ViewState["CompletedText"] = value; }
        }

        /// <summary>
        /// Gets or sets the text to display to the right of the button when the postback is completed.
        /// </summary>
        /// <value>
        /// The button text
        /// </value>
        [
        Bindable( true ),
        Description( "The text to display to the right of the button when the postback is completed." )
        ]
        public string CompletedMessage
        {
            get { return ViewState["CompletedMessage"] as string ?? string.Empty; }
            set { ViewState["CompletedMessage"] = value; }
        }

        private bool _isButtonClicked = false;

        /// <summary>
        /// Adds the attributes of the <see cref="T:System.Web.UI.WebControls.LinkButton" /> control to the output stream for rendering on the client.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter" /> that contains the output stream to render on the client.</param>
        protected override void AddAttributesToRender( HtmlTextWriter writer )
        {
            // Implementation of LinkButton and WebControl base methods to prevent href value from being rendered...

            if ( this.Page != null )
            {
                this.Page.VerifyRenderingInServerForm( this );
            }

            // Check for enabled/disabled
            bool isEnabled = base.IsEnabled;
            if ( ( this.Enabled && !isEnabled && this.SupportsDisabledAttribute ) || _isButtonClicked )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Disabled, "disabled" );
            }

            if ( this.ID != null )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
            }

            if ( !this.Enabled )
            {
                if ( this.SupportsDisabledAttribute )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Disabled, "disabled" );
                }

                if ( !string.IsNullOrEmpty( this.CssClass ) )
                {
                    this.ControlStyle.CssClass = string.Concat( WebControl.DisabledCssClass, " ", this.CssClass );
                }
                else
                {
                    this.ControlStyle.CssClass = WebControl.DisabledCssClass;
                }
            }

            int tabIndex = this.TabIndex;
            if ( tabIndex != 0 )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Tabindex, tabIndex.ToString( NumberFormatInfo.InvariantInfo ) );
            }
            string toolTip = this.ToolTip;
            if ( toolTip.Length > 0 )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Title, toolTip );
            }

            if ( this.ControlStyleCreated && !this.ControlStyle.IsEmpty )
            {
                this.ControlStyle.AddAttributesToRender( writer, this );
            }

            string postBackEventReference = "";
            if ( isEnabled && this.Page != null )
            {
                PostBackOptions postBackOptions = this.GetPostBackOptions();
                if ( postBackOptions != null )
                {
                    postBackEventReference = this.Page.ClientScript.GetPostBackEventReference( postBackOptions, true );
                }
            }

            if ( !string.IsNullOrWhiteSpace( DataLoadingText ) )
            {
                writer.AddAttribute( "data-loading-text", DataLoadingText );
            }
            writer.AddAttribute( "data-completed-text", CompletedText );
            writer.AddAttribute( "data-completed-message", CompletedMessage );
            writer.AddAttribute( "data-timeout-text", CompletedDuration );
            writer.AddAttribute( "data-init-text", Text );

            writer.AddAttribute( HtmlTextWriterAttribute.Onclick, "Rock.controls.bootstrapButton.showLoading(this);" );
            writer.AddAttribute( HtmlTextWriterAttribute.Href, postBackEventReference );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.WebControls.LinkButton.Command" /> event of the <see cref="T:System.Web.UI.WebControls.LinkButton" /> control.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Web.UI.WebControls.CommandEventArgs" /> that contains the event data.</param>
        protected override void OnCommand( CommandEventArgs e )
        {
            base.OnCommand( e );

            if ( CompletedText.IsNotNullOrWhiteSpace() || CompletedMessage.IsNotNullOrWhiteSpace() )
            {
                _isButtonClicked = true;
                var script = string.Format(
            @"
            Rock.controls.bootstrapButton.onCompleted({0})", this.ClientID );
                ScriptManager.RegisterStartupScript( this, this.GetType(), "BootstrapButton_" + this.ClientID, script, true );
            }

        }

    }
}