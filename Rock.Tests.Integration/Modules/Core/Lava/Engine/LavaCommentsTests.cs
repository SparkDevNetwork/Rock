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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Lava;
using Rock.Lava.Fluid;

namespace Rock.Tests.Integration.Core.Lava
{
    /// <summary>
    /// Tests for Lava Template comments.
    /// </summary>
    [TestClass]
    public class LavaCommentsFilterTests : LavaIntegrationTestBase
    {
        /// <summary>
        /// Verify that an empty comment block can be parsed correctly.
        /// This test validates a Rock-specific change to the Fluid Parser.
        /// </summary>
        [TestMethod]
        public void CommentBlock_WithEmptyContent_ParsesCorrectly()
        {
            // This Lava template would throw an error in the default Fluid parser, but should process successfully here.
            TestHelper.AssertTemplateOutput( string.Empty, "{% comment %}{% endcomment %}" );
        }

        /// <summary>
        /// Verify that a comment block containing another comment block is parsed as a single comment.
        /// This test validates a Rock-specific change to the Fluid Parser.
        /// </summary>
        [Ignore( "This is a known issue, but it is documented here for reference and may be fixed in the future." )]
        [TestMethod]
        public void CommentBlock_WithNestedCommentBlock_ParsesCorrectly()
        {
            // This Lava template would throw an error in the default Fluid parser, but should process successfully here.
            TestHelper.AssertTemplateOutput( string.Empty, "{% comment %} outer comment {% comment %} inner comment {% endcomment %} {% endcomment %}" );
        }

        /// <summary>
        /// Verify that a comment containing an invalid tag does not cause a parser error.
        /// This test validates a Rock-specific change to the Fluid Parser.
        /// </summary>
        [TestMethod]
        [Ignore( "This is a known issue, but it is documented here for reference and may be fixed in the future." )]
        public void CommentBlock_ContainingInvalidTag_IsIgnored()
        {
            // This Lava template would throw an error in the default Fluid parser, but should process successfully here.
            TestHelper.AssertTemplateOutput( string.Empty, "{% comment %} This comment contains an {% unknown_tag %} {% endcomment %}" );
        }

        [TestMethod]
        public void ShorthandLineComment_AfterContent_ReturnsContent()
        {
            var input = @"
Line 1<br>
Line 2<br>//- This is a single line comment.
Line 3<br>
";

            var expectedOutput = @"
Line 1<br>
Line 2<br>
Line 3<br>
";

            input = input.Trim();
            expectedOutput = expectedOutput.Trim();

            TestHelper.AssertTemplateOutput( expectedOutput, input, new LavaTestRenderOptions { IgnoreWhiteSpace = false } );
        }

        [TestMethod]
        public void ShorthandComment_CommentInStringLiteral_IsNotRemoved()
        {
            var input = @"
-- Begin Example --
Lava Comments can be added as follows:
For a single line comment, use ""//- Single Line Comment 1"" or '//- Single Line Comment 2'.
For a block comment, use ""/- Block Comment 1...
... like this! -/""
or '/- Block Comment 2...
... like this! -/'
-- End Example --
";

            var expectedOutput = @"
-- Begin Example --
Lava Comments can be added as follows:
For a single line comment, use ""//- Single Line Comment 1"" or '//- Single Line Comment 2'.
For a block comment, use ""/- Block Comment 1...
... like this! -/""
or '/- Block Comment 2...
... like this! -/'
-- End Example --
";

            input = input.Trim();
            expectedOutput = expectedOutput.Trim();

            TestHelper.AssertTemplateOutput( expectedOutput, input, new LavaTestRenderOptions { IgnoreWhiteSpace = false } );
        }

