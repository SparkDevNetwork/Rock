using System.Collections.Concurrent;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Web.v2;

namespace Rock.Tests.Web.v2
{
    [TestClass]
    public class LavaPageSectionBlockTests
    {
        [TestMethod]
        public async Task Section_WithContent_RendersEmptyString()
        {
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );

            engine.RegisterBlock( "section", _ => new LavaPageSectionBlock() );

            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );
            var result = engine.RenderTemplate( "{% section id:'main' %}hello{% endsection %}", parameters );

            Assert.IsFalse( result.HasErrors, "Lava generated errors." );
            Assert.AreEqual( string.Empty, result.Text );
        }

        [TestMethod]
        public async Task Section_WithContent_SetsSection()
        {
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );

            engine.RegisterBlock( "section", _ => new LavaPageSectionBlock() );

            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );
            var result = engine.RenderTemplate( "{% section id:'main' %}hello{% endsection %}", parameters );

            Assert.IsFalse( result.HasErrors, "Lava generated errors." );
            Assert.IsNotNull( context.GetInternalField( "LavaPageSections" ) );

            var sections = context.GetInternalField( "LavaPageSections" ) as ConcurrentDictionary<string, string>;

            Assert.IsTrue( sections.ContainsKey( "main" ) );
            Assert.AreEqual( "hello", sections["main"] );
        }

        [TestMethod]
        public async Task Section_WithoutContent_SetsSection()
        {
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );

            engine.RegisterBlock( "section", _ => new LavaPageSectionBlock() );

            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );
            var result = engine.RenderTemplate( "{% section id:'main' %}hello{% endsection %}", parameters );

            Assert.IsFalse( result.HasErrors, "Lava generated errors." );
            Assert.IsNotNull( context.GetInternalField( "LavaPageSections" ) );

            var sections = context.GetInternalField( "LavaPageSections" ) as ConcurrentDictionary<string, string>;

            Assert.IsTrue( sections.ContainsKey( "main" ) );
        }

        [TestMethod]
        public async Task Section_WithoutId_RendersEmptyString()
        {
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );

            engine.RegisterBlock( "section", _ => new LavaPageSectionBlock() );

            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );
            var result = engine.RenderTemplate( "{% section %}hello{% endsection %}", parameters );

            Assert.IsFalse( result.HasErrors, "Lava generated errors." );
            Assert.AreEqual( string.Empty, result.Text );
        }
    }
}
