using System;
using System.Collections.Generic;

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
    /// a person's grade, age and birthdate for the check-in process. This filter
    /// is only used with certain <see cref="TemplateConfigurationData.GradeAndAgeMatchingBehavior"/>
    /// values.
    /// </summary>
    /// <seealso cref="GradeAndAgeOpportunityFilter"/>
    [TestClass]
    public class GradeAndAgeOpportunityFilterTests
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void IsGroupValid_WithGradeAndAgeRequiredBehavior_SkipsFilter()
        {
            // This test would normally fail, so we use this along with the
            // behavior to verify that it doesn't actually filter anything.
            var minAge = 3;

            var filter = CreateGradeAndAgeFilter( null, null, GradeAndAgeMatchingMode.GradeAndAgeMustMatch );
            var groupOpportunity = CreateGroupOpportunity( minAge: minAge );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithNoGradeAndMismatchedAge_ExcludesGroup()
        {
            // This test would normally fail, so we use this along with the
            // behavior to verify that it doesn't actually filter anything.
            var minAge = 3;

            var filter = CreateGradeAndAgeFilter( null, RockDateTime.Now.AddDays( -365 ) );
            var groupOpportunity = CreateGroupOpportunity( minAge: minAge );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void IsGroupValid_WithNoGradeAndMissingAge_ExcludesGroup()
        {
            // This test would normally fail, so we use this along with the
            // behavior to verify that it doesn't actually filter anything.
            var minAge = 3;

            var filter = CreateGradeAndAgeFilter( null, null );
            var groupOpportunity = CreateGroupOpportunity( minAge: minAge );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        #endregion

        #region FilterGroups

        [TestMethod]
        public void FilterGroups_WithAgeMatchNotRequired_IncludesNonGradeMatches()
        {
            var minGrade = 3;
            var minAge = 3;
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old

            var filter = CreateGradeAndAgeFilter( minGrade, birthdate, GradeAndAgeMatchingMode.AgeMatchNotRequired );
            var groupOpportunityByGrade = CreateGroupOpportunity( minGradeOffset: minGrade );
            var groupOpportunityByAge = CreateGroupOpportunity( minAge: minAge );
            var opportunities = new OpportunityCollection
            {
                Groups = new List<GroupOpportunity>
                {
                    groupOpportunityByGrade,
                    groupOpportunityByAge
                }
            };

            filter.FilterGroups( opportunities );

            Assert.AreEqual( 2, opportunities.Groups.Count );
        }

        [TestMethod]
        public void FilterGroups_WithPrioritizeGradeOverAge_ExcludesNonGradeMatches()
        {
            var minGrade = 3;
            var minAge = 3;
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old

            var filter = CreateGradeAndAgeFilter( minGrade, birthdate, GradeAndAgeMatchingMode.PrioritizeGradeOverAge );
            var groupOpportunityByGrade = CreateGroupOpportunity( minGradeOffset: minGrade );
            var groupOpportunityByAge = CreateGroupOpportunity( minAge: minAge );
            var opportunities = new OpportunityCollection
            {
                Groups = new List<GroupOpportunity>
                {
                    groupOpportunityByGrade,
                    groupOpportunityByAge
                }
            };

            filter.FilterGroups( opportunities );

            Assert.AreEqual( 1, opportunities.Groups.Count );
            Assert.AreSame( groupOpportunityByGrade, opportunities.Groups[0] );
        }

        [TestMethod]
        public void FilterGroups_WithPrioritizeGradeOverAge_IncludesGroupsWithDataView()
        {
            var minGrade = 3;
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old

            var filter = CreateGradeAndAgeFilter( minGrade, birthdate, GradeAndAgeMatchingMode.PrioritizeGradeOverAge );
            var groupOpportunityByGrade = CreateGroupOpportunity( minGradeOffset: minGrade );
            var groupOpportunityByDataView = CreateGroupOpportunity( dataViewGuids: new List<Guid> { Guid.NewGuid() } );
            var opportunities = new OpportunityCollection
            {
                Groups = new List<GroupOpportunity>
                {
                    groupOpportunityByGrade,
                    groupOpportunityByDataView
                }
            };

            filter.FilterGroups( opportunities );

            Assert.AreEqual( 2, opportunities.Groups.Count );
            Assert.AreSame( groupOpportunityByGrade, opportunities.Groups[0] );
            Assert.AreSame( groupOpportunityByDataView, opportunities.Groups[1] );
        }

        [TestMethod]
        public void FilterGroups_WithPrioritizeGradeOverAge_IncludesGroupsWithAlreadyEnrolled()
        {
            var minGrade = 3;
            var birthdate = RockDateTime.Now.AddDays( -1277 ); // 3.5 years old

            var filter = CreateGradeAndAgeFilter( minGrade, birthdate, GradeAndAgeMatchingMode.PrioritizeGradeOverAge );
            var groupOpportunityByGrade = CreateGroupOpportunity( minGradeOffset: minGrade );
            var groupOpportunityByDataView = CreateGroupOpportunity( attendanceRule: AttendanceRule.AlreadyEnrolledInGroup );
            var opportunities = new OpportunityCollection
            {
                Groups = new List<GroupOpportunity>
                {
                    groupOpportunityByGrade,
                    groupOpportunityByDataView
                }
            };

            filter.FilterGroups( opportunities );

            Assert.AreEqual( 2, opportunities.Groups.Count );
            Assert.AreSame( groupOpportunityByGrade, opportunities.Groups[0] );
            Assert.AreSame( groupOpportunityByDataView, opportunities.Groups[1] );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="GradeAndAgeOpportunityFilter"/> along with a
        /// person to be filtered.
        /// </summary>
        /// <param name="grade">The grade to give the person if not <c>null</c>.</param>
        /// <param name="personBirthdate">The birthdate to give the person if not <c>null</c>.</param>
        /// <param name="gradeAndAgeMatchingBehavior">The configuration value to use for the template.</param>
        /// <returns>An instance of <see cref="GradeAndAgeOpportunityFilter"/>.</returns>
        private GradeAndAgeOpportunityFilter CreateGradeAndAgeFilter( int? grade, DateTime? personBirthdate, GradeAndAgeMatchingMode gradeAndAgeMatchingBehavior = GradeAndAgeMatchingMode.AgeMatchNotRequired )
        {
            // Create the template configuration.
            var templateConfigurationMock = new Mock<TemplateConfigurationData>( MockBehavior.Strict );

            templateConfigurationMock.Setup( m => m.IsAgeRequired ).Returns( true );
            templateConfigurationMock.Setup( m => m.IsGradeRequired ).Returns( true );
            templateConfigurationMock.Setup( m => m.GradeAndAgeMatchingBehavior ).Returns( gradeAndAgeMatchingBehavior );

            var filter = new GradeAndAgeOpportunityFilter
            {
                Person = new Attendee
                {
                    Person = new PersonBag()
                },
                TemplateConfiguration = templateConfigurationMock.Object
            };

            if ( personBirthdate.HasValue )
            {
                filter.Person.Person.GradeOffset = grade;
                filter.Person.Person.BirthDate = personBirthdate;
                filter.Person.Person.BirthYear = filter.Person.Person.BirthDate.Value.Year;
                filter.Person.Person.BirthMonth = filter.Person.Person.BirthDate.Value.Month;
                filter.Person.Person.BirthDay = filter.Person.Person.BirthDate.Value.Day;

                var tempPerson = new Person
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
        /// <param name="minGradeOffset">The minimum grade offset or <c>null</c>.</param>
        /// <param name="maxGradeOffset">The maximum grade offset or <c>null</c>.</param>
        /// <param name="minAge">The minimum age value or <c>null</c>.</param>
        /// <param name="maxAge">The maximum age value or <c>null</c>.</param>
        /// <param name="minBirthdate">The minimum birthdate value or <c>null</c>.</param>
        /// <param name="maxBirthdate">The maximum birthdate value or <c>null</c>.</param>
        /// <param name="dataViewGuids">The list of data view unique identifiers configured for this opportunity.</param>
        /// <param name="attendanceRule">The attendance rule for the area of this opportunity.</param>
        /// <returns>A new instance of <see cref="GroupOpportunity"/>.</returns>
        private GroupOpportunity CreateGroupOpportunity( int? minGradeOffset = null, int? maxGradeOffset = null, decimal? minAge = null, decimal? maxAge = null, DateTime? minBirthdate = null, DateTime? maxBirthdate = null, List<Guid> dataViewGuids = null, AttendanceRule? attendanceRule = null )
        {
            var groupConfigurationMock = new Mock<GroupConfigurationData>( MockBehavior.Strict );

            groupConfigurationMock.Setup( m => m.MinimumGradeOffset ).Returns( minGradeOffset );
            groupConfigurationMock.Setup( m => m.MaximumGradeOffset ).Returns( maxGradeOffset );
            groupConfigurationMock.Setup( m => m.MinimumAge ).Returns( minAge );
            groupConfigurationMock.Setup( m => m.MaximumAge ).Returns( maxAge );
            groupConfigurationMock.Setup( m => m.MinimumBirthdate ).Returns( minBirthdate );
            groupConfigurationMock.Setup( m => m.MaximumBirthdate ).Returns( maxBirthdate );
            groupConfigurationMock.Setup( m => m.DataViewGuids ).Returns( dataViewGuids ?? new List<Guid>() );

            var areaConfigurationMock = new Mock<AreaConfigurationData>( MockBehavior.Strict );

            areaConfigurationMock.Setup( m => m.AttendanceRule ).Returns( attendanceRule ?? AttendanceRule.None );

            return new GroupOpportunity
            {
                Id = Guid.NewGuid().ToString(),
                CheckInData = groupConfigurationMock.Object,
                CheckInAreaData = areaConfigurationMock.Object,
            };
        }

        #endregion
    }
}
