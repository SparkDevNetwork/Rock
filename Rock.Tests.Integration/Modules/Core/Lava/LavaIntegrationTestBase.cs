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
using Rock.Tests.Shared.Lava;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Lava
{
    public abstract class LavaIntegrationTestBase : DatabaseTestsBase
    {
        public static LavaIntegrationTestHelper TestHelper
        {
            get
            {
                return LavaIntegrationTestHelper.CurrentInstance;
            }
        }

        [ClassInitialize( InheritanceBehavior.BeforeEachDerivedClass )]
        public static void LavaClassInitialize( TestContext context )
        {
            LavaIntegrationTestHelper.Initialize( testRockLiquidEngine: true, testDotLiquidEngine: false, testFluidEngine: true, loadShortcodes: true );
            LogHelper.SetTestContext( context );
        }
    }
}
