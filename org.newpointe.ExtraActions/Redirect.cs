using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Web;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Workflow;

namespace org.newpointe.ExtraActions
{
    /// <summary>
    /// Redirects the user to a different page.
    /// </summary>
    [ActionCategory( "Extra Actions" )]
    [Description( "Redirects the user to a different page. NOTE: This will be replaced with a built-in action in Rock v7! You should replace this once you upgrade!" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Redirect to Page" )]

    [UrlLinkField( "Url", "The Url to redirect to." )]
    [WorkflowTextOrAttribute( "Url", "Url Attribute", "The Url to redirect to.  <span class='tip tip-lava'></span>", true, "", "", 0, "Url", new[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.UrlLinkFieldType", "Rock.Field.Types.AudioUrlFieldType", "Rock.Field.Types.VideoUrlFieldType" } )]
    [CustomDropdownListField( "Processing Options", "How should workflow continue processing?", "0^Always continue,1^Only continue on redirect,2^Never continue", true, "0", "", 1 )]
    public class Redirect : ActionComponent
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

            string url = GetAttributeValue( action, "Url" );
            Guid guid = url.AsGuid();
            if ( guid.IsEmpty() )
            {
                url = url.ResolveMergeFields( GetMergeFields( action ) );
            }
            else
            {
                url = action.GetWorklowAttributeValue( guid );
            }

            if ( !string.IsNullOrWhiteSpace( url ) && HttpContext.Current != null )
            {
                HttpContext.Current.Response.Redirect( url, false );
            }

            string processOpt = GetAttributeValue( action, "ProcessingOptions" );
            if ( processOpt == "1" )
            {
                return HttpContext.Current != null;
            }

            return processOpt != "2";

        }
    }
}
