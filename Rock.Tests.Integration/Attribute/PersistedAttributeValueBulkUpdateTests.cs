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
using System.Data.Entity;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Web.Cache;

using AttributeHelper = Rock.Attribute.Helper;
using RockAttribute = Rock.Model.Attribute;

namespace Rock.Tests.Integration.Attribute
{
    /// <summary>
    /// Tests for the bulk attribute-value update helpers used by the
    /// <see cref="Rock.Jobs.UpdatePersistedAttributeValues"/> job. These exercise
    /// the raw SQL statements in <see cref="Rock.Attribute.Helper"/> against a real
    /// database (they depend on the <c>ValueChecksum</c> computed column and the
    /// <c>dbo.IdList</c> table-valued parameter).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <c>*_MatchesSeparateCalls</c> tests are equivalence (golden-master)
    ///         tests: they assert that the combined
    ///         <see cref="Rock.Attribute.Helper.BulkUpdateAttributeValueComputedAndPersistedValues(int, string, Field.PersistedValues, RockContext)"/>
    ///         method produces byte-identical column values and return counts to the
    ///         original pair of <c>BulkUpdateAttributeValueComputedColumns</c> +
    ///         <c>BulkUpdateAttributeValuePersistedValues</c> calls it replaced. They pass
    ///         an identical hand-built <see cref="Field.PersistedValues"/> to both paths so
    ///         the only thing under test is the merged UPDATE statement, not field-type
    ///         formatting.
    ///     </para>
    /// </remarks>
    [TestClass]
    public class PersistedAttributeValueBulkUpdateTests : DatabaseTestsBase
    {
        #region Setup

        // A Text field type is used because these helpers operate directly on the
        // AttributeValue table and never invoke field-type formatting.
        private const string TextFieldTypeGuid = Rock.SystemGuid.FieldType.TEXT;

        private const string PersonEntityTypeGuid = Rock.SystemGuid.EntityType.PERSON;

        private readonly List<int> _createdAttributeIds = new List<int>();

        [TestCleanup]
        public void TestCleanup()
        {
            if ( !_createdAttributeIds.Any() )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var ids = string.Join( ",", _createdAttributeIds );

                // Remove the seeded values and attributes directly. The bulk helpers
                // bypass EF change tracking, so a raw delete is the simplest cleanup.
                rockContext.Database.ExecuteSqlCommand( $"DELETE FROM [AttributeValue] WHERE [AttributeId] IN ({ids})" );
                rockContext.Database.ExecuteSqlCommand( $"DELETE FROM [Attribute] WHERE [Id] IN ({ids})" );
            }

            _createdAttributeIds.Clear();
        }

        /// <summary>
        /// Creates a Text attribute on the Person entity type for use by a single test.
        /// </summary>
        /// <returns>The identifier of the newly created attribute.</returns>
        private int CreateTextAttribute()
        {
            using ( var rockContext = new RockContext() )
            {
                var attribute = new RockAttribute
                {
                    Key = $"TestPersistedBulk_{System.Guid.NewGuid():N}",
                    Name = "Test Persisted Bulk Update",
                    EntityTypeId = EntityTypeCache.GetId( PersonEntityTypeGuid.AsGuid() ),
                    FieldTypeId = FieldTypeCache.GetId( TextFieldTypeGuid.AsGuid() ).Value
                };

                new AttributeService( rockContext ).Add( attribute );
                rockContext.SaveChanges();

                _createdAttributeIds.Add( attribute.Id );

                return attribute.Id;
            }
        }

