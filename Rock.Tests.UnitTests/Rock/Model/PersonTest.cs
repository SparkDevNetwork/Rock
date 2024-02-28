using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Model;
using System;

namespace Rock.Tests.UnitTests.Rock.Model
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
    }
}
