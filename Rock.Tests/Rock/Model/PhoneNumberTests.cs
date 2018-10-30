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

            Assert.Equal( "+16235553322", phoneNumber.E164Format );
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

            Assert.Equal( "+447351777888", phoneNumber.E164Format );
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

            Assert.Equal( "+44123456", phoneNumber.E164Format );
        }

        [Fact]
        public void E164NullOrEmptyStringReturnsEmptyString()
        {
            var nullPhoneNumber = new PhoneNumber();
            nullPhoneNumber.Number = null;

            Assert.Equal( string.Empty, nullPhoneNumber.E164Format );

            var whiteSpacePhoneNumber = new PhoneNumber();
            whiteSpacePhoneNumber.Number = " ";
            Assert.Equal( string.Empty, whiteSpacePhoneNumber.E164Format );
        }

        [Fact]
        public void USE164ToCountryCodeandNumber()
        {
            string e164 = "+14155552671";
            Assert.Equal( "14155552671", PhoneNumber.E164ToNumberWithCountryCode( e164 ) );
        }

        [Fact]
        public void UKE164ToCountryCodeandNumber()
        {
            string e164 = "+441536737441";
            Assert.Equal( "4401536737441", PhoneNumber.E164ToNumberWithCountryCode( e164 ) );
        }

        [Fact]
        public void NullWhiteSpaceE164ToCountryCodeandNumber()
        {
            string e164Null = null;
            Assert.Equal( string.Empty, PhoneNumber.E164ToNumberWithCountryCode( e164Null ) );

            string e164WhiteSpace = " ";
            Assert.Equal( string.Empty, PhoneNumber.E164ToNumberWithCountryCode( e164WhiteSpace ) );
        }

        [Fact]
        public void BadFormatE164ToCountryCodeandNumber()
        {
            string badNumber = "Bad Number";
            Assert.Equal(PhoneNumber.E164ToNumberWithCountryCode( badNumber ), string.Empty );
        }

        [Fact]
        public void ToStringUnlisted()
        {
            var unlistedNumber = new PhoneNumber();
            unlistedNumber.Number = "123";
            unlistedNumber.IsUnlisted = true;

            Assert.Equal("Unlisted", unlistedNumber.ToString() );
        }
    }
}
