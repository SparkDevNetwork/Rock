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
    /// a person's age and birthdate for the check-in process.
    /// </summary>
    /// <seealso cref="AgeOpportunityFilter"/>
    [TestClass]
    public class AgeOpportunityFilterTests
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void AgeFilter_WithNoConditions_IncludesAnyBirthdate()
        {
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old

            var filter = CreateAgeFilter( birthdate, false );
            var groupOpportunity = CreateGroupOpportunity( null, null, null, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithNoConditions_IncludesEmptyBirthdate()
        {
            var filter = CreateAgeFilter( null, false );
            var groupOpportunity = CreateGroupOpportunity( null, null, null, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithOptionalMinAge_IncludesEmptyAge()
        {
            var minAge = 3;

            var filter = CreateAgeFilter( null, false );
            var groupOpportunity = CreateGroupOpportunity( minAge, null, null, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithOptionalMaxAge_IncludesEmptyAge()
        {
            var maxAge = 4;

            var filter = CreateAgeFilter( null, false );
            var groupOpportunity = CreateGroupOpportunity( null, maxAge, null, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMinAge_ExcludesEmptyAge()
        {
            var minAge = 3;

            var filter = CreateAgeFilter( null, true );
            var groupOpportunity = CreateGroupOpportunity( minAge, null, null, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMaxAge_ExcludesEmptyAge()
        {
            var maxAge = 4;

            var filter = CreateAgeFilter( null, true );
            var groupOpportunity = CreateGroupOpportunity( null, maxAge, null, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMinAge_IncludesHigherAge()
        {
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old
            var minAge = 3;

            var filter = CreateAgeFilter( birthdate, true );
            var groupOpportunity = CreateGroupOpportunity( minAge, null, null, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMinAge_ExcludesLowerAge()
        {
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old
            var minAge = 4;

            var filter = CreateAgeFilter( birthdate, true );
            var groupOpportunity = CreateGroupOpportunity( minAge, null, null, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMaxAge_IncludesHigherAge()
        {
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old
            var maxAge = 4;

            var filter = CreateAgeFilter( birthdate, true );
            var groupOpportunity = CreateGroupOpportunity( null, maxAge, null, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMaxAge_ExcludesLowerAge()
        {
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old
            var maxAge = 3.01M;

            var filter = CreateAgeFilter( birthdate, true );
            var groupOpportunity = CreateGroupOpportunity( null, maxAge, null, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMaxAge_IncludesSamePrecisionAge()
        {
            // The test here is to make sure that even though the precise age
            // is 3.492 (give or take), a max age of "3" will include it by
            // determining a precision of 0 decimal places so the precise age
            // is truncated to that same precision, which leaves us with an
            // age of 3 as well.
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old
            var maxAge = 3;

            var filter = CreateAgeFilter( birthdate, true );
            var groupOpportunity = CreateGroupOpportunity( null, maxAge, null, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMinAndMaxAge_IncludesMatchingAge()
        {
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old
            var minAge = 3;
            var maxAge = 4;

            var filter = CreateAgeFilter( birthdate, true );
            var groupOpportunity = CreateGroupOpportunity( minAge, maxAge, null, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMinBirthdate_ExcludesEmptyBirthdate()
        {
            var minBirthdate = RockDateTime.Now.AddYears( -4 );

            var filter = CreateAgeFilter( null, true );
            var groupOpportunity = CreateGroupOpportunity( null, null, minBirthdate, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMaxBirthdate_ExcludesEmptyBirthdate()
        {
            var maxBirthdate = RockDateTime.Now.AddYears( -3 );

            var filter = CreateAgeFilter( null, true );
            var groupOpportunity = CreateGroupOpportunity( null, null, null, maxBirthdate );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithOptionalMinBirthdate_IncludesEmptyBirthdate()
        {
            var minBirthdate = RockDateTime.Now.AddYears( -4 );

            var filter = CreateAgeFilter( null, false );
            var groupOpportunity = CreateGroupOpportunity( null, null, minBirthdate, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithOptionalMaxBirthdate_IncludesEmptyBirthdate()
        {
            var maxBirthdate = RockDateTime.Now.AddYears( -3 );

            var filter = CreateAgeFilter( null, false );
            var groupOpportunity = CreateGroupOpportunity( null, null, null, maxBirthdate );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMinBirthdate_IncludesLaterBirthdate()
        {
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old
            var minBirthdate = RockDateTime.Now.AddYears( -4 );

            var filter = CreateAgeFilter( birthdate, true );
            var groupOpportunity = CreateGroupOpportunity( null, null, minBirthdate, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMinBirthdate_IncludesSameDayBirthdate()
        {
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old
            var minBirthdate = birthdate;

            var filter = CreateAgeFilter( birthdate, true );
            var groupOpportunity = CreateGroupOpportunity( null, null, minBirthdate, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMinBirthdate_ExcludesEarlierBirthdate()
        {
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old
            var minBirthdate = RockDateTime.Now.AddYears( -3 );

            var filter = CreateAgeFilter( birthdate, true );
            var groupOpportunity = CreateGroupOpportunity( null, null, minBirthdate, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMaxBirthdate_IncludesEarlierBirthdate()
        {
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old
            var maxBirthdate = RockDateTime.Now.AddYears( -3 );

            var filter = CreateAgeFilter( birthdate, true );
            var groupOpportunity = CreateGroupOpportunity( null, null, null, maxBirthdate );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMaxBirthdate_IncludesSameDayBirthdate()
        {
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old
            var maxBirthdate = birthdate;

            var filter = CreateAgeFilter( birthdate, true );
            var groupOpportunity = CreateGroupOpportunity( null, null, null, maxBirthdate );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMaxBirthdate_ExcludesLaterBirthdate()
        {
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old
            var maxBirthdate = RockDateTime.Now.AddYears( -4 );

            var filter = CreateAgeFilter( birthdate, true );
            var groupOpportunity = CreateGroupOpportunity( null, null, null, maxBirthdate );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMinAndMaxBirthdate_IncludesMatchingBirthdate()
        {
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old
            var minBirthdate = RockDateTime.Now.AddYears( -4 );
            var maxBirthdate = RockDateTime.Now.AddYears( -3 );

            var filter = CreateAgeFilter( birthdate, true );
            var groupOpportunity = CreateGroupOpportunity( null, null, minBirthdate, maxBirthdate );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMinAgeAndMaxBirthdate_IncludesMatchingAgeWithoutMatchingBirthdate()
        {
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old
            var minAge = 3;
            var maxBirthdate = RockDateTime.Now.AddYears( -5 );

            var filter = CreateAgeFilter( birthdate, true );
            var groupOpportunity = CreateGroupOpportunity( minAge, null, null, maxBirthdate );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void AgeFilter_WithRequiredMinAgeAndMaxBirthdate_IncludesMatchingBirthdateWithoutMatchingAge()
        {
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old
            var minAge = 4;
            var maxBirthdate = RockDateTime.Now.AddYears( -3);

            var filter = CreateAgeFilter( birthdate, true );
            var groupOpportunity = CreateGroupOpportunity( minAge, null, null, maxBirthdate );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="AgeOpportunityFilter"/> along with a
        /// person to be filtered.
        /// </summary>
        /// <param name="personBirthdate">The birthdate to give the person if not <c>null</c>.</param>
        /// <param name="isAgeRequired"><c>true</c> if the configuration specifies that age is required; otherwise <c>false</c>.</param>
        /// <returns>An instance of <see cref="AgeOpportunityFilter"/>.</returns>
        private AgeOpportunityFilter CreateAgeFilter( DateTime? personBirthdate, bool isAgeRequired)
        {
            // Create the template configuration.
            var templateConfigurationMock = new Mock<TemplateConfigurationData>( MockBehavior.Strict );

            templateConfigurationMock.Setup( m => m.IsAgeRequired ).Returns( isAgeRequired );

            var filter = new AgeOpportunityFilter
            {
                Person = new Attendee
                {
                    Person = new PersonBag()
                },
                TemplateConfiguration = templateConfigurationMock.Object
            };

            if ( personBirthdate.HasValue )
            {
                filter.Person.Person.BirthDate = personBirthdate;
                filter.Person.Person.BirthYear = filter.Person.Person.BirthDate.Value.Year;
                filter.Person.Person.BirthMonth = filter.Person.Person.BirthDate.Value.Month;
                filter.Person.Person.BirthDay = filter.Person.Person.BirthDate.Value.Day;

                var tempPerson = new global::Rock.Model.Person
                {
                    BirthYear = personBirthdate.Value.Year,
                    BirthMonth = personBirthdate.Value.Month,
                    BirthDay = personBirthdate.Value.Day
                };

                filter.Person.Person.AgePrecise = tempPerson.AgePrecise;
            }

            return filter;
        }

        /// <summary>
        /// Creates a group opportunity with the specified birth month range
        /// attribute value.
        /// </summary>
        /// <param name="minAge">The minimum age value or <c>null</c>.</param>
        /// <param name="maxAge">The maximum age value or <c>null</c>.</param>
        /// <param name="minBirthdate">The minimum birthdate value or <c>null</c>.</param>
        /// <param name="maxBirthdate">The maximum birthdate value or <c>null</c>.</param>
        /// <returns>A new instance of <see cref="GroupOpportunity"/>.</returns>
        private GroupOpportunity CreateGroupOpportunity( decimal? minAge, decimal? maxAge, DateTime? minBirthdate, DateTime? maxBirthdate )
        {
            var groupConfigurationMock = new Mock<GroupConfigurationData>( MockBehavior.Strict );

            groupConfigurationMock.Setup( m => m.MinimumAge ).Returns( minAge );
            groupConfigurationMock.Setup( m => m.MaximumAge ).Returns( maxAge );
            groupConfigurationMock.Setup( m => m.MinimumBirthdate ).Returns( minBirthdate );
            groupConfigurationMock.Setup( m => m.MaximumBirthdate ).Returns( maxBirthdate );

            return new GroupOpportunity
            {
                CheckInData = groupConfigurationMock.Object
            };
        }

        #endregion
    }
}
