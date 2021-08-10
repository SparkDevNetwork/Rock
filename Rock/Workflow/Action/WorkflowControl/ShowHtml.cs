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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Shows HTML in the WorkflowEntry block
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Shows HTML in the WorkflowEntry block." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Workflow Entry Show HTML" )]

    [CodeEditorField( "HTML", "The HTML to show. <span class='tip tip-lava'></span>", Rock.Web.UI.Controls.CodeEditorMode.Html, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, true, "", "", 0 )]
    [BooleanField( "Hide Status Message", "Whether or not to hide the built-in status message.", false, "", 1 )]
    public class ShowHtml : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            HttpContext httpContext = HttpContext.Current;
            if ( httpContext != null )
            {
                System.Web.UI.Page page = httpContext.Handler as System.Web.UI.Page;
                if ( page != null )
                {
                    var workflowEntryBlock = page.ControlsOfTypeRecursive<Rock.Web.UI.RockBlock>().Where( x => x.BlockCache?.BlockType?.Guid == Rock.SystemGuid.BlockType.WORKFLOW_ENTRY.AsGuid() ).FirstOrDefault();
                    if ( workflowEntryBlock != null )
                    {
                        workflowEntryBlock.PreRender += ( sender, args ) =>
                        {
                            var notificationBox = workflowEntryBlock.ControlsOfTypeRecursive<Rock.Web.UI.Controls.NotificationBox>().Where( a => a.CssClass?.Contains( "js-workflow-entry-message-notification-box" ) == true ).FirstOrDefault();
                            if ( notificationBox != null )
                            {
                                notificationBox.Visible = notificationBox.Visible && !GetAttributeValue( action, "HideStatusMessage" ).AsBoolean();
                                var index = notificationBox.Parent.Controls.IndexOf( notificationBox );
                                if ( index > -1 )
                                {
                                    notificationBox.Parent.Controls.AddAt( index + 1, new System.Web.UI.LiteralControl( GetAttributeValue( action, "HTML" ).ResolveMergeFields( GetMergeFields( action ) ) ) );
                                }
                            }
                        };
                    }
                }
            }

            return true;
        }
    }
}
