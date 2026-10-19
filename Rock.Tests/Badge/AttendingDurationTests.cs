using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Badge.Component;

namespace Rock.Tests.Badge
{
    /// <summary>
    /// Tests for the <see cref="AttendingDuration"/> badge.
    /// </summary>
    [TestClass]
    public class AttendingDurationTests
    {
        [TestMethod]
        public void GetTooltipAttributeValue_HtmlInNickName_IsEncodedTwice()
        {
            var tooltip = AttendingDuration.GetTooltipAttributeValue( "<img src=x onerror=alert(1)>", "is new this week." );

            Assert.AreEqual( "&amp;lt;img src=x onerror=alert(1)&amp;gt; is new this week.", tooltip );
        }

        [TestMethod]
        public void GetTooltipAttributeValue_QuoteInNickName_CannotEndAttribute()
        {
            var tooltip = AttendingDuration.GetTooltipAttributeValue( "O'Brien", "is new this week." );

            Assert.AreEqual( "O&#39;Brien is new this week.", tooltip );
        }

        [TestMethod]
        public void GetTooltipAttributeValue_AmpersandInNickName_StaysText()
        {
            var tooltip = AttendingDuration.GetTooltipAttributeValue( "Smith & Sons", "first visited 2 weeks ago." );

            Assert.AreEqual( "Smith &amp;amp; Sons first visited 2 weeks ago.", tooltip );
        }

        [TestMethod]
        public void GetTooltipAttributeValue_NullNickName_ReturnsMessage()
        {
            var tooltip = AttendingDuration.GetTooltipAttributeValue( null, "is new this week." );

            Assert.AreEqual( " is new this week.", tooltip );
        }
    }
}
