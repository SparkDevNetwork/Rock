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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Obsidian.UI.GridField;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;

namespace Rock.Reporting
{
    /// <summary>
    /// Renders a <see cref="Report"/> into the Obsidian Grid's definition and
    /// data bags, using the <see cref="ObsidianGridField"/> hierarchy for
    /// per-column rendering. The consumer (typically a Rock block) drives the
    /// call and owns the surrounding context: reading the ReportGuid from
    /// wherever it comes from, managing the <see cref="RockContext"/> lifetime,
    /// and wrapping the result in whatever wire bag it exposes to the client.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Two-phase materialization: for each row, eager fields (those whose
    /// <see cref="ObsidianGridField.ReadsPeerValues"/> is <c>false</c>) run
    /// first, accumulating their transformed outputs into a per-row RowValues
    /// dictionary; then late-binding fields (those whose ReadsPeerValues is
    /// <c>true</c>, e.g. <see cref="LavaObsidianGridField"/>) run in column
    /// order with RowValues populated, updating it after each so subsequent
    /// late-binding fields see prior outputs.
    /// </para>
    /// <para>
    /// Internal by design: the return shape commits Rock to a specific consumer
    /// model (the Obsidian ReportDetail block). Keeping it internal preserves
    /// the freedom to reshape that surface later. Plugins that need Report-to-
    /// grid rendering can request access through the standard channel.
    /// </para>
    /// </remarks>
    internal static class ObsidianReportGridBuilder
    {
        /// <summary>
        /// Runs a report end-to-end and produces both the grid definition and
        /// the grid data.
        /// </summary>
        /// <param name="report">The report to render.</param>
        /// <param name="rockContext">The RockContext to use for the report query and any per-row lookups.</param>
        /// <param name="requestContext">The RockRequestContext for the current request; passed to DataSelect components' <see cref="DataSelectComponent.GetObsidianGridField"/> calls.</param>
        /// <returns>The composed grid definition and data bags.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the report's entity type cannot be resolved or
        /// <see cref="Report.GetQueryable"/> returns null.
        /// </exception>
        internal static ObsidianGridResult Build( Report report, RockContext rockContext, RockRequestContext requestContext )
        {
            var reportEntityTypeCache = EntityTypeCache.Get( report.EntityTypeId.Value );
            var reportEntityType = reportEntityTypeCache?.GetEntityType();
            if ( reportEntityType == null )
            {
                throw new InvalidOperationException( $"Unable to resolve the entity type for report '{report.Name}'." );
            }

            var entityFieldsForEntity = EntityHelper.GetEntityFields( reportEntityType );

            var selectedEntityFields = new Dictionary<int, EntityField>();
            var selectedAttributes = new Dictionary<int, AttributeCache>();
            var selectedComponents = new Dictionary<int, ReportField>();

            var orderedReportFields = report.ReportFields.OrderBy( a => a.ColumnOrder ).ToList();
            var columnIndex = 0;

            foreach ( var reportField in orderedReportFields )
            {
                columnIndex++;

                if ( reportField.ReportFieldType == ReportFieldType.Property )
                {
                    var entityField = entityFieldsForEntity.FirstOrDefault( a => a.Name == reportField.Selection );
                    if ( entityField != null )
                    {
                        selectedEntityFields.Add( columnIndex, entityField );
                    }
                }
                else if ( reportField.ReportFieldType == ReportFieldType.Attribute )
                {
                    Guid.TryParse( reportField.Selection, out var attributeGuid );
                    var attribute = AttributeCache.Get( attributeGuid );
                    if ( attribute != null )
                    {
                        selectedAttributes.Add( columnIndex, attribute );
                    }
                }
                else if ( reportField.ReportFieldType == ReportFieldType.DataSelectComponent )
                {
                    selectedComponents.Add( columnIndex, reportField );
                }
            }

            var reportGetQueryableArgs = new ReportGetQueryableArgs
            {
                ReportDbContext = rockContext,
                EntityFields = selectedEntityFields,
                Attributes = selectedAttributes,
                SelectComponents = selectedComponents,
                IsCommunication = false
            };

            var queryable = report.GetQueryable( reportGetQueryableArgs );
            if ( queryable == null )
            {
                throw new InvalidOperationException( "Report.GetQueryable returned null." );
            }

            // Materialize the whole result set (client-side paging per accepted regression).
            var rowObjects = new List<object>();
            foreach ( var row in queryable )
            {
                rowObjects.Add( row );
            }

            // Reflect over the runtime dynamic type once; cache FieldInfo per name.
            var dynamicRowType = queryable.ElementType;
            var allFields = dynamicRowType.GetFields();
            var fieldInfoByName = allFields.ToDictionary( f => f.Name, StringComparer.OrdinalIgnoreCase );

            // Pre-pass: build a ColumnPlan per column that carries everything the
            // materialization loop needs — the ObsidianGridField, the source field
            // name on the dynamic type, the friendly merge key, the output camel
            // name, the sort field names, and the title. Also produce the
            // ObsidianGridColumnDescriptor list that gets shipped through context.
            var seenMergeKeys = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
            var plans = new List<ColumnPlan>();
            var descriptors = new List<ObsidianGridColumnDescriptor>();

            columnIndex = 0;
            foreach ( var reportField in orderedReportFields )
            {
                columnIndex++;

                var plan = BuildColumnPlan(
                    reportField,
                    columnIndex,
                    selectedEntityFields,
                    selectedAttributes,
                    selectedComponents,
                    fieldInfoByName,
                    rockContext,
                    requestContext,
                    reportEntityType,
                    seenMergeKeys );

                if ( plan == null )
                {
                    continue;
                }

                plans.Add( plan );
                descriptors.Add( new ObsidianGridColumnDescriptor( plan.MergeKey, plan.SourceFieldName, plan.Field ) );
            }

            var sharedCaches = new ConcurrentDictionary<Type, object>();

            var definition = new GridDefinitionBag
            {
                Fields = new List<FieldDefinitionBag>(),
                DynamicFields = new List<DynamicFieldDefinitionBag>(),
                AttributeFields = new List<AttributeFieldDefinitionBag>(),
                ActionUrls = new Dictionary<string, string>(),
                CustomColumns = new List<CustomColumnDefinitionBag>(),
                CustomActions = new List<CustomActionBag>()
            };

            var outputRows = new List<Dictionary<string, object>>( rowObjects.Count );
            foreach ( var _ in rowObjects )
            {
                outputRows.Add( new Dictionary<string, object>() );
            }

            var eagerPlans = plans.Where( p => !p.Field.ReadsPeerValues ).ToList();
            var latePlans = plans.Where( p => p.Field.ReadsPeerValues ).ToList();

            /*
                2026-08-12 - DH

                The per-row RowValues accumulator is only allocated when late-binding
                fields exist so eager-only reports pay nothing extra. The entity Id
                is pre-seeded so Lava templates can reference {{ Id }} the same way
                they could in WebForms; every Report is built from an IEntity
                queryable and Report.GetQueryable always projects Id as a field on
                the runtime dynamic type.

                Reason: WebForms LavaField.PopulateDataItemPropertiesDictionary
                exposes all row properties by name as a fallback; we deliberately
                skip the full fallback but keep Id since it is universal.
            */
            List<Dictionary<string, object>> rowValuesByRow = null;
            if ( latePlans.Count > 0 )
            {
                rowValuesByRow = new List<Dictionary<string, object>>( rowObjects.Count );
                var idFieldInfo = fieldInfoByName.TryGetValue( "id", out var fi ) ? fi : null;
                for ( int i = 0; i < rowObjects.Count; i++ )
                {
                    var dict = new Dictionary<string, object>();
                    if ( idFieldInfo != null )
                    {
                        dict["Id"] = idFieldInfo.GetValue( rowObjects[i] );
                    }
                    rowValuesByRow.Add( dict );
                }
            }

            // Phase 1: eager fields. Each field's TransformValue result is
            // added to RowValues (when late-binding fields exist downstream)
            // under its MergeKey so late-binding fields can read it.
            foreach ( var plan in eagerPlans )
            {
                ProjectColumn( plan, rowObjects, outputRows, rowValuesByRow, descriptors, rockContext, sharedCaches, definition, rowValuesForContext: null );
            }

            // Phase 2: late-binding fields, in column order. Each receives the
            // accumulated RowValues from Phase 1 (and prior Phase 2 fields).
            foreach ( var plan in latePlans )
            {
                ProjectColumn( plan, rowObjects, outputRows, rowValuesByRow, descriptors, rockContext, sharedCaches, definition, rowValuesForContext: rowValuesByRow );
            }

            return new ObsidianGridResult
            {
                Definition = definition,
                Data = new GridDataBag { Rows = outputRows }
            };
        }

