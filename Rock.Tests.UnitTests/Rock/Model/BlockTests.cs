using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Rock.Model
{
    [TestClass]
    public class BlockTests
    {
        /// <summary>
        /// Should perform a shallow copy of a Block object, resulting in a new Block.
        /// </summary>
        [TestMethod]
        public void ShallowClone()
        {
            var block = new Block { Name = "Foo" };
            var result = block.Clone( false );
            Assert.That.AreEqual( result.Name, block.Name );
        }

        /// <summary>
        /// Should perform a shallow copy of a Block, including its BlockType.
        /// </summary>
        [TestMethod]
        public void Clone()
        {
            var block = new Block { BlockType = new BlockType() };
            var result = block.Clone() as Block;
            Assert.That.IsNotNull( result );
            Assert.That.IsNotNull( result.BlockType );
        }

        /// <summary>
        /// Should serialize a Block into a non-empty string.
        /// </summary>
        [TestMethod]
        public void ToJson()
        {
            var block = new Block { Name = "Foo" };
            var result = block.ToJson();
            Assert.That.IsNotEmpty( result );
        }

        /// <summary>
        /// Should serialize a HtmlContent into a JSON string.
        /// </summary>
        [TestMethod]
        public void ExportJson()
        {
            var block = new Block
            {
                Name = "Foo"
            };

            var result = block.ToJson();
            const string key = "\"Name\":\"Foo\"";
            Assert.That.AreNotEqual( result.IndexOf( key ), -1 );
        }

        /// <summary>
        /// Should take a JSON string and copy its contents to a new Block, including its BlockType
        /// </summary>
        [TestMethod]
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
            Assert.That.IsNotNull( blockType );
            Assert.That.AreEqual( blockType.Name, obj.BlockType.Name );
        }
    }
}