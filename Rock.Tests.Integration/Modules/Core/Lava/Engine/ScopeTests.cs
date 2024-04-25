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
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Engine
{
    /// <summary>
    /// Test the scoping of variables in a Lava context using various container configurations
    /// </summary>
    [TestClass]
    public class ScopeTests : LavaIntegrationTestBase
    {
        // TODO: This is the observed behavior, but is it correct?
        [TestMethod]
        public void Scope_LocalVariableWithSameNameAsContainerVariable_ContainerVariableIsReturned()
        {
            var input = @"
{% execute type:'class' %}
    using Rock;
    using Rock.Data;
    using Rock.Model;
    
    public class MyScript 
    {
        public string Execute() {
            using(RockContext rockContext = new RockContext()){
                var person = new PersonService(rockContext).Get({{ CurrentPerson | Property: 'Id' }});
                
                return person.FullName;
            }
        }
    }
{% endexecute %}
";
            var expectedOutput = @"Admin Admin"; // NOT 'Ted Decker'

            var values = new LavaDataDictionary { { "CurrentPerson", TestHelper.GetTestPersonTedDecker() } };

            var options = new LavaTestRenderOptions() { EnabledCommands = "execute", MergeFields = values };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        /// <summary>
        /// The purpose of this test is to document a valid but unexpected behavior of the Assign statement in Liquid.
        /// The Assign statement either creates or replaces an existing variable of the same name, in the current scope or any higher level scope.
        /// The nesting level at which an Assign statement first creates an internal variable determines the nesting level of that variable for the entire document.
        /// This does not align with the scoping rules for other programming languages.
        /// </summary>
        [TestMethod]
        public void Assign_InnerScopeAssign_ModifiesOuterVariable()
        {
            var input = @"
Context Value (Level 0): {{ currentBlock }}
{%- assign currentBlock = 'document_1' -%}
Document Value (Level 1): {{ currentBlock }}
{%- for i in (1..3) -%}
    {%- assign currentBlock = i | Prepend:'for_loop_' -%}
    Loop Value (Level 2): {{ currentBlock }}
{%- endfor -%}
Document Value (Level 1): {{ currentBlock }}
{%- assign currentBlock = 'document_2' -%}
Document Value (Level 1): {{ currentBlock }}
";


            var expectedOutput = @"
Context Value (Level 0): context
Document Value (Level 1): document_1   
Loop Value (Level 2): for_loop_1
Loop Value (Level 2): for_loop_2
Loop Value (Level 2): for_loop_3
Document Value (Level 1): for_loop_3
Document Value (Level 1): document_2";

            var mergeFields = new LavaDataDictionary() { { "currentBlock", "context" } };

            var options = new LavaTestRenderOptions { MergeFields = mergeFields };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        /// <summary>
        /// The purpose of this test is to document a valid but unexpected behavior of the Assign statement in Liquid.
        /// The Assign statement either creates or replaces an existing variable of the same name, in the current scope or any higher level scope.
        /// The nesting level at which an Assign statement first creates an internal variable determines the nesting level of that variable for the entire document.
        /// This does not align with the scoping rules for other programming languages.
        /// </summary>
        [TestMethod]
        public void ForLoop_InnerLoop_Works()
        {
            var input = @"
{%- for i in (1..3) -%}
> Outer Loop Value: {{ i }}
    {%- for j in (1..3) -%}
--> Inner Loop Value: {{i }}.{{ j }}
    {%- endfor -%}
{%- endfor -%}
";


            var expectedOutput = @"
> Outer Loop Value: 1
--> Inner Loop Value: 1.1
--> Inner Loop Value: 1.2
--> Inner Loop Value: 1.3
> Outer Loop Value: 2
--> Inner Loop Value: 2.1
--> Inner Loop Value: 2.2
--> Inner Loop Value: 2.3
> Outer Loop Value: 3
--> Inner Loop Value: 3.1
--> Inner Loop Value: 3.2
--> Inner Loop Value: 3.3
";

            var mergeFields = new LavaDataDictionary() { { "currentBlock", "context" } };

            var options = new LavaTestRenderOptions { MergeFields = mergeFields };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }
}

}
