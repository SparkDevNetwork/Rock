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
using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Web.Cache;
using Rock.WebStartup;

using RockAttribute = Rock.Model.Attribute;

namespace Rock.Tests.Integration.Attribute
{
    /// <summary>
    /// Tests for <see cref="LinqExtensions.WithQueryableAttributeValues{T}(IQueryable{T})"/>,
    /// which rewrites comparisons on attribute values to also check the indexed
    /// <c>ValueChecksum</c> column. These run against a real database because the
    /// rewrite uses the SQL Server <c>CHECKSUM()</c> function.
    /// </summary>
    /// <remarks>
    /// Each test runs the same query with and without the rewrite and checks
    /// that both return the expected rows. The rewrite should only make the
    /// query faster, never change its results.
    /// </remarks>
    [TestClass]
    public class AttributeValueExpressionVisitorTests : DatabaseTestsBase
    {
        #region Setup

        private const string FirstValue = "alpha";

        private const string SecondValue = "beta";

        /*
            SQL Server treats some emoji as equal to an empty string, which is
            why RockCleanup checks the length instead. See RockCleanup.CleanupEmptyAttributeValues().
        */
        private const string EmojiValue = "\U0001F600";

        private int _attributeId;

        private int _firstEntityId;

        private int _secondEntityId;

        private int _emptyEntityId;

        private int _emojiEntityId;

        private static bool _areViewsInitialized;

        [TestInitialize]
        public void TestInitialize()
        {
            /*
                The queryable attribute value views (e.g. AttributeValue_Person) are
                normally created by web startup, which the test database does not run.
                This creates or recreates any view whose definition is missing or out
                of date, and drops AttributeValue_* views that no longer match an
                entity type, the same as web startup does.

                It runs here instead of in [ClassInitialize] because the base class
                starts the test database in its own [TestInitialize], so there is no
                database yet when [ClassInitialize] runs. The flag makes it run once.
            */
            if ( !_areViewsInitialized )
            {
                RockApplicationStartupHelper.InitializeQueryableAttributeValues();
                _areViewsInitialized = true;
            }

            using ( var rockContext = new RockContext() )
            {
                // The queryable attribute view is reached through the Person
                // navigation property, so the values must belong to real people.
                var personIds = new PersonService( rockContext )
                    .Queryable()
                    .OrderBy( p => p.Id )
                    .Select( p => p.Id )
                    .Take( 4 )
                    .ToList();

                if ( personIds.Count < 4 )
                {
                    Assert.Inconclusive( "The test database needs at least 4 people." );
                }

                _firstEntityId = personIds[0];
                _secondEntityId = personIds[1];
                _emptyEntityId = personIds[2];
                _emojiEntityId = personIds[3];

                var attribute = new RockAttribute
                {
                    Key = $"TestChecksumVisitor_{Guid.NewGuid():N}",
                    Name = "Test Checksum Visitor",
                    EntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.PERSON.AsGuid() ),
                    FieldTypeId = FieldTypeCache.GetId( Rock.SystemGuid.FieldType.TEXT.AsGuid() ).Value
                };

                new AttributeService( rockContext ).Add( attribute );
                rockContext.SaveChanges();

                _attributeId = attribute.Id;

                var attributeValueService = new AttributeValueService( rockContext );

                attributeValueService.Add( new AttributeValue { AttributeId = _attributeId, EntityId = _firstEntityId, Value = FirstValue } );
                attributeValueService.Add( new AttributeValue { AttributeId = _attributeId, EntityId = _secondEntityId, Value = SecondValue } );
                attributeValueService.Add( new AttributeValue { AttributeId = _attributeId, EntityId = _emptyEntityId, Value = string.Empty } );
                attributeValueService.Add( new AttributeValue { AttributeId = _attributeId, EntityId = _emojiEntityId, Value = EmojiValue } );

                rockContext.SaveChanges();
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if ( _attributeId == 0 )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( $"DELETE FROM [AttributeValue] WHERE [AttributeId] = {_attributeId}" );
                rockContext.Database.ExecuteSqlCommand( $"DELETE FROM [Attribute] WHERE [Id] = {_attributeId}" );
            }

            _attributeId = 0;
        }

        #endregion

        #region AttributeValue

        [TestMethod]
        public void AttributeValue_ValueEqualsVariable_ReturnsMatchingRows()
        {
            var target = FirstValue;

            AssertAttributeValueQuery( av => av.Value == target, _firstEntityId );
        }

        [TestMethod]
        public void AttributeValue_VariableEqualsValue_ReturnsMatchingRows()
        {
            var target = FirstValue;

            AssertAttributeValueQuery( av => target == av.Value, _firstEntityId );
        }

        [TestMethod]
        public void AttributeValue_ValueEqualsConstant_ReturnsMatchingRows()
        {
            AssertAttributeValueQuery( av => av.Value == SecondValue, _secondEntityId );
        }

