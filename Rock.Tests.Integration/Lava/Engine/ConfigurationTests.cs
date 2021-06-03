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
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;

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

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var testEngine = LavaService.NewEngineInstance( engine.EngineType, options );

                var context = testEngine.NewRenderContext();

                var enabledCommands = context.GetEnabledCommands();

                Assert.That.Contains( enabledCommands, "Execute" );
            } );
        }

        #endregion

        #region Caching

        /// <summary>
        /// Verify that templates with varying amounts of whitespace are correctly cached and return the expected output.
        /// </summary>
        [TestMethod]
        public void WebsiteLavaTemplateCacheService_WhitespaceTemplatesWithDifferentLengths_AreCachedIndependently()
        {
            var options = new LavaEngineConfigurationOptions();

            ILavaTemplateCacheService cacheService = new WebsiteLavaTemplateCacheService();

            options.CacheService = cacheService;

            TestHelper.ExecuteForActiveEngines( ( defaultEngineInstance ) =>
            {
                // Remove all existing items from the cache.
                cacheService.ClearCache();

                var engine = LavaService.NewEngineInstance( defaultEngineInstance.EngineType, options );

                // Process a zero-length whitespace template - this should be cached separately.
                var input0 = string.Empty;
                var key0 = cacheService.GetCacheKeyForTemplate( input0 );

                // Verify that the template does not initially exist in the cache.
                var exists = cacheService.ContainsKey( key0 );

                Assert.IsFalse( exists, "String-0 Template found in cache unexpectedly." );

                // Render the template, which will automatically add it to the cache.
                var output0 = engine.RenderTemplate( input0 );

                // Verify that the template now exists in the cache.
                exists = cacheService.ContainsKey( key0 );

                Assert.IsTrue( exists, "String-0 Template not found in cache." );

                // Render a whitespace template of a different length - this should be cached separately from the first template.
                // If not, the caching mechanism would cause some whitespace to be rendered incorrectly.
                var input1 = new string( ' ', 1 );
                var key1 = engine.TemplateCacheService.GetCacheKeyForTemplate( input1 );

                var output1 = engine.RenderTemplate( input1 );

                // Verify that the 100-character whitespace template now exists in the cache.
                exists = cacheService.ContainsKey( key1 );

                Assert.IsTrue( exists, "String-1 Template not found in cache." );

                // Verify that a whitespace template of some other length is not equated with the whitespace templates we have specifically added.
                var keyX = engine.TemplateCacheService.GetCacheKeyForTemplate( new string( ' ', 9 ) );

                exists = cacheService.ContainsKey( keyX );

                Assert.IsFalse( exists, "String-9 Template found in cache unexpectedly." );
            } );
        }

        /// <summary>
        /// Verify that cached shortcode definitions are refreshed after updates.
        /// </summary>
        [TestMethod]
        public void WebsiteLavaShortcodeProvider_ModifiedShortcode_ReturnsCorrectVersionAfterModification()
        {
            var options = new LavaEngineConfigurationOptions();

            ILavaTemplateCacheService cacheService = new WebsiteLavaTemplateCacheService();

            options.CacheService = cacheService;

            TestHelper.ExecuteForActiveEngines( ( defaultEngineInstance ) =>
            {
                if ( defaultEngineInstance.EngineType == LavaEngineTypeSpecifier.DotLiquid )
                {
                    Debug.Write( "Shortcode caching is not currently implemented for DotLiquid." );
                    return;
                }

                var engine = LavaService.NewEngineInstance( defaultEngineInstance.EngineType, options );

                var shortcodeProvider = new TestLavaDynamicShortcodeProvider();

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
                lavaShortcode.TagType = TagType.Inline;

                lavaShortcode.Markup = "Hello!";

                rockContext.SaveChanges();

                shortcodeProvider.RegisterShortcode( engine, lavaShortcode );

                // Resolve a template using the new shortcode and verify the result.
                engine.ClearTemplateCache();

                shortcodeProvider.ClearCache();

                LavaService.SetCurrentEngine( engine );

                TestHelper.AssertTemplateOutput( engine, "Hello!", "{[ TestShortcode1 ]}" );

                lavaShortcode.Markup = "Goodbye!";

                rockContext.SaveChanges();

                shortcodeProvider.ClearCache();

                engine.ClearTemplateCache();

                TestHelper.AssertTemplateOutput( engine, "Goodbye!", "{[ TestShortcode1 ]}" );
            } );
        }

        #endregion

        #region Support classes

        /// <summary>
        /// Returns the definition of Lava shortcodes stored in the current Rock database.
        /// This implementation uses a website-based data caching model.
        /// </summary>
        public class TestLavaDynamicShortcodeProvider
        {
            private Dictionary<string, DynamicShortcodeDefinition> _cachedShortcodes = new Dictionary<string, DynamicShortcodeDefinition>( StringComparer.OrdinalIgnoreCase );

            /// <summary>
            /// Register the specified shortcodes with the Lava Engine.
            /// </summary>
            /// <param name="engine"></param>
            public void RegisterShortcode( ILavaEngine engine, DynamicShortcodeDefinition shortcode )
            {
                engine.RegisterShortcode( shortcode.Name, ( shortcodeName ) => GetShortcodeDefinition( shortcodeName ) );
            }

            /// <summary>
            /// Register the specified shortcodes with the Lava Engine.
            /// </summary>
            /// <param name="engine"></param>
            public void RegisterShortcode( ILavaEngine engine, LavaShortcode shortcode )
            {
                engine.RegisterShortcode( shortcode.TagName, ( shortcodeName ) => GetShortcodeDefinition( shortcodeName ) );
            }

            /// <summary>
            /// Clears all of the entries from the shortcode cache.
            /// </summary>
            public void ClearCache()
            {
                _cachedShortcodes.Clear();
            }

            /// <summary>
            /// Gets a shortcode definition for the specified shortcode name.
            /// </summary>
            /// <param name="shortcodeName"></param>
            /// <returns></returns>
            public DynamicShortcodeDefinition GetShortcodeDefinition( string shortcodeName )
            {
                if ( _cachedShortcodes.ContainsKey( shortcodeName ) )
                {
                    return _cachedShortcodes[shortcodeName];
                }

                // Get the shortcode and add it to the cache.
                var rockContext = new RockContext();

                var lavaShortcodeService = new LavaShortcodeService( rockContext );

                var shortcode = lavaShortcodeService.Queryable().Where( c => c.TagName == shortcodeName ).FirstOrDefault();

                var shortcodeDefinition = GetShortcodeDefinitionFromShortcodeEntity( shortcode );

                if ( shortcode != null )
                {
                    _cachedShortcodes.Add( shortcode.TagName, shortcodeDefinition );
                }

                return shortcodeDefinition;
            }

            private DynamicShortcodeDefinition GetShortcodeDefinitionFromShortcodeEntity( LavaShortcode shortcode )
            {
                if ( shortcode == null )
                {
                    return null;
                }

                var newShortcode = new DynamicShortcodeDefinition();

                newShortcode.Name = shortcode.Name;
                newShortcode.TemplateMarkup = shortcode.Markup;

                var parameters = RockSerializableDictionary.FromUriEncodedString( shortcode.Parameters );

                newShortcode.Parameters = new Dictionary<string, string>( parameters.Dictionary );

                newShortcode.EnabledLavaCommands = shortcode.EnabledLavaCommands.SplitDelimitedValues( ",", StringSplitOptions.RemoveEmptyEntries ).ToList();

                if ( shortcode.TagType == TagType.Block )
                {
                    newShortcode.ElementType = LavaShortcodeTypeSpecifier.Block;
                }
                else
                {
                    newShortcode.ElementType = LavaShortcodeTypeSpecifier.Inline;
                }

                return newShortcode;
            }
        }

        #endregion
    }
}
