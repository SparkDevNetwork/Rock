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
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;

using Rock.Data;

namespace Rock.SchemaCheck;

/// <summary>
/// Produces the <see cref="SchemaExpectedModel"/> from EF6's store model,
/// the tables and columns EF6 maps <see cref="RockContext"/> to. This is
/// the only code in the schema checks that reads EF metadata.
/// </summary>
internal static class Ef6ModelAdapter
{
    /// <summary>
    /// Reads the expected tables, columns, primary keys and relationships
    /// from the store (SSpace) side of the EF6 model. The store model
    /// describes tables exactly as EF6 maps them, including join tables
    /// and tables shared by several entity types.
    /// </summary>
    /// <param name="connectionString">A connection string EF6 can use to build the model.</param>
    /// <returns>What the model maps.</returns>
    public static SchemaExpectedModel Build( string connectionString )
    {
        using var rockContext = new RockContext( connectionString );
        var workspace = ( ( IObjectContextAdapter ) rockContext ).ObjectContext.MetadataWorkspace;
        var storeContainer = workspace.GetItems<EntityContainer>( DataSpace.SSpace ).Single();
        var model = new SchemaExpectedModel();
        var tablesBySet = new Dictionary<EntitySetBase, SchemaExpectedTable>();

        foreach ( var entitySet in storeContainer.BaseEntitySets.OfType<EntitySet>() )
        {
            var table = new SchemaExpectedTable
            {
                Schema = entitySet.Schema ?? "dbo",
                Name = entitySet.Table ?? entitySet.Name,
                PrimaryKeyColumns = entitySet.ElementType.KeyProperties.Select( p => p.Name ).ToList()
            };

            table.Columns.AddRange( entitySet.ElementType.Properties.Select( ToColumn ) );

            model.Tables.Add( table );
            tablesBySet.Add( entitySet, table );
        }

        foreach ( var associationSet in storeContainer.BaseEntitySets.OfType<AssociationSet>() )
        {
            var constraint = associationSet.ElementType.ReferentialConstraints.SingleOrDefault();

            if ( constraint == null )
            {
                continue;
            }

            var principalSet = associationSet.AssociationSetEnds[constraint.FromRole.Name].EntitySet;
            var dependentSet = associationSet.AssociationSetEnds[constraint.ToRole.Name].EntitySet;

            tablesBySet[dependentSet].ForeignKeys.Add( new SchemaCatalogForeignKey
            {
                Columns = constraint.ToProperties.Select( p => p.Name ).ToList(),
                ReferencedTable = tablesBySet[principalSet].Key,
                ReferencedColumns = constraint.FromProperties.Select( p => p.Name ).ToList(),
                IsCascadeDelete = constraint.FromRole.DeleteBehavior == OperationAction.Cascade
            } );
        }

        return model;
    }

    /// <summary>
    /// Converts a store property to a column with the facets the mapping
    /// check compares, using the same conventions as the catalog reader.
    /// </summary>
    /// <param name="property">The store (SSpace) property.</param>
    /// <returns>The expected column.</returns>
    private static SchemaCatalogColumn ToColumn( EdmProperty property )
    {
        var storeType = property.TypeName.ToLowerInvariant();
        var column = new SchemaCatalogColumn
        {
            Name = property.Name,
            IsNullable = property.Nullable
        };

        // EF6 names max types "nvarchar(max)" rather than setting a facet.
        if ( storeType.EndsWith( "(max)" ) )
        {
            column.StoreType = storeType.Substring( 0, storeType.Length - 5 );
            column.MaxLength = -1;

            return column;
        }

        // SQL Server reports rowversion columns as timestamp.
        column.StoreType = storeType == "rowversion" ? "timestamp" : storeType;

        switch ( column.StoreType )
        {
            case "nchar":
            case "nvarchar":
            case "char":
            case "varchar":
            case "binary":
            case "varbinary":
                column.MaxLength = property.IsMaxLength ? -1 : property.MaxLength;
                break;

            case "decimal":
            case "numeric":
                column.Precision = property.Precision;
                column.Scale = property.Scale;
                break;

            case "datetime2":
            case "time":
            case "datetimeoffset":
                column.Precision = property.Precision;
                break;
        }

        return column;
    }
}
