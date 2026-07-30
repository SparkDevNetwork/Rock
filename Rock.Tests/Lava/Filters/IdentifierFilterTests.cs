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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava.Fluid;

namespace Rock.Tests.Lava.Filters
{
    /// <summary>
    /// Tests for the Lava filters that produce or translate identifier values.
    /// </summary>
    [TestClass]
    public class IdentifierFilterTests : LavaUnitTestBase
    {
        /// <summary>
        /// The standard RFC 4122 namespace for fully-qualified domain names, which has widely published
        /// version 5 test vectors.
        /// </summary>
        private const string DnsNamespace = "6ba7b810-9dad-11d1-80b4-00c04fd430c8";

        /// <summary>
        /// Verifies the filter against a published RFC 4122 version 5 test vector, which proves the filter
        /// is wired to a correct implementation rather than merely returning something Guid-shaped.
        /// </summary>
        [TestMethod]
        public void ToGuidV5Filter_WithKnownTestVector_ReturnsExpectedGuid()
        {
            var template = "{{ 'python.org' | ToGuidV5:'" + DnsNamespace + "' }}";

            TestHelper.AssertTemplateOutput( "886313e1-3b8a-5372-9b90-0c9aee199e5d", template );
        }

        /// <summary>
        /// Verifies the usage shown in the filter's own documentation comment produces a stable version 5
        /// Guid.
        /// </summary>
        [TestMethod]
        public void ToGuidV5Filter_DocumentationExample_ReturnsExpectedOutput()
        {
            var input = "{% assign hashedGuid = '7e6286f7-0297-41ff-bdf6-bd5656e1bc53' | ToGuidV5:'" + DnsNamespace + "' %}{{ hashedGuid }}";

            var expectedOutput = "7e6286f7-0297-41ff-bdf6-bd5656e1bc53".ToGuidV5( new Guid( DnsNamespace ) ).ToString();

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        /// <summary>
        /// The filter must be deterministic, since that is the entire reason for using a version 5 Guid.
        /// </summary>
        [TestMethod]
        public void ToGuidV5Filter_CalledTwiceWithSameInput_ReturnsSameGuid()
        {
            var input = "{% assign first = 'alpha' | ToGuidV5:'" + DnsNamespace + "' %}"
                + "{% assign second = 'alpha' | ToGuidV5:'" + DnsNamespace + "' %}"
                + "{% if first == second %}match{% else %}differ{% endif %}";

            TestHelper.AssertTemplateOutput( "match", input );
        }

        /// <summary>
        /// The intended input is a Guid string, which reaches Lava in mixed casing depending on its source,
        /// so an upper-case and a lower-case Guid must hash to the same value.
        /// </summary>
        [TestMethod]
        public void ToGuidV5Filter_WithDifferingInputCase_ReturnsSameGuid()
        {
            var input = "{% assign lower = '7e6286f7-0297-41ff-bdf6-bd5656e1bc53' | ToGuidV5:'" + DnsNamespace + "' %}"
                + "{% assign upper = '7E6286F7-0297-41FF-BDF6-BD5656E1BC53' | ToGuidV5:'" + DnsNamespace + "' %}"
                + "{% if lower == upper %}match{% else %}differ{% endif %}";

            TestHelper.AssertTemplateOutput( "match", input );
        }

        /// <summary>
        /// The namespace argument must also tolerate mixed casing, since it is authored by hand in templates.
        /// </summary>
        [TestMethod]
        public void ToGuidV5Filter_WithDifferingNamespaceCase_ReturnsSameGuid()
        {
            var input = "{% assign lower = 'alpha' | ToGuidV5:'" + DnsNamespace + "' %}"
                + "{% assign upper = 'alpha' | ToGuidV5:'" + DnsNamespace.ToUpper() + "' %}"
                + "{% if lower == upper %}match{% else %}differ{% endif %}";

            TestHelper.AssertTemplateOutput( "match", input );
        }

        /// <summary>
        /// Different names under the same namespace must not collide.
        /// </summary>
        [TestMethod]
        public void ToGuidV5Filter_WithDifferentInput_ReturnsDifferentGuid()
        {
            var input = "{% assign first = 'alpha' | ToGuidV5:'" + DnsNamespace + "' %}"
                + "{% assign second = 'beta' | ToGuidV5:'" + DnsNamespace + "' %}"
                + "{% if first == second %}match{% else %}differ{% endif %}";

            TestHelper.AssertTemplateOutput( "differ", input );
        }

        /// <summary>
        /// An empty or whitespace input has no name to hash, so the filter returns nothing rather than a
        /// misleading identifier.
        /// </summary>
        [TestMethod]
        [DataRow( "''", DisplayName = "Empty String" )]
        [DataRow( "'   '", DisplayName = "Whitespace" )]
        public void ToGuidV5Filter_WithEmptyInput_ReturnsEmptyOutput( string inputValue )
        {
            var template = "{{ " + inputValue + " | ToGuidV5:'" + DnsNamespace + "' }}";

            TestHelper.AssertTemplateOutput( string.Empty, template );
        }

        /// <summary>
        /// A missing or malformed namespace would still hash to a valid-looking Guid, so the filter fails
        /// loudly instead of emitting a silently wrong identifier.
        /// </summary>
        [TestMethod]
        [DataRow( "''", DisplayName = "Empty Namespace" )]
        [DataRow( "'not-a-guid'", DisplayName = "Malformed Namespace" )]
        public void ToGuidV5Filter_WithInvalidNamespace_ReportsError( string namespaceValue )
        {
            var template = "{{ 'alpha' | ToGuidV5:" + namespaceValue + " }}";

            var engine = TestHelper.GetEngineInstance( typeof( FluidEngine ) );
            var result = engine.RenderTemplate( template );

            Assert.IsTrue( result.HasErrors, "Expected an invalid namespace to produce a render error." );
            StringAssert.Contains( result.Error.ToString(), "Invalid Namespace Guid Value" );
        }
    }
}
