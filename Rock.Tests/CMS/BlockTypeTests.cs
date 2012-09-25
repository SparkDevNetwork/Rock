//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using Rock.Cms;
using Xunit;

namespace Rock.Tests.Cms
{
    public class BlockTypeTests
    {
        public class TheExportObjectMethod
        {
            [Fact]
            public void ShouldCopyEntity()
            {
                var blockType = new BlockType() {Name = "some block type"};
                dynamic result = blockType.ExportObject();
                Assert.Equal( result.Name, blockType.Name );
            }
        }

        public class TheExportJsonMethod
        {
            [Fact]
            public void ShouldNotBeEmpty()
            {
                var blockType = new BlockType() { Name = "some block type" };
                var result = blockType.ExportJson();
                Assert.NotEmpty( result );
            }
        }
    }
}