        [TestMethod]
        public void AttributeValue_ValueNotEqualsVariable_ReturnsNonMatchingRows()
        {
            var target = FirstValue;

            AssertAttributeValueQuery( av => av.Value != target, _secondEntityId, _emptyEntityId, _emojiEntityId );
        }

        [TestMethod]
        public void AttributeValue_VariableNotEqualsValue_ReturnsNonMatchingRows()
        {
            var target = FirstValue;

            AssertAttributeValueQuery( av => target != av.Value, _secondEntityId, _emptyEntityId, _emojiEntityId );
        }

        /// <summary>
        /// Uses the same filter as RockCleanup.CleanupEmptyAttributeValues(),
        /// the only core caller of WithQueryableAttributeValues().
        /// </summary>
        [TestMethod]
        public void AttributeValue_RockCleanupEmptyValueFilter_ReturnsOnlyEmptyRows()
        {
            AssertAttributeValueQuery( av => av.Value == null || av.Value.Length == 0, _emptyEntityId );
        }

        #endregion

        #region QueryableAttributeValue

        [TestMethod]
        public void QueryableAttributeValue_ValueEqualsVariable_ReturnsMatchingRows()
        {
            var target = FirstValue;

            AssertQueryableAttributeValueQuery( qav => qav.Value == target, _firstEntityId );
        }

        [TestMethod]
        public void QueryableAttributeValue_VariableEqualsValue_ReturnsMatchingRows()
        {
            var target = FirstValue;

            AssertQueryableAttributeValueQuery( qav => target == qav.Value, _firstEntityId );
        }

        [TestMethod]
        public void QueryableAttributeValue_ValueNotEqualsVariable_ReturnsNonMatchingRows()
        {
            var target = FirstValue;

            AssertQueryableAttributeValueQuery( qav => qav.Value != target, _secondEntityId, _emptyEntityId, _emojiEntityId );
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Runs a filter against the AttributeValue table for the test attribute,
        /// with and without the checksum rewrite, and checks both return the
        /// expected entities.
        /// </summary>
        /// <param name="predicate">The filter to apply to the attribute values.</param>
        /// <param name="expectedEntityIds">The entities whose values should match.</param>
        private void AssertAttributeValueQuery( System.Linq.Expressions.Expression<Func<AttributeValue, bool>> predicate, params int[] expectedEntityIds )
        {
            using ( var rockContext = new RockContext() )
            {
                var query = new AttributeValueService( rockContext )
                    .Queryable()
                    .Where( av => av.AttributeId == _attributeId )
                    .Where( predicate );

                AssertSameEntityIds( query.Select( av => av.EntityId.Value ), expectedEntityIds );
            }
        }

        /// <summary>
        /// Runs a filter against the Person queryable attribute value view for
        /// the test attribute, with and without the checksum rewrite, and
        /// checks both return the expected entities.
        /// </summary>
        /// <param name="predicate">The filter to apply to the attribute values.</param>
        /// <param name="expectedEntityIds">The entities whose values should match.</param>
        private void AssertQueryableAttributeValueQuery( System.Linq.Expressions.Expression<Func<Person.PersonQueryableAttributeValue, bool>> predicate, params int[] expectedEntityIds )
        {
            // The view has a row for every person, using the default value
            // when the person has no value, so limit it to the seeded people.
            var seededEntityIds = new List<int> { _firstEntityId, _secondEntityId, _emptyEntityId, _emojiEntityId };

            using ( var rockContext = new RockContext() )
            {
                var query = new PersonService( rockContext )
                    .Queryable()
                    .SelectMany( p => p.PersonAttributeValues )
                    .Where( qav => qav.AttributeId == _attributeId && seededEntityIds.Contains( qav.EntityId ) )
                    .Where( predicate );

                AssertSameEntityIds( query.Select( qav => qav.EntityId ), expectedEntityIds );
            }
        }

        /// <summary>
        /// Checks that the query returns the expected entities both as written
        /// and after it has been rewritten by WithQueryableAttributeValues().
        /// </summary>
        /// <param name="entityIdQuery">The query that returns entity identifiers.</param>
        /// <param name="expectedEntityIds">The entity identifiers expected.</param>
        private static void AssertSameEntityIds( IQueryable<int> entityIdQuery, int[] expectedEntityIds )
        {
            var expected = expectedEntityIds.OrderBy( id => id ).ToList();

            var withoutRewrite = entityIdQuery.ToList().OrderBy( id => id ).ToList();
            var withRewrite = entityIdQuery.WithQueryableAttributeValues().ToList().OrderBy( id => id ).ToList();

            CollectionAssert.AreEqual( expected, withoutRewrite, "Query without the checksum rewrite returned unexpected rows." );
            CollectionAssert.AreEqual( expected, withRewrite, "Query with the checksum rewrite returned unexpected rows." );
        }

        #endregion
    }
}
