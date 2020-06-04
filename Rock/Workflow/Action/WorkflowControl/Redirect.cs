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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Web;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Redirects the user to a different page.
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Redirects the user to a different page." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Redirect to Page" )]

    [WorkflowTextOrAttribute( "Url", "Url Attribute", "The full URL to redirect to, for example: http://www.rockrms.com  <span class='tip tip-lava'></span>", true, "", "", 0, "Url", new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.UrlLinkFieldType", "Rock.Field.Types.AudioUrlFieldType", "Rock.Field.Types.VideoUrlFieldType" } )]
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
                url = action.GetWorkflowAttributeValue( guid );
            }

            var processOpt = GetAttributeValue( action, "ProcessingOptions" );
            bool canSendRedirect = !string.IsNullOrWhiteSpace( url ) && HttpContext.Current != null;

            if ( canSendRedirect )
            {
                HttpContext.Current.Response.Redirect( url, false );
            }

            if ( processOpt == "1" )
            {
                // 1) if HttpContext.Current is null, this workflow action might be running as a job (there is no browser session associated), so Redirect couldn't have been sent to a browser
                // 2) if there was no url specified, the redirect wouldn't have executed either
                return canSendRedirect;
            }
            else
            {
                return processOpt != "2";
            }
        }
    }
}
