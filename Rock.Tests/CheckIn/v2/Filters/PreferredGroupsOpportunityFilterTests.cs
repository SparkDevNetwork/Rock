using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
using Rock.Configuration;
using Rock.Data;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.ViewModels.CheckIn;

namespace Rock.Tests.CheckIn.v2.Filters
{
    /// <summary>
    /// This suite checks the various combinations of filter settings related to
    /// a the preferred group status.
    /// </summary>
    /// <seealso cref="PreferredGroupsOpportunityFilter"/>
    [TestClass]
    public class PreferredGroupsFilterTests
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void PreferredGroupsFilter_WithNoPreferredGroups_IncludesAllGroups()
        {
            var firstGroupId = 10;
            var secondGroupId = 20;
            int locationId = 30;

            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();
            var filter = CreateFilter( rockContext );
            var firstGroupOpportunity = CreateGroupOpportunity( firstGroupId, false, locationId );
            var secondGroupOpportunity = CreateGroupOpportunity( secondGroupId, false, locationId );

            var opportunities = new OpportunityCollection
            {
                Groups = new List<GroupOpportunity>
                {
                    firstGroupOpportunity,
                    secondGroupOpportunity
                },
                Locations = new List<LocationOpportunity>
                {
                    new LocationOpportunity
                    {
                        Id = IdHasher.Instance.GetHash( locationId ),
                        IsClosed = false,
                    }
                },
            };

            filter.FilterGroups( opportunities );

            Assert.HasCount( 2, opportunities.Groups );
        }

        [TestMethod]
        public void PreferredGroupsFilter_WithOnePreferredGroup_IncludesOnlyPreferredGroups()
        {
            var firstGroupId = 10;
            var secondGroupId = 20;
            int locationId = 30;

            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();
            var filter = CreateFilter( rockContext );
            var firstGroupOpportunity = CreateGroupOpportunity( firstGroupId, true, locationId );
            var secondGroupOpportunity = CreateGroupOpportunity( secondGroupId, false, locationId );

            var opportunities = new OpportunityCollection
            {
                Groups = new List<GroupOpportunity>
                {
                    firstGroupOpportunity,
                    secondGroupOpportunity
                },
                Locations = new List<LocationOpportunity>
                {
                    new LocationOpportunity
                    {
                        Id = IdHasher.Instance.GetHash( locationId ),
                        IsClosed = false,
                    }
                },
            };

            filter.FilterGroups( opportunities );

            Assert.HasCount( 1, opportunities.Groups );
            Assert.AreEqual( firstGroupId, IdHasher.Instance.GetId( firstGroupOpportunity.Id ) );
        }

        [TestMethod]
        public void PreferredGroupsFilter_WithOnePreferredGroupLocationClosed_IncludesAllGroups()
        {
            var firstGroupId = 10;
            var secondGroupId = 20;
            int locationId = 30;

            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();
            var filter = CreateFilter( rockContext );
            var firstGroupOpportunity = CreateGroupOpportunity( firstGroupId, true, locationId );
            var secondGroupOpportunity = CreateGroupOpportunity( secondGroupId, false, locationId );

            var opportunities = new OpportunityCollection
            {
                Groups = new List<GroupOpportunity>
                {
                    firstGroupOpportunity,
                    secondGroupOpportunity
                },
                Locations = new List<LocationOpportunity>
                {
                    new LocationOpportunity
                    {
                        Id = IdHasher.Instance.GetHash( locationId ),
                        IsClosed = true,
                    }
                },
            };

            filter.FilterGroups( opportunities );

            Assert.HasCount( 2, opportunities.Groups );
        }

        [TestMethod]
        public void PreferredGroupsFilter_WithTwoPreferredGroups_IncludesAllPreferredGroups()
        {
            var firstGroupId = 10;
            var secondGroupId = 20;
            int locationId = 30;

            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();
            var filter = CreateFilter( rockContext );
            var firstGroupOpportunity = CreateGroupOpportunity( firstGroupId, true, locationId );
            var secondGroupOpportunity = CreateGroupOpportunity( secondGroupId, true, locationId );

            var opportunities = new OpportunityCollection
            {
                Groups = new List<GroupOpportunity>
                {
                    firstGroupOpportunity,
                    secondGroupOpportunity
                },
                Locations = new List<LocationOpportunity>
                {
                    new LocationOpportunity
                    {
                        Id = IdHasher.Instance.GetHash( locationId ),
                        IsClosed = false,
                    }
                },
            };

            filter.FilterGroups( opportunities );

            Assert.HasCount( 2, opportunities.Groups );
        }

