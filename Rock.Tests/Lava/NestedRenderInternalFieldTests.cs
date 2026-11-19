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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared.Constants;
using Rock.Tests.Shared.TestFramework;
using Rock.Web;

namespace Rock.Tests.Lava
{
    /// <summary>
    /// Tests that Lava commands, filters and shortcodes which render a second
    /// template keep the internal fields of the outer render, such as the
    /// request context.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class NestedRenderInternalFieldTests
    {
        private const string MarkerFilterName = "ReadTestMarker";
        private const string MarkerFieldKey = "TestMarker";
        private const string MarkerValue = "marker-value";

        #region Dynamic Shortcode

        /// <summary>
        /// The body of a block shortcode is rendered in its own context, so it
        /// must be given the internal fields of the calling template.
        /// </summary>
        [TestMethod]
        public void DynamicShortcode_BlockContent_SeesOuterInternalField()
        {
            var tagName = $"{nameof( NestedRenderInternalFieldTests )}_{nameof( DynamicShortcode_BlockContent_SeesOuterInternalField )}";

            var input = $"{{[ {tagName} ]}}{{{{ 'x' | {MarkerFilterName} }}}}{{[ end{tagName} ]}}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine => RegisterBlockShortcode( engine, tagName ), engine =>
            {
                var output = RenderWithMarker( engine, input, new List<string>() );

                Assert.AreEqual( MarkerValue, output.Trim() );
            } );
        }

