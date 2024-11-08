using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
using Rock.Enums.CheckIn;
using Rock.Model;
using Rock.ViewModels.CheckIn;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// This suite checks the various combinations of filter settings related to
    /// a person's ability level for the check-in process.
    /// </summary>
    /// <seealso cref="AbilityLevelOpportunityFilter"/>
    [TestClass]
    public class AbilityLevelOpportunityFilterTests
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void IsGroupValid_WithDoNotAsk_IncludesGroupWithMatchingAbilityLevel()
        {
            var filter = CreateAbilityLevelFilter( AbilityLevelDeterminationMode.DoNotAsk, "123" );
            var groupOpportunity = CreateGroupOpportunity( "123" );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithDoNotAsk_ExcludesGroupWithEmptyPersonAbilityLevel()
        {
            var filter = CreateAbilityLevelFilter( AbilityLevelDeterminationMode.DoNotAsk, null );
            var groupOpportunity = CreateGroupOpportunity( "123" );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithDoNotAsk_ExcludesGroupWithMismatchedPersonAbilityLevel()
        {
            var filter = CreateAbilityLevelFilter( AbilityLevelDeterminationMode.DoNotAsk, "123" );
            var groupOpportunity = CreateGroupOpportunity( "456" );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithDoNotAskIfThereIsNoAbilityLevel_ExcludesGroupWhenPersonHasNoAbilityLevel()
        {
            var filter = CreateAbilityLevelFilter( AbilityLevelDeterminationMode.DoNotAskIfThereIsNoAbilityLevel, null );
            var groupOpportunity = CreateGroupOpportunity( "123" );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithDoNotAskIfThereIsNoAbilityLevel_IncludesGroupWithoutAbilityLevel()
        {
            var filter = CreateAbilityLevelFilter( AbilityLevelDeterminationMode.DoNotAskIfThereIsNoAbilityLevel, null );
            var groupOpportunity = CreateGroupOpportunity( null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithDoNotAskIfThereIsAnAbilityLevel_ExcludesGroupWithAbilityLevelWhenPersonHasAbilityLevel()
        {
            var filter = CreateAbilityLevelFilter( AbilityLevelDeterminationMode.DoNotAskIfThereIsAnAbilityLevel, "123" );
            var groupOpportunity = CreateGroupOpportunity( "456" );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithDoNotAskIfThereIsAnAbilityLevel_IncludesGroupWithoutAbilityLevel()
        {
            var filter = CreateAbilityLevelFilter( AbilityLevelDeterminationMode.DoNotAskIfThereIsNoAbilityLevel, "123" );
            var groupOpportunity = CreateGroupOpportunity( null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithDoNotAskIfThereIsAnAbilityLevel_IncludesGroupWithMatchingAbilityLevel()
        {
            var filter = CreateAbilityLevelFilter( AbilityLevelDeterminationMode.DoNotAskIfThereIsAnAbilityLevel, "123" );
            var groupOpportunity = CreateGroupOpportunity( "123" );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="AbilityLevelOpportunityFilter"/> along with a
        /// person to be filtered.
        /// </summary>
        /// <param name="determinationMode">The <see cref="AbilityLevelDeterminationMode"/> to configure check-in with.</param>
        /// <param name="abilityLevelId">The ability level to attach to the person.</param>
        /// <returns>An instance of <see cref="AbilityLevelOpportunityFilter"/>.</returns>
        private AbilityLevelOpportunityFilter CreateAbilityLevelFilter( AbilityLevelDeterminationMode determinationMode, string abilityLevelId )
        {
            // Create the template configuration.
            var templateConfigurationMock = new Mock<TemplateConfigurationData>( MockBehavior.Strict );

            templateConfigurationMock.Setup( c => c.AbilityLevelDetermination )
                .Returns( determinationMode );

            var filter = new AbilityLevelOpportunityFilter
            {
                Person = new Attendee
                {
                    Person = new PersonBag()
                    {
                    }
                },
                TemplateConfiguration = templateConfigurationMock.Object
            };

            if ( abilityLevelId != null )
            {
                filter.Person.Person.AbilityLevel = new CheckInItemBag
                {
                    Id = abilityLevelId
                };
            }

            return filter;
        }

        /// <summary>
        /// Creates a group opportunity with the specified gender filter value.
        /// </summary>
        /// <param name="requiredAbilityLevelId">The required ability level value or <c>null</c>.</param>
        /// <returns>A new instance of <see cref="GroupOpportunity"/>.</returns>
        private GroupOpportunity CreateGroupOpportunity( string requiredAbilityLevelId )
        {
            var groupConfigurationMock = new Mock<GroupConfigurationData>( MockBehavior.Strict );

            return new GroupOpportunity
            {
                AbilityLevelId = requiredAbilityLevelId,
                CheckInData = groupConfigurationMock.Object
            };
        }

        #endregion
    }
}
