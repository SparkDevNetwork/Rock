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
using System.Linq.Expressions;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Defines the base filter expression class that concrete filter expressions
    /// must inherit from. These define the ability for a user to create a filter
    /// and then evaluate an object against that filter.
    /// </summary>
    public abstract class FilterExpression
    {
        /// <summary>
        /// Gets the expression that will be used to evaluate this filter.
        /// </summary>
        /// <param name="target">The target object whose property will be evaluated.</param>
        /// <param name="propertyName">Name of the property on the target to be evaluated.</param>
        /// <returns>A LINQ Expression that will evaluate this comparison.</returns>
        public virtual Expression GetExpression( object target, string propertyName )
        {
            var property = Expression.Property( Expression.Constant( target ), propertyName );

            return GetExpression( property );
        }

        /// <summary>
        /// Gets the expression that will be used to evaluate this filter.
        /// </summary>
        /// <param name="property">The property that contains the value to be compared.</param>
        /// <returns>A LINQ Expression that will evaluate this comparison.</returns>
        public abstract Expression GetExpression( MemberExpression property );

        /// <summary>
        /// Evaluates the filter against the target's property.
        /// </summary>
        /// <param name="target">The target object whose property will be evaluated.</param>
        /// <param name="propertyName">Name of the property on the target to be evaluated.</param>
        /// <returns><c>true</c> if the evaluation is truthful, <c>false</c> otherwise.</returns>
        public virtual bool Evaluate( object target, string propertyName )
        {
            var expression = GetExpression( target, propertyName );
            var expressionFunc = Expression.Lambda<Func<bool>>( expression ).Compile();

            return expressionFunc();
        }

        /// <summary>
        /// Evaluates the filter against a property value.
        /// </summary>
        /// <param name="property">The property that contains the value to be compared.</param>
        /// <returns><c>true</c> if the evaluation is truthful, <c>false</c> otherwise.</returns>
        public virtual bool Evaluate( MemberExpression property )
        {
            var expression = GetExpression( property );
            var expressionFunc = Expression.Lambda<Func<bool>>( expression ).Compile();

            return expressionFunc();
        }

        /// <summary>
        /// Creates a FilterExpression from the given JSON data.
        /// </summary>
        /// <param name="value">The JSON string value.</param>
        /// <returns>A FilterExpression object or null if the data was invalid.</returns>
        public static FilterExpression FromJsonOrNull( string value )
        {
            if ( value.IsNullOrWhiteSpace() )
            {
                return null;
            }

            try
            {
                return FromJObject( Newtonsoft.Json.Linq.JObject.Parse( value ) );
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a FilterExpression from the given JSON object.
        /// </summary>
        /// <param name="jobject">The JSON object.</param>
        /// <returns>A FilterExpression object.</returns>
        public static FilterExpression FromJObject( Newtonsoft.Json.Linq.JObject jobject )
        {
            if ( jobject["Filters"] != null )
            {
                return new CompoundFilterExpression( jobject );
            }
            else
            {
                return new ComparisonFilterExpression( jobject );
            }
        }
    }
}
