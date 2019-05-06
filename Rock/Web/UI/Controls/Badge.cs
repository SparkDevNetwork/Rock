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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays a bootstrap badge
    /// </summary>
    [ToolboxData( "<{0}:Badge runat=server></{0}:Badge>" )]
    public class Badge : Literal
    {
        /// <summary>
        /// Gets or sets the tool tip.
        /// </summary>
        /// <value>
        /// The tool tip.
        /// </value>
        public string ToolTip 
        {
            get { return ViewState["ToolTip"] as string ?? string.Empty; }
            set { ViewState["ToolTip"] = value; }
        }

        /// <summary>
        /// Gets or sets the type of the badge.
        /// </summary>
        /// <value>
        /// The type of the badge.
        /// </value>
        public string BadgeType
        {
            get { return ViewState["BadgeType"] as string ?? string.Empty; }
            set { ViewState["BadgeType"] = value; }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                string css = "badge";
                if ( !string.IsNullOrWhiteSpace(BadgeType))
                {
                    css += " badge-" + BadgeType.ToLower();
                }

                writer.AddAttribute( "class", css );
                if ( !string.IsNullOrWhiteSpace( ToolTip ) )
                {
                    writer.AddAttribute( "title", ToolTip );
                    writer.AddAttribute( "data-toggle", "tooltip" );
                }
                writer.RenderBeginTag( HtmlTextWriterTag.Span );

                // Renders the Text property
                base.RenderControl( writer );

                writer.RenderEndTag();
            }
        }
    }

}