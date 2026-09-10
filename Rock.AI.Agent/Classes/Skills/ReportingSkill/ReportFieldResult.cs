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

using Rock.AI.Agent.Classes.Entity;
using Rock.Model;

namespace Rock.AI.Agent.Classes.Skills.ReportingSkill;

/// <summary>
/// A single field (column) of a report. Its IdKey is what identifies the field
/// when requesting a sort from <c>GetReportItems</c>.
/// </summary>
internal class ReportFieldResult : EntityResultBase
{
    /// <summary>
    /// How the field's value is produced: a property of the entity, an attribute
    /// value, or a data select component.
    /// </summary>
    public ReportFieldType ReportFieldType { get; set; }

    /// <summary>
    /// The field's selection: the property or attribute name, or the serialized
    /// configuration of the data select component.
    /// </summary>
    public string Selection { get; set; }

    /// <summary>
    /// The column header shown for the field.
    /// </summary>
    public string ColumnHeaderText { get; set; }

    /// <summary>
    /// The left-to-right position of the column.
    /// </summary>
    public int ColumnOrder { get; set; }

    /// <summary>
    /// The field's position in the report's saved sort, or <c>null</c> when the
    /// report does not sort on it.
    /// </summary>
    public int? SortOrder { get; set; }

    /// <summary>
    /// Indicates that the field may be used as the sort field in
    /// <c>GetReportItems</c>. Property and attribute fields can always be sorted; a
    /// data select component field can be sorted only when its component allows it.
    /// </summary>
    public bool IsSortable { get; set; }

    /// <summary>
    /// Indicates that the field is shown in the report grid.
    /// </summary>
    public bool ShowInGrid { get; set; }
}
