//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Rock.Model;
using Assert = NUnit.Framework.Assert;

namespace Rock.Tests.Model
{
    /// <summary>
    /// Unit tests for the Rock.Model.Block class
    /// </summary>
    [TestFixture]
    public class BlockTests
    {
        /// <summary>
        /// Tests for the CopyProperties method
        /// </summary>
        [TestClass]
        public class TheCopyPropertiesMethod
        {
            /// <summary>
            /// Should perform a shallow copy of a Block object, resulting in a new Block.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Block" )]
            public void ShouldCopyProperties()
            {
                var block = new Block { Name = "Foo" };
                var result = block.Clone( false );
                Assert.AreEqual( result.Name, block.Name );
            }
        }

        /// <summary>
        /// Tests for the ToJson method
        /// </summary>
        [TestClass]
        public class TheToJsonMethod
        {
            /// <summary>
            /// Should serialize a Block into a non-empty string.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Block" )]
            public void ShouldNotBeEmpty()
            {
                var block = new Block { Name = "Foo" };
                var result = block.ToJson();
                Assert.IsNotEmpty( result );
            }

            /// <summary>
            /// Shoulds serialize a HtmlContent into a JSON string.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Block" )]
            public void ShouldExportAsJson()
            {
                var block = new Block
                    {
                        Name = "Foo"
                    };

                var result = block.ToJson();
                const string key = "\"Name\": \"Foo\"";
                Assert.Greater( result.IndexOf( key ), -1, string.Format( "'{0}' was not found in '{1}'.", key, result ) );
            }
        }

        /// <summary>
        /// Tests for the FromJson method
        /// </summary>
        [TestClass]
        public class TheFromJsonMethod
        {
            /// <summary>
            /// Should take a JSON string and copy its contents to a Rock.Model.Block instance
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Block" )]
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

            /// <summary>
            /// Should deserialize a JSON string and restore a Block's BlockType property.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Block" )]
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
                        BlockType = new
                            {
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

        /// <summary>
        /// Tests for the Clone method
        /// </summary>
        [TestClass]
        public class TheCloneMethod
        {
            /// <summary>
            /// Should perform a shallow copy of a Block, including its BlockType.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Block" )]
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
