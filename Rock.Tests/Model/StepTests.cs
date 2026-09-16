using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared.Utility;

namespace Rock.Tests.Model
{
    [TestClass]
    public class StepTests
    {
        [TestMethod]
        public void StepCompletedDateKeyGetsSetCorrectly()
        {
            var testList = DateTimeTestHelper.GetDateKeyTestData();

            foreach ( var keyValue in testList )
            {
                var step = new Rock.Model.Step();
                step.CompletedDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, step.CompletedDateKey );
            }
        }

        [TestMethod]
        public void StepStartDateKeyGetsSetCorrectly()
        {
            var testList = DateTimeTestHelper.GetDateKeyTestData();

            foreach ( var keyValue in testList )
            {
                var step = new Rock.Model.Step();
                step.StartDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, step.StartDateKey );
            }
        }

        [TestMethod]
        public void StepEndDateKeyGetsSetCorrectly()
        {
            var testList = DateTimeTestHelper.GetDateKeyTestData();

            foreach ( var keyValue in testList )
            {
                var step = new Rock.Model.Step();
                step.EndDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, step.EndDateKey );
            }
        }

        [TestMethod]
        public void StepCompletedDateKeyWorksWithNullValue()
        {
            var step = new Rock.Model.Step();
            step.CompletedDateTime = null;
            Assert.IsNull( step.CompletedDateKey );
        }

        [TestMethod]
        public void StepStartDateKeyWorksWithNullValue()
        {
            var step = new Rock.Model.Step();
            step.StartDateTime = null;
            Assert.IsNull( step.StartDateKey );
        }

        [TestMethod]
        public void StepEndDateKeyGetsWorksWithNullValue()
        {
            var step = new Rock.Model.Step();
            step.EndDateTime = null;
            Assert.IsNull( step.EndDateKey );
        }
    }
}
