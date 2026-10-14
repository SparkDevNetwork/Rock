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

using Rock.Configuration;
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

using RockAppTestHelper = Rock.Tests.Shared.TestFramework.TestHelper;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava.Filters
{
    /// <summary>
    /// Tests for Lava filters that report on the Rock instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Both sides of a RockInstanceConfig assertion read from the same mocked
    /// hosting settings, so what these verify is the filter's mapping from a
    /// setting name to the property behind it - that "ApplicationDirectory"
    /// resolves to VirtualRootPath rather than WebRootPath, for example. That
    /// mapping is the logic in the filter.
    /// </para>
    /// <para>
    /// The AppendFollowing, FilterFollowed, FilterUnfollowed and ZebraPhoto tests
    /// stay in Rock.Tests.Integration. They need Followings and a persisted
    /// dataset seeded for the whole class, and a person's photo as a binary file.
    /// The Debug filter test stays too: the filter dereferences the result of
    /// context.GetMergeFields() and throws against a context with no merge fields,
    /// so making it run here would mean reproducing the merge field set a live
    /// request supplies rather than testing the filter.
    /// </para>
    /// </remarks>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class MiscellaneousFilterTests
    {
        #region RockInstanceConfig

        [TestMethod]
        public void RockInstanceConfigFilter_MachineName_RendersExpectedValue()
        {
            using ( var app = RockAppTestHelper.CreateScopedRockApp() )
            {
                var expectedValue = RockApp.Current.HostingSettings.MachineName;

                AssertConfigValue( "MachineName", expectedValue );
            }
        }

        [TestMethod]
        public void RockInstanceConfigFilter_ApplicationDirectory_RendersExpectedValue()
        {
            using ( var app = RockAppTestHelper.CreateScopedRockApp() )
            {
                var expectedValue = RockApp.Current.HostingSettings.VirtualRootPath;

                AssertConfigValue( "ApplicationDirectory", expectedValue );
            }
        }

        [TestMethod]
        public void RockInstanceConfigFilter_PhysicalDirectory_RendersExpectedValue()
        {
            using ( var app = RockAppTestHelper.CreateScopedRockApp() )
            {
                var expectedValue = RockApp.Current.HostingSettings.WebRootPath;

                AssertConfigValue( "PhysicalDirectory", expectedValue );
            }
        }

        [TestMethod]
        public void RockInstanceConfigFilter_IsClustered_RendersExpectedValue()
        {
            using ( var app = RockAppTestHelper.CreateScopedRockApp() )
            {
                // No web farm setting is seeded, so this reports the default.
                var expectedValue = WebFarm.RockWebFarm.IsEnabled().ToTrueFalse();

                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ 'IsClustered' | RockInstanceConfig }}" );

                    // Lava renders a boolean in lower case; ToTrueFalse capitalizes it.
                    Assert.AreEqual( expectedValue, output, ignoreCase: true );
                } );
            }
        }

        [TestMethod]
        public void RockInstanceConfigFilter_SystemDateTime_RendersExpectedValue()
        {
            using ( var app = RockAppTestHelper.CreateScopedRockApp() )
            {
                var expectedValue = RockDateTime.SystemDateTime;

                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ 'SystemDateTime' | RockInstanceConfig | Date:'yyyy-MM-dd HH:mm:ss' }}" );

                    var actualDateTime = output.AsDateTime();

                    Assert.IsNotNull( actualDateTime, $"Invalid DateTime - Output = \"{output}\"" );
                    Assert.That.AreProximate( expectedValue, actualDateTime, new System.TimeSpan( 0, 0, 30 ) );
                } );
            }
        }

        [TestMethod]
        public void RockInstanceConfigFilter_LavaEngine_RendersExpectedValue()
        {
            using ( var app = RockAppTestHelper.CreateScopedRockApp() )
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ 'LavaEngine' | RockInstanceConfig }}" );

                    Assert.AreEqual( RockApp.Current.GetCurrentLavaEngineName(), output );
                } );
            }
        }

        [TestMethod]
        public void RockInstanceConfigFilter_InvalidParameterName_RendersErrorMessage()
        {
            using ( var app = RockAppTestHelper.CreateScopedRockApp() )
            {
                AssertConfigValue( "unknown_setting", "Configuration setting \"unknown_setting\" is not available." );
            }
        }

        #endregion RockInstanceConfig

        #region Support Methods

        /// <summary>
        /// Renders the RockInstanceConfig filter for the named setting and asserts
        /// the result.
        /// </summary>
        /// <param name="settingName">The configuration setting name passed to the filter.</param>
        /// <param name="expectedValue">The value the filter should return.</param>
        private void AssertConfigValue( string settingName, string expectedValue )
        {
            var input = "{{ '" + settingName + "' | RockInstanceConfig }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expectedValue, output );
            } );
        }

        #endregion Support Methods
    }
}
