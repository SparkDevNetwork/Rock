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
    public class MediaWatchFieldTypeTests : DatabaseTestsBase
    {
        /// <summary>
        /// Given an empty string the text value should be an empty string.
        /// </summary>
        [TestMethod]
        public void GetTextValue_EmptyString()
        {
            var mediaWatchFieldType = new MediaWatchFieldType();
            var expectedResult = string.Empty;
            var result = mediaWatchFieldType.GetTextValue( string.Empty, new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }

        /// <summary>
        /// Given a null value the text value should be null
        /// </summary>
        [TestMethod]
        public void GetTextValue_Null()
        {
            var mediaWatchFieldType = new MediaWatchFieldType();
            string expectedResult = null;
            var result = mediaWatchFieldType.GetTextValue( null, new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }

        /// <summary>
        /// Given a non empty string return a new string with the text "% watched" appended.
        /// Note: The field type method <strong>does not</strong> verify the input is a valid number between 0-100.
        /// </summary>
        [TestMethod]
        public void GetTextValue_NoValidEventCalendarForTheGuid()
        {
            var eventCalendarFieldType = new MediaWatchFieldType();
            string expectedResult = "10% watched";
            var result = eventCalendarFieldType.GetTextValue( "10", new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }
    }
}
