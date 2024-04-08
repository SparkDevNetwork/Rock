using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
using Rock.ViewModels.CheckIn;
using Rock.Web.Cache;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Filters
{
    [TestClass]
    public class GradeOpportunityFilterTests
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void IsGroupValid_BlankFilterWithGrade_IsTrue()
        {
            // Arrange
            var filter = CreateGradeFilter( 5, false );
            var groupOpportunity = CreateGroupOpportunity( null, null );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsTrue( isValid );
        }

        [TestMethod]
        public void IsGroupValid_BlankFilterWithoutGrade_IsTrue()
        {
            // Arrange
            var filter = CreateGradeFilter( null, false );
            var groupOpportunity = CreateGroupOpportunity( null, null );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsTrue( isValid );
        }

        [TestMethod]
        public void IsGroupValid_MinGradeRequiredFilterWithoutGrade_IsFalse()
        {
            // Arrange
            var filter = CreateGradeFilter( null, true );
            var groupOpportunity = CreateGroupOpportunity( 3, null );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsGroupValid_MinGradeNotRequiredFilterWithoutGrade_IsTrue()
        {
            // Arrange
            var filter = CreateGradeFilter( null, false );
            var groupOpportunity = CreateGroupOpportunity( 3, null );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsTrue( isValid );
        }

        [TestMethod]
        public void IsGroupValid_MaxGradeRequiredFilterWithoutGrade_IsFalse()
        {
            // Arrange
            var filter = CreateGradeFilter( null, true );
            var groupOpportunity = CreateGroupOpportunity( null, 3 );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsGroupValid_MaxGradeNotRequiredFilterWithoutGrade_IsTrue()
        {
            // Arrange
            var filter = CreateGradeFilter( null, false );
            var groupOpportunity = CreateGroupOpportunity( null, 3 );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsTrue( isValid );
        }

        [TestMethod]
        [DataRow( 0 )]
        [DataRow( 1 )]
        [DataRow( 2 )]
        public void IsGroupValid_MinGradeFilterWithLessThanGrade_IsFalse( int grade )
        {
            // Arrange
            var filter = CreateGradeFilter( grade, false );
            var groupOpportunity = CreateGroupOpportunity( 3, null );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsGroupValid_MinGradeFilterWithEqualGrade_IsTrue()
        {
            // Arrange
            var filter = CreateGradeFilter( 3, false );
            var groupOpportunity = CreateGroupOpportunity( 3, null );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsTrue( isValid );
        }

        [TestMethod]
        [DataRow( 4 )]
        [DataRow( 5 )]
        [DataRow( 6 )]
        public void IsGroupValid_MinGradeFilterWithGreaterThanGrade_IsTrue( int grade )
        {
            // Arrange
            var filter = CreateGradeFilter( grade, false );
            var groupOpportunity = CreateGroupOpportunity( 3, null );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsTrue( isValid );
        }

        [TestMethod]
        [DataRow( 0 )]
        [DataRow( 1 )]
        [DataRow( 2 )]
        public void IsGroupValid_MaxGradeFilterWithLessThanGrade_IsTrue( int month )
        {
            // Arrange
            var filter = CreateGradeFilter( 3, false );
            var groupOpportunity = CreateGroupOpportunity( null, 3 );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsTrue( isValid );
        }

        [TestMethod]
        public void IsGroupValid_MaxGradeFilterWithEqualGrade_IsTrue()
        {
            // Arrange
            var filter = CreateGradeFilter( 3, false );
            var groupOpportunity = CreateGroupOpportunity( null, 3 );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsTrue( isValid );
        }

        [TestMethod]
        [DataRow( 4 )]
        [DataRow( 5 )]
        [DataRow( 6 )]
        public void IsGroupValid_MaxGradeFilterWithGreaterThanGrade_IsFalse( int grade )
        {
            // Arrange
            var filter = CreateGradeFilter( grade, false );
            var groupOpportunity = CreateGroupOpportunity( null, 3 );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsFalse( isValid );
        }

        [TestMethod]
        [DataRow( 3 )]
        [DataRow( 4 )]
        [DataRow( 5 )]
        [DataRow( 6 )]
        [DataRow( 7 )]
        [DataRow( 8 )]
        [DataRow( 9 )]
        public void IsGroupValid_MinMaxGradeFilterWithInRangeGrade_IsTrue( int grade )
        {
            // Arrange
            var filter = CreateGradeFilter( grade, false );
            var groupOpportunity = CreateGroupOpportunity( 3, 9 );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsTrue( isValid );
        }

        [TestMethod]
        [DataRow( 1 )]
        [DataRow( 2 )]
        [DataRow( 10 )]
        [DataRow( 11 )]
        [DataRow( 12 )]
        public void IsGroupValid_MinMaxGradeFilterWithOutOfRangeGrade_IsFalse( int grade )
        {
            // Arrange
            var filter = CreateGradeFilter( grade, false );
            var groupOpportunity = CreateGroupOpportunity( 3, 9 );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsFalse( isValid );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="GradeOpportunityFilter"/> along with a
        /// person to be filtered.
        /// </summary>
        /// <param name="grade">The grade to give the person if not <c>null</c>.</param>
        /// <param name="isGradeRequired"><c>true</c> if the configuration template should indicate that grade is required.</param>
        /// <returns>An instance of <see cref="GradeOpportunityFilter"/>.</returns>
        private GradeOpportunityFilter CreateGradeFilter( int? grade, bool isGradeRequired )
        {
            // Create the template configuration.
            var templateConfigurationMock = new Mock<TemplateConfigurationData>( MockBehavior.Strict );

            templateConfigurationMock.Setup( m => m.IsGradeRequired ).Returns( isGradeRequired );

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
        /// <param name="gradeRange">The grade range.</param>
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
