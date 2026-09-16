using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Model
{
    [TestClass]
    public class IdKeyTests
    {
        /// <summary>
        /// The identifier of the seeded test group used by every test. This
        /// matches the "RSR - Rock Administration" group from the sample data
        /// that the original integration tests relied on.
        /// </summary>
        private const int TestGroupId = 2;

        /// <summary>
        /// The unique identifier of the seeded test group used by every test.
        /// </summary>
        private const string TestGroupGuid = "628C51A8-4613-43ED-A18D-4A6FB999273E";

        /// <summary>
        /// Gets a mocked <see cref="RockContext"/> that has been seeded with a
        /// single <see cref="Group"/> whose Id and Guid match the values the
        /// tests look up. This replaces the sample-data group the integration
        /// tests depended on.
        /// </summary>
        /// <param name="app">The scoped RockApp providing the mocked context.</param>
        /// <returns>A mocked <see cref="RockContext"/> containing the test group.</returns>
        private static RockContext GetSeededRockContext( TestHelper.RockAppScope app )
        {
            var rockContext = app.App.CreateRockContext();

            rockContext.Set<Group>().Add( new Group
            {
                Id = TestGroupId,
                Guid = new Guid( TestGroupGuid ),
                Name = "RSR - Rock Administration"
            } );

            return rockContext;
        }

        #region Service<T>.Get( string, bool )

        [TestMethod]
        public void Get_WithIntegerId_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.Get( "2" ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void Get_NoIntegerAllowed_WithIntegerId_ReturnsNull()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.Get( "2", false ); // RSR - Rock Administration

            Assert.IsNull( result );
        }

        [TestMethod]
        public void Get_WithGuid_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.Get( "628C51A8-4613-43ED-A18D-4A6FB999273E" ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void Get_NoIntegerAllowed_WithGuid_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.Get( "628C51A8-4613-43ED-A18D-4A6FB999273E", false ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void Get_WithHashKey_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );
            var hashKey = Rock.Utility.IdHasher.Instance.GetHash( 2 );

            var result = groupService.Get( hashKey ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void Get_NoIntegerAllowed_WithHashKey_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );
            var hashKey = Rock.Utility.IdHasher.Instance.GetHash( 2 );

            var result = groupService.Get( hashKey, false ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        #endregion

        #region Service<T>.GetInclude( string, Expression, bool )

        [TestMethod]
        public void GetInclude_WithIntegerId_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.GetInclude( "2", g => g.GroupType ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void GetInclude_NoIntegerAllowed_WithIntegerId_ReturnsNull()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.GetInclude( "2", g => g.GroupType, false ); // RSR - Rock Administration

            Assert.IsNull( result );
        }

        [TestMethod]
        public void GetInclude_WithGuid_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.GetInclude( "628C51A8-4613-43ED-A18D-4A6FB999273E", g => g.GroupType ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void GetInclude_NoIntegerAllowed_WithGuid_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.GetInclude( "628C51A8-4613-43ED-A18D-4A6FB999273E", g => g.GroupType, false ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void GetInclude_WithHashKey_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );
            var hashKey = Rock.Utility.IdHasher.Instance.GetHash( 2 );

            var result = groupService.GetInclude( hashKey, g => g.GroupType ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void GetInclude_NoIntegerAllowed_WithHashKey_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );
            var hashKey = Rock.Utility.IdHasher.Instance.GetHash( 2 );

            var result = groupService.GetInclude( hashKey, g => g.GroupType, false ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        #endregion

        #region Service<T>.GetNoTracking( string, bool )

        [TestMethod]
        public void GetNoTracking_WithIntegerId_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.GetNoTracking( "2" ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void GetNoTracking_NoIntegerAllowed_WithIntegerId_ReturnsNull()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.GetNoTracking( "2", false ); // RSR - Rock Administration

            Assert.IsNull( result );
        }

        [TestMethod]
        public void GetNoTracking_WithGuid_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.GetNoTracking( "628C51A8-4613-43ED-A18D-4A6FB999273E" ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void GetNoTracking_NoIntegerAllowed_WithGuid_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.GetNoTracking( "628C51A8-4613-43ED-A18D-4A6FB999273E", false ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void GetNoTracking_WithHashKey_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );
            var hashKey = Rock.Utility.IdHasher.Instance.GetHash( 2 );

            var result = groupService.GetNoTracking( hashKey ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void GetNoTracking_NoIntegerAllowed_WithHashKey_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );
            var hashKey = Rock.Utility.IdHasher.Instance.GetHash( 2 );

            var result = groupService.GetNoTracking( hashKey, false ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        #endregion

        #region Service<T>.GetSelect( string, Expression, bool )

        [TestMethod]
        public void GetSelect_WithIntegerId_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.GetSelect( "2", g => new { g.Id } ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void GetSelect_NoIntegerAllowed_WithIntegerId_ReturnsNull()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.GetSelect( "2", g => new { g.Id }, false ); // RSR - Rock Administration

            Assert.IsNull( result );
        }

        [TestMethod]
        public void GetSelect_WithGuid_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.GetSelect( "628C51A8-4613-43ED-A18D-4A6FB999273E", g => new { g.Id } ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void GetSelect_NoIntegerAllowed_WithGuid_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.GetSelect( "628C51A8-4613-43ED-A18D-4A6FB999273E", g => new { g.Id }, false ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void GetSelect_WithHashKey_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );
            var hashKey = Rock.Utility.IdHasher.Instance.GetHash( 2 );

            var result = groupService.GetSelect( hashKey, g => new { g.Id } ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void GetSelect_NoIntegerAllowed_WithHashKey_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );
            var hashKey = Rock.Utility.IdHasher.Instance.GetHash( 2 );

            var result = groupService.GetSelect( hashKey, g => new { g.Id }, false ); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        #endregion

        #region Service<T>.GetQueryableByKey( string, bool )

        [TestMethod]
        public void GetQueryableByKey_WithIntegerId_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.GetQueryableByKey( "2" ).SingleOrDefault(); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void GetQueryableByKey_NoIntegerAllowed_WithIntegerId_ReturnsNull()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.GetQueryableByKey( "2", false ).SingleOrDefault(); // RSR - Rock Administration

            Assert.IsNull( result );
        }

        [TestMethod]
        public void GetQueryableByKey_WithGuid_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.GetQueryableByKey( "628C51A8-4613-43ED-A18D-4A6FB999273E" ).SingleOrDefault(); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void GetQueryableByKey_NoIntegerAllowed_WithGuid_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );

            var result = groupService.GetQueryableByKey( "628C51A8-4613-43ED-A18D-4A6FB999273E", false ).SingleOrDefault(); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void GetQueryableByKey_WithHashKey_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );
            var hashKey = Rock.Utility.IdHasher.Instance.GetHash( 2 );

            var result = groupService.GetQueryableByKey( hashKey ).SingleOrDefault(); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        [TestMethod]
        public void GetQueryableByKey_NoIntegerAllowed_WithHashKey_LoadsEntity()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var groupService = new GroupService( GetSeededRockContext( app ) );
            var hashKey = Rock.Utility.IdHasher.Instance.GetHash( 2 );

            var result = groupService.GetQueryableByKey( hashKey, false ).SingleOrDefault(); // RSR - Rock Administration

            Assert.IsNotNull( result );
        }

        #endregion
    }
}
