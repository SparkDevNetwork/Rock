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

using Rock.Tests.Lava.Shared;

using Rock.Lava;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava.Shortcodes
{
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    [TestCategory( "Core.Lava.Shortcodes" )]
    [TestCategory( "ShortcodeScopeBehavior" )]
    public class ShortcodeScopeBehaviorTests
    {
        [TestMethod]
        public void ShortcodeScopeBehavior_Isolated_DoesNotLeakShortcodeVariablesToParentScope()
        {
            var tagName = $"{nameof( ShortcodeScopeBehaviorTests )}_{nameof( ShortcodeScopeBehavior_Isolated_DoesNotLeakShortcodeVariablesToParentScope )}";

            var templateMarkup = @"
{% assign innerList = '1,2,3' | Split: ',' %}
{% for j in innerList %}
    <InnerPass {{ j }}>
    {% assign counter = counter | Plus:1 %}
{% endfor %}
Inner Scope: counter={{ counter }},
";

            var input = $@"
{{% assign list = '1,2,3' | Split: ',' %}}
{{% assign counter = 0 %}}

{{% for i in list %}}
    <Pass {{{{ forloop.index }}}}>
    {{[ {tagName} ]}}
    Outer Scope: counter={{{{ counter }}}}
{{% endfor %}}
";

            // Isolation means each pass gets its own inner counter, which always
            // reaches 3, and never writes back to the outer one.
            var expectedOutput = "\n\n\n\n"
                + GetExpectedPassOutput( 1, innerCounter: 3, outerCounter: 0 )
                + GetExpectedPassOutput( 2, innerCounter: 3, outerCounter: 0 )
                + GetExpectedPassOutput( 3, innerCounter: 3, outerCounter: 0 )
                + "\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                engine.RegisterShortcode( tagName, ( name ) =>
                {
                    return new DynamicShortcodeDefinition
                    {
                        Name = tagName,
                        TemplateMarkup = templateMarkup,
                        ShortcodeScopeBehavior = Enums.Cms.ShortcodeScopeBehavior.Isolated,
                        ElementType = LavaShortcodeTypeSpecifier.Inline
                    };
                } );

                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void ShortcodeScopeBehavior_Shared_SharesShortcodeVariablesWithParentScope()
        {
            var tagName = $"{nameof( ShortcodeScopeBehaviorTests )}_{nameof( ShortcodeScopeBehavior_Shared_SharesShortcodeVariablesWithParentScope )}";

            var templateMarkup = @"
{% assign innerList = '1,2,3' | Split: ',' %}
{% for j in innerList %}
    <InnerPass {{ j }}>
    {% assign counter = counter | Plus:1 %}
{% endfor %}
Inner Scope: counter={{ counter }},
";

            var input = $@"
{{% assign list = '1,2,3' | Split: ',' %}}
{{% assign counter = 0 %}}

{{% for i in list %}}
    <Pass {{{{ forloop.index }}}}>
    {{[ {tagName} ]}}
    Outer Scope: counter={{{{ counter }}}}
{{% endfor %}}
";

            // Sharing means the shortcode keeps incrementing the outer counter, so
            // each pass starts where the last one finished.
            var expectedOutput = "\n\n\n\n"
                + GetExpectedPassOutput( 1, innerCounter: 3, outerCounter: 3 )
                + GetExpectedPassOutput( 2, innerCounter: 6, outerCounter: 6 )
                + GetExpectedPassOutput( 3, innerCounter: 9, outerCounter: 9 )
                + "\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                engine.RegisterShortcode( tagName, ( name ) =>
                {
                    return new DynamicShortcodeDefinition
                    {
                        Name = tagName,
                        TemplateMarkup = templateMarkup,
                        ShortcodeScopeBehavior = Enums.Cms.ShortcodeScopeBehavior.Shared,
                        ElementType = LavaShortcodeTypeSpecifier.Inline
                    };
                } );

                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expectedOutput, output );
            } );
        }
            /// <summary>
        /// Builds the output one pass of the outer loop produces.
        /// </summary>
        /// <param name="pass">The 1-based pass number.</param>
        /// <param name="innerCounter">The counter value the shortcode reports.</param>
        /// <param name="outerCounter">The counter value the outer scope reports.</param>
        /// <returns>The expected text for that pass.</returns>
        private static string GetExpectedPassOutput( int pass, int innerCounter, int outerCounter )
        {
            return "\n    <Pass " + pass + ">"
                + "\n    <InnerPass 1>\n    \n"
                + "\n    <InnerPass 2>\n    \n"
                + "\n    <InnerPass 3>\n    \n"
                + "\nInner Scope: counter=" + innerCounter + ",\n    Outer Scope: counter=" + outerCounter + "\n";
        }
}
}
