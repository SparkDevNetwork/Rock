using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
using Rock.Utility;
using Rock.ViewModels.CheckIn;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// This suite checks the various combinations of filter settings related to
    /// a person already being checked in for the check-in process.
    /// </summary>
    /// <seealso cref="DuplicateCheckInOpportunityFilter"/>
    [TestClass]
    public class DuplicateCheckInOpportunityFilterTests
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void DuplicateCheckInFilter_WithNoConditions_IncludesAnySchedule()
        {
            var scheduleId = "6ecc6067-6464-4c4c-a297-533b47e76254";

            var filter = CreateDuplicateCheckInFilter( false, Array.Empty<RecentAttendance>() );
            var scheduleOpportunity = CreateScheduleOpportunity( scheduleId );

            var isIncluded = filter.IsScheduleValid( scheduleOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void DuplicateCheckInFilter_WithDuplicateAttendance_ExcludesSchedule()
        {
            var scheduleId = "6ecc6067-6464-4c4c-a297-533b47e76254";
            var recentAttendance = new List<RecentAttendance>
            {
                new RecentAttendance
                {
                    StartDateTime = RockDateTime.Now,
                    ScheduleId = scheduleId
                }
            };

            var filter = CreateDuplicateCheckInFilter( true, recentAttendance );
            var scheduleOpportunity = CreateScheduleOpportunity( scheduleId );

            var isIncluded = filter.IsScheduleValid( scheduleOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void DuplicateCheckInFilter_WithDuplicateCheckedOutAttendance_IncludesSchedule()
        {
            var scheduleId = "6ecc6067-6464-4c4c-a297-533b47e76254";
            var recentAttendance = new List<RecentAttendance>
            {
                new RecentAttendance
                {
                    StartDateTime = RockDateTime.Now,
                    EndDateTime = RockDateTime.Now,
                    ScheduleId = scheduleId
                }
            };

            var filter = CreateDuplicateCheckInFilter( true, recentAttendance );
            var scheduleOpportunity = CreateScheduleOpportunity( scheduleId );

            var isIncluded = filter.IsScheduleValid( scheduleOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void DuplicateCheckInFilter_WithDuplicateAttendanceForDifferentDay_IncludesSchedule()
        {
            var scheduleId = "6ecc6067-6464-4c4c-a297-533b47e76254";
            var recentAttendance = new List<RecentAttendance>
            {
                new RecentAttendance
                {
                    StartDateTime = RockDateTime.Now.AddDays( -1 ),
                    ScheduleId = scheduleId
                }
            };

            var filter = CreateDuplicateCheckInFilter( true, recentAttendance );
            var scheduleOpportunity = CreateScheduleOpportunity( scheduleId );

            var isIncluded = filter.IsScheduleValid( scheduleOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void DuplicateCheckInFilter_WithoutDuplicateAttendance_IncludesSchedule()
        {
            var scheduleId = "6ecc6067-6464-4c4c-a297-533b47e76254";
            var recentAttendance = new List<RecentAttendance>();

            var filter = CreateDuplicateCheckInFilter( true, recentAttendance );
            var scheduleOpportunity = CreateScheduleOpportunity( scheduleId );

            var isIncluded = filter.IsScheduleValid( scheduleOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void DuplicateCheckInFilter_WithDuplicateAttendanceAndTemplateNotPrevented_IncludesSchedule()
        {
            var scheduleId = "6ecc6067-6464-4c4c-a297-533b47e76254";
            var recentAttendance = new List<RecentAttendance>
            {
                new RecentAttendance
                {
                    StartDateTime = RockDateTime.Now,
                    ScheduleId = scheduleId
                }
            };

            var filter = CreateDuplicateCheckInFilter( false, recentAttendance );
            var scheduleOpportunity = CreateScheduleOpportunity( scheduleId );

            var isIncluded = filter.IsScheduleValid( scheduleOpportunity );

            Assert.IsTrue( isIncluded );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="DuplicateCheckInOpportunityFilter"/> along with a
        /// person to be filtered.
        /// </summary>
        /// <param name="isDuplicateCheckInPrevented"><c>true</c> if duplicate check-in should be prevented; otherwise <c>false</c>.</param>
        /// <param name="recentAttendance">The recent attendance records to attach to the person.</param>
        /// <returns>An instance of <see cref="DuplicateCheckInOpportunityFilter"/>.</returns>
        private DuplicateCheckInOpportunityFilter CreateDuplicateCheckInFilter( bool isDuplicateCheckInPrevented, IReadOnlyCollection<RecentAttendance> recentAttendance )
        {
            // Create the template configuration.
            var templateConfigurationMock = new Mock<TemplateConfigurationData>( MockBehavior.Strict );

            templateConfigurationMock.Setup( m => m.IsDuplicateCheckInPrevented )
                .Returns( isDuplicateCheckInPrevented );

            var filter = new DuplicateCheckInOpportunityFilter
            {
                Person = new Attendee
                {
                    Person = new PersonBag()
                },
                TemplateConfiguration = templateConfigurationMock.Object
            };

            filter.Person.RecentAttendances = new List<RecentAttendance>( recentAttendance );

            return filter;
        }

        /// <summary>
        /// Creates a schedule opportunity.
        /// </summary>
        /// <param name="scheduleId">The identifier of the schedule.</param>
        /// <returns>A new instance of <see cref="ScheduleOpportunity"/>.</returns>
        private ScheduleOpportunity CreateScheduleOpportunity( string scheduleId )
        {
            return new ScheduleOpportunity
            {
                Id = scheduleId
            };
        }

        #endregion
    }
}
