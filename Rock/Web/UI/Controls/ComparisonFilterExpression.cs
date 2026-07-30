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
using System.Reflection;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Defines the information required to build a LINQ expression that will compare values.
    /// </summary>
    public class ComparisonFilterExpression : FilterExpression
    {
        #region Properties

        /// <summary>
        /// Gets or sets the value of this expression.
        /// </summary>
        /// <value>
        /// The value of this expression.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the second value of this expression.
        /// </summary>
        /// <value>
        /// The second value of this expression.
        /// </value>
        public string Value2 { get; set; }

        /// <summary>
        /// Gets or sets the comparision operation to use when building the expression.
        /// </summary>
        /// <value>
        /// The comparison operation to use when building the expression.
        /// </value>
        public Model.ComparisonType Comparison { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonFilterExpression"/> class.
        /// </summary>
        public ComparisonFilterExpression()
        {
            Value = string.Empty;
            Comparison = Model.ComparisonType.Contains;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonFilterExpression"/> class.
        /// </summary>
        /// <param name="jobject">The JSON object that contains the data.</param>
        public ComparisonFilterExpression( Newtonsoft.Json.Linq.JObject jobject )
            : this()
        {
            Value = jobject.Value<string>( "Value" );
            Comparison = ( Model.ComparisonType ) jobject.Value<int>( "Comparison" );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the expression that will be used to evaluate this comparison.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>A LINQ Expression that will evaluate this comparison.</returns>
        public override Expression GetExpression( MemberExpression property )
        {
            object value = Value.ToLower();
            object value2 = !string.IsNullOrEmpty( Value2 ) ? Value2.ToLower() : null;

            if ( property.Type != typeof( string ) )
            {
                value = Convert.ChangeType( value, property.Type );
                if ( value2 != null )
                {
                    value2 = Convert.ChangeType( value2, property.Type );
                }
            }

            //
            // Handle processing of Regular Expressions since they are not supported
            // by the ComparisonHelper.
            //
            if ( Comparison == Model.ComparisonType.RegularExpression )
            {
                Expression valueExpression;

                if ( property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof( Nullable<> ) )
                {
                    // if Nullable Type compare on the .Value of the property (if it HasValue)
                    valueExpression = Expression.Property( property, "Value" );
                }
                else
                {
                    valueExpression = property;
                }

                if ( valueExpression.Type == typeof( string ) )
                {
                    var miToLower = typeof( string ).GetMethod( "ToLower", new Type[] { } );
                    valueExpression = Expression.Call( valueExpression, miToLower );
                }

                var methodInfo = typeof( System.Text.RegularExpressions.Regex )
                    .GetMethod( "IsMatch", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof( string ), typeof( string ), typeof( System.Text.RegularExpressions.RegexOptions ) }, null );

                return Expression.Call( null, methodInfo, valueExpression, Expression.Constant( value ), Expression.Constant( System.Text.RegularExpressions.RegexOptions.IgnoreCase ) );
            }

            if ( property.Type == typeof( string ) )
            {
                var fakeObject = new
                {
                    Value = Expression.Lambda<Func<string>>( property ).Compile()().ToStringSafe().ToLower()
                };
                property = Expression.Property( Expression.Constant( fakeObject ), "Value" );
            }

            return Rock.Reporting.ComparisonHelper.ComparisonExpression( Comparison, property, Expression.Constant( value ), value2 != null ? Expression.Constant( value2 ) : null );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            switch ( Comparison )
            {
                case Model.ComparisonType.Between:
                    return string.Empty;

                case Model.ComparisonType.Contains:
                    return $"Contains '{ Value }'";

                case Model.ComparisonType.DoesNotContain:
                    return $"Does Not contain '{ Value }'";

                case Model.ComparisonType.EndsWith:
                    return $"Ends With '{ Value }'";

                case Model.ComparisonType.EqualTo:
                    return $"Equal To '{ Value }'";

                case Model.ComparisonType.GreaterThan:
                    return $"Greater Than '{ Value }'";

                case Model.ComparisonType.GreaterThanOrEqualTo:
                    return $"Greater Than Or Equal To '{ Value }'";

                case Model.ComparisonType.IsBlank:
                    return "Is Blank";

                case Model.ComparisonType.IsNotBlank:
                    return "Is Not Blank";

                case Model.ComparisonType.LessThan:
                    return $"Less Than '{ Value }'";

                case Model.ComparisonType.LessThanOrEqualTo:
                    return $"Less Than Or Equal To '{ Value }'";

                case Model.ComparisonType.NotEqualTo:
                    return $"Not Equal To";

                case Model.ComparisonType.RegularExpression:
                    return $"Matches Expression '{ Value }'";

                case Model.ComparisonType.StartsWith:
                    return $"Starts With '{ Value }'";

                default:
                    return $"{ Comparison.ToString() } '{ Value }'";
            }
        }

        #endregion
    }
}