        [TestMethod]
        public void ShorthandLineComment_ContainingQuotedString_IsRemoved()
        {
            var input = @"
Line 1<br>
Line 2<br>//-Please enter the following: ""//- This is a single line comment."" and '//- This is also a single line comment'.
Line 3<br>
";

            var expectedOutput = @"
Line 1<br>
Line 2<br>
Line 3<br>
";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        public void ShorthandBlockComment_ContainingQuotedString_IsRemoved()
        {
            var input = @"
Line 1<br>
Line 2<br>/- Please enter the following:
""//- This is a single line comment.""
and
'//- This is also a single line comment'.
-/
Line 3<br>
";

            var expectedOutput = @"
Line 1<br>
Line 2<br>
Line 3<br>
";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void ShorthandComment_CommentInRawTag_IsNotRemoved()
        {
            var input = @"
Example Start<br>
Valid Lava Comment Styles are:
{% raw %}//- Line Comment: A comment that is confined to a single line.
or
/- Block Comment: A comment that can span...
   ... multiple lines. -/{% endraw %}
Example End<br>
";

            var expectedOutput = @"
Example Start<br>
Valid Lava Comment Styles are:
//- Line Comment: A comment that is confined to a single line.
or
/- Block Comment: A comment that can span...
   ... multiple lines. -/
Example End<br>
";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void ShorthandBlockComment_SpanningMultipleLines_RemovesNewLinesContainedInComment()
        {
            var input = @"
Line 1<br>
Line 2 Start<br>/- This is a block comment...


   ... spanning multiple lines. -/Line 2 End<br>
Line 3<br>
";

            var expectedOutput = @"
Line 1<br>
Line 2 Start<br>Line 2 End<br>
Line 3<br>
";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void ShorthandComment_CommentsInIncludeFile_AreRemoved()
        {
            var fileProvider = GetFileProviderWithComments();

            var input = @"
{%- include '_comments.lava' -%}
";

            var expectedOutput = @"
Line 1<br>
Line 2<br>
Line 3<br>
Line 4<br>
";

            var options = new LavaEngineConfigurationOptions
            {
                FileSystem = GetFileProviderWithComments()
            };

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // Create a new engine instance of the same type, but with a test file system configuration.
                var testEngine = LavaService.NewEngineInstance( engine.GetType(), options );

                TestHelper.AssertTemplateOutput( testEngine, expectedOutput, input );
            } );

        }

        [TestMethod]
        public void ShorthandBlockComment_Inline_RendersCorrectLineContent()
        {
            var input = @"
Line 1<br>
Line 2 Start<br>/- This is an inline block comment -/Line 2 End<br>
Line 3<br>
";

            var expectedOutput = @"
Line 1<br>
Line 2 Start<br>Line 2 End<br>
Line 3<br>
";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void ShorthandLineComment_AsFinalElement_RendersCorrectLineContent()
        {
            // Input template terminating with a comment, no new line character.
            var input = @"
Line 1<br>
//- Lava single line comment";

            var expectedOutput = @"Line 1<br>";

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, input );
        }

        [TestMethod]
        public void ShorthandBlockComment_AsFinalElement_RendersCorrectLineContent()
        {
            // Input template terminating with a comment, no new line character.
            var input = @"
Line 1<br>
/- Lava block comment -/";

            var expectedOutput = @"Line 1<br>";

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, input );
        }

        /// <summary>
        /// Verify that a comment containing an invalid tag does not cause a parser error.
        /// This test validates a Rock-specific change to the Fluid Parser.
        /// </summary>
        [TestMethod]
        public void ShorthandBlockComment_ContainingInvalidTag_IsIgnored()
        {
            // This Lava template would throw an error in the default Fluid parser, but should process successfully here.
            TestHelper.AssertTemplateOutput( string.Empty, "/- This comment contains an {% unknown_tag %} -/" );
        }

        private MockFileProvider GetFileProviderWithComments()
        {
            var fileProvider = new MockFileProvider();

            // Add a lava template that includes Lava-specific comments.
            var commentsTemplate = @"
Line 1<br>
//- Lava single line comment
Line 2<br>
/- Lava multi-line
   comment -/
Line 3<br>
{%- comment -%} Liquid comment {%- endcomment -%}
Line 4<br>
";

            fileProvider.Add( "_comments.lava", commentsTemplate );

            return fileProvider;
        }

        [TestMethod]
        public void ShorthandBlockComment_InShortcode_DoesNotRender()
        {
            var input = @"
{[ panel title:'Test' ]}
Line 1<br>
Line 2 Start<br>/- This is an inline block comment -/Line 2 End<br>
Line 3<br>
{[ endpanel ]}
";

            var expectedOutput = @"
<div class=`panel panel-default`>
    <div class=`panel-heading`>
      <h3 class=`panel-title`>
          Test
      </h3>
    </div>
    <div class=`panel-body`>
        Line 1<br>
        Line 2 Start<br>Line 2 End<br>
        Line 3<br>
    </div>
</div>
";
            expectedOutput = expectedOutput.Trim().Replace( "`", @"""" );

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void ShorthandLineComment_InShortcode_DoesNotRender()
        {
            var input = @"
{[ panel title:'Test' ]}
Line 1<br>
Line 2<br>//- This is a single line comment.
Line 3<br>
{[ endpanel ]}
";

            var expectedOutput = @"
<div class=`panel panel-default`>
    <div class=`panel-heading`>
      <h3 class=`panel-title`>
          Test
      </h3>
    </div>
    <div class=`panel-body`>
        Line 1<br>
        Line 2<br>
        Line 3<br>
    </div>
</div>
";

            input = input.Trim();
            expectedOutput = expectedOutput.Trim().Replace( "`", @"""" );

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }
    }
}
