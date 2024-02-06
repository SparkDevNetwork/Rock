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
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Lava.RockLiquid;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Commands
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

            var context = new LavaDataDictionary() { { "fontsize", 99 } };

            var options = new LavaTestRenderOptions { MergeFields = context };

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // RockLiquid uses a different mechanism for registering shortcodes that cannot be tested here.
                if ( engine.GetType() == typeof( RockLiquidEngine ) )
                {
                    return;
                }

                engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

                TestHelper.AssertTemplateOutput( engine, expectedOutput, input, options );
            } );
        }

        [TestMethod]
        public void Shortcode_ReferencingItemFromParentScope_CorrectlyResolvesItem()
        {
            var shortcodeTemplate = @"
ValueInShortcodeScope = {{ Value }}
";

            // Create a new test shortcode.
            var shortcodeDefinition = new DynamicShortcodeDefinition();

            shortcodeDefinition.ElementType = LavaShortcodeTypeSpecifier.Inline;
            shortcodeDefinition.TemplateMarkup = shortcodeTemplate;
            shortcodeDefinition.Name = "debug";

            var input = @"
ValueInOuterScope = {{ Value }}
{[ debug ]}
";

            var expectedOutput = @"
ValueInOuterScope = 99
ValueInShortcodeScope = 99
";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            var context = new LavaDataDictionary() { { "Value", 99 } };

            var options = new LavaTestRenderOptions { MergeFields = context };

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // RockLiquid uses a different mechanism for registering shortcodes that cannot be tested here.
                if ( engine.GetType() == typeof( RockLiquidEngine ) )
                {
                    return;
                }

                engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

                TestHelper.AssertTemplateOutput( engine, expectedOutput, input, options );
            } );
        }

        /// <summary>
        /// A shortcode with no specific commands enabled should inherit the enabled commands from the outer scope.
        /// </summary>
        [TestMethod]
        public void Shortcode_WithUnspecifiedEnabledCommands_InheritsEnabledCommandsFromOuterScope()
        {
            var shortcodeTemplate = @"
{% execute %}
    return ""Shortcode!"";
{% endexecute %}
";

            // Create a new test shortcode with no enabled commands.
            var shortcodeDefinition = new DynamicShortcodeDefinition();

            shortcodeDefinition.ElementType = LavaShortcodeTypeSpecifier.Inline;
            shortcodeDefinition.TemplateMarkup = shortcodeTemplate;
            shortcodeDefinition.Name = "shortcode_execute";
            shortcodeDefinition.EnabledLavaCommands = new List<string> { "" };

            var input = @"
Shortcode Output:
{[ shortcode_execute ]}
<br>
Main Output:
{% execute %}
    return ""Main!"";
{% endexecute %}
<br>
";

            var expectedOutput = @"
Shortcode Output: Shortcode!<br>
Main Output: Main!<br>
";

            // Render the template with the "execute" command enabled.
            // This permission setting should be inherited by the shortcode, allowing it to render the "execute" command.
            var options = new LavaTestRenderOptions { EnabledCommands = "execute" };

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // RockLiquid uses a different mechanism for registering shortcodes that cannot be tested here.
                if ( engine.GetType() == typeof( RockLiquidEngine ) )
                {
                    return;
                }

                engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

                TestHelper.AssertTemplateOutput( engine, expectedOutput, input, options );
            } );
        }

        /// <summary>
        /// A shortcode that enables a specific command should not cause that command to be enabled outside the scope of the shortcode.
        /// </summary>
        [TestMethod]
        public void Shortcode_WithEnabledCommand_DoesNotEnableCommandForOuterScope()
        {
            var shortcodeTemplate = @"
{% execute %}
    return ""Shortcode!"";
{% endexecute %}
";

            // Create a new test shortcode with the "execute" command permission.
            var shortcodeDefinition = new DynamicShortcodeDefinition();

            shortcodeDefinition.ElementType = LavaShortcodeTypeSpecifier.Inline;
            shortcodeDefinition.TemplateMarkup = shortcodeTemplate;
            shortcodeDefinition.Name = "shortcode_execute";
            shortcodeDefinition.EnabledLavaCommands = new List<string> { "execute" };

            var input = @"
Shortcode Output:
{[ shortcode_execute ]}
<br>
Main Output:
{% execute %}
    return ""Main!"";
{% endexecute %}
<br>
";

            var expectedOutput = @"
Shortcode Output: Shortcode!<br>
Main Output: The Lava command 'execute' is not configured for this template.<br>
";

            // Render the template with no enabled commands.
            // The shortcode should render correctly using the enabled commands defined by its definition,
            // but the main template should show a permission error.
            var options = new LavaTestRenderOptions { EnabledCommands = "" };

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // RockLiquid uses a different mechanism for registering shortcodes that cannot be tested here.
                if ( engine.GetType() == typeof( RockLiquidEngine ) )
                {
                    return;
                }

                engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

                TestHelper.AssertTemplateOutput( engine, expectedOutput, input, options );
            } );
        }

        #region Bootstrap Alert

        /// <summary>
        /// Using the BootstrapAlert shortcode produces the expected output.
        /// </summary>
        [DataTestMethod]
        [DataRow( "{[ bootstrapalert ]}This is an information message.{[ endbootstrapalert ]}", "<div class='alert alert-info'>This is an information message.</div>" )]
        [DataRow( "{[ bootstrapalert type:'success' ]}This is a success message.{[ endbootstrapalert ]}", "<div class='alert alert-success'>This is a success message.</div>" )]
        public void BootstrapAlertShortcode_VariousTypes_ProducesCorrectHtml( string input, string expectedResult )
        {
            TestHelper.AssertTemplateOutput( expectedResult, input );
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
{% capture isScheduleActive %}{[ scheduledcontent scheduleid:'6' ]}true{[ endscheduledcontent ]}
{% endcapture %}
Schedule Active = {{isScheduleActive}}
";
            var expectedOutput = @"Schedule Active = true";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        #endregion

        #region MediaPlayer

        [TestMethod]
        public void MediaPlayerShortcode_WithDefaultParameters_ProducesCorrectHtml()
        {
            var input = @"
{[mediaplayer media:'18' ]}{[endmediaplayer]}
";
            var expectedOutput = @"
<div id=`mediaplayer_*` style=`--plyr-color-main:var(--color-primary);`$></div>
<script>
(function(){newRock.UI.MediaPlayer(`#mediaplayer_*`,{`autopause`:true,`autoplay`:false,`clickToPlay`:true,`controls`:`play-large,play,progress,current-time,mute,volume,captions,settings,pip,airplay,fullscreen`,`debug`:false,`hideControls`:true,`map`:``,`mediaUrl`:``,`muted`:false,`posterUrl`:``,`resumePlaying`:true,`seekTime`:10.0,`trackProgress`:true,`type`:``,`volume`:1.0,`writeInteraction`:true});})();
</script>
";
            expectedOutput = expectedOutput.Replace( "`", @"""" );

            var options = new LavaTestRenderOptions
            {
                OutputMatchType = LavaTestOutputMatchTypeSpecifier.RegEx,
                Wildcards = new List<string> { "*" }
            };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
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

        [TestMethod]
        public void ScripturizeShortcode_WithInvalidLandingSite_ProducesErrorMessage()
        {
            TestHelper.AssertTemplateOutput( "<!--the landing site provided to the scripturize shortcode was not correct-->John 3:16",
                                          "{[ scripturize defaulttranslation:'NLT' landingsite:'InvalidSite' cssclass:'scripture' ]}John 3:16{[ endscripturize ]}" );
        }

        #endregion

        /// <summary>
        /// Verify that an invalid shortcode name correctly throws a shortcode parsing error when embedded in an if/endif block.
        /// </summary>
        [TestMethod]
        public void ShortcodeParsing_UndefinedShortcodeTag_ThrowsUnknownShortcodeParsingError()
        {
            // Create a template containing an undefined shortcode "testshortcode1".

            var input = @"
<p>Document start.</p>
{[ testshortcode1 ]}
<p>Document end.</p>
";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var result = engine.RenderTemplate( input, new LavaRenderParameters { ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.Ignore } );

                // Verify that the result emits the expected parse error.
                var error = result.Error?.InnerException;
                if ( !(error is LavaParseException ) )
                {
                    throw new Exception( "Parse exception expected but not encountered." );
                }

                if ( engine.GetType() == typeof( FluidEngine ) )
                {
                    Assert.That.IsTrue( error.Message.Contains( "Unknown shortcode 'testshortcode1'" ), "Unexpected Lava error message." );
                }
            } );

        }

        /// <summary>
        /// Verify that an invalid shortcode name correctly throws a shortcode parsing error when embedded in an if/endif block.
        /// </summary>
        [TestMethod]
        public void ShortcodeParsing_UndefinedShortcodeEmbeddedInIfBlock_ThrowsCorrectParsingError()
        {
            // Create a template containing an undefined shortcode "testshortcode1".

            var input = @"
{% if 1 == 1 %}
    {[ invalidshortcode ]}
{% endif %}
";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var result = engine.RenderTemplate( input, new LavaRenderParameters { ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.Ignore } );

                // Verify that the result emits the expected parse error.
                var error = result.Error?.InnerException;
                if ( !( error is LavaParseException ) )
                {
                    throw new Exception( "Parse exception expected but not encountered." );
                }

                // In Fluid, parse error should correctly identify the invalid shortcode.
                if ( engine.GetType() == typeof( FluidEngine ) )
                {
                    if ( !error.Message.Contains( "Unknown shortcode 'invalidshortcode'" ) )
                    {
                        throw result.Error;
                    }
                }

            } );

        }

        /// <summary>
        /// Verify that a shortcode tag is parsed correctly when embedded in an if/endif block.
        /// </summary>
        /// <remarks>This test is necessary to verify custom changes to the Fluid parser.</remarks>
        [TestMethod]
        public void ShortcodeParsing_ShortcodeEmbeddedInIfBlock_IsParsedCorrectly()
        {
            var input = @"
{% if 1 == 1 %}
{[ sparkline type:'line' data:'5,6,7,9,9,5,3,2,2,4,6,7' ]}
{% endif %}
";

            var expectedResult = @"
<script src='~/Scripts/sparkline/jquery-sparkline.min.js' type='text/javascript'></script>
<span class=""sparkline sparkline-id-<guid>"">Loading...</span><script>
  $("".sparkline-id-<guid>"").sparkline([5,6,7,9,9,5,3,2,2,4,6,7], {
      type: 'line'
      , width: 'auto'
      , height: 'auto'
      , lineColor: '#ee7625'
      , fillColor: '#f7c09b'
      , lineWidth: 1
      , spotColor: '#f80'
      , minSpotColor: '#f80'
      , maxSpotColor: '#f80'
      , highlightSpotColor: ''
      , highlightLineColor: ''
      , spotRadius: 1.5
      , chartRangeMin: undefined
      , chartRangeMax: undefined
      , chartRangeMinX: undefined
      , chartRangeMaxX: undefined
      , normalRangeMin: undefined
      , normalRangeMax: undefined
      , normalRangeColor: '#ccc'
    });
  </script>
";

            TestHelper.AssertTemplateOutput( expectedResult, input, new LavaTestRenderOptions { Wildcards = new List<string> { "<guid>" } } );
        }

        /// <summary>
        /// Verify that an invalid shortcode name correctly throws a shortcode parsing error when embedded in an if/endif block.
        /// </summary>
        [TestMethod]
        public void ShortcodeParsing_ShortcodeEmbeddedInOuterShortcode_IsParsedCorrectly()
        {
            var input = @"
{[ accordion ]}
    [[ item title:'Line Chart' ]]
        {[ sparkline type:'line' data:'5,6,7,9,9,5,3,2,2,4,6,7' ]}
    [[ enditem ]]
{[ endaccordion ]}
";

            var expectedResult = @"
<div class=""panel-group"" id=""accordion-id-<guid1>"" role=""tablist"" aria-multiselectable=""true"">
    <div class=""panel panel-default"">
        <div class=""panel-heading"" role=""tab"" id=""heading1-id-<guid1>"">
          <h4 class=""panel-title"">
            <a role=""button"" data-toggle=""collapse"" data-parent=""#accordion-id-<guid1>"" href=""#collapse1-id-<guid1>"" aria-expanded=""true"" aria-controls=""collapse1"">
              Line Chart
            </a>
          </h4>
        </div>
        <div id=""collapse1-id-<guid1>"" class=""panel-collapse collapse in"" role=""tabpanel"" aria-labelledby=""heading1-id-<guid1>"">
          <div class=""panel-body"">
            <script src='~/Scripts/sparkline/jquery-sparkline.min.js' type='text/javascript'></script>
            <span class=""sparkline sparkline-id-<guid2>"">Loading...</span>
            <script>
              $("".sparkline-id-<guid2>"").sparkline([5,6,7,9,9,5,3,2,2,4,6,7], {
                  type: 'line'
                  , width: 'auto'
                  , height: 'auto'
                  , lineColor: '#ee7625'
                  , fillColor: '#f7c09b'
                  , lineWidth: 1
                  , spotColor: '#f80'
                  , minSpotColor: '#f80'
                  , maxSpotColor: '#f80'
                  , highlightSpotColor: ''
                  , highlightLineColor: ''
                  , spotRadius: 1.5
                  , chartRangeMin: undefined
                  , chartRangeMax: undefined
                  , chartRangeMinX: undefined
                  , chartRangeMaxX: undefined
                  , normalRangeMin: undefined
                  , normalRangeMax: undefined
                  , normalRangeColor: '#ccc'
                });
            </script>
          </div>
        </div>
    </div>
</div>
";

            TestHelper.AssertTemplateOutput( expectedResult, input, new LavaTestRenderOptions { Wildcards = new List<string> { "<guid1>", "<guid2>" } } );
        }
    }
}
