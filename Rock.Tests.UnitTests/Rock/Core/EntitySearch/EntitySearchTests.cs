using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Core.EntitySearch;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.ViewModels.Core;

namespace Rock.Tests.UnitTests.Rock.Core.EntitySearch
{
    /// <summary>
    /// <para>
    /// This defines tests that cover most scenarios with Entity Search. This
    /// should include any tests that don't need an actual database / LINQ-to-SQL
    /// translation.
    /// </para>
    /// <para>
    /// For example, we can verify that the expression tree that is build for
    /// the where expression "EntityType.Id == 2" is working correctly. We can
    /// verify that it returns the desired item.
    /// </para>
    /// <para>
    /// Where things get sticky, and may require integration tests, is when
    /// we get into some of the more complex scenarios such as enforcing
    /// entity security or performing case insensitive comparisons on
    /// strings.
    /// </para>
    /// </summary>
    [TestClass]
    public class EntitySearchTests
    {
        #region Where Tests

        [TestMethod]
        public void WhereWithoutSelect_ReturnsEntity()
        {
            var queryable = GetGroupsQueryableMock();

            var systemQuery = new EntitySearchSystemQuery
            {
                WhereExpression = "Id == 1"
            };

            var results = EntitySearchHelper.GetSearchResults( queryable, systemQuery, null );
            var items = results.Items.Cast<dynamic>().ToList();

            Assert.That.Equal( typeof( Group ), ( Type ) items[0].GetType() );
        }

        [TestMethod]
        public void WhereWithId_ReturnsExactItem()
        {
            var queryable = GetGroupsQueryableMock();

            var systemQuery = new EntitySearchSystemQuery
            {
                WhereExpression = "Id == 2"
            };

            var results = EntitySearchHelper.GetSearchResults( queryable, systemQuery, null );
            var items = results.Items.Cast<dynamic>().ToList();

            Assert.That.Equal( 1, results.Items.Count );
            Assert.That.Equal( 2, ( int ) items[0].Id );
        }

        [TestMethod]
        public void WhereWithMissingId_ReturnsNoResults()
        {
            var queryable = GetGroupsQueryableMock();

            var systemQuery = new EntitySearchSystemQuery
            {
                WhereExpression = "Id == 2893723"
            };

            var results = EntitySearchHelper.GetSearchResults( queryable, systemQuery, null );

            Assert.That.Equal( 0, results.Items.Count );
        }

        #endregion

        #region Select Tests

        [TestMethod]
        public void SelectWithGroupTypeName_ReturnsOnlyName()
        {
            var queryable = GetGroupsQueryableMock();

            var systemQuery = new EntitySearchSystemQuery
            {
                WhereExpression = "Id == 1",
                SelectExpression = "GroupType.Name"
            };

            var results = EntitySearchHelper.GetSearchResults( queryable, systemQuery, null );
            var items = results.Items.Cast<dynamic>().ToList();

            Assert.That.Equal( "Group Type 1", ( string ) items[0] );
        }

        #endregion

        #region GroupBy Tests

        [TestMethod]
        public void GroupBy_ReturnsGroupedItems()
        {
            var queryable = GetGroupsQueryableMock();

            // Validate source data.
            if ( !queryable.Any( g => g.GroupTypeId == 1 ) || !queryable.Any( g => g.GroupTypeId == 2 ) )
            {
                throw new AssertInconclusiveException( "Mock groups queryable is missing expected group types." );
            }

            if ( queryable.Count( g => g.GroupTypeId == 1 || g.GroupTypeId == 2 ) == 2 )
            {
                throw new AssertInconclusiveException( "Mock groups queryable contains unexpected group counts." );
            }

            var systemQuery = new EntitySearchSystemQuery
            {
                WhereExpression = "GroupTypeId == 1 || GroupTypeId == 2",
                GroupByExpression = "GroupTypeId"
            };

            var results = EntitySearchHelper.GetSearchResults( queryable, systemQuery, null );

            Assert.That.Equal( 2, results.Items.Count );
        }

        [TestMethod]
        public void GroupByWithCountingSelect_ReturnsGroupedItemCounts()
        {
            var queryable = GetGroupsQueryableMock();

            // Validate source data.
            if ( !queryable.Any( g => g.GroupTypeId == 1 ) || !queryable.Any( g => g.GroupTypeId == 2 ) )
            {
                throw new AssertInconclusiveException( "Mock groups queryable is missing expected group types." );
            }

            if ( queryable.Count( g => g.GroupTypeId == 1 || g.GroupTypeId == 2 ) == 2 )
            {
                throw new AssertInconclusiveException( "Mock groups queryable contains unexpected group counts." );
            }

            var groupType1Count = queryable.Count( g => g.GroupTypeId == 1 );
            var groupType2Count = queryable.Count( g => g.GroupTypeId == 2 );

            var systemQuery = new EntitySearchSystemQuery
            {
                WhereExpression = "GroupTypeId == 1 || GroupTypeId == 2",
                GroupByExpression = "GroupTypeId",
                SelectExpression = "new { Key as GroupTypeId, Count() as Count }",
                OrderByExpression = "GroupTypeId"
            };

            var results = EntitySearchHelper.GetSearchResults( queryable, systemQuery, null );
            var items = results.Items.Cast<dynamic>().ToList();

            Assert.That.Equal( 2, results.Items.Count );
            Assert.That.Equal( 1, ( int ) items[0].GroupTypeId );
            Assert.That.Equal( 2, ( int ) items[1].GroupTypeId );
            Assert.That.Equal( groupType1Count, ( int ) items[0].Count );
            Assert.That.Equal( groupType2Count, ( int ) items[1].Count );
        }

