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
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Lava
{
    [TestClass]
    public class LavaConfigurationTests : LavaIntegrationTestBase
    {
        #region Configuration: DefaultEntityCommands

        /// <summary>
        /// Verify that templates with varying amounts of whitespace are correctly cached and return the expected output.
        /// </summary>
        [TestMethod]
        public void Configuration_SetDefaultEntityCommandExecute_IsEnabledForNewDefaultContext()
        {
            var options = new LavaEngineConfigurationOptions
            {
                DefaultEnabledCommands = new List<string> { "Execute" }
            };

            TestHelper.AssertAction( ( engine ) =>
            {
                var testEngine = LavaEngine.NewEngineInstance( engine.EngineType, options );

                var context = testEngine.NewRenderContext();

                var enabledCommands = context.GetEnabledCommands();

                Assert.That.Contains( enabledCommands, "Execute" );
            } );
        }

        #endregion

        #region Caching

        [TestMethod]
        public void WebsiteLavaTemplateCacheService_WhitespaceTemplatesWithDifferentLengths_AreCachedIndependently_Pass2()
        {
            WebsiteLavaTemplateCacheService_WhitespaceTemplatesWithDifferentLengths_AreCachedIndependently();
        }

        /// <summary>
        /// Verify that templates with varying amounts of whitespace are correctly cached and return the expected output.
        /// </summary>
        [TestMethod]
        public void WebsiteLavaTemplateCacheService_WhitespaceTemplatesWithDifferentLengths_AreCachedIndependently()
        {
            var options = new LavaEngineConfigurationOptions();

            var cacheService = new WebsiteLavaTemplateCache() as ILavaTemplateCacheService;

            cacheService.ClearCache();

            options.CacheService = cacheService;

            LavaEngine.Initialize( LavaEngine.CurrentEngine.EngineType, options );

            // Process a zero-length whitespace template - this should be cached separately.
            var input0 = string.Empty;

            // Verify that the template does not initially exist in the cache.
            var exists = cacheService.ContainsTemplate( input0 );

            Assert.IsFalse( exists, "String-0 Template found in cache unexpectedly." );

            // Render the template, which will automatically add it to the cache.
            var output0 = LavaEngine.CurrentEngine.RenderTemplate( input0 );

            // Verify that the template now exists in the cache.
            exists = cacheService.ContainsTemplate( input0 );

            Assert.IsTrue( exists, "String-0 Template not found in cache." );

            // Render a whitespace template of a different length - this should be cached separately from the first template.
            // If not, the caching mechanism would cause some whitespace to be rendered incorrectly.
            var input1 = new string( ' ', 1 );

            var output1 = LavaEngine.CurrentEngine.RenderTemplate( input1 );

            // Verify that the 100-character whitespace template now exists in the cache.
            exists = cacheService.ContainsTemplate( input1 );

            Assert.IsTrue( exists, "String-1 Template not found in cache." );

            // Verify that a whitespace template of some other length is not equated with the whitespace templates we have specifically added.
            exists = cacheService.ContainsTemplate( new string( ' ', 9 ) );

            Assert.IsFalse( exists, "String-9 Template found in cache unexpectedly." );
        }

        /// <summary>
        /// Verify that templates with varying amounts of whitespace are correctly cached and return the expected output.
        /// </summary>
        [TestMethod]
        public void ShortcodeCaching_ModifiedShortcode_ReturnsCorrectVersionAfterModification()
        {
            //LavaShortcode lavaShortcode;

            var rockContext = new RockContext();
            var lavaShortCodeService = new LavaShortcodeService( rockContext );

            // Create a new Shortcode.
            var shortcodeGuid1 = TestGuids.Shortcodes.ShortcodeTest1.AsGuid();

            var lavaShortcode = lavaShortCodeService.Queryable().FirstOrDefault( x => x.Guid == shortcodeGuid1 );

            if ( lavaShortcode == null )
            {
                lavaShortcode = new LavaShortcode();

                lavaShortCodeService.Add( lavaShortcode );
            }

            lavaShortcode.Guid = shortcodeGuid1;
            lavaShortcode.TagName = "TestShortcode1";
            lavaShortcode.Name = "Test Shortcode 1";
            lavaShortcode.IsActive = true;
            lavaShortcode.Description = "Test shortcode";
            //lavaShortcode.Documentation = htmlDocumentation.Text;
            lavaShortcode.TagType = TagType.Inline;

            lavaShortcode.Markup = "Hello!";
            //lavaShortcode.Parameters = kvlParameters.Value;
            //lavaShortcode.EnabledLavaCommands = String.Join( ",", lcpLavaCommands.SelectedLavaCommands );

            rockContext.SaveChanges();

            LavaEngine.CurrentEngine.RegisterDynamicShortcode( "TestShortcode1",
                ( shortcodeName ) => WebsiteLavaShortcodeProvider.GetShortcodeDefinition( shortcodeName ) );

            // Resolve a template using the new shortcode and verify the result.
            LavaEngine.CurrentEngine.ClearTemplateCache();

            LavaShortcodeCache.Clear();

            TestHelper.AssertTemplateOutput( "Hello!", "{[ testshortcode1 ]}" );

            lavaShortcode.Markup = "Goodbye!";

            rockContext.SaveChanges();

            LavaShortcodeCache.Clear();

            LavaEngine.CurrentEngine.ClearTemplateCache();

            TestHelper.AssertTemplateOutput( "Goodbye!", "{[ testshortcode1 ]}" );
        }

        #endregion
    }
}
