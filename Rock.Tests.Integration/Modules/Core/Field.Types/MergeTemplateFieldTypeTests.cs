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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Field.Types;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Field.Types
{
    [TestClass]
    public class MergeTemplateFieldTypeTests : DatabaseTestsBase
    {
        /// <summary>
        /// Given an empty string value return an empty string.
        /// </summary>
        [TestMethod]
        public void GetTextValue_EmptyString()
        {
            var mergeTemplateFieldType = new MergeTemplateFieldType();
            var expectedResult = string.Empty;
            var result = mergeTemplateFieldType.GetTextValue( string.Empty, new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }

        /// <summary>
        /// Given a null value return a null value.
        /// </summary>
        [TestMethod]
        public void GetTextValue_Null()
        {
            var mergeTemplateFieldType = new MergeTemplateFieldType();
            string expectedResult = null;
            var result = mergeTemplateFieldType.GetTextValue( null, new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }

        /// <summary>
        /// Given a GUID not associated with a MergeTemplate return the GUID.
        /// </summary>
        [TestMethod]
        public void GetTextValue_NoValidMergeTemplateForGuid()
        {
            var mergeTemplateFieldType = new MergeTemplateFieldType();
            string expectedResult = System.Guid.NewGuid().ToString();
            var result = mergeTemplateFieldType.GetTextValue( expectedResult, new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }

        /// <summary>
        /// Given the GUID for the "Sample Letter" MergeTemplate return the name of the template.
        /// </summary>
        [TestMethod]
        public void GetTextValue_ValidMetricForGuid()
        {
            var mergeTemplateFieldType = new MergeTemplateFieldType();
            string expectedResult = "Sample Letter";
            var result = mergeTemplateFieldType.GetTextValue( "9FEE4B0F-E5A4-6997-4620-60CAA0964D19", new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }
    }
}
