using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Core.Model
{
    [TestClass]
    [Ignore( "These tests require additional data. Rows are expected in the IdentityVerificationCode table." )]
    public class IdentityVerificationCodeTests : DatabaseTestsBase
    {
        [TestMethod]
        public void GetRandomPhoneVerificationCodeReturnsValue()
        {
            var code = IdentityVerificationCodeService.GetRandomIdentityVerificationCode();

            Assert.IsNotNull( code );
        }

        [TestMethod]
        public void GetRandomPhoneVerificationCodeReturnShouldHaveLastIssueDateTimeSet()
        {
            var code = IdentityVerificationCodeService.GetRandomIdentityVerificationCode();
            var expectedIssueDateTime = RockDateTime.Now;

            Assert.IsNotNull( code );
            Assert.IsNotNull( code.LastIssueDateTime );
            Assert.AreEqual( expectedIssueDateTime.Date, code.LastIssueDateTime.Value.Date );
            Assert.AreEqual( expectedIssueDateTime.Hour, code.LastIssueDateTime.Value.Hour );
            Assert.AreEqual( expectedIssueDateTime.Minute, code.LastIssueDateTime.Value.Minute );
        }
    }
}
