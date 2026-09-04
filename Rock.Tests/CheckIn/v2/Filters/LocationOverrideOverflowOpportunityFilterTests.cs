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
    /// automatically making overflow locations available during override.
    /// </summary>
    /// <seealso cref="LocationOverrideOverflowOpportunityFilter"/>
    [TestClass]
    public class LocationOverrideOverflowOpportunityFilterTests
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void OverrideOverflowFilter_WithNoOverflowLocation_IncludesOriginalLocations()
        {
            using var scope = TestHelper.CreateScopedRockApp();

            var filter = CreateOverflowFilter( scope.App.CreateRockContext() );
            var opportunities = new OpportunityCollection
            {
                Groups = [
                    new GroupOpportunity
                    {
                        Name = "Kindergarten",
                        Locations = [
                            new LocationAndScheduleBag
                            {
                                LocationId = "101"
                            }
                        ],
                        OverflowLocations = [],
                    },
                ],
                Locations = [
                    new LocationOpportunity
                    {
                        Id = "101"
                    },
                ],
            };

            filter.FilterLocations( opportunities );

            Assert.HasCount( 1, opportunities.Groups[0].Locations );
            Assert.AreEqual( "101", opportunities.Groups[0].Locations[0].LocationId );
        }

        [TestMethod]
        public void OverrideOverflowFilter_WithOverflowLocation_IncludesOriginalLocations()
        {
            using var scope = TestHelper.CreateScopedRockApp();

            var filter = CreateOverflowFilter( scope.App.CreateRockContext(), isOverride: true );
            var opportunities = new OpportunityCollection
            {
                Groups = [
                    new GroupOpportunity
                    {
                        Name = "Kindergarten",
                        Locations = [
                            new LocationAndScheduleBag
                            {
                                LocationId = "101"
                            }
                        ],
                        OverflowLocations = [
                            new LocationAndScheduleBag
                            {
                                LocationId = "201"
                            }
                        ],
                    },
                ],
                Locations = [
                    new LocationOpportunity
                    {
                        Id = "101"
                    },
                ],
            };

            filter.FilterLocations( opportunities );

            Assert.IsGreaterThanOrEqualTo( 1, opportunities.Groups[0].Locations.Count );
            Assert.AreEqual( "101", opportunities.Groups[0].Locations[0].LocationId );
        }

        [TestMethod]
        public void OverrideOverflowFilter_WithOverflowLocation_IncludesOverflowLocation()
        {
            using var scope = TestHelper.CreateScopedRockApp();

            var filter = CreateOverflowFilter( scope.App.CreateRockContext(), isOverride: true );
            var opportunities = new OpportunityCollection
            {
                Groups = [
                    new GroupOpportunity
                    {
                        Name = "Kindergarten",
                        Locations = [
                            new LocationAndScheduleBag
                            {
                                LocationId = "101"
                            }
                        ],
                        OverflowLocations = [
                            new LocationAndScheduleBag
                            {
                                LocationId = "201"
                            }
                        ],
                    },
                ],
                Locations = [
                    new LocationOpportunity
                    {
                        Id = "101"
                    },
                ],
            };

            filter.FilterLocations( opportunities );

            Assert.IsGreaterThanOrEqualTo( 2, opportunities.Groups[0].Locations.Count );
            Assert.AreEqual( "101", opportunities.Groups[0].Locations[0].LocationId );
            Assert.AreEqual( "201", opportunities.Groups[0].Locations[1].LocationId );
        }

        [TestMethod]
        public void OverrideOverflowFilter_WithoutOverride_ExcludesOverflowLocation()
        {
            using var scope = TestHelper.CreateScopedRockApp();

            var filter = CreateOverflowFilter( scope.App.CreateRockContext(), isOverride: false );
            var opportunities = new OpportunityCollection
            {
                Groups = [
                    new GroupOpportunity
                    {
                        Name = "Kindergarten",
                        Locations = [
                            new LocationAndScheduleBag
                            {
                                LocationId = "101"
                            }
                        ],
                        OverflowLocations = [
                            new LocationAndScheduleBag
                            {
                                LocationId = "201"
                            }
                        ],
                    },
                ],
                Locations = [
                    new LocationOpportunity
                    {
                        Id = "101"
                    },
                ],
            };

            filter.FilterLocations( opportunities );

            Assert.HasCount( 1, opportunities.Groups[0].Locations );
            Assert.AreEqual( "101", opportunities.Groups[0].Locations[0].LocationId );
        }

        [TestMethod]
        public void OverrideOverflowFilter_IsSkippedDuringOverride_IsFalse()
        {
            using var scope = TestHelper.CreateScopedRockApp();

            var filter = CreateOverflowFilter( scope.App.CreateRockContext(), isOverride: false );

            Assert.IsFalse( filter.IsSkippedDuringOverride );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="LocationOverrideOverflowOpportunityFilter"/> along with the
        /// person to be filtered.
        /// </summary>
        /// <param name="personId">The identifier of the person being checked in.</param>
        /// <returns>An instance of <see cref="LocationOverrideOverflowOpportunityFilter"/>.</returns>
        private LocationOverrideOverflowOpportunityFilter CreateOverflowFilter( RockContext rockContext, bool isOverride = false )
        {
            // Create the template configuration.
            var templateConfigurationMock = new Mock<TemplateConfigurationData>( MockBehavior.Strict );

            var director = new CheckInDirector( rockContext );

            // Create the filter.
            var filter = new LocationOverrideOverflowOpportunityFilter
            {
                Session = new CheckInSession( director, templateConfigurationMock.Object )
                {
                    IsOverrideEnabled = isOverride,
                },
                TemplateConfiguration = templateConfigurationMock.Object,
            };

            return filter;
        }

        #endregion
    }
}
