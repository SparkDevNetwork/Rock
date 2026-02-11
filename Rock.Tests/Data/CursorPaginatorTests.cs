using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Data;
using Rock.Model;

namespace Rock.Tests.Data
{
    [TestClass]
    public class CursorPaginatorTests
    {
        [TestMethod]
        public void GetNextPage_WithNextCursor_ReturnsAdditionalItems()
        {
            var campuses = GetTestCampuses();

            var cursorBuilder = new CursorPaginator<Campus>( q => q
                .OrderByDescending( c => c.CreatedDateTime.HasValue )
                .ThenByDescending( c => c.CreatedDateTime )
                .ThenBy( c => c.LeaderPersonAlias.Person.LastName )
                .ThenBy( c => c.Id ) );

            var page1 = cursorBuilder.GetNextPage( campuses.AsQueryable(), null, 2, false );
            var page2 = cursorBuilder.GetNextPage( campuses.AsQueryable(), page1.NextCursor, 2, false );

            Assert.AreEqual( 2, page1.Items[0].Id );
            Assert.AreEqual( 4, page1.Items[1].Id );
            Assert.AreEqual( 1, page2.Items[0].Id );
            Assert.AreEqual( 3, page2.Items[1].Id );
        }

        [TestMethod]
        public void GetNextPage_WithSecurityAndNoAccess_ExcludesItem()
        {
            var campusMock = new Mock<Campus>();
            campusMock.Setup( m => m.IsAuthorized( It.IsAny<string>(), It.IsAny<Person>() ) ).Returns( false );

            var campuses = new List<Campus> { campusMock.Object };

            var cursorBuilder = new CursorPaginator<Campus>( null, q => q.OrderBy( c => c.Id ) )
            {
                EnforceEntitySecurity = true
            };

            var page1 = cursorBuilder.GetNextPage( campuses.AsQueryable(), null, 1, false );

            Assert.IsEmpty( page1.Items );
        }

        [TestMethod]
        public void GetNextPage_WithoutSecurityAndNoAccess_IncludesItem()
        {
            var campusMock = new Mock<Campus>();
            campusMock.Setup( m => m.IsAuthorized( It.IsAny<string>(), It.IsAny<Person>() ) ).Returns( false );

            var campuses = new List<Campus> { campusMock.Object };

            var cursorBuilder = new CursorPaginator<Campus>( q => q.OrderBy( c => c.Id ) )
            {
                EnforceEntitySecurity = false
            };

            var page1 = cursorBuilder.GetNextPage( campuses.AsQueryable(), null, 1, false );

            Assert.HasCount( 1, page1.Items );
        }

        [TestMethod]
        public void GetNextPage_WithSecurityAndAccess_IncludesItem()
        {
            var campusMock = new Mock<Campus>();
            campusMock.Setup( m => m.IsAuthorized( It.IsAny<string>(), It.IsAny<Person>() ) ).Returns( true );

            var campuses = new List<Campus> { campusMock.Object };

            var cursorBuilder = new CursorPaginator<Campus>( null, q => q.OrderBy( c => c.Id ) )
            {
                EnforceEntitySecurity = true
            };

            var page1 = cursorBuilder.GetNextPage( campuses.AsQueryable(), null, 1, false );

            Assert.HasCount( 1, page1.Items );
        }

        [TestMethod]
        public void GetNextPage_WithoutLookAheadOnLastItem_ReturnsNextCursor()
        {
            var campuses = GetTestCampuses();

            var cursorBuilder = new CursorPaginator<Campus>( q => q.OrderBy( c => c.Id ) );

            var page1 = cursorBuilder.GetNextPage( campuses.AsQueryable(), null, campuses.Count, enableLookAhead: false );

            Assert.IsNotNull( page1.NextCursor );
        }

        [TestMethod]
        public void GetNextPage_WithLookAheadOnLastItem_DoesNotReturnNextCursor()
        {
            var campuses = GetTestCampuses();

            var cursorBuilder = new CursorPaginator<Campus>( q => q.OrderBy( c => c.Id ) );

            var page1 = cursorBuilder.GetNextPage( campuses.AsQueryable(), null, campuses.Count, enableLookAhead: true );

            Assert.IsNull( page1.NextCursor );
        }

