using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Reporting
{
    [TestClass]
    public class ExpressionHelperTests : DatabaseTestsBase
    {
        private static readonly string BaptismDateReferenceValue = "2024-06-03T00:00:00.0000000";
        private static readonly Guid BaptismDateAttributeGuid = Guid.Parse( "d42763fa-28e9-4a55-a25a-48998d7d7fef" );
        private static readonly string BaptismDateAttributeKey = "BaptismDate";

        #region GetAttributeMemoryExpression Less Than Date Attribute Tests

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanDate_ExcludesNullValue()
        {
            var parameterExpression = Expression.Parameter( typeof( Person ), "p" );
            var filterValues = new List<string>
            {
                ComparisonType.LessThan.ToString(),
                BaptismDateReferenceValue
            };

            var attributeCache = AttributeCache.Get( BaptismDateAttributeGuid );
            var expression = ExpressionHelper.GetAttributeMemoryExpression( parameterExpression, attributeCache, filterValues );

            var tedDecker = new Person();
            tedDecker.LoadAttributes();

            tedDecker.SetAttributeValue( BaptismDateAttributeKey, null );

            var lambda = Expression.Lambda<Func<Person, bool>>( expression, parameterExpression );
            var isMatch = lambda.Compile().Invoke( tedDecker );

            Assert.That.IsFalse( isMatch );
        }

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanDate_ExcludesEmptyValue()
        {
            var parameterExpression = Expression.Parameter( typeof( Person ), "p" );
            var filterValues = new List<string>
            {
                ComparisonType.LessThan.ToString(),
                BaptismDateReferenceValue
            };

            var attributeCache = AttributeCache.Get( BaptismDateAttributeGuid );
            var expression = ExpressionHelper.GetAttributeMemoryExpression( parameterExpression, attributeCache, filterValues );

            var tedDecker = new Person();
            tedDecker.LoadAttributes();

            tedDecker.SetAttributeValue( BaptismDateAttributeKey, string.Empty );

            var lambda = Expression.Lambda<Func<Person, bool>>( expression, parameterExpression );
            var isMatch = lambda.Compile().Invoke( tedDecker );

            Assert.That.IsFalse( isMatch );
        }

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanDate_ExcludesLaterDate()
        {
            var parameterExpression = Expression.Parameter( typeof( Person ), "p" );
            var filterValues = new List<string>
            {
                ComparisonType.LessThan.ToString(),
                BaptismDateReferenceValue
            };

            var attributeCache = AttributeCache.Get( BaptismDateAttributeGuid );
            var expression = ExpressionHelper.GetAttributeMemoryExpression( parameterExpression, attributeCache, filterValues );

            var tedDecker = new Person();
            tedDecker.LoadAttributes();

            tedDecker.SetAttributeValue( BaptismDateAttributeKey, "2024-07-03T00:00:00.0000000" );

            var lambda = Expression.Lambda<Func<Person, bool>>( expression, parameterExpression );
            var isMatch = lambda.Compile().Invoke( tedDecker );

            Assert.That.IsFalse( isMatch );
        }

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanDate_ExcludesEqualDate()
        {
            var parameterExpression = Expression.Parameter( typeof( Person ), "p" );
            var filterValues = new List<string>
            {
                ComparisonType.LessThan.ToString(),
                BaptismDateReferenceValue
            };

            var attributeCache = AttributeCache.Get( BaptismDateAttributeGuid );
            var expression = ExpressionHelper.GetAttributeMemoryExpression( parameterExpression, attributeCache, filterValues );

            var tedDecker = new Person();
            tedDecker.LoadAttributes();

            tedDecker.SetAttributeValue( BaptismDateAttributeKey, BaptismDateReferenceValue );

            var lambda = Expression.Lambda<Func<Person, bool>>( expression, parameterExpression );
            var isMatch = lambda.Compile().Invoke( tedDecker );

            Assert.That.IsFalse( isMatch );
        }

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanDate_IncludesEarlierDate()
        {
            var parameterExpression = Expression.Parameter( typeof( Person ), "p" );
            var filterValues = new List<string>
            {
                ComparisonType.LessThan.ToString(),
                BaptismDateReferenceValue
            };

            var attributeCache = AttributeCache.Get( BaptismDateAttributeGuid );
            var expression = ExpressionHelper.GetAttributeMemoryExpression( parameterExpression, attributeCache, filterValues );

            var tedDecker = new Person();
            tedDecker.LoadAttributes();

            tedDecker.SetAttributeValue( BaptismDateAttributeKey, "2022-06-03T00:00:00.0000000" );

            var lambda = Expression.Lambda<Func<Person, bool>>( expression, parameterExpression );
            var isMatch = lambda.Compile().Invoke( tedDecker );

            Assert.That.IsTrue( isMatch );
        }

        #endregion

        #region GetAttributeMemoryExpression Less Than Or Equal To Date Attribute Tests

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanOrEqualToDate_ExcludesNullValue()
        {
            var parameterExpression = Expression.Parameter( typeof( Person ), "p" );
            var filterValues = new List<string>
            {
                ComparisonType.LessThan.ToString(),
                BaptismDateReferenceValue
            };

            var attributeCache = AttributeCache.Get( BaptismDateAttributeGuid );
            var expression = ExpressionHelper.GetAttributeMemoryExpression( parameterExpression, attributeCache, filterValues );

            var tedDecker = new Person();
            tedDecker.LoadAttributes();

            tedDecker.SetAttributeValue( BaptismDateAttributeKey, null );

            var lambda = Expression.Lambda<Func<Person, bool>>( expression, parameterExpression );
            var isMatch = lambda.Compile().Invoke( tedDecker );

            Assert.That.IsFalse( isMatch );
        }

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanOrEqualToDate_ExcludesEmptyValue()
        {
            var parameterExpression = Expression.Parameter( typeof( Person ), "p" );
            var filterValues = new List<string>
            {
                ComparisonType.LessThan.ToString(),
                BaptismDateReferenceValue
            };

            var attributeCache = AttributeCache.Get( BaptismDateAttributeGuid );
            var expression = ExpressionHelper.GetAttributeMemoryExpression( parameterExpression, attributeCache, filterValues );

            var tedDecker = new Person();
            tedDecker.LoadAttributes();

            tedDecker.SetAttributeValue( BaptismDateAttributeKey, string.Empty );

            var lambda = Expression.Lambda<Func<Person, bool>>( expression, parameterExpression );
            var isMatch = lambda.Compile().Invoke( tedDecker );

            Assert.That.IsFalse( isMatch );
        }

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanOrEqualToDate_ExcludesLaterDate()
        {
            var parameterExpression = Expression.Parameter( typeof( Person ), "p" );
            var filterValues = new List<string>
            {
                ComparisonType.LessThan.ToString(),
                BaptismDateReferenceValue
            };

            var attributeCache = AttributeCache.Get( BaptismDateAttributeGuid );
            var expression = ExpressionHelper.GetAttributeMemoryExpression( parameterExpression, attributeCache, filterValues );

            var tedDecker = new Person();
            tedDecker.LoadAttributes();

            tedDecker.SetAttributeValue( BaptismDateAttributeKey, "2024-07-03T00:00:00.0000000" );

            var lambda = Expression.Lambda<Func<Person, bool>>( expression, parameterExpression );
            var isMatch = lambda.Compile().Invoke( tedDecker );

            Assert.That.IsFalse( isMatch );
        }

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanOrEqualToDate_IncludesEqualDate()
        {
            var parameterExpression = Expression.Parameter( typeof( Person ), "p" );
            var filterValues = new List<string>
            {
                ComparisonType.LessThan.ToString(),
                BaptismDateReferenceValue
            };

            var attributeCache = AttributeCache.Get( BaptismDateAttributeGuid );
            var expression = ExpressionHelper.GetAttributeMemoryExpression( parameterExpression, attributeCache, filterValues );

            var tedDecker = new Person();
            tedDecker.LoadAttributes();

            tedDecker.SetAttributeValue( BaptismDateAttributeKey, BaptismDateReferenceValue );

            var lambda = Expression.Lambda<Func<Person, bool>>( expression, parameterExpression );
            var isMatch = lambda.Compile().Invoke( tedDecker );

            Assert.That.IsFalse( isMatch );
        }

        [TestMethod]
        public void GetAttributeMemoryExpression_WithLessThanOrEqualToDate_IncludesEarlierDate()
        {
            var parameterExpression = Expression.Parameter( typeof( Person ), "p" );
            var filterValues = new List<string>
            {
                ComparisonType.LessThan.ToString(),
                BaptismDateReferenceValue
            };

            var attributeCache = AttributeCache.Get( BaptismDateAttributeGuid );
            var expression = ExpressionHelper.GetAttributeMemoryExpression( parameterExpression, attributeCache, filterValues );

            var tedDecker = new Person();
            tedDecker.LoadAttributes();

            tedDecker.SetAttributeValue( BaptismDateAttributeKey, "2022-06-03T00:00:00.0000000" );

            var lambda = Expression.Lambda<Func<Person, bool>>( expression, parameterExpression );
            var isMatch = lambda.Compile().Invoke( tedDecker );

            Assert.That.IsTrue( isMatch );
        }

        #endregion
    }
}
