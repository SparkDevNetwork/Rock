//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rock.Model;

namespace Rock.Tests.Cms
{
    [TestFixture]
    public class BlockTests
    {
        public class TheExportObjectMethod
        {
            [Test]
            public void ShouldCopyEntity()
            {
                var block = new Block { Name = "Foo" };
                dynamic result = block.ExportObject();
                Assert.AreEqual( result.Name, block.Name );
            }

            [Test]
            public void ShouldCopyHtmlContents()
            {
                var block = new Block { HtmlContents = new List<HtmlContent>() };
                block.HtmlContents.Add( new HtmlContent() );
                dynamic result = block.ExportObject();
                Assert.NotNull( result.HtmlContents );
                Assert.IsNotEmpty( result.HtmlContents );
            }

            [Test]
            public void ShouldCopyBlock()
            {
                var block = new Block { BlockType = new BlockType() };
                dynamic result = block.ExportObject();
                Assert.NotNull( result.BlockType );
            }
        }

        public class TheExportJsonMethod
        {
            [Test]
            public void ShouldNotBeEmpty()
            {
                var block = new Block() { Name = "Foo" };
                var result = block.ExportJson();
                Assert.IsNotEmpty( result );
            }
        }

        public class TheImportJsonMethod
        {
            [Test]
            public void ShouldCopyPropertiesToEntity()
            {
                var obj = new
                {
                    Name = "FooInstance",
                    IsSystem = true
                };

                var json = obj.ToJSON();
                var block = new Block();
                block.ImportJson( json );
                Assert.AreEqual( obj.Name, block.Name );
                Assert.AreEqual( obj.IsSystem, block.IsSystem );
            }

            [Test]
            public void ShouldImportHtmlContents()
            {
                var obj = new
                {
                    Name = "Foo Block",
                    IsSystem = true,
                    HtmlContents = new List<dynamic> { new { Content = "Foo Html" } }
                };

                var json = obj.ToJSON();
                var block = new Block();
                block.ImportJson( json );
                var htmlContents = block.HtmlContents;
                Assert.IsNotNull( htmlContents );
                Assert.IsNotEmpty( htmlContents );
                Assert.AreEqual( htmlContents.First().Content, obj.HtmlContents[ 0 ].Content );
            }
        }
    }
}