        [TestMethod]
        public void GetNextPage_WithLookAheadAndAdditionalItems_ReturnsNextCursor()
        {
            var campuses = GetTestCampuses();

            var cursorBuilder = new CursorPaginator<Campus>( q => q.OrderBy( c => c.Id ) );

            var page1 = cursorBuilder.GetNextPage( campuses.AsQueryable(), null, campuses.Count - 1, enableLookAhead: true );

            Assert.IsNotNull( page1.NextCursor );
        }

        [TestMethod]
        public void GetNextPage_WithSecurityAndLotsOfNoAccessItems_EventuallyGivesUp()
        {
            var campuses = new List<Campus>();

            for ( int i = 1; i <= 1000; i++ )
            {
                var campusMock = new Mock<Campus>();
                var allowed = i == 1 | i == 1000;
                campusMock.Setup( m => m.IsAuthorized( It.IsAny<string>(), It.IsAny<Person>() ) ).Returns( allowed );

                campuses.Add( campusMock.Object );
            }

            var cursorBuilder = new CursorPaginator<Campus>( null, q => q.OrderBy( c => c.Id ) )
            {
                EnforceEntitySecurity = true
            };

            var page1 = cursorBuilder.GetNextPage( campuses.AsQueryable(), null, 2, false );

            Assert.HasCount( 1, page1.Items );
        }

        [TestMethod]
        public void GetNextPage_WithInvalidOrderBy_ThrowsInvalidOperationException()
        {
            var campuses = GetTestCampuses();

            var cursorBuilder = new CursorPaginator<Campus>( q => q.OrderBy( c => c.Id.ToString() ) );

            Assert.Throws<InvalidOperationException>( () =>
            {
                cursorBuilder.GetNextPage( campuses.AsQueryable(), null, 1, false );
            } );
        }

        [TestMethod]
        public void GetNextPage_WithOrderByCast_Succeeds()
        {
            var campuses = GetTestCampuses();

            var cursorBuilder = new CursorPaginator<Campus>( q => q.OrderBy( c => ( int? ) c.Id ) );

            var page1 = cursorBuilder.GetNextPage( campuses.AsQueryable(), null, 1, false );

            Assert.HasCount( 1, page1.Items );
        }

        // TODO: Test with different member expressions.

        private static List<Campus> GetTestCampuses()
        {
            return new List<Campus>
            {
                new Campus
                {
                    Id = 1,
                    Guid = new Guid( "2bef3a4e-b66e-4997-9087-fae447c12843" ),
                    Name = "Campus A",
                    CreatedDateTime = new DateTime( 2024, 1, 1 ),
                    LeaderPersonAlias = new PersonAlias { Person = new Person { LastName = "Smith" } }
                },
                new Campus
                {
                    Id = 2,
                    Guid = new Guid( "e80bb54f-e415-42e1-8162-b41e899d1b29" ),
                    Name = "Campus B",
                    CreatedDateTime = new DateTime( 2025, 1, 1 ),
                    LeaderPersonAlias = new PersonAlias { Person = new Person { LastName = "Jackson" } }
                },
                new Campus
                {
                    Id = 3,
                    Guid = new Guid( "27aa332c-241c-44f1-a0d3-d083be88efdc" ),
                    Name = "Campus C",
                    CreatedDateTime = null,
                    LeaderPersonAlias = new PersonAlias { Person = new Person { LastName = "Smith" } }
                },
                new Campus
                {
                    Id = 4,
                    Guid = new Guid( "e18f6659-c1b5-4963-a76b-e29a21c926c2" ),
                    Name = "Campus D",
                    CreatedDateTime = new DateTime( 2024, 7, 4 ),
                    LeaderPersonAlias = new PersonAlias { Person = new Person { LastName = "Jackson" } }
                },
            };
        }
    }
}
