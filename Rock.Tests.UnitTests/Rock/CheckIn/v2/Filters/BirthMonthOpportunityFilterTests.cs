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
    public class BirthMonthOpportunityFilterTests
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void IsGroupValid_BlankFilterWithBirthdate_IsTrue()
        {
            // Arrange
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, 4, 12 ) );
            var groupOpportunity = CreateGroupOpportunity();

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsTrue( isValid );
        }

        [TestMethod]
        public void IsGroupValid_BlankFilterWithoutBirthdate_IsTrue()
        {
            // Arrange
            var filter = CreateBirthMonthFilter();
            var groupOpportunity = CreateGroupOpportunity();

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsTrue( isValid );
        }

        [TestMethod]
        public void IsGroupValid_MinMonthFilterWithoutBirthdate_IsFalse()
        {
            // Arrange
            var filter = CreateBirthMonthFilter();
            var groupOpportunity = CreateGroupOpportunity( "3," );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsGroupValid_MaxMonthFilterWithoutBirthdate_IsFalse()
        {
            // Arrange
            var filter = CreateBirthMonthFilter();
            var groupOpportunity = CreateGroupOpportunity( ",3" );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsFalse( isValid );
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
        public void IsGroupValid_MinMonthFilterWithLessThanBirthMonth_IsFalse( int month )
        {
            // Arrange
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, 4, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( "9," );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsGroupValid_MinMonthFilterWithEqualBirthMonth_IsTrue()
        {
            // Arrange
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, 9, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( "9," );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsTrue( isValid );
        }

        [TestMethod]
        [DataRow( 10 )]
        [DataRow( 11 )]
        [DataRow( 12 )]
        public void IsGroupValid_MinMonthFilterWithGreaterThanBirthMonth_IsTrue( int month )
        {
            // Arrange
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, month, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( "9," );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsTrue( isValid );
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
        public void IsGroupValid_MaxMonthFilterWithLessThanBirthMonth_IsTrue( int month )
        {
            // Arrange
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, month, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( ",9" );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsTrue( isValid );
        }

        [TestMethod]
        public void IsGroupValid_MaxMonthFilterWithEqualBirthMonth_IsTrue()
        {
            // Arrange
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, 9, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( ",9" );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsTrue( isValid );
        }

        [TestMethod]
        [DataRow( 10 )]
        [DataRow( 11 )]
        [DataRow( 12 )]
        public void IsGroupValid_MaxMonthFilterWithGreaterThanBirthMonth_IsFalse( int month )
        {
            // Arrange
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, month, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( ",9" );

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
        [DataRow( 8 )]
        public void IsGroupValid_MinMaxMonthFilterWithInRangeBirthMonth_IsTrue( int month )
        {
            // Arrange
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, month, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( "3,9" );

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
        public void IsGroupValid_MinMaxMonthFilterWithOutOfRangeBirthMonth_IsFalse( int month )
        {
            // Arrange
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, month, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( "3,9" );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsFalse( isValid );
        }

        [TestMethod]
        [DataRow( 1 )]
        [DataRow( 2 )]
        [DataRow( 3 )]
        [DataRow( 9 )]
        [DataRow( 10 )]
        [DataRow( 11 )]
        [DataRow( 12 )]
        public void IsGroupValid_InvertedMinMaxMonthFilterWithInRangeBirthMonth_IsTrue( int month )
        {
            // Arrange
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, month, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( "9,3" );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsTrue( isValid );
        }

        [TestMethod]
        [DataRow( 4 )]
        [DataRow( 5 )]
        [DataRow( 6 )]
        [DataRow( 7 )]
        [DataRow( 8 )]
        public void IsGroupValid_InvertedMinMaxMonthFilterWithOutOfRangeBirthMonth_IsFalse( int month )
        {
            // Arrange
            var filter = CreateBirthMonthFilter( RockDateTime.New( 2019, month, 12 ) );
            var groupOpportunity = CreateGroupOpportunity( "9,3" );

            // Act
            var isValid = filter.IsGroupValid( groupOpportunity );

            // Assert
            Assert.IsFalse( isValid );
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
        /// <param name="birthMonthRange">The birth month range.</param>
        /// <returns>A new instance of <see cref="GroupOpportunity"/>.</returns>
        private GroupOpportunity CreateGroupOpportunity( string birthMonthRange = null )
        {
            var attributeValues = new Dictionary<string, AttributeValueCache>();

            if ( birthMonthRange != null )
            {
                attributeValues.Add( "BirthMonthRange", new AttributeValueCache( 0, null, birthMonthRange ) );
            }

            var groupCacheMock = new Mock<GroupCache>( MockBehavior.Strict );

            groupCacheMock.Setup( m => m.AttributeValues ).Returns( attributeValues );

            return new GroupOpportunity
            {
                CheckInData = new GroupConfigurationData( groupCacheMock.Object, null )
            };
        }

        #endregion
    }
}
