using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Model
{
    [TestClass]
    public class FinancialAccountServiceTests : DatabaseTestsBase
    {
        [TestMethod]
        public void GetTree_IncludesAllRequiredColumns()
        {
            // This test checks that the GetTree method does not throw and
            // exception due to missing columns in the query.
            using ( var rockContext = new RockContext() )
            {
                var financialAccountService = new FinancialAccountService( rockContext );

                // This will throw an exception if any required columns are missing.
                var count = financialAccountService.GetTree().Count();

                Assert.AreNotEqual( 0, count, "Expected to retrieve financial accounts, but got none." );
            }
        }
    }
}
