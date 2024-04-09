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
    /// <summary>
    /// Unit tests for the <see cref="EventCalendarFieldType"/> field type
    /// </summary>
    [TestClass]
    public class EventCalendarFieldTypeTests : DatabaseTestsBase
    {
        /// <summary>
        /// Given an empty string the text value should be an empty string.
        /// </summary>
        [TestMethod]
        public void GetTextValue_EmptyString()
        {
            var eventCalendarFieldType = new EventCalendarFieldType();
            var expectedResult = string.Empty;
            var result = eventCalendarFieldType.GetTextValue( string.Empty, new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }

        /// <summary>
        /// Given a null value the text value should be null
        /// </summary>
        [TestMethod]
        public void GetTextValue_Null()
        {
            var eventCalendarFieldType = new EventCalendarFieldType();
            string expectedResult = null;
            var result = eventCalendarFieldType.GetTextValue( null, new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }

        /// <summary>
        /// Given a GUID that does not corraspond to an EventCalendar the text value should be the GUID.
        /// </summary>
        [TestMethod]
        public void GetTextValue_NoValidEventCalendarForTheGuid()
        {
            var eventCalendarFieldType = new EventCalendarFieldType();
            string expectedResult = System.Guid.NewGuid().ToString();
            var result = eventCalendarFieldType.GetTextValue( expectedResult, new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }

        /// <summary>
        /// Given the GUID for the public calendar retun the name of the calendar.
        /// </summary>
        [TestMethod]
        public void GetTextValue_ValidEventCalendarForGuid()
        {
            var eventCalendarFieldType = new EventCalendarFieldType();
            string expectedResult = "Public";
            var result = eventCalendarFieldType.GetTextValue( "8A444668-19AF-4417-9C74-09F842572974", new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }

        /// <summary>
        /// Given a GUID list containg the public and internal EventCalendar GUIDs return the names.
        /// </summary>
        [TestMethod]
        public void GetTextValue_ValidEventCalendarForGuids()
        {
            var eventCalendarFieldType = new EventCalendarFieldType();
            string expectedResult = "Public, Internal";
            var result = eventCalendarFieldType.GetTextValue( "8A444668-19AF-4417-9C74-09F842572974, 8C7F7F4E-1C51-41D3-9AC3-02B3F4054798", new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }
    }
}
