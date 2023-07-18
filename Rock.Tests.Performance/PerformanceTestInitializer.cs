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
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Integration;

namespace Rock.Tests.Performance
{
    [TestClass]
    public sealed class PerformanceTestInitializer
    {
        /// <summary>
        /// This will run before any tests in this assembly are run.
        /// </summary>
        /// <param name="context">The context.</param>
        [AssemblyInitialize]
        public static void AssemblyInitialize( TestContext context )
        {
            // Copy the configuration settings to the TestContext so they can be accessed by the integration tests project initializer.
            AddTestContextSettingsFromConfigurationFile( context );

            IntegrationTestInitializer.Initialize( context );
        }

        public static void AddTestContextSettingsFromConfigurationFile( TestContext context )
        {
            // This project is not a Test Project type, so it does not load configuration from a .runsettings file.
            // Copy the configuration settings to the TestContext so they can be accessed by the integration tests project initializer.
            foreach ( var key in ConfigurationManager.AppSettings.AllKeys )
            {
                context.Properties[key] = ConfigurationManager.AppSettings[key];
            }

        }

    }
}
