using DotLiquid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Integration.Lava
{
    [TestClass]
    public class LavaCachingTests
    {
        #region Caching

        /// <summary>
        /// Verify that templates with varying amounts of whitespace are correctly cached and return the expected output.
        /// </summary>
        [TestMethod]
        public void TemplateCaching_WhitespaceTemplatesWithDifferentLengths_AreCachedIndependently()
        {
            // Process an initial whitespace template - this will be cached.
            var input0 = string.Empty;
            AssertTemplateOutput( input0, input0 );

            // Process a whitespace template of a different length - this should be cached separately from the first template.
            // If not, the caching mechanism may cause whitespace to be rendered incorrectly.
            var input1 = new string( ' ', 100 );
            AssertTemplateOutput( input1, input1 );
        }

        private void AssertTemplateOutput( string expectedOutput, string templateContent )
        {
            // Get access to the private GetTemplate method so we can use the internal Lava template caching mechanism.
            var GetTemplateMethodsInfo = typeof( ExtensionMethods ).GetMethod( "GetTemplate", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic );

            var template = GetTemplateMethodsInfo.Invoke( null, new object[] { templateContent } ) as Template;

            var output = template.Render();

            // Verify that the rendered template matches the expected output.
            Assert.AreEqual( expectedOutput, output, "Template Output does not match expected output." );
        }
        #endregion
    }
}
