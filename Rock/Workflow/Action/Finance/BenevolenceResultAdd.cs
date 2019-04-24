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
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Runs a SQL query
    /// </summary>
    [ActionCategory( "Finance" )]
    [Description( "Adds a benevolence result." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Benevolence Result Add" )]

    [WorkflowAttribute( "Benevolence Request", "Workflow attribute to use to set the resut for.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.BenevolenceRequestFieldType" } )]

    [WorkflowTextOrAttribute( "Result Details", "Results Detail Attribute", "Text or workflow attribute that contains the result's details. <span class='tip tip-lava'></span>", false, "", "", 1, "ResultDetails",
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" }, 2 )]

    [WorkflowTextOrAttribute( "Next Steps", "Next Steps Attribute", "Text or workflow attribute that contains the next steps provided. <span class='tip tip-lava'></span>", false, "", "", 1, "NextSteps",
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" }, 2 )]

    [WorkflowAttribute( "Result Type", "Workflow attribute that contains the result type defined value.", true, "", "", 3, null,
        new string[] { "Rock.Field.Types.DefinedValueFieldType" } )]

    [WorkflowAttribute( "Result Amount", "Workflow attribute that contains the amount of assistance that was given.", true, "", "", 4, null,
        new string[] { "Rock.Field.Types.CurrencyFieldType" } )]

    [WorkflowTextOrAttribute( "Result Summary", "Result Summary Attribute", "Text or workflow attribute that contains the benevolence result summary. <span class='tip tip-lava'></span>", false, "", "", 5, "ResultSummary",
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" }, 2 )]


    public class BenevolenceResultAdd : ActionComponent
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

            // get benevolence request id
            var requestGuid = GetAttributeValue( action, "BenevolenceRequest", true ).AsGuidOrNull();

            if ( !requestGuid.HasValue )
            {
                var errorMessage = "A valid benevolence request ID was not provided.";
                errorMessages.Add( errorMessage );
                action.AddLogEntry( errorMessage, true );
                return false;
            }

            // get result summary
            var resultSummary = GetAttributeValue( action, "ResultSummary", true ).ResolveMergeFields( mergeFields );

            // get next steps
            var nextSteps = GetAttributeValue( action, "NextSteps", true ).ResolveMergeFields( mergeFields );

            // get result type
            var resultType = DefinedValueCache.Get( GetAttributeValue( action, "ResultType", true ).AsGuid() );
            if (resultType == null )
            {
                var errorMessage = "A valid result type was not provided.";
                errorMessages.Add( errorMessage );
                action.AddLogEntry( errorMessage, true );
                return false;
            }

            // get result details
            var resultDetails = GetAttributeValue( action, "ResultDetails", true ).ResolveMergeFields( mergeFields );

            // get amount
            var amount = GetAttributeValue( action, "ResultAmount", true ).AsDecimalOrNull();
            if ( !amount.HasValue )
            {
                var errorMessage = "A valid result amount was not provided.";
                errorMessages.Add( errorMessage );
                action.AddLogEntry( errorMessage, true );
                return false;
            }

            // create benevolence request
            BenevolenceRequestService benevolenceRequestService = new BenevolenceRequestService( rockContext );

            BenevolenceRequest request = benevolenceRequestService.Get( requestGuid.Value );

            if (request == null )
            {
                var errorMessage = "The benevolence request provided could not be found.";
                errorMessages.Add( errorMessage );
                action.AddLogEntry( errorMessage, true );
                return false;
            }
            
            if ( nextSteps.IsNotNullOrWhiteSpace() )
            {
                request.ProvidedNextSteps = nextSteps;
            }

            if ( resultSummary.IsNotNullOrWhiteSpace() )
            {
                request.ResultSummary = resultSummary;
            }

            BenevolenceResult result = new BenevolenceResult();
            request.BenevolenceResults.Add( result );

            result.Amount = amount;
            result.ResultTypeValueId = resultType.Id;
            result.ResultSummary = resultDetails;

            rockContext.SaveChanges();
            
            return true;
        }
    }
}
