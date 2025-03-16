using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
using Rock.Data;
using Rock.Enums.CheckIn;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.ViewModels.CheckIn;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// This suite checks the various combinations of filter settings related to
    /// a person's grade for the check-in process.
    /// </summary>
    /// <seealso cref="ScheduleRequirementOpportunityFilter"/>
    [TestClass]
    public class ScheduleRequirementOpportunityFilterTests : CheckInMockDatabase
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void IsGroupValid_WithSchedulingDisabledAndNoSchedule_IncludesGroup()
        {
            var groupIdHash = IdHasher.Instance.GetHash( 100 );
            var filter = CreateFilter( GetRockContextMock().Object );
            var groupOpportunity = CreateGroupOpportunity( groupIdHash, AttendanceRecordRequiredForCheckIn.ScheduleNotRequired, false );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithScheduleNotRequiredAndNoSchedule_IncludesGroup()
        {
            var groupIdHash = IdHasher.Instance.GetHash( 100 );
            var filter = CreateFilter( GetRockContextMock().Object );
            var groupOpportunity = CreateGroupOpportunity( groupIdHash, AttendanceRecordRequiredForCheckIn.ScheduleNotRequired, true );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithScheduleRequiredAndNoSchedule_ExcludesGroup()
        {
            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet<Attendance>();

            var groupIdHash = IdHasher.Instance.GetHash( 100 );
            var filter = CreateFilter( rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupIdHash, AttendanceRecordRequiredForCheckIn.ScheduleRequired, true );

            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = IdHasher.Instance.GetHash( 200 ),
                ScheduleId = IdHasher.Instance.GetHash( 300 )
            } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithScheduleRequiredAndMatchingSchedule_IncludesGroup()
        {
            var rockContextMock = GetRockContextMock();
            var groupId = 100;
            var attendance = new Attendance
            {
                ScheduledToAttend = true,
                PersonAlias = new PersonAlias
                {
                    PersonId = 1
                },
                Occurrence = new AttendanceOccurrence
                {
                    OccurrenceDate = RockDateTime.Today,
                    GroupId = groupId,
                    LocationId = 200,
                    ScheduleId = 300
                }
            };

            rockContextMock.SetupDbSet( attendance );

            var groupIdHash = IdHasher.Instance.GetHash( groupId );
            var filter = CreateFilter( rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupIdHash, AttendanceRecordRequiredForCheckIn.ScheduleRequired, true );

            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = IdHasher.Instance.GetHash( 200 ),
                ScheduleId = IdHasher.Instance.GetHash( 300 )
            } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithScheduleRequiredAndYesterdaySchedule_ExcludesGroup()
        {
            var rockContextMock = GetRockContextMock();
            var groupId = 100;
            var attendance = new Attendance
            {
                ScheduledToAttend = true,
                PersonAlias = new PersonAlias
                {
                    PersonId = 1
                },
                Occurrence = new AttendanceOccurrence
                {
                    // Make sure it only matches attendance records for today.
                    OccurrenceDate = RockDateTime.Today.AddDays( -1 ),
                    GroupId = groupId,
                    LocationId = 200,
                    ScheduleId = 300
                }
            };

            rockContextMock.SetupDbSet( attendance );

            var groupIdHash = IdHasher.Instance.GetHash( groupId );
            var filter = CreateFilter( rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupIdHash, AttendanceRecordRequiredForCheckIn.ScheduleRequired, true );

            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = IdHasher.Instance.GetHash( 200 ),
                ScheduleId = IdHasher.Instance.GetHash( 300 )
            } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithScheduleRequiredAndMatchingSchedule_RemovesNonMatchingLocations()
        {
            var rockContextMock = GetRockContextMock();
            var groupId = 100;
            var attendance = new Attendance
            {
                ScheduledToAttend = true,
                PersonAlias = new PersonAlias
                {
                    PersonId = 1
                },
                Occurrence = new AttendanceOccurrence
                {
                    OccurrenceDate = RockDateTime.Today,
                    GroupId = groupId,
                    LocationId = 200,
                    ScheduleId = 300
                }
            };

            rockContextMock.SetupDbSet( attendance );

            var groupIdHash = IdHasher.Instance.GetHash( groupId );
            var filter = CreateFilter( rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupIdHash, AttendanceRecordRequiredForCheckIn.ScheduleRequired, true );

            // The matching location.
            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = IdHasher.Instance.GetHash( 200 ),
                ScheduleId = IdHasher.Instance.GetHash( 300 )
            } );

            // The non-matching location that should be removed.
            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = IdHasher.Instance.GetHash( 201 ),
                ScheduleId = IdHasher.Instance.GetHash( 300 )
            } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
            Assert.AreEqual( 1, groupOpportunity.Locations.Count );
            Assert.AreEqual( IdHasher.Instance.GetHash( 200 ), groupOpportunity.Locations[0].LocationId );
        }

        [TestMethod]
        public void IsGroupValid_WithScheduleRequiredAndMatchingSchedule_RemovesNonMatchingOverflowLocations()
        {
            var rockContextMock = GetRockContextMock();
            var groupId = 100;
            var attendance = new Attendance
            {
                ScheduledToAttend = true,
                PersonAlias = new PersonAlias
                {
                    PersonId = 1
                },
                Occurrence = new AttendanceOccurrence
                {
                    OccurrenceDate = RockDateTime.Today,
                    GroupId = groupId,
                    LocationId = 200,
                    ScheduleId = 300
                }
            };

            rockContextMock.SetupDbSet( attendance );

            var groupIdHash = IdHasher.Instance.GetHash( groupId );
            var filter = CreateFilter( rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupIdHash, AttendanceRecordRequiredForCheckIn.ScheduleRequired, true );

            // The matching location.
            groupOpportunity.OverflowLocations.Add( new LocationAndScheduleBag
            {
                LocationId = IdHasher.Instance.GetHash( 200 ),
                ScheduleId = IdHasher.Instance.GetHash( 300 )
            } );

            // The non-matching location that should be removed.
            groupOpportunity.OverflowLocations.Add( new LocationAndScheduleBag
            {
                LocationId = IdHasher.Instance.GetHash( 201 ),
                ScheduleId = IdHasher.Instance.GetHash( 300 )
            } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
            Assert.AreEqual( 1, groupOpportunity.OverflowLocations.Count );
            Assert.AreEqual( IdHasher.Instance.GetHash( 200 ), groupOpportunity.OverflowLocations[0].LocationId );
        }

        [TestMethod]
        public void IsGroupValid_WithPreSelectAndMatchingSchedule_SelectsLocation()
        {
            var rockContextMock = GetRockContextMock();
            var groupId = 100;
            var attendance = new Attendance
            {
                ScheduledToAttend = true,
                PersonAlias = new PersonAlias
                {
                    PersonId = 1
                },
                Occurrence = new AttendanceOccurrence
                {
                    OccurrenceDate = RockDateTime.Today,
                    GroupId = groupId,
                    LocationId = 200,
                    ScheduleId = 300
                }
            };

            rockContextMock.SetupDbSet( attendance );

            var groupIdHash = IdHasher.Instance.GetHash( groupId );
            var filter = CreateFilter( rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupIdHash, AttendanceRecordRequiredForCheckIn.PreSelect, true );

            groupOpportunity.AreaId = IdHasher.Instance.GetHash( 400 );
            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = IdHasher.Instance.GetHash( 200 ),
                ScheduleId = IdHasher.Instance.GetHash( 300 )
            } );

            // Add the related opportunity options.
            filter.Person.Opportunities.Areas.Add( new AreaOpportunity
            {
                Id = IdHasher.Instance.GetHash( 400 ),
                Name = "Area 400"
            } );
            filter.Person.Opportunities.Locations.Add( new LocationOpportunity
            {
                Id = IdHasher.Instance.GetHash( 200 ),
                Name = "Location 200"
            } );
            filter.Person.Opportunities.Schedules.Add( new ScheduleOpportunity
            {
                Id = IdHasher.Instance.GetHash( 300 ),
                Name = "Schedule 300"
            } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
            Assert.AreEqual( 1, filter.Person.PreSelectedOpportunities.Count );
            Assert.AreEqual( IdHasher.Instance.GetHash( 200 ), filter.Person.PreSelectedOpportunities[0].Location.Id );
        }

        [TestMethod]
        public void IsGroupValid_WithPreSelectAndNoSchedule_DoesNotSelectLocation()
        {
            var rockContextMock = GetRockContextMock();
            var groupId = 100;

            rockContextMock.SetupDbSet<Attendance>();

            var groupIdHash = IdHasher.Instance.GetHash( groupId );
            var filter = CreateFilter( rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupIdHash, AttendanceRecordRequiredForCheckIn.PreSelect, true );

            groupOpportunity.AreaId = IdHasher.Instance.GetHash( 400 );
            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = IdHasher.Instance.GetHash( 200 ),
                ScheduleId = IdHasher.Instance.GetHash( 300 )
            } );

            // Add the related opportunity options.
            filter.Person.Opportunities.Areas.Add( new AreaOpportunity
            {
                Id = IdHasher.Instance.GetHash( 400 ),
                Name = "Area 400"
            } );
            filter.Person.Opportunities.Locations.Add( new LocationOpportunity
            {
                Id = IdHasher.Instance.GetHash( 200 ),
                Name = "Location 200"
            } );
            filter.Person.Opportunities.Schedules.Add( new ScheduleOpportunity
            {
                Id = IdHasher.Instance.GetHash( 300 ),
                Name = "Schedule 300"
            } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
            Assert.AreEqual( 0, filter.Person.PreSelectedOpportunities.Count );
        }

        [TestMethod]
        public void IsGroupValid_WithPreSelectAndScheduleForDifferentGroup_DoesNotSelectLocation()
        {
            var rockContextMock = GetRockContextMock();
            var groupId = 100;
            var attendance = new Attendance
            {
                ScheduledToAttend = true,
                PersonAlias = new PersonAlias
                {
                    PersonId = 1
                },
                Occurrence = new AttendanceOccurrence
                {
                    OccurrenceDate = RockDateTime.Today,
                    // Scheduled Attendance for a different group.
                    GroupId = groupId + 1,
                    LocationId = 200,
                    ScheduleId = 300
                }
            };

            rockContextMock.SetupDbSet( attendance );

            var groupIdHash = IdHasher.Instance.GetHash( groupId );
            var filter = CreateFilter( rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupIdHash, AttendanceRecordRequiredForCheckIn.PreSelect, true );

            groupOpportunity.AreaId = IdHasher.Instance.GetHash( 400 );
            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = IdHasher.Instance.GetHash( 200 ),
                ScheduleId = IdHasher.Instance.GetHash( 300 )
            } );

            // Add the related opportunity options.
            filter.Person.Opportunities.Areas.Add( new AreaOpportunity
            {
                Id = IdHasher.Instance.GetHash( 400 ),
                Name = "Area 400"
            } );
            filter.Person.Opportunities.Locations.Add( new LocationOpportunity
            {
                Id = IdHasher.Instance.GetHash( 200 ),
                Name = "Location 200"
            } );
            filter.Person.Opportunities.Schedules.Add( new ScheduleOpportunity
            {
                Id = IdHasher.Instance.GetHash( 300 ),
                Name = "Schedule 300"
            } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
            Assert.AreEqual( 0, filter.Person.PreSelectedOpportunities.Count );
        }

        [TestMethod]
        public void IsGroupValid_WithPreSelectAndMatchingSchedule_DoesNotOverwriteSelection()
        {
            var rockContextMock = GetRockContextMock();
            var groupId = 100;
            var attendance = new Attendance
            {
                ScheduledToAttend = true,
                PersonAlias = new PersonAlias
                {
                    PersonId = 1
                },
                Occurrence = new AttendanceOccurrence
                {
                    OccurrenceDate = RockDateTime.Today,
                    GroupId = groupId,
                    LocationId = 200,
                    ScheduleId = 300
                }
            };

            rockContextMock.SetupDbSet( attendance );

            var groupIdHash = IdHasher.Instance.GetHash( groupId );
            var filter = CreateFilter( rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupIdHash, AttendanceRecordRequiredForCheckIn.PreSelect, true );

            groupOpportunity.AreaId = IdHasher.Instance.GetHash( 400 );
            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = IdHasher.Instance.GetHash( 200 ),
                ScheduleId = IdHasher.Instance.GetHash( 300 )
            } );

            // Add the related opportunity options.
            filter.Person.Opportunities.Areas.Add( new AreaOpportunity
            {
                Id = IdHasher.Instance.GetHash( 400 ),
                Name = "Area 400"
            } );
            filter.Person.Opportunities.Locations.Add( new LocationOpportunity
            {
                Id = IdHasher.Instance.GetHash( 200 ),
                Name = "Location 200"
            } );
            filter.Person.Opportunities.Schedules.Add( new ScheduleOpportunity
            {
                Id = IdHasher.Instance.GetHash( 300 ),
                Name = "Schedule 300"
            } );

            filter.Person.PreSelectedOpportunities.Add( new OpportunitySelectionBag
            {
                Group = new CheckInItemBag
                {
                    Id = groupIdHash,
                    Name = "OriginalSelection"
                },
                Location = new CheckInItemBag
                {
                    Id = IdHasher.Instance.GetHash( 200 )
                },
                Schedule = new CheckInItemBag
                {
                    Id = IdHasher.Instance.GetHash( 300 )
                }
            } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
            Assert.AreEqual( 1, filter.Person.PreSelectedOpportunities.Count );
            Assert.AreEqual( "OriginalSelection", filter.Person.PreSelectedOpportunities[0].Group.Name );
        }

        [TestMethod]
        public void IsGroupValid_WithPreSelectAndMatchingSchedule_DoesNotOverwriteDifferentGroupSelection()
        {
            var rockContextMock = GetRockContextMock();
            var groupId = 100;
            var attendance = new Attendance
            {
                ScheduledToAttend = true,
                PersonAlias = new PersonAlias
                {
                    PersonId = 1
                },
                Occurrence = new AttendanceOccurrence
                {
                    OccurrenceDate = RockDateTime.Today,
                    GroupId = groupId,
                    LocationId = 200,
                    ScheduleId = 300
                }
            };

            rockContextMock.SetupDbSet( attendance );

            var groupIdHash = IdHasher.Instance.GetHash( groupId );
            var filter = CreateFilter( rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupIdHash, AttendanceRecordRequiredForCheckIn.PreSelect, true );

            groupOpportunity.AreaId = IdHasher.Instance.GetHash( 400 );
            groupOpportunity.Locations.Add( new LocationAndScheduleBag
            {
                LocationId = IdHasher.Instance.GetHash( 200 ),
                ScheduleId = IdHasher.Instance.GetHash( 300 )
            } );

            // Add the related opportunity options.
            filter.Person.Opportunities.Areas.Add( new AreaOpportunity
            {
                Id = IdHasher.Instance.GetHash( 400 ),
                Name = "Area 400"
            } );
            filter.Person.Opportunities.Locations.Add( new LocationOpportunity
            {
                Id = IdHasher.Instance.GetHash( 200 ),
                Name = "Location 200"
            } );
            filter.Person.Opportunities.Schedules.Add( new ScheduleOpportunity
            {
                Id = IdHasher.Instance.GetHash( 300 ),
                Name = "Schedule 300"
            } );

            filter.Person.PreSelectedOpportunities.Add( new OpportunitySelectionBag
            {
                Group = new CheckInItemBag
                {
                    Id = IdHasher.Instance.GetHash( groupId + 1 ),
                    Name = "OriginalSelection"
                },
                Location = new CheckInItemBag
                {
                    Id = IdHasher.Instance.GetHash( 200 )
                },
                Schedule = new CheckInItemBag
                {
                    Id = IdHasher.Instance.GetHash( 300 )
                }
            } );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
            Assert.AreEqual( 2, filter.Person.PreSelectedOpportunities.Count );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="ScheduleRequirementOpportunityFilter"/> along with a
        /// person to be filtered.
        /// </summary>
        /// <returns>An instance of <see cref="ScheduleRequirementOpportunityFilter"/>.</returns>
        private ScheduleRequirementOpportunityFilter CreateFilter( RockContext rockContext )
        {
            var templateConfigurationMock = new Mock<TemplateConfigurationData>( MockBehavior.Strict );
            var director = new CheckInDirector( rockContext );

            // Create the filter.
            var filter = new ScheduleRequirementOpportunityFilter
            {
                Person = new Attendee
                {
                    Person = new PersonBag
                    {
                        Id = IdHasher.Instance.GetHash( 1 )
                    },
                    Opportunities = new OpportunityCollection
                    {
                        Areas = new List<AreaOpportunity>(),
                        Groups = new List<GroupOpportunity>(),
                        Locations = new List<LocationOpportunity>(),
                        Schedules = new List<ScheduleOpportunity>()
                    },
                    PreSelectedOpportunities = new List<OpportunitySelectionBag>()
                },
                Session = new CheckInSession( director, templateConfigurationMock.Object )
            };

            return filter;
        }

        /// <summary>
        /// Creates a group opportunity with the specified options.
        /// </summary>
        /// <param name="id">The identifier value of the group.</param>
        /// <param name="scheduleRequirement">The value for the group schedule requirement option.</param>
        /// <param name="isSchedulingEnabled">The value for the area scheduling enabled option.</param>
        /// <returns>A new instance of <see cref="GroupOpportunity"/>.</returns>
        private GroupOpportunity CreateGroupOpportunity( string id, AttendanceRecordRequiredForCheckIn scheduleRequirement, bool isSchedulingEnabled )
        {
            var areaConfigurationMock = new Mock<AreaConfigurationData>( MockBehavior.Strict );
            var groupConfigurationMock = new Mock<GroupConfigurationData>( MockBehavior.Strict );

            areaConfigurationMock.Setup( m => m.IsSchedulingEnabled ).Returns( isSchedulingEnabled );
            groupConfigurationMock.Setup( m => m.AttendanceRecordRequiredForCheckIn ).Returns( scheduleRequirement );

            return new GroupOpportunity
            {
                Id = id,
                CheckInData = groupConfigurationMock.Object,
                CheckInAreaData = areaConfigurationMock.Object,
                Locations = new List<LocationAndScheduleBag>(),
                OverflowLocations = new List<LocationAndScheduleBag>()
            };
        }

        #endregion
    }
}
