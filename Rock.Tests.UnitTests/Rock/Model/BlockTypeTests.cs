using Rock.Model;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;

namespace Rock.Tests.Rock.Model
{
    [TestClass]
    public class BlockTypeTests
    {
        /// <summary>
        /// Should perform a shallow copy of a BlockType object, resulting in a new BlockType.
        /// </summary>
        [TestMethod]
        public void ShallowClone()
        {
            var blockType = new BlockType { Name = "some block type" };
            var result = blockType.Clone( false );
            Assert.That.AreEqual( result.Name, blockType.Name );
        }

        /// <summary>
        /// Should perform a copy of a BlockType, including its collection of Blocks.
        /// </summary>
        [TestMethod]
        public void Clone()
        {
            var blockType = new BlockType();
            blockType.Blocks.Add( new Block() );
            var result = blockType.Clone() as BlockType;
            Assert.That.IsNotNull( result );
            // TODO: Fix Clone() to include all child objects
            //Assert.That.IsNotNull( result.Blocks );
            //Assert.That.IsNotEmpty( result.Blocks );
        }

        /// <summary>
        /// Should serialize a BlockType into a non-empty string.
        /// </summary>
        [TestMethod]
        public void ToJson()
        {
            var blockType = new BlockType { Name = "some block type" };
            var result = blockType.ToJson();
            Assert.That.IsNotEmpty( result );
        }

        /// <summary>
        /// Should serialize a BlockType into a JSON string.
        /// </summary>
        [TestMethod]
        public void ExportJson()
        {
            var blockType = new BlockType { Name = "Foo" };
            var result = blockType.ToJson();
            const string key = "\"Name\":\"Foo\"";
            Assert.That.AreNotEqual( result.IndexOf( key ), -1 );
        }

        /// <summary>
        /// Should deserialize a JSON string and restore a BlockType, including its Blocks
        /// </summary>
        [TestMethod]
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
            Assert.That.IsNotNull( blockType );
            // TODO: Fix Clone() to include all child objects
            //Assert.That.IsNotNull( blocks );
            //Assert.That.IsNotEmpty( blocks );
            //Assert.That.AreEqual( blocks.First().Name, obj.Blocks[0].Name );
        }
    }
}