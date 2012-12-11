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
    public class BlockTypeTests
    {
        public class TheExportObjectMethod
        {
            [Test]
            public void ShouldCopyEntity()
            {
                var blockType = new BlockType() {Name = "some block type"};
                dynamic result = blockType.ToDynamic( true );
                Assert.AreEqual( result.Name, blockType.Name );
            }
        }

        public class TheExportJsonMethod
        {
            [Test]
            public void ShouldNotBeEmpty()
            {
                var blockType = new BlockType() { Name = "some block type" };
                var result = blockType.ToJson( true );
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
                    Description = "Test desc",
                    IsSystem = false
                };

                var json = obj.ToJSON();
                var blockType = new BlockType();
                blockType.FromJson( json );
                Assert.AreEqual( obj.Description, blockType.Description );
                Assert.AreEqual( obj.IsSystem, blockType.IsSystem );
            }

            [Test]
            public void ShouldImportBlocks()
            {
                var obj = new
                {
                    Description = "Test desc",
                    IsSystem = true,
                    Blocks = new List<dynamic> { new { Name = "Test block instance" } }
                };

                var json = obj.ToJSON();
                var blockType = new BlockType();
                blockType.FromJson( json );
                var blocks = blockType.Blocks;
                Assert.IsNotNull( blocks );
                Assert.IsNotEmpty( blocks );
                Assert.AreEqual( blocks.First().Name, obj.Blocks[ 0 ].Name );
            }
        }
    }
}
