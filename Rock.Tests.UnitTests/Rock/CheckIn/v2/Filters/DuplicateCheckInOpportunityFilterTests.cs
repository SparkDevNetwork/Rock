using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
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
        #region IsScheduleValid Tests

        [TestMethod]
        public void IsScheduleValid_WithNoConditions_IncludesAnySchedule()
        {
            var scheduleId = "6ecc6067-6464-4c4c-a297-533b47e76254";

            var filter = CreateDuplicateCheckInFilter( false, Array.Empty<RecentAttendance>() );
            var scheduleOpportunity = CreateScheduleOpportunity( scheduleId );

            var isIncluded = filter.IsScheduleValid( scheduleOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void IsScheduleValid_WithDuplicateAttendance_ExcludesSchedule()
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
        public void IsScheduleValid_WithDuplicateCheckedOutAttendance_IncludesSchedule()
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
        public void IsScheduleValid_WithDuplicateAttendanceForDifferentDay_IncludesSchedule()
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
        public void IsScheduleValid_WithoutDuplicateAttendance_IncludesSchedule()
        {
            var scheduleId = "6ecc6067-6464-4c4c-a297-533b47e76254";
            var recentAttendance = new List<RecentAttendance>();

            var filter = CreateDuplicateCheckInFilter( true, recentAttendance );
            var scheduleOpportunity = CreateScheduleOpportunity( scheduleId );

            var isIncluded = filter.IsScheduleValid( scheduleOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void IsScheduleValid_WithDuplicateAttendanceAndTemplateNotPrevented_IncludesSchedule()
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

        #region FilterSchedules Tests

        [TestMethod]
        public void FilterSchedules_WithDuplicateAttendanceAndNoOtherSchedules_SetsUnavailableMessage()
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
            var opportunities = new OpportunityCollection
            {
                Schedules = new List<ScheduleOpportunity>
                {
                    CreateScheduleOpportunity( scheduleId )
                }
            };

            filter.FilterSchedules( opportunities );

            Assert.IsTrue( filter.Person.IsUnavailable );
            Assert.IsNotNull( filter.Person.UnavailableMessage );
        }

        [TestMethod]
        public void FilterSchedules_WithDuplicateAttendanceAndNoOtherSchedules_DoesNotReplaceExistingUnavailableMessage()
        {
            var expectedMessage = "This is the original message.";
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
            var opportunities = new OpportunityCollection
            {
                Schedules = new List<ScheduleOpportunity>
                {
                    CreateScheduleOpportunity( scheduleId )
                }
            };
            filter.Person.IsUnavailable = true;
            filter.Person.UnavailableMessage = expectedMessage;

            filter.FilterSchedules( opportunities );

            Assert.AreEqual( expectedMessage, filter.Person.UnavailableMessage );
        }

        [TestMethod]
        public void FilterSchedules_WithDuplicateAttendanceAndAnotherValidSchedule_DoesNotSetUnavailableMessage()
        {
            var scheduleId = "6ecc6067-6464-4c4c-a297-533b47e76254";
            var secondScheduleId = "3bf8bd6c-fadf-4de8-ab10-469c848334e7";
            var recentAttendance = new List<RecentAttendance>
            {
                new RecentAttendance
                {
                    StartDateTime = RockDateTime.Now,
                    ScheduleId = scheduleId
                }
            };

            var filter = CreateDuplicateCheckInFilter( true, recentAttendance );
            var opportunities = new OpportunityCollection
            {
                Schedules = new List<ScheduleOpportunity>
                {
                    CreateScheduleOpportunity( scheduleId ),
                    CreateScheduleOpportunity( secondScheduleId )
                }
            };

            filter.FilterSchedules( opportunities );

            Assert.IsFalse( filter.Person.IsUnavailable );
            Assert.IsNull( filter.Person.UnavailableMessage );
        }

        [TestMethod]
        public void FilterSchedules_WithNoSchedules_DoesNotSetUnavailableMessage()
        {
            var recentAttendance = new List<RecentAttendance>();

            var filter = CreateDuplicateCheckInFilter( true, recentAttendance );
            var opportunities = new OpportunityCollection
            {
                Schedules = new List<ScheduleOpportunity>()
            };

            filter.FilterSchedules( opportunities );

            Assert.IsFalse( filter.Person.IsUnavailable );
            Assert.IsNull( filter.Person.UnavailableMessage );
        }

        #endregion

        #region IsGroupValid Tests

        [TestMethod]
        public void IsGroupValid_WithNoConditions_IncludesAnyGroup()
        {
            var filter = CreateDuplicateCheckInFilter( false, Array.Empty<RecentAttendance>() );
            var groupOpportunity = CreateGroupOpportunity( false );

            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = "07e27d26-29f3-4eff-96fe-870e77c964db",
                ScheduleId = "fdd6fff0-2b0f-4212-a78a-9343c23b6553"
            } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithDuplicateAttendanceAndSingleSchedule_ExcludesGroup()
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
            var groupOpportunity = CreateGroupOpportunity( true );

            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = "07e27d26-29f3-4eff-96fe-870e77c964db",
                ScheduleId = scheduleId
            } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithDuplicateAttendanceAndTwoSchedules_IncludesGroup()
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
            var groupOpportunity = CreateGroupOpportunity( true );

            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = "07e27d26-29f3-4eff-96fe-870e77c964db",
                ScheduleId = scheduleId
            } );

            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = "07e27d26-29f3-4eff-96fe-870e77c964db",
                ScheduleId = "c9fc6ec4-3498-4fb3-ae1d-7f1d0a580d55"
            } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithDuplicateAttendanceAndOverflowLocation_ExcludesGroup()
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
            var groupOpportunity = CreateGroupOpportunity( true );

            groupOpportunity.OverflowLocations.Add( new LocationAndScheduleBag
            {
                LocationId = "07e27d26-29f3-4eff-96fe-870e77c964db",
                ScheduleId = scheduleId
            } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithDuplicateAttendanceAndDifferentOverflowLocationSchedule_IncludesGroup()
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
            var groupOpportunity = CreateGroupOpportunity( true );

            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = "07e27d26-29f3-4eff-96fe-870e77c964db",
                ScheduleId = scheduleId
            } );

            groupOpportunity.OverflowLocations.Add( new LocationAndScheduleBag
            {
                LocationId = "07e27d26-29f3-4eff-96fe-870e77c964db",
                ScheduleId = "c9fc6ec4-3498-4fb3-ae1d-7f1d0a580d55"
            } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        #endregion

        #region FilterGroups Tests

        [TestMethod]
        public void FilterGroups_WithNoInitialGroups_DoesNothing()
        {
            var filter = CreateDuplicateCheckInFilter( false, Array.Empty<RecentAttendance>() );
            var opportunities = new OpportunityCollection
            {
                Groups = new List<GroupOpportunity>()
            };

            filter.FilterGroups( opportunities );

            Assert.IsFalse( filter.Person.IsUnavailable );
            Assert.IsNull( filter.Person.UnavailableMessage );
        }

        [TestMethod]
        public void FilterGroups_WithNoGroupsRemaining_SetsUnavailableStatus()
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

            var groupOpportunity = CreateGroupOpportunity( true );

            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = "07e27d26-29f3-4eff-96fe-870e77c964db",
                ScheduleId = scheduleId
            } );

            var filter = CreateDuplicateCheckInFilter( false, recentAttendance );
            var opportunities = new OpportunityCollection
            {
                Groups = new List<GroupOpportunity>
                {
                    groupOpportunity
                }
            };

            filter.FilterGroups( opportunities );

            Assert.AreEqual( 0, opportunities.Groups.Count );
            Assert.IsTrue( filter.Person.IsUnavailable );
            Assert.IsNotNull( filter.Person.UnavailableMessage );
        }

        [TestMethod]
        public void FilterGroups_WithNoGroupsRemainingAndAlreadyUnavailable_DoesNotChangeUnavailableStatus()
        {
            var expectedUnavailableMessage = "test message";
            var scheduleId = "6ecc6067-6464-4c4c-a297-533b47e76254";
            var recentAttendance = new List<RecentAttendance>
            {
                new RecentAttendance
                {
                    StartDateTime = RockDateTime.Now,
                    ScheduleId = scheduleId
                }
            };

            var groupOpportunity = CreateGroupOpportunity( true );

            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = "07e27d26-29f3-4eff-96fe-870e77c964db",
                ScheduleId = scheduleId
            } );

            var filter = CreateDuplicateCheckInFilter( false, recentAttendance );
            var opportunities = new OpportunityCollection
            {
                Groups = new List<GroupOpportunity>
                {
                    groupOpportunity
                }
            };

            filter.Person.IsUnavailable = true;
            filter.Person.UnavailableMessage = expectedUnavailableMessage;

            filter.FilterGroups( opportunities );

            Assert.AreEqual( expectedUnavailableMessage, filter.Person.UnavailableMessage );
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

        /// <summary>
        /// Creates a group opportunity.
        /// </summary>
        /// <param name="groupId">The identifier of the group.</param>
        /// <returns>A new instance of <see cref="ScheduleOpportunity"/>.</returns>
        private GroupOpportunity CreateGroupOpportunity( bool isConcurrentCheckinPrevented )
        {
            var areaConfigurationMock = new Mock<AreaConfigurationData>( MockBehavior.Strict );

            areaConfigurationMock.Setup( m => m.IsConcurrentCheckInPrevented ).Returns( isConcurrentCheckinPrevented );

            return new GroupOpportunity
            {
                Locations = new List<LocationAndScheduleBag>(),
                OverflowLocations = new List<LocationAndScheduleBag>(),
                CheckInAreaData = areaConfigurationMock.Object
            };
        }

        #endregion
    }
}
