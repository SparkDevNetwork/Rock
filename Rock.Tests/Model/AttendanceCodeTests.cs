using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;

namespace Rock.Tests.Model
{
    [TestClass]
    public class AttendanceCodeTests
    {
        [TestMethod]
        public void EnsureGetNextNumericCodeAsStringReturnsGoodCodesTest1()
        {
            int prefixLength = 2;
            int numericLength = 4;
            string lastCode = "AN0665";

            var generatedCode = AttendanceCodeService.GetSequentialNumericCodeAsString( prefixLength, numericLength, lastCode );
            Assert.AreEqual( "0667", generatedCode );
        }

        [TestMethod]
        public void EnsureGetNextNumericCodeAsStringReturnsGoodCodesTest2()
        {
            int prefixLength = 2;
            int numericLength = 4;
            string lastCode = "AN6665";

            var generatedCode = AttendanceCodeService.GetSequentialNumericCodeAsString( prefixLength, numericLength, lastCode );
            Assert.AreEqual( "6670", generatedCode );
        }

    }
}
