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

using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Model
{
    [TestClass]
    public class NoteServiceTests
    {
        private const string EditorMention = "<span class=\"mention\" data-identifier=\"Wqx8yPkAez\">@Ted Decker</span>";

        [TestMethod]
        [DataRow( "First line\n" + EditorMention + " said Tom &amp; Jerry say 3 &lt; 5&nbsp;\nLast line", "First line\n" + EditorMention + " said Tom &amp; Jerry say 3 &lt; 5&nbsp;\nLast line", DisplayName = "Editor output is unchanged" )]
        [DataRow( "Plain text with no markup", "Plain text with no markup", DisplayName = "Plain text is unchanged" )]
        [DataRow( "Tom & Jerry", "Tom &amp; Jerry", DisplayName = "Bare ampersand is encoded" )]
        [DataRow( "<h1>Heading</h1>", "Heading", DisplayName = "Heading is reduced to its text" )]
        [DataRow( "<a href=\"https://www.rockrms.com\">Rock</a>", "Rock", DisplayName = "Link is reduced to its text" )]
        [DataRow( EditorMention, EditorMention, DisplayName = "Mention is unchanged" )]
        [DataRow( "<span class=\"mention\" data-identifier=\"Wqx8yPkAez\" onclick=\"alert(1)\" style=\"color: red\">@Ted Decker</span>", EditorMention, DisplayName = "Mention keeps only class and data-identifier" )]
        [DataRow( "<img src=\"x\" onerror=\"alert(1)\">", "", DisplayName = "Image is removed" )]
        [DataRow( "<script>alert(1)</script>after", "after", DisplayName = "Script is removed with its content" )]
        [DataRow( "<b style=\"color: red\">bold</b>", "bold", DisplayName = "Bold is reduced to its text" )]
        [DataRow( "<div class=\"x\">one</div><br style=\"x\">two", "<div>one</div><br>two", DisplayName = "Div and br keep no attributes" )]
        [DataRow( "<div onclick=\"alert(1)\">one</div>", "<div>one</div>", DisplayName = "Event handler is removed from an allowed tag" )]
        [DataRow( "<style>body{display:none}</style>after", "after", DisplayName = "Style is removed with its content" )]
        [DataRow( "<iframe src=\"https://www.rockrms.com\"></iframe>after", "after", DisplayName = "Iframe is removed" )]
        [DataRow( "<span class=\"mention\" data-identifier=\"a&quot; onclick=&quot;alert(1)\">@X</span>", "<span class=\"mention\" data-identifier=\"a&quot; onclick=&quot;alert(1)\">@X</span>", DisplayName = "Mention identifier cannot break out of its quotes" )]
        [DataRow( "", "", DisplayName = "Empty is unchanged" )]
        [DataRow( null, null, DisplayName = "Null is unchanged" )]
        public void SanitizeNoteText_ReturnsExpectedText( string text, string expected )
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                Assert.AreEqual( expected, NoteService.SanitizeNoteText( text ) );
            }
        }

        [TestMethod]
        public void SanitizeNoteText_WithEditorMentions_KeepsMentionKeys()
        {
            var text = "Hi " + EditorMention + "\n\n<span class=\"mention\" data-identifier=\"Kd93mXpQr2\">@Cindy Decker</span> thanks";

            using ( TestHelper.CreateScopedRockApp() )
            {
                var expected = NoteService.GetMentionIdKeysFromContent( text );
                var actual = NoteService.GetMentionIdKeysFromContent( NoteService.SanitizeNoteText( text ) );

                CollectionAssert.AreEqual( new[] { "Wqx8yPkAez", "Kd93mXpQr2" }, expected );
                CollectionAssert.AreEqual( expected, actual );
            }
        }
    }
}
