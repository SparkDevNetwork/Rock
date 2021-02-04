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
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Irony.Parsing;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// A wrapper for a Lava Block that enables it to be rendered by the Fluid templating engine.
    /// </summary>
    /// <remarks>
    /// This implementation uses a set of pre-registered factory methods to configure instances of the FluidBlockProxy
    /// dynamically at runtime.
    /// The FluidBlockProxy wraps a LavaBlock that is executed internally to render the element content.
    /// This approach allows the LavaBlock to be more easily adapted for use with alternative Liquid templating engines.
    /// </remarks>
    internal class FluidBlockProxy : ITagEx, ILiquidFrameworkElementRenderer
    {
        #region Static factory methods

        private static Dictionary<string, Func<string, IRockLavaBlock>> _factoryMethods = new Dictionary<string, Func<string, IRockLavaBlock>>( StringComparer.OrdinalIgnoreCase );
        private static object _factoryLock = new object();

        /// <summary>
        /// Register a factory that is capable of creating instances of the named block.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        public static void RegisterFactory( string name, Func<string, IRockLavaBlock> factoryMethod )
        {
            if ( string.IsNullOrWhiteSpace( name ) )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            lock ( _factoryLock )
            {
                _factoryMethods[name] = factoryMethod;
            }

        }

        #endregion

        #region ITagEx Implementation (extended from Fluid.ITag)

        /// <summary>
        /// Retrieve the syntax rules for the argument markup in this element tag.
        /// The syntax is defined by a set of Irony.NET grammar rules that Fluid uses to parse the tag.
        /// </summary>
        /// <param name="grammar"></param>
        /// <returns></returns>
        public BnfTerm GetSyntax( FluidGrammar grammar )
        {
            var blockEndTag = "%}";

            // Lava syntax uses whitespace as a separator between arguments, which Fluid/Irony does not support.
            // Therefore we return a syntax for this element that captures the entire argument list as a single token
            // and we will then parse the arguments list later in the process.
            var lavaArgumentList = new FreeTextLiteral( "lavaElementAttributesMarkup", FreeTextOptions.AllowEmpty | FreeTextOptions.AllowEof, blockEndTag );

            // Return a syntax that allows an empty arguments list, a comma-delimited list per the standard Fluid implementation,
            // or a freetext string to support the whitespace-delimited argument list used by Lava syntax.
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
            /* The Fluid framework parses the block into Liquid tokens using an adapted Irony.Net grammar.
             * Lava uses some syntax that is not recognized by Liquid, so we need to do some parsing of our own.
             * Also, some Lava blocks are designed to parse the raw source text of the block.
             * To maintain compatibility with that design, our parse process involves these steps:
             * 1. Extract the whitespace-delimited argument list from the open tag and parse it here.
             * 2. Extract text tokens from the source document, to allow Lava blocks to parse the raw source text.
             * The Lava parsing process is deferred to the Fluid rendering phase, because we need access to the template context that holds the template source document.
             *
             * Here, we simply add a Fluid statement to call a method to parse and render the block.
             * 
             * The source text will be passed to the Lava block during the rendering phase so it can be parsed and rendered at the same time.
            */
            var blockName = node.Term.Name;

            var factoryMethod = _factoryMethods[blockName];

            var lavaBlock = factoryMethod( blockName );

            // Get the markup for the block attributes.
            var argsNode = context.CurrentBlock.Tag.ChildNodes[0].ChildNodes[0];

            var blockAttributesMarkup = argsNode.FindTokenAndGetText().Trim();

            var statements = context.CurrentBlock.Statements;

            // Custom blocks expect to receive a set of tokens for the block that excludes the opening tag and includes the closing tag.
            // This behavior is preserved for compatibility with prior implementations.
            var tokens = new List<string>();

            tokens.Add( context.CurrentBlock.AdditionalData.InnerText );
            tokens.Add( context.CurrentBlock.AdditionalData.CloseTag );

            var renderBlockDelegate = new DelegateStatement( ( writer, encoder, ctx ) => WriteToAsync( writer, encoder, ctx, lavaBlock, blockName, blockAttributesMarkup, tokens, statements ) );

            return renderBlockDelegate;
        }

        Statement ITag.Parse( ParseTreeNode node, ParserContext context )
        {
            // This method is required to implement ITag.Parse, but it is not used.
            throw new NotImplementedException("Call Parse(ParseTreeNode, LavaFluidParserContext) instead.");
        }

        private ValueTask<Completion> WriteToAsync( TextWriter writer, TextEncoder encoder, TemplateContext context, IRockLavaBlock lavaBlock, string blockName, string blockAttributesMarkup, List<string> tokens, List<Statement> statements )
        {
            var lavaContext = new FluidLavaContext( context );

            var elementRenderer = lavaBlock as ILiquidFrameworkElementRenderer;

            if ( elementRenderer == null )
            {
                throw new Exception( "Block proxy cannot be rendered." );
            }

            // Initialize the block, then allow it to post-process the tokens parsed from the source template.
            lavaBlock.OnInitialize( blockName, blockAttributesMarkup, tokens );

            // Earlier implementations of Lava required that the document tokens passed to the block be consumed as they are processed.
            // This function is called each time the block is rendered, so we pass a copy of the token list to preserve compatibility
            // with custom blocks that implement this behavior.
            var parseTokens = tokens.ToList();

            lavaBlock.OnParsed( parseTokens );

            // Store the Fluid Statements required to render the block in the template context.
            lavaContext.SetInternalField( Constants.ContextKeys.SourceTemplateStatements, statements );

            // Execute the block rendering process.
            elementRenderer.Render( this, lavaContext, writer, encoder );

            return new ValueTask<Completion>( Completion.Normal );
        }

        #endregion

        private async ValueTask<Completion> WriteToDefaultAsync( TextWriter writer, TextEncoder encoder, TemplateContext context, List<Statement> statements )
        {
            Completion completion;

            if ( encoder == null )
            {
                encoder = global::Fluid.NullEncoder.Default;
            }

            foreach ( var statement in statements )
            {
                completion = await statement.WriteToAsync( writer, encoder, context );

                if ( completion != Completion.Normal )
                {
                    // Stop processing the block statements
                    return completion;
                }
            }

            return Completion.Normal;
        }

        #region ILiquidFrameworkRenderer implementation

        void ILiquidFrameworkElementRenderer.Render( ILiquidFrameworkElementRenderer baseRenderer, ILavaContext context, TextWriter writer, TextEncoder encoder )
        {
            var fluidContext = ( (FluidLavaContext)context ).FluidContext;

            var statements = context.GetInternalField( Constants.ContextKeys.SourceTemplateStatements ) as List<Statement>;

            var result = WriteToDefaultAsync( writer, encoder, fluidContext, statements );
        }

        #endregion
    }
}
