//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Rock.Model;
using Assert = NUnit.Framework.Assert;

namespace Rock.Tests.Model
{
    /// <summary>
    /// Unit tests for the Rock.Model.BlockType class
    /// </summary>
    [TestFixture]
    public class BlockTypeTests
    {
        /// <summary>
        /// Tests for the CopyProperties method
        /// </summary>
        [TestClass]
        public class TheCopyPropertiesMethod
        {
            /// <summary>
            /// Should perform a shallow copy of a BlockType object, resulting in a new BlockType.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.BlockType" )]
            public void ShouldCopyProperties()
            {
                var blockType = new BlockType { Name = "some block type" };
                var result = blockType.Clone( false );
                Assert.AreEqual( result.Name, blockType.Name );
            }
        }

        /// <summary>
        /// Tests for the ToJson method
        /// </summary>
        [TestClass]
        public class TheToJsonMethod
        {
            /// <summary>
            /// Should serialize a BlockType into a non-empty string.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.BlockType" )]
            public void ShouldNotBeEmpty()
            {
                var blockType = new BlockType { Name = "some block type" };
                var result = blockType.ToJson();
                Assert.IsNotEmpty( result );
            }

            /// <summary>
            /// Shoulds serialize a BlockType into a JSON string.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.BlockType" )]
            public void ShouldExportAsJson()
            {
                var blockType = new BlockType
                {
                    Name = "Foo"
                };
                var result = blockType.ToJson();
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
            /// Should take a JSON string and copy its contents to a Rock.Model.BlockType instance
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.BlockType" )]
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

            /// <summary>
            /// Should deserialize a JSON string and restore a BlockType and it's collection of Blocks.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.BlockType" )]
            public void ShouldImportBlocks()
            {
                var obj = new
                    {
                        IsSystem = false,
                        Path = "Test Path",
                        Name = "Test Name",
                        Description = "Test desc",
                        Blocks = new List<dynamic>
                            {
                                new
                                    {
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

        /// <summary>
        /// Tests for the Clone method
        /// </summary>
        [TestClass]
        public class TheCloneMethod
        {
            /// <summary>
            /// Should perform a shallow copy of a BlockType, including its collection of Blocks.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.BlockType" )]
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
