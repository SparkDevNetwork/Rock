using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
using Rock.Model;
using Rock.ViewModels.CheckIn;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// This suite checks the various combinations of filter settings related to
    /// a person's gender for the check-in process.
    /// </summary>
    /// <seealso cref="GenderOpportunityFilter"/>
    [TestClass]
    public class GenderOpportunityFilterTests
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void GenderFilter_WithNoConditions_IncludesUnknownGender()
        {
            var personGender = Gender.Unknown;

            var filter = CreateGenderFilter( personGender );
            var groupOpportunity = CreateGroupOpportunity( null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void GenderFilter_WithMaleGender_IncludesMaleGender()
        {
            var personGender = Gender.Male;
            var groupGender = Gender.Male;

            var filter = CreateGenderFilter( personGender );
            var groupOpportunity = CreateGroupOpportunity( groupGender );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void GenderFilter_WithMale_ExcludesFemaleGender()
        {
            var personGender = Gender.Female;
            var groupGender = Gender.Male;

            var filter = CreateGenderFilter( personGender );
            var groupOpportunity = CreateGroupOpportunity( groupGender );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void GenderFilter_WithMale_ExcludesUnknownGender()
        {
            var personGender = Gender.Unknown;
            var groupGender = Gender.Male;

            var filter = CreateGenderFilter( personGender );
            var groupOpportunity = CreateGroupOpportunity( groupGender );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="GenderOpportunityFilter"/> along with a
        /// person to be filtered.
        /// </summary>
        /// <param name="personGender">The recent attendance records to attach to the person.</param>
        /// <returns>An instance of <see cref="GenderOpportunityFilter"/>.</returns>
        private GenderOpportunityFilter CreateGenderFilter( Gender personGender )
        {
            // Create the template configuration.
            var templateConfigurationMock = new Mock<TemplateConfigurationData>( MockBehavior.Strict );

            var filter = new GenderOpportunityFilter
            {
                Person = new Attendee
                {
                    Person = new PersonBag()
                },
                TemplateConfiguration = templateConfigurationMock.Object
            };

            filter.Person.Person.Gender = personGender;

            return filter;
        }

        /// <summary>
        /// Creates a group opportunity with the specified gender filter value.
        /// </summary>
        /// <param name="requiredGender">The required gender value or <c>null</c>.</param>
        /// <returns>A new instance of <see cref="GroupOpportunity"/>.</returns>
        private GroupOpportunity CreateGroupOpportunity( Gender? requiredGender )
        {
            var groupConfigurationMock = new Mock<GroupConfigurationData>( MockBehavior.Strict );

            groupConfigurationMock.Setup( m => m.Gender ).Returns( requiredGender );

            return new GroupOpportunity
            {
                CheckInData = groupConfigurationMock.Object
            };
        }

        #endregion
    }
}
