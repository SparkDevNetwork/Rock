using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Web.Cache
{
    /// <summary>
    /// This suite checks the GroupTypeCache object to make sure that
    /// all logic works as intended.
    /// </summary>
    /// <seealso cref="GroupTypeCache"/>
    [TestClass]
    public class GroupTypeCacheTests : MockDatabaseTestsBase
    {
        #region GetRootGroupTypes

        [TestMethod]
        public void GetRootGroupTypes_WithSelfRecursiveGroupType_Succeeds()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
            var groupType = new GroupType
            {
                Id = 1,
                Guid = new Guid( "02353bd9-7c7c-4158-9ca2-5506ed6be2ab" ),
            };

            // Make this group type recursive.
            groupType.ParentGroupTypes.Add( groupType );
            groupType.ChildGroupTypes.Add( groupType );

            rockContextMock.Object.Set<GroupType>().Add( groupType );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var groupTypeCache = GroupTypeCache.Get( 1, rockContextMock.Object );

                var rootGroupTypes = groupTypeCache.GetRootGroupTypes( rockContextMock.Object ).ToList();

                Assert.HasCount( 1, rootGroupTypes );
                Assert.AreEqual( groupType.Id, rootGroupTypes[0].Id );
            }
        }

        [TestMethod]
        public void GetRootGroupTypes_WithRecursiveGroupType_Succeeds()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
            var groupTypeA = new GroupType
            {
                Id = 1,
                Guid = new Guid( "02353bd9-7c7c-4158-9ca2-5506ed6be2ab" ),
            };
            var groupTypeB = new GroupType
            {
                Id = 2,
                Guid = new Guid( "4472c4d9-98af-4bb8-9202-fe41e9b44ac9" ),
            };

            // Make these group types recursive.
            groupTypeA.ParentGroupTypes.Add( groupTypeB );
            groupTypeA.ChildGroupTypes.Add( groupTypeB );

            groupTypeB.ParentGroupTypes.Add( groupTypeA );
            groupTypeB.ChildGroupTypes.Add( groupTypeA );

            rockContextMock.Object.Set<GroupType>().Add( groupTypeA );
            rockContextMock.Object.Set<GroupType>().Add( groupTypeB );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var groupTypeCache = GroupTypeCache.Get( 1, rockContextMock.Object );

                var rootGroupTypes = groupTypeCache.GetRootGroupTypes( rockContextMock.Object ).ToList();

                // It should succeed, but in this case there is no logical root to return.
                Assert.IsEmpty( rootGroupTypes );
            }
        }

        [TestMethod]
        public void GetRootGroupTypes_WithABCPattern_ReturnsA()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
            var groupTypeA = new GroupType
            {
                Id = 1,
                Guid = new Guid( "02353bd9-7c7c-4158-9ca2-5506ed6be2ab" ),
            };
            var groupTypeB = new GroupType
            {
                Id = 2,
                Guid = new Guid( "4472c4d9-98af-4bb8-9202-fe41e9b44ac9" ),
            };
            var groupTypeC = new GroupType
            {
                Id = 3,
                Guid = new Guid( "ee137ffe-3f02-48c1-a533-227a16e329a6" ),
            };

            // Configures the group types as A -> B -> C.
            groupTypeA.ChildGroupTypes.Add( groupTypeB );

            groupTypeB.ParentGroupTypes.Add( groupTypeA );
            groupTypeB.ChildGroupTypes.Add( groupTypeC );

            groupTypeC.ParentGroupTypes.Add( groupTypeB );

            rockContextMock.Object.Set<GroupType>().Add( groupTypeA );
            rockContextMock.Object.Set<GroupType>().Add( groupTypeB );
            rockContextMock.Object.Set<GroupType>().Add( groupTypeC );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var groupTypeCache = GroupTypeCache.Get( 3, rockContextMock.Object );

                var rootGroupTypes = groupTypeCache.GetRootGroupTypes( rockContextMock.Object ).ToList();

                Assert.HasCount( 1, rootGroupTypes );
                Assert.AreEqual( groupTypeA.Id, rootGroupTypes[0].Id );
            }
        }

        [TestMethod]
        public void GetRootGroupTypes_WithAABCPattern_ReturnsA()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
            var groupTypeA = new GroupType
            {
                Id = 1,
                Guid = new Guid( "02353bd9-7c7c-4158-9ca2-5506ed6be2ab" ),
            };
            var groupTypeB = new GroupType
            {
                Id = 2,
                Guid = new Guid( "4472c4d9-98af-4bb8-9202-fe41e9b44ac9" ),
            };
            var groupTypeC = new GroupType
            {
                Id = 3,
                Guid = new Guid( "ee137ffe-3f02-48c1-a533-227a16e329a6" ),
            };

            // Configures the group types as A -> A|B -> C.
            groupTypeA.ParentGroupTypes.Add( groupTypeA );
            groupTypeA.ChildGroupTypes.Add( groupTypeA );
            groupTypeA.ChildGroupTypes.Add( groupTypeB );

            groupTypeB.ParentGroupTypes.Add( groupTypeA );
            groupTypeB.ChildGroupTypes.Add( groupTypeC );

            groupTypeC.ParentGroupTypes.Add( groupTypeB );

            rockContextMock.Object.Set<GroupType>().Add( groupTypeA );
            rockContextMock.Object.Set<GroupType>().Add( groupTypeB );
            rockContextMock.Object.Set<GroupType>().Add( groupTypeC );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var groupTypeCache = GroupTypeCache.Get( 3, rockContextMock.Object );

                var rootGroupTypes = groupTypeCache.GetRootGroupTypes( rockContextMock.Object ).ToList();

                Assert.HasCount( 1, rootGroupTypes );
                Assert.AreEqual( groupTypeA.Id, rootGroupTypes[0].Id );
            }
        }

        [TestMethod]
        public void GetRootGroupTypes_WithCheckinPurpose_ReturnsAB()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
            var groupTypeA = new GroupType
            {
                Id = 1,
                Guid = new Guid( "02353bd9-7c7c-4158-9ca2-5506ed6be2ab" ),
            };
            var groupTypeB = new GroupType
            {
                Id = 2,
                Guid = new Guid( "4472c4d9-98af-4bb8-9202-fe41e9b44ac9" ),
            };
            var groupTypeC = new GroupType
            {
                Id = 3,
                Guid = new Guid( "ee137ffe-3f02-48c1-a533-227a16e329a6" ),
            };

            var definedValue = new DefinedValue
            {
                Id = 1,
                Guid = new Guid( SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ),
            };

            groupTypeB.GroupTypePurposeValueId = definedValue.Id;

            // Configures the group types as A -> B -> B|C.
            groupTypeA.ParentGroupTypes.Add( groupTypeA );
            groupTypeA.ChildGroupTypes.Add( groupTypeA );
            groupTypeA.ChildGroupTypes.Add( groupTypeB );

            groupTypeB.ParentGroupTypes.Add( groupTypeA );
            groupTypeB.ChildGroupTypes.Add( groupTypeC );

            groupTypeC.ParentGroupTypes.Add( groupTypeB );

            rockContextMock.Object.Set<GroupType>().Add( groupTypeA );
            rockContextMock.Object.Set<GroupType>().Add( groupTypeB );
            rockContextMock.Object.Set<GroupType>().Add( groupTypeC );
            rockContextMock.Object.Set<DefinedValue>().Add( definedValue );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var groupTypeCache = GroupTypeCache.Get( 3, rockContextMock.Object );

                var rootGroupTypes = groupTypeCache.GetRootGroupTypes( rockContextMock.Object ).ToList();

                Assert.HasCount( 2, rootGroupTypes );

                // It should return A since it is the logical root.
                CollectionAssert.Contains( rootGroupTypes.Select( gt => gt.Id ).ToList(), groupTypeA.Id );

                // It should return B since it is the one marked as a check-in template.
                CollectionAssert.Contains( rootGroupTypes.Select( gt => gt.Id ).ToList(), groupTypeB.Id );
            }
        }

        #endregion
    }
}
