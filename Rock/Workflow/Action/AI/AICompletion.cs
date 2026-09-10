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
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Rock.AI;
using Rock.Attribute;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Executes the provided completion and returns the result.
    /// </summary>
    [ActionCategory( "AI" )]
    [Description( "Executes the provided completion and returns the result." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "AI Completion" )]

    #region Block Atttributes

    [WorkflowTextOrAttribute(
        textLabel: "Prompt",
        attributeLabel: "Attribute Value",
        Description = "The text to send to the AI provider.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.Prompt,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" } )]

    [WorkflowAttribute(
        "Output Attribute",
        Description = "The attribute to save the prompt output to.",
        IsRequired = true,
        Order = 3,
        Key = AttributeKey.OutputAttribute,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" } )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "9FFF1BC7-90FF-4589-BB92-786601FDD07A" )]
    public class AICompletion : ActionComponent
    {
        /// <summary>
        /// Keys for the attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string Prompt = "Prompt";
            public const string OutputAttribute = "OutputAttribute";
        }

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

            var service = RockApp.Current.GetRequiredService<TextProcessingService>();

            if ( !service.IsAvailable )
            {
                errorMessages.Add( "AI completion services are not available." );

                return false;
            }

            var completionResult = ProcessChatCompletion( service, action, errorMessages );

            SetWorkflowAttributeValue( action, AttributeKey.OutputAttribute, completionResult );

            if ( errorMessages.Any() )
            {
                errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Processes a chat completion request.
        /// </summary>
        /// <param name="service">The service to use for the completion.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="errorMessages">The list to store error messages.</param>
        /// <returns>The result of the chat completion request.</returns>
        private string ProcessChatCompletion( TextProcessingService service, WorkflowAction action, List<string> errorMessages )
        {
            var chatCompletionRequest = new ChatCompletionRequest
            {
                Message = GetAttributeValue( action, AttributeKey.Prompt, true )
            };

            ChatCompletionResponse chatResponse = null;

            try
            {
                chatResponse = Task.Run( () => service.GetChatCompletionAsync( chatCompletionRequest ) ).Result;
            }
            catch ( Exception ex )
            {
                errorMessages.Add( ex.InnerException.ToString() ?? ex.ToString() );
                return string.Empty;
            }

            if ( chatResponse.IsSuccessful )
            {
                return chatResponse.GetText();
            }
            else
            {
                var output = $"Error: {chatResponse.ErrorMessage}";
                errorMessages.Add( output );

                return output;
            }
        }
    }
}