using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    [Ignore( "These tests require additional data. Rows are expected in the IdentityVerificationCode table." )]
    public class IdentityVerificationCodeTests : DatabaseTestsBase
    {
        [TestMethod]
        public void GetRandomPhoneVerificationCodeReturnsValue()
        {
            var code = IdentityVerificationCodeService.GetRandomIdentityVerificationCode();

            Assert.That.IsNotNull( code );
        }

        [TestMethod]
        public void GetRandomPhoneVerificationCodeReturnShouldHaveLastIssueDateTimeSet()
        {
            var code = IdentityVerificationCodeService.GetRandomIdentityVerificationCode();
            var expectedIssueDateTime = RockDateTime.Now;

            Assert.That.IsNotNull( code );
            Assert.That.IsNotNull( code.LastIssueDateTime );
            Assert.That.AreEqual( expectedIssueDateTime.Date, code.LastIssueDateTime.Value.Date );
            Assert.That.AreEqual( expectedIssueDateTime.Hour, code.LastIssueDateTime.Value.Hour );
            Assert.That.AreEqual( expectedIssueDateTime.Minute, code.LastIssueDateTime.Value.Minute );
        }
    }
}
