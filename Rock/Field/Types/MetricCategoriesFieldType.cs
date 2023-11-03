﻿// <copyright>
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
using System.Data.Entity;
using System.Linq;
#if WEBFORMS
using System.Web.UI;
#endif

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Stored as a List of Metric.Guid|MetricCategory.Guid (MetricCategory.Guid included so we can preserve which category the metric was selected from)
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.METRIC_CATEGORIES )]
    public class MetricCategoriesFieldType : FieldType, IEntityReferenceFieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> configurationValues )
        {
            if ( string.IsNullOrWhiteSpace( value ) )
            {
                return string.Empty;
            }

            var guidPairs = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs( value );
            if ( !guidPairs.Any() )
            {
                return string.Empty;
            }

            var metricGuids = guidPairs.Select( a => a.MetricGuid );

            using ( var rockContext = new RockContext() )
            {
                var metrics = new MetricService( rockContext ).Queryable().AsNoTracking().Where( a => metricGuids.Contains( a.Guid ) );
                if ( metrics.Any() )
                {
                    return string.Join( ", ", metrics.Select( m => m.Title ) );
                }
            }

            return string.Empty;
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var jsonValue = publicValue.FromJsonOrNull<List<ListItemBag>>();

            if ( jsonValue != null )
            {
                var guids = jsonValue.ConvertAll( l => l.Value.AsGuid() );
                using ( var rockContext = new RockContext() )
                {
                    var guidPairList = new MetricCategoryService( rockContext ).GetByGuids( guids )
                        .Select( mc => new
                        {
                            MetricGuid = mc.Metric.Guid,
                            CategoryGuid = mc.Category.Guid,
                        } ).ToList();

                    if ( guidPairList.Any() )
                    {
                        return guidPairList.ConvertAll( s => string.Format( "{0}|{1}", s.MetricGuid, s.CategoryGuid ) )
                            .AsDelimited( "," );
                    }
                }
            }

            return base.GetPrivateEditValue( publicValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var metricCategories = new List<ListItemBag>();
            var guidPairs = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs( privateValue );

            using ( var rockContext = new RockContext() )
            {
                var metricCategoryService = new MetricCategoryService( new RockContext() );

                foreach ( var guidPair in guidPairs )
                {
                    // first try to get each metric from the category that it was selected from
                    var metricCategory = metricCategoryService.Queryable()
                        .Where( a => a.Metric.Guid == guidPair.MetricGuid && a.Category.Guid == guidPair.CategoryGuid )
                        .Select( a => new ListItemBag()
                        {
                            Text = a.Metric.Title,
                            Value = a.Guid.ToString()
                        } )
                        .FirstOrDefault();

                    if ( metricCategory == null )
                    {
                        // if the metric isn't found in the original category, just the first one, ignoring category
                        metricCategory = metricCategoryService.Queryable()
                            .Where( a => a.Metric.Guid == guidPair.MetricGuid )
                            .Select( a => new ListItemBag()
                            {
                                Text = a.Metric.Title,
                                Value = a.Guid.ToString()
                            } )
                            .FirstOrDefault();
                    }

                    if ( metricCategory != null )
                    {
                        metricCategories.Add( metricCategory );
                    }
                }

                return metricCategories.ToCamelCaseJson( false, true );
            }
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guidPairs = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs( privateValue );

            if ( !guidPairs.Any() )
            {
                return null;
            }
            var metricGuids = guidPairs.Select( a => a.MetricGuid );

            using ( var rockContext = new RockContext() )
            {
                var referencedEntities = new MetricService( rockContext )
                                    .Queryable()
                                    .AsNoTracking()
                                    .Where( m => metricGuids.Contains( m.Guid ) )
                                    .Select( m => m.Id )
                                    .ToList()
                                    .Select( m => new ReferencedEntity( EntityTypeCache.GetId<Metric>().Value, m ) );
                if ( !referencedEntities.Any() )
                {
                    return null;
                }
                return referencedEntities.ToList();
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a Metric and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Metric>().Value, nameof( Metric.Title ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) );
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new MetricCategoryPicker { ID = id, AllowMultiSelect = true };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as MetricCategoryPicker;
            string result = string.Empty;

            if ( picker != null )
            {
                var ids = picker.SelectedValuesAsInt();
                using ( var rockContext = new RockContext() )
                {
                    var metricCategories = new MetricCategoryService( rockContext ).Queryable().AsNoTracking().Where( a => ids.Contains( a.Id ) );

                    if ( metricCategories.Any() )
                    {
                        var guidPairList = metricCategories.Select( a => new { MetricGuid = a.Metric.Guid, CategoryGuid = a.Category.Guid } ).ToList();
                        result = guidPairList.Select( s => string.Format( "{0}|{1}", s.MetricGuid, s.CategoryGuid ) ).ToList().AsDelimited( "," );
                    }
                }

                return result;
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as MetricCategoryPicker;

            if ( picker != null )
            {
                List<MetricCategory> metricCategories = new List<MetricCategory>();
                var guidPairs = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs( value );
                MetricCategoryService metricCategoryService = new MetricCategoryService( new RockContext() );

                foreach ( var guidPair in guidPairs )
                {
                    // first try to get each metric from the category that it was selected from
                    var metricCategory = metricCategoryService.Queryable().Where( a => a.Metric.Guid == guidPair.MetricGuid && a.Category.Guid == guidPair.CategoryGuid ).FirstOrDefault();
                    if ( metricCategory == null )
                    {
                        // if the metric isn't found in the original category, just the first one, ignoring category
                        metricCategory = metricCategoryService.Queryable().Where( a => a.Metric.Guid == guidPair.MetricGuid ).FirstOrDefault();
                    }

                    if ( metricCategory != null )
                    {
                        metricCategories.Add( metricCategory );
                    }
                }

                picker.SetValues( metricCategories );
            }
        }

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            // This field type does not support filtering
            return null;
        }

#endif
        #endregion
    }
}
