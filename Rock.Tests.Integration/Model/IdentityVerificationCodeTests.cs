using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Model
{
    [TestClass]
    public class IdentityVerificationCodeTests
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