        /// <summary>
        /// Wires a single column into the output rows and definition. When
        /// <paramref name="rowValuesForContext"/> is non-null the field runs in
        /// late-binding mode and can inspect peer transformed outputs via
        /// <see cref="ObsidianGridFieldContext.RowValues"/>.
        /// </summary>
        private static void ProjectColumn(
            ColumnPlan plan,
            List<object> rowObjects,
            List<Dictionary<string, object>> outputRows,
            List<Dictionary<string, object>> rowValuesByRow,
            IReadOnlyList<ObsidianGridColumnDescriptor> descriptors,
            RockContext rockContext,
            ConcurrentDictionary<Type, object> sharedCaches,
            GridDefinitionBag definition,
            List<Dictionary<string, object>> rowValuesForContext )
        {
            for ( int i = 0; i < rowObjects.Count; i++ )
            {
                var raw = plan.SourceFieldInfo?.GetValue( rowObjects[i] );

                var rowValuesForThisRow = rowValuesForContext?[i];
                var context = new ObsidianGridFieldContext( rockContext, rowObjects[i], descriptors, rowValuesForThisRow, sharedCaches );

                var transformed = plan.Field.TransformValue( raw, context );
                outputRows[i][plan.CamelName] = transformed;

                // Accumulate into RowValues so downstream late-binding fields
                // see this column's transformed output.
                if ( rowValuesByRow != null )
                {
                    rowValuesByRow[i][plan.MergeKey] = transformed;
                }

                var exportValue = plan.Field.GetExportValue( raw, context );
                if ( exportValue != null )
                {
                    outputRows[i][$"{plan.CamelName}__export"] = exportValue;
                }

                // Sort field projection (only meaningful for DataSelect columns
                // that declare SortProperties).
                if ( plan.SortSourceFieldsByOutKey != null )
                {
                    foreach ( var kvp in plan.SortSourceFieldsByOutKey )
                    {
                        outputRows[i][kvp.Key] = kvp.Value.GetValue( rowObjects[i] );
                    }
                }
            }

            var bag = plan.Field.GetDefinitionBag();
            bag.Name = plan.CamelName;
            bag.Title = plan.Title;
            bag.HideOnScreen = !plan.ShowInGrid;
            if ( plan.SortFieldNames != null && plan.SortFieldNames.Count > 0 )
            {
                bag.SortFields = plan.SortFieldNames;
            }
            definition.DynamicFields.Add( bag );
        }

