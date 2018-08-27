using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Rock.Model
{
    [TestClass]
    public class PersonTests
    {
        [TestMethod]
        public void GraduatesThisYear()
        {
            InitGlobalAttributesCache();
            var Person = new Person();
            Person.GradeOffset = 1;

            Assert.IsTrue( Person.GraduationYear == RockDateTime.Now.AddYears( 1 ).Year );
        }

        private static void InitGlobalAttributesCache()
        {
            DateTime today = RockDateTime.Now;
            GlobalAttributesCache globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "GradeTransitionDate", string.Format( "{0}/{1}", today.Month, today.Day ), false );
        }
    }
}
