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
using System.Text.RegularExpressions;
using DotLiquid;

namespace Rock.Lava.DotLiquid
{
    /// <summary>
    /// Represents an implementation of a Lava Block for the DotLiquid Templating Framework.
    /// </summary>
    /// <remarks>
    /// This class implements a Lava Block element using the DotLiquid.Block Type for processing by the DotLiquid framework.
    /// </remarks>
    internal class DotLiquidBlockProxy : Block, ILiquidFrameworkElementRenderer
    {
        #region Static methods

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

        private ILavaBlock _lavaBlock = null;

        #region DotLiquid Block Overrides

        /// <summary>
        /// Initialize the block to process a specific occurrence in a template.
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="markup"></param>
        /// <param name="tokens"></param>
        public override void Initialize( string tagName, string markup, List<string> tokens )
        {
            if ( !_factoryMethods.ContainsKey( tagName ) )
            {
                throw new Exception( "Block factory could not be found." );
            }

            var factoryMethod = _factoryMethods[tagName];

            _lavaBlock = factoryMethod( tagName );

            if ( _lavaBlock == null )
            {
                throw new Exception( "Block factory could not provide a compatible block instance." );
            }

            // Initialize the Lava block first, because it may be called during the DotLiquid.Block initialization process.
            var blockTokens = GetBlockTokens( tagName, tokens, includeClosingTag: true );

            _lavaBlock.OnInitialize( tagName, markup, blockTokens );

            // Initialize the DotLiquid block.
            base.Initialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Gets the set of tokens that comprise the current block.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        private List<string> GetBlockTokens( string tagName, List<string> tokens, bool includeClosingTag = false )
        {
            // Get the block markup. The list of tokens contains all of the lava from the start tag to
            // the end of the template. This will pull out just the internals of the block.
            // We must take into consideration nested tags of the same type and include them in the block content.
            var endTagFound = false;

            var startTag = $@"{{\%\s*{ tagName }\s*\%}}";
            var endTag = $@"{{\%\s*end{ tagName }\s*\%}}";

            var childTags = 0;

            var regExStart = new Regex( startTag );
            var regExEnd = new Regex( endTag );

            var blockTokens = new List<string>();

            foreach ( var token in tokens )
            {
                Match startTagMatch = regExStart.Match( token );
                if ( startTagMatch.Success )
                {
                    childTags++;
                }
                else
                {
                    Match endTagMatch = regExEnd.Match( token );

                    if ( endTagMatch.Success )
                    {
                        if ( childTags > 0 )
                        {
                            childTags--;
                        }
                        else
                        {
                            endTagFound = true;
                        }
                    }
                }

                if ( !endTagFound || includeClosingTag )
                {
                    blockTokens.Add( token );
                }

                if ( endTagFound )
                {
                    break;
                }
            }

            if ( !endTagFound )
            {
                throw new LavaException( $"{tagName} was never closed." );
            }

            return blockTokens;
        }

        /// <summary>
        /// Render a specific instance of the block in the provided context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        public override void Render( Context context, TextWriter result )
        {
            var lavaContext = new DotLiquidRenderContext( context );

            var block = _lavaBlock as ILiquidFrameworkElementRenderer;

            if ( block == null )
            {
                throw new Exception( "Block proxy cannot be rendered." );
            }

            // Call the renderer implemented by the wrapped Lava block.
            block.Render( this, lavaContext, result, null );
        }

        /// <summary>
        /// Called by the DotLiquid framework when required to parse the supplied tokens from the Lava document.
        /// </summary>
        /// <param name="tokens"></param>
        protected override void Parse( List<string> tokens )
        {
            // Tokens are consumed as they are processed, so create a copy of the list for post-processing.
            var blockTokens = new List<string>( tokens );

            base.Parse( tokens );

            if ( this.NodeList == null )
            {
                this.NodeList = new List<object>();
            }
        }

        #endregion

        #region ILiquidFrameworkRenderer implementation

        void ILiquidFrameworkElementRenderer.Render( ILiquidFrameworkElementRenderer baseRenderer, ILavaRenderContext context, TextWriter result, TextEncoder encoder )
        {
            // Call the default DotLiquid renderer.
            var dotLiquidContext = ( (DotLiquidRenderContext)context ).DotLiquidContext;

            base.Render( dotLiquidContext, result );
        }

        #endregion
    }
}
