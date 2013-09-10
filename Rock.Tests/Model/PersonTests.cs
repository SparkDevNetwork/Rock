//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Data.Entity.Spatial;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Newtonsoft.Json;
using Rock.Model;
using Assert = NUnit.Framework.Assert;
using Rock.Web.Cache;

namespace Rock.Tests.Model
{
    [TestFixture]
    public class PersonTests
    {
        /// <summary>
        /// Note: Using these Microsoft.VisualStudio.TestTools.UnitTesting Attribute markup
        /// allows the tests to be grouped by "trait" (ie, the TestCategory defined below).
        /// </summary>
        [TestClass]
        public class TheGradeProperty
        {
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Person" )]
            public void ShoulBeGrade13GraduateToday()
            {
                InitGlobalAttributesCache();
                var Person = new Person();
                Person.GraduationDate = DateTime.Now; // the "year" the person graduates.

                Assert.True( 13 == Person.Grade );
            }

            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Person" )]
            public void ShoulBeGrade12GraduateTomorrow()
            {
                InitGlobalAttributesCache();

                DateTime tomorrow = DateTime.Now.AddDays( 1 );
                SetGradeTransitionDateGlobalAttribute( tomorrow.Month, tomorrow.Day );

                var Person = new Person();
                Person.GraduationDate = DateTime.Now; // the "year" the person graduates.

                Assert.True( 12 == Person.Grade );
            }

            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Person" )]
            public void ShouldSetGraduationToNextYearIfGradeGraduationToday()
            {
                InitGlobalAttributesCache();
                var Person = new Person();
                Person.Grade = 12;
                
                Assert.True( Person.GraduationDate.Value.Year == DateTime.Now.AddYears(1).Year );
            }

            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Person" )]
            public void ShouldSetGraduationToThisYearIfGrade12GradTomorrow()
            {
                InitGlobalAttributesCache();

                DateTime tomorrow = DateTime.Now.AddDays( 1 );
                SetGradeTransitionDateGlobalAttribute( tomorrow.Month, tomorrow.Day );

                var Person = new Person();
                Person.Grade = 12;

                Assert.True( Person.GraduationDate.Value.Year == DateTime.Now.Year );
            }

            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Person" )]
            public void ShouldSetGraduationDateToGradeTransitionDate()
            {
                InitGlobalAttributesCache();

                DateTime lastMonth = DateTime.Now.AddMonths( -1 ).AddDays( -DateTime.Now.Day+1 );
                SetGradeTransitionDateGlobalAttribute( lastMonth.Month, lastMonth.Day );

                var Person = new Person();
                Person.Grade = 11;
                System.Diagnostics.Debug.WriteLine( "Person's Graduation Date: " + Person.GraduationDate.Value.ToShortDateString() );
                System.Diagnostics.Debug.WriteLine( "GradeTransitionDate: " + lastMonth.ToShortDateString() );
                Assert.True( Person.GraduationDate.Value.Month == lastMonth.Month );
                Assert.True( Person.GraduationDate.Value.Day == lastMonth.Day );
            }

            private static void InitGlobalAttributesCache()
            {
                DateTime today = DateTime.Now;
                GlobalAttributesCache globalAttributes = GlobalAttributesCache.Read();
                globalAttributes.SetValue( "GradeTransitionDate", string.Format( "{0}/{1}", today.Month, today.Day ), -1, false );
            }

            private static void SetGradeTransitionDateGlobalAttribute( int month, int day )
            {
                GlobalAttributesCache globalAttributes = GlobalAttributesCache.Read();
                globalAttributes.SetValue( "GradeTransitionDate", string.Format( "{0}/{1}", month, day ), -1, false );
            }


        }
    }
}