        [TestMethod]
        public void PreferredGroupsFilter_WithPreferredGroupOnOneSchedule_KeepsNonPreferredGroupOnOtherSchedule()
        {
            // Regression test for Issue #7067. The preferred group is only
            // available on the first schedule, so the non-preferred group must
            // only lose the first schedule and remain available on the second.
            var preferredGroupId = 10;
            var otherGroupId = 20;
            var firstLocationId = 30;
            var secondLocationId = 40;
            var firstScheduleId = 50;
            var secondScheduleId = 60;

            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();
            var filter = CreateFilter( rockContext );
            var preferredGroupOpportunity = CreateGroupOpportunity( preferredGroupId, true, new[] { (firstLocationId, firstScheduleId) } );
            var otherGroupOpportunity = CreateGroupOpportunity( otherGroupId, false, new[] { (firstLocationId, firstScheduleId), (secondLocationId, secondScheduleId) } );

            var opportunities = CreateOpportunities( new[] { preferredGroupOpportunity, otherGroupOpportunity }, new[] { firstLocationId, secondLocationId }, new int[0] );

            filter.FilterGroups( opportunities );

            Assert.HasCount( 2, opportunities.Groups );
            Assert.AreSame( preferredGroupOpportunity, opportunities.Groups[0] );
            Assert.AreSame( otherGroupOpportunity, opportunities.Groups[1] );
            Assert.HasCount( 1, otherGroupOpportunity.Locations );
            Assert.AreEqual( IdHasher.Instance.GetHash( secondLocationId ), otherGroupOpportunity.Locations[0].LocationId );
            Assert.AreEqual( IdHasher.Instance.GetHash( secondScheduleId ), otherGroupOpportunity.Locations[0].ScheduleId );
        }

        [TestMethod]
        public void PreferredGroupsFilter_WithNonPreferredGroupOnlyOnPreferredSchedule_RemovesNonPreferredGroup()
        {
            var preferredGroupId = 10;
            var otherGroupId = 20;
            var locationId = 30;
            var scheduleId = 50;

            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();
            var filter = CreateFilter( rockContext );
            var preferredGroupOpportunity = CreateGroupOpportunity( preferredGroupId, true, new[] { (locationId, scheduleId) } );
            var otherGroupOpportunity = CreateGroupOpportunity( otherGroupId, false, new[] { (locationId, scheduleId) } );

            var opportunities = CreateOpportunities( new[] { preferredGroupOpportunity, otherGroupOpportunity }, new[] { locationId }, new int[0] );

            filter.FilterGroups( opportunities );

            Assert.HasCount( 1, opportunities.Groups );
            Assert.AreSame( preferredGroupOpportunity, opportunities.Groups[0] );
        }

        [TestMethod]
        public void PreferredGroupsFilter_WithPreferredLocationClosedOnOneSchedule_OnlyRemovesNonPreferredOnOpenSchedule()
        {
            // The preferred group is in a closed location on the first schedule
            // so it must not suppress other groups on that schedule, but it is
            // in an open location on the second schedule so it does there.
            var preferredGroupId = 10;
            var otherGroupId = 20;
            var closedLocationId = 30;
            var openLocationId = 40;
            var otherLocationId = 50;
            var firstScheduleId = 60;
            var secondScheduleId = 70;

            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();
            var filter = CreateFilter( rockContext );
            var preferredGroupOpportunity = CreateGroupOpportunity( preferredGroupId, true, new[] { (closedLocationId, firstScheduleId), (openLocationId, secondScheduleId) } );
            var otherGroupOpportunity = CreateGroupOpportunity( otherGroupId, false, new[] { (otherLocationId, firstScheduleId), (otherLocationId, secondScheduleId) } );

            var opportunities = CreateOpportunities( new[] { preferredGroupOpportunity, otherGroupOpportunity }, new[] { openLocationId, otherLocationId }, new[] { closedLocationId } );

            filter.FilterGroups( opportunities );

            Assert.HasCount( 2, opportunities.Groups );
            Assert.HasCount( 1, otherGroupOpportunity.Locations );
            Assert.AreEqual( IdHasher.Instance.GetHash( firstScheduleId ), otherGroupOpportunity.Locations[0].ScheduleId );
        }

