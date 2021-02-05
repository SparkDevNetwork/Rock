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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Web.Cache;

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

            TestHelper.LavaEngine.RegisterDynamicShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

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

            TestHelper.AssertTemplateOutputWithWildcard( expectedOutput, input, context, ignoreWhiteSpace: true, wildCard: "<?>" );
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

        [TestMethod]
        public void Shortcode_WithEmbeddedCommandHavingInsufficientPermission_FailsWithError()
        {
            ConfigureTestShortcodeWithEmbeddedSqlCommand( "testshortcodesql", shortcodeGrantsSqlPermission: false );

            TestHelper.AssertTemplateOutput( "The Lava command 'sql' is not configured for this template.", "{[ testshortcodesql ]}", ignoreWhitespace: true );
        }

        [TestMethod]
        public void Shortcode_WithEmbeddedCommandHavingSufficientPermission_RendersExpectedOutput()
        {
            ConfigureTestShortcodeWithEmbeddedSqlCommand( "testshortcodesql", shortcodeGrantsSqlPermission: true );

            TestHelper.AssertTemplateOutput( "Cindy Decker<br>Ted Decker<br>", "{[ testshortcodesql ]}", ignoreWhitespace: true );
        }

        private void ConfigureTestShortcodeWithEmbeddedSqlCommand( string shortcodeName, bool shortcodeGrantsSqlPermission )
        {
            var rockContext = new RockContext();
            var lavaShortCodeService = new LavaShortcodeService( rockContext );

            // Create a new Shortcode.
            var shortcodeGuid1 = TestGuids.Shortcodes.ShortcodeTestSql1.AsGuid();

            var lavaShortcode = lavaShortCodeService.Queryable().FirstOrDefault( x => x.Guid == shortcodeGuid1 );

            if ( lavaShortcode == null )
            {
                lavaShortcode = new LavaShortcode();

                lavaShortCodeService.Add( lavaShortcode );
            }

            var shortcodeTemplate = @"
{% sql %}
   SELECT [NickName],[LastName] FROM [Person]
   WHERE [LastName] = 'Decker' AND [NickName] IN ('Ted','Cindy')
   ORDER BY [NickName]
{% endsql %}
{% for item in results %}
   {{ item.NickName }} {{ item.LastName }}<br>
{% endfor %}
";
            lavaShortcode.Guid = shortcodeGuid1;
            lavaShortcode.TagName = shortcodeName; // "TestShortcodeSql";
            lavaShortcode.Name = shortcodeName; // "Test Shortcode Sql";
            lavaShortcode.IsActive = true;
            lavaShortcode.Description = "Test shortcode.";
            lavaShortcode.TagType = TagType.Inline;

            lavaShortcode.Markup = shortcodeTemplate;

            if ( shortcodeGrantsSqlPermission )
            {
                lavaShortcode.EnabledLavaCommands = "sql,execute";
            }
            else
            {
                lavaShortcode.EnabledLavaCommands = null;
            }

            rockContext.SaveChanges();

            LavaEngine.CurrentEngine.RegisterDynamicShortcode( "TestShortcodeSql",
                ( name ) => WebsiteLavaShortcodeProvider.GetShortcodeDefinition( name ) );

            // Clear caches to ensure that the updated shortcode definition  is loaded.
            LavaEngine.CurrentEngine.ClearTemplateCache();

            LavaShortcodeCache.Clear();

            // NOTE: There appears to be a caching issue here - the updated version of the shortcode is not always returned
            // until the test is run a second time.
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

            TestHelper.AssertTemplateOutput( expectedOutput, input, ignoreWhitespace: true );
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
