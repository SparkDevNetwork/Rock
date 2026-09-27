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
using System.Diagnostics;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Lava;
using Rock.Tests.Lava.Shared;

namespace Rock.Tests.Integration.Core.Lava.Engine
{
    /// <summary>
    /// Tests to provide performance metrics for critical components of the Lava engine implementation.
    /// </summary>
    [TestClass]
    [Ignore( "Enable these tests only when evaluating engine performance." )]
    public class PerformanceTests : LavaIntegrationTestBase
    {
        /// <summary>
        /// Provide performance metrics for the custom Fluid ValueConverter function implemented for IDictionary Types.
        /// This is useful for testing the performance impact of changes to the value converter function.
        /// </summary>
        [TestMethod]
        public void FluidEngineValueConverterPerformance_DictionaryKeyResolution_OutputsPerformanceMeasures()
        {
            var totalSets = 10;
            var totalIterationsPerSet = 1000000;

            // Create a Fluid engine with template caching disabled.
            var engineOptions = new LavaEngineConfigurationOptions { CacheService = new NullTemplateCacheService() };

            var engine = LavaService.NewEngineInstance( typeof( FluidEngine ), engineOptions );

            var standardLavaDictionary = new Dictionary<string, object>();

            var formatString = new string( '0', totalIterationsPerSet.ToString().Length );

            for ( int i = 0; i < totalIterationsPerSet; i++ )
            {
                standardLavaDictionary.Add( i.ToString( formatString ), i );
            }

            var mergeFields = new LavaDataDictionary { { "Dictionary", standardLavaDictionary } };

            var options = new LavaRenderOptions { MergeFields = mergeFields };

            var totalTime = GetElapsedTime( () =>
            {
                engine.ClearTemplateCache();

                for ( int repeatCount = 1; repeatCount <= totalSets; repeatCount++ )
                {
                    var elapsedTime = GetElapsedTime( () =>
                    {
                        for ( int i = 0; i < totalIterationsPerSet; i++ )
                        {
                            var template = "{{ Dictionary['" + i.ToString( formatString ) + "']}}";

                            var output = LavaRenderTestHelper.Render( engine, template, options );

                            // Verify that the correct entry was retrieved.
                            Assert.AreEqual( i.ToString(), output );
                        }
                    } );

                    Debug.Print( $"Pass {repeatCount} Elapsed Time: {elapsedTime.TotalMilliseconds} " );
                }
            } );

            Debug.Print( $"Average Time: {totalTime.TotalMilliseconds / totalSets} " );
        }

        /// <summary>
        /// Verify that creating a second instance of the Fluid engine does not cause any changes to the configuration of the first instance.
        /// </summary>
        [TestMethod]
        public void LavaEngine_CreateSecondInstance_FirstInstanceConfigurationIsUnaffected()
        {
            const int mobilePhoneNumberTypeValueId = 12;

            var templateInput = @"
{{ CurrentPerson.NickName }}'s other contact numbers are: {{ CurrentPerson.PhoneNumbers | Where:'NumberTypeValueId', 12, 'notequal' | Select:'NumberFormatted' | Join:', ' }}.'
";

            templateInput.Replace( "<mobilePhoneId>", mobilePhoneNumberTypeValueId.ToString() );

            var expectedOutput = @"
Ted's other contact numbers are: (623) 555-3322,(623) 555-2444.'
".NormalizeLineEndings();

            var mergeFields = new Dictionary<string, object> { { "CurrentPerson", GetWhereFilterTestPersonTedDecker() } };

            var options = new LavaRenderOptions { MergeFields = mergeFields };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                // Create a second instance of the engine, and verify that the template is resolved identically.
                var secondEngine = LavaTestEngineFactory.CreateFluidEngine( new LavaEngineConfigurationOptions { FileSystem = new MockFileProvider(), CacheService = null } );

                var firstOutput = LavaRenderTestHelper.Render( engine, templateInput, options );

                Assert.AreEqual( expectedOutput, firstOutput );

                var secondOutput = LavaRenderTestHelper.Render( secondEngine, templateInput, options );

                Assert.AreEqual( expectedOutput, secondOutput );
            } );
        }

        /// <summary>
        /// Runs the action and returns how long it took.
        /// </summary>
        /// <param name="action">The action to time.</param>
        /// <returns>The elapsed time.</returns>
        private static TimeSpan GetElapsedTime( Action action )
        {
            var stopwatch = Stopwatch.StartNew();

            action();

            stopwatch.Stop();

            return stopwatch.Elapsed;
        }

        private Person GetWhereFilterTestPersonTedDecker()
        {
            var rockContext = RockApp.Current.CreateRockContext();

            var personTedDecker = new PersonService( rockContext ).Queryable()
                .FirstOrDefault( x => x.LastName == "Decker" && x.NickName == "Ted" );

            var phones = personTedDecker.PhoneNumbers;

            Assert.IsNotNull( personTedDecker, "Test person not found in current database." );

            return personTedDecker;
        }

    }
}
