// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Crm.Following
{
    /// <summary>
    /// Tests for the Following feature.
    /// </summary>
    [TestClass]
    public class FollowingTests : DatabaseTestsBase
    {
        /// <summary>
        /// Verify that Followings added for either a Person entity or Person Alias entity are correctly retrieved.
        /// </summary>
        /// <remarks>
        /// This test verifies a fix for GitHub Issue #3012. (https://github.com/SparkDevNetwork/Rock/issues/3012)
        /// </remarks>
        [TestMethod]
        public void Following_GetFollowedPeople_ReturnsFollowedPersonAndPersonAliasEntities()
        {
            RemovePersonFollowings();

            var rockContext = new RockContext();
            var followingService = new FollowingService( rockContext );
            var personService = new PersonService( rockContext );

            var personEntityTypeId = EntityTypeCache.Get<Person>().Id;
            var personAliasEntityTypeId = EntityTypeCache.Get<PersonAlias>().Id;
            var tedDecker = personService.Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var billMarble = personService.Get( TestGuids.TestPeople.BillMarble.AsGuid() );
            var alishaMarble = personService.Get( TestGuids.TestPeople.AlishaMarble.AsGuid() );

            AddPersonFollowings();

            // Verify that the Followed Items requested for the Person entity include both the Person and PersonAlias entities.
            var followedItemsForPersonEntity = followingService.GetFollowedItems( personEntityTypeId, tedDecker.Id )
                .Cast<Person>()
                .ToList();

            Assert.IsNotNull( followedItemsForPersonEntity.FirstOrDefault( x => x.Id == billMarble.Id ) );
            Assert.IsNotNull( followedItemsForPersonEntity.FirstOrDefault( x => x.Id == alishaMarble.Id ) );

            var followedPersons = followingService.GetFollowedPersonItems( tedDecker.PrimaryAliasId.GetValueOrDefault() )
                .ToList();

            // Verify that the Followed Persons include the person followed by PersonAlias entity (Alisha Marble).
            // This is best-practice for following a person.
            Assert.IsNotNull( followedPersons.FirstOrDefault( x => x.Id == alishaMarble.Id ) );
            // Verify that the Followed Persons include the person followed by Person entity (Bill Marble).
            // This is not the recommended method of following a person, but is included to allow
            // for Followings that may have been added from non-standard sources.
            Assert.IsNotNull( followedPersons.FirstOrDefault( x => x.Id == billMarble.Id ) );

            RemovePersonFollowings();
        }

        /// <summary>
        /// Adding a following for a Person entity should add a following for the associated primary PersonAlias instead.
        /// </summary>
        [TestMethod]
        public void Following_AddFollowingForPersonEntity_IsTranslatedToPersonAliasFollowing()
        {
            RemovePersonFollowings();

            var rockContext = new RockContext();
            var followingService = new FollowingService( rockContext );
            var personService = new PersonService( rockContext );

            var personEntityTypeId = EntityTypeCache.Get<Person>().Id;
            var personAliasEntityTypeId = EntityTypeCache.Get<PersonAlias>().Id;
            var tedDecker = personService.Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var billMarble = personService.Get( TestGuids.TestPeople.BillMarble.AsGuid() );

            // Ted Decker --> Bill Marble (via Person entity)
            var newFollowing = followingService.GetOrAddFollowing( personEntityTypeId, billMarble.Id, tedDecker.PrimaryAliasId.Value, string.Empty );

            // Verify that the requested Following is correctly transposed to the PersonAlias rather than the Person entity.
            Assert.AreEqual( personAliasEntityTypeId, newFollowing.EntityTypeId );
            Assert.AreEqual( billMarble.PrimaryAliasId.Value, newFollowing.EntityId );

            RemovePersonFollowings();
        }

        /// <summary>
        /// Adding a following for a PersonAlias entity should use the standard following logic.
        /// </summary>
        [TestMethod]
        public void Following_AddFollowingForPersonAliasEntity_IsAdded()
        {
            RemovePersonFollowings();

            var rockContext = new RockContext();
            var followingService = new FollowingService( rockContext );
            var personService = new PersonService( rockContext );

            var personAliasEntityTypeId = EntityTypeCache.Get<PersonAlias>().Id;
            var tedDecker = personService.Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var alishaMarble = personService.Get( TestGuids.TestPeople.AlishaMarble.AsGuid() );

            // Ted Decker --> Alisha Marble (via PersonAlias entity)
            var newFollowing = followingService.GetOrAddFollowing( personAliasEntityTypeId, alishaMarble.PrimaryAliasId.Value, tedDecker.PrimaryAliasId.Value, string.Empty );

            // Verify that the requested Following is correctly recorded.
            Assert.AreEqual( personAliasEntityTypeId, newFollowing.EntityTypeId );
            Assert.AreEqual( alishaMarble.PrimaryAliasId.Value, newFollowing.EntityId );

            RemovePersonFollowings();
        }

        /// <summary>
        /// Adding a following for a Group entity should use the standard following logic.
        /// </summary>
        [TestMethod]
        public void Following_AddFollowingForGroup_IsAdded()
        {
            RemovePersonFollowings();

            var rockContext = new RockContext();
            var followingService = new FollowingService( rockContext );
            var personService = new PersonService( rockContext );
            var groupService = new GroupService( rockContext );

            var tedDecker = personService.Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var groupEntityTypeId = EntityTypeCache.Get<Group>().Id;
            var groupMarble = groupService.Get( TestGuids.Groups.SmallGroupMarbleGuid.AsGuid() );

            // Ted Decker --> Marble Group
            var newFollowing = followingService.GetOrAddFollowing( groupEntityTypeId, groupMarble.Id, tedDecker.PrimaryAliasId.Value, string.Empty );

            // Verify that the requested Following is correctly added.
            Assert.AreEqual( groupEntityTypeId, newFollowing.EntityTypeId );
            Assert.AreEqual( groupMarble.Id, newFollowing.EntityId );

            RemovePersonFollowings();
        }

        private void RemovePersonFollowings()
        {
            var rockContext = new RockContext();
            var followingService = new FollowingService( rockContext );

            var personEntityTypeId = EntityTypeCache.Get<Person>().Id;
            var personAliasEntityTypeId = EntityTypeCache.Get<PersonAlias>().Id;

            // Remove all Person followings for Ted Decker.
            var tedDeckerGuid = TestGuids.TestPeople.TedDecker.AsGuid();
            var followedPeopleTedDeckerQuery = followingService.Queryable()
                .Where( f => f.PersonAlias.Person.Guid == tedDeckerGuid
                && ( f.EntityTypeId == personEntityTypeId || f.EntityTypeId == personAliasEntityTypeId ) );

            followingService.DeleteRange( followedPeopleTedDeckerQuery.ToList() );

            rockContext.SaveChanges();
        }

        private void AddPersonFollowings()
        {
            var rockContext = new RockContext();
            var followingService = new FollowingService( rockContext );
            var personService = new PersonService( rockContext );

            var personEntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
            var personAliasEntityTypeId = EntityTypeCache.Get<Rock.Model.PersonAlias>().Id;
            var tedDecker = personService.Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var billMarble = personService.Get( TestGuids.TestPeople.BillMarble.AsGuid() );
            var alishaMarble = personService.Get( TestGuids.TestPeople.AlishaMarble.AsGuid() );

            // Ted Decker --> Bill Marble (via Person entity)
            Rock.Model.Following following;
            following = new Rock.Model.Following();
            following.EntityTypeId = personEntityTypeId;
            following.EntityId = billMarble.Id;
            following.PersonAliasId = tedDecker.PrimaryAliasId.Value;
            followingService.Add( following );

            // Ted Decker --> Alisha Marble (via PersonAlias entity)
            following = new Rock.Model.Following();
            following.EntityTypeId = personAliasEntityTypeId;
            following.EntityId = alishaMarble.PrimaryAliasId.Value;
            following.PersonAliasId = tedDecker.PrimaryAliasId.Value;
            followingService.Add( following );

            rockContext.SaveChanges();
        }
    }
}
