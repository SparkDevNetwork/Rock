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
    /// Renders the title of a page
    /// </summary>
    [ToolboxData( "<{0}:ConfirmPageUnload runat=server></{0}:ConfirmPageUnload>" )]
    public class ConfirmPageUnload : Control
    {
        /// <summary>
        /// Gets or sets the confirmation message.
        /// </summary>
        /// <value>The confirmation message.</value>
        public string ConfirmationMessage
        {
            get { return ViewState["ConfirmationMessage"] as string ?? string.Empty; }
            set { ViewState["ConfirmationMessage"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ConfirmPageUnload"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get { return ViewState["Enabled"] as bool? ?? false; }
            set { ViewState["Enabled"] = value; }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            string script = string.Format( @"
    window.addEventListener('beforeunload', function(e) {{
        confirmationMessage = $('#{0}').val();
        
        var activeControl = document.activeElement;

        if ($('#{1}').find(activeControl).length ) {{
           // todo if it isn't the cancel button

           // if the active control is a child of our update panel, assume we aren't navigating away
           return;           
        }}

        if ($(activeControl).parents('.modal').length) {{
            // if the active control is part of a modal (for example, a confirm delete modal) assume we aren't navigating away
           return; 
        }}
 
        if (confirmationMessage || '' > '') {{
          (e || window.event).returnValue = confirmationMessage;
          return confirmationMessage;
        }}
    }});
", this.ClientID, this.ParentUpdatePanel().ClientID );

            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "ConfirmPageUnload", script, true );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            base.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Type, "hidden" );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
            writer.AddAttribute( HtmlTextWriterAttribute.Value, Enabled ? ConfirmationMessage : "" );
            writer.RenderBeginTag( HtmlTextWriterTag.Input );
            writer.RenderEndTag();
        }
    }
}