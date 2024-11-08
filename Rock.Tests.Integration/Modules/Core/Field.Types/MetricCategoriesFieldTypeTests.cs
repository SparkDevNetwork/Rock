// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Field.Types;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Field.Types
{
    /// <summary>
    /// Defines test class MetricCategoriesFieldTypeTests.
    /// </summary>
    [TestClass]
    public class MetricCategoriesFieldTypeTests : DatabaseTestsBase
    {
        /// <summary>
        /// Given an empty string value return an empty string.
        /// </summary>
        [TestMethod]
        public void GetTextValue_EmptyString()
        {
            var metricCategoriesFieldTypeFieldType = new MetricCategoriesFieldType();
            var expectedResult = string.Empty;
            var result = metricCategoriesFieldTypeFieldType.GetTextValue( string.Empty, new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }

        /// <summary>
        /// Given a null value return an empty string.
        /// </summary>
        [TestMethod]
        public void GetTextValue_Null()
        {
            var metricCategoriesFieldTypeFieldType = new MetricCategoriesFieldType();
            string expectedResult = string.Empty;
            var result = metricCategoriesFieldTypeFieldType.GetTextValue( null, new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }

        /// <summary>
        /// Given a GUID not associated with a Metric return an empty string.
        /// </summary>
        [TestMethod]
        public void GetTextValue_NoValidMetricForGuid()
        {
            var metricCategoriesFieldTypeFieldType = new MetricCategoriesFieldType();
            string expectedResult = string.Empty;
            var result = metricCategoriesFieldTypeFieldType.GetTextValue( System.Guid.NewGuid().ToString(), new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }

        /// <summary>
        /// Given a pipe seperated guid par of Metric|Category for Adult Attendance return the title of the metric
        /// </summary>
        [TestMethod]
        public void GetTextValue_ValidMetricForGuid()
        {
            var metricCategoryValue = "0D126800-2FDA-4B34-96FD-9BAE76F3A89A|64B29ADE-144D-4E84-96CC-A79398589733";
            var metricCategoriesFieldTypeFieldType = new MetricCategoriesFieldType();
            string expectedResult = "Adult Attendance";
            var result = metricCategoriesFieldTypeFieldType.GetTextValue( metricCategoryValue, new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }

        /// <summary>
        /// Given a list of pipe seperated guid pairs of Metric|Category return a csv of Metric.Title
        /// Note this test also orders the results to compare with the expected result.
        /// </summary>
        [TestMethod]
        public void GetTextValue_ValidMetricsForGuids()
        {
            var metricCategoryValues = "0D126800-2FDA-4B34-96FD-9BAE76F3A89A|64B29ADE-144D-4E84-96CC-A79398589733,34EA42B9-1142-43DA-8A8B-AA1864A1CA72|64B29ADE-144D-4E84-96CC-A79398589733,491061B7-1834-44DA-8EA1-BB73B2D52AD3|073ADD0C-B1F3-43AB-8360-89A1CE05A95D,64D538D0-EE05-4646-91F5-EBE06460BDAB|370FBBD8-7766-4B3F-81A9-F13EE819A832,68C54F46-A99E-4DD1-91CA-FC5941E6CFBE|370FBBD8-7766-4B3F-81A9-F13EE819A832,8A1F73DD-4275-47C0-AF2A-6EABDA06E3C7|370FBBD8-7766-4B3F-81A9-F13EE819A832,ECB1B552-9A3D-46FC-952B-D57DBC4A329D|073ADD0C-B1F3-43AB-8360-89A1CE05A95D,F0A24208-F8AC-4E04-8309-1A276885F6A6|073ADD0C-B1F3-43AB-8360-89A1CE05A95D,F90F9446-8754-4001-887C-1AB920968C6D|370FBBD8-7766-4B3F-81A9-F13EE819A832";
            var metricCategoriesFieldTypeFieldType = new MetricCategoriesFieldType();
            string expectedResult = @"Active Connection Requests, Active Families, Active Records, Adult Attendance, Hard Connects Per Second, Number of Active Connections, Number of Free Connections, Salvations, Soft Connects Per Second";
            var result = metricCategoriesFieldTypeFieldType.GetTextValue( metricCategoryValues, new Dictionary<string, string>() );
            var resultOrderedList = result.Split( ',' ).Select( a => a.Trim() ).ToList().OrderBy( a => a ).ToList();
            var resultOrderedString = string.Join( ", ", resultOrderedList );
            Assert.AreEqual( expectedResult, resultOrderedString );
        }
    }
}
