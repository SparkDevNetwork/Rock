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
    /// Provides base functionality for a Lava shortcode element.
    /// </summary>
    public abstract class LavaShortcodeBase : ILavaShortcode, ILiquidFrameworkElementRenderer
    {
        private string _elementName = null;
        private string _internalName = null;

        /// <summary>
        /// The name of the element.
        /// </summary>
        public string SourceElementName
        {
            get
            {
                if ( _elementName == null )
                {
                    // If the shortcode does not have an assigned name, use the Type name by convention -
                    // but remove the "...Shortcode" suffix if it exists.
                    var name = this.GetType().Name.ToLower();

                    if ( name.EndsWith( "shortcode" ) )
                    {
                        name = name.Substring( 0, name.Length - 9 );
                    }

                    return name;
                }

                return _elementName;
            }

            set
            {
                if ( string.IsNullOrWhiteSpace( value ) )
                {
                    _elementName = null;
                }
                else
                {
                    _elementName = value.Trim().ToLower();
                }
            }
        }

        /// <summary>
        /// The text that defines this element in the Lava source document.
        /// </summary>
        public string SourceText { get; set; }

        /// <summary>
        /// The key used to identify the shortcode internally.
        /// This is an augmented version of the tag name to prevent collisions with standard block and tag element names.
        /// </summary>
        public string InternalElementName
        {
            get
            {
                if ( _internalName == null )
                {
                    return LavaUtilityHelper.GetLiquidElementNameFromShortcodeName( this.SourceElementName );
                }

                return _internalName;
            }

            set
            {
                _internalName = value;
            }
        }

        /// <summary>
        /// Determines if this block is authorized in the specified Lava context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected bool IsAuthorized( ILavaRenderContext context )
        {
            return LavaSecurityHelper.IsAuthorized( context, this.SourceElementName );
        }

        #region IRockLavaElement Implementation

        /// <summary>
        /// Override this method to provide custom initialization for the block.
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="markup"></param>
        /// <param name="tokens"></param>
        public virtual void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            //
        }

        /// <summary>
        /// Renders the specified context.
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
        /// Executed at Rock startup to perform one-time initialization tasks for this code element.
        /// </summary>
        public virtual void OnStartup( ILavaEngine engine )
        {
            //
        }

        #endregion

        protected virtual void AssertMissingDelimitation()
        {
            throw new Exception( string.Format( "BlockTagNotClosedException: {0}", this.SourceElementName ) );
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

        /// <summary>
        /// The type of document element used by this shortcode.
        /// </summary>
        public abstract LavaShortcodeTypeSpecifier ElementType { get; }

        #region ILiquidFrameworkRenderer implementation

        private ILiquidFrameworkElementRenderer _baseRenderer = null;

        /// <summary>
        /// Render this component using the Liquid templating engine.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <param name="result"></param>
        /// <param name="encoder"></param>
        void ILiquidFrameworkElementRenderer.Render( ILiquidFrameworkElementRenderer baseRenderer, ILavaRenderContext context, TextWriter result, TextEncoder encoder )
        {
            _baseRenderer = baseRenderer;

            // Forward this call through to the implementation provided by the shortcode component.
            OnRender( context, result );
        }

        #endregion
    }
}
