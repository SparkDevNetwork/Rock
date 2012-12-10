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
                dynamic result = block.ToDynamic( true );
                Assert.AreEqual( result.Name, block.Name );
            }

            [Test]
            public void ShouldCopyBlock()
            {
                var block = new Block { BlockType = new BlockType() };
                dynamic result = block.ToDynamic( true );
                Assert.NotNull( result.BlockType );
            }
        }

        public class TheExportJsonMethod
        {
            [Test]
            public void ShouldNotBeEmpty()
            {
                var block = new Block() { Name = "Foo" };
                var result = block.ToJson( true );
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
                block.FromJson( json );
                Assert.AreEqual( obj.Name, block.Name );
                Assert.AreEqual( obj.IsSystem, block.IsSystem );
            }

        }
    }
}
