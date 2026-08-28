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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Core.Model
{
    /// <summary>
    /// Tests for the ExceptionGroupHash computed column that the Exception List and Exception Occurrences blocks
    /// group and filter by.
    /// </summary>
    [TestClass]
    [TestCategory( "Core.ExceptionLog" )]
    public class ExceptionLogExceptionGroupHashTests : DatabaseTestsBase
    {
        #region Fields

        private const string _columnName = "ExceptionGroupHash";
        private const string _tableName = "dbo.ExceptionLog";

        /// <summary>
        /// Marks the exceptions created by the running test so that they can be removed afterwards without touching
        /// any exception the database was seeded with.
        /// </summary>
        private string _exceptionForeignKey;

        #endregion Fields

        #region Setup

        [TestInitialize]
        public void TestInitialize()
        {
            _exceptionForeignKey = $"Test {Guid.NewGuid()}";
        }

        [TestCleanup]
        public void TestCleanup()
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( $"DELETE [ExceptionLog] WHERE [ForeignKey] = '{_exceptionForeignKey}'" );
            }
        }

        #endregion Setup

        #region Column Definition

        /// <summary>
        /// The whole performance fix rests on this column being usable in an index, which SQL Server allows only for
        /// a computed column it can prove is deterministic and precise. A definition that yields nvarchar(max),
        /// which both CONCAT() and an uncast LEFT() do, is neither, and nothing reports it: the schema applies, every
        /// query still returns correct results, and the covering index quietly stops being able to hold the column.
        /// </summary>
        [TestMethod]
        public void ExceptionGroupHash_IsIndexable()
        {
            Assert.AreEqual( 1, GetColumnProperty( "IsIndexable" ), $"[{_columnName}] must be indexable; a definition that yields nvarchar(max) is not." );
        }

        /// <summary>
        /// The column has to stay computed and non-persisted. Persisting it would write the value on every insert
        /// and rewrite the whole table when it is added, for a value whose only reader is the index that already
        /// stores its own copy.
        /// </summary>
        [TestMethod]
        public void ExceptionGroupHash_IsComputedAndNotPersisted()
        {
            using ( var rockContext = new RockContext() )
            {
                var isPersisted = rockContext.Database.SqlQuery<bool?>( $@"
SELECT [is_persisted]
FROM [sys].[computed_columns]
WHERE [object_id] = OBJECT_ID( '{_tableName}' )
  AND [name] = '{_columnName}'" ).FirstOrDefault();

                Assert.IsNotNull( isPersisted, $"[{_columnName}] was not found as a computed column on [{_tableName}]." );
                Assert.IsFalse( isPersisted.Value, $"[{_columnName}] must not be persisted." );
            }
        }

        /// <summary>
        /// The column is BINARY(32) because SHA-256 produces 32 bytes and a fixed width keeps it in the fixed
        /// portion of the index row. BINARY silently right-pads a shorter value and silently truncates a longer one,
        /// so a width that disagrees with the algorithm would corrupt grouping with no error at all.
        /// </summary>
        [TestMethod]
        public void ExceptionGroupHash_IsBinaryThirtyTwoBytes()
        {
            using ( var rockContext = new RockContext() )
            {
                var columnType = rockContext.Database.SqlQuery<string>( $@"
SELECT [t].[name] + '(' + CAST( [c].[max_length] AS VARCHAR(10) ) + ')'
FROM [sys].[columns] AS [c]
JOIN [sys].[types] AS [t] ON [t].[user_type_id] = [c].[user_type_id]
WHERE [c].[object_id] = OBJECT_ID( '{_tableName}' )
  AND [c].[name] = '{_columnName}'" ).FirstOrDefault();

                Assert.AreEqual( "binary(32)", columnType, $"[{_columnName}] must be a fixed 32 byte binary column." );
            }
        }

        #endregion Column Definition

        #region Grouping Semantics

        /// <summary>
        /// Two exceptions that agree on their type and on the hashed prefix of their description belong to the same
        /// Exception List row, however much their descriptions differ past that prefix. This is what lets a stack of
        /// exceptions whose messages end in different ids, paths or timestamps collapse into one row.
        /// </summary>
        [TestMethod]
        public void ExceptionGroupHash_WithDescriptionsDifferingOnlyAfterThePrefix_HashesMatch()
        {
            var sharedPrefix = BuildDescriptionOfExactlyPrefixLength();

            var firstHash = GetExceptionGroupHash( AddException( "System.InvalidOperationException", sharedPrefix + " for person 12345." ) );
            var secondHash = GetExceptionGroupHash( AddException( "System.InvalidOperationException", sharedPrefix + " for person 98765, twice, on a different page." ) );

            Assert.IsTrue( firstHash.SequenceEqual( secondHash ), "Exceptions differing only past the hashed prefix must group together." );
        }

        /// <summary>
        /// The last character of the prefix has to count. If the hashed length were shorter than
        /// DescriptionGroupingPrefixLength, this pair would collide and two genuinely different errors would be
        /// merged into one Exception List row, which is the bug that motivated raising the length from 95 to 255.
        /// </summary>
        [TestMethod]
        public void ExceptionGroupHash_WithDescriptionsDifferingAtTheLastPrefixCharacter_HashesDiffer()
        {
            var prefixLength = ExceptionLogService.DescriptionGroupingPrefixLength;
            var firstDescription = BuildDescriptionOfExactlyPrefixLength();
            var secondDescription = firstDescription.Substring( 0, prefixLength - 1 ) + "Z";

            var firstHash = GetExceptionGroupHash( AddException( "System.InvalidOperationException", firstDescription ) );
            var secondHash = GetExceptionGroupHash( AddException( "System.InvalidOperationException", secondDescription ) );

            Assert.IsFalse( firstHash.SequenceEqual( secondHash ), $"The character at position {prefixLength} must be part of the hash." );
        }

        /// <summary>
        /// The same message raised as two different exception types is two different problems, so the type has to be
        /// part of the hash.
        /// </summary>
        [TestMethod]
        public void ExceptionGroupHash_WithDifferentExceptionTypes_HashesDiffer()
        {
            var description = "Object reference not set to an instance of an object.";

            var firstHash = GetExceptionGroupHash( AddException( "System.NullReferenceException", description ) );
            var secondHash = GetExceptionGroupHash( AddException( "System.InvalidOperationException", description ) );

            Assert.IsFalse( firstHash.SequenceEqual( secondHash ), "Exceptions of different types must not group together." );
        }

        /// <summary>
        /// The pipe between the two parts is what stops the boundary from moving. Hashing the concatenation without
        /// a delimiter would make these two exceptions - which share the string "SomeExceptionBoom" but split it
        /// differently - collide.
        /// </summary>
        [TestMethod]
        public void ExceptionGroupHash_WithTheTypeAndDescriptionBoundaryShifted_HashesDiffer()
        {
            var firstHash = GetExceptionGroupHash( AddException( "SomeException", "Boom" ) );
            var secondHash = GetExceptionGroupHash( AddException( "SomeExceptionBoo", "m" ) );

            Assert.IsFalse( firstHash.SequenceEqual( secondHash ), "The delimiter between the type and the description must keep their boundary fixed." );
        }

        /// <summary>
        /// Both hashed columns are nullable, and HASHBYTES of a NULL input returns NULL. The ISNULL guards in the
        /// column definition are what stop such an exception from getting a NULL hash, which would drop it out of
        /// every grouped and filtered query the blocks run.
        /// </summary>
        [TestMethod]
        public void ExceptionGroupHash_WithNullTypeAndDescription_IsStillAThirtyTwoByteHash()
        {
            var hash = GetExceptionGroupHash( AddException( null, null ) );

            Assert.IsNotNull( hash, "An exception with no type and no description must still be hashed." );
            Assert.HasCount( 32, hash );
        }

        #endregion Grouping Semantics

        #region FilterByExceptionGroupHash

        /// <summary>
        /// The Exception Occurrences block reaches its rows through this filter, so it has to return every exception
        /// in the group and nothing outside it.
        /// </summary>
        [TestMethod]
        public void FilterByExceptionGroupHash_ReturnsOnlyTheExceptionsInTheGroup()
        {
            var sharedPrefix = BuildDescriptionOfExactlyPrefixLength();

            var firstInGroupId = AddException( "System.InvalidOperationException", sharedPrefix + " for person 12345." );
            var secondInGroupId = AddException( "System.InvalidOperationException", sharedPrefix + " for person 98765." );
            var outsideGroupId = AddException( "System.NullReferenceException", sharedPrefix + " for person 12345." );

            var groupHash = GetExceptionGroupHash( firstInGroupId );

            using ( var rockContext = new RockContext() )
            {
                var exceptionLogService = new ExceptionLogService( rockContext );

                var matchedIds = exceptionLogService
                    .FilterByExceptionGroupHash( exceptionLogService.Queryable().Where( e => e.ForeignKey == _exceptionForeignKey ), groupHash )
                    .Select( e => e.Id )
                    .ToList();

                Assert.HasCount( 2, matchedIds );
                CollectionAssert.Contains( matchedIds, firstInGroupId );
                CollectionAssert.Contains( matchedIds, secondInGroupId );
                CollectionAssert.DoesNotContain( matchedIds, outsideGroupId );
            }
        }

        #endregion FilterByExceptionGroupHash

        #region Helpers

        /// <summary>
        /// Builds a description that is exactly DescriptionGroupingPrefixLength characters long, so that appending to
        /// it adds characters the hash must ignore and editing its last character changes one the hash must include.
        /// </summary>
        /// <returns>A description of exactly the hashed prefix length.</returns>
        private static string BuildDescriptionOfExactlyPrefixLength()
        {
            const string lead = "The operation is not valid because ";

            return lead + new string( 'a', ExceptionLogService.DescriptionGroupingPrefixLength - lead.Length );
        }

        /// <summary>
        /// Adds an exception marked with the running test's foreign key.
        /// </summary>
        /// <param name="exceptionType">The exception type to record.</param>
        /// <param name="description">The description to record.</param>
        /// <returns>The identifier of the new exception.</returns>
        private int AddException( string exceptionType, string description )
        {
            using ( var rockContext = new RockContext() )
            {
                var exceptionLog = new ExceptionLog
                {
                    ExceptionType = exceptionType,
                    Description = description,
                    ForeignKey = _exceptionForeignKey
                };

                new ExceptionLogService( rockContext ).Add( exceptionLog );
                rockContext.SaveChanges();

                return exceptionLog.Id;
            }
        }

        /// <summary>
        /// Reads an exception's group hash back from the database. A new context is used so that the value comes from
        /// SQL Server rather than from the entity that was just saved.
        /// </summary>
        /// <param name="exceptionId">The identifier of the exception to read.</param>
        /// <returns>The exception's group hash.</returns>
        private static byte[] GetExceptionGroupHash( int exceptionId )
        {
            using ( var rockContext = new RockContext() )
            {
                return new ExceptionLogService( rockContext )
                    .Queryable()
                    .Where( e => e.Id == exceptionId )
                    .Select( e => e.ExceptionGroupHash )
                    .First();
            }
        }

        /// <summary>
        /// Reads one of SQL Server's COLUMNPROPERTY values for the group hash column.
        /// </summary>
        /// <param name="propertyName">The COLUMNPROPERTY name to read, such as "IsIndexable".</param>
        /// <returns>The property value, or <c>null</c> when the column does not exist.</returns>
        private static int? GetColumnProperty( string propertyName )
        {
            using ( var rockContext = new RockContext() )
            {
                return rockContext.Database
                    .SqlQuery<int?>( $"SELECT COLUMNPROPERTY( OBJECT_ID( '{_tableName}' ), '{_columnName}', '{propertyName}' )" )
                    .FirstOrDefault();
            }
        }

        #endregion Helpers
    }
}
