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
using System.Linq.Expressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.Reporting
{
    /// <summary>
    /// Tests for the in-memory filter expressions that ExpressionHelper builds for
    /// a date attribute.
    /// </summary>
    /// <remarks>
    /// The attribute is seeded into a mocked context rather than read from the
    /// sample data, so the test owns the field type and the value it filters on.
    /// </remarks>
    [TestClass]
    public class ExpressionHelperTests
    {
        #region Fields

        private const string BaptismDateReferenceValue = "2024-06-03T00:00:00.0000000";
        private const string BaptismDateAttributeKey = "BaptismDate";
        private const string DateBeforeReference = "2022-06-03T00:00:00.0000000";
        private const string DateAfterReference = "2024-07-03T00:00:00.0000000";

        #endregion Fields

        #region LessThan

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanDate_ExcludesNullValue()
        {
            Assert.IsFalse( IsMatch( ComparisonType.LessThan, null ) );
        }

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanDate_ExcludesEmptyValue()
        {
            Assert.IsFalse( IsMatch( ComparisonType.LessThan, string.Empty ) );
        }

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanDate_ExcludesLaterDate()
        {
            Assert.IsFalse( IsMatch( ComparisonType.LessThan, DateAfterReference ) );
        }

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanDate_ExcludesEqualDate()
        {
            Assert.IsFalse( IsMatch( ComparisonType.LessThan, BaptismDateReferenceValue ) );
        }

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanDate_IncludesEarlierDate()
        {
            Assert.IsTrue( IsMatch( ComparisonType.LessThan, DateBeforeReference ) );
        }

        #endregion LessThan

        #region LessThanOrEqualTo

        /*
            9/26/26 - CLAUDE

            Every test in this region previously built its filter with
            ComparisonType.LessThan, making the whole region a copy of the LessThan
            region above under names that promised otherwise. The giveaway was
            IncludesEqualDate, which asserted IsFalse - correct for "<" and wrong
            for "<=".

            These now use ComparisonType.LessThanOrEqualTo and assert what that
            comparison means, so the region tests the behavior its names describe.

            Reason: The LessThanOrEqualTo tests were duplicates of the LessThan
            tests and never exercised the comparison they were named for.
        */

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanOrEqualToDate_ExcludesNullValue()
        {
            Assert.IsFalse( IsMatch( ComparisonType.LessThanOrEqualTo, null ) );
        }

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanOrEqualToDate_ExcludesEmptyValue()
        {
            Assert.IsFalse( IsMatch( ComparisonType.LessThanOrEqualTo, string.Empty ) );
        }

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanOrEqualToDate_ExcludesLaterDate()
        {
            Assert.IsFalse( IsMatch( ComparisonType.LessThanOrEqualTo, DateAfterReference ) );
        }

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanOrEqualToDate_IncludesEqualDate()
        {
            Assert.IsTrue( IsMatch( ComparisonType.LessThanOrEqualTo, BaptismDateReferenceValue ) );
        }

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanOrEqualToDate_IncludesEarlierDate()
        {
            Assert.IsTrue( IsMatch( ComparisonType.LessThanOrEqualTo, DateBeforeReference ) );
        }

        #endregion LessThanOrEqualTo

        #region Support Methods

        /// <summary>
        /// Builds the in-memory filter expression for the seeded date attribute and
        /// evaluates it against a person holding the supplied attribute value.
        /// </summary>
        /// <param name="comparisonType">The comparison the filter applies.</param>
        /// <param name="personAttributeValue">The value held by the person under test.</param>
        /// <returns><c>true</c> when the person matches the filter.</returns>
        private static bool IsMatch( ComparisonType comparisonType, string personAttributeValue )
        {
            using ( var app = TestHelper.CreateScopedRockApp() )
            {
                var attributeCache = SeedDateAttribute();

                var parameterExpression = Expression.Parameter( typeof( Person ), "p" );

                var filterValues = new List<string>
                {
                    comparisonType.ToString(),
                    BaptismDateReferenceValue
                };

                var expression = ExpressionHelper.GetAttributeMemoryExpression( parameterExpression, attributeCache, filterValues );

                var person = new Person();
                person.LoadAttributes();

                person.SetAttributeValue( BaptismDateAttributeKey, personAttributeValue );

                var lambda = Expression.Lambda<Func<Person, bool>>( expression, parameterExpression );

                return lambda.Compile().Invoke( person );
            }
        }

        /// <summary>
        /// Seeds a date attribute on the Person entity into the mocked context and
        /// returns its cache entry.
        /// </summary>
        /// <returns>The cached attribute.</returns>
        private static AttributeCache SeedDateAttribute()
        {
            var rockContext = RockApp.Current.CreateRockContext();

            // The expression builder resolves the comparison through the field
            // type, so the attribute has to point at the real DateFieldType.
            var fieldType = MockData.CreateFieldType( rockContext,
                SystemGuid.FieldType.DATE.AsGuid(),
                "Date",
                "Rock.Field.Types.DateFieldType" );

            var personEntityTypeId = EntityTypeCache.GetId( typeof( Person ) ) ?? 0;

            var attribute = MockData.CreateAttribute( rockContext,
                BaptismDateAttributeKey,
                "Baptism Date",
                fieldType.Id,
                personEntityTypeId );

            return AttributeCache.Get( attribute.Guid );
        }

        #endregion Support Methods
    }
}
