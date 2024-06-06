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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.CheckIn.v2.Labels;
using Rock.Enums.CheckIn.Labels;
using Rock.Model;
using Rock.Reporting;
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
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "3f477b52-6062-4af4-abb7-b8c153f6242a" )]
    [Rock.SystemGuid.BlockTypeGuid( "8c4ad18f-9f81-4145-8ad0-ab90e451d0d6" )]
    public class LabelDesigner : RockBlockType
    {
        public override object GetObsidianBlockInitialization()
        {
            var dataSources = FieldSourceHelper.GetFamilyLabelDataSources()
                .Select( ds => ToDataSourceBag( ds ) )
                .OrderByDescending( ds => ds.Category == "Common" )
                .ThenByDescending( ds => ds.Category.Contains( "Properties" ) )
                .ThenBy( ds => ds.Category )
                .ThenBy( ds => ds.Name )
                .ToList();

            var filterSources = FieldSourceHelper.GetPersonLabelFilterSources()
                .OrderByDescending( s => s.Category == "Common" )
                .ThenByDescending( s => s.Category.Contains( "Properties" ) )
                .ThenBy( s => s.Category )
                .ThenBy( s => s.Property?.Title ?? s.Attribute?.Name )
                .ToList();

            return new
            {
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

        /// <summary>
        /// Creates a <see cref="FieldFilterGroup"/> object from its view
        /// model representation.
        /// </summary>
        /// <param name="viewModel">The view model that represents the object.</param>
        /// <returns>The object created from the view model.</returns>
        private static FieldFilterGroup FromViewModel( FieldFilterGroupBag viewModel )
        {
            return new FieldFilterGroup
            {
                FilterExpressionType = viewModel.ExpressionType,
                Rules = viewModel.Rules.Select( FromViewModel ).ToList()
            };
        }

        /// <summary>
        /// Creates a <see cref="FieldFilterRule"/> object from its view
        /// model representation.
        /// </summary>
        /// <param name="viewModel">The view model that represents the object.</param>
        /// <returns>The object created from the view model.</returns>
        private static FieldFilterRule FromViewModel( FieldFilterRuleBag viewModel )
        {
            var rule = new FieldFilterRule
            {
                Guid = viewModel.Guid,
                ComparisonType = viewModel.ComparisonType,
            };

            var comparisonValue = new ComparisonValue
            {
                ComparisonType = rule.ComparisonType,
                Value = viewModel.Value
            };

            if ( viewModel.AttributeGuid.HasValue )
            {
                rule.AttributeGuid = viewModel.AttributeGuid;

                var attribute = AttributeCache.Get( rule.AttributeGuid.Value );

                if ( attribute?.FieldType?.Field != null )
                {
                    var filterValues = attribute.FieldType.Field.GetPrivateFilterValue( comparisonValue, attribute.ConfigurationValues ).FromJsonOrNull<List<string>>();

                    if ( filterValues != null && filterValues.Count == 2 )
                    {
                        rule.ComparedToValue = filterValues[1];
                    }
                    else if ( filterValues != null && filterValues.Count == 1 )
                    {
                        rule.ComparedToValue = filterValues[0];
                    }
                }
            }
            else if ( viewModel.PropertyName.IsNotNullOrWhiteSpace() )
            {
                rule.PropertyName = viewModel.PropertyName;

                var field = EntityHelper.GetEntityField( typeof( Person ), EntityHelper.MakePropertyNameUnique( rule.PropertyName ) );

                if ( field != null )
                {
                    var filterValues = field.FieldType.Field.GetPrivateFilterValue( comparisonValue, field.FieldConfig.ToDictionary( c => c.Key, c => c.Value.Value ) ).FromJsonOrNull<List<string>>();

                    if ( filterValues != null && filterValues.Count == 2 )
                    {
                        rule.ComparedToValue = filterValues[1];
                    }
                    else if ( filterValues != null && filterValues.Count == 1 )
                    {
                        rule.ComparedToValue = filterValues[0];
                    }
                }
            }

            return rule;
        }

        [BlockAction]
        public BlockActionResult DoFilter( FieldFilterGroupBag filter )
        {
            var f = FromViewModel( filter );

            var ted = new Person
            {
                FirstName = "Ted",
                LastName = "Decker"
            };

            ted.LoadAttributes();
            ted.SetAttributeValue( "Allergy", "nuts" );

            var result = f.Evaluate( ted );

            return ActionOk( result );
        }
    }
}
