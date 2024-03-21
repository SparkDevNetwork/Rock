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

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

using Rock.Lava.Fluid;
using Rock.Tests.Performance.Modules.Lava;
using Rock.Tests.Shared.Lava;
using Rock.Tests.UnitTests.Lava;

namespace Rock.Tests.Performance.BenchmarkRunners
{
    /// <summary>
    /// The main entry point for this utility application.
    /// </summary>
    class Program
    {
        static void Main( string[] args )
        {
            RunLavaTemplateParse();
            //RunPersonPickerBenchmarks( args );
        }

        private static void RunPersonPickerBenchmarks( string[] args )
        {
            IConfig config = null;

            if ( System.Diagnostics.Debugger.IsAttached )
            {
                // If in Debug mode, use the debug config.
                config = new DebugInProcessConfig();
                config.WithOption( ConfigOptions.StopOnFirstError, true );

                //var thisAssembly = typeof( Program ).Assembly;
                //var summaries = BenchmarkSwitcher.FromAssembly( thisAssembly )
                //    .Run( args, new DebugInProcessConfig() );
            }
            else
            {
                config = new ManualConfig();
                config.WithOption( ConfigOptions.StopOnFirstError, true );
            }

            var thisAssembly = typeof( Program ).Assembly;
            var summaries = BenchmarkSwitcher.FromAssembly( thisAssembly )
                .Run( args, config );

            //var summaries = BenchmarkRunner.Run<PersonPickerAddressSearchBenchmarks>( config, args );
        }

        private static void RunLavaPerformanceTests( string[] args )
        {
            string command;

            // Get the command-line args.
            if ( args != null && args.Length > 0 )
            {
                command = args[0].Trim().ToLower();
            }
            else
            {
                command = "lava-pt";
            }

            if ( command == "lava-pt" )
            {
                LavaPerformanceTestConsole.Execute( args );
            }
            else
            {
                throw new Exception( $"Unknown Command \"{command}\"." );
            }
        }

        private static void RunLavaTemplateParse()
        {
            LavaUnitTestHelper.Initialize( testRockLiquidEngine: false, testDotLiquidEngine: false, testFluidEngine: true );

            var tests = new CollectionFilterTests();
            tests.AddToArray_AddToStringCollection_AppendsNewItem();
        }

        private static void RunLavaTemplateParse( string[] args )
        {
            LavaIntegrationTestHelper.Initialize( testRockLiquidEngine:false, testDotLiquidEngine:false, testFluidEngine: true, loadShortcodes: false );

            var lavaHelper = LavaIntegrationTestHelper.CurrentInstance;

            lavaHelper.GetTemplateOutput( typeof( FluidEngine ), "" );
        }
    }
}