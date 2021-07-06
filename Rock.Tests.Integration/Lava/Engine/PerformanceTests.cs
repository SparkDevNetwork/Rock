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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Lava;
using System.Collections.Generic;
using Rock.Lava.Fluid;
using System.Diagnostics;

namespace Rock.Tests.Integration.Lava
{
    /// <summary>
    /// Tests to provide performance metrics for critical components of the Lava engine implementation.
    /// </summary>
    [TestClass]
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
            var engineOptions = new LavaEngineConfigurationOptions { CacheService = new MockTemplateCacheService() };

            var engine = LavaService.NewEngineInstance( LavaEngineTypeSpecifier.Fluid, engineOptions );

            var standardLavaDictionary = new Dictionary<string, object>();

            var formatString = new string( '0', totalIterationsPerSet.ToString().Length );

            for ( int i = 0; i < totalIterationsPerSet; i++ )
            {
                standardLavaDictionary.Add( i.ToString( formatString ), i );
            }

            var mergeFields = new LavaDataDictionary { { "Dictionary", standardLavaDictionary } };

            var totalTime = TestHelper.ExecuteAndGetElapsedTime( () =>
            {
                engine.ClearTemplateCache();

                for ( int repeatCount = 1; repeatCount <= totalSets; repeatCount++ )
                {
                    var elapsedTime = TestHelper.ExecuteAndGetElapsedTime( () =>
                    {
                        for ( int i = 0; i < totalIterationsPerSet; i++ )
                        {
                            var template = "{{ Dictionary['" + i.ToString( formatString ) + "']}}";

                            var options = new LavaTestRenderOptions { MergeFields = mergeFields };

                            var output = TestHelper.GetTemplateOutput( engine, template, mergeFields );

                            // Verify that the correct entry was retrieved.
                            Assert.AreEqual( i.ToString(), output );
                        }
                    } );

                    Debug.Print( $"Pass {repeatCount} Elapsed Time: {elapsedTime.TotalMilliseconds} " );
                }
            } );

            Debug.Print( $"Average Time: {totalTime.TotalMilliseconds / totalSets} " );
        }
    }
}
