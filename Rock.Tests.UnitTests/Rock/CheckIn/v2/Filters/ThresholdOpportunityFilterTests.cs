using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
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
            var personGuid = new Guid( "ad90891f-274d-4b8b-838e-61144080288e" );

            var filter = CreateThresholdFilter( personGuid );
            var locationOpportunity = CreateLocationOpportunity( null, Array.Empty<Guid>() );

            var isIncluded = filter.IsLocationValid( locationOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void ThresholdFilter_WithNewAttendee_IncludesLocationUnderCapacity()
        {
            var capacity = 3;
            var personGuid = new Guid( "ad90891f-274d-4b8b-838e-61144080288e" );
            var currentAttendeePersonGuids = new[]
            {
                new Guid( "10317735-d78f-419b-97ef-ca379753fbbd" ),
                new Guid( "32eea596-fabd-4732-bd2c-63c1f89b81a3" )
            };

            var filter = CreateThresholdFilter( personGuid );
            var locationOpportunity = CreateLocationOpportunity( capacity, currentAttendeePersonGuids );

            var isIncluded = filter.IsLocationValid( locationOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void ThresholdFilter_WithNewAttendee_ExcludesLocationAtCapacity()
        {
            var capacity = 2;
            var personGuid = new Guid( "ad90891f-274d-4b8b-838e-61144080288e" );
            var currentAttendeePersonGuids = new[]
            {
                new Guid( "10317735-d78f-419b-97ef-ca379753fbbd" ),
                new Guid( "32eea596-fabd-4732-bd2c-63c1f89b81a3" )
            };

            var filter = CreateThresholdFilter( personGuid );
            var locationOpportunity = CreateLocationOpportunity( capacity, currentAttendeePersonGuids );

            var isIncluded = filter.IsLocationValid( locationOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void ThresholdFilter_WithNewAttendee_ExcludesLocationOverCapacity()
        {
            var capacity = 1;
            var personGuid = new Guid( "ad90891f-274d-4b8b-838e-61144080288e" );
            var currentAttendeePersonGuids = new[]
            {
                new Guid( "10317735-d78f-419b-97ef-ca379753fbbd" ),
                new Guid( "32eea596-fabd-4732-bd2c-63c1f89b81a3" )
            };

            var filter = CreateThresholdFilter( personGuid );
            var locationOpportunity = CreateLocationOpportunity( capacity, currentAttendeePersonGuids );

            var isIncluded = filter.IsLocationValid( locationOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void ThresholdFilter_WithAttendeeAlreadyInLocation_IncludesLocationAtCapacity()
        {
            var capacity = 2;
            var personGuid = new Guid( "ad90891f-274d-4b8b-838e-61144080288e" );
            var currentAttendeePersonGuids = new[]
            {
                new Guid( "10317735-d78f-419b-97ef-ca379753fbbd" ),
                personGuid
            };

            var filter = CreateThresholdFilter( personGuid );
            var locationOpportunity = CreateLocationOpportunity( capacity, currentAttendeePersonGuids );

            var isIncluded = filter.IsLocationValid( locationOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void ThresholdFilter_WithAttendeeAlreadyInLocation_IncludesLocationOverCapacity()
        {
            var capacity = 1;
            var personGuid = new Guid( "ad90891f-274d-4b8b-838e-61144080288e" );
            var currentAttendeePersonGuids = new[]
            {
                new Guid( "10317735-d78f-419b-97ef-ca379753fbbd" ),
                personGuid
            };

            var filter = CreateThresholdFilter( personGuid );
            var locationOpportunity = CreateLocationOpportunity( capacity, currentAttendeePersonGuids );

            var isIncluded = filter.IsLocationValid( locationOpportunity );

            Assert.IsTrue( isIncluded );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="ThresholdOpportunityFilter"/> along with the
        /// person to be filtered.
        /// </summary>
        /// <param name="personGuid">The unique identifier of the person being checked in.</param>
        /// <returns>An instance of <see cref="ThresholdOpportunityFilter"/>.</returns>
        private ThresholdOpportunityFilter CreateThresholdFilter( Guid personGuid )
        {
            // Create the filter.
            var filter = new ThresholdOpportunityFilter
            {
                Person = new Attendee
                {
                    Person = new PersonBag()
                },
            };

            filter.Person.Person.Guid = personGuid;

            return filter;
        }

        /// <summary>
        /// Creates a location opportunity with the specified capacity and occupants.
        /// </summary>
        /// <param name="capacity">The maximum location capacity or <c>null</c>.</param>
        /// <param name="personGuids">The unique identifiers of the people already in this location.</param>
        /// <returns>A new instance of <see cref="LocationOpportunity"/>.</returns>
        private LocationOpportunity CreateLocationOpportunity( int? capacity, IReadOnlyCollection<Guid> personGuids )
        {
            return new LocationOpportunity
            {
                Capacity = capacity,
                CurrentCount = personGuids.Count,
                CurrentPersonGuids = new HashSet<Guid>( personGuids )
            };
        }

        #endregion
    }
}
