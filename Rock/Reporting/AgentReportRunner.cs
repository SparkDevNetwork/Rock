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

using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Reporting
{
    /// <summary>
    /// The System.Web-free result of running a report through
    /// <see cref="AgentReportRunner"/>. It carries only BCL types so the agent
    /// assembly can consume it without referencing System.Web.
    /// </summary>
    internal sealed class AgentReportResult
    {
        /// <summary>
        /// The report rows, in the order the report produced them.
        /// </summary>
        internal List<AgentReportRow> Rows { get; set; }
    }

    /// <summary>
    /// A single row of report output: the record's Id plus its column values keyed
    /// by column header.
    /// </summary>
    internal sealed class AgentReportRow
    {
        /// <summary>
        /// The Id of the underlying record for this row.
        /// </summary>
        internal int Id { get; set; }

        /// <summary>
        /// The row's values keyed by column header. Values are the raw query
        /// values; columns the current person may not view carry the mask instead.
        /// </summary>
        internal Dictionary<string, object> Values { get; set; }
    }

    /*
        9/1/26 - CLAUDE

        This is a deliberately thin, internal shim that lets the System.Web-free
        AI agent run a report without referencing System.Web itself. It runs the
        report through Report.GetQueryable (the same engine that backs the report
        detail and dynamic report blocks) and projects the raw values off the
        resulting rows into a dictionary. Report.GetQueryable and the SortProperty
        type in its arguments live in Rock.dll, which already references System.Web,
        so keeping the report call here keeps every System.Web type on this side of
        the boundary.

        Values are intentionally raw: entity properties come through as their native
        typed value, and attribute values as their stored form. No display
        formatting (HTML wrapping, lookup name resolution) is applied, which suits an
        LLM consumer far better than grid markup. Field-level view security is
        enforced here: attribute columns the current person may not view are masked.

        It is intentionally internal (exposed to Rock.AI.Agent through
        InternalsVisibleTo) so it carries no API-stability promise and can be
        reshaped or deleted as the reporting engine evolves.

        Reason: Give the agent a headless seam onto Report.GetQueryable.
    */

    /// <summary>
    /// Internal bridge that runs a report for the AI agent through a
    /// <see cref="System.Web"/>-free surface. See the file header for why it
    /// exists and why it is internal.
    /// </summary>
    internal static class AgentReportRunner
    {
        /// <summary>
        /// The value substituted for a column the current person is not authorized
        /// to view.
        /// </summary>
        private const string OutputFieldMask = "***";

        /// <summary>
        /// Runs a report through <see cref="Report.GetQueryable"/> and projects its
        /// rows into raw, System.Web-free values. The report's own saved sort is
        /// used. Attribute columns the current person may not view are masked.
        /// </summary>
        /// <param name="report">The report to run.</param>
        /// <param name="currentPerson">The person the report is run as, for per-field view security.</param>
        /// <param name="rockContext">The context to query against.</param>
        /// <param name="sortByFieldId">The <c>ReportField.Id</c> to sort by, or <c>null</c> to use the report's own saved sort.</param>
        /// <param name="sortDescending">When sorting by <paramref name="sortByFieldId"/>, whether to sort descending.</param>
        /// <returns>The report rows.</returns>
        internal static AgentReportResult GetReportData( Report report, Person currentPerson, RockContext rockContext, int? sortByFieldId = null, bool sortDescending = false )
        {
            // Resolve the entity type, classify the report's fields, and build the
            // entity query. This shared step is also used by the Obsidian report grid
            // builder so the two cannot drift. Attribute columns return their persisted
            // display text so lookup field types (e.g. defined value) yield a readable
            // value rather than a raw Guid.
            var reportQuery = ReportQueryBuilder.Create( report, rockContext, useAttributePersistedTextValues: true, sortByFieldId: sortByFieldId, sortDescending: sortDescending );

            // Build a column map (runtime field name, display header, and view
            // authorization) for each selected column, reproducing the column-index
            // order the query used so each report field lines up with its selection.
            var columns = new List<ReportColumnMap>();
            var usedHeaders = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
            var columnIndex = 0;

            foreach ( var reportField in report.ReportFields.OrderBy( f => f.ColumnOrder ) )
            {
                columnIndex++;

                if ( reportQuery.EntityFields.TryGetValue( columnIndex, out var entityField ) )
                {
                    columns.Add( new ReportColumnMap
                    {
                        FieldName = $"entity_{entityField.Name}_{columnIndex}",
                        Header = MakeUniqueHeader( ResolveHeader( reportField, entityField.Title ), usedHeaders ),
                        IsAuthorized = true
                    } );
                }
                else if ( reportQuery.Attributes.TryGetValue( columnIndex, out var attribute ) )
                {
                    columns.Add( new ReportColumnMap
                    {
                        FieldName = $"attribute_{attribute.Id}_{columnIndex}",
                        Header = MakeUniqueHeader( ResolveHeader( reportField, attribute.Name ), usedHeaders ),
                        IsAuthorized = attribute.IsAuthorized( Authorization.VIEW, currentPerson )
                    } );
                }
                else if ( reportQuery.Components.ContainsKey( columnIndex ) )
                {
                    // The component and its entity type resolved during Create, so
                    // this cannot be null here.
                    var component = DataSelectContainer.GetComponent( reportField.DataSelectComponentEntityType.Name );
                    columns.Add( new ReportColumnMap
                    {
                        FieldName = $"data_{component.ColumnPropertyName}_{columnIndex}",
                        Header = MakeUniqueHeader( ResolveHeader( reportField, component.ColumnHeaderText ), usedHeaders ),
                        IsAuthorized = true
                    } );
                }
            }

            var queryable = reportQuery.Queryable;

            // Reflect over the runtime dynamic type once; the projected field names
            // are matched case-insensitively.
            var fieldByName = queryable.ElementType.GetFields().ToDictionary( f => f.Name, StringComparer.OrdinalIgnoreCase );
            var idField = fieldByName.TryGetValue( "id", out var idf ) ? idf : null;

            var rows = new List<AgentReportRow>();

            foreach ( var rowObject in queryable )
            {
                var values = new Dictionary<string, object>( columns.Count );

                foreach ( var column in columns )
                {
                    if ( !column.IsAuthorized )
                    {
                        values[column.Header] = OutputFieldMask;
                    }
                    else if ( fieldByName.TryGetValue( column.FieldName, out var field ) )
                    {
                        values[column.Header] = field.GetValue( rowObject );
                    }
                    else
                    {
                        values[column.Header] = null;
                    }
                }

                rows.Add( new AgentReportRow
                {
                    Id = idField != null ? ( int ) idField.GetValue( rowObject ) : 0,
                    Values = values
                } );
            }

            return new AgentReportResult { Rows = rows };
        }

        /// <summary>
        /// Resolves a column's display header, preferring the report field's own
        /// header text and falling back to the supplied default.
        /// </summary>
        /// <param name="reportField">The report field.</param>
        /// <param name="fallback">The header to use when the field has no header text.</param>
        /// <returns>The header text.</returns>
        private static string ResolveHeader( ReportField reportField, string fallback )
        {
            return reportField.ColumnHeaderText.IsNotNullOrWhiteSpace() ? reportField.ColumnHeaderText : fallback;
        }

        /// <summary>
        /// Ensures a header is unique within the row dictionary by suffixing a
        /// counter when the same header would otherwise collide.
        /// </summary>
        /// <param name="header">The desired header.</param>
        /// <param name="usedHeaders">The set of headers already assigned.</param>
        /// <returns>A header not yet present in <paramref name="usedHeaders"/>.</returns>
        private static string MakeUniqueHeader( string header, HashSet<string> usedHeaders )
        {
            var candidate = header.IsNotNullOrWhiteSpace() ? header : "Column";
            var uniqueHeader = candidate;
            var suffix = 2;

            while ( !usedHeaders.Add( uniqueHeader ) )
            {
                uniqueHeader = $"{candidate} {suffix}";
                suffix++;
            }

            return uniqueHeader;
        }

        /// <summary>
        /// Maps a report column to the runtime field that holds its value, the
        /// display header to key it under, and whether the current person may view
        /// it.
        /// </summary>
        private sealed class ReportColumnMap
        {
            /// <summary>
            /// The name of the field on the runtime dynamic type that holds this
            /// column's value.
            /// </summary>
            public string FieldName { get; set; }

            /// <summary>
            /// The display header to key this column under in the row dictionary.
            /// </summary>
            public string Header { get; set; }

            /// <summary>
            /// Whether the current person is authorized to view this column's value.
            /// When false, the value is masked.
            /// </summary>
            public bool IsAuthorized { get; set; }
        }
    }
}
