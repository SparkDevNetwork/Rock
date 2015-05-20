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
using System;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;

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
            get { return ViewState["DataLoadingText"] as string ?? "<i class='fa fa-refresh fa-spin working'></i>"; }
            set { ViewState["DataLoadingText"] = value; }
        }

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
            if ( this.Enabled && !isEnabled && this.SupportsDisabledAttribute )
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

            writer.AddAttribute( HtmlTextWriterAttribute.Onclick, "Rock.controls.bootstrapButton.showLoading(this);" + postBackEventReference );
            writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
        }

    }
}