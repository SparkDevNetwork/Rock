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
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava.Engine
{
    /// <summary>
    /// Tests for the configuration options applied when a Lava engine is created.
    /// </summary>
    /// <remarks>
    /// The template cache tests stay in Rock.Tests.Integration, because
    /// WebsiteLavaTemplateCacheService and LavaShortcodeService both reach the
    /// database.
    /// </remarks>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class LavaConfigurationTests
    {
        [TestMethod]
        public void Configuration_SetDefaultEntityCommandExecute_IsEnabledForNewDefaultContext()
        {
            var engineOptions = new LavaEngineConfigurationOptions
            {
                DefaultEnabledCommands = new List<string> { "Execute" }
            };

            var engine = LavaTestEngineFactory.CreateFluidEngine( engineOptions );

            var context = engine.NewRenderContext();

            var enabledCommands = context.GetEnabledCommands();

            Assert.Contains( "Execute", enabledCommands );
        }
    }
}
