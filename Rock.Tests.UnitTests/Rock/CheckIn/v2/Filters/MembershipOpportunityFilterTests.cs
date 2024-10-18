using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
using Rock.Data;
using Rock.Enums.CheckIn;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
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
            var groupId = 10;
            var personId = 20;

            var rockContextMock = GetRockContextMock();
            var filter = CreateMembershipFilter( personId, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupId, AttendanceRule.None );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void MembershipFilter_WithAddOnCheckInRule_IncludesAnyGroup()
        {
            var groupId = 10;
            var personId = 20;

            var rockContextMock = GetRockContextMock();
            var filter = CreateMembershipFilter( personId, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupId, AttendanceRule.AddOnCheckIn );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void MembershipFilter_WithAlreadyBelongsRuleAndActiveMember_IncludesGroup()
        {
            var groupId = 10;
            var personId = 20;
            var groupMemberStatus = GroupMemberStatus.Active;

            var groupMemberMock = CreateGroupMemberMock( groupId, personId, groupMemberStatus, true );
            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet( groupMemberMock.Object );

            var filter = CreateMembershipFilter( personId, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupId, AttendanceRule.AlreadyEnrolledInGroup );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( isIncluded );
        }

        [TestMethod]
        public void MembershipFilter_WithAlreadyBelongsRuleAndNotRoleCheckInAllowed_ExcludesGroup()
        {
            var groupId = 10;
            var personId = 20;
            var groupMemberStatus = GroupMemberStatus.Active;

            var groupMemberMock = CreateGroupMemberMock( groupId, personId, groupMemberStatus, false );
            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet( groupMemberMock.Object );

            var filter = CreateMembershipFilter( personId, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupId, AttendanceRule.AlreadyEnrolledInGroup );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void MembershipFilter_WithAlreadyBelongsRuleAndInactiveMember_ExcludesGroup()
        {
            var groupId = 10;
            var personId = 20;
            var groupMemberStatus = GroupMemberStatus.Inactive;

            var groupMemberMock = CreateGroupMemberMock( groupId, personId, groupMemberStatus, true );
            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet( groupMemberMock.Object );

            var filter = CreateMembershipFilter( personId, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupId, AttendanceRule.AlreadyEnrolledInGroup );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void MembershipFilter_WithAlreadyBelongsRuleAndPendingMember_ExcludesGroup()
        {
            var groupId = 10;
            var personId = 20;
            var groupMemberStatus = GroupMemberStatus.Pending;

            var groupMemberMock = CreateGroupMemberMock( groupId, personId, groupMemberStatus, true );
            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet( groupMemberMock.Object );

            var filter = CreateMembershipFilter( personId, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupId, AttendanceRule.AlreadyEnrolledInGroup );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void MembershipFilter_WithAlreadyBelongsRuleAndMissingMember_ExcludesGroup()
        {
            var groupId = 10;
            var personId = 20;

            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet<GroupMember>();

            var filter = CreateMembershipFilter( personId, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupId, AttendanceRule.AlreadyEnrolledInGroup );

            var isIncluded = filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( isIncluded );
        }

        [TestMethod]
        public void MembershipFilter_WithPreferEnrolledGroupsAndActiveMember_MakesGroupPreferred()
        {
            var groupId = 10;
            var personId = 20;
            var groupMemberStatus = GroupMemberStatus.Active;

            var groupMemberMock = CreateGroupMemberMock( groupId, personId, groupMemberStatus, true );
            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet( groupMemberMock.Object );

            var filter = CreateMembershipFilter( personId, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupId, AttendanceRule.AlreadyEnrolledInGroup, AlreadyEnrolledMatchingLogic.PreferEnrolledGroups );

            filter.IsGroupValid( groupOpportunity );

            Assert.IsTrue( groupOpportunity.IsPreferredGroup );
        }

        [TestMethod]
        public void MembershipFilter_WithPreferEnrolledGroupsAndMissingMember_DoesNotMakeGroupPreferred()
        {
            var groupId = 10;
            var personId = 20;

            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet<GroupMember>();

            var filter = CreateMembershipFilter( personId, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupId, AttendanceRule.AlreadyEnrolledInGroup, AlreadyEnrolledMatchingLogic.MustBeEnrolled );

            filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( groupOpportunity.IsPreferredGroup );
        }

        [TestMethod]
        public void MembershipFilter_WithMustBeEnrolledAndActiveMember_DoesNotMakeGroupPreferred()
        {
            var groupId = 10;
            var personId = 20;
            var groupMemberStatus = GroupMemberStatus.Active;

            var groupMemberMock = CreateGroupMemberMock( groupId, personId, groupMemberStatus, true );
            var rockContextMock = GetRockContextMock();

            rockContextMock.SetupDbSet( groupMemberMock.Object );

            var filter = CreateMembershipFilter( personId, rockContextMock.Object );
            var groupOpportunity = CreateGroupOpportunity( groupId, AttendanceRule.AlreadyEnrolledInGroup, AlreadyEnrolledMatchingLogic.MustBeEnrolled );

            filter.IsGroupValid( groupOpportunity );

            Assert.IsFalse( groupOpportunity.IsPreferredGroup );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Creates the <see cref="MembershipOpportunityFilter"/> along with a
        /// person to be filtered.
        /// </summary>
        /// <param name="personId">The identifier of the person to be used for membership check.</param>
        /// <param name="rockContext">The context to use when accessing database objects.</param>
        /// <returns>An instance of <see cref="MembershipOpportunityFilter"/>.</returns>
        private MembershipOpportunityFilter CreateMembershipFilter( int personId, RockContext rockContext )
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

            filter.Person.Person.Id = IdHasher.Instance.GetHash( personId );

            return filter;
        }

        /// <summary>
        /// Creates a group opportunity with the specified data view unique
        /// identifier values.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="areaAttendanceRule">The attendance rule for the area that owns this group.</param>
        /// <returns>A new instance of <see cref="GroupOpportunity"/>.</returns>
        private GroupOpportunity CreateGroupOpportunity( int groupId, AttendanceRule areaAttendanceRule, AlreadyEnrolledMatchingLogic matchingLogic = AlreadyEnrolledMatchingLogic.MustBeEnrolled )
        {
            var areaConfigurationMock = new Mock<AreaConfigurationData>( MockBehavior.Strict );

            areaConfigurationMock.Setup( m => m.AttendanceRule ).Returns( areaAttendanceRule );
            areaConfigurationMock.Setup( m => m.AlreadyEnrolledMatchingLogic ).Returns( matchingLogic );

            return new GroupOpportunity
            {
                Id = IdHasher.Instance.GetHash( groupId ),
                CheckInAreaData = areaConfigurationMock.Object
            };
        }

        /// <summary>
        /// Creates a <see cref="GroupMember"/> object that provides the
        /// minimal amount of required information for the filter to work.
        /// </summary>
        /// <param name="groupId">The identifier of the group.</param>
        /// <param name="personId">The identifier of the person.</param>
        /// <param name="memberStatus">The status of the group member record.</param>
        /// <param name="isCheckInAllowed">The state of the <see cref="GroupTypeRole.IsCheckInAllowed" /> property.</param>
        /// <returns>A mocking instance for <see cref="GroupMember"/>.</returns>
        private Mock<GroupMember> CreateGroupMemberMock( int groupId, int personId, GroupMemberStatus memberStatus, bool isCheckInAllowed )
        {
            var groupMock = new Mock<Group>( MockBehavior.Strict );

            groupMock.Object.Id = groupId;

            var personMock = new Mock<Person>( MockBehavior.Strict );

            personMock.Object.Id = personId;

            var groupRoleMock = new Mock<GroupTypeRole>( MockBehavior.Strict );

            groupRoleMock.Object.IsCheckInAllowed = isCheckInAllowed;

            var groupMemberMock = new Mock<GroupMember>( MockBehavior.Strict );

            groupMemberMock.Setup( m => m.Person ).Returns( personMock.Object );
            groupMemberMock.Setup( m => m.Group ).Returns( groupMock.Object );
            groupMemberMock.Setup( m => m.GroupRole ).Returns( groupRoleMock.Object );
            groupMemberMock.Object.GroupMemberStatus = memberStatus;

            return groupMemberMock;
        }

        #endregion
    }
}
