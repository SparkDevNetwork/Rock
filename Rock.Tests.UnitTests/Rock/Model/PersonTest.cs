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
            person.SetBirthDate( RockDateTime.Now );

            var expectedAge = 25;

            person.BirthYear = RockDateTime.Now.AddYears( -expectedAge ).Year;

            Assert.AreEqual( expectedAge, person.Age );
        }
    }
}
