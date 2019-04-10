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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Runs a SQL query
    /// </summary>
    [ActionCategory( "Finance" )]
    [Description( "Adds a new related document to a benevolence request." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Benevolence Request Add Document" )]
    
    [WorkflowAttribute( "Benevolence Request", "Workflow attribute to set the returned benevolence request to.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.BenevolenceRequestFieldType" } )]

    [WorkflowAttribute( "Document", "Workflow attribute that contains the document to be added.", true, "", "", 1, null,
        new string[] { "Rock.Field.Types.FileFieldType" } )]
    public class BenevolenceRequestAddDocument : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var mergeFields = GetMergeFields( action );

            BenevolenceRequestService benevolenceRequestService = new BenevolenceRequestService( rockContext );

            var benevolenceRequest = benevolenceRequestService.Get( GetAttributeValue( action, "BenevolenceRequest", true ).AsGuid() );

            if (benevolenceRequest == null )
            {
                var errorMessage = "Benevolence request could not be found.";
                errorMessages.Add( errorMessage );
                action.AddLogEntry( errorMessage, true );
                return false;
            }

            var binaryFile = new BinaryFileService(rockContext).Get(GetAttributeValue( action, "Document", true ).AsGuid());

            if ( binaryFile == null )
            {
                action.AddLogEntry( "The document to add to the benevolence request was not be found.", true );
                return true; // returning true here to allow the action to run 'successfully' without a document. This allows the action to be easily used when the document is optional without a bunch of action filter tests.
            }

            BenevolenceRequestDocument requestDocument = new BenevolenceRequestDocument();
            benevolenceRequest.Documents.Add( requestDocument );

            requestDocument.BinaryFileId = binaryFile.Id;

            rockContext.SaveChanges();

            action.AddLogEntry( "Added document to the benevolence request." );
            return true;
        }
    }
}
