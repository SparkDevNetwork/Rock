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

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Reporting
{
    /// <summary>
    /// Resolves a report's field selections and builds the underlying entity query
    /// through <see cref="Report.GetQueryable"/>. This centralizes the report
    /// entity-type resolution, field classification, and query construction that the
    /// report consumers (the Obsidian grid builder and the headless AI agent report
    /// runner) would otherwise each duplicate, so the logic cannot drift between them.
    /// </summary>
    internal static class ReportQueryBuilder
    {
        /// <summary>
        /// The classified field selections and the resulting entity query for a report.
        /// Each selection dictionary is keyed by the one-based column index in report
        /// field <see cref="ReportField.ColumnOrder"/> order, which the caller can
        /// reproduce to associate a report field with its selection.
        /// </summary>
        internal sealed class ReportQuery
        {
            /// <summary>
            /// The report's entity type.
            /// </summary>
            public Type EntityType { get; set; }

            /// <summary>
            /// The entity query projecting the report's columns onto a runtime type.
            /// </summary>
            public IQueryable Queryable { get; set; }

            /// <summary>
            /// The selected entity property fields, keyed by column index.
            /// </summary>
            public Dictionary<int, EntityField> EntityFields { get; set; }

            /// <summary>
            /// The selected attributes, keyed by column index.
            /// </summary>
            public Dictionary<int, AttributeCache> Attributes { get; set; }

            /// <summary>
            /// The selected data select component fields, keyed by column index.
            /// </summary>
            public Dictionary<int, ReportField> Components { get; set; }
        }

        /// <summary>
        /// Classifies a report's fields and builds its entity query. A data select
        /// component whose entity type cannot be resolved throws, and
        /// <see cref="Report.GetQueryable"/> likewise throws when the component itself
        /// cannot be loaded, matching the behavior of the report grid
        /// (<see cref="ReportingHelper.BindGrid"/>).
        /// </summary>
        /// <param name="report">The report to build a query for.</param>
        /// <param name="rockContext">The context to query against.</param>
        /// <param name="useAttributePersistedTextValues">When <c>true</c>, attribute columns return their persisted display text instead of the raw stored value.</param>
        /// <param name="sortByFieldId">The <c>ReportField.Id</c> to sort by, or <c>null</c> to use the report's own saved sort.</param>
        /// <param name="sortDescending">When sorting by <paramref name="sortByFieldId"/>, whether to sort descending.</param>
        /// <returns>The classified selections and the entity query.</returns>
        /// <exception cref="RockReportException">The report has no resolvable entity type, or produced no query.</exception>
        /// <exception cref="RockReportFieldExpressionException">A data select component field's entity type cannot be resolved, or the requested sort field is not a sortable field of the report.</exception>
        internal static ReportQuery Create( Report report, RockContext rockContext, bool useAttributePersistedTextValues = false, int? sortByFieldId = null, bool sortDescending = false )
        {
            if ( report == null )
            {
                throw new ArgumentNullException( nameof( report ) );
            }

            if ( !report.EntityTypeId.HasValue )
            {
                throw new RockReportException( report, "The report has no entity type." );
            }

            var reportEntityType = EntityTypeCache.Get( report.EntityTypeId.Value, rockContext )?.GetEntityType();

            if ( reportEntityType == null )
            {
                throw new RockReportException( report, $"Unable to resolve the entity type for report '{report.Name}'." );
            }

            var entityFields = EntityHelper.GetEntityFields( reportEntityType );

            // Classify each report field into the selection dictionaries the report
            // query builder expects, keyed by column index in ColumnOrder order.
            var selectedEntityFields = new Dictionary<int, EntityField>();
            var selectedAttributes = new Dictionary<int, AttributeCache>();
            var selectedComponents = new Dictionary<int, ReportField>();

            // The sort binding token for the requested sort field, resolved once its
            // column index is known during classification.
            string sortExpression = null;

            var columnIndex = 0;

            foreach ( var reportField in report.ReportFields.OrderBy( f => f.ColumnOrder ) )
            {
                columnIndex++;

                if ( reportField.ReportFieldType == ReportFieldType.Property )
                {
                    var entityField = entityFields.FirstOrDefault( f => f.Name == reportField.Selection );

                    if ( entityField != null )
                    {
                        selectedEntityFields.Add( columnIndex, entityField );
                    }
                }
                else if ( reportField.ReportFieldType == ReportFieldType.Attribute )
                {
                    var attributeGuid = reportField.Selection.AsGuidOrNull();
                    var attribute = attributeGuid.HasValue ? AttributeCache.Get( attributeGuid.Value ) : null;

                    if ( attribute != null )
                    {
                        selectedAttributes.Add( columnIndex, attribute );
                    }
                }
                else if ( reportField.ReportFieldType == ReportFieldType.DataSelectComponent )
                {
                    // Report.GetQueryable reads the component entity type's name
                    // directly, so ensure the navigation is populated first.
                    if ( reportField.DataSelectComponentEntityType == null && reportField.DataSelectComponentEntityTypeId.HasValue )
                    {
                        reportField.DataSelectComponentEntityType = new EntityTypeService( rockContext ).Get( reportField.DataSelectComponentEntityTypeId.Value );
                    }

                    if ( reportField.DataSelectComponentEntityType == null )
                    {
                        throw new RockReportFieldExpressionException( reportField, $"Unable to determine the data select component entity type for report '{report.Name}'." );
                    }

                    selectedComponents.Add( columnIndex, reportField );
                }

                // Resolve the sort token for the requested field now that its column
                // index (and, for components, its loaded entity type) is known.
                if ( sortByFieldId.HasValue && reportField.Id == sortByFieldId.Value )
                {
                    sortExpression = GetSortExpression( reportField, columnIndex );

                    if ( sortExpression == null )
                    {
                        throw new RockReportFieldExpressionException( reportField, $"The requested sort field cannot be sorted on for report '{report.Name}'." );
                    }
                }
            }

            if ( sortByFieldId.HasValue && sortExpression == null )
            {
                throw new RockReportException( report, $"The requested sort field is not a field of report '{report.Name}'." );
            }

            var queryableArgs = new ReportGetQueryableArgs
            {
                ReportDbContext = rockContext,
                EntityFields = selectedEntityFields,
                Attributes = selectedAttributes,
                SelectComponents = selectedComponents,
                IsCommunication = false,
                UseAttributePersistedTextValues = useAttributePersistedTextValues
            };

            if ( sortExpression != null )
            {
                queryableArgs.SortProperty = new Rock.Web.UI.Controls.SortProperty
                {
                    Property = sortExpression,
                    Direction = sortDescending ? System.Web.UI.WebControls.SortDirection.Descending : System.Web.UI.WebControls.SortDirection.Ascending
                };
            }

            var queryable = report.GetQueryable( queryableArgs );

            if ( queryable == null )
            {
                throw new RockReportException( report, $"The report '{report.Name}' produced no query." );
            }

            return new ReportQuery
            {
                EntityType = reportEntityType,
                Queryable = queryable,
                EntityFields = selectedEntityFields,
                Attributes = selectedAttributes,
                Components = selectedComponents
            };
        }

        /// <summary>
        /// Determines whether a report field can be sorted on. Property and attribute
        /// fields always can; a data select component field can unless its component
        /// disables sorting (by returning <see cref="string.Empty"/> from
        /// <see cref="DataSelectComponent.SortProperties"/>) or cannot be resolved.
        /// </summary>
        /// <param name="field">The report field.</param>
        /// <returns><c>true</c> if the field can be sorted on.</returns>
        internal static bool IsFieldSortable( ReportField field )
        {
            // The column index does not affect sortability, only the token text, so
            // any index answers the question.
            return field != null && GetSortExpression( field, 0 ) != null;
        }

        /// <summary>
        /// Builds the sort binding token(s) for a report field at a given column
        /// index, matching the member names <see cref="Report.GetQueryable"/> binds on
        /// the runtime type. A data select component expands to its declared sort
        /// columns (which may be more than one). Returns <c>null</c> when the field
        /// cannot be sorted on.
        /// </summary>
        /// <param name="field">The report field.</param>
        /// <param name="columnIndex">The one-based column index of the field.</param>
        /// <returns>The comma-delimited sort token(s), or <c>null</c> if not sortable.</returns>
        private static string GetSortExpression( ReportField field, int columnIndex )
        {
            if ( field.ReportFieldType == ReportFieldType.Property )
            {
                // The property value binding is itself sortable; its name is the
                // property (the field's selection).
                return $"Entity_{field.Selection}_{columnIndex}";
            }

            if ( field.ReportFieldType == ReportFieldType.Attribute )
            {
                var attributeGuid = field.Selection.AsGuidOrNull();
                var attributeId = attributeGuid.HasValue ? AttributeCache.Get( attributeGuid.Value )?.Id : null;

                return attributeId.HasValue ? $"Attribute_{attributeId.Value}_{columnIndex}" : null;
            }

            if ( field.ReportFieldType == ReportFieldType.DataSelectComponent )
            {
                var component = GetDataSelectComponent( field );

                if ( component == null )
                {
                    return null;
                }

                var customSortProperties = component.SortProperties( field.Selection ?? string.Empty );

                // Empty string means the component disables sorting; null means sort
                // on the value column itself; otherwise sort on the declared columns.
                if ( customSortProperties == string.Empty )
                {
                    return null;
                }

                if ( customSortProperties == null )
                {
                    return $"Data_{component.ColumnPropertyName}_{columnIndex}";
                }

                return string.Join( ",", customSortProperties.Split( ',' ).Select( p => $"Sort_{p}_{columnIndex}" ) );
            }

            return null;
        }

        /// <summary>
        /// Resolves the data select component backing a report field, relying on the
        /// field's already-loaded component entity type navigation.
        /// </summary>
        /// <param name="field">The data select component report field.</param>
        /// <returns>The component, or <c>null</c> if it cannot be resolved.</returns>
        private static DataSelectComponent GetDataSelectComponent( ReportField field )
        {
            var entityTypeName = field.DataSelectComponentEntityType?.Name;

            return !string.IsNullOrWhiteSpace( entityTypeName ) ? DataSelectContainer.GetComponent( entityTypeName ) : null;
        }
    }
}
