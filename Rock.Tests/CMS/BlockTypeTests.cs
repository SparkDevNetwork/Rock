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
        public class TheCopyPropertiesMethod
        {
            [Test]
            public void ShouldCopyProperties()
            {
                var blockType = new BlockType {Name = "some block type"};
                var result = new BlockType();
                result.CopyPropertiesFrom( blockType );
                Assert.AreEqual( result.Name, blockType.Name );
            }
        }

        public class TheToJsonMethod
        {
            [Test]
            public void ShouldNotBeEmpty()
            {
                var blockType = new BlockType { Name = "some block type" };
                var result = blockType.ToJson();
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
                    IsSystem = false,
                    Path = "Test Path",
                    Name = "Test Name",
                    Description = "Test desc"
                };

                var json = obj.ToJson();
                var blockType = BlockType.FromJson( json );
                Assert.AreEqual( obj.Description, blockType.Description );
                Assert.AreEqual( obj.IsSystem, blockType.IsSystem );
            }

            [Test]
            public void ShouldImportBlocks()
            {
                var obj = new
                {
                    IsSystem = false,
                    Path = "Test Path",
                    Name = "Test Name",
                    Description = "Test desc",
                    Blocks = new List<dynamic> { new {
                            IsSystem = true,
                            BlockTypeId = 1,
                            Zone = "TestZone",
                            Order = 3,
                            Name = "FooInstance",
                            OutputCacheDuration = 0
                        }
                    }
                };

                var json = obj.ToJson();
                var blockType = BlockType.FromJson( json );
                var blocks = blockType.Blocks;
                Assert.IsNotNull( blocks );
                Assert.IsNotEmpty( blocks );
                Assert.AreEqual( blocks.First().Name, obj.Blocks[ 0 ].Name );
            }
        }

        public class TheCloneMethod
        {
            [Test]
            public void ShouldCloneObject()
            {
                var blockType = new BlockType();
                blockType.Blocks.Add(new Block());
                var result = blockType.Clone() as BlockType;
                Assert.NotNull( result );
                Assert.NotNull( result.Blocks );
                Assert.IsNotEmpty( result.Blocks );
            }
        }
    }
}
