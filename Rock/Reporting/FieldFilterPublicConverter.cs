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

using Rock.Enums.Reporting;
using Rock.ViewModels.Reporting;

namespace Rock.Reporting
{
    /// <summary>
    /// Handles conversion of <see cref="FieldFilterGroupBag"/> and
    /// <see cref="FieldFilterRuleBag"/> objects between public and private
    /// variants. This must be used when sending or receiving these objects
    /// from remote clients.
    /// </summary>
    internal class FieldFilterPublicConverter
    {
        /// <summary>
        /// A function that will be called for each rule to retrieve the
        /// <see cref="EntityField"/> for value conversion.
        /// </summary>
        private readonly Func<FieldFilterRuleBag, EntityField> _entityFieldProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldFilterPublicConverter"/> class.
        /// </summary>
        /// <param name="entityFieldProvider">A function that will be called for each rule to retrieve the <see cref="EntityField"/> for value conversion.</param>
        public FieldFilterPublicConverter( Func<FieldFilterRuleBag, EntityField> entityFieldProvider )
        {
            _entityFieldProvider = entityFieldProvider;
        }

        /// <summary>
        /// Converts a public <see cref="FieldFilterGroupBag"/> into a private
        /// bag. This returns the same type but will have converted all rule
        /// field values into the private values. The original object is not
        /// modified.
        /// </summary>
        /// <param name="publicBag">The public filter bag.</param>
        /// <returns>The private filter bag.</returns>
        public FieldFilterGroupBag ToPrivateBag( FieldFilterGroupBag publicBag )
        {
            if ( publicBag == null )
            {
                return null;
            }

            return new FieldFilterGroupBag
            {
                Guid = publicBag.Guid,
                ExpressionType = publicBag.ExpressionType,
                Rules = publicBag.Rules?.Select( r => ToPrivateBag( r ) ).ToList(),
                Groups = publicBag.Groups?.Select( g => ToPrivateBag( g ) ).ToList()
            };
        }

        /// <summary>
        /// Converts a public <see cref="FieldFilterRuleBag"/> into a private
        /// bag. This returns the same type but will have converted the rule
        /// field value into the private value. The original object is not
        /// modified.
        /// </summary>
        /// <param name="publicBag">The public filter bag.</param>
        /// <returns>The private filter bag.</returns>
        public FieldFilterRuleBag ToPrivateBag( FieldFilterRuleBag publicBag )
        {
            if ( publicBag == null )
            {
                return null;
            }

            var rule = new FieldFilterRuleBag
            {
                Guid = publicBag.Guid,
                ComparisonType = publicBag.ComparisonType,
                SourceType = publicBag.SourceType,
                Path = publicBag.Path
            };

            var entityField = _entityFieldProvider( publicBag );

            // If we couldn't find an entity field then we need to abort. Return
            // the rule as is which will leave it in an incomplete state without
            // the details of how to access an attribute or property. This way
            // it can't be used to back-door anything from a blank value.
            if ( entityField == null || entityField.FieldType == null )
            {
                return rule;
            }

            if ( publicBag.SourceType == FieldFilterSourceType.Attribute )
            {
                rule.AttributeGuid = publicBag.AttributeGuid;
            }
            else if ( publicBag.SourceType == FieldFilterSourceType.Property )
            {
                rule.PropertyName = publicBag.PropertyName;
            }

            // Convert the public value to a private value.
            var comparisonValue = new ComparisonValue
            {
                ComparisonType = rule.ComparisonType,
                Value = publicBag.Value
            };

            var fieldConfig = entityField.FieldConfig.ToDictionary( c => c.Key, c => c.Value.Value );
            var filterValues = entityField.FieldType.Field.GetPrivateFilterValue( comparisonValue, fieldConfig ).FromJsonOrNull<List<string>>();

            if ( filterValues != null && filterValues.Count == 2 )
            {
                rule.Value = filterValues[1];
            }
            else if ( filterValues != null && filterValues.Count == 1 )
            {
                rule.Value = filterValues[0];
            }

            return rule;
        }

        /// <summary>
        /// Converts a private <see cref="FieldFilterGroupBag"/> into a public
        /// bag. This returns the same type but will have converted all rule
        /// field values into the public values. The original object is not
        /// modified.
        /// </summary>
        /// <param name="privateBag">The private filter bag.</param>
        /// <returns>The public filter bag.</returns>
        public FieldFilterGroupBag ToPublicBag( FieldFilterGroupBag privateBag )
        {
            if ( privateBag == null )
            {
                return null;
            }

            return new FieldFilterGroupBag
            {
                Guid = privateBag.Guid,
                ExpressionType = privateBag.ExpressionType,
                Rules = privateBag.Rules?.Select( r => ToPublicBag( r ) ).ToList(),
                Groups = privateBag.Groups?.Select( g => ToPublicBag( g ) ).ToList()
            };
        }

        /// <summary>
        /// Converts a private <see cref="FieldFilterRuleBag"/> into a public
        /// bag. This returns the same type but will have converted the rule
        /// field value into the public value. The original object is not
        /// modified.
        /// </summary>
        /// <param name="privateBag">The private filter bag.</param>
        /// <returns>The public filter bag.</returns>
        public FieldFilterRuleBag ToPublicBag( FieldFilterRuleBag privateBag )
        {
            if ( privateBag == null )
            {
                return null;
            }

            var rule = new FieldFilterRuleBag
            {
                Guid = privateBag.Guid,
                ComparisonType = privateBag.ComparisonType,
                SourceType = privateBag.SourceType,
                Path = privateBag.Path
            };

            var entityField = _entityFieldProvider( privateBag );

            // If we couldn't find an entity field then we need to abort. Return
            // the rule as is which will leave it in an incomplete state without
            // the details of how to access an attribute or property. This way
            // it can't be used to back-door anything from a blank value.
            if ( entityField == null || entityField.FieldType == null )
            {
                return rule;
            }

            if ( privateBag.SourceType == FieldFilterSourceType.Attribute )
            {
                rule.AttributeGuid = privateBag.AttributeGuid;
            }
            else if ( privateBag.SourceType == FieldFilterSourceType.Property )
            {
                rule.PropertyName = privateBag.PropertyName;
            }

            // Convert the private value to a public value.
            var filterValues = new List<string>( 2 );
            var comparisonType = privateBag.ComparisonType.ConvertToString();

            if ( comparisonType.IsNotNullOrWhiteSpace() )
            {
                filterValues.Add( comparisonType );
            }

            filterValues.Add( privateBag.Value );

            var fieldConfig = entityField.FieldConfig.ToDictionary( c => c.Key, c => c.Value.Value );
            var comparisonValue = entityField.FieldType.Field.GetPublicFilterValue( filterValues.ToJson(), fieldConfig );

            rule.Value = comparisonValue?.Value;

            return rule;
        }
    }
}