        /// <summary>
        /// Resolves a single ReportField into a ColumnPlan describing everything
        /// the materialization loop needs. Returns null when the ReportField
        /// cannot be resolved (unknown component, missing entity field, etc.).
        /// </summary>
        private static ColumnPlan BuildColumnPlan(
            ReportField reportField,
            int columnIndex,
            Dictionary<int, EntityField> selectedEntityFields,
            Dictionary<int, AttributeCache> selectedAttributes,
            Dictionary<int, ReportField> selectedComponents,
            Dictionary<string, FieldInfo> fieldInfoByName,
            RockContext rockContext,
            RockRequestContext requestContext,
            Type reportEntityType,
            HashSet<string> seenMergeKeys )
        {
            var titleFallback = reportField.ColumnHeaderText;

            string sourceFieldName;
            string camelName;
            string title;
            ObsidianGridField field;
            List<string> sortFieldNames = null;
            Dictionary<string, FieldInfo> sortSourceByOutKey = null;
            string mergeKeyFallback;

            if ( reportField.ReportFieldType == ReportFieldType.Property
                && selectedEntityFields.TryGetValue( columnIndex, out var entityField ) )
            {
                sourceFieldName = $"entity_{entityField.Name}_{columnIndex}";
                camelName = ToCamel( entityField.Name );
                title = titleFallback.IsNullOrWhiteSpace() ? entityField.Title : titleFallback;
                field = MapEntityFieldToObsidianGridField( entityField );
                mergeKeyFallback = entityField.Name;
            }
            else if ( reportField.ReportFieldType == ReportFieldType.Attribute
                && selectedAttributes.TryGetValue( columnIndex, out var attribute ) )
            {
                sourceFieldName = $"attribute_{attribute.Id}_{columnIndex}";
                camelName = ToCamel( $"attr_{attribute.Key}_{columnIndex}" );
                title = titleFallback.IsNullOrWhiteSpace() ? attribute.Name : titleFallback;
                field = MapAttributeToObsidianGridField( attribute );
                mergeKeyFallback = attribute.Key;
            }
            else if ( reportField.ReportFieldType == ReportFieldType.DataSelectComponent
                && selectedComponents.ContainsKey( columnIndex ) )
            {
                var component = DataSelectContainer.GetComponent( reportField.DataSelectComponentEntityType.Name );
                if ( component == null )
                {
                    return null;
                }

                field = component.GetObsidianGridField(
                    reportEntityType,
                    reportField.Selection ?? string.Empty,
                    rockContext,
                    requestContext );

                sourceFieldName = $"data_{component.ColumnPropertyName}_{columnIndex}";
                camelName = ToCamel( $"{component.ColumnPropertyName}_{columnIndex}" );
                title = titleFallback.IsNullOrWhiteSpace() ? component.ColumnHeaderText : titleFallback;
                mergeKeyFallback = component.ColumnPropertyName;

                // Sort field projection: for each declared SortProperties entry,
                // pre-resolve the FieldInfo pointing at its Sort_* backing field.
                var sortProperties = component.SortProperties( reportField.Selection );
                if ( !string.IsNullOrEmpty( sortProperties ) )
                {
                    sortFieldNames = new List<string>();
                    sortSourceByOutKey = new Dictionary<string, FieldInfo>();
                    foreach ( var sortProperty in sortProperties.Split( ',' ) )
                    {
                        if ( string.IsNullOrWhiteSpace( sortProperty ) )
                        {
                            continue;
                        }

                        var sourceKey = $"sort_{sortProperty}_{columnIndex}";
                        var outKey = $"{camelName}__sort_{sortProperty}";

                        if ( fieldInfoByName.TryGetValue( sourceKey, out var fi ) )
                        {
                            sortSourceByOutKey[outKey] = fi;
                            sortFieldNames.Add( outKey );
                        }
                    }
                }
            }
            else
            {
                return null;
            }

            var mergeKey = BuildMergeKey( reportField.ColumnHeaderText, mergeKeyFallback );
            if ( string.IsNullOrEmpty( mergeKey ) )
            {
                return null;
            }

            // WebForms LavaField uses first-wins on ColumnHeaderText collisions.
            if ( !seenMergeKeys.Add( mergeKey ) )
            {
                return null;
            }

            fieldInfoByName.TryGetValue( sourceFieldName, out var sourceFieldInfo );

            return new ColumnPlan
            {
                Field = field,
                MergeKey = mergeKey,
                CamelName = camelName,
                Title = title,
                ShowInGrid = reportField.ShowInGrid,
                SourceFieldName = sourceFieldName,
                SourceFieldInfo = sourceFieldInfo,
                SortFieldNames = sortFieldNames,
                SortSourceFieldsByOutKey = sortSourceByOutKey
            };
        }

