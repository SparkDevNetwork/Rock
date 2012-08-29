//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using Rock.Cms;
using Xunit;

namespace Rock.Tests.Cms
{
    public class BlockInstanceTests
    {
        public class TheExportObjectMethod
        {
            [Fact]
            public void ShouldCopyEntity()
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

        public class TheExportJsonMethod
        {
            [Fact]
            public void ShouldNotBeEmpty()
            {
                var blockIntance = new BlockInstance() { Name = "Foo" };
                var result = blockIntance.ExportJson();
                Assert.NotEmpty( result );
            }
        }
    }
}
