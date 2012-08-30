//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using Rock.Cms;
using Xunit;

namespace Rock.Tests.Cms
{
    public class BlockTests
    {
        public class TheExportObjectMethod
        {
            [Fact]
            public void ShouldCopyEntity()
            {
                var block = new Block() {Name = "some block"};
                dynamic result = block.ExportObject();
                Assert.Equal( result.Name, block.Name );
            }
        }

        public class TheExportJsonMethod
        {
            [Fact]
            public void ShouldNotBeEmpty()
            {
                var block = new Block() { Name = "some block" };
                var result = block.ExportJson();
                Assert.NotEmpty( result );
            }
        }
    }
}
