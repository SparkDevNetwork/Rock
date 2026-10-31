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
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;

namespace Rock.Tests.Model
{
    /// <summary>
    /// Tests the keyset paging used by the Person bulk index (issue #7080).
    /// </summary>
    [TestClass]
    [TestCategory( "Core.Crm.Person" )]
    public class PersonIndexPagingTests
    {
        [TestMethod]
        public void GetPersonIdPages_WithGapsInIds_ReturnsEveryIdOnceInAscendingPages()
        {
            var ids = new List<int> { 3, 1, 2, 10, 11, 50, 51, 52, 1000, 7 };

            var pages = GetPages( ids, 3 );

            AssertPagesCoverIds( ids, pages, 3 );
            Assert.HasCount( 4, pages );
        }

        [TestMethod]
        public void GetPersonIdPages_WithCountEqualToPageSizeMultiple_ReturnsFullPagesOnly()
        {
            var ids = Enumerable.Range( 1, 6 ).ToList();

            var pages = GetPages( ids, 3 );

            AssertPagesCoverIds( ids, pages, 3 );
            Assert.HasCount( 2, pages );
        }

        [TestMethod]
        public void GetPersonIdPages_WithNoPeople_ReturnsNoPages()
        {
            var pages = GetPages( new List<int>(), 1000 );

            Assert.IsEmpty( pages );
        }

        [TestMethod]
        public void GetPersonIdPages_WithPageSizeOne_ReturnsOnePagePerPerson()
        {
            var ids = new List<int> { 5, 9, 2 };

            var pages = GetPages( ids, 1 );

            AssertPagesCoverIds( ids, pages, 1 );
            Assert.HasCount( 3, pages );
        }

        [TestMethod]
        public void GetPersonIdPages_WithSinglePerson_ReturnsOnePage()
        {
            var ids = new List<int> { 42 };

            var pages = GetPages( ids, 1000 );

            AssertPagesCoverIds( ids, pages, 1000 );
            Assert.HasCount( 1, pages );
        }

        [TestMethod]
        public void GetPersonIdPages_RespectsFilterOnBaseQuery()
        {
            var people = Enumerable.Range( 1, 20 )
                .Select( id => new Person { Id = id, RecordTypeValueId = id % 2 } )
                .ToList();
            var expectedIds = people.Where( p => p.RecordTypeValueId == 1 ).Select( p => p.Id ).ToList();

            var baseQuery = people.AsQueryable().Where( p => p.RecordTypeValueId == 1 ).OrderBy( p => p.Id );
            var pages = Person.GetPersonIdPages( baseQuery, 4 ).ToList();

            AssertPagesCoverIds( expectedIds, pages, 4 );
        }

        [TestMethod]
        public void GetPersonIdPages_RangeOfEachPageSelectsOnlyThatPage()
        {
            // The indexer loads each page with a first to last Id range on the
            // same base query, so the range must contain exactly the page's Ids.
            var ids = new List<int> { 1, 4, 6, 8, 9, 15, 16, 30 };
            var people = ids.Select( id => new Person { Id = id } ).ToList();
            var baseQuery = people.AsQueryable().OrderBy( p => p.Id );

            foreach ( var pageIds in Person.GetPersonIdPages( baseQuery, 3 ) )
            {
                var firstPageId = pageIds[0];
                var lastPageId = pageIds[pageIds.Count - 1];
                var rangeIds = baseQuery.Where( p => p.Id >= firstPageId && p.Id <= lastPageId ).Select( p => p.Id ).ToList();

                CollectionAssert.AreEqual( pageIds, rangeIds );
            }
        }

        #region Support Methods

        /// <summary>
        /// Gets all the pages for an in memory set of people with the given Ids.
        /// </summary>
        private static List<List<int>> GetPages( List<int> ids, int pageSize )
        {
            var people = ids.Select( id => new Person { Id = id } ).ToList();

            return Person.GetPersonIdPages( people.AsQueryable().OrderBy( p => p.Id ), pageSize ).ToList();
        }

        /// <summary>
        /// Asserts that every Id appears in exactly one page, pages ascend without
        /// overlap and no page is larger than the page size.
        /// </summary>
        private static void AssertPagesCoverIds( List<int> expectedIds, List<List<int>> pages, int pageSize )
        {
            var allPagedIds = pages.SelectMany( p => p ).ToList();

            CollectionAssert.AreEqual( expectedIds.OrderBy( id => id ).ToList(), allPagedIds );

            for ( var i = 0; i < pages.Count; i++ )
            {
                Assert.IsTrue( pages[i].Count > 0 && pages[i].Count <= pageSize );

                if ( i > 0 )
                {
                    Assert.IsGreaterThan( pages[i - 1].Last(), pages[i].First() );
                }
            }
        }

        #endregion
    }
}
