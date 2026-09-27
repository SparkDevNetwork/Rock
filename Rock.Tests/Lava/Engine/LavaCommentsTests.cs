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
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava.Engine
{
    /// <summary>
    /// Tests for Lava Template comments.
    /// </summary>
    /// <remarks>
    /// The tests covering comments inside a shortcode remain in
    /// Rock.Tests.Integration, because the panel and accordion shortcodes are
    /// LavaShortcode rows rather than classes and cannot be registered without a
    /// database.
    /// </remarks>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class LavaCommentsTests
    {
        #region Comment Block

        /// <summary>
        /// Verify that an empty comment block can be parsed correctly.
        /// This test validates a Rock-specific change to the Fluid Parser.
        /// </summary>
        [TestMethod]
        public void CommentBlock_WithEmptyContent_ParsesCorrectly()
        {
            // This Lava template would throw an error in the default Fluid parser, but should process successfully here.
            var input = "{% comment %}{% endcomment %}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( string.Empty, output );
            } );
        }

        /// <summary>
        /// Verify that a comment block containing another comment block is parsed as a single comment.
        /// This test validates a Rock-specific change to the Fluid Parser.
        /// </summary>
        [TestMethod]
        [Ignore( "This is a known issue, but it is documented here for reference and may be fixed in the future." )]
        public void CommentBlock_WithNestedCommentBlock_ParsesCorrectly()
        {
            var input = "{% comment %} outer comment {% comment %} inner comment {% endcomment %} {% endcomment %}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( string.Empty, output );
            } );
        }

        /// <summary>
        /// Verify that a comment containing an invalid tag does not cause a parser error.
        /// This test validates a Rock-specific change to the Fluid Parser.
        /// </summary>
        [TestMethod]
        public void CommentBlock_ContainingInvalidTag_IsIgnored()
        {
            // This Lava template would throw an error in the default Fluid parser, but should process successfully here.
            var input = "{% comment %} This comment contains an {% unknown_tag %} {% endcomment %}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( string.Empty, output );
            } );
        }

        /// <summary>
        /// Verify that a comment containing an invalid shortcode does not cause a parser error.
        /// This test validates a Rock-specific change to the Fluid Parser.
        /// </summary>
        [TestMethod]
        public void CommentBlock_ContainingInvalidShortcode_IsIgnored()
        {
            var input = "{% comment %} This comment contains an {[ invalid_shortcode ]} {% endcomment %}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( string.Empty, output );
            } );
        }

        #endregion Comment Block

        #region Shorthand Line Comment

        /// <summary>
        /// This test verifies the standard newline character '\n' as an effective inline comment delimiter.
        /// This is the delimiter used by the Rock text editor, as opposed to the Windows standard '\r\n'
        /// that is implicitly used in templates elsewhere in this test project.
        /// </summary>
        [TestMethod]
        public void ShorthandLineComment_TerminatedbyNewlineOnly_IsTerminatedCorrectly()
        {
            var input = "//- This is a single line comment.\nLine 1";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                // Removing the comment leaves the newline that terminated it.
                Assert.AreEqual( "\nLine 1", output );
            } );
        }

        [TestMethod]
        public void ShorthandLineComment_AsFirstElement_IsIgnored()
        {
            var input = """
                //- This is a single line comment.
                Line 1
                """;

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                // Removing the comment leaves the newline that terminated it.
                Assert.AreEqual( "\nLine 1", output );
            } );
        }

        [TestMethod]
        public void ShorthandLineComment_AfterContent_ReturnsContent()
        {
            var input = """
                Line 1<br>
                Line 2<br>//- This is a single line comment.
                Line 3<br>
                """;

            var expected = """
                Line 1<br>
                Line 2<br>
                Line 3<br>
                """.NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expected, output );
            } );
        }

        [TestMethod]
        public void ShorthandLineComment_ContainingQuotedString_IsRemoved()
        {
            var input = """
                Line 1<br>
                Line 2<br>//-Please enter the following: "//- This is a single line comment." and '//- This is also a single line comment'.
                Line 3<br>
                """;

            var expected = """
                Line 1<br>
                Line 2<br>
                Line 3<br>
                """.NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expected, output );
            } );
        }

        [TestMethod]
        public void ShorthandLineComment_AsFinalElement_RendersCorrectLineContent()
        {
            // Input template terminating with a comment, no new line character.
            var input = """
                Line 1<br>
                //- Lava single line comment
                """;

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                // A trailing line comment leaves both the newline that preceded it
                // and one in place of the comment itself. A trailing block comment
                // leaves only the first - see
                // ShorthandBlockComment_AsFinalElement_RendersCorrectLineContent.
                Assert.AreEqual( "Line 1<br>\n\n", output );
            } );
        }

        [TestMethod]
        public void ShorthandLineComment_WithLeadingWhiteSpace_RendersLineWithWhiteSpace()
        {
            var input = "Line 1\n   //-\nLine 2";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( "Line 1\n   \nLine 2", output );
            } );
        }

        #endregion Shorthand Line Comment

        #region Shorthand Block Comment

        [TestMethod]
        public void ShorthandBlockComment_ContainingQuotedString_IsRemoved()
        {
            /*
                9/26/26 - CLAUDE

                This test carried no [TestMethod] attribute in its previous home, so
                it had never run. The attribute is added here and the expectation
                below is what the parser actually produces.

                Reason: A test that cannot run is not a test.
            */
            var input = """
                Line 1<br>
                Line 2<br>/- Please enter the following:
                "//- This is a single line comment."
                and
                '//- This is also a single line comment'.
                -/
                Line 3<br>
                """;

            var expected = """
                Line 1<br>
                Line 2<br>
                Line 3<br>
                """.NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expected, output );
            } );
        }

        [TestMethod]
        public void ShorthandBlockComment_SpanningMultipleLines_RemovesNewLinesContainedInComment()
        {
            var input = """
                Line 1<br>
                Line 2 Start<br>/- This is a block comment...


                   ... spanning multiple lines. -/Line 2 End<br>
                Line 3<br>
                """;

            var expected = """
                Line 1<br>
                Line 2 Start<br>Line 2 End<br>
                Line 3<br>
                """.NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expected, output );
            } );
        }

        [TestMethod]
        public void ShorthandBlockComment_Inline_RendersCorrectLineContent()
        {
            var input = """
                Line 1<br>
                Line 2 Start<br>/- This is an inline block comment -/Line 2 End<br>
                Line 3<br>
                """;

            var expected = """
                Line 1<br>
                Line 2 Start<br>Line 2 End<br>
                Line 3<br>
                """.NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expected, output );
            } );
        }

        [TestMethod]
        public void ShorthandBlockComment_AsFinalElement_RendersCorrectLineContent()
        {
            // Input template terminating with a comment, no new line character.
            var input = """
                Line 1<br>
                /- Lava block comment -/
                """;

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( "Line 1<br>\n", output );
            } );
        }

        /// <summary>
        /// Verify that a comment containing an invalid tag does not cause a parser error.
        /// This test validates a Rock-specific change to the Fluid Parser.
        /// </summary>
        [TestMethod]
        public void ShorthandBlockComment_ContainingInvalidTag_IsIgnored()
        {
            // This Lava template would throw an error in the default Fluid parser, but should process successfully here.
            var input = "/- This comment contains an {% unknown_tag %} -/";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( string.Empty, output );
            } );
        }

        #endregion Shorthand Block Comment

        #region Comment Placement

        [TestMethod]
        public void ShorthandComment_CommentsAnywhereInLine_AreRemoved()
        {
            // It was discussed on 2025/01/20 how shorthand comments should
            // work. The decision was that they should work "like a programming
            // language works". So just like "//" anywhere in C# code will
            // comment out the remainder of the line, so should the shorthand
            // comments. This does have the side affect that somebody cannot
            // use "/- " or "//- " as legitimate output without workarounds.
            // This is intentional.
            var input = """"
                -- Begin Example --
                Lava Comments can be added as follows:
                For a single line comment, use "//- Single Line Comment 1" or '//- Single Line Comment 2'.
                For a block comment, use "/- Block Comment 1...
                ... like this! -/"
                or '/- Block Comment 2...
                ... like this! -/'
                -- End Example --
                """";

            var expected = """"
                -- Begin Example --
                Lava Comments can be added as follows:
                For a single line comment, use "
                For a block comment, use ""
                or ''
                -- End Example --
                """".NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expected, output );
            } );
        }

        [TestMethod]
        public void ShorthandComment_CommentInRawTag_IsNotRemoved()
        {
            var input = """
                Example Start<br>
                Valid Lava Comment Styles are:
                {% raw %}//- Line Comment: A comment that is confined to a single line.
                or
                /- Block Comment: A comment that can span...
                   ... multiple lines. -/{% endraw %}
                Example End<br>
                """;

            var expected = """
                Example Start<br>
                Valid Lava Comment Styles are:
                //- Line Comment: A comment that is confined to a single line.
                or
                /- Block Comment: A comment that can span...
                   ... multiple lines. -/
                Example End<br>
                """.NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expected, output );
            } );
        }

        /// <summary>
        /// Verify that a Lava shorthand comment embedded in a Lava tag is correctly parsed.
        /// </summary>
        [TestMethod]
        public void ShorthandComment_CommentInLavaTag_IsRemoved()
        {
            var input = """
                Example Start<br>
                {% lava
                    assign var1 = 'Value 1'
                    //- Line Comment: A comment that is confined to a single line.
                    assign var2 = 'Value 2'
                    /- Block Comment: A comment that can span...
                       ... multiple lines. -/
                    assign var3 = 'Value 3'
                %}
                //- Line Comment #2
                {{ var1 }}<br>
                {{ var2 }}<br>
                {{ var3 }}<br>
                Example End<br>
                """;

            // The two blank lines are what the removed {% lava %} tag and the
            // trailing line comment leave behind.
            var expected = """
                Example Start<br>


                Value 1<br>
                Value 2<br>
                Value 3<br>
                Example End<br>
                """.NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expected, output );
            } );
        }

        [TestMethod]
        public void ShorthandComment_CommentsInIncludeFile_AreRemoved()
        {
            var input = "{%- include '_comments.lava' -%}";

            /*
                9/26/26 - CLAUDE

                The blank lines between the first three lines, and their absence
                before "Line 4", record a real difference between the two comment
                styles. A shorthand comment ("//-" or "/- ... -/") is removed but
                the newline that terminated it is not, so each one leaves a blank
                line. The Liquid comment block in this template is written with
                whitespace-trim markers ({%- comment -%}), which consume the
                surrounding newlines, so "Line 4" ends up on the same line as
                "Line 3".

                The predecessor test compared with all whitespace stripped from both
                sides and so asserted nothing about any of this.

                Reason: Record how each comment style actually treats the
                surrounding whitespace.
            */
            var expected = "Line 1<br>\n\nLine 2<br>\n\nLine 3<br>Line 4<br>";

            // The include statement needs a file system to resolve against, so this
            // test renders with its own engine rather than the shared one.
            var engineOptions = new LavaEngineConfigurationOptions
            {
                FileSystem = GetFileProviderWithComments()
            };

            var engine = LavaTestEngineFactory.CreateFluidEngine( engineOptions );

            var output = LavaRenderTestHelper.Render( engine, input );

            Assert.AreEqual( expected, output );
        }

        #endregion Comment Placement

        #region Support Methods

        /// <summary>
        /// Builds a file system containing a template that uses every comment style.
        /// </summary>
        /// <returns>The file provider.</returns>
        private MockFileProvider GetFileProviderWithComments()
        {
            var fileProvider = new MockFileProvider();

            var commentsTemplate = """
                Line 1<br>
                //- Lava single line comment
                Line 2<br>
                /- Lava multi-line
                   comment -/
                Line 3<br>
                {%- comment -%} Liquid comment {%- endcomment -%}
                Line 4<br>
                """;

            fileProvider.Add( "_comments.lava", commentsTemplate );

            return fileProvider;
        }

        #endregion Support Methods
    }
}
