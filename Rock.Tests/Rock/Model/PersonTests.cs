using Rock.Model;
using Rock.Web.Cache;
using System;
using Xunit;

namespace Rock.Tests.Rock.Model
{
    public class PersonTests
    {
        [Fact( Skip = "Need a mock for Global Attributes" )]
        public void OffsetGraduatesToday()
        {
            InitGlobalAttributesCache();
            var person = new Person();
            person.GraduationYear = RockDateTime.Now.Year; // the "year" the person graduates.

            Assert.True( 0 == person.GradeOffset );
        }

        [Fact( Skip = "Need a mock for Global Attributes" )]
        public void OffsetGraduatesTomorrow()
        {
            InitGlobalAttributesCache();

            DateTime tomorrow = RockDateTime.Now.AddDays( 1 );
            SetGradeTransitionDateGlobalAttribute( tomorrow.Month, tomorrow.Day );

            var Person = new Person();
            Person.GraduationYear = RockDateTime.Now.Year; // the "year" the person graduates.

            Assert.True( 1 == Person.GradeOffset );
        }

        [Fact( Skip = "Need a mock for Global Attributes" )]
        public void GraduatesThisYear()
        {
            InitGlobalAttributesCache();
            var Person = new Person();
            Person.GradeOffset = 1;

            Assert.True( Person.GraduationYear == RockDateTime.Now.AddYears( 1 ).Year );
        }

        [Fact( Skip = "Need a mock for Global Attributes" )]
        public void GraduatesNextYear()
        {
            InitGlobalAttributesCache();

            DateTime tomorrow = RockDateTime.Now.AddDays( 1 );
            SetGradeTransitionDateGlobalAttribute( tomorrow.Month, tomorrow.Day );

            var Person = new Person();
            Person.GradeOffset = 1;

            Assert.True( Person.GraduationYear == RockDateTime.Now.Year );
        }

        private static void InitGlobalAttributesCache()
        {
            DateTime today = RockDateTime.Now;
            GlobalAttributesCache globalAttributes = GlobalAttributesCache.Read();
            globalAttributes.SetValue( "GradeTransitionDate", string.Format( "{0}/{1}", today.Month, today.Day ), false );
        }

        private static void SetGradeTransitionDateGlobalAttribute( int month, int day )
        {
            GlobalAttributesCache globalAttributes = GlobalAttributesCache.Read();
            globalAttributes.SetValue( "GradeTransitionDate", string.Format( "{0}/{1}", month, day ), false );
        }
    }
}