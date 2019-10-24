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
using System.Linq;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="HelpBlock"/> class.
        /// </summary>
        public HelpBlock()
        {
            var style = new Style();
            this.Style = style.GetStyleAttributes( this );
        }

        /// <summary>
        /// Gets or sets the style.
        /// </summary>
        /// <value>
        /// The style.
        /// </value>
        public CssStyleCollection Style { get; set; }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Text.Trim() != string.Empty )
            {
                writer.AddAttribute( "class", "help" );
                writer.AddAttribute( "href", "#" );
                writer.AddAttribute( "tabindex", "-1" );

                foreach (var key in this.Style.Keys.OfType<HtmlTextWriterStyle>())
                {
                    writer.AddStyleAttribute( key, this.Style[key] );
                }

                writer.AddAttribute("data-toggle", "tooltip");
                writer.AddAttribute("data-placement", "auto");
                writer.AddAttribute("data-container", "body");
                writer.AddAttribute("data-html", "true");
                writer.AddAttribute("title", this.Text.ConvertCrLfToHtmlBr().Trim() );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute("class", "fa fa-info-circle");
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
        }
    }
}