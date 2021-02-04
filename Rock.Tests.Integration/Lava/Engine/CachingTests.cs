using System.Linq;
using DotLiquid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Lava
{
    [TestClass]
    public class LavaCachingTests : LavaIntegrationTestBase
    {
        #region Caching

        //public class LavaTemplateCacheService : ILavaTemplateCacheService
        //{
        //    public void ClearCache()
        //    {
        //        throw new System.NotImplementedException();
        //    }

        //    public bool GetOrAddTemplate( string content, System.Func<ILavaTemplate> itemFactory, out ILavaTemplate template )
        //    {
        //        throw new System.NotImplementedException();
        //    }
        //}

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

            var cacheService = new LavaTemplateCache() as ILavaTemplateCacheService;

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
            //LavaEngine.CurrentEngine.TryRender( "{[ testshortcode1 ]}", out templateOutput );

            //AssertTemplateOutput( "Goodbye!", templateOutput );
        }

        private void AssertTemplateOutput( string expectedOutput, string templateContent )
        {
            //r 
            //LavaEngine.CurrentEngine.Initialize()
            // Get access to the private GetTemplate method so we can use the internal Lava template caching mechanism.
            var GetTemplateMethodsInfo = typeof( ExtensionMethods ).GetMethod( "GetTemplate", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic );

            var template = GetTemplateMethodsInfo.Invoke( null, new object[] { templateContent } ) as ILavaTemplate;

            var output = template.Render();

            // Verify that the rendered template matches the expected output.
            Assert.AreEqual( expectedOutput, output, "Template Output does not match expected output." );
        }
        #endregion
    }
}
