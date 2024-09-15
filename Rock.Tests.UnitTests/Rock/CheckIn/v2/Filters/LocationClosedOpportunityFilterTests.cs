using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
using Rock.ViewModels.CheckIn;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// This suite checks the various combinations of filter settings related to
    /// a location being open or closed for the check-in process.
    /// </summary>
    /// <seealso cref="LocationClosedOpportunityFilter"/>
    [TestClass]
    public class LocationClosedOpportunityFilterTests
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void LocationFilter_WithNoConditions_IncludesOpenLocation()
        {
            var locationIsClosed = false;

            var filter = CreateLocationClosedFilter();
            var locationOpportunity = CreateLocationOpportunity( locationIsClosed );

            var isIncluded = filter.IsLocationValid( locationOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void LocationFilter_WithNoConditions_ExcludesClosedLocation()
        {
            var locationIsClosed = true;

            var filter = CreateLocationClosedFilter();
            var locationOpportunity = CreateLocationOpportunity( locationIsClosed );

            var isIncluded = filter.IsLocationValid( locationOpportunity );

            Assert.IsFalse( isIncluded );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="LocationClosedOpportunityFilter"/> along with a
        /// person to be filtered.
        /// </summary>
        /// <returns>An instance of <see cref="LocationClosedOpportunityFilter"/>.</returns>
        private LocationClosedOpportunityFilter CreateLocationClosedFilter()
        {
            // Create the template configuration.
            var templateConfigurationMock = new Mock<TemplateConfigurationData>( MockBehavior.Strict );

            var filter = new LocationClosedOpportunityFilter
            {
                Person = new Attendee
                {
                    Person = new PersonBag()
                },
                TemplateConfiguration = templateConfigurationMock.Object
            };

            return filter;
        }

        /// <summary>
        /// Creates a location opportunity with the open/closed state.
        /// </summary>
        /// <returns>A new instance of <see cref="GroupOpportunity"/>.</returns>
        private LocationOpportunity CreateLocationOpportunity( bool isClosed )
        {
            return new LocationOpportunity
            {
                IsClosed = isClosed
            };
        }

        #endregion
    }
}
