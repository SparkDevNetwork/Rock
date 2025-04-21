﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
using Rock.Enums.CheckIn;
using Rock.ViewModels.CheckIn;

namespace Rock.Tests.CheckIn.v2.Filters
{
    /// <summary>
    /// This suite checks the various combinations of filter settings related to
    /// a person's grade for the check-in process.
    /// </summary>
    /// <seealso cref="GradeOpportunityFilter"/>
    [TestClass]
    public class GradeOpportunityFilterTests
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void GradeFilter_WithNoConditions_IncludesAnyGrade()
        {
            var personGrade = 5;

            var filter = CreateGradeFilter( personGrade, false );
            var groupOpportunity = CreateGroupOpportunity( null, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void GradeFilter_WithNoConditions_IncludesUnknownGrade()
        {
            int? personGrade = null;

            var filter = CreateGradeFilter( personGrade, false );
            var groupOpportunity = CreateGroupOpportunity( null, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void GradeFilter_WithRequiredMinGrade_ExcludesUnknownGrade()
        {
            var minGrade = 3;

            var filter = CreateGradeFilter( null, true );
            var groupOpportunity = CreateGroupOpportunity( minGrade, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void GradeFilter_WithOptionalMinGrade_IncludesUnknownGrade()
        {
            var minGrade = 3;

            var filter = CreateGradeFilter( null, false );
            var groupOpportunity = CreateGroupOpportunity( minGrade, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void GradeFilter_WithRequiredMaxGrade_ExcludesUnknownGrade()
        {
            var maxGrade = 3;

            var filter = CreateGradeFilter( null, true );
            var groupOpportunity = CreateGroupOpportunity( null, maxGrade );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void GradeFilter_WithOptionalMaxGrade_IncludesUnknownGrade()
        {
            var maxGrade = 3;

            var filter = CreateGradeFilter( null, false );
            var groupOpportunity = CreateGroupOpportunity( null, maxGrade );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        [DataRow( 0 )]
        [DataRow( 1 )]
        [DataRow( 2 )]
        public void GradeFilter_WithOptionalMinGrade_ExcludesLowerGrade( int personGrade )
        {
            var minGrade = 3;

            var filter = CreateGradeFilter( personGrade, false );
            var groupOpportunity = CreateGroupOpportunity( minGrade, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void GradeFilter_WithOptionalMinGrade_IncludesEqualGrade()
        {
            var personGrade = 3;
            var minGrade = 3;

            var filter = CreateGradeFilter( personGrade, false );
            var groupOpportunity = CreateGroupOpportunity( minGrade, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        [DataRow( 4 )]
        [DataRow( 5 )]
        [DataRow( 6 )]
        public void GradeFilter_WithOptionalMinGrade_IncludesHigherGrade( int personGrade )
        {
            var minGrade = 3;

            var filter = CreateGradeFilter( personGrade, false );
            var groupOpportunity = CreateGroupOpportunity( minGrade, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        [DataRow( 0 )]
        [DataRow( 1 )]
        [DataRow( 2 )]
        public void GradeFilter_WithOptionalMaxGrade_IncludesLowerGrade( int personGrade )
        {
            var maxGrade = 3;

            var filter = CreateGradeFilter( personGrade, false );
            var groupOpportunity = CreateGroupOpportunity( null, maxGrade );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void GradeFilter_WithOptionalMAxGrade_IncludesEqualGrade()
        {
            var personGrade = 3;
            var maxGrade = 3;

            var filter = CreateGradeFilter( personGrade, false );
            var groupOpportunity = CreateGroupOpportunity( null, maxGrade );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        [DataRow( 4 )]
        [DataRow( 5 )]
        [DataRow( 6 )]
        public void GradeFilter_WithOptionalMaxGrade_ExcludesHigherGrade( int personGrade )
        {
            var maxGrade = 3;

            var filter = CreateGradeFilter( personGrade, false );
            var groupOpportunity = CreateGroupOpportunity( null, maxGrade );

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
        public void GradeFilter_WithOptionalMinAndMaxGrade_IncludesInRangeGrade( int personGrade )
        {
            var minGrade = 3;
            var maxGrade = 9;

            var filter = CreateGradeFilter( personGrade, false );
            var groupOpportunity = CreateGroupOpportunity( minGrade, maxGrade );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        [DataRow( 1 )]
        [DataRow( 2 )]
        [DataRow( 10 )]
        [DataRow( 11 )]
        [DataRow( 12 )]
        public void GradeFilter_WithOptionalMinAndMaxGrade_ExcludesOutOfRangeGrade( int personGrade )
        {
            var minGrade = 3;
            var maxGrade = 9;

            var filter = CreateGradeFilter( personGrade, false );
            var groupOpportunity = CreateGroupOpportunity( minGrade, maxGrade );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void GradeFilter_WithBehaviorOtherThanGradeAndAgeMustMatch_SkipsFilter()
        {
            // This test would normally fail, so we use this along with the
            // behavior to verify that it doesn't actually filter anything.
            var personGrade = 2;
            var minGrade = 3;

            var filter = CreateGradeFilter( personGrade, false, GradeAndAgeMatchingMode.PrioritizeGradeOverAge );
            var groupOpportunity = CreateGroupOpportunity( minGrade, null );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="GradeOpportunityFilter"/> along with a
        /// person to be filtered.
        /// </summary>
        /// <param name="grade">The grade to give the person if not <c>null</c>.</param>
        /// <param name="isGradeRequired"><c>true</c> if the configuration template should indicate that grade is required.</param>
        /// <param name="gradeAndAgeMatchingBehavior">The configuration value to use for the template.</param>
        /// <returns>An instance of <see cref="GradeOpportunityFilter"/>.</returns>
        private GradeOpportunityFilter CreateGradeFilter( int? grade, bool isGradeRequired, GradeAndAgeMatchingMode gradeAndAgeMatchingBehavior = GradeAndAgeMatchingMode.GradeAndAgeMustMatch )
        {
            // Create the template configuration.
            var templateConfigurationMock = new Mock<TemplateConfigurationData>( MockBehavior.Strict );

            templateConfigurationMock.Setup( m => m.IsGradeRequired ).Returns( isGradeRequired );
            templateConfigurationMock.Setup( m => m.GradeAndAgeMatchingBehavior ).Returns( gradeAndAgeMatchingBehavior );

            // Create the filter.
            var filter = new GradeOpportunityFilter
            {
                Person = new Attendee
                {
                    Person = new PersonBag()
                },
                TemplateConfiguration = templateConfigurationMock.Object
            };

            filter.Person.Person.GradeOffset = grade;

            return filter;
        }

        /// <summary>
        /// Creates a group opportunity with the specified grade range
        /// attribute value.
        /// </summary>
        /// <param name="minGradeOffset">The minimum grade offset for this group opportunity.</param>
        /// <param name="maxGradeOffset">The maximum grade offset for this group opportunity.</param>
        /// <returns>A new instance of <see cref="GroupOpportunity"/>.</returns>
        private GroupOpportunity CreateGroupOpportunity( int? minGradeOffset, int? maxGradeOffset )
        {
            var groupConfigurationMock = new Mock<GroupConfigurationData>( MockBehavior.Strict );

            groupConfigurationMock.Setup( m => m.MinimumGradeOffset ).Returns( minGradeOffset );
            groupConfigurationMock.Setup( m => m.MaximumGradeOffset ).Returns( maxGradeOffset );

            return new GroupOpportunity
            {
                CheckInData = groupConfigurationMock.Object
            };
        }

        #endregion
    }
}
