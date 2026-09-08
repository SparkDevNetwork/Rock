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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Rock.AI;
using Rock.AI.Classes.ChatCompletions;
using Rock.AI.Classes.TextCompletions;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    /// Lava shortcode for displaying AI Completions.
    /// </summary>
    [LavaShortcodeMetadata(
        Name = "AI Completion (Experimental)",
        TagName = "aicompletion",
        Description = "Generate text using an AI provider. Best suited for back-end or batch use, as responses may be slow.",
        Documentation = DocumentationMetadata,
        Parameters = ParameterNamesMetadata,
        Categories = SystemGuid.Category.LAVA_SHORTCODE_AI )]
    public class AICompletionShortcode : LavaShortcodeBase, ILavaBlock
    {
        #region Attribute Constants

        /// <summary>
        /// The parameter names that are used in the shortcode.
        /// </summary>
        internal static class ParameterKeys
        {
        }

        /// <summary>
        /// The parameter names that will be used in the <see cref="LavaShortcodeMetadataAttribute"/>.
        /// </summary>
        internal const string ParameterNamesMetadata = "";

        /// <summary>
        /// The documentation for the shortcode that will be used in the <see cref="LavaShortcodeMetadataAttribute"/>.
        /// </summary>
        internal const string DocumentationMetadata = @"
<div class=""alert alert-warning"">This shortcode is experimental and should not be used in production. It is likely to change before it's final release in v17.</div>

<p>This shortcode allows you to interact with AI models to provide dynamic responses to various prompts you provide. These APIs can be
slow. It's recommended that they not be used on public facing websites. They're better used in back-end or batch processes.</p>

<pre>{[ aicompletion ]}give me three options for greeting ted decker{[ endaicompletion ]}</pre>
";

        #endregion

        #region Properties

        /// <summary>
        /// Specifies the type of Liquid element for this shortcode.
        /// </summary>
        public override LavaShortcodeTypeSpecifier ElementType => LavaShortcodeTypeSpecifier.Block;

        #endregion

        #region Fields

        /// <summary>
        /// The markup that was passed after the shortcode name and before the closing ]}.
        /// </summary>
        private string _markup = string.Empty;

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _markup = markup;

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            // Get parameter values.
            var parms = new Dictionary<string, string>
            {
            };

            LavaHelper.ParseCommandMarkup( _markup, context, parms );

            var service = RockApp.Current.GetRequiredService<TextProcessingService>();

            if ( !service.IsAvailable )
            {
                throw new Exception( "The AI completions service is not available." );
            }

            ProcessChatCompletion( context, result );
        }

        /// <summary>
        /// Processes a chat completion request.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        private void ProcessChatCompletion( ILavaRenderContext context, TextWriter result )
        {
            var textCompletionsRequest = new ChatCompletionRequest();

            using ( var writer = new StringWriter() )
            {
                base.OnRender( context, writer );

                textCompletionsRequest.Message = writer.ToString().Trim();
            }

            var service = RockApp.Current.GetRequiredService<TextProcessingService>();
            var response = Task.Run( () => service.GetChatCompletionAsync( textCompletionsRequest ) ).Result;

            if ( response.IsSuccessful )
            {
                result.WriteLine( response.GetText() );
            }
            else
            {
                result.WriteLine( $"Error: {response.ErrorMessage}" );
            }
        }

        #endregion
    }
}
