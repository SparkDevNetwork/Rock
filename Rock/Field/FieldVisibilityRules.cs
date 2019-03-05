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
using System.Linq.Expressions;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Field
{
    /// <summary>
    /// A Collection of FieldVisibilityRules and the FilterExpressionType (GroupAll, GroupAny) that should be used to evaluate them
    /// </summary>
    [Serializable]
    [System.Diagnostics.DebuggerDisplay( "{DebuggerFormattedRules}" )]
    public class FieldVisibilityRules
    {
        /// <summary>
        /// Gets or sets the rule list.
        /// </summary>
        /// <value>
        /// The rule list.
        /// </value>
        [DataMember]
        public List<FieldVisibilityRule> RuleList { get; set; } = new List<FieldVisibilityRule>();

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public FieldVisibilityRules Clone()
        {
            var clone = new FieldVisibilityRules();
            clone.RuleList.AddRange( this.RuleList );
            clone.FilterExpressionType = this.FilterExpressionType;
            return clone;
        }

        /// <summary>
        /// Gets or sets the type of the filter expression.
        /// </summary>
        /// <value>
        /// The type of the filter expression.
        /// </value>
        [DataMember]
        public FilterExpressionType FilterExpressionType { get; set; } = FilterExpressionType.GroupAll;

        /// <summary>
        /// Returns true if the field for these FieldVisibilityRules should be visible given the supplied attributeValues
        /// </summary>
        /// <param name="attributeValues">The attribute values.</param>
        /// <returns></returns>
        public bool Evaluate( Dictionary<int, AttributeValueCache> attributeValues )
        {
            bool visible = true;
            var fieldVisibilityRules = this;

            if ( !fieldVisibilityRules.RuleList.Any() || !attributeValues.Any() )
            {
                // if no rules or attribute values, just exit
                return visible;
            }

            foreach ( var fieldVisibilityRule in fieldVisibilityRules.RuleList.Where( a => a.ComparedToAttributeGuid.HasValue ) )
            {
                var filterValues = new List<string>();

                var comparedToAttribute = AttributeCache.Get( fieldVisibilityRule.ComparedToAttributeGuid.Value );

                // if this is a TextFieldType, In-Memory LINQ is case-sensitive but LinqToSQL is not, so lets compare values using ToLower()
                if ( comparedToAttribute.FieldType.Field is Rock.Field.Types.TextFieldType )
                {
                    fieldVisibilityRule.ComparedToValue = fieldVisibilityRule.ComparedToValue?.ToLower();
                }

                filterValues.Add( fieldVisibilityRule.ComparisonType.ConvertToString( false ) );
                filterValues.Add( fieldVisibilityRule.ComparedToValue );
                Expression entityCondition;

                ParameterExpression parameterExpression = Expression.Parameter( typeof( Rock.Model.AttributeValue ) );

                entityCondition = comparedToAttribute.FieldType.Field.AttributeFilterExpression( comparedToAttribute.QualifierValues, filterValues, parameterExpression );
                if ( entityCondition is NoAttributeFilterExpression )
                {
                    continue;
                }

                var conditionLambda = Expression.Lambda<Func<Rock.Model.AttributeValue, bool>>( entityCondition, parameterExpression );
                var conditionFunc = conditionLambda.Compile();
                var comparedToAttributeValue = attributeValues.GetValueOrNull( comparedToAttribute.Id )?.Value;

                // if this is a TextFieldType, In-Memory LINQ is case-sensitive but LinqToSQL is not, so lets compare values using ToLower()
                if ( comparedToAttribute.FieldType.Field is Rock.Field.Types.TextFieldType )
                {
                    comparedToAttributeValue = comparedToAttributeValue?.ToLower();
                }

                // create an instance of an AttributeValue to run the expressions against
                var attributeValueToEvaluate = new Rock.Model.AttributeValue
                {
                    AttributeId = comparedToAttribute.Id,
                    Value = comparedToAttributeValue,
                    ValueAsBoolean = comparedToAttributeValue.AsBooleanOrNull(),
                    ValueAsNumeric = comparedToAttributeValue.AsDecimalOrNull(),
                    ValueAsDateTime = comparedToAttributeValue.AsDateTime()
                };

                var conditionResult = conditionFunc.Invoke( attributeValueToEvaluate );
                switch ( fieldVisibilityRules.FilterExpressionType )
                {
                    case Rock.Model.FilterExpressionType.GroupAll:
                        {
                            visible = visible && conditionResult;
                            break;
                        }

                    case Rock.Model.FilterExpressionType.GroupAllFalse:
                        {
                            visible = visible && !conditionResult;
                            break;
                        }

                    case Rock.Model.FilterExpressionType.GroupAny:
                        {
                            visible = visible || conditionResult;
                            break;
                        }

                    case Rock.Model.FilterExpressionType.GroupAnyFalse:
                        {
                            visible = visible || !conditionResult;
                            break;
                        }

                    default:
                        {
                            // ignore if unexpected FilterExpressionType
                            break;
                        }
                }
            }

            return visible;
        }

        /// <summary>
        /// Gets the debugger formatted rules.
        /// </summary>
        /// <value>
        /// The debugger formatted rules.
        /// </value>
        internal string DebuggerFormattedRules => $"{FilterExpressionType} {this.RuleList.AsDelimited( " and " )}";
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class FieldVisibilityRule
    {
        /// <summary>
        /// Gets or sets the compared to attribute unique identifier.
        /// </summary>
        /// <value>
        /// The compared to attribute unique identifier.
        /// </value>
        [DataMember]
        public Guid? ComparedToAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        [DataMember]
        public Guid Guid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the type of the comparison.
        /// </summary>
        /// <value>
        /// The type of the comparison.
        /// </value>
        [DataMember]
        public ComparisonType ComparisonType { get; set; }

        /// <summary>
        /// Gets or sets FieldType value (as interpreted by FieldType of the field that this rule is acting upon ) to be used when doing the comparison
        /// </summary>
        /// <value>
        /// The compared to value.
        /// </value>
        [DataMember]
        public string ComparedToValue { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var comparedToAttribute = this.ComparedToAttributeGuid.HasValue ? AttributeCache.Get( this.ComparedToAttributeGuid.Value ) : null;
            List<string> filterValues = new List<string>( new string[2] { this.ComparisonType.ConvertToString(), this.ComparedToValue } );
            var result = $"{comparedToAttribute?.Name} {comparedToAttribute?.FieldType.Field.FormatFilterValues( comparedToAttribute.QualifierValues, filterValues ) } ";
            return result;
        }
    }
}
