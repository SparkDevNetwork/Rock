using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Web.v2;

namespace Rock.Tests.Web.v2
{
    [TestClass]
    public class LavaPageRenderSectionBlockTests
    {
        [TestMethod]
        public async Task RenderSection_WithoutAnyDefinedSections_RendersEmptyString()
        {
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );

            engine.RegisterBlock( "rendersection", _ => new LavaPageRenderSectionBlock() );

            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );
            var result = engine.RenderTemplate( "{% rendersection id:'main' %}{% endrendersection %}", parameters );

            Assert.IsFalse( result.HasErrors, "Lava generated errors." );
            Assert.AreEqual( string.Empty, result.Text );
        }

        [TestMethod]
        public async Task RenderSection_WithoutId_RendersEmptyString()
        {
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );

            engine.RegisterBlock( "rendersection", _ => new LavaPageRenderSectionBlock() );
            engine.RegisterBlock( "section", _ => new LavaPageSectionBlock() );

            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );
            var result = engine.RenderTemplate( "{% section id:'main' %}hello{% endsection %}{% rendersection %}{% endrendersection %}", parameters );

            Assert.IsFalse( result.HasErrors, "Lava generated errors." );
            Assert.AreEqual( string.Empty, result.Text );
        }

        [TestMethod]
        public async Task RenderSection_WithMissingSection_RendersDefaultContent()
        {
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );

            engine.RegisterBlock( "rendersection", _ => new LavaPageRenderSectionBlock() );
            engine.RegisterBlock( "section", _ => new LavaPageSectionBlock() );

            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );
            var result = engine.RenderTemplate( "{% section id:'other' %}hello{% endsection %}{% rendersection id:'main' %}default{% endrendersection %}", parameters );

            Assert.IsFalse( result.HasErrors, "Lava generated errors." );
            Assert.AreEqual( "default", result.Text );
        }

        [TestMethod]
        public async Task RenderSection_WithSection_RendersContent()
        {
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );

            engine.RegisterBlock( "rendersection", _ => new LavaPageRenderSectionBlock() );
            engine.RegisterBlock( "section", _ => new LavaPageSectionBlock() );

            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );
            var result = engine.RenderTemplate( "{% section id:'main' %}hello{% endsection %}{% rendersection id:'main' %}{% endrendersection %}", parameters );

            Assert.IsFalse( result.HasErrors, "Lava generated errors." );
            Assert.AreEqual( "hello", result.Text );
        }
    }
}
