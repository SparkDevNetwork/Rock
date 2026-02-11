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

namespace Rock.Tests.Lava.Shortcodes
{
    [TestClass]
    [TestCategory( "Core.Lava.Shortcodes" )]
    [TestCategory( "ShortcodeScopeBehavior" )]
    public class ShortcodeScopeBehaviorTests : LavaUnitTestBase
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

            var expectedOutput = "<Pass1><InnerPass1><InnerPass2><InnerPass3>InnerScope:counter=3,OuterScope:counter=0<Pass2><InnerPass1><InnerPass2><InnerPass3>InnerScope:counter=3,OuterScope:counter=0<Pass3><InnerPass1><InnerPass2><InnerPass3>InnerScope:counter=3,OuterScope:counter=0";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
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

                TestHelper.AssertTemplateOutput( engine, expectedOutput, input, ignoreWhitespace: true );
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

            var expectedOutput = "<Pass1><InnerPass1><InnerPass2><InnerPass3>InnerScope:counter=3,OuterScope:counter=3<Pass2><InnerPass1><InnerPass2><InnerPass3>InnerScope:counter=6,OuterScope:counter=6<Pass3><InnerPass1><InnerPass2><InnerPass3>InnerScope:counter=9,OuterScope:counter=9";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
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

                TestHelper.AssertTemplateOutput( engine, expectedOutput, input, ignoreWhitespace: true );
            } );
        }
    }
}