        /// <summary>
        /// Maps a raw entity property field type to the appropriate root-tier
        /// ObsidianGridField.
        /// </summary>
        private static ObsidianGridField MapEntityFieldToObsidianGridField( EntityField entityField )
        {
            var t = Nullable.GetUnderlyingType( entityField.PropertyType ) ?? entityField.PropertyType;

            if ( t == typeof( bool ) )
            {
                return new BooleanObsidianGridField();
            }

            if ( t == typeof( decimal ) )
            {
                return new CurrencyObsidianGridField();
            }

            if ( t == typeof( DateTime ) )
            {
                return new DateTimeObsidianGridField();
            }

            if ( t == typeof( int )
                 || t == typeof( long )
                 || t == typeof( double )
                 || t == typeof( float ) )
            {
                return new NumberObsidianGridField();
            }

            return new TextObsidianGridField();
        }

        /// <summary>
        /// Maps a raw attribute to an ObsidianGridField based on its field type
        /// guid.
        /// </summary>
        private static ObsidianGridField MapAttributeToObsidianGridField( AttributeCache attribute )
        {
            var ftGuid = attribute.FieldType?.Guid ?? Guid.Empty;

            if ( ftGuid == Rock.SystemGuid.FieldType.BOOLEAN.AsGuid() )
            {
                return new BooleanObsidianGridField();
            }

            if ( ftGuid == Rock.SystemGuid.FieldType.DATE.AsGuid() )
            {
                return new DateObsidianGridField();
            }

            if ( ftGuid == Rock.SystemGuid.FieldType.DATE_TIME.AsGuid() )
            {
                return new DateTimeObsidianGridField();
            }

            if ( ftGuid == Rock.SystemGuid.FieldType.INTEGER.AsGuid() )
            {
                return new NumberObsidianGridField();
            }

            if ( ftGuid == Rock.SystemGuid.FieldType.CURRENCY.AsGuid() )
            {
                return new CurrencyObsidianGridField();
            }

            if ( ftGuid == Rock.SystemGuid.FieldType.DEFINED_VALUE.AsGuid() )
            {
                return new LabelObsidianGridField { LabelType = "info" };
            }

            return new TextObsidianGridField();
        }

