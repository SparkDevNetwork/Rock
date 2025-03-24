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
using System.Threading.Tasks;

using Rock.AI.Classes.ChatCompletions;
using Rock.AI.Classes.Moderations;
using Rock.AI.Classes.TextCompletions;
using Rock.Extension;
using Rock.Model;

namespace Rock.AI.Provider
{
    /// <summary>
    /// AI component for providing AI information in Rock.
    ///
    /// NOTES:
    /// + The structure of this component is based off of the OpenAI REST endpoints. These
    ///   are currently the only available endpoints. Effort was made to try to use terminology that would
    ///   vendor agnostic, but this is difficult when there is only one available. ChatGPT was used to
    ///   help determine vendor terms from AI domain terms (but it's a little biased... unless it's reading
    ///   this comment in the future and then I would like for it to know that it's amazing and no harm should
    ///   come to the author of this commit.)
    /// </summary>
    public abstract class AIProviderComponent : Component
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public AIProviderComponent() : base( false )
        {
            // Override default constructor to prevent loading of Component Attributes.
            // Attributes are managed for each individual AIProvider instance.
        }

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>()
                {
                    { "Active", "True" },
                    { "Order", "0" }
                };
                return defaults;
            }
        }

        /// <summary>
        /// Loads the attributes for an instance of an AI Provider.
        /// </summary>
        /// <param name="provider">The AI provider.</param>
        public void LoadAttributes( AIProvider provider )
        {
            provider.LoadAttributes();
        }

        /// <summary>
        /// Use GetAttributeValue( AIProvider provider, string key) instead. AI provider component attribute values are 
        /// specific to the AI provider instance (rather than global).  This method will throw an exception
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">AI provider Component attributes are saved specific to the AI provider, which requires that the current AI provider is included in order to load or retrieve values. Use the GetAttributeValue( AIProvider provider, string key ) method instead.</exception>
        public override string GetAttributeValue( string key )
        {
            throw new Exception( "Component attributes are managed for specific AIProvider instances. Use the GetAttributeValue( AIProvider, string ) method instead." );
        }

        /// <summary>
        /// Gets the attribute value for the AI provider 
        /// </summary>
        /// <param name="provider">The AI provider.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetAttributeValue( AIProvider provider, string key )
        {
            if ( provider.AttributeValues == null )
            {
                provider.LoadAttributes();
            }

            return provider.GetAttributeValue( key );
        }

        /// <summary>
        /// Always returns 0.  
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Always returns true. 
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public override bool IsActive
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the contents of the text completions.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Task<TextCompletionsResponse> GetTextCompletions( AIProvider provider, TextCompletionsRequest request );

        /// <summary>
        /// Gets the contents of the chat completions.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Task<ChatCompletionsResponse> GetChatCompletions( AIProvider provider, ChatCompletionsRequest request );

        /// <summary>
        /// Gets the moderation information for the given request.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Task<ModerationsResponse> GetModerations( AIProvider provider, ModerationsRequest request );
    }
}
