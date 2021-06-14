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
using Parlot;

namespace Rock.Lava.Fluid
{
    public class FluidLavaTagStatement : Statement, ILiquidFrameworkElementRenderer
    {
        #region Static factory methods

        private static Dictionary<string, Func<string, ILavaTag>> _factoryMethods = new Dictionary<string, Func<string, ILavaTag>>( StringComparer.OrdinalIgnoreCase );
        private static object _factoryLock = new object();

        /// <summary>
        /// Register a factory that is capable of creating instances of the named tag.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        public static void RegisterFactory( string name, Func<string, ILavaTag> factoryMethod )
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
        private readonly string _tagName;

        internal FluidLavaTagStatement( string tagName, in TextSpan attributesMarkup )
        {
            _tagName = tagName;

            _attributesMarkup = attributesMarkup.ToString() ?? string.Empty;
        }

        #endregion

        public override ValueTask<Completion> WriteToAsync( TextWriter writer, TextEncoder encoder, TemplateContext context )
        {
            var lavaContext = new FluidRenderContext( context );

            // Create an instance of the tag.
            var factoryMethod = _factoryMethods[_tagName];

            var lavaTag = factoryMethod( _tagName );

            var elementRenderer = lavaTag as ILiquidFrameworkElementRenderer;

            if ( elementRenderer == null )
            {
                throw new Exception( "Block proxy cannot be rendered." );
            }

            // Initialize the tag with the attributes markup and a list of empty tokens to represent the child content.
            lavaTag.OnInitialize( _tagName, _attributesMarkup, new List<string>() );

            // Execute the block rendering process.
            elementRenderer.Render( this, lavaContext, writer, encoder );

            return new ValueTask<Completion>( Completion.Normal );
        }

        private async ValueTask<Completion> WriteToDefaultAsync( TextWriter writer, TextEncoder encoder, TemplateContext context, List<Statement> statements )
        {
            Completion completion;

            if ( encoder == null )
            {
                encoder = global::Fluid.NullEncoder.Default;
            }

            // Execute each of the statements in the block.
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

        void ILiquidFrameworkElementRenderer.Render( ILiquidFrameworkElementRenderer baseRenderer, ILavaRenderContext context, TextWriter writer, TextEncoder encoder )
        {
            var fluidContext = ( (FluidRenderContext)context ).FluidContext;

            var statements = context.GetInternalField( Constants.ContextKeys.SourceTemplateStatements ) as List<Statement>;

            if ( statements == null )
            {
                statements = new List<Statement>();
            }

            var result = WriteToDefaultAsync( writer, encoder, fluidContext, statements );
        }

        #endregion
    }
}