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
using Rock.Tests.Lava.Shared;

namespace Rock.Tests.Integration.Core.Lava.Shortcodes
{
    /// <summary>
    /// Test for shortcodes that are defined and implemented as code components rather than as parameterized Lava templates.
    /// </summary>
    [TestClass]
    public partial class ShortcodeCodeTests : LavaIntegrationTestBase
    {
        /// <summary>
        /// A shortcode with no specific commands enabled should inherit the enabled commands from the outer scope.
        /// </summary>
        [TestMethod]
        public void Shortcode_WithUnspecifiedEnabledCommands_InheritsEnabledCommandsFromOuterScope()
        {
            var shortcodeTemplate = @"
{%- execute -%}
    return ""Shortcode!"";
{%- endexecute -%}
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
{%- execute -%}
    return ""Main!"";
{%- endexecute -%}
<br>
";

            var expectedOutput = "\n"
                + "Shortcode Output:\n"
                + "Shortcode!\n"
                + "<br>\n"
                + "Main Output:Main!<br>\n";

            // Render the template with the "execute" command enabled.
            // This permission setting should be inherited by the shortcode, allowing it to render the "execute" command.
            var options = new LavaRenderOptions { EnabledCommands = "execute" };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// A shortcode that enables a specific command should not cause that command to be enabled outside the scope of the shortcode.
        /// </summary>
        [TestMethod]
        public void Shortcode_WithEnabledCommand_DoesNotEnableCommandForOuterScope()
        {
            var shortcodeTemplate = @"
{%- execute -%}
    return ""Shortcode!"";
{%- endexecute -%}
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
{%- execute -%}
    return ""Main!"";
{%- endexecute -%}
<br>
";

            var expectedOutput = "\n"
                + "Shortcode Output:\n"
                + "Shortcode!\n"
                + "<br>\n"
                + "Main Output:The Lava command 'execute' is not configured for this template.<br>\n";

            // Render the template with no enabled commands.
            // The shortcode should render correctly using the enabled commands defined by its definition,
            // but the main template should show a permission error.
            var options = new LavaRenderOptions { EnabledCommands = "" };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        #region Bootstrap Alert

        /// <summary>
        /// Using the BootstrapAlert shortcode produces the expected output.
        /// </summary>
        [TestMethod]
        [DataRow( "{[ bootstrapalert ]}This is an information message.{[ endbootstrapalert ]}", "<div class='alert alert-info'>This is an information message.</div>" )]
        [DataRow( "{[ bootstrapalert type:'success' ]}This is a success message.{[ endbootstrapalert ]}", "<div class='alert alert-success'>This is a success message.</div>" )]
        public void BootstrapAlertShortcode_VariousTypes_ProducesCorrectHtml( string input, string expectedResult )
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expectedResult, output );
            } );
        }

        #endregion

        #region MediaPlayer

        [TestMethod]
        public void MediaPlayerShortcode_WithDefaultParameters_ProducesCorrectHtml()
        {
            var input = @"
{[mediaplayer media:'18' ]}{[endmediaplayer]}
";
            // The player element carries an identifier generated per render, so
            // it is matched as a wildcard rather than written out.
            var expectedOutput = "\n"
                + "<div id=\"mediaplayer_<id>\" style=\"--plyr-color-main: var(--color-primary);\"$></div>\n"
                + "<script>\n"
                + "(function() {\n"
                + "    new Rock.UI.MediaPlayer(\"#mediaplayer_<id>\", {\"autopause\":true,\"autoplay\":false,\"clickToPlay\":true,\"controls\":\"play-large,play,progress,current-time,mute,volume,captions,settings,pip,airplay,fullscreen\",\"debug\":false,\"hideControls\":true,\"map\":\"\",\"mediaUrl\":\"\",\"muted\":false,\"posterUrl\":\"\",\"resumePlaying\":true,\"seekTime\":10.0,\"trackProgress\":true,\"type\":\"\",\"volume\":1.0,\"writeInteraction\":true});\n"
                + "})();\n"
                + "</script>\n"
                + "\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                LavaAssert.Matches( expectedOutput, output, "<id>" );
            } );
        }

        #endregion

        #region Scripturize

        /// <summary>
        /// Using the Scripturize shortcode produces the expected output.
        /// </summary>
        [TestMethod]
        [DataRow( "John 3:16", "<a href=\"https://www.bible.com/bible/116/JHN.3.16.NLT\"  class=\"scripture\" title=\"YouVersion\">John 3:16</a>" )]
        [DataRow( "Jn 3:16", "<a href=\"https://www.bible.com/bible/116/JHN.3.16.NLT\"  class=\"scripture\" title=\"YouVersion\">Jn 3:16</a>" )]
        [DataRow( "John 3", "<a href=\"https://www.bible.com/bible/116/JHN.3..NLT\"  class=\"scripture\" title=\"YouVersion\">John 3</a>" )]

        public void ScripturizeShortcode_YouVersion_ProducesCorrectHtml( string input, string expectedResult )
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{[ scripturize defaulttranslation:'NLT' landingsite:'YouVersion' cssclass:'scripture' ]}" + input + "{[ endscripturize ]}" );

                Assert.AreEqual( expectedResult, output );
            } );
        }

        [TestMethod]
        public void ScripturizeShortcode_WithInvalidLandingSite_ProducesErrorMessage()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{[ scripturize defaulttranslation:'NLT' landingsite:'InvalidSite' cssclass:'scripture' ]}John 3:16{[ endscripturize ]}" );

                Assert.AreEqual( "<!-- the landing site provided to the scripturize shortcode was not correct -->John 3:16", output );
            } );
        }

        #endregion

        #region Workflow Activate

        [TestMethod]
        [DataRow( "Workflow", "some workflow" )]
        [DataRow( "Activity", "some activity" )]
        public void WorkflowActivate_WithPreexistingReservedMergeField_SavesAndRestoresMergeFieldValue( string mergeFieldKey, string mergeFieldValue )
        {
            var input = $@"
{{%- workflowactivate workflowtype:'51FE9641-FB8F-41BF-B09E-235900C3E53E' -%}}
{{%- endworkflowactivate -%}}
Restored Value: {{{{{mergeFieldKey}}}}}";

            var expectedOutput = $"Restored Value: {mergeFieldValue}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions
                {
                    EnabledCommands = "workflowactivate",
                    MergeFields = new LavaDataDictionary { { mergeFieldKey, mergeFieldValue } }
                };

                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.Contains( expectedOutput, output, $"Reserved merge field with key '{mergeFieldKey}' was not restored." );
            } );
        }

        [TestMethod]
        public void WorkflowActivate_WithPreexistingErrorMergeField_SavesAndRestoresMergeFieldValue()
        {
            var input = @"
{%- workflowactivate workflowtype:'some-invalid-workflow-type-guid' -%}
{%- endworkflowactivate -%}
Restored Value: {{Error}}";

            var expectedOutput = $"Restored Value: some error";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions
                {
                    EnabledCommands = "workflowactivate",
                    MergeFields = new LavaDataDictionary { { "Error", "some error" } }
                };

                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.Contains( expectedOutput, output, $"Reserved merge field with key 'Error' was not restored." );
            } );
        }

        #endregion

        /// <summary>
        /// Verify that a shortcode tag is parsed correctly when embedded in an if/endif block.
        /// </summary>
        /// <remarks>This test is necessary to verify custom changes to the Fluid parser.</remarks>
        [TestMethod]
        public void ShortcodeParsing_ShortcodeEmbeddedInIfBlock_IsParsedCorrectly()
        {
            var input = @"
{%- if 1 == 1 -%}
{[ sparkline type:'line' data:'5,6,7,9,9,5,3,2,2,4,6,7' ]}
{%- endif -%}
";

            // The whitespace control on the if tags leaves no newline before the
            // first script element, and none after the last one.
            var expectedResult = @"<script src='~/Scripts/sparkline/jquery-sparkline.min.js' type='text/javascript'></script>
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
".NormalizeLineEndings()
                // A line of two spaces, written separately so that it is not
                // lost to an editor that trims trailing whitespace.
                + "  \n"
                + "  </script>";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                LavaAssert.Matches( expectedResult, output, "<guid>" );
            } );
        }

        /// <summary>
        /// Verify that nested shortcode blocks are rendered correctly.
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

            var expectedResult = string.Join( "\n", new[]
            {
                @"",
                @"<div class=""panel-group"" id=""accordion-id-<guid1>"" role=""tablist"" aria-multiselectable=""true""><div class=""panel panel-default"">",
                @"        <div class=""panel-heading"" role=""tab"" id=""heading1-id-<guid1>"">",
                @"          <h4 class=""panel-title"">",
                @"            <a role=""button"" data-toggle=""collapse"" data-parent=""#accordion-id-<guid1>"" href=""#collapse1-id-<guid1>"" aria-expanded=""true"" aria-controls=""collapse1"">",
                @"              Line Chart",
                @"            </a>",
                @"          </h4>",
                @"        </div>",
                @"        <div id=""collapse1-id-<guid1>"" class=""panel-collapse collapse in"" role=""tabpanel"" aria-labelledby=""heading1-id-<guid1>"">",
                @"          <div class=""panel-body"">",
                @"            <script src='~/Scripts/sparkline/jquery-sparkline.min.js' type='text/javascript'></script>",
                @"<span class=""sparkline sparkline-id-<guid2>"">Loading...</span><script>",
                @"  $("".sparkline-id-<guid2>"").sparkline([5,6,7,9,9,5,3,2,2,4,6,7], {",
                @"      type: 'line'",
                @"      , width: 'auto'",
                @"      , height: 'auto'",
                @"      , lineColor: '#ee7625'",
                @"      , fillColor: '#f7c09b'",
                @"      , lineWidth: 1",
                @"      , spotColor: '#f80'",
                @"      , minSpotColor: '#f80'",
                @"      , maxSpotColor: '#f80'",
                @"      , highlightSpotColor: ''",
                @"      , highlightLineColor: ''",
                @"      , spotRadius: 1.5",
                @"      , chartRangeMin: undefined",
                @"      , chartRangeMax: undefined",
                @"      , chartRangeMinX: undefined",
                @"      , chartRangeMaxX: undefined",
                @"      , normalRangeMin: undefined",
                @"      , normalRangeMax: undefined",
                @"      , normalRangeColor: '#ccc'",
                @"    });",
                @"  ",
                @"  </script>",
                @"          </div>",
                @"        </div>",
                @"      </div></div>",
                @"",
            } );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                LavaAssert.Matches( expectedResult, output, "<guid1>", "<guid2>" );
            } );
        }

        /// <summary>
        /// Verify that a shortcode embedded in an [[ item ]] tag is rendered correctly.
        /// </summary>
        [TestMethod]
        //[Ignore( "This test documents a potential bug. The nested [[ item ]] tag is not resolved correctly." )]
        public void ShortcodeParsing_ShortcodeEmbeddedInItemElement_IsParsedCorrectly()
        {
            var input = @"
{[ accordion ]}
    [[ item title:'Item 1' ]]
        {[ accordion ]}
            [[ item title:'Item 2' ]]
            [[ enditem ]]
        {[ endaccordion ]}
    [[ enditem ]]
{[ endaccordion ]}
";

            var expectedResult = string.Join( "\n", new[]
            {
                @"",
                @"<div class=""panel-group"" id=""accordion-id-<guid1>"" role=""tablist"" aria-multiselectable=""true""><div class=""panel panel-default"">",
                @"        <div class=""panel-heading"" role=""tab"" id=""heading1-id-<guid1>"">",
                @"          <h4 class=""panel-title"">",
                @"            <a role=""button"" data-toggle=""collapse"" data-parent=""#accordion-id-<guid1>"" href=""#collapse1-id-<guid1>"" aria-expanded=""true"" aria-controls=""collapse1"">",
                @"              Item 1",
                @"            </a>",
                @"          </h4>",
                @"        </div>",
                @"        <div id=""collapse1-id-<guid1>"" class=""panel-collapse collapse in"" role=""tabpanel"" aria-labelledby=""heading1-id-<guid1>"">",
                @"          <div class=""panel-body"">",
                @"            <div class=""panel-group"" id=""accordion-id-<guid1>"" role=""tablist"" aria-multiselectable=""true""><div class=""panel panel-default"">",
                @"        <div class=""panel-heading"" role=""tab"" id=""heading1-id-<guid1>"">",
                @"          <h4 class=""panel-title"">",
                @"            <a role=""button"" data-toggle=""collapse"" data-parent=""#accordion-id-<guid1>"" href=""#collapse1-id-<guid1>"" aria-expanded=""true"" aria-controls=""collapse1"">",
                @"              Item 2",
                @"            </a>",
                @"          </h4>",
                @"        </div>",
                @"        <div id=""collapse1-id-<guid1>"" class=""panel-collapse collapse in"" role=""tabpanel"" aria-labelledby=""heading1-id-<guid1>"">",
                @"          <div class=""panel-body"">",
                @"            ",
                @"          </div>",
                @"        </div>",
                @"      </div></div>",
                @"          </div>",
                @"        </div>",
                @"      </div></div>",
                @"",
            } );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                LavaAssert.Matches( expectedResult, output, "<guid1>", "<guid2>" );
            } );
        }

    }
}
