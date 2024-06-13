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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Rock.Attribute;
using Rock.CheckIn.v2.Labels;
using Rock.Data;
using Rock.Enums.CheckIn.Labels;
using Rock.Enums.Reporting;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.CheckIn.Configuration.LabelDesigner;
using Rock.ViewModels.CheckIn.Labels;
using Rock.ViewModels.Reporting;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn.Configuration
{
    /// <summary>
    /// Designs a check-in label with a nice drag and drop experience.
    /// </summary>

    [DisplayName( "Label Designer" )]
    [Category( "Check-in > Configuration" )]
    [Description( "Designs a check-in label with a nice drag and drop experience." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "3f477b52-6062-4af4-abb7-b8c153f6242a" )]
    [Rock.SystemGuid.BlockTypeGuid( "8c4ad18f-9f81-4145-8ad0-ab90e451d0d6" )]
    public class LabelDesigner : RockBlockType
    {
        private static class PageParameterKey
        {
            public const string CheckInLabelId = "CheckInLabelId";
        }

        public override object GetObsidianBlockInitialization()
        {
            var label = new CheckInLabelService( RockContext ).Get( PageParameter( PageParameterKey.CheckInLabelId ), !RequestContext.Page.Layout.Site.DisablePredictableIds );

            if ( label == null )
            {
                return new { };
            }

            var dataSources = FieldSourceHelper.GetDataSources( label?.LabelType ?? LabelType.Family )
                .Select( ds => ToDataSourceBag( ds ) )
                .OrderByDescending( ds => ds.Category == "Common" )
                .ThenByDescending( ds => ds.Category.Contains( "Properties" ) )
                .ThenBy( ds => ds.Category )
                .ThenBy( ds => ds.Name )
                .ToList();

            var filterSources = FieldSourceHelper.GetFilterSources( label?.LabelType ?? LabelType.Family )
                .OrderByDescending( s => s.Category == "Common" )
                .ThenBy( s => s.Category )
                .ThenBy( s => s.Property?.Title ?? s.Attribute?.Name )
                .ToList();

            var designedLabel = label.Content.FromJsonOrNull<DesignedLabelBag>();

            if ( designedLabel?.Fields != null )
            {
                foreach ( var field in designedLabel.Fields )
                {
                    field.ConditionalVisibility = ToPublicBag( field.ConditionalVisibility, r => FieldSourceHelper.GetEntityFieldForRule( label.LabelType, r ) );
                }
            }

            return new
            {
                IdKey = label.IdKey,
                Label = designedLabel,
                LabelName = label.Name,
                LabelType = label.LabelType,
                DataSources = dataSources,
                FilterSources = filterSources,
            };
        }

        private DataSourceBag ToDataSourceBag( FieldDataSource dataSource )
        {
            return new DataSourceBag
            {
                Key = dataSource.Key,
                Name = dataSource.Name,
                TextSubType = dataSource.TextSubType,
                IsCollection = dataSource.IsCollection,
                Category = dataSource.Category,
                CustomFields = dataSource.Formatter?.CustomFields,
                FormatterOptions = dataSource.Formatter?.Options ?? new List<DataFormatterOptionBag>()
            };
        }

        #region Extension Methods

        /// <summary>
        /// Converts a public <see cref="FieldFilterGroupBag"/> into a private
        /// bag. This returns the same type but will have converted all rule
        /// field values into the private values. The original object is not
        /// modified.
        /// </summary>
        /// <param name="publicBag">The public filter bag.</param>
        /// <param name="entityFieldProvider">A function that will be called for each rule to retrieve the <see cref="EntityField"/> for value conversion.</param>
        /// <returns>The private filter bag.</returns>
        private static FieldFilterGroupBag ToPrivateBag( FieldFilterGroupBag publicBag, Func<FieldFilterRuleBag, EntityField> entityFieldProvider )
        {
            if ( publicBag == null )
            {
                return null;
            }

            return new FieldFilterGroupBag
            {
                Guid = publicBag.Guid,
                ExpressionType = publicBag.ExpressionType,
                Rules = publicBag.Rules?.Select( r => ToPrivateBag( r, entityFieldProvider ) ).ToList(),
                Groups = publicBag.Groups?.Select( g => ToPrivateBag( g, entityFieldProvider ) ).ToList()
            };
        }

        /// <summary>
        /// Converts a public <see cref="FieldFilterRuleBag"/> into a private
        /// bag. This returns the same type but will have converted the rule
        /// field value into the private value. The original object is not
        /// modified.
        /// </summary>
        /// <param name="publicBag">The public filter bag.</param>
        /// <param name="entityFieldProvider">A function that will be called for each rule to retrieve the <see cref="EntityField"/> for value conversion.</param>
        /// <returns>The private filter bag.</returns>
        private static FieldFilterRuleBag ToPrivateBag( FieldFilterRuleBag publicBag, Func<FieldFilterRuleBag, EntityField> entityFieldProvider )
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

            var entityField = entityFieldProvider( publicBag );

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
        /// <param name="entityFieldProvider">A function that will be called for each rule to retrieve the <see cref="EntityField"/> for value conversion.</param>
        /// <returns>The public filter bag.</returns>
        private static FieldFilterGroupBag ToPublicBag( FieldFilterGroupBag privateBag, Func<FieldFilterRuleBag, EntityField> entityFieldProvider )
        {
            if ( privateBag == null )
            {
                return null;
            }

            return new FieldFilterGroupBag
            {
                Guid = privateBag.Guid,
                ExpressionType = privateBag.ExpressionType,
                Rules = privateBag.Rules?.Select( r => ToPublicBag( r, entityFieldProvider ) ).ToList(),
                Groups = privateBag.Groups?.Select( g => ToPublicBag( g, entityFieldProvider ) ).ToList()
            };
        }

        /// <summary>
        /// Converts a private <see cref="FieldFilterRuleBag"/> into a public
        /// bag. This returns the same type but will have converted the rule
        /// field value into the public value. The original object is not
        /// modified.
        /// </summary>
        /// <param name="privateBag">The private filter bag.</param>
        /// <param name="entityFieldProvider">A function that will be called for each rule to retrieve the <see cref="EntityField"/> for value conversion.</param>
        /// <returns>The public filter bag.</returns>
        private static FieldFilterRuleBag ToPublicBag( FieldFilterRuleBag privateBag, Func<FieldFilterRuleBag, EntityField> entityFieldProvider )
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

            var entityField = entityFieldProvider( privateBag );

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

        #endregion

        [BlockAction]
        public BlockActionResult Save( string key, DesignedLabelBag label, string previewData )
        {
            var entityService = new CheckInLabelService( RockContext );
            var checkInLabel = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( checkInLabel == null )
            {
                return ActionBadRequest( $"{CheckInLabel.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit ${CheckInLabel.FriendlyTypeName}." );
            }

            if ( label.Fields != null )
            {
                foreach ( var field in label.Fields )
                {
                    field.ConditionalVisibility = ToPrivateBag( field.ConditionalVisibility, r => FieldSourceHelper.GetEntityFieldForRule( checkInLabel.LabelType, r ) );
                }
            }

            checkInLabel.Content = label.ToJson();

            if ( previewData.IsNotNullOrWhiteSpace() )
            {
                checkInLabel.PreviewImage = Convert.FromBase64String( previewData );
            }
            else
            {
                checkInLabel.PreviewImage = new byte[0];
            }

            RockContext.SaveChanges();

            var returnUrl = this.GetParentPageUrl( new Dictionary<string, string>
            {
                [PageParameterKey.CheckInLabelId] = checkInLabel.IdKey
            } );

            return ActionOk( returnUrl );
        }

        private class CustomFieldFilterBuilder : FieldFilterExpressionBuilder
        {
            /// <summary>
            /// The MethodInfo that describes the Enumerable.Any method taking
            /// a <see cref="string"/> as a generic type.
            /// </summary>
            private static readonly Lazy<MethodInfo> _anyStringMethod = new Lazy<MethodInfo>( () => typeof( Enumerable )
                .GetMethods()
                .Where( m => m.Name == nameof( Enumerable.Any )
                    && m.GetParameters().Length == 2
                    && m.GetParameters()[1].ParameterType.IsGenericType
                    && m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof( Func<,> ) )
                .FirstOrDefault()
                .MakeGenericMethod( typeof( string ) ) );

            /// <inheritdoc/>
            protected override Expression GetRulePropertyExpression( Expression instanceExpression, FieldFilterRuleBag rule, RockContext rockContext )
            {
                // If the instance is an entity, then we don't need to do
                // anything special. We should never be called with a RockContext
                // but bail out just in case.
                if ( typeof( Data.IEntity ).IsAssignableFrom( instanceExpression.Type ) || rockContext != null )
                {
                    return base.GetRulePropertyExpression( instanceExpression, rule, rockContext );
                }

                var property = instanceExpression.Type.GetProperty( rule.PropertyName );

                // If property was not found, return an expression that
                // never matches.
                if ( property == null )
                {
                    return Expression.Constant( false );
                }

                if ( typeof( ICollection<string> ).IsAssignableFrom( property.PropertyType ) )
                {
                    // If the property is a collection of strings, then we want
                    // to run the normal text expression on all strings in the
                    // collection and return true if any of them match. We also
                    // need to convert everything to lower case so so that the
                    // comparisons happen case-insensitive.
                    var filterValues = new List<string>
                    {
                        rule.ComparisonType.ConvertToString( false ),
                        rule.Value?.ToLower()
                    };

                    var propertyExpression = Expression.Property( instanceExpression, property );

                    // Create an expression for prop.Any( (s) => s.ToLower() == value ) and then pass
                    // that value to the property filter expression.
                    var innerParameterExpression = Expression.Parameter( typeof( string ), "s" );
                    var lowerStringExpression = Expression.Call( innerParameterExpression, nameof( string.ToLower ), Type.EmptyTypes );
                    var propertyFilterExpression = ExpressionHelper.PropertyFilterExpression( filterValues, lowerStringExpression );
                    var propertyFilterFunc = Expression.Lambda<Func<string, bool>>( propertyFilterExpression, innerParameterExpression );

                    return Expression.Call( _anyStringMethod.Value, propertyExpression, propertyFilterFunc );
                }

                return base.GetRulePropertyExpression( instanceExpression, rule, rockContext );
            }
        }
    }
}
