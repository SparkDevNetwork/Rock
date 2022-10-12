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
using Parlot;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid.Ast;
using Fluid;
using System;
using System.Collections.Generic;
using Fluid.Parser;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// A Lava block element renderer for the Fluid framework.
    /// </summary>
    public class FluidLavaBlockStatement : Statement, ILiquidFrameworkElementRenderer
    {
        #region Static factory methods

        private static Dictionary<string, Func<string, ILavaBlock>> _factoryMethods = new Dictionary<string, Func<string, ILavaBlock>>( StringComparer.OrdinalIgnoreCase );
        private static object _factoryLock = new object();

        /// <summary>
        /// Register a factory that is capable of creating instances of the named block.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        public static void RegisterFactory( string name, Func<string, ILavaBlock> factoryMethod )
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

        #region Fluid Statement

        private readonly string _attributesMarkup;
        private readonly string _blockContent;
        private readonly string _tagName;
        private LavaTagFormatSpecifier _tagFormat;

        private readonly LavaFluidParser _parser;

        internal FluidLavaBlockStatement( LavaFluidParser parser, string tagName, LavaTagFormatSpecifier tagFormat, in TextSpan attributesMarkup, in TextSpan blockContent )
        {
            _parser = parser;
            _tagName = tagName;
            _tagFormat = tagFormat;

            _attributesMarkup = attributesMarkup.ToString() ?? string.Empty;

            _attributesMarkup = _attributesMarkup.Trim();

            _blockContent = blockContent.ToString() ?? string.Empty;
        }

        #endregion

        public override ValueTask<Completion> WriteToAsync( TextWriter writer, TextEncoder encoder, TemplateContext context )
        {
            var lavaContext = new FluidRenderContext( context );

            var registeredTagName = _tagName + ( _tagFormat == LavaTagFormatSpecifier.LavaShortcode ? "_" : string.Empty );

            ILavaBlock lavaBlock = null;

            if ( _factoryMethods.ContainsKey( registeredTagName ) )
            {
                var factoryMethod = _factoryMethods[registeredTagName];

                lavaBlock = factoryMethod( _tagName );
            }

            var elementRenderer = lavaBlock as ILiquidFrameworkElementRenderer;

            if ( elementRenderer == null )
            {
                throw new Exception( $"FluidLavaBlock factory failed. Could not create an instance of block \"${_tagName}\"." );
            }

            // Parse the block content into tokens.
            var tokens = LavaFluidParser.ParseToTokens( _blockContent );

            // Custom Lava blocks created for previous implementations of the Lava library expect a set of tokens that excludes the opening tag and includes the closing tag.
            // This behavior is preserved by default, but can be disabled explicitly to simplify parsing.
            var addEndToken = true;
            if ( lavaBlock is LavaBlockBase blockBase )
            {
                addEndToken = blockBase.IncludeClosingTokenInParseResult;
            }
            if ( addEndToken )
            { 
                if ( _tagFormat == LavaTagFormatSpecifier.LavaShortcode )
                {
                    tokens.Add( $"{{[ end{_tagName} ]}}" );
                }
                else
                {
                    tokens.Add( $"{{% end{_tagName} %}}" );
                }
            }

            // Initialize the block, then allow it to post-process the tokens parsed from the source template.
            lavaBlock.OnInitialize( _tagName, _attributesMarkup, tokens );

            // Render the block content.
            elementRenderer.Render( this, lavaContext, writer, encoder );

            return new ValueTask<Completion>( Completion.Normal );
        }

        #region ILiquidFrameworkRenderer implementation

        void ILiquidFrameworkElementRenderer.Render( ILiquidFrameworkElementRenderer baseRenderer, ILavaRenderContext context, TextWriter writer, TextEncoder encoder )
        {
            // The default render implementation for the block is to parse the block content into a series of Fluid statements and
            // write the output of those statements to the provided stream.
            var fluidContext = ( ( FluidRenderContext ) context ).FluidContext;

            // Parse the content of the block into a set of Fluid statements.
            var blockContext = new FluidParseContext( _blockContent );

            var parseResult = new ParseResult<List<Statement>>();

            _ = _parser.Grammar.Parse( blockContext, ref parseResult );

            // Execute each of the statements in the block.
            var statements = parseResult.Value;

            if ( encoder == null )
            {
                encoder = global::Fluid.NullEncoder.Default;
            }

            foreach ( var statement in statements )
            {
                var task = statement.WriteToAsync( writer, encoder, fluidContext ).AsTask();

                task.Wait();

                if ( task.Result != Completion.Normal )
                {
                    // Stop processing the block statements
                    return;
                }
            }
        }

        #endregion
    }
}