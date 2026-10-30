using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;

namespace Rock.Tests.Model
{
    [TestClass]
    public class PersonTest
    {
        [TestMethod]
        public void ShouldUpdateAgeOnSetBirthdate()
        {
            var expectedAge = 25;
            Person person = new Person();
            person.SetBirthDate( RockDateTime.Now.AddYears( -expectedAge ) );

            Assert.AreEqual( expectedAge, person.Age );
        }

        [TestMethod]
        public void ShouldUpdateAgeOnBirthYearChange()
        {
            Person person = new Person();
            var now = RockDateTime.Now;

            // Changing the BirthYear directly ends up creating an invalid
            // date on leap days. So adjust for that.
            if ( now.Month == 2 && now.Day == 29 )
            {
                now = now.AddDays( -1 );
            }

            person.SetBirthDate( now );

            var expectedAge = 25;

            person.BirthYear = now.AddYears( -expectedAge ).Year;

            Assert.AreEqual( expectedAge, person.Age );
        }

        [TestMethod]
        public void GetAnchorTag_WithHtmlInName_EncodesName()
        {
            var person = new Person
            {
                Id = 1,
                NickName = "<img src=x onerror=alert(1)>",
                LastName = "O'Brien & Sons"
            };

            var anchorTag = person.GetAnchorTag( "/" );

            Assert.AreEqual( "<a class='' href='/Person/1'>&lt;img src=x onerror=alert(1)&gt; O&#39;Brien &amp; Sons</a>", anchorTag );
        }

        [TestMethod]
        public void GetAnchorTag_WithQuoteInCssClass_EncodesCssClass()
        {
            var person = new Person
            {
                Id = 1,
                NickName = "Ted",
                LastName = "Decker"
            };

            var anchorTag = person.GetAnchorTag( "/", "x' onmouseover='y" );

            Assert.AreEqual( "<a class='x&#39; onmouseover=&#39;y' href='/Person/1'>Ted Decker</a>", anchorTag );
        }
    }
}
