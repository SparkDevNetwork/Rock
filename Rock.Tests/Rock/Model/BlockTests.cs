using Rock.Model;
using Xunit;

namespace Rock.Tests.Rock.Model
{
    public class BlockTests
    {
        /// <summary>
        /// Should perform a shallow copy of a Block object, resulting in a new Block.
        /// </summary>
        [Fact]
        public void ShallowClone()
        {
            var block = new Block { Name = "Foo" };
            var result = block.Clone( false );
            Assert.Equal( result.Name, block.Name );
        }

        /// <summary>
        /// Should perform a shallow copy of a Block, including its BlockType.
        /// </summary>
        [Fact]
        public void Clone()
        {
            var block = new Block { BlockType = new BlockType() };
            var result = block.Clone() as Block;
            Assert.NotNull( result );
            Assert.NotNull( result.BlockType );
        }

        /// <summary>
        /// Should serialize a Block into a non-empty string.
        /// </summary>
        [Fact]
        public void ToJson()
        {
            var block = new Block { Name = "Foo" };
            var result = block.ToJson();
            Assert.NotNull( result );
            Assert.NotEmpty( result );
        }

        /// <summary>
        /// Should serialize a HtmlContent into a JSON string.
        /// </summary>
        [Fact( Skip = "Need a mock for Global Attributes" )]
        public void ExportJson()
        {
            var block = new Block
            {
                Name = "Foo"
            };

            var result = block.ToJson();
            const string key = "\"Name\":\"Foo\"";
            Assert.NotEqual( result.IndexOf( key ), -1 );
        }

        /// <summary>
        /// Should take a JSON string and copy its contents to a new Block, including its BlockType
        /// </summary>
        [Fact( Skip = "Missing IsCommon in JSON" )]
        public void ImportJson()
        {
            var obj = new
            {
                IsSystem = true,
                BlockTypeId = 1,
                Zone = "TestZone",
                Order = 3,
                Name = "FooInstance",
                OutputCacheDuration = 0,
                BlockType = new
                {
                    IsSystem = false,
                    IsCommon = false,
                    Path = "Test Path",
                    Name = "Test Name",
                    Description = "Test desc"
                }
            };

            var json = obj.ToJson();
            var block = Block.FromJson( json );
            var blockType = block.BlockType;
            Assert.NotNull( blockType );
            Assert.Equal( blockType.Name, obj.BlockType.Name );
        }
    }
}