        /// <summary>
        /// Seeds a set of attribute values that all share the same raw value.
        /// </summary>
        /// <param name="attributeId">The attribute the values belong to.</param>
        /// <param name="value">The raw value for every seeded row.</param>
        /// <param name="entityIds">The entity identifiers to create rows for.</param>
        /// <param name="isDirty">Whether the seeded rows should be marked persisted-value dirty.</param>
        /// <returns>The identifiers of the created attribute values.</returns>
        private List<int> SeedValues( int attributeId, string value, IEnumerable<int> entityIds, bool isDirty )
        {
            var createdGuids = new List<System.Guid>();

            using ( var rockContext = new RockContext() )
            {
                var service = new AttributeValueService( rockContext );

                foreach ( var entityId in entityIds )
                {
                    var attributeValue = new AttributeValue
                    {
                        AttributeId = attributeId,
                        EntityId = entityId,
                        Value = value,
                        IsPersistedValueDirty = isDirty
                    };

                    service.Add( attributeValue );
                    createdGuids.Add( attributeValue.Guid );
                }

                rockContext.SaveChanges();

                // Capture the generated identifiers for exactly the rows we added.
                return service.Queryable()
                    .Where( av => createdGuids.Contains( av.Guid ) )
                    .Select( av => av.Id )
                    .ToList();
            }
        }

        /// <summary>
        /// Builds a distinct set of persisted values for a given raw value so a
        /// mis-mapping between values would be caught.
        /// </summary>
        private static Field.PersistedValues PersistedFor( string value )
        {
            return new Field.PersistedValues
            {
                TextValue = $"T:{value}",
                HtmlValue = $"H:{value}",
                CondensedTextValue = $"CT:{value}",
                CondensedHtmlValue = $"CH:{value}"
            };
        }

        private static List<AttributeValue> LoadValues( int attributeId )
        {
            using ( var rockContext = new RockContext() )
            {
                return new AttributeValueService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( av => av.AttributeId == attributeId )
                    .ToList();
            }
        }

        private static void AssertColumnsMatch( AttributeValue expected, AttributeValue actual, string value )
        {
            Assert.AreEqual( expected.ValueAsBoolean, actual.ValueAsBoolean, $"ValueAsBoolean mismatch for '{value}'." );
            Assert.AreEqual( expected.ValueAsDateTime, actual.ValueAsDateTime, $"ValueAsDateTime mismatch for '{value}'." );
            Assert.AreEqual( expected.ValueAsNumeric, actual.ValueAsNumeric, $"ValueAsNumeric mismatch for '{value}'." );
            Assert.AreEqual( expected.ValueAsPersonId, actual.ValueAsPersonId, $"ValueAsPersonId mismatch for '{value}'." );
            Assert.AreEqual( expected.PersistedTextValue, actual.PersistedTextValue, $"PersistedTextValue mismatch for '{value}'." );
            Assert.AreEqual( expected.PersistedHtmlValue, actual.PersistedHtmlValue, $"PersistedHtmlValue mismatch for '{value}'." );
            Assert.AreEqual( expected.PersistedCondensedTextValue, actual.PersistedCondensedTextValue, $"PersistedCondensedTextValue mismatch for '{value}'." );
            Assert.AreEqual( expected.PersistedCondensedHtmlValue, actual.PersistedCondensedHtmlValue, $"PersistedCondensedHtmlValue mismatch for '{value}'." );
            Assert.AreEqual( expected.IsPersistedValueDirty, actual.IsPersistedValueDirty, $"IsPersistedValueDirty mismatch for '{value}'." );
        }

        // A representative spread of values that exercise the different computed
        // columns (numeric, boolean, date, and plain text).
        private static readonly string[] SampleValues = { "5", "true", "2001-02-03", "Hello" };

        /// <summary>
        /// Seeds every <see cref="SampleValues"/> value onto the attribute, giving each
        /// row a unique EntityId (the AttributeValue table has a unique index on
        /// EntityId + AttributeId, so a value cannot reuse an entity already used by
        /// another value on the same attribute).
        /// </summary>
        /// <returns>A map of raw value to the identifiers of the rows seeded for it.</returns>
        private Dictionary<string, List<int>> SeedSampleValues( int attributeId, bool isDirty, int entityIdBase )
        {
            var map = new Dictionary<string, List<int>>();

            for ( var i = 0; i < SampleValues.Length; i++ )
            {
                var value = SampleValues[i];
                var entityIds = new[] { entityIdBase + ( i * 10 ), entityIdBase + ( i * 10 ) + 1, entityIdBase + ( i * 10 ) + 2 };

                map[value] = SeedValues( attributeId, value, entityIds, isDirty );
            }

            return map;
        }

