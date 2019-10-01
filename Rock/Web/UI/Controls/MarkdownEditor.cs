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
    /// An editor for Markdown
    /// </summary>
    /// <seealso cref="Rock.Web.UI.Controls.RockTextBox" />
    [ToolboxData( "<{0}:MarkdownEditor runat=server></{0}:MarkdownEditor>" )]
    public class MarkdownEditor : CodeEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownEditor"/> class.
        /// </summary>
        public MarkdownEditor()
            : base()
        {
            this.TextMode = TextBoxMode.MultiLine;
            this.EditorHeight = "250";
            this.EditorMode = CodeEditorMode.Markdown;
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            base.RenderControl( writer );
            writer.Write( "<small class='pull-right text-muted' style='margin-top:-20px'><strong>**bold**</strong> &nbsp;<em>*italics*</em> &nbsp;>quote &nbsp;[link text](link address) &nbsp;<a href='http://commonmark.org/help/' class='btn btn-xs text-muted' target='_blank'>more</a></small>" );
        }
    }
}
