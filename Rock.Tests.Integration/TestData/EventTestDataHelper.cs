﻿using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.TestData
{
    public static partial class TestDataHelper
    {
        #region Events

        public static class Events
        {
            private const string TestDataForeignKey = "test_data";
            private const string EventFinancesClassGuid = "6EFC00B0-F5D3-4352-BC3B-F09852FB5788";
            private const string ScheduleSat1630Guid = "7883CAC8-6E30-482B-95A7-2F0DEE859BE1";
            private const string ScheduleSun1200Guid = "1F6C15DA-982F-43B1-BDE9-D4E70CFBCB45";
            private const string FinancesClassOccurrenceSat1630Guid = "E7116C5A-9FEE-42D4-A0DB-7FEBFCCB6B8B";
            private const string FinancesClassOccurrenceSun1200Guid = "3F3EA420-E3F0-435A-9401-C2D058EF37DE";
            private static string MainCampusGuidString = "76882AE3-1CE8-42A6-A2B6-8C0B29CF8CF8";

            /// <summary>
            /// Modifies the Rock Solid Finances Class to add multiple schedules and campuses.
            /// </summary>
            public static void AddDataForRockSolidFinancesClass()
            {
                var rockContext = new RockContext();

                // Get Campus 2.
                var campus2 = TestDataHelper.GetOrAddCampusSteppingStone( rockContext );

                // Get existing schedules.
                var scheduleService = new ScheduleService( rockContext );

                var scheduleSat1630Id = scheduleService.GetId( ScheduleSat1630Guid.AsGuid() );
                var scheduleSat1800Id = scheduleService.GetId( ScheduleSun1200Guid.AsGuid() );

                // Get Event "Rock Solid Finances".
                // This event is associated with both the Internal and Public calendars.
                var eventItemService = new EventItemService( rockContext );
                var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );

                var financeEvent = eventItemService.Get( EventFinancesClassGuid.AsGuid() );

                // Add an occurrence of this event for each Schedule.
                var financeEvent1 = eventItemOccurrenceService.Get( FinancesClassOccurrenceSat1630Guid.AsGuid() );

                if ( financeEvent1 == null )
                {
                    financeEvent1 = new EventItemOccurrence();
                    financeEvent.EventItemOccurrences.Add( financeEvent1 );
                }

                var mainCampusId = CampusCache.GetId( MainCampusGuidString.AsGuid() );
                var secondCampusId = campus2.Id;

                financeEvent1.Location = "Meeting Room 1";
                financeEvent1.ForeignKey = TestDataForeignKey;
                financeEvent1.ScheduleId = scheduleSat1630Id;
                financeEvent1.Guid = FinancesClassOccurrenceSat1630Guid.AsGuid();
                financeEvent1.CampusId = mainCampusId;

                var financeEvent2 = eventItemOccurrenceService.Get( FinancesClassOccurrenceSun1200Guid.AsGuid() );

                if ( financeEvent2 == null )
                {
                    financeEvent2 = new EventItemOccurrence();
                    financeEvent.EventItemOccurrences.Add( financeEvent2 );
                }

                financeEvent2.Location = "Meeting Room 2";
                financeEvent2.ForeignKey = TestDataForeignKey;
                financeEvent2.ScheduleId = scheduleSat1800Id;
                financeEvent2.Guid = FinancesClassOccurrenceSun1200Guid.AsGuid();
                financeEvent2.CampusId = secondCampusId;

                rockContext.SaveChanges();
            }
        }

        #endregion

    }
}
