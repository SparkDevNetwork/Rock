//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using NUnit.Framework;
using Rock.Model;

namespace Rock.Tests.Model
{
    [TestFixture]
    public class BlockTests
    {
        public class TheCopyPropertiesMethod
        {
            [Test]
            public void ShouldCopyProperties()
            {
                var block = new Block { Name = "Foo" };
                var result = block.Clone( false );
                Assert.AreEqual( result.Name, block.Name );
            }
        }

        public class TheToJsonMethod
        {
            [Test]
            public void ShouldNotBeEmpty()
            {
                var block = new Block { Name = "Foo" };
                var result = block.ToJson();
                Assert.IsNotEmpty( result );
            }
        }

        public class TheFromJsonMethod
        {
            [Test]
            public void ShouldImportProperties()
            {
                var obj = new
                {
                    IsSystem = true,
                    BlockTypeId = 1,
                    Zone = "TestZone",
                    Order = 3,
                    Name = "FooInstance",
                    OutputCacheDuration = 0
                };

                var json = obj.ToJson();
                var block = Block.FromJson( json );
                Assert.AreEqual( obj.Name, block.Name );
                Assert.AreEqual( obj.IsSystem, block.IsSystem );
            }

            [Test]
            public void ShouldImportBlockType()
            {
                var obj = new
                {
                    IsSystem = true,
                    BlockTypeId = 1,
                    Zone = "TestZone",
                    Order = 3,
                    Name = "FooInstance",
                    OutputCacheDuration = 0,
                    BlockType = new {
                        IsSystem = false,
                        Path = "Test Path",
                        Name = "Test Name",
                        Description = "Test desc"
                    }
                };

                var json = obj.ToJson();
                var block = Block.FromJson( json );
                var blockType = block.BlockType;
                Assert.IsNotNull( blockType );
                Assert.AreEqual( blockType.Name, obj.BlockType.Name );
            }

        }

        public class TheCloneMethod
        {
            [Test]
            public void ShouldCloneObject()
            {
                var block = new Block { BlockType = new BlockType() };
                var result = block.Clone() as Block;
                Assert.NotNull( result );
                Assert.NotNull( result.BlockType );
            }
        }
    }
}
