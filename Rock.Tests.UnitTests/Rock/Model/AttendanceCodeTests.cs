using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Model;
using Rock.Tests.Shared;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Tests.UnitTests.Rock.Model
{
    [TestClass]
    public class AttendanceCodeTests
    {
        [TestMethod]
        public void EnsureGetNextNumericCodeAsStringReturnsGoodCodesTest1()
        {
            int alphaNumericLength = 0;
            int alphaLength = 2;
            int numericLength = 4;
            bool isRandomized = false;
            string lastCode = "AN0665";

            var generatedCode = AttendanceCodeService.GetNextNumericCodeAsString( alphaNumericLength, alphaLength, numericLength, isRandomized, lastCode );
            Assert.AreEqual( "0667", generatedCode );
        }

        [TestMethod]
        public void EnsureGetNextNumericCodeAsStringReturnsGoodCodesTest2()
        {
            int alphaNumericLength = 0;
            int alphaLength = 2;
            int numericLength = 4;
            bool isRandomized = false;
            string lastCode = "AN6665";

            var generatedCode = AttendanceCodeService.GetNextNumericCodeAsString( alphaNumericLength, alphaLength, numericLength, isRandomized, lastCode );
            Assert.AreEqual( "6670", generatedCode );
        }

    }
}
