using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.FileProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Lava;
using Rock.Web.v2;

namespace Rock.Tests.Web.v2
{
    [TestClass]
    public class LavaPageLayoutBlockTests
    {
        [TestMethod]
        public async Task Layout_RendersSourceTemplate()
        {
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );

            engine.RegisterBlock( "layout", _ => new LavaPageLayoutBlock( GetMockFileProvider() ) );
            engine.RegisterBlock( "renderbody", _ => new LavaPageRenderBodyBlock() );

            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );
            var result = engine.RenderTemplate( "{% layout src:'/main.lava' %}hello{% endlayout %}", parameters );

            Assert.IsFalse( result.HasErrors, "Lava generated errors." );
            Assert.Contains( "<!-- main.lava -->", result.Text );
        }

        [TestMethod]
        public async Task Layout_WithBody_RendersBody()
        {
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );

            engine.RegisterBlock( "layout", _ => new LavaPageLayoutBlock( GetMockFileProvider() ) );
            engine.RegisterBlock( "renderbody", _ => new LavaPageRenderBodyBlock() );

            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );
            var result = engine.RenderTemplate( "{% layout src:'/main.lava' %}<!-- body -->{% endlayout %}", parameters );

            Assert.IsFalse( result.HasErrors, "Lava generated errors." );
            Assert.Contains( "<!-- body -->", result.Text );
        }

        [TestMethod]
        public async Task Layout_WithBody_SetsMergeField()
        {
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );

            engine.RegisterBlock( "layout", _ => new LavaPageLayoutBlock( GetMockFileProvider() ) );
            engine.RegisterBlock( "renderbody", _ => new LavaPageRenderBodyBlock() );

            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );
            var result = engine.RenderTemplate( "{% layout src:'/main.lava' %}hello{% endlayout %}", parameters );

            Assert.IsFalse( result.HasErrors, "Lava generated errors." );
            Assert.AreEqual( "hello", context.GetInternalField( "LavaPageBody" ) );
        }

        [TestMethod]
        public async Task Layout_WithoutBody_SetsMergeField()
        {
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );

            engine.RegisterBlock( "layout", _ => new LavaPageLayoutBlock( GetMockFileProvider() ) );
            engine.RegisterBlock( "renderbody", _ => new LavaPageRenderBodyBlock() );

            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );
            var result = engine.RenderTemplate( "{% layout src:'/main.lava' %}{% endlayout %}", parameters );

            Assert.IsFalse( result.HasErrors, "Lava generated errors." );
            Assert.AreEqual( string.Empty, context.GetInternalField( "LavaPageBody" ) );
        }

        [TestMethod]
        public async Task Layout_WithMissingFile_EmitsError()
        {
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );

            engine.RegisterBlock( "layout", _ => new LavaPageLayoutBlock( GetMockFileProvider() ) );

            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );
            var result = engine.RenderTemplate( "{% layout src:'/missing.lava' %}{% endlayout %}", parameters );

            Assert.IsTrue( result.HasErrors, "Lava failed to generate expected error." );
        }

        private IFileProvider GetMockFileProvider()
        {
            var mainLavaMock = new Mock<IFileInfo>();
            var missingLavaMock = new Mock<IFileInfo>();
            var fileProviderMock = new Mock<IFileProvider>();

            var mainLavaStream = new MemoryStream();

            using ( var writer = new StreamWriter( mainLavaStream, Encoding.UTF8, 4096, true ) )
            {
                writer.WriteLine( "<html><!-- main.lava -->{% renderbody %}{% endrenderbody %}</html>" );
            }

            mainLavaStream.Position = 0;

            mainLavaMock.Setup( m => m.Exists ).Returns( true );
            mainLavaMock.Setup( m => m.CreateReadStream() ).Returns( mainLavaStream );

            missingLavaMock.Setup( m => m.Exists ).Returns( false );

            fileProviderMock.Setup( m => m.GetFileInfo( "/main.lava" ) ).Returns( mainLavaMock.Object );
            fileProviderMock.Setup( m => m.GetFileInfo( "/missing.lava" ) ).Returns( missingLavaMock.Object );

            return fileProviderMock.Object;
        }
    }
}
