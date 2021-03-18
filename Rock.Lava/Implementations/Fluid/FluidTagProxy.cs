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
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Irony.Parsing;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// A wrapper for a Lava Tag that enables it to be rendered by the Fluid templating engine.
    /// </summary>
    /// <remarks>
    /// This implementation allows a set of factory methods to be registered, and subsequently used to 
    /// generate instances of Fluid Tag elements dynamically at runtime.
    /// The FluidTagProxy wraps a LavaTag that is executed internally to render the element content.
    /// This approach allows the LavaTag to be more easily adapted for use with alternative Liquid templating engines.
    /// </remarks>
    internal class FluidTagProxy : ITagEx, ILiquidFrameworkElementRenderer
    {
        #region Static factory methods

        private static Dictionary<string, Func<string, ILavaTag>> _factoryMethods = new Dictionary<string, Func<string, ILavaTag>>( StringComparer.OrdinalIgnoreCase );

        public static void RegisterFactory( string name, Func<string, ILavaTag> factoryMethod )
        {
            if ( string.IsNullOrWhiteSpace( name ) )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            _factoryMethods[name] = factoryMethod;
        }

        #endregion

        private ILavaTag _lavaTag = null;


        #region Fluid.ITagEx Implementation

        /// <summary>
        /// Retrieve the syntax rules for the argument markup in this element tag.
        /// The syntax is defined by a set of Irony.NET grammar rules that Fluid uses to parse the tag.
        /// </summary>
        /// <param name="grammar"></param>
        /// <returns></returns>
        public BnfTerm GetSyntax( FluidGrammar grammar )
        {
            // Lava syntax uses whitespace as a separator between arguments, which Fluid/Irony does not support.
            // Therefore we return a syntax for this element that captures the entire argument list as a single token
            // and we will then parse the arguments list ourselves.
            var lavaArgumentList = new FreeTextLiteral( "lavaElementAttributesMarkup", FreeTextOptions.AllowEmpty | FreeTextOptions.AllowEof, "%}" );

            // Return a syntax that allows an empty arguments list, a comma-delimited list per the standard Fluid implementation,
            // or a whitespace-delimited list to support Lava syntax.
            return grammar.Empty | grammar.FilterArguments.Rule | lavaArgumentList;
        }

        /// <summary>
        /// Parses an element from a Lava document for the Fluid framework.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Statement Parse( ParseTreeNode node, LavaFluidParserContext context )
        {
            // Get the tag instance.
            var tagName = node.Term.Name;

            var factoryMethod = _factoryMethods[tagName];

            _lavaTag = factoryMethod( tagName );

            // Get the markup for the tag attributes.
            var attributesMarkup = node.FindTokenAndGetText().Trim();

            // When this element is rendered, write the content to the output stream.
            return new DelegateStatement( ( writer, encoder, ctx ) => WriteToAsync( writer, encoder, ctx, _lavaTag, tagName, attributesMarkup ) );
        }

        Statement ITag.Parse( ParseTreeNode node, ParserContext context )
        {
            // This method is required to implement ITag.Parse, but it is not used.
            throw new NotImplementedException( "Call Parse(ParseTreeNode, LavaFluidParserContext) instead." );
        }

        public ValueTask<Completion> WriteToAsync( TextWriter writer, TextEncoder encoder, TemplateContext context, ILavaTag lavaTag, string tagName, string tagAttributesMarkup )
        {
            var lavaContext = new FluidRenderContext( context );

            var elementRenderer = _lavaTag as ILiquidFrameworkElementRenderer;

            if ( elementRenderer == null )
            {
                throw new Exception( "Tag proxy cannot be rendered." );
            }

            // Initialize the tag, and execute post-processing for the parsing phase.
            // This is to ensure consistency with block element processing, even though the tag does not have any additional tokens.
            var tokens = new List<string>();

            lavaTag.OnInitialize( tagName, tagAttributesMarkup, new List<string>() );

            lavaTag.OnParsed( tokens );

            // Execute the tag rendering process.
            elementRenderer.Render( this, lavaContext, writer, encoder );

            return new ValueTask<Completion>( Completion.Normal );
        }

        #endregion

        #region ILiquidFrameworkRenderer implementation

        void ILiquidFrameworkElementRenderer.Render( ILiquidFrameworkElementRenderer baseRenderer, ILavaRenderContext context, TextWriter result, TextEncoder encoder )
        {
            // By default, rendering a custom tag does not produce any output.
        }

        #endregion
    }
}
