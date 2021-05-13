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
using System.Threading.Tasks;
using System.Reflection;
using Fluid;
using Fluid.Ast;
using Fluid.Parser;
using Fluid.Values;
using Parlot.Fluent;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// An implementation of the Lava Engine using the Fluid Templating Framework.
    /// </summary>
    public class FluidEngine : LavaEngineBase
    {
        #region Static methods

        private static TemplateOptions _templateOptions = null;
        private static readonly LavaFluidParser _parser = new LavaFluidParser();

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
            var fluidContext = new global::Fluid.TemplateContext( _templateOptions );

            //fluidContext.ParserFactory = _parserFactory;

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
            var templateOptions = GetTemplateOptions();

            // Re-register the basic Liquid filters implemented by Fluid using CamelCase rather than the default snakecase.
            HideSnakeCaseFilters( templateOptions );
            RegisterBaseFilters( templateOptions );

            // Set the default strategy for locating object properties to our custom implementation that adds
            // the ability to resolve properties of nested anonymous Types using Reflection.
            templateOptions.MemberAccessStrategy = new LavaObjectMemberAccessStrategy();

            // Register all Types that implement LavaDataDictionary interfaces as safe to render.
            RegisterSafeType( typeof( Rock.Lava.ILavaDataDictionary ) );
            RegisterSafeType( typeof( Rock.Lava.ILavaDataDictionarySource ) );

            // Set the file provider to resolve included file references.
            if ( options.FileSystem == null )
            {
                options.FileSystem = new LavaNullFileSystem();
            }

            templateOptions.FileProvider = new FluidFileSystem( options.FileSystem );
        }

        private TemplateOptions GetTemplateOptions()
        {
            if ( _templateOptions == null )
            {
                _templateOptions = new TemplateOptions();


            }

            return _templateOptions;
        }

        /// <summary>
        /// This method hides the snake-case filters that are registered
        /// by default. Rock uses CamelCase filter names and to ensure that
        /// a mistype doesn't cause it to work anyway we hide these.
        /// </summary>
        private void HideSnakeCaseFilters( TemplateOptions options )
        {
            options.Filters.AddFilter( "join", NoOp );
            options.Filters.AddFilter( "first", NoOp );
            options.Filters.AddFilter( "last", NoOp );
            options.Filters.AddFilter( "concat", NoOp );
            options.Filters.AddFilter( "map", NoOp );
            options.Filters.AddFilter( "reverse", NoOp );
            options.Filters.AddFilter( "size", NoOp );
            options.Filters.AddFilter( "sort", NoOp );
            options.Filters.AddFilter( "sort_natural", NoOp );
            options.Filters.AddFilter( "uniq", NoOp );
            options.Filters.AddFilter( "where", NoOp );

            options.Filters.AddFilter( "default", NoOp );
            options.Filters.AddFilter( "date", NoOp );
            options.Filters.AddFilter( "format_date", NoOp );
            options.Filters.AddFilter( "raw", NoOp );
            options.Filters.AddFilter( "compact", NoOp );
            options.Filters.AddFilter( "url_encode", NoOp );
            options.Filters.AddFilter( "url_decode", NoOp );
            options.Filters.AddFilter( "strip_html", NoOp );
            options.Filters.AddFilter( "escape", NoOp );
            options.Filters.AddFilter( "escape_once", NoOp );
            options.Filters.AddFilter( "handle", NoOp );
            options.Filters.AddFilter( "handleize", NoOp );

            options.Filters.AddFilter( "abs", NoOp );
            options.Filters.AddFilter( "at_least", NoOp );
            options.Filters.AddFilter( "at_most", NoOp );
            options.Filters.AddFilter( "ceil", NoOp );
            options.Filters.AddFilter( "divided_by", NoOp );
            options.Filters.AddFilter( "floor", NoOp );
            options.Filters.AddFilter( "minus", NoOp );
            options.Filters.AddFilter( "modulo", NoOp );
            options.Filters.AddFilter( "plus", NoOp );
            options.Filters.AddFilter( "round", NoOp );
            options.Filters.AddFilter( "times", NoOp );

            options.Filters.AddFilter( "append", NoOp );
            options.Filters.AddFilter( "capitalize", NoOp );
            options.Filters.AddFilter( "downcase", NoOp );
            options.Filters.AddFilter( "lstrip", NoOp );
            options.Filters.AddFilter( "rstrip", NoOp );
            options.Filters.AddFilter( "newline_to_br", NoOp );
            options.Filters.AddFilter( "prepend", NoOp );
            options.Filters.AddFilter( "removefirst", NoOp );
            options.Filters.AddFilter( "remove", NoOp );
            options.Filters.AddFilter( "replacefirst", NoOp );
            options.Filters.AddFilter( "replace", NoOp );
            options.Filters.AddFilter( "slice", NoOp );
            options.Filters.AddFilter( "split", NoOp );
            options.Filters.AddFilter( "strip", NoOp );
            options.Filters.AddFilter( "strip_newlines", NoOp );
            options.Filters.AddFilter( "truncate", NoOp );
            options.Filters.AddFilter( "truncatewords", NoOp );
            options.Filters.AddFilter( "upcase", NoOp );
        }

        /// <summary>
        /// Registers all the base Fluid filters with the proper CamelCase.
        /// </summary>
        private void RegisterBaseFilters( TemplateOptions options )
        {
            options.Filters.AddFilter( "Join", global::Fluid.Filters.ArrayFilters.Join );
            options.Filters.AddFilter( "First", global::Fluid.Filters.ArrayFilters.First );
            options.Filters.AddFilter( "Last", global::Fluid.Filters.ArrayFilters.Last );
            options.Filters.AddFilter( "Map", global::Fluid.Filters.ArrayFilters.Map );
            options.Filters.AddFilter( "Reverse", global::Fluid.Filters.ArrayFilters.Reverse );
            options.Filters.AddFilter( "Size", global::Fluid.Filters.ArrayFilters.Size );
            options.Filters.AddFilter( "Sort", global::Fluid.Filters.ArrayFilters.Sort );
            options.Filters.AddFilter( "Uniq", global::Fluid.Filters.ArrayFilters.Uniq );
            options.Filters.AddFilter( "Where", global::Fluid.Filters.ArrayFilters.Where );

            options.Filters.AddFilter( "Default", global::Fluid.Filters.MiscFilters.Default );
            options.Filters.AddFilter( "Date", global::Fluid.Filters.MiscFilters.Date );
            options.Filters.AddFilter( "UnescapeDataString", global::Fluid.Filters.MiscFilters.UrlDecode );
            options.Filters.AddFilter( "EscapeDataString", global::Fluid.Filters.MiscFilters.UrlEncode );
            options.Filters.AddFilter( "StripHtml", global::Fluid.Filters.MiscFilters.StripHtml );
            options.Filters.AddFilter( "Escape", global::Fluid.Filters.MiscFilters.Escape );

            options.Filters.AddFilter( "AtLeast", global::Fluid.Filters.NumberFilters.AtLeast );
            options.Filters.AddFilter( "AtMost", global::Fluid.Filters.NumberFilters.AtMost );
            options.Filters.AddFilter( "Ceiling", global::Fluid.Filters.NumberFilters.Ceil );
            options.Filters.AddFilter( "DividedBy", global::Fluid.Filters.NumberFilters.DividedBy );
            options.Filters.AddFilter( "Floor", global::Fluid.Filters.NumberFilters.Floor );
            options.Filters.AddFilter( "Minus", global::Fluid.Filters.NumberFilters.Minus );
            options.Filters.AddFilter( "Modulo", global::Fluid.Filters.NumberFilters.Modulo );
            options.Filters.AddFilter( "Plus", global::Fluid.Filters.NumberFilters.Plus );
            options.Filters.AddFilter( "Times", global::Fluid.Filters.NumberFilters.Times );

            options.Filters.AddFilter( "Append", global::Fluid.Filters.StringFilters.Append );
            options.Filters.AddFilter( "Capitalize", global::Fluid.Filters.StringFilters.Capitalize );
            options.Filters.AddFilter( "Downcase", global::Fluid.Filters.StringFilters.Downcase );
            options.Filters.AddFilter( "NewlineToBr", global::Fluid.Filters.StringFilters.NewLineToBr );
            options.Filters.AddFilter( "Prepend", global::Fluid.Filters.StringFilters.Prepend );
            options.Filters.AddFilter( "RemoveFirst", global::Fluid.Filters.StringFilters.RemoveFirst );
            options.Filters.AddFilter( "Remove", global::Fluid.Filters.StringFilters.Remove );
            options.Filters.AddFilter( "ReplaceFirst", global::Fluid.Filters.StringFilters.ReplaceFirst );
            options.Filters.AddFilter( "Replace", global::Fluid.Filters.StringFilters.Replace );
            options.Filters.AddFilter( "Slice", global::Fluid.Filters.StringFilters.Slice );
            options.Filters.AddFilter( "Split", global::Fluid.Filters.StringFilters.Split );
            options.Filters.AddFilter( "StripNewlines", global::Fluid.Filters.StringFilters.StripNewLines );
            options.Filters.AddFilter( "Truncate", global::Fluid.Filters.StringFilters.Truncate );
            options.Filters.AddFilter( "Truncatewords", global::Fluid.Filters.StringFilters.TruncateWords );
            options.Filters.AddFilter( "Upcase", global::Fluid.Filters.StringFilters.Upcase );
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

            var templateOptions = GetTemplateOptions();

            foreach ( var lavaFilterMethod in lavaFilterMethods )
            {
                if ( lavaFilterMethod.Name == lastFilterName )
                {
                    continue;
                }

                lastFilterName = lavaFilterMethod.Name;

                this.OnRegisterFilter( lavaFilterMethod, null );
            }
        }

        /// <summary>
        /// Registers a set of Liquid-style filter functions for use with the Fluid templating engine.
        /// The original filters are wrapped in a function with a Fluid-compatible signature so they can be called by Fluid.
        /// </summary>
        /// <param name="lavaFilterMethod">The MethodInfo object that describes the function.</param>
        /// <param name="filterName">The Lava name for the filter.</param>
        protected override void OnRegisterFilter( MethodInfo lavaFilterMethod, string filterName )
        {
            var lavaFilterMethodParameters = lavaFilterMethod.GetParameters();

            if ( lavaFilterMethodParameters.Length == 0 )
            {
                return;
            }

            var templateOptions = GetTemplateOptions();

            filterName = filterName ?? lavaFilterMethod.Name;

            // The first argument passed to the Lava filter is either the Lava Context or the template input.
            var hasContextParameter = lavaFilterMethodParameters[0].ParameterType == typeof( ILavaRenderContext );

            var firstParameterIndex = 1 + ( hasContextParameter ? 1 : 0 );

            // Define the Fluid-compatible filter function that will wrap the Lava filter method.
            ValueTask<FluidValue> fluidFilterFunction( FluidValue input, FilterArguments arguments, TemplateContext context )
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

                return FluidValue.Create( result, templateOptions );
            }

            templateOptions.Filters.AddFilter( filterName, fluidFilterFunction );
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
        private static ValueTask<FluidValue> NoOp( FluidValue input, FilterArguments arguments, TemplateContext context )
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
            var options = GetTemplateOptions();

            if ( allowedMembers != null
                 && allowedMembers.Any() )
            {
                options.MemberAccessStrategy.Register( type, allowedMembers.ToArray() );
            }
            else
            {
                options.MemberAccessStrategy.Register( type );
            }
        }

        /// <summary>
        /// Pre-parses a Lava template to ensure it is using Liquid-compliant syntax, and creates a new template object.
        /// </summary>
        /// <param name="lavaTemplate"></param>
        /// <param name=""></param>
        /// <returns></returns>
        private FluidTemplate CreateNewFluidTemplate( string lavaTemplate, out string liquidTemplate )
        {
            IEnumerable<string> errors;
            FluidTemplate template;

            liquidTemplate = ConvertToLiquid( lavaTemplate );

            var isValidTemplate = TryParse( liquidTemplate, out template, out errors );

            if ( !isValidTemplate )
            {
                throw new LavaException( "Create Lava Template failed.", errors );
            }

            return template;
        }


        private bool TryParse( string template, out FluidTemplate result, out IEnumerable<string> errors )
        {
            string error;
            IFluidTemplate fluidTemplate;

            var success = _parser.TryParse( template, out fluidTemplate, out error );

            errors = new List<string> { error };

            var fluidTemplateObject = (FluidTemplate)fluidTemplate;

            if ( success )
            {
                result = new FluidTemplate( new List<Statement>( fluidTemplateObject.Statements ) );
                return true;
            }
            else
            {
                result = new FluidTemplate( new List<Statement>() );
                return false;
            }
        }

        protected override bool OnTryRender( ILavaTemplate inputTemplate, LavaRenderParameters parameters, out string output, out List<Exception> errors )
        {
            var templateProxy = inputTemplate as FluidTemplateProxy;

            var fluidTemplate = templateProxy?.FluidTemplate;

            return TryRenderInternal( fluidTemplate, parameters, out output, out errors );
        }

        private bool TryRenderInternal( FluidTemplate template, LavaRenderParameters parameters, out string output, out List<Exception> errors )
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
            FluidLavaTagStatement.RegisterFactory( name, factoryMethod );

            _parser.RegisterLavaTag( name );
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
            FluidLavaBlockStatement.RegisterFactory( name, factoryMethod );

            _parser.RegisterLavaBlock( name );
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
