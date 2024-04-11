using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.CheckIn;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// This suite checks the various combinations of filter settings related to
    /// a person's membership in a dataview for the check-in process.
    /// </summary>
    /// <seealso cref="MembershipOpportunityFilter"/>
    [TestClass]
    public class MembershipOpportunityFilterTests : CheckInMockDatabase
    {
        #region IsGroupValid Tests

        [TestMethod]
        public void MembershipFilter_WithNoneRule_IncludesAnyGroup()
        {
            var groupGuid = new Guid( "f2bc089d-7754-4b9d-ba94-328b5279e0b4" );
            var personGuid = new Guid( "e1c1aa1f-f822-4dc5-8510-17ec124bbfd0" );

            var rockContextMock = GetRockContextMock();
            var filter = CreateMembershipFilter( personGuid, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupGuid, AttendanceRule.None );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void MembershipFilter_WithAddOnCheckInRule_IncludesAnyGroup()
        {
            var groupGuid = new Guid( "f2bc089d-7754-4b9d-ba94-328b5279e0b4" );
            var personGuid = new Guid( "e1c1aa1f-f822-4dc5-8510-17ec124bbfd0" );

            var rockContextMock = GetRockContextMock();
            var filter = CreateMembershipFilter( personGuid, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupGuid, AttendanceRule.AddOnCheckIn );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void MembershipFilter_WithAlreadyBelongsRuleAndActiveMember_IncludesGroup()
        {
            var groupGuid = new Guid( "f2bc089d-7754-4b9d-ba94-328b5279e0b4" );
            var personGuid = new Guid( "e1c1aa1f-f822-4dc5-8510-17ec124bbfd0" );
            var groupMemberStatus = GroupMemberStatus.Active;

            var groupMemberMock = CreateGroupMemberMock( groupGuid, personGuid, groupMemberStatus );
            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet( groupMemberMock.Object );

            var filter = CreateMembershipFilter( personGuid, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupGuid, AttendanceRule.AlreadyBelongs );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void MembershipFilter_WithAlreadyBelongsRuleAndInactiveMember_ExcludesGroup()
        {
            var groupGuid = new Guid( "f2bc089d-7754-4b9d-ba94-328b5279e0b4" );
            var personGuid = new Guid( "e1c1aa1f-f822-4dc5-8510-17ec124bbfd0" );
            var groupMemberStatus = GroupMemberStatus.Inactive;

            var groupMemberMock = CreateGroupMemberMock( groupGuid, personGuid, groupMemberStatus );
            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet( groupMemberMock.Object );

            var filter = CreateMembershipFilter( personGuid, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupGuid, AttendanceRule.AlreadyBelongs );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void MembershipFilter_WithAlreadyBelongsRuleAndPendingMember_ExcludesGroup()
        {
            var groupGuid = new Guid( "f2bc089d-7754-4b9d-ba94-328b5279e0b4" );
            var personGuid = new Guid( "e1c1aa1f-f822-4dc5-8510-17ec124bbfd0" );
            var groupMemberStatus = GroupMemberStatus.Pending;

            var groupMemberMock = CreateGroupMemberMock( groupGuid, personGuid, groupMemberStatus );
            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet( groupMemberMock.Object );

            var filter = CreateMembershipFilter( personGuid, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupGuid, AttendanceRule.AlreadyBelongs );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void MembershipFilter_WithAlreadyBelongsRuleAndMissingMember_ExcludesGroup()
        {
            var groupGuid = new Guid( "f2bc089d-7754-4b9d-ba94-328b5279e0b4" );
            var personGuid = new Guid( "e1c1aa1f-f822-4dc5-8510-17ec124bbfd0" );

            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet<GroupMember>();

            var filter = CreateMembershipFilter( personGuid, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupGuid, AttendanceRule.AlreadyBelongs );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="MembershipOpportunityFilter"/> along with a
        /// person to be filtered.
        /// </summary>
        /// <param name="personGuid">The unique identifier of the person to be used for membership check.</param>
        /// <param name="rockContext">The context to use when accessing database objects.</param>
        /// <returns>An instance of <see cref="MembershipOpportunityFilter"/>.</returns>
        private MembershipOpportunityFilter CreateMembershipFilter( Guid personGuid, RockContext rockContext )
        {
            // Create the template configuration.
            var templateConfigurationMock = new Mock<TemplateConfigurationData>( MockBehavior.Strict );

            var director = new CheckInDirector( rockContext );

            // Create the filter.
            var filter = new MembershipOpportunityFilter
            {
                Person = new Attendee
                {
                    Person = new PersonBag()
                },
                Session = new CheckInSession( director, templateConfigurationMock.Object ),
                TemplateConfiguration = templateConfigurationMock.Object
            };

            filter.Person.Person.Guid = personGuid;

            return filter;
        }

        /// <summary>
        /// Creates a group opportunity with the specified data view unique
        /// identifier values.
        /// </summary>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="areaAttendanceRule">The attendance rule for the area that owns this group.</param>
        /// <returns>A new instance of <see cref="GroupOpportunity"/>.</returns>
        private GroupOpportunity CreateGroupOpportunity( Guid groupGuid, AttendanceRule areaAttendanceRule )
        {
            var areaConfigurationMock = new Mock<AreaConfigurationData>( MockBehavior.Strict );

            areaConfigurationMock.Setup( m => m.AttendanceRule ).Returns( areaAttendanceRule );

            return new GroupOpportunity
            {
                Guid = groupGuid,
                CheckInAreaData = areaConfigurationMock.Object
            };
        }

        /// <summary>
        /// Creates a <see cref="GroupMember"/> object that provides the
        /// minimal amount of required information for the filter to work.
        /// </summary>
        /// <param name="groupGuid">The unique identifier of the group.</param>
        /// <param name="personGuid">The unique identifier of the person.</param>
        /// <param name="memberStatus">The status of the group member record.</param>
        /// <returns>A mocking instance for <see cref="GroupMember"/>.</returns>
        private Mock<GroupMember> CreateGroupMemberMock( Guid groupGuid, Guid personGuid, GroupMemberStatus memberStatus )
        {
            var groupMock = new Mock<Group>( MockBehavior.Strict );

            groupMock.Object.Guid = groupGuid;

            var personMock = new Mock<Person>( MockBehavior.Strict );

            personMock.Object.Guid = personGuid;

            var groupMemberMock = new Mock<GroupMember>( MockBehavior.Strict );

            groupMemberMock.Setup( m => m.Person ).Returns( personMock.Object );
            groupMemberMock.Setup( m => m.Group ).Returns( groupMock.Object );
            groupMemberMock.Object.GroupMemberStatus = memberStatus;

            return groupMemberMock;
        }

        #endregion
    }
}
