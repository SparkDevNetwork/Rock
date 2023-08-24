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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Rock.AI.Classes.ChatCompletions;
using Rock.AI.Classes.TextCompletions;
using Rock.AI.Provider;
using Rock.Data;
using Rock.Media;
using Rock.Model;
using Rock.Web.UI;
using Rock.Workflow;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    /// Lava shortcode for displaying scripture links
    /// </summary>
    [LavaShortcodeMetadata(
        Name = "AI Completion (Experimental)",
        TagName = "aicompletion",
        Description = "Processes a completion using a AI provider. The APIs that are called by this shortcode can be slow. It's recommended that they not be used on public facing websites. They're better used in back-end or batch processes.",
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
            /// <summary>
            /// The model you would like to use to generate your response. The default model will be used if a value is not provided.
            /// </summary>
            public const string Model = "model";

            /// <summary>
            /// The level of randomness or creativity in the generated text. See documentation for your provider for valid values.
            /// </summary>
            public const string Temperature = "temperature";

            /// <summary>
            /// The completion type to use (text or chat).
            /// </summary>
            public const string Type = "type";
        }

        /// <summary>
        /// The parameter names that will be used in the <see cref="LavaShortcodeMetadataAttribute"/>.
        /// </summary>
        internal const string ParameterNamesMetadata = ParameterKeys.Model
            + "," + ParameterKeys.Temperature
            + "," + ParameterKeys.Type;

        /// <summary>
        /// The documentation for the shortcode that will be used in the <see cref="LavaShortcodeMetadataAttribute"/>.
        /// </summary>
        internal const string DocumentationMetadata = @"
<div class=""alert alert-warning"">This shortcode is experimental and should not be used in production. It is likely to change before it's final release in v17.</div>

<p>This shortcode allows you to interact with AI models to provide dynamic responses to various prompts you provide. These APIs can be
slow. It's recommended that they not be used on public facing websites. They're better used in back-end or batch processes.</p>

<pre>{[ aicompletion ]}give me three options for greeting ted decker{[ endaicompletion ]}</pre>

<p>Let's take a look at some of the parameters and options that are available so
so you can customize this to be exactly what you want.</p>

<ul>
    <li><strong>model</strong> (default model) - The model you would like to use to generate your response. The default model will be used if a value is not provided.</li>
    <li><strong>temperature</strong> (default) - The level of randomness or creativity in the generated text. See documentation for your provider for valid values.</li>
    <li><strong>type</strong> (chat) - The completion type to use. Valid values are text/chat.</li>
</ul>
";

        #endregion

        #region Properties

        /// <summary>
        /// Specifies the type of Liquid element for this shortcode.
        /// </summary>
        public override LavaShortcodeTypeSpecifier ElementType
        {
            get
            {
                return LavaShortcodeTypeSpecifier.Block;
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// The markup that was passed after the shortcode name and before the closing ]}.
        /// </summary>
        string _markup = string.Empty;

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
            var currentPerson = GetCurrentPerson( context );
            

            // Get the active AI provider
            var activeComponent = Rock.AI.Provider.AIProviderContainer.GetActiveComponent();
            if ( activeComponent != null )
            {
                var parms = ParseMarkup( _markup, context );

                // Call the appropriate completion type.
                if ( parms[ParameterKeys.Type] == "text" )
                {
                    ProcessTextCompletion( activeComponent, parms, context, result );
                }
                else
                {
                    ProcessChatCompletion( activeComponent, parms, context, result );
                }
            }
            else
            {
                result.WriteLine( "There is no active AI provider configured." );
            }
        }

        /// <summary>
        /// Processes a chat completion request.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="parms"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        private void ProcessChatCompletion( AIProviderComponent component, Dictionary<string,string> parms, ILavaRenderContext context, TextWriter result )
        {
            // Get contents of the prompt
            using ( TextWriter writer = new StringWriter() )
            {
                base.OnRender( context, writer );
                

                var chatCompletionsRequest = new ChatCompletionsRequest();
                chatCompletionsRequest.Messages.Add( new ChatCompletionsRequestMessage() { Role = Rock.Enums.AI.ChatMessageRole.User, Content = writer.ToString().Trim() } );

                // Configure settings
                if ( parms[ParameterKeys.Model].IsNotNullOrWhiteSpace() )
                {
                    chatCompletionsRequest.Model = parms[ParameterKeys.Model];
                }

                if ( parms[ParameterKeys.Temperature].AsInteger() != -1 )
                {
                    chatCompletionsRequest.Temperature = parms[ParameterKeys.Temperature].AsDouble();
                }

                // Make call
                var chatResponse = Task.Run( () => component.GetChatCompletions( chatCompletionsRequest ) ).Result;

                if ( chatResponse.IsSuccessful )
                {
                    foreach ( var choice in chatResponse.Choices )
                    {
                        result.WriteLine( $"{choice.Text}" );
                    }
                }
                else
                {
                    result.WriteLine( $"Error: {chatResponse.ErrorMessage}" );
                }
            }
        }

        /// <summary>
        /// Processes a text completion request.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="parms"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        private void ProcessTextCompletion( AIProviderComponent component, Dictionary<string, string> parms, ILavaRenderContext context, TextWriter result )
        {
            // Get contents of the prompt
            using ( TextWriter writer = new StringWriter() )
            {
                base.OnRender( context, writer );

                var textCompletionsRequest = new TextCompletionsRequest();
                textCompletionsRequest.Prompt = writer.ToString().Trim();

                // Configure settings
                if ( parms[ParameterKeys.Model].IsNotNullOrWhiteSpace() )
                {
                    textCompletionsRequest.Model = parms[ParameterKeys.Model];
                }

                if ( parms[ParameterKeys.Temperature].AsInteger() != -1 )
                {
                    textCompletionsRequest.Temperature = parms[ParameterKeys.Temperature].AsDouble();
                }

                // Make call
                var chatResponse = Task.Run( () => component.GetTextCompletions( textCompletionsRequest ) ).Result;

                if ( chatResponse.IsSuccessful )
                {
                    foreach ( var choice in chatResponse.Choices )
                    {
                        result.WriteLine( $"{choice.Text}" );
                    }
                }
                else
                {
                    result.WriteLine( $"Error: {chatResponse.ErrorMessage}" );
                }
            }
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static Person GetCurrentPerson( ILavaRenderContext context )
        {
            // First check for a person override value included in lava context
            var currentPerson = context.GetMergeField( "CurrentPerson", null ) as Person;

            if ( currentPerson == null )
            {
                var httpContext = HttpContext.Current;

                if ( context != null && httpContext.Items.Contains( "CurrentPerson" ) )
                {
                    currentPerson = httpContext.Items["CurrentPerson"] as Person;
                }
            }

            return currentPerson;
        }

        /// <summary>
        /// Parses the markup.
        /// </summary>
        /// <param name="markup">The markup.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private Dictionary<string, string> ParseMarkup( string markup, ILavaRenderContext context )
        {
            // first run lava across the inputted markup
            var internalMergeFields = context.GetMergeFields();

            var resolvedMarkup = markup.ResolveMergeFields( internalMergeFields );

            return ParseResolvedMarkup( resolvedMarkup );
        }

        /// <summary>
        /// Parses the resolved markup to get the passed parameters.
        /// </summary>
        /// <param name="resolvedMarkup">The resolved markup.</param>
        /// <returns>A dictionary of all the parameters and values.</returns>
        internal static Dictionary<string, string> ParseResolvedMarkup( string resolvedMarkup )
        {
            // Initialize default parameter values.
            var parms = new Dictionary<string, string>
            {
                { ParameterKeys.Model, "" },
                { ParameterKeys.Temperature, "-1" },
                { ParameterKeys.Type, "chat" }
            };

            // Parse each parameter name and value in the format of name:'value'
            var markupItems = Regex.Matches( resolvedMarkup, @"(\S*?:'[^']+')" )
                .Cast<Match>()
                .Select( m => m.Value )
                .ToList();

            foreach ( var item in markupItems )
            {
                var itemParts = item.ToString().Split( new char[] { ':' }, 2 );

                if ( itemParts.Length > 1 )
                {
                    parms.AddOrReplace( itemParts[0].Trim().ToLower(), itemParts[1].Trim().Substring( 1, itemParts[1].Length - 2 ) );
                }
            }

            return parms;
        }

        #endregion
    }
}
