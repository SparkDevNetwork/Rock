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

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Renders a term and description
    /// </summary>
    [ToolboxData( "<{0}:TermDescription runat=server></{0}:TermDescription>" )]
    public class TermDescription : Control
    {
        /// <summary>
        /// Gets or sets the term.
        /// </summary>
        /// <value>
        /// The term.
        /// </value>
        public string Term
        {
            get { return ViewState["Term"] as string ?? string.Empty; }
            set { ViewState["Term"] = value; }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get { return ViewState["Description"] as string ?? string.Empty; }
            set { ViewState["Description"] = value; }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                writer.RenderBeginTag( HtmlTextWriterTag.Dt );
                writer.Write( CheckEmpty( Term ) );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Dd );
                writer.Write( CheckEmpty( Description ) );
                writer.RenderEndTag();
            }
        }

        private string CheckEmpty( string value )
        {
            if ( string.IsNullOrEmpty( value ) )
            {
                return "&nbsp;";
            }
            else
            {
                return value;
            }
        }

    }
}