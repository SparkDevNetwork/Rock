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
using System.Linq;
using Fluid;
using Fluid.Values;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// An implementation of the Lava Engine using the Fluid Templating Framework.
    /// </summary>
    public class FluidEngine : LavaEngineBase
    {
        #region Static methods

        private static LavaFluidParserFactory _parserFactory = new LavaFluidParserFactory();

        #endregion
        /// <summary>
        /// The descriptive name of the engine.
        /// </summary>
        public override string EngineName
        {
            get
            {
                return "Fluid";
            }
        }

        /// <summary>
        /// The type specifier for the framework.
        /// </summary>
        public override LavaEngineTypeSpecifier EngineType
        {
            get
            {
                return LavaEngineTypeSpecifier.Fluid;
            }
        }

        /// <summary>
        /// Create a new template context containing the specified merge fields.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <returns></returns>
        protected override ILavaRenderContext OnCreateRenderContext()
        {
            var fluidContext = new global::Fluid.TemplateContext();

            fluidContext.ParserFactory = _parserFactory;

            var context = new FluidRenderContext( fluidContext );

            return context;
        }

        /// <summary>
        /// Initializes the Lava engine.
        /// Doing this in startup will force the static Liquid class to get instantiated
        /// so that the standard filters are loaded prior to the custom RockFilter.
        /// This is to allow the custom 'Date' filter to replace the standard Date filter.
        /// </summary>
        public override void OnSetConfiguration( LavaEngineConfigurationOptions options )
        {
            // Re-register the basic Liquid filters implemented by Fluid using CamelCase rather than the default snakecase.
            HideSnakeCaseFilters();
            RegisterBaseFilters();

            // Set the default strategy for locating object properties to our custom implementation that adds
            // the ability to resolve properties of nested anonymous Types using Reflection.
            TemplateContext.GlobalMemberAccessStrategy = new LavaObjectMemberAccessStrategy();

            // Register all Types that implement LavaDataDictionary interfaces as safe to render.
            RegisterSafeType( typeof( Rock.Lava.ILavaDataDictionary ) );
            RegisterSafeType( typeof( Rock.Lava.ILavaDataDictionarySource ) );

            // Set the file provider to resolve included file references.
            if ( options.FileSystem == null )
            {
                options.FileSystem = new LavaNullFileSystem();
            }

            TemplateContext.GlobalFileProvider = new FluidFileSystem( options.FileSystem );
        }

        /// <summary>
        /// This method hides the snake-case filters that are registered
        /// by default. Rock uses CamelCase filter names and to ensure that
        /// a mistype doesn't cause it to work anyway we hide these.
        /// </summary>
        private void HideSnakeCaseFilters()
        {
            TemplateContext.GlobalFilters.AddFilter( "join", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "first", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "last", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "concat", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "map", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "reverse", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "size", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "sort", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "sort_natural", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "uniq", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "where", NoOp );

            TemplateContext.GlobalFilters.AddFilter( "default", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "date", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "format_date", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "raw", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "compact", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "url_encode", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "url_decode", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "strip_html", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "escape", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "escape_once", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "handle", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "handleize", NoOp );

            TemplateContext.GlobalFilters.AddFilter( "abs", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "at_least", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "at_most", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "ceil", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "divided_by", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "floor", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "minus", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "modulo", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "plus", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "round", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "times", NoOp );

            TemplateContext.GlobalFilters.AddFilter( "append", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "capitalize", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "downcase", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "lstrip", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "rstrip", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "newline_to_br", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "prepend", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "removefirst", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "remove", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "replacefirst", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "replace", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "slice", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "split", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "strip", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "strip_newlines", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "truncate", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "truncatewords", NoOp );
            TemplateContext.GlobalFilters.AddFilter( "upcase", NoOp );
        }

        /// <summary>
        /// Registers all the base Fluid filters with the proper CamelCase.
        /// </summary>
        private void RegisterBaseFilters()
        {
            TemplateContext.GlobalFilters.AddFilter( "Join", global::Fluid.Filters.ArrayFilters.Join );
            TemplateContext.GlobalFilters.AddFilter( "First", global::Fluid.Filters.ArrayFilters.First );
            TemplateContext.GlobalFilters.AddFilter( "Last", global::Fluid.Filters.ArrayFilters.Last );
            TemplateContext.GlobalFilters.AddAsyncFilter( "Map", global::Fluid.Filters.ArrayFilters.Map );
            TemplateContext.GlobalFilters.AddFilter( "Reverse", global::Fluid.Filters.ArrayFilters.Reverse );
            TemplateContext.GlobalFilters.AddAsyncFilter( "Size", global::Fluid.Filters.ArrayFilters.Size );
            TemplateContext.GlobalFilters.AddAsyncFilter( "Sort", global::Fluid.Filters.ArrayFilters.Sort );
            TemplateContext.GlobalFilters.AddFilter( "Uniq", global::Fluid.Filters.ArrayFilters.Uniq );
            TemplateContext.GlobalFilters.AddAsyncFilter( "Where", global::Fluid.Filters.ArrayFilters.Where );

            TemplateContext.GlobalFilters.AddFilter( "Default", global::Fluid.Filters.MiscFilters.Default );
            TemplateContext.GlobalFilters.AddFilter( "Date", global::Fluid.Filters.MiscFilters.Date );
            TemplateContext.GlobalFilters.AddFilter( "UnescapeDataString", global::Fluid.Filters.MiscFilters.UrlDecode );
            TemplateContext.GlobalFilters.AddFilter( "EscapeDataString", global::Fluid.Filters.MiscFilters.UrlEncode );
            TemplateContext.GlobalFilters.AddFilter( "StripHtml", global::Fluid.Filters.MiscFilters.StripHtml );
            TemplateContext.GlobalFilters.AddFilter( "Escape", global::Fluid.Filters.MiscFilters.Escape );

            TemplateContext.GlobalFilters.AddFilter( "AtLeast", global::Fluid.Filters.NumberFilters.AtLeast );
            TemplateContext.GlobalFilters.AddFilter( "AtMost", global::Fluid.Filters.NumberFilters.AtMost );
            TemplateContext.GlobalFilters.AddFilter( "Ceiling", global::Fluid.Filters.NumberFilters.Ceil );
            TemplateContext.GlobalFilters.AddFilter( "DividedBy", global::Fluid.Filters.NumberFilters.DividedBy );
            TemplateContext.GlobalFilters.AddFilter( "Floor", global::Fluid.Filters.NumberFilters.Floor );
            TemplateContext.GlobalFilters.AddFilter( "Minus", global::Fluid.Filters.NumberFilters.Minus );
            TemplateContext.GlobalFilters.AddFilter( "Modulo", global::Fluid.Filters.NumberFilters.Modulo );
            TemplateContext.GlobalFilters.AddFilter( "Plus", global::Fluid.Filters.NumberFilters.Plus );
            TemplateContext.GlobalFilters.AddFilter( "Times", global::Fluid.Filters.NumberFilters.Times );

            TemplateContext.GlobalFilters.AddFilter( "Append", global::Fluid.Filters.StringFilters.Append );
            TemplateContext.GlobalFilters.AddFilter( "Capitalize", global::Fluid.Filters.StringFilters.Capitalize );
            TemplateContext.GlobalFilters.AddFilter( "Downcase", global::Fluid.Filters.StringFilters.Downcase );
            TemplateContext.GlobalFilters.AddFilter( "NewlineToBr", global::Fluid.Filters.StringFilters.NewLineToBr );
            TemplateContext.GlobalFilters.AddFilter( "Prepend", global::Fluid.Filters.StringFilters.Prepend );
            TemplateContext.GlobalFilters.AddFilter( "RemoveFirst", global::Fluid.Filters.StringFilters.RemoveFirst );
            TemplateContext.GlobalFilters.AddFilter( "Remove", global::Fluid.Filters.StringFilters.Remove );
            TemplateContext.GlobalFilters.AddFilter( "ReplaceFirst", global::Fluid.Filters.StringFilters.ReplaceFirst );
            TemplateContext.GlobalFilters.AddFilter( "Replace", global::Fluid.Filters.StringFilters.Replace );
            TemplateContext.GlobalFilters.AddFilter( "Slice", global::Fluid.Filters.StringFilters.Slice );
            TemplateContext.GlobalFilters.AddFilter( "Split", global::Fluid.Filters.StringFilters.Split );
            TemplateContext.GlobalFilters.AddFilter( "StripNewlines", global::Fluid.Filters.StringFilters.StripNewLines );
            TemplateContext.GlobalFilters.AddFilter( "Truncate", global::Fluid.Filters.StringFilters.Truncate );
            TemplateContext.GlobalFilters.AddFilter( "Truncatewords", global::Fluid.Filters.StringFilters.TruncateWords );
            TemplateContext.GlobalFilters.AddFilter( "Upcase", global::Fluid.Filters.StringFilters.Upcase );
        }

        /// <summary>
        /// Registers a set of Liquid-style filter functions for use with the Fluid templating engine.
        /// The original filters are wrapped in a function with a Fluid-compatible signature so they can be called by Fluid.
        /// </summary>
        /// <param name="type">The type that contains the Liquid filter functions.</param>
        protected override void OnRegisterFilters( Type implementingType )
        {
            // Get the filter methods ordered by name and parameter count.
            // Fluid only allows one registered method for each filter name, so use the overload with the most parameters.
            // This addresses the vast majority of use cases, but we could modify our Fluid filter function wrapper to
            // distinguish different method signatures if necessary.
            var lavaFilterMethods = implementingType.GetMethods( System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public )
                .ToList()
                .OrderBy( x => x.Name )
                .ThenByDescending( x => x.GetParameters().Count() );

            string lastFilterName = null;

            foreach ( var lavaFilterMethod in lavaFilterMethods )
            {
                if ( lavaFilterMethod.Name == lastFilterName )
                {
                    continue;
                }

                lastFilterName = lavaFilterMethod.Name;

                var lavaFilterMethodParameters = lavaFilterMethod.GetParameters();

                if ( lavaFilterMethodParameters.Length == 0 )
                {
                    continue;
                }

                // The first argument passed to the Lava filter is either the Lava Context or the template input.
                var hasContextParameter = lavaFilterMethodParameters[0].ParameterType == typeof( ILavaRenderContext );

                var firstParameterIndex = 1 + ( hasContextParameter ? 1 : 0 );

                // Define the Fluid-compatible filter function that will wrap the Lava filter method.
                FluidValue fluidFilterFunction( FluidValue input, FilterArguments arguments, TemplateContext context )
                {
                    var lavaFilterMethodArguments = new object[lavaFilterMethodParameters.Length];

                    for ( int i = 0; i < lavaFilterMethodParameters.Length; i++ )
                    {
                        FluidValue fluidFilterArgument = null;

                        // Get the value for the argument.
                        if ( i == 0 )
                        {
                            // If this is the first parameter, it may be a LavaContext or the input template.
                            if ( hasContextParameter )
                            {
                                lavaFilterMethodArguments[0] = new FluidRenderContext( context );
                                continue;
                            }
                            else
                            {
                                fluidFilterArgument = input;
                            }
                        }
                        else if ( i == 1 && hasContextParameter )
                        {
                            // If this is the second parameter, it must be the input template if the first parameter is a LavaContext.
                            fluidFilterArgument = input;
                        }
                        else if ( arguments.Count > ( i - firstParameterIndex ) )
                        {
                            // This parameter is a filter argument.
                            fluidFilterArgument = arguments.At( i - firstParameterIndex );
                        }

                        if ( fluidFilterArgument == null && lavaFilterMethodParameters[i].IsOptional )
                        {
                            lavaFilterMethodArguments[i] = lavaFilterMethodParameters[i].DefaultValue;
                        }
                        else
                        {
                            lavaFilterMethodArguments[i] = GetLavaParameterArgumentFromFluidValue( fluidFilterArgument, lavaFilterMethodParameters[i].ParameterType );
                        }
                    }

                    var result = lavaFilterMethod.Invoke( null, lavaFilterMethodArguments );

                    return FluidValue.Create( result );
                }

                TemplateContext.GlobalFilters.AddFilter( lavaFilterMethod.Name, fluidFilterFunction );
            }
        }

        private static object GetLavaParameterArgumentFromFluidValue( FluidValue fluidFilterArgument, Type argumentType )
        {
            object lavaArgument = null;

            if ( argumentType == typeof( string ) )
            {
                if ( fluidFilterArgument != null )
                {
                    lavaArgument = fluidFilterArgument.ToStringValue();
                }
            }
            else if ( argumentType == typeof( int ) )
            {
                if ( fluidFilterArgument == null )
                {
                    lavaArgument = 0;
                }
                else
                {
                    lavaArgument = (int)fluidFilterArgument.ToNumberValue();
                }
            }
            else if ( argumentType == typeof( bool ) )
            {
                if ( fluidFilterArgument == null )
                {
                    lavaArgument = false;
                }
                else
                {
                    lavaArgument = fluidFilterArgument.ToBooleanValue();
                }
            }
            else if ( argumentType == typeof( object ) )
            {
                if ( fluidFilterArgument != null )
                {
                    // Get the object value, ensuring that any Fluid wrapper that has been applied is removed.
                    lavaArgument = fluidFilterArgument.ToRealObjectValue();
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException( argumentType.Name, $"Parameter type '{argumentType.Name}' is not supported for RockLiquid filters." );
            }

            return lavaArgument;
        }

        /// <summary>
        /// Performs a no-operation filter. Just return the input. This simulates using
        /// a filter that doesn't exist.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static FluidValue NoOp( FluidValue input, FilterArguments arguments, TemplateContext context )
        {
            return input;
        }

        /// <summary>
        /// Register a specific System.Type as available for referencing in a Lava template.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="allowedMembers"></param>
        public override void RegisterSafeType( Type type, IEnumerable<string> allowedMembers )
        {
            if ( allowedMembers != null
                 && allowedMembers.Any() )
            {
                TemplateContext.GlobalMemberAccessStrategy.Register( type, allowedMembers.ToArray() );
            }
            else
            {
                TemplateContext.GlobalMemberAccessStrategy.Register( type );
            }
        }

        /// <summary>
        /// Pre-parses a Lava template to ensure it is using Liquid-compliant syntax, and creates a new template object.
        /// </summary>
        /// <param name="lavaTemplate"></param>
        /// <param name=""></param>
        /// <returns></returns>
        private LavaFluidTemplate CreateNewFluidTemplate( string lavaTemplate, out string liquidTemplate )
        {
            IEnumerable<string> errors;
            LavaFluidTemplate template;

            liquidTemplate = ConvertToLiquid( lavaTemplate );

            var isValidTemplate = TryParse( liquidTemplate, out template, out errors );

            if ( !isValidTemplate )
            {
                throw new LavaException( "Create Lava Template failed.", errors );
            }

            return template;
        }

        private bool TryParse( string template, out LavaFluidTemplate result, out IEnumerable<string> errors )
        {
            // This is a replacement for the BaseFluidTemplate.TryParse() method, which allows us to use a modified parser.
            var parser = _parserFactory.CreateParser() as FluidParserEx;

            var elements = new List<FluidParsedTemplateElement>();

            parser.ElementParsed += ( object sender, FluidElementParseEventArgs e ) =>
            {
                elements.Add( new FluidParsedTemplateElement { ElementId = e.ElementId, Statement = e.Statement, Node = e.ElementText, StartIndex = e.StartIndex, EndIndex = e.EndIndex } );
            };

            List<global::Fluid.Ast.Statement> statements;

            var success = parser.TryParse( template, false, out statements, out errors );

            if ( success )
            {
                result = new LavaFluidTemplate
                {
                    Elements = elements,
                    Statements = statements
                };
                return true;
            }
            else
            {
                result = new LavaFluidTemplate();
                return false;
            }
        }

        protected override bool OnTryRender( ILavaTemplate inputTemplate, LavaRenderParameters parameters, out string output, out List<Exception> errors )
        {
            var templateProxy = inputTemplate as FluidTemplateProxy;

            var fluidTemplate = templateProxy?.FluidTemplate;

            return TryRenderInternal( fluidTemplate, parameters, out output, out errors );
        }

        private bool TryRenderInternal( LavaFluidTemplate template, LavaRenderParameters parameters, out string output, out List<Exception> errors )
        {
            var templateContext = parameters.Context as FluidRenderContext;

            if ( templateContext == null )
            {
                throw new LavaException( "Invalid LavaContext parameter. This context type is not compatible with the Fluid templating engine." );
            }

            /* The Fluid framework parses the input template into a set of executable statements that can be rendered.
             * To remain independent of a specific framework, custom Lava tags and blocks parse the original source template text to extract
             * the information necessary to render their output. For this reason, we need to store the source in the context so that it can be passed
             * to the Lava custom components when they are rendered.
             */
            templateContext.SetInternalField( Constants.ContextKeys.SourceTemplateElements, template.Elements );

            templateContext.FluidContext.ParserFactory = _parserFactory;
            try
            {

                output = template.Render( templateContext.FluidContext );
                errors = new List<Exception>();

                return true;
            }
            catch ( Exception ex )
            {
                ProcessException( ex, out output );

                errors = new List<Exception> { ex };

                return false;
            }

        }

        /// <summary>
        /// Register a Lava Tag element.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        public override void RegisterTag( string name, Func<string, ILavaTag> factoryMethod )
        {
            if ( name == null )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            base.RegisterTag( name, factoryMethod );

            // Some Lava elements, such as shortcodes, are defined dynamically at runtime.
            // Therefore, we register the tag as a factory that can produce the requested element on demand.
            var lavaTag = factoryMethod( name );

            var fluidTag = new FluidTagProxy();

            FluidTagProxy.RegisterFactory( name, factoryMethod );

            // Register the proxy for the specified tag name.
            var proxyInstance = new FluidTagProxy();

            _parserFactory.RegisterTag( name, proxyInstance );
        }

        /// <summary>
        /// Register a Lava Block element.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        public override void RegisterBlock( string name, Func<string, ILavaBlock> factoryMethod )
        {
            if ( name == null )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            base.RegisterBlock( name, factoryMethod );

            // Some Lava elements, such as shortcodes, are defined dynamically at runtime.
            // To implement this behaviour, register the tag as a factory that can create the requested element on demand.
            FluidBlockProxy.RegisterFactory( name, factoryMethod );

            var proxyInstance = new FluidBlockProxy();

            _parserFactory.RegisterBlock( name, proxyInstance );
        }

        protected override ILavaTemplate OnParseTemplate( string lavaTemplate )
        {
            string liquidTemplate;

            var fluidTemplate = this.CreateNewFluidTemplate( lavaTemplate, out liquidTemplate );

            var newTemplate = new FluidTemplateProxy( fluidTemplate );

            return newTemplate;
        }

        /// <summary>
        /// Compare two objects for equivalence according to the applicable Lava equality rules for the input object types.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>True if the two objects are considered equal.</returns>
        public override bool AreEqualValue( object left, object right )
        {
            if ( right == null )
            {
                if ( left == null )
                {
                    return true;
                }

                return false;
            }

            return left.Equals( right );
        }
    }
}
