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

using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Field.Types
{
    /// <summary>
    /// Unit tests for the <see cref="EventCalendarFieldType"/> field type
    /// </summary>
    [TestClass]
    public class EventCalendarFieldTypeTests
    {
        private const string PublicCalendarGuid = "8A444668-19AF-4417-9C74-09F842572974";
        private const string InternalCalendarGuid = "8C7F7F4E-1C51-41D3-9AC3-02B3F4054798";

        /// <summary>
        /// Seeds the two sample event calendars that the tests resolve by Guid.
        /// The field type reads these through <see cref="Rock.Web.Cache.EventCalendarCache"/>,
        /// which loads from the scoped mock context.
        /// </summary>
        private static void SeedEventCalendars( RockContext rockContext )
        {
            rockContext.Set<EventCalendar>().Add( new EventCalendar { Id = 1, Guid = new Guid( PublicCalendarGuid ), Name = "Public" } );
            rockContext.Set<EventCalendar>().Add( new EventCalendar { Id = 2, Guid = new Guid( InternalCalendarGuid ), Name = "Internal" } );
        }

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
            using var app = TestHelper.CreateScopedRockApp();

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
            using var app = TestHelper.CreateScopedRockApp();
            SeedEventCalendars( app.App.CreateRockContext() );

            var eventCalendarFieldType = new EventCalendarFieldType();
            string expectedResult = "Public";
            var result = eventCalendarFieldType.GetTextValue( PublicCalendarGuid, new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }

        /// <summary>
        /// Given a GUID list containg the public and internal EventCalendar GUIDs return the names.
        /// </summary>
        [TestMethod]
        public void GetTextValue_ValidEventCalendarForGuids()
        {
            using var app = TestHelper.CreateScopedRockApp();
            SeedEventCalendars( app.App.CreateRockContext() );

            var eventCalendarFieldType = new EventCalendarFieldType();
            string expectedResult = "Public, Internal";
            var result = eventCalendarFieldType.GetTextValue( $"{PublicCalendarGuid}, {InternalCalendarGuid}", new Dictionary<string, string>() );
            Assert.AreEqual( expectedResult, result );
        }
    }
}
