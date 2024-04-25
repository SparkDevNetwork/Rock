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
//
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core
{
    /// <summary>
    /// Tests for Entity Attributes.
    /// </summary>
    [TestClass]
    public class AttributeTests : DatabaseTestsBase
    {
        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            InitializeTestData();
        }

        [TestMethod]
        public void InheritedAttributes_ParentEntityAttributeValues_IncludesChildEntityAttributeValues()
        {
            var rockContext = new RockContext();
            var eventItemService = new EventItemService( rockContext );

            // Verify the values of the test Attribute for Event A and Event B.
            // These Attributes are defined per-Calendar and so are directly linked to the EventItemCalendar entity,
            // but they should also appear as inherited attributes of the EventItem.
            var eventA = eventItemService.Get( EventAGuid.AsGuid() );
            eventA.LoadAttributes();
            Assert.IsTrue( eventA.AttributeValues[InternalCalendarAttribute1Key].Value == "Event A Internal" );
            Assert.IsTrue( eventA.AttributeValues[PublicCalendarAttribute1Key].Value == "Event A Public" );

            var eventB = eventItemService.Get( EventBGuid.AsGuid() );
            eventB.LoadAttributes();
            Assert.IsTrue( eventB.AttributeValues[PublicCalendarAttribute1Key].Value == "Event B Public" );
            Assert.IsTrue( eventB.AttributeValues[InternalCalendarAttribute1Key].Value == null, $"Unexpected Attribute Value: {InternalCalendarAttribute1Key}." );
        }

        [TestMethod]
        [Ignore( "SaveAttributeValues() does not correctly save inherited values." )]
        public void InheritedAttributes_SetAttributeOnParentEntity_SetsValueForChildEntity()
        {
            var rockContext = new RockContext();
            var eventItemService = new EventItemService( rockContext );
            var eventCalendarService = new EventCalendarItemService( rockContext );

            // Setting the value of an inherited Attribute should persist the value for the child entity.
            var eventA = eventItemService.Get( EventAGuid.AsGuid() );
            eventA.LoadAttributes( rockContext );
            eventA.SetAttributeValue( InternalCalendarAttribute1Key, "xyzzy" );

            // TODO: SaveAttributeValues does not correctly save inherited values.
            // It wrongly attributes them to the entity on which they are set rather than the inherited entity.
            eventA.SaveAttributeValues( rockContext );

            var calendarItemInternal = eventCalendarService.Get( EventACalendarInternalGuid.AsGuid() );
            var calendarItemPublic = eventCalendarService.Get( EventACalendarPublicGuid.AsGuid() );

            calendarItemInternal.LoadAttributes();
            Assert.IsTrue( calendarItemInternal.AttributeValues[InternalCalendarAttribute1Key].Value == "xyzzy" );
            calendarItemPublic.LoadAttributes();
            Assert.IsTrue( calendarItemPublic.AttributeValues[InternalCalendarAttribute1Key].Value == "xyzzy" );
        }


        #region Test Data

        private static string EventAGuid = "7EBBD36C-0359-4DF0-8548-704B65EB7E46";
        private static string EventACalendarInternalGuid = "B83ADD19-7197-4598-AEBC-EA1D816679B4";
        private static string EventACalendarPublicGuid = "A779464B-8AB3-4C51-A1D6-CB4793374D8E";
        private static string EventBGuid = "2983F72E-044B-4576-B102-427FEBF78E0D";
        private static string EventBCalendarInternalGuid = "1297843D-B3B4-4739-B17A-823D1224BC1D";
        private static string InternalCalendarAttribute1Guid = "ACF1E091-A047-4B28-8B4C-2D3B39C42F63";
        private static string PublicCalendarAttribute1Guid = "907AF802-4B62-44FC-86E5-DF2136F38980";
        private static string EventBCalendarPublicGuid = "AE894097-9A99-4E9E-B6D1-71DC2AC14FD6";
        private static string InternalCalendarAttribute1Key = "InternalCalendarAttribute1";
        private static string PublicCalendarAttribute1Key = "PublicCalendarAttribute1";

        private static void InitializeTestData()
        {
            InitializeInheritedAttributesTestData();
        }

        /// <summary>
        /// Adds test data for Events that can be used to test inherited attributes.
        /// </summary>
        private static void InitializeInheritedAttributesTestData()
        {
            var rockContext = new RockContext();

            var EventCalendarPublicId = EventCalendarCache.All().First( x => x.Name == "Public" ).Id;
            var EventCalendarInternalId = EventCalendarCache.All().First( x => x.Name == "Internal" ).Id;

            // Add Attributes for Calendars.
            // EventItem Attributes are defined per Calendar, so they are directly associated with the EventCalendarItem entity.
            // The Attributes collection for the EventItem shows the collection of attributes inherited from the EventCalendarItems associated with the event.
            var attributeService = new AttributeService( rockContext );
            var textFieldTypeId = FieldTypeCache.GetId( SystemGuid.FieldType.TEXT.AsGuid() ).GetValueOrDefault();

            // Add Attribute A for Internal Calendar.
            var attributeAInternal = attributeService.Get( InternalCalendarAttribute1Guid.AsGuid() );
            if ( attributeAInternal != null )
            {
                attributeService.Delete( attributeAInternal );
                rockContext.SaveChanges();
            }
            attributeAInternal = new Rock.Model.Attribute();
            attributeAInternal.EntityTypeId = EntityTypeCache.GetId( typeof( EventCalendarItem ) );
            attributeAInternal.EntityTypeQualifierColumn = "EventCalendarId";
            attributeAInternal.EntityTypeQualifierValue = EventCalendarInternalId.ToString();
            attributeAInternal.Name = InternalCalendarAttribute1Key;
            attributeAInternal.Key = InternalCalendarAttribute1Key;
            attributeAInternal.Guid = InternalCalendarAttribute1Guid.AsGuid();
            attributeAInternal.FieldTypeId = textFieldTypeId;

            attributeService.Add( attributeAInternal );
            rockContext.SaveChanges();

            // Add Attribute A for Public Calendar.
            var attributeAPublic = attributeService.Get( PublicCalendarAttribute1Guid.AsGuid() );
            if ( attributeAPublic != null )
            {
                attributeService.Delete( attributeAPublic );
                rockContext.SaveChanges();
            }
            attributeAPublic = new Rock.Model.Attribute();
            attributeAPublic.EntityTypeId = EntityTypeCache.GetId( typeof( EventCalendarItem ) );
            attributeAPublic.EntityTypeQualifierColumn = "EventCalendarId";
            attributeAPublic.EntityTypeQualifierValue = EventCalendarPublicId.ToString();
            attributeAPublic.Name = PublicCalendarAttribute1Key;
            attributeAPublic.Key = PublicCalendarAttribute1Key;
            attributeAPublic.Guid = PublicCalendarAttribute1Guid.AsGuid();
            attributeAPublic.FieldTypeId = textFieldTypeId;

            attributeService.Add( attributeAPublic );
            rockContext.SaveChanges();

            // Create a new Event: "Event A".
            // This event exists in the Internal and Public calendars.
            var eventItemService = new EventItemService( rockContext );

            var eventA = eventItemService.Get( EventAGuid.AsGuid() );
            if ( eventA != null )
            {
                eventItemService.Delete( eventA );
                rockContext.SaveChanges();
            }
            eventA = new EventItem();
            eventA.Name = "Event A";
            eventA.Guid = EventAGuid.AsGuid();
            var eventACalendarInternal = new EventCalendarItem { EventCalendarId = EventCalendarInternalId, Guid = EventACalendarInternalGuid.AsGuid() };
            var eventACalendarPublic = new EventCalendarItem { EventCalendarId = EventCalendarPublicId, Guid = EventACalendarPublicGuid.AsGuid() };
            eventA.EventCalendarItems.Add( eventACalendarInternal );
            eventA.EventCalendarItems.Add( eventACalendarPublic );

            eventItemService.Add( eventA );

            rockContext.SaveChanges();

            // Create a new Event: "Event B".
            // This event exists in the Internal calendar only.
            // This event is created manually, to set the ID of Event B to match an Event Calendar Item associated with Event A.
            // The purpose is to create an EventItem and an EventCalendarItem that have an identical ID, 
            // so we can verify that Attribute values for parent and child entities are correctly differentiated
            // by Entity Type when they are collated for inherited attributes.
            var eventB = eventItemService.Get( EventBGuid.AsGuid() );
            if ( eventB != null )
            {
                eventItemService.Delete( eventB );
                rockContext.SaveChanges();
            }

            var matchedID = eventACalendarInternal.Id;

            var sql = @"
SET IDENTITY_INSERT [EventItem] ON
;
INSERT INTO [EventItem] (Id, Name, Guid, IsActive)
VALUES (@p0, @p1, @p2, 1)
;
SET IDENTITY_INSERT [EventItem] OFF
;
";
            rockContext.Database.ExecuteSqlCommand( sql, matchedID, "Event B", EventBGuid.AsGuid() );

            eventB = eventItemService.Get( EventBGuid.AsGuid() );
            var eventBCalendarInternal = new EventCalendarItem { EventCalendarId = EventCalendarInternalId, Guid = EventBCalendarInternalGuid.AsGuid() };
            var eventBCalendarPublic = new EventCalendarItem { EventCalendarId = EventCalendarPublicId, Guid = EventBCalendarPublicGuid.AsGuid() };
            eventB.EventCalendarItems.Add( eventBCalendarInternal );
            eventB.EventCalendarItems.Add( eventBCalendarPublic );

            rockContext.SaveChanges();

            // Set Attribute Values.
            rockContext = new RockContext();
            AttributeCache.Clear();

            // Set Attribute Values for Event B.
            eventBCalendarPublic.LoadAttributes( rockContext );
            eventBCalendarPublic.SetAttributeValue( PublicCalendarAttribute1Key, "Event B Public" );
            eventBCalendarPublic.SaveAttributeValues( rockContext );

            rockContext.SaveChanges();

            // Set Attribute Values for Event A
            eventACalendarInternal.LoadAttributes( rockContext );
            eventACalendarInternal.SetAttributeValue( InternalCalendarAttribute1Key, "Event A Internal" );
            eventACalendarInternal.SaveAttributeValues( rockContext );

            eventACalendarPublic.LoadAttributes( rockContext );
            eventACalendarPublic.SetAttributeValue( PublicCalendarAttribute1Key, "Event A Public" );
            eventACalendarPublic.SaveAttributeValues( rockContext );

            rockContext.SaveChanges();
        }

        #endregion
    }
}
