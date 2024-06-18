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
using Rock.Tests.Shared;

namespace Rock.Tests.Rock.Lava
{
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class LavaHelperTests
    {
        [TestMethod]
        public void RemoveLavaComments_NullInput_ReturnsEmptyString()
        {
            var actualResult = LavaHelper.RemoveLavaComments( null );

            Assert.That.AreEqual( string.Empty, actualResult );
        }

        [TestMethod]
        public void RemoveLavaComments_LineCommentAfterContent_ReturnsContent()
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

            var templateUncommented = LavaHelper.RemoveLavaComments( input );

            Assert.That.AreEqual( expectedOutput, templateUncommented );
        }

        [TestMethod]
        public void RemoveLavaComments_CommentInSingleLineStringLiteral_IsNotRemoved()
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

            var templateUncommented = LavaHelper.RemoveLavaComments( input );

            Assert.That.AreEqual( expectedOutput, templateUncommented );
        }

        [TestMethod]
        public void RemoveLavaComments_LineCommentContainingQuotedString_IsRemoved()
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

            var templateUncommented = LavaHelper.RemoveLavaComments( input );

            Assert.That.AreEqual( expectedOutput, templateUncommented );
        }

        /// <summary>
        /// Verify that single line comments enclosed in quoted text are removed
        /// if the enclosed content spans multiple lines.
        /// </summary>
        /// <remarks>
        /// Fixes Issue #4975. (refer https://github.com/SparkDevNetwork/Rock/issues/4975)
        /// </remarks>
        [TestMethod]
        public void RemoveLavaComments_LineCommentInMultlineStringLiteral_IsRemoved()
        {
            var input = @"
/- The following lines contain single-line comments enclosed in quoted text spanning multiple lines.
   These comments should be removed to avoid exposing them unintentionally as output. -/
Insert 1st Person's Name Here.<br>//- This is single line comment 1.
Insert 2nd Person's Name Here.<br>//- This is single line comment 2.
A famous (mis)quote: ""That's one small step for man,//- (should be *a* man)
one giant leap for mankind.""
/- The following lines contains literal text that includes a comment token.
   However, it should not be regarded as a comment because it is not terminated by a newline. -/
{% assign stringIncludingComment = '//- This is literal text.' %}
";

            var expectedOutput = @"
Insert 1st Person's Name Here.<br>
Insert 2nd Person's Name Here.<br>
A famous (mis)quote: ""That's one small step for man,
one giant leap for mankind.""
{% assign stringIncludingComment = '//- This is literal text.' %}
";

            var templateUncommented = LavaHelper.RemoveLavaComments( input );

            Assert.That.AreEqual( expectedOutput, templateUncommented );
        }

        public void RemoveLavaCommentsBlockCommentContainingQuotedStringIsRemoved()
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

            var templateUncommented = LavaHelper.RemoveLavaComments( input );

            Assert.That.AreEqual( expectedOutput, templateUncommented );
        }

        [TestMethod]
        public void RemoveLavaComments_CommentInRawTag_IsNotRemoved()
        {
            var input = @"
//- Line Comment: A comment that is confined to a single line.
/- Block Comment: A comment that can span...
   ... multiple lines. -/
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
{% raw %}//- Line Comment: A comment that is confined to a single line.
or
/- Block Comment: A comment that can span...
   ... multiple lines. -/{% endraw %}
Example End<br>
";

            var templateUncommented = LavaHelper.RemoveLavaComments( input );

            Assert.That.AreEqual( expectedOutput, templateUncommented );
        }

        [TestMethod]
        public void RemoveLavaComments_BlockCommentSpanningMultipleLines_RemovesNewLinesContainedInComment()
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

            var templateUncommented = LavaHelper.RemoveLavaComments( input );

            Assert.That.AreEqual( expectedOutput, templateUncommented );
        }

        [TestMethod]
        public void RemoveLavaComments_BlockCommentInline_RendersCorrectLineContent()
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

            var templateUncommented = LavaHelper.RemoveLavaComments( input );

            Assert.That.AreEqual( expectedOutput, templateUncommented );
        }

        [TestMethod]
        public void IsLavaTemplate_HtmlWithComment_RendersTrue()
        {
            var input = @"
Line 1<br>
Line 2 Start<br>/- This is an inline block comment -/Line 2 End<br>
Line 3<br>
";

            var isTemplate = LavaHelper.IsLavaTemplate( input );

            Assert.That.AreEqual( true, isTemplate );
        }

        [TestMethod]
        public void IsLavaTemplate_TextWithDoubleQuotedComment_ReturnsFalse()
        {
            // In Liquid, a string must be surrounded by a pair of double quotes or single quotes: "the 'quoted' text" or 'the "quoted" text'.
            var input = @"
Line 1<br>
Line 2<br>Here is a sample comment: ""/- This is a double-quoted inline comment with an internal 'quote' -/""<br>
Line 3<br>
";

            var isTemplate = LavaHelper.IsLavaTemplate( input );

            Assert.That.AreEqual( false, isTemplate );
        }

        [TestMethod]
        public void IsLavaTemplate_TextWithSingleQuotedComment_ReturnsFalse()
        {
            // In Liquid, a string must be surrounded by a pair of double quotes or single quotes: "the 'quoted' text" or 'the "quoted" text'.
            var input = @"
Line 1<br>
Line 2<br>Here is a sample comment: '/- This is a quoted inline comment with an internal ""quote"" -/'<br>
Line 3<br>

";

            var isTemplate = LavaHelper.IsLavaTemplate( input );

            Assert.That.AreEqual( false, isTemplate );
        }

        [TestMethod]
        public void IsLavaTemplate_CommentContainingQuotedText_ReturnsTrue()
        {
            var input = @"
Line 1<br>
Line 2<br> /- I said ""This comment contains some quoted text"" -/<br>
Line 3<br>
";

            var isTemplate = LavaHelper.IsLavaTemplate( input );

            Assert.That.AreEqual( true, isTemplate );
        }

        [TestMethod]
        public void IsLavaTemplate_SingleLavaTag_ReturnsTrue()
        {
            // The Lava/Liquid tag can span multiple lines, so we need to ensure that the open/close tokens are correctly detected.
            var template = @"
{% lava 
   echo ""Hello World"" %}
";

            Assert.That.AreEqual( true, LavaHelper.IsLavaTemplate( template ) );
        }

        [TestMethod]
        public void IsLavaTemplate_UnmatchedTag_ReturnsFalse()
        {
            Assert.That.AreEqual( false, LavaHelper.IsLavaTemplate( "{% This is not a Lava template." ) );
            Assert.That.AreEqual( false, LavaHelper.IsLavaTemplate( "... nor is this %}" ) );
        }
    }
}
