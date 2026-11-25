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

namespace Rock.Data;

/// <summary>
/// What the entity model maps: the tables, columns, primary keys and
/// relationships a database must support. Produced by a model adapter
/// (EF6's mapping API on develop), which is the only code that reads EF
/// metadata, so the comparison itself has no EF dependency.
/// </summary>
internal sealed class SchemaExpectedModel
{
    /// <summary>
    /// The mapped tables.
    /// </summary>
    public List<SchemaExpectedTable> Tables { get; } = new List<SchemaExpectedTable>();
}

/// <summary>
/// A table the entity model maps.
/// </summary>
internal sealed class SchemaExpectedTable
{
    /// <summary>
    /// The schema the table belongs to.
    /// </summary>
    public string Schema { get; set; } = "dbo";

    /// <summary>
    /// The table name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The <c>schema.table</c> key.
    /// </summary>
    public string Key => $"{Schema}.{Name}";

    /// <summary>
    /// The mapped columns. The store facets use the same conventions as
    /// <see cref="SchemaCatalogColumn"/>; a facet left <c>null</c> is not checked.
    /// </summary>
    public List<SchemaCatalogColumn> Columns { get; } = new List<SchemaCatalogColumn>();

    /// <summary>
    /// The primary key columns, in key order.
    /// </summary>
    public List<string> PrimaryKeyColumns { get; set; } = new List<string>();

    /// <summary>
    /// The relationships for which this table holds the foreign key. Only
    /// reported, never a failure (see the spec's mapping check rules).
    /// </summary>
    public List<SchemaCatalogForeignKey> ForeignKeys { get; } = new List<SchemaCatalogForeignKey>();
}
