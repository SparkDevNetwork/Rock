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
using Rock.Lava.Fluid;
using Rock.Tests.Shared;

namespace Rock.Tests.Lava
{
    /// <summary>
    /// Tests for the website Lava template cache service.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class WebsiteLavaTemplateCacheServiceTests
    {
        #region Support

        private TestHelper.RockAppScope _rockAppScope;

        /// <summary>
        /// Configures a RockApp with no database so the Rock cache can publish its update messages.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _rockAppScope = TestHelper.CreateScopedRockApp();
        }

        /// <summary>
        /// Restores the previous RockApp configuration.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            // Remove any entries added by the test so they do not leak into other tests.
            ( ( ILavaTemplateCacheService ) new WebsiteLavaTemplateCacheService() ).ClearCache();

            _rockAppScope?.Dispose();
            _rockAppScope = null;
        }

        private static ILavaEngine GetEngine( ILavaTemplateCacheService cacheService )
        {
            var options = new LavaEngineConfigurationOptions
            {
                FileSystem = new WebsiteLavaFileSystem(),
                HostService = new WebsiteLavaHost(),
                CacheService = cacheService,
                TimeZone = RockDateTime.OrgTimeZoneInfo
            };

            var engine = new FluidEngine();
            engine.Initialize( options );

            engine.RegisterFilters( typeof( global::Rock.Lava.Filters.TemplateFilters ) );
            engine.RegisterFilters( typeof( global::Rock.Lava.LavaFilters ) );

            // Start each test with an empty shared template cache.
            cacheService.ClearCache();

            return engine;
        }

        #endregion

        /// <summary>
        /// A cache entry stored under a template's cache key but parsed from different
        /// source text (as happens when two templates collide on the 32-bit hash key)
        /// must not be served for that template.
        /// </summary>
        [TestMethod]
        public void AddOrGetTemplate_CollidingKeyWithDifferentSource_DoesNotServeOtherTemplate()
        {
            ILavaTemplateCacheService cacheService = new WebsiteLavaTemplateCacheService();
            var engine = GetEngine( cacheService );

            var targetTemplate = "TH153 target {{ 'abc' | Upcase }}";
            var targetKey = cacheService.GetCacheKeyForTemplate( targetTemplate );

            // Simulate a hash collision by placing a different template in the target's key slot.
            var markerTemplate = engine.ParseTemplate( "{{ 7 | Plus:7 }}" ).Template;
            cacheService.AddTemplate( markerTemplate, targetKey );

            var output = engine.RenderTemplate( targetTemplate ).Text;

            Assert.AreEqual( "TH153 target ABC", output );
            Assert.AreEqual( 0, cacheService.CacheHits, "A mismatched cache entry was counted as a cache hit." );
            Assert.AreEqual( 1, cacheService.CacheMisses );

            // The replaced entry now belongs to the target template and is served from cache.
            output = engine.RenderTemplate( targetTemplate ).Text;

            Assert.AreEqual( "TH153 target ABC", output );
            Assert.AreEqual( 1, cacheService.CacheHits );
            Assert.AreEqual( 1, cacheService.CacheMisses );
        }

        /// <summary>
        /// A real key collision with no mocking: templates of 10 characters or fewer use their own
        /// text as the cache key, while longer templates use their xxHash decimal string. A short
        /// template whose text equals a long template's hash shares that long template's key, and
        /// each must still render its own output.
        /// </summary>
        [TestMethod]
        public void AddOrGetTemplate_ShortTemplateMatchingLongTemplateHash_DoesNotServeOtherTemplate()
        {
            ILavaTemplateCacheService cacheService = new WebsiteLavaTemplateCacheService();
            var engine = GetEngine( cacheService );

            var longTemplate = "TH153 long {{ 'abc' | Upcase }}";
            var longKey = cacheService.GetCacheKeyForTemplate( longTemplate );
            var shortTemplate = longKey.Substring( longKey.LastIndexOf( ':' ) + 1 );

            Assert.IsTrue( shortTemplate.Length <= 10, $"Unexpected cache key format: {longKey}" );
            Assert.AreEqual( longKey, cacheService.GetCacheKeyForTemplate( shortTemplate ), "The short template does not collide with the long template." );

            var shortOutput = engine.RenderTemplate( cacheService.AddOrGetTemplate( engine, shortTemplate ).Template, new LavaRenderParameters() ).Text;
            var longOutput = engine.RenderTemplate( cacheService.AddOrGetTemplate( engine, longTemplate ).Template, new LavaRenderParameters() ).Text;

            Assert.AreEqual( shortTemplate, shortOutput );
            Assert.AreEqual( "TH153 long ABC", longOutput );

            shortOutput = engine.RenderTemplate( cacheService.AddOrGetTemplate( engine, shortTemplate ).Template, new LavaRenderParameters() ).Text;

            Assert.AreEqual( shortTemplate, shortOutput );
        }

        /// <summary>
        /// A cache entry with no recorded source text must not be trusted, even for a null template.
        /// </summary>
        [TestMethod]
        public void AddOrGetTemplate_EntryWithoutSourceText_IsNotServedForNullTemplate()
        {
            ILavaTemplateCacheService cacheService = new WebsiteLavaTemplateCacheService();
            var engine = GetEngine( cacheService );

            var nullKey = cacheService.GetCacheKeyForTemplate( null );
            var markerTemplate = engine.ParseTemplate( "{{ 7 | Plus:7 }}" ).Template;
            cacheService.AddTemplate( markerTemplate, nullKey );

            var result = cacheService.AddOrGetTemplate( engine, null );

            Assert.AreNotSame( markerTemplate, result.Template );
            Assert.AreEqual( 0, cacheService.CacheHits );
        }

        /// <summary>
        /// A template rendered under an explicit cache key must not return a different
        /// template that was previously cached under the same key.
        /// </summary>
        [TestMethod]
        public void AddOrGetTemplate_SameCacheKeyWithChangedSource_ReturnsNewTemplate()
        {
            ILavaTemplateCacheService cacheService = new WebsiteLavaTemplateCacheService();
            var engine = GetEngine( cacheService );

            var firstOutput = engine.RenderTemplate( "TH153 first {{ 'one' | Upcase }}", new LavaRenderParameters { CacheKey = "TH153:Block:1" } ).Text;
            var secondOutput = engine.RenderTemplate( "TH153 second {{ 'two' | Upcase }}", new LavaRenderParameters { CacheKey = "TH153:Block:1" } ).Text;

            Assert.AreEqual( "TH153 first ONE", firstOutput );
            Assert.AreEqual( "TH153 second TWO", secondOutput );
        }

        /// <summary>
        /// Rendering the same template twice must still be served from the cache the second time.
        /// </summary>
        [TestMethod]
        public void AddOrGetTemplate_IdenticalSource_IsServedFromCache()
        {
            ILavaTemplateCacheService cacheService = new WebsiteLavaTemplateCacheService();
            var engine = GetEngine( cacheService );

            var template = "TH153 repeat {{ 'abc' | Upcase }}";

            var output1 = engine.RenderTemplate( template ).Text;

            Assert.AreEqual( 0, cacheService.CacheHits );
            Assert.AreEqual( 1, cacheService.CacheMisses );

            var output2 = engine.RenderTemplate( template ).Text;

            Assert.AreEqual( "TH153 repeat ABC", output1 );
            Assert.AreEqual( "TH153 repeat ABC", output2 );
            Assert.AreEqual( 1, cacheService.CacheHits );
            Assert.AreEqual( 1, cacheService.CacheMisses );
        }

        /// <summary>
        /// Whitespace templates of different lengths must use separate cache keys and render their own content.
        /// </summary>
        [TestMethod]
        public void AddOrGetTemplate_WhitespaceTemplatesWithDifferentLengths_AreCachedIndependently()
        {
            ILavaTemplateCacheService cacheService = new WebsiteLavaTemplateCacheService();
            var engine = GetEngine( cacheService );

            var input0 = string.Empty;
            var input1 = new string( ' ', 1 );
            var input9 = new string( ' ', 9 );

            var key0 = cacheService.GetCacheKeyForTemplate( input0 );
            var key1 = cacheService.GetCacheKeyForTemplate( input1 );
            var key9 = cacheService.GetCacheKeyForTemplate( input9 );

            Assert.AreNotEqual( key0, key1 );
            Assert.AreNotEqual( key1, key9 );

            Assert.AreEqual( input0, engine.RenderTemplate( input0 ).Text );
            Assert.AreEqual( input1, engine.RenderTemplate( input1 ).Text );
            Assert.AreEqual( input9, engine.RenderTemplate( input9 ).Text );

            Assert.IsTrue( cacheService.ContainsKey( key1 ) );
            Assert.IsTrue( cacheService.ContainsKey( key9 ) );
        }

        /// <summary>
        /// The cache key for longer templates is a 32-bit hash. The key format is unchanged
        /// by the source text check, which is what makes collisions harmless.
        /// </summary>
        [TestMethod]
        public void GetCacheKeyForTemplate_LongTemplate_UsesThirtyTwoBitHashKey()
        {
            ILavaTemplateCacheService cacheService = new WebsiteLavaTemplateCacheService();
            GetEngine( cacheService );

            var key = cacheService.GetCacheKeyForTemplate( "TH153 target {{ 'abc' | Upcase }}" );
            var hashPart = key.Substring( key.LastIndexOf( ':' ) + 1 );

            Assert.IsTrue( uint.TryParse( hashPart, out _ ), $"Unexpected cache key format: {key}" );
        }

#pragma warning disable CS0612, CS0618
        /// <summary>
        /// The obsolete Get method must not return an entry parsed from different source text.
        /// </summary>
        [TestMethod]
        public void Get_CollidingKeyWithDifferentSource_DoesNotServeOtherTemplate()
        {
            var cacheService = new WebsiteLavaTemplateCacheService();
            var engine = GetEngine( cacheService );

            var key = "TH153:Obsolete:1";
            var markerTemplate = engine.ParseTemplate( "{{ 7 | Plus:7 }}" ).Template;
            ( ( ILavaTemplateCacheService ) cacheService ).AddTemplate( markerTemplate, key );

            var content = "TH153 obsolete {{ 'abc' | Upcase }}";
            var cacheEntry = cacheService.Get( engine, key, content );

            Assert.AreEqual( content, cacheEntry.SourceText );
            Assert.AreEqual( "TH153 obsolete ABC", engine.RenderTemplate( cacheEntry.Template, new LavaRenderParameters() ).Text );
        }
#pragma warning restore CS0612, CS0618
    }
}
