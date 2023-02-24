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
using Rock.Tests.UnitTests.Lava;

namespace Rock.Tests.UnitTests.Rock.Lava.Filters
{
    [TestClass]
    public class StructuredContentFilterTests : LavaUnitTestBase
    {
        [TestMethod]
        public void RenderStructuredContentAsHtml_ForValidStructuredContent_ProducesHtml()
        {
            const string jsonString = @"
{
    ""time"":1676039688279,
    ""blocks"":[
       {
          ""id"":""a2FYCrj8NG"",
          ""type"":""header"",
          ""data"":{
             ""text"":""Things I love."",
             ""level"":2
          }
       },
       {
          ""id"":""egdM-bpIfg"",
          ""type"":""list"",
          ""data"":{
             ""style"":""ordered"",
             ""items"":[
                {
                   ""content"":""Reading a good book."",
                   ""items"":[
                      
                   ]
                },
                {
                   ""content"":""Helping other's."",
                   ""items"":[
                      
                   ]
                },
                {
                   ""content"":""Seeing other people laugh."",
                   ""items"":[
                      
                   ]
                }
             ]
          }
       }
    ],
    ""version"":""2.22.1""
 }
";

            const string expectedOutput = @"<h2>Things I love.</h2>
<ol>
<li>Reading a good book.</li>
<li>Helping other's.</li>
<li>Seeing other people laugh.</li>
</ol>
";

            var mergeValues = new LavaDataDictionary { { "JsonString", jsonString } };

            TestHelper.AssertTemplateOutput( expectedOutput,
                "{{ JsonString | RenderStructuredContentAsHtml }}",
                mergeValues, true );
        }

        [TestMethod]
        public void RenderStructuredContentAsHtml_ForInValidStructuredContent_ProducesEmptyString()
        {
            const string jsonString = @"
       {
          ""id"":""egdM-bpIfg"",
          ""type"":""list"",
          ""data"":{
             ""style"":""ordered"",
             ""items"":[
                {
                   ""content"":""Reading a good book."",
                   ""items"":[
                      
                   ]
                },
                {
                   ""content"":""Helping other's."",
                   ""items"":[
                      
                   ]
                },
                {
                   ""content"":""Seeing other people laugh."",
                   ""items"":[
                      
                   ]
                }
             ]
          }
       }";
            string expectedOutput = string.Empty;

            var mergeValues = new LavaDataDictionary { { "JsonString", jsonString } };

            TestHelper.AssertTemplateOutput(expectedOutput,
                "{{ JsonString | RenderStructuredContentAsHtml }}",
                mergeValues, true);
        }
    }
}