        #endregion

        #region Single-value overload

        /// <summary>
        /// The combined single-value method must produce the same column values and
        /// return count as calling the separate computed-column and persisted-value
        /// methods in sequence.
        /// </summary>
        [TestMethod]
        public void BulkUpdateComputedAndPersisted_SingleValue_MatchesSeparateCalls()
        {
            var oldAttributeId = CreateTextAttribute();
            var newAttributeId = CreateTextAttribute();

            SeedSampleValues( oldAttributeId, isDirty: true, entityIdBase: 10000 );
            SeedSampleValues( newAttributeId, isDirty: true, entityIdBase: 20000 );

            // Act - old path (two statements) versus new path (one statement).
            using ( var rockContext = new RockContext() )
            {
                foreach ( var value in SampleValues )
                {
                    var persisted = PersistedFor( value );

                    var oldComputedCount = AttributeHelper.BulkUpdateAttributeValueComputedColumns( oldAttributeId, value, rockContext );
                    var oldPersistedCount = AttributeHelper.BulkUpdateAttributeValuePersistedValues( oldAttributeId, value, persisted, rockContext );

                    var newCount = AttributeHelper.BulkUpdateAttributeValueComputedAndPersistedValues( newAttributeId, value, persisted, rockContext );

                    Assert.AreEqual( 3, oldPersistedCount, $"Old persisted count unexpected for '{value}'." );
                    Assert.AreEqual( oldPersistedCount, newCount, $"Combined return count should match the persisted-values count for '{value}'." );
                    Assert.AreEqual( oldComputedCount, newCount, $"Combined return count should match the computed-columns count for '{value}'." );
                }
            }

            // Assert - the resulting rows are identical, matched by value.
            var oldValues = LoadValues( oldAttributeId ).ToLookup( av => av.Value );
            var newValues = LoadValues( newAttributeId ).ToLookup( av => av.Value );

            foreach ( var value in SampleValues )
            {
                var expected = oldValues[value].First();

                foreach ( var actual in newValues[value] )
                {
                    AssertColumnsMatch( expected, actual, value );
                }

                Assert.IsFalse( expected.IsPersistedValueDirty, $"Dirty flag should be cleared for '{value}'." );
            }
        }

        #endregion

        #region ValueIds overload

        /// <summary>
        /// With <c>onlyDirty = false</c> the combined value-ids method must produce the
        /// same column values and return count as the separate calls (both touch every
        /// row in the id set).
        /// </summary>
        [TestMethod]
        public void BulkUpdateComputedAndPersisted_ValueIds_NotOnlyDirty_MatchesSeparateCalls()
        {
            var oldAttributeId = CreateTextAttribute();
            var newAttributeId = CreateTextAttribute();

            var oldIdsByValue = SeedSampleValues( oldAttributeId, isDirty: false, entityIdBase: 30000 );
            var newIdsByValue = SeedSampleValues( newAttributeId, isDirty: false, entityIdBase: 40000 );

            using ( var rockContext = new RockContext() )
            {
                foreach ( var value in SampleValues )
                {
                    var oldIds = oldIdsByValue[value];
                    var newIds = newIdsByValue[value];
                    var persisted = PersistedFor( value );

                    var oldComputedCount = AttributeHelper.BulkUpdateAttributeValueComputedColumns( oldAttributeId, oldIds, value, rockContext );
                    var oldPersistedCount = AttributeHelper.BulkUpdateAttributeValuePersistedValues( oldAttributeId, oldIds, persisted, false, rockContext );

                    var newCount = AttributeHelper.BulkUpdateAttributeValueComputedAndPersistedValues( newAttributeId, newIds, value, persisted, false, rockContext );

                    Assert.AreEqual( 3, oldComputedCount, $"Old computed count unexpected for '{value}'." );
                    Assert.AreEqual( oldPersistedCount, newCount, $"Combined return count should match the persisted-values count for '{value}'." );
                }
            }

            var oldValues = LoadValues( oldAttributeId ).ToLookup( av => av.Value );
            var newValues = LoadValues( newAttributeId ).ToLookup( av => av.Value );

            foreach ( var value in SampleValues )
            {
                var expected = oldValues[value].First();

                foreach ( var actual in newValues[value] )
                {
                    AssertColumnsMatch( expected, actual, value );
                }
            }
        }