        /// <summary>
        /// Inside a loop the internal fields live in an outer scope. The
        /// shortcode body must still get them.
        /// </summary>
        [TestMethod]
        public void DynamicShortcode_BlockContentInsideLoop_SeesOuterInternalField()
        {
            var tagName = $"{nameof( NestedRenderInternalFieldTests )}_{nameof( DynamicShortcode_BlockContentInsideLoop_SeesOuterInternalField )}";

            var input = $"{{% for i in (1..1) %}}{{[ {tagName} ]}}{{{{ 'x' | {MarkerFilterName} }}}}{{[ end{tagName} ]}}{{% endfor %}}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine => RegisterBlockShortcode( engine, tagName ), engine =>
            {
                var output = RenderWithMarker( engine, input, new List<string>() );

                Assert.AreEqual( MarkerValue, output.Trim() );
            } );
        }

        /// <summary>
        /// Copying internal fields also copies the enabled commands. The body
        /// must still run with the caller's commands only, not the commands
        /// the shortcode definition enables, so the sql command is refused.
        /// </summary>
        [TestMethod]
        public void DynamicShortcode_BlockContent_KeepsCallerEnabledCommands()
        {
            var tagName = $"{nameof( NestedRenderInternalFieldTests )}_{nameof( DynamicShortcode_BlockContent_KeepsCallerEnabledCommands )}";

            var input = $"{{[ {tagName} ]}}{{% sql %}}SELECT 1 AS [Value]{{% endsql %}}{{[ end{tagName} ]}}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine => RegisterBlockShortcode( engine, tagName, new List<string> { "Sql" } ), engine =>
            {
                var output = RenderWithMarker( engine, input, new List<string>() );

                Assert.AreEqual( "The Lava command 'sql' is not configured for this template.", output.Trim() );
            } );
        }

        #endregion

        #region Cache Block

        /// <summary>
        /// The content of a cache block is rendered in a new context and must
        /// keep the internal fields of the calling template. This guards
        /// behavior that already worked before the nested render changes.
        /// </summary>
        [TestMethod]
        public void CacheBlock_Content_SeesOuterInternalField()
        {
            var input = $"{{% cache key:'nested-render-internal-field' duration:'0' %}}{{{{ 'x' | {MarkerFilterName} }}}}{{% endcache %}}";

            LavaRenderTestHelper.ExecuteForActiveEngines( RegisterMarkerFilter, engine =>
            {
                var output = RenderWithMarker( engine, input, new List<string> { "Cache" } );

                Assert.AreEqual( MarkerValue, output.Trim() );
            } );
        }

        /// <summary>
        /// A cache block inside a loop must still get internal fields that
        /// live in an outer scope.
        /// </summary>
        [TestMethod]
        public void CacheBlock_ContentInsideLoop_SeesOuterInternalField()
        {
            var input = $"{{% for i in (1..1) %}}{{% cache key:'nested-render-internal-field-loop' duration:'0' %}}{{{{ 'x' | {MarkerFilterName} }}}}{{% endcache %}}{{% endfor %}}";

            LavaRenderTestHelper.ExecuteForActiveEngines( RegisterMarkerFilter, engine =>
            {
                var output = RenderWithMarker( engine, input, new List<string> { "Cache" } );

                Assert.AreEqual( MarkerValue, output.Trim() );
            } );
        }

        #endregion

        #region SetCulture Block

        /// <summary>
        /// The content of a setculture block inside a loop must still get
        /// internal fields that live in an outer scope.
        /// </summary>
        [TestMethod]
        public void SetCultureBlock_ContentInsideLoop_SeesOuterInternalField()
        {
            var input = $"{{% for i in (1..1) %}}{{% setculture culture:'invariant' %}}{{{{ 'x' | {MarkerFilterName} }}}}{{% endsetculture %}}{{% endfor %}}";

            LavaRenderTestHelper.ExecuteForActiveEngines( RegisterMarkerFilter, engine =>
            {
                var output = RenderWithMarker( engine, input, new List<string>() );

                Assert.AreEqual( MarkerValue, output.Trim() );
            } );
        }

        /// <summary>
        /// A setculture block inside a loop must run its content with the
        /// caller's enabled commands, not the engine defaults. The defaults
        /// here enable Sql and the caller does not, so sql is refused.
        /// </summary>
        [TestMethod]
        public void SetCultureBlock_ContentInsideLoop_KeepsCallerEnabledCommands()
        {
            var input = "{% for i in (1..1) %}{% setculture culture:'invariant' %}{% sql %}SELECT 1 AS [Value]{% endsql %}{% endsetculture %}{% endfor %}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                ( ( LavaEngineBase ) engine ).DefaultEnabledCommands = new List<string> { "Sql" };

                var output = RenderWithMarker( engine, input, new List<string>() );

                Assert.AreEqual( "The Lava command 'sql' is not configured for this template.", output.Trim() );
            } );
        }

        #endregion

        #region Observe Block

        /// <summary>
        /// The content of an observe block inside a loop must still get
        /// internal fields that live in an outer scope.
        /// </summary>
        [TestMethod]
        public void ObserveBlock_ContentInsideLoop_SeesOuterInternalField()
        {
            var input = $"{{% for i in (1..1) %}}{{% observe name:'nested-render-test' %}}{{{{ 'x' | {MarkerFilterName} }}}}{{% endobserve %}}{{% endfor %}}";

            LavaRenderTestHelper.ExecuteForActiveEngines( RegisterMarkerFilter, engine =>
            {
                var output = RenderWithMarker( engine, input, new List<string> { "Observe" } );

                Assert.AreEqual( MarkerValue, output.Trim() );
            } );
        }

        /// <summary>
        /// An observe block inside a loop must run its content with the
        /// caller's enabled commands, not the engine defaults. The defaults
        /// here enable Sql and the caller does not, so sql is refused.
        /// </summary>
        [TestMethod]
        public void ObserveBlock_ContentInsideLoop_KeepsCallerEnabledCommands()
        {
            var input = "{% for i in (1..1) %}{% observe name:'nested-render-test' %}{% sql %}SELECT 1 AS [Value]{% endsql %}{% endobserve %}{% endfor %}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                ( ( LavaEngineBase ) engine ).DefaultEnabledCommands = new List<string> { "Observe", "Sql" };

                var output = RenderWithMarker( engine, input, new List<string> { "Observe" } );

                Assert.AreEqual( "The Lava command 'sql' is not configured for this template.", output.Trim() );
            } );
        }

        #endregion

        #region Attribute Filter

        /// <summary>
        /// The Attribute filter renders a system setting value that contains
        /// Lava. That render must keep the internal fields of the calling
        /// template.
        /// </summary>
        [TestMethod]
        public void AttributeFilter_SystemSettingContainingLava_SeesOuterInternalField()
        {
            using var app = TestHelper.CreateScopedRockApp();

            var settingKey = nameof( AttributeFilter_SystemSettingContainingLava_SeesOuterInternalField );
            AddSystemSetting( settingKey, $"{{{{ 'x' | {MarkerFilterName} }}}}" );

            var input = $"{{{{ 'SystemSetting' | Attribute:'{settingKey}' }}}}";

            LavaRenderTestHelper.ExecuteForActiveEngines( RegisterMarkerFilter, engine =>
            {
                var output = RenderWithMarker( engine, input, new List<string>() );

                Assert.AreEqual( MarkerValue, output.Trim() );
            } );
        }

        #endregion

        #region Command Markup

        /// <summary>
        /// Lava commands that parse their parameters with
        /// <see cref="LavaHelper.ParseCommandMarkup"/> render the parameter
        /// markup separately, and that render must keep the internal fields of
        /// the calling template.
        /// </summary>
        [TestMethod]
        public void ParseCommandMarkup_ParameterContainingLava_SeesOuterInternalField()
        {
            using var app = TestHelper.CreateScopedRockApp();

            var markup = $"value:'{{{{ 'x' | {MarkerFilterName} }}}}'";

            LavaRenderTestHelper.ExecuteForActiveEngines( RegisterMarkerFilter, engine =>
            {
                var context = engine.NewRenderContext( GetMarkerMergeFields(), new List<string>() );
                var parameters = new Dictionary<string, string>();

                LavaHelper.ParseCommandMarkup( markup, context, parameters );

                Assert.AreEqual( MarkerValue, parameters["value"] );
            } );
        }

        #endregion

        #region RenderLava

        /// <summary>
        /// The RenderLava overload that takes a person override must accept
        /// merge fields that carry internal fields, such as the request
        /// context added by GetCommonMergeFields, and pass them to the render.
        /// </summary>
        [TestMethod]
        public void RenderLava_WithPersonOverride_PassesInternalFields()
        {
            using var app = TestHelper.CreateScopedRockApp();

            var template = $"{{{{ 'x' | {MarkerFilterName} }}}}";

            LavaRenderTestHelper.ExecuteForActiveEngines( RegisterMarkerFilter, engine =>
            {
                var output = template.RenderLava( GetMarkerMergeFields(), ( Person ) null, string.Empty );

                Assert.AreEqual( MarkerValue, output );
            } );
        }

        /// <summary>
        /// The RenderLava person override overload must render with the
        /// commands it is given, not the engine defaults. The defaults here
        /// enable Sql and the caller does not, so sql is refused.
        /// </summary>
        [TestMethod]
        public void RenderLava_WithPersonOverride_AppliesGivenEnabledCommands()
        {
            using var app = TestHelper.CreateScopedRockApp();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                ( ( LavaEngineBase ) engine ).DefaultEnabledCommands = new List<string> { "Sql" };

                var output = "{% sql %}SELECT 1 AS [Value]{% endsql %}".RenderLava( new Dictionary<string, object>(), ( Person ) null, "Cache" );

                Assert.AreEqual( "The Lava command 'sql' is not configured for this template.", output.Trim() );
            } );
        }

        /// <summary>
        /// When the merge fields have no CurrentPerson, the RenderLava person
        /// override supplies it.
        /// </summary>
        [TestMethod]
        public void RenderLava_WithPersonOverride_UsesOverrideWhenMergeFieldsHaveNoCurrentPerson()
        {
            using var app = TestHelper.CreateScopedRockApp();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = "{{ CurrentPerson.FirstName }}".RenderLava( new Dictionary<string, object>(), new Person { FirstName = "Override" }, string.Empty );

                Assert.AreEqual( "Override", output );
            } );
        }

        /// <summary>
        /// A CurrentPerson in the merge fields has always replaced the
        /// RenderLava person override, and still must.
        /// </summary>
        [TestMethod]
        public void RenderLava_WithPersonOverride_KeepsCurrentPersonFromMergeFields()
        {
            using var app = TestHelper.CreateScopedRockApp();

            var mergeFields = new Dictionary<string, object>
            {
                { "CurrentPerson", new Person { FirstName = "MergeField" } }
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = "{{ CurrentPerson.FirstName }}".RenderLava( mergeFields, new Person { FirstName = "Override" }, string.Empty );

                Assert.AreEqual( "MergeField", output );
            } );
        }

        #endregion

        #region Support

        /// <summary>
        /// Returns the value of the test marker internal field, so a template
        /// can show whether the internal field reached it.
        /// </summary>
        /// <param name="context">The render context.</param>
        /// <param name="input">The filter input, which is ignored.</param>
        /// <returns>The marker value, or an empty string if it is missing.</returns>
        public static string ReadTestMarker( ILavaRenderContext context, object input )
        {
            return context.GetInternalField( MarkerFieldKey )?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Registers the filter that reads the test marker.
        /// </summary>
        /// <param name="engine">The engine under test.</param>
        private static void RegisterMarkerFilter( ILavaEngine engine )
        {
            engine.RegisterFilter( typeof( NestedRenderInternalFieldTests ).GetMethod( nameof( ReadTestMarker ) ), MarkerFilterName );
        }

        /// <summary>
        /// Registers the marker filter and a block shortcode whose template
        /// writes out its block content.
        /// </summary>
        /// <param name="engine">The engine under test.</param>
        /// <param name="tagName">The shortcode tag name.</param>
        /// <param name="shortcodeEnabledCommands">The commands enabled by the shortcode definition.</param>
        private static void RegisterBlockShortcode( ILavaEngine engine, string tagName, List<string> shortcodeEnabledCommands = null )
        {
            RegisterMarkerFilter( engine );

            engine.RegisterShortcode( tagName, name => new DynamicShortcodeDefinition
            {
                Name = tagName,
                TemplateMarkup = "{{ blockContent }}",
                ElementType = LavaShortcodeTypeSpecifier.Block,
                EnabledLavaCommands = shortcodeEnabledCommands
            } );
        }

        /// <summary>
        /// Renders a template in a context that has the test marker set as an
        /// internal field.
        /// </summary>
        /// <param name="engine">The engine under test.</param>
        /// <param name="template">The template to render.</param>
        /// <param name="enabledCommands">The commands enabled for the template.</param>
        /// <returns>The rendered text.</returns>
        private static string RenderWithMarker( ILavaEngine engine, string template, List<string> enabledCommands )
        {
            var context = engine.NewRenderContext( GetMarkerMergeFields(), enabledCommands );

            return engine.RenderTemplate( template, LavaRenderParameters.WithContext( context ) ).Text;
        }

        /// <summary>
        /// Gets a merge field dictionary that carries the test marker as an
        /// internal field, the same way GetCommonMergeFields carries the
        /// request context.
        /// </summary>
        /// <returns>The merge field dictionary.</returns>
        private static Dictionary<string, object> GetMarkerMergeFields()
        {
            return new Dictionary<string, object>
            {
                { LavaRenderContextBase.InternalMergeFieldPrefix + MarkerFieldKey, MarkerValue }
            };
        }

        /// <summary>
        /// Adds a system setting to the mocked database and clears the cached
        /// system settings so the next read sees it.
        /// </summary>
        /// <param name="key">The setting key.</param>
        /// <param name="value">The setting value.</param>
        private static void AddSystemSetting( string key, string value )
        {
            var rockContext = RockApp.Current.CreateRockContext();

            rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
            {
                Id = 1,
                EntityTypeId = null,
                EntityTypeQualifierColumn = Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER,
                EntityTypeQualifierValue = string.Empty,
                Key = key,
                Name = key,
                DefaultValue = value,
                Guid = Guid.NewGuid(),
                Categories = new List<Rock.Model.Category>(),
                AttributeQualifiers = new List<Rock.Model.AttributeQualifier>()
            } );

            SystemSettings.Remove();
        }

        #endregion
    }
}
