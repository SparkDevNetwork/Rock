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
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.TestData
{
    /// <summary>
    /// A set of actions that can be run manually to configure the test environment.
    /// These actions are marked as tests to enable them to be executed manually through the Test Explorer,
    /// but they return an inconclusive result by default so as not to interfere with automated tests.
    /// </summary>
    [TestClass]
    public abstract class UtilityTestActionsBase
    {
        private static bool _utilityTestActionsEnabled = false;

        /// <summary>
        /// Initialize the Rock application environment for integration testing.
        /// </summary>
        /// <param name="context">The context.</param>
        [ClassInitialize( InheritanceBehavior.BeforeEachDerivedClass )]
        public static void Initialize( TestContext context )
        {
            _utilityTestActionsEnabled = context.Properties[TestConfigurationKeys.UtilityTestActionsEnabled]
                .ToStringOrDefault( string.Empty )
                .AsBoolean();
        }

        [TestInitialize()]
        public void TestInitialize()
        {
            // Test methods in this class should not be executed by default.
            // Return an inconclusive result if utility test actions are disabled.
            if ( !_utilityTestActionsEnabled )
            {
                Assert.Inconclusive( $"Utility Test Actions are disabled in this configuration. To enable these actions, set the configuration option [{TestConfigurationKeys.UtilityTestActionsEnabled}]=true." );
            }
        }
    }
}