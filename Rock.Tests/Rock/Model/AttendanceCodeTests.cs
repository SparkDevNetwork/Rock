using Rock.Model;
using Xunit;

namespace Rock.Tests.Rock.Model
{
    public class AttendanceCodeTests
    {
        [Fact]
        public void AvoidTripleSix()
        {
            int alphaNumericLength = 0;
            int alphaLength = 0;
            int numericLength = 4;
            bool isRandomized = false;
            string lastCode = "0665";

            string code = AttendanceCodeService.GetNextNumericCodeAsString( alphaNumericLength, alphaLength, numericLength, isRandomized, lastCode );
            Assert.Equal( "0667", code );
        }
    }
}
