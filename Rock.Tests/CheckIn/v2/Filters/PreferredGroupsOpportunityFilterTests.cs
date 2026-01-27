using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
using Rock.Data;
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
    public class PreferredGroupsFilterTests : CheckInMockDatabase
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void PreferredGroupsFilter_WithNoPreferredGroups_IncludesAllGroups()
        {
            var firstGroupId = 10;
            var secondGroupId = 20;
            int locationId = 30;

            var rockContextMock = GetRockContextMock();
            var filter = CreateFilter( rockContextMock.Object );
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

            var rockContextMock = GetRockContextMock();
            var filter = CreateFilter( rockContextMock.Object );
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

            var rockContextMock = GetRockContextMock();
            var filter = CreateFilter( rockContextMock.Object );
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

            var rockContextMock = GetRockContextMock();
            var filter = CreateFilter( rockContextMock.Object );
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

        #endregion
    }
}
