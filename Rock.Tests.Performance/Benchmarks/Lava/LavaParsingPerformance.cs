using System.Collections.Generic;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using Rock.Lava;
using Rock.Lava.Fluid;

namespace Rock.Tests.Performance.Benchmarks.Security
{
    /// <summary>
    /// Performs some basic performance tests on the...
    /// class. TL;DR; It's fast.
    /// </summary>
    [MemoryDiagnoser( true )]
    [Attributes.OperationsPerSecondColumn]
    [GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
    [CategoriesColumn]
    public class LavaParsingPerformance
    {
        private FluidEngine fluidEngine = new FluidEngine();

        #region Test Data

        private readonly string _templateIfElseIf = @"
{% liquid

    assign NickName = 'Ted'
    assign lastName = 'Decker'

    if lastName == ''
        assign result = 'nope'
    else
        if NickName == 'Cindy'
            assign result = 'female'
        elseif NickName == 'Ted'
            assign result = 'male'
        else
            assign result = 'unknown'
        endif
    endif

    echo result
%}
";
        private readonly string _expectedOutputIfElseIf = "male";

        private readonly string _templateWithComments = @"
{% liquid

    //- Comment level one
    assign isTest = true
    if isTest
        
        //- Comment level two
        assign isTest = false
        
    endif
    echo isTest
%}
";
        private readonly string _expectedOutputWithComments = "false";
        #endregion

        [GlobalSetup]
        public void Setup()
        {
            var engineOptions = new LavaEngineConfigurationOptions
            {
                DefaultEnabledCommands = new List<string>()
            };

            fluidEngine.Initialize( engineOptions );
            fluidEngine.RegisterFilters( typeof( Rock.Lava.Filters.TemplateFilters ) );

            // Verify both templates are working as expected.
            var output = fluidEngine.ParseTemplate( _templateIfElseIf );
            if ( output.HasErrors != false )
            {
                throw new System.Exception( "Lava engine setup failed: the template has errors." );
            }

            var renderResult = fluidEngine.RenderTemplate( output.Template, new LavaRenderParameters() );
            if ( output == null || renderResult.Text.Trim() != _expectedOutputIfElseIf )
            {
                throw new System.Exception( $"Lava engine setup failed: unexpected _expectedOutputIfElseIf output (was {renderResult.Text.Trim()})" );
            }

            output = fluidEngine.ParseTemplate( _templateWithComments );
            renderResult = fluidEngine.RenderTemplate( output.Template, new LavaRenderParameters() );
            if ( output == null || renderResult.Text.Trim() != _expectedOutputWithComments )
            {
                throw new System.Exception( $"Lava engine setup failed: unexpected _expectedOutputWithComments output (was {renderResult.Text.Trim()})." );
            }
        }

        [Benchmark]
        [BenchmarkCategory( "Lava ElseIf" )]
        public LavaParseResult ParseLavaIfElseIfTemplate()
        {
            return fluidEngine.ParseTemplate( _templateIfElseIf );
        }

        [Benchmark]
        [BenchmarkCategory( "Lava Comments" )]
        public LavaParseResult ParseLavaTemplateWithComments()
        {
            return fluidEngine.ParseTemplate( _templateWithComments );
        }

    }
}