        /// <summary>
        /// With <c>onlyDirty = true</c> the combined method updates and clears the dirty
        /// rows while leaving clean rows entirely untouched - including their computed
        /// columns. This is the intentional behavior change over the old pair (whose
        /// computed-columns statement had no dirty filter); it is safe because the only
        /// caller that passes <c>true</c> supplies a value set already selected on
        /// <c>IsPersistedValueDirty</c>.
        /// </summary>
        [TestMethod]
        public void BulkUpdateComputedAndPersisted_ValueIds_OnlyDirty_SkipsCleanRows()
        {
            var attributeId = CreateTextAttribute();
            const string value = "5";

            var dirtyIds = SeedValues( attributeId, value, new[] { 3001, 3002 }, isDirty: true );
            var cleanIds = SeedValues( attributeId, value, new[] { 3003, 3004 }, isDirty: false );

            // Snapshot the clean rows so we can assert they are completely untouched.
            // (Their ValueAs* columns are already populated by the save hook, and the
            // persisted columns default to empty string, so a before/after comparison is
            // more reliable than checking for null.)
            var cleanBefore = LoadValues( attributeId )
                .Where( av => cleanIds.Contains( av.Id ) )
                .ToDictionary( av => av.Id );

            var persisted = PersistedFor( value );
            var allIds = dirtyIds.Concat( cleanIds ).ToList();

            int updatedCount;
            using ( var rockContext = new RockContext() )
            {
                updatedCount = AttributeHelper.BulkUpdateAttributeValueComputedAndPersistedValues( attributeId, allIds, value, persisted, true, rockContext );
            }

            Assert.AreEqual( dirtyIds.Count, updatedCount, "Only the dirty rows should have been updated." );

            var byId = LoadValues( attributeId ).ToDictionary( av => av.Id );

            foreach ( var id in dirtyIds )
            {
                var row = byId[id];
                Assert.IsFalse( row.IsPersistedValueDirty, "Dirty row should be cleared." );
                Assert.AreEqual( "T:5", row.PersistedTextValue, "Dirty row should have persisted text set." );
                Assert.AreEqual( 5m, row.ValueAsNumeric, "Dirty row should have computed numeric set." );
            }

            foreach ( var id in cleanIds )
            {
                AssertColumnsMatch( cleanBefore[id], byId[id], $"clean row {id}" );
            }
        }

        #endregion

        #region Entity references

        /// <summary>
        /// A non-entity-reference field type (e.g. Text) must not create any attribute
        /// references. This guards the reorder in
        /// <see cref="Rock.Attribute.Helper.UpdateAttributeEntityReferences(RockAttribute, RockContext)"/>
        /// that skips the referenced-entity query for non-reference field types - the
        /// only behavioral delta of that change.
        /// </summary>
        [TestMethod]
        public void UpdateAttributeEntityReferences_NonReferenceFieldType_CreatesNoReferences()
        {
            var attributeId = CreateTextAttribute();

            using ( var rockContext = new RockContext() )
            {
                var attribute = new AttributeService( rockContext ).Queryable()
                    .Include( a => a.AttributeQualifiers )
                    .Single( a => a.Id == attributeId );

                AttributeHelper.UpdateAttributeEntityReferences( attribute, rockContext );
                rockContext.SaveChanges();
            }

            using ( var rockContext = new RockContext() )
            {
                var referenceCount = rockContext.Set<AttributeReferencedEntity>()
                    .Count( re => re.AttributeId == attributeId );

                Assert.AreEqual( 0, referenceCount, "A non-reference field type should not create attribute references." );
            }
        }

        #endregion
    }
}
