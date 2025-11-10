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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Rock.AI.Agent;
using Rock.AI.Agent.Classes.Common;

namespace Rock.Lava
{
    internal static partial class LavaFilters
    {
        /// <summary>
        /// Creates a new result for a Lava based AI Agent Tool. This can be
        /// used to provide complex results to the agent.
        /// </summary>
        /// <param name="context">The current Lava execution context.</param>
        /// <param name="input">The input that describes the state of the result. Must be either 'Success' or 'Error'.</param>
        /// <param name="payloadOrMessage">The payload or message to include in the result.</param>
        /// <param name="key">An optional key for the history content.</param>
        /// <returns>The <see cref="RockToolResult"/> instance.</returns>
        public static object AgentToolResult( ILavaRenderContext context, object input, object payloadOrMessage = null, string key = null )
        {
            if ( !( context.GetInternalField( "ProxyFunction", null ) is Dictionary<string, object> proxyFunctionResponse ) )
            {
                throw new LavaToolException( "The 'AgentToolResult' filter can only be used within the context of an AI Agent Tool." );
            }

            var inputString = input.ToStringSafe();
            RockToolResult toolResult;

            if ( inputString.Equals( "Success", StringComparison.OrdinalIgnoreCase ) )
            {
                if ( payloadOrMessage != null )
                {
                    toolResult = RockToolResult.Success( payloadOrMessage );
                }
                else
                {
                    toolResult = RockToolResult.Success();
                }
            }
            else if ( inputString.Equals( "Error", StringComparison.OrdinalIgnoreCase ) )
            {
                if ( payloadOrMessage == null )
                {
                    toolResult = RockToolResult.Error( "An error occurred." );
                }
                else if ( payloadOrMessage is IEnumerable<string> errorStrings )
                {
                    toolResult = RockToolResult.Error( errorStrings );
                }
                else
                {
                    toolResult = RockToolResult.Error( payloadOrMessage.ToStringSafe() );
                }
            }
            else
            {
                toolResult = CreateAgentToolTemplateError( "The AgentToolResult filter must be passed either 'Success' or 'Error'." );

                throw new LavaToolException( toolResult );
            }

            if ( key.IsNotNullOrWhiteSpace() )
            {
                toolResult.WithHistoryKey( key );
            }

            proxyFunctionResponse["ToolResult"] = toolResult;

            return toolResult;
        }

        /// <summary>
        /// Adds instructions that will be returned to the AI agent.
        /// </summary>
        /// <param name="context">The current Lava execution context.</param>
        /// <param name="input">The input parameter for the filter. This must be the <see cref="RockToolResult"/> instance.</param>
        /// <param name="instructions">The instructions to add to the result.</param>
        /// <returns>The <see cref="RockToolResult"/> object.</returns>
        public static object AgentToolInstructions( ILavaRenderContext context, object input, string instructions )
        {
            if ( !TryGetRockToolResult( context, input, out var rockToolResult ) )
            {
                throw new LavaToolException( rockToolResult );
            }

            return rockToolResult.WithInstructions( instructions );
        }

        /// <summary>
        /// Adds or replaces the existing history content on a that will be
        /// returned to the AI agent.
        /// </summary>
        /// <param name="context">The current Lava execution context.</param>
        /// <param name="input">The input parameter for the filter. This must be the <see cref="RockToolResult"/> instance.</param>
        /// <param name="content">The content to use for the history that will be available to later chat messages.</param>
        /// <param name="key">An optional key for the history content.</param>
        /// <returns>The <see cref="RockToolResult"/> object.</returns>
        public static object AgentToolHistoryContent( ILavaRenderContext context, object input, object content, string key = null )
        {
            if ( !TryGetRockToolResult( context, input, out var rockToolResult ) )
            {
                throw new LavaToolException( rockToolResult );
            }

            return rockToolResult.WithHistoryContent( content, key ?? string.Empty );
        }

        /// <summary>
        /// Adds metadata that will be returned to the AI agent.
        /// </summary>
        /// <param name="context">The current Lava execution context.</param>
        /// <param name="input">The input parameter for the filter. This must be the <see cref="RockToolResult"/> instance.</param>
        /// <param name="keyOrDictionary">Either a string key name or a dictionary of keys and values.</param>
        /// <param name="value">If <paramref name="keyOrDictionary"/> is a string, this is the value to set for the specified key.</param>
        /// <returns>The <see cref="RockToolResult"/> object.</returns>
        public static object AgentToolMetadata( ILavaRenderContext context, object input, object keyOrDictionary, object value = null )
        {
            if ( !TryGetRockToolResult( context, input, out var rockToolResult ) )
            {
                throw new LavaToolException( rockToolResult );
            }

