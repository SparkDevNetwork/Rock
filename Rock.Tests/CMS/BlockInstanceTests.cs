using System.Collections.Generic;
using Rock.CMS;
using Xunit;

namespace Rock.Tests.CMS
{
    public class BlockInstanceTests
    {
        public class TheExportObjectMethod
        {
            [Fact]
            public void ShouldCopyDTO()
            {
                var blockInstance = new BlockInstance { Name = "Foo" };
                dynamic result = blockInstance.ExportObject();
                Assert.Equal( result.Name, blockInstance.Name );
            }

            [Fact]
            public void ShouldCopyHtmlContents()
            {
                var blockInstance = new BlockInstance { HtmlContents = new List<HtmlContent>() };
                blockInstance.HtmlContents.Add( new HtmlContent() );
                dynamic result = blockInstance.ExportObject();
                Assert.NotNull( result.HtmlContents );
                Assert.NotEmpty( result.HtmlContents );
            }

            [Fact]
            public void ShouldCopyBlock()
            {
                var blockInstane = new BlockInstance { Block = new Block() };
                dynamic result = blockInstane.ExportObject();
                Assert.NotNull( result.Block );
            }
        }
    }
}
