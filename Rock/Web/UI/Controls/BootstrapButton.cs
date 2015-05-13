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
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( !string.IsNullOrWhiteSpace( DataLoadingText ) )
            {
                writer.AddAttribute( "data-loading-text", DataLoadingText );
            }

            // disabling the line of code below as it causes the click event to not occur in IE in edge cases (fast clicks)
            // see: Patrick's answer here: http://stackoverflow.com/questions/2155048/onclientclick-and-onclick-is-not-working-at-the-same-time
            //this.OnClientClick = "Rock.controls.bootstrapButton.showLoading(this);";
            base.RenderControl( writer );
        }
    }
}