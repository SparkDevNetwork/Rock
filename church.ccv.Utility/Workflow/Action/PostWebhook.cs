using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using RestSharp;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Workflow;

namespace church.ccv.Utility.Workflow.Action
{
    /// <summary>
    /// Webhook Post to API endpoint
    /// </summary>
    [ActionCategory( "Utility" )]
    [Description( "Posts the specified template to an API endpoint." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Webhook Post" )]
    [CodeEditorField( "Template", "The body of the post submission <span class='tip tip-lava'></span>", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 400, true, "", "", 0 )]
    [TextField( "Url", "The URL to use <span class='tip tip-lava'></span>", false, "", "", 2 )]
    public class PostWebhook : ActionComponent
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

            var template = GetAttributeValue( action, "Template" );

            var mergeFields = GetMergeFields( action );
            template = template.ResolveMergeFields( mergeFields );

            try
            {
                var client = new RestClient( GetAttributeValue( action, "Url" ) );
                var request = new RestRequest( Method.POST );
                request.AddHeader( "cache-control", "no-cache" );
                request.AddParameter( "undefined", template, ParameterType.RequestBody );
                IRestResponse response = client.Execute( request );

                return true;
            }
            catch ( Exception ex )
            {
                action.AddLogEntry( ex.Message, true );
                errorMessages.Add( ex.Message );
                return false;
            }
        }
    }
}
