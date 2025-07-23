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

using Rock.AI.Agent;

namespace Rock.Lava
{
    internal static partial class LavaFilters
    {
        /// <summary>
        /// Sets a custom AI context value for the specified key.
        /// </summary>
        /// <param name="context">The Lava context.</param>
        /// <param name="input">The text to be added as the context.</param>
        /// <param name="key">The unique key to store the context under for later retrieval.</param>
        /// <param name="expireDateTime">An optional date and time that the context will expire, if not set it will default to 1 hour from now.</param>
        /// <param name="isInternal">An optional value indicating if the context is internal and should not be sent to the language model.</param>
        /// <returns>A new <see cref="Rock.Model.EntitySet"/> object.</returns>
        public static void SetAiContextValue( ILavaRenderContext context, object input, string key, object expireDateTime = null, object isInternal = null )
        {
            var internalValue = false;
            var expirationOffset = TimeSpan.FromHours( 1 );
            var agentContext = context.GetMergeField( "AgentContext" ) as AgentRequestContext
                ?? throw new InvalidOperationException( "The AgentContext merge field is not available in the current context." );

            if ( key.IsNullOrWhiteSpace() )
            {
                throw new ArgumentException( "The key parameter cannot be null or empty.", nameof( key ) );
            }

            if ( expireDateTime != null )
            {
                if ( expireDateTime is DateTimeOffset dto )
                {
                    expirationOffset = dto - RockDateTime.Now.ToRockDateTimeOffset();
                }
                else if ( expireDateTime is DateTime dt )
                {
                    expirationOffset = dt - RockDateTime.Now;
                }
                else if ( DateTime.TryParse( expireDateTime.ToString(), out dt ) )
                {
                    expirationOffset = dt - RockDateTime.Now;
                }
            }

            if ( isInternal != null )
            {
                if ( isInternal is bool b )
                {
                    internalValue = b;
                }
                else
                {
                    internalValue = isInternal.ToString().AsBoolean();
                }
            }

            agentContext.ChatAgent
                .AddSessionContextAsync( key, input.ToStringSafe(), null, expirationOffset, internalValue )
                .GetAwaiter()
                .GetResult();

        }

        /// <summary>
        /// Retrieves the value associated with the specified key from the AI session context.
        /// </summary>
        /// <param name="context">The Lava context.</param>
        /// <param name="key">The key used to retrieve the value from the AI session context. Cannot be null or empty.</param>
        /// <returns>The value associated with the specified key from the AI session context. Returns an empty string if the key does not exist.</returns>
        public static string GetAiContextValue( ILavaRenderContext context, string key )
        {
            var agentContext = context.GetMergeField( "AgentContext" ) as AgentRequestContext
                ?? throw new InvalidOperationException( "The AgentContext merge field is not available in the current context." );

            if ( key.IsNullOrWhiteSpace() )
            {
                throw new ArgumentException( "The key parameter cannot be null or empty.", nameof( key ) );
            }

            return agentContext.ChatAgent.GetSessionContextContent( key ) ?? string.Empty;
        }
    }
}
