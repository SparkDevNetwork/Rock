using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
using Rock.Utility;
using Rock.ViewModels.CheckIn;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// This suite checks the various combinations of filter settings related to
    /// a locations capacity threshold.
    /// </summary>
    /// <seealso cref="ThresholdOpportunityFilter"/>
    [TestClass]
    public class ThresholdOpportunityFilterTests : CheckInMockDatabase
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void ThresholdFilter_WithNewAttendee_IncludesLocationWithoutCapacity()
        {
            var personId = 20;

            var filter = CreateThresholdFilter( personId );
            var locationOpportunity = CreateLocationOpportunity( null, Array.Empty<int>() );

            var isIncluded = filter.IsLocationValid( locationOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void ThresholdFilter_WithNewAttendee_IncludesLocationUnderCapacity()
        {
            var capacity = 3;
            var personId = 20;
            var currentAttendeePersonIds = new[]
            {
                21,
                22
            };

            var filter = CreateThresholdFilter( personId );
            var locationOpportunity = CreateLocationOpportunity( capacity, currentAttendeePersonIds );

            var isIncluded = filter.IsLocationValid( locationOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void ThresholdFilter_WithNewAttendee_ExcludesLocationAtCapacity()
        {
            var capacity = 2;
            var personId = 20;
            var currentAttendeePersonIds = new[]
            {
                21,
                22
            };

            var filter = CreateThresholdFilter( personId );
            var locationOpportunity = CreateLocationOpportunity( capacity, currentAttendeePersonIds );

            var isIncluded = filter.IsLocationValid( locationOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void ThresholdFilter_WithNewAttendee_ExcludesLocationOverCapacity()
        {
            var capacity = 1;
            var personId = 20;
            var currentAttendeePersonIds = new[]
            {
                21,
                22
            };

            var filter = CreateThresholdFilter( personId );
            var locationOpportunity = CreateLocationOpportunity( capacity, currentAttendeePersonIds );

            var isIncluded = filter.IsLocationValid( locationOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void ThresholdFilter_WithAttendeeAlreadyInLocation_IncludesLocationAtCapacity()
        {
            var capacity = 2;
            var personId = 20;
            var currentAttendeePersonIds = new[]
            {
                21,
                personId
            };

            var filter = CreateThresholdFilter( personId );
            var locationOpportunity = CreateLocationOpportunity( capacity, currentAttendeePersonIds );

            var isIncluded = filter.IsLocationValid( locationOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void ThresholdFilter_WithAttendeeAlreadyInLocation_IncludesLocationOverCapacity()
        {
            var capacity = 1;
            var personId = 20;
            var currentAttendeePersonIds = new[]
            {
                21,
                personId
            };

            var filter = CreateThresholdFilter( personId );
            var locationOpportunity = CreateLocationOpportunity( capacity, currentAttendeePersonIds );

            var isIncluded = filter.IsLocationValid( locationOpportunity );

            Assert.IsTrue( isIncluded );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="ThresholdOpportunityFilter"/> along with the
        /// person to be filtered.
        /// </summary>
        /// <param name="personId">The identifier of the person being checked in.</param>
        /// <returns>An instance of <see cref="ThresholdOpportunityFilter"/>.</returns>
        private ThresholdOpportunityFilter CreateThresholdFilter( int personId )
        {
            // Create the filter.
            var filter = new ThresholdOpportunityFilter
            {
                Person = new Attendee
                {
                    Person = new PersonBag()
                },
            };

            filter.Person.Person.Id = IdHasher.Instance.GetHash( personId );

            return filter;
        }

        /// <summary>
        /// Creates a location opportunity with the specified capacity and occupants.
        /// </summary>
        /// <param name="capacity">The maximum location capacity or <c>null</c>.</param>
        /// <param name="personIds">The identifiers of the people already in this location.</param>
        /// <returns>A new instance of <see cref="LocationOpportunity"/>.</returns>
        private LocationOpportunity CreateLocationOpportunity( int? capacity, IReadOnlyCollection<int> personIds )
        {
            return new LocationOpportunity
            {
                Capacity = capacity,
                CurrentCount = personIds.Count,
                CurrentPersonIds = new HashSet<string>( personIds.Select( id => IdHasher.Instance.GetHash( id ) ) )
            };
        }

        #endregion
    }
}
