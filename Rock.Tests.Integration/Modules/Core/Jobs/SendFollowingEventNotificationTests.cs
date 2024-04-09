using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Jobs
{
    /// <summary>
    /// Defines test class SendFollowingEventNotificationTests.
    /// </summary>
    [TestClass]
    public class SendFollowingEventNotificationTests : DatabaseTestsBase
    {
        #region Tests

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void SendFollowingEventNotification()
        {
            var rockContext = new RockContext();

            var groupService = new GroupService( rockContext );
            var groupTypeService = new GroupTypeService( rockContext );
            var personService = new PersonService( rockContext );

            var rsrStaffWorkersGroupGuid = SystemGuid.Group.GROUP_STAFF_MEMBERS.AsGuid();
            var rsrStaffWorkers = groupService.GetByGuid( rsrStaffWorkersGroupGuid );
            Assert.IsNotNull( rsrStaffWorkers );

            var smallGroupGuid = SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP.AsGuid();
            var smallGroupType = groupTypeService.Get( smallGroupGuid );
            Assert.IsNotNull( smallGroupType );

            // Get Admin Person
            var admin = personService.GetByEmail( "admin@organization.com" ).FirstOrDefault();
            Assert.IsNotNull( admin );

            var adminAlias = admin.Aliases.First();

            var personGuid = "B1550760-3793-4419-BA43-24E02AB9AF67".AsGuid();
            var person = personService.Get( personGuid );
            if ( person == null )
            {
                person = new Person
                {
                    FirstName = "Test",
                    LastName = $"Tester-{personGuid}",
                    Email = $"{personGuid}@test.com",
                    Guid = personGuid,
                    IsEmailActive = true
                };

                person.SetBirthDate( new System.DateTime( 1977, 12, 01 ) );

                personService.Add( person );
                rockContext.SaveChanges();
            }

            var personAlias = person.Aliases.First();

            if ( !groupService.GroupHasMember( rsrStaffWorkersGroupGuid, person.Id ) )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                groupMemberService.AddOrRestoreGroupMember( rsrStaffWorkers, person.Id, 1 );
                rockContext.SaveChanges();
            }

            var eventTypeService = new FollowingEventTypeService( rockContext );

            // JoinedSmallGroup
            var eventTypeGuid = SystemGuid.FollowingEventType.JOINED_SMALL_GROUP.AsGuid();
            var eventType = eventTypeService.Get( eventTypeGuid );
            Assert.IsNotNull( eventType );

            var followingEventSubscriptionService = new FollowingEventSubscriptionService( rockContext );

            // Person event subscription
            var personEventSubscriptionGuid = "50D538EA-BAAD-4E75-9843-26165E8DFE92".AsGuid();
            var personEventSubscription = followingEventSubscriptionService.Get( personEventSubscriptionGuid );

            if ( personEventSubscription == null )
            {
                personEventSubscription = new FollowingEventSubscription
                {
                    Guid = personEventSubscriptionGuid,
                    PersonAliasId = personAlias.Id,
                    EventTypeId = eventType.Id
                };

                followingEventSubscriptionService.Add( personEventSubscription );
                rockContext.SaveChanges();
            }

            // Admin event subscription
            var adminEventSubscriptionGuid = "50874360-8A6C-4FC3-8A94-C1BEDE874664".AsGuid();
            var adminEventSubscription = followingEventSubscriptionService.Get( adminEventSubscriptionGuid );

            if ( adminEventSubscription == null )
            {
                adminEventSubscription = new FollowingEventSubscription
                {
                    Guid = adminEventSubscriptionGuid,
                    PersonAliasId = adminAlias.Id,
                    EventTypeId = eventType.Id
                };

                followingEventSubscriptionService.Add( adminEventSubscription );
                rockContext.SaveChanges();
            }

            // Add following
            var followingService = new FollowingService( rockContext );
            var personAliasEntityType = EntityTypeCache.Get( typeof( Rock.Model.PersonAlias ) );

            var followingGuid = "1F70FF92-D382-4D38-8BD1-F64F7B537334".AsGuid();
            var following = followingService.Get( followingGuid );
            if ( following == null )
            {
                following = new Following
                {
                    Guid = followingGuid,
                    EntityTypeId = personAliasEntityType.Id,
                    EntityId = personAlias.Id, // folowee
                    PersonAliasId = adminAlias.Id // follower
                };

                followingService.Add( following );
                rockContext.SaveChanges();
            }

            var job = new SendFollowingEvents();
            var followingEventSystemEmailGuid = "CA7576CD-0A10-4ADA-A068-62EE598178F5".AsGuid();
            var testAttributeValues = new Dictionary<string, string>();
            testAttributeValues.AddOrReplace( "EligibleFollowers", rsrStaffWorkersGroupGuid.ToString() );
            testAttributeValues.AddOrReplace( "EmailTemplate", followingEventSystemEmailGuid.ToString());
            job.ExecuteInternal( testAttributeValues );
        }

        #endregion Tests
    }
}
