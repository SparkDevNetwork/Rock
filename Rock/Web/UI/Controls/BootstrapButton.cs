//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
            get { return ViewState["DataLoadingText"] as string ?? "<i class='icon-spinner icon-spin icon-large'></i>"; }
            set { ViewState["DataLoadingText"] = value; }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( !string.IsNullOrWhiteSpace( DataLoadingText ) )
            {
                writer.AddAttribute( "data-loading-text", DataLoadingText );
            }

            this.OnClientClick = "Rock.controls.bootstrapButton.showLoading(this);";
            base.RenderControl( writer );
        }
    }
}