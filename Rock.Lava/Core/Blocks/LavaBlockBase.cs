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

namespace Rock.Lava
{
    /// <summary>
    /// Provides base functionality for implementation of a Rock Lava block.
    /// </summary>
    public abstract class LavaBlockBase : ILavaBlock, ILiquidFrameworkElementRenderer
    {
        private string _sourceElementName = null;

        /// <summary>
        /// The name of the block as it appears in the source tag.
        /// </summary>
        public string SourceElementName
        {
            get
            {
                if ( _sourceElementName == null )
                {
                    // If the block does not have an assigned name, use the Type name by convention -
                    // but remove the "...Block" suffix if it exists.
                    var name = this.GetType().Name.ToLower();

                    if ( name.EndsWith("block" ) )
                    {
                        name = name.Substring( 0, name.Length - 5 );
                    }

                    return name;
                }

                return _sourceElementName;
            }

            set
            {
                _sourceElementName = ( value == null ) ? null : value.Trim().ToLower();
            }
        }

        /// <summary>
        /// The name of the block as it is appears in the source tag.
        /// </summary>
        public string InternalElementName
        {
            get
            {
                return this.SourceElementName;
            }
        }

        /// <summary>
        /// Determines if this block is authorized in the specified Lava context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected bool IsAuthorized( ILavaRenderContext context )
        {
            return IsAuthorized( context, this.SourceElementName );
        }

        /// <summary>
        /// Determines if this block is authorized in the specified Lava context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected bool IsAuthorized( ILavaRenderContext context, string commandName )
        {
            return LavaSecurityHelper.IsAuthorized( context, commandName );
        }

        public bool IncludeClosingTokenInParseResult { get; set; } = true;

        #region IRockLavaElement Implementation

        /// <summary>
        /// Override this method to provide custom initialization for the block.
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="markup"></param>
        /// <param name="tokens"></param>
        public virtual void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            // Reset the base renderer.
            _baseRenderer = null;
        }

        /// <summary>
        /// Override this method to provide custom rendering for the block.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public virtual void OnRender( ILavaRenderContext context, TextWriter result )
        {
            // By default, call the underlying engine to render this element.
            if ( _baseRenderer != null )
            {
                _baseRenderer.Render( null, context, result, null );
            }
        }

        /// <summary>
        /// Override this method to perform custom actions after parsing of the block is completed.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="nodes"></param>
        public virtual void OnParsed( List<string> tokens )
        {
            //
        }

        /// <summary>
        /// Override this method to perform tasks when the block is first loaded at startup.
        /// </summary>
        public virtual void OnStartup( ILavaEngine engine )
        {
            //
        }

        protected virtual void AssertMissingDelimitation()
        {
            throw new LavaException( string.Format( "BlockTagNotClosedException: {0}", this.SourceElementName ) );
        }

        /// <summary>
        /// Gets the not authorized message.
        /// </summary>
        /// <value>
        /// The not authorized message.
        /// </value>
        public static string NotAuthorizedMessage
        {
            get
            {
                return "The Lava command '{0}' is not configured for this template.";
            }
        }

        #endregion

        #region ILiquidFrameworkRenderer implementation

        private ILiquidFrameworkElementRenderer _baseRenderer = null;

        /// <summary>
        /// Render this component using the Liquid templating engine.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <param name="proxy"></param>
        void ILiquidFrameworkElementRenderer.Render( ILiquidFrameworkElementRenderer baseRenderer, ILavaRenderContext context, TextWriter result, TextEncoder encoder )
        {
            // If the derived block is calling back into this method, render the block using the base implementation.
            if ( _baseRenderer != null )
            {
                baseRenderer.Render( null, context, result, encoder );

                return;
            }

            // Call the derived block to render the content.
            // Store a reference to the calling block to handle the case where there is a callback to use the default rendering process.
            try
            {
                _baseRenderer = baseRenderer;

                OnRender( context, result );
            }
            catch ( LavaInterruptException liex )
            {
                // Re-throw the LavaInterruptionException so the Lava engine can catch it.
                throw liex;
            }
            catch ( Exception ex )
            {                
                // Throw a user-friendly error message that is suitable for rendering to output.
                throw new Exception( $"(Block: {this.InternalElementName}) {ex.Message}", ex );
            }
            finally
            {
                _baseRenderer = null;
            }
        }

        #endregion
    }
}
