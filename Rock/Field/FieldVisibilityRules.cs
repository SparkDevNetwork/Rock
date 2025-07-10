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
using Rock.Field.Types;
using Rock.Model;
using Rock.ViewModels.Reporting;
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
        /// <param name="personFieldValues">The person field values.</param>
        /// <returns></returns>
        public bool Evaluate( Dictionary<int, AttributeValueCache> attributeValues, Dictionary<RegistrationPersonFieldType, string> personFieldValues )
        {
            var fieldVisibilityRules = this;

            List<bool> conditionResults = new List<bool>();

            if ( !fieldVisibilityRules.RuleList.Any() || ( !attributeValues.Any() && !personFieldValues.Any() ) )
            {
                // if no rules or no values, just exit
                return true;
            }

            foreach ( var fieldVisibilityRule in fieldVisibilityRules.RuleList.Where( a => a.ComparedToFormFieldGuid.HasValue ) )
            {
                bool conditionResult;
                var filterValues = new List<string>();
                var comparedToField = RegistrationTemplateFormFieldCache.Get( fieldVisibilityRule.ComparedToFormFieldGuid.Value );
                var comparedToFieldAttributeId = comparedToField?.AttributeId ?? AttributeCache.Get( fieldVisibilityRule.ComparedToFormFieldGuid.Value )?.Id;

                if ( comparedToFieldAttributeId != null )
                {
                    var comparedToAttribute = AttributeCache.Get( comparedToFieldAttributeId.Value );

                    // if this is a TextFieldType, In-Memory LINQ is case-sensitive but LinqToSQL is not, so lets compare values using ToLower()
                    if ( comparedToAttribute.FieldType.Field is Rock.Field.Types.TextFieldType )
                    {
                        fieldVisibilityRule.ComparedToValue = fieldVisibilityRule.ComparedToValue?.ToLower();
                    }

                    var comparisonTypeValue = fieldVisibilityRule.ComparisonType.ConvertToString( false );
                    if ( comparisonTypeValue != null )
                    {
                        // only add the comparisonTypeValue if it is specified, just like the logic at https://github.com/SparkDevNetwork/Rock/blob/22f64416b2461c8a988faf4b6e556bc3dcb209d3/Rock/Field/FieldType.cs#L558
                        filterValues.Add( comparisonTypeValue );
                    }                    

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

                    conditionResult = conditionFunc.Invoke( attributeValueToEvaluate );
                }
                else if ( comparedToField != null && IsFieldSupported( comparedToField.PersonFieldType ) )
                {
                    var comparedToFieldValue = personFieldValues.GetValueOrNull( comparedToField.PersonFieldType );
                    conditionResult = comparedToFieldValue == fieldVisibilityRule.ComparedToValue;
                }
                else
                {
                    // ignore if not an attribute and not a supported field type
                    continue;
                }

                conditionResults.Add( conditionResult );
            }

            if ( !conditionResults.Any() )
            {
                // ended up not having any conditions, so return true
                return true;
            }

            bool visible;

            switch ( fieldVisibilityRules.FilterExpressionType )
            {
                case Rock.Model.FilterExpressionType.GroupAll:
                    {
                        // Show if all of the conditions are met (A && B && C && D)
                        visible = conditionResults.All( a => a == true );
                        break;
                    }
                case Rock.Model.FilterExpressionType.GroupAllFalse:
                    {
                        // Hide if all of the conditions are met (A && B && C && D)
                        visible = !conditionResults.All( a => a == true );
                        break;
                    }
                case Rock.Model.FilterExpressionType.GroupAny:
                    {
                        // Show if any of the conditions are met (A || B || C || D)
                        visible = conditionResults.Any( a => a == true );
                        break;
                    }
                case Rock.Model.FilterExpressionType.GroupAnyFalse:
                    {
                        // Hide if any of the conditions are met (A || B || C || D)
                        visible = !conditionResults.Any( a => a == true );
                        break;
                    }

                default:
                    {
                        // show if unexpected FilterExpressionType
                        visible = true;
                        break;
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
        internal string DebuggerFormattedRules
        {
            get
            {
                switch ( FilterExpressionType )
                {
                    case FilterExpressionType.GroupAll:
                        {
                            return $"Show if {this.RuleList.AsDelimited( " and " )}";
                        }
                    case FilterExpressionType.GroupAnyFalse:
                        {
                            return $"Hide if {this.RuleList.AsDelimited( " or " )}";
                        }
                    case FilterExpressionType.GroupAny:
                        {
                            return $"Show if {this.RuleList.AsDelimited( " or " )}";
                        }
                    case FilterExpressionType.GroupAllFalse:
                        {
                            return $"Hide if {this.RuleList.AsDelimited( " and " )}";
                        }
                    default:
                        {
                            // some unexpected FilterExpressionType
                            return $"{FilterExpressionType} {this.RuleList.AsDelimited( " and " ) }";
                        }
                }

            }
        }

        /// <summary>
        /// Determines if the registration field type is supported for conditional field visibility
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static bool IsFieldSupported( RegistrationPersonFieldType fieldType )
        {
            return GetSupportedFieldTypeCache( fieldType ) != null;
        }

        /// <summary>
        /// Gets the field type cache used for the given registration field type if it is supported
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static FieldTypeCache GetSupportedFieldTypeCache( RegistrationPersonFieldType fieldType )
        {
            switch ( fieldType )
            {
                case RegistrationPersonFieldType.Gender:
                    return FieldTypeCache.Get( SystemGuid.FieldType.GENDER );
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class FieldVisibilityRule
    {
        /// <summary>
        /// Gets or sets the compared to Form Field unique identifier.
        /// </summary>
        /// <value>
        /// The compared to form field unique identifier.
        /// </value>
        [DataMember]
        public Guid? ComparedToFormFieldGuid { get; set; }

        /// <summary>
        /// Obsolete. Use <see cref="ComparedToFormFieldGuid" /> instead.
        /// </summary>
        /// <value>The compared to registration template form field unique identifier.</value>
        [DataMember]
        [Obsolete( "Use ComparedToFormFieldGuid Instead" )]
        [RockObsolete( "12.5" )]
        public Guid? ComparedToRegistrationTemplateFormFieldGuid
        {
            /* 2021-10-06 MDP
              
               A Data Migration takes care of moving the data to the ComparedToFormFieldGuid field,
               so it'll be safe to remove the ComparedToRegistrationTemplateFormFieldGuid property
               when we delete the obsoleted ComparedToRegistrationTemplateFormFieldGuid field.

               We'll keep the ComparedToRegistrationTemplateFormFieldGuid, but mark it obsolete just in
               case plugin developers are using this field.
            */

            get => ComparedToFormFieldGuid;
            set => ComparedToFormFieldGuid = value;
        }

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
        /// Gets the public <see cref="FieldFilterRuleBag"/> that represents
        /// the values for an attribute filter. This is meant to be sent down
        /// to a client and will not be valid for server-side filtering.
        /// </summary>
        /// <param name="attribute">The attribute to be filtered, this is used for value conversion.</param>
        /// <param name="comparisonType">The type of comparison to be performed.</param>
        /// <param name="comparedToValue">The private database value to be compared against.</param>
        /// <returns>A new instance of <see cref="FieldFilterRuleBag"/> or <c>null</c> if the attribute was not valid.</returns>
        public static FieldFilterRuleBag GetPublicRuleBag( AttributeCache attribute, ComparisonType comparisonType, string comparedToValue )
        {
            var filterValues = new List<string>();
            var field = attribute?.FieldType?.Field;

            if ( field == null )
            {
                return null;
            }

            var comparisonTypeValue = comparisonType.ConvertToString( false );
            if ( comparisonTypeValue != null )
            {
                // only add the comparisonTypeValue if it is specified, just like
                // the logic at https://github.com/SparkDevNetwork/Rock/blob/22f64416b2461c8a988faf4b6e556bc3dcb209d3/Rock/Field/FieldType.cs#L558
                filterValues.Add( comparisonTypeValue );
            }

            filterValues.Add( comparedToValue );

            var comparisonValue = field.GetPublicFilterValue( filterValues.ToJson(), attribute.ConfigurationValues );

            return new FieldFilterRuleBag
            {
                ComparisonType = comparisonValue.ComparisonType ?? 0,
                Value = comparisonValue.Value,
                SourceType = Enums.Reporting.FieldFilterSourceType.Attribute,
                AttributeGuid = attribute.Guid
            };
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( ComparedToFormFieldGuid.HasValue )
            {
                var comparedToRegistrationTemplateField = RegistrationTemplateFormFieldCache.Get( this.ComparedToFormFieldGuid.Value );
                var comparedToWorkflowFormField = AttributeCache.Get( this.ComparedToFormFieldGuid.Value );

                if ( comparedToRegistrationTemplateField?.AttributeId.HasValue == true )
                {
                    var comparedToAttribute = AttributeCache.Get( comparedToRegistrationTemplateField.AttributeId.Value );
                    var filterValues = new List<string>( new string[2] { this.ComparisonType.ConvertToString(), this.ComparedToValue } );
                    return $"{comparedToAttribute?.Name} {comparedToAttribute?.FieldType.Field.FormatFilterValues( comparedToAttribute.QualifierValues, filterValues ) } ";
                }
                else if ( comparedToWorkflowFormField != null )
                {
                    var comparedToAttribute = AttributeCache.Get( comparedToWorkflowFormField.Id );
                    var filterValues = new List<string>( new string[2] { this.ComparisonType.ConvertToString(), this.ComparedToValue } );
                    return $"{comparedToAttribute?.Name} {comparedToAttribute?.FieldType.Field.FormatFilterValues( comparedToAttribute.QualifierValues, filterValues ) } ";
                }
                else if ( comparedToRegistrationTemplateField?.FieldSource == RegistrationFieldSource.PersonField )
                {
                    return $"{comparedToRegistrationTemplateField.PersonFieldType.ConvertToString()} is {ComparedToValue}";
                }
            }

            return base.ToString();
        }
    }
}
