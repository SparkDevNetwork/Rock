using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
using Rock.Tests.Shared;
using Rock.Utility;
using Rock.ViewModels.CheckIn;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// This suite checks the various combinations of filter settings related to
    /// a locations overflow setting.
    /// </summary>
    /// <seealso cref="LocationOverflowOpportunityFilter"/>
    [TestClass]
    public class LocationOverflowOpportunityFilterTests : CheckInMockDatabase
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void OverflowFilter_WithNoOverflowLocation_IncludesOriginalLocations()
        {
            var personId = 20;

            var filter = CreateOverflowFilter( personId );
            var opportunities = new OpportunityCollection
            {
                Groups = new List<GroupOpportunity>
                {
                    new GroupOpportunity
                    {
                        Name = "Kindergarten",
                        Locations = new List<LocationAndScheduleBag>
                        {
                            new LocationAndScheduleBag
                            {
                                LocationId = "101"
                            }
                        },
                        OverflowLocations = new List<LocationAndScheduleBag>()
                    },
                    new GroupOpportunity
                    {
                        Name = "5yr olds",
                        Locations = new List<LocationAndScheduleBag>
                        {
                            new LocationAndScheduleBag
                            {
                                LocationId = "201"
                            }
                        },
                        OverflowLocations = new List<LocationAndScheduleBag>()
                    }
                },
                Locations = new List<LocationOpportunity>
                {
                    new LocationOpportunity
                    {
                        Id = "101"
                    },
                    new LocationOpportunity
                    {
                        Id = "201"
                    }
                }
            };

            filter.FilterLocations( opportunities );

            Assert.That.AreEqual( 1, opportunities.Groups[0].Locations.Count );
            Assert.That.AreEqual( "101", opportunities.Groups[0].Locations[0].LocationId );
            Assert.That.AreEqual( 1, opportunities.Groups[1].Locations.Count );
            Assert.That.AreEqual( "201", opportunities.Groups[1].Locations[0].LocationId );
            Assert.That.AreEqual( 2, opportunities.Locations.Count );
        }

        [TestMethod]
        public void OverflowFilter_WithBothRegularLocationsAndOverflowLocation_IncludesNormalLocations()
        {
            var personId = 20;

            var filter = CreateOverflowFilter( personId );
            var opportunities = new OpportunityCollection
            {
                Groups = new List<GroupOpportunity>
                {
                    new GroupOpportunity
                    {
                        Name = "Kindergarten",
                        Locations = new List<LocationAndScheduleBag>
                        {
                            new LocationAndScheduleBag
                            {
                                LocationId = "101"
                            }
                        },
                        OverflowLocations = new List<LocationAndScheduleBag>()
                    },
                    new GroupOpportunity
                    {
                        Name = "5yr olds",
                        Locations = new List<LocationAndScheduleBag>
                        {
                            new LocationAndScheduleBag
                            {
                                LocationId = "201"
                            }
                        },
                        OverflowLocations = new List<LocationAndScheduleBag>
                        {
                            new LocationAndScheduleBag
                            {
                                LocationId = "202"
                            }
                        }
                    }
                },
                Locations = new List<LocationOpportunity>
                {
                    new LocationOpportunity
                    {
                        Id = "101"
                    },
                    new LocationOpportunity
                    {
                        Id = "201"
                    },
                    new LocationOpportunity
                    {
                        Id = "202"
                    }
                }
            };

            filter.FilterLocations( opportunities );

            Assert.That.AreEqual( 1, opportunities.Groups[0].Locations.Count );
            Assert.That.AreEqual( "101", opportunities.Groups[0].Locations[0].LocationId );
            Assert.That.AreEqual( 0, opportunities.Groups[0].OverflowLocations.Count );

            Assert.That.AreEqual( 1, opportunities.Groups[1].Locations.Count );
            Assert.That.AreEqual( "201", opportunities.Groups[1].Locations[0].LocationId );
            Assert.That.AreEqual( 0, opportunities.Groups[1].OverflowLocations.Count );

            Assert.That.AreEqual( 2, opportunities.Locations.Count );
            Assert.That.AreEqual( "101", opportunities.Locations[0].Id );
            Assert.That.AreEqual( "201", opportunities.Locations[1].Id );
        }

        [TestMethod]
        public void OverflowFilter_WithOnlyOverflowLocation_IncludesOverflowLocations()
        {
            var personId = 20;

            var filter = CreateOverflowFilter( personId );
            var opportunities = new OpportunityCollection
            {
                Groups = new List<GroupOpportunity>
                {
                    new GroupOpportunity
                    {
                        Name = "Kindergarten",
                        Locations = new List<LocationAndScheduleBag>
                        {
                            new LocationAndScheduleBag
                            {
                                LocationId = "101"
                            }
                        },
                        OverflowLocations = new List<LocationAndScheduleBag>()
                    },
                    new GroupOpportunity
                    {
                        Name = "5yr olds",
                        Locations = new List<LocationAndScheduleBag>
                        {
                            new LocationAndScheduleBag
                            {
                                LocationId = "201"
                            }
                        },
                        OverflowLocations = new List<LocationAndScheduleBag>
                        {
                            new LocationAndScheduleBag
                            {
                                LocationId = "202"
                            }
                        }
                    }
                },
                Locations = new List<LocationOpportunity>
                {
                    // 101 and 201 are full, so they are not included here
                    // to simulate them being removed already.
                    new LocationOpportunity
                    {
                        Id = "202"
                    }
                }
            };

            filter.FilterLocations( opportunities );

            // The kindergarten group should now have no locations which will
            // cause it to be removed in production.
            Assert.That.AreEqual( 0, opportunities.Groups[0].Locations.Count );
            Assert.That.AreEqual( 0, opportunities.Groups[0].OverflowLocations.Count );

            Assert.That.AreEqual( 1, opportunities.Groups[1].Locations.Count );
            Assert.That.AreEqual( "202", opportunities.Groups[1].Locations[0].LocationId );
            Assert.That.AreEqual( 0, opportunities.Groups[1].OverflowLocations.Count );

            Assert.That.AreEqual( 1, opportunities.Locations.Count );
            Assert.That.AreEqual( "202", opportunities.Locations[0].Id );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="LocationOverflowOpportunityFilter"/> along with the
        /// person to be filtered.
        /// </summary>
        /// <param name="personId">The identifier of the person being checked in.</param>
        /// <returns>An instance of <see cref="LocationOverflowOpportunityFilter"/>.</returns>
        private LocationOverflowOpportunityFilter CreateOverflowFilter( int personId )
        {
            // Create the filter.
            var filter = new LocationOverflowOpportunityFilter
            {
                Person = new Attendee
                {
                    Person = new PersonBag()
                },
            };

            filter.Person.Person.Id = IdHasher.Instance.GetHash( personId );

            return filter;
        }

        #endregion
    }
}
