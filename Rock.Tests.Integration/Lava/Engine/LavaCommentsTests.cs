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
using DotLiquid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Lava;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Lava
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

        [TestMethod]
        public void RemoveLavaCommentsReturnsEmptyStringForNullInput()
        {
            var actualResult = LavaHelper.RemoveLavaComments( null );

            Assert.That.AreEqual( string.Empty, actualResult );
        }

        [TestMethod]
        public void LavaHelperRemoveComments_LineCommentAfterContent_ReturnsContent()
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

            var output = templateUncommented.ResolveMergeFields( null );

            Assert.That.AreEqual( expectedOutput, output );
        }

        [TestMethod]
        public void LavaHelperRemoveComments_CommentInStringLiteral_IsNotRemoved()
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

            var output = templateUncommented.ResolveMergeFields( null );

            Assert.That.AreEqual( expectedOutput, output );
        }


        [TestMethod]
        public void LavaHelperRemoveComments_LineCommentContainingQuotedString_IsRemoved()
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

            var output = templateUncommented.ResolveMergeFields( null );

            Assert.That.AreEqual( expectedOutput, output );
        }

        public void LavaHelperRemoveComments_BlockCommentContainingQuotedString_IsRemoved()
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

            var output = templateUncommented.ResolveMergeFields( null );

            Assert.That.AreEqual( expectedOutput, output );
        }

        [TestMethod]
        public void LavaHelperRemoveComments_CommentInRawTag_IsNotRemoved()
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

            var templateUncommented = LavaHelper.RemoveLavaComments( input );

            var output = templateUncommented.ResolveMergeFields( null );

            Assert.That.AreEqual( expectedOutput, output );
        }
        [TestMethod]
        public void LavaHelperRemoveComments_BlockCommentSpanningMultipleLines_RemovesNewLinesContainedInComment()
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

            var output = templateUncommented.ResolveMergeFields( null );

            Assert.That.AreEqual( expectedOutput, output );
        }

        [TestMethod]
        public void LavaHelperRemoveComments_BlockCommentInline_RendersCorrectLineContent()
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

            var output = templateUncommented.ResolveMergeFields( null );

            Assert.That.AreEqual( expectedOutput, output );
        }
    }
}
