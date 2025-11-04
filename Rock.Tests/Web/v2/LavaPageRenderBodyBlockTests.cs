using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Web.v2;

namespace Rock.Tests.Web.v2
{
    [TestClass]
    public class LavaPageRenderBodyBlockTests
    {
        [TestMethod]
        public async Task RenderBody_WithoutContentOrDefault_RendersEmptyString()
        {
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );

            engine.RegisterBlock( "renderbody", _ => new LavaPageRenderBodyBlock() );

            var result = engine.RenderTemplate( "{% renderbody %}{% endrenderbody %}", parameters );

            Assert.IsFalse( result.HasErrors, "Lava generated errors." );
            Assert.AreEqual( string.Empty, result.Text );
        }

        [TestMethod]
        public async Task RenderBody_WithContent_RendersContent()
        {
            var expectedContent = "test-marker";
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );

            engine.RegisterBlock( "renderbody", _ => new LavaPageRenderBodyBlock() );
            context.SetInternalField( "LavaPageBody", expectedContent );

            var result = engine.RenderTemplate( "{% renderbody %}{% endrenderbody %}", parameters );

            Assert.IsFalse( result.HasErrors, "Lava generated errors." );
            Assert.AreEqual( expectedContent, result.Text );
        }


        [TestMethod]
        public async Task RenderBody_WithoutContent_RendersDefaultContent()
        {
            var expectedContent = "test-marker";
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );

            engine.RegisterBlock( "renderbody", _ => new LavaPageRenderBodyBlock() );

            var result = engine.RenderTemplate( $"{{% renderbody %}}{expectedContent}{{% endrenderbody %}}", parameters );

            Assert.IsFalse( result.HasErrors, "Lava generated errors." );
            Assert.AreEqual( expectedContent, result.Text );
        }
    }
}
