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
    public class BlockTests
    {
        public class TheExportObjectMethod
        {
            [Fact]
            public void ShouldCopyEntity()
            {
                var block = new Block { Name = "Foo" };
                dynamic result = block.ExportObject();
                Assert.Equal( result.Name, block.Name );
            }

            [Fact]
            public void ShouldCopyHtmlContents()
            {
                var block = new Block { HtmlContents = new List<HtmlContent>() };
                block.HtmlContents.Add( new HtmlContent() );
                dynamic result = block.ExportObject();
                Assert.NotNull( result.HtmlContents );
                Assert.NotEmpty( result.HtmlContents );
            }

            [Fact]
            public void ShouldCopyBlock()
            {
                var block = new Block { BlockType = new BlockType() };
                dynamic result = block.ExportObject();
                Assert.NotNull( result.Block );
            }
        }

        public class TheExportJsonMethod
        {
            [Fact]
            public void ShouldNotBeEmpty()
            {
                var block = new Block() { Name = "Foo" };
                var result = block.ExportJson();
                Assert.NotEmpty( result );
            }
        }
    }
}
