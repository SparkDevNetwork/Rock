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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Parser;
using Fluid.Values;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// An implementation of the Lava Engine using the Fluid Templating Framework.
    /// </summary>
    public class FluidEngine : LavaEngineBase
    {
        private TemplateOptions _templateOptions = null;
        private readonly LavaFluidParser _parser = new LavaFluidParser();

        private static readonly Guid _engineIdentifier = new Guid( "605445FE-6ECC-4E67-9A95-98F7173F7389" );

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
        /// The unique identifier of this engine.
        /// </summary>
        public override Guid EngineIdentifier
        {
            get
            {
                return _engineIdentifier;
            }
        }

        /// <summary>
        /// Create a new template context containing the specified merge fields.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <returns></returns>
        protected override ILavaRenderContext OnCreateRenderContext()
        {
            var options = GetTemplateOptions();
            var fluidContext = new global::Fluid.TemplateContext( options );
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
            ApplyEngineConfigurationOptions( options );
        }

        /// <summary>
        /// Register value converters for Types that are not natively handled by Fluid.
        /// </summary>
        private void RegisterValueConverters()
        {
            var templateOptions = GetTemplateOptions();

            /* [2021-06-24] DL
             * Value Converters can have a significant impact on rendering performance.
             * Wherever possible, a conversion function should:
             * 1. Process all conversions related to a specific Type domain, to avoid the need to execute similar code in multiple converters.
             * 2. Order from most to least frequently executed and least to most expensive execution time.
             * 3. Return a FluidValue as quickly as possible, to avoid executing subsequent value converters in the collection.
             */

            // TODO: Fluid executes value converters prior to internal converters.
            // Performance test this implementation with short-circuiting for basic types.
            templateOptions.ValueConverters.Add( ( value ) =>
            {
                // If the value is an Enum, render the value name.
                if ( value is Enum e )
                {
                    return new LavaEnumValue( e );
                }

                // This converter cannot process the value.
                return null;
            } );

            // Substitute the default Fluid DateTimeValue with an implementation that renders in the General DateTime format
            // rather than in UTC format.
            templateOptions.ValueConverters.Add( ( value ) =>
            {
                if ( value is DateTime dt )
                {
                    // Convert the DateTime to a DateTimeOffset value.
                    // MinValue/MaxValue are substituted directly because they are not timezone dependent.
                    if ( dt == DateTime.MinValue )
                    {
                        return new LavaDateTimeValue( DateTimeOffset.MinValue );
                    }
                    else if ( dt == DateTime.MaxValue )
                    {
                        return new LavaDateTimeValue( DateTimeOffset.MaxValue );
                    }
                    else
                    {
                        // Assume that the DateTime is expressed in Rock time.
                        return new LavaDateTimeValue( LavaDateTime.NewDateTimeOffset( dt.Ticks ) );
                    }
                }
                else if ( value is DateTimeOffset dto )
                {
                    return new LavaDateTimeValue( dto );
                }
                else if ( value is TimeSpan ts )
                {
                    return new LavaTimeSpanValue( ts );
                }

                // This converter cannot process the value.
                return null;
            } );

            // DBNull is required to process results from the Sql command.
            // If this type is not registered, Fluid throws some seemingly unrelated exceptions.
            templateOptions.ValueConverters.Add( ( value ) => value is System.DBNull ? FluidValue.Create( null, templateOptions ) : null );

            // Converter for a Dictionary with a non-string key type; wraps the dictionary in a proxy so it can be accessed
            // with a key that is not a string type.
            // If this converter is not registered, any attempt to access a non-standard dictionary by key returns a null value.
            templateOptions.ValueConverters.Add( ( value ) =>
            {
                // If the value is not a dictionary, this converter is not applicable.
                if ( !( value is IDictionary ) )
                {
                    return value;
                }

                // If the value is a standard Liquid-compatible dictionary,
                // return the appropriate Fluid wrapper to short-circuit further conversion attempts.
                if ( value is IDictionary<string, object> liquidDictionary )
                {
                    return new DictionaryValue( new ObjectDictionaryFluidIndexable<object>( liquidDictionary, templateOptions ) );
                }

                var valueType = value.GetType();

                // If the value is derived from LavaDataObject, no conversion is needed.
                if ( typeof( LavaDataObject ).IsAssignableFrom( valueType ) )
                {
                    return value;
                }

                // If this is a generic dictionary with a string key type, no conversion is needed.
                if ( valueType.IsGenericType
                     && valueType.GetGenericTypeDefinition() == typeof( Dictionary<,> ) )
                {
                    var keyType = valueType.GetGenericArguments()[0];

                    if ( keyType == typeof( string ) )
                    {
                        return value;
                    }
                }

                // Wrap the dictionary in a proxy so it can be accessed with a key that is not a string type.
                return new LavaDataObject( value );
            } );
        }

        /// <summary>
        /// Apply Lava Engine configuration options to the Fluid engine.  
        /// </summary>
        /// <param name="options"></param>
        private void ApplyEngineConfigurationOptions( LavaEngineConfigurationOptions options )
        {
            var templateOptions = GetTemplateOptions();

            // Set Fluid to use the local server culture for formatting dates, times and currencies.
            templateOptions.CultureInfo = options.Culture ?? CultureInfo.CurrentCulture;

            // Set Fluid to use the Rock Organization timezone.
            templateOptions.TimeZone = options.TimeZone ?? RockDateTime.OrgTimeZoneInfo;

            TemplateOptions.Default.TimeZone = templateOptions.TimeZone;

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

                // Re-register the basic Liquid filters implemented by Fluid using CamelCase rather than the default snakecase.
                HideSnakeCaseFilters( _templateOptions );
                RegisterBaseFilters( _templateOptions );

                // Set the default strategy for locating object properties to our custom implementation that adds
                // the ability to resolve properties of nested anonymous Types using Reflection.
                _templateOptions.MemberAccessStrategy = new LavaObjectMemberAccessStrategy();

                // Register value converters for Types that are not natively handled by Fluid.
                RegisterValueConverters();

                // Register all Types that implement LavaDataDictionary interfaces as safe to render.
                RegisterSafeType( typeof( Rock.Lava.ILavaDataDictionary ) );
                RegisterSafeType( typeof( Rock.Lava.ILavaDataDictionarySource ) );
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
            options.Filters.AddFilter( "Concat", global::Fluid.Filters.ArrayFilters.Concat );
            options.Filters.AddFilter( "Map", global::Fluid.Filters.ArrayFilters.Map );
            options.Filters.AddFilter( "Reverse", global::Fluid.Filters.ArrayFilters.Reverse );
            options.Filters.AddFilter( "Size", global::Fluid.Filters.ArrayFilters.Size );
            options.Filters.AddFilter( "Sort", global::Fluid.Filters.ArrayFilters.Sort );
            options.Filters.AddFilter( "SortNatural", global::Fluid.Filters.ArrayFilters.SortNatural );
            options.Filters.AddFilter( "Uniq", global::Fluid.Filters.ArrayFilters.Uniq );
            options.Filters.AddFilter( "Where", global::Fluid.Filters.ArrayFilters.Where );

            options.Filters.AddFilter( "Default", global::Fluid.Filters.MiscFilters.Default );
            options.Filters.AddFilter( "Date", global::Fluid.Filters.MiscFilters.Date );
            options.Filters.AddFilter( "Compact", global::Fluid.Filters.MiscFilters.Compact );
            options.Filters.AddFilter( "UnescapeDataString", global::Fluid.Filters.MiscFilters.UrlDecode );
            options.Filters.AddFilter( "EscapeDataString", global::Fluid.Filters.MiscFilters.UrlEncode );
            options.Filters.AddFilter( "EscapeOnce", global::Fluid.Filters.MiscFilters.EscapeOnce );
            options.Filters.AddFilter( "StripHtml", global::Fluid.Filters.MiscFilters.StripHtml );
            options.Filters.AddFilter( "Escape", global::Fluid.Filters.MiscFilters.Escape );

            options.Filters.AddFilter( "Abs", global::Fluid.Filters.NumberFilters.Abs );
            options.Filters.AddFilter( "AtLeast", global::Fluid.Filters.NumberFilters.AtLeast );
            options.Filters.AddFilter( "AtMost", global::Fluid.Filters.NumberFilters.AtMost );
            options.Filters.AddFilter( "Ceiling", global::Fluid.Filters.NumberFilters.Ceil );
            options.Filters.AddFilter( "DividedBy", global::Fluid.Filters.NumberFilters.DividedBy );
            options.Filters.AddFilter( "Floor", global::Fluid.Filters.NumberFilters.Floor );
            options.Filters.AddFilter( "Minus", global::Fluid.Filters.NumberFilters.Minus );
            options.Filters.AddFilter( "Modulo", global::Fluid.Filters.NumberFilters.Modulo );
            options.Filters.AddFilter( "Plus", global::Fluid.Filters.NumberFilters.Plus );
            options.Filters.AddFilter( "Round", global::Fluid.Filters.NumberFilters.Round );
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
            options.Filters.AddFilter( "TruncateWords", global::Fluid.Filters.StringFilters.TruncateWords );
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

                try
                {
                    var result = lavaFilterMethod.Invoke( null, lavaFilterMethodArguments );

                    return FluidValue.Create( result, templateOptions );
                }
                catch ( TargetInvocationException ex )
                {
                    // Any exceptions thrown from the filter method are wrapped in a TargetInvocationException by the .NET framework.
                    // Rethrow the actual exception thrown by the filter, where possible.
                    throw ex.InnerException ?? ex;
                }
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
                    lavaArgument = ( int ) fluidFilterArgument.ToNumberValue();
                }
            }
            else if ( argumentType == typeof( double ) )
            {
                if ( fluidFilterArgument == null )
                {
                    lavaArgument = 0;
                }
                else
                {
                    lavaArgument = ( double ) fluidFilterArgument.ToNumberValue();
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
            else
            {
                // By default, attempt to pass the parameter value as an object;
                if ( fluidFilterArgument != null )
                {
                    // Get the object value, ensuring that any Fluid wrapper that has been applied is removed.
                    lavaArgument = fluidFilterArgument.ToRealObjectValue();
                }
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
        private FluidTemplate CreateNewFluidTemplate( string lavaTemplate )
        {
            FluidTemplate template;
            string error;
            IFluidTemplate fluidTemplate;

            var success = _parser.TryParse( lavaTemplate, out fluidTemplate, out error );

            var fluidTemplateObject = ( FluidTemplate ) fluidTemplate;

            if ( success )
            {
                template = new FluidTemplate( new List<Statement>( fluidTemplateObject.Statements ) );
            }
            else
            {
                throw new LavaParseException( this.EngineName, lavaTemplate, error );
            }

            return template;
        }

        protected override LavaRenderResult OnRenderTemplate( ILavaTemplate inputTemplate, LavaRenderParameters parameters )
        {
            var templateProxy = inputTemplate as FluidTemplateProxy;
            var template = templateProxy?.FluidTemplate;

            var templateContext = parameters.Context as FluidRenderContext;
            if ( templateContext == null )
            {
                throw new LavaException( "Invalid LavaContext parameter. This context type is not compatible with the Fluid templating engine." );
            }

            var result = new LavaRenderResult();
            var sb = new StringBuilder();

            // Set the render options for culture and timezone if they are specified.
            if ( parameters.Culture != null )
            {
                templateContext.FluidContext.Options.CultureInfo = parameters.Culture;
            }
            if ( parameters.TimeZone != null )
            {
                templateContext.FluidContext.Options.TimeZone = parameters.TimeZone;
            }

            // Set the render options for encoding.
            System.Text.Encodings.Web.TextEncoder encoder;
            if ( parameters.ShouldEncodeStringsAsXml )
            {
                encoder = System.Text.Encodings.Web.HtmlEncoder.Default;
            }
            else
            {
                encoder = NullEncoder.Default;
            }

            using ( var writer = new StringWriter( sb ) )
            {
                try
                {
                    template.Render( templateContext.FluidContext, encoder, writer );

                    writer.Flush();
                    result.Text = sb.ToString();

                }
                catch ( LavaInterruptException )
                {
                    // The render was terminated intentionally, so return the current buffer content.
                    writer.Flush();
                    result.Text = sb.ToString();
                }
            }

            return result;
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

            name = name.Trim();

            base.RegisterTag( name, factoryMethod );

            // Some Lava elements, such as shortcodes, are defined dynamically at runtime.
            // Therefore, we register the tag as a factory that can produce the requested element on demand.
            FluidLavaTagStatement.RegisterFactory( name, factoryMethod );

            if ( name.EndsWith( "_" ) )
            {
                _parser.RegisterLavaTag( name, LavaTagFormatSpecifier.LavaShortcode );
            }
            else
            {
                _parser.RegisterLavaTag( name, LavaTagFormatSpecifier.LiquidTag );
            }
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

            name = name.Trim();

            base.RegisterBlock( name, factoryMethod );

            // Some Lava elements, such as shortcodes, are defined dynamically at runtime.
            // To implement this behaviour, register the tag as a factory that can create the requested element on demand.
            FluidLavaBlockStatement.RegisterFactory( name, factoryMethod );

            if ( name.EndsWith( "_" ) )
            {
                _parser.RegisterLavaBlock( name, LavaTagFormatSpecifier.LavaShortcode );
            }
            else
            {
                _parser.RegisterLavaBlock( name, LavaTagFormatSpecifier.LiquidTag );
            }
        }

        /// <summary>
        /// Process a template and return the list of valid tokens identified by the parser.
        /// </summary>
        /// <param name="lavaTemplate"></param>
        /// <returns></returns>
        public List<string> TokenizeTemplate( string lavaTemplate )
        {
            return LavaFluidParser.ParseToTokens( lavaTemplate );
        }

        /// <summary>
        /// Process a template and return the list of statements identified by the parser.
        /// </summary>
        /// <param name="lavaTemplate"></param>
        /// <returns></returns>
        public List<string> ParseTemplateToStatements( string lavaTemplate )
        {
            return LavaFluidParser.ParseToStatements( lavaTemplate );
        }

        protected override ILavaTemplate OnParseTemplate( string lavaTemplate )
        {
            var fluidTemplate = CreateNewFluidTemplate( lavaTemplate );

            var newTemplate = new FluidTemplateProxy( fluidTemplate, lavaTemplate );

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
