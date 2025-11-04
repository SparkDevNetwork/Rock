using System.IO;
using System.Text;

using AngleSharp;
using AngleSharp.Html.Parser;

using Microsoft.Extensions.FileProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Lava;
using Rock.Web.v2;

namespace Rock.Tests.Web.v2
{
    [TestClass]
    public class LavaPageLayoutBuilderTests
    {
        [TestMethod]
        public void Test()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:RenderBody />
    {{ Content.Main }}
</body>
</html>
";

            var layoutLava = @"<Rock:ParentLayout src=""/main.lava"">
    <div>body</div>
</Rock:ParentLayout>";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava },
                new[] { "/layout.lava", layoutLava } );

            var builder = new LavaPageLayoutBuilder( fileProvider );
            var factory = new LavaEngineFactory();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );

            var layout = builder.GetLayout( "/layout.lava", engine );
        }

        private IFileProvider GetMockFileProvider( params string[][] filesAndContents )
        {
            var fileProviderMock = new Mock<IFileProvider>();

            fileProviderMock.Setup( m => m.GetFileInfo( It.IsAny<string>() ) ).Returns<string>( path =>
            {
                var fileInfoMock = new Mock<IFileInfo>();

                for ( int i = 0; i < filesAndContents.Length; i++ )
                {
                    if ( filesAndContents[i][0] != path )
                    {
                        continue;
                    }

                    var stream = new MemoryStream();

                    using ( var writer = new StreamWriter( stream, Encoding.UTF8, 4096, true ) )
                    {
                        writer.Write( filesAndContents[i][1] );
                    }

                    fileInfoMock.Setup( m => m.Exists ).Returns( true );
                    fileInfoMock.Setup( m => m.CreateReadStream() ).Returns( () =>
                    {
                        var fileStream = new MemoryStream();

                        stream.Position = 0;
                        stream.CopyTo( fileStream );

                        return fileStream;
                    } );
                }

                fileInfoMock.Setup( m => m.Exists ).Returns( false );

                return fileInfoMock.Object;
            } );

            return fileProviderMock.Object;
        }
    }
}
