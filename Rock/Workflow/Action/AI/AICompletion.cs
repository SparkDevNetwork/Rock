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

using Rock.AI.Classes.ChatCompletions;
using Rock.Attribute;
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
        description: "The text to send to the AI provider.",
        required: false,
        order: 0,
        key: AttributeKey.Prompt,
        fieldTypeClassNames: new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" } )]

    [AIProviderField(
        name: "Provider",
        description: "The AI Provider to use for the completion. Leave blank to use the default provider.",
        required: false,
        order: 1,
        key: AttributeKey.Provider )]

    [DecimalField(
        name: "Temperature",
        description: "The level of randomness or creativity in the generated text. See documentation for your provider for valid values.",
        required: false,
        order: 2,
        key: AttributeKey.Temperature )]

    [WorkflowAttribute(
        name: "Output Attribute",
        description: "The attribute to save the prompt output to.",
        required: true,
        order: 3,
        key: AttributeKey.OutputAttribute,
        fieldTypeClassNames: new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" } )]

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
            public const string Provider = "Provider";
            public const string Temperature = "Temperature";
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

            var providerIdentifier = GetAttributeValue( action, AttributeKey.Provider, true );
            var provider = GetProvider( providerIdentifier, rockContext );

            if ( provider == null )
            {
                if ( providerIdentifier.IsNotNullOrWhiteSpace() )
                {
                    errorMessages.Add( $"The specified AI provider is not available. [provider=\"{providerIdentifier}\"]" );
                }
                else
                {
                    errorMessages.Add( "There is no active AI provider configured." );
                }

                return false;
            }

            var completionResult = ProcessChatCompletion( provider, action, errorMessages );

            SetWorkflowAttributeValue( action, AttributeKey.OutputAttribute, completionResult );

            if ( errorMessages.Any() )
            {
                errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the AI provider based on the provided identifier.
        /// </summary>
        /// <param name="providerComponentId">The identifier of the provider component.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The AI provider if found; otherwise, null.</returns>
        private AIProvider GetProvider( string providerComponentId, RockContext rockContext )
        {
            AIProvider provider;

            // Get the active AI provider
            var providerService = new AIProviderService( rockContext );

            if ( providerComponentId.IsNullOrWhiteSpace() )
            {
                // Use the first active provider.
                provider = providerService.GetActiveProvider();
            }
            else
            {
                // Get the provider by Guid or ID...
                provider = providerService.Get( providerComponentId, allowIntegerIdentifier: true );

                if ( provider == null )
                {
                    // ...or try to match by name.
                    provider = providerService.Queryable().FirstOrDefault( p => p.Name.Equals( providerComponentId, StringComparison.OrdinalIgnoreCase ) );
                }
            }

            return provider;
        }

        /// <summary>
        /// Processes a chat completion request.
        /// </summary>
        /// <param name="provider">The AI provider to use for the completion.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="errorMessages">The list to store error messages.</param>
        /// <returns>The result of the chat completion request.</returns>
        private string ProcessChatCompletion( AIProvider provider, WorkflowAction action, List<string> errorMessages )
        {
            var chatCompletionsRequest = new ChatCompletionsRequest();
            chatCompletionsRequest.Messages.Add( new ChatCompletionsRequestMessage() { Role = Rock.Enums.AI.ChatMessageRole.User, Content = GetAttributeValue( action, AttributeKey.Prompt, true ) } );

            if ( GetAttributeValue( action, AttributeKey.Temperature ).AsInteger() != -1 )
            {
                chatCompletionsRequest.Temperature = GetAttributeValue( action, AttributeKey.Temperature, true ).AsDouble();
            }

            var component = provider.GetAIComponent();
            ChatCompletionsResponse chatResponse = null;
            var output = string.Empty;

            try
            {
                chatResponse = Task.Run( () => component.GetChatCompletions( provider, chatCompletionsRequest ) ).Result;
            }
            catch ( Exception ex )
            {
                errorMessages.Add( ex.InnerException.ToString() ?? ex.ToString() );
                return output;
            }

            if ( chatResponse.IsSuccessful )
            {
                output = string.Join( Environment.NewLine, chatResponse.Choices.Select( c => c.Text ) );
            }
            else
            {
                output = $"Error: {chatResponse.ErrorMessage}";
                errorMessages.Add( output );
            }

            return output;
        }
    }
}