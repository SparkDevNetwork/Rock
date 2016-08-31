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
    /// 
    /// </summary>
    [ToolboxData( "<{0}:ColorPicker runat=server></{0}:ColorPicker>" )]
    public class ColorPicker : RockTextBox
    {
        /// <summary>
        /// Gets or sets the original value.
        /// </summary>
        /// <value>
        /// The original value.
        /// </value>
        public string OriginalValue
        { 
            get { return ViewState["OriginalValue"] as string ?? string.Empty; }
            set { ViewState["OriginalValue"] = value; }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value {
            get
            {
                EnsureChildControls();
                return this.Text;
            }

            set
            {
                EnsureChildControls();
                this.Text = value;
            }
        }
        
        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            this.AppendText = "<i></i>";
            this.AddCssClass( "rock-colorpicker-input input-width-lg" );

            string script = "$('.rock-colorpicker-input').colorpicker();";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "rock-colorpicker", script, true );
            base.RenderControl( writer );

        }
    }
}
