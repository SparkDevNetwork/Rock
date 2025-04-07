using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Cms;
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
            var rockContextMock = MockDatabaseHelper.GetRockContextMock();
            var groupTypeMock = MockDatabaseHelper.CreateEntityMock<GroupType>( 1, new Guid( "02353bd9-7c7c-4158-9ca2-5506ed6be2ab" ) );

            // Make this group type recursive.
            groupTypeMock.Object.ParentGroupTypes.Add( groupTypeMock.Object );
            groupTypeMock.Object.ChildGroupTypes.Add( groupTypeMock.Object );

            rockContextMock.SetupDbSet( groupTypeMock.Object );
            rockContextMock.SetupDbSet<DefinedValue>();

            var groupTypeCache = GroupTypeCache.Get( 1, rockContextMock.Object );

            var rootGroupTypes = groupTypeCache.GetRootGroupTypes( rockContextMock.Object ).ToList();

            Assert.That.Equal( 1, rootGroupTypes.Count );
            Assert.That.Equal( groupTypeMock.Object.Id, rootGroupTypes[0].Id );
        }

        [TestMethod]
        public void GetRootGroupTypes_WithRecursiveGroupType_Succeeds()
        {
            var rockContextMock = MockDatabaseHelper.GetRockContextMock();
            var groupTypeAMock = MockDatabaseHelper.CreateEntityMock<GroupType>( 1, new Guid( "02353bd9-7c7c-4158-9ca2-5506ed6be2ab" ) );
            var groupTypeBMock = MockDatabaseHelper.CreateEntityMock<GroupType>( 2, new Guid( "4472c4d9-98af-4bb8-9202-fe41e9b44ac9" ) );

            // Make these group types recursive.
            groupTypeAMock.Object.ParentGroupTypes.Add( groupTypeBMock.Object );
            groupTypeAMock.Object.ChildGroupTypes.Add( groupTypeBMock.Object );

            groupTypeBMock.Object.ParentGroupTypes.Add( groupTypeAMock.Object );
            groupTypeBMock.Object.ChildGroupTypes.Add( groupTypeAMock.Object );

            rockContextMock.SetupDbSet( groupTypeAMock.Object, groupTypeBMock.Object );
            rockContextMock.SetupDbSet<DefinedValue>();

            var groupTypeCache = GroupTypeCache.Get( 1, rockContextMock.Object );

            var rootGroupTypes = groupTypeCache.GetRootGroupTypes( rockContextMock.Object ).ToList();

            // It should succeed, but in this case there is no logical root to return.
            Assert.That.Equal( 0, rootGroupTypes.Count );
        }

        [TestMethod]
        public void GetRootGroupTypes_WithABCPattern_ReturnsA()
        {
            var rockContextMock = MockDatabaseHelper.GetRockContextMock();
            var groupTypeAMock = MockDatabaseHelper.CreateEntityMock<GroupType>( 1, new Guid( "02353bd9-7c7c-4158-9ca2-5506ed6be2ab" ) );
            var groupTypeBMock = MockDatabaseHelper.CreateEntityMock<GroupType>( 2, new Guid( "4472c4d9-98af-4bb8-9202-fe41e9b44ac9" ) );
            var groupTypeCMock = MockDatabaseHelper.CreateEntityMock<GroupType>( 3, new Guid( "ee137ffe-3f02-48c1-a533-227a16e329a6" ) );

            // Configures the group types as A -> B -> C.
            groupTypeAMock.Object.ChildGroupTypes.Add( groupTypeBMock.Object );

            groupTypeBMock.Object.ParentGroupTypes.Add( groupTypeAMock.Object );
            groupTypeBMock.Object.ChildGroupTypes.Add( groupTypeCMock.Object );

            groupTypeCMock.Object.ParentGroupTypes.Add( groupTypeBMock.Object );

            rockContextMock.SetupDbSet( groupTypeAMock.Object, groupTypeBMock.Object, groupTypeCMock.Object );
            rockContextMock.SetupDbSet<DefinedValue>();

            var groupTypeCache = GroupTypeCache.Get( 3, rockContextMock.Object );

            var rootGroupTypes = groupTypeCache.GetRootGroupTypes( rockContextMock.Object ).ToList();

            Assert.That.Equal( 1, rootGroupTypes.Count );
            Assert.That.Equal( groupTypeAMock.Object.Id, rootGroupTypes[0].Id );
        }

        [TestMethod]
        public void GetRootGroupTypes_WithAABCPattern_ReturnsA()
        {
            var rockContextMock = MockDatabaseHelper.GetRockContextMock();
            var groupTypeAMock = MockDatabaseHelper.CreateEntityMock<GroupType>( 1, new Guid( "02353bd9-7c7c-4158-9ca2-5506ed6be2ab" ) );
            var groupTypeBMock = MockDatabaseHelper.CreateEntityMock<GroupType>( 2, new Guid( "4472c4d9-98af-4bb8-9202-fe41e9b44ac9" ) );
            var groupTypeCMock = MockDatabaseHelper.CreateEntityMock<GroupType>( 3, new Guid( "ee137ffe-3f02-48c1-a533-227a16e329a6" ) );

            // Configures the group types as A -> A|B -> C.
            groupTypeAMock.Object.ParentGroupTypes.Add( groupTypeAMock.Object );
            groupTypeAMock.Object.ChildGroupTypes.Add( groupTypeAMock.Object );
            groupTypeAMock.Object.ChildGroupTypes.Add( groupTypeBMock.Object );

            groupTypeBMock.Object.ParentGroupTypes.Add( groupTypeAMock.Object );
            groupTypeBMock.Object.ChildGroupTypes.Add( groupTypeCMock.Object );

            groupTypeCMock.Object.ParentGroupTypes.Add( groupTypeBMock.Object );

            rockContextMock.SetupDbSet( groupTypeAMock.Object, groupTypeBMock.Object, groupTypeCMock.Object );
            rockContextMock.SetupDbSet<DefinedValue>();

            var groupTypeCache = GroupTypeCache.Get( 3, rockContextMock.Object );

            var rootGroupTypes = groupTypeCache.GetRootGroupTypes( rockContextMock.Object ).ToList();

            Assert.That.Equal( 1, rootGroupTypes.Count );
            Assert.That.Equal( groupTypeAMock.Object.Id, rootGroupTypes[0].Id );
        }

        [TestMethod]
        public void GetRootGroupTypes_WithCheckinPurpose_ReturnsAB()
        {
            var rockContextMock = MockDatabaseHelper.GetRockContextMock();
            var groupTypeAMock = MockDatabaseHelper.CreateEntityMock<GroupType>( 1, new Guid( "02353bd9-7c7c-4158-9ca2-5506ed6be2ab" ) );
            var groupTypeBMock = MockDatabaseHelper.CreateEntityMock<GroupType>( 2, new Guid( "4472c4d9-98af-4bb8-9202-fe41e9b44ac9" ) );
            var groupTypeCMock = MockDatabaseHelper.CreateEntityMock<GroupType>( 3, new Guid( "ee137ffe-3f02-48c1-a533-227a16e329a6" ) );
            var definedValueMock = MockDatabaseHelper.CreateEntityMock<DefinedValue>( 1, new Guid( SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ) );

            groupTypeBMock.Object.GroupTypePurposeValueId = 1;

            // Configures the group types as A -> B -> B|C.
            groupTypeAMock.Object.ParentGroupTypes.Add( groupTypeAMock.Object );
            groupTypeAMock.Object.ChildGroupTypes.Add( groupTypeAMock.Object );
            groupTypeAMock.Object.ChildGroupTypes.Add( groupTypeBMock.Object );

            groupTypeBMock.Object.ParentGroupTypes.Add( groupTypeAMock.Object );
            groupTypeBMock.Object.ChildGroupTypes.Add( groupTypeCMock.Object );

            groupTypeCMock.Object.ParentGroupTypes.Add( groupTypeBMock.Object );

            rockContextMock.SetupDbSet( groupTypeAMock.Object, groupTypeBMock.Object, groupTypeCMock.Object );
            rockContextMock.SetupDbSet<DefinedValue>( definedValueMock.Object );

            var groupTypeCache = GroupTypeCache.Get( 3, rockContextMock.Object );

            var rootGroupTypes = groupTypeCache.GetRootGroupTypes( rockContextMock.Object ).ToList();

            Assert.That.Equal( 2, rootGroupTypes.Count );

            // It should return A since it is the logical root.
            CollectionAssert.Contains( rootGroupTypes.Select( gt => gt.Id ).ToList(), groupTypeAMock.Object.Id );

            // It should return B since it is the one marked as a check-in template.
            CollectionAssert.Contains( rootGroupTypes.Select( gt => gt.Id ).ToList(), groupTypeBMock.Object.Id );
        }

        #endregion
    }
}