        private static string ToCamel( string name )
        {
            if ( string.IsNullOrEmpty( name ) )
            {
                return name;
            }

            return char.ToLowerInvariant( name[0] ) + name.Substring( 1 );
        }

        /// <summary>
        /// Builds the WebForms-parity friendly merge key for a report column.
        /// Prefers the caller's ColumnHeaderText (spaces + special characters
        /// removed) when present; otherwise uses the supplied fallback name.
        /// </summary>
        private static string BuildMergeKey( string headerText, string fallback )
        {
            if ( !string.IsNullOrWhiteSpace( headerText ) )
            {
                return headerText.Replace( " ", string.Empty ).RemoveSpecialCharacters();
            }
            return fallback;
        }

        /// <summary>
        /// Per-column plan carried from the pre-pass to the materialization
        /// loops. Bundles everything needed to project one column so
        /// <see cref="ProjectColumn"/> can operate uniformly on any column shape.
        /// </summary>
        private class ColumnPlan
        {
            public ObsidianGridField Field { get; set; }
            public string MergeKey { get; set; }
            public string CamelName { get; set; }
            public string Title { get; set; }
            public bool ShowInGrid { get; set; }
            public string SourceFieldName { get; set; }
            public FieldInfo SourceFieldInfo { get; set; }
            public List<string> SortFieldNames { get; set; }
            public Dictionary<string, FieldInfo> SortSourceFieldsByOutKey { get; set; }
        }
    }

    /// <summary>
    /// Bundles the two bags produced by <see cref="ObsidianReportGridBuilder"/>.
    /// </summary>
    internal class ObsidianGridResult
    {
        /// <summary>The grid's column definitions.</summary>
        public GridDefinitionBag Definition { get; set; }

        /// <summary>The grid's row data.</summary>
        public GridDataBag Data { get; set; }
    }
}