            if ( keyOrDictionary is IDictionary dictionary )
            {
                foreach ( var key in dictionary.Keys )
                {
                    rockToolResult = rockToolResult.WithMetadata( key.ToStringSafe(), dictionary[key].ToStringSafe() );
                }
            }
            else if ( keyOrDictionary is string key )
            {
                rockToolResult = rockToolResult.WithMetadata( key, value.ToStringSafe() );
            }
            else
            {
                rockToolResult = CreateAgentToolTemplateError( "The AgentToolMetadata filter must be passed either a dictionary or a key and value." );

                throw new LavaToolException( rockToolResult );
            }

            return rockToolResult;
        }

        /// <summary>
        /// Adds or replaces the reference route that will be returned to the
        /// AI agent.
        /// </summary>
        /// <param name="context">The current Lava execution context.</param>
        /// <param name="input">The input parameter for the filter. This must be the <see cref="RockToolResult"/> instance.</param>
        /// <param name="text">The display text to show for the reference link, such as 'View Results'.</param>
        /// <param name="route">The route to the resource, such as '/person/123'.</param>
        /// <param name="secured">If false, the route will only be included if the current person has access to view the page. Defaults to true.</param>
        /// <returns>The <see cref="RockToolResult"/> object.</returns>
        public static object AgentToolReferenceRoute( ILavaRenderContext context, object input, string text, string route, object secured = null )
        {
            if ( !TryGetRockToolResult( context, input, out var rockToolResult ) )
            {
                throw new LavaToolException( rockToolResult );
            }

            if ( !( context.GetMergeField( "AgentContext" ) is AgentRequestContext agentContext ) )
            {
                throw new LavaToolException( CreateAgentToolTemplateError( "The AgentToolReferenceRoute filter can only be used within the context of an AI Agent Tool." ) );
            }

            return rockToolResult.WithReferenceRoute( agentContext.RockRequestContext, text, route, secured.ToStringSafe().AsBoolean()  );
        }

        /// <summary>
        /// Excludes history content from the result that will be returned to
        /// the AI agent.
        /// </summary>
        /// <param name="context">The current Lava execution context.</param>
        /// <param name="input">The input parameter for the filter. This must be the <see cref="RockToolResult"/> instance.</param>
        /// <returns>The <see cref="RockToolResult"/> object.</returns>
        public static object AgentToolNoHistory( ILavaRenderContext context, object input )
        {
            if ( !TryGetRockToolResult( context, input, out var rockToolResult ) )
            {
                throw new LavaToolException( rockToolResult );
            }

            return rockToolResult.WithoutHistoryContent();
        }

        #region Support Methods

        /// <summary>
        /// Creates a standardized error result for issues with the Lava Tool Template.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <returns>An instance of <see cref="RockToolResult"/> that contains the error message.</returns>
        private static RockToolResult CreateAgentToolTemplateError( string message )
        {
            return RockToolResult.Error( $"Invalid Lava Tool Template. {message}" )
                .WithInstructions( "An internal error has occurred. The error message should be displayed so the user can diagnose the problem." );
        }

        /// <summary>
        /// Attempts to extract a <see cref="RockToolResult"/> from the current
        /// Lava context and input object. This will produce an error result if
        /// either the context or input are invalid.
        /// </summary>
        /// <param name="context">The current lava execution context.</param>
        /// <param name="input">The input parameter to the filter.</param>
        /// <param name="rockToolResult">On return contains the <see cref="RockToolResult"/> object.</param>
        /// <param name="filterName">The name of the filter.</param>
        /// <returns><c>true</c> if the <paramref name="rockToolResult"/> contains the current result; <c>false</c> if it contains the error that should be returned.</returns>
        private static bool TryGetRockToolResult( ILavaRenderContext context, object input, out RockToolResult rockToolResult, [CallerMemberName] string filterName = null )
        {
            if ( !( context.GetInternalField( "ProxyFunction", null ) is Dictionary<string, object> ) )
            {
                throw new LavaToolException( "The 'AgentToolResult' filter can only be used within the context of an AI Agent Tool." );
            }

            if ( input is RockToolResult result )
            {
                rockToolResult = result;
                return true;
            }
            else
            {
                rockToolResult = CreateAgentToolTemplateError( $"The wrong object type was passed to the ${filterName} filter." );
                return false;
            }
        }

        #endregion
    }
}
