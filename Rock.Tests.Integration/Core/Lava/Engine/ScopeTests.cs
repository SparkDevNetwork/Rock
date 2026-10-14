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
using Rock.Tests.Integration.TestData;
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Integration.Core.Lava.Engine
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
            /*
                9/26/26 - CLAUDE

                What this test turns on is the Id of the merge object, not its
                name. The object is named after Ted Decker but carries Id 1,
                which belongs to Admin, and the script looks the person up by
                that Id. So the output is "Admin Admin" rather than the name on
                the object, which is what the test is asserting.

                The stand-in is built here rather than fetched, because a real
                Ted Decker would carry his own Id and the test would no longer
                be showing anything.

                Reason: The Id on the merge object is the point of the test.
            */
            var expectedOutput = "\nAdmin Admin\n";

            var standInPerson = new LavaDataObject
            {
                ["Id"] = 1,
                ["NickName"] = "Ted",
                ["LastName"] = "Decker"
            };

            var values = new LavaDataDictionary { { "CurrentPerson", standInPerson } };

            var options = new LavaRenderOptions() { EnabledCommands = "execute", MergeFields = values };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }
    }
}
