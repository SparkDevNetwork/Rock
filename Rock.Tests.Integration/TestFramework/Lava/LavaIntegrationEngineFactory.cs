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
using System.Linq;

using Rock.Model;
using Rock.Lava;
using Rock.Tests.Lava.Shared;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.TestFramework.Lava
{
    /// <summary>
    /// Builds the Lava engines that integration tests render against.
    /// </summary>
    /// <remarks>
    /// The component discovery is shared with the unit tests through
    /// <see cref="LavaTestEngineFactory"/>. What an integration test adds is the
    /// shortcodes that are defined as LavaShortcode rows, which only exist once
    /// there is a database to read them from.
    /// </remarks>
    public static class LavaIntegrationEngineFactory
    {
        #region Methods

        /// <summary>
        /// Points <see cref="LavaRenderTestHelper"/> at engines configured for
        /// integration tests. This is called once while the test environment is
        /// being initialized; the engines themselves are built per test.
        /// </summary>
        public static void RegisterEngineFactory()
        {
            LavaRenderTestHelper.ActiveEngineFactory = CreateActiveEngines;
        }

        /// <summary>
        /// Builds the engines a single test renders against.
        /// </summary>
        /// <returns>The engines to render against.</returns>
        public static IEnumerable<ILavaEngine> CreateActiveEngines()
        {
            return new List<ILavaEngine> { CreateFluidEngine() };
        }

        /// <summary>
        /// Builds a Fluid engine configured the way an integration test needs it.
        /// </summary>
        /// <returns>The configured engine.</returns>
        public static ILavaEngine CreateFluidEngine()
        {
            return CreateFluidEngine( shouldRegisterDynamicShortcodes: true );
        }

        /// <summary>
        /// Builds a Fluid engine configured the way an integration test needs it.
        /// </summary>
        /// <param name="shouldRegisterDynamicShortcodes">
        /// Whether to register the shortcodes defined as LavaShortcode rows. This
        /// is false for the callers that build an engine before the database has
        /// been populated, such as the sample data loader.
        /// </param>
        /// <returns>The configured engine.</returns>
        public static ILavaEngine CreateFluidEngine( bool shouldRegisterDynamicShortcodes )
        {
            var engineOptions = new LavaEngineConfigurationOptions
            {
                ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput,
                FileSystem = new MockFileProvider(),
                CacheService = new NullTemplateCacheService(),
                HostService = new WebsiteLavaHost()
            };

            Action<ILavaEngine> configureEngine = shouldRegisterDynamicShortcodes
                ? ( Action<ILavaEngine> ) RegisterDynamicShortcodes
                : null;

            return LavaTestEngineFactory.CreateFluidEngine( engineOptions, configureEngine );
        }

        /// <summary>
        /// Builds an engine and installs it as the current one, for code that
        /// resolves the engine from <see cref="LavaService"/> rather than being
        /// given one.
        /// </summary>
        /// <remarks>
        /// A test gets a fresh engine per test through
        /// <see cref="LavaRenderTestHelper"/>. This is for the code that runs
        /// outside a test, while a database container is being prepared, where
        /// Lava has to be usable before any test starts.
        /// </remarks>
        /// <param name="shouldRegisterDynamicShortcodes">Whether to register the shortcodes defined as LavaShortcode rows.</param>
        /// <returns>The engine that was installed.</returns>
        public static ILavaEngine InitializeCurrentEngine( bool shouldRegisterDynamicShortcodes )
        {
            var engine = CreateFluidEngine( shouldRegisterDynamicShortcodes );

            LavaService.SetCurrentEngine( engine );

            return engine;
        }

        /// <summary>
        /// Registers the shortcodes that are defined as LavaShortcode rows.
        /// </summary>
        /// <remarks>
        /// Each shortcode is registered with a factory rather than a definition so
        /// that the current row is read from the cache every time the shortcode is
        /// used. A test that edits a shortcode then renders it sees its own edit.
        /// </remarks>
        /// <param name="engine">The engine to register against.</param>
        public static void RegisterDynamicShortcodes( ILavaEngine engine )
        {
            Func<string, DynamicShortcodeDefinition> shortcodeFactory = ( shortcodeName ) =>
            {
                var shortcodeDefinition = LavaShortcodeCache.All().Where( c => c.TagName == shortcodeName ).FirstOrDefault();

                if ( shortcodeDefinition == null )
                {
                    return null;
                }

                var parameters = RockSerializableDictionary.FromUriEncodedString( shortcodeDefinition.Parameters );

                return new DynamicShortcodeDefinition
                {
                    Name = shortcodeDefinition.Name,
                    TemplateMarkup = shortcodeDefinition.Markup,
                    Parameters = new Dictionary<string, string>( parameters.Dictionary ),
                    EnabledLavaCommands = shortcodeDefinition.EnabledLavaCommands.SplitDelimitedValues( ",", StringSplitOptions.RemoveEmptyEntries ).ToList(),
                    ElementType = shortcodeDefinition.TagType == TagType.Block
                        ? LavaShortcodeTypeSpecifier.Block
                        : LavaShortcodeTypeSpecifier.Inline
                };
            };

            foreach ( var shortcode in LavaShortcodeCache.All() )
            {
                engine.RegisterShortcode( shortcode.TagName, shortcodeFactory );
            }
        }

        #endregion Methods
    }
}