        [TestMethod]
        public void PreferredGroupsFilter_WithPreferredGroupOverflowLocationOnOneSchedule_KeepsNonPreferredGroupOnOtherSchedule()
        {
            var preferredGroupId = 10;
            var otherGroupId = 20;
            var locationId = 30;
            var overflowLocationId = 40;
            var firstScheduleId = 50;
            var secondScheduleId = 60;

            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();
            var filter = CreateFilter( rockContext );
            var preferredGroupOpportunity = CreateGroupOpportunity( preferredGroupId, true, new (int, int)[0], new[] { (overflowLocationId, firstScheduleId) } );
            var otherGroupOpportunity = CreateGroupOpportunity( otherGroupId, false, new[] { (locationId, firstScheduleId), (locationId, secondScheduleId) } );

            var opportunities = CreateOpportunities( new[] { preferredGroupOpportunity, otherGroupOpportunity }, new[] { locationId, overflowLocationId }, new int[0] );

            filter.FilterGroups( opportunities );

            Assert.HasCount( 2, opportunities.Groups );
            Assert.HasCount( 1, otherGroupOpportunity.Locations );
            Assert.AreEqual( IdHasher.Instance.GetHash( secondScheduleId ), otherGroupOpportunity.Locations[0].ScheduleId );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="PreferredGroupsOpportunityFilter"/> along with a
        /// person to be filtered.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing database objects.</param>
        /// <returns>An instance of <see cref="MembershipOpportunityFilter"/>.</returns>
        private PreferredGroupsOpportunityFilter CreateFilter( RockContext rockContext )
        {
            // Create the template configuration.
            var templateConfigurationMock = new Mock<TemplateConfigurationData>( MockBehavior.Strict );

            var director = new CheckInDirector( rockContext );

            // Create the filter.
            var filter = new PreferredGroupsOpportunityFilter
            {
                Person = new Attendee
                {
                    Person = new PersonBag()
                },
                Session = new CheckInSession( director, templateConfigurationMock.Object ),
                TemplateConfiguration = templateConfigurationMock.Object
            };

            filter.Person.Person.Id = IdHasher.Instance.GetHash( 0 );

            return filter;
        }

        /// <summary>
        /// Creates a group opportunity with the specified data view unique
        /// identifier values.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="isPreferred">The preferred state of the group.</param>
        /// <returns>A new instance of <see cref="GroupOpportunity"/>.</returns>
        private GroupOpportunity CreateGroupOpportunity( int groupId, bool isPreferred, int locationId )
        {
            return new GroupOpportunity
            {
                Id = IdHasher.Instance.GetHash( groupId ),
                IsPreferredGroup = isPreferred,
                Locations = new List<LocationAndScheduleBag>
                {
                    new LocationAndScheduleBag
                    {
                        LocationId = IdHasher.Instance.GetHash( locationId ),
                    }
                },
                OverflowLocations = new List<LocationAndScheduleBag>()
            };
        }

        /// <summary>
        /// Creates a group opportunity with the specified location and schedule
        /// pairs.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="isPreferred">The preferred state of the group.</param>
        /// <param name="locations">The location and schedule identifier pairs for the group.</param>
        /// <param name="overflowLocations">The overflow location and schedule identifier pairs for the group.</param>
        /// <returns>A new instance of <see cref="GroupOpportunity"/>.</returns>
        private GroupOpportunity CreateGroupOpportunity( int groupId, bool isPreferred, IEnumerable<(int LocationId, int ScheduleId)> locations, IEnumerable<(int LocationId, int ScheduleId)> overflowLocations = null )
        {
            return new GroupOpportunity
            {
                Id = IdHasher.Instance.GetHash( groupId ),
                IsPreferredGroup = isPreferred,
                Locations = locations
                    .Select( l => new LocationAndScheduleBag
                    {
                        LocationId = IdHasher.Instance.GetHash( l.LocationId ),
                        ScheduleId = IdHasher.Instance.GetHash( l.ScheduleId )
                    } )
                    .ToList(),
                OverflowLocations = ( overflowLocations ?? Enumerable.Empty<(int LocationId, int ScheduleId)>() )
                    .Select( l => new LocationAndScheduleBag
                    {
                        LocationId = IdHasher.Instance.GetHash( l.LocationId ),
                        ScheduleId = IdHasher.Instance.GetHash( l.ScheduleId )
                    } )
                    .ToList()
            };
        }

        /// <summary>
        /// Creates an opportunity collection with the specified groups and
        /// locations.
        /// </summary>
        /// <param name="groups">The groups to include.</param>
        /// <param name="openLocationIds">The identifiers of the locations that are open.</param>
        /// <param name="closedLocationIds">The identifiers of the locations that are closed.</param>
        /// <returns>A new instance of <see cref="OpportunityCollection"/>.</returns>
        private OpportunityCollection CreateOpportunities( IEnumerable<GroupOpportunity> groups, IEnumerable<int> openLocationIds, IEnumerable<int> closedLocationIds )
        {
            var locations = openLocationIds
                .Select( id => new LocationOpportunity { Id = IdHasher.Instance.GetHash( id ), IsClosed = false } )
                .Concat( closedLocationIds.Select( id => new LocationOpportunity { Id = IdHasher.Instance.GetHash( id ), IsClosed = true } ) )
                .ToList();

            return new OpportunityCollection
            {
                Groups = groups.ToList(),
                Locations = locations
            };
        }

        #endregion
    }
}
