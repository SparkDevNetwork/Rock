using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Workflow;

namespace org.newpointe.ExtraActions
{
    /// <summary>
    /// InjectHtml
    /// </summary>
    [ActionCategory( "Extra Actions" )]
    [Description( "Shows HTML in the WorkflowEntry block. NOTE: This will be replaced with a built-in action in Rock v7! You should replace this once you upgrade!" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Show HTML (Workflow Entry Only)" )]

    [CodeEditorField( "HTML", "The HTML to show. <span class='tip tip-lava'></span>", CodeEditorMode.Html, CodeEditorTheme.Rock, 200, true, "Boop" )]
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
                    RockBlock workflowEntryBlock = page.ControlsOfTypeRecursive<RockBlock>().FirstOrDefault( x => x.BlockName == "Workflow Entry" );
                    if ( workflowEntryBlock != null )
                    {
                        workflowEntryBlock.PreRender += ( sender, args ) =>
                        {
                            NotificationBox notificationBox = workflowEntryBlock.ControlsOfTypeRecursive<NotificationBox>().FirstOrDefault();
                            if ( notificationBox != null )
                            {
                                notificationBox.Visible = notificationBox.Visible && !GetAttributeValue( action, "HideStatusMessage" ).AsBoolean();
                                int index = notificationBox.Parent.Controls.IndexOf( notificationBox );
                                if ( index > -1 )
                                    notificationBox.Parent.Controls.AddAt( index + 1, new System.Web.UI.LiteralControl( GetAttributeValue( action, "HTML" ).ResolveMergeFields( GetMergeFields( action ) ) ) );
                            }
                        };
                    }
                }
            }
            return true;
        }
    }
}
