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
using Rock.Data;
using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Shortcodes
{
    /// <summary>
    /// Test for shortcodes that are defined and implemented as code components rather than as parameterized Lava templates.
    /// </summary>
    [TestClass]
    [TestCategory( "Core.AI" )]
    public class AiCompletionShortcodeTests : LavaIntegrationTestBase
    {
        [TestMethod]
        public void AiCompletionShortcode_DocumentationExample1_EmitsCorrectHtml()
        {
            var input = @"
{[ aicompletion ]}give me three options for greeting ted decker{[ endaicompletion ]}
";

            // TODO: Identify a means of evaluating the output.
            var expectedOutput = @"
";

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, input );
        }

        [TestMethod]
        public void AiCompletionShortcode_WithProviderParameter_UsesSpecifiedProviderComponent()
        {
            var input = @"
{[ aicompletion provider:'Open AI Compatible 1 (Test)' ]}
give me three options for greeting ted decker
{[ endaicompletion ]}
";

            // TODO: Identify a means of evaluating the output.
            var expectedOutput = @"
";

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, input );
        }

        [TestMethod]
        public void AiCompletionShortcode_WithNoActiveProvider_ReturnsErrorMessageAsOutput()
        {
            var input = @"
{[ aicompletion ]}give me three options for greeting ted decker{[ endaicompletion ]}
";

            // TODO: Identify a means of evaluating the output.
            var expectedOutput = @"
";

            TestHelper.AssertTemplateOutput( typeof(FluidEngine), expectedOutput, input );
        }

    }
}
