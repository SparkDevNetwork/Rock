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
    /// Provides base functionality for a Lava Tag element.
    /// </summary>
    public abstract class LavaTagBase : ILavaTag, ILiquidFrameworkElementRenderer
    {
        private string _sourceElementName = null;
        private string _attributesMarkup;

        /// <summary>
        /// The text that defines this element in the Lava source document.
        /// </summary>
        public string SourceText { get; set; }

        /// <summary>
        /// The raw markup for any additional Attributes contained in the element source tag.
        /// </summary>
        public string ElementAttributesMarkup
        {
            get
            {
                return _attributesMarkup;
            }
        }

        #region IRockLavaTag/IRockLavaElement implementation

        /// <summary>
        /// The name of the tag.
        /// </summary>
        /// <summary>
        /// The name of the block.
        /// </summary>
        public string SourceElementName
        {
            get
            {
                if ( _sourceElementName == null )
                {
                    // If the tag does not have an assigned name, use the Type name by convention -
                    // but remove the "...Tag" suffix if it exists.
                    var name = this.GetType().Name.ToLower();

                    if ( name.EndsWith( "tag" ) )
                    {
                        name = name.Substring( 0, name.Length - 3 );
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
        /// The name of the block as it appears in the source tag.
        /// </summary>
        public string InternalElementName
        {
            get
            {
                return this.SourceElementName;
            }
        }

        /// <summary>
        /// Determines if this tag is authorized in the specified Lava context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected bool IsAuthorized( ILavaRenderContext context )
        {
            return IsAuthorized( context, this.SourceElementName );
        }

        /// <summary>
        /// Determines if this tag is authorized in the specified Lava context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected bool IsAuthorized( ILavaRenderContext context, string commandName )
        {
            return LavaSecurityHelper.IsAuthorized( context, commandName );
        }

        public virtual void OnStartup( ILavaEngine engine )
        {
            //
        }

        public virtual void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _attributesMarkup = markup;
        }

        /// <summary>
        /// Parse a set of Lava tokens into a set of document nodes that can be processed by the underlying rendering framework.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="nodes"></param>
        public virtual void OnParsed( List<string> tokens )
        {
            return;
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
            // If this tag was previously called with a different base renderer, exit to prevent a circular reference.
            if ( _baseRenderer != null )
            {
                return;
            }

            _baseRenderer = baseRenderer;

            OnRender( context, result );
        }

        #endregion
    }
}
