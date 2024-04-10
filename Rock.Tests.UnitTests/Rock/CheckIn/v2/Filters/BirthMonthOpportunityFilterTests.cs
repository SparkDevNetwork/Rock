using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
using Rock.ViewModels.CheckIn;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// This suite checks the various combinations of filter settings related to
    /// a person's birth month for the check-in process.
    /// </summary>
    /// <seealso cref="BirthMonthOpportunityFilter"/>
    [TestClass]
    public class BirthMonthOpportunityFilterTests
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void BirthMonthFilter_WithNoConditions_IncludesBirthdate()
        {
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, 4, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( null, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void BirthMonthFilter_WithNoConditions_IncludesEmptyBirthdate()
        {
            var filter = CreateBirthMonthFilter();
            var groupOpportunity = CreateGroupOpportunity( null, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void BirthMonthFilter_WithMinMonth_ExcludesEmptyBirthdate()
        {
            var filter = CreateBirthMonthFilter();
            var groupOpportunity = CreateGroupOpportunity( 3, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void BirthMonthFilter_WithMaxMonth_ExcludesEmptyBirthdate()
        {
            var filter = CreateBirthMonthFilter();
            var groupOpportunity = CreateGroupOpportunity( null, 3 );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        [DataRow( 1 )]
        [DataRow( 2 )]
        [DataRow( 3 )]
        [DataRow( 4 )]
        [DataRow( 5 )]
        [DataRow( 6 )]
        [DataRow( 7 )]
        [DataRow( 8 )]
        public void BirthMonthFilter_WithMinMonth_ExcludesLowerBirthMonth( int month )
        {
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, month, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( 9, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void BirthMonthFilter_WithMinMonth_IncludesEqualBirthMonth()
        {
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, 9, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( 9, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        [DataRow( 10 )]
        [DataRow( 11 )]
        [DataRow( 12 )]
        public void BirthMonthFilter_WithMinMonth_IncludesHigherBirthMonth( int month )
        {
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, month, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( 9, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        [DataRow( 1 )]
        [DataRow( 2 )]
        [DataRow( 3 )]
        [DataRow( 4 )]
        [DataRow( 5 )]
        [DataRow( 6 )]
        [DataRow( 7 )]
        [DataRow( 8 )]
        public void BirthMonthFilter_WithMaxMonth_IncludesLowerBirthMonth( int month )
        {
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, month, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( null, 9 );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void BirthMonthFilter_WithMaxMonth_IncludesEqualBirthMonth()
        {
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, 9, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( null, 9 );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        [DataRow( 10 )]
        [DataRow( 11 )]
        [DataRow( 12 )]
        public void BirthMonthFilter_WithMaxMonth_ExcludesHigherBirthMonth( int month )
        {
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, month, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( null, 9 );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        [DataRow( 3 )]
        [DataRow( 4 )]
        [DataRow( 5 )]
        [DataRow( 6 )]
        [DataRow( 7 )]
        [DataRow( 8 )]
        [DataRow( 9 )]
        public void BirthMonthFilter_WithMinAndMaxMonth_IncludesInRangeBirthMonth( int month )
        {
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, month, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( 3, 9 );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        [DataRow( 1 )]
        [DataRow( 2 )]
        [DataRow( 10 )]
        [DataRow( 11 )]
        [DataRow( 12 )]
        public void BirthMonthFilter_WithMinAndMaxMonth_ExcludesOutOfRangeBirthMonth( int month )
        {
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, month, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( 3, 9 );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        [DataRow( 1 )]
        [DataRow( 2 )]
        [DataRow( 3 )]
        [DataRow( 9 )]
        [DataRow( 10 )]
        [DataRow( 11 )]
        [DataRow( 12 )]
        public void BirthMonthFilter_WithInvertedMinAndMaxMonth_IncludesInRangeBirthMonth( int month )
        {
            // An inverted min and max value means between "October and March".
            // So the months starting in October and ending in March should be
            // included.
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, month, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( 9, 3 );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        [DataRow( 4 )]
        [DataRow( 5 )]
        [DataRow( 6 )]
        [DataRow( 7 )]
        [DataRow( 8 )]
        public void BirthMonthFilter_WithInvertedMinAndMaxMonth_ExcludesOutOfRangeBirthMonth( int month )
        {
            // An inverted min and max value means between "October and March".
            // So the months starting in April and ending in September should be
            // excluded.
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, month, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( 9, 3 );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="BirthMonthOpportunityFilter"/> along with a
        /// person to be filtered.
        /// </summary>
        /// <param name="birthdate">The birthdate to give the person if not <c>null</c>.</param>
        /// <returns>An instance of <see cref="BirthMonthOpportunityFilter"/>.</returns>
        private BirthMonthOpportunityFilter CreateBirthMonthFilter( DateTime? birthdate = null )
        {
            var filter = new BirthMonthOpportunityFilter
            {
                Person = new Attendee
                {
                    Person = new PersonBag()
                }
            };

            if ( birthdate.HasValue )
            {
                filter.Person.Person.BirthDate = birthdate;
                filter.Person.Person.BirthYear = filter.Person.Person.BirthDate.Value.Year;
                filter.Person.Person.BirthMonth = filter.Person.Person.BirthDate.Value.Month;
                filter.Person.Person.BirthDay = filter.Person.Person.BirthDate.Value.Day;
            }

            return filter;
        }

        /// <summary>
        /// Creates a group opportunity with the specified birth month range
        /// attribute value.
        /// </summary>
        /// <param name="minBirthMonth">The minimum birth month value or <c>null</c>.</param>
        /// <param name="maxBirthMonth">The maximum birth month value or <c>null</c>.</param>
        /// <returns>A new instance of <see cref="GroupOpportunity"/>.</returns>
        private GroupOpportunity CreateGroupOpportunity( int? minBirthMonth, int? maxBirthMonth )
        {
            var groupConfigurationMock = new Mock<GroupConfigurationData>( MockBehavior.Strict );

            groupConfigurationMock.Setup( m => m.MinimumBirthMonth ).Returns( minBirthMonth );
            groupConfigurationMock.Setup( m => m.MaximumBirthMonth ).Returns( maxBirthMonth );

            return new GroupOpportunity
            {
                CheckInData = groupConfigurationMock.Object
            };
        }

        #endregion
    }
}
