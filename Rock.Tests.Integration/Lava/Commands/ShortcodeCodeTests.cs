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

namespace Rock.Tests.Integration.Lava
{
    /// <summary>
    /// Test for shortcodes that are defined and implemented as code components rather than as parameterized Lava templates.
    /// </summary>
    [TestClass]
    public class ShortcodeCodeTests : LavaIntegrationTestBase
    {
        [TestMethod]
        public void Shortcode_WithMergeFieldAsParameter_CorrectlyResolvesParameters()
        {
            var shortcodeTemplate = @"
Font Name: {{ fontname }}
Font Size: {{ fontsize }}
Font Bold: {{ fontbold }}
";

            // Create a new test shortcode.
            var shortcodeDefinition = new DynamicShortcodeDefinition();

            shortcodeDefinition.ElementType = LavaShortcodeTypeSpecifier.Block;
            shortcodeDefinition.TemplateMarkup = shortcodeTemplate;
            shortcodeDefinition.Name = "shortcodetest";

            var input = @"
{[ shortcodetest fontname:'Arial' fontsize:'{{ fontsize }}' fontbold:'true' ]}
{[ endshortcodetest ]}
";

            var expectedOutput = @"
Font Name: Arial
Font Size: 99
Font Bold: true
";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            var context = new LavaDataDictionary() { { "fontsize", 99 }  };

            var options = new LavaTestRenderOptions { MergeFields = context };

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

                TestHelper.AssertTemplateOutput( engine.EngineType, expectedOutput, input, options );
            } );
        }

        #region Bootstrap Alert

        /// <summary>
        /// Using the Scripturize shortcode produces the expected output.
        /// </summary>
        [DataTestMethod]
        [DataRow( "{[ bootstrapalert type='info' ]}This is an information message.{[ endbootstrapalert ]}", "<div class='alert alert-info'>This is an information message.</div>" )]

        public void BootstrapAlertShortcode_VariousTypes_ProducesCorrectHtml( string input, string expectedResult )
        {
            
            TestHelper.AssertTemplateOutput( expectedResult,
                                          input );
        }

        #endregion

        #region ScheduledContent

        [TestMethod]
        public void ScheduledContentShortcode_Basic_EmitsCorrectHtml()
        {
            var input = @"
{[ scheduledcontent scheduleid:'1' asatdate:'2020-10-17 16:35' ]}
Schedule Name: {{ Schedule.Name }}
<br>
Schedule Live: {{ IsLive }}
<br>
{[ endscheduledcontent ]}
";

            var expectedOutput = @"
ScheduleName:Saturday4:30pm<br>ScheduleLive:true<br>
";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void ScheduledContentShortcode_ContainedInCaptureBlock_EmitsCorrectOutput()
        {
            var input = @"
{% capture isScheduleActive %}
{[ scheduledcontent scheduleid:'6' ]}true{[ endscheduledcontent ]}
{% endcapture %}
Schedule Active = {{isScheduleActive}}
";
            var expectedOutput = @"Schedule Active = true";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        #endregion

        #region Scripturize

        /// <summary>
        /// Using the Scripturize shortcode produces the expected output.
        /// </summary>
        [DataTestMethod]
        [DataRow( "John 3:16", "<a href=\"https://www.bible.com/bible/116/JHN.3.16.NLT\"  class=\"scripture\" title=\"YouVersion\">John 3:16</a>" )]
        [DataRow( "Jn 3:16", "<a href=\"https://www.bible.com/bible/116/JHN.3.16.NLT\"  class=\"scripture\" title=\"YouVersion\">Jn 3:16</a>" )]
        [DataRow( "John 3", "<a href=\"https://www.bible.com/bible/116/JHN.3..NLT\"  class=\"scripture\" title=\"YouVersion\">John 3</a>" )]

        public void ScripturizeShortcode_YouVersion_ProducesCorrectHtml( string input, string expectedResult )
        {
            TestHelper.AssertTemplateOutput( expectedResult,
                                          "{[ scripturize defaulttranslation:'NLT' landingsite:'YouVersion' cssclass:'scripture' ]}" + input + "{[ endscripturize ]}" );
        }

        #endregion
    }
}