        #endregion

        #region Count Tests

        [TestMethod]
        public void CountOnly_ReturnsCount()
        {
            var queryable = GetGroupsQueryableMock();

            var systemQuery = new EntitySearchSystemQuery
            {
            };

            var userQuery = new EntitySearchQueryBag
            {
                IsCountOnly = true
            };

            var results = EntitySearchHelper.GetSearchResults( queryable, systemQuery, userQuery );

            Assert.That.Equal( queryable.Count(), results.Count );
        }

        [TestMethod]
        public void CountOnly_ReturnsNullItems()
        {
            var queryable = GetGroupsQueryableMock();

            var systemQuery = new EntitySearchSystemQuery
            {
            };

            var userQuery = new EntitySearchQueryBag
            {
                IsCountOnly = true
            };

            var results = EntitySearchHelper.GetSearchResults( queryable, systemQuery, userQuery );

            Assert.That.Null( results.Items );
        }

        [TestMethod]
        public void CountOnlyWithGroupBy_ReturnsNumberOfGroupings()
        {
            var queryable = GetGroupsQueryableMock();

            // Validate source data.
            if ( !queryable.Any( g => g.GroupTypeId == 1 ) || !queryable.Any( g => g.GroupTypeId == 2 ) )
            {
                throw new AssertInconclusiveException( "Mock groups queryable is missing expected group types." );
            }

            if ( queryable.Count( g => g.GroupTypeId == 1 || g.GroupTypeId == 2 ) <= 2 )
            {
                throw new AssertInconclusiveException( "Mock groups queryable contains unexpected group counts." );
            }

            var systemQuery = new EntitySearchSystemQuery
            {
                WhereExpression = "GroupTypeId == 1 || GroupTypeId == 2",
                GroupByExpression = "GroupTypeId"
            };

            var userQuery = new EntitySearchQueryBag
            {
                IsCountOnly = true
            };

            var results = EntitySearchHelper.GetSearchResults( queryable, systemQuery, userQuery );

            // We have more than two groups, so make sure we only get back "2"
            // which would indicate the count is for the number of groupings
            // rather than the individual groups.
            Assert.That.Equal( 2, results.Count );
        }

        #endregion

        #region Other Tests

        [TestMethod]
        public void EmptySearch_ReturnsAllItems()
        {
            var queryable = GetGroupsQueryableMock();

            var systemQuery = new EntitySearchSystemQuery
            {
            };

            var results = EntitySearchHelper.GetSearchResults( queryable, systemQuery, null );

            Assert.That.Equal( queryable.Count(), results.Items.Count );
        }

        #endregion

        #region Support Methods

        private static IQueryable<Group> GetGroupsQueryableMock()
        {
            var groups = new List<Group>();

            groups.Add( new Group
            {
                Id = 1,
                GroupTypeId = 1,
                GroupType = new GroupType
                {
                    Id = 1,
                    Name = "Group Type 1"
                },
                Name = "Group 1",
                Members =
                {
                    new GroupMember
                    {
                        Id = 1,
                        Person = GetTedDeckerMock(),
                        GroupMemberStatus = GroupMemberStatus.Active
                    },
                    new GroupMember
                    {
                        Id = 2,
                        Person = GetCindyDeckerMock(),
                        GroupMemberStatus = GroupMemberStatus.Active
                    }
                }
            } );

            groups.Add( new Group
            {
                Id = 2,
                GroupTypeId = 1,
                GroupType = new GroupType
                {
                    Id = 1,
                    Name = "Group Type 1"
                },
                Name = "Group 2",
                Members =
                {
                    new GroupMember
                    {
                        Id = 3,
                        Person = GetNoahDeckerMock(),
                        GroupMemberStatus = GroupMemberStatus.Pending
                    }
                }
            } );

            groups.Add( new Group
            {
                Id = 3,
                GroupTypeId = 2,
                GroupType = new GroupType
                {
                    Id = 2,
                    Name = "Group Type 2"
                },
                Name = "Group 3",
                Members =
                {
                    new GroupMember
                    {
                        Id = 4,
                        Person = GetAlexDeckerMock(),
                        GroupMemberStatus = GroupMemberStatus.Active
                    }
                }
            } );

            return groups.AsQueryable();
        }

        private static Person GetTedDeckerMock()
        {
            return new Person
            {
                Id = 1,
                FirstName = "Theodore",
                NickName = "Ted",
                LastName = "Decker"
            };
        }

        private static Person GetCindyDeckerMock()
        {
            return new Person
            {
                Id = 2,
                FirstName = "Cynthia",
                NickName = "Cindy",
                LastName = "Decker"
            };
        }

        private static Person GetAlexDeckerMock()
        {
            return new Person
            {
                Id = 3,
                FirstName = "Alexis",
                NickName = "Alex",
                LastName = "Decker"
            };
        }

        private static Person GetNoahDeckerMock()
        {
            return new Person
            {
                Id = 4,
                FirstName = "Noah",
                NickName = "Noah",
                LastName = "Decker"
            };
        }

        #endregion
    }
}
