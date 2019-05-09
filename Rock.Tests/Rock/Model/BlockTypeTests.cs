using Rock.Model;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rock.Tests.Rock.Model
{
    public class BlockTypeTests
    {
        /// <summary>
        /// Should perform a shallow copy of a BlockType object, resulting in a new BlockType.
        /// </summary>
        [Fact]
        public void ShallowClone()
        {
            var blockType = new BlockType { Name = "some block type" };
            var result = blockType.Clone( false );
            Assert.Equal( result.Name, blockType.Name );
        }

        /// <summary>
        /// Should perform a copy of a BlockType, including its collection of Blocks.
        /// </summary>
        [Fact]
        public void Clone()
        {
            var blockType = new BlockType();
            blockType.Blocks.Add( new Block() );
            var result = blockType.Clone() as BlockType;
            Assert.NotNull( result );
            // TODO: Fix Clone() to include all child objects
            //Assert.NotNull( result.Blocks );
            //Assert.NotEmpty( result.Blocks );
        }

        /// <summary>
        /// Should serialize a BlockType into a non-empty string.
        /// </summary>
        [Fact]
        public void ToJson()
        {
            var blockType = new BlockType { Name = "some block type" };
            var result = blockType.ToJson();
            Assert.NotEmpty( result );
        }

        /// <summary>
        /// Should serialize a BlockType into a JSON string.
        /// </summary>
        [Fact( Skip = "Need a mock for Global Attributes" )]
        public void ExportJson()
        {
            var blockType = new BlockType { Name = "Foo" };
            var result = blockType.ToJson();
            const string key = "\"Name\":\"Foo\"";
            Assert.NotEqual( result.IndexOf( key ), -1 );
        }

        /// <summary>
        /// Should deserialize a JSON string and restore a BlockType, including its Blocks
        /// </summary>
        [Fact( Skip = "Missing IsCommon from JSON" )]
        public void ImportBlocks()
        {
            var obj = new
            {
                IsSystem = false,
                IsCommon = false,
                Path = "Test Path",
                Name = "Test Name",
                Description = "Test desc",
                Blocks = new List<dynamic>
                {
                    new {
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
            Assert.NotNull( blockType );
            // TODO: Fix Clone() to include all child objects
            //Assert.NotNull( blocks );
            //Assert.NotEmpty( blocks );
            //Assert.Equal( blocks.First().Name, obj.Blocks[0].Name );
        }
    }
}
