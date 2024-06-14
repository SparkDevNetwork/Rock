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

            return new LabelDesignerOptionsBag
            {
                IdKey = label.IdKey,
                Label = GetLabelDetailBag( label ),
                LabelName = label.Name,
                LabelType = label.LabelType,
                DataSources = dataSources,
                FilterSources = filterSources,
                Icons = GetIconList()
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

        /// <summary>
        /// Gets the list of icons that are supported.
        /// </summary>
        /// <returns>A list of <see cref="IconItemBag"/> objects.</returns>
        private List<IconItemBag> GetIconList()
        {
            return new List<IconItemBag>
            {
                new IconItemBag
                {
                    Value = "birthday_cake",
                    Text = "Birthday Cake",
                    Weight = 900,
                    Code = "\uF1FD"
                },
                new IconItemBag
                {
                    Value = "star",
                    Text = "Star",
                    Weight = 900,
                    Code = "\uF005"
                }
            };
        }

        private LabelDetailBag GetLabelDetailBag( CheckInLabel checkInLabel )
        {
            var designedLabel = checkInLabel.Content.FromJsonOrNull<DesignedLabelBag>();

            // Ensure we have the required properties set.
            designedLabel = designedLabel ?? new DesignedLabelBag();
            designedLabel.Fields = designedLabel.Fields ?? new List<LabelFieldBag>();

            var converter = new FieldFilterPublicConverter( r => FieldSourceHelper.GetEntityFieldForRule( checkInLabel.LabelType, r ) );

            foreach ( var field in designedLabel.Fields )
            {
                field.ConditionalVisibility = converter.ToPublicBag( field.ConditionalVisibility );
            }

            return new LabelDetailBag
            {
                LabelData = designedLabel,
                ConditionalVisibility = converter.ToPublicBag( checkInLabel.GetConditionalPrintCriteria() )
            };
        }

        [BlockAction]
        public BlockActionResult Save( string key, LabelDetailBag label, string previewData )
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

            if ( label == null || label.LabelData == null )
            {
                return ActionBadRequest( "Invalid data provided." );
            }

            // Note: We don't do the ValidProperties<> boxing around the label
            // because it would get very complicated to deal with all the child
            // properties that are nested multiple layers deep. This is a special
            // purpose block that is tied closely to the Obsidian implementation
            // so other code should not just use the block actions and assume
            // they will work.

            var converter = new FieldFilterPublicConverter( r => FieldSourceHelper.GetEntityFieldForRule( checkInLabel.LabelType, r ) );

            if ( label.LabelData.Fields != null )
            {
                foreach ( var field in label.LabelData.Fields )
                {
                    field.ConditionalVisibility = converter.ToPrivateBag( field.ConditionalVisibility );
                }
            }

            // This should become a merge just in case something weird happens in Obsidian.
            checkInLabel.Content = label.LabelData.ToJson();

            checkInLabel.SetConditionalPrintCriteria( converter.ToPrivateBag( label.ConditionalVisibility ?? new FieldFilterGroupBag() ) );

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
