using Rock.Model;
using Xunit;

namespace Rock.Tests.Rock.Model
{
    public class PhoneNumberTests
    {
        /// <summary>
        /// US numbers should not have any characters stripped
        /// </summary>
        [Fact]
        public void USNumberE164FormatMatches()
        {
            var phoneNumber = new PhoneNumber();
            phoneNumber.Number = "6235553322";
            phoneNumber.CountryCode = "1"; // This would be set on pre-save if not specified

            Assert.Equal( phoneNumber.E164Format, "+16235553322" );
        }

        /// <summary>
        /// UK numbers shoukd have a 0 stripped if they start with a 0
        /// </summary>
        [Fact]
        public void UKNumberWithLeading0E164FormatMatches()
        {
            var phoneNumber = new PhoneNumber();
            phoneNumber.Number = "07351777888";
            phoneNumber.CountryCode = "44";

            Assert.Equal( phoneNumber.E164Format, "+447351777888" );
        }

        /// <summary>
        /// UK numbers without a leading 0 should be unchanged
        /// </summary>
        [Fact]
        public void UKNumberWithoutLeading0E164FormatMatches()
        {
            var phoneNumber = new PhoneNumber();
            phoneNumber.Number = "123456";
            phoneNumber.CountryCode = "44";

            Assert.Equal( phoneNumber.E164Format, "+44123456" );
        }

        [Fact]
        public void E164NullOrEmptyStringReturnsEmptyString()
        {
            var nullPhoneNumber = new PhoneNumber();
            nullPhoneNumber.Number = null;

            Assert.Equal( nullPhoneNumber.E164Format, string.Empty );

            var whiteSpacePhoneNumber = new PhoneNumber();
            whiteSpacePhoneNumber.Number = " ";
            Assert.Equal( whiteSpacePhoneNumber.E164Format, string.Empty );
        }

    }
}
