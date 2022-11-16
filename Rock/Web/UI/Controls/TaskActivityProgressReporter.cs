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
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays the progress of a task activity.
    /// </summary>
    public class TaskActivityProgressReporter : CompositeControl
    {
        #region Properties

        /// <summary>
        /// Gets or sets the task identifier.
        /// </summary>
        /// <value>The task identifier.</value>
        public string TaskId
        {
            get => ViewState["TaskId"] as string;
            set => ViewState["TaskId"] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a lightweight reporter
        /// like BulkUpdate uses or a full-size reporter like CommunicationEntryWizard.
        /// </summary>
        /// <value><c>true</c> if this instance is a lightweight reporter; otherwise, <c>false</c>.</value>
        public bool IsLightweight
        {
            get => ViewState["IsLightweight"] as bool? ?? true;
            set => ViewState["IsLightweight"] = value;
        }

        /// <summary>
        /// Gets the connection identifier of the RealTime client.
        /// </summary>
        /// <value>The connection identifier of the RealTime client.</value>
        public string ConnectionId
        {
            get
            {
                EnsureChildControls();
                return _hfConnectionId.Value ?? string.Empty;
            }
        }

        #endregion

        #region Child Controls

        private HiddenField _hfConnectionId;

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _hfConnectionId = new HiddenField
            {
                ID = "hfConnectionId"
            };

            Controls.Add( _hfConnectionId );
        }

        /// <inheritdoc/>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            RockPage.AddScriptLink( Page, Page.ResolveUrl( "~/Scripts/Rock/realtime.js" ) );
            RockPage.AddScriptLink( Page, Page.ResolveUrl( "~/Scripts/Rock/Controls/taskActivityProgressReporter.js" ) );

            var script = $@"
;(function() {{
    Rock.controls.taskActivityProgressReporter.initialize({{
        controlId: '{ClientID}'
    }});
}})();";

            ScriptManager.RegisterStartupScript( this, GetType(), "task-activity-progress-reporter-" + ClientID, script, true );
        }

        /// <inheritdoc/>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( IsLightweight )
            {
                RenderLightweightControl( writer );
            }
            else
            {
                throw new NotImplementedException( "Heavy task reporter is not yet implemented." );
            }
        }

        /// <summary>
        /// Renders the lightweight version of the control. This is a simple progress
        /// bar with a small alert at the end showing the final status message.
        /// </summary>
        /// <param name="writer">The HTML writer to render the content to.</param>
        private void RenderLightweightControl( HtmlTextWriter writer )
        {
            // Open container div.
            writer.AddAttribute( "id", ClientID );
            writer.AddAttribute( "data-task-id", TaskId );
            if ( !Visible || TaskId.IsNullOrWhiteSpace() )
            {
                writer.AddStyleAttribute( "display", "none" );
            }
            writer.RenderBeginTag( "div" );

            RenderChildren( writer );

            // Progress spinner and status text.
            writer.AddAttribute( "class", "js-preparing" );
            writer.RenderBeginTag( "div" );
            writer.AddAttribute( "class", "fa fa-spinner fa-spin" );
            writer.RenderBeginTag( "i" );
            writer.RenderEndTag();
            writer.AddAttribute( "class", "js-progress-message" );
            writer.RenderBeginTag( "span" );
            writer.WriteEncodedText( " Preparing..." );
            writer.RenderEndTag();
            writer.RenderEndTag();

            // Open progress bar container.
            writer.AddAttribute( "class", "js-progress-div margin-t-lg" );
            writer.AddStyleAttribute( "display", "none" );
            writer.RenderBeginTag( "div" );

            writer.Write( "<strong>Progress</strong><br />" );

            // Open progress bar.
            writer.AddAttribute( "class", "progress" );
            writer.RenderBeginTag( "div" );
            writer.AddAttribute( "class", "progress-bar js-progress-bar" );
            writer.AddAttribute( "role", "progressbar" );
            writer.AddStyleAttribute( "width", "0%" );
            writer.AddAttribute( "aria-valuenow", "0" );
            writer.AddAttribute( "aria-valuemax", "0" );
            writer.RenderBeginTag( "div" );
            writer.WriteEncodedText( "0/0" );
            writer.RenderEndTag();
            writer.RenderEndTag();

            // Close progress bar container.
            writer.RenderEndTag();

            // Results message.
            writer.AddAttribute( "class", "js-results alert alert-success" );
            writer.AddStyleAttribute( "display", "none" );
            writer.RenderBeginTag( "div" );
            writer.RenderEndTag();

            // Close container div.
            writer.RenderEndTag();
        }

        #endregion
    }
}
