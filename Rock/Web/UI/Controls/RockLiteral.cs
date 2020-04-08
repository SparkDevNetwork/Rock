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
    /// A <see cref="T:System.Web.UI.WebControls.Literal"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:RockLiteral runat=server></{0}:RockLiteral>" )]
    public class RockLiteral : Literal
    {
        #region Properties

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        /// <value>
        /// The CSS class.
        /// </value>
        public string CssClass
        {
            get { return ViewState["CssClass"] as string ?? string.Empty; }
            set { ViewState["CssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the label CSS class.
        /// </summary>
        /// <value>
        /// The label CSS class.
        /// </value>
        public string LabelCssClass
        {
            get { return ViewState["LabelCssClass"] as string ?? string.Empty; }
            set { ViewState["LabelCssClass"] = value; }
        }
        #endregion

        /// <summary>
        /// Renders a label and <see cref="T:System.Web.UI.WebControls.TextBox"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group static-control " + CssClass );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, ("control-label " + this.LabelCssClass).Trim() );
                writer.AddAttribute( HtmlTextWriterAttribute.For, this.ClientID );
                writer.RenderBeginTag( HtmlTextWriterTag.Label );
                writer.Write( Label );
                writer.RenderEndTag();  // label

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-wrapper" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute("class", "form-control-static");
                writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
                writer.RenderBeginTag( HtmlTextWriterTag.Div ); // changed this from a <p> to a <div> as controls that have a <div> can not be rendered into a <p>.
                base.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderEndTag();  // control-wrapper

                writer.RenderEndTag();  // form-group            
            }
        }
    }
}