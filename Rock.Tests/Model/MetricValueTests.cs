using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared.Utility;

namespace Rock.Tests.Model
{
    [TestClass]
    public class MetricValueTests
    {
        [TestMethod]
        public void MetricValueDateKeyGetsSetCorrectly()
        {
            var testList = DateTimeTestHelper.GetDateKeyTestData();

            foreach ( var keyValue in testList )
            {
                var metricValue = new Rock.Model.MetricValue();
                metricValue.MetricValueDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, metricValue.MetricValueDateKey );
            }
        }

        [TestMethod]
        public void MetricValueDateKeyKeyWorksWithNullValue()
        {
            var metricValue = new Rock.Model.MetricValue();
            metricValue.MetricValueDateTime = null;
            Assert.IsNull( metricValue.MetricValueDateKey );
        }
    }
}
